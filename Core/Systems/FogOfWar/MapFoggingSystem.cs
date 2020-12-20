using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.FogOfWar
{
    /// <summary>
    /// Keeps fog of war information for the townmap, which is a 1-bit bitmap.
    /// </summary>
    public class TownmapFogTile
    {
        public bool AllExplored;

        public byte[] Data = new byte[8192];
    }

    public class MapFoggingSystem : IGameSystem, IResetAwareSystem
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly RenderingDevice _device;

        [TempleDllLocation(0x10824468)] [TempleDllLocation(0x108EC4C8)]
        public locXY _fogScreenBufferOrigin;

        [TempleDllLocation(0x10820458)]
        public int _fogScreenBufferWidthSubtiles;

        [TempleDllLocation(0x10824490)]
        public int _fogScreenBufferHeightSubtiles;

        [TempleDllLocation(0x108254A0)]
        internal bool _fogOfWarEnabled;

        /// <summary>
        /// Contains the combined fog information aligned with the current viewport.
        /// </summary>
        [TempleDllLocation(0x108A5498)]
        internal byte[] _fogScreenBuffer;

        // 8 entries, one for each controllable party member
        // The buffers themselves contain one byte per sub-tile within the creature's line of sight area
        // determined by MaxLosDiameter
        private readonly LineOfSightBuffer[] _lineOfSightBuffers;

        [TempleDllLocation(0x11E61560)]
        private TownmapFogTile[,] _townmapFogData = new TownmapFogTile[2,2];

        /// <summary>
        /// This flag indicates that all party member line of sight needs to be recalculated.
        /// </summary>
        [TempleDllLocation(0x108EC590)]
        private bool _lineOfSightInvalidated;

        /// <summary>
        /// The screen size that was used to calculate the size in tiles of the screen fog buffer.
        /// </summary>
        [TempleDllLocation(0x108EC6A0)] [TempleDllLocation(0x108EC6A4)]
        public Size _screenSize;

        // TODO This seems to be fully unused
        [TempleDllLocation(0x102ACF00)]
        private int fogcol_field1;

        // TODO This is used in the townmap UI
        [TempleDllLocation(0x102ACF04)]
        private int fogcol_field2;

        [TempleDllLocation(0x102ACF08)]
        public int _currentTranslationX;

        [TempleDllLocation(0x102ACF10)]
        public int _currentTranslationY;

        /// <summary>
        /// Index of the party member for which the next line of sight
        /// check will be made (during the next frame).
        /// </summary>
        [TempleDllLocation(0x108EC6A8)]
        private int _nextLineOfSightCheckIndex;

        private MemoryPool<Vector2> _vertexPool = MemoryPool<Vector2>.Shared;

        private int _resizeListenerId;

        public FogOfWarRenderer Renderer { get; }

        public TownmapFogTile[,] TownMapFogTiles => _townmapFogData;

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
            _device = renderingDevice;

            _fogScreenBuffer = null;

            _fogOfWarEnabled = true;

            _lineOfSightBuffers = new LineOfSightBuffer[8];

            InitScreenBuffers();

            Renderer = new FogOfWarRenderer(this, renderingDevice);

            // Originally @ 0x10032290
            _resizeListenerId = renderingDevice.AddResizeListener((x, y) =>
            {
                UpdateFogLocation();
            });
        }

        private void InitScreenBuffers()
        {
            UpdateFogLocation();
        }

        private void UpdateFogLocation()
        {
            var camera = _device.GetCamera();

            if (_screenSize == camera.ScreenSize)
            {
                var leftCorner = camera.ScreenToTile(0, 0);

                _fogScreenBufferOrigin.locy = leftCorner.location.locy;
                if ((leftCorner.off_y < leftCorner.off_x) || -leftCorner.off_x > leftCorner.off_y)
                {
                    _fogScreenBufferOrigin.locy--;
                }

                var rightCorner = camera.ScreenToTile(_screenSize.Width, 0);
                _fogScreenBufferOrigin.locx = rightCorner.location.locx;
            }
            else
            {
                ResizeViewport();
            }
        }

        private void ResizeViewport()
        {
            var camera = _device.GetCamera();
            _screenSize = camera.ScreenSize;

            // Calculate the tile locations in each corner of the screen
            var topLeftLoc = camera.ScreenToTile(0, 0);
            var topRightLoc = camera.ScreenToTile(_screenSize.Width, 0);
            var bottomLeftLoc = camera.ScreenToTile(0, _screenSize.Height);
            var bottomRightLoc = camera.ScreenToTile(_screenSize.Width, _screenSize.Height);

            _fogScreenBufferOrigin.locx = topRightLoc.location.locx;
            _fogScreenBufferOrigin.locy = topLeftLoc.location.locy;

            // Whatever the point of this may be ...
            if (topLeftLoc.off_y < topLeftLoc.off_x || topLeftLoc.off_y < -topLeftLoc.off_x)
            {
                _fogScreenBufferOrigin.locy--;
            }

            _fogScreenBufferWidthSubtiles = (bottomLeftLoc.location.locx - _fogScreenBufferOrigin.locx + 3) * 3;
            _fogScreenBufferHeightSubtiles = (bottomRightLoc.location.locy - _fogScreenBufferOrigin.locy + 3) * 3;

            if (_fogScreenBuffer == null || _fogScreenBuffer.Length !=
                _fogScreenBufferWidthSubtiles * _fogScreenBufferHeightSubtiles)
            {
                _fogScreenBuffer = new byte[_fogScreenBufferWidthSubtiles * _fogScreenBufferHeightSubtiles];
                _lineOfSightInvalidated = true;
            }
        }

        [TempleDllLocation(0x1002eb80)]
        public void Dispose()
        {
            _device.RemoveResizeListener(_resizeListenerId);
        }

        [TempleDllLocation(0x1002ebd0)]
        public void Reset()
        {
            _lineOfSightInvalidated = false;
            _explorationData.Clear();
        }


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
            if (!_fogOfWarEnabled || GameSystems.Party.PartySize <= 0)
            {
                return;
            }

            UpdateFogLocation();

            var updateScreenFogBuffer = 0;

            int numberOfFogChecks;
            if (_lineOfSightInvalidated)
            {
                numberOfFogChecks = 8;
            }
            else
            {
                numberOfFogChecks = Globals.Config.LineOfSightChecksPerFrame;
            }

            for (var i = 0; i < numberOfFogChecks; i++)
            {
                var partyIndex = _nextLineOfSightCheckIndex;
                if (_nextLineOfSightCheckIndex >= GameSystems.Party.PartySize)
                {
                    partyIndex = 0;
                    _nextLineOfSightCheckIndex = 0;
                }

                _nextLineOfSightCheckIndex++;

                var partyMember = GameSystems.Party.GetPartyGroupMemberN(partyIndex);

                var losBuffer = GetLineOfSightBuffer(partyIndex);
                var partyMemberPos = partyMember.GetLocationFull();
                if (!_lineOfSightInvalidated && losBuffer.IsValid(partyMemberPos))
                {
                    continue;
                }

                updateScreenFogBuffer |= 1; // Some party member's fog buffer has changed

                losBuffer.UpdateCenter(partyMemberPos);
                CreateLineOfSightBuffer(losBuffer);
            }

            _lineOfSightInvalidated = false;

            var translationX = GameSystems.Location.LocationTranslationX;
            var translationY = GameSystems.Location.LocationTranslationY;
            if (_currentTranslationX != translationX || _currentTranslationY != translationY)
            {
                updateScreenFogBuffer |= 2; // Translation has changed
                _currentTranslationX = translationX;
                _currentTranslationY = translationY;
            }

            if (updateScreenFogBuffer != 0)
            {
                _fogScreenBuffer.AsSpan().Fill(0);

                for (int i = 0; i < GameSystems.Party.PartySize; i++)
                {
                    if (_lineOfSightBuffers[i] != null)
                    {
                        AddLineOfSight(_lineOfSightBuffers[i]);
                    }
                }

                MarkExploredSubtiles();
            }
        }

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
                                    ref buffer[
                                        (3 * yIndex + subtileY) * LineOfSightBuffer.Dimension + 3 * xIndex + subtileX];

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

        /// <summary>
        /// Marks the subtiles covered by closed doors as blocked so they block line of sight.
        /// </summary>
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
                        if (obj.type == ObjectType.portal && !obj.IsPortalOpen())
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
                                    vertexBuffer[i].X = (meshVertices[i].X - minXWorld) / locXY.INCH_PER_SUBTILE;
                                    vertexBuffer[i].Y = (meshVertices[i].Z - minYWorld) / locXY.INCH_PER_SUBTILE;
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
                    if ((buffer[(tileX + tileY * LineOfSightBuffer.Dimension) * 3] & LineOfSightBuffer.LINE_OF_SIGHT) !=
                        0)
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

                        var unexploredTiles = _townmapFogData[gridX, gridY];
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

                        if ((buffer[yIndex * LineOfSightBuffer.Dimension + xIndex] & LineOfSightBuffer.LINE_OF_SIGHT) !=
                            0)
                        {
                            exploredData.MarkExplored(subtileX, subtileY);
                        }
                    }
                }
            }
        }

        private void MarkExploredSubtiles()
        {
            var sectorLocMin = new SectorLoc(new locXY(_fogScreenBufferOrigin.locx, _fogScreenBufferOrigin.locy));
            var sectorLocMax =
                new SectorLoc(new locXY(_fogScreenBufferOrigin.locx + _fogScreenBufferWidthSubtiles / 3 - 1,
                    _fogScreenBufferOrigin.locy + _fogScreenBufferHeightSubtiles / 3 - 1));

            var fogCheckData = _fogScreenBuffer.AsSpan();

            for (var secY = sectorLocMin.Y; secY <= sectorLocMax.Y; secY++)
            {
                for (var secX = sectorLocMin.X; secX <= sectorLocMax.X; secX++)
                {
                    var sectorLoc = new SectorLoc(secX, secY);

                    var sectorExploration = GetOrLoadExploredSectorData(sectorLoc);
                    if (sectorExploration.State != SectorExplorationState.Unexplored)
                    {
                        var sectorOriginLoc = sectorLoc.GetBaseTile();

                        var startSubtileX = 3 * (_fogScreenBufferOrigin.locx - sectorOriginLoc.locx);
                        var startSubtileY = 3 * (_fogScreenBufferOrigin.locy - sectorOriginLoc.locy);
                        var endSubtileX = startSubtileX + _fogScreenBufferWidthSubtiles;
                        var endSubtileY = startSubtileY + _fogScreenBufferHeightSubtiles;

                        if (startSubtileX < 0)
                        {
                            startSubtileX = 0;
                        }

                        if (startSubtileY < 0)
                        {
                            startSubtileY = 0;
                        }

                        if (endSubtileX > Sector.SectorSideSize * 3)
                        {
                            endSubtileX = Sector.SectorSideSize * 3;
                        }

                        if (endSubtileY > Sector.SectorSideSize * 3)
                        {
                            endSubtileY = Sector.SectorSideSize * 3;
                        }

                        if (startSubtileY < endSubtileY && startSubtileX < endSubtileX)
                        {
                            for (var y = startSubtileY; y < endSubtileY; y++)
                            {
                                var idx = (y + 3 * (sectorOriginLoc.locy - _fogScreenBufferOrigin.locy)) *
                                          _fogScreenBufferWidthSubtiles
                                          + 3 * (sectorOriginLoc.locx - _fogScreenBufferOrigin.locx);
                                for (var x = startSubtileX; x < endSubtileX; x++)
                                {
                                    if (sectorExploration.IsExplored(x, y))
                                    {
                                        fogCheckData[idx + x] |= LineOfSightBuffer.EXPLORED;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This adds the line of sight information from the given line of sight buffer onto the
        /// current fog screen buffer.
        /// </summary>
        /// <param name="losBuffer"></param>
        [TempleDllLocation(0x10031E00)]
        private void AddLineOfSight(LineOfSightBuffer losBuffer)
        {
            // Compute source rectangle
            var srcRect = new Rectangle(
                3 * (_fogScreenBufferOrigin.locx - losBuffer.OriginTile.locx),
                3 * (_fogScreenBufferOrigin.locy - losBuffer.OriginTile.locy),
                _fogScreenBufferWidthSubtiles,
                _fogScreenBufferHeightSubtiles
            );

            // Clamp the source rectangle to the actual line of sight buffer's size
            srcRect.Intersect(new Rectangle(0, 0, LineOfSightBuffer.Dimension, LineOfSightBuffer.Dimension));

            if (!srcRect.IsEmpty)
            {
                var destStride = _fogScreenBufferWidthSubtiles - srcRect.Width;

                var fogDataOut = _fogScreenBuffer.AsSpan();

                var fogDataOutX = 3 * (losBuffer.OriginTile.locx - _fogScreenBufferOrigin.locx) + srcRect.Left;
                var fogDataOutY = 3 * (losBuffer.OriginTile.locy - _fogScreenBufferOrigin.locy) + srcRect.Top;
                var destIndex = fogDataOutX + fogDataOutY * _fogScreenBufferWidthSubtiles;

                ReadOnlySpan<byte> fogBuffer = losBuffer.Buffer;

                var sourceIndex = srcRect.Left + srcRect.Top * LineOfSightBuffer.Dimension;
                var srcStride = LineOfSightBuffer.Dimension - srcRect.Width;
                for (var i = 0; i < srcRect.Height; i++) {
                    // NOTE: Using SIMD here could probably greatly benefit this
                    for (var j = 0; j < srcRect.Width; j++)
                    {
                        fogDataOut[destIndex++] |= fogBuffer[sourceIndex++];
                    }

                    sourceIndex += srcStride;
                    destIndex += destStride;
                }
            }
        }

        [TempleDllLocation(0x1002ECB0)]
        public byte GetFogStatus(locXY loc, float offsetX, float offsetY)
        {
            if (_fogOfWarEnabled)
            {
                if (_fogScreenBuffer != null
                    && loc.locx >= _fogScreenBufferOrigin.locx
                    && loc.locy >= _fogScreenBufferOrigin.locy
                    && loc.locx < _fogScreenBufferOrigin.locx + _fogScreenBufferWidthSubtiles / 3
                    && loc.locy < _fogScreenBufferOrigin.locy + _fogScreenBufferHeightSubtiles / 3)
                {
                    GetSubtileFromOffsets(offsetX, offsetY, out var subtileX, out var subtileY);

                    var bufferX = (loc.locx - _fogScreenBufferOrigin.locx) * 3 + subtileX;
                    var bufferY = (loc.locy - _fogScreenBufferOrigin.locy) * 3 + subtileY;

                    return _fogScreenBuffer[bufferY * _fogScreenBufferWidthSubtiles + bufferX];
                }
                else
                {
                    return 0;
                }
            }

            var sectorLoc = new SectorLoc(loc);

            GetSubtileFromOffsets(offsetX, offsetY, out var visibilityX, out var visibilityY);

            var sectorOrigin = sectorLoc.GetBaseTile();
            visibilityX += (loc.locx - sectorOrigin.locx) * 3;
            visibilityY += (loc.locy - sectorOrigin.locy) * 3;

            var result = LineOfSightBuffer.UNK1;

            var visibility = GameSystems.SectorVisibility.Lock(sectorLoc);
            var visibilityFlags = visibility[visibilityX, visibilityY];
            if (visibilityFlags.HasFlag(VisibilityFlags.Extend))
            {
                result |= LineOfSightBuffer.EXTEND;
            }

            if (visibilityFlags.HasFlag(VisibilityFlags.End))
            {
                result |= LineOfSightBuffer.END;
            }

            if (visibilityFlags.HasFlag(VisibilityFlags.Base))
            {
                result |= LineOfSightBuffer.BASE;
            }

            if (visibilityFlags.HasFlag(VisibilityFlags.Archway))
            {
                result |= LineOfSightBuffer.ARCHWAY;
            }

            GameSystems.SectorVisibility.Unlock(sectorLoc);

            // When fog of war is disabled, tiles are always at least explored.
            result |= LineOfSightBuffer.EXPLORED;

            return result;
        }

        private const float HalfSubtile = locXY.INCH_PER_SUBTILE / 2;

        private static void GetSubtileFromOffsets(float offsetX, float offsetY, out int subtileX, out int subtileY)
        {
            subtileY = 0;
            subtileX = 0;
            if (offsetX > -HalfSubtile)
            {
                subtileX = 1;
            }

            if (offsetX > HalfSubtile)
            {
                subtileX = 2;
            }

            if (offsetY > -HalfSubtile)
            {
                subtileY = 1;
            }

            if (offsetY > HalfSubtile)
            {
                subtileY = 2;
            }
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
            _fogOfWarEnabled = false;
        }

        [TempleDllLocation(0x1002ec80)]
        public void Enable()
        {
            _fogOfWarEnabled = true;
        }

        [TempleDllLocation(0x10030d10)]
        public void LoadCurrentTownMapFogOfWar(string baseDir)
        {
            if (!_fogOfWarEnabled)
            {
                _townmapFogData = new TownmapFogTile[2, 2];
                for (var y = 0; y < 2; y++)
                {
                    for (var x = 0; x < 2; x++)
                    {
                        _townmapFogData[x, y] = new TownmapFogTile
                        {
                            AllExplored = true
                        };
                    }
                }
            }
            else
            {
                _townmapFogData = LoadTownMapFogOfWar(baseDir);
            }
        }

        [TempleDllLocation(0x10030d10)]
        public TownmapFogTile[,] LoadTownMapFogOfWar(string baseDir)
        {
            var result = new TownmapFogTile[2, 2];

            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 2; x++)
                {
                    var path = $"{baseDir}/etd{y:D3}{x:D3}";

                    var unexploredData = new TownmapFogTile();
                    if (File.Exists(path))
                    {
                        using var reader = new BinaryReader(new FileStream(path, FileMode.Open));
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

                    result[x, y] = unexploredData;
                }
            }

            return result;
        }

        [TempleDllLocation(0x1002eca0)]
        public void UpdateLineOfSight()
        {
            _lineOfSightInvalidated = true;
        }

        internal Span<byte> GetLineOfSightBuffer(int partyIndex, out Size size, out locXY originTile)
        {
            var losBuffer = GetLineOfSightBuffer(partyIndex);

            size = new Size(LineOfSightBuffer.Dimension, LineOfSightBuffer.Dimension);
            originTile = losBuffer.OriginTile;
            return losBuffer.Buffer;
        }

        public bool IsExplored(LocAndOffsets loc) => (GetFogStatus(loc) & LineOfSightBuffer.EXPLORED) != 0;
    }
}