using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.AAS;

internal class AnimatedModelAdapter : IAnimatedModel
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private const float WorldScaleX = locXY.INCH_PER_TILE;
    private const float WorldScaleY = locXY.INCH_PER_TILE;
    private const float RotationOffset = 2.3561945f;

    private readonly IMaterialResolver _materialResolver;
    private readonly Func<string, Mesh> _meshLoader;
    private readonly AnimatedModel _model;
    private readonly Mesh _mesh;
    private readonly Skeleton _skeleton;
    private readonly List<Mesh> _additionalMeshes = new();
    private EncodedAnimId _animId = new(WeaponAnim.None);
    private TimePoint timeLoaded = TimePoint.Now;

    public AnimatedModelAdapter(
        IMaterialResolver materialResolver,
        Func<string, Mesh> meshLoader,
        Mesh mesh,
        Skeleton skeleton,
        EncodedAnimId idleAnimId,
        in AnimatedModelParams animParams
    )
    {
        _materialResolver = materialResolver;
        _meshLoader = meshLoader;

        _mesh = mesh;
        _skeleton = skeleton;

        _model = new AnimatedModel(_skeleton);
        _model.AddMesh(_mesh, _materialResolver);

        SetAnimId(idleAnimId);
        Advance(0, 0, 0, in animParams);
    }

    public bool AddAddMesh(string filename)
    {
        // Should actually check if it's already loaded
        var mesh = _meshLoader(filename);
        _additionalMeshes.Add(mesh);
        _model.AddMesh(mesh, _materialResolver);
        return true;
    }

    public void ClearAddMeshes()
    {
        foreach (var mesh in _additionalMeshes)
        {
            _model.RemoveMesh(mesh);
        }

        _additionalMeshes.Clear();
    }

    public AnimatedModelEvents Advance(float deltaTimeInSecs, float deltaDistance, float deltaRotation,
        in AnimatedModelParams animParams)
    {
        var aasParams = Convert(in animParams);

        GetWorldMatrix(in aasParams, out var worldMatrix);

        var events = new AnimEvents();

        _model.EventHandler.SetFlagsOut(events);
        _model.SetScale(animParams.Scale);
        _model.Advance(
            worldMatrix,
            deltaTimeInSecs,
            deltaDistance,
            deltaRotation);
        _model.EventHandler.ClearFlagsOut();

        return new AnimatedModelEvents(events.end, events.action);
    }

    public EncodedAnimId GetAnimId()
    {
        return _animId;
    }

    public int GetBoneCount() => _model.GetBoneCount();

    public string GetBoneName(int boneId) => _model.GetBoneName(boneId);

    public int GetBoneParentId(int boneId) => _model.GetBoneParentId(boneId);

    public bool GetBoneWorldMatrixByName(in AnimatedModelParams animParams, ReadOnlySpan<char> boneName,
        out Matrix4x4 worldMatrixOut)
    {
        var aasParams = Convert(animParams);

        GetWorldMatrix(in aasParams, out _);

        _model.GetBoneMatrix(boneName, out var worldMatrixOut3X4);
        worldMatrixOut = worldMatrixOut3X4;

        return true;
    }

    public bool GetBoneWorldMatrixByNameForChild(IAnimatedModel child,
        in AnimatedModelParams animParams,
        ReadOnlySpan<char> boneName,
        out Matrix4x4 worldMatrixOut)
    {
        var realChild = (AnimatedModelAdapter) child;
        var aasParams = Convert(animParams);

        // TODO: This function just seems pointless.....
        // If it ever succeeds, it's using the children's bone matrix, ignoring the parent completely

        var model = realChild._model;

        GetWorldMatrix(in aasParams, out var worldMatrix);

        if (_model != null)
        {
            var parentModel = _model;
            model.GetBoneMatrix(aasParams.attachedBoneName, out worldMatrix);
        }

        model.GetBoneMatrix(boneName, out var worldMatrixOut3X4);
        worldMatrixOut = worldMatrixOut3X4;
        return true;
    }

    private void UpdateWorldMatrix(in AasAnimParams animParams, bool forParticles = false)
    {
        Matrix3x4 worldMatrix;
        if (forParticles)
        {
            GetWorldMatrixForParticles(in animParams, out worldMatrix);
        }
        else
        {
            GetWorldMatrix(in animParams, out worldMatrix);
        }

        _model.SetWorldMatrix(worldMatrix);
        _model.SetScale(animParams.scale);
    }

    private void GetWorldMatrix(in AasAnimParams animParams, out Matrix3x4 worldMatrix)
    {
        Matrix3x4 scale = Matrix3x4.scaleMatrix(-1.0f, 1.0f, 1.0f);
        Quaternion q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, animParams.rotation - RotationOffset);
        var rotation = Matrix3x4.rotationMatrix(q);

        float x = WorldScaleX * (animParams.locX + 0.5f) + animParams.offsetX;
        float y = animParams.offsetZ;
        float z = WorldScaleY * (animParams.locY + 0.5f) + animParams.offsetY;
        var translation = Matrix3x4.translationMatrix(x, y, z);

        worldMatrix = Matrix3x4.multiplyMatrix3x3_3x4(Matrix3x4.multiplyMatrix3x3(scale, rotation), translation);

        if ((animParams.flags & 2) != 0 && animParams.parentAnim != null)
        {
            var parentModel = (AnimatedModel) animParams.parentAnim;
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

        float x = WorldScaleX * (animParams.locX + 0.5f) + animParams.offsetX;
        float y = animParams.offsetZ;
        float z = WorldScaleY * (animParams.locY + 0.5f) + animParams.offsetY;
        var translation = Matrix3x4.translationMatrix(x, y, z);

        worldMatrix = Matrix3x4.multiplyMatrix3x3_3x4(
            Matrix3x4.multiplyMatrix3x3(
                Matrix3x4.multiplyMatrix3x3(Matrix3x4.multiplyMatrix3x3(scale, rotationRoll), rotationPitch),
                rotationYaw), translation);

        if ((animParams.flags & 2) != 0 && animParams.parentAnim != null)
        {
            animParams.parentAnim.GetBoneMatrix(animParams.attachedBoneName, out var mat);
            worldMatrix = Matrix3x4.makeMatrixOrthogonal(ref mat);
        }
    }

    public float GetDistPerSec() => _model.GetDistPerSec();

    public float GetRotationPerSec() => _model.GetRotationPerSec();

    [TempleDllLocation(0x10262690)]
    public bool HasAnim(EncodedAnimId animId)
    {
        int fallbackState = 0;
        while (true)
        {
            if (_model.HasAnimation(animId.GetName()))
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
        Logger.Error("            : Anim File: '{0}', Mesh File: '{1}'", _skeleton.Path, _mesh.Path);
        return false;
    }

    public void SetTime(in AnimatedModelParams animParams, float timeInSecs)
    {
        var aasParams = Convert(animParams);
        GetWorldMatrixForParticles(in aasParams, out var worldMatrix);
        _model.SetTime(timeInSecs, worldMatrix);
    }

    public bool HasBone(ReadOnlySpan<char> boneName) => _model.HasBone(boneName);

    public void AddReplacementMaterial(MaterialPlaceholderSlot slot, IMdfRenderMaterial material)
    {
        _model.SetSpecialMaterial(slot, material);
    }

    [TempleDllLocation(0x10262540)]
    public bool SetAnimId(EncodedAnimId animId)
    {
        this._animId = animId;

        int fallbackState = 0;
        while (true)
        {
            if (_model.SetAnimByName(animId.GetName()))
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
        Logger.Error("            : Anim File: '{0}', Mesh File: '{1}'", _skeleton.Path, _mesh.Path);
        return false;
    }

    public void SetClothFlag()
    {
        _model.SetClothFlagSth();
    }

    public IMdfRenderMaterial?[] GetSubmeshes()
    {
        var materials = _model.GetSubmeshes();
        var result = new IMdfRenderMaterial[materials.Count];
        for (var i = 0; i < materials.Count; i++)
        {
            var material = materials[i].Material;
            if (material != null)
            {
                result[i] = ((ResourceRef<IMdfRenderMaterial>) material).Resource;
            }
        }

        return result;
    }

    public ISubmesh GetSubmesh(in AnimatedModelParams animParams, int submeshIdx)
    {
        var aasParams = Convert(animParams);
        UpdateWorldMatrix(in aasParams);
        return _model.GetSubmesh(submeshIdx);
    }

    public ISubmesh GetSubmeshForParticles(in AnimatedModelParams animParams, int submeshIdx)
    {
        var aasParams = Convert(animParams);
        UpdateWorldMatrix(in aasParams, true);
        return _model.GetSubmesh(submeshIdx);
    }

    public bool HitTestRay(in AnimatedModelParams animParams, in Ray3d ray, out float hitDistance)
    {
        var origin = ray.origin;
        var direction = Vector3.Normalize(ray.direction);

        var submeshes = GetSubmeshes();

        var hit = false;
        hitDistance = float.MaxValue;

        for (var i = 0; i < submeshes.Length; i++)
        {
            var submesh = GetSubmesh(animParams, i);
            var positions = submesh.Positions;
            var indices = submesh.Indices;

            for (int j = 0; j < submesh.PrimitiveCount; j++)
            {
                var v0 = positions[indices[j * 3]].ToVector3();
                var v1 = positions[indices[j * 3 + 1]].ToVector3();
                var v2 = positions[indices[j * 3 + 2]].ToVector3();

                if (TriangleTests.Intersects(origin, direction, v0, v1, v2, out var dist) && dist < hitDistance)
                {
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

    private static bool IsInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Barycentric(p, a, b, c, out var u, out var v, out var w);
        return u >= 0 && u <= 1
                      && v >= 0 && v <= 1
                      && w >= 0 && w <= 1;
    }

    // Returns the distance of "p" from the line p1.p2
    private static float DistanceFromLine(Vector3 p1, Vector3 p2, Vector3 p)
    {
        // Project the point P onto the line going through V0V1
        var edge1 = p2 - p1;
        var edge1Len = edge1.Length();
        var edge1Norm = edge1 / edge1Len;
        var projFactor = Vector3.Dot(p - p1, edge1Norm);
        if (projFactor >= 0 && projFactor < edge1Len)
        {
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

        for (var i = 0; i < submeshes.Length; i++)
        {
            var submesh = GetSubmesh(animParams, i);
            var positions = submesh.Positions;
            var indices = submesh.Indices;

            // Get the closest distance to any of the vertices
            foreach (ref readonly var vertexPos in positions)
            {
                var vertexDist = (vertexPos.ToVector3() - p).Length();
                if (vertexDist < closestDist)
                {
                    closestDist = vertexDist;
                }
            }

            for (var j = 0; j < submesh.PrimitiveCount; j++)
            {
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
                if (IsInTriangle(projectedPos, v0, v1, v2))
                {
                    if (MathF.Abs(distFromPlane) < closestDist)
                    {
                        closestDist = MathF.Abs(distFromPlane);
                    }
                }
                else
                {
                    // Project the point P onto the line going through V0V1
                    float edge1Dist = DistanceFromLine(v0, v1, p);
                    if (edge1Dist < closestDist)
                    {
                        closestDist = edge1Dist;
                    }

                    float edge2Dist = DistanceFromLine(v0, v2, p);
                    if (edge2Dist < closestDist)
                    {
                        closestDist = edge2Dist;
                    }

                    float edge3Dist = DistanceFromLine(v2, v1, p);
                    if (edge3Dist < closestDist)
                    {
                        closestDist = edge3Dist;
                    }
                }
            }
        }

        return closestDist;
    }

    public float GetHeight(int scale = 100)
    {
        _model.SetScale(scale / 100.0f);
        return _model.GetHeight();
    }

    public float GetRadius(int scale = 100)
    {
        var animParams = AnimatedModelParams.Default;
        animParams.Scale = scale / 100.0f;
        var aasParams = Convert(animParams);
        UpdateWorldMatrix(in aasParams);
        _model.Method19();

        return _model.GetRadius();
    }

    public IRenderState? RenderState
    {
        get => _model.RenderState;
        set => _model.RenderState = value;
    }

    public event Action<AasEvent> OnAnimEvent
    {
        add => _model.EventHandler = new EventHandler(value);
        remove => throw new NotSupportedException();
    }

    internal static AasAnimParams Convert(in AnimatedModelParams animParams)
    {
        var result = new AasAnimParams();
        result.flags = 0;
        result.locX = animParams.X;
        result.locY = animParams.Y;
        result.scale = animParams.Scale;
        result.offsetX = animParams.OffsetX;
        result.offsetY = animParams.OffsetY;
        result.offsetZ = animParams.OffsetZ;
        result.rotation = animParams.Rotation;
        result.rotationYaw = animParams.RotationYaw;
        result.rotationPitch = animParams.RotationPitch;
        result.rotationRoll = animParams.RotationRoll;
        result.attachedBoneName = animParams.AttachedBoneName;
        result.unknown = 0;
        if (animParams.ParentAnim is AnimatedModelAdapter parentAdapter)
        {
            result.parentAnim = parentAdapter._model;
            result.flags = 2;
        }
        else
        {
            result.parentAnim = null;
            result.flags = 1;
        }

        return result;
    }
}