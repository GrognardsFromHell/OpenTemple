using System.Collections.Generic;

namespace OpenTemple.Core.GFX.Materials
{
    public class Material : GpuResource<Material>
    {
        public ResourceRef<BlendState> BlendState { get; private set; }

        public ResourceRef<DepthStencilState> DepthStencilState { get; private set; }

        public ResourceRef<RasterizerState> RasterizerState { get; private set; }

        public List<ResourceRef<MaterialSamplerBinding>> Samplers { get; private set; }

        public ResourceRef<VertexShader> VertexShader { get; private set; }

        public ResourceRef<PixelShader> PixelShader { get; private set; }

        public Material(RenderingDevice device,
            ResourceRef<BlendState> blendState,
            ResourceRef<DepthStencilState> depthStencilState,
            ResourceRef<RasterizerState> rasterizerState,
            List<ResourceRef<MaterialSamplerBinding>> samplers,
            ResourceRef<VertexShader> vertexShader,
            ResourceRef<PixelShader> pixelShader) : base()
        {
            BlendState = blendState;
            DepthStencilState = depthStencilState;
            RasterizerState = rasterizerState;
            Samplers = samplers;
            VertexShader = vertexShader;
            PixelShader = pixelShader;
        }

        protected override void FreeResource()
        {
            BlendState.Dispose();
            DepthStencilState.Dispose();
            RasterizerState.Dispose();
            foreach (var samplerBinding in Samplers)
            {
                samplerBinding.Dispose();
            }

            VertexShader.Dispose();
            PixelShader.Dispose();
        }
    };
}