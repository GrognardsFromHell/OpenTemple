using System;
using System.Numerics;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX.RenderMaterials;
using EventHandler = SpicyTemple.Core.AAS.EventHandler;

namespace SpicyTemple.Core.GFX
{
    /*
        Represents the events that can trigger when the animation
        of an animated model is advanced.
    */
    public readonly struct AnimatedModelEvents
    {
        public bool IsEnd { get; }
        public bool IsAction { get; }

        public AnimatedModelEvents(bool isEnd, bool isAction)
        {
            IsEnd = isEnd;
            IsAction = isAction;
        }
    }

    public struct AnimatedModelParams
    {
        // see: objects.GetAnimParams(handle)
        public int x;
        public int y;
        public float offsetX;
        public float offsetY;
        public float offsetZ;
        public float rotation;
        public float scale;
        public float rotationRoll;
        public float rotationPitch;
        public float rotationYaw;
        public IAnimatedModel parentAnim;
        public string attachedBoneName;
        public bool rotation3d; // Enables use of rotationRoll/rotationPitch/rotationYaw

        public static AnimatedModelParams Default =>
            new AnimatedModelParams
            {
                scale = 1.0f
            };
    }

    public interface IRenderState : IDisposable
    {
    }

    public interface IAnimatedModelFactory
    {
        IAnimatedModel FromIds(
            int meshId,
            int skeletonId,
            EncodedAnimId idleAnimId,
            in AnimatedModelParams animParams,
            bool borrow = false);

        IAnimatedModel FromFilenames(
            string meshFilename,
            string skeletonFilename,
            EncodedAnimId idleAnimId,
            in AnimatedModelParams animParams);

        IAnimatedModel BorrowByHandle(uint handle);

        void FreeHandle(uint handle);

        void FreeAll();
    }

    public interface IAnimatedModel
    {
        uint GetHandle();

        bool AddAddMesh(string filename);

        bool ClearAddMeshes();

        AnimatedModelEvents Advance(float deltaTime,
            float deltaDistance,
            float deltaRotation,
            in AnimatedModelParams animParams);

        EncodedAnimId GetAnimId();

        int GetBoneCount();

        string GetBoneName(int boneId);

        int GetBoneParentId(int boneId);

        bool GetBoneWorldMatrixByName(
            in AnimatedModelParams animParams,
            ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut);

        bool GetBoneWorldMatrixByNameForChild(IAnimatedModel child,
            in AnimatedModelParams animParams,
            ReadOnlySpan<char> boneName,
            out Matrix4x4 worldMatrixOut);


        float GetDistPerSec();

        float GetRotationPerSec();

        bool HasAnim(EncodedAnimId animId);

        void SetTime(in AnimatedModelParams animParams, float timeInSecs);

        [TempleDllLocation(0x10263a10)]
        bool HasBone(ReadOnlySpan<char> boneName);

        void AddReplacementMaterial(MaterialPlaceholderSlot slot, IMdfRenderMaterial material);

        void SetAnimId(EncodedAnimId animId);

        // This seems to reset cloth simulation state
        void SetClothFlag();

        IMdfRenderMaterial[] GetSubmeshes();

        ISubmesh GetSubmesh(in AnimatedModelParams animParams, int submeshIdx);

        ISubmesh GetSubmeshForParticles(in AnimatedModelParams animParams, int submeshIdx);

        bool HitTestRay(in AnimatedModelParams animParams, in Ray3d ray, out float hitDistance);

        /**
         * Find the closest distance that the given point is away from the surface of this mesh.
         */
        float GetDistanceToMesh(in AnimatedModelParams animParams, Vector3 pos);

        /**
            This calculates the effective height in world coordinate units of the model in its current
            state. Scale is the model scale in percent.
        */
        float GetHeight(int scale = 100);

        /**
            This calculates the visible radius of the model in its current state.
            The radius is the maximum distance of any vertex on the x,z plane from the models origin.
            If the model has no vertices, 0 is returned.
            Scale is model scale in percent.
        */
        float GetRadius(int scale = 100);

        /**
         * Sets a custom render state pointer that will be freed when this model is freed.
         */
        IRenderState RenderState { get; set; }

        event Action<AasEvent> OnAnimEvent;
    }
}