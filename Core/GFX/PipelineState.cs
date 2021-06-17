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

    /// <summary>
    /// Rasterizer state is special in that it contains a flag to indicate whether multisampling is enabled,
    /// which we actually want to automatically set to true for multisampled render targets.
    /// </summary>
    public class RasterizerState : GpuResource<RasterizerState>
    {
        public RasterizerSpec Spec { get; }

        public SharpDX.Direct3D11.RasterizerState State { get; internal set; }

        public SharpDX.Direct3D11.RasterizerState MultiSamplingState { get; internal set; }

        public RasterizerState(RasterizerSpec spec,
            SharpDX.Direct3D11.RasterizerState state,
            SharpDX.Direct3D11.RasterizerState multiSamplingState)
        {
            Spec = spec;
            State = state;
            MultiSamplingState = multiSamplingState;
        }

        protected override void FreeResource()
        {
            State?.Dispose();
            State = null;
            MultiSamplingState?.Dispose();
            MultiSamplingState = null;
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