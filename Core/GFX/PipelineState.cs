using SharpDX;
using OpenTemple.Core.GFX.Materials;

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

    public class RasterizerState : PipelineState<RasterizerState, RasterizerSpec, SharpDX.Direct3D11.RasterizerState>
    {
        public RasterizerState(RenderingDevice device, RasterizerSpec spec, SharpDX.Direct3D11.RasterizerState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

    public class BlendState : PipelineState<BlendState, BlendSpec, SharpDX.Direct3D11.BlendState>
    {
        public BlendState(RenderingDevice device, BlendSpec spec, SharpDX.Direct3D11.BlendState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

    public class DepthStencilState : PipelineState<DepthStencilState, DepthStencilSpec, SharpDX.Direct3D11.DepthStencilState>
    {
        public DepthStencilState(RenderingDevice device, DepthStencilSpec spec, SharpDX.Direct3D11.DepthStencilState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }


    public class SamplerState : PipelineState<SamplerState, SamplerSpec, SharpDX.Direct3D11.SamplerState>
    {
        public SamplerState(RenderingDevice device, SamplerSpec spec, SharpDX.Direct3D11.SamplerState gpuObject) : base(device, spec, gpuObject)
        {
        }
    }

}