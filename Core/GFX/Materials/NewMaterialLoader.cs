using System;
using System.Collections.Generic;
using System.Text.Json;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.GFX.Materials
{
    public class NewMaterialLoader
    {
        private readonly RenderingDevice _device;

        private readonly string _path;

        public NewMaterialLoader(string path, RenderingDevice device)
        {
            _path = path;
            _device = device;
        }

        /// <summary>
        /// Loads a new-style JSON based material with more options than MDF.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ResourceRef<Material> LoadMaterial(ReadOnlyMemory<byte> content)
        {
            var document = JsonDocument.Parse(content);

            var root = document.RootElement;
            if (root.Type != JsonValueType.Object)
            {
                throw new InvalidMaterialException(_path, "The root of the material document should be an object.");
            }

            return ParseMaterial(root);
        }

        private ResourceRef<Material> ParseMaterial(JsonElement root)
        {
            if (!root.TryGetProperty("vertexShader", out var vertexShaderEl))
            {
                throw new InvalidMaterialException(_path, "A vertexShader is required.");
            }

            var vertexShader = _device.GetShaders().LoadVertexShader(vertexShaderEl.GetString());

            if (!root.TryGetProperty("fragmentShader", out var fragmentShaderEl))
            {
                throw new InvalidMaterialException(_path, "A fragmentShader is required.");
            }

            var fragmentShader = _device.GetShaders().LoadPixelShader(fragmentShaderEl.GetString());

            var blendState = new BlendSpec();
            if (root.TryGetProperty("blend", out var blendEl))
            {
                ParseBlendSpec(blendEl, blendState);
            }

            var depthStencil = new DepthStencilSpec();
            if (root.TryGetProperty("depthStencil", out var depthStencilEl))
            {
                ParseDepthStencil(depthStencilEl, depthStencil);
            }

            var rasterizer = new RasterizerSpec();
            if (root.TryGetProperty("rasterizer", out var rasterizerEl))
            {
                ParseRasterizer(rasterizerEl, rasterizer);
            }

            var samplers = new List<MaterialSamplerSpec>();

            if (root.TryGetProperty("samplers", out var samplersEl))
            {
                foreach (var samplerEl in samplersEl.EnumerateArray())
                {
                    var samplerState = new SamplerSpec();
                    ParseSampler(samplerEl, samplerState, out var texture);
                    samplers.Add(new MaterialSamplerSpec(texture, samplerState));
                }
            }

            return _device.CreateMaterial(
                blendState,
                depthStencil,
                rasterizer,
                samplers.ToArray(),
                vertexShader,
                fragmentShader
            ).Ref();
        }

        private void ParseBlendSpec(in JsonElement blendEl, BlendSpec blendState)
        {
            foreach (var property in blendEl.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "blendEnable":
                        blendState.blendEnable = property.Value.GetBoolean();
                        break;
                    case "srcBlend":
                        blendState.srcBlend = Enum.Parse<BlendOperand>(property.Value.GetString());
                        break;
                    case "destBlend":
                        blendState.destBlend = Enum.Parse<BlendOperand>(property.Value.GetString());
                        break;
                    case "srcAlphaBlend":
                        blendState.srcAlphaBlend = Enum.Parse<BlendOperand>(property.Value.GetString());
                        break;
                    case "destAlphaBlend":
                        blendState.destAlphaBlend = Enum.Parse<BlendOperand>(property.Value.GetString());
                        break;
                    case "writeRed":
                        blendState.writeRed = property.Value.GetBoolean();
                        break;
                    case "writeGreen":
                        blendState.writeGreen = property.Value.GetBoolean();
                        break;
                    case "writeBlue":
                        blendState.writeBlue = property.Value.GetBoolean();
                        break;
                    case "writeAlpha":
                        blendState.writeAlpha = property.Value.GetBoolean();
                        break;
                    default:
                        throw new InvalidMaterialException(_path, $"Unknown property {property.Name} " +
                                                                  $"of 'blend'");
                }
            }
        }

        private void ParseDepthStencil(in JsonElement depthStencilEl, DepthStencilSpec depthStencil)
        {
            foreach (var property in depthStencilEl.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "depthEnable":
                        depthStencil.depthEnable = property.Value.GetBoolean();
                        break;
                    case "depthWrite":
                        depthStencil.depthWrite = property.Value.GetBoolean();
                        break;
                    case "depthFunc":
                        depthStencil.depthFunc = Enum.Parse<ComparisonFunc>(property.Value.GetString());
                        break;
                    default:
                        throw new InvalidMaterialException(_path, $"Unknown property {property.Name} " +
                                                                  $"of 'depthStencil'");
                }
            }
        }

        private void ParseRasterizer(in JsonElement rasterizerEl, RasterizerSpec rasterizer)
        {
            foreach (var property in rasterizerEl.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "wireframe":
                        rasterizer.wireframe = property.Value.GetBoolean();
                        break;
                    case "cullMode":
                        rasterizer.cullMode = Enum.Parse<CullMode>(property.Value.GetString());
                        break;
                    case "scissor":
                        rasterizer.scissor = property.Value.GetBoolean();
                        break;
                    default:
                        throw new InvalidMaterialException(_path, $"Unknown property {property.Name} " +
                                                                  $"of 'rasterizer'");
                }
            }
        }

        private void ParseSampler(in JsonElement samplerEl, SamplerSpec samplerState, out ResourceRef<ITexture> texture)
        {
            texture = default;

            foreach (var property in samplerEl.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "minFilter":
                        samplerState.minFilter = Enum.Parse<TextureFilterType>(property.Value.GetString());
                        break;
                    case "magFilter":
                        samplerState.magFilter = Enum.Parse<TextureFilterType>(property.Value.GetString());
                        break;
                    case "mipFilter":
                        samplerState.mipFilter = Enum.Parse<TextureFilterType>(property.Value.GetString());
                        break;
                    case "addressU":
                        samplerState.addressU = Enum.Parse<TextureAddress>(property.Value.GetString());
                        break;
                    case "addressV":
                        samplerState.addressV = Enum.Parse<TextureAddress>(property.Value.GetString());
                        break;
                    case "texture":
                        texture = LoadTexture(property.Value);
                        break;
                    default:
                        throw new InvalidMaterialException(_path, $"Unknown property {property.Name} " +
                                                                  $"of 'sampler'");
                }
            }
        }

        private ResourceRef<ITexture> LoadTexture(in JsonElement textureEl)
        {
            // Must specify path at least
            string path = null;
            var mipMaps = false;
            foreach (var property in textureEl.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "mipMaps":
                        mipMaps = property.Value.GetBoolean();
                        break;
                    case "path":
                        path = property.Value.GetString();
                        break;
                    default:
                        throw new InvalidMaterialException(_path, $"Unknown property {property.Name} " +
                                                                  $"of 'sampler'");
                }
            }

            if (path == null)
            {
                throw new InvalidMaterialException(_path, "Texture is missing required 'path' property.");
            }

            return _device.GetTextures().Resolve(path, mipMaps);
        }
    }

    public class InvalidMaterialException : Exception
    {
        public InvalidMaterialException(string path, string message) : base(
            $"Failed to parse material {path}: {message}")
        {
        }
    }

    public static class NewMaterialLoaderExtensions
    {
        public static ResourceRef<Material> LoadNewMaterial(this RenderingDevice device, string path)
        {
            using var content = Tig.FS.ReadFile(path);

            var loader = new NewMaterialLoader(path, device);
            return loader.LoadMaterial(content.Memory);
        }
    }
}