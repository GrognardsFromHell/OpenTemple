using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Raycast
{
    public struct PreciseSectorCol
    {
        public int colCount;
        public locXY[] Tiles;
        public SectorLoc[] sectors;
        public int[] startTiles;
        public int[] strides;
        public int field_68;
        public int field_6C;
    }

    public struct PreciseSectorRows
    {
        public int RowCount;
        public PreciseSectorCol[] Rows;

        [TempleDllLocation(0x10081d80)]
        private static int build_clamped_tile_coordlist(int startLoc, int endLoc, int increment, Span<int> locs)
        {
            var idx = 0;
            locs[idx++] = startLoc;
            for (var nextLoc = (startLoc / increment + 1) * increment; nextLoc < endLoc; nextLoc += increment)
            {
                locs[idx++] = nextLoc;
            }

            locs[idx] = endLoc;
            return idx + 1;
        }

        [TempleDllLocation(0x100824d0)]
        public static bool Build(TileRect rect, out PreciseSectorRows tiles)
        {
            Span<int> xList = stackalloc int[5];
            var xSize = build_clamped_tile_coordlist(rect.x1, rect.x2 + 1, 64, xList);

            var xSizeMin1 = xSize - 1;
            if (xSize == 1)
            {
                tiles = default;
                return false;
            }

            Span<int> yList = stackalloc int[5];
            var ySize = build_clamped_tile_coordlist(rect.y1, rect.y2 + 1, 64, yList) - 1;
            if (ySize <= 0)
            {
                tiles = default;
                return false;
            }

            tiles = new PreciseSectorRows();
            tiles.Init();

            var v17 = 0;
            var v18 = ySize;
            do
            {
                var deltaY = yList[v17 + 1] - yList[v17];
                tiles.Rows[v17].colCount = 0;
                tiles.Rows[v17].field_68 = deltaY;

                var v14 = 0;
                if (xSizeMin1 > 0)
                {
                    do
                    {
                        tiles.Rows[v17].Tiles[v14] = new locXY(xList[v14], yList[v17]);
                        tiles.Rows[v17].sectors[v14] = new SectorLoc(tiles.Rows[v17].Tiles[v14]);

                        tiles.Rows[v17].startTiles[v14] = Sector.GetSectorTileIndex(xList[v14], yList[v17]);
                        tiles.Rows[v17].strides[v14] = xList[v14 + 1] - xList[v14];
                        v14++;
                    } while (v14 < xSizeMin1);
                }

                tiles.Rows[v17].colCount = xSizeMin1;
                v17++;
                v18--;
            } while (v18 > 0);

            tiles.RowCount = ySize;
            return true;
        }

        private void Init()
        {
            Rows = new PreciseSectorCol[4];
            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i].startTiles = new int[4];
                Rows[i].strides = new int[4];
                Rows[i].sectors = new SectorLoc[4];
                Rows[i].Tiles = new locXY[4];
            }
        }
    }

    public class RaycastPacket : IDisposable, IReadOnlyList<RaycastResultItem>
    {
        public RaycastFlag flags;
        public int field4;
        public LocAndOffsets origin;
        public LocAndOffsets targetLoc;
        public float radius;
        public int field2C;
        public GameObjectBody sourceObj;
        public GameObjectBody target;
        public float rayRangeInches; // limits the distance from the origin
        public List<RaycastResultItem> results;

        [TempleDllLocation(0x100babb0)]
        public RaycastPacket()
        {
            flags = RaycastFlag.HasToBeCleared;
            results = new List<RaycastResultItem>();
        }

        private struct RaycastPointSearchPacket
        {
            public Vector2 origin;
            public Vector2 target;
            public Vector2 direction; // is normalized, was: ux, uy
            public float rangeInch;

            public float
                absOdotU; // dot product of the origin point and the direction vector, normalized by the direction vector norm

            public float radius;
        }

        private struct PointAlongSegment
        {
            public float absX;
            public float absY;
            public float distFromOrigin;
        }

        private delegate bool SearchPointAlongRay(Vector2 worldPos, float radiusAdjAmount,
            in RaycastPointSearchPacket srchPkt, out PointAlongSegment point);

        private static readonly Vector2 SearchExtentInches = new Vector2(150.0f, 150.0f);

        private TileRect BuildSearchRectangle(Vector2 from, Vector2 to)
        {
            var minPoint = Vector2.Min(from, to) - SearchExtentInches;
            var maxPoint = Vector2.Max(from, to) + SearchExtentInches;

            var minLoc = locXY.FromInches(minPoint);
            var maxLoc = locXY.FromInches(maxPoint);

            return new TileRect {x1 = minLoc.locx, y1 = minLoc.locy, x2 = maxLoc.locx, y2 = maxLoc.locy};
        }

        [TempleDllLocation(0x100bace0)]
        public int Raycast()
        {
            var packet = this;
            PointAlongSegment point_found;
            RaycastPointSearchPacket ray_search;
            GameObjectBody source_obj_1 = null;
            GameObjectBody target_obj_1 = null;
            if (flags.HasFlag(RaycastFlag.HasSourceObj))
            {
                source_obj_1 = sourceObj;
            }

            if (flags.HasFlag(RaycastFlag.HasTargetObj))
            {
                target_obj_1 = target;
            }

            if (!flags.HasFlag(RaycastFlag.HasRadius))
            {
                packet.radius = 0.1f;
            }

            ray_search = new RaycastPointSearchPacket();
            ray_search.origin = packet.origin.ToInches2D();
            ray_search.target = packet.targetLoc.ToInches2D();
            ray_search.radius = packet.radius;

            ray_search.direction = ray_search.target - ray_search.origin;
            ray_search.rangeInch = ray_search.direction.Length();
            ray_search.direction /= ray_search.rangeInch;

            ray_search.absOdotU = Vector2.Dot(ray_search.direction, ray_search.origin);
            if (packet.flags.HasFlag(RaycastFlag.HasRangeLimit))
            {
                ray_search.rangeInch = packet.rayRangeInches;
                ray_search.target = ray_search.origin + ray_search.direction * packet.rayRangeInches;
                packet.targetLoc = LocAndOffsets.FromInches(ray_search.target);
            }

            var locBeforeCover = packet.targetLoc;
            SearchPointAlongRay search_func = GetPointAlongSegmentNearestToOriginDistanceRfromV;
            if (!flags.HasFlag(RaycastFlag.GetObjIntersection))
            {
                search_func = IsPointCloseToSegment;
            }

            var tile_rect = BuildSearchRectangle(ray_search.origin, ray_search.target);
            var canFly = flags.HasFlag(RaycastFlag.IgnoreFlyover);

            if (!PreciseSectorRows.Build(tile_rect, out var sector_tiles))
            {
                return 0;
            }

            var tileRadius = 1 + (int) (radius / locXY.INCH_PER_TILE);

            var local_254 = packet.origin.location.locx;
            var local_2a4 = packet.origin.location.locy;

            var local_25c = packet.targetLoc.location.locx;
            if (packet.targetLoc.location.locx < local_254)
            {
                local_25c = local_254;
                local_254 = packet.targetLoc.location.locx;
            }

            var local_2b4 = packet.targetLoc.location.locy;
            if (packet.targetLoc.location.locy < local_2a4)
            {
                local_2b4 = local_2a4;
                local_2a4 = packet.targetLoc.location.locy;
            }

            local_254 -= tileRadius;
            local_2a4 -= tileRadius;
            local_25c += tileRadius;
            local_2b4 += tileRadius;

            for (var local_2dc = 0; local_2dc < sector_tiles.RowCount; local_2dc++)
            {
                ref var pPVar2 = ref sector_tiles.Rows[local_2dc];

                Span<int> local_208 = stackalloc int[pPVar2.colCount];
                Span<int> local_1d8 = stackalloc int[pPVar2.colCount];
                var local_29c = new LockedMapSector[pPVar2.colCount];

                for (var i = 0; i < pPVar2.colCount; i++)
                {
                    local_208[i] = pPVar2.startTiles[i];
                    local_1d8[i] = 64 - pPVar2.strides[i];
                    local_29c[i] = new LockedMapSector(pPVar2.sectors[i]);
                }

                for (var local_2f0 = 0; local_2f0 < pPVar2.colCount; local_2f0++)
                {
                    var sector = local_29c[local_2f0];
                    if (!sector.IsValid)
                    {
                        continue;
                    }

                    var sectorBaseTile = sector.Loc.GetBaseTile();
                    var sectorTileMinX = local_254 - sectorBaseTile.locx;
                    var sectorTileMaxX = local_25c - sectorBaseTile.locx;
                    var sectorTileMinY = local_2a4 - sectorBaseTile.locy;
                    var sectorTileMaxY = local_2b4 - sectorBaseTile.locy;
                    if (sectorTileMinX < 0)
                    {
                        sectorTileMinX = 0;
                    }

                    if (sectorTileMinY < 0)
                    {
                        sectorTileMinY = 0;
                    }

                    if (sectorTileMaxX > 63)
                    {
                        sectorTileMaxX = 63;
                    }

                    if (sectorTileMaxY > 63)
                    {
                        sectorTileMaxY = 63;
                    }

                    for (var sectorTileY = sectorTileMinY; sectorTileY <= sectorTileMaxY; sectorTileY++)
                    {
                        for (var sectorTileX = sectorTileMinX; sectorTileX <= sectorTileMaxX; sectorTileX++)
                        {
                            var tilePos = new locXY(sectorBaseTile.locx + sectorTileX,
                                sectorBaseTile.locy + sectorTileY);
                            var tileFlags = sector.Sector.tilePkt
                                .tiles[Sector.GetSectorTileIndex(sectorTileX, sectorTileY)].flags;

                            bool TestSubtile(int subtileX, int subtileY)
                            {
                                var blockingFlag = SectorTile.GetBlockingFlag(subtileX, subtileY);
                                var flyOverFlag = SectorTile.GetFlyOverFlag(subtileX, subtileY);

                                if (tileFlags.HasFlag(blockingFlag) || !canFly && tileFlags.HasFlag(flyOverFlag))
                                {
                                    var subTilePos = new LocAndOffsets(
                                        tilePos,
                                        (subtileX - 1) * locXY.INCH_PER_SUBTILE,
                                        (subtileY - 1) * locXY.INCH_PER_SUBTILE
                                    );
                                    var subTileWorldPos = subTilePos.ToInches2D();
                                    if (search_func(subTileWorldPos, SubtileRadius, in ray_search, out point_found))
                                    {
                                        if (tileFlags.HasFlag(TileFlags.BlockX0Y0) ||
                                            tileFlags.HasFlag(TileFlags.FlyOverX0Y0 | TileFlags.ProvidesCover))
                                        {
                                            flags |= RaycastFlag.FoundCoverProvider;
                                        }

                                        var resultItem = new RaycastResultItem
                                        {
                                            loc = subTilePos
                                        };

                                        if (tileFlags.HasFlag(TileFlags.BlockX0Y0))
                                        {
                                            resultItem.flags |= RaycastResultFlag.BlockerSubtile;
                                        }

                                        if (tileFlags.HasFlag(TileFlags.FlyOverX0Y0))
                                        {
                                            resultItem.flags |= RaycastResultFlag.FlyoverSubtile;
                                        }

                                        if (flags.HasFlag(RaycastFlag.GetObjIntersection))
                                        {
                                            resultItem.intersectionPoint =
                                                LocAndOffsets.FromInches(point_found.absX, point_found.absY);
                                            resultItem.intersectionDistance = point_found.distFromOrigin;
                                        }

                                        results.Add(resultItem);

                                        if (flags.HasFlag(RaycastFlag.StopAfterFirstBlockerFound) &&
                                            tileFlags.HasFlag(TileFlags.BlockX0Y0) ||
                                            flags.HasFlag(RaycastFlag.StopAfterFirstFlyoverFound) &&
                                            tileFlags.HasFlag(TileFlags.FlyOverX0Y0))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            // Test all 9 subtiles of the tile
                            if (TestSubtile(0, 0)
                                || TestSubtile(1, 0)
                                || TestSubtile(2, 0)
                                || TestSubtile(0, 1)
                                || TestSubtile(1, 1)
                                || TestSubtile(2, 1)
                                || TestSubtile(0, 2)
                                || TestSubtile(1, 2)
                                || TestSubtile(2, 2))
                            {
                                foreach (var lockedSector in local_29c)
                                {
                                    lockedSector.Dispose();
                                }

                                return Count;
                            }

                            if (flags.HasFlag(RaycastFlag.RequireDistToSourceLessThanTargetDist) && Count > 0)
                            {
                                locBeforeCover = results[0].loc;
                                // I think this cancels out of the sector's loop
                                sectorTileX = 64;
                                sectorTileY = 64;
                            }
                        }
                    }
                }

                for (int i = 0; i < pPVar2.field_68; i++)
                {
                    for (var j = 0; j < pPVar2.colCount; j++)
                    {
                        var sector = local_29c[j];
                        if (!sector.IsValid)
                        {
                            continue;
                        }

                        var objects = sector.Sector.objects.tiles;
                        for (var k = 0; k < pPVar2.strides[j]; k++)
                        {
                            Sector.GetSectorTileFromIndex(local_208[j] + k, out var objX, out var objY);

                            if (objects[objX, objY] == null)
                            {
                                continue;
                            }

                            foreach (var obj in objects[objX, objY])
                            {
                                if (!IsBlockingObject(obj))
                                {
                                    continue;
                                }

                                // Determine whether circle at origin intersects with object
                                var objRadius = obj.GetRadius();
                                if (!search_func(obj.GetLocationFull().ToInches2D(), objRadius, ray_search, out point_found))
                                {
                                    continue;
                                }

                                if (flags.HasFlag(RaycastFlag.RequireDistToSourceLessThanTargetDist))
                                {
                                    var local_2a8 = obj.DistanceToLocInFeet(origin) * 12 - objRadius;
                                    if (local_2a8 > origin.DistanceTo(locBeforeCover))
                                    {
                                        continue;
                                    }
                                }

                                flags |= RaycastFlag.FoundCoverProvider;

                                var resultItem = new RaycastResultItem
                                {
                                    obj = obj,
                                    loc = obj.GetLocationFull()
                                };
                                if (flags.HasFlag(RaycastFlag.GetObjIntersection))
                                {
                                    resultItem.intersectionPoint =
                                        LocAndOffsets.FromInches(point_found.absX, point_found.absY);
                                    resultItem.intersectionDistance = point_found.distFromOrigin;
                                }
                                results.Add(resultItem);
                            }
                        }

                        local_208[j] += local_1d8[j];
                    }
                }

                foreach (var lockedSector in local_29c)
                {
                    lockedSector.Dispose();
                }
            }

            return packet.Count;
        }

        [TempleDllLocation(0x100baa30)]
        private bool GetPointAlongSegmentNearestToOriginDistanceRfromV(Vector2 worldPos, float radiusadjamount,
            in RaycastPointSearchPacket srchpkt, out PointAlongSegment point)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100ba980)]
        private static bool IsPointCloseToSegment(Vector2 worldPos, float radiusAdjAmount,
            in RaycastPointSearchPacket srchPkt, out PointAlongSegment pointOut)
        {
            pointOut = default; // This function does not provide this result.

            var projection = Vector2.Dot(worldPos, srchPkt.direction) - srchPkt.absOdotU;
            float distanceFromRay;
            if (projection >= 0.0f)
            {
                if (projection <= srchPkt.rangeInch)
                {
                    // Compute the point on the line
                    var pointOnLine = (srchPkt.origin + projection * srchPkt.direction);
                    // Then compute the distance from that point (which is the distance from the rays outer edge)
                    distanceFromRay = (pointOnLine - worldPos).LengthSquared();
                }
                else
                {
                    // Distance from the target point
                    distanceFromRay = (srchPkt.target - worldPos).LengthSquared();
                }
            }
            else
            {
                return false;
            }

            // Distance from center of the ray to it's edge
            var radiusAdj = srchPkt.radius + radiusAdjAmount;

            return distanceFromRay < radiusAdj * radiusAdj;
        }

        private bool IntersectsOrigin(LocAndOffsets worldPos, float extraRadius)
        {
            var hitRadius = radius + extraRadius;
            hitRadius *= hitRadius;

            var originPos = origin.ToInches2D();

            var distFromOrigin = (originPos - worldPos.ToInches2D()).LengthSquared();

            return distFromOrigin < hitRadius;
        }

        private const float SubtileRadius = locXY.INCH_PER_SUBTILE / 2;

        private void TestSubtile(TileFlags tileFlags, locXY tilePos, float xOffset, float yOffset,
            TileFlags blockFlag, TileFlags flyOverFlag)
        {
            if (!tileFlags.HasFlag(blockFlag) && !tileFlags.HasFlag(flyOverFlag))
            {
                return;
            }

            var subTilePos = new LocAndOffsets(tilePos, xOffset, yOffset);
            if (!IntersectsOrigin(subTilePos, SubtileRadius))
            {
                return;
            }

            if (tileFlags.HasFlag(blockFlag) ||
                tileFlags.HasFlag(TileFlags.ProvidesCover) && tileFlags.HasFlag(flyOverFlag))
            {
                flags |= RaycastFlag.FoundCoverProvider;
            }

            results.Add(new RaycastResultItem {loc = subTilePos});
        }

        private bool IsBlockingObject(GameObjectBody obj)
        {
            if (obj == null)
            {
                return false;
            }

            if ((obj.GetFlags() & ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.NO_BLOCK) != 0)
            {
                return false;
            }

            if (flags.HasFlag(RaycastFlag.HasSourceObj) && obj == sourceObj)
            {
                return false;
            }

            if (flags.HasFlag(RaycastFlag.HasTargetObj) && obj == target)
            {
                return false;
            }

            if (GameSystems.MapObject.IsUntargetable(obj) && !obj.IsSecretDoor())
            {
                return false;
            }

            if (flags.HasFlag(RaycastFlag.ExcludeItemObjects) && obj.IsItem())
            {
                return false;
            }

            if (flags.HasFlag(RaycastFlag.ExcludePortals) && obj.type == ObjectType.portal)
            {
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x100bc750)]
        public int RaycastShortRange()
        {
            if (!flags.HasFlag(RaycastFlag.HasRadius))
            {
                radius = 0;
            }

            var originPos = origin.ToInches2D();

            var locBeforeCover = targetLoc;

            var tileRect = BuildSearchRectangle(originPos, originPos);

            if (PreciseSectorRows.Build(tileRect, out var sectorStripes))
            {
                var tileRadius = 1 + (int) (radius / locXY.INCH_PER_TILE);
                var local_1e0 = origin.location.locx - tileRadius;
                var local_24c = origin.location.locy - tileRadius;
                var local_268 = origin.location.locx + tileRadius;
                var local_270 = origin.location.locy + tileRadius;

                for (var local_294 = 0; local_294 < sectorStripes.RowCount; local_294++)
                {
                    ref var local_2b4 = ref sectorStripes.Rows[local_294];

                    Span<int> local_220 = stackalloc int[local_2b4.colCount];
                    Span<int> local_1d8 = stackalloc int[local_2b4.colCount];
                    var local_230 = new LockedMapSector[local_2b4.colCount];

                    for (var i = 0; i < local_2b4.colCount; i++)
                    {
                        local_220[i] = local_2b4.startTiles[i];
                        local_1d8[i] = 64 - local_2b4.strides[i];
                        local_230[i] = new LockedMapSector(local_2b4.sectors[i]);
                    }

                    for (var local_2b8 = 0; local_2b8 < local_2b4.colCount; local_2b8++)
                    {
                        var sector = local_230[local_2b8];
                        if (!sector.IsValid)
                        {
                            continue;
                        }

                        var sectorBaseTile = sector.Loc.GetBaseTile();
                        var local_29c = local_1e0 - sectorBaseTile.locx;
                        var local_2a8 = local_270 - sectorBaseTile.locy;

                        var local_2a0 = local_268 - sectorBaseTile.locx;
                        var local_2b0 = local_24c - sectorBaseTile.locy;

                        if (local_29c < 0)
                        {
                            local_29c = 0;
                        }

                        if (local_2b0 < 0)
                        {
                            local_2b0 = 0;
                        }

                        if (local_2a0 >= 64)
                        {
                            local_2a0 = 63;
                        }

                        if (local_2a8 >= 64)
                        {
                            local_2a8 = 0;
                        }

                        for (; local_2b0 <= local_2a8; local_2b0++)
                        {
                            for (var local_2ac = local_29c; local_2ac < local_2a0; local_2ac++)
                            {
                                var tilePos = new locXY(sectorBaseTile.locx + local_2ac,
                                    sectorBaseTile.locy + local_2b0);
                                var tileFlags = sector.Sector.tilePkt.tiles[local_2b0 * 64 + local_2ac].flags;

                                TestSubtile(tileFlags, tilePos, -locXY.INCH_PER_SUBTILE, -locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX0Y0, TileFlags.FlyOverX0Y0);
                                TestSubtile(tileFlags, tilePos, 0, -locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX1Y0, TileFlags.FlyOverX1Y0);
                                TestSubtile(tileFlags, tilePos, locXY.INCH_PER_SUBTILE, -locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX2Y0, TileFlags.FlyOverX2Y0);

                                TestSubtile(tileFlags, tilePos, -locXY.INCH_PER_SUBTILE, 0,
                                    TileFlags.BlockX0Y1, TileFlags.FlyOverX0Y1);
                                TestSubtile(tileFlags, tilePos, 0, 0,
                                    TileFlags.BlockX1Y1, TileFlags.FlyOverX1Y1);
                                TestSubtile(tileFlags, tilePos, locXY.INCH_PER_SUBTILE, 0,
                                    TileFlags.BlockX2Y1, TileFlags.FlyOverX2Y1);

                                TestSubtile(tileFlags, tilePos, -locXY.INCH_PER_SUBTILE, locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX0Y2, TileFlags.FlyOverX0Y2);
                                TestSubtile(tileFlags, tilePos, 0, locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX1Y2, TileFlags.FlyOverX1Y2);
                                TestSubtile(tileFlags, tilePos, locXY.INCH_PER_SUBTILE, locXY.INCH_PER_SUBTILE,
                                    TileFlags.BlockX2Y2, TileFlags.FlyOverX2Y2);

                                if ((flags & RaycastFlag.StopAfterFirstBlockerFound |
                                     RaycastFlag.StopAfterFirstFlyoverFound) != 0
                                    && results.Count > 0)
                                {
                                    foreach (var lockedSector in local_230)
                                    {
                                        lockedSector.Dispose();
                                    }

                                    return results.Count;
                                }

                                if (flags.HasFlag(RaycastFlag.RequireDistToSourceLessThanTargetDist) &&
                                    results.Count > 0)
                                {
                                    locBeforeCover = results[0].loc;
                                    local_2ac = 0;
                                    local_2b0 = 64;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < local_2b4.field_68; i++)
                    {
                        for (var j = 0; j < local_2b4.colCount; j++)
                        {
                            var sector = local_230[j];
                            if (!sector.IsValid)
                            {
                                continue;
                            }

                            var objects = sector.Sector.objects.tiles;
                            for (var k = 0; k < local_2b4.strides[j]; k++)
                            {
                                Sector.GetSectorTileFromIndex(local_220[j] + k, out var objX, out var objY);

                                if (objects[objX, objY] == null)
                                {
                                    continue;
                                }

                                foreach (var obj in objects[objX, objY])
                                {
                                    if (!IsBlockingObject(obj))
                                    {
                                        continue;
                                    }

                                    // Determine whether circle at origin intersects with object
                                    if (!IntersectsOrigin(obj.GetLocationFull(), obj.GetRadius()))
                                    {
                                        continue;
                                    }

                                    if (flags.HasFlag(RaycastFlag.RequireDistToSourceLessThanTargetDist))
                                    {
                                        var local_2a8 = obj.DistanceToLocInFeet(origin) * 12 - obj.GetRadius();
                                        if (local_2a8 > origin.DistanceTo(locBeforeCover))
                                        {
                                            continue;
                                        }
                                    }

                                    flags |= RaycastFlag.FoundCoverProvider;

                                    results.Add(new RaycastResultItem
                                    {
                                        obj = obj,
                                        loc = obj.GetLocationFull()
                                    });
                                }
                            }

                            local_220[j] += local_1d8[j];
                        }
                    }

                    foreach (var lockedSector in local_230)
                    {
                        lockedSector.Dispose();
                    }
                }
            }

            foreach (var goalDest in GameSystems.Raycast.GoalDestinations)
            {
                var obj = goalDest.obj;
                if (!IsBlockingObject(obj))
                {
                    continue;
                }

                if (!IntersectsOrigin(goalDest.loc, obj.GetRadius()))
                {
                    continue;
                }

                if (flags.HasFlag(RaycastFlag.RequireDistToSourceLessThanTargetDist))
                {
                    var local_2a8 = obj.DistanceToLocInFeet(origin) * 12 - obj.GetRadius();
                    if (local_2a8 > origin.DistanceTo(locBeforeCover))
                    {
                        continue;
                    }
                }

                results.Add(new RaycastResultItem
                {
                    obj = obj,
                    loc = goalDest.loc
                });
            }

            return results.Count;
        }

        [TempleDllLocation(0x100BABE0)]
        public void Dispose()
        {
            flags &= RaycastFlag.HasToBeCleared;
            if (results != null)
            {
                results.Clear();
                results = null;
            }
        }

        public IEnumerable<GameObjectBody> EnumerateObjects()
        {
            foreach (var resultItem in results)
            {
                yield return resultItem.obj;
            }
        }

        public IEnumerator<RaycastResultItem> GetEnumerator() => results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => results.Count;

        public RaycastResultItem this[int index] => results[index];

        /// <summary>
        /// Sinus Lookup table for -90, -45, 45 and 90 degree rotations.
        /// </summary>
        private static readonly float[] SinLookupTable =
        {
            MathF.Sin(Angles.ToRadians(-90)),
            MathF.Sin(Angles.ToRadians(-45)),
            MathF.Sin(Angles.ToRadians(45)),
            MathF.Sin(Angles.ToRadians(90))
        };

        /// <summary>
        /// Cosine Lookup table for -90, -45, 45 and 90 degree rotations.
        /// </summary>
        private static readonly float[] CosLookupTable =
        {
            MathF.Cos(Angles.ToRadians(-90)),
            MathF.Cos(Angles.ToRadians(-45)),
            MathF.Cos(Angles.ToRadians(45)),
            MathF.Cos(Angles.ToRadians(90))
        };

        public bool TestLineOfSight()
        {
            return TestLineOfSight(false, out _);
        }

        public bool TestLineOfSight(bool findCover, out bool foundCover)
        {
            foundCover = false;
            Raycast();

            if (findCover)
            {
                foreach (var resultItem in this)
                {
                    var blockingObj = resultItem.obj;
                    if (blockingObj != null
                        && blockingObj.IsCritter()
                        && !GameSystems.Critter.IsDeadNullDestroyed(blockingObj)
                        && !GameSystems.Critter.IsProne(blockingObj)
                        && !GameSystems.Critter.IsDeadOrUnconscious(blockingObj))
                    {
                        foundCover = true;
                    }
                }

                if (flags.HasFlag(RaycastFlag.FoundCoverProvider))
                {
                    foundCover = true;
                }
            }

            var foundBlockers = HasBlockerOrClosedDoor() ? 1 : 0;

            // When we don't have a target and no radius, we can't try alternate angles
            if (target == null && !flags.HasFlag(RaycastFlag.HasRadius))
            {
                return foundBlockers == 0;
            }

            if (foundBlockers <= 0)
            {
                return true;
            }

            var originPos = origin.ToInches2D();
            var targetPos = targetLoc.ToInches2D();

            // This is a vector from target in the direction of origin that ends on the radius
            var targetRadius = target?.GetRadius() ?? radius;
            if (targetRadius < locXY.INCH_PER_SUBTILE)
            {
                targetRadius = locXY.INCH_PER_SUBTILE;
            }

            var dirVecTimesRadius = Vector2.Normalize(originPos - targetPos) * targetRadius;

            for (int i = 0; i < 4; i++)
            {
                using var fallbackRaycast = new RaycastPacket();
                fallbackRaycast.flags = flags;
                fallbackRaycast.sourceObj = sourceObj;
                fallbackRaycast.target = target;
                fallbackRaycast.origin = origin;

                var dirX = CosLookupTable[i] * dirVecTimesRadius.X - SinLookupTable[i] * dirVecTimesRadius.Y;
                var dirY = SinLookupTable[i] * dirVecTimesRadius.X + CosLookupTable[i] * dirVecTimesRadius.Y;

                var overallOffX = targetPos.X + dirX;
                var overallOffY = targetPos.Y + dirY;
                fallbackRaycast.targetLoc = LocAndOffsets.FromInches(overallOffX, overallOffY);
                fallbackRaycast.Raycast();

                if (fallbackRaycast.HasBlockerOrClosedDoor())
                {
                    foundBlockers++;
                }
            }

            return foundBlockers <= 2;

        }


        private bool HasBlockerOrClosedDoor()
        {
            foreach (var resultItem in this)
            {
                if (resultItem.obj == null)
                {
                    if (resultItem.flags.HasFlag(RaycastResultFlag.BlockerSubtile))
                    {
                        return true;
                    }

                    continue;
                }

                if (resultItem.obj.type == ObjectType.portal)
                {
                    if (resultItem.obj != target && !resultItem.obj.IsPortalOpen())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}