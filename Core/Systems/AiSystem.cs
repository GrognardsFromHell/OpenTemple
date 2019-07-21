using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public delegate void AiCancelDialog(GameObjectBody critter);

    public delegate void AiShowTextBubble(GameObjectBody critter, GameObjectBody speakingTo,
        string text, int speechId);

    public enum AiFightStatus {
        NONE = 0,
        FIGHTING = 1,
        FLEEING =2,
        SURRENDERED = 3,
        FINDING_HELP = 4,
        BEING_DRAWN = 5 // New TODO for Harpy Song
    }

    public class AiSystem : IGameSystem, IModuleAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public AiSystem()
        {
            Stub.TODO();
        }

        public void Dispose()
        {
            Stub.TODO();
        }

        public void LoadModule()
        {
            Stub.TODO();
        }

        public void UnloadModule()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1005d5e0)]
        public void AddAiTimer(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10AA4BC8)]
        private AiCancelDialog _cancelDialog;

        [TempleDllLocation(0x10AA73B0)]
        private AiShowTextBubble _showTextBubble;

        [TempleDllLocation(0x10056ef0)]
        public void SetDialogFunctions(AiCancelDialog cancelDialog, AiShowTextBubble showTextBubble)
        {
            _cancelDialog = cancelDialog;
            _showTextBubble = showTextBubble;
        }

        [TempleDllLocation(0x1005be60)]
        public void AddOrReplaceAiTimer(GameObjectBody obj, int unknownFlag)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.AI, evt => evt.arg1.handle == obj);

            var newEvt = new TimeEvent(TimeEventType.AI);
            newEvt.arg1.handle = obj;
            newEvt.arg2.int32 = unknownFlag;

            GameSystems.TimeEvent.ScheduleNow(newEvt);
        }

        [TempleDllLocation(0x1005b5f0)]
        public void FollowerAddWithTimeEvent(GameObjectBody obj, bool forceFollower)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100588d0)]
        public void RemoveAiTimer(GameObjectBody obj)
        {
            GameSystems.TimeEvent.Remove(TimeEventType.AI, evt => evt.arg1.handle == obj);
        }

        [TempleDllLocation(0x100e5460)]
        public void OnAddToInitiative(GameObjectBody obj)
        {
            Stub.TODO();
        }

        /// <summary>
        /// Moves back ex-followers to their original intended map and location.
        /// </summary>
        [TempleDllLocation(0x10058620)]
        public void MoveExFollowers()
        {
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (obj.IsNPC())
                {
                    if (obj.GetNPCFlags().HasFlag(NpcFlag.EX_FOLLOWER))
                    {
                        var mapId = GameSystems.Critter.GetTeleportMap(obj);
                        if (GetCurrentStandpoint(obj, out var location))
                        {
                            GameSystems.MapObject.MoveToMap(obj, mapId, new LocAndOffsets(location, 0, 0));
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100584f0)]
        private bool GetCurrentStandpoint(GameObjectBody obj, out locXY location)
        {
            if (GameSystems.Critter.GetLeader(obj) != null)
            {
                location = default;
                return false;
            }

            StandPoint standPoint;
            var v2 = obj.GetNPCFlags();
            if (v2.HasFlag(NpcFlag.USE_ALERTPOINTS))
            {
                if (!GameSystems.Script.GetGlobalFlag(144))
                {
                    GetStandpoint(obj, 0, out standPoint);
                }
                else
                {
                    GetStandpoint(obj, 1, out standPoint);
                }
            }
            else
            {
                if (GameSystems.TimeEvent.IsDaytime)
                {
                    // Daytime standpoint
                    GetStandpoint(obj, 0, out standPoint);
                }
                else
                {
                    // Nighttime standpoint
                    GetStandpoint(obj, 1, out standPoint);
                }
            }

            location = standPoint.location.location;
            return location.locx != 0 || location.locy != 0;
        }

        [TempleDllLocation(0x100ba890)]
        private void GetStandpoint(GameObjectBody obj, int n, out StandPoint standPoint)
        {
            Debugger.Break();

            var standpointArray = obj.GetInt64Array(obj_f.npc_standpoints);

            Span<long> packedStandpoint = stackalloc long[10];

            for (int i = 0; i < 10; i++)
            {
                packedStandpoint[i] = standpointArray[10 * n + i];
            }

            standPoint = MemoryMarshal.Read<StandPoint>(MemoryMarshal.Cast<long, byte>(packedStandpoint));
        }

        [TempleDllLocation(0x1005e8d0)]
        public void ProvokeHostility(GameObjectBody agitator, GameObjectBody provokedNpc, int rangeType, int flags)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005df40)]
        public void SetNoFlee(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        // NO idea why this is in the AI subsystem
        [TempleDllLocation(0x1005bf20)]
        public PortalLockStatus AttemptToOpenDoor(GameObjectBody actor, GameObjectBody portal)
        {
            if (GameSystems.MapObject.IsBusted(portal))
            {
                return PortalLockStatus.PLS_OPEN;
            }

            if (!actor.IsCritter())
            {
                return PortalLockStatus.PLS_INVALID_OPENER;
            }

            if (GameSystems.Script.ExecuteObjectScript(actor, portal, ObjScriptEvent.Unlock) == 0)
            {
                return PortalLockStatus.PLS_DENIED_BY_SCRIPT;
            }

            if (portal.type != ObjectType.portal)
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var portalFlags = portal.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.JAMMED))
            {
                return PortalLockStatus.PLS_JAMMED;
            }

            if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return PortalLockStatus.PLS_MAGICALLY_HELD;
            }

            if (!portalFlags.HasFlag(PortalFlag.ALWAYS_LOCKED))
            {
                if (actor.IsNPC())
                {
                    var leader = GameSystems.Critter.GetLeaderRecursive(actor);
                    if (leader != null)
                    {
                        if (portal.IsPortalOpen())
                        {
                            return PortalLockStatus.PLS_OPEN;
                        }
                    }
                }
                else if (portal.IsPortalOpen())
                {
                    return PortalLockStatus.PLS_OPEN;
                }
            }

            if (!portal.NeedsToBeUnlocked())
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var keyId = portal.GetInt32(obj_f.portal_key_id);
            if (GameSystems.Item.HasKey(actor, keyId))
            {
                GameUiBridge.MarkKeyUsed(keyId, GameSystems.TimeEvent.GameTime);
                return PortalLockStatus.PLS_OPEN;
            }

            if (portal.IsUndetectedSecretDoor())
            {
                return PortalLockStatus.PLS_SECRET_UNDISCOVERED;
            }

            return PortalLockStatus.PLS_LOCKED;
        }

        /**
         * Same as AttemptToOpenDoor but without actually it.
         */
        [TempleDllLocation(0x1005c0a0)]
        public PortalLockStatus DryRunAttemptOpenDoor(GameObjectBody actor, GameObjectBody portal)
        {
            if (GameSystems.MapObject.IsBusted(portal))
            {
                return PortalLockStatus.PLS_OPEN;
            }

            if (!actor.IsCritter())
            {
                return PortalLockStatus.PLS_INVALID_OPENER;
            }

            if (portal.type != ObjectType.portal)
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var portalFlags = portal.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.JAMMED))
            {
                return PortalLockStatus.PLS_JAMMED;
            }

            if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return PortalLockStatus.PLS_MAGICALLY_HELD;
            }

            if (!portalFlags.HasFlag(PortalFlag.ALWAYS_LOCKED))
            {
                if (actor.IsNPC())
                {
                    var leader = GameSystems.Critter.GetLeaderRecursive(actor);
                    if (leader != null)
                    {
                        if (portal.IsPortalOpen())
                        {
                            return PortalLockStatus.PLS_OPEN;
                        }
                    }
                }
                else if (portal.IsPortalOpen())
                {
                    return PortalLockStatus.PLS_OPEN;
                }
            }

            if (!portal.NeedsToBeUnlocked())
            {
                return PortalLockStatus.PLS_OPEN;
            }

            var keyId = portal.GetInt32(obj_f.portal_key_id);
            if (GameSystems.Item.HasKey(actor, keyId))
            {
                return PortalLockStatus.PLS_OPEN;
            }

            return PortalLockStatus.PLS_LOCKED;
        }

        [TempleDllLocation(0x1005a640)]
        public bool ForceSpreadOut(GameObjectBody critter, locXY? optionalLocation = null)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005a170)]
        public int GetTalkingDistance(GameObjectBody critter)
        {
            return critter.IsPC() ? 5 : 10;
        }

        [TempleDllLocation(0x10057790)]
        public void sub_10057790(GameObjectBody actor, GameObjectBody target)
        {
            if (actor.id != target.id)
            {
                _cancelDialog?.Invoke(actor);
                if (!GameSystems.Critter.IsCombatModeActive(actor))
                {
                    GameSystems.Combat.EnterCombat(actor);
                }

                if (!GameSystems.Anim.HasRunSlot(actor) || GameSystems.Anim.IsIdleOrFidgeting(actor))
                {
                    if (Globals.Config.AutoAttack)
                    {
                        if (!GameSystems.Anim.IsRunningGoal(actor, AnimGoalType.attempt_attack, out _))
                        {
                            GameSystems.Anim.PushAttackOther(actor, target);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100583e0)]
        public void ClearWaypointDelay(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                critter.AiFlags &= ~AiFlag.WaypointDelay;
            }
        }

        [TempleDllLocation(0x10058ca0)]
        public int FindObstacleObj(GameObjectBody obj, locXY tgtLoc, out GameObjectBody obstructor)
        {
            obstructor = null;
            var objLoc = obj.GetLocation();

            if (objLoc == tgtLoc)
            {
                return 0;
            }

            var deltas = new sbyte[200];
            var pathLength = GameSystems.PathX.RasterizeLineBetweenLocsScreenspace(objLoc, tgtLoc, deltas);
            if (pathLength <= 0)
            {
                return 100;
            }

            var blockingFlags = MapObjectSystem.ObstacleFlag.UNK_4 | MapObjectSystem.ObstacleFlag.UNK_8;
            int offsetX = (int) obj.OffsetX;
            int offsetY = (int) obj.OffsetY;
            GameSystems.Location.GetTranslation(objLoc.locx, objLoc.locy,
                out var screenX, out var screenY);

            var cost = 0;
            for (var i = 0; i < pathLength; i += 2)
            {
                if (tgtLoc != objLoc)
                {
                    break;
                }

                offsetX += deltas[i];
                offsetY += deltas[i + 1];
                GameSystems.Location.ScreenToLoc(screenX + offsetX + 20, screenY + offsetY + 14, out var locOut);
                if (locOut != objLoc)
                {
                    var dir = objLoc.GetCompassDirection(locOut);
                    if (locOut == tgtLoc)
                    {
                        blockingFlags |= MapObjectSystem.ObstacleFlag.UNK_10;
                    }

                    var preciseObjLoc = new LocAndOffsets(objLoc);
                    cost += GameSystems.MapObject.GetBlockingObjectInDir(null, preciseObjLoc, dir, blockingFlags,
                        out obstructor);
                    if (obstructor != null)
                    {
                        break;
                    }

                    objLoc = locOut;
                }
            }

            return cost;
        }

        // regards facing (rather crudely however)
        [TempleDllLocation(0x10059470)]
        public int HasLineOfSight(GameObjectBody obj, GameObjectBody target)
        {
            var tgt = target;
            if (obj == target)
            {
                return 0;
            }

            if ((obj.GetCritterFlags() & (CritterFlag.SLEEPING | CritterFlag.BLINDED | CritterFlag.STUNNED)) !=
                default)
            {
                return 1000;
            }

            if (GameSystems.Critter.IsDeadOrUnconscious(obj)
                || GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Blinded) != 0
                || GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Is_Invisible) != 0
                && GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible) == 0)
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpellIds.InvisibilityToUndead) != 0
                && GameSystems.Critter.IsUndead(obj))
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpellIds.InvisibilityToAnimals) != 0
                && GameSystems.Critter.IsAnimal(obj))
            {
                return 1000;
            }

            if (!IsFacingTarget(obj, target))
            {
                return 1000;
            }

            using var objIterator = new RaycastPacket();
            objIterator.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound
                                                          | RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
            objIterator.origin = obj.GetLocationFull();
            objIterator.targetLoc = target.GetLocationFull();
            objIterator.sourceObj = obj;
            objIterator.target = target;
            if (!objIterator.TestLineOfSight())
            {
                return 1000;
            }

            int spotCheckResult;
            if (GameSystems.Critter.IsMovingSilently(tgt) || GameSystems.Critter.IsConcealed(tgt))
            {
                // Make opposing hide/spot checks to determine whether the critter can actually see the target
                // Range is factored in below
                var hidePenalty =
                    1 - GameSystems.D20.Actions.dispatch1ESkillLevel(tgt, SkillId.hide, obj, 1);
                spotCheckResult =
                    GameSystems.D20.Actions.dispatch1ESkillLevel(obj, SkillId.spot, target, 1)
                    + hidePenalty;
            }
            else
            {
                // Make a spot check to determine how far we can see
                spotCheckResult = 15 +
                                  GameSystems.D20.Actions.dispatch1ESkillLevel(obj, SkillId.spot, tgt, 1);
                if (spotCheckResult < 15)
                {
                    spotCheckResult = 15;
                }
            }

            if (spotCheckResult < 0)
            {
                spotCheckResult = 0;
            }

            // TODO: This is hot garbage in my opinion because it uses ToEE tiles as a unit of measurement
            // while anything rule related should be in feet.
            var tileDelta = obj.GetLocation().EstimateDistance(target.GetLocation());
            if (tileDelta > 1000)
            {
                return 1000;
            }

            var sightFailed = tileDelta - spotCheckResult;
            if (tileDelta - spotCheckResult < 0)
            {
                sightFailed = 0;
            }

            FindObstacleObj(obj, obj.GetLocation(), out var obstructor);
            if (obstructor != null)
            {
                ++sightFailed;
            }

            return sightFailed;
        }

        // TODO Test this function
        private bool IsFacingTarget(GameObjectBody obj, GameObjectBody target)
        {
            var relPosCode = obj.GetCompassDirection(target);
            var v3 = (relPosCode - MapObjectSystem.GetCurrentForwardDirection(obj)) % 8;
            return v3 == 0 || v3 == 1 || v3 == 7 || v3 == 2 || v3 == 6;
        }

        public enum CannotTalkCause
        {
            None = 0,
            Dead = 1,
            AlreadySpeaking = 4,
            CantSpeak = 5,
            Sleeping = 6
        }

        public bool CanTalkTo(GameObjectBody speaker, GameObjectBody listener) =>
            GetCannotTalkReason(speaker, listener) == CannotTalkCause.None;

        [TempleDllLocation(0x10058900)]
        public CannotTalkCause GetCannotTalkReason(GameObjectBody speaker, GameObjectBody listener)
        {
            if (speaker.IsOffOrDestroyed)
                return CannotTalkCause.CantSpeak;
            if (!speaker.IsCritter())
                return CannotTalkCause.None;

            var critterFlags = speaker.GetCritterFlags();

            if ((critterFlags & (CritterFlag.MUTE | CritterFlag.STUNNED | CritterFlag.PARALYZED)) != default
                || GameSystems.D20.D20Query(speaker, D20DispatcherKey.QUE_Mute) != 0
                || GameSystems.D20.D20Query(listener, D20DispatcherKey.QUE_Mute) != 0)
            {
                return CannotTalkCause.CantSpeak;
            }

            if ((critterFlags & CritterFlag.SLEEPING) != default)
            {
                return CannotTalkCause.Sleeping;
            }

            var spellFlags = speaker.GetSpellFlags();
            if (spellFlags.HasFlag(SpellFlag.STONED))
            {
                return CannotTalkCause.CantSpeak;
            }
            else if (GameSystems.Critter.IsDeadNullDestroyed(speaker))
            {
                if (spellFlags.HasFlag(SpellFlag.SPOKEN_WITH_DEAD))
                {
                    return 0;
                }
                else
                {
                    return CannotTalkCause.Dead;
                }
            }
            else
            {
                if (GameSystems.Critter.IsDeadOrUnconscious(speaker))
                {
                    // Can't be dead here anymore (so only unconscious)
                    return CannotTalkCause.Sleeping;
                }

                var currentlyTalkingTo = GameSystems.Reaction.GetLastReactionPlayer(speaker);
                if (currentlyTalkingTo != null && currentlyTalkingTo != listener)
                {
                    return CannotTalkCause.AlreadySpeaking;
                }
            }

            return CannotTalkCause.None;
        }

        [TempleDllLocation(0x1005e410)]
        public void CritterKilled(GameObjectBody critter, GameObjectBody killer)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1005eec0)]
        public void AiProcess(GameObjectBody critter)
        {
            // TEMPLE PLUS IMPL AVAILABLE
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005AD20)]
        public bool IsPcUnderAiControl(GameObjectBody critter)
        {
            if (!GameSystems.Party.IsPlayerControlled(critter)
                && critter.IsPC()
                && (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0
                    || GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_AIControlled) != 0
                    || GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) != 0))
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) != 0)
                {
                    var afraidOf =
                        GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Afraid);

                    if (critter.DistanceToObjInFeet(afraidOf) <= 40.0f &&
                        GameSystems.Combat.HasLineOfAttack(afraidOf, critter))
                    {
                        return true;
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        [TempleDllLocation(0x1005AE10)]
        public void AiProcessPc(GameObjectBody critter)
        {
            GameObjectBody charmedBy = null;
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0)
            {
                charmedBy = GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed);
            }

            GameObjectBody afraidOf = null;
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) != 0)
            {
                afraidOf = GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Afraid);
            }

            if (charmedBy != null && !GameSystems.Critter.IsFriendly(charmedBy, critter))
            {
                GameObjectBody target;
                if (charmedBy.IsNPC())
                {
                    target = charmedBy.GetObject(obj_f.npc_combat_focus);
                    if (target == critter)
                    {
                        target = AiPcFindVicinityNonfriendly(critter);
                    }
                }
                else
                {
                    target = AiPcFindVicinityNonfriendly(charmedBy);
                }

                if (target == null)
                {
                    Logger.Info("Unabled to find a target for AI PC=( {0} )",
                        GameSystems.MapObject.GetDisplayName(critter));
                }

                StrategyParse(critter, target);
            }
            else if (afraidOf != null)
            {
                AiFleeProcess(critter, afraidOf);
            }
            else
            {
                var target = AiPcFindVicinityNonfriendly(critter);
                if (target == null)
                {
                    Logger.Info("Unabled to find a target for AI PC=( {0} )",
                        GameSystems.MapObject.GetDisplayName(critter));
                }

                StrategyParse(critter, target);
            }

            if ( GameSystems.Combat.IsCombatActive() )
            {
                var currentActor = GameSystems.D20.Initiative.CurrentActor;
                if ( currentActor == critter && !GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor) )
                {
                    if ( GameSystems.D20.Actions.IsSimulsCompleted() )
                    {
                        if ( !GameSystems.D20.Actions.IsLastSimultPopped(currentActor) )
                        {
                            var actorName = GameSystems.MapObject.GetDisplayName(currentActor);
                            Logger.Info("AI for {0} ending turn...", actorName);
                            GameSystems.Combat.AdvanceTurn(currentActor);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x10057030)]
        private GameObjectBody AiPcFindVicinityNonfriendly(GameObjectBody objHnd)
        {
            using var listResult = ObjList.ListVicinity(objHnd, ObjectListFilter.OLC_CRITTERS);

            foreach (var otherCritter in listResult)
            {
                if (!GameSystems.Critter.IsFriendly(objHnd, otherCritter))
                {
                    return otherCritter;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100e50c0)]
        private void StrategyParse(GameObjectBody critter, GameObjectBody target)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005A1F0)]
        private void AiFleeProcess(GameObjectBody obj, GameObjectBody target)
        {
            if ( target != null )
            {
                if ( obj.IsNPC() )
                {
                    if ( !obj.AiFlags.HasFlag(AiFlag.HasSpokenFlee))
                    {
                        GameSystems.Dialog.GetFleeVoiceLine(obj, target, out var text, out var soundId);
                        GameSystems.Dialog.PlayCritterVoiceLine(obj, target, text, soundId);
                        obj.AiFlags |= AiFlag.HasSpokenFlee;
                    }
                }

                if ( obj.DistanceToObjInFeet(target) >= 75.0f )
                {
                    if ( obj.IsNPC() )
                    {
                        UpdateAiFlags(obj, AiFightStatus.SURRENDERED, target);
                    }
                }
                else
                {
                    // TODO: This is done differently from the vanilla logic. Confirm it works.
                    // Flee away from the target in a direct line
                    var from = target.GetLocationFull().ToInches2D();
                    var to = obj.GetLocationFull().ToInches2D();
                    var directional = Vector2.Normalize(to - from);
                    var fleeTo = LocAndOffsets.FromInches(from + directional * 6 * locXY.INCH_PER_TILE);

                    if ( GameSystems.D20.Actions.TurnBasedStatusInit(obj) )
                    {
                        GameSystems.D20.Actions.CurSeqReset(obj);
                        GameSystems.D20.Actions.GlobD20ActnInit();
                        GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(0, 0);
                        GameSystems.D20.Actions.GlobD20ActnSetTarget(null, fleeTo);
                        GameSystems.D20.Actions.ActionAddToSeq();
                        GameSystems.D20.Actions.sequencePerform();
                    }
                }
            }
        }

        [TempleDllLocation(0x1005da00)]
        private void UpdateAiFlags(GameObjectBody handle, AiFightStatus aiFightStatus, GameObjectBody target, object soundMap = null)
        {
            throw new NotImplementedException();
        }
    }

    public enum PortalLockStatus
    {
        PLS_OPEN = 0,
        PLS_LOCKED = 1,
        PLS_JAMMED = 2,
        PLS_MAGICALLY_HELD = 3,
        PLS_DENIED_BY_SCRIPT = 4,
        PLS_INVALID_OPENER = 5,
        PLS_SECRET_UNDISCOVERED = 6,
    }
}