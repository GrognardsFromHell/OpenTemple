using System;
using System.Diagnostics;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.Teleport;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Anim
{
    public static class AnimGoalActions
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10016530)]
        private static bool ContinueWithAnimation(GameObjectBody handle, AnimSlot slot, IAnimatedModel animHandle,
            out int eventOut)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000FF10)]
        public static bool GoalIsAlive(AnimSlot slot)
        {
            if (slot.param1.obj != null)
            {
                if (!slot.param1.obj.IsCritter()
                    || !GameSystems.Critter.IsDeadNullDestroyed(slot.param1.obj))
                    return true;
            }

            return false;
        }

        [TempleDllLocation(0x10010D60)]
        public static bool GoalActionPerform2(AnimSlot slot)
        {
            //Logger.Debug("GSF54 AnimGoalType.attempt_attack action frame");
            Trace.Assert(slot.param1.obj != null);
            Trace.Assert(slot.param2.obj != null);

            var targetFlags = slot.param2.obj.GetFlags();
            if (!targetFlags.HasFlag(ObjectFlag.OFF) && !targetFlags.HasFlag(ObjectFlag.DESTROYED))
            {
                GameSystems.D20.Actions.ActionFrameProcess(slot.param1.obj);
            }

            return true;
        }

        [TempleDllLocation(0x1001C100)]
        public static bool GoalAttackEndTurnIfUnreachable(AnimSlot slot)
        {
            var sub_10017BF0 = temple.GetRef < BOOL(__cdecl)(GameObjectBody, GameObjectBody) > (0x10017BF0);
            var result = sub_10017BF0(slot.param1.obj, slot.param2.obj);
            if (!result)
            {
                if (GameSystems.Combat.IsCombatActive())
                {
                    Logger.Debug("Anim sys for {0} ending turn...",
                        GameSystems.MapObject.GetDisplayName(slot.param1.obj));
                    GameSystems.Combat.AdvanceTurn(slot.param1.obj);
                }
            }

            return result;
        }

        [TempleDllLocation(0x1000E270)]
        public static bool GoalIsProne(AnimSlot slot)
        {
            if (slot.param1.obj != null)
            {
                return GameSystems.Critter.IsProne(slot.param1.obj);
            }

            Logger.Debug("Anim Assertion failed: obj != OBJ_HANDLE_NULL");
            return false;
        }

        [TempleDllLocation(0x1000E250)]
        public static bool GoalIsConcealed(AnimSlot slot)
        {
            //Logger.Debug("GoalState IsConcealed");
            return (GameSystems.Critter.IsConcealed(slot.param1.obj));
        }

        private static readonly float OneDegreeRadians = Angles.ToRadians(1.0f);

        [TempleDllLocation(0x100125F0)]
        public static bool GoalIsRotatedTowardNextPathNode(AnimSlot slot)
        {
            // static var GoalIsRotatedTowardNextPathNode = temple.GetPointer<int(AnimSlot *slot)>(0x100125f0);
            // return GoalIsRotatedTowardNextPathNode(&slot);

            if (slot.pCurrentGoal == null)
            {
                Debugger.Break(); // TODO: This is not allowed to happen
                slot.pCurrentGoal = slot.goals[slot.currentGoal];
            }

            if (slot.path.nodeCount <= 0)
            {
                return true;
            }

            // get the mover's location
            var obj = slot.param1.obj;
            var objLoc = obj.GetLocationFull();
            var objAbs = objLoc.ToInches2D();

            // get node loc
            if (slot.path.currentNode > 200 || slot.path.currentNode < 0)
            {
                Logger.Info("Anim: Illegal current node detected!");
                return true;
            }

            var nodeLoc = slot.path.nodes[slot.path.currentNode];
            var nodeAbs = nodeLoc.ToInches2D();

            if (objAbs == nodeAbs)
            {
                if (slot.path.currentNode + 1 >= slot.path.nodeCount)
                {
                    return true;
                }

                nodeLoc = slot.path.nodes[slot.path.currentNode + 1];
                nodeAbs = nodeLoc.ToInches2D();
            }

            var rot = slot.pCurrentGoal.scratchVal2.floatNum;
            var delta = nodeAbs - objAbs;
            rot = (float) (2 * MathF.PI + MathF.PI * 0.75 - MathF.Atan2(delta.Y, delta.X));
            if (rot < 0.0)
            {
                rot += 2 * MathF.PI;
            }

            if (rot > 2 * MathF.PI)
            {
                rot -= 2 * MathF.PI;
            }

            slot.pCurrentGoal.scratchVal2.floatNum = rot;

            var objRot = obj.GetFloat(obj_f.rotation);

            if (MathF.Sin(objRot - rot) > OneDegreeRadians)
                return false;

            if (MathF.Cos(objRot) - MathF.Cos(rot) > OneDegreeRadians) // in case it's a 180 degrees difference
                return false;

            return true;
        }

        [TempleDllLocation(0x10012C70)]
        public static bool GoalIsSlotFlag10NotSet(AnimSlot slot)
        {
            return !slot.flags.HasFlag(AnimSlotFlag.UNK5);
        }

        [TempleDllLocation(0x10011880)]
        public static bool GoalPlayGetHitAnim(AnimSlot slot)
        {
            if (slot.param1.obj == null)
            {
                Logger.Warn("Error in GSF65");
                return false;
            }

            var obj = slot.param1.obj;
            var locFull = obj.GetLocationFull();
            var worldXY = locFull.ToInches2D();

            var obj2 = slot.param2.obj;
            var loc2 = obj2.GetLocationFull();
            var worldXY2 = loc2.ToInches2D();

            var rot = obj.GetFloat(obj_f.rotation);

            var delta = worldXY2 - worldXY;

            var newRot = MathF.Atan2(delta.Y, delta.X) + MathF.PI * 3 / 4 - rot;
            while (newRot > MathF.PI * 2) newRot -= MathF.PI * 2;
            while (newRot < 0) newRot += MathF.PI * 2;

            var newRotAdj = newRot - MathF.PI / 4;

            WeaponAnim weaponIdParam = WeaponAnim.FrontHit;
            if (newRotAdj < MathF.PI / 4)
                weaponIdParam = WeaponAnim.FrontHit;
            else if (newRotAdj < MathF.PI * 3 / 4)
                weaponIdParam = WeaponAnim.LeftHit;
            else if (newRotAdj < MathF.PI * 5 / 4)
                weaponIdParam = WeaponAnim.BackHit;
            else if (newRotAdj < MathF.PI * 7 / 4)
                weaponIdParam = WeaponAnim.RightHit;

            var weaponAnimId = GameSystems.Critter.GetAnimId(slot.param1.obj, weaponIdParam);
            obj.SetAnimId(weaponAnimId);

            return true;
        }

        [TempleDllLocation(0x1000D150)]
        public static bool GoalIsCurrentPathValid(AnimSlot slot)
        {
            return slot.path.flags.HasFlag(PathFlags.PF_COMPLETE);
        }

        [TempleDllLocation(0x100185e0)]
        public static bool GoalUnconcealAnimate(AnimSlot slot)
        {
            //Logger.Debug("GSF 106 for {}, goal {}, flags {:x}, currentState {:x}", description.getDisplayName(slot.animObj), animGoalTypeNames[slot.pCurrentGoal.goalType], (uint)slot.flags, (uint)slot.currentState);
            var obj = slot.param1.obj;
            Trace.Assert(slot.param1.obj != null);

            var aasHandle = obj.GetOrCreateAnimHandle();
            Trace.Assert(aasHandle != null);

            if (IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            var animId = aasHandle.GetAnimId();

            if (!animId.IsWeaponAnim() &&
                (animId.GetNormalAnimType() == NormalAnimType.Death ||
                 animId.GetNormalAnimType() == NormalAnimType.Death2 ||
                 animId.GetNormalAnimType() == NormalAnimType.Death3))
            {
                ContinueWithAnimation(obj, slot, aasHandle, out var eventOut);

                // This is the ACTION trigger
                if ((eventOut & 1) != 0)
                {
                    slot.flags |= AnimSlotFlag.UNK3;
                    return true;
                }

                // If the animation is a looping animation, it does NOT have a
                // defined end, so we just trigger the end here anyway otherwise
                // this'll loop endlessly
                bool looping = false;
                /*if (animId.IsWeaponAnim() && ( animId.GetWeaponAnim() == WeaponAnim.Idle || animId.GetWeaponAnim() == WeaponAnim.CombatIdle)) {*/

                if (animId.IsWeaponAnim() && (animId.GetWeaponAnim() == WeaponAnim.Idle))
                {
                    //	// We will continue anyway down below, because the character is idling, so log a message
                    if ((eventOut & 2) == 0)
                    {
                        Logger.Info(
                            "Ending wait for animation action/end in goal {0}, because the idle animation would never end.",
                            slot.pCurrentGoal.goalType);
                    }

                    looping = true;
                }

                // This is the END trigger
                if (!looping && (eventOut & 2) == 0)
                {
                    //Logger.Debug("GSF 106: eventOut & 2, returned true");
                    return true;
                }

                // Clears WaypointDelay flag
                if (obj.IsNPC())
                {
                    var flags = obj.GetUInt64(obj_f.npc_ai_flags64);
                    obj.SetUInt64(obj_f.npc_ai_flags64, flags & 0xFFFFFFFFFFFFFFFD);
                }

                // Clear 0x10 slot flag
                slot.flags &= ~AnimSlotFlag.UNK5;
            }

            return false;
        }

        public static bool GoalStateFunc130(AnimSlot slot)
        {
            //Logger.Debug("GSF 130");
            var obj = slot.param1.obj;
            if (obj == null)
            {
                Logger.Warn("Missing anim object!");
                return false;
            }

            var aasHandle = obj.GetOrCreateAnimHandle();
            if (aasHandle == null)
            {
                Logger.Warn("No aas handle!");
                return false;
            }

            if (IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            ContinueWithAnimation(obj, slot, aasHandle, out var eventOut);

            if ((eventOut & 1) != 0)
                slot.flags |= AnimSlotFlag.UNK3;

            if ((eventOut & 2) != 0)
            {
                slot.flags &= ~(AnimSlotFlag.UNK5);
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x100107E0)]
        public static bool GoalPickpocketPerform(AnimSlot slot)
        {
            var tgt = slot.param2.obj;
            var handle = slot.param1.obj;

            if (tgt == null || handle.IsOffOrDestroyed || tgt.IsOffOrDestroyed)
            {
                return false;
            }

            GameSystems.Critter.Pickpocket(handle, tgt, out var gotCaught);

            if (!gotCaught)
            {
                slot.flags |= AnimSlotFlag.UNK12;
            }

            return true;
        }


        [TempleDllLocation(0x10012c80)]
        public static bool GoalSlotFlagSet8If4AndNotSetYet(AnimSlot slot)
        {
            //Logger.Debug("GSF83 for {}, current goal {} ({})", description.getDisplayName(slot.animObj), animGoalTypeNames[slot.pCurrentGoal.goalType], slot.currentGoal);
            var flags = slot.flags;
            if (flags.HasFlag(AnimSlotFlag.UNK3) && !flags.HasFlag(AnimSlotFlag.UNK4))
            {
                slot.flags = flags | AnimSlotFlag.UNK4;
                //Logger.Debug("GSF83 return true");
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x10010520)]
        public static bool GoalIsAnimatingConjuration(AnimSlot slot)
        {
            //Logger.Debug("GSF45");
            var obj = slot.param1.obj;
            Trace.Assert(obj != null);
            var aasHandle = obj.GetOrCreateAnimHandle();
            Trace.Assert(aasHandle != null);
            var animId = aasHandle.GetAnimId();
            return animId.IsConjureAnimation();
        }

        [TempleDllLocation(0x10010290)]
        public static bool GoalReturnFalse(AnimSlot slot)
        {
            // Was spell cast related, looked up 1st arg as spell
            return false;
        }

        [TempleDllLocation(0x100102c0)]
        public static bool GoalAttemptSpell(AnimSlot slot)
        {
            //Logger.Debug("GSF42");
            var obj = slot.param1.obj;
            Trace.Assert(obj != null);

            var spellId = slot.param2.number;
            slot.flags |= AnimSlotFlag.UNK3 | AnimSlotFlag.UNK4; // Sets 8 and 4

            if (spellId != 0)
            {
                GameSystems.D20.Actions.ActionFrameProcess(obj);
                GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.SpellEffect);

                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spell))
                {
                    var targetCount = spell.targetCount;
                    bool found = false;
                    for (uint i = 0; i < targetCount; i++)
                    {
                        if (spell.targetListHandles[i] == spell.caster)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        var dispIo = new DispIOTurnBasedStatus();
                        dispIo.tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
                        GameSystems.D20.Actions.dispatchTurnBasedStatusInit(spell.caster, dispIo);
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x1000e4f0)]
        public static bool GoalHasDoorInPath(AnimSlot slot)
        {
            /*static var org = temple.GetRef<std.remove_pointer<GoalCallback>.type>(0x1000e4f0);
            return org(slot);*/

            var obj = slot.param1.obj;
            Trace.Assert(obj != null);

            var nextPathPos = slot.path.GetNextNode();
            if (!nextPathPos.HasValue)
            {
                return false;
            }

            // There's no real rhyme or reason to this constant
            var testHeight = 36.0f;

            var nextPathPosWorld = nextPathPos.Value.ToInches3D();

            var currentPos = obj.GetLocationFull();
            var radius = obj.GetRadius();

            var currentPosWorld = currentPos.ToInches3D();

            // Effectively this builds a world position that is one tile ahead of the current
            // position along the critter's trajectory
            var testPos = currentPosWorld + locXY.INCH_PER_TILE * Vector3.Normalize(nextPathPosWorld - currentPosWorld);
            testPos.Z = testHeight;

            // List every door in a certain radius
            var doorSearch = ObjList.ListRadius(currentPos, radius + 150.0f, ObjectListFilter.OLC_PORTAL);

            foreach (var doorObj in doorSearch)
            {
                var doorModel = doorObj.GetOrCreateAnimHandle();
                if (doorModel != null)
                {
                    var doorAnimParams = doorObj.GetAnimParams();

                    var distFromDoor = doorModel.GetDistanceToMesh(doorAnimParams, testPos);
                    if (distFromDoor < radius)
                    {
                        // Store the door we will collide with in the scratchObj slot
                        slot.pCurrentGoal.scratch.obj = doorObj;
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x1000c9c0)]
        public static bool GoalStateCallback1(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var parentObj = slot.param2.obj;
            var goalType = (AnimGoalType) slot.stateFlagData;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            AssertAnimParam(parentObj != null); /*parentObj != OBJ_HANDLE_NULL*/

            if (obj == null || parentObj == null)
            {
                return false;
            }

            var newGoal = new AnimSlotGoalStackEntry(obj, goalType);
            slot.pCurrentGoal.CopyParamsTo(newGoal);

            var anim = obj.GetOrCreateAnimHandle();
            AssertAnimParam(anim != null);
            newGoal.animIdPrevious.number = anim.GetAnimId();
            newGoal.parent.obj = parentObj;
            return GameSystems.Anim.PushGoal(newGoal, out _);
        }


        [TempleDllLocation(0x1000ccf0)]
        public static bool GoalStateCallback3(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var ObjHnd = slot.param2.obj;
            var parentObj = slot.pCurrentGoal.parent.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(parentObj != null); /*parentObj != OBJ_HANDLE_NULL*/

            if (sourceObj == null || parentObj == null)
            {
                return false;
            }

            locXY targetLocation;
            if (ObjHnd != null)
            {
                targetLocation = ObjHnd.GetLocation();
            }
            else
            {
                targetLocation = slot.pCurrentGoal.targetTile.location.location;
            }

            var parentLocation = parentObj.GetLocation();
            // TODO This function does actually not do anything sub_100627C0(parentLocation, targetLocation);
            return true;
        }

        [TempleDllLocation(0x1000ce10)]
        public static bool GoalSetOffAndDestroyParam1(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            if (obj == null || obj.HasFlag(ObjectFlag.DESTROYED))
            {
                return false;
            }

            obj.SetFlag(ObjectFlag.OFF, true);
            GameSystems.Object.Destroy(obj);
            return true;
        }

        [TempleDllLocation(0x1000ce60)]
        public static bool GoalParam1ObjCloseToParam2Loc(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL

            var location = slot.param2.location;
            var locSelf = obj.GetLocationFull();

            var distance = locSelf.DistanceTo(location);

            return distance <= locXY.INCH_PER_TILE / 6.0f;
        }


        [TempleDllLocation(0x1000cf10)]
        public static bool GoalTargetLocWithinRadius(AnimSlot slot)
        {
            var location = slot.param2.location;
            var sourceObj = slot.param1.obj;
            var range = slot.pCurrentGoal.animId.number; // TODO: this just seems wrong
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null)
            {
                var objLoc = sourceObj.GetLocationFull();
                return objLoc.DistanceTo(location) >= range;
            }

            return false;
        }


        [TempleDllLocation(0x1000cfe0)]
        public static bool GoalStateCallback7(AnimSlot slot)
        {
            var pathFlags = slot.path.flags;
            if (pathFlags.HasFlag(PathFlags.PF_COMPLETE) && !pathFlags.HasFlag(PathFlags.PF_2))
            {
                return false;
            }

            // TODO: This does not make a lot of sense since it just copies the value over itself
            // TODO: Is this intended to copy it "back" one goal???
            if (slot.currentGoal > 0)
            {
                slot.goals[slot.currentGoal].scratchVal4 = slot.pCurrentGoal.scratchVal4;
            }

            slot.path.flags &= PathFlags.PF_COMPLETE;
            GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);

            slot.flags &= ~ AnimSlotFlag.RUNNING;

            if (slot.path.flags.HasFlag(PathFlags.PF_2))
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
            }

            return true;
        }


        [TempleDllLocation(0x1000d060)]
        public static bool GoalStateCallback8(AnimSlot slot)
        {
            if (slot.param2.obj != null)
            {
                var pOut = slot.param2.obj.GetLocationFull();
                if (slot.pCurrentGoal.targetTile.location.DistanceTo(pOut) <= 0.000001f)
                {
                    slot.path.flags &= PathFlags.PF_COMPLETE;
                    GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
                }
            }

            // TODO: From here onwards this seems like an inlined GoalStateCallback7

            var pathFlags = slot.path.flags;
            if (pathFlags.HasFlag(PathFlags.PF_COMPLETE) && !pathFlags.HasFlag(PathFlags.PF_2))
            {
                return false;
            }

            if (slot.currentGoal > 0)
            {
                slot.goals[slot.currentGoal].scratchVal4 = slot.pCurrentGoal.scratchVal4;
            }

            slot.path.flags &= PathFlags.PF_COMPLETE;
            GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);

            slot.flags &= ~ AnimSlotFlag.RUNNING;

            if (slot.path.flags.HasFlag(PathFlags.PF_2))
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
            }

            return true;
        }


        [TempleDllLocation(0x1000d560)]
        public static bool GoalCalcPathToTarget(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            slot.field_14 = slot.currentGoal;

            var query = new PathQuery();
            query.flags2 = 0x2000;
            if (slot.flags.HasFlag(AnimSlotFlag.UNK8))
                query.flags2 |= 0x78;
            query.flags = PathQueryFlags.PQF_HAS_CRITTER;
            query.critter = obj;
            query.from = obj.GetLocationFull();
            query.to = targetLoc;
            query.flags = PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_800;
            if (!GameSystems.Combat.IsCombatActive())
                query.flags |= PathQueryFlags.PQF_IGNORE_CRITTERS;
            if (GameSystems.PathX.FindPath(query, out slot.path))
            {
                if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
                {
                    GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                }

                return true;
            }

            if (!slot.flags.HasFlag(AnimSlotFlag.UNK8))
            {
                slot.flags |= AnimSlotFlag.UNK8;
                query.flags2 |= 0x78;
                if (GameSystems.PathX.FindPath(query, out slot.path))
                {
                    if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
                    {
                        GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                    }

                    return true;
                }
            }

            return false;
        }


        [TempleDllLocation(0x1000db30)]
        public static bool GoalCalcPathToTarget2(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL

            var targetLoc = slot.param2.location;
            slot.field_14 = slot.currentGoal;
            if (targetLoc.location == locXY.Zero)
            {
                return false;
            }

            var query = new PathQuery();
            query.flags = PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_TO_EXACT;
            query.from = obj.GetLocationFull();
            query.to = targetLoc;
            query.critter = obj;
            query.flags2 = 0;
            GameSystems.PathX.FindPath(query, out slot.path);
            if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
            {
                GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
            }

            var objLoc = obj.GetLocation();

            var deltaIdxMax =
                GameSystems.PathX.RasterizeLineBetweenLocsScreenspace(objLoc, targetLoc.location, slot.animPath.deltas);
            slot.animPath.deltaIdxMax = deltaIdxMax;
            if (deltaIdxMax == 0)
                return false;
            slot.animPath.flags &= ~AnimPathFlag.UNK_4;
            slot.animPath.fieldD4 = 0;
            return true;
        }


        [TempleDllLocation(0x1000dca0)]
        public static bool GoalKnockbackFunc(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var location = slot.param2.location.location;

            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL
            slot.field_14 = slot.currentGoal;

            if (location == locXY.Zero)
            {
                return false;
            }

            var objLoc = obj.GetLocation();
            slot.animPath.fieldD0 = objLoc.GetCompassDirection(location);
            var deltaIdxMax =
                GameSystems.PathX.RasterizeLineBetweenLocsScreenspace(objLoc, location, slot.animPath.deltas);
            slot.animPath.deltaIdxMax = deltaIdxMax;
            if (deltaIdxMax == 0)
                return false;
            slot.animPath.flags &= ~AnimPathFlag.UNK_4;
            slot.animPath.fieldD4 = 0;
            return true;
        }

        private ref struct AnimPathData
        {
            public GameObjectBody handle; // TODO: movingObj
            public locXY srcLoc;
            public locXY destLoc;
            public int size;
            public Span<sbyte> deltas;
            public AnimPathDataFlags flags; // Same as PathQuery.flags2
            public int distTiles;
        }

        [Flags]
        public enum AnimPathDataFlags
        {
            UNK_1 = 1,
            CantOpenDoors = 2,
            UNK_4 = 4,
            UNK_8 = 4,
            MovingSilently = 0x200,
            UNK10 = 0x10,
            UNK20 = 0x20,
            UNK40 = 0x40,
            UNK_2000 = 0x2000
        }

        private static bool AnimPathSpecInit(ref AnimPathData pPathData)
        {
            Trace.Assert(pPathData.handle != null);

            if (pPathData.handle.IsCritter())
            {
                if (pPathData.handle.GetSpellFlags().HasFlag(SpellFlag.ENTANGLED))
                {
                    return false;
                }

                if (!GameSystems.Critter.CanOpenPortals(pPathData.handle))
                {
                    pPathData.flags |= AnimPathDataFlags.CantOpenDoors;
                }

                // This is AAS related somehow
                pPathData.flags |= AnimPathDataFlags.UNK_4;

                if (GameSystems.Critter.IsMovingSilently(pPathData.handle))
                {
                    pPathData.flags |= AnimPathDataFlags.MovingSilently;
                }

                pPathData.flags |= AnimPathDataFlags.UNK10;
            }

            return true;
        }

        [TempleDllLocation(0x1000d750)]
        private static int anim_create_path_max_length(ref AnimPath pAnimPath,
            locXY srcLoc,
            locXY destLoc,
            GameObjectBody obj)
        {
            Trace.Assert(pAnimPath.maxPathLength >= 0);

            if (pAnimPath.maxPathLength > 0)
            {
                pAnimPath.maxPathLength += 5;
                if (pAnimPath.maxPathLength > pAnimPath.fieldE4)
                {
                    if (pAnimPath.fieldE4 <= 0)
                    {
                        var estdistance = srcLoc.EstimateDistance(destLoc);
                        if (estdistance > pAnimPath.range)
                        {
                            Logger.Error("anim_create_path_max_length: Estimated Distance is too large: {0} [{1}]",
                                estdistance, obj);
                        }

                        pAnimPath.fieldE4 = 4 * estdistance + 5;
                        if (pAnimPath.fieldE4 > pAnimPath.range)
                            pAnimPath.fieldE4 = pAnimPath.range;
                        if (pAnimPath.maxPathLength > pAnimPath.fieldE4)
                        {
                            if (pAnimPath.fieldE4 <= 0)
                                pAnimPath.fieldE4 = pAnimPath.range;
                            else
                                pAnimPath.maxPathLength = pAnimPath.fieldE4;
                        }
                    }
                    else
                    {
                        pAnimPath.maxPathLength = pAnimPath.fieldE4;
                    }
                }
            }
            else
            {
                var estDist = srcLoc.EstimateDistance(destLoc);
                if (estDist > pAnimPath.range)
                {
                    Logger.Error("anim_create_path_max_length: Estimated Distance is too large: {0} [{1}]", estDist,
                        obj);
                    Logger.Warn("   SrcLocAxis: ({0})", srcLoc);
                    Logger.Warn(", DstLocAxis: ({0})", destLoc);
                }

                pAnimPath.maxPathLength = 4 * estDist + 5;
                if (pAnimPath.fieldE4 > pAnimPath.range)
                    pAnimPath.fieldE4 = pAnimPath.range;
            }

            if (pAnimPath.maxPathLength > 0)
            {
                if (pAnimPath.maxPathLength > pAnimPath.range)
                {
                    Logger.Error("anim_create_path_max_length: Path Length is out of range: {0} [{1}]",
                        pAnimPath.maxPathLength, obj);
                }
            }
            else
            {
                Logger.Error("anim_create_path_max_length: Path Length is 0 [{0}]", obj);
            }

            if (pAnimPath.maxPathLength > pAnimPath.range)
                pAnimPath.maxPathLength = pAnimPath.range;
            return pAnimPath.maxPathLength;
        }

        [TempleDllLocation(0x1003fca0)]
        private static int AnimPathSearch(ref AnimPathData pathData)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000dd80)]
        public static bool GoalMoveAwayFromObj(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            if (targetObj == null || sourceObj == null)
            {
                return false;
            }

            var distance = slot.pCurrentGoal.animId.number;
            var source_location = sourceObj.GetLocation();
            var target_location = targetObj.GetLocation();
            if (targetObj.IsOffOrDestroyed)
                return false;
            var dir = target_location.GetCompassDirection(source_location);

            if (distance != -1)
            {
                for (var i = 0; i < distance + 1; i++)
                {
                    target_location = target_location.Offset(dir);
                }
            }

            slot.pCurrentGoal.targetTile.location = new LocAndOffsets(target_location, 0, 0);

            var pathQuery = new PathQuery();
            pathQuery.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER;
            pathQuery.critter = sourceObj;
            pathQuery.to = new LocAndOffsets(target_location, targetObj.OffsetX, targetObj.OffsetY);
            pathQuery.from = sourceObj.GetLocationFull();
            pathQuery.flags2 = 0x2000;
            GameSystems.PathX.FindPath(pathQuery, out slot.path);
            if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
            {
                GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
            }

            slot.field_14 = slot.currentGoal;
            var maxLen = anim_create_path_max_length(ref slot.animPath, source_location, target_location, sourceObj);
            var pathDataFlags = AnimPathDataFlags.UNK_2000;
            if (sourceObj.GetSpellFlags().HasFlag(SpellFlag.POLYMORPHED))
                pathDataFlags = AnimPathDataFlags.UNK_2000 | AnimPathDataFlags.CantOpenDoors | AnimPathDataFlags.UNK_4;
            if (slot.flags.HasFlag(AnimSlotFlag.UNK8))
                pathDataFlags |= AnimPathDataFlags.UNK_8 | AnimPathDataFlags.UNK10 | AnimPathDataFlags.UNK20 |
                                 AnimPathDataFlags.UNK40;
            var deltas = slot.animPath.deltas.AsSpan();

            var noOffsets = true;
            if (MathF.Abs(sourceObj.OffsetX) >= 0 || MathF.Abs(sourceObj.OffsetY) >= 0)
            {
                --maxLen;
                deltas = deltas.Slice(1);
                noOffsets = false;
            }

            var pPathData = new AnimPathData();
            pPathData.handle = sourceObj;
            pPathData.srcLoc = sourceObj.GetLocation();
            pPathData.destLoc = target_location;
            pPathData.deltas = deltas;
            pPathData.size = maxLen;
            pPathData.flags = pathDataFlags;
            if (!AnimPathSpecInit(ref pPathData))
            {
                AiEndCombatTurn(sourceObj);
                return false;
            }

            ref var animPath = ref slot.animPath;
            animPath.deltaIdxMax = AnimPathSearch(ref pPathData);
            animPath.objLoc = pPathData.srcLoc;
            animPath.tgtLoc = target_location;

            if (animPath.deltaIdxMax == 0)
            {
                AiEndCombatTurn(sourceObj);
                return false;
            }

            if (pPathData.distTiles != 0)
            {
                animPath.flags |= AnimPathFlag.UNK_20;
                GameSystems.AI.SetNoFlee(sourceObj);
            }

            animPath.flags &= ~AnimPathFlag.UNK_4;
            animPath.fieldD4 = 0;
            if (!noOffsets)
            {
                var direction = (sbyte) (sourceObj.Rotation * 4.0f / MathF.PI);
                if (direction == animPath.deltas[1])
                {
                    animPath.fieldD4 = 1;
                    return true;
                }

                var posDirection = (direction + 4) % 8;
                var rotation = posDirection * MathF.PI / 4.0f;
                GameSystems.MapObject.SetRotation(sourceObj, rotation);
                slot.animPath.deltas[0] = (sbyte) posDirection;
            }

            return true;
        }


        [TempleDllLocation(0x1000e2c0)]
        public static bool GoalStunnedExpire(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL
            if (!GameSystems.Combat.IsCombatActive())
            {
                --slot.pCurrentGoal.scratchVal5.number;
            }

            if (slot.pCurrentGoal.scratchVal5.number > 0)
            {
                return false;
            }

            var critterFlags = obj.GetCritterFlags();
            if (critterFlags.HasFlag(CritterFlag.STUNNED))
            {
                obj.SetCritterFlags(critterFlags & ~CritterFlag.STUNNED);
            }

            return true;
        }


        [TempleDllLocation(0x1000e6f0)]
        public static bool GoalFindPathNear(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            slot.field_14 = slot.currentGoal;

            var query = new PathQuery();
            query.flags2 = 0x2000;
            if (slot.flags.HasFlag(AnimSlotFlag.UNK8))
                query.flags2 = 0x2078;
            query.to = targetLoc;
            query.critter = obj;
            query.from = obj.GetLocationFull();
            query.distanceToTargetMin = 0.0f;
            query.flags = PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE | PathQueryFlags.PQF_HAS_CRITTER |
                          PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_800;

            var slota = slot.pCurrentGoal.animId.number;
            query.tolRadius = slota * 12.0f; // TODO Conversion to inches?
            if (!GameSystems.Combat.IsCombatActive())
                query.flags |= PathQueryFlags.PQF_IGNORE_CRITTERS;

            if (GameSystems.PathX.FindPath(query, out slot.path))
            {
                if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
                {
                    GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                }

                return true;
            }

            if (!slot.flags.HasFlag(AnimSlotFlag.UNK8))
            {
                slot.flags |= AnimSlotFlag.UNK8;
                query.flags2 |= 0x78;

                if (GameSystems.PathX.FindPath(query, out slot.path))
                {
                    if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
                    {
                        GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                    }

                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1000D160)]
        private static void SetPathQueryFlag2(PathQuery query)
        {
            if (!GameSystems.Critter.CanOpenPortals(query.critter))
            {
                query.flags2 |= (int) AnimPathDataFlags.CantOpenDoors;
            }

            // This is AAS related somehow
            query.flags2 |= (int) AnimPathDataFlags.UNK_4;

            if (GameSystems.Critter.IsMovingSilently(query.critter))
            {
                query.flags2 |= (int) AnimPathDataFlags.MovingSilently;
            }

            query.flags2 |= (int) AnimPathDataFlags.UNK10;
        }

        [TempleDllLocation(0x1000e8b0)]
        public static bool GoalFindPathNearObject(AnimSlot slot)
        {
            var source = slot.param1.obj;
            var target = slot.param2.obj;
            AssertAnimParam(source != null); /*source != OBJ_HANDLE_NULL*/
            AssertAnimParam(target != null); /*target != OBJ_HANDLE_NULL*/
            Logger.Info("AGcreatePathNearObj( {0} )", source);
            slot.field_14 = slot.currentGoal;
            if (target.IsOffOrDestroyed)
                return false;
            var v12 = slot.pCurrentGoal.animId.number;
            var sourceLoc = source.GetLocationFull();
            var targetLoc = target.GetLocationFull();
            anim_create_path_max_length(ref slot.animPath, sourceLoc.location, targetLoc.location, source);
            slot.pCurrentGoal.targetTile.location = targetLoc;
            PathQuery a1 = new PathQuery();
            a1.from = sourceLoc;
            a1.tolRadius = v12 * 12.0f; // TODO Conversion to inches?
            a1.targetObj = target;
            a1.to = targetLoc;
            a1.flags = PathQueryFlags.PQF_ADJUST_RADIUS | PathQueryFlags.PQF_TARGET_OBJ |
                       PathQueryFlags.PQF_IGNORE_CRITTERS | PathQueryFlags.PQF_HAS_CRITTER |
                       PathQueryFlags.PQF_TO_EXACT;
            a1.critter = source;
            a1.distanceToTargetMin = 0.0f;
            a1.from = source.GetLocationFull();

            var v10 = (v12 != 0) ? 1 : 0;
            a1.flags2 = v10;

            if (!slot.flags.HasFlag(AnimSlotFlag.UNK9))
                a1.flags2 = 0;
            SetPathQueryFlag2(a1);
            if (GameSystems.PathX.FindPath(a1, out slot.path))
            {
                if (slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
                {
                    GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                }

                return true;
            }

            if (v10 == a1.flags2)
                return false;
            if (slot.flags.HasFlag(AnimSlotFlag.UNK9))
                return false;
            a1.flags2 = v10;

            SetPathQueryFlag2(a1);
            if (GameSystems.PathX.FindPath(a1, out slot.path) && slot.path.flags.HasFlag(PathFlags.PF_COMPLETE))
            {
                GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                if (v12 > 0 && slot.path.GetPathResultLength() < v12)
                {
                    return false;
                }

                return true;
            }

            return false;
        }


        [TempleDllLocation(0x1000ec10)]
        public static bool GoalFindPathNearObjectCombat(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            if (sourceObj == null || targetObj == null)
            {
                return false;
            }

            slot.field_14 = slot.currentGoal;
            if (targetObj.IsOffOrDestroyed)
                return false;

            var pString = sourceObj.GetLocation();
            var v26 = targetObj.GetLocation();
            var v22 = anim_create_path_max_length(
                ref slot.animPath,
                pString,
                v26,
                sourceObj);
            slot.pCurrentGoal.targetTile.location = targetObj.GetLocationFull();

            AnimPathDataFlags slota = default;
            var pathNumber = slot.pCurrentGoal.animId.number;
            if (pathNumber > 0)
                slota = AnimPathDataFlags.UNK_1;

            var pPathData = new AnimPathData();
            pPathData.srcLoc = pString;
            pPathData.destLoc = v26;
            pPathData.deltas = slot.animPath.deltas.AsSpan();
            pPathData.handle = sourceObj;
            pPathData.size = v22;
            pPathData.flags = slota;
            if (!slot.flags.HasFlag(AnimSlotFlag.UNK9))
                pPathData.flags = slota & ~AnimPathDataFlags.UNK_1;
            if (AnimPathSpecInit(ref pPathData))
                slot.animPath.deltaIdxMax = AnimPathSearch(ref pPathData);
            else
                slot.animPath.deltaIdxMax = 0;

            slot.animPath.objLoc = pPathData.srcLoc;
            slot.animPath.tgtLoc = pPathData.destLoc;
            if (slot.animPath.deltaIdxMax != 0)
            {
                slot.animPath.flags &= ~AnimPathFlag.UNK_4;
                slot.animPath.fieldD4 = 0;
                return true;
            }

            if (slota == pPathData.flags || slot.flags.HasFlag(AnimSlotFlag.UNK9))
            {
                AiEndCombatTurn(sourceObj);
                return false;
            }

            pPathData.flags = slota;
            if (!AnimPathSpecInit(ref pPathData))
            {
                AiEndCombatTurn(sourceObj);
                return false;
            }

            slot.animPath.deltaIdxMax = AnimPathSearch(ref pPathData);
            slot.animPath.objLoc = pPathData.srcLoc;
            slot.animPath.tgtLoc = pPathData.destLoc;
            if (slot.animPath.deltaIdxMax == 0)
            {
                AiEndCombatTurn(sourceObj);
                return false;
            }

            slot.animPath.flags &= ~AnimPathFlag.UNK_1;
            slot.animPath.fieldD4 = 0;
            if (pathNumber <= 0)
                return true;

            slot.animPath.deltaIdxMax -= pathNumber + 1;
            if (slot.animPath.deltaIdxMax >= 1)
                return true;

            AiEndCombatTurn(sourceObj);
            return false;
        }

        private static void AiEndCombatTurn(GameObjectBody obj)
        {
            if (!GameSystems.Party.IsPlayerControlled(obj))
            {
                Logger.Info("AI for {0} ending turn...", obj);
                GameSystems.Combat.AdvanceTurn(obj);
            }
        }

        [TempleDllLocation(0x1000efb0)]
        public static bool GoalIsParam1Door(AnimSlot slot)
        {
            var door = slot.param1.obj;
            if (door == null || door.IsOffOrDestroyed)
                return false;

            return door.type == ObjectType.portal;
        }

        [TempleDllLocation(0x1000f000)]
        public static bool GoalPlayDoorLockedSound(AnimSlot slot) // TODO: More like "attempt open"
        {
            var doorObj = slot.param1.obj;
            var selfObj = slot.param2.obj;
            AssertAnimParam(doorObj != null);
            AssertAnimParam(selfObj != null);
            if (doorObj == null)
            {
                return false;
            }

            if (selfObj != null)
            {
                if (doorObj.IsOffOrDestroyed)
                {
                    return false;
                }

                if (GameSystems.AI.AttemptToOpenDoor(selfObj, doorObj) != PortalLockStatus.PLS_OPEN)
                {
                    var soundId = GameSystems.SoundMap.GetPortalSoundEffect(doorObj, 2);
                    GameSystems.SoundGame.PositionalSound(soundId, 1, doorObj);
                    return false;
                }
            }

            return true;
        }


        [TempleDllLocation(0x1000f0d0)]
        public static bool GoalIsDoorMagicallyHeld(AnimSlot slot)
        {
            var doorObj = slot.param1.obj;
            AssertAnimParam(doorObj != null); // doorObj != OBJ_HANDLE_NULL

            if (doorObj.IsOffOrDestroyed)
            {
                return false;
            }

            return doorObj.GetPortalFlags().HasFlag(PortalFlag.MAGICALLY_HELD);
        }


        [TempleDllLocation(0x1000f140)]
        public static bool GoalAttemptOpenDoor(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var portalObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(portalObj != null); /*portalObj != OBJ_HANDLE_NULL*/

            if (sourceObj == null || portalObj == null || !GameSystems.Critter.CanOpenPortals(sourceObj))
            {
                return false;
            }

            var openResult = GameSystems.AI.AttemptToOpenDoor(sourceObj, portalObj);
            if (openResult == PortalLockStatus.PLS_OPEN)
            {
                return true;
            }

            var translation = Tig.FS.ReadMesFile("mes/intgame.mes"); // TODO: Cache / move somewhere else
            string line;
            switch (openResult)
            {
                case PortalLockStatus.PLS_JAMMED:
                    line = translation[26];
                    break;
                case PortalLockStatus.PLS_MAGICALLY_HELD:
                    line = translation[27];
                    break;
                case PortalLockStatus.PLS_LOCKED:
                    if (GameSystems.Script.ExecuteObjectScript(sourceObj, portalObj, ObjScriptEvent.UnlockAttempt) == 0)
                    {
                        return false;
                    }

                    line = translation[9];
                    break;
                case PortalLockStatus.PLS_SECRET_UNDISCOVERED:
                    line = null;
                    break;
                default:
                    line = translation[9];
                    break;
            }

            if (line != null)
            {
                GameSystems.TextFloater.FloatLine(portalObj, TextFloaterCategory.Generic, TextFloaterColor.Blue, line);
            }

            return false;
        }


        [TempleDllLocation(0x1000f2c0)]
        public static bool GoalIsDoorLocked(AnimSlot slot)
        {
            var doorObj = slot.param1.obj;
            var actor = slot.param2.obj;
            AssertAnimParam(doorObj != null); // doorObj != OBJ_HANDLE_NULL
            if (doorObj.IsOffOrDestroyed)
            {
                return false;
            }

            return GameSystems.AI.AttemptToOpenDoor(actor, doorObj) != PortalLockStatus.PLS_OPEN;
        }


        [TempleDllLocation(0x1000f350)]
        public static bool GoalUnlockDoorReturnFalse(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var doorObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(doorObj != null); /*doorObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null && doorObj != null && !doorObj.IsOffOrDestroyed)
            {
                GameSystems.AI.AttemptToOpenDoor(sourceObj, doorObj);
            }

            return false;
        }

        [TempleDllLocation(0x1000f400)]
        public static bool GoalIsDoorUnlockedAlwaysReturnFalse(AnimSlot slot)
        {
            return false;
        }

        [TempleDllLocation(0x1000f490)]
        public static bool GoalSetRadiusTo2(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;

            AssertAnimParam(sourceObj != null); // sourceObj != OBJ_HANDLE_NULL
            AssertAnimParam(targetObj != null); // targetObj != OBJ_HANDLE_NULL

            if (sourceObj != null && targetObj != null)
            {
                slot.pCurrentGoal.animId.number = 2;
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1000f550)]
        public static bool GoalUseObject(AnimSlot slot)
        {
            var target = slot.param2.obj;
            var source = slot.param1.obj;
            AssertAnimParam(source != null); // source != OBJ_HANDLE_NULL
            if (target == null)
            {
                target = slot.pCurrentGoal.scratch.obj;
                AssertAnimParam(target != null); /*target!= OBJ_HANDLE_NULL*/
            }

            if (GameSystems.Tile.IsBlockingOldVersion(target.GetLocation()))
                return false;

            if (target.type == ObjectType.container)
            {
                if (target.NeedsToBeUnlocked()
                    || GameSystems.Script.ExecuteObjectScript(source, target, source, ObjScriptEvent.Use, 0) != 0)
                {
                    if (source.type == ObjectType.pc)
                    {
                        GameUiBridge.OpenContainer(source, target);
                        return true;
                    }

                    return true;
                }

                return false;
            }

            if (target.IsCritter())
            {
                if (source.GetSpellFlags().HasFlag(SpellFlag.POLYMORPHED))
                {
                    return false;
                }

                if (source.type == ObjectType.pc)
                {
                    GameUiBridge.OpenContainer(source, target);
                    return true;
                }

                return true;
            }

            if (target.type == ObjectType.scenery)
            {
                if (GameSystems.Script.ExecuteObjectScript(source, target, source, ObjScriptEvent.Use, 0) == 0)
                {
                    return false;
                }

                if (target.GetSceneryFlags().HasFlag(SceneryFlag.USE_OPEN_WORLDMAP))
                {
                    GameUiBridge.ShowWorldMap(0);
                    return true;
                }

                var jumpPoint = target.GetInt32(obj_f.scenery_teleport_to);
                if (jumpPoint != 0)
                {
                    var teleport = new FadeAndTeleportArgs();
                    if (GameSystems.JumpPoint.TryGet(jumpPoint, out _, out teleport.destMap, out teleport.destLoc))
                    {
                        var currentMapId = GameSystems.Map.GetCurrentMapId();
                        var currentPos = source.GetLocationFull();

                        teleport.flags = FadeAndTeleportFlags.CenterOnPartyLeader
                                         | FadeAndTeleportFlags.FadeIn | FadeAndTeleportFlags.FadeOut;

                        teleport.FadeOutArgs.color = PackedLinearColorA.Black;
                        teleport.FadeOutArgs.transitionTime = 1.0f;
                        teleport.FadeOutArgs.fadeSteps = 48;

                        teleport.FadeInArgs.flags = FadeFlag.FadeIn;
                        teleport.FadeInArgs.transitionTime = 1.0f;
                        teleport.FadeInArgs.fadeSteps = 48;

                        if (teleport.destMap == currentMapId)
                        {
                            teleport.destMap = -1;
                        }
                        else
                        {
                            teleport.movieId = GameSystems.Map.GetEnterMovie(teleport.destMap, false);
                            if (teleport.movieId != 0)
                            {
                                teleport.flags |= FadeAndTeleportFlags.play_movie;
                                teleport.movieFlags = 0;
                            }
                        }

                        teleport.somehandle = source;
                        GameSystems.Teleport.FadeAndTeleport(teleport);

                        GameSystems.AI.ForceSpreadOut(source);
                        GameSystems.Secretdoor.MarkUsed(target);
                        GameSystems.Map.SetFleeInfo(currentMapId, currentPos, teleport.destLoc);
                    }
                }
            }
            else if (GameSystems.Script.ExecuteObjectScript(source, target, source, ObjScriptEvent.Use, 0) == 0)
            {
                return false;
            }

            return true;
        }


        [TempleDllLocation(0x1000f860)]
        public static bool GoalUseItemOnObj(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            var itemObj = slot.pCurrentGoal.scratch.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            if (targetObj == null)
            {
                targetObj = itemObj;
                AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            }

            AssertAnimParam(itemObj != null); /*itemObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null && targetObj != null && itemObj != null &&
                GameSystems.Item.GetParent(itemObj) == sourceObj)
            {
                GameSystems.Item.UseOnObject(sourceObj, itemObj, targetObj);
                return true;
            }
            else
            {
                return false;
            }
        }


        [TempleDllLocation(0x1000f9a0)]
        public static bool GoalUseItemOnObjWithSkillDummy(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            var itemObj = slot.pCurrentGoal.scratch.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            if (targetObj == null)
            {
                targetObj = itemObj;
                AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            }

            AssertAnimParam(itemObj != null); /*itemObj != OBJ_HANDLE_NULL*/

            if (sourceObj != null
                && targetObj != null
                && itemObj != null
                && !targetObj.IsOffOrDestroyed
                && !itemObj.IsOffOrDestroyed
                && GameSystems.Item.GetParent(itemObj) == sourceObj)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [TempleDllLocation(0x1000fbc0)]
        public static bool GoalUseItemOnLoc(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var itemObj = slot.pCurrentGoal.scratch.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetLoc != LocAndOffsets.Zero); /*targetLoc != 0*/
            AssertAnimParam(itemObj != null); /*itemObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null
                && targetLoc != LocAndOffsets.Zero
                && itemObj != null
                && GameSystems.Item.GetParent(itemObj) == sourceObj)
            {
                GameSystems.Item.UseOnLocation(sourceObj, itemObj, targetLoc);
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1000fce0)]
        public static bool GoalUseItemOnLocWithSkillDummy(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var itemObj = slot.pCurrentGoal.scratch.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetLoc != LocAndOffsets.Zero); /*targetLoc != 0*/
            AssertAnimParam(itemObj != null); /*itemObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null
                && targetLoc != LocAndOffsets.Zero
                && itemObj != null
                && !itemObj.IsOffOrDestroyed
                && GameSystems.Item.GetParent(itemObj) == sourceObj)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1000fec0)]
        public static bool GoalSetNoFlee(AnimSlot slot)
        {
            var critObj = slot.param1.obj;
            AssertAnimParam(critObj != null); // critObj != OBJ_HANDLE_NULL

            if (critObj != null)
            {
                GameSystems.AI.SetNoFlee(critObj);
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x1000ff60)]
        public static bool GoalPlaySoundScratch5(AnimSlot slot)
        {
            var selfObj = slot.param1.obj;
            AssertAnimParam(selfObj != null); // selfObj != OBJ_HANDLE_NULL

            if (selfObj != null)
            {
                var soundId = slot.pCurrentGoal.scratchVal5.number;
                if (soundId != -1)
                {
                    GameSystems.SoundGame.PositionalSound(soundId, 1, selfObj);
                    for (var v3 = 0; v3 <= slot.currentGoal; v3++)
                    {
                        slot.goals[v3].scratchVal5.number = -1;
                    }
                }

                return true;
            }

            return false;
        }


        [TempleDllLocation(0x1000fff0)]
        public static bool GoalAttemptAttackCheck(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            if (sourceObj == null
                || targetObj == null
                || !GameSystems.Critter.IsCombatModeActive(sourceObj)
                || !GameSystems.Party.IsPlayerControlled(sourceObj)
                || !GameSystems.Combat.IsGameConfigAutoAttack()
                || targetObj.IsOffOrDestroyed
                || GameSystems.Critter.IsDeadNullDestroyed(targetObj)
                || GameSystems.MapObject.IsBusted(targetObj)
                || slot.flags.HasFlag(AnimSlotFlag.UNK10)
                || GameSystems.Combat.IsCombatActive()
                || sourceObj.GetCritterFlags().HasFlag(CritterFlag.NON_LETHAL_COMBAT)
                || sourceObj.GetCritterFlags2().HasFlag(CritterFlag2.USING_BOOMERANG)
            )
            {
                return false;
            }

            slot.flags &= ~(AnimSlotFlag.UNK10 |
                            AnimSlotFlag.UNK7 |
                            AnimSlotFlag.UNK5 |
                            AnimSlotFlag.UNK4 |
                            AnimSlotFlag.UNK3);
            slot.animPath.maxPathLength = 0;
            return true;
        }


        [TempleDllLocation(0x10010160)]
        public static bool GoalCritterShouldNotAutoAnimate(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            AssertAnimParam(sourceObj != null); // sourceObj != OBJ_HANDLE_NULL

            return sourceObj.GetCritterFlags2().HasFlag(CritterFlag2.AUTO_ANIMATES)
                   && GameSystems.Party.DistanceToParty(sourceObj) < 60;
        }


        [TempleDllLocation(0x100101d0)]
        public static bool GoalAttackerHasRangedWeapon(AnimSlot slot)
        {
            var attacker = slot.param1.obj;
            AssertAnimParam(attacker != null); /*attacker != OBJ_HANDLE_NULL*/
            var mainWeapon = GameSystems.Combat.GetMainHandWeapon(attacker);
            if (mainWeapon != null && mainWeapon.type == ObjectType.weapon)
            {
                return mainWeapon.GetItemWearFlags().HasFlag(WeaponFlag.RANGED_WEAPON);
            }

            return false;
        }


        [TempleDllLocation(0x10010250)]
        public static bool GoalReturnTrue(AnimSlot slot)
        {
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            return true;
        }


        [TempleDllLocation(0x10010410)]
        public static bool GoalCastConjureEnd(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Anim_CastConjureEnd);

            if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap() || obj == null)
            {
                return false;
            }

            slot.pCurrentGoal.flagsData.number &= ~0x40;
            return (slot.pCurrentGoal.flagsData.number & 0x80) == 0;
        }


        [TempleDllLocation(0x100104a0)]
        public static bool GoalDestroyParam1(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL
            if (obj != null)
            {
                if (!obj.GetFlags().HasFlag(ObjectFlag.DESTROYED))
                {
                    GameSystems.Object.Destroy(obj);
                }

                return true;
            }

            return false;
        }


        [TempleDllLocation(0x10010500)]
        public static bool GoalWasInterrupted(AnimSlot slot)
        {
            return GameSystems.D20.Actions.WasInterrupted(slot.animObj);
        }


        [TempleDllLocation(0x100105f0)]
        public static bool GoalStartConjurationAnim(AnimSlot slot)
        {
            var v2 = slot.param1.obj;
            AssertAnimParam(v2 != null); /*obj != OBJ_HANDLE_NULL*/
            var v4 = v2.GetOrCreateAnimHandle();
            AssertAnimParam(v4 != null); /*handle != AAS_HANDLE_NULL*/

            if (v4.GetAnimId().ToCastingAnim(out var castingAnimId))
            {
                v4.SetAnimId(castingAnimId);
            }

            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            slot.flags = (slot.flags & ~(AnimSlotFlag.UNK3 | AnimSlotFlag.UNK4)) | AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x10010760)]
        public static bool GoalAreOnSameTile(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;

            AssertAnimParam(sourceObj != null); // sourceObj != OBJ_HANDLE_NULL

            if (targetObj != null)
            {
                return sourceObj.GetLocation() == targetObj.GetLocation();
            }

            return true;
        }


        [TempleDllLocation(0x10010aa0)]
        public static bool GoalActionPerform(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null);
            AssertAnimParam(targetObj != null);
            if (sourceObj != null && targetObj != null)
            {
                if (targetObj.IsOffOrDestroyed)
                {
                    return false;
                }

                GameSystems.D20.Actions.ActionFrameProcess(sourceObj);
            }

            return true;
        }


        [TempleDllLocation(0x10010b50)]
        public static bool GoalCheckSlotFlag40000(AnimSlot slot)
        {
            return slot.flags.HasFlag(AnimSlotFlag.UNK12);
        }

        [TempleDllLocation(0x10010b70)]
        public static bool GoalCheckParam2AgainstStateFlagData(AnimSlot slot)
        {
            return slot.param2.number == slot.stateFlagData;
        }

        [TempleDllLocation(0x10010b90)]
        public static bool GoalPickLock(AnimSlot slot)
        {
            var source = slot.param1.obj;
            var target = slot.param2.obj;
            AssertAnimParam(source != null); /*source != OBJ_HANDLE_NULL*/
            AssertAnimParam(target != null); /*target != OBJ_HANDLE_NULL*/

            int lockDc;
            if (target.type == ObjectType.container)
            {
                lockDc = target.GetInt32(obj_f.container_lock_dc);
            }
            else if (target.type == ObjectType.portal)
            {
                lockDc = target.GetInt32(obj_f.portal_lock_dc);
            }
            else
            {
                return false;
            }

            if (GameSystems.Script.ExecuteObjectScript(source, target, source, ObjScriptEvent.Unlock, 0) != 0)
            {
                int flags;
                if (GameSystems.Combat.IsCombatActive())
                {
                    flags = 1;
                }
                else
                {
                    flags = 0x2000;
                }

                string line;
                if (GameSystems.Skill.SkillRoll(source, SkillId.open_lock, lockDc, out _, flags))
                {
                    line = GameSystems.Skill.GetSkillUiMessage(550);
                    GameSystems.MapObject.SetLocked(target, false);
                }
                else
                {
                    line = GameSystems.Skill.GetSkillUiMessage(551);
                }

                GameSystems.TextFloater.FloatLine(target, TextFloaterCategory.Generic, TextFloaterColor.Blue, line);
            }

            return false;
        }


        [TempleDllLocation(0x10010cd0)]
        public static bool GoalAttemptTrapDisarm(AnimSlot slot)
        {
            var source = slot.param1.obj;
            var target = slot.param2.obj;
            AssertAnimParam(source != null); /*source != OBJ_HANDLE_NULL*/
            AssertAnimParam(target != null); /*target != OBJ_HANDLE_NULL*/
            GameSystems.Trap.AttemptDisarm(source, target, out _);
            return false;
        }


        [TempleDllLocation(0x10010e00)]
        public static bool GoalHasReachWithMainWeapon(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            AssertAnimParam(sourceObj != null); // sourceObj != OBJ_HANDLE_NULL
            if (sourceObj.IsOffOrDestroyed)
            {
                return false;
            }

            var mainWeapon = GameSystems.Combat.GetMainHandWeapon(sourceObj);
            return GameSystems.Item.GetReachWithWeapon(mainWeapon, sourceObj) > 1;
        }

        [TempleDllLocation(0x10010f70)]
        public static bool GoalThrowItem(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var itemObj = slot.param2.obj;
            var targetLocation = slot.pCurrentGoal.target.location.location;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(itemObj != null); /*itemObj != OBJ_HANDLE_NULL*/
            if (sourceObj == null)
            {
                return false;
            }

            if (targetLocation == locXY.Zero)
            {
                return false;
            }

            if (itemObj.IsOffOrDestroyed)
            {
                return false;
            }

            GameSystems.Combat.ThrowItem(sourceObj, itemObj, targetLocation);
            slot.pCurrentGoal.scratch.obj = null;
            return true;
        }

        private static bool IsStonedStunnedOrParalyzed(GameObjectBody obj)
        {
            if (obj.GetSpellFlags().HasFlag(SpellFlag.STONED))
            {
                return true;
            }

            return obj.IsCritter() && (obj.GetCritterFlags() & (CritterFlag.PARALYZED | CritterFlag.STUNNED)) != 0;
        }


        [TempleDllLocation(0x100110a0)]
        public static bool GoalNotPreventedFromTalking(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/

            if (sourceObj == null || targetObj == null)
            {
                return false;
            }

            if (targetObj.IsOffOrDestroyed)
            {
                return false;
            }

            if (IsStonedStunnedOrParalyzed(sourceObj))
            {
                return false;
            }

            if (IsStonedStunnedOrParalyzed(targetObj))
            {
                return false;
            }

            return targetObj.IsCritter();
        }


        [TempleDllLocation(0x100111e0)]
        public static bool GoalIsWithinTalkingDistance(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/

            if (sourceObj == null || targetObj == null || targetObj.IsOffOrDestroyed)
            {
                return false;
            }

            var distance = sourceObj.GetLocation().EstimateDistance(targetObj.GetLocation());
            return distance <= GameSystems.AI.GetTalkingDistance(sourceObj);
        }

        [TempleDllLocation(0x100112d0)]
        public static bool GoalInitiateDialog(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null);
            AssertAnimParam(targetObj != null);
            if (sourceObj != null && targetObj != null && !targetObj.IsOffOrDestroyed)
            {
                GameUiBridge.InitiateDialog(sourceObj, targetObj);
            }

            return false;
        }


        [TempleDllLocation(0x10011370)]
        public static bool GoalOpenDoorCleanup(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null); /*door != OBJ_HANDLE_NULL*/

            if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap() || door.IsPortalOpen())
            {
                return false;
            }

            if (door.IsUndetectedSecretDoor() || door.GetPortalFlags().HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return false;
            }

            door.SetPortalFlags(door.GetPortalFlags() | PortalFlag.OPEN);
            GameSystems.MapFogging.SetMapDoFoggingUpdate();
            return true;
        }


        [TempleDllLocation(0x10011420)]
        public static bool GoalCloseDoorCleanup(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null);

            if (slot.flags.HasFlag(AnimSlotFlag.UNK11)
                || GameSystems.Map.IsClearingMap()
                || !door.IsPortalOpen()
                || door.IsUndetectedSecretDoor()
                || door.GetPortalFlags().HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return false;
            }

            door.SetPortalFlags(door.GetPortalFlags() & ~PortalFlag.OPEN);
            GameSystems.MapFogging.SetMapDoFoggingUpdate();
            return true;
        }


        [TempleDllLocation(0x100114d0)]
        public static bool GoalIsDoorSticky(AnimSlot slot)
        {
            var doorObj = slot.param1.obj;
            AssertAnimParam(doorObj != null);
            return doorObj.GetPortalFlags().HasFlag(PortalFlag.NOT_STICKY);
        }

        [TempleDllLocation(0x10011530)]
        public static bool GoalIsLiveCritterNear(AnimSlot slot)
        {
            var doorObj = slot.param1.obj;
            AssertAnimParam(doorObj != null);
            if (GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            var doorLoc = doorObj.GetLocation();
            using var critterList = ObjList.ListTile(doorLoc, ObjectListFilter.OLC_CRITTERS);
            foreach (var critter in critterList)
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(critter))
                {
                    return false;
                }
            }

            return true;
        }


        [TempleDllLocation(0x10011600)]
        public static bool GoalSetRunningFlag(AnimSlot slot)
        {
            AssertAnimParam(slot.animObj != null);
            slot.flags |= AnimSlotFlag.RUNNING;
            return true;
        }


        [TempleDllLocation(0x10011660)]
        public static bool GoalEnterCombat(AnimSlot slot)
        {
            var attacker = slot.param1.obj;
            var victim = slot.param2.obj;
            AssertAnimParam(attacker != null);

            GameSystems.Combat.EnterCombat(attacker);
            if (GameSystems.Party.IsInParty(victim))
            {
                GameSystems.Combat.EnterCombat(victim);
            }
            else
            {
                var leader = GameSystems.Critter.GetLeaderRecursive(victim);
                if (leader != null)
                {
                    if (!GameSystems.Critter.IsDeadNullDestroyed(leader))
                    {
                        GameSystems.Combat.EnterCombat(leader);
                    }
                }
            }

            if (!GameSystems.Critter.IsCombatModeActive(attacker))
            {
                return false;
            }

            var v5 = slot.pCurrentGoal.flagsData.number;
            bool leftHandedAttack = (v5 & 0x10000) != 0;
            bool criticalHit = (v5 & 0x8000) != 0;

            WeaponAnim animType;
            if (criticalHit)
            {
                if (leftHandedAttack)
                {
                    animType = WeaponAnim.LeftCriticalSwing;
                }
                else
                {
                    animType = WeaponAnim.RightCriticalSwing;
                }
            }
            else
            {
                if (GameSystems.Random.GetInt(1, 100) <= 33)
                {
                    animType = leftHandedAttack ? WeaponAnim.LeftAttack3 : WeaponAnim.RightAttack3;
                }
                else
                {
                    if (GameSystems.Random.GetInt(1, 100) <= 50)
                    {
                        animType = leftHandedAttack ? WeaponAnim.LeftAttack2 : WeaponAnim.RightAttack2;
                    }
                    else
                    {
                        animType = leftHandedAttack ? WeaponAnim.LeftAttack : WeaponAnim.RightAttack;
                    }
                }
            }

            attacker.GetOrCreateAnimHandle();
            slot.pCurrentGoal.animIdPrevious.number = (int) animType;
            attacker.SetAnimId(GameSystems.Critter.GetAnimId(attacker, animType));
            return true;
        }


        [TempleDllLocation(0x100117f0)]
        public static bool GoalLeaveCombat(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var anim = (NormalAnimType) slot.pCurrentGoal.range.number;
            if (Globals.Config.ViolenceFilter)
            {
                anim = NormalAnimType.Death;
            }

            GameSystems.Combat.CritterLeaveCombat(obj);
            obj.SetAnimId(new EncodedAnimId(anim));
            slot.pCurrentGoal.animIdPrevious.number = (int) anim;
            return true;
        }


        [TempleDllLocation(0x10011a30)]
        public static bool GoalPlayDodgeAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var attacker = slot.param2.obj;

            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/

            var rotationTo = obj.RotationTo(attacker) - obj.Rotation;

            while (rotationTo > MathF.PI * 2)
                rotationTo -= MathF.PI * 2;
            while (rotationTo < 0.0)
                rotationTo += MathF.PI * 2;

            var angle = Angles.ToDegrees(rotationTo) - 45.0f;

            var weaponAnim = WeaponAnim.FrontDodge;
            if (angle < 45.0f)
            {
                weaponAnim = WeaponAnim.FrontDodge;
            }
            else if (angle < 135.0f)
            {
                weaponAnim = WeaponAnim.RightDodge;
            }
            else if (angle < 225.0f)
            {
                weaponAnim = WeaponAnim.BackDodge;
            }
            else if (angle < 315.0f)
            {
                weaponAnim = WeaponAnim.LeftDodge;
            }

            obj.SetAnimId(GameSystems.Critter.GetAnimId(obj, weaponAnim));
            return true;
        }


        [TempleDllLocation(0x10011be0)]
        public static bool GoalPlayAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var packedAnimId = slot.stateFlagData;
            AssertAnimParam(packedAnimId != -1); /*info.params[ 2 ].dataVal != -1*/

            EncodedAnimId animId;
            if (packedAnimId >= 64)
            {
                animId = new EncodedAnimId((NormalAnimType) (packedAnimId - 64));
            }
            else
            {
                animId = GameSystems.Critter.GetAnimId(obj, (WeaponAnim) packedAnimId);
            }

            obj.SetAnimId(animId);

            if (packedAnimId == (int) WeaponAnim.Idle)
            {
                slot.flags &= AnimSlotFlag.RUNNING;
            }
            else
            {
                slot.flags &= ~(AnimSlotFlag.UNK3 | AnimSlotFlag.UNK4);
                slot.flags |= AnimSlotFlag.UNK5;
            }

            return true;
        }


        [TempleDllLocation(0x10011cf0)]
        public static bool GoalSaveParam1InScratch(AnimSlot slot)
        {
            slot.pCurrentGoal.scratch.obj = slot.param1.obj;
            return true;
        }


        [TempleDllLocation(0x10011d20)]
        public static bool GoalSaveStateDataInSkillData(AnimSlot slot)
        {
            slot.pCurrentGoal.skillData.number = slot.stateFlagData;
            return true;
        }


        [TempleDllLocation(0x10011d40)]
        public static bool GoalSaveStateDataOrSpellRangeInRadius(AnimSlot slot)
        {
            var radius = slot.stateFlagData;
            if (radius != -1)
            {
                slot.pCurrentGoal.animId.number =
                    radius; // TODO: Naming just seems wrong (used in lieu of range/radius)
                return true;
            }
            else
            {
                var pSpellPacket = GameSystems.Spell.GetActiveSpell(slot.param1.number);
                if (pSpellPacket.animFlags.HasFlag(SpellAnimationFlag.SAF_UNK8))
                {
                    slot.pCurrentGoal.animId.number = (int) pSpellPacket.spellRange;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        [TempleDllLocation(0x10011dc0)]
        public static bool GoalSetTargetLocFromObj(AnimSlot slot)
        {
            var targetObj = slot.param1.obj;
            AssertAnimParam(targetObj != null); // targetObj != OBJ_HANDLE_NULL

            var targetLoc = targetObj.GetLocationFull();
            if (slot.pCurrentGoal.targetTile.location.location == targetLoc.location)
            {
                return false;
            }

            slot.pCurrentGoal.targetTile.location = targetLoc;

            slot.animPath.flags |= AnimPathFlag.UNK_1;
            return true;
        }


        [TempleDllLocation(0x10011e70)]
        public static bool GoalSetRadiusTo4(AnimSlot slot)
        {
            slot.pCurrentGoal.animId.number = 4; // TODO: This just seems wrong (see other uses of 'animId')
            return true;
        }


        [TempleDllLocation(0x10011e90)]
        public static bool GoalSetRadiusToAiSpread(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL

            if (obj != null)
            {
                var spreadOut = false;
                if (obj.IsNPC())
                {
                    spreadOut = obj.GetNPCFlags().HasFlag(NpcFlag.AI_SPREAD_OUT);
                }

                slot.pCurrentGoal.animId.number = spreadOut ? 5 : 2;
                return true;
            }

            return false;
        }


        [TempleDllLocation(0x10011f20)]
        public static bool GoalIsCloserThanDesiredSpread(AnimSlot slot)
        {
            var targetObj = slot.param2.obj;
            var sourceObj = slot.param1.obj;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            if (sourceObj == null || targetObj == null)
            {
                return false;
            }

            var desiredSpread = 1;
            if (sourceObj.IsNPC() && sourceObj.GetNPCFlags().HasFlag(NpcFlag.AI_SPREAD_OUT))
            {
                desiredSpread = 3;
            }

            var distance = sourceObj.GetLocation().EstimateDistance(targetObj.GetLocation());
            return distance < desiredSpread;
        }

        [TempleDllLocation(0x10012040)]
        public static bool GoalTurnTowardsOrAway(AnimSlot slot)
        {
            var target = slot.param2.obj;
            var source = slot.param1.obj;
            AssertAnimParam(source != null); /*source != OBJ_HANDLE_NULL*/
            AssertAnimParam(target != null); /*target != OBJ_HANDLE_NULL*/
            if (IsStonedStunnedOrParalyzed(source))
            {
                return false;
            }

            // Face away from it
            var rotation = source.RotationTo(target) + Angles.ToRadians(180);
            GameSystems.MapObject.SetRotation(source, rotation);
            return true;
        }

        [TempleDllLocation(0x100121b0)]
        public static bool GoalPlayRotationAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var slota = slot.param2.floatNum;
            obj.GetOrCreateAnimHandle();
            var remainingRotation = slota - obj.Rotation;
            if (remainingRotation < 0)
            {
                remainingRotation += MathF.PI * 2;
            }

            EncodedAnimId animId;
            if (remainingRotation <= MathF.PI)
            {
                animId = GameSystems.Critter.GetAnimId(obj, WeaponAnim.RightTurn);
            }
            else
            {
                animId = GameSystems.Critter.GetAnimId(obj, WeaponAnim.LeftTurn);
            }

            obj.SetAnimId(animId);

            slot.path.someDelay = 16;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x100122a0)]
        public static bool GoalRotate(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var targetRotation = slot.param2.floatNum;
            var v3 = obj.GetOrCreateAnimHandle();
            var a1 = v3;
            AssertAnimParam(v3 != null); /*handle != AAS_HANDLE_NULL*/

            if (IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            var elapsedTime = GameSystems.TimeEvent.AnimTime - slot.gametimeSth;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            if (elapsedTime.TotalMilliseconds < 1)
            {
                return true;
            }

            var rotationPerSecond = a1.GetRotationPerSec();
            if (rotationPerSecond == 0.0f)
            {
                rotationPerSecond = 12.566371f;
            }

            var elapsedSeconds = (float) elapsedTime.TotalSeconds;
            var rotationThisStep = elapsedSeconds * MathF.Abs(rotationPerSecond);
            var currentRotation = obj.Rotation;
            var remainingRotation = targetRotation - currentRotation;
            if (remainingRotation < 0.0)
            {
                remainingRotation += MathF.PI * 2;
            }

            if (rotationThisStep >= remainingRotation || rotationThisStep >= MathF.PI * 2 - remainingRotation)
            {
                GameSystems.MapObject.SetRotation(obj, targetRotation);
                return false;
            }

            if (remainingRotation > MathF.PI)
            {
                currentRotation -= rotationThisStep;
            }
            else
            {
                currentRotation += rotationThisStep;
            }

            GameSystems.MapObject.SetRotation(obj, currentRotation);

            var animParams = obj.GetAnimParams();
            a1.Advance(elapsedSeconds, 0.0f, rotationThisStep, animParams);

            if (obj.IsCritter())
            {
                var mainHandWeapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
                if (mainHandWeapon != null)
                {
                    var weaponAnimParams = mainHandWeapon.GetAnimParams();
                    var weaponAnim = mainHandWeapon.GetOrCreateAnimHandle();
                    weaponAnim?.Advance(elapsedSeconds, 0.0f, 0.0f, weaponAnimParams);
                }

                var offHandWeapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);
                if (offHandWeapon == null)
                {
                    offHandWeapon = GameSystems.Item.ItemWornAt(obj, EquipSlot.Shield);
                }

                if (offHandWeapon != null)
                {
                    var weaponAnimParams = offHandWeapon.GetAnimParams();
                    var weaponAnim = offHandWeapon.GetOrCreateAnimHandle();
                    weaponAnim?.Advance(elapsedSeconds, 0.0f, 0.0f, weaponAnimParams);
                }
            }

            return true;
        }


        [TempleDllLocation(0x100127b0)]
        public static bool GoalIsRotatedTowardTarget(AnimSlot slot)
        {
            if (slot.pCurrentGoal == null)
            {
                slot.pCurrentGoal = slot.goals[slot.currentGoal];
            }

            var src = slot.param1.obj;
            var target = slot.param2.obj;
            var srcLoc = src.GetLocationFull().ToInches2D();
            var tgtLoc = target.GetLocationFull().ToInches2D();

            if (Math.Abs(srcLoc.X - tgtLoc.X) < 0.001 && Math.Abs(srcLoc.Y - tgtLoc.Y) < 0.001)
            {
                return true;
            }

            var targetRotation = src.RotationTo(target);
            slot.pCurrentGoal.scratchVal1.floatNum = targetRotation;
            var delta = Angles.ToDegrees(MathF.Abs(targetRotation - src.Rotation));
            return delta <= 1;
        }


        [TempleDllLocation(0x10012910)]
        public static bool GoalSetRotationToParam2(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/

            var rotation = slot.param2.floatNum;
            if (obj == null || IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            GameSystems.MapObject.SetRotation(obj, rotation);
            return true;
        }


        [TempleDllLocation(0x10012a00)]
        public static bool GoalSetRotationToFaceTargetObj(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(sourceObj != null);
            if (targetObj != null && sourceObj != targetObj)
            {
                if (IsStonedStunnedOrParalyzed(sourceObj))
                {
                    return false;
                }

                GameSystems.MapObject.SetRotation(sourceObj, sourceObj.RotationTo(targetObj));
            }

            return true;
        }


        [TempleDllLocation(0x10012b60)]
        public static bool GoalSetRotationToFaceTargetLoc(AnimSlot slot)
        {

            var source = slot.param1.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(source != null); /*source != OBJ_HANDLE_NULL*/
            var pOut = source.GetLocationFull();
            if (pOut.location.locx != targetLoc.location.locx || pOut.location.locy != targetLoc.location.locy)
            {
                var rotation = source.GetLocationFull().RotationTo(targetLoc);
                GameSystems.MapObject.SetRotation(source, rotation);
            }

            return true;
        }


        [TempleDllLocation(0x10012ca0)]
        public static bool GoalProjectileCleanup(AnimSlot slot)
        {
            var currentGoal = slot.pCurrentGoal;
            if (currentGoal != null)
            {
                var projectile = currentGoal.self.obj;
                var parent = currentGoal.parent.obj;
                if (projectile != null)
                {
                    if (!projectile.GetFlags().HasFlag(ObjectFlag.DESTROYED))
                    {
                        GameSystems.Combat.ProjectileCleanup2(projectile, parent);
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10012cf0)]
        public static bool GoalAnimateCleanup(AnimSlot slot)
        {
            GameSystems.AI.ClearWaypointDelay(slot.param1.obj);
            return false;
        }

        [TempleDllLocation(0x10012d10)]
        public static bool GoalAnimateForever(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null); /*handle != AAS_HANDLE_NULL*/
            EncodedAnimId animId;
            if (slot.param2.number == -1)
            {
                animId = animHandle.GetAnimId();
            }
            else
            {
                animId = new EncodedAnimId(slot.param2.number);
            }

            if (IsStonedStunnedOrParalyzed(obj))
            {
                if (animId.IsSpecialAnim()
                    || animId.GetNormalAnimType() == NormalAnimType.Death
                    && animId.GetNormalAnimType() == NormalAnimType.Death2
                    && animId.GetNormalAnimType() == NormalAnimType.Death3)
                {
                    return false;
                }
            }
            obj.SetAnimId(animId);

            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            AssertAnimParam(slot.pCurrentGoal != null); /*info.pCurrentGoal != NULL*/
            if (GameSystems.Party.DistanceToParty(obj) < 30.0f)
            {
                slot.pCurrentGoal.scratchVal4.number = 1;
            }
            else
            {
                slot.pCurrentGoal.scratchVal4.number = 0;
            }

            slot.flags |= AnimSlotFlag.UNK5;

            if (slot.flags.HasFlag(AnimSlotFlag.UNK6))
            {
                return true;
            }

            var soundId = GameSystems.SoundMap.GetAnimateForeverSoundEffect(obj, 2);
            if (soundId == -1)
            {
                return true;
            }

            var soundMaxRange = GameSystems.SoundGame.GetSoundOutOfRangeRange(obj);
            var streamId = slot.goals[0].soundHandle.number;
            if (GameSystems.Party.DistanceToParty(obj) > soundMaxRange)
            {
                // Sound is out of range of party
                if (streamId != -1)
                {
                    Tig.Sound.FreeStream(streamId);
                    slot.pCurrentGoal.soundHandle.number = -1;
                }

                return true;
            }

            if (streamId != -1)
            {
                // Sound already playing and in range
                return true;
            }

            streamId = GameSystems.SoundGame.PositionalSound(soundId, 0, obj);
            if (streamId == -1)
            {
                Logger.Warn("Animate Forever: Sound {0} failed to start!", soundId);
            }
            else
            {
                slot.goals[0].soundHandle.number = streamId;
            }

            return true;
        }


        [TempleDllLocation(0x10012fa0)]
        public static bool GoalLoopWhileCloseToParty(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var shoppingMapId = GameSystems.Map.GetMapIdByType(MapType.ShoppingMap);
            if (GameSystems.Map.GetCurrentMapId() == shoppingMapId)
            {
                return true;
            }

            if (obj != null)
            {
                ++slot.pCurrentGoal.scratchVal5.number;
                var currentGoal = slot.pCurrentGoal;
                if (currentGoal.scratchVal5.number < 3)
                {
                    return currentGoal.scratchVal4.number != 0;
                }

                currentGoal.scratchVal5.number = 0;
                if (GameSystems.Party.DistanceToParty(obj) < 30)
                {
                    slot.pCurrentGoal.scratchVal4.number = 1;
                    return true;
                }
            }

            slot.pCurrentGoal.scratchVal4.number = 0;
            return false;
        }


        [TempleDllLocation(0x10013080)]
        public static bool GoalFreeSoundHandle(AnimSlot slot)
        {
            if (slot.pCurrentGoal.soundHandle.number != -1 && !slot.flags.HasFlag(AnimSlotFlag.UNK6))
            {
                Tig.Sound.FreeStream(slot.pCurrentGoal.soundHandle.number);
                slot.pCurrentGoal.soundHandle.number = -1;
            }

            return true;
        }


        [TempleDllLocation(0x100130f0)]
        public static bool GoalIsAliveAndConscious(AnimSlot slot)
        {
            var selfObj = slot.param1.obj;
            AssertAnimParam(selfObj != null);
            return !GameSystems.Critter.IsDeadOrUnconscious(selfObj);
        }

        [TempleDllLocation(0x10013250)]
        public static bool GoalBeginMoveStraight(AnimSlot slot)
        {
            var projectile = slot.param1.obj;
            var targetLoc = slot.param2.location;
            AssertAnimParam(projectile != null);
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            Logger.Info("BeginMoveStraight({0}): info.current_ping={1}", projectile, slot.currentPing);

            // Turn the projectile towards the target location
            var rotation = projectile.GetLocationFull().RotationTo(targetLoc);
            GameSystems.MapObject.SetRotation(projectile, rotation);

            // Special handling for magic missile
            if (projectile.ProtoId == 3004)
            {
                var angleInCone = Angles.ToRadians(GameSystems.Random.GetInt(0, 90) - 45);
                var accelX = - MathF.Sin(angleInCone);
                var remainder = MathF.Cos(angleInCone);

                var angleInCone2 = Angles.ToRadians(GameSystems.Random.GetInt(0, 90) - 45);
                angleInCone2 += Angles.ToRadians(90);
                var accelY = MathF.Cos(angleInCone2) * remainder;
                var accelZ = MathF.Sin(angleInCone2) * remainder;

                projectile.SetFloat(obj_f.projectile_acceleration_x, accelX);
                projectile.SetFloat(obj_f.projectile_acceleration_y, accelY);
                projectile.SetFloat(obj_f.projectile_acceleration_z, accelZ);
            }

            slot.pCurrentGoal.scratchVal3.number = 0;
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x10013af0)]
        public static bool GoalUpdateMoveStraight(AnimSlot slot)
        {
            var projectile = slot.param1.obj;
            var targetObj = slot.param2.obj;
            AssertAnimParam(projectile != null); /*projectile != OBJ_HANDLE_NULL*/

            // Special handling for magic missile
            if (projectile.ProtoId == 3004)
            {
                return UpdateMagicMissile(slot);
            }

            var animModel = projectile.GetOrCreateAnimHandle();
            var pOut = projectile.GetLocationFull();

            LocAndOffsets targetLoc;
            if (targetObj != null)
            {
                targetLoc = targetObj.GetLocationFull();
            } else {
                targetLoc = slot.pCurrentGoal.targetTile.location;
            }

            var targetLoc2d = targetLoc.ToInches2D();
            var projectileLoc2d = pOut.ToInches2D();

            var rotation = pOut.RotationTo(targetLoc);
            GameSystems.MapObject.SetRotation(projectile, rotation);
            var dps = projectile.GetFloat(obj_f.speed_run);
            GameSystems.Script.SetAnimObject(projectile);
            var elapsedTime = GameSystems.TimeEvent.AnimTime - slot.gametimeSth;
            var dt = (float) elapsedTime.TotalSeconds;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            var dist = dt * dps;
            Logger.Info(
                "UpdateMoveStraight({0}): info.current_ping={1}, dps={2}, dt={3}, dist={4}",
                projectile,
                slot.currentPing,
                dps,
                dt,
                dist);

            var remainingDistance = Vector2.Distance(projectileLoc2d, targetLoc2d);
            if (dist > remainingDistance )
            {
                dt *= remainingDistance / dist;
                dist = remainingDistance;
            }

            // Move the projectile along it's path for as much as we can given the elapsed time / remaining distance
            projectileLoc2d += ((targetLoc2d - projectileLoc2d) / remainingDistance) * dist;

            var traveledDistance = slot.pCurrentGoal.scratchVal3.floatNum;
            slot.pCurrentGoal.scratchVal3.floatNum += dist;

            var animParams = projectile.GetAnimParams();
            animModel.Advance(dt, dist, 0.0f, animParams);

            var parentObj = slot.pCurrentGoal.parent.obj;
            GameSystems.MapObject.Move(projectile, LocAndOffsets.FromInches(projectileLoc2d));

            var fullDistance = remainingDistance + traveledDistance;
            var oscillation = (1.0f - 2 * (traveledDistance / fullDistance)) * fullDistance * dt * 0.5f;
            projectile.SetFloat(obj_f.offset_z, projectile.GetFloat(obj_f.offset_z) + oscillation);

            float rotationPitch;
            if (GameSystems.Item.IsThrownWeaponProjectile(projectile))
            {
                rotationPitch = traveledDistance * 0.0334212f;
            }
            else
            {
                rotationPitch = -MathF.Atan2(dist, oscillation);
            }
            projectile.SetFloat(obj_f.rotation_pitch, rotationPitch);

            DoProjectileCheck(
                parentObj,
                targetObj,
                targetLoc,
                projectile,
                slot.pCurrentGoal.scratchVal3.floatNum);

            if (Vector2.Distance(projectileLoc2d, targetLoc2d) <= locXY.INCH_PER_TILE / 6.0f)
            {
                GameSystems.D20.Actions.ProjectileHit(projectile, parentObj);
                DestroyProjectile(projectile);
                return false;
            }
            else
            {
                return true;
            }
        }

        [TempleDllLocation(0x10013470)]
        private static bool UpdateMagicMissile(AnimSlot slot)
        {
              var projectile = slot.param1.obj;
              var targetObj = slot.param2.obj;
              AssertAnimParam(projectile != null);
              var animId = projectile.GetOrCreateAnimHandle();
              var critter = slot.pCurrentGoal.parent.obj;
              var projLoc = projectile.GetLocationFull();

              LocAndOffsets targetLoc;
              if (targetObj != null)
              {
                  targetLoc = targetObj.GetLocationFull();
              }
              else
              {
                  targetLoc = slot.pCurrentGoal.targetTile.location;
              }

              var targetLoc2d = targetLoc.ToInches2D();

              var projectileLoc2d = projLoc.ToInches2D();

              var rotation = projLoc.RotationTo(targetLoc);
              GameSystems.MapObject.SetRotation(projectile, rotation);

              var speed = projectile.GetFloat(obj_f.speed_run);
              GameSystems.Script.SetAnimObject(projectile);
              slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
              var deltaTimeInSecs = (float) (GameSystems.TimeEvent.AnimTime - slot.gametimeSth).TotalSeconds;
              var deltaDistance = deltaTimeInSecs * speed;
              var remainingDistance = Vector2.Distance(projectileLoc2d, targetLoc2d);
              if ( deltaDistance > remainingDistance )
                deltaDistance = remainingDistance;
              var accel = new Vector3(
                  projectile.GetFloat(obj_f.projectile_acceleration_x),
                    projectile.GetFloat(obj_f.projectile_acceleration_y),
                    projectile.GetFloat(obj_f.projectile_acceleration_z)
                  );

              var newLoc3d = new Vector3(projectileLoc2d, projectile.OffsetZ);
              newLoc3d += accel * speed * deltaTimeInSecs;
              var targetLoc3d = new Vector3(targetLoc2d, projectile.OffsetZ);

              var dirVec = targetLoc3d - newLoc3d;
              var movementNormal = Vector3.Normalize(dirVec);

              var maxDistFac = slot.pCurrentGoal.scratchVal3.floatNum / (critter.DistanceToObjInFeet(targetObj) * 120.0f);
              if (maxDistFac >= 1.0f)
              {
                  maxDistFac = 1.0f;
              }

              var inertiaVec = movementNormal - accel;
              var movementXYPlane = new Vector2(inertiaVec.X, inertiaVec.Y).Length();
              if ( movementXYPlane < 0.15f && maxDistFac < 0.2f )
              {
                  inertiaVec *= 10.0f;
              }
              else
              {
                  inertiaVec *= maxDistFac;
              }

              inertiaVec += accel;
              inertiaVec = Vector3.Normalize(inertiaVec);

              projectile.SetFloat(obj_f.projectile_acceleration_x, inertiaVec.X);
              projectile.SetFloat(obj_f.projectile_acceleration_y, inertiaVec.Y);
              projectile.SetFloat(obj_f.projectile_acceleration_z, inertiaVec.Z);

              var animParams = projectile.GetAnimParams();
              animId.Advance(deltaTimeInSecs, deltaDistance, 0.0f, animParams);
              slot.pCurrentGoal.scratchVal3.floatNum += deltaDistance;
              projectile.SetFloat(obj_f.offset_z, newLoc3d.Z);
              GameSystems.MapObject.Move(projectile, LocAndOffsets.FromInches(newLoc3d));

              DoProjectileCheck(
                critter,
                targetObj,
                targetLoc,
                projectile,
                slot.pCurrentGoal.scratchVal3.floatNum
                );

              var distToTarget = projLoc.DistanceTo(targetLoc);
              if ( targetObj != null )
              {
                  var targetRadius = targetObj.GetRadius();
                if ( distToTarget <= targetRadius )
                {
                  GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                  DestroyProjectile(projectile);
                  return false;
                }
              }
              else if ( distToTarget <= locXY.INCH_PER_SUBTILE )
              {
                  GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                  DestroyProjectile(projectile);
                  return false;
              }
              return true;
        }

        [TempleDllLocation(0x100B4E20)]
        private static void DoProjectileCheck(GameObjectBody critter, GameObjectBody target, LocAndOffsets targetLoc,
            GameObjectBody projectile, float traveledDistance)
        {
            if ( projectile.GetLocation() == targetLoc.location && target == null )
            {
                GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                DestroyProjectile(projectile);
                return;
            }

            float distanceToTargetFt;
            if ( target != null )
            {
                distanceToTargetFt = critter.DistanceToObjInFeet(target);
                if (distanceToTargetFt < 0.0f)
                {
                    distanceToTargetFt = 0.0f;
                }
            }
            else
            {
                distanceToTargetFt = critter.DistanceToLocInFeet(targetLoc);
            }

            var traveledDistanceFt = traveledDistance / 12;
            if ( traveledDistanceFt > 25.0 * distanceToTargetFt )
            {
                GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                DestroyProjectile(projectile);
            }
        }

        [TempleDllLocation(0x100b4df0)]
        private static void DestroyProjectile(GameObjectBody projectile)
        {
            var partSysId = projectile.GetInt32(obj_f.projectile_part_sys_id);
            if (partSysId != 0)
            {
                GameSystems.ParticleSys.End(partSysId);
            }
            GameSystems.Object.Destroy(projectile);
        }

        [TempleDllLocation(0x100140c0)]
        public static bool GoalSetNoBlockIfNotInParty(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj == null)
            {
                return false;
            }

            if (!slot.flags.HasFlag(AnimSlotFlag.UNK_200) && GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                if (!GameSystems.Party.IsInParty(obj))
                {
                    GameSystems.MapObject.SetFlags(obj, ObjectFlag.NO_BLOCK | ObjectFlag.FLAT);
                }
            }

            return true;
        }


        [TempleDllLocation(0x10014170)]
        public static bool GoalDyingCleanup(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var handle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(handle != null);
            if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap())
            {
                return false;
            }

            if (GameSystems.Tile.IsBlockingOldVersion(obj.GetLocation()))
            {
                GameSystems.Object.Destroy(obj);
            }
            return true;
        }

        [TempleDllLocation(0x100147d0)]
        public static bool GoalMoveAlongPath(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/

            var nextPathNodeLoc = slot.path.GetNextNode();
            if (!nextPathNodeLoc.HasValue)
                return false;
            var pathNodeLoc = nextPathNodeLoc.Value;
            var pathNodePos = pathNodeLoc.ToInches2D();

            var objLoc = obj.GetLocationFull();
            var objPos = obj.GetLocationFull().ToInches2D();
            GameSystems.MapObject.SetRotation(obj, objLoc.RotationTo(pathNodeLoc));

            float speed;
            if (slot.flags.HasFlag(AnimSlotFlag.RUNNING))
                speed = obj.GetFloat(obj_f.speed_run);
            else
                speed = obj.GetFloat(obj_f.speed_walk);

            var objAnim = obj.GetOrCreateAnimHandle();
            GameSystems.Script.SetAnimObject(obj);
            var elapsedTime = GameSystems.TimeEvent.AnimTime - slot.gametimeSth;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            if (elapsedTime.TotalMilliseconds < 1.0)
            {
                return true;
            }

            var deltaTime = (float) elapsedTime.TotalSeconds * speed;
            var distanceMoved = deltaTime * objAnim.GetDistPerSec();
            while ((pathNodePos - objPos).Length() < distanceMoved)
            {
                distanceMoved -= (pathNodePos - objPos).Length();
                ++slot.path.currentNode;
                GameSystems.MapObject.Move(obj, pathNodeLoc);

                objLoc = obj.GetLocationFull();
                objPos = objLoc.ToInches2D();

                if (slot.path.currentNode >= slot.path.nodeCount)
                {
                    slot.flags &= ~(AnimSlotFlag.UNK5|AnimSlotFlag.UNK7);
                    slot.path.flags &= PathFlags.PF_COMPLETE;
                    GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
                    return false;
                }


                nextPathNodeLoc = slot.path.GetNextNode();
                if (!nextPathNodeLoc.HasValue)
                {
                    slot.flags &= ~(AnimSlotFlag.UNK5|AnimSlotFlag.UNK7);
                    slot.path.flags &= PathFlags.PF_COMPLETE;
                    GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
                    return false;
                }
                pathNodeLoc = nextPathNodeLoc.Value;
                pathNodePos = pathNodeLoc.ToInches2D();

                GameSystems.MapObject.SetRotation(obj, objLoc.RotationTo(pathNodeLoc));
            }

            var dirVec = Vector2.Normalize(pathNodePos - objPos) * distanceMoved;
            var newObjPos = objPos + dirVec;

            var animParam = obj.GetAnimParams();
            var a4 = MathF.Abs(objAnim.GetRotationPerSec()) * deltaTime;
            objAnim.Advance(deltaTime, distanceMoved, a4, animParam);

            if (obj.IsCritter())
            {
                var mainHandItem = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
                var offHandItem = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);
                if (offHandItem != null)
                {
                    offHandItem = GameSystems.Item.ItemWornAt(obj, EquipSlot.Shield);
                }

                if (mainHandItem != null)
                {
                    var itemParams = mainHandItem.GetAnimParams();
                    var itemModel = mainHandItem.GetOrCreateAnimHandle();
                    itemModel?.Advance(deltaTime, 0.0f, 0.0f, itemParams);
                }

                if (offHandItem != null)
                {
                    var itemParams = mainHandItem.GetAnimParams();
                    var itemModel = mainHandItem.GetOrCreateAnimHandle();
                    itemModel?.Advance(deltaTime, 0.0f, 0.0f, itemParams);
                }
            }

            GameSystems.MapObject.Move(obj, LocAndOffsets.FromInches(newObjPos));
            if ((newObjPos - objPos).Length() >= 0.1)
            {
                return true;
            }

            if (++slot.path.currentNode < slot.path.nodeCount)
            {
                return true;
            }

            slot.flags &= ~(AnimSlotFlag.UNK5|AnimSlotFlag.UNK7);
            slot.path.flags &= PathFlags.PF_COMPLETE;
            GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
            GameSystems.MapObject.Move(obj, pathNodeLoc);
            return false;
        }


        [TempleDllLocation(0x10014f10)]
        public static bool GoalIsNotStackFlagsData20(AnimSlot slot)
        {
            var flagsData = slot.pCurrentGoal.flagsData.number;
            return (flagsData & 0x20) == 0;
        }

        [TempleDllLocation(0x10014f30)]
        public static bool GoalJiggleAlongYAxis(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj == null)
            {
                return false;
            }

            var iteration = slot.pCurrentGoal.scratchVal1.number;
            float shift;
            if (iteration <= 0)
            {
                iteration--;
                shift = -2;
                if (iteration < -5)
                {
                    iteration = 1; // Start moving in the other direction
                }
            }
            else
            {
                iteration++;
                shift = 2;
                if (iteration > 5)
                {
                    iteration = -1; // Start moving in the other direction
                }
            }

            slot.pCurrentGoal.scratchVal1.number = iteration;
            var offsetX = obj.OffsetX;
            var offsetY = obj.OffsetY + shift;
            GameSystems.MapObject.MoveOffsets(obj, offsetX, offsetY);
            return true;
        }


        [TempleDllLocation(0x10014ff0)]
        public static bool GoalStartJigglingAlongYAxis(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj == null)
            {
                return false;
            }

            slot.pCurrentGoal.flagsData.number |= 0x20;
            slot.pCurrentGoal.scratchVal1.number = 1;
            slot.path.someDelay = 100;
            var offsetX = obj.OffsetX;
            var offsetY = obj.OffsetY - 15.0f;
            GameSystems.MapObject.MoveOffsets(obj, offsetX, offsetY);
            return true;
        }


        [TempleDllLocation(0x100150a0)]
        public static bool GoalEndJigglingAlongYAxis(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj == null)
            {
                return false;
            }

            slot.pCurrentGoal.flagsData.number &= ~0x20;
            var iteration = slot.pCurrentGoal.scratchVal1.number;
            var offsetX = obj.OffsetX;
            var offsetY = obj.OffsetY + (15.0f - 2 * iteration);
            GameSystems.MapObject.MoveOffsets(obj, offsetX, offsetY);
            return true;
        }


        [TempleDllLocation(0x10015150)]
        public static bool GoalIsNotStackFlagsData40(AnimSlot slot)
        {
            var flagsData = slot.pCurrentGoal.flagsData.number;
            return (flagsData & 0x40) == 0;
        }

        [TempleDllLocation(0x10015170)]
        public static bool GoalSetSlotFlags4(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj != null)
            {
                slot.flags |= AnimSlotFlag.UNK3;
            }

            return false;
        }


        [TempleDllLocation(0x10015240)]
        public static bool GoalActionPerform3(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj != null)
            {
                GameSystems.D20.Actions.ActionFrameProcess(obj);
                return true;
            }

            return false;
        }


        [TempleDllLocation(0x10017100)]
        public static bool GoalSpawnFireball(AnimSlot slot)
        {
            var location = slot.param2.location;

            var pOut = GameSystems.MapObject.CreateObject(3000, location.location);
            GameSystems.MapObject.SetFlags(pOut, ObjectFlag.DONTLIGHT);
            pOut.ProjectileFlags = 0;
            var partSys = GameSystems.ParticleSys.PlayEffect("sp-fireball-proj", pOut);
            pOut.SetInt32(obj_f.projectile_part_sys_id, (int) partSys); // TODO: This will break hard
            GameSystems.MapObject.SetFlags(pOut, ObjectFlag.DONTDRAW);

            return true;
        }

        [TempleDllLocation(0x10017170)]
        public static bool GoalPleaseMove(AnimSlot slot)
        {

            var v1 = slot;
            var sourceObj = slot.param1.obj;

            AssertAnimParam(sourceObj != null);
            if (slot.param2.location.location != locXY.Zero)
            {
                GameSystems.Anim.customDelayInMs = 0;
                return true;
            }

            var v6 = sourceObj.GetLocation();
            v7 = HIDWORD(v6);
            var v8 = slot.pCurrentGoal;
            var v9 = v6;
            if (v8.target.obj != null)
            {
                v10 = v8.target.obj.GetLocation();
                v30 = HIDWORD(v10);
            }
            else
            {
                LODWORD(v10) = 0;
                v30 = 0;
            }

            v28 = v10;
            var v11 = GameSystems.Random.GetInt(0, 8);
            var v26 = v11;
            var v12 = v11;
            if (v11 < 8)
            {
                do
                {
                    v13 = &a4 + 2 * v12;
                    if (GetLocationOffsetByUnityInDirection((location2d) v9, v12, (location2d*) &a4 + v12))
                    {
                        v14 = *v13;
                        *(&v33 + 2 * v12) = 0;
                        v34[2 * v12] = 0;
                        if (!sub_1000D3A0(sourceObj, v9, v7, v14, v13[1], v12))
                        {
                            v22 = slot.pCurrentGoal;
                            v22.target_tile.xy.X = *(&a4 + 2 * v12);
                            v22.target_tile.xy.Y = v32[2 * v12];
                            LODWORD(slot.pCurrentGoal.target_tile.offsetx) = 0;
                            LODWORD(slot.pCurrentGoal.target_tile.offsety) = 0;
                            return true;
                        }
                    }

                    ++v12;
                } while (v12 < 8);

                v1 = slot;
                v11 = v26;
            }

            if (v11 > 0)
            {
                v15 = 0;
                if (v11 > 0)
                {
                    do
                    {
                        v16 = &a4 + 2 * v15;
                        if (GetLocationOffsetByUnityInDirection((location2d) v9, v15, (location2d*) &a4 + v15))
                        {
                            v17 = v16[1];
                            v18 = *v16;
                            *(&v33 + 2 * v15) = 0;
                            v34[2 * v15] = 0;
                            if (!sub_1000D3A0(sourceObj, v9, v7, v18, v17, v15))
                            {
                                v23 = slot.pCurrentGoal;
                                v23.target_tile.xy.X = *(&a4 + 2 * v15);
                                v23.target_tile.xy.Y = v32[2 * v15];
                                LODWORD(slot.pCurrentGoal.target_tile.offsetx) = 0;
                                LODWORD(slot.pCurrentGoal.target_tile.offsety) = 0;
                                return true;
                            }
                        }

                        v11 = v26;
                        ++v15;
                    } while (v15 < v26);

                    v1 = slot;
                }
            }

            i = v11;
            if (v11 < 8)
            {
                while (1)
                {
                    v20 = v28;
                    if (*(&a4 + 2 * i) != v28 || v32[2 * i] != v30)
                    {
                        HIDWORD(v21.id) = v34[2 * i];
                        LODWORD(v21.id) = *(&v33 + 2 * i);
                        if (anim_push_please_move(sourceObj, v21))
                        {
                            LABEL_37:
                            v25 = v1.pCurrentGoal;
                            v25.target_tile.xy.X = *(&a4 + 2 * i);
                            v25.target_tile.xy.Y = v32[2 * i];
                            LODWORD(v1.pCurrentGoal.target_tile.offsetx) = 0;
                            LODWORD(v1.pCurrentGoal.target_tile.offsety) = 0;
                            GameSystems.Anim.customDelayInMs = 1000;
                            return true;
                        }

                        v11 = v26;
                    }

                    if (++i >= 8)
                        goto LABEL_29;
                }
            }

            v20 = v28;
            LABEL_29:
            if (v11 > 0)
            {
                for (i = 0; i < v11; ++i)
                {
                    if (*(&a4 + 2 * i) != v20 || v32[2 * i] != v30)
                    {
                        HIDWORD(v24.id) = v34[2 * i];
                        LODWORD(v24.id) = *(&v33 + 2 * i);
                        if (anim_push_please_move(sourceObj, v24))
                            goto LABEL_37;
                        v11 = v26;
                    }
                }
            }

            return false;
        }


        [TempleDllLocation(0x10017460)]
        public static bool GoalIsTargetWithinRadius(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;
            var range = slot.pCurrentGoal.animId.number;
            AssertAnimParam(sourceObj != null); /*sourceObj != OBJ_HANDLE_NULL*/
            AssertAnimParam(targetObj != null); /*targetObj != OBJ_HANDLE_NULL*/
            if (sourceObj != null && targetObj != null && sourceObj.DistanceToObjInFeet(targetObj) <= range)
            {
                return !GameSystems.Tile.IsBlockingOldVersion(targetObj.GetLocation());
            }
            else
            {
                return false;
            }
        }


        [TempleDllLocation(0x10017570)]
        public static bool GoalWander(AnimSlot slot)
        {
            AnimSlot* v1;
            int result;
            CHAR* v3;
            long v4;
            AnimSlotGoalStackEntry* v5;
            int v6;
            location2d pString;
            __int64 v8;
            __int64 v9;
            __int64 v10;
            int v11;
            ulong v12;
            int v13;
            int v14;
            int v15;
            int v16;
            int v17;
            int v18;
            AnimSlotGoalStackEntry* v19;
            CHAR* v20;
            ObjHndl ObjHnd;
            int v22;
            anim_path_data a1;
            AnimSlot slota;

            AssertAnimParam(slot.param1.obj != null); // obj != OBJ_HANDLE_NULL

            v1 = slot;
            AssertAnimParam(slot); /*pRunInfo != NULL*/
            ObjHnd.id = slot.param1.obj;
            if (slot.param1.obj)
            {
                v3 = (CHAR*) slot.animPath;
                v4 = obj_get_int64(slot.param1.obj, obj_f.location);
                v5 = slot.pCurrentGoal;
                v6 = LODWORD(v5.radius);
                pString = (location2d) v4;
                v8 = LODWORD(v5.rotation);
                slota = (AnimSlot*) v6;
                v9 = v5.tetherLoc;
                v10 = GameSystems.Random.GetInt(-v6, v6);
                v11 = GameSystems.Random.GetInt(-v6, v6);
                v12 = _longint_mul(v9 + v11, 0x100000000i64);
                v13 = (v8 + v10) | v12;
                v1.field_14 = v1.currentGoalIdx + 1;
                v22 = ((ulong) (v8 + v10) >> 32) | HIDWORD(v12);
                a1.movingObj = ObjHnd;
                a1.loc1 = pString;
                a1.objLoc.X = v13;
                a1.objLoc.Y = v22;
                a1.maxPathLength ? = anim_create_path_max_length(ref v1.animPath, pString.X, pString.Y, v13, v22,
                    ObjHnd);
                a1.field_1C = (int) &v1.animPath.deltas;
                a1.pathQueryFlags = 0;
                if (AnimPathSpecInit((AnimPathData*) &a1))
                    v1.animPath.deltaIdxMax = AnimPathSearch((AnimPathData*) &a1);
                else
                    v1.animPath.deltaIdxMax = 0;
                v14 = v1.animPath.deltaIdxMax;
                if (v14 && v14 <= (int) slota
                    || (a1.pathQueryFlags = 1, AnimPathSpecInit((AnimPathData*) &a1))
                    && (v15 = AnimPathSearch((AnimPathData*) &a1), (v1.animPath.deltaIdxMax = v15) != 0)
                    && v15 <= (int) slota)
                {
                    v16 = a1.loc1.X;
                    *(_DWORD*) v3 &= 0xFFFFFFFC;
                    v1.animPath.objLoc.Y = a1.loc1.Y;
                    v17 = a1.objLoc.Y;
                    v1.animPath.objLoc.X = v16;
                    v18 = a1.objLoc.X;
                    v1.animPath.loc.Y = v17;
                    v1.animPath.loc.X = v18;
                    v1.animPath.fieldD4 = 0;
                    v19 = v1.pCurrentGoal;
                    v19.target_tile.xy.X = v13;
                    v19.target_tile.xy.Y = v22;
                    LODWORD(v1.pCurrentGoal.target_tile.offsetx) = 0;
                    LODWORD(v1.pCurrentGoal.target_tile.offsety) = 0;
                    result = 1;
                }
                else
                {
                    AiEndCombatTurn(ObjHnd);
                    result = 0;
                }
            }

            return result;
        }


        [TempleDllLocation(0x10017810)]
        public static bool GoalWanderSeekDarkness(AnimSlot slot)
        {
            anim_path* v2;
            long v3;
            AnimSlotGoalStackEntry* v4;
            int v5;
            __int64 v6;
            __int64 v7;
            __int64 v8;
            int v9;
            ulong v10;
            int v11;
            int v12;
            unsigned __int8 v13;
            int v14;
            int v15;
            int v16;
            int v17;
            int v18;
            AnimSlotGoalStackEntry* v19;
            CHAR* v20;
            int v21;
            ObjHndl ObjHnd;
            location2d pString;
            anim_path_data a1;

            AssertAnimParam(slot.param1.obj != null); // obj != OBJ_HANDLE_NULL

            AssertAnimParam(slot); /*pRunInfo != NULL*/
            ObjHnd.id = slot.param1.obj;
            v3 = obj_get_int64(slot.param1.obj, obj_f.location);
            v4 = slot.pCurrentGoal;
            v5 = LODWORD(v4.radius);
            pString = (location2d) v3;
            v6 = LODWORD(v4.rotation);
            v21 = v5;
            v7 = v4.tetherLoc;
            v8 = GameSystems.Random.GetInt(-v5, v5);
            v9 = GameSystems.Random.GetInt(-v5, v5);
            v10 = _longint_mul(v7 + v9, 0x100000000i64);
            v11 = ((ulong) (v6 + v8) >> 32) | HIDWORD(v10);
            v12 = (v6 + v8) | v10;
            v13 = sub_100A62B0();
            if ((unsigned __int8)sub_100A62B0() <= v13 )
            {
                slot.field_14 = slot.currentGoal + 1;
                a1.movingObj = ObjHnd;
                a1.loc1 = pString;
                a1.objLoc.X = v12;
                a1.objLoc.Y = v11;
                a1.maxPathLength ? = anim_create_path_max_length(ref slot.animPath, pString.X, pString.Y, v12, v11,
                    ObjHnd);
                a1.field_1C = (int) &slot.animPath.deltas;
                a1.pathQueryFlags = 0;
                if (AnimPathSpecInit((AnimPathData*) &a1))
                    slot.animPath.deltaIdxMax = AnimPathSearch((AnimPathData*) &a1);
                else
                    slot.animPath.deltaIdxMax = 0;
                v14 = slot.animPath.deltaIdxMax;
                if (v14 && v14 <= v21
                    || (a1.pathQueryFlags = 1, AnimPathSpecInit((AnimPathData*) &a1))
                    && (v15 = AnimPathSearch((AnimPathData*) &a1), (slot.animPath.deltaIdxMax = v15) != 0)
                    && v15 <= v21)
                {
                    v16 = a1.loc1.X;
                    slot.animPath.flags &= 0xFFFFFFFC;
                    slot.animPath.objLoc.Y = a1.loc1.Y;
                    v17 = a1.objLoc.Y;
                    slot.animPath.objLoc.X = v16;
                    v18 = a1.objLoc.X;
                    slot.animPath.loc.Y = v17;
                    slot.animPath.loc.X = v18;
                    slot.animPath.fieldD4 = 0;
                    v19 = slot.pCurrentGoal;
                    v19.target_tile.xy.X = v12;
                    v19.target_tile.xy.Y = v11;
                    LODWORD(slot.pCurrentGoal.target_tile.offsetx) = 0;
                    LODWORD(slot.pCurrentGoal.target_tile.offsety) = 0;
                    return true;
                }

                AiEndCombatTurn(ObjHnd);
            }
            return 0;
        }


        [TempleDllLocation(0x10017b30)]
        public static bool GoalIsDoorFullyClosed(AnimSlot slot)
        {
            uint v1;
            uint v2;
            int v3;
            int result;
            AnimSlotId a1;

            v1 = slot.param1.obj;
            v2 = HIDWORD(slot.param1.obj);
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            result = 0;
            if (!(v1.IsOffOrDestroyed)
                && !v1.IsPortalOpen())
            {
                if (!anim_run_id_for_obj(v1, &a1)
                    || animpriv_get_slot(&a1, &slot)
                    && ((v3 = slot.goals[slot.currentGoal].goalType, v3 == AnimGoalType.anim_idle)
                        || v3 == AnimGoalType.anim_fidget
                        || v3 == AnimGoalType.animate_loop))
                {
                    result = 1;
                }
            }

            return result;
        }


        [TempleDllLocation(0x10017dd0)]
        public static bool GoalTriggerSpell(AnimSlot slot)
        {
            AnimSlot* v1;
            __int64 v2;
            int v3;
            int v4;
            int v6;
            int v7;
            AnimSlot* v8;
            int spellId;

            v1 = slot;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            spellId = slot.param2.spellId;
            AssertAnimParam(v2); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            AasAnimatedModelGetAnimId(v3, (int*) &slot);
            if (v2.GetSpellFlags() & SpellFlag.STONED)
                return 0;
            if (IsStonedStunnedOrParalyzed(v2)
                && ((uint) slot >> 30
                    || ((uint) slot & 0xFFFFFFF) != 9
                    && ((uint) slot & 0xFFFFFFF) != 10
                    && ((uint) slot & 0xFFFFFFF) != 11))
            {
                return 0;
            }

            if (v2.IsCritter() && v1.pCurrentGoal.goalType == AnimGoalType.dying)
            {
                v6 = Soundmap_Critter(v2, 1);
                GameSystems.SoundGame.PositionalSound(v6, 1, v2);
            }

            v7 = v1.pCurrentGoal.animIdPrevious;
            if (v7 == -1)
                v8 = (AnimSlot*) WA_RIGHT_HIT;
            else
                v8 = (AnimSlot*) ((v7 - 64) & 0xFFFFFFF);
            slot = v8;
            v2.SetAnimId((AnimationIds) v8);
            anim_play_water_ripples(v2);
            v1.path.someDelay = 33;
            v1.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            if (spellId)
            {
                o_2_a_10C_s_B___9(spellId);
                python_spell_trigger(spellId, 1);
            }

            v1.flags = v1.flags & 0xFFFFFFF3 | 0x10;
            return true;
        }


        [TempleDllLocation(0x10017f80)]
        public static bool GoalUnconcealCleanup(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            ObjHndl v4;
            int v5;
            int result;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj.objid); /*obj != OBJ_HANDLE_NULL*/
            print_debug_message(" endUnconceal\n");
            v3 = slot.flags;
            if (SBYTE1(v3) < 0 || GameSystems.Map.IsClearingMap())
            {
                result = 0;
            }
            else
            {
                if (obj_get_int32(v1, obj_f.type) == 13
                    || obj_get_int32(v1, obj_f.type) == 14)
                {
                    v4.id = GetLeader(v1).id;
                    if (!v4.id || !Is_Obj_Concealed(v4))
                        Obj_Concealed_Set(v1, 0);
                }

                v5 = get_idle_anim(v1);
                v1.SetAnimId((AnimationIds) v5);
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10018050)]
        public static bool GoalResetToIdleAnim(AnimSlot slot)
        {
            int v1;
            int result;
            __int64 v3;
            int v4;
            int v5;
            int v6;
            int pField4Out;

            pField4Out = v1;
            if (GameSystems.Map.IsClearingMap())
            {
                result = 1;
            }
            else
            {
                HIDWORD(v3) = slot.param1.loc.xy.X;
                LODWORD(v3) = slot.param1.loc.xy.Y;
                AssertAnimParam(v3); /*obj != OBJ_HANDLE_NULL*/
                if (is_critter_and_prone(v3))
                    goto LABEL_20;
                v4 = obj_get_aas_handle(v3);
                AssertAnimParam(v4); /*handle != AAS_HANDLE_NULL*/
                AasAnimatedModelGetAnimId(v4, &pField4Out);
                v5 = slot.flags;
                if (SBYTE1(v5) < 0 || GameSystems.Map.IsClearingMap() || !v3)
                {
                    LABEL_20:
                    result = 0;
                }
                else
                {
                    v6 = obj_get_int32(v3, obj_f.type);
                    if (v6 == 13 || v6 == 14)
                    {
                        if (!CritterIsMovingSilently(v3))
                            pField4Out = get_idle_anim(v3);
                        v3.SetAnimId((AnimationIds) pField4Out);
                    }

                    result = 1;
                }
            }

            return result;
        }


        [TempleDllLocation(0x10018160)]
        public static bool GoalResetToIdleAnimUnstun(AnimSlot slot)
        {
            int v1;
            int result;
            __int64 v3;
            int v4;
            int v5;
            int v6;
            int v7;
            int pField4Out;

            pField4Out = v1;
            if (GameSystems.Map.IsClearingMap())
            {
                result = 1;
            }
            else
            {
                HIDWORD(v3) = slot.param1.loc.xy.X;
                LODWORD(v3) = slot.param1.loc.xy.Y;
                AssertAnimParam(v3); /*obj != OBJ_HANDLE_NULL*/
                v4 = obj_get_aas_handle(v3);
                AssertAnimParam(v4); /*handle != AAS_HANDLE_NULL*/
                AasAnimatedModelGetAnimId(v4, &pField4Out);
                v5 = slot.flags;
                if (SBYTE1(v5) < 0 || GameSystems.Map.IsClearingMap() || !v3)
                {
                    result = 0;
                }
                else
                {
                    v6 = obj_get_int32(v3, obj_f.type);
                    if (v6 == 13 || v6 == 14)
                    {
                        if (!CritterIsMovingSilently(v3))
                            pField4Out = get_idle_anim(v3);
                        v3.SetAnimId((AnimationIds) pField4Out);
                        anim_move_reset_offsets_consider_frog(v3, 0.0, 0.0);
                        v7 = obj_get_int32(v3, obj_f.critter_flags);
                        obj_set_int32_or_float32(v3, obj_f.critter_flags, v7 & ~CritterFlag.STUNNED);
                    }

                    result = 1;
                }
            }

            return result;
        }


        [TempleDllLocation(0x10018290)]
        public static bool GoalThrowItemCleanup(AnimSlot slot)
        {
            int v1;
            int result;
            __int64 v3;
            int v4;
            int v5;
            int v6;
            AnimSlotGoalStackEntry* v7;
            int v8;
            int v9;
            int v10;
            long v11;
            int pField4Out;

            pField4Out = v1;
            if (GameSystems.Map.IsClearingMap())
                return true;
            HIDWORD(v3) = slot.param1.loc.xy.X;
            LODWORD(v3) = slot.param1.loc.xy.Y;
            AssertAnimParam(v3); /*obj != OBJ_HANDLE_NULL*/
            v4 = obj_get_aas_handle(v3);
            AssertAnimParam(v4); /*handle != AAS_HANDLE_NULL*/
            AasAnimatedModelGetAnimId(v4, &pField4Out);
            v5 = slot.flags;
            if (SBYTE1(v5) < 0 || GameSystems.Map.IsClearingMap() || !v3)
            {
                result = 0;
            }
            else
            {
                v6 = obj_get_int32(v3, obj_f.type);
                if (v6 == 13 || v6 == 14)
                {
                    if (!CritterIsMovingSilently(v3))
                        pField4Out = get_idle_anim(v3);
                    v3.SetAnimId((AnimationIds) pField4Out);
                    anim_move_reset_offsets_consider_frog(v3, 0.0, 0.0);
                    v7 = slot.pCurrentGoal;
                    v8 = v7.scratch_obj.objid;
                    v9 = HIDWORD(v7.scratch_obj.objid);
                    if (v7.scratch_obj.objid)
                    {
                        v10 = sub_100675B0(v8, v9, v3);
                        if (v10 != -1)
                        {
                            ItemInsertAtLocation(v8, v3, v10);
                            return true;
                        }

                        v11 = v3.GetLocation();
                        MoveItemUnflagNotransfer(v8, (location2d) v11);
                    }
                }

                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10018400)]
        public static bool GoalThrowItemPlayAnim(AnimSlot slot)
        {
            AnimSlot* v1;
            int v2;
            int v3;
            int v4;
            int v5;
            int v7;

            v1 = slot;
            v2 = slot.param1.loc.xy.X;
            v3 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v4 = v2.GetOrCreateAnimHandle();
            AssertAnimParam(v4); /*handle != OBJ_HANDLE_NULL*/
            if (v1.param2.animId == -1)
            {
                AssertAnimParam(!AasAnimatedModelGetAnimId(v4, (int*) &slot)); /*ar == AAS_OK*/
            }
            else
            {
                slot = (AnimSlot*) v1.param2.animId;
            }

            AssertAnimParam(!v2.SetAnimId((AnimationIds) slot)); /*ar == AAS_OK*/

            if (IsStonedStunnedOrParalyzed(v2))
            {
                AasAnimatedModelGetAnimId(v4, (int*) &slot);
                if ((uint) slot >> 30
                    || ((uint) slot & 0xFFFFFFF) != 9
                    && ((uint) slot & 0xFFFFFFF) != 10
                    && ((uint) slot & 0xFFFFFFF) != 11)
                {
                    return 0;
                }

                if (v1.pCurrentGoal.goalType == AnimGoalType.dying)
                {
                    v7 = Soundmap_Critter(v2, 1);
                    GameSystems.SoundGame.PositionalSound(v7, 1, v2);
                }
            }

            v1.path.someDelay = 33;
            v1.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            anim_play_water_ripples(v2);
            v1.flags = v1.flags & 0xFFFFFFF3 | 0x10;
            return true;
        }


        [TempleDllLocation(0x10018730)]
        public static bool GoalStartIdleAnimIfCloseToParty(AnimSlot slot)
        {
            int v1;
            int v2;
            signed __int64 v3;
            int v4;
            int result;
            int v6;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v3 = GameSystems.Party.DistanceToParty(v1);
            if (SHIDWORD(v3) >= 0 && (SHIDWORD(v3) > 0 || (uint) v3 > 60)
                || IsStonedStunnedOrParalyzed(v1))
            {
                result = 0;
            }
            else
            {
                slot.path.someDelay = 33;
                slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
                v6 = get_idle_anim(v1);
                v1.SetAnimId((AnimationIds) v6);
                slot.flags = slot.flags & 0xFFFFFFF3 | 0x10;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10018810)]
        public static bool GoalStartFidgetAnimIfCloseToParty(AnimSlot slot)
        {
            int v1;
            int v2;
            signed __int64 v3;
            int v4;
            int result;
            int v6;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v3 = GameSystems.Party.DistanceToParty(v1);
            if (SHIDWORD(v3) >= 0 && (SHIDWORD(v3) > 0 || (uint) v3 > 0x3C)
                || IsStonedStunnedOrParalyzed(v1))
            {
                result = 0;
            }
            else
            {
                slot.path.someDelay = 33;
                slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
                v6 = get_fidget_animation(v1);
                v1.SetAnimId((AnimationIds) v6);
                anim_play_water_ripples(v1);
                slot.flags = slot.flags & 0xFFFFFFF3 | 0x10;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x100188f0)]
        public static bool GoalContinueWithAnim(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            CHAR v4;
            int result;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            v4 = eventOut;
            if (eventOut & 1)
                slot.flags |= 4u;
            if (v4 & 2)
            {
                slot.flags &= 0xFFFFFFEF;
                result = 0;
            }
            else
            {
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x100189b0)]
        public static bool GoalContinueWithAnim2(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            CHAR v4;
            int result;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            v4 = eventOut;
            if (eventOut & 1)
                slot.flags |= 4u;
            if (v4 & 2)
            {
                slot.flags &= 0xFFFFFFEF;
                result = 0;
            }
            else
            {
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10018a70)]
        public static bool GoalPlayDoorOpenAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int result;
            int v5;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*door != OBJ_HANDLE_NULL*/
            AssertAnimParam(v1.GetOrCreateAnimHandle()); /*handle != OBJ_HANDLE_NULL*/
            v3 = obj_get_int32(v1, obj_f.portal_flags);
            if (BYTE1(v3) & 2)
            {
                result = 0;
            }
            else
            {
                AssertAnimParam(!v1.SetAnimId(aid_open)); /*ar == AAS_OK*/
                slot.path.someDelay = 33;
                slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
                anim_play_water_ripples(v1);
                v5 = GameSystems.SoundMap.GetPortalSoundEffect(v1, 0);
                GameSystems.SoundGame.PositionalSound(v5, 1, v1);
                if (!(v3 & 4))
                {
                    obj_set_int32_or_float32(v1, obj_f.portal_flags, v3 | 0x200);
                    GameSystems.MapFogging.SetMapDoFoggingUpdate();
                }

                slot.flags = slot.flags & 0xFFFFFFF3 | 0x10;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10018b90)]
        public static bool GoalContinueWithDoorOpenAnim(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*door != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            if (eventOut & 1)
            {
                slot.flags |= 4u;
                return true;
            }

            if (!(eventOut & 2))
                return true;
            slot.flags &= 0xFFFFFFEF;
            return 0;
        }


        [TempleDllLocation(0x10018c50)]
        public static bool GoalPlayDoorCloseAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj != null); /*door != OBJ_HANDLE_NULL*/
            AssertAnimParam(v2.GetOrCreateAnimHandle()); /*handle != OBJ_HANDLE_NULL*/
            AssertAnimParam(!v2.SetAnimId(aid_close)); /*ar == AAS_OK*/
            slot.path.someDelay = 33;
            slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            anim_play_water_ripples(v2);
            v3 = GameSystems.SoundMap.GetPortalSoundEffect(v2, 0);
            GameSystems.SoundGame.PositionalSound(v3, 1, v2);
            slot.flags = slot.flags & 0xFFFFFFF3 | 0x10;
            return true;
        }


        [TempleDllLocation(0x10018d40)]
        public static bool GoalContinueWithDoorCloseAnim(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*door != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            if (eventOut & 1)
            {
                slot.flags |= 4u;
                return true;
            }

            if (!(eventOut & 2))
                return true;
            slot.flags &= 0xFFFFFFEF;
            return 0;
        }


        [TempleDllLocation(0x10018e00)]
        public static bool GoalPickLockPlayPushDoorOpenAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int v4;
            int result;
            int v6;
            int v7;
            timeevent_time v8;
            int v9;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_int32(v2, obj_f.type);
            v4 = v3;

            AssertAnimParam(v2.IsCritter()); // obj_type_is_critter( obj_type )

            if (v2.IsCritter())
            {
                v6 = slot.param2.animId;
                if (v6 == -1)
                    v7 = WA_LEFT_ATTACK_3;
                else
                    v7 = (v6 - 64) & 0xFFFFFFF;
                v2.SetAnimId((AnimationIds) v7);
                anim_play_water_ripples(v2);
                slot.path.someDelay = 33;
                v8 = GameSystems.TimeEvent.AnimTime;
                v9 = slot.flags & 0xFFFFFFF3 | 0x10;
                slot.gametimeSth = (ulong) v8;
                slot.flags = v9;
                result = 1;
            }
            else
            {
                result = 0;
            }

            return result;
        }


        [TempleDllLocation(0x10018ee0)]
        public static bool GoalPickLockContinueWithAnim(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            if (eventOut & 1)
            {
                slot.flags |= 4u;
                return true;
            }

            if (!(eventOut & 2))
                return true;
            slot.flags &= 0xFFFFFFEF;
            return 0;
        }


        [TempleDllLocation(0x10018fa0)]
        public static bool GoalDyingPlaySoundAndRipples(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int v4;
            int result;
            int v6;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            AssertAnimParam(v2.IsCritter());
            if (v2.IsCritter())
            {
                v6 = Soundmap_Critter(v2, 1);
                GameSystems.SoundGame.PositionalSound(v6, 1, v2);
                slot.path.someDelay = 33;
                slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
                anim_play_water_ripples(v2);
                slot.flags |= 0x10u;
                result = 1;
            }
            else
            {
                result = 0;
            }

            return result;
        }


        [TempleDllLocation(0x10019070)]
        public static bool GoalDyingContinueAnim(AnimSlot slot)
        {
            DWORD v1;
            __int64 v2;
            int v3;
            CHAR v4;
            int result;
            DWORD eventOut;

            eventOut = v1;
            HIDWORD(v2) = slot.param1.loc.xy.X;
            LODWORD(v2) = slot.param1.loc.xy.Y;
            eventOut = 0;
            AssertAnimParam(v2); /*obj != OBJ_HANDLE_NULL*/
            v3 = obj_get_aas_handle(v2);
            AssertAnimParam(v3); /*handle != AAS_HANDLE_NULL*/
            anim_continue_with_animation(v2, slot, v3, &eventOut);
            v4 = eventOut;
            if (eventOut & 1)
                slot.flags |= 4u;
            if (v4 & 2)
            {
                slot.flags &= 0xFFFFFFEF;
                result = 0;
            }
            else
            {
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10019130)]
        public static bool GoalAnimateFireDmgContinueAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            CHAR v3;
            int v4;
            int result;
            CHAR v6;
            DWORD eventOut;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            v3 = slot.param1.obj == 0;
            eventOut = 0;
            AssertAnimParam(!v3); /*obj != OBJ_HANDLE_NULL*/
            v4 = v1.GetOrCreateAnimHandle();
            AssertAnimParam(v4); /*handle != AAS_HANDLE_NULL*/
            if (v1.GetSpellFlags() & SpellFlag.STONED)
            {
                result = 0;
            }
            else
            {
                anim_continue_with_animation(v1, slot, v4, &eventOut);
                v6 = eventOut;
                if (eventOut & 2)
                    slot.flags &= 0xFFFFFFFB;
                if (v6 & 1)
                    slot.flags |= 4u;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x100191f0)]
        public static bool GoalStunnedPlayAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            uint v3;
            int v4;
            int result;
            timeevent_time v6;
            int v7;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            AssertAnimParam(v1.GetOrCreateAnimHandle()); /*handle != AAS_HANDLE_NULL*/
            v3 = GameSystems.Critter.GetAnimId(v1, WA_PANIC);
            if (v1.GetSpellFlags() & SpellFlag.STONED
                || ((v4 = obj_get_int32(v1, obj_f.type), v4 == 13) || v4 == 14)
                && obj_get_int32(v1, obj_f.critter_flags) & 0x40
                && (v3 >> 30 || (v3 & 0xFFFFFFF) != 9 && (v3 & 0xFFFFFFF) != 10 && (v3 & 0xFFFFFFF) != 11))
            {
                result = 0;
            }
            else
            {
                v1.SetAnimId((AnimationIds) v3);
                anim_play_water_ripples(v1);
                slot.path.someDelay = 33;
                v6 = GameSystems.TimeEvent.AnimTime;
                v7 = slot.flags & 0xFFFFFFF3 | 0x10;
                slot.gametimeSth = (ulong) v6;
                slot.flags = v7;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10019330)]
        public static bool GoalStunnedContinueAnim(AnimSlot slot)
        {
            AnimSlot* v1;
            int v2;
            int v3;
            CHAR v4;
            int v5;
            int v6;
            CHAR v7;
            DWORD eventOut;

            v1 = slot;
            v2 = slot.param1.loc.xy.X;
            v3 = slot.param1.loc.xy.Y;
            v4 = slot.param1.obj == 0;
            eventOut = 0;
            AssertAnimParam(!v4); /*obj != OBJ_HANDLE_NULL*/
            v5 = v2.GetOrCreateAnimHandle();
            AssertAnimParam(v5); /*handle != AAS_HANDLE_NULL*/
            if (v2.GetSpellFlags() & SpellFlag.STONED)
                return 0;
            v6 = obj_get_int32(v2, obj_f.type);
            if (v6 == 13 || v6 == 14)
            {
                if (obj_get_int32(v2, obj_f.critter_flags) & 0x40)
                {
                    AasAnimatedModelGetAnimId(v5, (int*) &slot);
                    if ((uint) slot >> 30
                        || ((uint) slot & 0xFFFFFFF) != 9
                        && ((uint) slot & 0xFFFFFFF) != 10
                        && ((uint) slot & 0xFFFFFFF) != 11)
                    {
                        return 0;
                    }
                }
            }

            anim_continue_with_animation(v2, v1, v5, &eventOut);
            v7 = eventOut;
            if (eventOut & 1)
                v1.flags |= 4u;
            if (v7 & 2)
            {
                v1.flags &= 0xFFFFFFEF;
                return 0;
            }

            return true;
        }


        [TempleDllLocation(0x10019470)]
        public static bool GoalPlayGetUpAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int result;
            timeevent_time v5;
            int v6;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            if (v1.GetSpellFlags() & SpellFlag.STONED
                || ((v3 = obj_get_int32(v1, obj_f.type), v3 == 13) || v3 == 14)
                && obj_get_int32(v1, obj_f.critter_flags) & 0x40)
            {
                result = 0;
            }
            else
            {
                v1.SetAnimId(aid_getup);
                anim_play_water_ripples(v1);
                slot.path.someDelay = 33;
                v5 = GameSystems.TimeEvent.AnimTime;
                v6 = slot.flags & 0xFFFFFFF3 | 0x10;
                slot.gametimeSth = (ulong) v5;
                slot.flags = v6;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10019540)]
        public static bool GoalPlayUnconcealAnim(AnimSlot slot)
        {
            int v1;
            int v2;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            AssertAnimParam(v2.GetOrCreateAnimHandle()); /*handle != OBJ_HANDLE_NULL*/
            AssertAnimParam(!v2.SetAnimId(aid_unconceal)); /*ar == AAS_OK*/
            slot.path.someDelay = 33;
            slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            anim_play_water_ripples(v2);
            Obj_Fade_To(v2, 255, 50, 51, 0);
            slot.flags = slot.flags & 0xFFFFFFF3 | 0x10;
            return true;
        }


        [TempleDllLocation(0x10019630)]
        public static bool GoalPlayMoveAnim(AnimSlot slot)
        {
            AnimSlot* v1;
            int v2;
            int v4;
            int v5;
            int v6;
            int v7;
            int v8;
            int v9;
            int v10;
            int v11;
            int v12;
            AnimSlotGoalStackEntry* v13;
            int v14;
            float rotation;
            long v16;
            int v17;
            int a5;
            int y;
            int a4[2];
            location2d newLoc;

            v1 = slot;
            v2 = slot.flags;
            if (BYTE1(v2) & 1)
                return 0;
            if (v2 & 0x20)
            {
                slot.flags = v2 | 0x10;
                return true;
            }

            v4 = slot.param1.loc.xy.X;
            v5 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v6 = v4.GetOrCreateAnimHandle();
            AssertAnimParam(v6); /*handle != AAS_HANDLE_NULL*/
            if (IsStonedStunnedOrParalyzed(v4))
            {
                return false;
            }

            AasAnimatedModelGetAnimId(v6, (int*) &slot);
            v1.pCurrentGoal.animData = (int) slot;
            AnimGetMoveAnimationId(v1, v4, (int*) &slot, ((uint) v1.flags >> 6) & 1, &a5);
            if (return_false_func())
            {
                *(_QWORD*) a4 = v4.GetLocation();
                y = a4[1];
                v8 = a4[0];
                while (sub_10062DA0() > 0)
                {
                    if (!sub_10014DC0(v1))
                        break;
                    if (sub_1000E360((int) v1, v4, v8, y))
                        break;
                    v9 = v1.animPath.fieldD4;
                    v10 = *((_BYTE*) &v1.animPath.deltas + v9);
                    v1.animPath.fieldD4 = v9 + 1;
                    GetLocationOffsetByUnityInDirection((location2d) v8, v10, (location2d*) a4);
                    v11 = v1.animPath.fieldD4;
                    v12 = v1.animPath.deltaIdxMax;
                    v8 = a4[0];
                    y = a4[1];
                    if (v11 >= v12)
                        break;
                }

                Obj_Move(v4, (location2d) v8, 0.0, 0.0);
                v13 = v1.pCurrentGoal;
                v14 = y;
                v13.target_tile.xy.X = v8;
                v13.target_tile.xy.Y = v14;
                LODWORD(v1.pCurrentGoal.target_tile.offsetx) = 0;
                LODWORD(v1.pCurrentGoal.target_tile.offsety) = 0;
                v1.goals[0].target_tile.xy.X = v8;
                v1.goals[0].target_tile.xy.Y = v14;
                LODWORD(v1.goals[0].target_tile.offsetx) = 0;
                LODWORD(v1.goals[0].target_tile.offsety) = 0;
                rotation = (long double)*((_BYTE*) &v1.animPath.flags + v1.animPath.fieldD4 + 3) * 0.78539819;
                GameSystems.MapObject.SetRotation(v4, rotation);
                return 0;
            }

            v16 = v4.GetLocation();
            if (!GetLocationOffsetByUnityInDirection((location2d) v16, LOBYTE(v1.animPath.deltas), &newLoc))
                return 0;
            v4.SetAnimId((AnimationIds) slot);
            v1.path.someDelay = 16;
            v1.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            anim_play_water_ripples(v4);
            goalstatefunc_124_get_pause_time((timeevent_time*) &v1.path.occupiedFlag, v4);
            if (v17 & 0x80000)
                v1.path.someDelay *= 2;
            v1.animPath.flags &= 0xFFFFFFFC;
            v1.animPath.fieldD4 = 0;
            v1.flags |= 0x30u;
            return true;
        }


        [TempleDllLocation(0x10019920)]
        public static bool GoalPlayWaterRipples(AnimSlot slot)
        {
            int v1;
            int v2;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            slot.path.someDelay = 33;
            slot.gametimeSth = (ulong) GameSystems.TimeEvent.AnimTime;
            anim_play_water_ripples(v2);
            slot.animPath.flags &= 0xFFFFFFFE;
            slot.animPath.fieldD4 = 0;
            slot.flags |= 0x10u;
            return true;
        }


        [TempleDllLocation(0x100199b0)]
        public static bool GoalContinueMoveStraight(AnimSlot slot)
        {
            AnimSlot* v1;
            int v2;
            int v3;
            CHAR v4;
            int v5;
            float v6;
            long v7;
            uint v8;
            int v9;
            int v10;
            long double v11;
            uint v12;
            int v13;
            int v14;
            int v15;
            int result;
            float offX;
            int v18;
            int v19;
            DWORD eventOut;
            int animHandle;
            int v22;
            uint a1;
            int tileY;
            int a3[2];
            double a4;
            int a2;
            ulong v28;
            ulong v29;
            game_anim* anima;
            game_anim* anim;

            v1 = slot;
            v2 = slot.param1.loc.xy.Y;
            v3 = slot.param1.loc.xy.X;
            v4 = slot.param1.obj == 0;
            eventOut = 0;
            AssertAnimParam(!v4); /*obj != OBJ_HANDLE_NULL*/
            animHandle = v3.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle); /*handle != AAS_HANDLE_NULL*/
            v5 = slot.pCurrentGoal.scratchVal5;
            v18 = slot.pCurrentGoal.scratchVal5;
            GameSystems.Anim.customDelayInMs = 35;
            if (!v5)
            {
                v18 = 4;
                v5 = 4;
                GameSystems.Anim.customDelayInMs = 35;
            }

            v19 = 0;
            if (v5 <= 0)
            {
                LABEL_14:
                result = 1;
            }
            else
            {
                while (1)
                {
                    v6 = obj_get_float32(v3, obj_f.offset_x);
                    *(float*) &anima = obj_get_float32(v3, obj_f.offset_y);
                    v7 = v3.GetLocation();
                    a2 = HIDWORD(v7);
                    v8 = v7;
                    anim_continue_with_animation(v3, v1, animHandle, &eventOut);
                    v9 = v1.animPath.fieldD4;
                    v10 = *((_BYTE*) &v1.animPath.deltas + v9 + 1);
                    v22 = *((_BYTE*) &v1.animPath.deltas + v9);
                    v11 = (long double)v22;
                    v22 = v10;
                    offX = v11 + v6;
                    *(float*) &anim = (long double)v10 + *(float*) &anima;
                    location_get_translation(v8, a2, (long*) a3, (long*) &a4);
                    if (screen_to_loc(
                        *(_QWORD*) a3 + (ulong) offX + 20,
                        *(_QWORD*) &a4 + (ulong) *(float*) &anim + 14,
                        (location2d*) &a1))
                    {
                        v12 = a1;
                        v13 = tileY;
                        if (a1 != v8 || tileY != a2)
                        {
                            location_get_translation(a1, tileY, (long*) &v28, (long*) &v29);
                            v13 = tileY;
                            v22 = LODWORD(a4) - v29;
                            v12 = a1;
                            offX = (long double)(a3[0] - (int) v28) + offX;
                            *(float*) &anim = (long double)(LODWORD(a4) - (int) v29) + *(float*) &anim;
                        }

                        Obj_Move(v3, (location2d) v12, offX, *(float*) &anim);
                        v14 = v1.animPath.deltaIdxMax;
                        v15 = v1.animPath.fieldD4 + 2;
                        v1.animPath.fieldD4 = v15;
                        if (v15 >= v14)
                            break;
                    }

                    if (++v19 >= v18)
                        goto LABEL_14;
                }

                v1.flags &= 0xFFFFFFEF;
                result = 0;
            }

            return result;
        }


        [TempleDllLocation(0x10019c20)]
        public static bool GoalApplyKnockback(AnimSlot slot)
        {
            __int64 v1;
            int v2;
            int v3;
            location2d v4;
            CHAR* v5;
            int v6;
            int v7;
            int v8;
            int v9;
            unsigned __int8 v10;
            unsigned __int8 v11;
            int result;
            int v13;
            location2d loc;
            location2d locNew;
            long transY;
            __int64 transX;
            ulong v18;
            ulong v19;

            HIDWORD(v1) = slot.param1.loc.xy.X;
            LODWORD(v1) = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            GameSystems.Anim.customDelayInMs = 35;
            if (v1)
            {
                v13 = 0;
                while (1)
                {
                    v2 = obj_get_int32(v1, obj_f.offset_x);
                    v3 = obj_get_int32(v1, obj_f.offset_y);
                    v4 = (location2d) v1.GetLocation();
                    v5 = (CHAR*) &slot.animPath + slot.animPath.fieldD4;
                    v6 = v5[4] + v2;
                    v7 = v5[5] + v3;
                    loc = v4;
                    location_get_translation(v4.X, v4.Y, &transX, &transY);
                    if (!screen_to_loc(transX + v6 + 20, transY + v7 + 14, &locNew))
                    {
                        AssertAnimParam(false);
                    }

                    if (locNew != loc)
                    {
                        if (GoalKnockbackFindTarget(&slot.animPath, slot, v1, loc, locNew))
                        {
                            Obj_Move(v1, loc, 0.0, 0.0);
                            return 0;
                        }

                        location_get_translation(locNew.X, locNew.Y, (long*) &v19, (long*) &v18);
                    }

                    v8 = slot.animPath.deltaIdxMax;
                    v9 = slot.animPath.fieldD4 + 2;
                    slot.animPath.fieldD4 = v9;
                    if (v9 >= v8)
                        break;
                    v11 = __OFSUB__(v13 + 1, 4);
                    v10 = v13++ - 3 < 0;
                    if (!(v10 ^ v11))
                        return true;
                }

                slot.flags &= 0xFFFFFFEF;
                result = 0;
            }
            else
            {
                slot.flags &= 0xFFFFFFEF;
                result = 0;
            }

            return result;
        }


        [TempleDllLocation(0x10019e10)]
        public static bool GoalDyingReturnTrue(AnimSlot slot)
        {
            int result;
            long v2;

            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/

            if (slot.param1.obj)
            {
                v2 = obj_get_int64(slot.param1.obj, obj_f.location);
                GameSystems.Tile.IsBlockingOldVersion(v2, SHIDWORD(v2), 0);
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10019e70)]
        public static bool GoalAttemptMoveCleanup(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int v4;
            int v5;
            int v6;
            int result;

            v1 = slot.param1.loc.xy.Y;
            v2 = slot.param1.loc.xy.X;
            AssertAnimParam(slot.param1.obj.objid); /*obj != OBJ_HANDLE_NULL*/
            v3 = slot.flags;
            if (SBYTE1(v3) < 0 || GameSystems.Map.IsClearingMap() || !(v1 | v2))
            {
                result = 0;
            }
            else
            {
                v4 = slot.field_14;
                v5 = slot.currentGoal;
                if (v4 == v5)
                    slot.animPath.flags |= 1u;
                if (v4 == v5 - 1)
                    slot.animPath.flags |= 1u;
                v6 = get_idle_anim(v2);
                v2.SetAnimId((AnimationIds) v6);
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x10019f00)]
        public static bool GoalAttackPlayWeaponHitEffect(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;
            int result;
            int v5;
            int v6;
            ObjHndl weapon;
            int v8;
            timeevent_time v9;
            ulong target;

            v1 = slot.param2.loc.xy.X;
            v2 = slot.param1.loc.xy.X;
            v3 = slot.param1.loc.xy.Y;
            target = slot.param2.obj;
            AssertAnimParam(slot.param1.obj != null); /*source_obj != OBJ_HANDLE_NULL*/
            AssertAnimParam((HIDWORD(target) | v1)); /*target_obj != OBJ_HANDLE_NULL*/
            if (IsStonedStunnedOrParalyzed(v2))
            {
                result = 0;
            }
            else
            {
                v5 = slot.pCurrentGoal.animIdPrevious;
                if (v5 == -1)
                    v5 = 1;
                v6 = GameSystems.Critter.GetAnimId(v2, (WeaponAnimId) v5);
                v2.SetAnimId((AnimationIds) v6);
                if (v5 < 4 || v5 > 6)
                    LODWORD(weapon.id) = Obj_Get_Item_At_Inventory_Location_n(v2, 203);
                else
                    LODWORD(weapon.id) = Obj_Get_Item_At_Inventory_Location_n(v2, 204);
                v8 = combat_find_weapon_sound(weapon, v2, target, 4);
                GameSystems.SoundGame.PositionalSound(v8, 1, v2);
                anim_play_water_ripples(v2);
                slot.path.someDelay = 33;
                v9 = GameSystems.TimeEvent.AnimTime;
                LODWORD(slot.gametimeSth) = v9.timeInDays;
                slot.flags |= 0x10u;
                HIDWORD(slot.gametimeSth) = v9.timeInMs;
                result = 1;
            }

            return result;
        }


        [TempleDllLocation(0x1001a080)]
        public static bool GoalAttackContinueWithAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            CHAR v3;
            int v4;
            int v5;
            CHAR v6;
            DWORD eventOut;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            v3 = slot.param1.obj == 0;
            eventOut = 0;
            AssertAnimParam(!v3); /*obj != OBJ_HANDLE_NULL*/
            v4 = v1.GetOrCreateAnimHandle();
            AssertAnimParam(v4); /*handle != AAS_HANDLE_NULL*/
            if (IsStonedStunnedOrParalyzed(v1))
            {
                return 0;
            }

            anim_continue_with_animation(v1, slot, v4, &eventOut);
            v6 = eventOut;
            if (eventOut & 1)
                slot.flags |= 4u;
            if (v6 & 2)
            {
                slot.flags &= 0xFFFFFFEF;
                return 0;
            }

            return true;
        }


        [TempleDllLocation(0x1001a170)]
        public static bool GoalAttackPlayIdleAnim(AnimSlot slot)
        {
            int v1;
            int v2;
            int v3;

            v1 = slot.param1.loc.xy.X;
            v2 = slot.param1.loc.xy.Y;
            AssertAnimParam(slot.param1.obj != null); /*obj != OBJ_HANDLE_NULL*/
            v3 = get_idle_anim(v1);
            v1.SetAnimId((AnimationIds) v3);
            return true;
        }


        [TempleDllLocation(0x1001bf70)]
        public static bool GoalMoveNearUpdateRadiusToReach(AnimSlot slot)
        {
            int v1;
            int v2;
            uint v3;
            long v4;
            ObjHndl v5;
            int v6;
            int v7;
            CHAR* v8;
            int v10;
            int v11;
            ObjHndl handle;
            uint v13;
            ObjHndl objOut;

            v1 = slot.param2.loc.xy.Y;
            v2 = slot.param2.loc.xy.X;
            v3 = 0;
            v13 = 0;
            handle.id = slot.param1.obj;
            if (slot.param2.obj)
            {
                v4 = v2.GetLocation();
                v3 = v4;
                LODWORD(v4) = slot.pCurrentGoal;
                v13 = HIDWORD(v4);
                if (*(_DWORD*) (v4 + 88) != v3 || *(_DWORD*) (v4 + 92) != HIDWORD(v4))
                    slot.animPath.flags |= 4u;
            }

            if (!(slot.animPath.flags & 4) && v13 | v3)
            {
                if (GoalMoveCanReachTarget(handle, v2))
                    return 0;
                if (FindObstacleObj(handle, (location2d) v3, &objOut) < 26
                    && (!objOut.id || objOut.id == v2))
                {
                    v5.id = GameSystems.Combat.GetMainHandWeapon(handle);
                    if (v5.id)
                    {
                        v6 = GameSystems.Item.GetReachWithWeapon(v5, handle);
                        v7 = 0;
                        if (slot.currentGoal > 0)
                        {
                            v8 = (CHAR*) &slot.goals[0].radius;
                            do
                            {
                                *(_DWORD*) v8 = v6;
                                ++v7;
                                v8 += 544;
                            } while (v7 < slot.currentGoal);
                        }

                        slot.animPath.flags |= 4u;
                    }
                }
            }

            if (!(slot.animPath.flags & 0xC))
                return 0;
            sub_10014DC0(slot);
            v10 = slot.currentGoal;
            if (v10 > 0)
                slot.goals[v10].scratchVal4 = slot.pCurrentGoal.scratchVal4;
            slot.animPath.flags = slot.animPath.flags & 0xFFFFFFFB | 1;
            v11 = slot.flags & 0xFFFFFFCF;
            slot.flags = v11;
            if (slot.animPath.flags & 8)
                slot.flags = v11 | 2;
            return true;
        }


        private static void AssertAnimParam(bool expression)
        {
            if (!expression)
            {
                GameSystems.Anim.Debug();
                Trace.Assert(!expression);
            }
        }
    }
}