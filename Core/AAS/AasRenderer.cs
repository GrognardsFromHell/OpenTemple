using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.AAS
{
    public class AasRenderer : IAnimatedModelRenderer, IDisposable
    {
        private RenderingDevice mDevice;
        private ShapeRenderer2d mShapeRenderer2d;
        private ShapeRenderer3d mShapeRenderer3d;

        private ResourceRef<Material> mGeometryShadowMaterial;

        // Shadow map related state
        private ResourceRef<RenderTargetTexture> mShadowTarget; // Shadow map texture
        private ResourceRef<RenderTargetTexture> mShadowTargetTmp; // Temp buffer for gauss blur
        private ResourceRef<Material> mShadowMapMaterial;
        private ResourceRef<Material> mGaussBlurHor; // Material for horizontal pass of gauss blur
        private ResourceRef<Material> mGaussBlurVer; // Material for vertical pass of gauss blur

        public AasRenderer(RenderingDevice device, ShapeRenderer2d shapeRenderer2d, ShapeRenderer3d shapeRenderer3d)
        {
            mDevice = device;
            mShapeRenderer2d = shapeRenderer2d;
            mShapeRenderer3d = shapeRenderer3d;
            mGeometryShadowMaterial = CreateGeometryShadowMaterial(device);
            mShadowTarget = device.CreateRenderTargetTexture(BufferFormat.A8R8G8B8, ShadowMapWidth, ShadowMapHeight,
                debugName: "AASShadowTarget");
            mShadowTargetTmp = device.CreateRenderTargetTexture(BufferFormat.A8R8G8B8, ShadowMapWidth, ShadowMapHeight,
                debugName: "AASShadowTargetTmp");
            mShadowMapMaterial = CreateShadowMapMaterial(device);
            mGaussBlurHor = CreateGaussBlurMaterial(device, mShadowTarget, true);
            mGaussBlurVer = CreateGaussBlurMaterial(device, mShadowTargetTmp, false);
        }

        private AasRenderSubmeshData GetSubmeshData(AasRenderData renderData,
            int submeshId,
            ISubmesh submesh)
        {
            if (renderData.submeshes[submeshId] == null)
            {
                renderData.submeshes[submeshId] = new AasRenderSubmeshData(mDevice);
            }

            var submeshData = renderData.submeshes[submeshId];
            if (!submeshData.created)
            {
                submeshData.posBuffer = mDevice.CreateVertexBuffer(submesh.Positions, false, "AasSubmeshPositions");
                submeshData.normalsBuffer = mDevice.CreateVertexBuffer<Vector4>(submesh.Normals, false, "AasSubmeshNormals");
                submeshData.uvBuffer = mDevice.CreateVertexBuffer(submesh.UV, debugName:"AasSubmeshUV");
                submeshData.idxBuffer = mDevice.CreateIndexBuffer(submesh.Indices);

                var binding = submeshData.binding.Resource;
                binding.AddBuffer<Vector4>(submeshData.posBuffer, 0)
                    .AddElement(VertexElementType.Float4, VertexElementSemantic.Position);
                binding.AddBuffer<Vector4>(submeshData.normalsBuffer, 0)
                    .AddElement(VertexElementType.Float4, VertexElementSemantic.Normal);
                binding.AddBuffer<Vector2>(submeshData.uvBuffer, 0)
                    .AddElement(VertexElementType.Float2, VertexElementSemantic.TexCoord);

                submeshData.created = true;
            }
            else
            {
                submeshData.posBuffer.Resource.Update(submesh.Positions);
                submeshData.normalsBuffer.Resource.Update<Vector4>(submesh.Normals);
            }

            return submeshData;
        }

        private static ResourceRef<Material> CreateGeometryShadowMaterial(RenderingDevice device)
        {
            BlendSpec blendState = new BlendSpec();
            blendState.blendEnable = true;
            blendState.srcBlend = BlendOperand.SrcAlpha;
            blendState.destBlend = BlendOperand.InvSrcAlpha;
            RasterizerSpec rasterizerState = new RasterizerSpec();
            rasterizerState.cullMode = CullMode.None;
            DepthStencilSpec depthStencilState = new DepthStencilSpec();
            depthStencilState.depthWrite = false;

            using var vs = device.GetShaders().LoadVertexShader("shadow_geom_vs");
            using var ps = device.GetShaders().LoadPixelShader("diffuse_only_ps");

            return device.CreateMaterial(blendState, depthStencilState, rasterizerState, null, vs, ps).Ref();
        }

        private static ResourceRef<Material> CreateShadowMapMaterial(RenderingDevice device)
        {
            DepthStencilSpec depthStencilState = new DepthStencilSpec();
            depthStencilState.depthEnable = false;
            using var vs = device.GetShaders().LoadVertexShader("shadowmap_geom_vs");
            using var ps = device.GetShaders().LoadPixelShader("diffuse_only_ps");

            BlendSpec blendSpec = new BlendSpec();
            blendSpec.blendEnable = true;
            blendSpec.destBlend = BlendOperand.Zero;
            blendSpec.srcBlend = BlendOperand.SrcAlpha;
            blendSpec.destAlphaBlend = BlendOperand.Zero;
            blendSpec.srcAlphaBlend = BlendOperand.One;

            return device.CreateMaterial(blendSpec, depthStencilState, null, null, vs, ps).Ref();
        }

        private static ResourceRef<Material> CreateGaussBlurMaterial(RenderingDevice device,
            RenderTargetTexture texture,
            bool horizontal)
        {
            var samplerState = new SamplerSpec();
            samplerState.addressU = TextureAddress.Clamp;
            samplerState.addressV = TextureAddress.Clamp;
            samplerState.magFilter = TextureFilterType.Linear;
            samplerState.minFilter = TextureFilterType.Linear;
            samplerState.mipFilter = TextureFilterType.Linear;
            var rasterizerState = new RasterizerSpec();
            rasterizerState.cullMode = CullMode.None;
            var depthStencilState = new DepthStencilSpec();
            depthStencilState.depthEnable = false;

            var vs = device.GetShaders().LoadVertexShader("gaussian_blur_vs");
            var horDefines = new Dictionary<string, string>();
            if (horizontal)
            {
                horDefines["HOR"] = "1";
            }

            var ps = device.GetShaders().LoadPixelShader("gaussian_blur_ps", horDefines);

            var samplers = new MaterialSamplerSpec[]
            {
                new (new ResourceRef<ITexture>(texture), samplerState)
            };
            return device.CreateMaterial(null, depthStencilState, rasterizerState, samplers, vs, ps).Ref();
        }

        private unsafe void RecalcNormals(
            int vertexCount,
            ReadOnlySpan<Vector4> pos,
            Span<Vector4> normals,
            int primCount,
            ReadOnlySpan<ushort> indices)
        {
            // Process every TRI we have
            fixed (ushort* indicesPtr = indices)
            {
                var curIdx = indicesPtr;
                for (var tri = 0; tri < primCount; ++tri)
                {
                    // Indices of the three vertices making up this triangle
                    var idx1 = *curIdx++;
                    var idx2 = *curIdx++;
                    var idx3 = *curIdx++;

                    var pos1 = pos[idx1].ToVector3();
                    var pos2 = pos[idx2].ToVector3();
                    var pos3 = pos[idx3].ToVector3();

                    var v1to2 = pos2 - pos1;
                    var v1to3 = pos3 - pos1;

                    // Calculate the surface normal of the surface defined
                    // by the two directional vectors
                    var surfNormal = Vector3.Cross(v1to2, v1to3) * -1;

                    // The surface normal contributes to all three vertex normals
                    normals[idx1] = surfNormal.ToVector4(0);
                    normals[idx2] = surfNormal.ToVector4(0);
                    normals[idx3] = surfNormal.ToVector4(0);
                }
            }

            // Re-Normalize the normals we calculated
            for (var i = 0; i < vertexCount; ++i)
            {
                normals[i] = Vector4.Normalize(normals[i]);
            }
        }

        private const int ShadowMapWidth = 256;
        private const int ShadowMapHeight = 256;

        private AasRenderData GetOrCreateState(IAnimatedModel model)
        {
            var state = (AasRenderData) model.RenderState;
            if (state == null)
            {
                state = new AasRenderData();
                model.RenderState = state;
            }

            return state;
        }

        public void Render(IGameViewport viewport, IAnimatedModel model, AnimatedModelParams animParams, IList<Light3d> lights,
            MdfRenderOverrides materialOverrides = null)
        {
            var renderData = GetOrCreateState(model);

            var materialIds = model.GetSubmeshes();
            for (var i = 0; i < materialIds.Length; ++i) {
                var material = materialIds[i];
                var submesh = animParams.rotation3d ? model.GetSubmeshForParticles(animParams, i) : model.GetSubmesh(animParams, i);

                // Usually this should not happen, since it means there's
                // an unbound replacement material
                if (material == null) {
                    continue;
                }

                material.Bind(viewport, mDevice, lights, materialOverrides);

                // Do we have to recalculate the normals?
                if (material.GetSpec().recalculateNormals) {
                    RecalcNormals(
                        submesh.VertexCount,
                        submesh.Positions,
                        submesh.Normals,
                        submesh.PrimitiveCount,
                        submesh.Indices
                    );
                }

                var submeshData = GetSubmeshData(renderData, i, submesh);
                submeshData.binding.Resource.Bind();

                mDevice.SetIndexBuffer(submeshData.idxBuffer);
                mDevice.DrawIndexed(PrimitiveType.TriangleList,
                    submesh.VertexCount,
                    submesh.PrimitiveCount * 3);
            }
        }

        public void RenderWithoutMaterial(IAnimatedModel model,
            in AnimatedModelParams animParams)
        {
            var renderData = GetOrCreateState(model);

            var materialIds = model.GetSubmeshes();
            for (var i = 0; i < materialIds.Length; ++i)
            {
                var submesh = model.GetSubmesh(animParams, i);

                var submeshData = GetSubmeshData(renderData, i, submesh);
                submeshData.binding.Resource.Bind();

                mDevice.SetIndexBuffer(submeshData.idxBuffer);
                mDevice.DrawIndexed(
                    PrimitiveType.TriangleList,
                    submesh.VertexCount,
                    submesh.PrimitiveCount * 3
                );
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ShadowGlobals {
            public Matrix4x4 projMatrix;
            public Vector4 globalLightDir;
            public Vector4 offsetZ;
            public Vector4 alpha;
        }

        public void RenderGeometryShadow(
            WorldCamera camera,
            IAnimatedModel model,
            in AnimatedModelParams animParams,
            Light3d globalLight,
            float alpha)
        {

            var renderData = GetOrCreateState(model);

            mDevice.SetMaterial(mGeometryShadowMaterial);

            var globals = new ShadowGlobals();
            globals.projMatrix = camera.GetViewProj();
            globals.globalLightDir = globalLight.dir;
            globals.offsetZ = new Vector4(animParams.offsetZ, 0, 0, 0);
            globals.alpha = new Vector4( alpha, 0, 0, 0 );
            mDevice.SetVertexShaderConstants(0, ref globals);

            var materialIds = model.GetSubmeshes();
            for (var i = 0; i < materialIds.Length; ++i) {
                var submesh = model.GetSubmesh(animParams, i);

                var submeshData = GetSubmeshData(renderData, i, submesh);
                submeshData.binding.Resource.Bind();

                mDevice.SetIndexBuffer(submeshData.idxBuffer);
                mDevice.DrawIndexed(PrimitiveType.TriangleList,
                    submesh.VertexCount,
                    submesh.PrimitiveCount * 3
                );
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ShadowShaderGlobals {
            public Vector4 shadowMapWorldPos;
            public Vector4  lightDir;
            public Vector4  height;
            public Vector4  color;
        }

        public void RenderShadowMapShadow(IGameViewport viewport,
            IList<IAnimatedModel> models,
            IList<AnimatedModelParams> modelParams,
            Vector3 center,
            float radius,
            float height,
            Vector4 lightDir,
            float alpha,
            bool softShadows)
        {
                    
	        Trace.Assert(models.Count == modelParams.Count);

	        float shadowMapWorldX, shadowMapWorldWidth,
		        shadowMapWorldZ, shadowMapWorldHeight;

	        if (lightDir.X < 0.0) {
		        shadowMapWorldX = center.X - 2 * radius + lightDir.X * height;
		        shadowMapWorldWidth = 4 * radius - lightDir.X * height;
	        } else {
		        shadowMapWorldX = center.X - 2 * radius;
		        shadowMapWorldWidth = lightDir.X * height + 4 * radius;
	        }

	        if (lightDir.Z < 0.0) {
		        shadowMapWorldZ = center.Z - 2 * radius + lightDir.Z * height;
		        shadowMapWorldHeight = 4 * radius - lightDir.Z * height;
	        } else {
		        shadowMapWorldZ = center.Z - 2 * radius;
		        shadowMapWorldHeight = lightDir.Z + height + 4 * radius;
	        }

	        mDevice.PushRenderTarget(mShadowTarget, null);

	        mDevice.SetMaterial(mShadowMapMaterial);

	        // Set shader params
	        ShadowShaderGlobals globals = new ShadowShaderGlobals();
	        globals.shadowMapWorldPos = new Vector4(
		        shadowMapWorldX,
		        shadowMapWorldZ,
		        shadowMapWorldWidth,
		        shadowMapWorldHeight
	        );
	        globals.lightDir = lightDir;
	        globals.height.X = center.Y;
	        globals.color = new Vector4(0, 0, 0, 0.5f);
	        mDevice.SetVertexShaderConstants(0, ref globals);

	        mDevice.ClearCurrentColorTarget(new LinearColorA(0, 0, 0, 0));

	        for (int i = 0; i < models.Count; ++i) {
		        RenderWithoutMaterial(models[i], modelParams[i]);
	        }
		        
	        if (softShadows) {
		        mDevice.PushRenderTarget(mShadowTargetTmp, null);
                mDevice.SetMaterial(mGaussBlurHor);
                mShapeRenderer2d.DrawFullScreenQuad();
                // Explicitly unbind the input texture because it'll be briefly restored to the output by PopRenderTarget
                mDevice.SetTexture(0, Textures.InvalidTexture);
                mDevice.PopRenderTarget();

		        mDevice.PushRenderTarget(mShadowTarget, null);
		        mDevice.SetMaterial(mGaussBlurVer);
		        mShapeRenderer2d.DrawFullScreenQuad();
                mDevice.PopRenderTarget();
	        }
	        
	        mDevice.PopRenderTarget();

	        var shadowMapWorldBottom = shadowMapWorldZ + shadowMapWorldHeight;
	        var shadowMapWorldRight = shadowMapWorldX + shadowMapWorldWidth;

	        Span<ShapeVertex3d> corners = stackalloc ShapeVertex3d[4];
	        corners[0].pos = new Vector4( shadowMapWorldX, center.Y, shadowMapWorldZ, 1 );
	        corners[1].pos = new Vector4( shadowMapWorldX, center.Y, shadowMapWorldBottom, 1 );
	        corners[2].pos = new Vector4( shadowMapWorldRight, center.Y, shadowMapWorldBottom, 1 );
	        corners[3].pos = new Vector4( shadowMapWorldRight, center.Y, shadowMapWorldZ, 1 );
	        corners[0].uv = new Vector2( 0, 0 );
	        corners[1].uv = new Vector2( 0, 1 );
	        corners[2].uv = new Vector2( 1, 1 );
	        corners[3].uv = new Vector2( 1, 0 );

	        mShapeRenderer3d.DrawQuad(viewport, corners, new PackedLinearColorA( 0xFFFFFFFF), mShadowTarget.Resource);
        }

        // Data to be stored for an AAS model so it can be rendered
        // more efficiently
        private class AasRenderSubmeshData : IDisposable
        {
            public bool created = false;
            public ResourceRef<VertexBuffer> posBuffer;
            public ResourceRef<VertexBuffer> normalsBuffer;
            public ResourceRef<VertexBuffer> uvBuffer;
            public ResourceRef<IndexBuffer> idxBuffer;
            public ResourceRef<BufferBinding> binding;

            public AasRenderSubmeshData(RenderingDevice device)
            {
                binding = device.CreateMdfBufferBinding().Ref();
            }

            public void Dispose()
            {
                posBuffer.Dispose();
                normalsBuffer.Dispose();
                uvBuffer.Dispose();
                idxBuffer.Dispose();
                binding.Dispose();
            }
        }

        private class AasRenderData : IRenderState
        {
            public readonly AasRenderSubmeshData[] submeshes = new AasRenderSubmeshData[16];

            public void Dispose()
            {
                submeshes.DisposeAndNull();
            }
        }


        public void Dispose()
        {
            mGeometryShadowMaterial.Dispose();
            mShadowTarget.Dispose();
            mShadowTargetTmp.Dispose();
            mShadowMapMaterial.Dispose();
            mGaussBlurHor.Dispose();
            mGaussBlurVer.Dispose();
        }
    }

    public interface IAnimatedModelRenderer
    {
        void Render(IGameViewport viewport, IAnimatedModel model,
            AnimatedModelParams animParams,
            IList<Light3d> lights,
            MdfRenderOverrides materialOverrides = null);
    }
}