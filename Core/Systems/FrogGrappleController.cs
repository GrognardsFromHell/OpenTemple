using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems;

public class GrappleState
{
    public ushort State { get; set; }
    public float CurrentLength { get; set; }
    public float TargetLength { get; set; }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct TongueVertex
{
    public Vector4 pos;
    public Vector4 normal;
    public Vector2 uv;

    public static readonly int Size = Marshal.SizeOf<TongueVertex>();
}

/// <summary>
/// Controls the tongue grapple performed by giant frogs.
/// </summary>
internal class FrogGrappleController
{
    public FrogGrappleController(RenderingDevice device,
        MdfMaterialFactory mdfFactory)
    {
        _device = device;
        _bufferBinding = device.CreateMdfBufferBinding();

        _tongueMaterial = mdfFactory.LoadMaterial("art/meshes/Monsters/GiantFrog/tongue.mdf");

        _vertexBuffer = device.CreateEmptyVertexBuffer(TongueVertex.Size * VertexCount, debugName:"FrogTongue");

        CreateIndexBuffer();

        _bufferBinding.AddBuffer(_vertexBuffer, 0, TongueVertex.Size)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
            .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
            .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
    }

    // TODO: Separate logic+rendering
    public void AdvanceAndRender(
        IGameViewport viewport,
        GameObject giantFrog,
        AnimatedModelParams animParams,
        IAnimatedModel model,
        IReadOnlyList<Light3d> lights,
        float alpha)
    {
        var grappleState = giantFrog.GrappleState;

        if (grappleState == null)
        {
            return;
        }

        model.GetBoneWorldMatrixByName(animParams, "Tongue_Ref", out var worldMatrixFrog);

        var grappledOpponent = GetGrappledOpponent(giantFrog);
        float tongueLength, tonguePosX, tonguePosZ;
        if (grappledOpponent != null)
        {
            var opponentModel = grappledOpponent.GetOrCreateAnimHandle();
            var opponentAnimParams = grappledOpponent.GetAnimParams();
            opponentModel.GetBoneWorldMatrixByName(opponentAnimParams, "Bip01 Spine1", out var worldMatrixOpponent);

            tonguePosX = worldMatrixOpponent.M41;
            tonguePosZ = worldMatrixOpponent.M43;

            var tongueDirX = worldMatrixOpponent.M41 - worldMatrixFrog.M41;
            var tongueDirY = worldMatrixOpponent.M42 - worldMatrixFrog.M42;
            var tongueDirZ = worldMatrixOpponent.M43 - worldMatrixFrog.M43;
            tongueLength = MathF.Sqrt(tongueDirX * tongueDirX + tongueDirY * tongueDirY + tongueDirZ * tongueDirZ);
            worldMatrixFrog.M31 = tongueDirX / tongueLength;
            worldMatrixFrog.M32 = tongueDirY / tongueLength;
            worldMatrixFrog.M33 = tongueDirZ / tongueLength;
            if (tongueLength > 0)
            {
                tongueLength -= 6.0f;
            }
        }
        else
        {
            tongueLength = 120.0f;
            tonguePosX = worldMatrixFrog.M31 * 120.0f + worldMatrixFrog.M41;
            tonguePosZ = worldMatrixFrog.M33 * 120.0f + worldMatrixFrog.M43;
        }

        switch (grappleState.State)
        {
            // This state seems to mean . Extending tongue to full length
            case 1:
                grappleState.CurrentLength += locXY.INCH_PER_TILE;
                if (grappleState.CurrentLength > tongueLength)
                {
                    grappleState.State = 2;
                    grappleState.CurrentLength = tongueLength;
                }

                break;
            // This state seems to mean . Retracting tongue
            case 2:
                grappleState.CurrentLength -= locXY.INCH_PER_TILE;
                if (grappleState.CurrentLength <= 0)
                {
                    grappleState.State = 0;
                    grappleState.CurrentLength = 0;
                }

                break;
            case 3:
                grappleState.CurrentLength += locXY.INCH_PER_TILE;
                if (grappleState.CurrentLength > tongueLength)
                {
                    grappleState.State = 4;
                    grappleState.CurrentLength = tongueLength;
                    var frogAnim = GameSystems.Critter.GetAnimId(giantFrog,
                        WeaponAnim.Special2);
                    giantFrog.SetAnimId(frogAnim);

                    var opponentAnim = GameSystems.Critter.GetAnimId(grappledOpponent,
                        WeaponAnim.Panic);
                    grappledOpponent.SetAnimId(opponentAnim);
                }

                break;
            case 4:
                // Maintain Tongue between frog and opponent without progressing
                grappleState.CurrentLength = tongueLength;
                break;
            case 5:
            case 6:
            {
                if (grappleState.State == 5)
                {
                    grappleState.TargetLength = tongueLength - 12.0f;
                    if (grappleState.TargetLength < 0)
                    {
                        grappleState.TargetLength = 0;
                    }

                    grappleState.State = 6;
                }

                grappleState.CurrentLength -= locXY.INCH_PER_HALFTILE;
                // Move the opponent closer to the frog
                float newX = tonguePosX - worldMatrixFrog.M31 * locXY.INCH_PER_HALFTILE;
                float newZ = tonguePosZ - worldMatrixFrog.M33 * locXY.INCH_PER_HALFTILE;
                var newLoc = LocAndOffsets.FromInches(newX, newZ);
                GameSystems.MapObject.Move(grappledOpponent, newLoc);

                if (grappleState.CurrentLength < grappleState.TargetLength)
                {
                    newX = worldMatrixFrog.M41 + grappleState.TargetLength * worldMatrixFrog.M31;
                    newZ = worldMatrixFrog.M43 + grappleState.TargetLength * worldMatrixFrog.M33;
                    newLoc = LocAndOffsets.FromInches(newX, newZ);
                    GameSystems.MapObject.Move(grappledOpponent, newLoc);
                    grappleState.CurrentLength = grappleState.TargetLength;
                    grappleState.State = 4;
                }
            }
                break;
            case 7:
            {
                grappleState.CurrentLength -= locXY.INCH_PER_HALFTILE;
                // Move the opponent closer to the frog
                float newX = tonguePosX - worldMatrixFrog.M31 * locXY.INCH_PER_HALFTILE;
                float newZ = tonguePosZ - worldMatrixFrog.M33 * locXY.INCH_PER_HALFTILE;
                var newLoc = LocAndOffsets.FromInches(newX, newZ);
                GameSystems.MapObject.Move(grappledOpponent, newLoc);

                if (grappleState.CurrentLength < 0)
                {
                    newX = worldMatrixFrog.M41;
                    newZ = worldMatrixFrog.M43;
                    newLoc = LocAndOffsets.FromInches(newX, newZ);
                    GameSystems.MapObject.Move(grappledOpponent, newLoc);
                    GameSystems.ObjFade.FadeTo(grappledOpponent, 0, 0, 16, 0);
                    grappleState.CurrentLength = 0;
                    grappleState.State = 0;

                    // Probably the swallow animation
                    var animId = GameSystems.Critter.GetAnimId(giantFrog, WeaponAnim.Special3);
                    giantFrog.SetAnimId(animId);
                }
            }
                break;
            default:
                break;
        }

        // The directional vector of the tongue ref point on the frog
        var tongueUp = worldMatrixFrog.GetRow(0).ToVector3();
        var tongueRight = worldMatrixFrog.GetRow(1).ToVector3();
        var tongueDir = worldMatrixFrog.GetRow(2).ToVector3();
        var tonguePos = worldMatrixFrog.GetRow(3).ToVector3();

        RenderTongue(viewport, grappleState, tongueDir, tongueUp, tongueRight, tonguePos, lights, alpha);
    }

    public bool IsGiantFrog(GameObject obj)
    {
        // Special rendering for giant frogs of various types
        var protoNum = obj.ProtoId;
        if (protoNum != 14057 & protoNum != 14445 && protoNum != 14300)
        {
            return false;
        }

        return true;
    }

    private const int VertexCount = 96;
    private const int TriCount = 180;

    private RenderingDevice _device;
    private ResourceRef<IMdfRenderMaterial> _tongueMaterial;
    private BufferBinding _bufferBinding;
    private ResourceRef<VertexBuffer> _vertexBuffer;
    private ResourceRef<IndexBuffer> _indexBuffer;

    private void CreateIndexBuffer()
    {
        // 12 tris are needed to fill the space between two discs
        // which comes down to 15 * 12 = 180 tris overall, each needing
        // 3 indices
        Span<ushort> indices = stackalloc ushort[TriCount * 3];
        int i = 0;
        for (var disc = 0; disc < 15; ++disc)
        {
            var discFirst = disc * 6; // Index of first vertex in the current disc
            var nextDiscFirst = (disc + 1) * 6; // Index of first vertex in the next disc

            for (var segment = 0; segment < 5; ++segment)
            {
                indices[i++] = (ushort) (discFirst + segment);
                indices[i++] = (ushort) (discFirst + segment + 1);
                indices[i++] = (ushort) (nextDiscFirst + segment + 1);
                indices[i++] = (ushort) (discFirst + segment);
                indices[i++] = (ushort) (nextDiscFirst + segment + 1);
                indices[i++] = (ushort) (nextDiscFirst + segment);
            }

            // The last segment of this disc wraps back around
            // and connects to the first segment
            indices[i++] = (ushort) (discFirst + 5);
            indices[i++] = (ushort) (discFirst);
            indices[i++] = (ushort) (nextDiscFirst);
            indices[i++] = (ushort) (discFirst + 5);
            indices[i++] = (ushort) (nextDiscFirst);
            indices[i++] = (ushort) (nextDiscFirst + 5);
        }

        _indexBuffer = _device.CreateIndexBuffer(indices);
    }

    private void RenderTongue(
        IGameViewport viewport,
        GrappleState grappleState,
        in Vector3 tongueDir,
        in Vector3 tongueUp,
        in Vector3 tongueRight,
        in Vector3 tonguePos,
        IReadOnlyList<Light3d> lights,
        float alpha)
    {
        Span<TongueVertex> vertices = stackalloc TongueVertex[VertexCount];

        // Generate 16 "discs" along the path of the tongue
        for (var i = 0; i < 16; ++i)
        {
            // This function ranges from 1 at the origin of the tongue to 0 in the middle and
            // 1 at the end. It causes the tongue to appear slightly thinned out in the middle
            float radiusVariance = (MathF.Cos(2 * MathF.PI * i / 15.0f) - 1.0f) * 0.5f;

            // This seems to be the radius of the tongue at this particular given disc
            float tongueRadius = 3.0f + radiusVariance * (grappleState.CurrentLength / 700.0f) * 3.0f;

            // Calculates the center point of the tongue along the directional vector
            // of the tongue_ref
            float distFromFrog = grappleState.CurrentLength * i / 15.0f;
            var posFromOrig = tongueDir * distFromFrog;

            // Each disc has 6 corner points on the
            // circle around the center of the tongue
            for (int j = 0; j < 6; ++j)
            {
                // The angle does a full rotation around the tongue's center
                float angle = Angles.ToRadians(360 / 6) * j;
                var angleCos = MathF.Cos(angle);
                var angleSin = MathF.Sin(angle);

                var scaledSin = tongueRadius * angleSin;
                var scaledCos = tongueRadius * angleCos;

                ref var vertex = ref vertices[i * 6 + j];

                var pos = tonguePos + tongueRight * scaledSin + tongueUp * scaledCos + posFromOrig;
                vertex.pos = new Vector4(pos, 1);

                var normal = tongueRight * angleSin + tongueUp * angleCos;
                vertex.normal = new Vector4(normal, 0);

                // The U texture coord just goes around the disc from 0 to 1
                vertex.uv.X = j / 5.0f;
                // Along the tongue's longer axis, the v coord just ranges from 0
                // on the first disc to 1 on the last
                vertex.uv.Y = i / 15.0f;
            }
        }

        _vertexBuffer.Resource.Update<TongueVertex>(vertices);

        _bufferBinding.Bind();
        MdfRenderOverrides? overrides = new MdfRenderOverrides();
        overrides.Alpha = alpha;
        _tongueMaterial.Resource.Bind(viewport, _device, lights, overrides);
        _device.SetIndexBuffer(_indexBuffer);

        _device.DrawIndexed(
            PrimitiveType.TriangleList,
            VertexCount,
            TriCount * 3);
    }

    public GameObject? GetGrappledOpponent(GameObject giantFrog)
    {
        int spellIdx;
        if (!GameSystems.D20.CritterHasCondition(giantFrog, "sp-Frog Tongue", out spellIdx))
        {
            if (!GameSystems.D20.CritterHasCondition(giantFrog, "sp-Frog Tongue Swallowing", out spellIdx))
            {
                return null; // Nothing attached
            }
        }

        if (!GameSystems.Spell.TryGetActiveSpell(spellIdx, out var spell))
        {
            return null;
        }

        return spell.Targets[0].Object;
    }

    [TempleDllLocation(0x100209a0)]
    public static void PlayFailedLatch(GameObject giantFrog)
    {
        giantFrog.GrappleState = new GrappleState {
            State = 1
        };
    }

    [TempleDllLocation(0x100209c0)]
    public static void PlayLatch(GameObject giantFrog)
    {
        giantFrog.GrappleState = new GrappleState {
            State = 3
        };
    }

    [TempleDllLocation(0x100209e0)]
    public static void PlayPull(GameObject giantFrog)
    {
        // Keep current tongue position as starting point
        GetOrCreateGrappleState(giantFrog).State = 5;
    }

    [TempleDllLocation(0x10020a60)]
    public static void PlayRetractTongue(GameObject giantFrog)
    {
        // Keep current tongue position as starting point
        GetOrCreateGrappleState(giantFrog).State = 2;
    }

    [TempleDllLocation(0x10020980)]
    public static void Reset(GameObject giantFrog)
    {
        giantFrog.GrappleState = null;
    }

    [TempleDllLocation(0x10020a20)]
    public static void PlaySwallow(GameObject giantFrog)
    {
        // Keep current tongue position as starting point
        var grappleState = GetOrCreateGrappleState(giantFrog);
        grappleState.State = 7;
    }

    private static GrappleState GetOrCreateGrappleState(GameObject frog)
    {
        frog.GrappleState ??= new GrappleState();
        return frog.GrappleState;
    }
}
