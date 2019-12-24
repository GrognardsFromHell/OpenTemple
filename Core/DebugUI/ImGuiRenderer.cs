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
using OpenTemple.Core.Platform;
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

namespace OpenTemple.Core.DebugUI
{
    internal class ImGuiRenderer
    {
        // ImGui Win32 + DirectX11 binding
        // In this binding, ImTextureID is used to store a 'ID3D11ShaderResourceView*' texture identifier. Read the FAQ about ImTextureID in imgui.cpp.

        // You can copy and use unmodified imgui_impl_* files in your project. See main.cpp for an example of using this.
        // If you use this binding you'll need to call 4 functions: ImGui_ImplXXXX_Init(), ImGui_ImplXXXX_NewFrame(), ImGui.Render() and ImGui_ImplXXXX_Shutdown().
        // If you are new to ImGui, see examples/README.txt and documentation at the top of imgui.cpp.
        // https://github.com/ocornut/imgui

        // Data
        private long g_Time = 0;
        private long g_TicksPerSecond = 0;

        private IntPtr g_hWnd;
        private Device g_pd3dDevice;
        private DeviceContext g_pd3dDeviceContext;
        private Buffer g_pVB;
        private Buffer g_pIB;
        private byte[] g_pVertexShaderBlob;
        private VertexShader g_pVertexShader;
        private InputLayout g_pInputLayout;
        private Buffer g_pVertexConstantBuffer;
        private byte[] g_pPixelShaderBlob;
        private PixelShader g_pPixelShader;
        private SamplerState g_pFontSampler;
        private ShaderResourceView g_pFontTextureView;
        private GCHandle g_pFontTextureViewHandle;
        private RasterizerState g_pRasterizerState;
        private BlendState g_pBlendState;
        private DepthStencilState g_pDepthStencilState;
        private int g_VertexBufferSize = 5000, g_IndexBufferSize = 10000;

        struct VERTEX_CONSTANT_BUFFER
        {
            public Matrix4x4 mvp;
        };

        private const int D3D11_VIEWPORT_AND_SCISSORRECT_OBJECT_COUNT_PER_PIPELINE = 16;

        class BACKUP_DX11_STATE
        {
            public RawRectangle[] ScissorRects;
            public RawViewportF[] Viewports;
            public RasterizerState RS;
            public BlendState BlendState;
            public RawColor4 BlendFactor = new RawColor4();
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
        public void ImGui_ImplDX11_RenderDrawLists(ImDrawDataPtr draw_data)
        {
            DeviceContext ctx = g_pd3dDeviceContext;

            // Create and grow vertex/index buffers if needed
            if (g_pVB == null || g_VertexBufferSize < draw_data.TotalVtxCount)
            {
                if (g_pVB != null)
                {
                    g_pVB.Dispose();
                    g_pVB = null;
                }

                g_VertexBufferSize = draw_data.TotalVtxCount + 5000;
                var desc = new BufferDescription();
                desc.Usage = ResourceUsage.Dynamic;
                desc.SizeInBytes = g_VertexBufferSize * Marshal.SizeOf<ImDrawVert>();
                desc.BindFlags = BindFlags.VertexBuffer;
                desc.CpuAccessFlags = CpuAccessFlags.Write;
                g_pVB = new Buffer(g_pd3dDevice, desc);
            }

            if (g_pIB == null || g_IndexBufferSize < draw_data.TotalIdxCount)
            {
                if (g_pIB != null)
                {
                    g_pIB.Dispose();
                    g_pIB = null;
                }

                g_IndexBufferSize = draw_data.TotalIdxCount + 10000;
                var desc = new BufferDescription();
                desc.Usage = ResourceUsage.Dynamic;
                desc.SizeInBytes = g_IndexBufferSize * sizeof(ushort);
                desc.BindFlags = BindFlags.IndexBuffer;
                desc.CpuAccessFlags = CpuAccessFlags.Write;
                g_pIB = new Buffer(g_pd3dDevice, desc);
            }

            // Copy and convert all vertices into a single contiguous buffer
            var vtx_resource = ctx.MapSubresource(g_pVB, 0, MapMode.WriteDiscard, 0);
            var idx_resource = ctx.MapSubresource(g_pIB, 0, MapMode.WriteDiscard, 0);
            Span<ImDrawVert> vtx_dst;
            Span<ushort> idx_dst;

            unsafe
            {
                vtx_dst = new Span<ImDrawVert>((void*) vtx_resource.DataPointer, g_VertexBufferSize);
                idx_dst = new Span<ushort>((void*) idx_resource.DataPointer, g_IndexBufferSize);
            }

            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                Span<ImDrawVert> vtx_src;
                Span<ushort> idx_src;

                unsafe
                {
                    vtx_src = new Span<ImDrawVert>((void*) cmd_list.VtxBuffer.Data, cmd_list.VtxBuffer.Size);
                    idx_src = new Span<ushort>((void*) cmd_list.IdxBuffer.Data, cmd_list.IdxBuffer.Size);
                }

                vtx_src.CopyTo(vtx_dst);
                vtx_dst = vtx_dst.Slice(vtx_src.Length);

                idx_src.CopyTo(idx_dst);
                idx_dst = idx_dst.Slice(idx_src.Length);
            }

            ctx.UnmapSubresource(g_pVB, 0);
            ctx.UnmapSubresource(g_pIB, 0);

            // Setup orthographic projection matrix into our constant buffer
            {
                var mapped_resource = ctx.MapSubresource(g_pVertexConstantBuffer, 0, MapMode.WriteDiscard, 0);
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
                ctx.UnmapSubresource(g_pVertexConstantBuffer, 0);
            }

            // Backup DX state that will be modified to restore it afterwards (unfortunately this is very ugly looking and verbose. Close your eyes!)

            BACKUP_DX11_STATE old = new BACKUP_DX11_STATE();
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
            ctx.InputAssembler.InputLayout = g_pInputLayout;
            ctx.InputAssembler.SetVertexBuffers(0, new Buffer[] {g_pVB}, new int[] {stride}, new int[] {offset});
            ctx.InputAssembler.SetIndexBuffer(g_pIB, Format.R16_UInt, 0);
            ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            ctx.VertexShader.Set(g_pVertexShader);
            ctx.VertexShader.SetConstantBuffer(0, g_pVertexConstantBuffer);
            ctx.PixelShader.Set(g_pPixelShader);
            ctx.PixelShader.SetSampler(0, g_pFontSampler);

            // Setup render state
            RawColor4 blend_factor = new RawColor4(0, 0, 0, 0);
            ctx.OutputMerger.SetBlendState(g_pBlendState, blend_factor, 0xffffffff);
            ctx.OutputMerger.SetDepthStencilState(g_pDepthStencilState, 0);
            ctx.Rasterizer.State = g_pRasterizerState;

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotSupportedException();
                        /*Marshal.GetDelegateForFunctionPointer()
                        pcmd.UserCallback(cmd_list, pcmd);*/
                    }
                    else
                    {
                        var handle = GCHandle.FromIntPtr(pcmd.TextureId);

                        ctx.PixelShader.SetShaderResource(0, (ShaderResourceView) handle.Target);
                        ctx.Rasterizer.SetScissorRectangle(
                            (int) pcmd.ClipRect.X,
                            (int) pcmd.ClipRect.Y,
                            (int) pcmd.ClipRect.Z,
                            (int) pcmd.ClipRect.W
                        );
                        ctx.DrawIndexed((int) pcmd.ElemCount, idx_offset, vtx_offset);
                    }

                    idx_offset += (int) pcmd.ElemCount;
                }

                vtx_offset += cmd_list.VtxBuffer.Size;
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

        public bool ImGui_ImplDX11_WndProcHandler(uint msg, ulong wParam, ulong lParam)
        {
            var io = ImGui.GetIO();
            switch (msg)
            {
                case WM_LBUTTONDOWN:
                    io.MouseDown[0] = true;
                    return true;
                case WM_LBUTTONUP:
                    io.MouseDown[0] = false;
                    return true;
                case WM_RBUTTONDOWN:
                    io.MouseDown[1] = true;
                    return true;
                case WM_RBUTTONUP:
                    io.MouseDown[1] = false;
                    return true;
                case WM_MBUTTONDOWN:
                    io.MouseDown[2] = true;
                    return true;
                case WM_MBUTTONUP:
                    io.MouseDown[2] = false;
                    return true;
                case WM_MOUSEWHEEL:
                    io.MouseWheel += ((short) (wParam >> 16)) > 0 ? +1.0f : -1.0f;
                    return true;
                case WM_MOUSEMOVE:
                    io.MousePos.X = (short) (lParam);
                    io.MousePos.Y = (short) (lParam >> 16);
                    return true;
                case WM_KEYDOWN:
                    if (wParam < 256)
                        io.KeysDown[(int) wParam] = true;
                    return true;
                case WM_KEYUP:
                    if (wParam < 256)
                        io.KeysDown[(int) wParam] = false;
                    return true;
                case WM_CHAR:
                    // You can also use ToAscii()+GetKeyboardState() to retrieve characters.
                    if (wParam > 0 && wParam < 0x10000)
                        io.AddInputCharacter((ushort) wParam);
                    return true;
            }

            return false;
        }

        private void ImGui_ImplDX11_CreateFontsTexture()
        {
            // Build texture atlas
            var io = ImGui.GetIO();
            unsafe
            {
                io.Fonts.GetTexDataAsRGBA32(out var pixels, out var width, out var height);

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
                subResource.DataPointer = (IntPtr) pixels;
                subResource.RowPitch = desc.Width * 4;
                subResource.SlicePitch = 0;
                using var pTexture = new Texture2D(g_pd3dDevice, desc, new[] {subResource});

                // Create texture view
                var srvDesc = new ShaderResourceViewDescription();
                srvDesc.Format = Format.R8G8B8A8_UNorm;
                srvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
                srvDesc.Texture2D.MipLevels = desc.MipLevels;
                srvDesc.Texture2D.MostDetailedMip = 0;

                g_pFontTextureView = new ShaderResourceView(g_pd3dDevice, pTexture, srvDesc);
                g_pFontTextureViewHandle = GCHandle.Alloc(g_pFontTextureView);
            }

            // Store our identifier
            io.Fonts.TexID = GCHandle.ToIntPtr(g_pFontTextureViewHandle);

            // Create texture sampler
            {
                SamplerStateDescription desc = new SamplerStateDescription();
                desc.Filter = Filter.MinMagMipLinear;
                desc.AddressU = TextureAddressMode.Wrap;
                desc.AddressV = TextureAddressMode.Wrap;
                desc.AddressW = TextureAddressMode.Wrap;
                desc.ComparisonFunction = Comparison.Always;
                g_pFontSampler = new SamplerState(g_pd3dDevice, desc);
            }
        }

        public bool ImGui_ImplDX11_CreateDeviceObjects()
        {
            if (g_pd3dDevice == null)
                return false;
            if (g_pFontSampler != null)
                ImGui_ImplDX11_InvalidateDeviceObjects();

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

                g_pVertexShaderBlob = ShaderBytecode.Compile(vertexShader, "main", "vs_4_0");

                if (g_pVertexShaderBlob == null
                ) // NB: Pass ID3D10Blob* pErrorBlob to D3DCompile() to get error showing in (const char*)pErrorBlob.GetBufferPointer(). Make sure to Release() the blob!
                    return false;

                g_pVertexShader = new VertexShader(g_pd3dDevice, g_pVertexShaderBlob);

                // Create the input layout
                var local_layout = new InputElement[]
                {
                    new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                    new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0),
                };
                g_pInputLayout = new InputLayout(g_pd3dDevice, g_pVertexShaderBlob, local_layout);

                // Create the constant buffer
                {
                    BufferDescription desc = new BufferDescription();
                    desc.SizeInBytes = 16 * sizeof(float);
                    desc.Usage = ResourceUsage.Dynamic;
                    desc.BindFlags = BindFlags.ConstantBuffer;
                    desc.CpuAccessFlags = CpuAccessFlags.Write;
                    g_pVertexConstantBuffer = new Buffer(g_pd3dDevice, desc);
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

                g_pPixelShaderBlob = ShaderBytecode.Compile(pixelShader, "main", "ps_4_0");

                if (g_pPixelShaderBlob == null
                ) // NB: Pass ID3D10Blob* pErrorBlob to D3DCompile() to get error showing in (const char*)pErrorBlob.GetBufferPointer(). Make sure to Release() the blob!
                    return false;

                g_pPixelShader = new PixelShader(g_pd3dDevice, g_pPixelShaderBlob);
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
                g_pBlendState = new BlendState(g_pd3dDevice, desc);
            }

            // Create the rasterizer state
            {
                RasterizerStateDescription desc = new RasterizerStateDescription();
                desc.FillMode = FillMode.Solid;
                desc.CullMode = CullMode.None;
                desc.IsScissorEnabled = true;
                desc.IsDepthClipEnabled = true;
                g_pRasterizerState = new RasterizerState(g_pd3dDevice, desc);
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
                g_pDepthStencilState = new DepthStencilState(g_pd3dDevice, desc);
            }

            ImGui_ImplDX11_CreateFontsTexture();

            return true;
        }

        public void ImGui_ImplDX11_InvalidateDeviceObjects()
        {
            if (g_pd3dDevice == null)
                return;

            if (g_pFontSampler != null)
            {
                g_pFontSampler.Dispose();
                g_pFontSampler = null;
            }

            if (g_pFontTextureView != null)
            {
                g_pFontTextureViewHandle.Free();
                g_pFontTextureView.Dispose();
                g_pFontTextureView = null;
                ImGui.GetIO().Fonts.TexID = IntPtr.Zero;
            }

            if (g_pIB != null)
            {
                g_pIB.Dispose();
                g_pIB = null;
            }

            if (g_pVB != null)
            {
                g_pVB.Dispose();
                g_pVB = null;
            }

            if (g_pBlendState != null)
            {
                g_pBlendState.Dispose();
                g_pBlendState = null;
            }

            if (g_pDepthStencilState != null)
            {
                g_pDepthStencilState.Dispose();
                g_pDepthStencilState = null;
            }

            if (g_pRasterizerState != null)
            {
                g_pRasterizerState.Dispose();
                g_pRasterizerState = null;
            }

            if (g_pPixelShader != null)
            {
                g_pPixelShader.Dispose();
                g_pPixelShader = null;
            }

            if (g_pVertexConstantBuffer != null)
            {
                g_pVertexConstantBuffer.Dispose();
                g_pVertexConstantBuffer = null;
            }

            if (g_pInputLayout != null)
            {
                g_pInputLayout.Dispose();
                g_pInputLayout = null;
            }

            if (g_pVertexShader != null)
            {
                g_pVertexShader.Dispose();
                g_pVertexShader = null;
            }
        }

        public bool ImGui_ImplDX11_Init(IntPtr hwnd, Device device, DeviceContext device_context)
        {
            g_hWnd = hwnd;
            g_pd3dDevice = device;
            g_pd3dDeviceContext = device_context;

            g_TicksPerSecond = Stopwatch.Frequency;
            g_Time = Stopwatch.GetTimestamp();

            // Keyboard mapping. ImGui will use those indices to peek into the io.KeyDown[] array that we will update during the application lifetime.
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int) ImGuiKey.Tab] = (int) VirtualKey.VK_TAB;
            io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) VirtualKey.VK_LEFT;
            io.KeyMap[(int) ImGuiKey.RightArrow] = (int) VirtualKey.VK_RIGHT;
            io.KeyMap[(int) ImGuiKey.UpArrow] = (int) VirtualKey.VK_UP;
            io.KeyMap[(int) ImGuiKey.DownArrow] = (int) VirtualKey.VK_DOWN;
            io.KeyMap[(int) ImGuiKey.PageUp] = (int) VirtualKey.VK_PRIOR;
            io.KeyMap[(int) ImGuiKey.PageDown] = (int) VirtualKey.VK_NEXT;
            io.KeyMap[(int) ImGuiKey.Home] = (int) VirtualKey.VK_HOME;
            io.KeyMap[(int) ImGuiKey.End] = (int) VirtualKey.VK_END;
            io.KeyMap[(int) ImGuiKey.Delete] = (int) VirtualKey.VK_DELETE;
            io.KeyMap[(int) ImGuiKey.Backspace] = (int) VirtualKey.VK_BACK;
            io.KeyMap[(int) ImGuiKey.Enter] = (int) VirtualKey.VK_RETURN;
            io.KeyMap[(int) ImGuiKey.Escape] = (int) VirtualKey.VK_ESCAPE;
            io.KeyMap[(int) ImGuiKey.A] = 'A';
            io.KeyMap[(int) ImGuiKey.C] = 'C';
            io.KeyMap[(int) ImGuiKey.V] = 'V';
            io.KeyMap[(int) ImGuiKey.X] = 'X';
            io.KeyMap[(int) ImGuiKey.Y] = 'Y';
            io.KeyMap[(int) ImGuiKey.Z] = 'Z';

            io.ImeWindowHandle = g_hWnd;

            return true;
        }

        public void ImGui_ImplDX11_Shutdown()
        {
            ImGui_ImplDX11_InvalidateDeviceObjects();
            g_pd3dDevice = null;
            g_pd3dDeviceContext = null;
            g_hWnd = IntPtr.Zero;
        }

        public void ImGui_ImplDX11_NewFrame(int width, int height)
        {
            if (g_pFontSampler == null)
                ImGui_ImplDX11_CreateDeviceObjects();

            var io = ImGui.GetIO();

            // Setup display size (every frame to accommodate for window resizing)
            io.DisplaySize = new Vector2(width, height);

            // Setup time step
            var currentTime = Stopwatch.GetTimestamp();
            io.DeltaTime = (float) (currentTime - g_Time) / g_TicksPerSecond;
            g_Time = currentTime;

            // Read keyboard modifiers inputs
            io.KeyCtrl = (GetKeyState(VirtualKey.VK_CONTROL) & 0x8000) != 0;
            io.KeyShift = (GetKeyState(VirtualKey.VK_SHIFT) & 0x8000) != 0;
            io.KeyAlt = (GetKeyState(VirtualKey.VK_MENU) & 0x8000) != 0;
            io.KeySuper = false;
            // io.KeysDown : filled by WM_KEYDOWN/WM_KEYUP events
            // io.MousePos : filled by WM_MOUSEMOVE events
            // io.MouseDown : filled by WM_*BUTTON* events
            // io.MouseWheel : filled by WM_MOUSEWHEEL events

            // Start the frame
            ImGui.NewFrame();
        }


        [DllImport("USER32.dll")]
        private static extern short GetKeyState(VirtualKey nVirtKey);

        private const uint WM_CHAR = 0x0102;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MBUTTONDOWN = 0x0207;
        private const uint WM_MBUTTONUP = 0x0208;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
    }
}