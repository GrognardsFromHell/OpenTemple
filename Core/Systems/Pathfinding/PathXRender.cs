using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Pathfinding
{

    internal enum PathSegmentColor
    {
        Green,
        Yellow,
        Red
    }

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

        private readonly RenderingDevice _device;

        private ResourceRef<BufferBinding> _bufferBinding;

        private ResourceRef<BufferBinding> _aooBufferBinding;

        private ResourceRef<IndexBuffer> _indexBuffer;

        private ResourceRef<VertexBuffer> _vertexBuffer;

        private ResourceRef<IndexBuffer> _aooIndexBuffer;

        private ResourceRef<VertexBuffer> _aooVertexBuffer;

        public PathXRenderSystem()
        {
            _device = Tig.RenderingDevice;

            // Create the indices now, they never change
            Span<ushort> indices = stackalloc ushort[6] {0, 2, 1, 0, 3, 2};

            _indexBuffer = _device.CreateIndexBuffer(indices);
            _vertexBuffer = _device.CreateEmptyVertexBuffer(IntgameVertex.Size * 4, debugName:"PathXVB");
            _bufferBinding = Tig.RenderingDevice.CreateMdfBufferBinding().Ref();
            _bufferBinding.Resource.AddBuffer<IntgameVertex>(_vertexBuffer, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _aooIndexBuffer = _device.CreateIndexBuffer(AooIndices);
            _aooVertexBuffer = _device.CreateEmptyVertexBuffer(IntgameVertex.Size * 7, debugName:"PathXVBAoo");
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
            _redLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line.mdf");
            _redLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/red-line_oc.mdf");
            _greenLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line.mdf");
            _greenLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/green-line_oc.mdf");
            _blueLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line.mdf");
            _blueLineOccludedMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/blue-line_oc.mdf");
            _yellowLineMaterial = Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line.mdf");
            _yellowLineOccludedMaterial =
                Tig.MdfFactory.LoadMaterial("art/interface/intgame_select/yellow-line_oc.mdf");
        }

        [TempleDllLocation(0x115b1e40)]
        private PathPreviewMode _pathPreviewMode;

        public int HourglassDepletionState
        {
            [TempleDllLocation(0x10109d10)]
            get
            {
                if (uiIntgamePathpreviewFromToDist > uiIntgameGreenMoveLength)
                {
                    if (uiIntgamePathpreviewFromToDist > uiIntgameTotalMoveLength)
                    {
                        // Full
                        return 2;
                    }
                    // Partial
                    return 1;
                }
                // Empty
                return 0;
            }
        }

        [TempleDllLocation(0x10bd2d3c)]
        private bool uiIntgamePathdrawInited;

        [TempleDllLocation(0x115b1e50)]
        private LocAndOffsets _previousStop;

        [TempleDllLocation(0x10bd2ce0)]
        private PointPacket uiIntgamePointPacket;

        [TempleDllLocation(0x10290C44)]
        private static readonly Dictionary<PathPreviewMode, PackedLinearColorA> WaypointOutlineColors = new Dictionary<PathPreviewMode, PackedLinearColorA>
        {
            {PathPreviewMode.None, new PackedLinearColorA(0xFF0000FF)},
            {PathPreviewMode.One, new PackedLinearColorA(0xFF00FF00)},
            {PathPreviewMode.Two, new PackedLinearColorA(0xFF808080)},
            {PathPreviewMode.IsMoving, new PackedLinearColorA(0xFF00FF00)}
        };

        [TempleDllLocation(0x10290C58)]
        private static readonly Dictionary<PathPreviewMode, PackedLinearColorA> OccludedWaypointOutlineColors = new Dictionary<PathPreviewMode, PackedLinearColorA>
        {
            {PathPreviewMode.None, new PackedLinearColorA(0x3F0000FF)},
            {PathPreviewMode.One, new PackedLinearColorA(0x3F00FF00)},
            {PathPreviewMode.Two, new PackedLinearColorA(0x3F808080)},
            {PathPreviewMode.IsMoving, new PackedLinearColorA(0x3F00FF00)}
        };

        [TempleDllLocation(0x10290C6C)]
        private static readonly Dictionary<PathSegmentColor, PackedLinearColorA> WaypointFillColors =
            new Dictionary<PathSegmentColor, PackedLinearColorA>
            {
                {PathSegmentColor.Green, new PackedLinearColorA(0x8000FF00)},
                {PathSegmentColor.Yellow, new PackedLinearColorA(0x80FFFF00)},
                {PathSegmentColor.Red, new PackedLinearColorA(0x80FF0000)},
            };

        [TempleDllLocation(0x10290C78)]
        private static readonly Dictionary<PathSegmentColor, PackedLinearColorA> OccludedWaypointFillColors =
            new Dictionary<PathSegmentColor, PackedLinearColorA>
            {
                {PathSegmentColor.Green, new PackedLinearColorA(0x1F00FF00)},
                {PathSegmentColor.Yellow, new PackedLinearColorA(0x1FFFFF00)},
                {PathSegmentColor.Red, new PackedLinearColorA(0x1FFF0000)},
            };

        [TempleDllLocation(0x115b1e4c)]
        private PathSegmentColor _previousSegmentColor = PathSegmentColor.Green;

        [TempleDllLocation(0x115b1e44)]
        private float uiIntgameGreenMoveLength;

        [TempleDllLocation(0x115b1e48)]
        private float uiIntgameTotalMoveLength;

        [TempleDllLocation(0x115b1e60)]
        private float previousStopCumulativeDist;

        [TempleDllLocation(0x115b1e78)]
        private float uiIntgamePathpreviewFromToDist;

        [TempleDllLocation(0x115b1e68)]
        private LocAndOffsets uiIntgamePathpreviewFrom;

        [TempleDllLocation(0x10107580)]
        public void RenderMovementTarget(IGameViewport viewport, LocAndOffsets loc, GameObjectBody mover)
        {
            var radius = mover.GetRadius();

            // Draw the occluded variant first
            var fillOccluded = new PackedLinearColorA(0x180000FF);
            var borderOccluded = new PackedLinearColorA(0x600000FF);
            DrawCircle3d(viewport, loc, 1.5f, fillOccluded, borderOccluded, radius, true);

            var fill = new PackedLinearColorA(0x400000FF);
            var border = new PackedLinearColorA(0xFF0000FF);
            DrawCircle3d(viewport, loc, 1.5f, fill, border, radius, false);
        }

        /// occludedOnly Previously was set via SetRenderZbufferDepth globally before calling this function.
        /// Setting it to 3 meant rendering only occluded objects.
        [TempleDllLocation(0x10106B70)]
        public void DrawCircle3d(
            IGameViewport viewport,
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
                viewport, center3d, radius, borderColor, fillColor, occludedOnly
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
            if (MathF.Abs(uiIntgamePathpreviewFromToDist) <= 0.0001f)
            {
                uiIntgamePathpreviewFrom = pqr.from;
            }

            uiIntgamePathpreviewFromToDist += uiIntgamePathpreviewFrom.DistanceTo(pqr.to) / locXY.INCH_PER_FEET;
            uiIntgamePathpreviewFrom = pqr.to;
        }

        [TempleDllLocation(0x10109d50)]
        public void RenderPathPreview(IGameViewport viewport, PathQueryResult pqr, bool showDestinationCircle)
        {
            if (_pathPreviewMode == PathPreviewMode.IsMoving)
            {
                if (showDestinationCircle)
                {
                    PathRenderEndpointCircle(viewport, pqr.to, pqr.mover, 1.0f);
                }

                return;
            }

            if (uiIntgamePathdrawInited)
            {
                RenderStraightPreviewPath(viewport, ref uiIntgamePointPacket, pqr.from);
            }
            else
            {
                var radius = pqr.mover.GetRadius();
                PointPacketInit(
                    ref uiIntgamePointPacket,
                    5.0f,
                    radius,
                    pqr.from);
                _previousStop = pqr.from;
                uiIntgamePathdrawInited = true;
            }

            if (showDestinationCircle)
            {
                SetCircleCenter(ref uiIntgamePointPacket, pqr.to);
            }

            var zoffset = 1.0f;
            if (pqr.flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                if (!showDestinationCircle)
                {
                    RenderStraightPreviewPath(viewport, ref uiIntgamePointPacket, pqr.to);
                    return;
                }
            }
            else
            {
                // TODO: Check the -1 here...
                for (var i = 0; i < pqr.nodes.Count - 1; i++)
                {
                    var location = pqr.nodes[i];
                    RenderStraightPreviewPath(viewport, ref uiIntgamePointPacket, location);
                }

                if (!showDestinationCircle)
                {
                    return;
                }

                zoffset += pqr.nodes.Count - 1; // TODO Super weird.... (and probably just wrong...)
            }

            RenderStraightPreviewPath(viewport, ref uiIntgamePointPacket, pqr.to);
            PathRender_10109930(viewport, ref uiIntgamePointPacket);
            PathRenderEndpointCircle(viewport, pqr.to, pqr.mover, zoffset);
            uiIntgamePathdrawInited = false;
        }

        [TempleDllLocation(0x10107560)]
        private void SetCircleCenter(ref PointPacket pntPkt, LocAndOffsets loc)
        {
            pntPkt.circleCenter = loc.ToInches3D();
            pntPkt.clipStatus = PathClipStatus.Enabled;
        }

        [TempleDllLocation(0x10109930)]
        private void PathRender_10109930(IGameViewport viewport, ref PointPacket pointPkt)
        {
            RenderColoredPathSegment(viewport, pointPkt.circleCenter, ref pointPkt);
        }

        [TempleDllLocation(0x101099f0)]
        public void UiIntgameInitPathpreviewVars(PathPreviewMode mode, float greenMoveLength, float totalMoveLength)
        {
            _pathPreviewMode = mode;
            uiIntgameGreenMoveLength = greenMoveLength;
            uiIntgameTotalMoveLength = totalMoveLength;
            previousStopCumulativeDist = 0;
            uiIntgamePathpreviewFromToDist = 0;
        }

        [TempleDllLocation(0x10109a30)]
        private void RenderStraightPreviewPath(IGameViewport viewport, ref PointPacket pntPkt, LocAndOffsets nextStop)
        {
            var cumulDist = previousStopCumulativeDist;
            var newCumulDist = _previousStop.DistanceTo(nextStop) / locXY.INCH_PER_FEET + cumulDist;
            if (uiIntgameGreenMoveLength - 0.1f <= cumulDist)
            {
                if (uiIntgameTotalMoveLength - 0.1f <= cumulDist)
                {
                    RenderColoredPathSegment(viewport, ref pntPkt, nextStop);
                    pntPkt.SetMaterials(_redLineMaterial, _redLineOccludedMaterial);
                    _previousSegmentColor = PathSegmentColor.Red;
                    previousStopCumulativeDist = newCumulDist;
                    _previousStop = nextStop;
                    return;
                }

                if (uiIntgameTotalMoveLength + 0.1f >= newCumulDist)
                {
                    RenderColoredPathSegment(viewport, ref pntPkt, nextStop);
                    pntPkt.SetMaterials(_yellowLineMaterial, _yellowLineOccludedMaterial);
                    _previousSegmentColor = PathSegmentColor.Yellow;
                    previousStopCumulativeDist = newCumulDist;
                    _previousStop = nextStop;
                    return;
                }

                // Split the line into yellow and red parts
                var factor = (uiIntgameTotalMoveLength - cumulDist) / (newCumulDist - cumulDist);
                var resultLoc = LerpLocation(_previousStop, nextStop, factor);
                RenderStraightPreviewPath(viewport, ref pntPkt, resultLoc);
                RenderStraightPreviewPath(viewport, ref pntPkt, nextStop);
                return;
            }

            if (uiIntgameGreenMoveLength + 0.1f < newCumulDist)
            {
                var factor = (uiIntgameGreenMoveLength - cumulDist) / (newCumulDist - cumulDist);
                var resultLoc = LerpLocation(_previousStop, nextStop, factor);
                RenderStraightPreviewPath(viewport, ref pntPkt, resultLoc);
                RenderStraightPreviewPath(viewport, ref pntPkt, nextStop);
                return;
            }

            RenderColoredPathSegment(viewport, ref pntPkt, nextStop);
            pntPkt.SetMaterials(_greenLineMaterial, _greenLineOccludedMaterial);
            _previousSegmentColor = PathSegmentColor.Green;
            previousStopCumulativeDist = newCumulDist;
            _previousStop = nextStop;
        }

        [TempleDllLocation(0x10040e70)]
        public LocAndOffsets LerpLocation(LocAndOffsets from, LocAndOffsets to, float factor)
        {
            var distBefore = from.DistanceTo(to);
            var targetPos = Vector2.Lerp(from.ToInches2D(), to.ToInches2D(), factor);
            var result = LocAndOffsets.FromInches(targetPos);
            var distAfter = result.DistanceTo(to);
            if (distAfter > distBefore)
            {
                Debugger.Break();
            }

            return result;
        }

        [TempleDllLocation(0x101099c0)]
        private void RenderColoredPathSegment(IGameViewport viewport, ref PointPacket pntPkt, LocAndOffsets loc)
        {
            var pntNode = loc.ToInches3D();
            RenderColoredPathSegment(viewport, pntNode, ref pntPkt);
        }

        // Used to get the perpendicular normal for extending out the path line to it's full width
        private static void Rotate90Degrees(Vector3 pathDirection, out Vector3 rotated)
        {
            rotated = new Vector3(
                pathDirection.Z,
                pathDirection.Y,
                - pathDirection.X
            );
        }

        [TempleDllLocation(0x10109330)]
        private void RenderColoredPathSegment(IGameViewport viewport, Vector3 segmentEnd, ref PointPacket pntPkt)
        {
            // TODO: This function is a mess and needs work
            var points = pntPkt.points;

            if (pntPkt.clipStatus == PathClipStatus.Clipped)
            {
                segmentEnd = points[2];
            }

            if (pntPkt.clipStatus == PathClipStatus.Enabled)
            {
                if ((pntPkt.circleCenter - segmentEnd).LengthSquared() < pntPkt.circleRadius * pntPkt.circleRadius)
                {
                    pntPkt.clipStatus = PathClipStatus.Clipped;
                    var dir = Vector3.Normalize(pntPkt.circleCenter - points[2]) * (-pntPkt.circleRadius);
                    segmentEnd = pntPkt.circleCenter + dir;
                }
            }

            Vector3 a1, a3, a5, pnt2;
            if ((points[2] - segmentEnd).Length() < 0.01f)
            {
                if ((points[2] - points[1]).Length() < 0.01f)
                {
                    return;
                }

                a3 = Vector3.Normalize(points[1] - points[2]);
                Rotate90Degrees(a3, out pnt2);

                a1 = pnt2;
                a5 = a3;
            }
            else
            {
                a5 = Vector3.Normalize(points[2] - segmentEnd);
                Rotate90Degrees(a5, out a1);

                if ((points[1] - points[2]).Length() >= 0.01f)
                {
                    a3 = Vector3.Normalize(points[1] - points[2]);
                    Rotate90Degrees(a3, out pnt2);
                }
                else
                {
                    a3 = a5;
                    pnt2 = a1;

                    pntPkt.startLeft = points[1] + a1 * pntPkt.halfWidth;
                    pntPkt.startRight = points[1] - a1 * pntPkt.halfWidth;
                }
            }


            // TODO: Double-check this, might be wrong
            if (Vector3.Dot(a3, a5) <= 0)
            {
                var endLeft = points[2] + pnt2 * pntPkt.halfWidth;
                var endRight = points[2] - pnt2 * pntPkt.halfWidth;
                var v50 = pntPkt.halfWidth * -0.70700002f; // Might be cos(135)
                endLeft += a3 * v50;
                endRight += a3 * v50;
                RenderColoredPathSegment(viewport, endRight, endLeft, ref pntPkt);

                var v56 = pntPkt.halfWidth * 0.70700002f; // Might be sin(135)
                var nextStartLeft = points[2] + a1 * pntPkt.halfWidth + a5 * v56;
                var nextStartRight = points[2] - a1 * pntPkt.halfWidth + a5 * v56;
                points[1] = points[2];
                points[2] = segmentEnd;
                pntPkt.startLeft = nextStartLeft;
                pntPkt.startRight = nextStartRight;
            }
            else
            {
                var endRight = points[1] + pnt2 * pntPkt.halfWidth;
                var a2 = points[1] - pnt2 * pntPkt.halfWidth;

                var tmp = a1 * pntPkt.halfWidth;

                var tmp2 = segmentEnd + tmp;
                var tmp3 = segmentEnd - tmp;

                Vector3 endLeft;
                if (sub_10107280(endRight, a3, tmp2, a5, out endLeft))
                {
                    sub_10107280(a2, a3, tmp3, a5, out endRight);
                }
                else
                {
                    endLeft = points[2] + tmp;
                    endRight = points[2] - tmp;
                }

                RenderColoredPathSegment(viewport, endRight, endLeft, ref pntPkt);
                points[1] = points[2];
                points[2] = segmentEnd;
                pntPkt.startLeft = endLeft;
                pntPkt.startRight = endRight;
            }
        }

// TODO: LINE CROSSES !??!?!?
        [TempleDllLocation(0x10107280)]
        public bool sub_10107280(Vector3 somePos1, Vector3 a3, Vector3 somePos2, Vector3 a5, out Vector3 result)
        {
            // 2D cross product to get the determinant
            var det = a5.X * a3.Z - a5.Z * a3.X;
            if (a5.LengthSquared() * a3.LengthSquared() * 0.0001f < det * det)
            {
                // This branch is entered, when the angle is >90°
                var v9 = ((somePos2.Z - somePos1.Z) * a5.X - (somePos2.X - somePos1.X) * a5.Z) * (1.0f / det);
                result = somePos1 + v9 * a3;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        [TempleDllLocation(0x101074a0)]
        private void RenderColoredPathSegment(IGameViewport viewport, Vector3 endRight, Vector3 endLeft, ref PointPacket pkt)
        {
            Span<IntgameVertex> vertices = stackalloc IntgameVertex[4];

            vertices[0].pos.X = pkt.startLeft.X;
            vertices[0].pos.Z = pkt.startLeft.Z;
            vertices[0].uv = new Vector2(1, 0);

            vertices[1].pos.X = pkt.startRight.X;
            vertices[1].pos.Z = pkt.startRight.Z;
            vertices[1].uv = new Vector2(1, 1);

            vertices[2].pos.X = endRight.X;
            vertices[2].pos.Z = endRight.Z;
            vertices[2].uv = new Vector2(0.5f, 1);

            vertices[3].pos.X = endLeft.X;
            vertices[3].pos.Z = endLeft.Z;
            vertices[3].uv = new Vector2(0.5f, 0);

            // This is highly suspect and seems to be a leftover from 2D thinking... the
            // argument to sin is actually the tilt of the camera
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
            pkt.occludedMaterial.Bind(viewport, _device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, 6);

            // Render again with just the unoccluded part
            pkt.normalMaterial.Bind(viewport, _device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, 6);
        }

        [TempleDllLocation(0x10109be0)]
        private void PathRenderEndpointCircle(IGameViewport viewport, LocAndOffsets loc, GameObjectBody obj, float zoffset)
        {
            var radius = obj.GetRadius();
            DrawCircle3d(
                viewport,
                loc,
                zoffset,
                OccludedWaypointFillColors[_previousSegmentColor],
                OccludedWaypointOutlineColors[_pathPreviewMode],
                radius,
                true);

            DrawCircle3d(
                viewport,
                loc,
                zoffset,
                WaypointFillColors[_previousSegmentColor],
                WaypointOutlineColors[_pathPreviewMode],
                radius,
                false);
        }

        [TempleDllLocation(0x101092a0)]
        private void PointPacketInit(ref PointPacket pntPkt, float halfWidth, float circleRadius,
            LocAndOffsets loc)
        {
            pntPkt.points = new Vector3[3];
            pntPkt.points[1] = loc.ToInches3D();
            pntPkt.points[2] = loc.ToInches3D();
            pntPkt.startLeft = loc.ToInches3D();
            pntPkt.startRight = loc.ToInches3D();
            pntPkt.halfWidth = halfWidth;
            pntPkt.circleRadius = circleRadius;
            pntPkt.clipStatus = 0;
            pntPkt.normalMaterial = _greenLineMaterial.Resource;
            pntPkt.occludedMaterial = _greenLineOccludedMaterial.Resource;
        }

        private static readonly Vector4[] AooPosOffsets =
        {
            new Vector4(0.0f, 0.0f, -5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, -5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, 5.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 5.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, -10.0f, 0.0f),
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.66670001f, 0.0f, 10.0f, 0.0f),
        };

        private static readonly Vector2[] AooUvs =
        {
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
        public void DrawAttackOfOpportunityIndicator(IGameViewport viewport, LocAndOffsets from, LocAndOffsets to)
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

            DrawAooLine(viewport, ref vertices[0], ref vertices[3]); // I think this is wrong (or the duplicate one below)
            DrawAooLine(viewport, ref vertices[1], ref vertices[2]);
            DrawAooLine(viewport, ref vertices[0], ref vertices[3]);
            DrawAooLine(viewport, ref vertices[4], ref vertices[5]);
            DrawAooLine(viewport, ref vertices[5], ref vertices[6]);
            DrawAooLine(viewport, ref vertices[6], ref vertices[4]);

            _device.UpdateBuffer<IntgameVertex>(_aooVertexBuffer, vertices);
            _aooBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_aooIndexBuffer);

            _redLineMaterial.Resource.Bind(viewport, _device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertices.Length, AooIndices.Length);
        }

        private static void DrawAooLine(IGameViewport viewport, ref IntgameVertex from, ref IntgameVertex to)
        {
            var from3 = from.pos.ToVector3();
            var to3 = to.pos.ToVector3();
            Tig.ShapeRenderer3d.DrawLine(viewport, from3, to3, new PackedLinearColorA(0xFFFF0000));
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

    internal enum PathClipStatus
    {
        Disabled = 0,
        Enabled = 1,
        Clipped = 2
    }

    internal struct PointPacket
    {
        public Vector3[] points;
        public Vector3 startLeft; // points[3]
        public Vector3 startRight; // points[4]
        public float halfWidth;

        public Vector3 circleCenter;
        public float circleRadius;
        // Path is clipped against the circle if this is not Disabled
        public PathClipStatus clipStatus;

        public IMdfRenderMaterial normalMaterial;
        public IMdfRenderMaterial occludedMaterial;

        [TempleDllLocation(0x10107470)]
        public void SetMaterials(ResourceRef<IMdfRenderMaterial> normalMaterial,
            ResourceRef<IMdfRenderMaterial> occludedMaterial)
        {
            this.normalMaterial = normalMaterial.Resource;
            this.occludedMaterial = occludedMaterial.Resource;
        }
    }
}