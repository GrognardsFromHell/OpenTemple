using System;
using System.Collections.Generic;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.Materials;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.MapSector
{
    /// <summary>
    /// Renders information about tiles, specifically their flags.
    /// </summary>
    public class SectorDebugRenderer : IDisposable
    {
        private readonly Dictionary<SectorLoc, SimpleMesh> _sectorDebugState;

        private readonly RenderingDevice _device = Tig.RenderingDevice;

        private readonly ResourceRef<Material> _material;

        public SectorDebugRenderer()
        {
            _sectorDebugState = new Dictionary<SectorLoc, SimpleMesh>();
            _material = CreateMaterial(_device);
        }

        private ResourceRef<Material> CreateMaterial(RenderingDevice device)
        {
            var blendState = new BlendSpec
            {
                blendEnable = true,
                srcBlend = BlendOperand.SrcAlpha,
                destBlend = BlendOperand.InvSrcAlpha
            };
            var depthState = new DepthStencilSpec();
            depthState.depthEnable = false;
            depthState.depthWrite = false;

            var rasterizerState = new RasterizerSpec();
            var vs = _device.GetShaders().LoadVertexShader("diffuse_only_vs");
            var ps = _device.GetShaders().LoadPixelShader("diffuse_only_ps");

            return device.CreateMaterial(
                blendState,
                depthState,
                rasterizerState,
                Array.Empty<MaterialSamplerSpec>(),
                vs,
                ps).Ref();
        }

        public void Render(TileRect tileRect)
        {
            _device.SetMaterial(_material.Resource);
            _device.SetVertexShaderConstant(0, StandardSlotSemantic.ViewProjMatrix);

            using var sectorIt = new SectorIterator(tileRect);
            foreach (var sector in sectorIt.EnumerateSectors())
            {
                if (!sector.IsValid)
                {
                    continue;
                }

                if (!_sectorDebugState.TryGetValue(sector.Loc, out var renderingState))
                {
                    renderingState = BuildSubTileMesh(_device, _material.Resource.VertexShader, sector.Sector);
                    _sectorDebugState.Add(sector.Loc, renderingState);
                }

                renderingState.Render(_device);
            }
        }

        public void Dispose()
        {
            _material.Dispose();
        }

        private SimpleMesh BuildSubTileMesh(RenderingDevice device, VertexShader shader, Sector sector)
        {
            var builder = new SubTileMeshBuilder(sector.secLoc);

            for (int y = 0; y < Sector.SectorSideSize; y++)
            {
                for (int x = 0; x < Sector.SectorSideSize; x++)
                {
                    var tileIdx = Sector.GetSectorTileIndex(x, y);
                    var tile = sector.tilePkt.tiles[tileIdx];

                    for (int dx = 0; dx < 3; dx++)
                    {
                        for (int dy = 0; dy < 3; dy++)
                        {
                            var blockFlag = SectorTile.GetBlockingFlag(dx, dy);
                            var flyOverFlag = SectorTile.GetFlyOverFlag(dx, dy);
                            var blocking = tile.flags.HasFlag(blockFlag);
                            var flyover = tile.flags.HasFlag(flyOverFlag);

                            PackedLinearColorA color;
                            if (blocking)
                            {
                                color = new PackedLinearColorA(200, 0, 0, 127);
                            }
                            else if (flyover)
                            {
                                color = new PackedLinearColorA(127, 0, 127, 127);
                            }
                            else
                            {
                                continue;
                            }

                            builder.Add(x * 3 + dx, y * 3 + dy, color);
                        }
                    }
                }
            }

            return builder.Build(shader);
        }
    }
}