using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
using CullMode = SharpDX.Direct3D11.CullMode;
using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11InfoQueue = SharpDX.Direct3D11.InfoQueue;
using DeviceChild = SharpDX.Direct3D11.DeviceChild;
using DXGIDevice = SharpDX.DXGI.Device;
using Resource = SharpDX.Direct3D11.Resource;
using TextEngine = OpenTemple.Core.GFX.TextRendering.TextEngine;

namespace OpenTemple.Core.GFX;

public enum MapMode
{
    Read,
    Discard,
    NoOverwrite
}

public class RenderingDevice : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly IOutputSurface _outputSurface;

    private SizeF _uiCanvasSize;

    private Matrix4x4 _uiProjection;

    public ref Matrix4x4 UiProjection => ref _uiProjection;

    /// <summary>
    /// Sets the UI canvas size that will be used as the coordinate system by drawing operations that
    /// refer to UI coordinates.
    /// </summary>
    public SizeF UiCanvasSize
    {
        get => _uiCanvasSize;
        set
        {
            _uiCanvasSize = value;
            _uiProjection = CreateUIProjection(value.Width, value.Height);
            _textEngine.CanvasSize = value;
        }
    }

    private int _drawDepth;

    private readonly Factory1 _dxgiFactory;

    // D3D11 device and related
    public D3D11Device Device { get; private set; }

    private SharpDX.Direct3D11.Device1? _d3d11Device1;

    private readonly DeviceContext _context;
    private ResourceRef<RenderTargetTexture> _backBuffer;
    private ResourceRef<RenderTargetDepthStencil> _backBufferDepthStencil;

    internal DeviceContext Context => _context;

    private readonly Shaders _shaders;
    private readonly Textures _textures;
    private readonly Stack<RenderTarget> _renderTargetStack = new(16);

    private List<DisplayDevice>? _displayDevices;

    private readonly Buffer _vsConstantBuffer;
    private readonly Buffer _psConstantBuffer;

    private readonly Dictionary<int, ResizeListener> _resizeListeners = new();
    private int _resizeListenersKey;

    private readonly List<IResourceLifecycleListener> _resourcesListeners = new();
    private bool _resourcesCreated;

    private TimePoint _lastFrameStart = TimePoint.Now;
    private readonly TimePoint _deviceCreated = TimePoint.Now;

    private int _usedSamplers;

    // Caches for created device states
    private readonly SamplerState[] _currentSamplerState = new SamplerState[4];

    private readonly Dictionary<SamplerSpec, ResourceRef<SamplerState>> _samplerStates = new();

    private DepthStencilState _currentDepthStencilState;

    private readonly Dictionary<DepthStencilSpec, ResourceRef<DepthStencilState>> _depthStencilStates = new();

    private BlendState _currentBlendState;

    private readonly Dictionary<BlendSpec, ResourceRef<BlendState>> _blendStates = new();

    private RasterizerState _currentRasterizerState;

    private readonly Dictionary<RasterizerSpec, ResourceRef<RasterizerState>> _rasterizerStates = new();

    // Debugging related
    private readonly bool _debugDevice;
    private readonly UserDefinedAnnotation? _annotation;

    // Text rendering (Direct2D integration)
    private readonly TextEngine _textEngine;

    private readonly FullQuadRenderer _fullQuadRenderer;

    public RenderingDevice(IFileSystem fs, IMainWindow mainWindow, int adapterIdx = 0, bool debugDevice = false)
    {
        _shaders = new Shaders(fs, this);
        _textures = new Textures(fs, this, 128 * 1024 * 1024);
        _debugDevice = debugDevice;

        _dxgiFactory = new Factory1();

        var displayDevices = GetDisplayDevices();

        // Find the adapter selected by the user, although we might fall back to the
        // default one if the user didn't select one or the adapter selection changed
        var adapter = GetAdapter(adapterIdx);
        if (adapter == null)
        {
            // Fall back to default
            Logger.Error("Couldn't retrieve adapter #{0}. Falling back to default", 0);
            adapter = GetAdapter(displayDevices[0].id);
            if (adapter == null)
            {
                throw new GfxException(
                    "Couldn't retrieve your configured graphics adapter, but also couldn't fall back to the default adapter.");
            }
        }

        // Required for the Direct2D support
        var deviceFlags = DeviceCreationFlags.BgraSupport;
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
            if (mainWindow is HeadlessMainWindow)
            {
                Logger.Info("Creating headless WARP device");
                Device = new D3D11Device(
                    DriverType.Warp,
                    deviceFlags,
                    requestedLevels
                );
            }
            else
            {
                Logger.Info("Creating D3D11 device on {0}", adapter.Description1.Description);
                Device = new D3D11Device(adapter, deviceFlags, requestedLevels);
            }
        }
        catch (SharpDXException e)
        {
            // DXGI_ERROR_SDK_COMPONENT_MISSING
            if (debugDevice && unchecked((uint) e.ResultCode.Code) == 0x887A002D)
            {
                throw new GfxException("To use the D3D debugging feature, you need to " +
                                       "install the corresponding Windows SDK component.");
            }

            throw new GfxException("Unable to create a Direct3D 11 device.", e);
        }

        _context = Device.ImmediateContext;

        Logger.Info("Created D3D11 device with feature level {0}", Device.FeatureLevel);

        if (debugDevice)
        {
            // Retrieve the interface used to emit event groupings for debugging
            _annotation = _context.QueryInterfaceOrNull<UserDefinedAnnotation>();
        }

        // Retrieve DXGI device
        using DXGIDevice dxgiDevice = Device.QueryInterfaceOrNull<DXGIDevice>();
        if (dxgiDevice == null)
        {
            throw new GfxException("Couldn't retrieve DXGI device from D3D11 device.");
        }

        using Adapter dxgiAdapter = dxgiDevice.GetParent<Adapter>();
        _dxgiFactory = dxgiAdapter.GetParent<Factory1>(); // Hang on to the DXGI factory used here

        // Create 2D rendering
        _textEngine = new TextEngine(Device.NativePointer, debugDevice);

        if (mainWindow is MainWindow outputWindow)
        {
            _outputSurface = new WindowOutputSurface(Device, outputWindow.NativeHandle);
        }
        else if (mainWindow is HeadlessMainWindow)
        {
            _outputSurface = new OffScreenOutputSurface();
        }
        else
        {
            throw new ArgumentException("Unknown main window type: " + mainWindow.GetType());
        }

        _backBuffer = _outputSurface.CreateBackBuffer(this);
        var backBufferSize = _backBuffer.Resource.GetSize();
        _backBufferDepthStencil = CreateRenderTargetDepthStencil(backBufferSize.Width, backBufferSize.Height);

        // Push back the initial render target that should never be removed
        PushBackBufferRenderTarget();

        // Create centralized constant buffers for the vertex and pixel shader stages
        _vsConstantBuffer = CreateEmptyConstantBuffer(MaxVsConstantBufferSize);
        SetDebugName(_vsConstantBuffer, "VsConstantBuffer");
        _psConstantBuffer = CreateEmptyConstantBuffer(MaxPsConstantBufferSize);
        SetDebugName(_psConstantBuffer, "PsConstantBuffer");

        // TODO: color bullshit is not yet done (tig_d3d_init_handleformat et al)

        foreach (var listener in _resourcesListeners)
        {
            listener.CreateResources(this);
        }

        _resourcesCreated = true;

        // This is only relevant if we are in windowed mode
        mainWindow.Resized += size => ResizeBuffers();
        mainWindow.UiCanvasSizeChanged += () => UiCanvasSize = mainWindow.UiCanvasSize;
        UiCanvasSize = mainWindow.UiCanvasSize;

        _fullQuadRenderer = new FullQuadRenderer(this);
    }

    public bool BeginDraw()
    {
        if (_drawDepth++ > 0)
        {
            return true;
        }

        ClearCurrentColorTarget(new LinearColorA(0, 0, 0, 1));
        ClearCurrentDepthTarget();

        return true;
    }

    public bool EndDraw()
    {
        --_drawDepth;
        return true;
    }

    /// <summary>
    /// Indicates that rendering a new frame is beginning.
    /// </summary>
    public void BeginFrame()
    {
        Debug.Assert(_drawDepth == 0);
        _lastFrameStart = TimePoint.Now;
    }

    public void Present()
    {
        _textures.FreeUnusedTextures();

        _outputSurface.Present(this);
    }

    public void Flush()
    {
        _context.Flush();
    }

    public void ClearCurrentColorTarget(LinearColorA color)
    {
        var target = GetCurrentRenderTargetColorBuffer();

        // Clear the current render target view
        if (target != null)
        {
            _context.ClearRenderTargetView(target.RenderTargetView, color);
        }
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

        _context.ClearDepthStencilView(depthStencil.DsView, flags, depthValue,
            stencilValue);
    }

    public TimePoint GetLastFrameStart() => _lastFrameStart;
    public TimePoint GetDeviceCreated() => _deviceCreated;

    public List<DisplayDevice> GetDisplayDevices()
    {
        // Recreate the DXGI factory if we want to enumerate a new list of devices
        if (_displayDevices != null && _dxgiFactory.IsCurrent)
        {
            return _displayDevices;
        }

        // Enumerate devices
        Logger.Info("Enumerating DXGI display devices...");

        _displayDevices = new List<DisplayDevice>();

        int adapterCount = _dxgiFactory.GetAdapterCount1();
        for (int adapterIdx = 0; adapterIdx < adapterCount; adapterIdx++)
        {
            using var adapter = _dxgiFactory.GetAdapter1(adapterIdx);

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

                var monitorName = GetMonitorName(ref outputDesc);

                DisplayDeviceOutput displayOutput = new DisplayDeviceOutput();
                displayOutput.id = deviceName;
                displayOutput.name = monitorName;
                Logger.Info("  Output #{0} Device '{1}' Monitor '{2}'", outputIdx,
                    deviceName, displayOutput.name);
                displayDevice.outputs.Add(displayOutput);
            }

            if (displayDevice.outputs.Count > 0)
            {
                _displayDevices.Add(displayDevice);
            }
            else
            {
                Logger.Info("Skipping device because it has no outputs.");
            }
        }

        return _displayDevices;
    }

    private static string GetMonitorName(ref OutputDescription outputDesc)
    {
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

        return new string(monitorName[..monitorNameSize]);
    }

    [DllImport(OpenTempleLib.Path)]
    private static extern unsafe bool Win32_GetMonitorName(IntPtr monitorHandle, char* name, ref int nameSize);

    // Resize the back buffer
    private void ResizeBuffers()
    {
        if (_renderTargetStack.Count != 1)
        {
            throw new InvalidOperationException("Cannot resize backbuffer while rendering is going on!");
        }

        // All references to the current back buffer need to be freed
        _backBuffer.Dispose();
        _backBufferDepthStencil.Dispose();
        PopRenderTarget();

        // Then re-acquire the backbuffer from our output target
        _backBuffer = _outputSurface.CreateBackBuffer(this);
        var backBufferSize = _backBuffer.Resource.GetSize();
        _backBufferDepthStencil = CreateRenderTargetDepthStencil(backBufferSize.Width, backBufferSize.Height);

        // restore the render target
        PushBackBufferRenderTarget();

        // Retrieve the *actual* back buffer size since we created it to match the client area above
        var size = _backBuffer.Resource.GetSize();

        // Notice listeners about changed backbuffer size
        foreach (var entry in _resizeListeners)
        {
            entry.Value(size.Width, size.Height);
        }
    }

    public Material CreateMaterial(
        BlendSpec? blendSpec,
        DepthStencilSpec? depthStencilSpec,
        RasterizerSpec? rasterizerSpec,
        MaterialSamplerSpec[]? samplerSpecs,
        VertexShader vs,
        PixelShader ps
    )
    {
        blendSpec ??= new BlendSpec();
        depthStencilSpec ??= new DepthStencilSpec();
        rasterizerSpec ??= new RasterizerSpec();
        samplerSpecs ??= Array.Empty<MaterialSamplerSpec>();

        var blendState = CreateBlendState(blendSpec);
        var depthStencilState = CreateDepthStencilState(depthStencilSpec);
        var rasterizerState = CreateRasterizerState(rasterizerSpec);

        var samplerBindings = new List<ResourceRef<MaterialSamplerBinding>>(samplerSpecs.Length);
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
        if (_blendStates.TryGetValue(spec, out var stateRef))
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

        var gpuState = new SharpDX.Direct3D11.BlendState(Device, blendDesc);

        stateRef = new ResourceRef<BlendState>(new BlendState(this, spec, gpuState));
        _blendStates[spec] = stateRef;
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
        if (_depthStencilStates.TryGetValue(spec, out var stateRef))
        {
            return stateRef.CloneRef();
        }

        var depthStencilDesc = DepthStencilStateDescription.Default();
        depthStencilDesc.IsDepthEnabled = spec.depthEnable;
        depthStencilDesc.DepthWriteMask = spec.depthWrite
            ? DepthWriteMask.All
            : DepthWriteMask.Zero;

        depthStencilDesc.DepthComparison = ConvertComparisonFunc(spec.depthFunc);

        var gpuState = new SharpDX.Direct3D11.DepthStencilState(Device, depthStencilDesc);

        stateRef = new ResourceRef<DepthStencilState>(new DepthStencilState(this, spec, gpuState));
        _depthStencilStates[spec] = stateRef;
        return stateRef.CloneRef();
    }

    public ResourceRef<RasterizerState> CreateRasterizerState(RasterizerSpec spec)
    {
        // Check if we have a matching state already
        if (_rasterizerStates.TryGetValue(spec, out var stateRef))
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

        var gpuState = new SharpDX.Direct3D11.RasterizerState(Device, rasterizerDesc);

        rasterizerDesc.IsMultisampleEnabled = true;
        var multiSamplingGpuState = new SharpDX.Direct3D11.RasterizerState(Device, rasterizerDesc);

        stateRef = new ResourceRef<RasterizerState>(new RasterizerState(spec, gpuState, multiSamplingGpuState));
        _rasterizerStates[spec] = stateRef;
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
        if (_samplerStates.TryGetValue(spec, out var stateRef))
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

        var gpuState = new SharpDX.Direct3D11.SamplerState(Device, samplerDesc);

        stateRef = new ResourceRef<SamplerState>(new SamplerState(this, spec, gpuState));
        _samplerStates[spec] = stateRef;
        return stateRef.CloneRef();
    }

    // Changes the current scissor rect to the given rectangle
    public void SetUiScissorRect(float x, float y, float width, float height)
    {
        // x, y, width, height are in UI space
        var size = _renderTargetStack.Peek().Size;
        var hFactor = size.Width / _uiCanvasSize.Width;
        var vFactor = size.Height / _uiCanvasSize.Height;

        var left = (int) (x * hFactor);
        var top = (int) (y * vFactor);
        var right = (int) MathF.Ceiling((x + width) * hFactor);
        var bottom = (int) MathF.Ceiling((y + height) * vFactor);

        _context.Rasterizer.SetScissorRectangle(left, top, right, bottom);

        // Text engine deals in UI space directly
        _textEngine.SetScissorRect(x, y, width, height);
    }

    // Resets the scissor rect to the current render target's size
    public void ResetScissorRect()
    {
        var size = _renderTargetStack.Peek().Size;
        _context.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);

        _textEngine.ResetScissorRect();
    }

    public ResourceRef<IndexBuffer> CreateEmptyIndexBuffer(int count, Format format = Format.R16_UInt,
        string? debugName = null)
    {
        var bufferDesc = new BufferDescription(
            count * sizeof(ushort),
            ResourceUsage.Dynamic,
            BindFlags.IndexBuffer,
            CpuAccessFlags.Write,
            ResourceOptionFlags.None,
            0
        );

        var buffer = new Buffer(Device, IntPtr.Zero, bufferDesc);
        if (debugName != null)
        {
            SetDebugName(buffer, debugName);
        }

        return new ResourceRef<IndexBuffer>(new IndexBuffer(this, buffer, format, count));
    }

    public ResourceRef<VertexBuffer> CreateEmptyVertexBuffer(int size, bool forPoints = false,
        string? debugName = null)
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

        var buffer = new Buffer(Device, IntPtr.Zero, bufferDesc);
        if (debugName != null)
        {
            SetDebugName(buffer, debugName);
        }

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

        var textureNew = new Texture2D(Device, textureDesc);
        SetDebugName(textureNew, "DynamicTexture");

        var resourceViewDesc = new ShaderResourceViewDescription();
        resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
        resourceViewDesc.Texture2D.MipLevels = 1;

        var resourceView = new ShaderResourceView(Device, textureNew, resourceViewDesc);

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

        var textureNew = new Texture2D(Device, textureDesc);
        SetDebugName(textureNew, "DynamicStagingTexture");

        var resourceViewDesc = new ShaderResourceViewDescription();
        resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
        resourceViewDesc.Texture2D.MipLevels = 1;

        var resourceView = new ShaderResourceView(Device, textureNew, resourceViewDesc);

        return new ResourceRef<DynamicTexture>(new DynamicTexture(this, textureNew, resourceView,
            size, bytesPerPixel));
    }

    public void CopyRenderTarget(RenderTargetTexture renderTarget, DynamicTexture stagingTexture)
    {
        _context.CopyResource(renderTarget.Texture, stagingTexture._texture);
    }

    public ResourceRef<RenderTargetTexture> CreateRenderTargetTexture(BufferFormat format, int width, int height,
        MultiSampleSettings multiSample = default, string? debugName = null)
    {
        var size = new Size(width, height);

        var formatDx = ConvertFormat(format, out var bpp);

        var bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
        var sampleCount = 1;
        var sampleQuality = 0;

        if (multiSample.IsEnabled)
        {
            // If this is a multi sample render target, we cannot use it as a texture, or at least, we shouldn't
            bindFlags = BindFlags.RenderTarget;
            sampleCount = multiSample.Samples;
            sampleQuality = multiSample.Quality;
        }

        debugName ??= "RenderTarget";
        Logger.Info("Creating render target '{0}' with size {1}x{2}, format: {3}, MSAA: {4}", debugName,
            width, height, format, multiSample);

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

        var texture = new Texture2D(Device, textureDesc);
        SetDebugName(texture, debugName + "Texture");

        // Create the render target view of the backing buffer
        var rtViewDesc = new RenderTargetViewDescription();
        rtViewDesc.Dimension = RenderTargetViewDimension.Texture2D;
        rtViewDesc.Texture2D.MipSlice = 0;

        if (multiSample.IsEnabled)
        {
            rtViewDesc.Dimension = RenderTargetViewDimension.Texture2DMultisampled;
        }

        var rtView = new RenderTargetView(Device, texture, rtViewDesc);

        var srvTexture = texture;
        Texture2D resolvedTexture = null;
        if (multiSample.IsEnabled)
        {
            // We have to create another non-multisampled texture and use it for the SRV instead

            // Adapt the existing texture Desc to be a non-MSAA texture with otherwise identical properties
            textureDesc.BindFlags = BindFlags.ShaderResource;
            textureDesc.SampleDescription.Count = 1;
            textureDesc.SampleDescription.Quality = 0;

            resolvedTexture = new Texture2D(Device, textureDesc);
            SetDebugName(resolvedTexture, debugName + "ResolvedTexture");
            srvTexture = resolvedTexture;
        }

        var resourceViewDesc = new ShaderResourceViewDescription();
        resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;
        resourceViewDesc.Texture2D.MipLevels = 1;

        var resourceView = new ShaderResourceView(Device, srvTexture, resourceViewDesc);

        return new ResourceRef<RenderTargetTexture>(new RenderTargetTexture(this, texture, rtView, resolvedTexture,
            resourceView, size, multiSample.IsEnabled));
    }

    public ResourceRef<RenderTargetDepthStencil> CreateRenderTargetDepthStencil(int width, int height,
        MultiSampleSettings multiSample = default)
    {
        var descDepth = new Texture2DDescription();
        descDepth.Format = Format.D24_UNorm_S8_UInt;
        descDepth.Width = width;
        descDepth.Height = height;
        descDepth.ArraySize = 1;
        descDepth.MipLevels = 1; // Disable Mip Map generation
        descDepth.BindFlags = BindFlags.DepthStencil;

        // Enable multi sampling
        if (multiSample.IsEnabled)
        {
            descDepth.SampleDescription.Count = multiSample.Samples;
            descDepth.SampleDescription.Quality = multiSample.Quality;
        }
        else
        {
            descDepth.SampleDescription.Count = 1;
        }

        var texture = new Texture2D(Device, descDepth);
        SetDebugName(texture, "RenderTargetDepthStencilTexture");

        // Create the depth stencil view
        var depthStencilViewDesc = new DepthStencilViewDescription();
        depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2D;
        depthStencilViewDesc.Format = descDepth.Format;

        if (multiSample.IsEnabled)
        {
            depthStencilViewDesc.Dimension = DepthStencilViewDimension.Texture2DMultisampled;
        }

        var depthStencilView = new DepthStencilView(Device, texture, depthStencilViewDesc);

        var size = new Size(width, height);
        return new ResourceRef<RenderTargetDepthStencil>(
            new RenderTargetDepthStencil(this, texture, depthStencilView, size)
        );
    }

    public ResourceRef<VertexBuffer> CreateVertexBuffer<T>(ReadOnlySpan<T> data, bool immutable = true,
        string? debugName = null)
        where T : struct
    {
        return CreateVertexBufferRaw(MemoryMarshal.Cast<T, byte>(data), immutable, debugName);
    }

    public unsafe ResourceRef<VertexBuffer> CreateVertexBufferRaw(ReadOnlySpan<byte> data, bool immutable = true,
        string? debugName = null)
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
            buffer = new Buffer(Device, (IntPtr) dataPtr, bufferDesc);
        }

        if (debugName != null)
        {
            SetDebugName(buffer, debugName);
        }

        return new ResourceRef<VertexBuffer>(new VertexBuffer(this, buffer, data.Length));
    }

    public unsafe ResourceRef<IndexBuffer> CreateIndexBuffer(ReadOnlySpan<ushort> data, bool immutable = true,
        string? debugName = null)
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
            buffer = new Buffer(Device, (IntPtr) dataPtr, bufferDesc);
        }

        if (debugName != null)
        {
            SetDebugName(buffer, debugName);
        }

        return new ResourceRef<IndexBuffer>(new IndexBuffer(this, buffer, Format.R16_UInt, data.Length));
    }

    public unsafe ResourceRef<IndexBuffer> CreateIndexBuffer(ReadOnlySpan<int> data, bool immutable = true,
        string? debugName = null)
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
            buffer = new Buffer(Device, (IntPtr) dataPtr, bufferDesc);
        }

        if (debugName != null)
        {
            SetDebugName(buffer, debugName);
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
        for (int i = material.Samplers.Count; i < _usedSamplers; ++i)
        {
            SetTexture(i, Textures.InvalidTexture);
        }

        _usedSamplers = material.Samplers.Count;

        material.VertexShader.Resource.Bind();
        material.PixelShader.Resource.Bind();
    }

    public void SetVertexShaderConstant(int startRegister, StandardSlotSemantic semantic)
    {
        Matrix4x4 matrix;
        switch (semantic)
        {
            case StandardSlotSemantic.UiProjMatrix:
                matrix = UiProjection;
                SetVertexShaderConstants(startRegister, ref matrix);
                break;
        }
    }

    public void SetPixelShaderConstant(int startRegister, StandardSlotSemantic semantic)
    {
        Span<Matrix4x4> matrix = stackalloc Matrix4x4[1];
        switch (semantic)
        {
            case StandardSlotSemantic.UiProjMatrix:
                matrix[0] = UiProjection;
                SetPixelShaderConstants<Matrix4x4>(startRegister, matrix);
                break;
        }
    }

    public void SetRasterizerState(RasterizerState state)
    {
        if (_currentRasterizerState == state)
        {
            return; // Already set
        }

        _currentRasterizerState = state;
        SetGpuRasterizerState();
    }

    /// <summary>
    /// Sets the correct rasterizer state depending on whether the render target is multi-sampled or not.
    /// </summary>
    private void SetGpuRasterizerState()
    {
        if (_currentRasterizerState != null && _renderTargetStack.TryPeek(out var currentTarget))
        {
            if (currentTarget.IsMultiSampled)
            {
                _context.Rasterizer.State = _currentRasterizerState.MultiSamplingState;
            }
            else
            {
                _context.Rasterizer.State = _currentRasterizerState.State;
            }
        }
    }

    public void SetBlendState(BlendState state)
    {
        if (_currentBlendState == state)
        {
            return; // Already set
        }

        _currentBlendState = state;
        _context.OutputMerger.BlendState = state.GpuState;
    }

    public void SetDepthStencilState(DepthStencilState state)
    {
        if (_currentDepthStencilState == state)
        {
            return; // Already set
        }

        _currentDepthStencilState = state;
        _context.OutputMerger.DepthStencilState = state.GpuState;
    }

    public void SetSamplerState(int samplerIdx, SamplerState state)
    {
        var curSampler = _currentSamplerState[samplerIdx];
        if (curSampler == state)
        {
            return; // Already set
        }

        _currentSamplerState[samplerIdx] = state;

        _context.PixelShader.SetSampler(samplerIdx, state.GpuState);
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

                _context.ResolveSubresource(
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
        _context.PixelShader.SetShaderResource(slot, resourceView);
    }

    public void SetIndexBuffer(IndexBuffer indexBuffer)
    {
        _context.InputAssembler.SetIndexBuffer(indexBuffer.Buffer, indexBuffer.Format, 0);
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

        _context.InputAssembler.PrimitiveTopology = primTopology;
        _context.Draw(vertexCount, startVertex);
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

        _context.InputAssembler.PrimitiveTopology = primTopology;
        _context.DrawIndexed(indexCount, startVertex, vertexBase);
    }

    /*
        Take a screenshot with the given size. The image will be stretched
        to the given size.
    */
    public void TakeScaledScreenshot(string filename, int width = 0, int height = 0, int quality = 90)
    {
        var currentTarget = GetCurrentRenderTargetColorBuffer();
        TakeScaledScreenshot(currentTarget, filename, width, height, quality);
    }

    public void TakeScaledScreenshot(RenderTargetTexture renderTarget,
        string filename, int scaledWidth = 0, int scaledHeight = 0, int quality = 90)
    {
        // Clamp quality to [1, 100]
        quality = Math.Min(100, Math.Max(1, quality));

        Logger.Debug("Creating screenshot with size {0}x{1} in {2}", scaledWidth, scaledHeight,
            filename);

        byte[] jpegData = null;
        ReadRenderTarget(renderTarget, (data, stride, width, height) =>
        {
            jpegData = IO.Images.ImageIO.EncodeJpeg(data,
                JpegPixelFormat.BGRX, width, height,
                quality, stride);
        }, scaledWidth, scaledHeight);

        try
        {
            File.WriteAllBytes(filename, jpegData);
        }
        catch (Exception e)
        {
            Logger.Error("Unable to save screenshot due to an IO error: {0}", e);
        }
    }

    /// <summary>
    /// Callback that is used to handle the data read back to the CPU from a render target.
    /// Pixel data is in BGRA format.
    /// </summary>
    public delegate void RenderTargetReader(ReadOnlySpan<byte> data, int rowStride, int width, int height);

    public void ReadRenderTarget(RenderTargetTexture renderTarget, RenderTargetReader reader,
        int width = 0, int height = 0)
    {
        _annotation?.SetMarker("ReadRenderTarget");

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

        using var stagingTex = new Texture2D(Device, stagingDesc);
        SetDebugName(stagingTex, "ScreenshotStagingTex");

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

            using var tmpTexture = new Texture2D(Device, tmpDesc);
            SetDebugName(tmpTexture, "ScreenshotTmpTexture");

            // Copy/resolve the current RT into the temp texture
            if (currentTargetDesc.SampleDescription.Count > 1)
            {
                _context.ResolveSubresource(renderTarget.Texture, 0, tmpTexture, 0, tmpDesc.Format);
            }
            else
            {
                // SharpDX just reversed these arguments. WTF.
                _context.CopyResource(renderTarget.Texture, tmpTexture);
            }

            // Create the Shader Resource View that we can use to use the tmp texture for sampling in a shader
            var srvDesc = new ShaderResourceViewDescription();
            srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            srvDesc.Texture2D.MipLevels = 1;
            var srv = new ShaderResourceView(Device, tmpTexture, srvDesc);

            // Create our own wrapper so we can use the standard rendering functions
            var tmpSize = new Size(currentTargetDesc.Width, currentTargetDesc.Height);
            var tmpTexWrapper = new DynamicTexture(this,
                tmpTexture,
                srv,
                tmpSize,
                4);

            // Create a texture the size of the target and stretch into it via a blt
            // the target also needs to be a render target for that to work
            using var stretchedRt = CreateRenderTargetTexture(renderTarget.Format, width, height,
                debugName: "StretchedReadBuffer");

            PushRenderTarget(stretchedRt.Resource, null);

            _fullQuadRenderer.Render(tmpTexWrapper);

            PopRenderTarget();

            // Copy our stretched RT to the staging resource
            _context.CopyResource(stretchedRt.Resource.Texture, stagingTex);
        }
        else
        {
            // Resolve multi sampling if necessary
            if (currentTargetDesc.SampleDescription.Count > 1)
            {
                _context.ResolveSubresource(renderTarget.Texture, 0, stagingTex, 0, stagingDesc.Format);
            }
            else
            {
                _context.CopyResource(renderTarget.Texture, stagingTex);
            }
        }

        // Lock the resource and retrieve it
        var mapped = _context.MapSubresource(
            stagingTex,
            0,
            SharpDX.Direct3D11.MapMode.Read,
            0,
            out _
        );

        try
        {
            unsafe
            {
                var mappedData = new Span<byte>((void*) mapped.DataPointer, height * mapped.RowPitch);
                reader(mappedData, mapped.RowPitch, width, height);
            }
        }
        finally
        {
            _context.UnmapSubresource(stagingTex, 0);
        }
    }

    private void SetDebugName(DeviceChild obj, string name)
    {
        if (_debugDevice)
        {
            obj.DebugName = name;
        }
    }

    // Creates a buffer binding for a MDF material that
    // is preinitialized with the correct shader
    public BufferBinding CreateMdfBufferBinding(bool perVertexColor = false)
    {
        var vs = GetShaders().LoadVertexShader("mdf_vs", new Dictionary<string, string>
        {
            {"TEXTURE_STAGES", "1"}, // Necessary so the input struct gets the UVs
            {"PER_VERTEX_COLOR", perVertexColor ? "1" : "0"} // Enable per-vertex color if necessary
        });

        return new BufferBinding(this, vs);
    }

    public Shaders GetShaders()
    {
        return _shaders;
    }

    public Textures GetTextures()
    {
        return _textures;
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
        return new MappedBuffer<TElement>(buffer.Buffer, _context, castData, 0);
    }

    public void Unmap(VertexBuffer buffer)
    {
        _context.UnmapSubresource(buffer.Buffer, 0);
    }

    // Index buffer memory mapping techniques
    public MappedBuffer<ushort> Map(IndexBuffer buffer, MapMode mode = MapMode.Discard)
    {
        var data = MapRaw(buffer.Buffer, buffer.Count * sizeof(ushort), mode);
        var castData = MemoryMarshal.Cast<byte, ushort>(data);
        return new MappedBuffer<ushort>(buffer.Buffer, _context, castData, 0);
    }

    public void Unmap(IndexBuffer buffer)
    {
        _context.UnmapSubresource(buffer.Buffer, 0);
    }

    public unsafe MappedBuffer<byte> Map(DynamicTexture texture, MapMode mode = MapMode.Discard)
    {
        var mapMode = ConvertMapMode(mode);

        var mapped = _context.MapSubresource(texture._texture, 0, 0, mapMode, 0, out _);

        var size = texture.GetSize().Height * mapped.RowPitch;
        var data = new Span<byte>((void*) mapped.DataPointer, size);
        var rowPitch = mapped.RowPitch;

        return new MappedBuffer<byte>(texture._texture, _context, data, rowPitch);
    }

    public void Unmap(DynamicTexture texture)
    {
        _context.UnmapSubresource(texture._texture, 0);
    }

    public const int MaxVsConstantBufferSize = 2048;

    public void SetVertexShaderConstants<T>(int slot, ref T buffer) where T : unmanaged
    {
        ReadOnlySpan<T> span = MemoryMarshal.CreateReadOnlySpan(ref buffer, 1);
        ReadOnlySpan<byte> rawSpan = MemoryMarshal.Cast<T, byte>(span);

        Trace.Assert(rawSpan.Length <= MaxVsConstantBufferSize, "Constant buffer exceeds maximum size");
        UpdateResource(_vsConstantBuffer, rawSpan);
        VSSetConstantBuffer(slot, _vsConstantBuffer);
    }

    public const int MaxPsConstantBufferSize = 512;

    public void SetPixelShaderConstants<T>(int slot, ReadOnlySpan<T> buffer) where T : unmanaged
    {
        var rawSpan = MemoryMarshal.Cast<T, byte>(buffer);
        Trace.Assert(rawSpan.Length <= MaxPsConstantBufferSize, "Constant buffer exceeds maximum size");
        UpdateResource(_psConstantBuffer, rawSpan);
        PSSetConstantBuffer(slot, _psConstantBuffer);
    }

    public Size GetBackBufferSize() => _backBuffer.Resource.GetSize();

    // Pushes the back buffer and it's depth buffer as the current render target
    public void PushBackBufferRenderTarget()
    {
        PushRenderTarget(_backBuffer.Resource, _backBufferDepthStencil.Resource);
    }

    public void PushRenderTarget(
        RenderTargetTexture colorBuffer,
        RenderTargetDepthStencil? depthStencilBuffer
    )
    {
        // If a depth stencil surface is to be used, it HAS to be the same size
        Trace.Assert(depthStencilBuffer == null ||
                     colorBuffer.GetSize() == depthStencilBuffer.Size);

        // Set the camera size to the size of the new render target
        var size = colorBuffer.GetSize();

        // Activate the render target on the device
        var rtv = colorBuffer.RenderTargetView;
        DepthStencilView depthStencilView = null; // Optional!
        if (depthStencilBuffer != null)
        {
            depthStencilView = depthStencilBuffer.DsView;
        }

        _context.OutputMerger.SetRenderTargets(depthStencilView, rtv);
        _textEngine.SetRenderTarget(colorBuffer.Texture.NativePointer);

        // Set the viewport accordingly
        var viewport = new RawViewportF();
        viewport.Width = size.Width;
        viewport.Height = size.Height;
        viewport.MinDepth = 0.0f;
        viewport.MaxDepth = 1.0f;
        _context.Rasterizer.SetViewports(new[] {viewport}, 1);

        _renderTargetStack.Push(new RenderTarget(colorBuffer, depthStencilBuffer));

        ResetScissorRect();
        SetGpuRasterizerState();
    }

    public void PopRenderTarget()
    {
        // The last target should NOT be popped, if the backbuffer was auto-pushed
        if (_backBuffer.IsValid)
        {
            Trace.Assert(_renderTargetStack.Count > 1);
        }

        var poppedTarget = _renderTargetStack.Pop();
        poppedTarget.ColorBuffer.Dispose();
        poppedTarget.DepthStencilBuffer.Dispose();

        if (_renderTargetStack.Count == 0)
        {
            _context.OutputMerger.SetRenderTargets(null, Array.Empty<RenderTargetView>());
            _textEngine.SetRenderTarget(IntPtr.Zero);
            return;
        }

        var newTarget = _renderTargetStack.Peek();

        // Activate the render target on the device
        var rtv = newTarget.ColorBuffer.Resource?.RenderTargetView;
        DepthStencilView depthStencilView = null; // Optional!
        if (newTarget.DepthStencilBuffer.Resource != null)
        {
            depthStencilView = newTarget.DepthStencilBuffer.Resource.DsView;
        }

        _context.OutputMerger.SetRenderTargets(depthStencilView, rtv);
        _textEngine.SetRenderTarget(newTarget.ColorBuffer.Resource?.Texture?.NativePointer ?? IntPtr.Zero);

        // Set the viewport accordingly
        var size = newTarget.Size;
        var viewport = new RawViewportF
        {
            Width = size.Width,
            Height = size.Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f
        };
        _context.Rasterizer.SetViewports(new[] {viewport}, 1);

        ResetScissorRect();
        SetGpuRasterizerState();
    }

    public RenderTargetTexture? GetCurrentRenderTargetColorBuffer()
    {
        return _renderTargetStack.Peek().ColorBuffer.Resource;
    }

    public RenderTargetDepthStencil? GetCurrentRenderTargetDepthStencilBuffer()
    {
        return _renderTargetStack.Peek().DepthStencilBuffer.Resource;
    }

    public int AddResizeListener(ResizeListener listener)
    {
        var newKey = ++_resizeListenersKey;
        _resizeListeners[newKey] = listener;
        return newKey;
    }

    public bool IsDebugDevice() => _debugDevice;

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
        if (_debugDevice)
        {
            _annotation?.EndEvent();
        }
    }

    public TextEngine TextEngine => _textEngine;

    public void Dispose()
    {
        _textEngine.Dispose();

        _d3d11Device1?.Dispose();
        _d3d11Device1 = null;

        Device.Dispose();
        _dxgiFactory.Dispose();
    }

    private void BeginPerfGroupInternal(string message)
    {
        _annotation?.BeginEvent(message);
    }

    internal void RemoveResizeListener(int key)
    {
        _resizeListeners.Remove(key);
    }

    internal void AddResourceListener(IResourceLifecycleListener listener)
    {
        _resourcesListeners.Add(listener);
        if (_resourcesCreated)
        {
            listener.CreateResources(this);
        }
    }

    internal void RemoveResourceListener(IResourceLifecycleListener listener)
    {
        _resourcesListeners.Remove(listener);
        if (_resourcesCreated)
        {
            listener.FreeResources(this);
        }
    }

    private unsafe void UpdateResource(Resource resource, ReadOnlySpan<byte> data)
    {
        var mapped = _context.MapSubresource(resource, 0, SharpDX.Direct3D11.MapMode.WriteDiscard,
            0, out var stream);

        try
        {
            var dest = new Span<byte>((void*) mapped.DataPointer, (int) stream.Length);
            data.CopyTo(dest);
        }
        finally
        {
            _context.UnmapSubresource(resource, 0);
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
                return new Buffer(Device, (IntPtr) rawDataPtr, bufferDesc);
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

        return new Buffer(Device, bufferDesc);
    }

    private void VSSetConstantBuffer(int slot, Buffer buffer)
    {
        _context.VertexShader.SetConstantBuffer(slot, buffer);
    }

    private void PSSetConstantBuffer(int slot, Buffer buffer)
    {
        _context.PixelShader.SetConstantBuffer(slot, buffer);
    }

    private unsafe Span<byte> MapRaw(Resource buffer, int bufferSize, MapMode mode)
    {
        var mapMode = ConvertMapMode(mode);

        var mapped = _context.MapSubresource(buffer, 0, mapMode, 0);

        return new Span<byte>((void*) mapped.DataPointer, bufferSize);
    }

    // Ported from XMMatrixOrthographicOffCenterLH
    private static Matrix4x4 CreateUIProjection(float width, float height)
    {
        var m = new Matrix4x4();
        m.M11 = 2 / width;
        m.M22 = -2 / height;
        m.M33 = 1;

        m.M41 = -1f;
        m.M42 = 1f;
        m.M44 = 1.0f;
        return m;
    }

    private Adapter1? GetAdapter(int index)
    {
        return _dxgiFactory.GetAdapter1(index);
    }

    private interface IOutputSurface : IDisposable
    {
        ResourceRef<RenderTargetTexture> CreateBackBuffer(RenderingDevice device);

        void Present(RenderingDevice device);
    }

    private class WindowOutputSurface : IOutputSurface
    {
        /// <summary>
        /// The HWND (on Window) of the window output surface.
        /// </summary>
        public IntPtr WindowHandle { get; }

        private SwapChain? _swapChain;

        public WindowOutputSurface(D3D11Device device, IntPtr windowHandle)
        {
            WindowHandle = windowHandle;
        }

        public ResourceRef<RenderTargetTexture> CreateBackBuffer(RenderingDevice device)
        {
            if (_swapChain == null)
            {
                var swapChainDesc = new SwapChainDescription();
                swapChainDesc.BufferCount = 2;
                swapChainDesc.ModeDescription.Format = Format.B8G8R8A8_UNorm;
                swapChainDesc.Usage = Usage.RenderTargetOutput;
                swapChainDesc.OutputHandle = WindowHandle;
                swapChainDesc.SampleDescription.Count = 1;
                swapChainDesc.IsWindowed = true; // As per the recommendation, we always create windowed

                _swapChain = new SwapChain(device._dxgiFactory, device.Device, swapChainDesc);

                // Disable Alt+Enter handling in DXGI itself. We need to handle this in our Main Window
                // to properly save the associated settings change.
                device._dxgiFactory.MakeWindowAssociation(WindowHandle, WindowAssociationFlags.IgnoreAltEnter);
            }
            else
            {
                _swapChain.ResizeBuffers(0, 0, 0, Format.Unknown, 0);
            }

            using var surface = _swapChain.GetBackBuffer<Texture2D>(0);

            var surfaceDesc = surface.Description;
            Logger.Info("Created Swap Chain: {0}x{1} @ {2}", surfaceDesc.Width, surfaceDesc.Height, surfaceDesc.Format);

            var rtvDesc = new RenderTargetViewDescription();
            rtvDesc.Format = surfaceDesc.Format;
            rtvDesc.Dimension = RenderTargetViewDimension.Texture2D;
            rtvDesc.Texture2D.MipSlice = 0;

            // Create a render target view for rendering to the real backbuffer
            var backBufferView = new RenderTargetView(
                device.Device, surface, rtvDesc
            );

            var size = new Size(surfaceDesc.Width, surfaceDesc.Height);
            return new ResourceRef<RenderTargetTexture>(new RenderTargetTexture(
                device,
                surface,
                backBufferView,
                null,
                null,
                size,
                false));
        }

        public void Present(RenderingDevice device)
        {
            _swapChain?.Present(0, 0);
        }

        public void Dispose()
        {
            _swapChain?.Dispose();
        }
    }

    private class OffScreenOutputSurface : IOutputSurface
    {
        public void Dispose()
        {
        }

        public ResourceRef<RenderTargetTexture> CreateBackBuffer(RenderingDevice device)
        {
            // For off-screen rendering, we just create normal backbuffer/depthstencil without an associated swapchain
            var offscreenSize = new Size(1024, 768);
            return device.CreateRenderTargetTexture(BufferFormat.A8R8G8B8, offscreenSize.Width,
                offscreenSize.Height, debugName: "BackBuffer");
        }

        public void Present(RenderingDevice device)
        {
            device.Context.Flush();
        }
    }

    struct RenderTarget
    {
        public OptionalResourceRef<RenderTargetTexture> ColorBuffer;
        public OptionalResourceRef<RenderTargetDepthStencil> DepthStencilBuffer;
        public bool IsMultiSampled { get; }

        public Size Size
        {
            get
            {
                if (ColorBuffer.Resource != null)
                {
                    return ColorBuffer.Resource.GetSize();
                }

                if (DepthStencilBuffer.Resource != null)
                {
                    return DepthStencilBuffer.Resource.Size;
                }

                return Size.Empty;
            }
        }

        public RenderTarget(RenderTargetTexture? colorBuffer, RenderTargetDepthStencil? depthStencilBuffer)
        {
            ColorBuffer = default;
            DepthStencilBuffer = default;
            IsMultiSampled = false;

            if (colorBuffer != null)
            {
                ColorBuffer = new OptionalResourceRef<RenderTargetTexture>(colorBuffer);
                IsMultiSampled = colorBuffer.IsMultiSampled;
            }

            DepthStencilBuffer = new OptionalResourceRef<RenderTargetDepthStencil>(depthStencilBuffer);
        }
    }
}

public delegate void ResizeListener(int w, int h);

public class MaterialSamplerSpec
{
    public OptionalResourceRef<ITexture> texture;
    public SamplerSpec samplerSpec;

    public MaterialSamplerSpec(OptionalResourceRef<ITexture> texture, SamplerSpec samplerSpec)
    {
        this.texture = texture;
        this.samplerSpec = samplerSpec;
    }
}