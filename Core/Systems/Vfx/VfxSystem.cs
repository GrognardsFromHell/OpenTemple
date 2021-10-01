using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.Vfx
{
    public class VfxSystem : IDisposable
    {
        private readonly ChainLightningRenderer _chainLightningRenderer;
        private readonly LightningBoltRenderer _lightningBoltRenderer;
        private readonly CallLightningRenderer _callLightningRenderer;

        private CallLightningEffect _callLightningEffect;

        private ChainLightningEffect _chainLightningEffect;

        private LightningBoltEffect _lightningBoltEffect;

        public PerlinNoise Noise { get; } = new();

        // Initializes several random tables, vectors and shuffled int lists as well as the lightning material.
        [TempleDllLocation(0x10087220)]
        public VfxSystem()
        {
            _callLightningRenderer = new CallLightningRenderer(Noise);
            _chainLightningRenderer = new ChainLightningRenderer(Noise);
            _lightningBoltRenderer = new LightningBoltRenderer(Noise);
        }

        [TempleDllLocation(0x10087440)]
        public void CallLightning(LocAndOffsets location)
        {
            _callLightningEffect = new CallLightningEffect(_callLightningRenderer, TimePoint.Now, location.ToInches3D());
        }

        [TempleDllLocation(0x100875c0)]
        public void LightningBolt(GameObjectBody caster, LocAndOffsets targetLocation)
        {
            // Make it so the lightning bolt originates directly at the caster's fingertips
            var source = GetCastingRef(caster);

            // Calculate a normalized direction vector and multiply with 1440 (which is the max. lightning bolt range)
            var target = targetLocation.ToInches3D(source.Y);

            _lightningBoltEffect = new LightningBoltEffect(_lightningBoltRenderer, TimePoint.Now, source, target);
        }

        [TempleDllLocation(0x10087480)]
        public void ChainLightning(GameObjectBody caster, IEnumerable<GameObjectBody> targets)
        {
            var origin = GetCastingRef(caster);

            var effectTargets = new List<ChainLightningTarget>();
            foreach (var target in targets)
            {
                var renderHeight = target.GetRenderHeight();
                var location = target.GetLocationFull().ToInches3D(renderHeight);

                effectTargets.Add(new ChainLightningTarget(target, location));
            }

            _chainLightningEffect = new ChainLightningEffect(_chainLightningRenderer, TimePoint.Now, origin, effectTargets);
        }

        private static Vector3 GetCastingRef(GameObjectBody caster)
        {
            var animParams = caster.GetAnimParams();
            var animModel = caster.GetOrCreateAnimHandle();
            if (animModel.GetBoneWorldMatrixByName(in animParams, "Casting_Ref", out var boneMatrix))
            {
                return boneMatrix.Translation;
            }

            return caster.GetLocationFull().ToInches3D();
        }

        [TempleDllLocation(0x10087e60)]
        public void Render(IGameViewport viewport)
        {
            if (_callLightningEffect != null)
            {
                _callLightningEffect.Render(viewport);
                if (_callLightningEffect.AtEnd)
                {
                    _callLightningEffect = null;
                }
            }

            if (_chainLightningEffect != null)
            {
                _chainLightningEffect.Render(viewport);
                if (_chainLightningEffect.AtEnd)
                {
                    _chainLightningEffect = null;
                }
            }

            if (_lightningBoltEffect != null)
            {
                _lightningBoltEffect.Render(viewport);
                if (_lightningBoltEffect.AtEnd)
                {
                    _lightningBoltEffect = null;
                }
            }
        }

        public void Dispose()
        {
            _callLightningRenderer.Dispose();
            _chainLightningRenderer.Dispose();
            _lightningBoltRenderer.Dispose();
        }
    }
}