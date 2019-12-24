
namespace OpenTemple.Core.GFX
{
    public class MaterialSamplerBinding : GpuResource<MaterialSamplerBinding>
    {
        public MaterialSamplerBinding(RenderingDevice device,
            ITexture texture,
            SamplerState samplerState) : base()
        {
            Texture = texture.Ref();
            SamplerState = samplerState.Ref();
        }

        public ResourceRef<ITexture> Texture { get; }

        public ResourceRef<SamplerState> SamplerState { get; }

        protected override void FreeResource()
        {
            Texture.Dispose();
            SamplerState.Dispose();
        }
    }
}