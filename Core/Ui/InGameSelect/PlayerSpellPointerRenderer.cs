using System;
using System.Collections.Generic;
using System.Numerics;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Systems.Pathfinding;

namespace SpicyTemple.Core.Ui.InGameSelect
{
    /// <summary>
    /// Renders the little circle with a pointer below the caster, which points in the direction of the spell target.
    /// </summary>
    public class PlayerSpellPointerRenderer : IDisposable
    {
        private static readonly IntgameVertex[] Vertices =
        {
            new IntgameVertex
            {
                pos = new Vector4(-1.0f, 0.0f, 1.0f, 0.0f),
                normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                uv = new Vector2(0.0f, 1.0f),
                diffuse = PackedLinearColorA.White
            },
            new IntgameVertex
            {
                pos = new Vector4(1.0f, 0.0f, 1.0f, 0.0f),
                normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                uv = new Vector2(1.0f, 1.0f),
                diffuse = PackedLinearColorA.White
            },
            new IntgameVertex
            {
                pos = new Vector4(-1.0f, 0.0f, -1.0f, 0.0f),
                normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                uv = new Vector2(0.0f, 0.0f),
                diffuse = PackedLinearColorA.White
            },
            new IntgameVertex
            {
                pos = new Vector4(1.0f, 0.0f, -1.0f, 0.0f),
                normal = new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
                uv = new Vector2(1.0f, 0.0f),
                diffuse = PackedLinearColorA.White
            }
        };

        private static readonly ushort[] Indices =
        {
            0, 1, 2, 1, 3, 2
        };

        private readonly RenderingDevice _device;

        private readonly ResourceRef<VertexBuffer> _vertexBuffer;
        private ResourceRef<BufferBinding> _bufferBinding;
        private ResourceRef<IndexBuffer> _indexBuffer;

        public PlayerSpellPointerRenderer(RenderingDevice device)
        {
            _device = device;
            _indexBuffer = device.CreateIndexBuffer(Indices);
            _vertexBuffer = device.CreateVertexBuffer<IntgameVertex>(Vertices, false);

            _bufferBinding = device.CreateMdfBufferBinding().Ref();
            _bufferBinding.Resource.AddBuffer<IntgameVertex>(_vertexBuffer.Resource, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _bufferBinding.Dispose();
        }

        [TempleDllLocation(0x10106d10)]
        public void Render(Vector3 centerPos, float radius, float direction, IMdfRenderMaterial material)
        {
            var tilt = MathF.Cos(2.3561945f) * (MathF.Cos(-0.77539754f) * -1.5f);
            var elevation = MathF.Sin(-0.77539754f) * -1.5f;

            // Texture is rotated not quite as we want it
            direction += MathF.PI / 2;

            var directionSin = MathF.Sin(direction);
            var directionCos = MathF.Cos(direction);

            Span<IntgameVertex> vertices = stackalloc IntgameVertex[Vertices.Length];
            Vertices.CopyTo(vertices);

            for (var i = 0; i < Vertices.Length; i++)
            {
                ref var pos = ref vertices[i].pos;

                pos.Y = elevation;

                var newX = (directionSin * pos.Z + directionCos * pos.X) * radius;
                var newZ = (directionCos * pos.Z - directionSin * pos.X) * radius;

                pos.X = tilt + newX + centerPos.X;
                pos.Z = tilt + newZ + centerPos.Z;
            }

            _vertexBuffer.Resource.Update<IntgameVertex>(vertices);

            _bufferBinding.Resource.Bind();
            _device.SetIndexBuffer(_indexBuffer);

            material.Bind(_device, new List<Light3d>());

            _device.DrawIndexed(
                PrimitiveType.TriangleList,
                vertices.Length,
                Indices.Length);
        }
    }
}