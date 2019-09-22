using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    internal struct GrappleState
    {
        public ushort state;
        public float currentLength;
        public float targetLength;
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
            mDevice = device;
            mBufferBinding = device.CreateMdfBufferBinding();

            mTongueMaterial = mdfFactory.LoadMaterial("art/meshes/Monsters/GiantFrog/tongue.mdf");

            mVertexBuffer = device.CreateEmptyVertexBuffer(TongueVertex.Size * VertexCount);

            CreateIndexBuffer();

            mBufferBinding.AddBuffer(mVertexBuffer, 0, TongueVertex.Size)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        public void AdvanceAndRender(GameObjectBody giantFrog,
            AnimatedModelParams animParams,
            IAnimatedModel model,
            IList<Light3d> lights,
            float alpha)
        {
            var grappleState = GetGrappleState(giantFrog);

            if (grappleState.state == 0)
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

            switch (grappleState.state)
            {
                // This state seems to mean . Extending tongue to full length
                case 1:
                    grappleState.currentLength += locXY.INCH_PER_TILE;
                    if (grappleState.currentLength > tongueLength)
                    {
                        grappleState.state = 2;
                        grappleState.currentLength = tongueLength;
                    }

                    break;
                // This state seems to mean . Retracting tongue
                case 2:
                    grappleState.currentLength -= locXY.INCH_PER_TILE;
                    if (grappleState.currentLength <= 0)
                    {
                        grappleState.state = 0;
                        grappleState.currentLength = 0;
                    }

                    break;
                case 3:
                    grappleState.currentLength += locXY.INCH_PER_TILE;
                    if (grappleState.currentLength > tongueLength)
                    {
                        grappleState.state = 4;
                        grappleState.currentLength = tongueLength;
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
                    grappleState.currentLength = tongueLength;
                    break;
                case 5:
                case 6:
                {
                    if (grappleState.state == 5)
                    {
                        grappleState.targetLength = tongueLength - 12.0f;
                        if (grappleState.targetLength < 0)
                        {
                            grappleState.targetLength = 0;
                        }

                        grappleState.state = 6;
                    }

                    grappleState.currentLength = grappleState.currentLength - locXY.INCH_PER_HALFTILE;
                    // Move the opponent closer to the frog
                    float newX = tonguePosX - worldMatrixFrog.M31 * locXY.INCH_PER_HALFTILE;
                    float newZ = tonguePosZ - worldMatrixFrog.M33 * locXY.INCH_PER_HALFTILE;
                    var newLoc = LocAndOffsets.FromInches(newX, newZ);
                    GameSystems.MapObject.Move(grappledOpponent, newLoc);

                    if (grappleState.currentLength < grappleState.targetLength)
                    {
                        newX = worldMatrixFrog.M41 + grappleState.targetLength * worldMatrixFrog.M31;
                        newZ = worldMatrixFrog.M43 + grappleState.targetLength * worldMatrixFrog.M33;
                        newLoc = LocAndOffsets.FromInches(newX, newZ);
                        GameSystems.MapObject.Move(grappledOpponent, newLoc);
                        grappleState.currentLength = grappleState.targetLength;
                        grappleState.state = 4;
                    }
                }
                    break;
                case 7:
                {
                    grappleState.currentLength = grappleState.currentLength - locXY.INCH_PER_HALFTILE;
                    // Move the opponent closer to the frog
                    float newX = tonguePosX - worldMatrixFrog.M31 * locXY.INCH_PER_HALFTILE;
                    float newZ = tonguePosZ - worldMatrixFrog.M33 * locXY.INCH_PER_HALFTILE;
                    var newLoc = LocAndOffsets.FromInches(newX, newZ);
                    GameSystems.MapObject.Move(grappledOpponent, newLoc);

                    if (grappleState.currentLength < 0)
                    {
                        newX = worldMatrixFrog.M41;
                        newZ = worldMatrixFrog.M43;
                        newLoc = LocAndOffsets.FromInches(newX, newZ);
                        GameSystems.MapObject.Move(grappledOpponent, newLoc);
                        GameSystems.ObjFade.FadeTo(grappledOpponent, 0, 0, 16, 0);
                        grappleState.currentLength = 0;
                        grappleState.state = 0;

                        // Probably the swallow animation
                        var animId = GameSystems.Critter.GetAnimId(giantFrog, WeaponAnim.Special3);
                        giantFrog.SetAnimId(animId);
                    }
                }
                    break;
                default:
                    break;
            }

            // Update to the new grapple state
            SetGrappleState(giantFrog, grappleState);

            // The directional vector of the tongue ref point on the frog
            var tongueUp = worldMatrixFrog.GetRow(0).ToVector3();
            var tongueRight = worldMatrixFrog.GetRow(1).ToVector3();
            var tongueDir = worldMatrixFrog.GetRow(2).ToVector3();
            var tonguePos = worldMatrixFrog.GetRow(3).ToVector3();

            RenderTongue(grappleState, tongueDir, tongueUp, tongueRight, tonguePos, lights, alpha);
        }

        public bool IsGiantFrog(GameObjectBody obj)
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

        private RenderingDevice mDevice;
        private ResourceRef<IMdfRenderMaterial> mTongueMaterial;
        private BufferBinding mBufferBinding;
        private ResourceRef<VertexBuffer> mVertexBuffer;
        private ResourceRef<IndexBuffer> mIndexBuffer;

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

            mIndexBuffer = mDevice.CreateIndexBuffer(indices);
        }

        private void RenderTongue(GrappleState grappleState,
            in Vector3 tongueDir,
            in Vector3 tongueUp,
            in Vector3 tongueRight,
            in Vector3 tonguePos,
            IList<Light3d> lights,
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
                float tongueRadius = 3.0f + radiusVariance * (grappleState.currentLength / 700.0f) * 3.0f;

                // Calculates the center point of the tongue along the directional vector
                // of the tongue_ref
                float distFromFrog = grappleState.currentLength * i / 15.0f;
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

            mVertexBuffer.Resource.Update<TongueVertex>(vertices);

            mBufferBinding.Bind();
            MdfRenderOverrides overrides = new MdfRenderOverrides();
            overrides.alpha = alpha;
            mTongueMaterial.Resource.Bind(mDevice, lights, overrides);
            mDevice.SetIndexBuffer(mIndexBuffer);

            mDevice.DrawIndexed(
                PrimitiveType.TriangleList,
                VertexCount,
                TriCount * 3);
        }

        public GameObjectBody GetGrappledOpponent(GameObjectBody giantFrog)
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
        public static void PlayFailedLatch(GameObjectBody giantFrog)
        {
            SetGrappleState(giantFrog, new GrappleState {
                state = 1
            });
        }

        [TempleDllLocation(0x100209c0)]
        public static void PlayLatch(GameObjectBody giantFrog)
        {
            SetGrappleState(giantFrog, new GrappleState {
                state = 3
            });
        }

        [TempleDllLocation(0x100209e0)]
        public static void PlayPull(GameObjectBody giantFrog)
        {
            // Keep current tongue position as starting point
            var grappleState = GetGrappleState(giantFrog);
            grappleState.state = 5;
            SetGrappleState(giantFrog, grappleState);
        }

        [TempleDllLocation(0x10020a20)]
        public static void PlaySwallow(GameObjectBody giantFrog)
        {
            // Keep current tongue position as starting point
            var grappleState = GetGrappleState(giantFrog);
            grappleState.state = 7;
            SetGrappleState(giantFrog, grappleState);
        }

        private static void SetGrappleState(GameObjectBody giantFrog, GrappleState state)
        {
            uint newGrappleState = (uint) (((byte) state.targetLength) << 24
                                           | ((byte) state.currentLength) << 16
                                           | state.state);
            giantFrog.SetUInt32(obj_f.grapple_state, newGrappleState);
        }

        private static GrappleState GetGrappleState(GameObjectBody giantFrog)
        {
            var grappleState = giantFrog.GetUInt32(obj_f.grapple_state);

            // Unpack the grapple state
            GrappleState result;
            result.state = (ushort) (grappleState & 0xFFFF);
            result.currentLength = (float) ((grappleState >> 16) & 0xFF);
            result.targetLength = (float) (grappleState >> 24);
            return result;
        }
    }
}