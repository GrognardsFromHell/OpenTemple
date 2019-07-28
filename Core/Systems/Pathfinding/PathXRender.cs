using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    public class PathXRenderSystem : IDisposable
    {
        [TempleDllLocation(0x115B1E94)]
        private ResourceRef<IMdfRenderMaterial> _redLineMaterial;

        [TempleDllLocation(0x115B1E98)]
        private ResourceRef<IMdfRenderMaterial> _redLineOccludedMaterial;

        [TempleDllLocation(0x115B1E8C)]
        private ResourceRef<IMdfRenderMaterial> _greenLineMaterial;

        [TempleDllLocation(0x115B1E9C)]
        private ResourceRef<IMdfRenderMaterial> _greenLineOccludedMaterial;

        [TempleDllLocation(0x115B1E88)]
        private ResourceRef<IMdfRenderMaterial> _blueLineMaterial;

        [TempleDllLocation(0x115B1E90)]
        private ResourceRef<IMdfRenderMaterial> _blueLineOccludedMaterial;

        [TempleDllLocation(0x115B1E80)]
        private ResourceRef<IMdfRenderMaterial> _yellowLineMaterial;

        [TempleDllLocation(0x115B1E84)]
        private ResourceRef<IMdfRenderMaterial> _yellowLineOccludedMaterial;

        [TempleDllLocation(0x10BD2C50)]
        private ResourceRef<IMdfRenderMaterial> _spellPointerMaterial;

        [TempleDllLocation(0x10BD2C00)]
        private TigTextStyle _textStyle;

        [TempleDllLocation(0x10BD2CD8)]
        private int intgameselTexts;

        private readonly RenderingDevice _device;

        private ResourceRef<BufferBinding> _bufferBinding;

        private ResourceRef<BufferBinding> _aooBufferBinding;

        private ResourceRef<IndexBuffer> _indexBuffer;

        private ResourceRef<VertexBuffer> _vertexBuffer;

        private ResourceRef<IndexBuffer> _aooIndexBuffer;

        private ResourceRef<VertexBuffer> _aooVertexBuffer;

        public PathXRenderSystem()
        {
            var device = Tig.RenderingDevice;

            // Create the indices now, they never change
            Span<ushort> indices = stackalloc ushort[6] {0, 2, 1, 0, 3, 2};

            _indexBuffer = device.CreateIndexBuffer(indices);
            _vertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 4);
            _bufferBinding = Tig.RenderingDevice.CreateMdfBufferBinding().Ref();
            _bufferBinding.Resource.AddBuffer<IntgameVertex>(_vertexBuffer, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _aooIndexBuffer = device.CreateIndexBuffer(AooIndices);
            _aooVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 7);
            _aooBufferBinding = Tig.RenderingDevice.CreateMdfBufferBinding().Ref();
            _aooBufferBinding.Resource.AddBuffer<IntgameVertex>(_aooVertexBuffer, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        [TempleDllLocation(0x101066d0)]
        public void LoadShaders()
        {
            _textStyle = new TigTextStyle();
            _textStyle.flags = 0;
            _textStyle.textColor = new ColorRect(PackedLinearColorA.White);
            _textStyle.shadowColor = new ColorRect(PackedLinearColorA.White);
            _textStyle.kerning = 2;
            _textStyle.leading = 0;

            _redLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line.mdf");
            _redLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line_oc.mdf");
            _greenLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line.mdf");
            _greenLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line_oc.mdf");
            _blueLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line.mdf");
            _blueLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line_oc.mdf");
            _yellowLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line.mdf");
            _yellowLineOccludedMaterial =
                Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line_oc.mdf");
            _spellPointerMaterial =
                Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/spell_player-pointer.mdf");

            intgameselTexts = 0;
        }

        [TempleDllLocation(0x10106f20)]
        public TigTextStyle GetTextStyle()
        {
            return _textStyle;
        }

        [TempleDllLocation(0x115b1e40)]
        public int uiIntgamePathpreviewState { get; set; }

        [TempleDllLocation(0x10bd2d3c)]
        private bool uiIntgamePathdrawInited;

        [TempleDllLocation(0x115b1e50)]
        private LocAndOffsets uiIntgamePathdrawPrevLoc;

        [TempleDllLocation(0x10bd2ce0)]
        private PointPacket uiIntgamePointPacket;

        [TempleDllLocation(0x10290C44)]
        private PackedLinearColorA[] uiIntgameOutlineColors2 =
        {
            new PackedLinearColorA(0xFF0000FF),
            new PackedLinearColorA(0xFF00FF00),
            new PackedLinearColorA(0xFF808080),
            new PackedLinearColorA(0xFF00FF00),
            new PackedLinearColorA(0xFF808080),
        };

        [TempleDllLocation(0x10290C58)]
        private PackedLinearColorA[] uiIntgameOutlineColors =
        {
            new PackedLinearColorA(0x3F0000FF),
            new PackedLinearColorA(0x3F00FF00),
            new PackedLinearColorA(0x3F808080),
            new PackedLinearColorA(0x3F00FF00),
            new PackedLinearColorA(0x3F808080),
        };

        [TempleDllLocation(0x10290C6C)]
        private PackedLinearColorA[] uiIntgameFillColors2 =
        {
            new PackedLinearColorA(0x8000FF00),
            new PackedLinearColorA(0x80FFFF00),
            new PackedLinearColorA(0x80FF0000),
        };

        [TempleDllLocation(0x10290C78)]
        private PackedLinearColorA[] uiIntgameFillColors =
        {
            new PackedLinearColorA(0x1F00FF00),
            new PackedLinearColorA(0x1FFFFF00),
            new PackedLinearColorA(0x1FFF0000),
        };

        [TempleDllLocation(0x115b1e4c)]
        private int uiIntgameActionbarDepletionState;

        [TempleDllLocation(0x115b1e44)]
        private float uiIntgameGreenMoveLength;

        [TempleDllLocation(0x115b1e48)]
        private float uiIntgameTotalMoveLength;

        [TempleDllLocation(0x115b1e60)]
        private float uiIntgamePathdrawCumulativeDist;

        [TempleDllLocation(0x115b1e78)]
        private float uiIntgamePathpreviewFromToDist;

        [TempleDllLocation(0x115b1e68)]
        private LocAndOffsets uiIntgamePathpreviewFrom;

        [TempleDllLocation(0x10107580)]
        public void RenderMovementTarget(LocAndOffsets loc, GameObjectBody mover)
        {
            var radius = mover.GetRadius();

            // Draw the occluded variant first
            var fillOccluded = new PackedLinearColorA(0x180000FF);
            var borderOccluded = new PackedLinearColorA(0x600000FF);
            DrawCircle3d(loc, 1.5f, fillOccluded, borderOccluded, radius, true);

            var fill = new PackedLinearColorA(0x400000FF);
            var border = new PackedLinearColorA(0xFF0000FF);
            DrawCircle3d(loc, 1.5f, fill, border, radius, false);
        }

        /// <param name="occludedOnly">Previously was set via SetRenderZbufferDepth globally before calling this function.
        /// Setting it to 3 meant rendering only occluded objects.</param>
        [TempleDllLocation(0x10106B70)]
        public void DrawCircle3d(
            LocAndOffsets center,
            float negElevation,
            PackedLinearColorA fillColor,
            PackedLinearColorA borderColor,
            float radius,
            bool occludedOnly)
        {
            // This is hell of a hacky way... the -44.2° seems to be a hardcoded assumption about the camera tilt
            var y = -(MathF.Sin(-0.77539754f) * negElevation);
            var center3d = center.ToInches3D(y);

            Tig.ShapeRenderer3d.DrawFilledCircle(
                center3d, radius, borderColor, fillColor, occludedOnly
            );
        }


        public void Dispose()
        {
            _bufferBinding.Dispose();
            _indexBuffer.Dispose();
            _vertexBuffer.Dispose();

            _aooBufferBinding.Dispose();
            _aooIndexBuffer.Dispose();
            _aooVertexBuffer.Dispose();
        }

        [TempleDllLocation(0x10109c80)]
        public void PathpreviewGetFromToDist(PathQueryResult pqr)
        {
            if ( MathF.Abs(uiIntgamePathpreviewFromToDist) <= 0.0001f )
            {
                uiIntgamePathpreviewFrom = pqr.from;
            }

            uiIntgamePathpreviewFromToDist += uiIntgamePathpreviewFrom.DistanceTo(pqr.to) / locXY.INCH_PER_FEET;
            uiIntgamePathpreviewFrom = pqr.to;
        }

        [TempleDllLocation(0x10109d50)]
        public void RenderPathPreview(PathQueryResult pqr, bool isLastActionWithPath)
        {
            if (uiIntgamePathpreviewState != 4)
            {
                if (uiIntgamePathpreviewState == 3)
                {
                    if (isLastActionWithPath)
                    {
                        PathRenderEndpointCircle(pqr.to, pqr.mover, 1.0f);
                    }

                    return;
                }

                if (uiIntgamePathdrawInited)
                {
                    PathdrawRecursive_10109A30(ref uiIntgamePointPacket, pqr.from);
                }
                else
                {
                    var radius = pqr.mover.GetRadius();
                    PointPacketInit(
                        ref uiIntgamePointPacket,
                        uiIntgameOutlineColors2[uiIntgamePathpreviewState],
                        5.0f,
                        radius,
                        pqr.from);
                    uiIntgamePathdrawPrevLoc = pqr.from;
                    uiIntgamePathdrawInited = true;
                }

                if (isLastActionWithPath)
                {
                    SetStartingPoint(ref uiIntgamePointPacket, pqr.to);
                }

                var zoffset = 1.0f;
                if (pqr.flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
                {
                    if (!isLastActionWithPath)
                    {
                        PathdrawRecursive_10109A30(ref uiIntgamePointPacket, pqr.to);
                        return;
                    }
                }
                else
                {
                    // TODO: Check the -1 here...
                    for (var i = 0; i < pqr.nodes.Count - 1; i++)
                    {
                        var location = pqr.nodes[i];
                        PathdrawRecursive_10109A30(ref uiIntgamePointPacket, location);
                    }

                    if (!isLastActionWithPath)
                    {
                        return;
                    }

                    zoffset += pqr.nodes.Count - 1; // TODO Super weird.... (and probably just wrong...)
                }

                PathdrawRecursive_10109A30(ref uiIntgamePointPacket, pqr.to);
                PathRender_10109930(ref uiIntgamePointPacket);
                PathRenderEndpointCircle(pqr.to, pqr.mover, zoffset);
                uiIntgamePathdrawInited = false;
            }
        }

        [TempleDllLocation(0x10107560)]
        private void SetStartingPoint(ref PointPacket pntPkt, LocAndOffsets loc)
        {
            pntPkt.points[0] = loc.ToInches3D();
            pntPkt.pathdrawStatus = 1;
        }

        [TempleDllLocation(0x10109930)]
        private void PathRender_10109930(ref PointPacket a1)
        {
            PathRender_10109330(a1.points[0], ref a1);
        }

        [TempleDllLocation(0x101099f0)]
        public void UiIntgameInitPathpreviewVars(int state, float greenMoveLength, float totalMoveLength)
        {
            uiIntgamePathpreviewState = state;
            uiIntgameGreenMoveLength = greenMoveLength;
            uiIntgameTotalMoveLength = totalMoveLength;
            uiIntgamePathdrawCumulativeDist = 0;
            uiIntgamePathpreviewFromToDist = 0;
        }

        [TempleDllLocation(0x10109a30)]
        private void PathdrawRecursive_10109A30(ref PointPacket pntPkt, LocAndOffsets loc)
        {
            var cumulDist = uiIntgamePathdrawCumulativeDist;
            var newCumulDist = uiIntgamePathdrawPrevLoc.DistanceTo(loc) / locXY.INCH_PER_FEET + cumulDist;
            if (uiIntgameGreenMoveLength - 0.1f <= cumulDist)
            {
                if (uiIntgameTotalMoveLength - 0.1f <= cumulDist)
                {
                    PathDraw_101099C0(ref pntPkt, loc);
                    pntPkt.SetMaterials(_redLineMaterial, _redLineOccludedMaterial);
                    pntPkt.SetColor3(new PackedLinearColorA(0x80FF0000));
                    uiIntgameActionbarDepletionState = 2;
                    uiIntgamePathdrawCumulativeDist = newCumulDist;
                    uiIntgamePathdrawPrevLoc = loc;
                    return;
                }

                if (uiIntgameTotalMoveLength + 0.1f >= newCumulDist)
                {
                    PathDraw_101099C0(ref pntPkt, loc);
                    pntPkt.SetMaterials(_yellowLineMaterial, _yellowLineOccludedMaterial);
                    pntPkt.SetColor3(new PackedLinearColorA(0x80FFFF00));
                    uiIntgameActionbarDepletionState = 1;
                    uiIntgamePathdrawCumulativeDist = newCumulDist;
                    uiIntgamePathdrawPrevLoc = loc;
                    return;
                }

                var factor = (uiIntgameTotalMoveLength - cumulDist) / (newCumulDist - cumulDist);
                var resultLoc = LerpLocation(uiIntgamePathdrawPrevLoc, loc, factor);
                PathdrawRecursive_10109A30(ref pntPkt, resultLoc);
                PathdrawRecursive_10109A30(ref pntPkt, loc);
                return;
            }

            if (uiIntgameGreenMoveLength + 0.1f < newCumulDist)
            {
                var factor = (uiIntgameGreenMoveLength - cumulDist) / (newCumulDist - cumulDist);
                var resultLoc = LerpLocation(uiIntgamePathdrawPrevLoc, loc, factor);
                PathdrawRecursive_10109A30(ref pntPkt, resultLoc);
                PathdrawRecursive_10109A30(ref pntPkt, loc);
                return;
            }

            PathDraw_101099C0(ref pntPkt, loc);
            pntPkt.SetMaterials(_greenLineMaterial, _greenLineOccludedMaterial);
            pntPkt.SetColor3(new PackedLinearColorA(0x8000FF00));
            uiIntgameActionbarDepletionState = 0;
            uiIntgamePathdrawCumulativeDist = newCumulDist;
            uiIntgamePathdrawPrevLoc = loc;
        }

        [TempleDllLocation(0x10040e70)]
        public LocAndOffsets LerpLocation(LocAndOffsets from, LocAndOffsets to, float factor)
        {
            var targetPos = Vector2.Lerp(from.ToInches2D(), to.ToInches2D(), factor);
            return LocAndOffsets.FromInches(targetPos);
        }

        [TempleDllLocation(0x101099c0)]
        private void PathDraw_101099C0(ref PointPacket pntPkt, LocAndOffsets loc)
        {
            var pntNode = loc.ToInches3D();
            PathRender_10109330 /*0x10109330*/(pntNode, ref pntPkt);
        }

        [TempleDllLocation(0x10109330)]
        private void PathRender_10109330(Vector3 pntNode, ref PointPacket pntPkt)
        {
            // TODO: This function is a mess and needs work
            Span<Vector3> points = pntPkt.points;

            var v85 = pntPkt.pathdrawStatus == 2 ? points[2] : pntNode;

            if (pntPkt.pathdrawStatus == 1)
            {
                if ((points[0] - v85).LengthSquared() < pntPkt.radius * pntPkt.radius)
                {
                    pntPkt.pathdrawStatus = 2;
                    var dir = Vector3.Normalize(points[0] - points[2]) * (-pntPkt.radius);
                    v85 = points[0] + dir;
                }
            }

            Vector3 a1, a3, a5, pnt2;
            if ((points[2] - v85).Length() < 0.01f)
            {
                if ((points[2] - points[1]).Length() < 0.01f)
                {
                    return;
                }

                a3 = Vector3.Normalize(points[1] - points[2]);

                pnt2.X = a3.Z;
                pnt2.Y = a3.Y;
                pnt2.Z = -a3.X;
                a1 = pnt2;
                a5 = a3;
            }
            else
            {
                a5 = Vector3.Normalize(points[2] - v85);

                a1.X = a5.Z;
                a1.Y = a5.Y;
                a1.Z = -a5.X;

                if ((points[1] - points[2]).Length() >= 0.01f)
                {
                    a3 = Vector3.Normalize(points[1] - points[2]);
                    pnt2.X = a3.Z;
                    pnt2.Y = a3.Y;
                    pnt2.Z = -a3.X;
                }
                else
                {
                    a3 = a5;

                    pnt2.X = a1.X;
                    pnt2.Y = a1.Y;
                    pnt2.Z = a1.Z;

                    points[3] = points[1] + a1 * pntPkt.color2;
                    points[4] = points[1] - a1 * pntPkt.color2;
                }
            }


            Vector3 v89;
            // TODO: Double-check this, might be wrong
            if (Vector3.Dot(a3, a5) <= 0)
            {
                var v42 = pnt2.X * pntPkt.color2;
                Vector3 pntA, pntB;
                v89.Z = pnt2.Z * pntPkt.color2;
                pntB.Y = 0;
                var v47 = v42 + points[2].X;
                pntA.Y = 0;
                a1.Y = 0;
                v89.Y = 0;
                pntB.X = v47;
                pntB.Z = v89.Z + points[2].Z;
                var v48 = points[2].X - v42;
                var v49 = points[2].Z - v89.Z;
                var v50 = pntPkt.color2 * -0.70700002f; // Might be cos(135)
                var v51 = a3.X * v50;
                a3.Z = a3.Z * v50;
                pntB.X = pntB.X + v51;
                pntB.Z = a3.Z + pntB.Z;
                pntA.X = v51 + v48;
                pntA.Z = a3.Z + v49;


                a1.X = a1.X * pntPkt.color2 + points[2].X;
                a1.Z = a1.Z * pntPkt.color2 + points[2].Z;
                var v54 = points[2].X - a1.X * pntPkt.color2;
                var v55 = points[2].Z - a1.Z * pntPkt.color2;
                var v56 = pntPkt.color2 * 0.70700002f; // Might be sin(135)
                var v57 = a5.X * v56;
                a3.Z = a5.Z * v56;
                a1.X = a1.X + v57;
                a1.Z = a1.Z + a3.Z;
                Vector3 newPoint4;
                newPoint4.X = v54 + v57;
                newPoint4.Y = 0;
                newPoint4.Z = v55 + a3.Z;
                RenderPathSegment(pntA, pntB, ref pntPkt);
                points[1] = points[2];
                points[2] = v85;
                points[3] = a1;
                points[4] = newPoint4;
            }
            else
            {
                v89 = points[1] + pnt2 * pntPkt.color2;
                var a2 = points[1] - pnt2 * pntPkt.color2;

                var tmp = a1 * pntPkt.color2;

                var tmp2 = v85 + tmp;
                var tmp3 = v85 - tmp;

                if (sub_10107280 /*0x10107280*/(ref a1, v89, a3, tmp2, a5))
                {
                    sub_10107280 /*0x10107280*/(ref v89, a2, a3, tmp3, a5);
                }
                else
                {
                    a1 = points[2] + tmp;
                    v89 = points[2] - tmp;
                }

                RenderPathSegment(v89, a1, ref pntPkt);
                points[1] = points[2];
                points[2] = v85;
                points[3] = a1;
                points[4] = v89;
            }
        }

// TODO: LINE CROSSES !??!?!?
        [TempleDllLocation(0x10107280)]
        public bool sub_10107280(ref Vector3 a1, Vector3 a2, Vector3 a3, Vector3 a4, Vector3 a5)
        {
            var a3a = a5.X * a3.Z - a5.Z * a3.X;
            if (a1.LengthSquared() * a2.LengthSquared() * 0.0001f < a3a * a3a)
            {
                var v9 = ((a4.Z - a2.Z) * a5.X - (a4.X - a2.X) * a5.Z) * (1.0f / a3a);

                a1.X = v9 * a3.X + a2.X;
                a1.Y = 0;
                a1.Z = v9 * a3.Z + a2.Z;
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x101074a0)]
        private void RenderPathSegment(Vector3 pnt1, Vector3 pnt2, ref PointPacket pkt)
        {
            ++pkt.renderCount;

            Span<IntgameVertex> vertices = stackalloc IntgameVertex[4];

            vertices[0].pos.X = pkt.points[3].X;
            vertices[0].pos.Z = pkt.points[3].Z;
            vertices[0].uv = new Vector2(1, 0);

            vertices[1].pos.X = pkt.points[4].X;
            vertices[1].pos.Z = pkt.points[4].Z;
            vertices[1].uv = new Vector2(1, 1);

            vertices[2].pos.X = pnt1.X;
            vertices[2].pos.Z = pnt1.Z;
            vertices[2].uv = new Vector2(0.5f, 1);

            vertices[3].pos.X = pnt2.X;
            vertices[3].pos.Z = pnt2.Z;
            vertices[3].uv = new Vector2(0.5f, 0);

            // This is highly suspect and seems to be a leftover from 2D thinking... the
            // argument to the sine is actually the tilt of the camera
            var height = MathF.Sin(-0.77539754f) * -1.5f;
            for (var i = 0; i < vertices.Length; i++)
            {
                ref var vertex = ref vertices[i];
                vertex.pos.Y = height;
                vertex.pos.W = 1;
                vertex.normal = Vector4.UnitY;
                vertex.diffuse = PackedLinearColorA.White;
            }

            _device.UpdateBuffer<IntgameVertex>(_vertexBuffer, vertices);
            _bufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_indexBuffer);

            // Render once with the occluded material
            pkt.occludedMaterial.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, 6);

            // Render again with just the unoccluded part
            pkt.normalMaterial.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, 6);
        }

        [TempleDllLocation(0x10109be0)]
        public void PathRenderEndpointCircle(LocAndOffsets loc, GameObjectBody obj, float zoffset)
        {
            var radius = obj.GetRadius();
            DrawCircle3d(
                loc,
                zoffset,
                uiIntgameFillColors[uiIntgameActionbarDepletionState],
                uiIntgameOutlineColors[uiIntgamePathpreviewState],
                radius,
                true);

            DrawCircle3d(
                loc,
                zoffset,
                uiIntgameFillColors2[uiIntgameActionbarDepletionState],
                uiIntgameOutlineColors2[uiIntgamePathpreviewState],
                radius,
                false);
        }

        [TempleDllLocation(0x101092a0)]
        private void PointPacketInit(ref PointPacket pntPkt, PackedLinearColorA color1, float color2, float moverRadius,
            LocAndOffsets loc)
        {
            pntPkt.points[1] = loc.ToInches3D();
            pntPkt.points[2] = loc.ToInches3D();
            pntPkt.points[3] = pntPkt.points[1];
            pntPkt.points[4] = pntPkt.points[2];
            pntPkt.color2 = color2;
            pntPkt.radius = moverRadius;
            pntPkt.colorSthg3 = PackedLinearColorA.White;
            pntPkt.color1 = color1;
            pntPkt.pathdrawStatus = 0;
            pntPkt.normalMaterial = _greenLineMaterial.Resource;
            pntPkt.occludedMaterial = _greenLineOccludedMaterial.Resource;
            pntPkt.renderCount = 0;
        }

        private static readonly Vector4[] AooPosOffsets = {
            new Vector4(0.0f, 0.0f, -5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, -5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, 5.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, -10.0f, 0.0f),
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, 10.0f, 0.0f),
        };

        private static readonly Vector2[] AooUvs = {
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.5f),
            new Vector2(0.0f, 0.0f)
        };

        private static readonly ushort[] AooIndices =
        {
            0, 2, 1, 0, 3, 2, 4, 5, 6, 0
        };

        [TempleDllLocation(0x101090e0)]
        public void DrawAttackOfOpportunityIndicator(LocAndOffsets from, LocAndOffsets to)
        {
            Span<IntgameVertex> vertices = stackalloc IntgameVertex[7];

            var pntNode = from.ToInches3D();
            var v11 = to.ToInches3D();

            // TODO: Another bogus way of determining "elevation"
            var negElevation = MathF.Sin(-0.77539754f) * -3.0f;
            var dir = (v11 - pntNode);
            var dirLength = dir.Length();
            dir /= dirLength;

            var offset = dirLength;
            if (offset > 36.0f)
            {
                offset = dirLength - 18.0f;
            }

            for (var i = 0; i < 7; i++)
            {
                vertices[i].pos.Y = negElevation;
                vertices[i].pos.X = dir.X * AooPosOffsets[i].X * offset + dir.Z * AooPosOffsets[i].Z + pntNode.X;
                vertices[i].pos.Z = dir.Z * AooPosOffsets[i].X * offset - dir.X * AooPosOffsets[i].Z + pntNode.Z;
                vertices[i].pos.W = 1;

                vertices[i].normal = Vector4.UnitY;
                vertices[i].uv = AooUvs[i];
                vertices[i].diffuse = PackedLinearColorA.White;
            }

            DrawAooLine(ref vertices[0], ref vertices[3]); // I think this is wrong (or the duplicate one below)
            DrawAooLine(ref vertices[1], ref vertices[2]);
            DrawAooLine(ref vertices[0], ref vertices[3]);
            DrawAooLine(ref vertices[4], ref vertices[5]);
            DrawAooLine(ref vertices[5], ref vertices[6]);
            DrawAooLine(ref vertices[6], ref vertices[4]);

            _device.UpdateBuffer<IntgameVertex>(_aooVertexBuffer, vertices);
            _aooBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_aooIndexBuffer);

            _redLineMaterial.Resource.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, AooIndices.Length);
        }

        private static void DrawAooLine(ref IntgameVertex from, ref IntgameVertex to)
        {
            var from3 = from.pos.ToVector3();
            var to3 = to.pos.ToVector3();
            Tig.ShapeRenderer3d.DrawLine(from3, to3, new PackedLinearColorA(0xFFFF0000));
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct IntgameVertex
    {
        public Vector4 pos;
        public Vector4 normal;
        public PackedLinearColorA diffuse;
        public Vector2 uv;

        public static readonly int Size = Marshal.SizeOf<IntgameVertex>();
    }

    internal struct PointPacket
    {
        public Vector3[] points;
        public float color2;
        public float radius;
        public PackedLinearColorA colorSthg3;
        public PackedLinearColorA color1;
        public int pathdrawStatus; // 1, 2
        public IMdfRenderMaterial normalMaterial;
        public IMdfRenderMaterial occludedMaterial;
        public int renderCount;

        [TempleDllLocation(0x10107490)]
        public void SetColor3(PackedLinearColorA color)
        {
            colorSthg3 = color;
        }

        [TempleDllLocation(0x10107470)]
        public void SetMaterials(ResourceRef<IMdfRenderMaterial> normalMaterial,
            ResourceRef<IMdfRenderMaterial> occludedMaterial)
        {
            this.normalMaterial = normalMaterial.Resource;
            this.occludedMaterial = occludedMaterial.Resource;
        }
    }

}
