using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.InGameSelect
{
    public class PickerAreaRenderer
    {
        private readonly RenderingDevice _device;

        private readonly ResourceRef<VertexBuffer> _fanVertexBuffer;
        private readonly ResourceRef<VertexBuffer> _ringVertexBuffer;
        private ResourceRef<IndexBuffer> _fanIndexBuffer;
        private ResourceRef<IndexBuffer> _ringIndexBuffer;
        private ResourceRef<BufferBinding> _fanBufferBinding;
        private ResourceRef<BufferBinding> _ringBufferBinding;

        // Indices used to draw the inner circle (as a triangle fan, essentially)
        private static readonly ushort[] FanIndices =
        {
            0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7, 0, 7, 8, 0, 8, 9, 0, 9, 10, 0, 10, 11, 0, 11, 12, 0,
            12, 13, 0, 13, 14, 0, 14, 15, 0, 15, 16, 0, 16, 17, 0, 17, 18, 0, 18, 19, 0, 19, 20, 0, 20, 21, 0, 21, 22,
            0, 22, 23, 0, 23, 24, 0, 24, 25, 0, 25, 26, 0, 26, 27, 0, 27, 28, 0, 28, 29, 0, 29, 30, 0, 30, 31, 0, 31,
            32, 0, 32, 33, 0, 33, 34, 0, 34, 35, 0, 35, 36, 0, 36, 37, 0, 37, 38,
        };
        // Indices used to draw the outer ring (using quads)
        private static readonly ushort[] RingIndices =
        {
            0, 1, 2, 1, 3, 2, 2, 3, 4, 3, 5, 4, 4, 5, 6, 5, 7, 6, 6, 7, 8, 7, 9, 8, 8, 9, 10, 9, 11, 10, 10, 11, 12, 11, 13, 12, 12, 13, 14, 13, 15, 14, 14, 15, 16, 15, 17, 16, 16, 17, 18, 17, 19, 18, 18, 19, 20, 19, 21, 20, 20, 21, 22, 21, 23, 22, 22, 23, 24, 23,
            25, 24, 24, 25, 26, 25, 27, 26, 26, 27, 28, 27, 29, 28, 28, 29, 30, 29, 31, 30, 30, 31, 32, 31, 33, 32, 32, 33, 34, 33, 35, 34, 34, 35, 36, 35, 37, 36, 36, 37, 38, 37, 39, 38, 38, 39, 40, 39, 41, 40, 40, 41, 42, 41, 43, 42, 42, 43, 44, 43, 45, 44, 44,
            45, 46, 45, 47, 46, 46, 47, 48, 47, 49, 48, 48, 49, 50, 49, 51, 50, 50, 51, 52, 51, 53, 52, 52, 53, 54, 53, 55, 54, 54, 55, 56, 55, 57, 56, 56, 57, 58, 57, 59, 58, 58, 59, 60, 59, 61, 60, 60, 61, 62, 61, 63, 62, 62, 63, 64, 63, 65, 64, 64, 65, 66, 65,
            67, 66, 66, 67, 68, 67, 69, 68, 68, 69, 70, 69, 71, 70, 70, 71, 72, 71, 73, 72, 72, 73, 74, 73, 75, 74
        };

        public PickerAreaRenderer(RenderingDevice device)
        {
            _device = device;
            _fanIndexBuffer = device.CreateIndexBuffer(FanIndices);
            _fanVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 39, debugName:"PickerAreaFan");
            _fanBufferBinding = device.CreateMdfBufferBinding().Ref();
            _fanBufferBinding.Resource.AddBuffer<IntgameVertex>(_fanVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _ringIndexBuffer = device.CreateIndexBuffer(RingIndices);
            _ringVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 76, debugName:"PickerAreaRing");
            _ringBufferBinding = device.CreateMdfBufferBinding().Ref();
            _ringBufferBinding.Resource.AddBuffer<IntgameVertex>(_ringVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        private const float AnglePerSlice = 0.16981582f; // 2 * MathF.PI / 37 (because of 38 slices)

        [TempleDllLocation(0x10107610)]
        public void Render(Vector3 centerPos,
            float factor,
            float radiusInch,
            IMdfRenderMaterial innerMaterial,
            IMdfRenderMaterial outerMaterial)
        {
            Span<IntgameVertex> vertices = stackalloc IntgameVertex[78];

            var elevation = -(MathF.Sin(-0.77539754f) * factor);

            var outerBorderRadius = Math.Min(24.0f, radiusInch / 7.0f);
            var innerRadius = radiusInch - outerBorderRadius;

            // Set up the center of the circle position
            vertices[0].uv = Vector2.Zero;
            vertices[0].normal = Vector4.UnitY;
            vertices[0].diffuse = PackedLinearColorA.White;
            vertices[0].pos = new Vector4(
                centerPos,
                1
            );

            for (var i = 1; i < 39; i++)
            {
                ref var vertex = ref vertices[i];

                var angle = i * AnglePerSlice;

                vertex.pos = new Vector4(
                    centerPos.X + MathF.Cos(angle) * innerRadius - MathF.Sin(angle) * 0.0f,
                    centerPos.Y + elevation,
                    centerPos.Z + MathF.Cos(angle) * 0.0f + MathF.Sin(angle) * innerRadius,
                    1
                );
                // Vanilla used a borked normal of (undefined, 0, 1, undefined) here
                vertex.normal = Vector4.UnitY;
                vertex.diffuse = PackedLinearColorA.White;
                vertex.uv = new Vector2(
                    1.0f - (i % 2),
                    1.0f
                );
            }

            _fanVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 39));
            _fanBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_fanIndexBuffer);

            innerMaterial.Bind(_device, new List<Light3d>());

            _device.DrawIndexed(PrimitiveType.TriangleList, 39, FanIndices.Length);

            // We're generating 2 more than we need because it'll close the loop, but the loop will be closed
            // automatically by the quad-stripe
            for (var i = 0; i < 78; i += 2)
            {
                var angle = i * AnglePerSlice;
                ref var vertex1 = ref vertices[i];
                vertex1.pos = new Vector4(
                    centerPos.X + MathF.Cos(angle) * innerRadius,
                    centerPos.Y + elevation,
                    centerPos.Z + MathF.Sin(angle) * innerRadius,
                    1
                );
                vertex1.normal = Vector4.UnitY;
                vertex1.uv = new Vector2(i / 2.0f, 0);
                vertex1.diffuse = PackedLinearColorA.White;

                ref var vertex2 = ref vertices[i + 1];
                vertex2.pos = new Vector4(
                    centerPos.X + MathF.Cos(angle) * radiusInch,
                    centerPos.Y + elevation,
                    centerPos.Z + MathF.Sin(angle) * radiusInch,
                    1
                );
                vertex2.normal = Vector4.UnitY;
                vertex2.uv = new Vector2(i / 2.0f, 1);
                vertex2.diffuse = PackedLinearColorA.White;
            }

            _ringVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 76));
            _ringBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_ringIndexBuffer);

            outerMaterial.Bind(_device, new List<Light3d>());

            _device.DrawIndexed(PrimitiveType.TriangleList, 76, RingIndices.Length);
        }
    }
}