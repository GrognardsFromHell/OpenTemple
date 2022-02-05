using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Clipping;

internal class ClippingMesh : IDisposable
{
    private readonly string _filename;
    private readonly List<ClippingMeshObj> _instances = new();
    private readonly ResourceLifecycleCallbacks _lifecycleCallbacks;
    private ResourceRef<IndexBuffer> _indexBuffer;
    private ResourceRef<VertexBuffer> _vertexBuffer;

    public ClippingMesh(RenderingDevice device, string filename)
    {
        _filename = filename;
        _lifecycleCallbacks = new ResourceLifecycleCallbacks(device, CreateResources, FreeResources);
    }

    public IReadOnlyList<ClippingMeshObj> Instances => _instances;

    public VertexBuffer VertexBuffer => _vertexBuffer;

    public IndexBuffer IndexBuffer => _indexBuffer;

    public int VertexCount { get; private set; }

    public int TriCount { get; private set; }

    public Vector3 BoundingSphereOrigin { get; private set; }

    public float BoundingSphereRadius { get; private set; }

    public void Dispose()
    {
        _lifecycleCallbacks.Dispose();
    }

    public void AddInstance(ClippingMeshObj obj)
    {
        _instances.Add(obj);
    }

    private void CreateResources(RenderingDevice device)
    {
        using var reader = Tig.FS.OpenBinaryReader(_filename);

        BoundingSphereOrigin = reader.ReadVector3();
        BoundingSphereRadius = reader.ReadSingle();
        var objCount = reader.ReadInt32();
        var dataStart = reader.ReadInt32();

        if (objCount != 1)
        {
            throw new InvalidOperationException("TemplePlus only supports DAG with a " +
                                                $"single object. {_filename} has {objCount} ObjectHandles.");
        }

        // Start reading again @ the data start offset
        reader.BaseStream.Seek(dataStart, SeekOrigin.Begin);

        VertexCount = reader.ReadInt32();
        TriCount = reader.ReadInt32();
        var vertexDataStart = reader.ReadInt32();
        var indexDataStart = reader.ReadInt32();

        // Swap the indices to achieve clock wise tri ordering
        Span<ushort> indexData = stackalloc ushort[TriCount * 3];
        reader.BaseStream.Seek(indexDataStart, SeekOrigin.Begin);
        for (var i = 0; i < TriCount * 3; i += 3)
        {
            indexData[i + 2] = reader.ReadUInt16();
            indexData[i + 1] = reader.ReadUInt16();
            indexData[i] = reader.ReadUInt16();
        }

        _indexBuffer = device.CreateIndexBuffer(indexData);

        var vertexBufferSize = VertexCount * 3 * sizeof(float);
        Span<byte> vertices = stackalloc byte[vertexBufferSize];
        reader.BaseStream.Seek(vertexDataStart, SeekOrigin.Begin);
        reader.Read(vertices);
        _vertexBuffer = device.CreateVertexBufferRaw(vertices, debugName:"ClippingMesh_" + _filename);
    }

    private void FreeResources(RenderingDevice device)
    {
        _indexBuffer.Dispose();
        _vertexBuffer.Dispose();
    }
}