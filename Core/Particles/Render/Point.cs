using System;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Utils;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Render
{
    internal class DiscParticleRenderer : QuadParticleRenderer
    {
        public DiscParticleRenderer(RenderingDevice device) : base(device)
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
                halfPartHeightX = new Vector4(-cosRot, 0, sinRot, 0);
                halfPartHeightY = new Vector4(sinRot, 0, cosRot, 0);
            }
            else
            {
                halfPartHeightX = new Vector4(scale, 0, 0, 0);
                halfPartHeightY = new Vector4(0, 0, scale, 0);
            }

            var partPos = new Vector4(
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_X, particleIdx),
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_Y, particleIdx),
                emitter.GetParticleState().GetState(ParticleStateField.PSF_POS_VAR_Z, particleIdx),
                1
            );

            // Upper left corner
            vertices[0].pos = partPos - halfPartHeightX - halfPartHeightY;
            // Upper right corner
            vertices[1].pos = partPos + halfPartHeightX - halfPartHeightY;
            // Lower right corner
            vertices[2].pos = partPos + halfPartHeightX + halfPartHeightY;
            // Lower left corner
            vertices[3].pos = partPos - halfPartHeightX + halfPartHeightY;

            // Set the diffuse color for all corners
            var diffuse = GeneralEmitterRenderState.GetParticleColor(emitter, particleIdx);
            vertices[0].diffuse = diffuse;
            vertices[1].diffuse = diffuse;
            vertices[2].diffuse = diffuse;
            vertices[3].diffuse = diffuse;

            // Set UV coordinates for the sprite. We need to do this every frame
            // because we're using DISCARD for locking
            vertices[0].u = 0;
            vertices[0].v = 0;
            vertices[1].u = 1;
            vertices[1].v = 0;
            vertices[2].u = 1;
            vertices[2].v = 1;
            vertices[3].u = 0;
            vertices[3].v = 1;
        }
    }
}