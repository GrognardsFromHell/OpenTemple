using OpenTemple.Core.GFX.Materials;
using SharpGen.Runtime;
using Vortice.Direct3D11;

namespace OpenTemple.Core.GFX
{
    public abstract class PipelineState<TSelf, TSpec, TGpuObject> : GpuResource<TSelf> where TSelf : GpuResource<TSelf>
        where TGpuObject : DisposeBase
    {
        public TSpec Spec { get; }

        public TGpuObject GpuState { get; internal set; }

        public PipelineState(RenderingDevice device, TSpec spec, TGpuObject gpuObject) :
            base()
        {
            Spec = spec;
            GpuState = gpuObject;
        }

        protected override void FreeResource()
        {
            GpuState?.Dispose();
            GpuState = null;
        }
    }

    public class RasterizerState : PipelineState<RasterizerState, RasterizerSpec, ID3D11RasterizerState>
    {
        public RasterizerState(RenderingDevice device, RasterizerSpec spec, ID3D11RasterizerState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

    public class BlendState : PipelineState<BlendState, BlendSpec, ID3D11BlendState>
    {
        public BlendState(RenderingDevice device, BlendSpec spec, ID3D11BlendState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

    public class DepthStencilState : PipelineState<DepthStencilState, DepthStencilSpec, ID3D11DepthStencilState>
    {
        public DepthStencilState(RenderingDevice device, DepthStencilSpec spec, ID3D11DepthStencilState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }


    public class SamplerState : PipelineState<SamplerState, SamplerSpec, ID3D11SamplerState>
    {
        public SamplerState(RenderingDevice device, SamplerSpec spec, ID3D11SamplerState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

}