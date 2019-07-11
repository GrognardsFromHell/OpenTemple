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
        private static void ContinueWithAnimation(GameObjectBody obj, AnimSlot slot, IAnimatedModel animHandle,
            out AnimatedModelEvents eventOut)
        {
            GameObjectBody mainHand = null;
            GameObjectBody offHand = null;
            if (obj.IsCritter())
            {
                mainHand = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
                offHand = GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);
                if (offHand == null)
                {
                    offHand = GameSystems.Item.ItemWornAt(obj, EquipSlot.Shield);
                }
            }

            GameSystems.Script.SetAnimObject(obj);

            var elapsedTime = GameSystems.TimeEvent.AnimTime - slot.gametimeSth;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;

            var elapsedSeconds = (float) elapsedTime.TotalSeconds;

            if (elapsedSeconds < 0.0f)
            {
                elapsedSeconds = 0.0f;
            }

            var animParams = obj.GetAnimParams();
            eventOut = animHandle.Advance(elapsedSeconds, 0.0f, 0.0f, animParams);

            if (mainHand != null)
            {
                var itemAnimParams = mainHand.GetAnimParams();
                var itemAnim = mainHand.GetOrCreateAnimHandle();
                itemAnim?.Advance(elapsedSeconds, 0.0f, 0.0f, itemAnimParams);
            }

            if (offHand != null)
            {
                var itemAnimParams = offHand.GetAnimParams();
                var itemAnim = offHand.GetOrCreateAnimHandle();
                itemAnim?.Advance(elapsedSeconds, 0.0f, 0.0f, itemAnimParams);
            }
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
            var result = GoalMoveCanReachTarget(slot.param1.obj, slot.param2.obj);
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
            var obj = slot.param1.obj;
            if (!obj.IsCritter())
            {
                return false;
            }

            return GameSystems.Critter.IsConcealed(obj);
        }

        private static readonly float OneDegreeRadians = Angles.ToRadians(1.0f);

        [TempleDllLocation(0x100125F0)]
        public static bool GoalIsRotatedTowardNextPathNode(AnimSlot slot)
        {
            if (slot.pCurrentGoal == null)
            {
                Debugger.Break(); // TODO: This is not allowed to happen
                slot.pCurrentGoal = slot.goals[slot.currentGoal];
            }

            if (slot.path.nodeCount <= 0 || slot.path.currentNode >= slot.path.nodeCount)
            {
                return true;
            }

            // get the mover's location
            var obj = slot.param1.obj;
            var objLoc = obj.GetLocationFull();
            var objAbs = objLoc.ToInches2D();

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

            var rot = (nodeAbs - objAbs).GetWorldRotation();
            slot.pCurrentGoal.scratchVal2.floatNum = rot;
            var objRot = obj.Rotation;
            var shortestAngle = Angles.ShortestAngleBetween(objRot, rot);
            return MathF.Abs(shortestAngle) <= OneDegreeRadians;
        }

        [TempleDllLocation(0x10012C70)]
        public static bool GoalIsSlotFlag10NotSet(AnimSlot slot)
        {
            return !slot.flags.HasFlag(AnimSlotFlag.UNK5);
        }

        [TempleDllLocation(0x10011880)]
        public static bool GoalPlayGetHitAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var worldXY = obj.GetLocationFull().ToInches2D();

            var obj2 = slot.param2.obj;
            var worldXY2 = obj2.GetLocationFull().ToInches2D();

            var delta = worldXY2 - worldXY;

            var newRot = delta.GetWorldRotation() - obj.Rotation;
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
                if (eventOut.IsAction)
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
                    if (eventOut.IsEnd)
                    {
                        Logger.Info(
                            "Ending wait for animation action/end in goal {0}, because the idle animation would never end.",
                            slot.pCurrentGoal.goalType);
                    }

                    looping = true;
                }

                // This is the END trigger
                if (!looping && !eventOut.IsEnd)
                {
                    //Logger.Debug("GSF 106: eventOut.IsEnd, returned true");
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

            if (eventOut.IsAction)
                slot.flags |= AnimSlotFlag.UNK3;

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
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
            AssertAnimParam(obj != null);

            var location = slot.param2.location;
            var locSelf = obj.GetLocationFull();

            var distance = locSelf.DistanceTo(location);

            return distance <= locXY.INCH_PER_HALFTILE / 2.0f;
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

            slot.ClearPath();
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
                    slot.ClearPath();
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

            slot.ClearPath();

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
                    var soundId = GameSystems.SoundMap.GetPortalSoundEffect(doorObj, PortalSoundEffect.Locked);
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
            GameSystems.MapFogging.UpdateLineOfSight();
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
            GameSystems.MapFogging.UpdateLineOfSight();
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

            AssertAnimParam(obj != null);

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
                slot.flags &= ~(AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7);
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
            AssertAnimParam(obj != null);

            var targetRotation = slot.param2.floatNum;

            var remainingRotation = Angles.NormalizeRadians(targetRotation - obj.Rotation);

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
            AssertAnimParam(obj != null);
            var targetRotation = slot.param2.floatNum;
            var animModel = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animModel != null);

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

            var rotationPerSecond = animModel.GetRotationPerSec();
            if (MathF.Abs(rotationPerSecond) < OneDegreeRadians)
            {
                rotationPerSecond = 4 * MathF.PI;
            }

            var elapsedSeconds = (float) elapsedTime.TotalSeconds;
            var rotationThisStep = elapsedSeconds * MathF.Abs(rotationPerSecond);
            var currentRotation = obj.Rotation;
            var remainingRotation = Angles.ShortestAngleBetween(currentRotation, targetRotation);

            if (MathF.Abs(remainingRotation) <= rotationThisStep)
            {
                GameSystems.MapObject.SetRotation(obj, targetRotation);
                return false;
            }

            // This value is negative if we're rotating left
            if (remainingRotation < 0)
            {
                rotationThisStep = -rotationThisStep;
            }

            currentRotation += rotationThisStep;

            GameSystems.MapObject.SetRotation(obj, currentRotation);

            var animParams = obj.GetAnimParams();
            animModel.Advance(elapsedSeconds, 0.0f, rotationThisStep, animParams);

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
                var accelX = -MathF.Sin(angleInCone);
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
            }
            else
            {
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
            if (dist > remainingDistance)
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
                rotationPitch = -MathF.Atan2(oscillation, dist);
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
            if (deltaDistance > remainingDistance)
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
            if (movementXYPlane < 0.15f && maxDistFac < 0.2f)
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
            if (targetObj != null)
            {
                var targetRadius = targetObj.GetRadius();
                if (distToTarget <= targetRadius)
                {
                    GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                    DestroyProjectile(projectile);
                    return false;
                }
            }
            else if (distToTarget <= locXY.INCH_PER_SUBTILE)
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
            if (projectile.GetLocation() == targetLoc.location && target == null)
            {
                GameSystems.D20.Actions.ProjectileHit(projectile, critter);
                DestroyProjectile(projectile);
                return;
            }

            float distanceToTargetFt;
            if (target != null)
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
            if (traveledDistanceFt > 25.0 * distanceToTargetFt)
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
                    slot.flags &= ~(AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7);
                    slot.ClearPath();
                    return false;
                }


                nextPathNodeLoc = slot.path.GetNextNode();
                if (!nextPathNodeLoc.HasValue)
                {
                    slot.flags &= ~(AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7);
                    slot.ClearPath();
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

            slot.flags &= ~(AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7);
            slot.ClearPath();
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
            var partSys = GameSystems.ParticleSys.CreateAtObj("sp-fireball-proj", pOut);
            pOut.SetInt32(obj_f.projectile_part_sys_id, (int) partSys); // TODO: This will break hard
            GameSystems.MapObject.SetFlags(pOut, ObjectFlag.DONTDRAW);

            return true;
        }

        [TempleDllLocation(0x1000D3A0)]
        private static bool IsObstacleInDirection(GameObjectBody sourceObj, locXY src, locXY tgt,
            CompassDirection direction)
        {
            MapObjectSystem.ObstacleFlag flags = 0;
            if (sourceObj.GetSpellFlags().HasFlag(SpellFlag.POLYMORPHED))
                flags |= MapObjectSystem.ObstacleFlag.UNK_1 | MapObjectSystem.ObstacleFlag.UNK_2;
            if (!GameSystems.Critter.CanOpenPortals(sourceObj))
                flags |= MapObjectSystem.ObstacleFlag.UNK_1;
            flags |= MapObjectSystem.ObstacleFlag.UNK_2;

            if (GameSystems.Tile.IsBlockingOldVersion(tgt))
            {
                return true;
            }

            var loc = new LocAndOffsets(src);
            return GameSystems.MapObject.HasBlockingObjectInDir(sourceObj, loc, direction, flags);
        }

        [TempleDllLocation(0x10017170)]
        public static bool GoalPleaseMove(AnimSlot slot)
        {
            Span<locXY> adjacentTiles = stackalloc locXY[8];
            GameObjectBody[] handles = new GameObjectBody[8];

            var sourceObj = slot.param1.obj;
            AssertAnimParam(sourceObj != null); // sourceObj

            if (slot.param2.location.location != locXY.Zero)
            {
                GameSystems.Anim.customDelayInMs = 0;
                return true;
            }

            var sourceObjLoc = sourceObj.GetLocation();

            var targetObj = slot.pCurrentGoal.target.obj;
            locXY targetLoc;
            if (targetObj == null)
            {
                targetLoc = locXY.Zero;
            }
            else
            {
                targetLoc = targetObj.GetLocation();
            }

            var preferredDir = GameSystems.Random.GetInt(0, 8);
            for (var i = preferredDir; i < 8; i++)
            {
                var dir = (CompassDirection) i;
                var adjacentTile = sourceObjLoc.Offset(dir);
                adjacentTiles[i] = adjacentTile;
                handles[i] = null;
                if (!IsObstacleInDirection(sourceObj, sourceObjLoc, adjacentTile, dir))
                {
                    slot.pCurrentGoal.targetTile.location = new LocAndOffsets(adjacentTile);
                    return true;
                }
            }

            for (var i = 0; i < preferredDir; i++)
            {
                var dir = (CompassDirection) i;
                var adjacentTile = sourceObjLoc.Offset(dir);
                adjacentTiles[i] = adjacentTile;
                handles[i] = null;
                if (!IsObstacleInDirection(sourceObj, sourceObjLoc, adjacentTile, dir))
                {
                    slot.pCurrentGoal.targetTile.location = new LocAndOffsets(adjacentTile);
                    return true;
                }
            }

            for (var i = preferredDir; i < 8; i++)
            {
                // TODO: This can NEVER be true since handles[i] is always null
                if (adjacentTiles[i] != targetLoc && GameSystems.Anim.PushPleaseMove(sourceObj, handles[i]))
                {
                    slot.pCurrentGoal.targetTile.location = new LocAndOffsets(adjacentTiles[i]);
                    GameSystems.Anim.customDelayInMs = 1000;
                    return true;
                }
            }

            for (var i = 0; i < preferredDir; i++)
            {
                // TODO: This can NEVER be true since handles[i] is always null
                if (adjacentTiles[i] != targetLoc && GameSystems.Anim.PushPleaseMove(sourceObj, handles[i]))
                {
                    slot.pCurrentGoal.targetTile.location = new LocAndOffsets(adjacentTiles[i]);
                    GameSystems.Anim.customDelayInMs = 1000;
                    return true;
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
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); // obj != OBJ_HANDLE_NULL

            if (obj == null)
            {
                return false;
            }

            var objLoc = obj.GetLocation();
            var range = slot.pCurrentGoal.animId.number;
            var x = slot.pCurrentGoal.scratchVal1.number;
            var y = slot.pCurrentGoal.scratchVal2.number;
            var randomX = GameSystems.Random.GetInt(-range, range);
            var randomY = GameSystems.Random.GetInt(-range, range);
            slot.field_14 = slot.currentGoal + 1;
            var randomLoc = new locXY(
                x + randomX,
                y + randomY
            );
            var animPathQuery = new AnimPathData();
            animPathQuery.handle = obj;
            animPathQuery.srcLoc = objLoc;
            animPathQuery.destLoc = randomLoc;
            animPathQuery.size = anim_create_path_max_length(ref slot.animPath, objLoc, randomLoc, obj);
            animPathQuery.deltas = slot.animPath.deltas.AsSpan();
            animPathQuery.flags = 0;
            if (AnimPathSpecInit(ref animPathQuery))
                slot.animPath.deltaIdxMax = AnimPathSearch(ref animPathQuery);
            else
                slot.animPath.deltaIdxMax = 0;
            var deltaIdxMax = slot.animPath.deltaIdxMax;
            if (deltaIdxMax <= 0 || deltaIdxMax > range)
            {
                // Retry with flag=1
                animPathQuery.flags = AnimPathDataFlags.UNK_1;
                if (!AnimPathSpecInit(ref animPathQuery))
                {
                    AiEndCombatTurn(obj);
                    return false;
                }

                slot.animPath.deltaIdxMax = AnimPathSearch(ref animPathQuery);
                if (slot.animPath.deltaIdxMax <= 0 || slot.animPath.deltaIdxMax > range)
                {
                    AiEndCombatTurn(obj);
                    return false;
                }
            }

            slot.animPath.flags &= ~AnimPathFlag.UNK_2;
            slot.animPath.objLoc = animPathQuery.srcLoc;
            slot.animPath.tgtLoc = animPathQuery.destLoc;
            slot.animPath.fieldD4 = 0;
            slot.pCurrentGoal.targetTile.location = new LocAndOffsets(randomLoc);
            return true;
        }


        [TempleDllLocation(0x10017810)]
        public static bool GoalWanderSeekDarkness(AnimSlot slot)
        {
            // TODO: This previously called a non-functional color-related method
            // which is most likely an Arkanum left-over
            return GoalWander(slot);
        }

        [TempleDllLocation(0x10017b30)]
        public static bool GoalIsDoorFullyClosed(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/

            if (!obj.IsOffOrDestroyed && !obj.IsPortalOpen())
            {
                // Ensure the door is not currently opening or closing
                // TODO: Try to use IsIdleOrFidet on AnimSystem

                var doorSlot = GameSystems.Anim.GetSlot(obj);
                if (doorSlot == null)
                {
                    return true;
                }

                var goalType = slot.goals[slot.currentGoal].goalType;
                return (goalType == AnimGoalType.anim_idle
                        || goalType == AnimGoalType.anim_fidget
                        || goalType == AnimGoalType.animate_loop);
            }

            return false;
        }

        [TempleDllLocation(0x100166f0)]
        private static void PlayWaterRipples(GameObjectBody obj)
        {
            if (obj.IsCritter())
            {
                var objLoc = obj.GetLocationFull();
                var depth = GameSystems.Height.GetDepth(objLoc);
                if (depth != 0)
                {
                    string effectName;
                    switch (GameSystems.Stat.DispatchGetSizeCategory(obj))
                    {
                        case SizeCategory.Fine:
                        case SizeCategory.Diminutive:
                        case SizeCategory.Tiny:
                            effectName = "ef-ripples-tiny";
                            break;
                        case SizeCategory.Small:
                            effectName = "ef-ripples-small";
                            break;
                        case SizeCategory.Large:
                            effectName = "ef-ripples-large";
                            break;
                        case SizeCategory.Huge:
                        case SizeCategory.Gargantuan:
                        case SizeCategory.Colossal:
                            effectName = "ef-ripples-huge";
                            break;
                        default:
                            effectName = "ef-ripples-med";
                            break;
                    }

                    GameSystems.ParticleSys.CreateAtObj(effectName, obj);
                }
            }
        }

        [TempleDllLocation(0x10017dd0)]
        public static bool GoalTriggerSpell(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            var spellId = slot.param2.spellId;

            var animId = obj.GetOrCreateAnimHandle().GetAnimId();

            if (obj.GetSpellFlags().HasFlag(SpellFlag.STONED))
            {
                return false;
            }

            if (IsStonedStunnedOrParalyzed(obj) && (animId.IsSpecialAnim()
                                                    || animId.GetNormalAnimType() != NormalAnimType.Death
                                                    && animId.GetNormalAnimType() != NormalAnimType.Death2
                                                    && animId.GetNormalAnimType() != NormalAnimType.Death3))
            {
                return false;
            }

            if (obj.IsCritter() && slot.pCurrentGoal.goalType == AnimGoalType.dying)
            {
                var soundId = GameSystems.SoundMap.GetCritterSoundEffect(obj, CritterSoundEffect.Death);
                if (soundId != -1)
                {
                    GameSystems.SoundGame.PositionalSound(soundId, 1, obj);
                }
            }

            var previousAnim = slot.pCurrentGoal.animIdPrevious.number;
            EncodedAnimId newAnim;
            if (previousAnim == -1)
                newAnim = new EncodedAnimId(NormalAnimType.AbjurationCasting);
            else
                newAnim = new EncodedAnimId(previousAnim -
                                            64); // TODO: This is most likely wrong since it's stored unmodified
            obj.SetAnimId(newAnim);

            PlayWaterRipples(obj);
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            if (spellId != 0)
            {
                GameSystems.Spell.IdentifySpellCast(spellId);
                GameSystems.Script.ExecuteSpellScript(spellId, SpellEvent.BeginSpellCast);
            }

            slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }

        [TempleDllLocation(0x10017f80)]
        public static bool GoalUnconcealCleanup(AnimSlot slot)
        {
            var obj = slot.param1.obj;

            AssertAnimParam(obj != null);
            Logger.Debug(" endUnconceal");

            if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap())
            {
                return false;
            }
            else
            {
                if (obj.IsCritter())
                {
                    var leader = GameSystems.Critter.GetLeader(obj);
                    if (leader == null || !GameSystems.Critter.IsConcealed(leader))
                    {
                        GameSystems.Critter.SetConcealed(obj, false);
                    }
                }

                obj.SetAnimId(obj.GetIdleAnimId());
                return true;
            }
        }

        [TempleDllLocation(0x10018050)]
        public static bool GoalResetToIdleAnim(AnimSlot slot)
        {
            if (GameSystems.Map.IsClearingMap())
            {
                return true;
            }
            else
            {
                var obj = slot.param1.obj;
                AssertAnimParam(obj != null);
                if (GameSystems.Critter.IsProne(obj))
                {
                    return false;
                }

                var handle = obj.GetOrCreateAnimHandle();
                AssertAnimParam(handle != null);

                if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap())
                {
                    return false;
                }
                else
                {
                    if (obj.IsCritter())
                    {
                        var animId = handle.GetAnimId();
                        if (!GameSystems.Critter.IsMovingSilently(obj))
                        {
                            animId = obj.GetIdleAnimId();
                        }

                        obj.SetAnimId(animId);
                    }

                    return true;
                }
            }
        }

        [TempleDllLocation(0x10018160)]
        public static bool GoalResetToIdleAnimUnstun(AnimSlot slot)
        {
            if (GameSystems.Map.IsClearingMap())
            {
                return true;
            }
            else
            {
                var obj = slot.param1.obj;
                AssertAnimParam(obj != null);
                if (GameSystems.Critter.IsProne(obj))
                {
                    return false;
                }

                var handle = obj.GetOrCreateAnimHandle();
                AssertAnimParam(handle != null);

                if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap())
                {
                    return false;
                }
                else
                {
                    if (obj.IsCritter())
                    {
                        var animId = handle.GetAnimId();
                        if (!GameSystems.Critter.IsMovingSilently(obj))
                        {
                            animId = obj.GetIdleAnimId();
                        }

                        obj.SetAnimId(animId);
                        // This previously loaded an int @ 10307554, which seemingly was always 0
                        GameSystems.MapObject.MoveOffsets(obj, 0.0f, 0.0f);
                        var critterFlags = obj.GetCritterFlags();
                        obj.SetCritterFlags(critterFlags & ~CritterFlag.STUNNED);
                    }

                    return true;
                }
            }
        }


        [TempleDllLocation(0x10018290)]
        public static bool GoalThrowItemCleanup(AnimSlot slot)
        {
            if (GameSystems.Map.IsClearingMap())
            {
                return true;
            }
            else
            {
                var obj = slot.param1.obj;
                AssertAnimParam(obj != null);
                if (GameSystems.Critter.IsProne(obj))
                {
                    return false;
                }

                var handle = obj.GetOrCreateAnimHandle();
                AssertAnimParam(handle != null);

                if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap())
                {
                    return false;
                }
                else
                {
                    if (obj.IsCritter())
                    {
                        var animId = handle.GetAnimId();
                        if (!GameSystems.Critter.IsMovingSilently(obj))
                        {
                            animId = obj.GetIdleAnimId();
                        }

                        obj.SetAnimId(animId);
                        // This previously loaded an int @ 10307554, which seemingly was always 0
                        GameSystems.MapObject.MoveOffsets(obj, 0.0f, 0.0f);

                        var scratchObj = slot.pCurrentGoal.scratch.obj;
                        if (scratchObj != null)
                        {
                            // Attempt putting it into the char's bag, otherwise place at their feet
                            var err = GameSystems.Item.ItemTransferWithFlags(scratchObj, obj, -1,
                                ItemInsertFlag.Unk4, null);
                            if (err == ItemErrorCode.OK)
                            {
                                return true;
                            }

                            GameSystems.Item.MoveItemClearNoTransfer(scratchObj, obj.GetLocation());
                        }
                    }

                    return true;
                }
            }
        }


        [TempleDllLocation(0x10018400)]
        public static bool GoalThrowItemPlayAnim(AnimSlot slot)
        {
            var v1 = slot;
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var animModel = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animModel != null); /*handle != OBJ_HANDLE_NULL*/

            var animId = new EncodedAnimId(v1.param2.number);
            if (animId == -1)
            {
                animId = animModel.GetAnimId();
            }

            obj.SetAnimId(animId);

            if (IsStonedStunnedOrParalyzed(obj))
            {
                // TODO: This branch does not seem to make a lot of sense here
                animId = animModel.GetAnimId();
                if (animId.IsSpecialAnim()
                    || animId.GetNormalAnimType() != NormalAnimType.Death
                    && animId.GetNormalAnimType() != NormalAnimType.Death2
                    && animId.GetNormalAnimType() != NormalAnimType.Death3)
                {
                    return false;
                }

                if (slot.pCurrentGoal.goalType == AnimGoalType.dying)
                {
                    var soundId = GameSystems.SoundMap.GetCritterSoundEffect(obj, CritterSoundEffect.Death);
                    if (soundId != -1)
                    {
                        GameSystems.SoundGame.PositionalSound(soundId, 1, obj);
                    }
                }
            }

            v1.path.someDelay = 33;
            v1.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(obj);
            slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x10018730)]
        public static bool GoalStartIdleAnimIfCloseToParty(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            var distance = GameSystems.Party.DistanceToParty(obj);
            if (distance > 60 || IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }
            else
            {
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                obj.SetAnimId(obj.GetIdleAnimId());
                slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
        }

        [TempleDllLocation(0x10018810)]
        public static bool GoalStartFidgetAnimIfCloseToParty(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var distance = GameSystems.Party.DistanceToParty(obj);
            if (distance > 60 || IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }
            else
            {
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                obj.SetAnimId(obj.GetFidgetAnimId());
                PlayWaterRipples(obj);
                slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
        }

        [TempleDllLocation(0x100188f0)]
        public static bool GoalContinueWithAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null); /*handle != AAS_HANDLE_NULL*/
            ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }
            else
            {
                return true;
            }
        }


        [TempleDllLocation(0x100189b0)]
        public static bool GoalContinueWithAnim2(AnimSlot slot)
        {
            var v2 = slot.param1.obj;
            AssertAnimParam(v2 != null);
            var v3 = v2.GetOrCreateAnimHandle();
            AssertAnimParam(v3 != null); /*handle != AAS_HANDLE_NULL*/
            ContinueWithAnimation(v2, slot, v3, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }
            else
            {
                return true;
            }
        }


        [TempleDllLocation(0x10018a70)]
        public static bool GoalPlayDoorOpenAnim(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null); /*door != OBJ_HANDLE_NULL*/

            var portalFlags = door.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.OPEN))
            {
                return false;
            }

            door.SetAnimId(new EncodedAnimId(NormalAnimType.Open));
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(door);

            var soundId = GameSystems.SoundMap.GetPortalSoundEffect(door, PortalSoundEffect.Open);
            GameSystems.SoundGame.PositionalSound(soundId, 1, door);

            if (!portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                door.SetPortalFlags(portalFlags | PortalFlag.OPEN);
                GameSystems.MapFogging.UpdateLineOfSight();
            }

            slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x10018b90)]
        public static bool GoalContinueWithDoorOpenAnim(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null);

            var animHandle = door.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null); /*handle != AAS_HANDLE_NULL*/
            ContinueWithAnimation(door, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
                return true;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }

            return true;
        }


        [TempleDllLocation(0x10018c50)]
        public static bool GoalPlayDoorCloseAnim(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null);
            door.SetAnimId(new EncodedAnimId(NormalAnimType.Close));
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(door);

            var soundId = GameSystems.SoundMap.GetPortalSoundEffect(door, PortalSoundEffect.Open);
            GameSystems.SoundGame.PositionalSound(soundId, 1, door);
            slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }


        [TempleDllLocation(0x10018d40)]
        public static bool GoalContinueWithDoorCloseAnim(AnimSlot slot)
        {
            var door = slot.param1.obj;
            AssertAnimParam(door != null);
            var animHandle = door.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);
            ContinueWithAnimation(door, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
                return true;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }

            return true;
        }


        [TempleDllLocation(0x10018e00)]
        public static bool GoalPickLockPlayPushDoorOpenAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            AssertAnimParam(obj.IsCritter());

            if (obj.IsCritter())
            {
                var paramAnimId = slot.param2.number;
                EncodedAnimId animId;
                if (paramAnimId != -1)
                {
                    // TODO: Encoded anim ids seem wonky
                    animId = new EncodedAnimId((NormalAnimType) (paramAnimId - 64));
                }
                else
                {
                    animId = new EncodedAnimId(NormalAnimType.PicklockConcentrated);
                }

                obj.SetAnimId(animId);

                PlayWaterRipples(obj);
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
            else
            {
                return false;
            }
        }


        [TempleDllLocation(0x10018ee0)]
        public static bool GoalPickLockContinueWithAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);

            ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
                return true;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }

            return true;
        }


        [TempleDllLocation(0x10018fa0)]
        public static bool GoalDyingPlaySoundAndRipples(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            AssertAnimParam(obj.IsCritter());
            if (obj.IsCritter())
            {
                var soundId = GameSystems.SoundMap.GetCritterSoundEffect(obj, CritterSoundEffect.Death);
                GameSystems.SoundGame.PositionalSound(soundId, 1, obj);
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                PlayWaterRipples(obj);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x10019070)]
        public static bool GoalDyingContinueAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null); /*handle != AAS_HANDLE_NULL*/
            ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
                slot.flags |= AnimSlotFlag.UNK3;
            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }
            else
            {
                return true;
            }
        }

        [TempleDllLocation(0x10019130)]
        public static bool GoalAnimateFireDmgContinueAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);
            if (obj.GetSpellFlags().HasFlag(SpellFlag.STONED))
            {
                return false;
            }
            else
            {
                ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
                if (eventOut.IsEnd)
                {
                    slot.flags &= ~AnimSlotFlag.UNK3;
                }

                if (eventOut.IsAction)
                {
                    slot.flags |= AnimSlotFlag.UNK3;
                }

                return true;
            }
        }


        [TempleDllLocation(0x100191f0)]
        public static bool GoalStunnedPlayAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/

            var animId = GameSystems.Critter.GetAnimId(obj, WeaponAnim.Panic);
            if (obj.GetSpellFlags().HasFlag(SpellFlag.STONED)
                || obj.IsCritter() && obj.GetCritterFlags().HasFlag(CritterFlag.PARALYZED)
                                   && (animId.IsSpecialAnim()
                                       || animId.GetNormalAnimType() == NormalAnimType.Death
                                       && animId.GetNormalAnimType() == NormalAnimType.Death2
                                       && animId.GetNormalAnimType() == NormalAnimType.Death3))
            {
                return false;
            }
            else
            {
                obj.SetAnimId(animId);
                PlayWaterRipples(obj);
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
        }

        [TempleDllLocation(0x10019330)]
        public static bool GoalStunnedContinueAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);

            if (obj.GetSpellFlags().HasFlag(ObjectFlag.STONED))
            {
                return false;
            }

            var animHandle = obj.GetOrCreateAnimHandle();
            if (obj.IsCritter())
            {
                if (obj.GetCritterFlags().HasFlag(CritterFlag.PARALYZED))
                {
                    var animId = animHandle.GetAnimId();
                    if (animId.IsSpecialAnim()
                        || animId.GetNormalAnimType() == NormalAnimType.Death
                        && animId.GetNormalAnimType() == NormalAnimType.Death2
                        && animId.GetNormalAnimType() == NormalAnimType.Death3)
                    {
                        return false;
                    }
                }
            }

            ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
                slot.flags |= AnimSlotFlag.UNK3;
            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }
            else
            {
                return true;
            }
        }


        [TempleDllLocation(0x10019470)]
        public static bool GoalPlayGetUpAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            if (obj.GetSpellFlags().HasFlag(SpellFlag.STONED)
                || obj.IsCritter() && obj.GetCritterFlags().HasFlag(CritterFlag.PARALYZED))
            {
                return false;
            }
            else
            {
                obj.SetAnimId(new EncodedAnimId(NormalAnimType.Getup));
                PlayWaterRipples(obj);
                slot.path.someDelay = 33;
                slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
                slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }
        }


        [TempleDllLocation(0x10019540)]
        public static bool GoalPlayUnconcealAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            obj.SetAnimId(new EncodedAnimId(NormalAnimType.Unconceal));
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(obj);
            GameSystems.ObjFade.FadeTo(obj, 255, 50, 51, 0);
            slot.flags &= ~(AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }

        [TempleDllLocation(0x10013140)]
        private static EncodedAnimId AnimGetMoveAnimationId(AnimSlot slot, GameObjectBody critter,
            bool requestRunning)
        {
            if (GameSystems.Critter.IsMovingSilently(critter))
            {
                return GameSystems.Critter.GetAnimId(critter, WeaponAnim.Sneak);
            }
            else
            {
                if (slot.currentGoal > 0 && requestRunning)
                {
                    return GameSystems.Critter.GetAnimId(critter, WeaponAnim.Run);
                }

                return GameSystems.Critter.GetAnimId(critter, WeaponAnim.Walk);
            }
        }

        [TempleDllLocation(0x10019630)]
        public static bool GoalPlayMoveAnim(AnimSlot slot)
        {
            if (slot.flags.HasFlag(AnimSlotFlag.UNK1))
            {
                return false;
            }

            if (slot.flags.HasFlag(AnimSlotFlag.UNK7))
            {
                slot.flags |= AnimSlotFlag.UNK5;
                return true;
            }

            var obj = slot.param1.obj;

            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);
            if (IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            slot.pCurrentGoal.animData.number = animHandle.GetAnimId();
            var running = slot.flags.HasFlag(AnimSlotFlag.RUNNING);
            var animId = AnimGetMoveAnimationId(slot, obj, running);

            obj.SetAnimId(animId);
            slot.path.someDelay = 16;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(obj);
            slot.path.someDelay = 33;
            if (obj.GetSpellFlags().HasFlag(SpellFlag.SHRUNK))
            {
                slot.path.someDelay *= 2;
            }

            slot.animPath.flags &= ~(AnimPathFlag.UNK_1 | AnimPathFlag.UNK_2);
            slot.animPath.fieldD4 = 0;
            slot.flags |= AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7;
            return true;
        }


        [TempleDllLocation(0x10019920)]
        public static bool GoalPlayWaterRipples(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            PlayWaterRipples(obj);
            slot.animPath.flags &= ~AnimPathFlag.UNK_1;
            slot.animPath.fieldD4 = 0;
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }

        [TempleDllLocation(0x100199b0)]
        public static bool GoalContinueMoveStraight(AnimSlot slot)
        {
            // TODO: Check and hope this is never used

            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);

            var iterations = slot.pCurrentGoal.scratchVal5.number;
            GameSystems.Anim.customDelayInMs = 35;
            if (iterations == 0)
            {
                iterations = 4;
                GameSystems.Anim.customDelayInMs = 35;
            }

            for (var i = 0; i < iterations; i++)
            {
                var objPos = obj.GetLocation();
                var objX = objPos.locx;
                var objY = objPos.locy;
                ContinueWithAnimation(obj, slot, animHandle, out var _);

                var animPathIdx = slot.animPath.fieldD4;
                var pathDeltaX = slot.animPath.deltas[animPathIdx];
                var pathDeltaY = slot.animPath.deltas[animPathIdx + 1];

                var offX = obj.OffsetX + pathDeltaX;
                var offY = obj.OffsetY + pathDeltaY;
                GameSystems.Location.GetTranslation(objX, objY, out var objScreenX, out var objScreenY);
                if (GameSystems.Location.ScreenToLoc(
                    (int) (objScreenX + offX + 20),
                    (int) (objScreenY + offY + 14),
                    out var shiftedObjPos))
                {
                    if (shiftedObjPos.locx != objX || shiftedObjPos.locy != objY)
                    {
                        GameSystems.Location.GetTranslation(shiftedObjPos.locx, shiftedObjPos.locy,
                            out var shiftedScreenX, out var shiftedScreenY);
                        offX += objScreenX - shiftedScreenX;
                        offY += objScreenY - shiftedScreenY;
                    }

                    GameSystems.MapObject.Move(obj, new LocAndOffsets(shiftedObjPos, offX, offY));

                    slot.animPath.fieldD4 += 2;
                    if (slot.animPath.fieldD4 >= slot.animPath.deltaIdxMax)
                    {
                        slot.flags &= ~AnimSlotFlag.UNK5;
                        return false;
                    }
                }
            }

            return true;
        }


        [TempleDllLocation(0x10019c20)]
        public static bool GoalApplyKnockback(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null); /*obj != OBJ_HANDLE_NULL*/
            GameSystems.Anim.customDelayInMs = 35;
            if (obj == null)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }

            for (var i = 0; i < 4; i++)
            {
                var objPos = obj.GetLocation();
                if (GoalKnockbackFindTarget(ref slot.animPath, slot, obj, objPos))
                {
                    GameSystems.MapObject.Move(obj, new LocAndOffsets(objPos));
                    return false;
                }

                slot.animPath.fieldD4 += 2;
                if (slot.animPath.fieldD4 >= slot.animPath.deltaIdxMax)
                {
                    slot.flags &= ~AnimSlotFlag.UNK5;
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x10013f60)]
        private static bool GoalKnockbackFindTarget(ref AnimPath animPath, AnimSlot slot,
            GameObjectBody obj, locXY curLoc)
        {
            if (!animPath.flags.HasFlag(AnimPathFlag.UNK_1))
            {
                var newLoc = curLoc.Offset(animPath.fieldD0);
                if (animPath.fieldD4 > animPath.deltaIdxMax - 2)
                {
                    using var critters = ObjList.ListTile(newLoc, ObjectListFilter.OLC_CRITTERS);
                    foreach (var critter in critters)
                    {
                        if (!GameSystems.Critter.IsDeadNullDestroyed(critter))
                        {
                            slot.pCurrentGoal.scratch.obj = critter;
                            return true;
                        }
                    }
                }

                if (!GameSystems.Tile.IsBlockingOldVersion(newLoc))
                {
                    var loc = new LocAndOffsets(curLoc);
                    if (!GameSystems.MapObject.HasBlockingObjectInDir(obj, loc, animPath.fieldD0,
                        MapObjectSystem.ObstacleFlag.UNK_1 | MapObjectSystem.ObstacleFlag.UNK_2))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10019e10)]
        public static bool GoalDyingReturnTrue(AnimSlot slot)
        {
            if (slot.param1.obj != null)
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10019e70)]
        public static bool GoalAttemptMoveCleanup(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            if (slot.flags.HasFlag(AnimSlotFlag.UNK11) || GameSystems.Map.IsClearingMap() || obj == null)
            {
                return false;
            }

            if (slot.field_14 == slot.currentGoal)
            {
                slot.animPath.flags |= AnimPathFlag.UNK_1;
            }

            if (slot.field_14 == slot.currentGoal - 1)
            {
                slot.animPath.flags |= AnimPathFlag.UNK_1;
            }

            obj.SetAnimId(obj.GetIdleAnimId());
            return true;
        }

        [TempleDllLocation(0x10019f00)]
        public static bool GoalAttackPlayWeaponHitEffect(AnimSlot slot)
        {
            var sourceObj = slot.param1.obj;
            var targetObj = slot.param2.obj;

            AssertAnimParam(sourceObj != null);
            AssertAnimParam(targetObj != null);
            if (IsStonedStunnedOrParalyzed(sourceObj))
            {
                return false;
            }

            WeaponAnim weaponAnim;
            if (slot.pCurrentGoal.animIdPrevious.number != -1)
            {
                weaponAnim = WeaponAnim.RightAttack;
            }
            else
            {
                weaponAnim = (WeaponAnim) slot.pCurrentGoal.animIdPrevious.number;
            }

            var animId = GameSystems.Critter.GetAnimId(sourceObj, weaponAnim);
            sourceObj.SetAnimId(animId);

            // TODO This seems the wrong way around!
            GameObjectBody weapon;
            if (weaponAnim < WeaponAnim.LeftAttack || weaponAnim > WeaponAnim.LeftAttack3)
            {
                weapon = GameSystems.Item.ItemWornAt(sourceObj, EquipSlot.WeaponPrimary);
            }
            else
            {
                weapon = GameSystems.Item.ItemWornAt(sourceObj, EquipSlot.WeaponSecondary);
            }

            var soundId = GameSystems.SoundMap.CombatFindWeaponSound(weapon, sourceObj, targetObj, 4);
            GameSystems.SoundGame.PositionalSound(soundId, 1, sourceObj);

            PlayWaterRipples(sourceObj);
            slot.path.someDelay = 33;
            slot.gametimeSth = GameSystems.TimeEvent.AnimTime;
            slot.flags |= AnimSlotFlag.UNK5;
            return true;
        }

        [TempleDllLocation(0x1001a080)]
        public static bool GoalAttackContinueWithAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            var animHandle = obj.GetOrCreateAnimHandle();
            AssertAnimParam(animHandle != null);

            if (IsStonedStunnedOrParalyzed(obj))
            {
                return false;
            }

            ContinueWithAnimation(obj, slot, animHandle, out var eventOut);
            if (eventOut.IsAction)
            {
                slot.flags |= AnimSlotFlag.UNK3;
            }

            if (eventOut.IsEnd)
            {
                slot.flags &= ~AnimSlotFlag.UNK5;
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1001a170)]
        public static bool GoalAttackPlayIdleAnim(AnimSlot slot)
        {
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            obj.SetAnimId(obj.GetIdleAnimId());
            return true;
        }

        [TempleDllLocation(0x1001bf70)]
        public static bool GoalMoveNearUpdateRadiusToReach(AnimSlot slot)
        {
            var handle = slot.param1.obj;
            var targetObj = slot.param2.obj;
            var targetLoc = locXY.Zero;
            if (targetObj != null)
            {
                targetLoc = targetObj.GetLocation();
                var currentGoal = slot.pCurrentGoal;
                if (currentGoal.targetTile.location.location != targetLoc)
                {
                    slot.animPath.flags |= AnimPathFlag.UNK_4;
                }
            }

            if (!slot.animPath.flags.HasFlag(AnimPathFlag.UNK_4) && targetLoc != locXY.Zero)
            {
                if (GoalMoveCanReachTarget(handle, targetObj))
                    return false;
                if (GameSystems.AI.FindObstacleObj(handle, targetLoc, out var objOut) < 26
                    && (objOut == null || objOut == targetObj))
                {
                    var mainWeapon = GameSystems.Combat.GetMainHandWeapon(handle);
                    if (mainWeapon != null)
                    {
                        var reachWithWeapon = GameSystems.Item.GetReachWithWeapon(mainWeapon, handle);
                        for (int i = 0; i < slot.currentGoal; i++)
                        {
                            slot.goals[i].animId.number = reachWithWeapon;
                        }

                        slot.animPath.flags |= AnimPathFlag.UNK_4;
                    }
                }
            }

            if (!slot.animPath.flags.HasFlag(AnimPathFlag.UNK_8) && !slot.animPath.flags.HasFlag(AnimPathFlag.UNK_4))
            {
                return false;
            }

            sub_10014DC0(slot);
            if (slot.currentGoal > 0)
            {
                slot.goals[slot.currentGoal].scratchVal4 = slot.pCurrentGoal.scratchVal4;
            }

            slot.animPath.flags &= ~AnimPathFlag.UNK_4;
            slot.animPath.flags |= AnimPathFlag.UNK_1;
            slot.flags &= ~(AnimSlotFlag.UNK5 | AnimSlotFlag.UNK7);
            if (slot.animPath.flags.HasFlag(AnimPathFlag.UNK_8))
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
            }

            return true;
        }

        private static void sub_10014DC0(AnimSlot slot)
        {
            // TODO: This sub likely does nothing and is an Arkanum leftover
            var obj = slot.param1.obj;
            AssertAnimParam(obj != null);
            if (slot.currentGoal > 0)
            {
                if (slot.flags.HasFlag(AnimSlotFlag.RUNNING))
                {
                    if (GameSystems.Combat.IsCombatModeActive(obj))
                    {
                        var maxCount = 5;
                        if (obj.GetCritterFlags2().HasFlag(CritterFlag2.FATIGUE_DRAINING))
                        {
                            maxCount = 1;
                        }

                        if (++slot.pCurrentGoal.scratchVal4.number >= maxCount)
                        {
                            slot.pCurrentGoal.scratchVal4.number = 0;
                        }
                    }
                }
                else
                {
                    slot.flags &= ~AnimSlotFlag.RUNNING;
                }
            }
        }

        [TempleDllLocation(0x10017bf0)]
        private static bool GoalMoveCanReachTarget(GameObjectBody source, GameObjectBody target)
        {
            AssertAnimParam(source != null);
            AssertAnimParam(target != null);

            var result = false;
            if (source.IsCritter() && GameSystems.Critter.IsDeadOrUnconscious(source)
                || target.IsCritter() && GameSystems.Critter.IsDeadOrUnconscious(target))
            {
                return false;
            }

            if (source.GetSpellFlags().HasFlag(SpellFlag.BODY_OF_AIR) &&
                !source.GetCritterFlags2().HasFlag(CritterFlag2.ELEMENTAL))
            {
                return false;
            }

            var v5 = target.GetLocation();

            GameSystems.AI.FindObstacleObj(source, v5, out var objOut);

            if (objOut == null || objOut == target)
            {
                return true;
            }

            var path = new AnimPath();
            path.flags = 0;
            path.maxPathLength = 0;
            path.fieldE4 = 0;
            path.fieldD4 = 0;
            path.deltaIdxMax = 0;
            path.fieldD0 = 0;
            return AnimCheckTgtPathMaker(source, v5, ref path);
        }

        [TempleDllLocation(0x10017ad0)]
        private static bool AnimCheckTgtPathMaker(GameObjectBody handle, locXY tgtLoc, ref AnimPath path)
        {
            var fromLoc = handle.GetLocation();
            return AnimCheckTgtPathMaker_Impl(handle, fromLoc, tgtLoc, ref path);
        }

        [TempleDllLocation(0x1000d980)]
        private static bool AnimCheckTgtPathMaker_Impl(GameObjectBody handle, locXY srcLoc, locXY tgtLoc,
            ref AnimPath animPath)
        {
            AssertAnimParam(handle != null);
            AnimPathDataFlags flags = AnimPathDataFlags.UNK_1;
            var maxPathLength = anim_create_path_max_length(ref animPath, srcLoc, tgtLoc, handle);
            if (handle.GetSpellFlags().HasFlag(SpellFlag.POLYMORPHED))
                flags |= AnimPathDataFlags.CantOpenDoors | AnimPathDataFlags.UNK_4;
            if (!GameSystems.Critter.CanOpenPortals(handle))
                flags |= AnimPathDataFlags.CantOpenDoors;
            flags |= AnimPathDataFlags.UNK_4;
            if (GameSystems.Critter.IsMovingSilently(handle))
                flags |= AnimPathDataFlags.MovingSilently;

            var pathData = new AnimPathData();
            pathData.srcLoc = srcLoc;
            pathData.destLoc = tgtLoc;
            pathData.flags = flags;
            pathData.handle = handle;
            pathData.size = maxPathLength;
            pathData.deltas = animPath.deltas.AsSpan();
            if (AnimPathSpecInit(ref pathData))
                animPath.deltaIdxMax = AnimPathSearch(ref pathData);
            else
                animPath.deltaIdxMax = 0;
            if (animPath.deltaIdxMax > 0)
            {
                animPath.flags &= ~(AnimPathFlag.UNK_1 | AnimPathFlag.UNK_2);
                animPath.objLoc = pathData.srcLoc;
                animPath.tgtLoc = pathData.destLoc;
                animPath.fieldD4 = 0;
                return true;
            }
            else
            {
                return false;
            }
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