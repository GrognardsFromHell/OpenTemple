
namespace OpenTemple.Core.GFX;

public class MaterialSamplerBinding : GpuResource<MaterialSamplerBinding>
{
    public MaterialSamplerBinding(RenderingDevice device,
        ITexture? texture,
        SamplerState samplerState)
    {
        Texture = new OptionalResourceRef<ITexture>(texture);
        SamplerState = samplerState.Ref();
    }

    public OptionalResourceRef<ITexture> Texture { get; }

    public ResourceRef<SamplerState> SamplerState { get; }

    protected override void FreeResource()
    {
        Texture.Dispose();
        SamplerState.Dispose();
    }
}
