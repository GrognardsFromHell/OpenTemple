using System;
using System.Buffers;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.GFX;

public ref struct SimpleMeshBuilder<T> where T : unmanaged, IVertexFormat
{
    private readonly SimpleMeshBuilderType _type;

    private T[]? _vertices;
    private int[]? _indices;

    private int _vertexCount;
    private int _indexCount;

    public static SimpleMeshBuilder<T> Quads(int primitiveCapacity)
    {
        return new SimpleMeshBuilder<T>(SimpleMeshBuilderType.Quads, primitiveCapacity);
    }

    public static SimpleMeshBuilder<T> Triangles(int primitiveCapacity)
    {
        return new SimpleMeshBuilder<T>(SimpleMeshBuilderType.Triangles, primitiveCapacity);
    }

    private SimpleMeshBuilder(SimpleMeshBuilderType type, int primitiveCapacity)
    {
        _type = type;
        EnsureIndexCapacity(primitiveCapacity * IndicesPerPrimitive, true);
        EnsureVertexCapacity(primitiveCapacity * VerticesPerPrimitive, true);
    }

    private void EnsureVertexCapacity(int capacity, bool exact = false)
    {
        EnsureCapacity(ref _vertices, _vertexCount, capacity, exact);
    }

    private int[] EnsureIndexCapacity(int capacity, bool exact = false)
    {
        return EnsureCapacity(ref _indices, _indexCount, capacity, exact);
    }

    private static TI[] EnsureCapacity<TI>(ref TI[] array, int currentCount, int capacity, bool exact)
    {
        if (array == null || capacity > array.Length)
        {
            var oldArray = array;
            if (!exact)
            {
                capacity = capacity * 125 / 100;
            }

            array = ArrayPool<TI>.Shared.Rent(capacity);

            if (oldArray != null)
            {
                oldArray.AsSpan(0, currentCount).CopyTo(array);
                ArrayPool<TI>.Shared.Return(oldArray);
            }
        }

        return array;
    }

    private int VerticesPerPrimitive => _type switch
    {
        SimpleMeshBuilderType.Quads => 4,
        SimpleMeshBuilderType.Triangles => 3,
        _ => throw new ArgumentOutOfRangeException()
    };

    private int IndicesPerPrimitive => _type switch
    {
        SimpleMeshBuilderType.Quads => 6,
        SimpleMeshBuilderType.Triangles => 3,
        _ => throw new ArgumentOutOfRangeException()
    };

    public ref T Vertex()
    {
        var idx = _vertexCount++;
        var idxInPrimitive = idx % VerticesPerPrimitive;

        if (idxInPrimitive == 0)
        {
            // Starting a new primitive
            EnsureVertexCapacity(_vertexCount + VerticesPerPrimitive);
        }
        else if (idxInPrimitive == VerticesPerPrimitive - 1)
        {
            var indices = EnsureIndexCapacity(_indexCount + IndicesPerPrimitive);

            // Last vertex in primitive, build the indices now
            if (_type == SimpleMeshBuilderType.Quads)
            {
                var topLeftIdx = idx - 3;
                var topRightIdx = idx - 2;
                var bottomRightIdx = idx - 1;
                var bottomLeftIdx = idx;
                indices[_indexCount++] = topLeftIdx;
                indices[_indexCount++] = topRightIdx;
                indices[_indexCount++] = bottomRightIdx;
                indices[_indexCount++] = bottomRightIdx;
                indices[_indexCount++] = bottomLeftIdx;
                indices[_indexCount++] = topLeftIdx;
            }
            else
            {
                indices[_indexCount++] = idx - 2;
                indices[_indexCount++] = idx - 1;
                indices[_indexCount++] = idx;
            }
        }

        return ref _vertices![idx];
    }

    public SimpleMesh Build(VertexShader shader)
    {
        // Check for an unfinished mesh
        if (_vertexCount % VerticesPerPrimitive != 0)
        {
            throw new InvalidOperationException($"Mesh has vertex-data for an unfinished primitive. {_vertexCount} vertices with {VerticesPerPrimitive} per primitive.");
        }

        if (_vertexCount == 0 || _indexCount == 0)
        {
            return new SimpleMesh(null, null, 0, 0);
        }

        var vertices = _vertices.AsSpan(0, _vertexCount);
        using var vertexBuffer = Tig.RenderingDevice.CreateVertexBuffer<T>(vertices, debugName: "MeshBuilder");
        var indices = _indices.AsSpan(0, _indexCount);
        using var indexBuffer = Tig.RenderingDevice.CreateIndexBuffer(indices);
        using var bufferBinding = new BufferBinding(Tig.RenderingDevice, shader).Ref();
        var builder = bufferBinding.Resource.AddBuffer<T>(vertexBuffer, 0);
        T.Describe(ref builder);

        return new SimpleMesh(indexBuffer, bufferBinding, _vertexCount, _indexCount);
    }

    public void Dispose()
    {
        // Return resources to pool
        if (_vertices != null)
        {
            ArrayPool<T>.Shared.Return(_vertices);
            _vertices = null;
        }

        if (_indices != null)
        {
            ArrayPool<int>.Shared.Return(_indices);
            _indices = null;
        }

        _vertexCount = 0;
        _indexCount = 0;
    }
}

internal enum SimpleMeshBuilderType
{
    Quads,
    Triangles
}