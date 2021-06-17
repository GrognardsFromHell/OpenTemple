using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.MaterialDefinitions;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.GFX.RenderMaterials
{

    public class MdfMaterialFactory : IDisposable
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly IFileSystem _fs;

        private readonly RenderingDevice _device;

        private readonly Textures _textures;

        public MdfMaterialFactory(IFileSystem fs, RenderingDevice device)
        {
            _fs = fs;
            _device = device;
            _textures = device.GetTextures();
        }

        public IMdfRenderMaterial GetById(int id)
        {
            if (mIdRegistry.TryGetValue(id, out var materialRef))
            {
                return materialRef.Resource;
            }

            return null;
        }

        public IMdfRenderMaterial GetByName(string name)
        {
            var nameLower = name.ToLowerInvariant();
            if (mNameRegistry.TryGetValue(nameLower, out var materialRef))
            {
                return materialRef.Resource;
            }

            return null;
        }

        public ResourceRef<IMdfRenderMaterial> LoadMaterial(string name)
        {
            var nameLower = name.ToLowerInvariant();

            if (mNameRegistry.TryGetValue(name, out var materialRef))
            {
                return materialRef.CloneRef();
            }

            try {
                var mdfContent = _fs.ReadTextFile(name);
                var parser = new MdfParser(name, mdfContent);
                var mdfMaterial = parser.Parse();

                Trace.Assert(_nextFreeId >= 1);
                // Assign ID
                var id = _nextFreeId++;

                var material = CreateDeviceMaterial(name, mdfMaterial);

                using var result = new ResourceRef<IMdfRenderMaterial>(
                    new MdfRenderMaterial(id, name, mdfMaterial, material)
                );

                mIdRegistry[id] = result.CloneRef();
                mNameRegistry[nameLower] = result.CloneRef();

                return result.CloneRef();
            } catch (Exception e) {
                Logger.Error("Unable to load MDF file '{0}': {1} {2}", name, e.GetType(), e.Message);
                return new ResourceRef<IMdfRenderMaterial>(InvalidMdfRenderMaterial.Instance);
            }
        }

        private Dictionary<int, ResourceRef<IMdfRenderMaterial>> mIdRegistry = new Dictionary<int, ResourceRef<IMdfRenderMaterial>>();
        private Dictionary<string, ResourceRef<IMdfRenderMaterial>> mNameRegistry = new Dictionary<string, ResourceRef<IMdfRenderMaterial>>();
        private int _nextFreeId = 1;

        private Material CreateDeviceMaterial(string name, MdfMaterial spec)
        {
            
		var rasterizerState = new RasterizerSpec();

		// Wireframe mode
		rasterizerState.wireframe = spec.wireframe;

		// Cull mode
		if (!spec.faceCulling) {
			rasterizerState.cullMode = CullMode.None;
		}

		BlendSpec blendState = new BlendSpec();

		switch (spec.blendType) {
		case MdfBlendType.Alpha:
			blendState.blendEnable = true;
			blendState.srcBlend = BlendOperand.SrcAlpha;
			blendState.destBlend = BlendOperand.InvSrcAlpha;
			break;
		case MdfBlendType.Add:
			blendState.blendEnable = true;
			blendState.srcBlend = BlendOperand.One;
			blendState.destBlend = BlendOperand.One;
			break;
		case MdfBlendType.AlphaAdd:
			blendState.blendEnable = true;
			blendState.srcBlend = BlendOperand.SrcAlpha;
			blendState.destBlend = BlendOperand.One;
			break;
		default:
		case MdfBlendType.None:
			break;
		}

		if (!spec.enableColorWrite) {
			blendState.writeAlpha = false;
			blendState.writeRed = false;
			blendState.writeGreen = false;
			blendState.writeBlue = false;
		}

		DepthStencilSpec depthStencilState = new DepthStencilSpec();
		depthStencilState.depthEnable = !spec.disableZ;
		depthStencilState.depthWrite = spec.enableZWrite;
		depthStencilState.depthFunc = ComparisonFunc.LessEqual;

		// Resolve texture references based on type
		Trace.Assert(spec.samplers.Count <= 4);

		Dictionary<string, string> psDefines = new Dictionary<string, string>();
		Dictionary<string, string> vsDefines = new Dictionary<string, string>();

		List<MaterialSamplerSpec> samplers = new List<MaterialSamplerSpec>(spec.samplers.Count);
		// A general MDF can reference up to 4 textures
		for (var i = 0; i < spec.samplers.Count; ++i) {
			var sampler = spec.samplers[i];
			if (sampler.filename == null)
				continue;

			var texture = _textures.Resolve(sampler.filename, true);
			if (!texture.IsValid) {
				Logger.Warn("General shader {0} references invalid texture '{1}' in sampler {2}",
					name, sampler.filename, i);
			}

			SamplerSpec samplerState = new SamplerSpec();
			// Set up the addressing
			if (spec.clamp) {
				samplerState.addressU = TextureAddress.Clamp;
				samplerState.addressV = TextureAddress.Clamp;
			}

			// Set up filtering
			if (spec.linearFiltering) {
				samplerState.magFilter = TextureFilterType.Linear;
				samplerState.minFilter = TextureFilterType.Linear;
				samplerState.mipFilter = TextureFilterType.Linear;
			} else {
				samplerState.magFilter = TextureFilterType.NearestNeighbor;
				samplerState.minFilter = TextureFilterType.NearestNeighbor;
				samplerState.mipFilter = TextureFilterType.NearestNeighbor;
			}
			
			samplers.Add(new MaterialSamplerSpec(texture, samplerState));

			/*
				Set the stage's blending type in the pixel shader defines.
				The numbers correlate with the defines in mdf_ps.hlsl
			*/
			var stageId = samplers.Count;
			var defName = $"TEXTURE_STAGE{stageId}_MODE";
			switch (sampler.blendType) {
			default:
			case MdfTextureBlendType.Modulate:
				psDefines[defName] = "1";
				break;
			case MdfTextureBlendType.Add:
				psDefines[defName] = "2";
				break;
			case MdfTextureBlendType.TextureAlpha:
				psDefines[defName] = "3";
				break;
			case MdfTextureBlendType.CurrentAlpha:
				psDefines[defName] = "4";
				break;
			case MdfTextureBlendType.CurrentAlphaAdd:
				psDefines[defName] = "5";
				break;
			}

			/*
				If the stage requires animated texture coordinates,
				we have to set this on the vertex shader. The numbers used
				correlate with defines in mdf_vs.hlsl
			*/
			defName = $"TEXTURE_STAGE{stageId}_UVANIM";
			vsDefines[$"TEXTURE_STAGE{stageId}_SPEEDU"] = sampler.speedU.ToString(CultureInfo.InvariantCulture);
			vsDefines[$"TEXTURE_STAGE{stageId}_SPEEDV"] = sampler.speedV.ToString(CultureInfo.InvariantCulture);
			switch (sampler.uvType) {				
			case MdfUvType.Environment:
				vsDefines[defName] = "1";
				break;
			case MdfUvType.Drift:
				vsDefines[defName] = "2";
				break;
			case MdfUvType.Swirl:
				vsDefines[defName] = "3";
				break;
			case MdfUvType.Wavey:
				vsDefines[defName] = "4";
				break;
			}
		}

		if (spec.glossmap != null) {
			var glossMap = _textures.Resolve(spec.glossmap, true);
			if (!glossMap.IsValid) {
				Logger.Warn("General shader {0} references invalid gloss map texture '{1}'",
					name, spec.glossmap);
			}
		}

		psDefines["TEXTURE_STAGES"] = samplers.Count.ToString(CultureInfo.InvariantCulture);
		vsDefines["TEXTURE_STAGES"] = samplers.Count.ToString(CultureInfo.InvariantCulture);
		if (spec.notLit) {
			vsDefines["LIGHTING"] = "0";
		} else {
			vsDefines["LIGHTING"] = "1";
		}

		// Special case for highlight shaders until we're able to encode this
		// in the material file itself
		if (name == "art/meshes/mouseover.mdf"
			 || name == "art/meshes/hilight.mdf"
			 || name.StartsWith("art/meshes/wg_")) {
			rasterizerState.cullMode = CullMode.Front;
			vsDefines["HIGHLIGHT"] = "1";
		}

		using var pixelShader = _device.GetShaders().LoadPixelShader("mdf_ps", psDefines);

		// Select the right vertex shader to use
		using var vertexShader = _device.GetShaders().LoadVertexShader("mdf_vs", vsDefines);

		if (!pixelShader.IsValid || !vertexShader.IsValid) {
			throw new GfxException($"Unable to create MDF material {name} due to missing shaders.");
		}

		return _device.CreateMaterial(blendState, depthStencilState, rasterizerState, samplers.ToArray(), vertexShader, pixelShader);
        }

        public void Dispose()
        {
            // Clear all loaded materials
            foreach (var value in mIdRegistry.Values)
            {
                value.Dispose();
            }
            mIdRegistry.Clear();

            foreach (var value in mNameRegistry.Values)
            {
                value.Dispose();
            }
            mNameRegistry.Clear();

        }
    }

    /// <summary>
    /// A render material that is used when the MDF file is corrupt or can otherwise not be loaded.
    /// </summary>
    internal class InvalidMdfRenderMaterial : IMdfRenderMaterial
    {

	    public static InvalidMdfRenderMaterial Instance { get; } = new InvalidMdfRenderMaterial();

	    private readonly MdfMaterial _spec = new MdfMaterial(MdfType.Textured);

	    public void Reference()
	    {
	    }

	    public void Dereference()
	    {
	    }

	    public int GetId()
	    {
		    return 0;
	    }

	    public string GetName()
	    {
		    return "<invalid>";
	    }

	    public ITexture GetPrimaryTexture()
	    {
		    return null;
	    }

	    public MdfMaterial GetSpec() => _spec;

	    public void Bind(IGameViewport viewport, RenderingDevice g, IList<Light3d> lights, MdfRenderOverrides overrides = null)
	    {
		    // Simply do nothing
	    }
    }
}