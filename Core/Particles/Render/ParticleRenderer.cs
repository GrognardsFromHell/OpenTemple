using System;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Particles.Spec;

namespace OpenTemple.Core.Particles.Render
{
    public abstract class ParticleRenderer
    {
        public abstract void Render(PartSysEmitter emitter);

        protected ParticleRenderer(RenderingDevice device)
        {
            _device = device;
        }


        private void ExtractScreenSpaceUnitVectors(Matrix4x4 projWorldMatrix)
        {
            // inverse, so screen.world?
            var invProj = Matrix4x4.Transpose(projWorldMatrix);
            screenSpaceUnitX = Vector4.Normalize(new Vector4(invProj.M11, invProj.M12, invProj.M13, 0));
            screenSpaceUnitY = Vector4.Normalize(new Vector4(invProj.M21, invProj.M22, invProj.M23, 0));
            screenSpaceUnitZ = Vector4.Normalize(new Vector4(invProj.M31, invProj.M32, invProj.M33, 0));
        }

        // No idea why this is a separate function
        private void ExtractScreenSpaceUnitVectors2(Matrix4x4 projWorldMatrix)
        {
            screenSpaceUnitX.X = projWorldMatrix.M11;
            screenSpaceUnitX.Y = projWorldMatrix.M12;
            screenSpaceUnitX.Z = projWorldMatrix.M13;
            screenSpaceUnitX.W = 0;
            screenSpaceUnitY.X = projWorldMatrix.M31;
            screenSpaceUnitY.Y = projWorldMatrix.M32;
            screenSpaceUnitY.Z = projWorldMatrix.M33;
            screenSpaceUnitY.W = 0;
            screenSpaceUnitZ.X = projWorldMatrix.M21;
            screenSpaceUnitZ.Y = projWorldMatrix.M22;
            screenSpaceUnitZ.Z = projWorldMatrix.M23;
            screenSpaceUnitZ.W = 0;

            screenSpaceUnitX = Vector4.Normalize(screenSpaceUnitX);
            screenSpaceUnitY = Vector4.Normalize(screenSpaceUnitY);
            screenSpaceUnitZ = Vector4.Normalize(screenSpaceUnitZ);
        }

        protected bool GetEmitterWorldMatrix(PartSysEmitter emitter, out Matrix4x4 worldMatrix)
        {
            worldMatrix = Matrix4x4.Identity;

            var spec = emitter.GetSpec();
            var particleSpace = spec.GetParticleSpace();
            var emitterSpace = spec.GetSpace();
            if (particleSpace == PartSysParticleSpace.SameAsEmitter)
            {
                if (emitterSpace == PartSysEmitterSpace.ObjectPos || emitterSpace == PartSysEmitterSpace.ObjectYpr)
                {
                    Matrix4x4 localMat;
                    if (emitterSpace == PartSysEmitterSpace.ObjectYpr)
                    {
                        var angle = emitter.GetObjRotation() + MathF.PI;
                        localMat = Matrix4x4.CreateRotationY(angle);
                    }
                    else
                    {
                        localMat = Matrix4x4.Identity;
                    }

                    // Set the translation component of the transformation matrix
                    localMat.Translation = emitter.GetObjPos();

                    worldMatrix = localMat * _device.GetCamera().GetViewProj();
                    ExtractScreenSpaceUnitVectors(worldMatrix);
                    return true;
                }

                if (emitterSpace == PartSysEmitterSpace.NodePos || emitterSpace == PartSysEmitterSpace.NodeYpr)
                {
                    var external = emitter.External;

                    Matrix4x4 boneMatrix;
                    if (emitterSpace == PartSysEmitterSpace.NodeYpr)
                    {
                        if (!external.GetBoneWorldMatrix(emitter.GetAttachedTo(), spec.GetNodeName(), out boneMatrix))
                        {
                            // This effectively acts as a fallback if the bone doesn't exist
                            boneMatrix = Matrix4x4.CreateTranslation(emitter.GetObjPos());
                        }

                        worldMatrix = boneMatrix * _device.GetCamera().GetViewProj();
                        ExtractScreenSpaceUnitVectors(worldMatrix);
                        return true;
                    }

                    if (external.GetBoneWorldMatrix(emitter.GetAttachedTo(), spec.GetNodeName(), out boneMatrix))
                    {
                        worldMatrix = Matrix4x4.CreateTranslation(boneMatrix.Translation) *
                                      _device.GetCamera().GetViewProj();
                        ExtractScreenSpaceUnitVectors(worldMatrix);
                        return true;
                    }

                    return false;
                }

                worldMatrix = _device.GetCamera().GetViewProj();
                ExtractScreenSpaceUnitVectors(worldMatrix);
                return true;
            }

            if (particleSpace == PartSysParticleSpace.World)
            {
                worldMatrix = _device.GetCamera().GetViewProj();
                ExtractScreenSpaceUnitVectors(worldMatrix);
                return true;
            }

            if (emitterSpace != PartSysEmitterSpace.ObjectPos && emitterSpace != PartSysEmitterSpace.ObjectYpr)
            {
                if (emitterSpace != PartSysEmitterSpace.NodePos && emitterSpace != PartSysEmitterSpace.NodeYpr)
                    return true;

                var external = emitter.External;

                Matrix4x4 boneMatrix;
                if (emitterSpace == PartSysEmitterSpace.NodeYpr)
                {
                    // Use the entire bone matrix if possible
                    external.GetBoneWorldMatrix(emitter.GetAttachedTo(), spec.GetNodeName(), out boneMatrix);
                }
                else
                {
                    // Only use the bone translation part
                    if (!external.GetBoneWorldMatrix(emitter.GetAttachedTo(), spec.GetNodeName(), out boneMatrix))
                        return false;
                    boneMatrix =
                        Matrix4x4.CreateTranslation(boneMatrix.Translation); // TODO: This might not be needed...
                }

                worldMatrix = _device.GetCamera().GetViewProj();
                ExtractScreenSpaceUnitVectors2(boneMatrix);
                return true;
            }

            Matrix4x4 matrix;
            if (emitterSpace == PartSysEmitterSpace.ObjectYpr)
            {
                var angle = emitter.GetObjRotation() + MathF.PI;
                matrix = Matrix4x4.CreateRotationY(angle);
            }
            else
            {
                matrix = Matrix4x4.Identity;
            }

            worldMatrix = _device.GetCamera().GetViewProj();
            ExtractScreenSpaceUnitVectors2(matrix);
            return true;
        }

        /*
            These vectors can be multiplied with screen space
            coordinates to get world coordinates.
        */
        protected Vector4 screenSpaceUnitX;
        protected Vector4 screenSpaceUnitY;
        protected Vector4 screenSpaceUnitZ;

        private readonly RenderingDevice _device;
    }
}