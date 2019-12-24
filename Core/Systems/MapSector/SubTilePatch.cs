using System;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.MapSector
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {
        public Vector4 pos;
        public PackedLinearColorA diffuse;
        public static readonly int Size = Marshal.SizeOf<Vertex>();
    }

    public class SubTileMeshBuilder
    {
        private const int InitialVerticesCapacity = 192 * 192 * 4;
        private int _verticesCapacity = InitialVerticesCapacity;
        private Vertex[] _vertices = new Vertex[InitialVerticesCapacity];

        private const int InitialIndicesCapacity = 192 * 192 * 6;
        private int _indicesCapacity = InitialIndicesCapacity;
        private int[] _indices = new int[InitialIndicesCapacity];

        private readonly Vector2 _basePos;

        private int _vertexCount;
        private int _indexCount;

        public SubTileMeshBuilder(SectorLoc sector)
        {
            _basePos = sector.GetBaseTile().ToInches2D();
        }

        private void EnsureVertexCapacity(int capacity)
        {
            if (capacity > _verticesCapacity)
            {
                _verticesCapacity = (_verticesCapacity * 125 / 100);
                Array.Resize(ref _vertices, _verticesCapacity);
            }
        }

        private void EnsureIndexCapacity(int capacity)
        {
            if (capacity > _indicesCapacity)
            {
                _indicesCapacity = (_indicesCapacity * 125 / 100);
                Array.Resize(ref _indices, _indicesCapacity);
            }
        }

        public void Add(int x, int y, PackedLinearColorA color)
        {
            int topLeftIdx = _vertexCount++;
            int topRightIdx = _vertexCount++;
            int bottomRightIdx = _vertexCount++;
            int bottomLeftIdx = _vertexCount++;
            EnsureVertexCapacity(_vertexCount);

            float subtileX = _basePos.X + x * locXY.INCH_PER_SUBTILE;
            float subtileY = _basePos.Y + y * locXY.INCH_PER_SUBTILE;

            _vertices[topLeftIdx] = new Vertex
            {
                diffuse = color,
                pos = new Vector4(subtileX, 0, subtileY, 1)
            };
            _vertices[topRightIdx] = new Vertex
            {
                diffuse = color,
                pos = new Vector4(subtileX + locXY.INCH_PER_SUBTILE, 0, subtileY, 1)
            };
            _vertices[bottomRightIdx] = new Vertex
            {
                diffuse = color,
                pos = new Vector4(subtileX + locXY.INCH_PER_SUBTILE, 0,
                    subtileY + locXY.INCH_PER_SUBTILE, 1)
            };
            _vertices[bottomLeftIdx] = new Vertex
            {
                diffuse = color,
                pos = new Vector4(subtileX, 0, subtileY + locXY.INCH_PER_SUBTILE, 1)
            };

            EnsureIndexCapacity(_indexCount + 6);
            _indices[_indexCount++] = topLeftIdx;
            _indices[_indexCount++] = bottomLeftIdx;
            _indices[_indexCount++] = bottomRightIdx;
            _indices[_indexCount++] = bottomRightIdx;
            _indices[_indexCount++] = topRightIdx;
            _indices[_indexCount++] = topLeftIdx;
        }

        public SimpleMesh Build(VertexShader shader)
        {
            if (_vertexCount == 0 || _indexCount == 0)
            {
                return new SimpleMesh(null, null, 0, 0);
            }

            var vertices = _vertices.AsSpan(0, _vertexCount);
            using var vertexBuffer = Tig.RenderingDevice.CreateVertexBuffer<Vertex>(vertices);
            var indices = _indices.AsSpan(0, _indexCount);
            using var indexBuffer = Tig.RenderingDevice.CreateIndexBuffer(indices);
            using var bufferBinding = new BufferBinding(Tig.RenderingDevice, shader).Ref();
            bufferBinding.Resource.AddBuffer<Vertex>(vertexBuffer, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color);

            return new SimpleMesh(indexBuffer, bufferBinding, _vertexCount, _indexCount);
        }
    }
}