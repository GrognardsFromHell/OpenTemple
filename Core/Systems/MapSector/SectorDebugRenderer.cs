using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.Materials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.MapSector
{
    /// <summary>
    /// Renders information about tiles, specifically their flags.
    /// </summary>
    public class SectorDebugRenderer : IDisposable
    {
        private readonly Dictionary<SectorLoc, SectorDebugState> _sectorDebugState;

        private readonly RenderingDevice _device = Tig.RenderingDevice;

        private readonly ResourceRef<Material> _material;

        public SectorDebugRenderer()
        {
            _sectorDebugState = new Dictionary<SectorLoc, SectorDebugState>();
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

            using var sectorIt = new SectorIterator(tileRect);
            foreach (var sector in sectorIt.EnumerateSectors())
            {
                if (!sector.IsValid)
                {
                    continue;
                }

                if (!_sectorDebugState.TryGetValue(sector.Loc, out var renderingState))
                {
                    renderingState = new SectorDebugState(_device, _material.Resource.VertexShader, sector.Sector);
                    _sectorDebugState.Add(sector.Loc, renderingState);
                }

                _device.SetVertexShaderConstant(0, StandardSlotSemantic.ViewProjMatrix);
                renderingState.BufferBinding.Resource.Bind();
                _device.SetIndexBuffer(renderingState.IndexBuffer);
                _device.DrawIndexed(PrimitiveType.TriangleList,
                    renderingState.VertexCount, renderingState.IndexCount);
            }
        }

        public void Dispose()
        {
            _material.Dispose();
        }

        private class SectorDebugState
        {
            public readonly ResourceRef<BufferBinding> BufferBinding;

            public readonly ResourceRef<IndexBuffer> IndexBuffer;

            public readonly ResourceRef<VertexBuffer> VertexBuffer;

            public readonly int VertexCount;

            public readonly int IndexCount;

            public SectorDebugState(RenderingDevice device, VertexShader shader, Sector sector)
            {
                // Each sub-tile needs it's own 4 vertices because the vertex colors would overlap
                const int subtilesPerTile = 3 * 3;
                const int vertexCount = 4 * Sector.TilesPerSector * subtilesPerTile;
                Span<Vertex> vertices = new Vertex[vertexCount];
                Span<int> indices = new int[Sector.TilesPerSector * subtilesPerTile * 6];
                int vertexIdx = 0;
                int indexIdx = 0;

                var basePos = sector.secLoc.GetBaseTile().ToInches2D();

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

                                var color = new PackedLinearColorA(0, 0, 0, 0);
                                if (blocking)
                                {
                                    color = new PackedLinearColorA(200, 0, 0, 127);
                                }
                                else if (flyover)
                                {
                                    color = new PackedLinearColorA(127, 0, 127, 127);
                                }

                                var subtileX = basePos.X + x * locXY.INCH_PER_TILE + dx * locXY.INCH_PER_SUBTILE;
                                var subtileY = basePos.Y + y * locXY.INCH_PER_TILE + dy * locXY.INCH_PER_SUBTILE;

                                int topLeftIdx = vertexIdx++;
                                int topRightIdx = vertexIdx++;
                                int bottomRightIdx = vertexIdx++;
                                int bottomLeftIdx = vertexIdx++;

                                vertices[topLeftIdx] = new Vertex
                                {
                                    diffuse = color,
                                    pos = new Vector4(subtileX, 0, subtileY, 1)
                                };
                                vertices[topRightIdx] = new Vertex
                                {
                                    diffuse = color,
                                    pos = new Vector4(subtileX + locXY.INCH_PER_SUBTILE, 0, subtileY, 1)
                                };
                                vertices[bottomRightIdx] = new Vertex
                                {
                                    diffuse = color,
                                    pos = new Vector4(subtileX + locXY.INCH_PER_SUBTILE, 0,
                                        subtileY + locXY.INCH_PER_SUBTILE, 1)
                                };
                                vertices[bottomLeftIdx] = new Vertex
                                {
                                    diffuse = color,
                                    pos = new Vector4(subtileX, 0, subtileY + locXY.INCH_PER_SUBTILE, 1)
                                };

                                indices[indexIdx++] = topLeftIdx;
                                indices[indexIdx++] = bottomLeftIdx;
                                indices[indexIdx++] = bottomRightIdx;
                                indices[indexIdx++] = bottomRightIdx;
                                indices[indexIdx++] = topRightIdx;
                                indices[indexIdx++] = topLeftIdx;
                            }
                        }
                    }
                }

                Trace.Assert(indexIdx == indices.Length);
                Trace.Assert(vertexIdx == vertices.Length);

                VertexCount = vertexIdx;
                IndexCount = indexIdx;
                VertexBuffer = device.CreateVertexBuffer<Vertex>(vertices);
                IndexBuffer = device.CreateIndexBuffer(indices);

                BufferBinding = new BufferBinding(device, shader).Ref();
                BufferBinding.Resource.AddBuffer<Vertex>(VertexBuffer, 0)
                    .AddElement(VertexElementType.Float4, VertexElementSemantic.Position)
                    .AddElement(VertexElementType.Color, VertexElementSemantic.Color);
            }

            public void Dispose()
            {
                VertexBuffer.Dispose();
                IndexBuffer.Dispose();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector4 pos;
            public PackedLinearColorA diffuse;
            public static readonly int Size = Marshal.SizeOf<Vertex>();
        }
    }
}