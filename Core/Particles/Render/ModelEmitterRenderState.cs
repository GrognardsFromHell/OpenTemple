using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Particles.Instances;

namespace SpicyTemple.Core.Particles.Render
{
    /// <summary>
    /// The rendering state associated with a point emitter.
    /// </summary>
    internal class ModelEmitterRenderState : IPartSysEmitterRenderState
    {
        public readonly IAnimatedModel Model;

        public ModelEmitterRenderState(IAnimatedModel model)
        {
            this.Model = model;
        }

        public void Dispose()
        {
            // TODO: Free model
            Stub.TODO();
        }
    }
}