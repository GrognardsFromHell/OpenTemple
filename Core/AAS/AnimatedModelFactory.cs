using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.AAS
{
    public class AnimatedModelFactory : IAnimatedModelFactory, IDisposable
    {
        private readonly IDictionary<int, string> meshTable_;

        public AnimatedModelFactory(
            IFileSystem fileSystem,
            IDictionary<int, string> meshTable,
            ScriptInterpreter runScript,
            Func<string, object> resolveMaterial)
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
                if (materials[i].Material != null)
                {
                    result[i] = ((ResourceRef<IMdfRenderMaterial>) materials[i].Material).Resource;
                }
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

            var origin = ray.origin;
            var direction = Vector3.Normalize(ray.direction);

            var submeshes = GetSubmeshes();

            var hit = false;
            hitDistance = float.MaxValue;

            for (var i = 0; i < submeshes.Length; i++) {
                var submesh = GetSubmesh(animParams, i);
                var positions = submesh.Positions;
                var indices = submesh.Indices;

                for (int j = 0; j < submesh.PrimitiveCount; j++) {

                    var v0 = positions[indices[j * 3]].ToVector3();
                    var v1 = positions[indices[j * 3 + 1]].ToVector3();
                    var v2 = positions[indices[j * 3 + 2]].ToVector3();

                    if (TriangleTests.Intersects(origin, direction, v0, v1, v2, out var dist) && dist < hitDistance) {
                        hitDistance = dist;
                        hit = true;
                    }
                }
            }


            return hit;

        }

        // Compute barycentric coordinates (u, v, w) for
        // point p with respect to triangle (a, b, c)
        private static void Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float u, out float v, out float w)
        {
            var v0 = b - a;
            var v1 = c - a;
            var v2 = p - a;
            var d00 = Vector3.Dot(v0, v0);
            var d01 = Vector3.Dot(v0, v1);
            var d11 = Vector3.Dot(v1, v1);
            var d20 = Vector3.Dot(v2, v0);
            var d21 = Vector3.Dot(v2, v1);
            var denom = d00 * d11 - d01 * d01;
            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0f - v - w;
        }

        private static bool IsInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c) {
            Barycentric(p, a, b, c, out var u, out var v, out var w);
            return u >= 0 && u <= 1
                          && v >= 0 && v <= 1
                          && w >= 0 && w <= 1;
        }

        // Returns the distance of "p" from the line p1.p2
        private static float DistanceFromLine(Vector3 p1, Vector3 p2, Vector3 p) {
            // Project the point P onto the line going through V0V1
            var edge1 = p2 - p1;
            var edge1Len = edge1.Length();
            var edge1Norm = edge1 / edge1Len;
            var projFactor = Vector3.Dot(p - p1, edge1Norm);
            if (projFactor >= 0 && projFactor < edge1Len) {
                // If projFactor < 0 or > the length of V0V1, it's outside the line
                var pp = p1 + projFactor * edge1Norm;
                return (pp - p).Length();
            }
            else
            {
                return float.MaxValue;
            }
        }

        [TempleDllLocation(0x1001E220)]
        public float GetDistanceToMesh(in AnimatedModelParams animParams, Vector3 pos)
        {
            float closestDist = float.MaxValue;

            var p = pos;

            var submeshes = GetSubmeshes();

            for (var i = 0; i < submeshes.Length; i++) {
                var submesh = GetSubmesh(animParams, i);
                var positions = submesh.Positions;
                var indices = submesh.Indices;

                // Get the closest distance to any of the vertices
                foreach (ref readonly var vertexPos in positions) {
                    var vertexDist = (vertexPos.ToVector3() - p).Length();
                    if (vertexDist < closestDist) {
                        closestDist = vertexDist;
                    }
                }

                for (var j = 0; j < submesh.PrimitiveCount; j++) {
                    var v0 = positions[indices[j * 3]].ToVector3();
                    var v1 = positions[indices[j * 3 + 1]].ToVector3();
                    var v2 = positions[indices[j * 3 + 2]].ToVector3();

                    // Compute the surface normal
                    var n = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

                    // Project the point into the plane of the triangle
                    var distFromPlane = Vector3.Dot(p - v0, n);
                    var projectedPos = p - distFromPlane * n;

                    // If the point is within the triangle when projected onto it using the
                    // plane's normal, then use the distance from the plane as the distance
                    if (IsInTriangle(projectedPos, v0, v1, v2)) {
                        if (MathF.Abs(distFromPlane) < closestDist) {
                            closestDist = MathF.Abs(distFromPlane);
                        }
                    } else {

                        // Project the point P onto the line going through V0V1
                        float edge1Dist = DistanceFromLine(v0, v1, p);
                        if (edge1Dist < closestDist) {
                            closestDist = edge1Dist;
                        }

                        float edge2Dist = DistanceFromLine(v0, v2, p);
                        if (edge2Dist < closestDist) {
                            closestDist = edge2Dist;
                        }

                        float edge3Dist = DistanceFromLine(v2, v1, p);
                        if (edge3Dist < closestDist) {
                            closestDist = edge3Dist;
                        }
                    }

                }

            }

            return closestDist;
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