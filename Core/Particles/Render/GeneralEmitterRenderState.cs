using System;
using System.Collections.Generic;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Particles.Spec;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Render
{
    public class GeneralEmitterRenderState : IPartSysEmitterRenderState
    {
        public ResourceRef<Material> material;

        public GeneralEmitterRenderState(RenderingDevice device, PartSysEmitter emitter, bool pointSprites)
        {
            material = CreateMaterial(device, emitter, pointSprites).Ref();
        }

        public static Material CreateMaterial(RenderingDevice device,
            PartSysEmitter emitter,
            bool pointSprites)
        {
            var blendState = new BlendSpec();
            blendState.blendEnable = true;

            switch (emitter.GetSpec().GetBlendMode())
            {
                case PartSysBlendMode.Add:
                    blendState.srcBlend = BlendOperand.SrcAlpha;
                    blendState.destBlend = BlendOperand.One;
                    break;
                case PartSysBlendMode.Subtract:
                    blendState.srcBlend = BlendOperand.Zero;
                    blendState.destBlend = BlendOperand.InvSrcAlpha;
                    break;
                case PartSysBlendMode.Blend:
                    blendState.srcBlend = BlendOperand.SrcAlpha;
                    blendState.destBlend = BlendOperand.InvSrcAlpha;
                    break;
                case PartSysBlendMode.Multiply:
                    blendState.srcBlend = BlendOperand.Zero;
                    blendState.destBlend = BlendOperand.SrcColor;
                    break;
                default:
                    break;
            }

            // Particles respect the depth buffer, but do not modify it
            DepthStencilSpec depthStencilState = new DepthStencilSpec();
            depthStencilState.depthEnable = true;
            depthStencilState.depthWrite = false;
            RasterizerSpec rasterizerState = new RasterizerSpec();
            rasterizerState.cullMode = CullMode.None;


            var samplers = new List<MaterialSamplerSpec>();

            var shaderName = "diffuse_only_ps";
            var textureName = emitter.GetSpec().GetTextureName();
            if (textureName.Length > 0)
            {
                var samplerState = new SamplerSpec
                {
                    addressU = TextureAddress.Clamp,
                    addressV = TextureAddress.Clamp,
                    minFilter = TextureFilterType.Linear,
                    magFilter = TextureFilterType.Linear,
                    mipFilter = TextureFilterType.Linear
                };
                var texture = device.GetTextures().Resolve(textureName, true);
                samplers.Add(new MaterialSamplerSpec(texture, samplerState));
                shaderName = "textured_simple_ps";
            }

            using var pixelShader = device.GetShaders().LoadPixelShader(shaderName);

            var vsName = pointSprites ? "particles_points_vs" : "particles_quads_vs";

            using var vertexShader = device.GetShaders().LoadVertexShader(vsName);

            return device.CreateMaterial(blendState, depthStencilState, rasterizerState,
                samplers.ToArray(), vertexShader, pixelShader);
        }

        public static uint CoerceToInteger(float value)
        {
            Span<byte> buffer = stackalloc byte[4];
            BitConverter.TryWriteBytes(buffer, value);
            return BitConverter.ToUInt32(buffer);
        }

        private static byte GetParticleColorComponent(ParticleStateField stateField,
            PartSysParamId paramId,
            PartSysEmitter emitter,
            int particleIdx)
        {
            var colorParam = emitter.GetParamState(paramId);
            byte value;
            if (colorParam != null)
            {
                var partAge = emitter.GetParticleAge(particleIdx);
                var partColor = colorParam.GetValue(emitter, particleIdx, partAge);
                partColor += emitter.GetParticleState().GetState(stateField, particleIdx);
                if (partColor >= 255)
                {
                    value = 255;
                }
                else if (partColor < 0)
                {
                    value = 0;
                }
                else
                {
                    value = (byte) partColor;
                }
            }
            else
            {
                value =
                    (byte) emitter.GetParticleState().GetState(stateField, particleIdx);
            }

            return value;
        }

        public static PackedLinearColorA GetParticleColor(PartSysEmitter emitter, int particleIdx)
        {
            var red =
                GetParticleColorComponent(ParticleStateField.PSF_RED, PartSysParamId.part_red, emitter, particleIdx);
            var green =
                GetParticleColorComponent(ParticleStateField.PSF_GREEN, PartSysParamId.part_green, emitter,
                    particleIdx);
            var blue =
                GetParticleColorComponent(ParticleStateField.PSF_BLUE, PartSysParamId.part_blue, emitter, particleIdx);
            var alpha =
                GetParticleColorComponent(ParticleStateField.PSF_ALPHA, PartSysParamId.part_alpha, emitter,
                    particleIdx);

            return new PackedLinearColorA(red, green, blue, alpha);
        }

        public void Dispose()
        {
            material.Dispose();
        }
    }
}