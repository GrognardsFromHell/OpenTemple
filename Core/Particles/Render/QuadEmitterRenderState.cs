using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Particles.Instances;

namespace SpicyTemple.Core.Particles.Render
{
    /// <summary>
    /// The rendering state associated with a point emitter.
    /// </summary>
    internal class QuadEmitterRenderState : GeneralEmitterRenderState {
        public QuadEmitterRenderState(RenderingDevice device, PartSysEmitter emitter) : base(device, emitter, false)
        {
            bufferBinding = new BufferBinding(device, material.Resource.VertexShader).Ref();

            var maxCount = emitter.GetSpec().GetMaxParticles();

            vertexBuffer =
                device.CreateEmptyVertexBuffer(SpriteVertex.Size * 4 * maxCount);

            bufferBinding.Resource
                .AddBuffer<SpriteVertex>(vertexBuffer, 0)
                .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                .AddElement(VertexElementType.Color, VertexElementSemantic.Color)
                .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);
        }

        public ResourceRef<VertexBuffer> vertexBuffer;
        public ResourceRef<BufferBinding> bufferBinding;
    }
}