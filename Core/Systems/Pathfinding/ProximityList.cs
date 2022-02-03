using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script.Hooks;

namespace OpenTemple.Core.Systems.Pathfinding
{
    internal readonly struct ProxListObj
    {
        public readonly GameObject obj;
        public readonly float radius;
        public readonly LocAndOffsets loc;

        public ProxListObj(GameObject obj, float radius, LocAndOffsets loc)
        {
            this.obj = obj;
            this.radius = radius;
            this.loc = loc;
        }
    }

    internal struct ProximityList
    {
        private List<ProxListObj> proxListObjs;

        public void Append(GameObject obj)
        {
            if (proxListObjs == null)
            {
                proxListObjs = new List<ProxListObj>();
            }
            proxListObjs.Add(new ProxListObj(obj, obj.GetRadius(), obj.GetLocationFull()));
        }

        public void Append(GameObject obj, float radius)
        {
            if (proxListObjs == null)
            {
                proxListObjs = new List<ProxListObj>();
            }
            proxListObjs.Add(new ProxListObj(obj, radius, obj.GetLocationFull()));
        }

        public bool FindNear(LocAndOffsets loc, float radius)
        {
            if (proxListObjs == null)
            {
                return false;
            }
            foreach (var proxObj in proxListObjs)
            {
                if (Math.Abs(loc.location.locx - proxObj.loc.location.locx) < radius + proxObj.radius)
                {
                    if (Math.Abs(loc.location.locy - proxObj.loc.location.locy) < radius)
                    {
                        if (loc.DistanceTo(proxObj.loc) < radius + proxObj.radius)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void Populate(PathQuery pq, Path pqr, float radius)
        {
            if (pqr.mover != null)
            {
                using var objlist = ObjList.ListRadius(pqr.from, locXY.INCH_PER_TILE * 40, ObjectListFilter.OLC_PATH_BLOCKER);
                foreach (var obj in objlist)
                {
                    var objFlags = obj.GetFlags();
                    var objType = obj.type;
                    if ((objFlags & (ObjectFlag.NO_BLOCK | ObjectFlag.DONTDRAW | ObjectFlag.OFF)) == 0)
                    {
                        if (pq.flags.HasFlag(PathQueryFlags.PQF_DOORS_ARE_BLOCKING) || objType != ObjectType.portal)
                        {
                            if (objType.IsCritter())
                            {
                                if (pq.flags.HasFlag(PathQueryFlags.PQF_IGNORE_CRITTERS) ||
                                    GameSystems.Critter.IsFriendly(obj, pqr.mover) ||
                                    GameSystems.Critter.IsDeadOrUnconscious(obj))
                                    continue;
                            }

                            if (!pq.flags.HasFlag(PathQueryFlags.PQF_AVOID_AOOS))
                            {
                                Append(obj);
                            }
                            else
                            {
                                var ignoreTarget = pq.critter.ShouldIgnoreTarget(obj);
                                if (!ignoreTarget)
                                {
                                    if (obj.HasRangedWeaponEquipped())
                                    {
                                        Append(obj);
                                    }
                                    else
                                    {
                                        float objReach = obj.GetReach();
                                        float objRadius = obj.GetRadius();
                                        Append(obj, objReach + objRadius);
                                    }
                                }
                                else
                                {
                                    Append(obj);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}