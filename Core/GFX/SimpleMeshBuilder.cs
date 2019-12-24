using System;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.GFX
{
    public class SimpleMeshBuilder<T> where T : unmanaged
    {
        private const int InitialVerticesCapacity = 192 * 192 * 4;
        private int _verticesCapacity = InitialVerticesCapacity;
        private T[] _vertices = new T[InitialVerticesCapacity];

        private const int InitialIndicesCapacity = 192 * 192 * 6;
        private int _indicesCapacity = InitialIndicesCapacity;
        private int[] _indices = new int[InitialIndicesCapacity];

        private int _vertexCount;
        private int _indexCount;

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

        public ref T AddVertex()
        {
            int topLeftIdx = _vertexCount++;
            int topRightIdx = _vertexCount++;
            int bottomRightIdx = _vertexCount++;
            int bottomLeftIdx = _vertexCount++;
            EnsureVertexCapacity(_vertexCount);

            EnsureIndexCapacity(_indexCount + 6);
            _indices[_indexCount++] = topLeftIdx;
            _indices[_indexCount++] = bottomLeftIdx;
            _indices[_indexCount++] = bottomRightIdx;
            _indices[_indexCount++] = bottomRightIdx;
            _indices[_indexCount++] = topRightIdx;
            _indices[_indexCount++] = topLeftIdx;

            return ref _vertices[_vertexCount];
        }

        public SimpleMesh Build(VertexShader shader)
        {
            if (_vertexCount == 0 || _indexCount == 0)
            {
                return new SimpleMesh(null, null, 0, 0);
            }

            var vertices = _vertices.AsSpan(0, _vertexCount);
            using var vertexBuffer = Tig.RenderingDevice.CreateVertexBuffer<T>(vertices);
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