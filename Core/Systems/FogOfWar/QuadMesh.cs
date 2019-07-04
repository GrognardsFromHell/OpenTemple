using System;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    public class QuadMesh : IDisposable
    {
        private ResourceRef<IndexBuffer> _indexBuffer;
        private ResourceRef<BufferBinding> _bufferBinding;
        public readonly int VertexCount;
        public readonly int IndexCount;

        public QuadMesh()
        {
        }

        public void Dispose()
        {
            _bufferBinding.Dispose();
            _indexBuffer.Dispose();
        }
    }
}