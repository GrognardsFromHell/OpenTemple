using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.InGameSelect;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.AI
{
    public delegate void AiCancelDialog(GameObjectBody critter);

    public delegate void AiShowTextBubble(GameObjectBody critter, GameObjectBody speakingTo,
        string text, int speechId);

    public class AiSystem : IGameSystem, IModuleAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        /// <summary>
        /// Used by cheats.
        /// </summary>
        [TempleDllLocation(0x102BD4E0)]
        public bool IsNpcFightingAllowed { get; set; } = true;

        private AiStrategies _strategies = new AiStrategies();

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
                    GetStandpoint(obj, StandPointType.Day, out standPoint);
                }
                else
                {
                    GetStandpoint(obj, StandPointType.Night, out standPoint);
                }
            }
            else
            {
                if (GameSystems.TimeEvent.IsDaytime)
                {
                    // Daytime standpoint
                    GetStandpoint(obj, StandPointType.Day, out standPoint);
                }
                else
                {
                    // Nighttime standpoint
                    GetStandpoint(obj, StandPointType.Night, out standPoint);
                }
            }

            location = standPoint.location.location;
            return location.locx != 0 || location.locy != 0;
        }

        [TempleDllLocation(0x100ba890)]
        internal void GetStandpoint(GameObjectBody obj, StandPointType type, out StandPoint standPoint)
        {
            Debugger.Break();

            var standpointArray = obj.GetInt64Array(obj_f.npc_standpoints);

            Span<long> packedStandpoint = stackalloc long[10];

            for (int i = 0; i < 10; i++)
            {
                packedStandpoint[i] = standpointArray[10 * (int) type + i];
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
            StopFleeing(obj);
            obj.SetCritterFlags(obj.GetCritterFlags() | CritterFlag.NO_FLEE);
        }

        [TempleDllLocation(0x1005dea0)]
        public void StopFleeing(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                var critterFlags = critter.GetCritterFlags();
                if ((critterFlags & (CritterFlag.FLEEING | CritterFlag.SURRENDERED)) != default)
                {
                    UpdateAiFlags(critter, AiFightStatus.NONE, null);
                }
                GameSystems.Anim.InterruptGoalsByType(critter, AnimGoalType.flee);
            }
        }

        // NO idea why this is in the AI subsystem
        [TempleDllLocation(0x1005bf20)]
        public LockStatus AttemptToOpenDoor(GameObjectBody actor, GameObjectBody portal)
        {
            if (GameSystems.MapObject.IsBusted(portal))
            {
                return LockStatus.PLS_OPEN;
            }

            if (!actor.IsCritter())
            {
                return LockStatus.PLS_INVALID_OPENER;
            }

            if (GameSystems.Script.ExecuteObjectScript(actor, portal, ObjScriptEvent.Unlock) == 0)
            {
                return LockStatus.PLS_DENIED_BY_SCRIPT;
            }

            if (portal.type != ObjectType.portal)
            {
                return LockStatus.PLS_OPEN;
            }

            var portalFlags = portal.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.JAMMED))
            {
                return LockStatus.PLS_JAMMED;
            }

            if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return LockStatus.PLS_MAGICALLY_HELD;
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
                            return LockStatus.PLS_OPEN;
                        }
                    }
                }
                else if (portal.IsPortalOpen())
                {
                    return LockStatus.PLS_OPEN;
                }
            }

            if (!portal.NeedsToBeUnlocked())
            {
                return LockStatus.PLS_OPEN;
            }

            var keyId = portal.GetInt32(obj_f.portal_key_id);
            if (GameSystems.Item.HasKey(actor, keyId))
            {
                GameUiBridge.MarkKeyUsed(keyId, GameSystems.TimeEvent.GameTime);
                return LockStatus.PLS_OPEN;
            }

            if (portal.IsUndetectedSecretDoor())
            {
                return LockStatus.PLS_SECRET_UNDISCOVERED;
            }

            return LockStatus.PLS_LOCKED;
        }

        /**
         * Same as AttemptToOpenDoor but without actually it.
         */
        [TempleDllLocation(0x1005c0a0)]
        public LockStatus DryRunAttemptOpenDoor(GameObjectBody actor, GameObjectBody portal)
        {
            if (GameSystems.MapObject.IsBusted(portal))
            {
                return LockStatus.PLS_OPEN;
            }

            if (!actor.IsCritter())
            {
                return LockStatus.PLS_INVALID_OPENER;
            }

            if (portal.type != ObjectType.portal)
            {
                return LockStatus.PLS_OPEN;
            }

            var portalFlags = portal.GetPortalFlags();
            if (portalFlags.HasFlag(PortalFlag.JAMMED))
            {
                return LockStatus.PLS_JAMMED;
            }

            if (portalFlags.HasFlag(PortalFlag.MAGICALLY_HELD))
            {
                return LockStatus.PLS_MAGICALLY_HELD;
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
                            return LockStatus.PLS_OPEN;
                        }
                    }
                }
                else if (portal.IsPortalOpen())
                {
                    return LockStatus.PLS_OPEN;
                }
            }

            if (!portal.NeedsToBeUnlocked())
            {
                return LockStatus.PLS_OPEN;
            }

            var keyId = portal.GetInt32(obj_f.portal_key_id);
            if (GameSystems.Item.HasKey(actor, keyId))
            {
                return LockStatus.PLS_OPEN;
            }

            return LockStatus.PLS_LOCKED;
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
                || GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Blinded)                 || GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Is_Invisible)                 && !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible) )
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToUndead)                 && GameSystems.Critter.IsUndead(obj))
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToAnimals)                 && GameSystems.Critter.IsAnimal(obj))
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
                || GameSystems.D20.D20Query(speaker, D20DispatcherKey.QUE_Mute)                 || GameSystems.D20.D20Query(listener, D20DispatcherKey.QUE_Mute) )
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
            // Check if Player Controlled (if so, skip)
            if (GameSystems.Party.IsPlayerControlled(critter))
                return;

            var isCombatActive = GameSystems.Combat.IsCombatActive();

            if (isCombatActive
                && GameSystems.Critter.IsCombatModeActive(critter)
                && GameSystems.D20.Initiative.CurrentActor != critter
                && GameSystems.D20.Actions.getNextSimulsPerformer() != critter)
            {
                return;
            }

            if (IsPcUnderAiControl(critter))
            {
                AiProcessPc(critter);
                Logger.Debug("Combat for {0} ending turn (script)...", critter);
                return;
            }

            // from Confusion Spell
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_AI_Has_Spell_Override) )
            {
                var confusionState =
                    (int) GameSystems.D20.D20QueryReturnData(critter, D20DispatcherKey.QUE_AI_Has_Spell_Override);
                if (confusionState > 0 && confusionState < 15)
                {
                    if (AiProcessHandleConfusion(critter, confusionState))
                        return;
                }
            }

            if (GameSystems.Map.IsClearingMap())
            {
                return;
            }

            if (GameSystems.Critter.IsDeadOrUnconscious(critter))
            {
                Logger.Info("AI for {0} ending turn (unconscious)...", critter);
                GameSystems.Combat.AdvanceTurn(critter);
                return;
            }

            var aiPacket = new AiPacket(critter);
            if (!aiPacket.PacketCreate())
            {
                return;
            }

            var isCombat = GameSystems.Combat.IsCombatActive();
            var curActor = GameSystems.D20.Initiative.CurrentActor;
            var nextSimuls = GameSystems.D20.Actions.getNextSimulsPerformer();

            if (!isCombat
                || GameSystems.Critter.IsCombatModeActive(critter)
                || curActor == critter
                || nextSimuls == critter)
            {
                aiPacket.ProcessCombat();
                return;
            }

            if (aiPacket.aiFightStatus == AiFightStatus.FIGHTING &&
                critter.DistanceToObjInFeet(aiPacket.target) <= 75.0f)
            {
                GameSystems.Combat.EnterCombat(critter);
            }
        }

        private bool AiProcessHandleConfusion(GameObjectBody critter, int confusionState)
        {
            if (!GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Confused) )
            {
                return false;
            }

            var spellId = (int) GameSystems.D20.D20QueryReturnData(critter, D20DispatcherKey.QUE_Critter_Is_Confused);
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spPkt))
            {
                return false;
            }

            if (confusionState == 11)
            {
                switch (Dice.Roll(1, 6) - 1)
                {
                    case 1:
                        confusionState = 6;
                        break;
                    case 2:
                        confusionState = 7;
                        break;
                    case 3:
                        confusionState = 8;
                        break;
                    case 4:
                        confusionState = 9;
                        break;
                    case 5:
                        confusionState = 12;
                        break;
                    default:
                        confusionState = 5;
                        break;
                }
            }

            GameObjectBody confusionTgt = null;
            switch (confusionState)
            {
                case 5:
                    FleeProcess(critter, spPkt.caster);
                    break;
                case 7:
                    if (!GameSystems.Combat.IsCombatActive())
                    {
                        FleeProcess(critter, spPkt.caster);
                    }
                    else
                    {
                        var randomFleeTarget = GameSystems.Random.PickRandom(GameSystems.D20.Initiative);
                        FleeProcess(critter, randomFleeTarget);
                    }

                    break;
                case 8:
                {
                    using var listResults = ObjList.ListVicinity(critter, ObjectListFilter.OLC_CRITTERS);
                    foreach (var otherCritter in listResults)
                    {
                        confusionTgt = otherCritter;
                        break;
                    }
                }
                    break;
                case 9:
                {
                    // Find the closest critter nearby (who's not dead)
                    using var listResults = ObjList.ListVicinity(critter, ObjectListFilter.OLC_CRITTERS);
                    foreach (var otherCritter in listResults)
                    {
                        if (GameSystems.Critter.IsDeadNullDestroyed(otherCritter))
                            continue;
                        if (confusionTgt == null ||
                            critter.DistanceToObjInFeet(otherCritter) < critter.DistanceToObjInFeet(confusionTgt))
                        {
                            confusionTgt = otherCritter;
                        }
                    }
                }
                    break;
                case 12:
                    confusionTgt = critter;
                    break;
                case 6: // do whatever was doing last I guess
                case 10:
                case 11:
                    break;
                default:
                    return false;
            }

            if (confusionTgt != null)
            {
                StrategyParse(critter, confusionTgt);
            }

            if (GameSystems.Combat.IsCombatActive())
            {
                var curActor = GameSystems.D20.Initiative.CurrentActor;
                if (curActor == critter
                    && !GameSystems.D20.Actions.IsCurrentlyPerforming(curActor)
                    && GameSystems.D20.Actions.IsSimulsCompleted()
                    && !GameSystems.D20.Actions.IsLastSimultPopped(curActor))
                {
                    Logger.Info("AI for {0} ending turn (confusion while simuls)", curActor);
                    GameSystems.Combat.AdvanceTurn(curActor);
                }
            }

            return true;
        }

        [TempleDllLocation(0x1005AD20)]
        public bool IsPcUnderAiControl(GameObjectBody critter)
        {
            if (!GameSystems.Party.IsPlayerControlled(critter)
                && critter.IsPC()
                && (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed)                     || GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_AIControlled)                     || GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) ))
            {
                if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) )
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
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) )
            {
                charmedBy = GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed);
            }

            GameObjectBody afraidOf = null;
            if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) )
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
                FleeProcess(critter, afraidOf);
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
        internal void StrategyParse(GameObjectBody critter, GameObjectBody target)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1005A1F0)]
        internal void FleeProcess(GameObjectBody obj, GameObjectBody target)
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

        [TempleDllLocation(0x10aa73b8)]
        private bool isAlertingAllies;

        internal AiFightStatus UpdateAiFlags(GameObjectBody critter, AiFightStatus aiFightStatus, GameObjectBody target)
        {
            // This is a call where we don't care about the sound map.
            int soundMap = -1;
            return UpdateAiFlags(critter, aiFightStatus, target, ref soundMap);
        }

        [TempleDllLocation(0x1005da00)]
        internal AiFightStatus UpdateAiFlags(GameObjectBody critter, AiFightStatus aiFightStatus, GameObjectBody target,
            ref int soundMap)
        {
            Logger.Debug("{0} entering ai state: {1}, target: {2}", critter, aiFightStatus, target);
            var critterFlags = critter.GetCritterFlags();
            var critterFlags2 = critter.GetCritterFlags2();
            var npcFlags = critter.GetNPCFlags();
            var aiFlags = critter.AiFlags;

            // handle Fleeing / Surrendered first because they may be converted to Fighting
            if (aiFightStatus == AiFightStatus.FLEEING || aiFightStatus == AiFightStatus.SURRENDERED)
            {
                if (aiFightStatus == AiFightStatus.FLEEING
                    && (critterFlags & CritterFlag.NO_FLEE) != default
                    && !GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Afraid) )
                {
                    aiFightStatus = AiFightStatus.FIGHTING;
                }
                else
                {
                    if (_showTextBubble != null
                        && (critterFlags & (CritterFlag.SURRENDERED | CritterFlag.FLEEING)) == default
                        && !GameSystems.Critter.IsDeadOrUnconscious(critter))
                    {
                        GameSystems.Dialog.GetFleeVoiceLine(critter, target, out var fleeText, out var soundId);
                        _showTextBubble(critter, target, fleeText, soundId);
                    }

                    critterFlags &= ~(CritterFlag.SURRENDERED | CritterFlag.FLEEING);
                    if (aiFightStatus == AiFightStatus.FLEEING)
                    {
                        critterFlags |= CritterFlag.FLEEING;
                    }
                    else
                    {
                        critterFlags |= CritterFlag.SURRENDERED;
                        GameSystems.Combat.CritterLeaveCombat(critter);
                    }

                    critter.SetObject(obj_f.critter_fleeing_from, target);
                }
            }

            if (aiFightStatus == AiFightStatus.FIGHTING)
            {
                if (!IsNpcFightingAllowed || (npcFlags & NpcFlag.NO_ATTACK) != default)
                    aiFightStatus = AiFightStatus.NONE;
            }

            if (aiFightStatus != AiFightStatus.SURRENDERED && aiFightStatus != AiFightStatus.FLEEING)
            {
                critterFlags &= ~(CritterFlag.SURRENDERED | CritterFlag.FLEEING);
                aiFlags &= ~AiFlag.HasSpokenFlee;
            }

            // update the flags
            critter.SetCritterFlags(critterFlags);
            critter.AiFlags = aiFlags;

            if (aiFightStatus != AiFightStatus.FIGHTING)
            {
                if (aiFightStatus == AiFightStatus.FINDING_HELP && (aiFlags & AiFlag.FindingHelp) == default)
                {
                    critter.SetObject(obj_f.npc_combat_focus, target);
                    aiFlags |= AiFlag.FindingHelp;
                    critter.AiFlags = aiFlags;
                }

                if ((npcFlags & NpcFlag.BACKING_OFF) != default)
                {
                    npcFlags &= ~(NpcFlag.BACKING_OFF);
                    critter.SetNPCFlags(npcFlags);
                }

                if ((aiFlags & AiFlag.Fighting) != default)
                {
                    aiFlags &= ~AiFlag.Fighting;
                    critter.AiFlags = aiFlags;
                    if (aiFightStatus != AiFightStatus.FLEEING)
                    {
                        critter.SetNPCFlags(npcFlags | NpcFlag.DEMAINTAIN_SPELLS);
                    }

                    GameSystems.Script.ExecuteObjectScript(target, critter, ObjScriptEvent.ExitCombat);
                }

                return aiFightStatus;
            }

            // AiFightStatus.FIGHTING
            if ((critterFlags2 & CritterFlag2.TARGET_LOCK) != default)
            {
                return AiFightStatus.FIGHTING;
            }

            if ((aiFlags & AiFlag.Fighting) == default)
            {
                aiFlags |= AiFlag.CheckWield | AiFlag.CheckGrenade | AiFlag.Fighting;
                critter.AiFlags = aiFlags;
                GameSystems.Script.ExecuteObjectScript(target, critter, ObjScriptEvent.EnterCombat);
                if ((critter.GetFlags() & ObjectFlag.OFF) != default)
                {
                    return AiFightStatus.NONE;
                }

                soundMap = GameSystems.SoundMap.GetCritterSoundEffect(critter, CritterSoundEffect.Unk5);
            }

            if (critter.IsPC())
            {
                // set reaction level to hostility level
                var reactionLvl = GameSystems.Reaction.GetReaction(critter, target);
                var aiParams = GetAiParams(critter);
                if (reactionLvl > aiParams.hostilityThreshold)
                {
                    GameSystems.Reaction.AdjustReaction(critter, target, aiParams.hostilityThreshold - reactionLvl);
                }
            }

            critter.SetObject(obj_f.npc_combat_focus, target);

            if (!isAlertingAllies)
            {
                isAlertingAllies = true;
                AlertAllies(target, critter, 1);
                isAlertingAllies = false;
            }

            return aiFightStatus;
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

        [TempleDllLocation(0x1005d890)]
        private void AlertAllies(GameObjectBody handle, GameObjectBody alertFrom, int rangeIdx)
        {
            var range = rangeTiles[rangeIdx];
            var tileDelta = alertFrom.GetLocation().EstimateDistance(handle.GetLocation());

            int aiTileDeltaMax = 20;

            // alert around the attacked critter (alertFrom object)
            if (tileDelta < 2 * aiTileDeltaMax)
            {
                using var objList = ObjList.ListRangeTiles(alertFrom, range, ObjectListFilter.OLC_NPC);
                foreach (var resHandle in objList)
                {
                    if (rangeIdx != 3 || GameSystems.Combat.HasLineOfAttack(handle, resHandle))
                    {
                        AlertAlly(resHandle, alertFrom, handle, rangeIdx);
                    }
                }
            }

            // alert around the attacker
            if (tileDelta > 1)
            {
                using var objList = ObjList.ListRangeTiles(handle, range, ObjectListFilter.OLC_NPC);
                foreach (var resHandle in objList)
                {
                    if (rangeIdx != 3 || GameSystems.Combat.HasLineOfAttack(handle, resHandle))
                    {
                        AlertAlly(resHandle, alertFrom, handle, rangeIdx);
                    }
                }
            }
        }

        [TempleDllLocation(0x1005d6f0)]
        private void AlertAlly(GameObjectBody handle, GameObjectBody alertFrom, GameObjectBody alertDispatcher, int rangeIdx)
        {
            if (handle == alertDispatcher || handle == alertFrom)
                return;

            if (GetAllegianceStrength(handle, alertDispatcher) != 0)
            {
                if (HasLineOfSight(handle, alertDispatcher) == 0
                    || CannotHear(handle, alertDispatcher, rangeIdx) == 0 || rangeIdx == 3)
                {
                    FightStatusProcess(handle, alertFrom);
                }

                return;
            }

            if (GetAllegianceStrength(handle, alertFrom) != 0)
            {
                if (HasLineOfSight(handle, alertFrom) == 0
                    || CannotHear(handle, alertFrom, rangeIdx) == 0)
                {
                    FightStatusProcess(handle, alertDispatcher);
                }

                return;
            }

            // the code below caused problems when attacking Mickey in the Tavern (made everyone go hostile on you)
            /*
             if ( !( handle.GetInt32(obj_f.critter_flags)  CritterFlag.NO_FLEE)){
                AiFightStatus aifs;
                GetAiFightStatus(handle, &aifs, null);
                if (aifs == AiFightStatus.NONE){
                    if (!GameSystems.Critter.HasLineOfSight(handle, alertDispatcher)
                        || !CannotHear(handle, alertDispatcher, rangeIdx)){
                        UpdateAiFlags(handle, AiFightStatus.FLEEING, alertFrom, null);
                    }
                }
            }
             */
        }

        [TempleDllLocation(0x1005cd50)]
        private void FightStatusProcess(GameObjectBody obj, GameObjectBody newTgt)
{
	if (GameSystems.Critter.IsDeadNullDestroyed(obj)){
		return;
	}

    GameObjectBody CheckNewTgt(GameObjectBody _obj, GameObjectBody _curTgt, GameObjectBody _newTgt)
    {
        if (_curTgt == null)
            return _newTgt;
        if (_newTgt == null || !_newTgt.IsCritter())
            return _curTgt;
        if (!_curTgt.IsCritter()
            || !GameSystems.Critter.IsDeadOrUnconscious(_newTgt) && GameSystems.Critter.IsDeadOrUnconscious(_curTgt)
            || _obj.DistanceToObjInFeet(_newTgt) <= 125.0f &&
            (_obj.DistanceToObjInFeet(_curTgt) > 125.0 || GameSystems.Critter.IsFriendly(_obj, _curTgt))
        )
            return _newTgt;
        return _curTgt;
    }

	bool WithinFleeDistance(GameObjectBody _obj, GameObjectBody _tgt)
	{
		if ((_obj.GetSpellFlags() & SpellFlag.MIND_CONTROLLED) != default)
			return true;
		AiParamPacket aiPar = GameSystems.AI.GetAiParams(_obj);
		var distTo = _obj.DistanceToObjInFeet(_tgt);
		return aiPar.fleeDistanceFeet < distTo;
	}

	GetAiFightStatus(obj, out var status, out var curTgt);
	switch (status ){
	case AiFightStatus.NONE:
		FightOrFlight(obj, newTgt);
		break;
	case AiFightStatus.FIGHTING:
		if (newTgt == curTgt || CheckNewTgt(obj, curTgt, newTgt) == newTgt)	{
			FightOrFlight(obj, newTgt);
		}
		break;
	case AiFightStatus.FLEEING:
		if (curTgt != newTgt && (curTgt == null || WithinFleeDistance(obj, curTgt)))
			FightOrFlight(obj, newTgt);
		break;
	case AiFightStatus.SURRENDERED:
		if (newTgt == curTgt || CheckNewTgt(obj, curTgt, newTgt) == newTgt) {
			FightOrFlight(obj, newTgt);
		} else
		{
			if (!GameSystems.Critter.IsDeadOrUnconscious(obj))
			{
				GameSystems.Dialog.GetFleeVoiceLine(obj, newTgt, out var fleeText, out var soundId);
				_showTextBubble(obj, newTgt, fleeText, soundId);
			}
			FleeProcess(obj, newTgt);
		}
		break;
	default:
		break;
	}

	if (!obj.IsOffOrDestroyed
		 && GameSystems.Combat.IsCombatActive() && !GameSystems.Critter.IsDeadNullDestroyed(obj)){
		GameSystems.D20.Initiative.AddToInitiative(obj);
	}
}

        [TempleDllLocation(0x1005c650)]
        private void FightOrFlight(GameObjectBody obj, GameObjectBody tgt)
        {
            if (ShouldFlee(obj, tgt))
            {
                UpdateAiFlags(obj, AiFightStatus.FLEEING, tgt);
            } else
            {
                UpdateAiFlags(obj, AiFightStatus.FIGHTING, tgt);
            }
        }
        
        [TempleDllLocation(0x1005c570)]
        public bool ShouldFlee(GameObjectBody obj, GameObjectBody target)
        {
            if ( (obj.GetCritterFlags() & CritterFlag.NO_FLEE) != default
                 || (obj.GetSpellFlags() & SpellFlag.MIND_CONTROLLED) != default )
            {
                return false;
            }

            var aiParamOut = GetAiParams(obj);

            // If the target is below a certain HP percentage, we might want to finish them off instead
            var targetHpPercent = GameSystems.Critter.GetHpPercent(target);
            if (targetHpPercent < aiParamOut.pcHpPercentToPreventFlee)
            {
                return false;
            }

            // If our HP is low, we might want to flee immediately
            if ( GameSystems.Critter.GetHpPercent(obj) <= aiParamOut.hpPercentToTriggerFlee )
            {
                return true;
            }

            // Check how many allies they have, and how many we have (and the sum of their effective level)
            GetAllyStrength(target, out var targetLevelCount, out var targetAllyCount);
            GetAllyStrength(obj, out var ourLevelCount, out var ourAllyCount);
            if (targetAllyCount - ourAllyCount >= aiParamOut.numPeopleToTriggerFlee)
            {
                return true;
            }

            // TODO: this is borked because the function above just returns counts, not levels
            return targetLevelCount - ourLevelCount >= aiParamOut.lvlDiffToTriggerFlee;
        }

        [TempleDllLocation(0x1005bec0)]
        public void GetAllyStrength(GameObjectBody obj, out int allyLevels, out int allyCount)
        {
            if ( obj.IsPC() )
            {
                CountFollowersAndSelf(out allyCount, out allyLevels, obj);
                return;
            }
            var leader = GameSystems.Critter.GetLeader(obj);
            if ( leader != null )
            {
                CountFollowersAndSelf(out allyCount, out allyLevels, leader);
            }
            else
            {
                allyCount = 1;
                allyLevels = 1;
            }
        }

        [TempleDllLocation(0x100590a0)]
        public void CountFollowersAndSelf(out int allyCount, out int levelCount, GameObjectBody obj)
        {
            allyCount = 1;
            levelCount = 1;
            foreach (var follower in GameSystems.Critter.EnumerateAllFollowers(obj))
            {
                ++allyCount;
                ++levelCount;
            }
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
                || GameSystems.D20.D20Query(aiHandle, D20DispatcherKey.QUE_Critter_Is_Charmed) )
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

                    if (GameSystems.D20.D20Query(triggerer, D20DispatcherKey.QUE_Critter_Is_Charmed) )
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
        internal AiParamPacket GetAiParams(GameObjectBody obj)
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
        internal bool ConsiderTarget(GameObjectBody obj, GameObjectBody tgt)
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
        internal bool IsCharmedBy(GameObjectBody critter, GameObjectBody charmer)
        {
            return GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Is_Charmed)                    && GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed) == charmer;
        }

        [TempleDllLocation(0x1005B7D0)]
        internal bool TargetIsPcPartyNotDead(GameObjectBody partyMember)
        {
            return partyMember.IsPC()
                   && PartyHasNoRemainingMembers() || GameSystems.Party.GetLivingPartyMemberCount() <= 1;
        }

        [TempleDllLocation(0x10057ca0)]
        private bool PartyHasNoRemainingMembers()
        {
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (!GameSystems.D20.D20Query(partyMember, D20DispatcherKey.QUE_Critter_Is_Charmed)                     && !GameSystems.Critter.IsDeadNullDestroyed(partyMember))
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
        internal GameObjectBody FindSuitableTarget(GameObjectBody handle)
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
                GetAiFightStatus(friendHandle, out var aifs, out var targetsFocus);

                //// Make AI ignore friend's target if it's unconscious (otherwise putting foes to sleep caused "enter combat" loops when more than 1 AI follower was in party)
                //if (targetsFocus && GameSystems.Critter.IsDeadOrUnconscious(targetsFocus)){
                //	return null;
                //}

                if (ConsiderTarget(handle, targetsFocus) &&
                    (aifs == AiFightStatus.FIGHTING || aifs == AiFightStatus.FLEEING ||
                     aifs == AiFightStatus.SURRENDERED))
                {
                    var allegianceStr = GetAllegianceStrength(handle, friendHandle);

                    if (allegianceStr != 0 && CannotHate(handle, targetsFocus, leader) == 0)
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
                || GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Deafened) )
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Is_Invisible)                 && !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible) )
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToUndead)                 && GameSystems.Critter.IsUndead(obj))
            {
                return 1000;
            }

            if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                    WellKnownSpells.InvisibilityToAnimals)                 && GameSystems.Critter.IsAnimal(obj))
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

        [TempleDllLocation(0x100592d0)]
        public LockStatus DryRunAttemptOpenContainer(GameObjectBody critter, GameObjectBody container)
        {
            if (container.ProtoId == 1000
                || GameSystems.MapObject.IsBusted(container)
                || container.type != ObjectType.container)
            {
                return LockStatus.PLS_OPEN;
            }

            var v2 = container.GetContainerFlags();
            if (v2.HasFlag(ContainerFlag.JAMMED))
            {
                return LockStatus.PLS_JAMMED;
            }

            if (v2.HasFlag(ContainerFlag.MAGICALLY_HELD))
            {
                return LockStatus.PLS_MAGICALLY_HELD;
            }

            if (!container.NeedsToBeUnlocked())
            {
                return LockStatus.PLS_OPEN;
            }

            var keyId = container.GetInt32(obj_f.container_key_id);
            return GameSystems.Item.HasKey(critter, keyId) ? LockStatus.PLS_OPEN : LockStatus.PLS_LOCKED;
        }

        [TempleDllLocation(0x1005be00)]
        private void AiTimeEventSchedule_Normal(GameObjectBody obj, TimeSpan delay)
        {
            GameSystems.TimeEvent.Remove(
                TimeEventType.AI,
                e => e.arg1.handle == obj
            );

            var evt = new TimeEvent(TimeEventType.AI);
            evt.arg1.handle = obj;
            evt.arg2.int32 = 0;
            GameSystems.TimeEvent.Schedule(evt, delay, out _);
        }


        [TempleDllLocation(0x1005b950)]
        public bool waypointsSthgsub_1005B950(GameObjectBody critter, bool immediate)
        {
            var aiFlags = critter.AiFlags;
            if (aiFlags.HasFlag(AiFlag.WaypointDelay))
            {
                return true;
            }

            var isSleeping = GameSystems.Critter.IsSleeping(critter);
            if (!isSleeping && (critter.GetFlags() & (ObjectFlag.DONTDRAW | ObjectFlag.OFF)) != default)
            {
                return false;
            }

            if (GameSystems.Waypoint.GetWaypointCount(critter) == 0)
            {
                return false;
            }

            var teleportMapId = GameSystems.Critter.GetTeleportMap(critter);
            var currentMapId = GameSystems.Map.GetCurrentMapId();
            if (teleportMapId != currentMapId && !immediate)
            {
                return false;
            }

            var npcFlags = critter.GetNPCFlags();
            if (GameSystems.TimeEvent.IsDaytime)
            {
                if (!npcFlags.HasFlag(NpcFlag.WAYPOINTS_DAY))
                {
                    return false;
                }
            }
            else if (!npcFlags.HasFlag(NpcFlag.WAYPOINTS_NIGHT))
            {
                return false;
            }

            if (!isSleeping)
            {
                var curWaypoint = critter.GetInt32(obj_f.npc_waypoint_current);
                var waypoint = GameSystems.Waypoint.GetWaypoint(critter, curWaypoint);

                if (GameSystems.Waypoint.CritterIsAtWaypoint(critter, waypoint))
                {
                    if (waypoint.HasFixedRotation)
                    {
                        GameSystems.MapObject.SetRotation(critter, waypoint.Rotation);
                    }

                    if (waypoint.HasAnimations)
                    {
                        var animIdx = critter.GetInt32(obj_f.npc_waypoint_anim);
                        if (animIdx < waypoint.Anims.Length)
                        {
                            var animId = waypoint.Anims[animIdx];
                            critter.AiFlags |= AiFlag.WaypointDelay;
                            GameSystems.Anim.PushAnimate(critter, animId);
                            critter.SetInt32(obj_f.npc_waypoint_anim, animIdx + 1);
                            return true;
                        }
                    }

                    if (waypoint.HasDelay && (aiFlags & AiFlag.WaypointDelayed) == 0)
                    {
                        CritterWaypointDelay(critter, waypoint.Delay);
                        return true;
                    }

                    // Advance to the next waypoint
                    var nextWaypoint = curWaypoint + 1;
                    if (nextWaypoint == GameSystems.Waypoint.GetWaypointCount(critter))
                    {
                        nextWaypoint = 0;
                    }

                    critter.AiFlags &= AiFlag.WaypointDelayed;
                    critter.SetInt32(obj_f.npc_waypoint_current, nextWaypoint);
                    critter.SetInt32(obj_f.npc_waypoint_anim, 0);
                    waypoint = GameSystems.Waypoint.GetWaypoint(critter, nextWaypoint);
                }

                if (immediate)
                {
                    if (teleportMapId == currentMapId)
                    {
                        GameSystems.Anim.Interrupt(critter, AnimGoalPriority.AGP_4);
                        GameSystems.MapObject.Move(critter, waypoint.Location);
                        return true;
                    }
                }
                else
                {
                    NpcWander_10058590(critter, waypoint.Location, 2 * locXY.INCH_PER_TILE);
                }
            }

            return true;
        }

        [TempleDllLocation(0x10058590)]
        public void NpcWander_10058590(GameObjectBody critter, LocAndOffsets target, float dist)
        {
            if (dist > locXY.INCH_PER_HALFTILE)
            {
                GameSystems.Anim.PushMoveNearTile(
                    critter,
                    target,
                    (int) (dist / locXY.INCH_PER_TILE)
                );
            }
            else
            {
                GameSystems.Anim.PushMoveToTile(critter, target);
            }
        }

        [TempleDllLocation(0x10058430)]
        public void CritterWaypointDelay(GameObjectBody critter, TimeSpan delay)
        {
            TimeEvent evt = new TimeEvent(TimeEventType.AI);
            critter.AiFlags |= AiFlag.WaypointDelay;
            evt.arg1.handle = critter;
            GameSystems.TimeEvent.Schedule(evt, delay, out _);
        }

        [TempleDllLocation(0x1005f090)]
        public void ExpireTimeEvent(GameObjectBody critter, bool isFirstHeartbeat)
        {
            var canDoHeartbeat = true;
            var isReschedule = true;

            if (!GameSystems.Critter.IsDeadNullDestroyed(critter) && isFirstHeartbeat)
            {
                critter.AiFlags |= AiFlag.CheckWield;

                // NPC Wander off
                if (!GameSystems.Party.IsInParty(critter)
                    && !GameSystems.Critter.IsCombatModeActive(critter)
                    && !waypointsSthgsub_1005B950(critter, true))
                {
                    NpcWander_1005BC00(critter, true); // NPC Wander
                }

                if (GameSystems.Script.ExecuteObjectScript(critter, critter, ObjScriptEvent.FirstHeartbeat) != 1)
                {
                    canDoHeartbeat = false;
                }
            }

            if (canDoHeartbeat)
            {
                canDoHeartbeat = CanDoHeartbeat(critter);
            }

            if (canDoHeartbeat)
            {
                if (GameSystems.Script.ExecuteObjectScript(critter, critter, ObjScriptEvent.Heartbeat) == 1)
                {
                    if (critter.GetNPCFlags().HasFlag(NpcFlag.GENERATOR))
                    {
                        if (GameSystems.MonsterGen.GetNextEventTime(critter, out var delay))
                        {
                            AiTimeEventSchedule_Normal(critter, delay);
                            return;
                        }
                        else
                        {
                            isReschedule = false;
                        }
                    }
                    else
                    {
                        AiProcess(critter);
                    }
                }
            }
            else
            {
                if (!GameSystems.Combat.IsCombatActive() && !GameSystems.Critter.IsDeadNullDestroyed(critter))
                {
                    GameSystems.Anim.Interrupt(critter, AnimGoalPriority.AGP_4);
                }

                var secLoc = new SectorLoc(critter.GetLocation());
                var sectorLoaded = GameSystems.MapSector.IsSectorLoaded(secLoc);
                if (!MoveToLeader(critter) && !sectorLoaded)
                {
                    isReschedule = false;
                }
            }


            if (!isReschedule)
            {
                if (!GameSystems.Combat.IsCombatActive())
                {
                    if ((critter.GetCritterFlags() & CritterFlag.ENCOUNTER) != default)
                    {
                        GameSystems.Object.Destroy(critter);
                    }
                }

                return;
            }

            // Reschedule AI event with fixed delay based on distance from party + optional random delay
            var aiEvtDelay = GetAiEventDelay(critter); // 250ms-5000ms
            if (isFirstHeartbeat)
            {
                aiEvtDelay += TimeSpan.FromMilliseconds(GameSystems.Random.GetInt(0, 5000));
            }

            AiTimeEventSchedule_Normal(critter, aiEvtDelay);
        }

        /// <summary>
        /// Determines the rate of AI updates based on distance to party.
        /// Minimum update rate is 250ms, maximum is 5000ms at a distance of 60 tiles.
        /// </summary>
        [TempleDllLocation(0x10058850)]
        public TimeSpan GetAiEventDelay(GameObjectBody critter)
        {
            var distToParty = GameSystems.Party.DistanceToParty(critter);
            if (distToParty < 0 || distToParty > 60)
            {
                distToParty = 60;
            }

            if (distToParty < 0 || distToParty > 60)
            {
                distToParty = 60;
            }

            var distanceBasedDelay = (int) (distToParty / 60.0f * 4750);

            return TimeSpan.FromMilliseconds(250 + distanceBasedDelay);
        }

        [TempleDllLocation(0x10058780)]
        public bool MoveToLeader(GameObjectBody critter)
        {
            if (GameSystems.Critter.IsDeadNullDestroyed(critter) || GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            var leader = GameSystems.Critter.GetLeaderRecursive(critter);
            if (leader == null)
            {
                return false;
            }

            if (leader.GetLocation().EstimateDistance(critter.GetLocation()) <= 60)
            {
                return false;
            }

            if ((critter.GetNPCFlags() & NpcFlag.AI_WAIT_HERE) != default
                || (critter.GetCritterFlags() & CritterFlag.PARALYZED) != default
                || GameSystems.Critter.IsDeadOrUnconscious(critter))
            {
                return false;
            }

            GameSystems.Anim.Interrupt(critter, AnimGoalPriority.AGP_4);
            GameSystems.MapObject.Move(critter, leader.GetLocationFull());
            return true;
        }

        [TempleDllLocation(0x10058730)]
        private bool CanDoHeartbeat(GameObjectBody critter)
        {
            if (GameSystems.Critter.IsDeadNullDestroyed(critter) || GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            var distanceToParty = GameSystems.Party.DistanceToParty(critter);
            if (distanceToParty > 60)
            {
                return false;
            }

            return !GameUiBridge.IsWorldmapMakingTrip();
        }

        [TempleDllLocation(0x1005bc00)]
        internal bool NpcWander_1005BC00(GameObjectBody critter, bool flagSthg)
        {
            var isSleeping = GameSystems.Critter.IsSleeping(critter);
            if (!isSleeping && (critter.GetFlags() & (ObjectFlag.DONTDRAW | ObjectFlag.OFF)) != default)
            {
                return false;
            }

            if (!GetCurrentStandpoint(critter, out var standPointLoc))
            {
                return false;
            }

            var critterMap = GameSystems.Critter.GetTeleportMap(critter);
            var curMap = GameSystems.Map.GetCurrentMapId();
            if (critterMap != curMap)
            {
                if (!flagSthg)
                {
                    return false;
                }
            }
            else
            {
                if (!flagSthg)
                {
                    if (GameSystems.TimeEvent.HourOfDay == 6)
                    {
                        if (GameSystems.Random.GetInt(1, 1000) != 1)
                        {
                            return false;
                        }
                    }
                    else if (GameSystems.TimeEvent.HourOfDay == 18 && GameSystems.Random.GetInt(1, 1000) != 1)
                    {
                        return false;
                    }
                }
            }

            var currentLoc = critter.GetLocation();
            var npcFlags = critter.GetNPCFlags();
            var wanderDistance = (npcFlags & (NpcFlag.WANDERS | NpcFlag.WANDERS_IN_DARK)) != 0 ? 4 : 1;
            if (standPointLoc.EstimateDistance(currentLoc) > wanderDistance)
            {
                if (!isSleeping)
                {
                    if (flagSthg)
                    {
                        if (critterMap == curMap)
                        {
                            GameSystems.Anim.Interrupt(critter, AnimGoalPriority.AGP_4);
                            GameSystems.MapObject.Move(critter, new LocAndOffsets(standPointLoc));
                            return true;
                        }
                    }
                    else
                    {
                        NpcWander_10058590(critter, new LocAndOffsets(standPointLoc), 1.0f);
                    }
                }
            }
            else if (!isSleeping)
            {
                if ((npcFlags & NpcFlag.WANDERS) != 0)
                {
                    GameSystems.Anim.PushWander(critter, standPointLoc, 4);
                    return true;
                }

                if ((npcFlags & NpcFlag.WANDERS_IN_DARK) != 0)
                {
                    GameSystems.Anim.PushWanderSeekDarkness(critter, standPointLoc, 4);
                    return true;
                }

                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1007a720)]
        internal void GetAiSpells(out AiSpellList aiSpell, GameObjectBody obj, AiSpellType aiSpellType)
        {
            aiSpell = new AiSpellList();
            aiSpell.spellEnums = new List<int>();
            aiSpell.spellData = new List<D20SpellData>();

            var objBod = obj;
            {
                var spellsMemo = objBod.GetSpellArray(obj_f.critter_spells_memorized_idx);
                for (var i = 0; i < spellsMemo.Count; i++)
                {
                    var spellData = spellsMemo[i];
                    if ((spellData.spellStoreState.usedUp & 1) != 0)
                    {
                        continue;
                    }

                    if (!GameSystems.Spell.TryGetSpellEntry(spellData.spellEnum, out var spellEntry))
                    {
                        continue;
                    }

                    if (!spellEntry.HasAiType(aiSpellType))
                    {
                        continue;
                    }

                    var spellAlreadyFound = aiSpell.spellEnums.Contains(spellData.spellEnum);
                    if (!spellAlreadyFound)
                    {
                        aiSpell.spellEnums.Add(spellData.spellEnum);

                        var d20SpellData = new D20SpellData();
                        d20SpellData.spellEnumOrg = spellData.spellEnum;
                        d20SpellData.spellClassCode = spellData.classCode;
                        d20SpellData.metaMagicData = spellData.metaMagicData;
                        d20SpellData.itemSpellData = -1;
                        d20SpellData.spellSlotLevel =
                            spellData.spellLevel; // hey, I think this was missing / wrong in the original code!

                        aiSpell.spellData.Add(d20SpellData);
                    }
                }
            }

            var spellsKnown = objBod.GetSpellArray(obj_f.critter_spells_known_idx);
            for (var i = 0; i < spellsKnown.Count; i++)
            {
                var spellData = spellsKnown[i];
                var spEnum = spellData.spellEnum;
                if (aiSpell.spellEnums.Contains(spEnum))
                {
                    continue;
                }

                if (GameSystems.Spell.TryGetSpellEntry(spEnum, out var spellEntry))
                {
                    continue;
                }

                if (!spellEntry.HasAiType(aiSpellType))
                {
                    continue;
                }

                if (GameSystems.Spell.IsDomainSpell(spellData.classCode))
                {
                    continue;
                }

                var casterClass = GameSystems.Spell.GetCastingClass(spellData.classCode);
                if (D20ClassSystem.IsVancianCastingClass(casterClass))
                {
                    continue;
                }

                if (!GameSystems.Spell.spellCanCast(obj, spEnum, spellData.classCode, spellData.spellLevel))
                {
                    continue;
                }

                if (GameSystems.Spell.IsNaturalSpellsPerDayDepleted(obj, spellData.spellLevel, spellData.classCode))
                {
                    continue;
                }

                aiSpell.spellEnums.Add(spellData.spellEnum);
                var d20SpellData = new D20SpellData();
                d20SpellData.spellEnumOrg = spellData.spellEnum;
                d20SpellData.spellClassCode = spellData.classCode;
                d20SpellData.metaMagicData = spellData.metaMagicData;
                d20SpellData.itemSpellData = -1;
                d20SpellData.spellSlotLevel =
                    spellData.spellLevel; // hey, I think this was missing / wrong in the original code!
                aiSpell.spellData.Add(d20SpellData);
            }
        }

        [TempleDllLocation(0x1005a3a0)]
        internal bool ChooseRandomSpellFromList(AiPacket aiPkt, ref AiSpellList aiSpells)
        {
            if (aiSpells.spellEnums.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < 5; i++)
            {
                var spellIdx = GameSystems.Random.GetInt(0, aiSpells.spellEnums.Count - 1);

                var spellEnum = aiSpells.spellEnums[spellIdx];

                aiPkt.spellPktBod = new SpellPacketBody();
                var spellData = aiSpells.spellData[spellIdx];
                aiPkt.spellPktBod.spellEnum = aiSpells.spellEnums[spellIdx];
                aiPkt.spellPktBod.caster = aiPkt.obj;
                aiPkt.spellPktBod.spellEnumOriginal = spellEnum;
                aiPkt.spellPktBod.spellKnownSlotLevel = spellData.spellSlotLevel;
                aiPkt.spellPktBod.spellClass = spellData.spellClassCode;
                GameSystems.Spell.SpellPacketSetCasterLevel(aiPkt.spellPktBod);

                var spellEntry = GameSystems.Spell.GetSpellEntry(spellEnum);

                var spellRange = GameSystems.Spell.GetSpellRange(spellEntry, aiPkt.spellPktBod.casterLevel,
                    aiPkt.spellPktBod.caster);
                aiPkt.spellPktBod.spellRange = spellRange;
                if (spellEntry.IsBaseModeTarget(UiPickerType.Area) &&
                    spellEntry.spellRangeType == SpellRangeType.SRT_Personal
                    || spellEntry.IsBaseModeTarget(UiPickerType.Personal) &&
                    (spellEntry.flagsTargetBitmask & UiPickerFlagsTarget.Radius) != default)
                {
                    spellRange = spellEntry.radiusTarget;
                }

                var tgt = aiPkt.target;
                if (spellEntry.IsBaseModeTarget(UiPickerType.Personal))
                {
                    tgt = aiPkt.obj;
                }

                if (tgt != aiPkt.obj
                    && tgt.IsCritter()
                    && (GameSystems.D20.D20Query(tgt, D20DispatcherKey.QUE_Critter_Is_Grappling)                         || GameSystems.D20.D20Query(tgt, D20DispatcherKey.QUE_Critter_Is_Charmed)                     ))
                {
                    continue;
                }

                if (!GameSystems.Spell.spellCanCast(aiPkt.obj, spellEnum, spellData.spellClassCode,
                    spellData.spellSlotLevel))
                {
                    Logger.Debug("AiCheckSpells(): object {0} ({1}) cannot cast spell {2}",
                        GameSystems.MapObject.GetDisplayName(aiPkt.obj), aiPkt.obj.id.ToString(), spellEnum);
                    continue;
                }

                if (!GameSystems.Spell.GetSpellTargets(aiPkt.obj, tgt, aiPkt.spellPktBod, spellEnum))
                {
                    continue;
                }

                if (aiPkt.spellPktBod.targetListHandles.Length > 0)
                {
                    tgt = aiPkt.spellPktBod.targetListHandles[0];
                }

                if (aiPkt.obj.DistanceToObjInFeet(tgt) > spellRange &&
                    (spellEntry.HasAiType(AiSpellType.ai_action_offensive)
                     || spellEntry.HasAiType(AiSpellType.ai_action_defensive)))
                {
                    continue;
                }

                aiPkt.aiState2 = 1;
                aiPkt.spellEnum = spellEnum;
                aiPkt.spellData = spellData;
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10063f90)]
        private bool IsWorthless(GameObjectBody item)
        {
            return item.GetInt32(obj_f.item_worth) == 0;
        }

        [TempleDllLocation(0x1005b1d0)]
        internal bool LookForAndPickupItem(GameObjectBody obj, ObjectType wantedType)
        {
            if (GameSystems.Critter.IsSavage(obj))
            {
                return false;
            }

            var filter = ObjList.GetFromType(wantedType);

            // Find the closest suitable item we could pick up directly
            using var itemsOnTheGround = ObjList.ListRangeTiles(obj, 10, filter);
            GameObjectBody closestItem = null;
            float closestItemDistance = float.MaxValue;
            LocAndOffsets closestItemLocation = LocAndOffsets.Zero;

            foreach (var suitableItem in itemsOnTheGround)
            {
                if (ShouldPickUpItem(obj, suitableItem))
                {
                    var distanceTo = obj.DistanceToObjInFeet(suitableItem);
                    if (closestItem == null || distanceTo < closestItemDistance)
                    {
                        closestItem = suitableItem;
                        closestItemDistance = distanceTo;
                        closestItemLocation = suitableItem.GetLocationFull();
                    }
                }
            }

            // This seems to search for junk-piles (which as far as I know are not used in ToEE)
            using var containerList = ObjList.ListRangeTiles(obj, 10, ObjectListFilter.OLC_CONTAINER);
            foreach (var junkPile in containerList)
            {
                if (junkPile.ProtoId != 1000)
                {
                    continue;
                }

                // Only consider a junk pile that is actually closer than the currently closest item
                var distToJunkPile = obj.DistanceToObjInFeet(junkPile);
                if (distToJunkPile >= closestItemDistance)
                {
                    continue;
                }

                // Check for suitable items within the junk pile
                foreach (var containedItem in junkPile.EnumerateChildren())
                {
                    if (containedItem.type == wantedType && ShouldPickUpItem(obj, containedItem))
                    {
                        closestItem = containedItem;
                        closestItemDistance = distToJunkPile;
                        closestItemLocation = junkPile.GetLocationFull();
                        break;
                    }
                }
            }

            if (closestItem == null)
            {
                // No suitable item was found
                return false;
            }

            // Queue an action to pick up the item
            if (GameSystems.D20.Actions.TurnBasedStatusInit(obj))
            {
                GameSystems.D20.Actions.CurSeqReset(obj);
                GameSystems.D20.Actions.GlobD20ActnInit();
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.PICKUP_OBJECT, 0);
                GameSystems.D20.Actions.GlobD20ActnSetTarget(closestItem, closestItemLocation);
                GameSystems.D20.Actions.ActionAddToSeq();
                GameSystems.D20.Actions.sequencePerform();
            }

            return true;
        }

        private bool ShouldPickUpItem(GameObjectBody critter, GameObjectBody item)
        {
            if ((item.GetItemFlags() & ItemFlag.NO_NPC_PICKUP) != default)
            {
                return false;
            }

            if (IsWorthless(item))
            {
                return false;
            }

            var insertLocation = 0;
            if (GameSystems.Item.ItemInsertGetLocation(item, critter, ref insertLocation, null, 0) != ItemErrorCode.OK)
            {
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1005b810)]
        internal bool ChooseRandomSpell(AiPacket aiPkt)
        {
            if (!IsNpcFightingAllowed)
            {
                return false;
            }

            var obj = aiPkt.obj;

            if (GameSystems.Critter.GetNumFollowers(aiPkt.obj, false) == 0)
            {
                GetAiSpells(out var aiSpell, obj, AiSpellType.ai_action_summon);
                if (ChooseRandomSpellFromList(aiPkt, ref aiSpell))
                {
                    return true;
                }
            }

            var aiDataIdx = obj.GetInt32(obj_f.npc_ai_data);
            Trace.Assert(aiDataIdx >= 0 && aiDataIdx <= 150);

            var aiParam = aiParams[aiDataIdx];
            if (aiParam.defensiveSpellChance > GameSystems.Random.GetInt(1, 100))
            {
                GetAiSpells(out var aiSpell, obj, AiSpellType.ai_action_defensive);
                if (ChooseRandomSpellFromList(aiPkt, ref aiSpell))
                {
                    return true;
                }
            }

            if (aiParam.offensiveSpellChance > GameSystems.Random.GetInt(1, 100))
            {
                GetAiSpells(out var aiSpell, obj, AiSpellType.ai_action_offensive);
                if (ChooseRandomSpellFromList(aiPkt, ref aiSpell))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1005a070)]
        internal void AiListRemove(GameObjectBody critter, GameObjectBody target, int aiType)
        {
            var aiList = critter.GetObjectArray(obj_f.npc_ai_list_idx);
            int aiListCount = aiList.Count;
            var lastIdx = aiListCount - 1;
            for (int i = 0; i < aiListCount; i++)
            {
                var aiListItem = critter.GetObject(obj_f.npc_ai_list_idx, i);
                var aiListItemType = critter.GetInt32(obj_f.npc_ai_list_type_idx, i);

                if (!(aiListItem == target && aiListItemType == aiType || aiListItem == null))
                    continue;

                // TODO: I think shuffling around the items might not be needed anymore when using removeobject -> check

                if (i < lastIdx)
                {
                    var lastItem = critter.GetObject(obj_f.npc_ai_list_idx, lastIdx);
                    var lastItemType = critter.GetInt32(obj_f.npc_ai_list_type_idx, lastIdx);
                    critter.SetObject(obj_f.npc_ai_list_idx, i, lastItem);
                    critter.SetInt32(obj_f.npc_ai_list_type_idx, i, lastItemType);
                }

                critter.RemoveObject(obj_f.npc_ai_list_idx, lastIdx);
                critter.RemoveInt32(obj_f.npc_ai_list_type_idx, lastIdx--);
                aiListCount--;
                i--;
            }
        }

        [TempleDllLocation(0x1005a190)]
        internal void TargetLockUnset(GameObjectBody critter)
        {
            var critFlags2 = critter.GetCritterFlags2();
            if ((critFlags2 & CritterFlag2.TARGET_LOCK) != default)
            {
                critFlags2 &= ~CritterFlag2.TARGET_LOCK;
                critter.SetCritterFlags2(critFlags2);
            }
        }
        
        [TempleDllLocation(0x10058a30)]
        [TempleDllLocation(0x10058ae0)]
        internal bool RefuseFollowCheck(GameObjectBody critter, GameObjectBody leader)
        {
            // NPC is being mind-controlled
            if ((critter.GetSpellFlags() & SpellFlag.MIND_CONTROLLED) != default
                && GameSystems.Critter.GetLeaderRecursive(critter) != null)
            {
                return false;
            }

            // The original vanilla function might have tried to check for leader here
            // but the result of the function call wasn't used

            // It's forced to be a follower
            if ((critter.GetNPCFlags() & NpcFlag.FORCED_FOLLOWER) != default)
            {
                return false;
            }

            var aiParams = GetAiParams(critter);
            var reactionLvl = GameSystems.Reaction.GetReaction(critter, leader);
            return reactionLvl <= aiParams.reactionLvlToRefuseFollowingPc;
        }

        [TempleDllLocation(0x100e5290)]
        internal bool AiStrategDefaultCast(GameObjectBody critter, GameObjectBody target, D20SpellData spellData,
            SpellPacketBody spellPkt)
        {
            GameSystems.Combat.EnterCombat(critter);

            var critterStratIdx = critter.GetInt32(obj_f.critter_strategy);
            var aiStrat = _strategies.GetAiStrategy(critterStratIdx);

            if (!GameSystems.D20.Actions.TurnBasedStatusInit(critter))
                return false;

            GameSystems.D20.Actions.CurSeqReset(critter);
            GameSystems.D20.Actions.GlobD20ActnInit();

            var aiTac = new AiTactic(critter, target);

            // loop through tactics defined in strategy.tab
            for (int i = 0; i < aiStrat.numTactics; i++)
            {
                aiStrat.GetConfig(i, aiTac);
                Logger.Info("AiStrategyDefaultCast: \t {0} attempting {1}...", critter, aiTac.aiTac.name);

                if (aiTac.aiTac.aiFunc(aiTac))
                {
                    Logger.Info("AiStrategyDefaultCast: \t AI tactic succeeded; performing.");
                    GameSystems.D20.Actions.sequencePerform();
                    return true;
                }
            }

            Logger.Info("AiStrategyDefaultCast: \t AI tactics failed, trying DefaultCast.");
            aiTac.d20SpellData = spellData;
            aiTac.aiTac = TacticDefaultCast.Instance; // default spellcast
            aiTac.tacticIdx = -1;
            aiTac.spellPktBody = spellPkt;

            if (aiTac.aiTac.aiFunc(aiTac))
            {
                Logger.Info("AiStrategyDefaultCast: \t DefaultCast succeeded; performing.");
                GameSystems.D20.Actions.sequencePerform();
                return true;
            }

            Logger.Info("AiStrategyDefaultCast: \t DefaultCast failed, trying to perform again with initial target.");
            aiTac.target = target;
            if (aiTac.aiTac.aiFunc(aiTac))
            {
                Logger.Info("AiStrategyDefaultCast: \t DefaultCast succeeded; performing.");
                GameSystems.D20.Actions.sequencePerform();
                return true;
            }

            // if nothing else, try to breakfree
            if (GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Is_BreakFree_Possible) )
            {
                Logger.Info("AiStrategy: \t {0} attempting to break free...", critter);
                if (BreakFree(aiTac))
                {
                    GameSystems.D20.Actions.sequencePerform();
                    return true;
                }
            }

            return false;
        }

        private bool BreakFree(AiTactic aiTac)
        {
            GameObjectBody performer = aiTac.performer;

            var currentActionNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;
            if (!GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_Is_BreakFree_Possible) )
            {
                return false;
            }

            int spellId =
                (int) GameSystems.D20.D20QueryReturnData(performer, D20DispatcherKey.QUE_Is_BreakFree_Possible);

            var performerLoc = aiTac.performer.GetLocationFull();
            if (GetTargetForBreakFree(aiTac.performer, performerLoc, out var target, out _))
            {
                if (GameSystems.Combat.IsWithinReach(aiTac.performer, target))
                {
                    return false;
                }
            }

            GameSystems.D20.Actions.CurSeqReset(aiTac.performer);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.BREAK_FREE, spellId);
            //GameSystems.D20.GlobD20ActnSetTarget(aiTac.performer, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            if (GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() != ActionErrorCode.AEC_OK)
            {
                GameSystems.D20.Actions.ActionSequenceRevertPath(currentActionNum);
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x100e2b80)]
        private bool GetTargetForBreakFree(GameObjectBody obj, LocAndOffsets loc, out GameObjectBody target,
            out float targetDistance)
        {
            targetDistance = 10000.0f;
            target = null;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (GameSystems.Critter.IsDeadOrUnconscious(combatant))
                {
                    continue;
                }

                if (GameSystems.Critter.IsFriendly(obj, combatant))
                {
                    // Skip tagets friendly with the caster
                    continue;
                }

                if (IsCharmedBy(combatant, obj)
                    && TargetIsPcPartyNotDead(combatant))
                {
                    // Skip targets charmed by the caster
                    continue;
                }

                if (combatant.HasFlag(ObjectFlag.INVULNERABLE))
                {
                    continue;
                }

                var dist = combatant.DistanceToLocInFeet(loc);
                if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                    !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                {
                    dist = (dist + 5.0f) * 2.5f;
                }

                if (dist < targetDistance)
                {
                    targetDistance = dist;
                    target = combatant;
                }
            }

            return target != null;
        }

        [TempleDllLocation(0x100e29c0)]
        internal bool AiFiveFootStepAttempt(AiTactic aiTac)
        {
            var actNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;

            var threateners = GameSystems.Combat.GetEnemiesCanMelee(aiTac.performer);
            if (threateners.Count == 0)
            {
                return true;
            }

            // check if those threateners are ignorable
            var shouldIgnoreThreateners = true;
            var args = new object[2];
            args[0] = aiTac.performer;
            foreach (var threatener in threateners)
            {
                args[1] = threatener;

                var ignoreTarget = GameSystems.Script.ExecuteScript<bool>("combat", "ShouldIgnoreTarget", args);

                if (!ignoreTarget)
                {
                    shouldIgnoreThreateners = false;
                    break;
                }
            }

            if (shouldIgnoreThreateners)
            {
                return true;
            }

            // got a reason to be afraid!
            var performerPos = aiTac.performer.GetLocationFull().ToInches2D();

            for (var angleDeg = 0.0f; angleDeg <= 360.0f; angleDeg += 45.0f)
            {
                var angleRad = Angles.ToRadians(angleDeg);
                var cosTheta = MathF.Cos(angleRad);
                var sinTheta = MathF.Sin(angleRad);
                var fiveFootStepX = performerPos.X - cosTheta * 60.0f; // five feet radius
                var fiveFootStepY = performerPos.Y + sinTheta * 60.0f;
                var fiveFootLoc = LocAndOffsets.FromInches(fiveFootStepX, fiveFootStepY);
                if (!GameSystems.D20.Combat.HasThreateningCrittersAtLoc(aiTac.performer, fiveFootLoc))
                {
                    GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.FIVEFOOTSTEP, 0);
                    GameSystems.D20.Actions.GlobD20ActnSetTarget(null, fiveFootLoc);
                    if (GameSystems.D20.Actions.ActionAddToSeq() == ActionErrorCode.AEC_OK
                        && GameSystems.D20.Actions.GetPathTargetLocFromCurD20Action(out fiveFootLoc)
                        && !GameSystems.D20.Combat.HasThreateningCrittersAtLoc(aiTac.performer, fiveFootLoc)
                        && GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() ==
                        ActionErrorCode.AEC_OK)
                        return true;
                    GameSystems.D20.Actions.ActionSequenceRevertPath(actNum);
                }
            }

            return false;
        }


        internal bool Is5FootStepWorth(AiTactic aiTac)
        {
            // when is it worth taking a 5' step to your target?

            if (aiTac.target == null)
                return false;

            var initialActNum = GameSystems.D20.Actions.CurrentSequence.d20ActArrayNum;

            // a. when you've used up your full round action and it's the only thing left to do
            var tbStat = GameSystems.D20.Actions.CurrentSequence.tbStatus;
            if (tbStat.hourglassState == HourglassState.EMPTY &&
                (tbStat.tbsFlags & (TurnBasedStatusFlags.Moved | TurnBasedStatusFlags.Moved5FootStep)) == 0)
            {
                return true;
            }

            // b. when you can path to it in a 5' step (and thus let you take advantage of full attack)
            // check if 5' step is possible, and if so whether you can reach the target with it
            var distToTgt = aiTac.performer.DistanceToObjInFeet(aiTac.target);
            var canReachTgtWithStep = false;
            {
                var canDoStep = false;
                GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.FIVEFOOTSTEP, 0);
                GameSystems.D20.Actions.GlobD20ActnSetTarget(aiTac.target, null);

                if (GameSystems.D20.Actions.ActionAddToSeq() == ActionErrorCode.AEC_OK
                    && GameSystems.D20.Actions.ActionSequenceChecksWithPerformerLocation() == ActionErrorCode.AEC_OK)
                {
                    canDoStep = true;

                    // Access the fake action we've just added to the sequence
                    var fiveFootAction = GameSystems.D20.Actions.CurrentSequence.d20ActArray[initialActNum];

                    // Check whether the action has a path and that path is not truncated
                    var path = fiveFootAction.path;
                    if (path != null && (fiveFootAction.d20Caf & D20CAF.TRUNCATED) == default)
                    {
                        if (distToTgt <= aiTac.performer.GetReach(D20ActionType.STANDARD_ATTACK) + 5.0f)
                        {
                            canReachTgtWithStep = true;
                        }
                    }
                }

                GameSystems.D20.Actions.ActionSequenceRevertPath(initialActNum);
                if (!canDoStep)
                {
                    return false;
                }
            }

            // if target is reachable with 5' step, then hell yeah!
            if (canReachTgtWithStep)
            {
                return true;
            }

            // c. when approaching your target otherwise would incur AoOs (regard tumbling here...)
            // todo

            // todo advanced - consider spring attack...

            return false;
        }

        [TempleDllLocation(0x100e2b80)]
        public bool TargetClosestEnemy(AiTactic aiTac, bool allowCoupDeGrace)
        {
            var currentClosest = 10000.0f;
            var foundTarget = false;

            foreach (var combatant in GameSystems.D20.Initiative)
            {
                if (combatant == aiTac.performer)
                {
                    continue;
                }

                if (GameSystems.Critter.IsFriendly(aiTac.performer, combatant))
                {
                    continue;
                }

                // Coup De Grace
                if (allowCoupDeGrace)
                {
                    if (!GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_CoupDeGrace) )
                    {
                        continue;
                    }
                }
                else
                {
                    if (GameSystems.Critter.IsDeadOrUnconscious(combatant))
                    {
                        continue;
                    }
                }

                var dist = aiTac.performer.DistanceToObjInFeet(combatant);

                if (GameSystems.D20.D20Query(combatant, D20DispatcherKey.QUE_Critter_Is_Invisible) &&
                    !GameSystems.D20.D20Query(aiTac.performer, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
                {
                    // Makes it less likely to be chosen
                    dist = (dist + 5.0f) * 2.5f;
                }

                if (dist < currentClosest)
                {
                    currentClosest = dist;
                    aiTac.target = combatant;
                    foundTarget = true;
                }
            }

            return foundTarget;
        }


    }
}