using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.InGameSelect
{
    // TODO: The functional structs need to be moved over to a game system

    [Flags]
    public enum PickerResultFlags
    {
        PRF_HAS_SINGLE_OBJ = 0x1,
        PRF_HAS_MULTI_OBJ = 0x2,
        PRF_HAS_LOCATION = 0x4,
        PRF_UNK8 = 0x8,
        PRF_CANCELLED = 0x10, // User pressed escape or RMB
        /// <summary>
        /// The first selected item is the "focus" of the area of effect selection.
        /// </summary>
        PRF_HAS_SELECTED_OBJECT = 0x20,
    }

    public struct PickerResult
    {
        public PickerResultFlags flags; // see PickerResultFlags
        public GameObjectBody handle;
        public List<GameObjectBody> objList;
        public LocAndOffsets location;
        public float offsetz;

        public bool HasSingleResult => flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ) && handle != null;

        public bool HasMultipleResults => flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ);

        public bool HasLocation => flags.HasFlag(PickerResultFlags.PRF_HAS_LOCATION);

        [TempleDllLocation(0x10136fd0)]
        public int TargetCount => HasSingleResult ? 1 : HasMultipleResults ? objList.Count : 0;

        public GameObjectBody FirstTarget
        {
            get
            {
                if (HasSingleResult)
                {
                    return handle;
                }
                else if (HasMultipleResults)
                {
                    return objList[0];
                }
                else
                {
                    return null;
                }
            }
        }

    }

    public delegate void PickerCallback(ref PickerResult result, object callbackArg);

    [Flags]
    public enum UiPickerIncFlags : ulong
    {
        UIPI_None = 0,
        UIPI_Self = 0x1,
        UIPI_Other = 0x2,
        UIPI_NonCritter = 0x4,
        UIPI_Dead = 0x8,
        UIPI_Undead = 0x10,
        UIPI_Unconscious = 0x20,
        UIPI_Hostile = 0x40,
        UIPI_Friendly = 0x80,
        UIPI_Potion = 0x100,
        UIPI_Scroll = 0x200
    }

    [Flags]
    public enum UiPickerFlagsTarget : ulong
    {
        None = 0,
        Min = 0x1,
        Max = 0x2,
        Radius = 0x4,
        Range = 0x8,
        Exclude1st = 0x10,
        Degrees = 0x20,
        FixedRadius = 0x40,

        // these are not supported by the spell rules importer, but apparently used in at least one place (the py cast_spell function)
        /// <summary>
        /// This ignores line of sight from the center of effect for area of effect targets.
        /// </summary>
        Unknown80h = 0x80,
        LosNotRequired = 0x100
    }

    [Flags]
    public enum PickerStatusFlags
    {
        OutOfRange = 0x1,
        Invalid = 0x2
    }

    /*
    note that the first byte denotes the "basic" targeting type
    */
    [Flags]
    public enum UiPickerType : ulong
    {
        None = 0,
        Single,
        Multi,
        Cone,
        Area,
        Location,
        Personal,
        InventoryItem,
        Ray = 8,
        Wall = 9, // NEW!
        BecomeTouch = 0x100,
        AreaOrObj = 0x200,
        /// <summary>
        /// Means that targets cannot be selected multiple times.
        /// </summary>
        OnceMulti = 0x400,
        /// <summary>
        /// Targets must be within 30 feet of each other.
        /// </summary>
        Any30Feet = 0x800,
        Primary30Feet = 0x1000,
        EndEarlyMulti = 0x2000,
        LocIsClear = 0x4000,
        PickOrigin = 0x8000 // New! denotes that the spell's point of origin can be freely chosen
    }

    public static class UiPickerTypeExtensions {

        public static UiPickerType GetBaseMode(this UiPickerType type)
        {
            return (UiPickerType) ((ulong) type & 0xFF);
        }

        public static bool IsBaseMode(this UiPickerType type, UiPickerType baseMode)
        {
            return type.GetBaseMode() == baseMode.GetBaseMode();
        }

    }

    public class PickerArgs
    {
        public UiPickerFlagsTarget flagsTarget;
        public UiPickerType modeTarget;
        public UiPickerIncFlags incFlags;
        public UiPickerIncFlags excFlags;
        public int minTargets;
        public int maxTargets;
        public int radiusTarget;
        public int range; // in feet
        public float degreesTarget;
        public int spellEnum;
        public GameObjectBody caster;
        public PickerCallback callback;
        public int field44;
        public PickerResult result;
        public float trimmedRangeInches; // after processing for collision with walls
        public int field10c;

        public bool IsBaseModeTarget(UiPickerType type) => modeTarget.IsBaseMode(type);

        public bool IsModeTargetFlagSet(UiPickerType type)
        {
            return (modeTarget & type) == type;
        }

        public void SetModeTargetFlag(UiPickerType type)
        {
            modeTarget |= type;
        }

        public UiPickerType GetBaseModeTarget()
        {
            return modeTarget & (UiPickerType) 0xFF;
        }

        public void GetTrimmedRange(LocAndOffsets originLoc, LocAndOffsets tgtLoc, float radiusInch, float maxRangeInch,
            float incrementInches)
        {
            trimmedRangeInches = maxRangeInch;

            using var rayPkt = new RaycastPacket();
            rayPkt.sourceObj = null;
            rayPkt.origin = originLoc;
            rayPkt.rayRangeInches = maxRangeInch;
            rayPkt.targetLoc = tgtLoc;
            rayPkt.radius = radiusInch;
            rayPkt.flags = RaycastFlag.ExcludeItemObjects | RaycastFlag.HasRadius | RaycastFlag.HasRangeLimit;
            rayPkt.Raycast();

            foreach (var rayRes in rayPkt)
            {
                if (rayRes.obj == null)
                {
                    var dist = rayPkt.origin.DistanceTo(rayRes.loc);
                    if (dist < trimmedRangeInches)
                    {
                        trimmedRangeInches = dist;
                    }
                }
            }

            if (incrementInches > 0)
            {
                trimmedRangeInches = MathF.Ceiling(this.trimmedRangeInches / incrementInches) * incrementInches;
            }
        }

        // must have valid trimmedRangeInches value; must also free preexisting ObjectList!
        public void GetTargetsInPath(LocAndOffsets originLoc, LocAndOffsets tgtLoc, float radiusInch)
        {
            using var rayPkt = new RaycastPacket();
            rayPkt.sourceObj = null;
            rayPkt.origin = originLoc;
            rayPkt.rayRangeInches = this.trimmedRangeInches;
            rayPkt.targetLoc = tgtLoc;
            rayPkt.radius = radiusInch;
            rayPkt.flags = RaycastFlag.ExcludeItemObjects | RaycastFlag.HasRadius | RaycastFlag.HasRangeLimit;
            rayPkt.Raycast();

            if (rayPkt.Count > 0)
            {
                result.objList = new List<GameObjectBody>();

                foreach (var resultItem in rayPkt)
                {
                    if (resultItem.obj != null)
                    {
                        result.objList.Add(resultItem.obj);
                    }
                }

                // is this ok? what if no ObjectHandles.were found? (this is in the original)
                result.flags |= PickerResultFlags.PRF_HAS_MULTI_OBJ;
            }
        }

        /// <summary>
        /// Add all targets spefified by the cone created between the caster and the picked location.
        /// </summary>
        [TempleDllLocation(0x100ba6a0)]
        public bool SetConeTargets(LocAndOffsets targetLocation)
        {
            result.flags = PickerResultFlags.PRF_HAS_LOCATION | PickerResultFlags.PRF_HAS_MULTI_OBJ;
            result.location = targetLocation;
            result.offsetz = 0;
            result.handle = null;

            var originPos = caster.GetLocationFull();
            var coneOrigin = originPos.ToInches2D();
            var coneTarget = targetLocation.ToInches2D();

            float radiusInches;
            if (!flagsTarget.HasFlag(UiPickerFlagsTarget.FixedRadius))
            {
                radiusInches = (coneTarget - coneOrigin).Length();
            }
            else
            {
                radiusInches = radiusTarget * locXY.INCH_PER_FEET;
            }

            var arcRad = Angles.ToRadians(degreesTarget);
            var arcStart = (coneTarget - coneOrigin).GetWorldRotation() - arcRad;

            using var objList = ObjList.ListCone(originPos, radiusInches, arcStart, arcRad, ObjectListFilter.OLC_ALL);
            result.objList = new List<GameObjectBody>(objList);

            if (!flagsTarget.HasFlag(UiPickerFlagsTarget.Unknown80h))
            {
                RemoveTargetsNotInLineOfSightOf(originPos);
            }

            DoExclusions();

            return true;
        }

        [TempleDllLocation(0x100ba540)]
        public void SetAreaTargets(LocAndOffsets targetLocation)
        {
            result = default;
            result.flags = PickerResultFlags.PRF_HAS_LOCATION | PickerResultFlags.PRF_HAS_MULTI_OBJ;
            result.location = targetLocation;

            var radiusInches = radiusTarget * locXY.INCH_PER_FEET;
            using var objList = ObjList.ListRadius(targetLocation, radiusInches, ObjectListFilter.OLC_ALL);
            result.objList = new List<GameObjectBody>(objList);

            if (!flagsTarget.HasFlag(UiPickerFlagsTarget.Unknown80h))
            {
                RemoveTargetsNotInLineOfSightOf(targetLocation);
            }

            DoExclusions();
        }

        [TempleDllLocation(0x100b9f60)]
        public void RemoveTargetsNotInLineOfSightOf(LocAndOffsets loc)
        {
            // Clear the result object if it's not in line of sight of the given location
            if ( result.HasSingleResult && !HasLineOfSight(loc, result.handle) )
            {
                result.handle = null;
                result.flags &= PickerResultFlags.PRF_HAS_SINGLE_OBJ;
            }

            if (result.HasMultipleResults)
            {
                for (var i = result.objList.Count - 1; i >= 0; i--)
                {
                    var obj = result.objList[i];
                    if (!HasLineOfSight(loc, obj))
                    {
                        result.objList.RemoveAt(i);
                    }
                }
            }
        }

        [TempleDllLocation(0x100B9C00)]
        private static bool HasLineOfSight(LocAndOffsets originLoc, GameObjectBody target)
        {
            if (originLoc == LocAndOffsets.Zero)
            {
                return false;
            }

            var objIterator = new RaycastPacket();
            objIterator.flags |= RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects
                | RaycastFlag.HasTargetObj;
            objIterator.origin = originLoc;
            objIterator.targetLoc = target.GetLocationFull();
            objIterator.target = target;
            return objIterator.TestLineOfSight();
        }

        public void DoExclusions()
        {
            if (flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st))
            {
                ExcludeTargets();
                FilterResults();
            }
            else
            {
                FilterResults();
                ExcludeTargets();
            }
        }


        [TempleDllLocation(0x100BA3C0)]
        private void ExcludeTargets()
        {
            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ))
            {
                if (!TargetValid(result.handle))
                {
                    result.handle = null;
                    result.flags &= ~PickerResultFlags.PRF_HAS_SINGLE_OBJ;
                }
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
            {
                result.objList.RemoveAll(target => !TargetValid(target));
            }
        }

        [TempleDllLocation(0x100BA030)]
        private void FilterResults()
        {
            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ))
            {
                if (!CheckTargetVsIncFlags(result.handle))
                {
                    result.handle = null;
                    result.flags &= ~PickerResultFlags.PRF_HAS_SINGLE_OBJ;
                }
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
            {
                result.objList.RemoveAll(target => !CheckTargetVsIncFlags(target));
            }
        }

        [TempleDllLocation(0x100b97c0)]
        public bool CheckTargetVsIncFlags(GameObjectBody tgt)
        {
            if (tgt == null)
            {
                return false;
            }

            if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Self) && (tgt == caster))
                return true;
            if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Other) && (tgt != caster))
                return true;
            var isCritter = tgt.IsCritter();
            if (incFlags.HasFlag(UiPickerIncFlags.UIPI_NonCritter) && !isCritter)
                return true;
            if (isCritter)
            {
                if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Dead) && GameSystems.Critter.IsDeadNullDestroyed(tgt))
                    return true;
                if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Undead) && GameSystems.Critter.IsUndead(tgt))
                    return true;
                if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Unconscious)
                    && GameSystems.Critter.IsDeadOrUnconscious(tgt))
                    return true;
                if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Hostile) && !GameSystems.Critter.IsFriendly(caster, tgt))
                    return true;
                if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Friendly) && GameSystems.Critter.IsFriendly(caster, tgt))
                    return true;
            }
            else if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Potion)
                     && tgt.type == ObjectType.food && GameSystems.Item.IsMagical(tgt))
                return true;
            else if (incFlags.HasFlag(UiPickerIncFlags.UIPI_Scroll) && tgt.type == ObjectType.scroll)
                return true;

            return false;
        }

        // check exclusions from flags, and range
        [TempleDllLocation(0x100ba0f0)]
        public bool TargetValid(GameObjectBody tgt)
        {
            if (tgt == null)
            {
                return false;
            }

            if (GameSystems.MapObject.IsUntargetable(tgt))
            {
                return false;
            }

            if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Self) && tgt == caster)
            {
                return false;
            }

            if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Other) && tgt != caster)
                return false;

            var isCritter = tgt.IsCritter();

            if (excFlags.HasFlag(UiPickerIncFlags.UIPI_NonCritter) && !isCritter)
            {
                return false;
            }

            if (isCritter)
            {
                if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Dead) && GameSystems.Critter.IsDeadNullDestroyed(tgt))
                    return false;
                if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Undead) && GameSystems.Critter.IsUndead(tgt))
                    return false;
                if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Unconscious) && GameSystems.Critter.IsDeadOrUnconscious(tgt))
                    return false;
                if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Hostile) && !GameSystems.Critter.IsFriendly(caster, tgt))
                    return false;
                if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Friendly) && GameSystems.Critter.IsFriendly(caster, tgt))
                    return false;
            }
            else if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Potion)
                     && tgt.type == ObjectType.food && GameSystems.Item.IsMagical(tgt))
                return false;
            else if (excFlags.HasFlag(UiPickerIncFlags.UIPI_Scroll) && tgt.type == ObjectType.scroll)
                return false;

            if (flagsTarget.HasFlag(UiPickerFlagsTarget.Range) && IsBaseModeTarget(UiPickerType.Single))
            {
                if (caster == null) return false;
                var casterLoc = caster.GetLocationFull();
                var tgtLoc = tgt.GetLocationFull();
                if (casterLoc.DistanceTo(tgtLoc) > locXY.INCH_PER_FEET * range + tgt.GetRadius() + caster.GetRadius())
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x101370E0)]
        public bool LosBlocked(GameObjectBody target)
        {
            using var raycast = new RaycastPacket();
            raycast.flags |= RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects
                | RaycastFlag.HasTargetObj;
            raycast.origin = caster.GetLocationFull();
            raycast.targetLoc = target.GetLocationFull();
            raycast.target = target;
            return !raycast.TestLineOfSight();
        }

        public bool SetSingleTgt(GameObjectBody tgt)
        {
            if (tgt == null)
            {
                return false;
            }

            if (result.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
            {
                result.objList = null;
            }

            result.flags = PickerResultFlags.PRF_HAS_LOCATION | PickerResultFlags.PRF_HAS_SINGLE_OBJ;
            result.handle = tgt;
            result.location = tgt.GetLocationFull();
            result.offsetz = tgt.GetFloat(obj_f.offset_z);

            DoExclusions();

            return true;
        }

        public void FreeObjlist()
        {
            result.objList = null;
            result.flags = 0;
        }

    }
}