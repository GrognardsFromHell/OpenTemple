using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using OpenTemple.Core.Utils;
using BlendState = SharpDX.Direct3D11.BlendState;
using Buffer = SharpDX.Direct3D11.Buffer;
using DepthStencilState = SharpDX.Direct3D11.DepthStencilState;
using Device = SharpDX.Direct3D11.Device;
using MapMode = SharpDX.Direct3D11.MapMode;
using PixelShader = SharpDX.Direct3D11.PixelShader;
using RasterizerState = SharpDX.Direct3D11.RasterizerState;
using SamplerState = SharpDX.Direct3D11.SamplerState;
using VertexShader = SharpDX.Direct3D11.VertexShader;
using DataBox = SharpDX.DataBox;
using static SDL2.SDL;

namespace OpenTemple.Core.DebugUI;

internal class ImGuiRenderer : IDisposable
{
    // ImGui Win32 + DirectX11 binding
    // In this binding, ImTextureID is used to store a 'ID3D11ShaderResourceView*' texture identifier. Read the FAQ about ImTextureID in imgui.cpp.

    // You can copy and use unmodified imgui_impl_* files in your project. See main.cpp for an example of using this.
    // If you use this binding you'll need to call 4 functions: ImGui_ImplXXXX_Init(), ImGui_ImplXXXX_NewFrame(), ImGui.Render() and ImGui_ImplXXXX_Shutdown().
    // If you are new to ImGui, see examples/README.txt and documentation at the top of imgui.cpp.
    // https://github.com/ocornut/imgui

    // Data
    private long _time = 0;
    private long _ticksPerSecond = 0;

    private Device? _device;
    private DeviceContext? _deviceContext;
    private Buffer? _vertexBuffer;
    private Buffer? _indexBuffer;
    private byte[]? _vertexShaderBlob;
    private VertexShader? _vertexShader;
    private InputLayout? _inputLayout;
    private Buffer? _vertexConstantBuffer;
    private byte[]? _pixelShaderBlob;
    private PixelShader? _pixelShader;
    private SamplerState? _fontSampler;
    private ShaderResourceView? _fontTextureView;
    private GCHandle _fontTextureViewHandle;
    private RasterizerState? _rasterizerState;
    private BlendState? _blendState;
    private DepthStencilState? _depthStencilState;
    private int _vertexBufferSize = 5000, _indexBufferSize = 10000;

    class StateBackup
    {
        public RawRectangle[] ScissorRects;
        public RawViewportF[] Viewports;
        public RasterizerState RS;
        public BlendState BlendState;
        public RawColor4 BlendFactor = new();
        public int SampleMask;
        public int StencilRef;
        public DepthStencilState DepthStencilState;
        public ShaderResourceView[] PSShaderResource;
        public SamplerState[] PSSampler;
        public PixelShader PS;
        public VertexShader VS;

        public PrimitiveTopology PrimitiveTopology;
        public Buffer[] VSConstantBuffer;
        public Buffer IndexBuffer;
        public Buffer[] VertexBuffer = new Buffer[1];
        public int IndexBufferOffset;
        public int[] VertexBufferStride = new int[1];
        public int[] VertexBufferOffset = new int[1];
        public Format IndexBufferFormat;
        public InputLayout InputLayout;
    };

    // This is the main rendering function that you have to implement and provide to ImGui (via setting up 'RenderDrawListsFn' in the ImGuiIO structure)
    // If text or lines are blurry when integrating ImGui in your engine:
    // - in your Render function, try translating your projection matrix by (0.5f,0.5f) or (0.375f,0.375f)
    public void ImGui_ImplDX11_RenderDrawLists(ImDrawDataPtr drawData)
    {
        var ctx = _deviceContext;
        if (ctx == null)
        {
            return;
        }

        // Create and grow vertex/index buffers if needed
        if (_vertexBuffer == null || _vertexBufferSize < drawData.TotalVtxCount)
        {
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = null;
            }

            _vertexBufferSize = drawData.TotalVtxCount + 5000;
            var desc = new BufferDescription();
            desc.Usage = ResourceUsage.Dynamic;
            desc.SizeInBytes = _vertexBufferSize * Marshal.SizeOf<ImDrawVert>();
            desc.BindFlags = BindFlags.VertexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Write;
            _vertexBuffer = new Buffer(_device, desc);
            _vertexBuffer.DebugName = "ImGui_VB";
        }

        if (_indexBuffer == null || _indexBufferSize < drawData.TotalIdxCount)
        {
            if (_indexBuffer != null)
            {
                _indexBuffer.Dispose();
                _indexBuffer = null;
            }

            _indexBufferSize = drawData.TotalIdxCount + 10000;
            var desc = new BufferDescription();
            desc.Usage = ResourceUsage.Dynamic;
            desc.SizeInBytes = _indexBufferSize * sizeof(ushort);
            desc.BindFlags = BindFlags.IndexBuffer;
            desc.CpuAccessFlags = CpuAccessFlags.Write;
            _indexBuffer = new Buffer(_device, desc);
            _indexBuffer.DebugName = "ImGui_IB";
        }

        // Copy and convert all vertices into a single contiguous buffer
        var vertexResource = ctx.MapSubresource(_vertexBuffer, 0, MapMode.WriteDiscard, 0);
        var indexResource = ctx.MapSubresource(_indexBuffer, 0, MapMode.WriteDiscard, 0);
        Span<ImDrawVert> vertexDest;
        Span<ushort> indexDest;

        unsafe
        {
            vertexDest = new Span<ImDrawVert>((void*) vertexResource.DataPointer, _vertexBufferSize);
            indexDest = new Span<ushort>((void*) indexResource.DataPointer, _indexBufferSize);
        }

        for (int n = 0; n < drawData.CmdListsCount; n++)
        {
            ImDrawListPtr cmd_list = drawData.CmdListsRange[n];

            Span<ImDrawVert> vtx_src;
            Span<ushort> idx_src;

            unsafe
            {
                vtx_src = new Span<ImDrawVert>((void*) cmd_list.VtxBuffer.Data, cmd_list.VtxBuffer.Size);
                idx_src = new Span<ushort>((void*) cmd_list.IdxBuffer.Data, cmd_list.IdxBuffer.Size);
            }

            vtx_src.CopyTo(vertexDest);
            vertexDest = vertexDest.Slice(vtx_src.Length);

            idx_src.CopyTo(indexDest);
            indexDest = indexDest.Slice(idx_src.Length);
        }

        ctx.UnmapSubresource(_vertexBuffer, 0);
        ctx.UnmapSubresource(_indexBuffer, 0);

        // Setup orthographic projection matrix into our constant buffer
        {
            var mapped_resource = ctx.MapSubresource(_vertexConstantBuffer, 0, MapMode.WriteDiscard, 0);
            float L = 0.0f;
            float R = ImGui.GetIO().DisplaySize.X;
            float B = ImGui.GetIO().DisplaySize.Y;
            float T = 0.0f;

            Span<float> constantBuffer;
            unsafe
            {
                constantBuffer = new Span<float>((void*) mapped_resource.DataPointer, 16);
            }

            Span<float> mvp = stackalloc float[16]
            {
                2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                0.0f, 0.0f, 0.5f, 0.0f,
                (R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f,
            };
            mvp.CopyTo(constantBuffer);
            ctx.UnmapSubresource(_vertexConstantBuffer, 0);
        }

        // Backup DX state that will be modified to restore it afterwards (unfortunately this is very ugly looking and verbose. Close your eyes!)

        StateBackup old = new StateBackup();
        old.ScissorRects = ctx.Rasterizer.GetScissorRectangles<RawRectangle>();
        old.Viewports = ctx.Rasterizer.GetViewports<RawViewportF>();
        old.RS = ctx.Rasterizer.State;
        old.BlendState = ctx.OutputMerger.GetBlendState(out old.BlendFactor, out old.SampleMask);
        old.DepthStencilState = ctx.OutputMerger.GetDepthStencilState(out old.StencilRef);
        old.PSShaderResource = ctx.PixelShader.GetShaderResources(0, 1);
        old.PSSampler = ctx.PixelShader.GetSamplers(0, 1);
        old.PS = ctx.PixelShader.Get();
        old.VS = ctx.VertexShader.Get();
        old.VSConstantBuffer = ctx.VertexShader.GetConstantBuffers(0, 1);
        old.PrimitiveTopology = ctx.InputAssembler.PrimitiveTopology;
        ctx.InputAssembler.GetIndexBuffer(out old.IndexBuffer, out old.IndexBufferFormat,
            out old.IndexBufferOffset);
        ctx.InputAssembler.GetVertexBuffers(0, 1, old.VertexBuffer, old.VertexBufferStride, old.VertexBufferOffset);
        old.InputLayout = ctx.InputAssembler.InputLayout;

        // Setup viewport
        RawViewportF vp = new RawViewportF();
        vp.Width = ImGui.GetIO().DisplaySize.X;
        vp.Height = ImGui.GetIO().DisplaySize.Y;
        vp.MinDepth = 0.0f;
        vp.MaxDepth = 1.0f;
        ctx.Rasterizer.SetViewports(new RawViewportF[] {vp}, 1);

        // Bind shader and vertex buffers
        int stride = Marshal.SizeOf<ImDrawVert>();
        int offset = 0;
        ctx.InputAssembler.InputLayout = _inputLayout;
        ctx.InputAssembler.SetVertexBuffers(0, new Buffer[] {_vertexBuffer}, new int[] {stride}, new int[] {offset});
        ctx.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
        ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        ctx.VertexShader.Set(_vertexShader);
        ctx.VertexShader.SetConstantBuffer(0, _vertexConstantBuffer);
        ctx.PixelShader.Set(_pixelShader);
        ctx.PixelShader.SetSampler(0, _fontSampler);

        // Setup render state
        RawColor4 blendFactor = new RawColor4(0, 0, 0, 0);
        ctx.OutputMerger.SetBlendState(_blendState, blendFactor, 0xffffffff);
        ctx.OutputMerger.SetDepthStencilState(_depthStencilState, 0);
        ctx.Rasterizer.State = _rasterizerState;

        // Render command lists
        var vertexOffset = 0;
        var indexOffset = 0;
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var cmdList = drawData.CmdListsRange[n];
            for (var cmdIdx = 0; cmdIdx < cmdList.CmdBuffer.Size; cmdIdx++)
            {
                var cmd = cmdList.CmdBuffer[cmdIdx];
                if (cmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotSupportedException();
                    /*Marshal.GetDelegateForFunctionPointer()
                    pcmd.UserCallback(cmd_list, pcmd);*/
                }
                else
                {
                    var handle = GCHandle.FromIntPtr(cmd.TextureId);

                    ctx.PixelShader.SetShaderResource(0, (ShaderResourceView) handle.Target);
                    ctx.Rasterizer.SetScissorRectangle(
                        (int) cmd.ClipRect.X,
                        (int) cmd.ClipRect.Y,
                        (int) cmd.ClipRect.Z,
                        (int) cmd.ClipRect.W
                    );
                    ctx.DrawIndexed((int) cmd.ElemCount, indexOffset, vertexOffset);
                }

                indexOffset += (int) cmd.ElemCount;
            }

            vertexOffset += cmdList.VtxBuffer.Size;
        }

        // Restore modified DX state
        ctx.Rasterizer.SetScissorRectangles(old.ScissorRects);
        ctx.Rasterizer.SetViewports(old.Viewports);
        ctx.Rasterizer.State = old.RS;
        old.RS?.Dispose();
        ctx.OutputMerger.SetBlendState(old.BlendState, old.BlendFactor, old.SampleMask);
        old.BlendState?.Dispose();
        ctx.OutputMerger.SetDepthStencilState(old.DepthStencilState, old.StencilRef);
        old.DepthStencilState?.Dispose();
        ctx.PixelShader.SetShaderResources(0, old.PSShaderResource);
        old.PSShaderResource.DisposeAndNull();
        ctx.PixelShader.SetSamplers(0, old.PSSampler);
        old.PSSampler.DisposeAndNull();
        ctx.PixelShader.Set(old.PS);
        old.PS?.Dispose();
        ctx.VertexShader.Set(old.VS);
        old.VS?.Dispose();
        ctx.VertexShader.SetConstantBuffers(0, old.VSConstantBuffer);
        old.VSConstantBuffer?.DisposeAndNull();
        ctx.InputAssembler.PrimitiveTopology = old.PrimitiveTopology;
        ctx.InputAssembler.SetIndexBuffer(old.IndexBuffer, old.IndexBufferFormat, old.IndexBufferOffset);
        old.IndexBuffer?.Dispose();
        ctx.InputAssembler.SetVertexBuffers(0, old.VertexBuffer, old.VertexBufferStride, old.VertexBufferOffset);
        old.VertexBuffer.DisposeAndNull();
        ctx.InputAssembler.InputLayout = old.InputLayout;
        old.InputLayout?.Dispose();
    }

    private void CreateFontsTexture()
    {
        // Build texture atlas
        var io = ImGui.GetIO();
        unsafe
        {
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height);

            // Upload texture to graphics system
            var desc = new Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = Format.R8G8B8A8_UNorm;
            desc.SampleDescription.Count = 1;
            desc.Usage = ResourceUsage.Default;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.CpuAccessFlags = 0;

            var subResource = new DataBox();
            subResource.DataPointer = pixels;
            subResource.RowPitch = desc.Width * 4;
            subResource.SlicePitch = 0;
            using var pTexture = new Texture2D(_device, desc, new[] {subResource});

            // Create texture view
            var srvDesc = new ShaderResourceViewDescription();
            srvDesc.Format = Format.R8G8B8A8_UNorm;
            srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            srvDesc.Texture2D.MipLevels = desc.MipLevels;
            srvDesc.Texture2D.MostDetailedMip = 0;

            _fontTextureView = new ShaderResourceView(_device, pTexture, srvDesc);
            _fontTextureViewHandle = GCHandle.Alloc(_fontTextureView);
        }

        // Store our identifier
        io.Fonts.TexID = GCHandle.ToIntPtr(_fontTextureViewHandle);

        // Create texture sampler
        {
            SamplerStateDescription desc = new SamplerStateDescription();
            desc.Filter = Filter.MinMagMipLinear;
            desc.AddressU = TextureAddressMode.Wrap;
            desc.AddressV = TextureAddressMode.Wrap;
            desc.AddressW = TextureAddressMode.Wrap;
            desc.ComparisonFunction = Comparison.Always;
            _fontSampler = new SamplerState(_device, desc);
        }
    }

    private bool CreateDeviceObjects()
    {
        if (_device == null)
            return false;
        if (_fontSampler != null)
            InvalidateDeviceObjects();

        // By using D3DCompile() from <d3dcompiler.h> / d3dcompiler.lib, we introduce a dependency to a given version of d3dcompiler_XX.dll (see D3DCOMPILER_DLL_A)
        // If you would like to use this DX11 sample code but remove this dependency you can:
        //  1) compile once, save the compiled shader blobs into a file or source code and pass them to CreateVertexShader()/CreatePixelShader() [prefered solution]
        //  2) use code to detect any version of the DLL and grab a pointer to D3DCompile from the DLL.
        // See https://github.com/ocornut/imgui/pull/638 for sources and details.

        // Create the vertex shader
        {
            const string vertexShader = @"
                cbuffer vertexBuffer : register(b0)
                {
                float4x4 ProjectionMatrix;
                };
                struct VS_INPUT
                {
                float2 pos : POSITION;
                float4 col : COLOR0;
                float2 uv  : TEXCOORD0;
                };

                struct PS_INPUT
                {
                float4 pos : SV_POSITION;
                float4 col : COLOR0;
                float2 uv  : TEXCOORD0;
                };

                PS_INPUT main(VS_INPUT input)
                {
                PS_INPUT output;
                output.pos = mul( ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
                output.col = input.col;
                output.uv  = input.uv;
                return output;
                }";

            _vertexShaderBlob = ShaderBytecode.Compile(vertexShader, "main", "vs_4_0");

            // NB: Pass ID3D10Blob* pErrorBlob to D3DCompile() to get error showing in (const char*)pErrorBlob.GetBufferPointer(). Make sure to Release() the blob!
            if (_vertexShaderBlob == null)
                return false;

            _vertexShader = new VertexShader(_device, _vertexShaderBlob);

            // Create the input layout
            var layout = new InputElement[]
            {
                new("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0),
            };
            _inputLayout = new InputLayout(_device, _vertexShaderBlob, layout);

            // Create the constant buffer
            {
                BufferDescription desc = new BufferDescription();
                desc.SizeInBytes = 16 * sizeof(float);
                desc.Usage = ResourceUsage.Dynamic;
                desc.BindFlags = BindFlags.ConstantBuffer;
                desc.CpuAccessFlags = CpuAccessFlags.Write;
                _vertexConstantBuffer = new Buffer(_device, desc);
                _vertexConstantBuffer.DebugName = "ImGui_VertexConstantBuffer";
            }
        }

        // Create the pixel shader
        {
            const string pixelShader =
                @"struct PS_INPUT
                {
                float4 pos : SV_POSITION;
                float4 col : COLOR0;
                float2 uv  : TEXCOORD0;
                };
                sampler sampler0;
                Texture2D texture0;

                float4 main(PS_INPUT input) : SV_Target
                {
                float4 out_col = input.col * texture0.Sample(sampler0, input.uv);
                return out_col;
                }";

            _pixelShaderBlob = ShaderBytecode.Compile(pixelShader, "main", "ps_4_0");

            if (_pixelShaderBlob == null
               ) // NB: Pass ID3D10Blob* pErrorBlob to D3DCompile() to get error showing in (const char*)pErrorBlob.GetBufferPointer(). Make sure to Release() the blob!
                return false;

            _pixelShader = new PixelShader(_device, _pixelShaderBlob);
        }

        // Create the blending setup
        {
            BlendStateDescription desc = new BlendStateDescription();
            desc.AlphaToCoverageEnable = false;
            desc.RenderTarget[0].IsBlendEnabled = true;
            desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseSourceAlpha;
            desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            _blendState = new BlendState(_device, desc);
        }

        // Create the rasterizer state
        {
            RasterizerStateDescription desc = new RasterizerStateDescription();
            desc.FillMode = FillMode.Solid;
            desc.CullMode = CullMode.None;
            desc.IsScissorEnabled = true;
            desc.IsDepthClipEnabled = true;
            _rasterizerState = new RasterizerState(_device, desc);
        }

        // Create depth-stencil State
        {
            DepthStencilStateDescription desc = new DepthStencilStateDescription();
            desc.IsDepthEnabled = false;
            desc.DepthWriteMask = DepthWriteMask.All;
            desc.DepthComparison = Comparison.Always;
            desc.IsStencilEnabled = false;
            desc.FrontFace.FailOperation = desc.FrontFace.DepthFailOperation =
                desc.FrontFace.PassOperation = StencilOperation.Keep;
            desc.FrontFace.Comparison = Comparison.Always;
            desc.BackFace = desc.FrontFace;
            _depthStencilState = new DepthStencilState(_device, desc);
        }

        CreateFontsTexture();

        return true;
    }

    private void InvalidateDeviceObjects()
    {
        if (_device == null)
            return;

        if (_fontSampler != null)
        {
            _fontSampler.Dispose();
            _fontSampler = null;
        }

        if (_fontTextureView != null)
        {
            _fontTextureViewHandle.Free();
            _fontTextureView.Dispose();
            _fontTextureView = null;
            ImGui.GetIO().Fonts.TexID = IntPtr.Zero;
        }

        if (_indexBuffer != null)
        {
            _indexBuffer.Dispose();
            _indexBuffer = null;
        }

        if (_vertexBuffer != null)
        {
            _vertexBuffer.Dispose();
            _vertexBuffer = null;
        }

        if (_blendState != null)
        {
            _blendState.Dispose();
            _blendState = null;
        }

        if (_depthStencilState != null)
        {
            _depthStencilState.Dispose();
            _depthStencilState = null;
        }

        if (_rasterizerState != null)
        {
            _rasterizerState.Dispose();
            _rasterizerState = null;
        }

        if (_pixelShader != null)
        {
            _pixelShader.Dispose();
            _pixelShader = null;
        }

        if (_vertexConstantBuffer != null)
        {
            _vertexConstantBuffer.Dispose();
            _vertexConstantBuffer = null;
        }

        if (_inputLayout != null)
        {
            _inputLayout.Dispose();
            _inputLayout = null;
        }

        if (_vertexShader != null)
        {
            _vertexShader.Dispose();
            _vertexShader = null;
        }
    }

    public bool Init(Device device, DeviceContext deviceContext)
    {
        _device = device;
        _deviceContext = deviceContext;

        _ticksPerSecond = Stopwatch.Frequency;
        _time = Stopwatch.GetTimestamp();

        return true;
    }

    public void Dispose()
    {
        InvalidateDeviceObjects();
        _device = null;
        _deviceContext = null;
    }

    public void NewFrame(int width, int height)
    {
        if (_fontSampler == null)
            CreateDeviceObjects();

        var io = ImGui.GetIO();

        // Setup display size (every frame to accommodate for window resizing)
        io.DisplaySize = new Vector2(width, height);

        // Setup time step
        var currentTime = Stopwatch.GetTimestamp();
        io.DeltaTime = (float) (currentTime - _time) / _ticksPerSecond;
        _time = currentTime;

        // Start the frame
        ImGui.NewFrame();
    }
}