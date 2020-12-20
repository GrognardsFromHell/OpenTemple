using System;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.Materials;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.MapSector
{
    public class SectorVisibilityRenderer : IDisposable
    {
        private PackedLinearColorA colorExtendFlag = new PackedLinearColorA(128, 128, 128, 100);
        private PackedLinearColorA colorEndFlag = new PackedLinearColorA(0, 255, 0, 100);
        private PackedLinearColorA colorBaseFlag = new PackedLinearColorA(0, 0, 255, 100);
        private PackedLinearColorA colorArchwayFlag = new PackedLinearColorA(255, 0, 0, 100);

        private readonly ResourceRef<Material> _material;

        public SectorVisibilityRenderer()
        {
            _material = CreateMaterial(Tig.RenderingDevice);
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
            var vs = device.GetShaders().LoadVertexShader("diffuse_only_vs");
            var ps = device.GetShaders().LoadPixelShader("diffuse_only_ps");

            return device.CreateMaterial(
                blendState,
                depthState,
                rasterizerState,
                Array.Empty<MaterialSamplerSpec>(),
                vs,
                ps).Ref();
        }

        private int map_sectorvb_flags = 0xF;

        [TempleDllLocation(0x100aaac0)]
        public void Render(TileRect tileRect)
        {
            if ((map_sectorvb_flags & 0xF) != 0)
            {
                Tig.RenderingDevice.SetMaterial(_material);
                Tig.RenderingDevice.SetVertexShaderConstant(0, StandardSlotSemantic.ViewProjMatrix);

                using var iterator = new SectorIterator(tileRect);

                while (iterator.HasNext)
                {
                    var sector = iterator.Next();

                    var visibility = GameSystems.SectorVisibility.Lock(sector.Loc);

                    var builder = new SubTileMeshBuilder(sector.Loc);

                    for (int y = 0; y < 192; y++)
                    {
                        for (int x = 0; x < 192; x++)
                        {
                            var flags = visibility[x, y];
                            if (flags.HasFlag(VisibilityFlags.Extend) && (map_sectorvb_flags & 1) != 0)
                            {
                                builder.Add(x, y, colorExtendFlag);
                            }

                            if (flags.HasFlag(VisibilityFlags.End) && (map_sectorvb_flags & 2) != 0)
                            {
                                builder.Add(x, y, colorEndFlag);
                            }

                            if (flags.HasFlag(VisibilityFlags.Base) && (map_sectorvb_flags & 4) != 0)
                            {
                                builder.Add(x, y, colorBaseFlag);
                            }

                            if (flags.HasFlag(VisibilityFlags.Archway) && (map_sectorvb_flags & 8) != 0)
                            {
                                builder.Add(x, y, colorArchwayFlag);
                            }
                        }
                    }

                    var mesh = builder.Build(_material.Resource.VertexShader);
                    mesh.Render(Tig.RenderingDevice);
                }
            }
        }

        public void Dispose()
        {
            _material.Dispose();
        }
    }
}