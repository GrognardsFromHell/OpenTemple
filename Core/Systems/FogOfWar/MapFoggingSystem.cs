using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
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

        // 8 entries, one for each controllable party member
        // The buffers themselves contain one byte per sub-tile within the creature's line of sight area
        // determined by MaxLosDiameter
        private readonly LineOfSightBuffer[] _lineOfSightBuffers;

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

        private LineOfSightBuffer GetLineOfSightBuffer(int partyIndex)
        {
            if (_lineOfSightBuffers[partyIndex] == null)
            {
                _lineOfSightBuffers[partyIndex] = new LineOfSightBuffer();
            }

            return _lineOfSightBuffers[partyIndex];
        }

        [TempleDllLocation(0x10032020)]
        public MapFoggingSystem(RenderingDevice renderingDevice)
        {
            mDevice = renderingDevice;

            mFogCheckData = null;

            mFoggingEnabled = true;

            _lineOfSightBuffers = new LineOfSightBuffer[8];

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

        // This is lazily populated
        [TempleDllLocation(0x108EC598)] [TempleDllLocation(0x108EC6B0)]
        private readonly Dictionary<SectorLoc, SectorExploration> _explorationData
            = new Dictionary<SectorLoc, SectorExploration>();

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

                var losBuffer = GetLineOfSightBuffer(partyIndex);
                var partyMemberPos = partyMember.GetLocationFull();
                if (!mDoFullUpdate && losBuffer.IsValid(partyMemberPos))
                {
                    continue;
                }

                updateScreenFogBuffer |= 1; // Some party member's fog buffer has changed

                losBuffer.UpdateCenter(partyMemberPos);
                CreateLineOfSightBuffer(losBuffer);
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
                    if (_lineOfSightBuffers[i] != null)
                    {
                        MarkLineOfSight(_lineOfSightBuffers[i]);
                    }
                }

                MarkExploredSubtiles();
            }
        }

        private MemoryPool<Vector2> _vertexPool = MemoryPool<Vector2>.Shared;

        [TempleDllLocation(0x100327a0)]
        private void CreateLineOfSightBuffer(LineOfSightBuffer losBuffer)
        {
            // Start out by clearing the LOS info
            losBuffer.Reset();

            PrepareLineOfSightBuffer(losBuffer);

            losBuffer.ComputeLineOfSight(60);

            losBuffer.ExtendLineOfSight();

            MarkLineOfSightAsExplored(losBuffer);

            MarkTownmapTilesExplored(losBuffer);
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

        private void PrepareLineOfSightBuffer(LineOfSightBuffer losBuffer)
        {
            var tilerect = losBuffer.TileRect;
            using var sectorIterator = new SectorIterator(tilerect);

            var buffer = losBuffer.Buffer;

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

                MarkClosedDoorsAsVisibilityBlockers(sector, tilerect, buffer, overlappingRect);

                /*
                 * The following loop will mark all blocking subtiles within line of sight of the critter,
                 * as well as copy the sector visibility flags.
                 */
                for (var tileY = overlappingRect.Top; tileY < overlappingRect.Bottom; tileY++)
                {
                    for (var tileX = overlappingRect.Left; tileX < overlappingRect.Right; tileX++)
                    {
                        // The coordinates of this tile in the part member's fog buffer
                        var xIndex = sectorOrigin.locx + tileX - tilerect.x1;
                        var yIndex = sectorOrigin.locy + tileY - tilerect.y1;

                        var tileFlags = sector.Sector.tilePkt.tiles[Sector.GetSectorTileIndex(tileX, tileY)].flags;

                        for (var subtileY = 0; subtileY < 3; subtileY++)
                        {
                            for (var subtileX = 0; subtileX < 3; subtileX++)
                            {
                                ref var losTile =
                                    ref buffer[(3 * yIndex + subtileY) * LineOfSightBuffer.Dimension + 3 * xIndex + subtileX];

                                var flag = SectorTile.GetBlockingFlag(subtileX, subtileY);
                                if ((tileFlags & flag) != 0)
                                {
                                    losTile = LineOfSightBuffer.BLOCKING;
                                }

                                var visibilityFlags = visibility[tileX * 3 + subtileX, tileY * 3 + subtileY];
                                if ((visibilityFlags & VisibilityFlags.Extend) != 0)
                                {
                                    losTile |= LineOfSightBuffer.EXTEND;
                                }

                                if ((visibilityFlags & VisibilityFlags.End) != 0)
                                {
                                    losTile |= LineOfSightBuffer.END;
                                }

                                if ((visibilityFlags & VisibilityFlags.Base) != 0)
                                {
                                    losTile |= LineOfSightBuffer.BASE;
                                }

                                if ((visibilityFlags & VisibilityFlags.Archway) != 0)
                                {
                                    losTile |= LineOfSightBuffer.ARCHWAY;
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

                                    TriangleRasterizer.Rasterize(LineOfSightBuffer.Dimension,
                                        LineOfSightBuffer.Dimension, losBuffer, vertices, 8);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Now we mark the line of sight as explored for the purposes of the town map, which uses
        // a greatly reduced resolution and does not consider actual current line of sight apparently
        private void MarkTownmapTilesExplored(LineOfSightBuffer losBuffer)
        {
            var camera = Tig.RenderingDevice.GetCamera();

            var losDiameterTiles = LineOfSightBuffer.Dimension / 3;

            var buffer = losBuffer.Buffer;

            for (var tileY = 0; tileY < losDiameterTiles; tileY++)
            {
                for (var tileX = 0; tileX < losDiameterTiles; tileX++)
                {
                    if ((buffer[(tileX + tileY * LineOfSightBuffer.Dimension) * 3] & LineOfSightBuffer.UNK) != 0)
                    {
                        var loc = losBuffer.OriginTile;
                        loc.locx += tileX;
                        loc.locy += tileY;

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
        private void MarkLineOfSightAsExplored(LineOfSightBuffer losBuffer)
        {
            var tilerect = losBuffer.TileRect;
            using var sectorIterator = new SectorIterator(tilerect);

            var buffer = losBuffer.Buffer;

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
                        var xIndex = (sectorOrigin.locx - losBuffer.OriginTile.locx) * 3 + subtileX;
                        var yIndex = (sectorOrigin.locy - losBuffer.OriginTile.locy) * 3 + subtileY;

                        if ((buffer[yIndex * LineOfSightBuffer.Dimension + xIndex] & LineOfSightBuffer.UNK) != 0)
                        {
                            exploredData.MarkExplored(subtileX, subtileY);
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
        private void MarkLineOfSight(LineOfSightBuffer losBuffer)
        {
            // TODO: This needs to be cleaned up
            var v1 = losBuffer.OriginTile.locx;
            var v2 = losBuffer.OriginTile.locy;
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

            if (v14 > LineOfSightBuffer.Dimension)
            {
                v14 = LineOfSightBuffer.Dimension;
            }

            if (v16 > LineOfSightBuffer.Dimension)
            {
                v16 = LineOfSightBuffer.Dimension;
            }

            if (v4 < v16 && v3 < v14)
            {
                var v15 = v3 + mSubtilesX - v14;

                var fogDataOut = mFogCheckData.AsSpan().Slice(
                    3 * (v1 - mFogMinX) + v3
                                        + mSubtilesX * (v4 + 3 * (v2 - mFogMinY))
                );
                var idx = 0;

                ReadOnlySpan<byte> fogBuffer = losBuffer.Buffer;
                var v9 = v4 * LineOfSightBuffer.Dimension + v3;
                var v10 = v3 + LineOfSightBuffer.Dimension - v14;
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
            var losBuffer = GetLineOfSightBuffer(partyIndex);

            size = new Size(LineOfSightBuffer.Dimension, LineOfSightBuffer.Dimension);
            originTile = losBuffer.OriginTile;
            return losBuffer.Buffer;
        }
    }
}