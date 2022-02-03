using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;

namespace OpenTemple.Core.Particles.Render;

/// <summary>
/// The rendering state associated with a point emitter.
/// </summary>
internal class ModelEmitterRenderState : IPartSysEmitterRenderState
{
    public readonly IAnimatedModel Model;

    public ModelEmitterRenderState(IAnimatedModel model)
    {
        Model = model;
    }

    public void Dispose()
    {
        // TODO: Free model
        Stub.TODO();
    }
}