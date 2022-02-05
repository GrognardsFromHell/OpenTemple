using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using Vector2 = System.Numerics.Vector2;

namespace OpenTemple.Core.Systems.Raycast;

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
    public GameObject sourceObj;
    public GameObject target;
    public float rayRangeInches; // limits the distance from the origin
    public List<RaycastResultItem> results;

    [TempleDllLocation(0x100babb0)]
    public RaycastPacket()
    {
        flags = RaycastFlag.HasToBeCleared;
        results = new List<RaycastResultItem>();
    }

    private static readonly Vector2 SearchExtentInches = new(150.0f, 150.0f);

    internal static TileRect BuildSearchRectangle(Vector2 from, Vector2 to)
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
        RaycastStats.RecordRaycast();

        var packet = this;
        if (!flags.HasFlag(RaycastFlag.HasRadius))
        {
            packet.radius = 0.1f;
        }

        var canFly = flags.HasFlag(RaycastFlag.IgnoreFlyover);
        var stopAfterFirstBlocker = flags.HasFlag(RaycastFlag.StopAfterFirstBlockerFound);
        var stopAfterFirstFlyover = flags.HasFlag(RaycastFlag.StopAfterFirstFlyoverFound);
        var getObjIntersection = flags.HasFlag(RaycastFlag.GetObjIntersection);

        var ray_search = new RaycastPointSearchPacket();
        ray_search.origin = packet.origin.ToInches2D();
        ray_search.target = packet.targetLoc.ToInches2D();
        ray_search.radius = packet.radius;

        ray_search.direction = ray_search.target - ray_search.origin;
        ray_search.rangeInch = ray_search.direction.Length();
        ray_search.direction /= ray_search.rangeInch;

        ray_search.absOdotU = Vector2.Dot(ray_search.direction, ray_search.origin);
        if ((packet.flags & RaycastFlag.HasRangeLimit) != 0)
        {
            ray_search.rangeInch = packet.rayRangeInches;
            ray_search.target = ray_search.origin + ray_search.direction * packet.rayRangeInches;
            packet.targetLoc = LocAndOffsets.FromInches(ray_search.target);
        }

        var locBeforeCover = packet.targetLoc;
        SearchPointAlongRay search_func = GetPointAlongSegmentNearestToOriginDistanceRfromV;
        if ((flags & RaycastFlag.GetObjIntersection) == 0)
        {
            search_func = IsPointCloseToSegment;
        }

        var tileRect = BuildSearchRectangle(ray_search.origin, ray_search.target);

        foreach (var partialSector in tileRect)
        {
            if (!partialSector.Sector.IsValid)
            {
                continue;
            }

            var baseTile = partialSector.SectorLoc.GetBaseTile();

            var subRect = partialSector.TileRectangle;

            for (var sectorTileY = subRect.Top; sectorTileY < subRect.Bottom; sectorTileY++)
            {
                for (var sectorTileX = subRect.Left; sectorTileX < subRect.Right; sectorTileX++)
                {
                    var tilePos = new locXY(baseTile.locx + sectorTileX,
                        baseTile.locy + sectorTileY);
                    var tileFlags = partialSector.Sector.Sector.tilePkt
                        .tiles[Sector.GetSectorTileIndex(sectorTileX, sectorTileY)].flags;

                    if ((tileFlags & (TileFlags.BlockMask | TileFlags.FlyOverMask)) == 0)
                    {
                        // If the entire tile is non-blocking and non-flyover, ignore it entirely
                        continue;
                    }

                    var providesCover = (tileFlags & TileFlags.ProvidesCover) != 0;

                    bool TestSubtile(int subtileX, int subtileY)
                    {
                        var blockingFlag = SectorTile.GetBlockingFlag(subtileX, subtileY);
                        var flyOverFlag = SectorTile.GetFlyOverFlag(subtileX, subtileY);
                        var blocking = (tileFlags & blockingFlag) != 0;
                        var flyOver = (tileFlags & flyOverFlag) != 0;

                        if (blocking || !canFly && flyOver)
                        {
                            var subTilePos = new LocAndOffsets(
                                tilePos,
                                (subtileX - 1) * locXY.INCH_PER_SUBTILE,
                                (subtileY - 1) * locXY.INCH_PER_SUBTILE
                            );
                            var subTileWorldPos = subTilePos.ToInches2D();
                            if (search_func(subTileWorldPos, SubtileRadius, in ray_search, out var point_found))
                            {
                                if (blocking || providesCover)
                                {
                                    flags |= RaycastFlag.FoundCoverProvider;
                                }

                                var resultItem = new RaycastResultItem
                                {
                                    loc = subTilePos
                                };

                                if (blocking)
                                {
                                    resultItem.flags |= RaycastResultFlag.BlockerSubtile;
                                }

                                if (flyOver)
                                {
                                    resultItem.flags |= RaycastResultFlag.FlyoverSubtile;
                                }

                                if (getObjIntersection)
                                {
                                    resultItem.intersectionPoint =
                                        LocAndOffsets.FromInches(point_found.absX, point_found.absY);
                                    resultItem.intersectionDistance = point_found.distFromOrigin;
                                }

                                results.Add(resultItem);

                                if (stopAfterFirstBlocker && blocking
                                    || stopAfterFirstFlyover && flyOver)
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
                        return Count;
                    }

                    if ((flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) != 0 && Count > 0)
                    {
                        locBeforeCover = results[0].loc;
                        // I think this cancels out of the sector's loop
                        sectorTileX = 64;
                        sectorTileY = 64;
                    }
                }
            }

            foreach (var obj in partialSector.EnumerateObjects())
            {
                if (!IsBlockingObject(obj))
                {
                    continue;
                }

                // Determine whether circle at origin intersects with object
                var objRadius = obj.GetRadius();
                if (!search_func(obj.GetLocationFull().ToInches2D(), objRadius, ray_search, out var point_found))
                {
                    continue;
                }

                if ((flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) != 0)
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

        return packet.Count;
    }

    [TempleDllLocation(0x100baa30)]
    internal static bool GetPointAlongSegmentNearestToOriginDistanceRfromV(Vector2 worldPos, float radiusadjamount,
        in RaycastPointSearchPacket srchpkt, out PointAlongSegment point)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100ba980)]
    internal static bool IsPointCloseToSegment(Vector2 worldPos, float radiusAdjAmount,
        in RaycastPointSearchPacket srchPkt, out PointAlongSegment pointOut)
    {
        RaycastStats.RecordIsPointCloseToSegment();

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
        if ((tileFlags & blockFlag) == 0 && (tileFlags & flyOverFlag) == 0)
        {
            return;
        }

        var subTilePos = new LocAndOffsets(tilePos, xOffset, yOffset);
        if (!IntersectsOrigin(subTilePos, SubtileRadius))
        {
            return;
        }

        if ((tileFlags & blockFlag) != 0 ||
            (tileFlags & TileFlags.ProvidesCover) != 0 && (tileFlags & flyOverFlag) != 0)
        {
            flags |= RaycastFlag.FoundCoverProvider;
        }

        results.Add(new RaycastResultItem {loc = subTilePos});
    }

    private bool IsBlockingObject(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if ((obj.GetFlags() & (ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.NO_BLOCK)) != 0)
        {
            return false;
        }

        if ((flags & RaycastFlag.HasSourceObj) != 0 && obj == sourceObj)
        {
            return false;
        }

        if ((flags & RaycastFlag.HasTargetObj) != 0 && obj == target)
        {
            return false;
        }

        if (GameSystems.MapObject.IsUntargetable(obj) && !obj.IsSecretDoor())
        {
            return false;
        }

        if ((flags & RaycastFlag.ExcludeItemObjects) != 0 && obj.IsItem())
        {
            return false;
        }

        if ((flags & RaycastFlag.ExcludePortals) != 0 && obj.type == ObjectType.portal)
        {
            return false;
        }

        return true;
    }

    [TempleDllLocation(0x100bc750)]
    public int RaycastShortRange()
    {
        if ((flags & RaycastFlag.HasRadius) == 0)
        {
            radius = 0;
        }

        var originPos = origin.ToInches2D();

        var locBeforeCover = targetLoc;

        var tileRect = BuildSearchRectangle(originPos, originPos);

        foreach (var partialSector in tileRect)
        {
            if (!partialSector.Sector.IsValid)
            {
                continue;
            }

            var baseTile = partialSector.SectorLoc.GetBaseTile();

            var subRect = partialSector.TileRectangle;

            for (var sectorTileY = subRect.Top; sectorTileY < subRect.Bottom; sectorTileY++)
            {
                for (var sectorTileX = subRect.Left; sectorTileX < subRect.Right; sectorTileX++)
                {
                    var tilePos = new locXY(baseTile.locx + sectorTileX,
                        baseTile.locy + sectorTileY);
                    var tileFlags = partialSector.Sector.Sector.tilePkt
                        .tiles[Sector.GetSectorTileIndex(sectorTileX, sectorTileY)].flags;

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

                    if ((flags & (RaycastFlag.StopAfterFirstBlockerFound |
                                  RaycastFlag.StopAfterFirstFlyoverFound)) != 0
                        && results.Count > 0)
                    {
                        return results.Count;
                    }

                    if ((flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) != 0
                        && results.Count > 0)
                    {
                        locBeforeCover = results[0].loc;
                        sectorTileX = 0;
                        sectorTileY = 64;
                    }
                }
            }

            foreach (var obj in partialSector.EnumerateObjects())
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

                if ((flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) != 0)
                {
                    var dist = obj.DistanceToLocInFeet(origin) * locXY.INCH_PER_FEET - obj.GetRadius();
                    if (dist > origin.DistanceTo(locBeforeCover))
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

            if ((flags & RaycastFlag.RequireDistToSourceLessThanTargetDist) != 0)
            {
                var dist = obj.DistanceToLocInFeet(origin) * 12 - obj.GetRadius();
                if (dist > origin.DistanceTo(locBeforeCover))
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

    public IEnumerable<GameObject> EnumerateObjects()
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

            if ((flags & RaycastFlag.FoundCoverProvider) != 0)
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


    public bool HasBlockerOrClosedDoor()
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

internal struct RaycastPointSearchPacket
{
    public Vector2 origin;
    public Vector2 target;
    public Vector2 direction; // is normalized, was: ux, uy
    public float rangeInch;

    // dot product of the origin point and the direction vector, normalized by the direction vector norm
    public float absOdotU;

    public float radius;
}

internal struct PointAlongSegment
{
    public float absX;
    public float absY;
    public float distFromOrigin;
}

internal delegate bool SearchPointAlongRay(Vector2 worldPos, float radiusAdjAmount,
    in RaycastPointSearchPacket srchPkt, out PointAlongSegment point);