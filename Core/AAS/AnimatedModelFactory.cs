using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;

namespace SpicyTemple.Core.AAS
{
    public class AnimatedModelFactory : IAnimatedModelFactory, IDisposable
    {
        private readonly IDictionary<int, string> meshTable_;

        public AnimatedModelFactory(
            IFileSystem fileSystem,
            IDictionary<int, string> meshTable,
            ScriptInterpreter runScript,
            Func<string, int> resolveMaterial)
        {
            meshTable_ = meshTable;

            var materialResolver = new MaterialResolver(resolveMaterial);

            aasSystem_ = new AasSystem(
                fileSystem,
                GetSkeletonFilename,
                GetMeshFilename,
                runScript,
                materialResolver
            );
        }

        private string GetMeshFilename(int meshId)
        {
            return "art/meshes/" + meshTable_[meshId] + ".skm";
        }

        private string GetSkeletonFilename(int meshId)
        {
            return "art/meshes/" + meshTable_[meshId] + ".ska";
        }

        public IAnimatedModel FromIds(int meshId, int skeletonId, EncodedAnimId idleAnimId,
            in AnimatedModelParams animParams,
            bool borrow = false)
        {
            var aasParams = AnimatedModelAdapter.Convert(animParams);

            var handle = aasSystem_.CreateModelFromIds(meshId, skeletonId, idleAnimId, aasParams);

            return new AnimatedModelAdapter(aasSystem_, handle, aasSystem_.GetAnimatedModel(handle), borrow);
        }

        public IAnimatedModel FromFilenames(string meshFilename, string skeletonFilename,
            EncodedAnimId idleAnimId,
            in AnimatedModelParams animParams)
        {
            var aasParams = AnimatedModelAdapter.Convert(animParams);

            var handle = aasSystem_.CreateModel(meshFilename, skeletonFilename, idleAnimId, aasParams);

            return new AnimatedModelAdapter(aasSystem_, handle, aasSystem_.GetAnimatedModel(handle));
        }

        /*
            Gets an existing animated model by its handle.
            The returned model will not free the actual animation when it is
            destroyed.
        */
        public IAnimatedModel BorrowByHandle(uint handle)
        {
            var aasHandle = new AasHandle(handle);
            var model = aasSystem_.GetAnimatedModel(aasHandle);
            return new AnimatedModelAdapter(aasSystem_, aasHandle, model, true);
        }

        public void FreeHandle(uint handle)
        {
            aasSystem_.ReleaseModel(new AasHandle(handle));
        }

        public void FreeAll()
        {
            aasSystem_.ReleaseAllModels();
        }

        // This is the mapping loaded from meshes.mes
        private readonly Dictionary<int, string> fileIdMapping_;

        private readonly AasSystem aasSystem_;

        public void Dispose()
        {
            aasSystem_.Dispose();
        }
    }

    internal class AnimatedModelAdapter : IAnimatedModel
    {
        private readonly AasSystem aasSystem_;
        private readonly AasHandle handle_;
        private readonly AnimatedModel model_;
        private readonly bool borrowed_;

        public AnimatedModelAdapter(AasSystem aasSystem, AasHandle handle, AnimatedModel model, bool borrowed = false)
        {
            aasSystem_ = aasSystem;
            handle_ = handle;
            model_ = model;
            borrowed_ = borrowed;
        }

        public uint GetHandle() => handle_.Handle;

        public bool AddAddMesh(string filename)
        {
            aasSystem_.AddAdditionalMesh(handle_, filename);
            return true;
        }

        public bool ClearAddMeshes()
        {
            aasSystem_.ClearAddmeshes(handle_);
            return true;
        }

        public AnimatedModelEvents Advance(float deltaTime, float deltaDistance, float deltaRotation,
            in AnimatedModelParams animParams)
        {
            var aasParams = Convert(in animParams);

            var events = aasSystem_.Advance(handle_, deltaTime, deltaDistance, deltaRotation, aasParams);

            return new AnimatedModelEvents(events.end, events.action);
        }

        public EncodedAnimId GetAnimId() => aasSystem_.GetAnimId(handle_);

        public int GetBoneCount() => model_.GetBoneCount();

        public string GetBoneName(int boneId) => model_.GetBoneName(boneId);

        public int GetBoneParentId(int boneId) => model_.GetBoneParentId(boneId);

        public bool GetBoneWorldMatrixByName(in AnimatedModelParams animParams, ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut)
        {
            var aasParams = Convert(animParams);
            return aasSystem_.GetBoneWorldMatrixByName(handle_, aasParams, boneName, out worldMatrixOut);
        }

        public bool GetBoneWorldMatrixByNameForChild(IAnimatedModel child, in AnimatedModelParams animParams,
            ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut)
        {
            var realChild = (AnimatedModelAdapter) child;
            var aasParams = Convert(animParams);
            return aasSystem_.GetBoneWorldMatrixByNameForChild(handle_, realChild.handle_, aasParams, boneName,
                out worldMatrixOut);
        }

        public float GetDistPerSec() => model_.GetDistPerSec();

        public float GetRotationPerSec() => model_.GetRotationPerSec();

        public bool HasAnim(EncodedAnimId animId) => aasSystem_.HasAnimId(handle_, animId);

        public void SetTime(in AnimatedModelParams animParams, float timeInSecs)
        {
            var aasParams = Convert(animParams);
            aasSystem_.SetTime(handle_, timeInSecs, aasParams);
        }

        public bool HasBone(ReadOnlySpan<char> boneName) => model_.HasBone(boneName);

        public void AddReplacementMaterial(MaterialPlaceholderSlot slot, IMdfRenderMaterial material)
        {
            aasSystem_.ReplaceMaterial(handle_, slot, material);
        }

        public void SetAnimId(EncodedAnimId animId)
        {
            aasSystem_.SetAnimById(handle_, animId);
        }

        public void SetClothFlag()
        {
            model_.SetClothFlagSth();
        }

        public IMdfRenderMaterial[] GetSubmeshes()
        {
            var materials = model_.GetSubmeshes();
            IMdfRenderMaterial[] result = new IMdfRenderMaterial[materials.Count];
            for (var i = 0; i < materials.Count; i++)
            {
                result[i] = (IMdfRenderMaterial) materials[i].Material;
            }

            return result;
        }

        public ISubmesh GetSubmesh(in AnimatedModelParams animParams, int submeshIdx)
        {
            var aasParams = Convert(animParams);
            aasSystem_.UpdateWorldMatrix(handle_, aasParams);
            return model_.GetSubmesh(submeshIdx);
        }

        public ISubmesh GetSubmeshForParticles(in AnimatedModelParams animParams, int submeshIdx)
        {
            var aasParams = Convert(animParams);
            aasSystem_.UpdateWorldMatrix(handle_, aasParams, true);
            return model_.GetSubmesh(submeshIdx);
        }

        public bool HitTestRay(in AnimatedModelParams animParams, in Ray3d ray, out float hitDistance)
        {
            throw new NotImplementedException();
        }

        public float GetDistanceToMesh(in AnimatedModelParams animParams, Vector3 pos)
        {
            throw new NotImplementedException();
        }

        public float GetHeight(int scale = 100)
        {
            model_.SetScale(scale / 100.0f);
            return model_.GetHeight();
        }

        public float GetRadius(int scale = 100)
        {
            model_.SetScale(scale / 100.0f);
            return model_.GetRadius();
        }

        public IRenderState RenderState
        {
            get => model_.RenderState;
            set => model_.RenderState = value;
        }

        internal static AasAnimParams Convert(in AnimatedModelParams animParams)
        {
            var result = new AasAnimParams();
            result.flags = 0;
            result.locX = animParams.x;
            result.locY = animParams.y;
            result.scale = animParams.scale;
            result.offsetX = animParams.offsetX;
            result.offsetY = animParams.offsetY;
            result.offsetZ = animParams.offsetZ;
            result.rotation = animParams.rotation;
            result.rotationYaw = animParams.rotationYaw;
            result.rotationPitch = animParams.rotationPitch;
            result.rotationRoll = animParams.rotationRoll;
            result.attachedBoneName = animParams.attachedBoneName;
            result.unknown = 0;
            if (animParams.parentAnim is AnimatedModelAdapter parentAdapter)
            {
                result.parentAnim = parentAdapter.handle_;
                result.flags = 2;
            }
            else
            {
                result.parentAnim = AasHandle.Null;
                result.flags = 1;
            }

            return result;
        }
    }
}