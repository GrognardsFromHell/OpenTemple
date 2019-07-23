using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public delegate void AiCancelDialog(GameObjectBody critter);

    public delegate void AiShowTextBubble(GameObjectBody critter, GameObjectBody speakingTo,
        string text, int speechId);

    public enum AiFightStatus
    {
        NONE = 0,
        FIGHTING = 1,
        FLEEING = 2,
        SURRENDERED = 3,
        FINDING_HELP = 4,
        BEING_DRAWN = 5 // New TODO for Harpy Song
    }

    public class AiSystem : IGameSystem, IModuleAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10056d50)]
        public AiSystem()
        {
            _cancelDialog = null;
            _showTextBubble = null;

            var aiParamsMes = Tig.FS.ReadMesFile("rules/ai_params.mes");

            for (int aiPacketIdx = 0; aiPacketIdx < 100; aiPacketIdx++)
            {
                if (aiParamsMes.TryGetValue(aiPacketIdx, out var line))
                {
                    var parts = line.Split(' ', '\t', '\n');
                    if (parts.Length != 17)
                    {
                        throw new InvalidOperationException($"AI Packet {aiPacketIdx} has invalid line.");
                    }

                    aiParams[aiPacketIdx] = new AiParamPacket
                    {
                        hpPercentToTriggerFlee = int.Parse(parts[0], CultureInfo.InvariantCulture),
                        numPeopleToTriggerFlee = int.Parse(parts[1], CultureInfo.InvariantCulture),
                        lvlDiffToTriggerFlee = int.Parse(parts[2], CultureInfo.InvariantCulture),
                        pcHpPercentToPreventFlee = int.Parse(parts[3], CultureInfo.InvariantCulture),
                        fleeDistanceFeet = int.Parse(parts[4], CultureInfo.InvariantCulture),
                        reactionLvlToRefuseFollowingPc = int.Parse(parts[5], CultureInfo.InvariantCulture),
                        unused7 = int.Parse(parts[6], CultureInfo.InvariantCulture),
                        unused8 = int.Parse(parts[7], CultureInfo.InvariantCulture),
                        maxLvlDiffToAgreeToJoin = int.Parse(parts[8], CultureInfo.InvariantCulture),
                        reactionLoweredOnFriendlyFire = int.Parse(parts[9], CultureInfo.InvariantCulture),
                        hostilityThreshold = int.Parse(parts[10], CultureInfo.InvariantCulture),
                        unused12 = int.Parse(parts[11], CultureInfo.InvariantCulture),
                        offensiveSpellChance = int.Parse(parts[12], CultureInfo.InvariantCulture),
                        defensiveSpellChance = int.Parse(parts[13], CultureInfo.InvariantCulture),
                        healSpellChance = int.Parse(parts[14], CultureInfo.InvariantCulture),
                        combatMinDistanceFeet = int.Parse(parts[15], CultureInfo.InvariantCulture),
                        canOpenPortals = int.Parse(parts[16], CultureInfo.InvariantCulture),
                    };
                }
            }
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
                    WellKnownSpells.InvisibilityToUndead) != 0
                && GameSystems.Critter.IsUndead(obj))
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToAnimals) != 0
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
                var hidePenalty = 1 - tgt.dispatch1ESkillLevel(SkillId.hide, obj, 1);
                spotCheckResult = obj.dispatch1ESkillLevel(SkillId.spot, target, 1) + hidePenalty;
            }
            else
            {
                // Make a spot check to determine how far we can see
                spotCheckResult = 15 + obj.dispatch1ESkillLevel(SkillId.spot, tgt, 1);
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

            if (GameSystems.Combat.IsCombatActive())
            {
                var currentActor = GameSystems.D20.Initiative.CurrentActor;
                if (currentActor == critter && !GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor))
                {
                    if (GameSystems.D20.Actions.IsSimulsCompleted())
                    {
                        if (!GameSystems.D20.Actions.IsLastSimultPopped(currentActor))
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
            if (target != null)
            {
                if (obj.IsNPC())
                {
                    if (!obj.AiFlags.HasFlag(AiFlag.HasSpokenFlee))
                    {
                        GameSystems.Dialog.GetFleeVoiceLine(obj, target, out var text, out var soundId);
                        GameSystems.Dialog.PlayCritterVoiceLine(obj, target, text, soundId);
                        obj.AiFlags |= AiFlag.HasSpokenFlee;
                    }
                }

                if (obj.DistanceToObjInFeet(target) >= 75.0f)
                {
                    if (obj.IsNPC())
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

                    if (GameSystems.D20.Actions.TurnBasedStatusInit(obj))
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
        private void UpdateAiFlags(GameObjectBody handle, AiFightStatus aiFightStatus, GameObjectBody target,
            object soundMap = null)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10057a70)]
        public void GetAiFightStatus(GameObjectBody obj, out AiFightStatus status, out GameObjectBody target)
        {
            var critFlags = obj.GetCritterFlags();

            if (critFlags.HasFlag(CritterFlag.FLEEING))
            {
                target = obj.GetObject(obj_f.critter_fleeing_from);
                status = AiFightStatus.FLEEING;
                return;
            }

            if (critFlags.HasFlag(CritterFlag.SURRENDERED))
            {
                target = obj.GetObject(obj_f.critter_fleeing_from);
                status = AiFightStatus.SURRENDERED;
                return;
            }

            var aiFlags = obj.AiFlags;
            if (aiFlags.HasFlag(AiFlag.Fighting))
            {
                target = obj.GetObject(obj_f.npc_combat_focus);
                status = AiFightStatus.FIGHTING;
                return;
            }

            if (aiFlags.HasFlag(AiFlag.FindingHelp))
            {
                target = obj.GetObject(obj_f.npc_combat_focus);
                status = AiFightStatus.FINDING_HELP;
                return;
            }

            target = null;
            status = AiFightStatus.NONE;
        }

        [TempleDllLocation(0x100590f0)]
        public int GetAllegianceStrength(GameObjectBody aiHandle, GameObjectBody tgt)
        {
            if (aiHandle == tgt)
            {
                return 4;
            }

            if (GameSystems.Combat.IsBrawling)
            {
                return 0;
            }

            var leader = GameSystems.Critter.GetLeader(aiHandle);
            if (leader != null && (leader == tgt || GameSystems.Party.IsInParty(tgt) &&
                                   (GameSystems.Party.IsInParty(leader) || GameSystems.Party.IsInParty(aiHandle))))
            {
                return 3;
            }

            if (aiHandle.GetSpellFlags().HasFlag(SpellFlag.MIND_CONTROLLED)
                || GameSystems.D20.D20Query(aiHandle, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0)
            {
                return 0;
            }

            if (GameSystems.Critter.NpcAllegianceShared(aiHandle, tgt))
            {
                return 1;
            }

            return 0;
            // there's a stub here for value 2
        }

        [TempleDllLocation(0x1005c920)]
        public bool WillKos(GameObjectBody aiHandle, GameObjectBody triggerer)
        {
            if (!ConsiderTarget(aiHandle, triggerer))
            {
                return false;
            }

            var leader = GameSystems.Critter.GetLeaderRecursive(aiHandle);

            if (GameSystems.Script.ExecuteObjectScript(triggerer, aiHandle, ObjScriptEvent.WillKos) == 0)
            {
                return false;
            }

            if (NpcAiListFindAlly(aiHandle, triggerer))
                return false;

            GetAiFightStatus(aiHandle, out var aiFightStatus, out var curTgt);

            if (aiFightStatus == AiFightStatus.SURRENDERED && curTgt == triggerer)
                return false;

            var isInParty = GameSystems.Party.IsInParty(aiHandle);
            if (leader == null && !isInParty)
            {
                var npcFlags = aiHandle.GetNPCFlags();
                if (npcFlags.HasFlag(NpcFlag.KOS) && !npcFlags.HasFlag(NpcFlag.KOS_OVERRIDE))
                {
                    if (!GameSystems.Critter.NpcAllegianceShared(aiHandle, triggerer) &&
                        (triggerer.IsPC() || !GameSystems.Critter.HasNoAllegiance(triggerer)))
                    {
                        return true;
                    }

                    if (GameSystems.D20.D20Query(triggerer, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0)
                    {
                        var charmer =
                            GameSystems.D20.D20QueryReturnObject(triggerer, D20DispatcherKey.QUE_Critter_Is_Charmed);
                        if (WillKos(aiHandle, charmer))
                        {
                            return true;
                        }
                    }
                }

                // check AI Params hostility threshold
                if (triggerer.IsPC())
                {
                    var aiPar = GetAiParams(aiHandle);
                    var reac = GetReaction(aiHandle, triggerer);
                    if (reac <= aiPar.hostilityThreshold)
                    {
                        return true;
                    }

                    return false;
                }
            }

            if (!triggerer.IsNPC())
                return false;
            GetAiFightStatus(triggerer, out aiFightStatus, out curTgt);
            if (aiFightStatus != AiFightStatus.FIGHTING || curTgt == null)
            {
                return false;
            }

            if (GetAllegianceStrength(aiHandle, curTgt) == 0)
            {
                return false; // there's a stub for more extensive logic here...
            }

            leader = GameSystems.Critter.GetLeader(aiHandle);
            if (CannotHate(aiHandle, triggerer, leader) == 0)
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100541a0)]
        private int GetReaction(GameObjectBody aiHandle, GameObjectBody triggerer)
        {
            return GameSystems.Reaction.GetReaction(aiHandle, triggerer);
        }

        [TempleDllLocation(0x10AA4BD0)]
        private readonly AiParamPacket[] aiParams = new AiParamPacket[100];

        [TempleDllLocation(0x10057a40)]
        private AiParamPacket GetAiParams(GameObjectBody obj)
        {
            var aiParamIdx = obj.GetInt32(obj_f.npc_ai_data);
            Trace.Assert(aiParamIdx >= 0 && aiParamIdx < 100000);
            return aiParams[aiParamIdx];
        }

        [TempleDllLocation(0x10059fc0)]
        private bool AiListFind(GameObjectBody aiHandle, GameObjectBody tgt, int typeToFind)
        {
            // ensure is NPC handle
            if (aiHandle == null)
                return false;
            var obj = aiHandle;
            if (!obj.IsNPC())
                return false;

            var typeListCount = obj.GetInt32Array(obj_f.npc_ai_list_type_idx).Count;
            if (typeListCount == 0)
                return false;

            var aiListCount = obj.GetObjectArray(obj_f.npc_ai_list_idx).Count;
            var count = Math.Min(aiListCount, typeListCount);

            for (var i = 0; i < count; i++)
            {
                var aiListType = obj.GetInt32(obj_f.npc_ai_list_type_idx, i);
                if (aiListType != typeToFind)
                    continue;

                var aiListItem = obj.GetObject(obj_f.npc_ai_list_idx, i);
                if (aiListItem == tgt)
                    return true;
            }

            return false;
        }

        [TempleDllLocation(0x1005d3f0)]
        private bool ConsiderTarget(GameObjectBody obj, GameObjectBody tgt)
        {
            if (tgt == null || tgt == obj)
                return false;

            var tgtObj = tgt;
            if ((tgtObj.GetFlags() &
                 (ObjectFlag.INVULNERABLE | ObjectFlag.DONTDRAW | ObjectFlag.OFF | ObjectFlag.DESTROYED)) != default)
            {
                return false;
            }

            var objBody = obj;

            var leader = GameSystems.Critter.GetLeader(obj);
            if (!tgtObj.IsCritter())
            {
                if (GameSystems.MapObject.IsBusted(tgt))
                {
                    return false;
                }
            }
            else
            {
                if (NpcAiListFindAlly(obj, tgt))
                    return false;
                if (GameSystems.Critter.IsDeadNullDestroyed(tgt))
                    return false;
                if (GameSystems.Critter.IsDeadOrUnconscious(tgt))
                {
                    if (objBody.GetCritterFlags().HasFlag(CritterFlag.NON_LETHAL_COMBAT))
                    {
                        return false;
                    }

                    //// Make AI party members ignore unconscious critters if nothing else is left...
                    //if (GameSystems.Party.IsInParty(obj)){
                    //	if (GameSystems.Combat.AllCombatantsFarFromParty()){
                    //		return false;
                    //	}
                    //}
                    var suitableCrit = FindSuitableTarget(obj);
                    if (suitableCrit != null && suitableCrit != tgt)
                        return false;
                }

                if (tgt == leader)
                    return false;

                var tgtLeader = GameSystems.Critter.GetLeader(tgt);
                if (tgtLeader != null && tgtLeader == leader)
                    return false;

                if (IsCharmedBy(tgt, obj))
                {
                    return TargetIsPcPartyNotDead(tgt);
                }

                if (GameSystems.Critter.IsFriendly(obj, tgt))
                    return false;
            }

            if (obj.DistanceToObjInFeet(tgt) > 125.0f)
            {
                return false;
            }

            if (leader != null)
            {
                var tileDelta = leader.GetLocation().EstimateDistance(tgt.GetLocation());
                if (tileDelta > 20)
                {
                    // added so your summons work at range...
                    if (obj.DistanceToObjInFeet(tgt) > 15.0f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10057C50)]
        private bool IsCharmedBy(GameObjectBody critter, GameObjectBody charmer)
        {
            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) != 0
                   && GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) == charmer;
        }

        [TempleDllLocation(0x1005B7D0)]
        private bool TargetIsPcPartyNotDead(GameObjectBody partyMember)
        {
            return partyMember.IsPC()
                   && PartyHasNoRemainingMembers() || GameSystems.Party.GetLivingPartyMemberCount() <= 1;
        }

        [TempleDllLocation(0x10057ca0)]
        private bool PartyHasNoRemainingMembers()
        {
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.D20.D20Query(partyMember, D20DispatcherKey.QUE_Critter_Is_Charmed) == 0
                    && !GameSystems.Critter.IsDeadNullDestroyed(partyMember))
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x1005c1c0)]
        public bool NpcAiListFindEnemy(GameObjectBody aiObj, GameObjectBody tgt)
        {
            return AiListFind(aiObj, tgt, 0);
        }

        [TempleDllLocation(0x1005c200)]
        public bool NpcAiListFindAlly(GameObjectBody aiObj, GameObjectBody tgt)
        {
            return AiListFind(aiObj, tgt, 1);
        }

        [TempleDllLocation(0x10AA73B4)]
        private bool aiSearchingTgt;

        [TempleDllLocation(0x102BD4D0)]
        private static readonly int[] rangeTiles =
        {
            5,
            10,
            20,
            20
        };

        [TempleDllLocation(0x1005ced0)]
        private GameObjectBody FindSuitableTarget(GameObjectBody handle)
        {
            if (aiSearchingTgt)
                return null;

            // begin search section
            aiSearchingTgt = true;

            GameObjectBody objToTurnTowards = null;
            using var objList = ObjList.ListRangeTiles(handle, 18, ObjectListFilter.OLC_CRITTERS);

            var numCritters = objList.Count;
            var critterList = new List<GameObjectBody>(objList);
            Span<long> tileDeltas = stackalloc long[numCritters];

            var leader = GameSystems.Critter.GetLeader(handle);

            // sort by distance?
            if (numCritters > 1)
            {
                for (var i = 0; i < numCritters; i++)
                {
                    var dude = critterList[i];
                    var tileDelta = handle.GetLocation().EstimateDistance(dude.GetLocation());
                    tileDeltas[i] = tileDelta;
                    if (GameSystems.Critter.IsDeadOrUnconscious(dude))
                        tileDeltas[i] += 1000;
                }

                for (var i = 1; i < numCritters; i++)
                {
                    var target = critterList[i];
                    var tileDelta = tileDeltas[i];
                    var j = i;
                    for (; j > 0; j--)
                    {
                        if (tileDeltas[j - 1] <= tileDelta)
                            break;
                        tileDeltas[j] = tileDeltas[j - 1];
                        critterList[j] = critterList[j - 1];
                    }

                    tileDeltas[j] = tileDelta;
                    critterList[j] = target;
                }
            }

            GameObjectBody kosCandidate = null;
            for (var i = 0; i < numCritters; i++)
            {
                var target = critterList[i];

                // Added 2019-01 because it was silly
                if (target == handle)
                    continue;

                var isUnconcealed = !GameSystems.Critter.IsMovingSilently(target)
                                    && !GameSystems.Critter.IsConcealed(target);

                if (CannotHear(handle, target, isUnconcealed ? 1 : 0) == 0
                    || HasLineOfSight(handle, target) == 0)
                {
                    var tgtObj = target;
                    if (tgtObj.IsPC() && !isUnconcealed)
                    {
                        objToTurnTowards = target;
                    }

                    if (WillKos(handle, target))
                    {
                        kosCandidate = target;
                        break;
                    }

                    var friendsCombatFocus = GetFriendsCombatFocus(handle, target, leader);
                    if (friendsCombatFocus != null)
                    {
                        kosCandidate = friendsCombatFocus;
                        break;
                    }
                }

                target = GetTargetFromDeadFriend(handle, target);
                if (target != null)
                {
                    isUnconcealed = !GameSystems.Critter.IsMovingSilently(target)
                                    && !GameSystems.Critter.IsConcealed(target);
                    if (CannotHear(handle, target, isUnconcealed ? 1 : 0) == 0
                        || HasLineOfSight(handle, target) == 0)
                    {
                        kosCandidate = target;
                        break;
                    }
                }
            }


            if (kosCandidate == null)
            {
                if (objToTurnTowards != null)
                {
                    var rotationTo = handle.RotationTo(objToTurnTowards);
                    GameSystems.Anim.PushRotate(handle, rotationTo);
                }
            }

            aiSearchingTgt = false;

            return kosCandidate;
        }

        [TempleDllLocation(0x1005CB60)]
        private GameObjectBody GetTargetFromDeadFriend(GameObjectBody obj, GameObjectBody target)
        {
            if (!GameSystems.Critter.IsDeadNullDestroyed(target) || !target.IsNPC() || NpcAiListFindAlly(obj, target))
            {
                return null;
            }

            var tgt = target.GetObject(obj_f.npc_combat_focus);
            if (tgt != null && ConsiderTarget(obj, tgt) && GetAllegianceStrength(obj, target) != 0)
            {
                return tgt;
            }

            return null;
        }

        private GameObjectBody GetFriendsCombatFocus(GameObjectBody handle, GameObjectBody friendHandle,
            GameObjectBody leader)
        {
            var tgtObj = friendHandle;
            if (tgtObj.IsNPC())
            {
                GameSystems.AI.GetAiFightStatus(friendHandle, out var aifs, out var targetsFocus);

                //// Make AI ignore friend's target if it's unconscious (otherwise putting foes to sleep caused "enter combat" loops when more than 1 AI follower was in party)
                //if (targetsFocus && GameSystems.Critter.IsDeadOrUnconscious(targetsFocus)){
                //	return null;
                //}

                if (GameSystems.AI.ConsiderTarget(handle, targetsFocus) &&
                    (aifs == AiFightStatus.FIGHTING || aifs == AiFightStatus.FLEEING ||
                     aifs == AiFightStatus.SURRENDERED))
                {
                    var allegianceStr = GameSystems.AI.GetAllegianceStrength(handle, friendHandle);

                    if (allegianceStr != 0 && GameSystems.AI.CannotHate(handle, targetsFocus, leader) == 0)
                    {
                        var isUnconcealed = !GameSystems.Critter.IsMovingSilently(targetsFocus) &&
                                            !GameSystems.Critter.IsConcealed(targetsFocus);

                        // check simple LOS/hearing
                        if (CannotHear(handle, targetsFocus, isUnconcealed ? 1 : 0) == 0 ||
                            HasLineOfSight(handle, targetsFocus) == 0)
                        {
                            return targetsFocus;
                        }

                        // new in Temple+ : check pathfinding short distances (to simulate sensing nearby critters)
                        var pathFlags = PathQueryFlags.PQF_HAS_CRITTER
                                        | PathQueryFlags.PQF_IGNORE_CRITTERS
                                        | PathQueryFlags.PQF_800
                                        | PathQueryFlags.PQF_TARGET_OBJ
                                        | PathQueryFlags.PQF_ADJUST_RADIUS
                                        | PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS
                                        | PathQueryFlags.PQF_DONT_USE_PATHNODES
                                        | PathQueryFlags.PQF_A_STAR_TIME_CAPPED;

                        if (!Globals.Config.alertAiThroughDoors)
                        {
                            pathFlags |= PathQueryFlags.PQF_DOORS_ARE_BLOCKING;
                        }

                        if (GameSystems.PathX.CanPathTo(handle, targetsFocus, pathFlags, 40))
                        {
                            return targetsFocus;
                        }
                        else if (!GameSystems.Party.IsInParty(handle))
                        {
                            var partyTgt = GameSystems.PathX.CanPathToParty(handle);
                            if (partyTgt != null)
                            {
                                return partyTgt;
                            }
                        }
                    }
                }
            }

            return null;
        }


        [TempleDllLocation(0x10059a10)]
        private int CannotHear(GameObjectBody obj, GameObjectBody target, int tileRangeIdx)
        {
            var tgtLo = target;
            if (obj == target)
            {
                return 0;
            }

            if (obj.GetCritterFlags().HasFlag(CritterFlag.STUNNED))
            {
                return 1000;
            }

            if (GameSystems.Critter.IsDeadOrUnconscious(obj)
                || GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Deafened) != 0)
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Is_Invisible) != 0
                && GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible) == 0)
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToUndead) != 0
                && GameSystems.Critter.IsUndead(obj))
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToAnimals) != 0
                && GameSystems.Critter.IsAnimal(obj))
            {
                return 1000;
            }

            var estimatedDistance = obj.GetLocation().EstimateDistance(target.GetLocation());
            if (estimatedDistance > 1000)
            {
                return 1000;
            }

            var raycastPkt = new RaycastPacket();
            raycastPkt.flags |= RaycastFlag.HasTargetObj | RaycastFlag.StopAfterFirstBlockerFound |
                                RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj;
            raycastPkt.origin = obj.GetLocationFull();
            raycastPkt.targetLoc = target.GetLocationFull();
            raycastPkt.sourceObj = obj;
            raycastPkt.target = target;
            if (!raycastPkt.TestLineOfSight())
            {
                return 1000;
            }

            // If the target is concealed or moving silently, use opposing skill checks to determine whether
            // we can hear them.
            int listenCheckResult;
            if (GameSystems.Critter.IsMovingSilently(tgtLo) || GameSystems.Critter.IsConcealed(tgtLo))
            {
                var moveSilPenalty = 1 - tgtLo.dispatch1ESkillLevel(SkillId.move_silently, obj, 1);
                listenCheckResult = obj.dispatch1ESkillLevel(SkillId.listen, tgtLo, 1) + moveSilPenalty;
            }
            else
            {
                listenCheckResult = 15 + obj.dispatch1ESkillLevel(SkillId.listen, tgtLo, 1);
                if (listenCheckResult < 15)
                {
                    listenCheckResult = 15;
                }
            }

            if (obj.GetCritterFlags().HasFlag(CritterFlag.SLEEPING))
            {
                listenCheckResult /= 2;
            }

            if (listenCheckResult < 0)
            {
                listenCheckResult = 0;
            }

            var effectiveCheckResult = listenCheckResult + rangeTiles[tileRangeIdx] - rangeTiles[0];
            var hearingDistance = effectiveCheckResult / 2 - SoundBlockageGet(obj, tgtLo);
            if (estimatedDistance > hearingDistance)
            {
                return estimatedDistance - hearingDistance;
            }

            return 0;
        }

        /// <summary>
        /// This function is very similar to <see cref="FindObstacleObj"/>, but it does not stop on the
        /// first obstacle. Rather it sums up all the "cost" and returns it.
        /// </summary>
        [TempleDllLocation(0x10058ec0)]
        private int SoundBlockageGet(GameObjectBody obj, GameObjectBody target)
        {
            var objLoc = obj.GetLocation();
            var tgtLoc = target.GetLocation();
            if (objLoc == tgtLoc)
            {
                return 0;
            }

            Span<sbyte> deltas = stackalloc sbyte[200];
            var pathLength = GameSystems.PathX.RasterizeLineBetweenLocsScreenspace(objLoc, tgtLoc, deltas);
            if (pathLength == 0)
            {
                // If we cannot path between the two points, we assume 100% blockage
                return 100;
            }

            var blockingFlags = MapObjectSystem.ObstacleFlag.SOUND_BLOCKERS;
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
                        out _);

                    objLoc = locOut;
                }
            }

            return cost;
        }

        [TempleDllLocation(0x10058b80)]
        private int CannotHate(GameObjectBody aiHandle, GameObjectBody triggerer, GameObjectBody aiLeader)
        {
            var obj = aiHandle;
            if (obj.GetSpellFlags().HasFlag(SpellFlag.MIND_CONTROLLED) &&
                GameSystems.Critter.GetLeaderRecursive(aiHandle) != null)
                return 0;
            if (triggerer == null || !triggerer.IsCritter())
                return 0;
            if (aiLeader != null && GameSystems.Critter.GetLeader(triggerer) == aiLeader)
                return 4;
            if (GameSystems.Critter.NpcAllegianceShared(aiHandle, triggerer))
                return 3;
            if (!aiHandle.HasCondition("sp-Sanctuary Save Failed") || !triggerer.HasCondition("sp-Sanctuary"))
            {
                return 0;
            }
            else
            {
                var triggererSanctuaryHandle = triggerer.HasCondition("sp-Sanctuary");
                var sancHandle = aiHandle.HasCondition("sp-Sanctuary Save Failed");
                if (sancHandle == triggererSanctuaryHandle)
                    return 5;
            }

            return 0;
        }
    }

    internal class AiParamPacket
    {
        // most of this stuff is arcanum leftovers
        public int hpPercentToTriggerFlee;
        public int numPeopleToTriggerFlee;
        public int lvlDiffToTriggerFlee;
        public int pcHpPercentToPreventFlee;
        public int fleeDistanceFeet;
        public int reactionLvlToRefuseFollowingPc;
        public int unused7;
        public int unused8;
        public int maxLvlDiffToAgreeToJoin;
        public int reactionLoweredOnFriendlyFire;
        public int hostilityThreshold;
        public int unused12;
        public int offensiveSpellChance;
        public int defensiveSpellChance;
        public int healSpellChance;
        public int combatMinDistanceFeet;
        public int canOpenPortals;
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