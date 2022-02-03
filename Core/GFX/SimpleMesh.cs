using System;

namespace OpenTemple.Core.GFX;

public class SimpleMesh : IDisposable
{
    public readonly ResourceRef<IndexBuffer> IndexBuffer;

    public readonly ResourceRef<BufferBinding> BufferBinding;

    public readonly int VertexCount;

    public readonly int IndexCount;

    public SimpleMesh(IndexBuffer indexBuffer, BufferBinding bufferBinding,
        int vertexCount, int indexCount)
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
    }

    public void Dispose()
    {
        IndexBuffer.Dispose();
        BufferBinding.Dispose();
    }

    public void Render(RenderingDevice device)
    {
        if (VertexCount > 0 && IndexCount > 0)
        {
            BufferBinding.Resource.Bind();
            device.SetIndexBuffer(IndexBuffer);
            device.DrawIndexed(PrimitiveType.TriangleList, VertexCount, IndexCount);
        }
    }
}