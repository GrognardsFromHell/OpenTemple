using System;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Spec;

namespace OpenTemple.Core.Particles.Render
{
    public class ParticleRendererManager : IDisposable
    {
        public ParticleRendererManager(RenderingDevice device,
            IAnimatedModelFactory modelFactory,
            IAnimatedModelRenderer modelRenderer)
        {
            _spriteRenderer = new SpriteParticleRenderer(device);
            _discRenderer = new DiscParticleRenderer(device);
            _modelRenderer = new ModelParticleRenderer(device, modelFactory, modelRenderer);
        }

        public ParticleRenderer GetRenderer(PartSysParticleType type)
        {
            switch (type)
            {
                case PartSysParticleType.Point:
                case PartSysParticleType.Sprite:
                    return _spriteRenderer;
                case PartSysParticleType.Disc:
                    return _discRenderer;
                case PartSysParticleType.Model:
                    return _modelRenderer;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private readonly SpriteParticleRenderer _spriteRenderer;
        private readonly DiscParticleRenderer _discRenderer;
        private readonly ModelParticleRenderer _modelRenderer;

        public void Dispose()
        {
            _spriteRenderer.Dispose();
            _discRenderer.Dispose();
        }
    }
}