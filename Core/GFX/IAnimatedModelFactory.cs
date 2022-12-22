using System;
using System.Numerics;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX.RenderMaterials;

namespace OpenTemple.Core.GFX;

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
    public int X;
    public int Y;
    public float OffsetX;
    public float OffsetY;
    public float OffsetZ;
    public float Rotation;
    public float Scale;
    public float RotationRoll;
    public float RotationPitch;
    public float RotationYaw;
    public IAnimatedModel ParentAnim;
    public string AttachedBoneName;
    public bool Rotation3d; // Enables use of rotationRoll/rotationPitch/rotationYaw

    public static AnimatedModelParams Default =>
        new()
        {
            Scale = 1.0f
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
        in AnimatedModelParams animParams);

    IAnimatedModel FromFilenames(
        string meshFilename,
        string skeletonFilename,
        EncodedAnimId idleAnimId,
        in AnimatedModelParams animParams);
}

public interface IAnimatedModel
{
    bool AddAddMesh(string filename);

    void ClearAddMeshes();

    AnimatedModelEvents Advance(float deltaTimeInSecs,
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

    bool SetAnimId(EncodedAnimId animId);

    // This seems to reset cloth simulation state
    void SetClothFlag();

    IMdfRenderMaterial?[] GetSubmeshes();

    ISubmesh GetSubmesh(in AnimatedModelParams animParams, int submeshIdx);

    ISubmesh GetSubmeshForParticles(in AnimatedModelParams animParams, int submeshIdx);

    bool HitTestRay(in AnimatedModelParams animParams, in Ray3d ray, out float hitDistance);

    /// <summary>
    /// Find the closest distance that the given point is away from the surface of this mesh.
    /// </summary>
    float GetDistanceToMesh(in AnimatedModelParams animParams, Vector3 pos);

    /// <summary>
    /// This calculates the effective height in world coordinate units of the model in its current
    /// state. Scale is the model scale in percent.
    /// </summary>
    float GetHeight(int scale = 100);

    /// <summary>
    /// This calculates the visible radius of the model in its current state.
    /// The radius is the maximum distance of any vertex on the x,z plane from the models origin.
    /// If the model has no vertices, 0 is returned.
    /// Scale is model scale in percent.
    /// </summary>
    float GetRadius(int scale = 100);

    /// <summary>
    /// Sets a custom render state pointer that will be freed when this model is freed.
    /// </summary>
    IRenderState? RenderState { get; set; }

    event Action<AasEvent> OnAnimEvent;
}