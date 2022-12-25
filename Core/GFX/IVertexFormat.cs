namespace OpenTemple.Core.GFX;

public interface IVertexFormat
{
    static abstract void Describe(ref BufferBindingBuilder builder);
}
