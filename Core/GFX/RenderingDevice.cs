using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using JetBrains.Annotations;
using OpenTemple.Core.Config;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using Buffer = SharpDX.Direct3D11.Buffer;
using CullMode = SharpDX.Direct3D11.CullMode;
using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11InfoQueue = SharpDX.Direct3D11.InfoQueue;
using DXGIDevice = SharpDX.DXGI.Device;
using MapFlags = SharpDX.DXGI.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

namespace OpenTemple.Core.GFX
{
    public enum MapMode
    {
        Read,
        Discard,
        NoOverwrite
    }

    public class RenderingDevice : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly IFileSystem _fs;

        public RenderingDevice(IFileSystem fs, IMainWindow mainWindow, int adapterIdx = 0, bool debugDevice = false)
        {
            _fs = fs;
            mWindowHandle = mainWindow.NativeHandle;
            mShaders = new Shaders(fs, this);
            mTextures = new Textures(fs, this, 128 * 1024 * 1024);
            this.debugDevice = debugDevice;

            mDxgiFactory = new Factory1();

            var displayDevices = GetDisplayDevices();

            // Find the adapter selected by the user, although we might fall back to the
            // default one if the user didn't select one or the adapter selection changed
            mAdapter = GetAdapter(adapterIdx);
            if (mAdapter == null)
            {
                // Fall back to default
                Logger.Error("Couldn't retrieve adapter #{0}. Falling back to default", 0);
                mAdapter = GetAdapter(displayDevices[0].id);
                if (mAdapter == null)
                {
                    throw new GfxException(
                        "Couldn't retrieve your configured graphics adapter, but also couldn't fall back to the default adapter.");
                }
            }

            // Required for the Direct2D support
            DeviceCreationFlags deviceFlags = DeviceCreationFlags.BgraSupport;
            if (debugDevice)
            {
                deviceFlags |=
                    DeviceCreationFlags.Debug | DeviceCreationFlags.DisableGpuTimeout;
            }

            FeatureLevel[] requestedLevels =
            {
                FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1,
                FeatureLevel.Level_10_0, FeatureLevel.Level_9_3, FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1
            };

            try
            {
                mD3d11Device = new D3D11Device(mAdapter, deviceFlags, requestedLevels);
                mFeatureLevel = mD3d11Device.FeatureLevel;
                mContext = mD3d11Device.ImmediateContext;
            }
            catch (SharpDXException e)
            {
                // DXGI_ERROR_SDK_COMPONENT_MISSING
                if (debugDevice && e.ResultCode.Code == 0x887A002D)
                {
                    throw new GfxException("To use the D3D debugging feature, you need to " +
                                           "install the corresponding Windows SDK component.");
                }

                throw new GfxException("Unable to create a Direct3D 11 device.", e);
            }

            Logger.Info("Created D3D11 device with feature level {0}", mFeatureLevel);

            if (debugDevice)
            {
                // Retrieve the interface used to emit event groupings for debugging
                annotation = mContext.QueryInterfaceOrNull<UserDefinedAnnotation>();
            }

            // Retrieve DXGI device
            using DXGIDevice dxgiDevice = mD3d11Device.QueryInterfaceOrNull<DXGIDevice>();
            if (dxgiDevice == null)
            {
                throw new GfxException("Couldn't retrieve DXGI device from D3D11 device.");
            }

            using Adapter dxgiAdapter = dxgiDevice.GetParent<Adapter>();
            mDxgiFactory = dxgiAdapter.GetParent<Factory1>(); // Hang on to the DXGI factory used here

            // Create 2D rendering
            textEngine = new TextEngine(mD3d11Device, debugDevice);

            if (mWindowHandle != IntPtr.Zero)
            {
                mSwapChainDesc = new SwapChainDescription();
                mSwapChainDesc.BufferCount = 2;
                mSwapChainDesc.ModeDescription.Format = Format.B8G8R8A8_UNorm;
                mSwapChainDesc.Usage = Usage.RenderTargetOutput;
                mSwapChainDesc.OutputHandle = mWindowHandle;
                mSwapChainDesc.SampleDescription.Count = 1;
                mSwapChainDesc.IsWindowed = true; // As per the recommendation, we always create windowed

                _swapChain = new SwapChain(mDxgiFactory, mD3d11Device, mSwapChainDesc);

                // Get the backbuffer from the swap chain
                using var backBufferTexture = _swapChain.GetBackBuffer<Texture2D>(0);

                mBackBufferNew = CreateRenderTargetForNativeSurface(backBufferTexture);
                var backBufferSize = mBackBufferNew.Resource.GetSize();
                mBackBufferDepthStencil = CreateRenderTargetDepthStencil(backBufferSize.Width, backBufferSize.Height);

                // Push back the initial render target that should never be removed
                PushBackBufferRenderTarget();
            }

            // Create centralized constant buffers for the vertex and pixel shader stages
            mVsConstantBuffer = CreateEmptyConstantBuffer(MaxVsConstantBufferSize);
            mPsConstantBuffer = CreateEmptyConstantBuffer(MaxPsConstantBufferSize);

            // TODO: color bullshit is not yet done (tig_d3d_init_handleformat et al)

            foreach (var listener in mResourcesListeners)
            {
                listener.CreateResources(this);
            }

            mResourcesCreated = true;

            // This is only relevant if we are in windowed mode
            mainWindow.Resized += size => ResizeBuffers();
        }

        public bool BeginFrame()
        {
            if (mBeginSceneDepth++ > 0)
            {
                return true;
            }

            ClearCurrentColorTarget(new LinearColorA(0, 0, 0, 1));
            ClearCurrentDepthTarget();

            mLastFrameStart = TimePoint.Now;
            ;

            return true;
        }

        public bool Present()
        {
            if (--mBeginSceneDepth > 0)
            {
                return true;
            }

            PresentForce();

            return true;
        }

        public void PresentForce()
        {
            mTextures.FreeUnusedTextures();

            _swapChain.Present(0, 0);
        }

        public void Flush()
        {
            mContext.Flush();
        }

        public void ClearCurrentColorTarget(LinearColorA color)
        {
            var target = GetCurrentRenderTargetColorBuffer();

            // Clear the current render target view
            mContext.ClearRenderTargetView(target.RenderTargetView, color);
        }

        public void ClearCurrentDepthTarget(bool clearDepth = true,
            bool clearStencil = true,
            float depthValue = 1.0f,
            byte stencilValue = 0)
        {
            if (!clearDepth && !clearStencil)
            {
                return;
            }

            DepthStencilClearFlags flags = 0;
            if (clearDepth)
            {
                flags |= DepthStencilClearFlags.Depth;
            }

            if (clearStencil)
            {
                flags |= DepthStencilClearFlags.Stencil;
            }

            var depthStencil = GetCurrentRenderTargetDepthStencilBuffer();

            if (depthStencil == null)
            {
                Logger.Warn("Trying to clear current depthstencil view, but none is bound.");
                return;
            }

            mContext.ClearDepthStencilView(depthStencil.DsView, flags, depthValue,
                stencilValue);
        }

        public TimePoint GetLastFrameStart() => mLastFrameStart;
        public TimePoint GetDeviceCreated() => mDeviceCreated;

        public List<DisplayDevice> GetDisplayDevices()
        {
            // Recreate the DXGI factory if we want to enumerate a new list of devices
            if (mDisplayDevices != null && mDxgiFactory.IsCurrent)
            {
                return mDisplayDevices;
            }

            // Enumerate devices
            Logger.Info("Enumerating DXGI display devices...");

            mDisplayDevices = new List<DisplayDevice>();

            int adapterCount = mDxgiFactory.GetAdapterCount1();
            for (int adapterIdx = 0; adapterIdx < adapterCount; adapterIdx++)
            {
                using var adapter = mDxgiFactory.GetAdapter1(adapterIdx);

                // Get an adapter descriptor
                var adapterDesc = adapter.Description;

                var displayDevice = new DisplayDevice();

                displayDevice.name = adapterDesc.Description;
                Logger.Info("Adapter #{0} '{1}'", adapterIdx, displayDevice.name);

                // Enumerate all outputs of the adapter
                var outputCount = adapter.GetOutputCount();

                for (int outputIdx = 0; outputIdx < outputCount; outputIdx++)
                {
                    using var output = adapter.GetOutput(outputIdx);

                    var outputDesc = output.Description;

                    var deviceName = outputDesc.DeviceName;

                    Span<char> monitorName = stackalloc char[128];
                    int monitorNameSize = monitorName.Length;
                    unsafe
                    {
                        fixed (char* monitorNamePtr = monitorName)
                        {
                            if (!Win32_GetMonitorName(outputDesc.MonitorHandle, monitorNamePtr, ref monitorNameSize))
                            {
                                Logger.Warn("Failed to determine monitor name.");
                            }
                        }
                    }

                    monitorName = monitorName.Slice(0, monitorNameSize);

                    DisplayDeviceOutput displayOutput = new DisplayDeviceOutput();
                    displayOutput.id = deviceName;
                    displayOutput.name = new string(monitorName);
                    Logger.Info("  Output #{0} Device '{1}' Monitor '{2}'", outputIdx,
                        deviceName, displayOutput.name);
                    displayDevice.outputs.Add(displayOutput);
                }

                if (displayDevice.outputs.Count > 0)
                {
                    mDisplayDevices.Add(displayDevice);
                }
                else
                {
                    Logger.Info("Skipping device because it has no outputs.");
                }
            }

            return mDisplayDevices;
        }

        [DllImport("OpenTemple.Native")]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe bool Win32_GetMonitorName(IntPtr monitorHandle, char* name, ref int nameSize);

        // Resize the back buffer
        private void ResizeBuffers()
        {
            if (_swapChain == null)
            {
                return;
            }

            if (mRenderTargetStack.Count != 1)
            {
                throw new InvalidOperationException("Cannot resize backbuffer while rendering is going on!");
            }

            mBackBufferNew.Dispose();
            mBackBufferDepthStencil.Dispose();
            PopRenderTarget();

            _swapChain.ResizeBuffers(0, 0, 0, Format.Unknown, 0);

            // Get the backbuffer from the swap chain
            using var backBufferTexture = _swapChain.GetBackBuffer<Texture2D>(0);

            mBackBufferNew = CreateRenderTargetForNativeSurface(backBufferTexture);
            var backBufferSize = mBackBufferNew.Resource.GetSize();
            mBackBufferDepthStencil = CreateRenderTargetDepthStencil(backBufferSize.Width, backBufferSize.Height);

            // restore the render target
            PushBackBufferRenderTarget();

            // Retrieve the *actual* back buffer size since we created it to match the client area above
            var size = mBackBufferNew.Resource.GetSize();

            // Notice listeners about changed backbuffer size
            foreach (var entry in mResizeListeners)
            {
                entry.Value(size.Width, size.Height);
            }
        }

        public Material CreateMaterial(
            BlendSpec blendSpec,
            DepthStencilSpec depthStencilSpec,
            RasterizerSpec rasterizerSpec,
            MaterialSamplerSpec[] samplerSpecs,
            VertexShader vs,
            PixelShader ps
        )
        {
            blendSpec = blendSpec ?? new BlendSpec();
            depthStencilSpec = depthStencilSpec ?? new DepthStencilSpec();
            rasterizerSpec = rasterizerSpec ?? new RasterizerSpec();
            samplerSpecs = samplerSpecs ?? Array.Empty<MaterialSamplerSpec>();

            var blendState = CreateBlendState(blendSpec);
            var depthStencilState = CreateDepthStencilState(depthStencilSpec);
            var rasterizerState = CreateRasterizerState(rasterizerSpec);

            List<ResourceRef<MaterialSamplerBinding>> samplerBindings =
                new List<ResourceRef<MaterialSamplerBinding>>(samplerSpecs.Length);
            foreach (var samplerSpec in samplerSpecs)
            {
                using var samplerState = CreateSamplerState(samplerSpec.samplerSpec);
                samplerBindings.Add(
                    new ResourceRef<MaterialSamplerBinding>(
                        new MaterialSamplerBinding(
                            this, samplerSpec.texture.Resource, samplerState
                        ))
                );
            }

            return new Material(this, blendState, depthStencilState, rasterizerState,
                samplerBindings, new ResourceRef<VertexShader>(vs), new ResourceRef<PixelShader>(ps));
        }

        private static BlendOption ConvertBlendOperand(BlendOperand op)
        {
            switch (op)
            {
                case BlendOperand.Zero:
                    return BlendOption.Zero;
                case BlendOperand.One:
                    return BlendOption.One;
                case BlendOperand.SrcColor:
                    return BlendOption.SourceColor;
                case BlendOperand.InvSrcColor:
                    return BlendOption.InverseSourceColor;
                case BlendOperand.SrcAlpha:
                    return BlendOption.SourceAlpha;
                case BlendOperand.InvSrcAlpha:
                    return BlendOption.InverseSourceAlpha;
                case BlendOperand.DestAlpha:
                    return BlendOption.DestinationAlpha;
                case BlendOperand.InvDestAlpha:
                    return BlendOption.InverseDestinationAlpha;
                case BlendOperand.DestColor:
                    return BlendOption.DestinationColor;
                case BlendOperand.InvDestColor:
                    return BlendOption.InverseDestinationColor;
                default:
                    throw new GfxException("Unknown blend operand.");
            }
        }

        public ResourceRef<BlendState> CreateBlendState(BlendSpec spec)
        {
            // Check if we have a matching state already
            if (blendStates.TryGetValue(spec, out var stateRef))
            {
                return stateRef.CloneRef();
            }

            var blendDesc = BlendStateDescription.Default();

            ref var targetDesc = ref blendDesc.RenderTarget[0];
            targetDesc.IsBlendEnabled = spec.blendEnable;
            targetDesc.SourceBlend = ConvertBlendOperand(spec.srcBlend);
            targetDesc.DestinationBlend = ConvertBlendOperand(spec.destBlend);
            targetDesc.SourceAlphaBlend = ConvertBlendOperand(spec.srcAlphaBlend);
            targetDesc.DestinationAlphaBlend = ConvertBlendOperand(spec.destAlphaBlend);

            ColorWriteMaskFlags writeMask = 0;
            // Never overwrite the alpha channel with random stuff when blending is disabled
            if (spec.writeAlpha && targetDesc.IsBlendEnabled)
            {
                writeMask |= ColorWriteMaskFlags.Alpha;
            }

            if (spec.writeRed)
            {
                writeMask |= ColorWriteMaskFlags.Red;
            }

            if (spec.writeGreen)
            {
                writeMask |= ColorWriteMaskFlags.Green;
            }

            if (spec.writeBlue)
            {
                writeMask |= ColorWriteMaskFlags.Blue;
            }

            targetDesc.RenderTargetWriteMask = writeMask;

            var gpuState = new SharpDX.Direct3D11.BlendState(mD3d11Device, blendDesc);

            stateRef = new ResourceRef<BlendState>(new BlendState(this, spec, gpuState));
            blendStates[spec] = stateRef;
            return stateRef.CloneRef();
        }

        private static Comparison ConvertComparisonFunc(ComparisonFunc func)
        {
            switch (func)
            {
                case ComparisonFunc.Never:
                    return Comparison.Never;
                case ComparisonFunc.Less:
                    return Comparison.Less;
                case ComparisonFunc.Equal:
                    return Comparison.Equal;
                case ComparisonFunc.LessEqual:
                    return Comparison.LessEqual;
                case ComparisonFunc.Greater:
                    return Comparison.Greater;
                case ComparisonFunc.NotEqual:
                    return Comparison.NotEqual;
                case ComparisonFunc.GreaterEqual:
                    return Comparison.GreaterEqual;
                case ComparisonFunc.Always:
                    return Comparison.Always;
                default:
                    throw new GfxException("Unknown comparison func.");
            }
        }

        public ResourceRef<DepthStencilState> CreateDepthStencilState(DepthStencilSpec spec)
        {
            // Check if we have a matching state already
            if (depthStencilStates.TryGetValue(spec, out var stateRef))
            {
                return stateRef.CloneRef();
            }

            var depthStencilDesc = DepthStencilStateDescription.Default();
            depthStencilDesc.IsDepthEnabled = spec.depthEnable;
            depthStencilDesc.DepthWriteMask = spec.depthWrite
                ? DepthWriteMask.All
                : DepthWriteMask.Zero;

            depthStencilDesc.DepthComparison = ConvertComparisonFunc(spec.depthFunc);

            var gpuState = new SharpDX.Direct3D11.DepthStencilState(mD3d11Device, depthStencilDesc);

            stateRef = new ResourceRef<DepthStencilState>(new DepthStencilState(this, spec, gpuState));
            depthStencilStates[spec] = stateRef;
            return stateRef.CloneRef();
        }

        public ResourceRef<RasterizerState> CreateRasterizerState(RasterizerSpec spec)
        {
            // Check if we have a matching state already
            if (rasterizerStates.TryGetValue(spec, out var stateRef))
            {
                return stateRef.CloneRef();
            }

            var rasterizerDesc = RasterizerStateDescription.Default();
            if (spec.wireframe)
            {
                rasterizerDesc.FillMode = FillMode.Wireframe;
            }

            switch (spec.cullMode)
            {
                case Materials.CullMode.Back:
                    rasterizerDesc.CullMode = CullMode.Back;
                    break;
                case Materials.CullMode.Front:
                    rasterizerDesc.CullMode = CullMode.Front;
                    break;
                case Materials.CullMode.None:
                    rasterizerDesc.CullMode = CullMode.None;
                    break;
            }

            rasterizerDesc.IsScissorEnabled = spec.scissor;

            rasterizerDesc.IsMultisampleEnabled = antiAliasing;

            var gpuState = new SharpDX.Direct3D11.RasterizerState(mD3d11Device, rasterizerDesc);

            stateRef = new ResourceRef<RasterizerState>(new RasterizerState(this, spec, gpuState));
            rasterizerStates[spec] = stateRef;
            return stateRef.CloneRef();
        }

        private static TextureAddressMode ConvertTextureAddress(TextureAddress address)
        {
            switch (address)
            {
                case TextureAddress.Clamp:
                    return TextureAddressMode.Clamp;
                case TextureAddress.Wrap:
                    return TextureAddressMode.Wrap;
                default:
                    throw new GfxException("Unknown texture address mode.");
            }
        }

        public ResourceRef<SamplerState> CreateSamplerState(SamplerSpec spec)
        {
            if (samplerStates.TryGetValue(spec, out var stateRef))
            {
                return stateRef.CloneRef();
            }

            var samplerDesc = SamplerStateDescription.Default();

            // we only support mapping point + linear
            bool minPoint = (spec.minFilter == TextureFilterType.NearestNeighbor);
            bool magPoint = (spec.magFilter == TextureFilterType.NearestNeighbor);
            bool mipPoint = (spec.mipFilter == TextureFilterType.NearestNeighbor);

            // This is a truth table for all possible values represented above
            if (!minPoint && !magPoint && !mipPoint)
            {
                samplerDesc.Filter = Filter.MinMagMipLinear;
            }
            else if (!minPoint && !magPoint && mipPoint)
            {
                samplerDesc.Filter = Filter.MinMagLinearMipPoint;
            }
            else if (!minPoint && magPoint && !mipPoint)
            {
                samplerDesc.Filter = Filter.MinLinearMagPointMipLinear;
            }
            else if (!minPoint && magPoint && mipPoint)
            {
                samplerDesc.Filter = Filter.MinLinearMagMipPoint;
            }
            else if (minPoint && !magPoint && !mipPoint)
            {
                samplerDesc.Filter = Filter.MinPointMagMipLinear;
            }
            else if (minPoint && !magPoint && mipPoint)
            {
                samplerDesc.Filter = Filter.MinPointMagLinearMipPoint;
            }
            else if (minPoint && magPoint && !mipPoint)
            {
                samplerDesc.Filter = Filter.MinMagPointMipLinear;
            }
            else if (minPoint && magPoint && mipPoint)
            {
                samplerDesc.Filter = Filter.MinMagMipPoint;
            }

            samplerDesc.AddressU = ConvertTextureAddress(spec.addressU);
            samplerDesc.AddressV = ConvertTextureAddress(spec.addressV);

            var gpuState = new SharpDX.Direct3D11.SamplerState(mD3d11Device, samplerDesc);

            stateRef = new ResourceRef<SamplerState>(new SamplerState(this, spec, gpuState));
            samplerStates[spec] = stateRef;
            return stateRef.CloneRef();
        }

        // Changes the current scissor rect to the given rectangle
        public void SetScissorRect(int x, int y, int width, int height)
        {
            mContext.Rasterizer.SetScissorRectangle(x, y, x + width, y + height);

            textEngine.SetScissorRect(x, y, width, height);
        }

        // Resets the scissor rect to the current render target's size
        public void ResetScissorRect()
        {
            var size = mRenderTargetStack.Peek().colorBuffer.Resource.GetSize();
            mContext.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);

            textEngine.ResetScissorRect();
        }

        public ResourceRef<IndexBuffer> CreateEmptyIndexBuffer(int count, Format format = Format.R16_UInt)
        {
            var bufferDesc = new BufferDescription(
                count * sizeof(ushort),
                ResourceUsage.Dynamic,
                BindFlags.IndexBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0
            );

            var buffer = new Buffer(mD3d11Device, IntPtr.Zero, bufferDesc);

            return new ResourceRef<IndexBuffer>(new IndexBuffer(this, buffer, format, count));
        }

        public ResourceRef<VertexBuffer> CreateEmptyVertexBuffer(int size, bool forPoints = false)
        {
            // Create a dynamic vertex buffer since it'll be updated (probably a lot)
            var bufferDesc = new BufferDescription(
                size,
                ResourceUsage.Dynamic,
                BindFlags.VertexBuffer,
                CpuAccessFlags.Write,
                ResourceOptionFlags.None,
                0
            );

            var buffer = new Buffer(mD3d11Device, IntPtr.Zero, bufferDesc);

            return new ResourceRef<VertexBuffer>(new VertexBuffer(this, buffer, size));
        }

        private static Format ConvertFormat(BufferFormat format, out int bytesPerPixel)
        {
            Format formatNew;
            switch (format)
            {
                case BufferFormat.A8:
                    formatNew = Format.R8_UNorm;
                    bytesPerPixel = 1;
                    break;
                case BufferFormat.A8R8G8B8:
                    formatNew = Format.B8G8R8A8_UNorm;
                    bytesPerPixel = 4;
                    break;
                case BufferFormat.X8R8G8B8:
                    formatNew = Format.B8G8R8X8_UNorm;
                    bytesPerPixel = 4;
                    break;
                default:
                    throw new GfxException($"Unsupported format: {format}");
            }

            return formatNew;
        }


        public ResourceRef<DynamicTexture> CreateDynamicTexture(BufferFormat format, int width, int height)
        {
            var size = new Size(width, height);

            var formatNew = ConvertFormat(format, out var bytesPerPixel);

            var textureDesc = new Texture2DDescription();
            textureDesc.Format = formatNew;
            textureDesc.Width = width;
            textureDesc.Height = height;
            textureDesc.MipLevels = 1;
            textureDesc.ArraySize = 1;
            textureDesc.BindFlags = BindFlags.ShaderResource;
            textureDesc.Usage = ResourceUsage.Dynamic;
            textureDesc.CpuAccessFlags = CpuAccessFlags.Write;
            textureDesc.SampleDescription.Count = 1;

            var textureNew = new Texture2D(mD3d11Device, textureDesc);


            var resourceViewDesc = new ShaderResourceViewDescription();
            resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            resourceViewDesc.Texture2D.MipLevels = 1;

            var resourceView = new ShaderResourceView(mD3d11Device, textureNew, resourceViewDesc);

            return new ResourceRef<DynamicTexture>(new DynamicTexture(this, textureNew, resourceView,
                size, bytesPerPixel));
        }

        public ResourceRef<DynamicTexture> CreateDynamicStagingTexture(BufferFormat format, int width, int height)
        {
            var size = new Size(width, height);

            var formatNew = ConvertFormat(format, out var bytesPerPixel);

            var textureDesc = new Texture2DDescription();
            textureDesc.Format = formatNew;
            textureDesc.Width = width;
            textureDesc.Height = height;
            textureDesc.MipLevels = 1;
            textureDesc.ArraySize = 1;
            textureDesc.BindFlags = BindFlags.ShaderResource;
            textureDesc.Usage = ResourceUsage.Staging;
            textureDesc.CpuAccessFlags = CpuAccessFlags.Read;
            textureDesc.SampleDescription.Count = 1;

            var textureNew = new Texture2D(mD3d11Device, textureDesc);

            var resourceViewDesc = new ShaderResourceViewDescription();
            resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            resourceViewDesc.Texture2D.MipLevels = 1;

            var resourceView = new ShaderResourceView(mD3d11Device, textureNew, resourceViewDesc);

            return new ResourceRef<DynamicTexture>(new DynamicTexture(this, textureNew, resourceView,
                size, bytesPerPixel));
        }

        public void CopyRenderTarget(RenderTargetTexture renderTarget, DynamicTexture stagingTexture)
        {
            mContext.CopyResource(renderTarget.Texture, stagingTexture.mTexture);
        }

        public ResourceRef<RenderTargetTexture> CreateRenderTargetTexture(BufferFormat format, int width, int height,
            bool multiSample = false)
        {
            var size = new Size(width, height);

            var formatDx = ConvertFormat(format, out var bpp);

            var bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            var sampleCount = 1;
            var sampleQuality = 0;

            if (multiSample)
            {
                Logger.Info("using multisampling");
                // If this is a multi sample render target, we cannot use it as a texture, or at least, we shouldn't
                bindFlags = BindFlags.RenderTarget;
                sampleCount = msaaSamples;
                sampleQuality = msaaQuality;
            }
            else
            {
                Logger.Info("not using multisampling");
            }

            Logger.Info("width {0} height {1}", width, height);
            var textureDesc = new Texture2DDescription();
            textureDesc.Format = formatDx;
            textureDesc.Width = width;
            textureDesc.Height = height;
            textureDesc.MipLevels = 1;
            textureDesc.ArraySize = 1;
            textureDesc.BindFlags = bindFlags;
            textureDesc.Usage = ResourceUsage.Default;
            textureDesc.SampleDescription.Count = sampleCount;
            textureDesc.SampleDescription.Quality = sampleQuality;

            var texture = new Texture2D(mD3d11Device, textureDesc);

            // Create the render target view of the backing buffer
            var rtViewDesc = new RenderTargetViewDescription();
            rtViewDesc.Dimension = RenderTargetViewDimension.Texture2D;
            rtViewDesc.Texture2D.MipSlice = 0;

            if (multiSample)
            {
                rtViewDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
            }

            var rtView = new RenderTargetView(mD3d11Device, texture, rtViewDesc);

            var srvTexture = texture;
            Texture2D resolvedTexture = null;
            if (multiSample)
            {
                // We have to create another non-multisampled texture and use it for the SRV instead

                // Adapt the existing texture Desc to be a non-MSAA texture with otherwise identical properties
                textureDesc.BindFlags = BindFlags.ShaderResource;
                textureDesc.SampleDescription.Count = 1;
                textureDesc.SampleDescription.Quality = 0;

                resolvedTexture = new Texture2D(mD3d11Device, textureDesc);
                srvTexture = resolvedTexture;
            }

            var resourceViewDesc = new ShaderResourceViewDescription();
            resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            resourceViewDesc.Texture2D.MipLevels = 1;

            var resourceView = new ShaderResourceView(mD3d11Device, srvTexture, resourceViewDesc);

            return new ResourceRef<RenderTargetTexture>(new RenderTargetTexture(this, texture, rtView, resolvedTexture,
                resourceView, size, multiSample));
        }

        public ResourceRef<RenderTargetTexture> CreateRenderTargetForNativeSurface(Texture2D surface)
        {
            var surfaceDesc = surface.Description;

            var rtvDesc = new RenderTargetViewDescription();
            rtvDesc.Format = surfaceDesc.Format;
            rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
            rtvDesc.Texture2D.MipSlice = 0;

            // Create a render target view for rendering to the real backbuffer
            var backBufferView = new RenderTargetView(
                mD3d11Device, surface, rtvDesc
            );

            var size = new Size(surfaceDesc.Width, surfaceDesc.Height);
            return new ResourceRef<RenderTargetTexture>(new RenderTargetTexture(
                this,
                surface,
                backBufferView,
                null,
                null,
                size,
                false));
        }

        public ResourceRef<RenderTargetTexture> CreateRenderTargetForSharedSurface(ComObject surface)
        {
            var dxgiResource = surface.QueryInterfaceOrNull<SharpDX.DXGI.Resource>();

            if (dxgiResource == null)
            {
                throw new ArgumentException("Given COM object is not a DXGI surface");
            }

            var sharedHandle = dxgiResource.SharedHandle;

            dxgiResource.Dispose();

            var sharedResource = mD3d11Device.OpenSharedResource<Resource>(sharedHandle);

            var sharedTexture = sharedResource.QueryInterface<Texture2D>();

            return CreateRenderTargetForNativeSurface(sharedTexture);
        }

        public ResourceRef<RenderTargetDepthStencil> CreateRenderTargetDepthStencil(int width, int height,
            bool multiSample = false)
        {
            var descDepth = new Texture2DDescription();
            descDepth.Format = Format.D24_UNorm_S8_UInt;
            descDepth.Width = width;
            descDepth.Height = height;
            descDepth.ArraySize = 1;
            descDepth.MipLevels = 1; // Disable Mip Map generation
            descDepth.BindFlags = BindFlags.DepthStencil;

            // Enable multi sampling
            if (multiSample)
            {
                descDepth.SampleDescription.Count = msaaSamples;
                descDepth.SampleDescription.Quality = msaaQuality;
            }
            else
            {
                descDepth.SampleDescription.Count = 1;
            }

            var texture = new Texture2D(mD3d11Device, descDepth);

            // Create the depth stencil view
            var depthStencilViewDesc = new DepthStencilViewDescription();
            depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
            depthStencilViewDesc.Format = descDepth.Format;

            if (multiSample)
            {
                depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
            }

            var depthStencilView = new DepthStencilView(mD3d11Device, texture, depthStencilViewDesc);

            var size = new Size(width, height);
            return new ResourceRef<RenderTargetDepthStencil>(
                new RenderTargetDepthStencil(this, texture, depthStencilView, size)
            );
        }

        public ResourceRef<VertexBuffer> CreateVertexBuffer<T>(ReadOnlySpan<T> data, bool immutable = true)
            where T : struct
        {
            return CreateVertexBufferRaw(MemoryMarshal.Cast<T, byte>(data), immutable);
        }

        public unsafe ResourceRef<VertexBuffer> CreateVertexBufferRaw(ReadOnlySpan<byte> data, bool immutable = true)
        {
            // Create a dynamic or immutable vertex buffer depending on the immutable flag
            var bufferDesc = new BufferDescription(data.Length,
                immutable ? ResourceUsage.Immutable : ResourceUsage.Dynamic,
                BindFlags.VertexBuffer,
                immutable ? 0 : CpuAccessFlags.Write,
                0,
                0
            );


            Buffer buffer;
            fixed (byte* dataPtr = data)
            {
                buffer = new Buffer(mD3d11Device, (IntPtr) dataPtr, bufferDesc);
            }

            return new ResourceRef<VertexBuffer>(new VertexBuffer(this, buffer, data.Length));
        }

        public unsafe ResourceRef<IndexBuffer> CreateIndexBuffer(ReadOnlySpan<ushort> data, bool immutable = true)
        {
            var bufferDesc = new BufferDescription(
                data.Length * sizeof(ushort),
                immutable ? ResourceUsage.Immutable : ResourceUsage.Dynamic,
                BindFlags.IndexBuffer,
                immutable ? 0 : CpuAccessFlags.Write,
                0,
                0
            );

            Buffer buffer;
            fixed (ushort* dataPtr = data)
            {
                buffer = new Buffer(mD3d11Device, (IntPtr) dataPtr, bufferDesc);
            }

            return new ResourceRef<IndexBuffer>(new IndexBuffer(this, buffer, Format.R16_UInt, data.Length));
        }

        public unsafe ResourceRef<IndexBuffer> CreateIndexBuffer(ReadOnlySpan<int> data, bool immutable = true)
        {
            var bufferDesc = new BufferDescription(
                data.Length * sizeof(int),
                immutable ? ResourceUsage.Immutable : ResourceUsage.Dynamic,
                BindFlags.IndexBuffer,
                immutable ? 0 : CpuAccessFlags.Write,
                0,
                0
            );

            Buffer buffer;
            fixed (int* dataPtr = data)
            {
                buffer = new Buffer(mD3d11Device, (IntPtr) dataPtr, bufferDesc);
            }

            return new ResourceRef<IndexBuffer>(new IndexBuffer(this, buffer, Format.R32_UInt, data.Length));
        }

        public void SetMaterial(Material material)
        {
            SetRasterizerState(material.RasterizerState.Resource);
            SetBlendState(material.BlendState.Resource);
            SetDepthStencilState(material.DepthStencilState.Resource);

            for (int i = 0; i < material.Samplers.Count; ++i)
            {
                var sampler = material.Samplers[i];
                if (sampler.Resource.Texture.Resource != null)
                {
                    SetTexture(i, sampler.Resource.Texture.Resource);
                }
                else
                {
                    SetTexture(i, Textures.InvalidTexture);
                }

                SetSamplerState(i, sampler.Resource.SamplerState.Resource);
            }

            // Free up the texture bindings of the samplers currently being used
            for (int i = material.Samplers.Count; i < mUsedSamplers; ++i)
            {
                SetTexture(i, Textures.InvalidTexture);
            }

            mUsedSamplers = material.Samplers.Count;

            material.VertexShader.Resource.Bind();
            material.PixelShader.Resource.Bind();
        }

        public void SetVertexShaderConstant(int startRegister, StandardSlotSemantic semantic)
        {
            Matrix4x4 matrix;
            switch (semantic)
            {
                case StandardSlotSemantic.ViewProjMatrix:
                    matrix = mCamera.GetViewProj();
                    SetVertexShaderConstants(startRegister, ref matrix);
                    break;
                case StandardSlotSemantic.UiProjMatrix:
                    matrix = mCamera.GetUiProjection();
                    SetVertexShaderConstants(startRegister, ref matrix);
                    break;
            }
        }

        public void SetPixelShaderConstant(int startRegister, StandardSlotSemantic semantic)
        {
            Span<Matrix4x4> matrix = stackalloc Matrix4x4[1];
            switch (semantic)
            {
                case StandardSlotSemantic.ViewProjMatrix:
                    matrix[0] = mCamera.GetViewProj();
                    SetPixelShaderConstants<Matrix4x4>(startRegister, matrix);
                    break;
                case StandardSlotSemantic.UiProjMatrix:
                    matrix[0] = mCamera.GetUiProjection();
                    SetPixelShaderConstants<Matrix4x4>(startRegister, matrix);
                    break;
            }
        }

        public void SetRasterizerState(RasterizerState state)
        {
            if (currentRasterizerState == state)
            {
                return; // Already set
            }

            currentRasterizerState = state;
            mContext.Rasterizer.State = state.GpuState;
        }

        public void SetBlendState(BlendState state)
        {
            if (currentBlendState == state)
            {
                return; // Already set
            }

            currentBlendState = state;
            mContext.OutputMerger.BlendState = state.GpuState;
        }

        public void SetDepthStencilState(DepthStencilState state)
        {
            if (currentDepthStencilState == state)
            {
                return; // Already set
            }

            currentDepthStencilState = state;
            mContext.OutputMerger.DepthStencilState = state.GpuState;
        }

        public void SetSamplerState(int samplerIdx, SamplerState state)
        {
            var curSampler = currentSamplerState[samplerIdx];
            if (curSampler == state)
            {
                return; // Already set
            }

            currentSamplerState[samplerIdx] = state;

            mContext.PixelShader.SetSampler(samplerIdx, state.GpuState);
        }

        public void SetTexture(int slot, ITexture texture)
        {
            // If we are binding a multisample render target, we automatically resolve the MSAA to use
            // a non-MSAA texture like a normal texture
            if (texture.Type == TextureType.RenderTarget)
            {
                var rt = (RenderTargetTexture) texture;

                if (rt.IsMultiSampled)
                {
                    var mDesc = rt.Texture.Description;

                    mContext.ResolveSubresource(
                        rt.Texture,
                        0,
                        rt.ResolvedTexture,
                        0,
                        mDesc.Format
                    );
                }
            }

            // D3D11
            var resourceView = texture.GetResourceView();
            mContext.PixelShader.SetShaderResource(slot, resourceView);
        }

        public void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            mContext.InputAssembler.SetIndexBuffer(indexBuffer.Buffer, indexBuffer.Format, 0);
        }

        public void Draw(PrimitiveType type, int vertexCount, int startVertex = 0)
        {
            PrimitiveTopology primTopology;

            switch (type)
            {
                case PrimitiveType.TriangleStrip:
                    primTopology = PrimitiveTopology.TriangleStrip;
                    break;
                case PrimitiveType.TriangleList:
                    primTopology = PrimitiveTopology.TriangleList;
                    break;
                case PrimitiveType.LineStrip:
                    primTopology = PrimitiveTopology.LineStrip;
                    break;
                case PrimitiveType.LineList:
                    primTopology = PrimitiveTopology.LineList;
                    break;
                case PrimitiveType.PointList:
                    primTopology = PrimitiveTopology.PointList;
                    break;
                default:
                    throw new GfxException("Unsupported primitive type");
            }

            mContext.InputAssembler.PrimitiveTopology = primTopology;
            mContext.Draw(vertexCount, startVertex);
        }

        public void DrawIndexed(PrimitiveType type, int vertexCount, int indexCount, int startVertex = 0,
            int vertexBase = 0)
        {
            PrimitiveTopology primTopology;

            switch (type)
            {
                case PrimitiveType.TriangleStrip:
                    primTopology = PrimitiveTopology.TriangleStrip;
                    break;
                case PrimitiveType.TriangleList:
                    primTopology = PrimitiveTopology.TriangleList;
                    break;
                case PrimitiveType.LineStrip:
                    primTopology = PrimitiveTopology.LineStrip;
                    break;
                case PrimitiveType.LineList:
                    primTopology = PrimitiveTopology.LineList;
                    break;
                case PrimitiveType.PointList:
                    primTopology = PrimitiveTopology.PointList;
                    break;
                default:
                    throw new GfxException("Unsupported primitive type");
            }

            mContext.InputAssembler.PrimitiveTopology = primTopology;
            mContext.DrawIndexed(indexCount, startVertex, vertexBase);
        }

        /*
          Changes the currently used cursor to the given surface.
         */
        public void SetCursor(int hotspotX, int hotspotY, string imagePath)
        {
            if (!cursorCache.TryGetValue(imagePath, out var cursor))
            {
                var textureData = _fs.ReadBinaryFile(imagePath);
                try
                {
                    cursor = IO.Images.ImageIO.LoadImageToCursor(textureData, hotspotX, hotspotY);
                }
                catch (Exception e)
                {
                    cursor = IntPtr.Zero;
                    Logger.Error("Failed to load cursor {0}: {1}", imagePath, e);
                }

                cursorCache[imagePath] = cursor;
            }

            Cursor.SetCursor(mWindowHandle, cursor);
            currentCursor = cursor;
        }

        public void ShowCursor()
        {
            if (currentCursor != IntPtr.Zero)
            {
                Cursor.SetCursor(mWindowHandle, currentCursor);
            }
        }

        public void HideCursor()
        {
            Cursor.HideCursor(mWindowHandle);
        }

        /*
            Take a screenshot with the given size. The image will be stretched
            to the given size.
        */
        public void TakeScaledScreenshot(string filename, int width, int height, int quality = 90)
        {
            var currentTarget = GetCurrentRenderTargetColorBuffer();
            TakeScaledScreenshot(currentTarget, filename, width, height, quality);
        }

        public void TakeScaledScreenshot(RenderTargetTexture renderTarget,
            string filename, int width = 0, int height = 0, int quality = 90)
        {
            annotation?.SetMarker("TakeScaledScreenshot");

            Logger.Debug("Creating screenshot with size {0}x{1} in {2}", width, height,
                filename);

            var targetSize = renderTarget.GetSize();

            // Support taking unscaled screenshots
            var stretch = true;
            if (width == 0 || height == 0)
            {
                width = targetSize.Width;
                height = targetSize.Height;
                stretch = false;
            }

            // Retrieve the backbuffer format...
            var currentTargetDesc = renderTarget.Texture.Description;

            // Create a staging surface for copying pixels back from the backbuffer
            // texture
            var stagingDesc = currentTargetDesc;
            stagingDesc.Width = width;
            stagingDesc.Height = height;
            stagingDesc.Usage = ResourceUsage.Staging;
            stagingDesc.BindFlags = 0; // Not going to bind it at all
            stagingDesc.CpuAccessFlags = CpuAccessFlags.Read;
            stagingDesc.MipLevels = 1;
            stagingDesc.ArraySize = 1;
            // Never use multi sampling for the screenshot
            stagingDesc.SampleDescription.Count = 1;
            stagingDesc.SampleDescription.Quality = 0;

            using var stagingTex = new Texture2D(mD3d11Device, stagingDesc);
            stagingTex.DebugName = "ScreenshotStagingTex";

            if (stretch)
            {
                // Create a default texture to copy the current RT to that we can use as a src for the blitting
                Texture2DDescription tmpDesc = currentTargetDesc;
                // Force MSAA off
                tmpDesc.SampleDescription.Count = 1;
                tmpDesc.SampleDescription.Quality = 0;
                // Make it a default texture with binding as Shader Resource
                tmpDesc.Usage = ResourceUsage.Default;
                tmpDesc.BindFlags = BindFlags.ShaderResource;

                using var tmpTexture = new Texture2D(mD3d11Device, tmpDesc);
                tmpTexture.DebugName = "ScreenshotTmpTexture";

                // Copy/resolve the current RT into the temp texture
                if (currentTargetDesc.SampleDescription.Count > 1)
                {
                    mContext.ResolveSubresource(renderTarget.Texture, 0, tmpTexture, 0, tmpDesc.Format);
                }
                else
                {
                    // SharpDX just reversed these arguments. WTF.
                    mContext.CopyResource(renderTarget.Texture, tmpTexture);
                }

                // Create the Shader Resource View that we can use to use the tmp texture for sampling in a shader
                var srvDesc = new ShaderResourceViewDescription();
                srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                srvDesc.Texture2D.MipLevels = 1;
                var srv = new ShaderResourceView(mD3d11Device, tmpTexture, srvDesc);

                // Create our own wrapper so we can use the standard rendering functions
                var tmpSize = new Size((int) currentTargetDesc.Width, (int) currentTargetDesc.Height);
                var tmpTexWrapper = new DynamicTexture(this,
                    tmpTexture,
                    srv,
                    tmpSize,
                    4);

                // Create a texture the size of the target and stretch into it via a blt
                // the target also needs to be a render target for that to work
                using var stretchedRt = CreateRenderTargetTexture(renderTarget.Format, width, height);

                PushRenderTarget(stretchedRt.Resource, null);
                ShapeRenderer2d renderer = new ShapeRenderer2d(this);

                var w = (float) mCamera.GetScreenWidth();
                var h = (float) mCamera.GetScreenHeight();
                renderer.DrawRectangle(0, 0, w, h, tmpTexWrapper);

                PopRenderTarget();

                // Copy our stretchted RT to the staging resource
                mContext.CopyResource(stretchedRt.Resource.Texture, stagingTex);
            }
            else
            {
                // Resolve multi sampling if necessary
                if (currentTargetDesc.SampleDescription.Count > 1)
                {
                    mContext.ResolveSubresource(renderTarget.Texture, 0, stagingTex, 0, stagingDesc.Format);
                }
                else
                {
                    mContext.CopyResource(renderTarget.Texture, stagingTex);
                }
            }

            // Lock the resource and retrieve it
            var mapped = mContext.MapSubresource(
                stagingTex,
                0,
                SharpDX.Direct3D11.MapMode.Read,
                0,
                out _
            );

            // Clamp quality to [1, 100]
            quality = Math.Min(100, Math.Max(1, quality));

            byte[] jpegData;
            unsafe
            {
                Span<byte> mappedData = new Span<byte>((void*) mapped.DataPointer, height * mapped.RowPitch);

                jpegData = IO.Images.ImageIO.EncodeJpeg(mappedData,
                    JpegPixelFormat.BGRX, width, height,
                    quality, mapped.RowPitch);
            }

            mContext.UnmapSubresource(stagingTex, 0);

            // We have to write using tio or else it goes god knows where
            try
            {
                File.WriteAllBytes(filename, jpegData);
            }
            catch (Exception e)
            {
                Logger.Error("Unable to save screenshot due to an IO error: {0}", e);
            }
        }

        // Creates a buffer binding for a MDF material that
        // is preinitialized with the correct shader
        public BufferBinding CreateMdfBufferBinding()
        {
            var vs = GetShaders().LoadVertexShader("mdf_vs", new Dictionary<string, string>
            {
                {"TEXTURE_STAGES", "1"} // Necessary so the input struct gets the UVs
            });

            return new BufferBinding(this, vs);
        }

        public Shaders GetShaders()
        {
            return mShaders;
        }

        public Textures GetTextures()
        {
            return mTextures;
        }

        public WorldCamera GetCamera()
        {
            return mCamera;
        }

        public void SetAntiAliasing(bool enable, int samples, int quality)
        {
            msaaQuality = quality;
            msaaSamples = samples;

            if (antiAliasing != enable)
            {
                antiAliasing = enable;

                // Recreate all rasterizer states to set the multisampling flag accordingly
                foreach (var entry in rasterizerStates)
                {
                    var gpuState = entry.Value;
                    var gpuDesc = gpuState.Resource.GpuState.Description;

                    gpuDesc.IsMultisampleEnabled = enable;

                    entry.Value.Resource.GpuState.Dispose();
                    entry.Value.Resource.GpuState = new SharpDX.Direct3D11.RasterizerState(mD3d11Device, gpuDesc);
                }
            }
        }

        public void UpdateBuffer<T>(VertexBuffer buffer, ReadOnlySpan<T> data) where T : struct
        {
            UpdateResource(buffer.Buffer, MemoryMarshal.Cast<T, byte>(data));
        }

        public unsafe void UpdateBuffer(VertexBuffer buffer, void* data, int size)
        {
            UpdateResource(buffer.Buffer, new ReadOnlySpan<byte>(data, size));
        }

        public void UpdateBuffer(IndexBuffer buffer, ReadOnlySpan<ushort> data)
        {
            UpdateResource(buffer.Buffer, MemoryMarshal.Cast<ushort, byte>(data));
        }

        private static SharpDX.Direct3D11.MapMode ConvertMapMode(MapMode mapMode)
        {
            switch (mapMode)
            {
                case MapMode.Read:
                    return SharpDX.Direct3D11.MapMode.Read;
                case MapMode.Discard:
                    return SharpDX.Direct3D11.MapMode.WriteDiscard;
                case MapMode.NoOverwrite:
                    return SharpDX.Direct3D11.MapMode.WriteNoOverwrite;
                default:
                    throw new GfxException("Unknown map type");
            }
        }

        public MappedBuffer<TElement> Map<TElement>(VertexBuffer buffer, MapMode mode = MapMode.Discard)
            where TElement : struct
        {
            var data = MapRaw(buffer.Buffer, buffer.Size, mode);
            var castData = MemoryMarshal.Cast<byte, TElement>(data);
            return new MappedBuffer<TElement>(buffer.Buffer, mContext, castData, 0);
        }

        public void Unmap(VertexBuffer buffer)
        {
            mContext.UnmapSubresource(buffer.Buffer, 0);
        }

        // Index buffer memory mapping techniques
        public MappedBuffer<ushort> Map(IndexBuffer buffer, MapMode mode = MapMode.Discard)
        {
            var data = MapRaw(buffer.Buffer, buffer.Count * sizeof(ushort), mode);
            var castData = MemoryMarshal.Cast<byte, ushort>(data);
            return new MappedBuffer<ushort>(buffer.Buffer, mContext, castData, 0);
        }

        public void Unmap(IndexBuffer buffer)
        {
            mContext.UnmapSubresource(buffer.Buffer, 0);
        }

        public unsafe MappedBuffer<byte> Map(DynamicTexture texture, MapMode mode = MapMode.Discard)
        {
            var mapMode = ConvertMapMode(mode);

            var mapped = mContext.MapSubresource(texture.mTexture, 0, 0, mapMode, 0, out _);

            var size = texture.GetSize().Height * mapped.RowPitch;
            var data = new Span<byte>((void*) mapped.DataPointer, size);
            var rowPitch = mapped.RowPitch;

            return new MappedBuffer<byte>(texture.mTexture, mContext, data, rowPitch);
        }

        public void Unmap(DynamicTexture texture)
        {
            mContext.UnmapSubresource(texture.mTexture, 0);
        }

        public const int MaxVsConstantBufferSize = 2048;

        public void SetVertexShaderConstants<T>(int slot, ref T buffer) where T : unmanaged
        {
            ReadOnlySpan<T> span = MemoryMarshal.CreateReadOnlySpan(ref buffer, 1);
            ReadOnlySpan<byte> rawSpan = MemoryMarshal.Cast<T, byte>(span);

            Trace.Assert(rawSpan.Length <= MaxVsConstantBufferSize, "Constant buffer exceeds maximum size");
            UpdateResource(mVsConstantBuffer, rawSpan);
            VSSetConstantBuffer(slot, mVsConstantBuffer);
        }

        public const int MaxPsConstantBufferSize = 512;

        public void SetPixelShaderConstants<T>(int slot, ReadOnlySpan<T> buffer) where T : unmanaged
        {
            var rawSpan = MemoryMarshal.Cast<T, byte>(buffer);
            Trace.Assert(rawSpan.Length <= MaxPsConstantBufferSize, "Constant buffer exceeds maximum size");
            UpdateResource(mPsConstantBuffer, rawSpan);
            PSSetConstantBuffer(slot, mPsConstantBuffer);
        }

        public Size GetBackBufferSize() => mBackBufferNew.Resource.GetSize();

        // Pushes the back buffer and it's depth buffer as the current render target
        public void PushBackBufferRenderTarget()
        {
            PushRenderTarget(mBackBufferNew.Resource, mBackBufferDepthStencil.Resource);
        }

        public void PushRenderTarget(
            RenderTargetTexture colorBuffer,
            RenderTargetDepthStencil depthStencilBuffer
        )
        {
            // If a depth stencil surface is to be used, it HAS to be the same size
            Trace.Assert(depthStencilBuffer == null ||
                         colorBuffer.GetSize() == depthStencilBuffer.Size);

            // Set the camera size to the size of the new render target
            var size = colorBuffer.GetSize();
            mCamera.SetScreenWidth((float) size.Width, (float) size.Height);

            // Activate the render target on the device
            var rtv = colorBuffer.RenderTargetView;
            DepthStencilView depthStencilView = null; // Optional!
            if (depthStencilBuffer != null)
            {
                depthStencilView = depthStencilBuffer.DsView;
            }

            mContext.OutputMerger.SetRenderTargets(depthStencilView, rtv);
            textEngine.SetRenderTarget(colorBuffer.Texture);

            // Set the viewport accordingly
            var viewport = new RawViewportF();
            viewport.Width = size.Width;
            viewport.Height = size.Height;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 1.0f;
            mContext.Rasterizer.SetViewports(new[] {viewport}, 1);

            mRenderTargetStack.Push(new RenderTarget(colorBuffer, depthStencilBuffer));

            ResetScissorRect();
        }

        public void PopRenderTarget()
        {
            // The last targt should NOT be popped, if the backbuffer was auto-pushed
            if (mBackBufferNew.Resource != null)
            {
                Trace.Assert(mRenderTargetStack.Count > 1);
            }

            var poppedTarget = mRenderTargetStack.Pop();
            poppedTarget.colorBuffer.Dispose();
            poppedTarget.depthStencilBuffer.Dispose();

            if (mRenderTargetStack.Count == 0)
            {
                mContext.OutputMerger.SetRenderTargets(null, new RenderTargetView[0]);
                textEngine.SetRenderTarget(null);
                return;
            }

            var newTarget = mRenderTargetStack.Peek();

            // Set the camera size to the size of the new render target
            var size = newTarget.colorBuffer.Resource.GetSize();
            mCamera.SetScreenWidth((float) size.Width, (float) size.Height);

            // Activate the render target on the device
            var rtv = newTarget.colorBuffer.Resource.RenderTargetView;
            DepthStencilView depthStencilView = null; // Optional!
            if (newTarget.depthStencilBuffer.Resource != null)
            {
                depthStencilView = newTarget.depthStencilBuffer.Resource.DsView;
            }

            mContext.OutputMerger.SetRenderTargets(depthStencilView, rtv);
            textEngine.SetRenderTarget(newTarget.colorBuffer.Resource.Texture);

            // Set the viewport accordingly
            var viewport = new RawViewportF();
            viewport.Width = size.Width;
            viewport.Height = size.Height;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 1.0f;
            mContext.Rasterizer.SetViewports(new[] {viewport}, 1);

            ResetScissorRect();
        }

        public RenderTargetTexture GetCurrentRenderTargetColorBuffer()
        {
            return mRenderTargetStack.Peek().colorBuffer.Resource;
        }

        public RenderTargetDepthStencil GetCurrentRenderTargetDepthStencilBuffer()
        {
            return mRenderTargetStack.Peek().depthStencilBuffer.Resource;
        }

        public int AddResizeListener(ResizeListener listener)
        {
            var newKey = ++mResizeListenersKey;
            mResizeListeners[newKey] = listener;
            return newKey;
        }

        public bool IsDebugDevice() => debugDevice;

        /**
         * Emits the start of a rendering call group if the debug device is being used.
         * This information can be used in the graphic debugger.
         */
        [StringFormatMethod("format")]
        public void BeginPerfGroup(string format, params object[] args)
        {
            if (IsDebugDevice())
            {
                BeginPerfGroupInternal(string.Format(format, args));
            }
        }

        [StringFormatMethod("format")]
        public PerfGroup CreatePerfGroup(string format, params object[] args)
        {
            BeginPerfGroup(format, args);
            return new PerfGroup(this);
        }

        /// <summary>
        /// Ends a previously started performance group.
        /// </summary>
        public void EndPerfGroup()
        {
            if (debugDevice)
            {
                annotation?.EndEvent();
            }
        }

        public TextEngine GetTextEngine() => textEngine;

        public void Dispose()
        {
            mD3d11Device1?.Dispose();
            mD3d11Device1 = null;

            mD3d11Device?.Dispose();
            mD3d11Device = null;

            mDxgiFactory?.Dispose();
            mDxgiFactory = null;
        }

        private void BeginPerfGroupInternal(string message)
        {
            annotation?.BeginEvent(message);
        }

        internal void RemoveResizeListener(int key)
        {
            mResizeListeners.Remove(key);
        }

        internal void AddResourceListener(IResourceLifecycleListener listener)
        {
            mResourcesListeners.Add(listener);
            if (mResourcesCreated)
            {
                listener.CreateResources(this);
            }
        }

        internal void RemoveResourceListener(IResourceLifecycleListener listener)
        {
            mResourcesListeners.Remove(listener);
            if (mResourcesCreated)
            {
                listener.FreeResources(this);
            }
        }

        private unsafe void UpdateResource(Resource resource, ReadOnlySpan<byte> data)
        {
            var mapped = mContext.MapSubresource(resource, 0, SharpDX.Direct3D11.MapMode.WriteDiscard,
                0, out var stream);

            try
            {
                var dest = new Span<byte>((void*) mapped.DataPointer, (int) stream.Length);
                data.CopyTo(dest);
            }
            finally
            {
                mContext.UnmapSubresource(resource, 0);
            }
        }

        private Buffer CreateConstantBuffer<T>(ReadOnlySpan<T> initialData) where T : struct
        {
            var rawData = MemoryMarshal.Cast<T, byte>(initialData);

            var bufferDesc = new BufferDescription(
                rawData.Length,
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                0,
                0
            );

            unsafe
            {
                fixed (byte* rawDataPtr = rawData)
                {
                    return new Buffer(mD3d11Device, (IntPtr) rawDataPtr, bufferDesc);
                }
            }
        }

        private Buffer CreateEmptyConstantBuffer(int initialSize)
        {
            var bufferDesc = new BufferDescription(
                initialSize,
                ResourceUsage.Dynamic,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.Write,
                0,
                0
            );

            return new Buffer(mD3d11Device, bufferDesc);
        }

        private void VSSetConstantBuffer(int slot, Buffer buffer)
        {
            mContext.VertexShader.SetConstantBuffer(slot, buffer);
        }

        private void PSSetConstantBuffer(int slot, Buffer buffer)
        {
            mContext.PixelShader.SetConstantBuffer(slot, buffer);
        }

        private unsafe Span<byte> MapRaw(Resource buffer, int bufferSize, MapMode mode)
        {
            var mapMode = ConvertMapMode(mode);

            var mapped = mContext.MapSubresource(buffer, 0, mapMode, 0);

            return new Span<byte>((void*) mapped.DataPointer, bufferSize);
        }

        private Adapter1 GetAdapter(int index)
        {
            return mDxgiFactory.GetAdapter1(index);
        }

        private int mBeginSceneDepth = 0;

        private IntPtr mWindowHandle;

        private Factory1 mDxgiFactory;

        // The DXGI adapter we use
        private Adapter1 mAdapter;

        // D3D11 device and related
        internal D3D11Device mD3d11Device;
        private SharpDX.Direct3D11.Device1 mD3d11Device1;
        private SwapChainDescription mSwapChainDesc;
        private SharpDX.DXGI.SwapChain _swapChain;
        internal SharpDX.Direct3D11.DeviceContext mContext;
        private ResourceRef<RenderTargetTexture> mBackBufferNew;
        private ResourceRef<RenderTargetDepthStencil> mBackBufferDepthStencil;

        struct RenderTarget
        {
            public ResourceRef<RenderTargetTexture> colorBuffer;
            public ResourceRef<RenderTargetDepthStencil> depthStencilBuffer;

            public RenderTarget(RenderTargetTexture colorBuffer, RenderTargetDepthStencil depthStencilBuffer)
            {
                this.colorBuffer = default;
                this.depthStencilBuffer = default;

                if (colorBuffer != null)
                {
                    this.colorBuffer = new ResourceRef<RenderTargetTexture>(colorBuffer);
                }

                if (depthStencilBuffer != null)
                {
                    this.depthStencilBuffer = new ResourceRef<RenderTargetDepthStencil>(depthStencilBuffer);
                }
            }
        };

        private Stack<RenderTarget> mRenderTargetStack = new Stack<RenderTarget>(16);

        private FeatureLevel mFeatureLevel = FeatureLevel.Level_9_1;

        private List<DisplayDevice> mDisplayDevices;

        private Buffer mVsConstantBuffer;
        private Buffer mPsConstantBuffer;

        private Dictionary<int, ResizeListener> mResizeListeners = new Dictionary<int, ResizeListener>();
        private int mResizeListenersKey = 0;

        private List<IResourceLifecycleListener> mResourcesListeners = new List<IResourceLifecycleListener>();
        private bool mResourcesCreated = false;


        private TimePoint mLastFrameStart = TimePoint.Now;
        private TimePoint mDeviceCreated = TimePoint.Now;

        private int mUsedSamplers = 0;

        private Shaders mShaders;
        private Textures mTextures;
        private WorldCamera mCamera = new WorldCamera();

        // Anti Aliasing Settings
        private bool antiAliasing = false;
        private int msaaSamples = 4;
        private int msaaQuality = 0;

        // Caches for cursors
        private Dictionary<string, IntPtr> cursorCache = new Dictionary<string, IntPtr>();
        private IntPtr currentCursor = IntPtr.Zero;

        // Caches for created device states
        private SamplerState[] currentSamplerState = new SamplerState[4];

        private Dictionary<SamplerSpec, ResourceRef<SamplerState>> samplerStates =
            new Dictionary<SamplerSpec, ResourceRef<SamplerState>>();

        private DepthStencilState currentDepthStencilState = null;

        private Dictionary<DepthStencilSpec, ResourceRef<DepthStencilState>> depthStencilStates =
            new Dictionary<DepthStencilSpec, ResourceRef<DepthStencilState>>();

        private BlendState currentBlendState = null;

        private Dictionary<BlendSpec, ResourceRef<BlendState>> blendStates =
            new Dictionary<BlendSpec, ResourceRef<BlendState>>();

        private RasterizerState currentRasterizerState = null;

        private Dictionary<RasterizerSpec, ResourceRef<RasterizerState>> rasterizerStates =
            new Dictionary<RasterizerSpec, ResourceRef<RasterizerState>>();

        // Debugging related
        private bool debugDevice = false;
        private UserDefinedAnnotation annotation;

        // Text rendering (Direct2D integration)
        private TextEngine textEngine;

    }

    public delegate void ResizeListener(int w, int h);

    public class MaterialSamplerSpec
    {
        public ResourceRef<ITexture> texture;
        public SamplerSpec samplerSpec;

        public MaterialSamplerSpec(ResourceRef<ITexture> texture, SamplerSpec samplerSpec)
        {
            this.texture = texture;
            this.samplerSpec = samplerSpec;
        }
    }
}