using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Pathfinding;

namespace OpenTemple.Core.Ui.InGameSelect
{
    public class RectangleRenderer : IDisposable
    {
        private readonly RenderingDevice _device;

        private readonly ResourceRef<VertexBuffer> _innerVertexBuffer;
        private readonly ResourceRef<IndexBuffer> _innerIndexBuffer;
        private readonly ResourceRef<BufferBinding> _innerBufferBinding;

        private readonly ResourceRef<VertexBuffer> _outerVertexBuffer;
        private readonly ResourceRef<IndexBuffer> _outerIndexBuffer;
        private readonly ResourceRef<BufferBinding> _outerBufferBinding;

        private static readonly ushort[] QuadIndices =
        {
            0, 1, 2, 1, 3, 2
        };

        private static readonly ushort[] InnerIndices =
        {
            0, 1, 2, 1, 3, 2, 2, 3, 4, 3, 5, 4, 4, 5, 6, 5, 7, 6, 6, 7, 8, 7, 9, 8, 8, 9, 10, 9, 11, 10, 10, 11, 12, 11,
            13, 12, 12, 13, 14, 13, 15, 14, 14, 15, 16, 15, 17, 16, 16, 17, 18, 17, 19, 18, 18, 19, 20, 19, 21, 20
        };

        private static readonly ushort[] OuterIndices =
            InnerIndices.Concat(InnerIndices.Select(i => (ushort) (i + 22)))
                .Concat(QuadIndices.Select(i => (ushort) (i + 44)))
                .Concat(QuadIndices.Select(i => (ushort) (i + 48)))
                .ToArray();

        public RectangleRenderer(RenderingDevice device)
        {
            _device = device;
            _innerIndexBuffer = device.CreateIndexBuffer(InnerIndices);
            _innerVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 22, debugName:"RectangleInner");
            _innerBufferBinding = device.CreateMdfBufferBinding().Ref();
            _innerBufferBinding.Resource.AddBuffer<IntgameVertex>(_innerVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _outerIndexBuffer = device.CreateIndexBuffer(OuterIndices);
            _outerVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 52, debugName:"RectangleOuter");
            _outerBufferBinding = device.CreateMdfBufferBinding().Ref();
            _outerBufferBinding.Resource.AddBuffer<IntgameVertex>(_outerVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        }

        [TempleDllLocation(0x10108340)]
        public void Render(IGameViewport viewport, Vector3 srcPos, Vector3 tgtPos, float rayThickness, float minRange,
            float maxRange, IMdfRenderMaterial materialInside, IMdfRenderMaterial materialOutside)
        {
            float v11;
            float v69;
            float v70;
            float v71;

            Span<IntgameVertex> vertices = stackalloc IntgameVertex[52];

            var elevation = MathF.Sin(-0.77539754f) * -1.5f;

            var srcAbsX = srcPos.X;
            var srcAbsY = srcPos.Z;

            var tgtAbsX = tgtPos.X;
            var tgtAbsY = tgtPos.Z;

            var deltaX = tgtAbsX - srcAbsX;
            var dettaY = tgtAbsY - srcAbsY;
            float dist = MathF.Sqrt(dettaY * dettaY + deltaX * deltaX);
            float normalizer = 1.0f / dist;
            var xHat = normalizer * deltaX;
            var yHat = normalizer * dettaY;
            var v62 = -xHat;
            if (dist < minRange)
            {
                dist = minRange;
                tgtAbsX = xHat * minRange + srcAbsX;
                v11 = yHat * minRange;
                tgtAbsY = v11 + srcAbsY;
            }
            else if (dist > maxRange)
            {
                dist = maxRange;
                tgtAbsX = xHat * maxRange + srcAbsX;
                v11 = yHat * maxRange;
                tgtAbsY = v11 + srcAbsY;
            }

            var v12 = dist / 7.0f;
            var v59 = v12;
            if (v12 > 24.0f)
            {
                v12 = 24.0f;
            }

            float v56 = rayThickness * 0.25f;
            if (v12 > v56)
            {
                v12 = v56;
            }

            var v16 = rayThickness - v12;
            var v57 = v16;
            if (dist - v12 * 3.0 < v57)
            {
                v57 = v59;
            }

            var v61 = v16;
            var v72 = rayThickness / v61;
            for (var i = 0; i < 22; i += 2)
            {
                float v21 = MathF.Cos((float) (i / 2) * 0.31415927f);
                var v22 = v21 * v61;
                v70 = yHat * v22;
                var v23 = v22;
                var v24 = MathF.Sqrt(v72 * v72 - v21 * v21) * v57;
                v71 = v24;

                v69 = v23 * v62;
                float v25 = v21 * rayThickness / locXY.INCH_PER_FEET;

                vertices[i] = new IntgameVertex
                {
                    pos = new Vector4(
                        srcAbsX + v70 + v24 * xHat,
                        elevation,
                        srcAbsY + v69 + v71 * yHat,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v25, 0),
                    diffuse = PackedLinearColorA.White
                };

                vertices[i+1] = new IntgameVertex
                {
                    pos = new Vector4(
                        tgtAbsX + v70 - xHat * v12,
                        elevation,
                        tgtAbsY + v69 - v12 * yHat,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v25, 1.0f),
                    diffuse = PackedLinearColorA.White
                };
            }

            _innerVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 22));
            _innerBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_innerIndexBuffer);
            materialInside.Bind(viewport, _device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, 22, InnerIndices.Length);

            // Note: Unlike Vanilla, we'll pack the entire rest into a single VB and draw that

            for (var i = 0; i < 22; i += 2)
            {
                float v28 = MathF.Cos((float) (i / 2) * 0.31415927f);
                var v29 = v28 * v61;
                var v30 = v29;
                var v31 = MathF.Sqrt(1.0f - v28 * v28) * v57;
                var v32 = v30 * v62;
                var v33 = v28 * rayThickness * 0.083333336f;
                var v34 = MathF.Sqrt(v72 * v72 - v28 * v28) * v57;

                vertices[i] = new IntgameVertex
                {
                    pos = new Vector4(
                        v31 * xHat + yHat * v29 + srcAbsX,
                        elevation,
                        v31 * yHat + v32 + srcAbsY,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v33, 0),
                    diffuse = PackedLinearColorA.White
                };

                vertices[i+1] = new IntgameVertex
                {
                    pos = new Vector4(
                        xHat * v34 + yHat * v29 + srcAbsX,
                        elevation,
                        v34 * yHat + v32 + srcAbsY,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v33, 1.0f),
                    diffuse = PackedLinearColorA.White
                };
            }

            for (var i = 0; i < 22; i += 2)
            {
                float v39 = MathF.Cos(i / 2 * 0.31415927f);
                var v40 = v61 * v39;
                var v41 = yHat * v40;
                var v42 = v41;
                var v43 = v40 * v62;
                v72 = v43;
                float v44 = v39 * rayThickness * 0.083333336f;
                vertices[22 + i] = new IntgameVertex
                {
                    pos = new Vector4(
                        v41 - xHat * v12 + tgtAbsX,
                        elevation,
                        v43 - v12 * yHat + tgtAbsY,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v44, 0),
                    diffuse = PackedLinearColorA.White
                };

                vertices[22 + i+1] = new IntgameVertex
                {
                    pos = new Vector4(
                        v42 + tgtAbsX,
                        elevation,
                        v72 + tgtAbsY,
                        1
                    ),
                    normal = Vector4.UnitY,
                    uv = new Vector2(v44, 1.0f),
                    diffuse = PackedLinearColorA.White
                };
            }

            var vertexIdx = 44;
            var v45 = v61 * yHat;
            var v46 = v45;
            var v47 = v61 * v62;
            var v48 = v47;
            float v49 = dist * 0.041666668f;
            var v50 = v49;
            var v51 = yHat * rayThickness;
            var v52 = v51;
            var v53 = v62 * rayThickness;
            var v54 = v53;

            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    v45 + srcAbsX,
                    elevation,
                    v47 + srcAbsY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    v46 + tgtAbsX,
                    elevation,
                    v48 + tgtAbsY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(v49, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    v51 + srcAbsX,
                    elevation,
                    v53 + srcAbsY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    v52 + tgtAbsX,
                    elevation,
                    v54 + tgtAbsY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(v49, 1),
                diffuse = PackedLinearColorA.White
            };

            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    srcAbsX - v46,
                    elevation,
                    srcAbsY - v48,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    tgtAbsX - v46,
                    elevation,
                    tgtAbsY - v48,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(v50, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    srcAbsX - v52,
                    elevation,
                    srcAbsY - v54,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[vertexIdx++] = new IntgameVertex
            {
                pos = new Vector4(
                    tgtAbsX - v52,
                    elevation,
                    tgtAbsY - v54,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(v50, 1),
                diffuse = PackedLinearColorA.White
            };

            _outerVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, vertexIdx));
            _outerBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_outerIndexBuffer);
            materialOutside.Bind(viewport, _device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, vertexIdx, OuterIndices.Length);
        }

        public void Dispose()
        {
        }
    }
}