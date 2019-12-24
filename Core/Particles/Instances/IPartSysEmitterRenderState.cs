using System;

namespace OpenTemple.Core.Particles.Instances
{
    /// <summary>
    /// Rendersystem specific state that can be attached to a particle
    /// system emitter. It's destroyed along with the particle system
    /// emitter.
    /// </summary>
    public interface IPartSysEmitterRenderState : IDisposable {
    }
}