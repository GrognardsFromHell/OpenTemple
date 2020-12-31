using System;
using OpenTemple.Core.Utils;
using SharpDX.Mathematics.Interop;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace OpenTemple.Core.GFX
{
    /// <summary>
    /// Not very elegant, but since we share a D3D11 device with Angle,
    /// we'll snapshot the state before we draw, and then restore it after we're done.
    /// </summary>
    public class GpuStateSnapshot : IDisposable
    {
        private readonly ID3D11DeviceContext _context;
        private readonly RawRect[] _scissorRects;
        private readonly Viewport[] _viewports;
        private readonly ID3D11RasterizerState _rs;
        private readonly ID3D11BlendState _blendState;
        private readonly Color4 _blendFactor;
        private readonly int _sampleMask;
        private readonly int _stencilRef;
        private readonly ID3D11DepthStencilState _depthStencilState;
        private readonly ID3D11ShaderResourceView[] _psShaderResource;
        private readonly ID3D11SamplerState[] _psSampler;
        private readonly ID3D11PixelShader _ps;
        private readonly ID3D11VertexShader _vs;
        private readonly PrimitiveTopology _primitiveTopology;
        private readonly ID3D11Buffer[] _vsConstantBuffer;
        private readonly ID3D11Buffer _indexBuffer;
        private readonly ID3D11Buffer[] _vertexBuffer = new ID3D11Buffer[1];
        private readonly int _indexBufferOffset;
        private readonly int[] _vertexBufferStride = new int[1];
        private readonly int[] _vertexBufferOffset = new int[1];
        private readonly Format _indexBufferFormat;
        private readonly ID3D11InputLayout _inputLayout;

        public GpuStateSnapshot(IntPtr handle)
        {
            _context = new ID3D11DeviceContext(handle);

            _scissorRects = new RawRect[16];
            _context.RSGetScissorRects(_scissorRects);

            _viewports = new Viewport[16];
            _context.RSGetViewports(_viewports);

            _rs = _context.RSGetState();
            _blendState = _context.OMGetBlendState(out _blendFactor, out _sampleMask);
            _context.OMGetDepthStencilState(out _depthStencilState, out _stencilRef);
            _psShaderResource = new ID3D11ShaderResourceView[1];
            _context.PSGetShaderResources(0, 1, _psShaderResource);
            _psSampler = new ID3D11SamplerState[4];
            _context.PSGetSamplers(0, _psSampler);

            _ps = _context.PSGetShader();
            _vs = _context.VSGetShader();
            _vsConstantBuffer = new ID3D11Buffer[1];
            _context.VSGetConstantBuffers(0, _vsConstantBuffer);
            _primitiveTopology = _context.IAGetPrimitiveTopology();
            _context.IAGetIndexBuffer(out _indexBuffer, out _indexBufferFormat,
                out _indexBufferOffset);
            _context.IAGetVertexBuffers(0, 1, _vertexBuffer, _vertexBufferStride, _vertexBufferOffset);
            _inputLayout = _context.IAGetInputLayout();
        }

        public void Restore()
        {
            // Restore modified DX state
            _context.RSSetScissorRects(_scissorRects);
            _context.RSSetViewports(_viewports);
            _context.RSSetState(_rs);
            _context.OMSetBlendState(_blendState, _blendFactor, _sampleMask);
            _context.OMSetDepthStencilState(_depthStencilState, _stencilRef);
            _context.PSSetShaderResources(0, _psShaderResource);
            _context.PSSetSamplers(0, _psSampler);
            _context.PSSetShader(_ps);
            _context.VSSetShader(_vs);
            _context.VSSetConstantBuffers(0, _vsConstantBuffer);
            _context.IASetPrimitiveTopology(_primitiveTopology);
            _context.IASetIndexBuffer(_indexBuffer, _indexBufferFormat, _indexBufferOffset);
            _context.IASetVertexBuffers(0, 1, _vertexBuffer, _vertexBufferStride, _vertexBufferOffset);
            _context.IASetInputLayout(_inputLayout);
        }

        public void Dispose()
        {
            _rs?.Dispose();
            _blendState?.Dispose();
            _depthStencilState?.Dispose();
            _psShaderResource.DisposeAndNull();
            _psSampler.DisposeAndNull();
            _ps?.Dispose();
            _vs?.Dispose();
            _vsConstantBuffer?.DisposeAndNull();
            _vertexBuffer.DisposeAndNull();
            _indexBuffer?.Dispose();
            _inputLayout?.Dispose();
            _context.Dispose();
        }
    }
}