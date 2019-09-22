using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class VfxSystem : IDisposable
    {
        [TempleDllLocation(0x10b397b4)]
        private int pfx_lightning_render;

        [TempleDllLocation(0x10ab7e54)]
        private int pfx_chain_lightning_render;

        [TempleDllLocation(0x10ab7f8c)]
        private int pfx_render_lightning_bolt;

        [TempleDllLocation(0x10b397b8)]
        private Vector2 lightning_scratch;

        [TempleDllLocation(0x10b397b0)]
        private TimePoint call_lightning_start;

        [TempleDllLocation(0x10ab7e50)]
        private TimePoint chain_lightning_start;

        [TempleDllLocation(0x10ab7e58)]
        private Vector3 chain_lightning_source;

        [TempleDllLocation(0x10ab7e68)]
        [TempleDllLocation(0x10ab7e64)]
        private List<ChainLightningTarget> chain_lightning_targets = new List<ChainLightningTarget>();

        private struct ChainLightningTarget
        {
            public GameObjectBody Object;
            public Vector3 Location;
        }

        [TempleDllLocation(0x10ab7f88)]
        private TimePoint lightning_bolt_start;

        [TempleDllLocation(0x10ab7f90)]
        private Vector3 lightning_bolt_source;

        [TempleDllLocation(0x10ab7f9c)]
        private Vector3 lightning_bolt_target;

        // Initializes several random tables, vectors and shuffled int lists as well as the lightning material.
        [TempleDllLocation(0x10087220)]
        public VfxSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10087440)]
        public void CallLightning(LocAndOffsets location)
        {
            lightning_scratch = location.ToInches2D();
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
            chain_lightning_source = GetCastingRef(caster);

            foreach (var target in targets)
            {
                var renderHeight = target.GetRenderHeight();
                var location = target.GetLocationFull().ToInches3D(renderHeight);

                chain_lightning_targets.Add(new ChainLightningTarget
                {
                    Object = target,
                    Location = location
                });
            }

            pfx_chain_lightning_render = 1;
        }

        [TempleDllLocation(0x10087e60)]
        public void Render()
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

            if (pfx_chain_lightning_render == 1)
            {
                chain_lightning_start = TimePoint.Now;
                pfx_chain_lightning_render = 2;

                RenderChainLightning();
            }
            else if (pfx_chain_lightning_render == 2)
            {
                RenderChainLightning();
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

        private void RenderChainLightning()
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