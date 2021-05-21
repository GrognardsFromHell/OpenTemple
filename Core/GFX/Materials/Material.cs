using System.Collections.Generic;

namespace OpenTemple.Core.GFX.Materials
{
    public class Material : GpuResource<Material>
    {
        public ResourceRef<BlendState> BlendState { get; }

        public ResourceRef<DepthStencilState> DepthStencilState { get; }

        public ResourceRef<RasterizerState> RasterizerState { get; }

        public List<ResourceRef<MaterialSamplerBinding>> Samplers { get; }

        public ResourceRef<VertexShader> VertexShader { get; }

        public ResourceRef<PixelShader> PixelShader { get; }

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