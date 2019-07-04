using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using SharpDX.DirectWrite;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.FogOfWar
{
    internal class MapFogdataChunk
    {
        public bool AllExplored;

        public byte[] Data = new byte[8192];
    }

    public class MapFoggingSystem : IGameSystem, IBufferResettingSystem, IResetAwareSystem
    {
        private readonly RenderingDevice mDevice;

        private const int sFogBufferDim = 102;

        [TempleDllLocation(0x10824468)]
        internal int mFogMinX;

        [TempleDllLocation(0x108EC4C8)]
        internal int mFogMinY;

        [TempleDllLocation(0x10820458)]
        internal int mSubtilesX;

        [TempleDllLocation(0x10824490)]
        internal int mSubtilesY;

        [TempleDllLocation(0x108254A0)]
        internal bool mFoggingEnabled;

        [TempleDllLocation(0x108A5498)]
        internal byte[] mFogCheckData;

        // Maximum line of sight radius
        private const int MaxLosRadius = 102;

        private const int MaxLosDiameter = MaxLosRadius * 2;

        // 8 entries, one for each controllable party member
        // The buffers themselves contain one byte per sub-tile within the creature's line of sight area
        // determined by MaxLosDiameter
        [TempleDllLocation(0x10824470)]
        private byte[][] mFogBuffers;

        [TempleDllLocation(0x11E61560)]
        private MapFogdataChunk[] mFogUnexploredData = new MapFogdataChunk[4];

        [TempleDllLocation(0x108EC590)]
        private bool mDoFullUpdate;

        [TempleDllLocation(0x102ACEFC)]
        private int mFogChecks;

        [TempleDllLocation(0x108EC6A0)]
        private int mScreenWidth => mScreenSize.Width;

        [TempleDllLocation(0x108EC6A4)]
        private int mScreenHeight => mScreenSize.Height;

        private Size mScreenSize;

        // TODO This seems to be fully unused
        [TempleDllLocation(0x102ACF00)]
        private int fogcol_field1;

        // TODO This is used in the townmap UI
        [TempleDllLocation(0x102ACF04)]
        private int fogcol_field2;

        [TempleDllLocation(0x102ACF08)]
        private int qword_102ACF08;

        [TempleDllLocation(0x102ACF10)]
        private int qword_102ACF10;

        public FogOfWarRenderer Renderer { get; }

        [TempleDllLocation(0x10032020)]
        public MapFoggingSystem(RenderingDevice renderingDevice)
        {
            mDevice = renderingDevice;

            mFogCheckData = null;

            mFoggingEnabled = true;
            mFogBuffers = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                mFogBuffers[i] = new byte[16 * sFogBufferDim * sFogBufferDim];
            }

            InitScreenBuffers();

            Renderer = new FogOfWarRenderer(this, renderingDevice);
        }

        private void InitScreenBuffers()
        {
            mScreenSize = mDevice.GetCamera().ScreenSize;

            UpdateFogLocation();
        }

        private void UpdateFogLocation()
        {
            var camera = mDevice.GetCamera();

            // Calculate the tile locations in each corner of the screen
            var topLeftLoc = camera.ScreenToTile(0, 0);
            var topRightLoc = camera.ScreenToTile(mScreenSize.Width, 0);
            var bottomLeftLoc = camera.ScreenToTile(0, mScreenSize.Height);
            var bottomRightLoc = camera.ScreenToTile(mScreenSize.Width, mScreenSize.Height);

            mFogMinX = topRightLoc.location.locx;
            mFogMinY = topLeftLoc.location.locy;

            // Whatever the point of this may be ...
            if (topLeftLoc.off_y < topLeftLoc.off_x || topLeftLoc.off_y < -topLeftLoc.off_x)
            {
                mFogMinY--;
            }

            mSubtilesX = (bottomLeftLoc.location.locx - mFogMinX + 3) * 3;
            mSubtilesY = (bottomRightLoc.location.locy - mFogMinY + 3) * 3;

            if (mFogCheckData == null || mFogCheckData.Length != mSubtilesX * mSubtilesY)
            {
                mFogCheckData = new byte[mSubtilesX * mSubtilesY];
                mDoFullUpdate = true;
            }
        }

        [TempleDllLocation(0x1002eb80)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1002ebd0)]
        public void Reset()
        {
            mDoFullUpdate = false;
            _explorationData.Clear();
        }

        [TempleDllLocation(0x108EC6A8)]
        private int fog_next_check_for_idx;

        [TempleDllLocation(0x1080FA88)]
        private locXY[] fog_check_locations = new locXY[32];

        [TempleDllLocation(0x108203C8)]
        private int[] fog_check_subtile_x = new int[32];

        [TempleDllLocation(0x108EC4D0)]
        private int[] fog_check_subtile_y = new int[32];

        // Specifies the X-component of the tile that is the origin of a party members line of sight buffer
        [TempleDllLocation(0x1080FB88)]
        private int[] fog_buffer_origin_x = new int[32];

        // Specifies the Y-component of the tile that is the origin of a party members line of sight buffer
        [TempleDllLocation(0x108EC550)]
        private int[] fog_buffer_origin_y = new int[32];

        // This is lazily populated
        [TempleDllLocation(0x108EC598)] [TempleDllLocation(0x108EC6B0)]
        private readonly Dictionary<SectorLoc, SectorExploration> _explorationData
            = new Dictionary<SectorLoc, SectorExploration>();

        private const float HalfSubtile = locXY.INCH_PER_SUBTILE / 2;

        private static int SubtileFromOffset(float offset)
        {
            if (offset < -HalfSubtile)
            {
                return 0;
            }
            else if (offset < HalfSubtile)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        [TempleDllLocation(0x10031ef0)]
        private SectorExploration GetOrLoadExploredSectorData(SectorLoc loc)
        {
            if (_explorationData.TryGetValue(loc, out var exploration))
            {
                return exploration;
            }

            exploration = GameSystems.Map.LoadSectorExploration(loc);
            _explorationData[loc] = exploration;
            return exploration;
        }

        [TempleDllLocation(0x10030f40)]
        public void FlushSectorExploration()
        {
            foreach (var (sectorLoc, exploration) in _explorationData)
            {
                GameSystems.Map.SaveSectorExploration(sectorLoc, exploration);
            }

            _explorationData.Clear();
        }

        public byte GetFogStatus(LocAndOffsets loc) => GetFogStatus(loc.location, loc.off_x, loc.off_y);

        [TempleDllLocation(0x100336B0)]
        public void PerformFogChecks()
        {
            if (!mFoggingEnabled || GameSystems.Party.PartySize <= 0)
            {
                return;
            }

            UpdateFogLocation();

            var updateScreenFogBuffer = 0;

            int numberOfFogChecks;
            if (mDoFullUpdate)
            {
                numberOfFogChecks = 8;
            }
            else
            {
                numberOfFogChecks = Globals.Config.FogChecksPerFrame;
            }

            for (var i = 0; i < numberOfFogChecks; i++)
            {
                var partyIndex = fog_next_check_for_idx;
                if (fog_next_check_for_idx >= GameSystems.Party.PartySize)
                {
                    partyIndex = 0;
                    fog_next_check_for_idx = 0;
                }

                fog_next_check_for_idx++;

                var partyMember = GameSystems.Party.GetPartyGroupMemberN(partyIndex);

                // Determine which sub-tile of the tile the critter is standing on
                var subtileX = SubtileFromOffset(partyMember.OffsetX);
                var subtileY = SubtileFromOffset(partyMember.OffsetY);

                if (!mDoFullUpdate)
                {
                    if (fog_check_locations[partyIndex] == partyMember.GetLocation()
                        && subtileX == fog_check_subtile_x[partyIndex]
                        && subtileY == fog_check_subtile_y[partyIndex])
                    {
                        continue;
                    }
                }

                updateScreenFogBuffer |= 1; // Some party member's fog buffer has changed
                fog_check_locations[partyIndex] = partyMember.GetLocation();
                fog_check_subtile_x[partyIndex] = SubtileFromOffset(partyMember.OffsetX);
                fog_check_subtile_y[partyIndex] = SubtileFromOffset(partyMember.OffsetY);

                CreateLineOfSightBuffer(partyMember, partyIndex);
            }

            mDoFullUpdate = false;

            var translationX = GameSystems.Location.LocationTranslationX;
            var translationY = GameSystems.Location.LocationTranslationY;
            if (qword_102ACF08 != translationX || qword_102ACF10 != translationY)
            {
                updateScreenFogBuffer |= 2; // Translation has changed
                qword_102ACF08 = translationX;
                qword_102ACF10 = translationY;
            }

            if (updateScreenFogBuffer != 0)
            {
                mFogCheckData.AsSpan().Fill(0);

                for (int i = 0; i < GameSystems.Party.PartySize; i++)
                {
                    MarkLineOfSight(i);
                }

                MarkExploredSubtiles();
            }
        }

        private MemoryPool<Vector2> _vertexPool = MemoryPool<Vector2>.Shared;

        [TempleDllLocation(0x100327a0)]
        private void CreateLineOfSightBuffer(GameObjectBody partyMember, int partyIndex)
        {
            // Lazily create the los buffer
            if (mFogBuffers[partyIndex] == null)
            {
                mFogBuffers[partyIndex] = new byte[MaxLosDiameter * MaxLosDiameter];
            }

            // Start out by clearing the LOS info
            Span<byte> losBuffer = mFogBuffers[partyIndex];
            losBuffer.Fill(0);

            var partyMemberPos = partyMember.GetLocation();
            var partyMemberX = partyMemberPos.locx;
            var partyMemberY = partyMemberPos.locy;
            var losRadiusTiles = MaxLosRadius / 3;

            fog_buffer_origin_x[partyIndex] = partyMemberX - losRadiusTiles;
            fog_buffer_origin_y[partyIndex] = partyMemberY - losRadiusTiles;

            var tilerect = new TileRect();
            tilerect.x1 = fog_buffer_origin_x[partyIndex];
            tilerect.x2 = tilerect.x1 + 2 * MaxLosRadius / 3;
            tilerect.y1 = fog_buffer_origin_y[partyIndex];
            tilerect.y2 = tilerect.y1 + 2 * MaxLosRadius / 3;

            PrepareLineOfSightBuffer(partyIndex, tilerect, losBuffer);

            ComputeLineOfSight(partyMemberPos, 60, partyIndex);

            ExtendLineOfSight(losBuffer);

            MarkLineOfSightAsExplored(partyIndex, tilerect, losBuffer);

            MarkTownmapTilesExplored(partyIndex, losBuffer);
        }

        private static void GetSectorOverlapWithTileRect(TileRect tilerect,
            SectorLoc sectorLoc,
            out Rectangle overlappingRect)
        {
            var sectorOrigin = sectorLoc.GetBaseTile();

            var minSectorTileX = Math.Max(0, tilerect.x1 - sectorOrigin.locx);
            var minSectorTileY = Math.Max(0, tilerect.y1 - sectorOrigin.locy);
            var maxSectorTileX = Math.Min(Sector.SectorSideSize, tilerect.x2 - sectorOrigin.locx + 1);
            var maxSectorTileY = Math.Min(Sector.SectorSideSize, tilerect.y2 - sectorOrigin.locy + 1);

            overlappingRect = new Rectangle(
                minSectorTileX,
                minSectorTileY,
                maxSectorTileX - minSectorTileX,
                maxSectorTileY - minSectorTileY
            );
        }

        private void PrepareLineOfSightBuffer(int partyIndex, TileRect tilerect, Span<byte> losBuffer)
        {
            using var sectorIterator = new SectorIterator(tilerect);

            while (sectorIterator.HasNext)
            {
                var sector = sectorIterator.Next();
                if (!sector.IsValid)
                {
                    continue;
                }

                var sectorOrigin = sector.Loc.GetBaseTile();

                var visibility = GameSystems.SectorVisibility.Lock(sector.Loc);

                GetSectorOverlapWithTileRect(tilerect, sector.Loc, out var overlappingRect);

                MarkClosedDoorsAsVisibilityBlockers(sector, tilerect, losBuffer, overlappingRect);

                /*
                 * The following loop will mark all blocking subtiles within line of sight of the critter,
                 * as well as copy the sector visibility flags.
                 */
                for (var tileY = overlappingRect.Top; tileY < overlappingRect.Bottom; tileY++)
                {
                    for (var tileX = overlappingRect.Left; tileX < overlappingRect.Right; tileX++)
                    {
                        // The coordinates of this tile in the part member's fog buffer
                        var yIndex = sectorOrigin.locy + tileY - fog_buffer_origin_y[partyIndex];
                        var xIndex = sectorOrigin.locx + tileX - fog_buffer_origin_x[partyIndex];

                        var tileFlags = sector.Sector.tilePkt.tiles[Sector.GetSectorTileIndex(tileX, tileY)].flags;

                        for (var subtileY = 0; subtileY < 3; subtileY++)
                        {
                            for (var subtileX = 0; subtileX < 3; subtileX++)
                            {
                                ref var losTile =
                                    ref losBuffer[(3 * yIndex + subtileY) * MaxLosDiameter + 3 * xIndex + subtileX];

                                var flag = SectorTile.GetBlockingFlag(subtileX, subtileY);
                                if ((tileFlags & flag) != 0)
                                {
                                    losTile = LOSBUFFER_BLOCKING;
                                }

                                var visibilityFlags = visibility[tileX * 3 + subtileX, tileY * 3 + subtileY];
                                if ((visibilityFlags & VisibilityFlags.Extend) != 0)
                                {
                                    losTile |= LOSBUFFER_EXTEND;
                                }

                                if ((visibilityFlags & VisibilityFlags.End) != 0)
                                {
                                    losTile |= LOSBUFFER_END;
                                }

                                if ((visibilityFlags & VisibilityFlags.Base) != 0)
                                {
                                    losTile |= LOSBUFFER_BASE;
                                }

                                if ((visibilityFlags & VisibilityFlags.Archway) != 0)
                                {
                                    losTile |= LOSBUFFER_ARCHWAY;
                                }
                            }
                        }
                    }
                }

                GameSystems.SectorVisibility.Unlock(sector.Loc);
            }
        }

        private void MarkClosedDoorsAsVisibilityBlockers(LockedMapSector sector,
            TileRect tilerect, Span<byte> losBuffer, Rectangle overlappingRect)
        {
            var minXWorld = tilerect.x1 * locXY.INCH_PER_TILE;
            var minYWorld = tilerect.y1 * locXY.INCH_PER_TILE;

            /*
             * The following loop will mark tiles occupied by closed doors within line of sight as blocking.
             */
            for (var tileY = overlappingRect.Top; tileY < overlappingRect.Bottom; tileY++)
            {
                for (var tileX = overlappingRect.Left; tileX < overlappingRect.Right; tileX++)
                {
                    var objects = sector.GetObjectsAt(tileX, tileY);
                    foreach (var obj in objects)
                    {
                        if (obj.type == ObjectType.portal && obj.IsPortalOpen())
                        {
                            var animHandle = obj.GetOrCreateAnimHandle();
                            if (animHandle == null)
                            {
                                continue;
                            }

                            var animParams = obj.GetAnimParams();

                            var materials = animHandle.GetSubmeshes();
                            for (var submeshIdx = 0; submeshIdx < materials.Length; submeshIdx++)
                            {
                                var submesh = animHandle.GetSubmesh(animParams, submeshIdx);

                                using var vertexBufferHandle = _vertexPool.Rent(submesh.VertexCount);
                                var vertexBuffer = vertexBufferHandle.Memory.Span;

                                var meshVertices = submesh.Positions;
                                for (var i = 0; i < submesh.VertexCount; i++)
                                {
                                    vertexBuffer[i].X = (meshVertices[i].X - minXWorld) / locXY.INCH_PER_TILE;
                                    vertexBuffer[i].Y = (meshVertices[i].Z - minYWorld) / locXY.INCH_PER_TILE;
                                }

                                var primIdx = 0;
                                var indices = submesh.Indices;
                                for (var i = 0; i < submesh.PrimitiveCount; i++)
                                {
                                    Span<Vector2> vertices = stackalloc Vector2[3]
                                    {
                                        vertexBuffer[indices[primIdx++]],
                                        vertexBuffer[indices[primIdx++]],
                                        vertexBuffer[indices[primIdx++]]
                                    };

                                    TriangleRasterizer.Rasterize(MaxLosDiameter,
                                        MaxLosDiameter, losBuffer, vertices, 8);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Now we mark the line of sight as explored for the purposes of the town map, which uses
        // a greatly reduced resolution and does not consider actual current line of sight apparently
        private void MarkTownmapTilesExplored(int partyIndex, Span<byte> losBuffer)
        {
            var camera = Tig.RenderingDevice.GetCamera();

            var losDiameterTiles = MaxLosDiameter / 3;

            for (var tileY = 0; tileY < losDiameterTiles; tileY++)
            {
                for (var tileX = 0; tileX < losDiameterTiles; tileX++)
                {
                    if ((losBuffer[(tileX + tileY * MaxLosDiameter) * 3] & LOSBUFFER_UNK) != 0)
                    {
                        var loc = new locXY(fog_buffer_origin_x[partyIndex] + tileX,
                            fog_buffer_origin_y[partyIndex] + tileY);

                        var locWorld = camera.TileToWorld(loc);
                        var townmapX = (int) locWorld.X + 8448;
                        var townmapY = (int) locWorld.Y - 6000;

                        var gridX = townmapX / 10240;
                        var gridY = townmapY / 10240;

                        var townmapPixelX = townmapX % 10240 / 40;
                        var townmapPixelY = townmapY % 10240 / 40;

                        var mapIndex = gridX + 2 * gridY;
                        var unexploredTiles = mFogUnexploredData[mapIndex];
                        var byteIndex = 32 * townmapPixelY + townmapPixelX / 8;
                        var mask = (byte) (1 << (townmapPixelX % 8));
                        unexploredTiles.Data[byteIndex] |= mask;
                    }
                }
            }
        }

        // Mark the current line of sight of a party member as explored for the purposes of fog of war
        private void MarkLineOfSightAsExplored(int partyIndex, TileRect tilerect, Span<byte> losBuffer)
        {
            using var sectorIterator = new SectorIterator(tilerect);

            while (sectorIterator.HasNext)
            {
                var sector = sectorIterator.Next();
                if (!sector.IsValid)
                {
                    continue;
                }

                var sectorOrigin = sector.Loc.GetBaseTile();
                var exploredData = GetOrLoadExploredSectorData(sector.Loc);

                if (exploredData.State == SectorExplorationState.AllExplored)
                {
                    continue; // Nothing left to mark
                }

                var minSectorTileX = Math.Max(0, tilerect.x1 - sectorOrigin.locx);
                var minSectorTileY = Math.Max(0, tilerect.y1 - sectorOrigin.locy);
                var maxSectorTileX = Math.Min(Sector.SectorSideSize, tilerect.x2 - sectorOrigin.locx + 1);
                var maxSectorTileY = Math.Min(Sector.SectorSideSize, tilerect.y2 - sectorOrigin.locy + 1);

                // Mark anything that's explored by the player on the persistent explored sector data bitmap
                for (var subtileY = minSectorTileY * 3; subtileY < maxSectorTileY * 3; subtileY++)
                {
                    for (var subtileX = minSectorTileX * 3; subtileX < maxSectorTileX * 3; subtileX++)
                    {
                        // The coordinates of this tile in the part member's fog buffer
                        var yIndex = (sectorOrigin.locy - fog_buffer_origin_y[partyIndex]) * 3 + subtileY;
                        var xIndex = (sectorOrigin.locx - fog_buffer_origin_x[partyIndex]) * 3 + subtileX;

                        if ((losBuffer[yIndex * MaxLosDiameter + xIndex] & LOSBUFFER_UNK) != 0)
                        {
                            exploredData.MarkExplored(subtileX, subtileY);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100326d0)]
        private void ComputeLineOfSight(locXY loc, int unk, int partyIndex)
        {
            var v5 = fog_check_subtile_x[partyIndex] + 3 * (loc.locx - fog_buffer_origin_x[partyIndex]);
            var v7 = fog_check_subtile_y[partyIndex] + 3 * (loc.locy - fog_buffer_origin_y[partyIndex]);
            var losBuffer = mFogBuffers[partyIndex].AsSpan();

            var v8 = 0;
            if (v8 <= MaxLosDiameter)
            {
                do
                {
                    fog_perform_fog_checks_3(v5, v7, v8, 0, unk, losBuffer);
                    fog_perform_fog_checks_3(v5, v7, v8, MaxLosDiameter - 1, unk, losBuffer);
                    ++v8;
                } while (v8 <= MaxLosDiameter);
            }

            var v9 = 0;
            if (v9 <= MaxLosDiameter)
            {
                do
                {
                    fog_perform_fog_checks_3(v5, v7, 0, v9, unk, losBuffer);
                    fog_perform_fog_checks_3(v5, v7, MaxLosDiameter - 1, v9, unk, losBuffer);
                    ++v9;
                } while (v9 <= MaxLosDiameter);
            }
        }

        [TempleDllLocation(0x108EC698)]
        private bool dword_108EC698;

        [TempleDllLocation(0x100310b0)]
        private void fog_perform_fog_checks_3(int a1, int a2, int a3, int a4, int a5, Span<byte> losBuffer)
        {
            int v6; // ebp@1
            int v7; // esi@1
            int v8; // edi@1
            int v9; // eax@1
            int v10; // ecx@1
            float v11; // fst7@1
            int v12; // ebx@26
            int v13; // ecx@32
            int v14; // ebx@34
            int v15; // edx@40
            int v16; // ebx@44
            int v17; // ecx@50
            int v18; // ebx@52
            int v19; // ecx@58
            int v20; // eax@60
            int v21; // ebx@63
            int v22; // ebx@68
            int v23; // ecx@70
            int v24; // ebp@74
            int v25; // ebx@74
            int v26; // ecx@80
            int v27; // ebx@81
            int v28; // ebp@82
            int v29; // ebx@87
            int v30; // ecx@92
            int v31; // ebx@94
            int v32; // ebp@95
            int v33; // eax@98
            int v34; // ebx@104
            int v35; // esi@104
            int v36; // ebp@105
            int v37; // edi@105
            int v38; // ebp@112
            int v39; // ebx@112
            int v40; // edi@113
            int v41; // esi@113
            int v42; // [sp+10h] [bp-14h]@1
            int v43; // [sp+14h] [bp-10h]@1
            int v44; // [sp+18h] [bp-Ch]@60
            int v45; // [sp+18h] [bp-Ch]@81
            int v46; // [sp+1Ch] [bp-8h]@27
            int v47; // [sp+1Ch] [bp-8h]@35
            int v48; // [sp+1Ch] [bp-8h]@45
            int v49; // [sp+1Ch] [bp-8h]@53
            int v50; // [sp+20h] [bp-4h]@27
            int v51; // [sp+20h] [bp-4h]@35
            int v52; // [sp+20h] [bp-4h]@45
            int v53; // [sp+20h] [bp-4h]@53
            int v54; // [sp+20h] [bp-4h]@68
            int v55; // [sp+28h] [bp+4h]@94
            int v56; // [sp+2Ch] [bp+8h]@82
            int v57; // [sp+2Ch] [bp+8h]@95

            v6 = a5;
            v7 = a2;
            v8 = a1;
            v9 = a3 - a1;
            v10 = a4 - a2;
            v42 = a4 - a2;
            v43 = a3 - a1;
            dword_108EC698 = false;
            v11 = (a4 - a2) / (float) (a3 - a1);
            if (a4 == a2)
            {
                if (a1 >= a3)
                {
                    if (a1 > a1 - a5)
                    {
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, a2))
                                break;
                            --v8;
                        } while (v8 > a1 - a5);
                    }
                }
                else if (a1 < a1 + a5)
                {
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, v8, a2))
                            break;
                        ++v8;
                    } while (v8 < a1 + a5);
                }
            }
            else if (a3 == a1)
            {
                if (a2 >= a4)
                {
                    if (a2 > a2 - a5)
                    {
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, a1, v7))
                                break;
                            --v7;
                        } while (v7 > a2 - a5);
                    }
                }
                else if (a2 < a2 + a5)
                {
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, a1, v7))
                            break;
                        ++v7;
                    } while (v7 < a2 + a5);
                }
            }
            else if (v9 == v10 || v9 == -v10)
            {
                v20 = a5 * a5;
                v44 = a5 * a5;
                if (a1 >= a3)
                {
                    if (a2 >= a4)
                    {
                        if (v20 >= 0)
                        {
                            v29 = 0;
                            do
                            {
                                if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                    break;
                                --v8;
                                --v7;
                                --v29;
                            } while (2 * v29 * v29 <= v44);
                        }
                    }
                    else if (v20 >= 0)
                    {
                        v24 = 0;
                        v25 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            --v8;
                            --v25;
                            ++v7;
                            ++v24;
                        } while (v24 * v24 + v25 * v25 <= v44);

                        v6 = a5;
                    }
                }
                else if (a2 >= a4)
                {
                    if (v20 >= 0)
                    {
                        v22 = 0;
                        v54 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            ++v8;
                            ++v22;
                            --v7;
                            v23 = (v54 - 1) * (v54 - 1);
                            --v54;
                        } while (v23 + v22 * v22 <= v44);
                    }
                }
                else if (v20 >= 0)
                {
                    v21 = 0;
                    do
                    {
                        if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                            break;
                        ++v8;
                        ++v7;
                        ++v21;
                    } while (2 * v21 * v21 <= v44);
                }
            }
            else if ((0.0 < v11) && (v11 < 1.0))
            {
                if (a1 >= a3)
                {
                    v14 = v43 - 2 * v42;
                    if (a5 * a5 > 0)
                    {
                        v47 = 0;
                        v51 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v14 >= 0)
                            {
                                --v7;
                                --v47;
                                v14 += 2 * (v43 - v42);
                            }
                            else
                            {
                                v14 -= 2 * v42;
                            }

                            --v8;
                            v15 = (v51 - 1) * (v51 - 1);
                            --v51;
                        } while (v47 * v47 + v15 < a5 * a5);
                    }
                }
                else
                {
                    v12 = 2 * v42 - v43;
                    if (a5 * a5 > 0)
                    {
                        v46 = 0;
                        v50 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v12 >= 0)
                            {
                                ++v7;
                                ++v46;
                                v12 += 2 * (v42 - v43);
                            }
                            else
                            {
                                v12 += 2 * v42;
                            }

                            ++v8;
                            v13 = (v50 + 1) * (v50 + 1);
                            ++v50;
                        } while (v46 * v46 + v13 < a5 * a5);
                    }
                }
            }
            else if (v11 > 1.0)
            {
                v7 = a2;
                v8 = a1;
                if (a2 >= a4)
                {
                    v18 = v42 - 2 * v43;
                    if (a5 * a5 > 0)
                    {
                        v53 = 0;
                        v49 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v18 >= 0)
                            {
                                --v8;
                                --v49;
                                v18 += 2 * (v42 - v43);
                            }
                            else
                            {
                                v18 -= 2 * v43;
                            }

                            --v7;
                            v19 = (v53 - 1) * (v53 - 1);
                            --v53;
                        } while (v19 + v49 * v49 < a5 * a5);
                    }
                }
                else
                {
                    v16 = 2 * v43 - v42;
                    if (a5 * a5 > 0)
                    {
                        v52 = 0;
                        v48 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v16 >= 0)
                            {
                                ++v8;
                                ++v48;
                                v16 += 2 * (v43 - v42);
                            }
                            else
                            {
                                v16 += 2 * v43;
                            }

                            ++v7;
                            v17 = (v52 + 1) * (v52 + 1);
                            ++v52;
                        } while (v17 + v48 * v48 < a5 * a5);
                    }
                }
            }

            if ((-1.0 < v11) && (v11 < 0.0))
            {
                v26 = a1;
                if (a1 >= a3)
                {
                    v55 = v43 + 2 * v42;
                    v31 = v6 * v6;
                    if ((v8 - v26) * (v8 - v26) + (v7 - a2) * (v7 - a2) < v6 * v6)
                    {
                        v32 = v7 - a2;
                        v57 = v8 - v26;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v55 >= 0)
                            {
                                ++v7;
                                ++v32;
                                v33 = v55 + 2 * (v43 + v42);
                            }
                            else
                            {
                                v33 = 2 * v42 + v55;
                            }

                            v55 = v33;
                            --v8;
                            --v57;
                        } while (v32 * v32 + v57 * v57 < v31);
                    }
                }
                else
                {
                    v45 = v6 * v6;
                    v27 = -(v43 + 2 * v42);
                    if ((v8 - a1) * (v8 - a1) + (v7 - a2) * (v7 - a2) < v6 * v6)
                    {
                        v28 = v7 - a2;
                        v56 = v8 - a1;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v8, v7))
                                break;
                            if (v27 >= 0)
                            {
                                --v7;
                                --v28;
                                v27 += -2 * (v43 + v42);
                            }
                            else
                            {
                                v27 -= 2 * v42;
                            }

                            ++v8;
                            v30 = (v56 + 1) * (v56 + 1);
                            ++v56;
                        } while (v28 * v28 + v30 < v45);
                    }
                }
            }
            else if ((v11 < -1.0))
            {
                if (a2 >= a4)
                {
                    v38 = v6 * v6;
                    v39 = v42 + 2 * v43;
                    if (v38 > 0)
                    {
                        v40 = 0;
                        v41 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v41 + a1, v40 + a2))
                                break;
                            if (v39 >= 0)
                            {
                                ++v41;
                                v39 += 2 * (v43 + v42);
                            }
                            else
                            {
                                v39 += 2 * v43;
                            }

                            --v40;
                        } while (v40 * v40 + v41 * v41 < v38);
                    }
                }
                else
                {
                    v34 = v6 * v6;
                    v35 = -(v42 + 2 * v43);
                    if (v6 * v6 > 0)
                    {
                        v36 = 0;
                        v37 = 0;
                        do
                        {
                            if (!fog_perform_fog_checks_4(losBuffer, v37 + a1, a2 + v36))
                                break;
                            if (v35 >= 0)
                            {
                                --v37;
                                v35 += -2 * (v43 + v42);
                            }
                            else
                            {
                                v35 -= 2 * v43;
                            }

                            ++v36;
                        } while (v36 * v36 + v37 * v37 < v34);
                    }
                }
            }
        }

        [TempleDllLocation(0x10820448)]
        private byte[] byte_10820448 = new byte[9];

        private bool fog_perform_fog_checks_4(Span<byte> losBuffer, int a2, int a3)
        {
            var v3 = a3 * MaxLosDiameter + a2;
            if (dword_108EC698)
            {
                if (a2 >= 1)
                {
                    if (a2 < MaxLosDiameter - 1 && a3 >= 1 && a3 < MaxLosDiameter - 1)
                    {
                        var v14 = v3 - MaxLosDiameter - 1;
                        var v16 = 0;
                        LABEL_15:
                        var v17 = 0;
                        while (byte_10820448[v16 + v17] == 0 || (losBuffer[v14] & 8) != 0)
                        {
                            byte_10820448[v16 + v17++] = (byte) (losBuffer[v14++] & 8);
                            if (v17 >= 3)
                            {
                                v16 += 3;
                                v14 += MaxLosDiameter - 2;
                                if (v16 < 9)
                                    goto LABEL_15;
                                losBuffer[v3] |= 3;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }


            if ((losBuffer[v3] & 8) == 0)
            {
                losBuffer[v3] |= 3;
                return true;
            }

            if (a2 < 1)
                return false;
            if (a2 >= MaxLosDiameter - 1 || a3 < 1 || a3 >= MaxLosDiameter - 1)
                return false;

            // This seems to copy over a 3x3 area into the temp buffer
            var v6 = v3 - MaxLosDiameter - 1;
            for (var v7 = 0; v7 < 9; v7 += 3)
            {
                byte_10820448[v7] = (byte) (losBuffer[v6] & 8);
                byte_10820448[v7 + 1] = (byte) (losBuffer[v6 + 1] & 8);
                byte_10820448[v7 + 2] = (byte) (losBuffer[v6 + 2] & 8);
                v6 += MaxLosDiameter;
            }

            losBuffer[v3] |= 3;
            dword_108EC698 = true;
            return true;
        }

        private const byte LOSBUFFER_UNK = 2;
        private const byte LOSBUFFER_BLOCKING = 0x8;
        private const byte LOSBUFFER_EXTEND = 0x10;
        private const byte LOSBUFFER_END = 0x20;
        private const byte LOSBUFFER_BASE = 0x40;
        private const byte LOSBUFFER_ARCHWAY = 0x80;

        [TempleDllLocation(0x100317e0)]
        private void ExtendLineOfSight(Span<byte> losBuffer)

        {
            for (var y = 0; y < MaxLosDiameter; y++)
            {
                var iVar5 = (y - 1) * MaxLosDiameter;
                var local_8 = y * MaxLosDiameter;
                for (var x = 0; x < MaxLosDiameter; x++)
                {
                    var bVar7 = losBuffer[x + local_8];
                    if ((bVar7 & LOSBUFFER_BLOCKING) != 0)
                    {
                        // This might be (-1, -1) in coordinate terms
                        var pbVar8 = iVar5 + x - 1;
                        if ((bVar7 & LOSBUFFER_UNK) == 0)
                        {
                            var iVar6 = y;
                            if (x < y)
                            {
                                iVar6 = x;
                            }

                            var endChain = false;
                            var iVar10 = 0;
                            for (var i = 0; i < iVar6; i++)
                            {
                                var bVar1 = losBuffer[pbVar8];
                                if ((bVar1 & (LOSBUFFER_EXTEND | LOSBUFFER_END)) == 0)
                                    break;

                                switch (iVar10)
                                {
                                    case 0:
                                        if ((bVar1 & LOSBUFFER_BLOCKING) == 0)
                                        {
                                            iVar10 = 1;
                                        }

                                        break;
                                    case 1:
                                        if ((bVar1 & LOSBUFFER_BLOCKING) != 0)
                                        {
                                            // This is weird because it is checking the previous tile in the chain
                                            if ((bVar7 & LOSBUFFER_END) != 0)
                                            {
                                                endChain = true;
                                                break;
                                            }

                                            iVar10 = 2;
                                        }

                                        break;
                                    case 2:
                                        if ((bVar1 & LOSBUFFER_END) != 0)
                                        {
                                            iVar10 = 3;
                                        }

                                        break;
                                    case 3:
                                        if ((bVar1 & LOSBUFFER_END) == 0)
                                        {
                                            endChain = true;
                                            break;
                                        }

                                        break;
                                }

                                if (endChain)
                                {
                                    break;
                                }

                                if ((bVar1 & LOSBUFFER_ARCHWAY) == 0)
                                {
                                    losBuffer[pbVar8] = (byte) (bVar1 & ~LOSBUFFER_UNK);
                                }

                                // Moving diagonally to the upper left????
                                pbVar8 -= MaxLosDiameter + 1;
                                bVar7 = bVar1;
                            }
                        }
                        else
                        {
                            var iVar9 = y;
                            if (x < y)
                            {
                                iVar9 = x;
                            }

                            var iVar6 = 0;
                            for (var i = 0; i < iVar9; i++)
                            {
                                var bVar1 = losBuffer[pbVar8];
                                if ((bVar1 & (LOSBUFFER_EXTEND | LOSBUFFER_END)) == 0)
                                    break;

                                var endChain = false;
                                switch (iVar6)
                                {
                                    case 0:
                                        iVar6 = 1;
                                        break;
                                    case 1:
                                        if ((bVar1 & LOSBUFFER_BLOCKING) != 0)
                                        {
                                            // This is weird because it is checking the previous tile in the chain
                                            if ((bVar7 & LOSBUFFER_END) != 0)
                                            {
                                                endChain = true;
                                                break;
                                            }

                                            iVar6 = 2;
                                        }

                                        break;
                                    case 2:
                                        if ((bVar1 & LOSBUFFER_END) != 0)
                                        {
                                            iVar6 = 3;
                                        }

                                        break;
                                    case 3:
                                        if ((bVar1 & LOSBUFFER_END) == 0)
                                        {
                                            endChain = true;
                                            break;
                                        }

                                        break;
                                }

                                if (endChain)
                                {
                                    break;
                                }

                                if ((bVar1 & LOSBUFFER_ARCHWAY) == 0)
                                {
                                    losBuffer[pbVar8] |= LOSBUFFER_UNK;
                                }

                                // This could be up one tile and left one tile
                                pbVar8 -= MaxLosDiameter + 1;
                                bVar7 = bVar1;
                            }
                        }
                    }

                    if ((losBuffer[x + local_8] & LOSBUFFER_BASE) != 0)
                    {
                        var bVar4 = false;
                        if ((losBuffer[x + local_8] & LOSBUFFER_UNK) == 0)
                        {
                            // This starts one tile up and one tile left again...
                            var pbVar8 = x + iVar5 - 1;
                            var iVar6 = Math.Min(y, x);

                            for (var iVar9 = 0; iVar9 < iVar6; iVar9++)
                            {
                                bVar7 = losBuffer[pbVar8];
                                if (bVar4)
                                {
                                    if ((bVar7 & LOSBUFFER_ARCHWAY) != 0)
                                    {
                                        losBuffer[pbVar8] &= unchecked((byte) ~LOSBUFFER_UNK);
                                    }

                                    break;
                                }
                                else
                                {
                                    if ((bVar7 & LOSBUFFER_ARCHWAY) != 0)
                                    {
                                        bVar4 = true;
                                        losBuffer[pbVar8] &= unchecked((byte) ~LOSBUFFER_UNK);
                                    }
                                }

                                // Again, one row up, one column left
                                pbVar8 -= (MaxLosDiameter + 1);
                            }
                        }
                        else
                        {
                            var pbVar8 = x + iVar5 - 1;
                            var iVar6 = Math.Min(y, x);

                            for (var iVar9 = 0; iVar9 < iVar6; iVar9++)
                            {
                                bVar7 = losBuffer[pbVar8];
                                if (bVar4)
                                {
                                    if ((bVar7 & LOSBUFFER_ARCHWAY) != 0)
                                    {
                                        losBuffer[pbVar8] |= LOSBUFFER_UNK;
                                    }

                                    break;
                                }
                                else
                                {
                                    if ((bVar7 & LOSBUFFER_ARCHWAY) != 0)
                                    {
                                        bVar4 = true;
                                        losBuffer[pbVar8] |= 2;
                                    }
                                }

                                // Again, one row up, one column left
                                pbVar8 -= (MaxLosDiameter + 1);
                            }
                        }
                    }
                }
            }
        }


        private void MarkExploredSubtiles()
        {
            var sectorLocMin = new SectorLoc(new locXY(mFogMinX, mFogMinY));
            var sectorLocMax = new SectorLoc(new locXY(mFogMinX + mSubtilesX / 3 - 1, mFogMinY + mSubtilesY / 3 - 1));

            var fogCheckData = mFogCheckData.AsSpan();

            for (var secY = sectorLocMin.Y; secY <= sectorLocMax.Y; secY++)
            {
                for (var secX = sectorLocMin.X; secX <= sectorLocMax.X; secX++)
                {
                    var sectorLoc = new SectorLoc(secX, secY);

                    var sectorExploration = GetOrLoadExploredSectorData(sectorLoc);
                    if (sectorExploration.State != SectorExplorationState.Unexplored)
                    {
                        var sectorOriginLoc = sectorLoc.GetBaseTile();

                        var startSubtileX = 3 * (mFogMinX - sectorOriginLoc.locx);
                        var startSubtileY = 3 * (mFogMinY - sectorOriginLoc.locy);
                        var endSubtileX = startSubtileX + mSubtilesX;
                        var endSubtileY = startSubtileY + mSubtilesY;

                        if (startSubtileX < 0)
                        {
                            startSubtileX = 0;
                        }

                        if (startSubtileY < 0)
                        {
                            startSubtileY = 0;
                        }

                        if (endSubtileX > 192)
                        {
                            endSubtileX = 192;
                        }

                        if (endSubtileY > 192)
                        {
                            endSubtileY = 192;
                        }

                        if (startSubtileY < endSubtileY && startSubtileX < endSubtileX)
                        {
                            for (var y = startSubtileY; y < endSubtileY; y++)
                            {
                                var idx = (y + 3 * (sectorOriginLoc.locy - mFogMinY)) * mSubtilesX
                                    + 3 * (sectorOriginLoc.locx - mFogMinX);
                                for (var x = startSubtileX; x < endSubtileX; x++)
                                {
                                    if (sectorExploration.IsExplored(x, y))
                                    {
                                        fogCheckData[idx + x] |= 4;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10031E00)]
        private void MarkLineOfSight(int partyIdx)
        {
            // TODO: This needs to be cleaned up
            var v1 = fog_buffer_origin_x[partyIdx];
            var v2 = fog_buffer_origin_y[partyIdx];
            var v3 = 3 * (mFogMinX - v1);
            var v4 = 3 * (mFogMinY - v2);
            var v14 = v3 + mSubtilesX;
            var v16 = v4 + mSubtilesY;

            if (v3 < 0)
            {
                v3 = 0;
            }

            if (v4 < 0)
            {
                v4 = 0;
            }

            if (v14 > MaxLosDiameter)
            {
                v14 = MaxLosDiameter;
            }

            if (v16 > MaxLosDiameter)
            {
                v16 = MaxLosDiameter;
            }

            if (v4 < v16 && v3 < v14)
            {
                var v15 = v3 + mSubtilesX - v14;

                var fogDataOut = mFogCheckData.AsSpan().Slice(
                    3 * (v1 - mFogMinX) + v3
                                        + mSubtilesX * (v4 + 3 * (v2 - mFogMinY))
                );
                var idx = 0;

                ReadOnlySpan<byte> fogBuffer = mFogBuffers[partyIdx];
                var v9 = v4 * MaxLosDiameter + v3;
                var v10 = v3 + MaxLosDiameter - v14;
                var v11 = v14 - v3;
                var v12 = v16 - v4;
                do
                {
                    var v13 = v11;
                    do
                    {
                        fogDataOut[idx++] |= fogBuffer[v9++];
                        --v13;
                    } while (v13 != 0);

                    v9 += v10;
                    idx += v15;
                    --v12;
                } while (v12 != 0);
            }
        }

        [TempleDllLocation(0x1002ec20)]
        public void ResetBuffers()
        {
            InitScreenBuffers();
        }

        [TempleDllLocation(0x1002ECB0)]
        public byte GetFogStatus(locXY loc, float offsetX, float offsetY)
        {
            return 0xFF;
        }

        [TempleDllLocation(0x10030e20)]
        public void SaveExploredTileData(int id)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10030BF0)]
        public void LoadFogColor(string dataDir)
        {
            var path = $"{dataDir}/fog.col";
            if (Tig.FS.FileExists(path))
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                fogcol_field1 = reader.ReadInt32();
                fogcol_field2 = reader.ReadInt32();
            }
        }

        [TempleDllLocation(0x1002ec90)]
        public void Disable()
        {
            mFoggingEnabled = false;
        }

        [TempleDllLocation(0x1002ec80)]
        public void Enable()
        {
            mFoggingEnabled = true;
        }

        [TempleDllLocation(0x10030d10)]
        public void LoadExploredTileData(string baseDir)
        {
            if (mFoggingEnabled)
            {
                int idx = 0;
                var otherIdx = 0;
                do
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var fileId = i + otherIdx;
                        var path = $"{baseDir}/etd{fileId:D6}";

                        var unexploredData = new MapFogdataChunk();
                        if (Tig.FS.FileExists(path))
                        {
                            using var reader = Tig.FS.OpenBinaryReader(path);
                            unexploredData.AllExplored = reader.ReadByte() != 0;
                            var unexploredRaw = unexploredData.Data.AsSpan();
                            if (unexploredData.AllExplored)
                            {
                                unexploredRaw.Fill(0xFF);
                            }
                            else
                            {
                                if (reader.Read(unexploredRaw) != unexploredRaw.Length)
                                {
                                    throw new InvalidOperationException("Failed to read unexplored sector data.");
                                }
                            }
                        }
                        else
                        {
                            unexploredData.AllExplored = false;
                        }

                        mFogUnexploredData[idx] = unexploredData;
                        ++i;
                        ++idx;
                    }

                    otherIdx += 1000;
                } while (idx < 4);
            }
        }

        [TempleDllLocation(0x1002eca0)]
        public void SetMapDoFoggingUpdate()
        {
            throw new NotImplementedException();
        }

        internal Span<byte> GetLineOfSightBuffer(int partyIndex, out Size size, out locXY originTile)
        {
            size = new Size(MaxLosDiameter, MaxLosDiameter);
            originTile = new locXY(
                fog_buffer_origin_x[partyIndex],
                fog_buffer_origin_y[partyIndex]
            );
            return mFogBuffers[partyIndex];
        }
    }
}