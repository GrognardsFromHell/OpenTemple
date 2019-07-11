using System;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Particles.Instances;
using SpicyTemple.Core.Utils;
using SpicyTemple.Particles.Params;

namespace SpicyTemple.Core.Particles.Render
{
    internal class SpriteParticleRenderer : QuadParticleRenderer
    {
        public SpriteParticleRenderer(RenderingDevice device) : base(device)
        {
        }

        protected override void FillVertex(PartSysEmitter emitter,
            int particleIdx,
            Span<SpriteVertex> vertices,
            int vertexIdx)
        {
            // Calculate the particle scale (default is 1)
            var scale = 1.0f;
            var scaleParam = emitter.GetParamState(PartSysParamId.part_scale_X);
            if (scaleParam != null)
            {
                scale = scaleParam.GetValue(emitter, particleIdx,
                    emitter.GetParticleAge(particleIdx));
            }

            Vector4 halfPartHeightX;
            Vector4 halfPartHeightY;
            var rotationParam = emitter.GetParamState(PartSysParamId.part_yaw);
            if (rotationParam != null)
            {
                var rotation = rotationParam.GetValue(
                    emitter, particleIdx, emitter.GetParticleAge(particleIdx));
                rotation += emitter.GetParticleState().GetState(ParticleStateField.PSF_ROTATION, particleIdx);
                rotation = Angles.ToRadians(rotation);

                var cosRot = MathF.Cos(rotation) * scale;
                var sinRot = MathF.Sin(rotation) * scale;
                halfPartHeightX = screenSpaceUnitX * cosRot - screenSpaceUnitY * sinRot;
                halfPartHeightY = screenSpaceUnitY * cosRot + screenSpaceUnitX * sinRot;
            }
            else
            {
                halfPartHeightX = screenSpaceUnitX * scale;
                halfPartHeightY = screenSpaceUnitY * scale;
            }

            var partPos = new Vector4(
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_X, particleIdx),
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_Y, particleIdx),
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_Z, particleIdx),
                1
            );

            // Upper left corner
            vertices[vertexIdx + 0].pos = partPos - halfPartHeightX + halfPartHeightY;
            // Upper right corner
            vertices[vertexIdx + 1].pos = partPos + halfPartHeightX + halfPartHeightY;
            // Lower right corner
            vertices[vertexIdx + 2].pos = partPos + halfPartHeightX - halfPartHeightY;
            // Lower left corner
            vertices[vertexIdx + 3].pos = partPos - halfPartHeightX - halfPartHeightY;

            // Set the diffuse color for all corners
            var diffuse = GeneralEmitterRenderState.GetParticleColor(emitter, particleIdx);
            vertices[vertexIdx + 0].diffuse = diffuse;
            vertices[vertexIdx + 1].diffuse = diffuse;
            vertices[vertexIdx + 2].diffuse = diffuse;
            vertices[vertexIdx + 3].diffuse = diffuse;

            // Set UV coordinates for the sprite. We need to do this every frame
            // because we're using DISCARD for locking
            vertices[vertexIdx + 0].u = 0;
            vertices[vertexIdx + 0].v = 1;
            vertices[vertexIdx + 1].u = 1;
            vertices[vertexIdx + 1].v = 1;
            vertices[vertexIdx + 2].u = 1;
            vertices[vertexIdx + 2].v = 0;
            vertices[vertexIdx + 3].u = 0;
            vertices[vertexIdx + 3].v = 0;
        }
    }
}