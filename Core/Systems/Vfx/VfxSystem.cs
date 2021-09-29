using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.Vfx
{
    public class VfxSystem : IDisposable
    {
        [TempleDllLocation(0x10b397b4)]
        private int pfx_lightning_render;

        [TempleDllLocation(0x10ab7f8c)]
        private int pfx_render_lightning_bolt;

        [TempleDllLocation(0x10B397B8)]
        private Vector2 lightning_start;

        [TempleDllLocation(0x10b397b0)]
        private TimePoint call_lightning_start;

        [TempleDllLocation(0x10ab7f88)]
        private TimePoint lightning_bolt_start;

        [TempleDllLocation(0x10ab7f90)]
        private Vector3 lightning_bolt_source;

        [TempleDllLocation(0x10ab7f9c)]
        private Vector3 lightning_bolt_target;

        private readonly ChainLightningRenderer _chainLightningRenderer;

        private ChainLightningEffect _chainLightningEffect;

        public PerlinNoise Noise { get; } = new();

        // Initializes several random tables, vectors and shuffled int lists as well as the lightning material.
        [TempleDllLocation(0x10087220)]
        public VfxSystem()
        {
            pfx_lightning_render = 0;
            pfx_render_lightning_bolt = 0;

            _chainLightningRenderer = new ChainLightningRenderer(Noise);
        }

        [TempleDllLocation(0x10087440)]
        public void CallLightning(LocAndOffsets location)
        {
            lightning_start = location.ToInches2D();
            pfx_lightning_render = 1;
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

        [TempleDllLocation(0x100875c0)]
        public void LightningBolt(GameObjectBody caster, LocAndOffsets targetLocation)
        {
            // Make it so the lightning bolt originates directly at the caster's fingertips
            lightning_bolt_source = GetCastingRef(caster);

            // Calculate a normalized direction vector and multiply with 1440 (which is the max. lightning bolt range)
            var target = targetLocation.ToInches3D(lightning_bolt_source.Y);
            var dir = Vector3.Normalize(target - lightning_bolt_source);
            lightning_bolt_target = dir * 1440.0f;

            pfx_render_lightning_bolt = 1;
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

        [TempleDllLocation(0x10087e60)]
        public void Render(IGameViewport viewport)
        {
            if (pfx_lightning_render == 1)
            {
                call_lightning_start = TimePoint.Now;
                pfx_lightning_render = 2;

                RenderCallLightning();
            }
            else if (pfx_lightning_render == 2)
            {
                RenderCallLightning();
            }

            if (_chainLightningEffect != null)
            {
                _chainLightningEffect.Render(viewport);
                if (_chainLightningEffect.AtEnd)
                {
                    _chainLightningEffect = null;
                }
            }

            if (pfx_render_lightning_bolt == 1)
            {
                lightning_bolt_start = TimePoint.Now;
                pfx_render_lightning_bolt = 2;

                RenderLightningBolt();
            }
            else if (pfx_render_lightning_bolt == 2)
            {
                RenderLightningBolt();
            }
        }

        private void RenderCallLightning()
        {
            throw new NotImplementedException();
        }

        private void RenderLightningBolt()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}