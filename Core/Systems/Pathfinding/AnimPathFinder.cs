using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Raycast;

namespace OpenTemple.Core.Systems.Pathfinding;

public class AnimPathFinder
{
    [TempleDllLocation(0x1003fca0, secondary: true)]
    public int AnimPathSearch(ref AnimPathData pathData)
    {
        var backoffFlags = pathData.flags;
        if ((backoffFlags & AnimPathDataFlags.UNK800) != 0)
        {
            return sub_1003F160(ref pathData);
        }

        pathData.distTiles = 0;
        if (pathData.srcLoc == pathData.destLoc)
        {
            return 0;
        }

        if ((backoffFlags & AnimPathDataFlags.UNK80) != 0
            || (backoffFlags & AnimPathDataFlags.UNK100) != 0)
        {
            return GameSystems.PathX.RasterizeLineBetweenLocsScreenspace(pathData.srcLoc, pathData.destLoc,
                pathData.deltas);
        }

        if ((backoffFlags & (AnimPathDataFlags.UNK40 | AnimPathDataFlags.UNK_1)) == 0)
        {
            if ((backoffFlags & AnimPathDataFlags.UNK_8) == 0
                && (backoffFlags & AnimPathDataFlags.UNK_4000) == 0
                && GameSystems.Tile.IsBlockingOldVersion(pathData.destLoc))
            {
                return 0;
            }


            ObjList objListResult;

            if ((pathData.flags & AnimPathDataFlags.UNK_2000) != 0)
            {
                var rangeInches = pathData.handle.GetRadius();
                objListResult = ObjList.ListRadius(new LocAndOffsets(pathData.destLoc), rangeInches,
                    ObjectListFilter.OLC_CRITTERS | ObjectListFilter.OLC_SCENERY | ObjectListFilter.OLC_CONTAINER);
            }
            else
            {
                objListResult = ObjList.ListTile(pathData.destLoc,
                    ObjectListFilter.OLC_CRITTERS | ObjectListFilter.OLC_SCENERY | ObjectListFilter.OLC_CONTAINER);
            }

            var foundObjAtTarget = false;
            foreach (var objAtTarget in objListResult)
            {
                if (!objAtTarget.HasFlag(ObjectFlag.NO_BLOCK))
                {
                    if (!objAtTarget.IsCritter())
                    {
                        foundObjAtTarget = true;
                        break;
                    }

                    if (!GameSystems.Critter.IsDeadNullDestroyed(objAtTarget))
                    {
                        foundObjAtTarget = true;
                        break;
                    }
                }
            }

            objListResult.Dispose();

            if (foundObjAtTarget)
            {
                return 0;
            }
        }

        int result;
        if ((pathData.flags & AnimPathDataFlags.UNK200) != 0)
        {
            result = 0;
        }
        else
        {
            result = SearchInStraightLine(ref pathData);
            if (result != 0)
            {
                return result;
            }
        }

        if ((pathData.flags & AnimPathDataFlags.UNK1000) != 0)
        {
            throw new NotImplementedException();
            // result = sub_1003E240 /*0x1003e240*/(pathData, 300);
        }
        else if ((pathData.flags & AnimPathDataFlags.UNK20) != 0)
        {
            throw new NotImplementedException();
            // result = sub_1003F2C0 /*0x1003f2c0*/(pathData);
        }

        return result;
    }

    [TempleDllLocation(0x1003f160)]
    public int sub_1003F160(ref AnimPathData backoff)
    {
        Span<int> directionOffsets = stackalloc int[8]
        {
            4,
            5,
            3,
            6,
            2,
            7,
            1,
            0
        };

        var flags = backoff.flags;
        var minDistTiles = backoff.distTiles;

        backoff.flags = flags & ~AnimPathDataFlags.UNK800;
        int tileDelta = backoff.srcLoc.EstimateDistance(backoff.destLoc);

        if (tileDelta >= minDistTiles)
        {
            return 0;
        }

        var savedDestLocDist = 0;
        var savedDestLoc = locXY.Zero;
        // The distance we need to back off is reduced by how far we're already away
        var neededDistance = minDistTiles - tileDelta;
        var relPosCode = backoff.srcLoc.GetCompassDirection(backoff.destLoc);

        // Try all directions
        int result = 0;
        foreach (var directionOffset in directionOffsets)
        {
            var direction = relPosCode.Rotate(directionOffset);
            BackOffInDirection(backoff.srcLoc, direction, neededDistance, out backoff.destLoc);
            result = SearchInStraightLine(ref backoff);
            if (result != 0)
            {
                break;
            }

            if (savedDestLocDist < backoff.distTiles)
            {
                savedDestLocDist = backoff.distTiles;
                savedDestLoc = backoff.destLoc;
            }
        }

        backoff.flags |= AnimPathDataFlags.UNK800;
        if (result == 0 && savedDestLocDist > 0)
        {
            backoff.destLoc = savedDestLoc;
            result = SearchInStraightLine(ref backoff);
        }

        return result;
    }

    [TempleDllLocation(0x10029ec0)]
    private void BackOffInDirection(locXY start, CompassDirection relPosCode, int tiles, out locXY locOut)
    {
        locOut = start;
        for (int i = 0; i < tiles; i++)
        {
            locOut = locOut.Offset(relPosCode);
        }
    }

    [TempleDllLocation(0x1003dcd0)]
    private int SearchInStraightLine(ref AnimPathData boPkt)
    {
        if (boPkt.srcLoc.EstimateDistance(boPkt.destLoc) > 32)
        {
            return 0;
        }

        var regardSinks = (boPkt.flags & AnimPathDataFlags.UNK_8) != 0;

        MapObjectSystem.ObstacleFlag flags = 0;
        if ((boPkt.flags & AnimPathDataFlags.CantOpenDoors) != 0)
        {
            flags = MapObjectSystem.ObstacleFlag.UNK_1;
        }

        if ((boPkt.flags & AnimPathDataFlags.UNK_4) != 0)
        {
            flags |= MapObjectSystem.ObstacleFlag.UNK_2;
        }

        if ((boPkt.flags & AnimPathDataFlags.UNK10) != 0)
        {
            flags |= MapObjectSystem.ObstacleFlag.UNK_4;
        }

        var curLoc = boPkt.srcLoc;

        int i;
        for (i = 0; i < boPkt.size; i++)
        {
            if (curLoc == boPkt.destLoc)
            {
                break;
            }

            var direction = curLoc.GetCompassDirection(boPkt.destLoc);
            boPkt.deltas[i] = (sbyte) direction;
            var nextLoc = curLoc.Offset(direction);
            var backoffFlags = boPkt.flags;
            if ((backoffFlags & AnimPathDataFlags.UNK40) == 0)
            {
                if (nextLoc == boPkt.destLoc && (backoffFlags & AnimPathDataFlags.UNK_1) != 0)
                {
                    curLoc = nextLoc;
                    continue;
                }

                if (!regardSinks
                    && (backoffFlags & AnimPathDataFlags.UNK_4000) == 0
                    && GameSystems.Tile.IsBlockingOldVersion(nextLoc))
                {
                    break;
                }

                if (GameSystems.MapObject.HasBlockingObjectInDir(boPkt.handle, new LocAndOffsets(curLoc), direction,
                        flags))
                {
                    break;
                }

                if ((boPkt.flags & AnimPathDataFlags.UNK_2000) != 0)
                {
                    using var objIterator = new RaycastPacket();
                    objIterator.flags |= RaycastFlag.StopAfterFirstFlyoverFound
                                         | RaycastFlag.StopAfterFirstBlockerFound
                                         | RaycastFlag.HasSourceObj
                                         | RaycastFlag.HasRadius;
                    objIterator.targetLoc.location.locx = nextLoc.locx;
                    objIterator.origin = new LocAndOffsets(curLoc);
                    objIterator.targetLoc.location.locy = nextLoc.locy;
                    objIterator.radius = boPkt.handle.GetRadius() * 0.5f;
                    objIterator.sourceObj = boPkt.handle;
                    if (objIterator.Raycast() > 0)
                    {
                        break;
                    }
                }

                var trap = FindTrapAtLocation(nextLoc);
                if (trap != null && GameSystems.Trap.KnowsAboutDangerousTrap(boPkt.handle, trap))
                {
                    break;
                }

                if ((boPkt.flags & AnimPathDataFlags.UNK400) != 0)
                {
                    if (FindBurningScenery(nextLoc) != null)
                    {
                        break;
                    }
                }
            }

            curLoc = nextLoc;
        }

        // Did not reach the destination
        if (curLoc != boPkt.destLoc)
        {
            boPkt.distTiles = i;
            return 0;
        }

        if ((boPkt.flags & AnimPathDataFlags.UNK_1) != 0)
        {
            return i - 1;
        }

        return i;
    }

    [TempleDllLocation(0x10050ba0)]
    private GameObject FindTrapAtLocation(locXY loc)
    {
        using var objListResult = ObjList.ListTile(loc, ObjectListFilter.OLC_TRAP);
        if (objListResult.Count > 0)
        {
            return objListResult[0];
        }

        return null;
    }

    [TempleDllLocation(0x100208b0)]
    private GameObject FindBurningScenery(locXY loc)
    {
        using var objListResult = ObjList.ListTile(loc, ObjectListFilter.OLC_SCENERY);

        foreach (var scenery in objListResult)
        {
            if (GameSystems.Anim.IsRunningGoal(scenery, AnimGoalType.animate_loop_fire_dmg, out _))
            {
                return scenery;
            }
        }

        return null;
    }
}