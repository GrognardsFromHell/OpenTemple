using System;
using OpenTemple.Core.GFX.Materials;

namespace OpenTemple.Core.GFX;

public class SimpleMesh : IDisposable
{
    public OptionalResourceRef<IndexBuffer> IndexBuffer { get; }

    public OptionalResourceRef<BufferBinding> BufferBinding { get; }

    public readonly int VertexCount;

    public readonly int IndexCount;
    
    public PrimitiveType PrimitiveType { get; }

    public SimpleMesh(IndexBuffer? indexBuffer, BufferBinding? bufferBinding,
        int vertexCount, int indexCount, PrimitiveType primitiveType = PrimitiveType.TriangleList)
    {
        if (indexBuffer != null)
        {
            IndexBuffer = indexBuffer.Ref();
        }

        if (bufferBinding != null)
        {
            BufferBinding = bufferBinding.Ref();
        }

        VertexCount = vertexCount;
        IndexCount = indexCount;
        PrimitiveType = primitiveType;
    }

    public void Dispose()
    {
        IndexBuffer.Dispose();
        BufferBinding.Dispose();
    }

    public void Render(RenderingDevice device)
    {
        var bufferBinding = BufferBinding.Resource;
        var indexBuffer = IndexBuffer.Resource;
        if (bufferBinding != null && indexBuffer != null && VertexCount > 0 && IndexCount > 0)
        {
            bufferBinding.Bind();
            device.SetIndexBuffer(IndexBuffer);
            device.DrawIndexed(PrimitiveType, VertexCount, IndexCount);
        }
    }
}