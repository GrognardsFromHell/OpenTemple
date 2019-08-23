using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui.InGameSelect
{
    public class ConeRenderer
    {

        private readonly RenderingDevice _device;

        private readonly ResourceRef<VertexBuffer> _fanVertexBuffer;
        private readonly ResourceRef<IndexBuffer> _fanIndexBuffer;
        private readonly ResourceRef<BufferBinding> _fanBufferBinding;

        private readonly ResourceRef<VertexBuffer> _ringVertexBuffer;
        private readonly ResourceRef<IndexBuffer> _ringIndexBuffer;
        private readonly ResourceRef<BufferBinding> _ringBufferBinding;

        private readonly ResourceRef<VertexBuffer> _flankVertexBuffer;
        private readonly ResourceRef<IndexBuffer> _flankIndexBuffer;
        private readonly ResourceRef<BufferBinding> _flankBufferBinding;

        private ushort[] FanIndices =
        {
            0, 1, 2, 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 6, 0, 6, 7, 0, 7, 8, 0, 8, 9, 0, 9, 10, 0, 10, 11, 0, 11, 12, 0,
            12, 13, 0, 13, 14, 0, 14, 15, 0, 15, 16, 0, 16, 17, 0, 17, 18, 0, 18, 19
        };

        private ushort[] RingIndices =
        {
            0, 1, 2, 1, 3, 2, 2, 3, 4, 3, 5, 4, 4, 5, 6, 5, 7, 6, 6, 7, 8, 7, 9, 8, 8, 9, 10, 9, 11, 10, 10, 11, 12, 11,
            13, 12, 12, 13, 14, 13, 15, 14, 14, 15, 16, 15, 17, 16, 16, 17, 18, 17, 19, 18, 18, 19, 20, 19, 21, 20, 20,
            21, 22, 21, 23, 22, 22, 23, 24, 23,
            25, 24, 24, 25, 26, 25, 27, 26, 26, 27, 28, 27, 29, 28, 28, 29, 30, 29, 31, 30, 30, 31, 32, 31, 33, 32, 32,
            33, 34, 33, 35, 34, 34, 35, 36, 35, 37, 36,
        };

        private ushort[] FlankIndices =
        {
            0, 1, 2, 1, 3, 2, 2, 3, 4,
            5, 6, 7, 6, 8, 7, 7, 8, 9,
        };

        public ConeRenderer(RenderingDevice device)
        {
            _device = device;
            _fanIndexBuffer = device.CreateIndexBuffer(FanIndices);
            _fanVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 20);
            _fanBufferBinding = device.CreateMdfBufferBinding().Ref();
            _fanBufferBinding.Resource.AddBuffer<IntgameVertex>(_fanVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _ringIndexBuffer = device.CreateIndexBuffer(RingIndices);
            _ringVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 38);
            _ringBufferBinding = device.CreateMdfBufferBinding().Ref();
            _ringBufferBinding.Resource.AddBuffer<IntgameVertex>(_ringVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

            _flankIndexBuffer = device.CreateIndexBuffer(FlankIndices);
            _flankVertexBuffer = device.CreateEmptyVertexBuffer(IntgameVertex.Size * 10);
            _flankBufferBinding = device.CreateMdfBufferBinding().Ref();
            _flankBufferBinding.Resource.AddBuffer<IntgameVertex>(_flankVertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

        }

        [TempleDllLocation(0x10107920)]
        public void Render(LocAndOffsets loc, LocAndOffsets tgtLoc,
            float degreesTarget,
            IMdfRenderMaterial materialInside,
            IMdfRenderMaterial materialOutside)
        {
            Span<IntgameVertex> vertices = stackalloc IntgameVertex[38];

            var elevation = MathF.Sin(-0.77539754f) * -1.5f;
            var outerConeAngle = Angles.ToRadians(degreesTarget);

            var originPos = loc.ToInches3D();

            var targetPos = tgtLoc.ToInches3D();

            var d = targetPos - originPos;
            var coneRadius = d.Length();
            // The normal pointing in the direction of the target straight down the middle of the cone
            var coneNormal = d / coneRadius;

            // A portion of the full cone will be rendered as a "rim" on the outside, which
            // will have this radius:
            var ringRadius = MathF.Min(24.0f, coneRadius / 7.0f);

            var innerConeRadius = coneRadius - ringRadius;

            // Set the origin point
            vertices[0].pos = new Vector4(originPos.X, elevation, originPos.Z, 1);
            vertices[0].normal = Vector4.UnitY;
            vertices[0].uv = new Vector2(0, 0);
            vertices[0].diffuse = PackedLinearColorA.White;

            var innerConeAngle = (1.0f - (ringRadius + ringRadius) / (outerConeAngle * coneRadius)) * outerConeAngle;
            var innerConeAngleHalf = innerConeAngle * 0.5f;
            float angleStepInner = innerConeAngle / 18;

            // This is the point at the center of the cone, but on the inner rim
            var coneNearX = innerConeRadius * coneNormal.X;
            var coneNearY = innerConeRadius * coneNormal.Z;

            for (var i = 1; i < 20; i++)
            {
                // angle=0 is straight down the center of the cone, so it is shifted to the left by coneAngle/2
                var angle = (i - 1) * angleStepInner - innerConeAngleHalf;
                vertices[i].pos = new Vector4(
                    originPos.X + MathF.Cos(angle) * coneNearX - MathF.Sin(angle) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(angle) * coneNearX + MathF.Cos(angle) * coneNearY,
                    1
                );

                vertices[i].normal = Vector4.UnitY;
                vertices[i].diffuse = PackedLinearColorA.White;
                vertices[i].uv = new Vector2(
                    (i + 1) % 2,
                    1.0f
                );
            }

            _fanVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 20));
            _fanBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_fanIndexBuffer);
            materialInside.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, 20, FanIndices.Length);

            var slice = 0.0f;
            // Far center point of the cone, on the outside rim
            var coneFarX = coneRadius * coneNormal.X;
            var coneFarY = coneRadius * coneNormal.Z;

            for (var i = 0; i < 38; i += 2)
            {
                var angle = slice * angleStepInner - innerConeAngleHalf;

                vertices[i].pos = new Vector4(
                    originPos.X + MathF.Cos(angle) * coneNearX - MathF.Sin(angle) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(angle) * coneNearX + MathF.Cos(angle) * coneNearY,
                    1
                );
                vertices[i].normal = Vector4.UnitY;
                vertices[i].uv = new Vector2(slice, 0);
                vertices[i].diffuse = PackedLinearColorA.White;

                vertices[i + 1].pos = new Vector4(
                    originPos.X + MathF.Cos(angle) * coneFarX - MathF.Sin(angle) * coneFarY,
                    elevation,
                    originPos.Z + MathF.Cos(angle) * coneFarY + MathF.Sin(angle) * coneFarX,
                    1
                );
                vertices[i + 1].normal = Vector4.UnitY;
                vertices[i + 1].uv = new Vector2(slice, 1);
                vertices[i + 1].diffuse = PackedLinearColorA.White;

                slice += 1;
            }

            _ringVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 38));
            _ringBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_ringIndexBuffer);
            materialOutside.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, 38, RingIndices.Length);

            // Render the left and right side of the cone using the same texture as the outside ring

            var innerConeAngleHalfInv = - innerConeAngleHalf;
            var outerConeAngleHalf = outerConeAngle * 0.5f;
            var outerConeAngleHalfInv = - outerConeAngleHalf;
            var v71 = innerConeRadius / ringRadius;

            // Right flank of the cone
            vertices[0] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(innerConeAngleHalfInv) * coneFarX - MathF.Sin(innerConeAngleHalfInv) * coneFarY,
                    elevation,
                    originPos.Z + MathF.Sin(innerConeAngleHalfInv) * coneFarX + MathF.Cos(innerConeAngleHalfInv) * coneFarY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[1] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(outerConeAngleHalfInv) * coneFarX - MathF.Sin(outerConeAngleHalfInv) * coneFarY,
                    elevation,
                    originPos.Z + MathF.Sin(outerConeAngleHalfInv) * coneFarX + MathF.Cos(outerConeAngleHalfInv) * coneFarY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[2] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(innerConeAngleHalfInv) * coneNearX - MathF.Sin(innerConeAngleHalfInv) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(innerConeAngleHalfInv) * coneNearX + MathF.Cos(innerConeAngleHalfInv) * coneNearY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[3] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(outerConeAngleHalfInv) * coneNearX - MathF.Sin(outerConeAngleHalfInv) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(outerConeAngleHalfInv) * coneNearX + MathF.Cos(outerConeAngleHalfInv) * coneNearY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[4] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X,
                    elevation,
                    originPos.Z,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(-v71, 0.5f),
                diffuse = PackedLinearColorA.White
            };

            // Left flank of the cone
            vertices[5] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(innerConeAngleHalf) * coneFarX - MathF.Sin(innerConeAngleHalf) * coneFarY,
                    elevation,
                    originPos.Z + MathF.Sin(innerConeAngleHalf) * coneFarX + MathF.Cos(innerConeAngleHalf) * coneFarY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[6] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(outerConeAngleHalf) * coneFarX - MathF.Sin(outerConeAngleHalf) * coneFarY,
                    elevation,
                    originPos.Z + MathF.Sin(outerConeAngleHalf) * coneFarX + MathF.Cos(outerConeAngleHalf) * coneFarY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            vertices[7] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(innerConeAngleHalf) * coneNearX - MathF.Sin(innerConeAngleHalf) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(innerConeAngleHalf) * coneNearX + MathF.Cos(innerConeAngleHalf) * coneNearY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 0),
                diffuse = PackedLinearColorA.White
            };
            vertices[8] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X + MathF.Cos(outerConeAngleHalf) * coneNearX - MathF.Sin(outerConeAngleHalf) * coneNearY,
                    elevation,
                    originPos.Z + MathF.Sin(outerConeAngleHalf) * coneNearX + MathF.Cos(outerConeAngleHalf) * coneNearY,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(0, 1),
                diffuse = PackedLinearColorA.White
            };
            // This could reuse vertices[4], but was here in vanilla also
            vertices[9] = new IntgameVertex
            {
                pos = new Vector4(
                    originPos.X,
                    elevation,
                    originPos.Z,
                    1
                ),
                normal = Vector4.UnitY,
                uv = new Vector2(v71, 0.5f),
                diffuse = PackedLinearColorA.White
            };

            _flankVertexBuffer.Resource.Update<IntgameVertex>(vertices.Slice(0, 10));
            _flankBufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_flankIndexBuffer);
            materialOutside.Bind(_device, new List<Light3d>());
            _device.DrawIndexed(PrimitiveType.TriangleList, 10, FlankIndices.Length);
        }
    }
}