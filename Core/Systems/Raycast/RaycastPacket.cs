using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;

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
                        tiles.Rows[v17].sectors[v14] = new SectorLoc(xList[v14], yList[v17]);

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

        [TempleDllLocation(0x100bace0)]
        public int Raycast()
        {
            Stub.TODO();
            return 0;
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

            var tileRectTopLeft = LocAndOffsets.FromInches(originPos.X - 150f, originPos.Y - 150f).location;
            var tileRectBottomRight = LocAndOffsets.FromInches(originPos.X + 150, originPos.Y + 150).location;

            TileRect tileRect = default;
            tileRect.x1 = tileRectTopLeft.locx;
            tileRect.y1 = tileRectTopLeft.locy;
            tileRect.x2 = tileRectBottomRight.locx;
            tileRect.y2 = tileRectBottomRight.locy;

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
                        if (sector.IsValid)
                        {
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
    }
}