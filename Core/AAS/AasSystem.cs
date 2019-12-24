using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.AAS
{
    public class AasSystem : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public AasSystem(
            IFileSystem fileSystem,
            Func<int, string> getSkeletonFilename,
            Func<int, string> getMeshFilename,
            IMaterialResolver materialResolver)
        {
            fileSystems_ = fileSystem;
            getMeshFilename_ = getMeshFilename;
            getSkeletonFilename_ = getSkeletonFilename;

            materialResolver_ = materialResolver;
        }

        // Originally @ 0x102641B0
        public AasHandle CreateModelFromIds(
            int meshId,
            int skeletonId,
            EncodedAnimId idleAnimId,
            in AasAnimParams animParams
        )
        {
            var skeletonFilename = getSkeletonFilename_(skeletonId);
            if (skeletonFilename.Length == 0)
            {
                throw new AasException($"Could not resolve the filename for skeleton id {skeletonId}");
            }

            var meshFilename = getMeshFilename_(meshId);
            if (meshFilename.Length == 0)
            {
                throw new AasException($"Could not resolve the filename for mesh id {meshId}");
            }

            return CreateModel(meshFilename, skeletonFilename, idleAnimId, animParams);
        }

        public AasHandle CreateModel(
            string meshFilename,
            string skeletonFilename,
            EncodedAnimId idleAnimId,
            in AasAnimParams animParams
        )
        {
            var animHandle = FindFreeHandle();

            var activeModel = new ActiveModel(animHandle);

            activeModel.skeleton = LoadSkeletonFile(skeletonFilename);

            // Get SKM data
            activeModel.mesh = LoadMeshFile(meshFilename);

            activeModel.model = new AnimatedModel();
            activeModel.model.SetSkeleton(activeModel.skeleton);
            activeModel.model.AddMesh(activeModel.mesh, materialResolver_);

            activeModels_[animHandle] = activeModel;

            try
            {
                SetAnimById(animHandle, idleAnimId);
                Advance(animHandle, 0, 0, 0, in animParams);
            }
            catch
            {
                ReleaseModel(animHandle);
                throw;
            }

            return animHandle;
        }

        private Skeleton LoadSkeletonFile(string skeletonFilename)
        {
            var buffer = fileSystems_.ReadBinaryFile(skeletonFilename);

            return new Skeleton(buffer)
            {
                Path = skeletonFilename
            };
        }

        private Mesh LoadMeshFile(string meshFilename)
        {
            var buffer = fileSystems_.ReadBinaryFile(meshFilename);

            return new Mesh(buffer)
            {
                Path = meshFilename
            };
        }

        // Originally @ 10262540
        public bool SetAnimById(AasHandle handle, EncodedAnimId animId)
        {
            var anim = GetActiveModel(handle);

            anim.animId = animId;

            int fallbackState = 0;
            while (true)
            {
                if (anim.model.SetAnimByName(animId.GetName()))
                {
                    return true;
                }

                if (animId.ToFallback())
                {
                    continue;
                }

                if (fallbackState == 0)
                {
                    fallbackState = 1;
                    if (animId.IsWeaponAnim())
                    {
                        // Retry with unarmed version
                        animId = new EncodedAnimId(animId.GetWeaponAnim());
                        continue;
                    }
                }

                if (fallbackState == 1)
                {
                    fallbackState = 2;
                    animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                    continue;
                }

                if (fallbackState == 2)
                {
                    break;
                }
            }

            Logger.Error("aas_anim_set: ERROR: could not fallback anim {0}", animId);
            Logger.Error("            : Anim File: '{0}', Mesh File: '{1}'", anim.skeleton.Path,
                anim.mesh.Path);
            return false;
        }

        // Originally @ 10262690
        public bool HasAnimId(AasHandle handle, EncodedAnimId animId)
        {
            var anim = GetActiveModel(handle);

            int fallbackState = 0;
            while (true)
            {
                if (anim.model.HasAnimation(animId.GetName()))
                {
                    return true;
                }

                if (animId.ToFallback())
                {
                    continue;
                }

                if (fallbackState == 0)
                {
                    fallbackState = 1;
                    if (animId.IsWeaponAnim())
                    {
                        // Retry with unarmed version
                        animId = new EncodedAnimId(animId.GetWeaponAnim());
                        continue;
                    }
                }

                if (fallbackState == 1)
                {
                    fallbackState = 2;
                    animId = new EncodedAnimId(NormalAnimType.ItemIdle);
                    continue;
                }

                if (fallbackState == 2)
                {
                    break;
                }
            }

            Logger.Error("aas_anim_set: ERROR: could not fallback anim {0}", animId);
            Logger.Error("            : Anim File: '{0}', Mesh File: '{1}'", anim.skeleton.Path, anim.mesh.Path);
            return false;
        }

        public EncodedAnimId GetAnimId(AasHandle handle) => GetActiveModel(handle).animId;

        public AnimEvents Advance(AasHandle handle,
            float deltaTimeInSecs,
            float deltaDistance,
            float deltaRotation,
            in AasAnimParams animParams)
        {
            GetWorldMatrix(in animParams, out var worldMatrix);

            var events = new AnimEvents();

            var model = GetAnimatedModel(handle);

            model.EventHandler.SetFlagsOut(events);
            model.SetScale(animParams.scale);
            model.Advance(
                worldMatrix,
                deltaTimeInSecs,
                deltaDistance,
                deltaRotation);
            model.EventHandler.ClearFlagsOut();

            return events;
        }

        public void UpdateWorldMatrix(AasHandle handle, in AasAnimParams animParams, bool forParticles = false)
        {
            var model = GetAnimatedModel(handle);

            Matrix3x4 worldMatrix;
            if (forParticles)
            {
                GetWorldMatrixForParticles(in animParams, out worldMatrix);
            }
            else
            {
                GetWorldMatrix(in animParams, out worldMatrix);
            }

            model.SetWorldMatrix(worldMatrix);
            model.SetScale(animParams.scale);
        }

        public ISubmesh GetSubmesh(AasHandle handle,
            in AasAnimParams animParams,
            int submeshIdx)
        {
            var model = GetAnimatedModel(handle);
            UpdateWorldMatrix(handle, animParams);
            return model.GetSubmesh(submeshIdx);
        }

        public ISubmesh GetSubmeshForParticles(AasHandle handle,
            in AasAnimParams animParams,
            int submeshIdx)
        {
            var model = GetAnimatedModel(handle);
            UpdateWorldMatrix(handle, animParams, true);
            return model.GetSubmesh(submeshIdx);
        }

        public void ReleaseAllModels()
        {
            for (var index = 0; index < activeModels_.Length; index++)
            {
                activeModels_[index]?.Dispose();
                activeModels_[index] = null;
            }
        }

        [TempleDllLocation(0x10264510)]
        public void ReleaseModel(AasHandle handle)
        {
            if (IsValidHandle(handle))
            {
                activeModels_[handle]?.Dispose();
                activeModels_[handle] = null;
            }
        }

        public bool IsValidHandle(AasHandle handle)
        {
            return handle >= 1
                   && handle < MaxAnims
                   && activeModels_[handle] != null;
        }

        internal AnimatedModel GetAnimatedModel(AasHandle handle) => GetActiveModel(handle).model;

        public void AddAdditionalMesh(AasHandle handle, string filename)
        {
            var anim = GetActiveModel(handle);

            // Should actually check if it's already loaded
            var mesh = LoadMeshFile(filename);
            anim.AdditionalMeshes.Add(mesh);

            anim.model.AddMesh(mesh, materialResolver_);
        }

        public void ClearAddmeshes(AasHandle handle)
        {
            var anim = GetActiveModel(handle);

            foreach (var mesh in anim.AdditionalMeshes)
            {
                anim.model.RemoveMesh(mesh);
            }

            anim.AdditionalMeshes.Clear();
        }

        public bool GetBoneWorldMatrixByName(AasHandle handle,
            in AasAnimParams animParams,
            ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut)
        {
            var model = GetAnimatedModel(handle);

            GetWorldMatrix(animParams, out _);

            model.GetBoneMatrix(boneName, out var worldMatrixOut3X4);
            worldMatrixOut = worldMatrixOut3X4;

            return true;
        }

        public bool GetBoneWorldMatrixByNameForChild(AasHandle parentHandle,
            AasHandle handle,
            in AasAnimParams animParams,
            ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut)
        {
            // TODO: This function just seems pointless.....
            // If it ever succeeds, it's using the children's bone matrix, ignoring the parent completely

            var model = GetAnimatedModel(handle);

            GetWorldMatrix(animParams, out var worldMatrix);

            if (parentHandle)
            {
                var parentModel = GetAnimatedModel(parentHandle);
                model.GetBoneMatrix(animParams.attachedBoneName, out worldMatrix);
            }

            model.GetBoneMatrix(boneName, out var worldMatrixOut3X4);
            worldMatrixOut = worldMatrixOut3X4;
            return true;
        }

        public void SetTime(AasHandle handle, float time, in AasAnimParams animParams)
        {
            var model = GetAnimatedModel(handle);

            GetWorldMatrixForParticles(in animParams, out var worldMatrix);

            model.SetTime(time, worldMatrix);
        }

        public void ReplaceMaterial(AasHandle handle, MaterialPlaceholderSlot slot, IMdfRenderMaterial material)
        {
            var model = GetAnimatedModel(handle);
            model.SetSpecialMaterial(slot, material);
        }

        private const int MaxAnims = 5000;

        private Func<int, string> getSkeletonFilename_;
        private Func<int, string> getMeshFilename_;
        private IMaterialResolver materialResolver_;

        private Dictionary<string, WeakReference<Skeleton>> skeletonCache_;

        private ActiveModel[] activeModels_ = new ActiveModel[MaxAnims];

        private const float worldScaleX_ = 28.284271f;
        private const float worldScaleY_ = 28.284271f;

        private readonly IFileSystem fileSystems_;

        private ActiveModel GetActiveModel(AasHandle handle)
        {
            if (IsValidHandle(handle))
            {
                return activeModels_[handle];
            }
            else
            {
                throw new AasException($"Invalid animation handle: {handle}");
            }
        }

        private AasHandle FindFreeHandle()
        {
            for (var i = 1; i < MaxAnims; i++)
            {
                if (activeModels_[i] == null)
                {
                    return new AasHandle((uint) i);
                }
            }

            throw new AasException("Maximum number of active animated models has been exceded.");
        }

        const float rotation_offset = 2.3561945f;

        private void GetWorldMatrix(in AasAnimParams animParams, out Matrix3x4 worldMatrix)
        {
            Matrix3x4 scale = Matrix3x4.scaleMatrix(-1.0f, 1.0f, 1.0f);
            Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, animParams.rotation - rotation_offset);
            var rotation = Matrix3x4.rotationMatrix(q);

            float x = worldScaleX_ * (animParams.locX + 0.5f) + animParams.offsetX;
            float y = animParams.offsetZ;
            float z = worldScaleY_ * (animParams.locY + 0.5f) + animParams.offsetY;
            var translation = Matrix3x4.translationMatrix(x, y, z);

            worldMatrix = Matrix3x4.multiplyMatrix3x3_3x4(Matrix3x4.multiplyMatrix3x3(scale, rotation), translation);

            if ((animParams.flags & 2) != 0 && IsValidHandle(animParams.parentAnim))
            {
                var parentModel = GetAnimatedModel(animParams.parentAnim);
                parentModel.GetBoneMatrix(animParams.attachedBoneName, out var mat);

                worldMatrix = Matrix3x4.makeMatrixOrthogonal(ref mat);
            }
        }

        private void GetWorldMatrixForParticles(in AasAnimParams animParams, out Matrix3x4 worldMatrix)
        {
            Matrix3x4 scale = Matrix3x4.scaleMatrix(-1.0f, 1.0f, 1.0f);
            var rotationYaw =
                Matrix3x4.rotationMatrix(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, animParams.rotationYaw));
            var rotationPitch =
                Matrix3x4.rotationMatrix(Quaternion.CreateFromAxisAngle(Vector3.UnitX, animParams.rotationPitch));
            var rotationRoll =
                Matrix3x4.rotationMatrix(Quaternion.CreateFromAxisAngle(Vector3.UnitY, animParams.rotationRoll));

            float x = worldScaleX_ * (animParams.locX + 0.5f) + animParams.offsetX;
            float y = animParams.offsetZ;
            float z = worldScaleY_ * (animParams.locY + 0.5f) + animParams.offsetY;
            var translation = Matrix3x4.translationMatrix(x, y, z);

            worldMatrix = Matrix3x4.multiplyMatrix3x3_3x4(
                Matrix3x4.multiplyMatrix3x3(
                    Matrix3x4.multiplyMatrix3x3(Matrix3x4.multiplyMatrix3x3(scale, rotationRoll), rotationPitch),
                    rotationYaw), translation);

            if ((animParams.flags & 2) != 0 && IsValidHandle(animParams.parentAnim))
            {
                var parentModel = GetAnimatedModel(animParams.parentAnim);
                parentModel.GetBoneMatrix(animParams.attachedBoneName, out var mat);

                worldMatrix = Matrix3x4.makeMatrixOrthogonal(ref mat);
            }
        }

        public void Dispose()
        {
            ReleaseAllModels();
        }

        public void SetAnimEventHandler(AasHandle handle, Action<AasEvent> evt)
        {
            activeModels_[handle].model.EventHandler = new EventHandler(evt);
        }
    }
}