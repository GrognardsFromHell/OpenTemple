using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems;

public class CombatSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10AA8418)]
    private bool _active;

    public event Action<bool>? OnCombatStatusChanged;

    [TempleDllLocation(0x10AA8420)]
    private int combatSubturnTimeEvent;

    [TempleDllLocation(0x10AA8440)]
    private int combatTimeEventSthg;

    [TempleDllLocation(0x10AA8438)]
    private GameObject combatActor;

    [TempleDllLocation(0x10AA8444)]
    private bool combatTimeEventIndicator;

    /// <summary>
    /// Used to timeout NPC combat activity after 20 seconds.
    /// </summary>
    [TempleDllLocation(0x10AA8404)]
    private readonly ActionBar.ActionBar _aiTurnTimeout;

    [TempleDllLocation(0x10063ba0)]
    public CombatSystem()
    {
        _aiTurnTimeout = GameSystems.Vagrant.AllocateActionBar();
        _aiTurnTimeout.OnEndRampCallback = ActionBarReset;
    }

    [TempleDllLocation(0x10062e60)]
    private void ActionBarReset()
    {
        Logger.Info("\nGREYBAR RESET!\n");
        var currentActor = GameSystems.D20.Initiative.CurrentActor;
        if (currentActor != null)
        {
            if (!GameSystems.Party.IsPlayerControlled(currentActor))
            {
                GameSystems.D20.Actions.GreybarReset();

                // If the same actor is _still_ the actor, (no turn was ended), make sure
                // the watchdog is restarted
                if (currentActor == GameSystems.D20.Initiative.CurrentActor)
                {
                    RestartTurnTimeout();
                }
            }
        }
    }

    [TempleDllLocation(0x10062eb0)]
    public void Dispose()
    {
        CombatEnd(true);
    }

    [TempleDllLocation(0x10062ed0)]
    public void Reset()
    {
        CombatEnd(true);
    }

    [TempleDllLocation(0x10062440)]
    public void SaveGame(SavedGameState savedGameState)
    {
        savedGameState.CombatState = new SavedCombatState
        {
            InCombat = _active
        };
    }

    [TempleDllLocation(0x10062470)]
    public void LoadGame(SavedGameState savedGameState)
    {
        _active = savedGameState.CombatState.InCombat;
        OnCombatStatusChanged?.Invoke(_active);
    }

    [TempleDllLocation(0x10062e20)]
    public void AdvanceTime(TimePoint time)
    {
        if (GameSystems.Combat.AllPcsUnconscious() && GameSystems.Party.PartySize >= 1)
        {
            GameUiBridge.TotalPartyKill();
        }
    }

    [TempleDllLocation(0x100628d0)]
    public bool IsCombatActive()
    {
        return _active;
    }

    [TempleDllLocation(0x10AA844C)]
    private int _combatInitiative;

    [TempleDllLocation(0x100634e0)]
    public void AdvanceTurn(GameObject obj)
    {
        if (GameSystems.Map.HasFleeInfo() && GameSystems.Map.IsFleeing())
        {
            FleeFromCombat(obj);
        }

        if (!GameSystems.Combat.IsCombatActive())
        {
            return;
        }

        GameSystems.D20.Initiative.Sort();

        if (GameSystems.D20.Initiative.CurrentActor != obj &&
            !(GameSystems.D20.Actions.isSimultPerformer(obj) || GameSystems.D20.Actions.IsSimulsCompleted()))
        {
            Logger.Warn("Combat Advance Turn: Not {0}'s turn...", GameSystems.MapObject.GetDisplayName(obj));
            return;
        }

        if (GameSystems.D20.Actions.IsLastSimulsPerformer(obj))
        {
            Logger.Warn("Combat Advance Turn: Next turn waiting on simuls actions...");
            return;
        }

        _combatInitiative++;
        var curSeq = GameSystems.D20.Actions.CurrentSequence;
        Logger.Debug("Combat Advance Turn: Actor {0} ending his turn. CurSeq: {1}", obj, curSeq);
        if (_combatInitiative <= GameSystems.D20.Initiative.Count + 1)
        {
            var initListIdx = GameSystems.D20.Initiative.CurrentActorIndex;
            EndTurn();
            if (IsCombatActive())
            {
                TurnStart2(initListIdx);
            }
        }

        _combatInitiative--;
    }

    [TempleDllLocation(0x100632b0)]
    private void EndTurn()
    {
        var actor = GameSystems.D20.Initiative.CurrentActor;

        if (GameSystems.Party.IsInParty(actor))
        {
            GameUiBridge.RefreshInitiativePortraits();
        }
        else
        {
            GameSystems.Script.ExecuteObjectScript(actor, actor, ObjScriptEvent.EndCombat);
        }

        GameSystems.D20.Initiative.NextActor();

        if (GameSystems.Party.IsInParty(actor) && !GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
        {
            AddToInitiativeWithinRect(actor);
        }

        // remove dead and ObjectFlag.OFF from initiative
        var initiative = GameSystems.D20.Initiative;
        for (int i = 0; i < initiative.Count;)
        {
            var combatant = initiative[i];

            var shouldRemove = GameSystems.Critter.IsDeadNullDestroyed(combatant) ||
                               combatant.HasFlag(ObjectFlag.OFF);
            // Added in Temple+ : Remove AIs that aren't in combat mode
            if (!shouldRemove && combatant.IsNPC() && !GameSystems.Party.IsInParty(combatant) && !IsBrawling)
            {
                GameSystems.AI.GetAiFightStatus(combatant, out var aifs, out _);
                if (aifs == AiFightStatus.NONE)
                {
                    shouldRemove = true;
                }
            }

            if (shouldRemove)
            {
                initiative.RemoveFromInitiative(combatant);
            }
            else
            {
                i++;
            }
        }

        if (GameSystems.D20.Initiative.CurrentActor != null)
        {
            GameSystems.D20.Actions.TurnStart(GameSystems.D20.Initiative.CurrentActor);
        }

        if (AllCombatantsFarFromParty())
        {
            Logger.Info("Ending combat (enemies far from GameSystems.Party)");
            var leader = GameSystems.Party.GetConsciousLeader();
            if (leader != null)
            {
                CritterLeaveCombat(leader);
            }
        }
        else if (AllPcsUnconscious())
        {
            var leader = GameSystems.Party.GetConsciousLeader();
            if (leader != null)
            {
                CritterLeaveCombat(leader);
            }
        }
    }

    [TempleDllLocation(0x10062CB0)]
    public bool AllCombatantsFarFromParty(float minDistance = 125.0f)
    {
        if (!IsCombatActive())
        {
            return true;
        }

        foreach (var combatant in GameSystems.D20.Initiative)
        {
            if (GameSystems.Party.IsInParty(combatant))
                continue;
            if (GameSystems.Critter.IsDeadOrUnconscious(combatant))
                continue;
            if (GameSystems.Critter.IsConcealed(combatant))
            {
                continue; // TODO maybe revamp this condition?
            }

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (combatant.DistanceToObjInFeet(partyMember) < minDistance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    [TempleDllLocation(0x100ebb90)]
    [TempleDllLocation(0x10BD01C0)]
    public bool IsBrawling { get; set; }

    [TempleDllLocation(0x102e7f38)]
    public int BrawlStatus { get; set; }

    [TempleDllLocation(0x10BD01C8)]
    private GameObject _brawlPlayer;

    [TempleDllLocation(0x10BD01D0)]
    private GameObject _brawlOpponent;

    [TempleDllLocation(0x100ebd40)]
    public void Brawl(GameObject player, GameObject brawlAi)
    {
        BrawlStatus = -1; // reset brawl state (fixes weird issues... also allows brawling to be reused)

        if (IsBrawling)
        {
            return;
        }

        _brawlPlayer = player;
        _brawlOpponent = brawlAi;

        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (partyMember == player)
            {
                player.AddCondition(StatusEffects.BrawlPlayer);
            }
            else
            {
                player.AddCondition(StatusEffects.BrawlSpectator);
            }
        }

        brawlAi.AddCondition(StatusEffects.BrawlOpponent);

        GameSystems.D20.D20SendSignal(player, D20DispatcherKey.SIG_DealNormalDamage);
        GameSystems.Item.UnequipItemInSlot(player, EquipSlot.WeaponPrimary);
        GameSystems.Item.UnequipItemInSlot(player, EquipSlot.WeaponSecondary);
        // TODO: Unequip shield...?
        EnterCombat(brawlAi);
        StartCombat(player, true);
        IsBrawling = true;
    }

    [TempleDllLocation(0x100638f0)]
    public void TurnStart2(int prevInitiativeIdx)
    {
        var actor = GameSystems.D20.Initiative.CurrentActor;
        int curActorInitIdx = GameSystems.D20.Initiative.CurrentActorIndex;
        if (prevInitiativeIdx > curActorInitIdx)
        {
            Logger.Debug(
                "TurnStart2: \t End Subturn. Cur Actor: {0}, Initiative Idx: {1}; Prev Initiative Idx: {2} ", actor,
                curActorInitIdx, prevInitiativeIdx);
            CombatSubturnEnd();
        }

        // start new turn for current actor
        actor = GameSystems.D20.Initiative.CurrentActor;
        curActorInitIdx = GameSystems.D20.Initiative.CurrentActorIndex;
        Logger.Debug("TurnStart2: \t Starting new turn for {0}. InitiativeIdx: {1}", actor, curActorInitIdx);
        Subturn();

        RestartTurnTimeout();

        // handle simuls
        if (GameSystems.D20.Actions.SimulsAdvance())
        {
            actor = GameSystems.D20.Initiative.CurrentActor;
            Logger.Debug("TurnStart2: \t Actor {0} starting turn...(simul)", actor);
            AdvanceTurn(actor);
        }
    }

    /// <summary>
    /// Initiate a (configurable) 20 second timeout for AI turns
    /// Used as a last resort to avoid stuck AI from resulting in a soft-lock
    /// </summary>
    private void RestartTurnTimeout()
    {
        GameSystems.Vagrant.ActionBarStopActivity(_aiTurnTimeout);
        if (!GameSystems.Party.IsPlayerControlled(GameSystems.D20.Initiative.CurrentActor)
            && Globals.Config.AITurnTimeout > 0)
        {
            GameSystems.Vagrant.ActionBarStartRamp(_aiTurnTimeout, 0.0f, Globals.Config.AITurnTimeout, 1.0f);
        }
    }

    [TempleDllLocation(0x10063760)]
    private void Subturn()
    {
        var actor = GameSystems.D20.Initiative.CurrentActor;
        var partyLeader = GameSystems.Party.GetConsciousLeader();

        if (!GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
        {
            if (GameSystems.Party.IsInParty(actor))
            {
                GameSystems.Combat.AddToInitiativeWithinRect(actor);
            }
            else if (partyLeader != null && !GameSystems.Critter.IsFriendly(actor, partyLeader))
            {
                using var objList = ObjList.ListRangeTiles(actor, 24, ObjectListFilter.OLC_CRITTERS);
                foreach (var resHandle in objList)
                {
                    if (resHandle == actor)
                        continue;

                    var resObj = resHandle;
                    if ((resObj.GetFlags() & (ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW)) !=
                        default)
                        continue;

                    if (GameSystems.Critter.IsDeadOrUnconscious(resHandle))
                    {
                        continue;
                    }

                    if (GameSystems.Party.IsInParty(resHandle))
                        continue;

                    if (GameSystems.D20.Initiative.Contains(resHandle) ||
                        GameSystems.Critter.IsCombatModeActive(resHandle))
                        continue;

                    if (!GameSystems.Combat.HasLineOfAttack(resHandle, actor))
                    {
                        if (actor.DistanceToObjInFeet(resHandle) > 40)
                        {
                            continue;
                        }

                        // check pathfinding short distances
                        var pathFlags = PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_IGNORE_CRITTERS
                                                                       | PathQueryFlags.PQF_800 |
                                                                       PathQueryFlags.PQF_TARGET_OBJ
                                                                       | PathQueryFlags.PQF_ADJUST_RADIUS |
                                                                       PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS
                                                                       | PathQueryFlags.PQF_DONT_USE_PATHNODES |
                                                                       PathQueryFlags.PQF_A_STAR_TIME_CAPPED;

                        if (!Globals.Config.alertAiThroughDoors)
                        {
                            pathFlags |= PathQueryFlags.PQF_DOORS_ARE_BLOCKING;
                        }

                        if (!GameSystems.PathX.CanPathTo(actor, resHandle, pathFlags, 40))
                        {
                            continue;
                        }
                    }

                    // check that they have a faction in common
                    if (GameSystems.AI.GetAllegianceStrength(resHandle, actor) != 0)
                    {
                        GameSystems.AI.ProvokeHostility(partyLeader, resHandle, 3, 0);
                        continue;
                    }

                    if (GameSystems.Critter.HasNoAllegiance(resHandle) &&
                        GameSystems.Critter.HasNoAllegiance(actor))
                    {
                        if (GameSystems.AI.WillKos(resHandle, partyLeader))
                        {
                            GameSystems.AI.ProvokeHostility(partyLeader, resHandle, 3, 0);
                        }
                    }
                }
            }
        }

        if (actor == null)
        {
            Logger.Error("Combat Subturn: Coudn't start TB combat Turn due to no Active Critters!");
            CombatEnd();
            return;
        }

        combatSubturnTimeEvent = 10;
        GameSystems.Party.ClearSelection();

        if (GameSystems.Party.IsPlayerControlled(actor))
        {
            GameUiBridge.RefreshInitiativePortraits();

            GameSystems.Party.AddToSelection(actor);

            if (GameSystems.Map.GetCurrentMapId() == 5118 && GameSystems.Script.GetGlobalFlag(7) && actor.IsPC())
            {
                GameUiBridge.EnableTutorial();
                GameUiBridge.ShowTutorialTopic(TutorialTopic.Room8Overview);
                GameSystems.Script.SetGlobalFlag(7, false);
            }

            return;
        }

        // non-player controlled

        GameUiBridge.RefreshInitiativePortraits();

        if (GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
            return;

        if (GameSystems.Script.ExecuteObjectScript(actor, actor, ObjScriptEvent.StartCombat) == 0)
        {
            Logger.Info("Skipping AI Process for {0} (script)", actor);
            GameSystems.Combat.AdvanceTurn(actor);
            return;
        }

        Logger.Info("Calling AI Process for {0}", actor);
        GameSystems.Combat.TurnProcessAi(actor);
    }

    [TempleDllLocation(0x10062c50)]
    private void CombatSubturnEnd()
    {
        TimeSpan timeDelta = TimeSpan.FromMilliseconds(1000);
        GameSystems.TimeEvent.AddGameTime(timeDelta);
        if (GameSystems.Party.GetConsciousLeader() != null)
        {
            ++(combatRoundCount);
            combatActor = null;
            combatTimeEventSthg = 0;
            combatTimeEventIndicator = false;
        }
    }

    [TempleDllLocation(0x100633c0)]
    private void FleeFromCombat(GameObject critter)
    {
        if (IsCombatActive())
        {
            var leader = GameSystems.Party.GetConsciousLeader();
            CritterLeaveCombat(leader);
        }

        if (GameSystems.Map.GetFleeInfo(out var fleeInfo))
        {
            Logger.Info("PerformFleeCombat: teleporting to map=( {0} )", fleeInfo.mapId);

            var teleportArgs = new FadeAndTeleportArgs();
            teleportArgs.destLoc = fleeInfo.location.location;
            teleportArgs.destMap = fleeInfo.mapId;
            teleportArgs.flags = FadeAndTeleportFlags.CenterOnPartyLeader
                                 | FadeAndTeleportFlags.FadeIn
                                 | FadeAndTeleportFlags.FadeOut;
            teleportArgs.FadeOutArgs.color = PackedLinearColorA.Black;
            teleportArgs.FadeOutArgs.transitionTime = 0x3F800000;
            teleportArgs.FadeOutArgs.fadeSteps = 48;
            teleportArgs.FadeInArgs.flags = FadeFlag.FadeIn;
            teleportArgs.FadeInArgs.transitionTime = 0x3F800000;
            teleportArgs.FadeInArgs.fadeSteps = 48;
            if (teleportArgs.destMap == GameSystems.Map.GetCurrentMapId())
            {
                teleportArgs.destMap = -1;
            }
            else
            {
                teleportArgs.movieId = GameSystems.Map.GetEnterMovie(teleportArgs.destMap, false);
                if (teleportArgs.movieId != 0)
                {
                    teleportArgs.flags |= FadeAndTeleportFlags.play_movie;
                }
            }

            teleportArgs.somehandle = critter;
            GameSystems.Teleport.FadeAndTeleport(in teleportArgs);
            GameSystems.AI.ForceSpreadOut(critter);
        }
    }

    [TempleDllLocation(0x100630f0)]
    public void CritterLeaveCombat(GameObject obj)
    {
        if (!GameSystems.Party.IsPlayerControlled(obj))
        {
            var critterFlags = obj.GetCritterFlags();
            if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
            {
                obj.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
            }

            return;
        }

        if (!IsCombatActive())
            return;

        if (GameSystems.Party.GetConsciousLeader() != obj)
        {
            return;
        }

        if (GameSystems.Critter.IsDeadNullDestroyed(obj))
        {
            if (GameSystems.Party.GetLivingPartyMemberCount() >= 1)
            {
                return;
            }
        }

        if (!CombatEnd())
        {
            return;
        }

        GameUiBridge.OnExitCombat();

        GameSystems.SoundGame.StopCombatMusic(obj);

        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            var critterFlags = partyMember.GetCritterFlags();
            if (critterFlags.HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
            {
                partyMember.SetCritterFlags(critterFlags & ~CritterFlag.COMBAT_MODE_ACTIVE);
            }
        }
    }

    [TempleDllLocation(0x10062a30)]
    private bool CombatEnd(bool resetting = false)
    {
        if (!IsCombatActive())
        {
            return true;
        }

        GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Combat_End);
        _active = false;
        GameSystems.Anim.SetAllGoalsClearedCallback(null);
        GameSystems.Anim.InterruptAllForTbCombat();

        GameSystems.D20.Actions.ActionSequencesResetOnCombatEnd();
        if (!resetting)
        {
            GameSystems.D20.Initiative.OnExitCombat();
        }

        GameSystems.D20.EndTurnBasedCombat();

        if (!resetting)
        {
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                AutoReloadCrossbow(partyMember);
            }

            GameSystems.D20.Combat.AwardCombatExperience();

            // If no party member is selected after combat, select the party leader
            if (GameSystems.Party.Selected.Count == 0 && GameSystems.Party.GetLeader() != null)
            {
                GameSystems.Party.AddToSelection(GameSystems.Party.GetLeader());
            }
        }

        OnCombatStatusChanged?.Invoke(false);
        return true;
    }

    [TempleDllLocation(0x100b70a0)]
    public void AutoReloadCrossbow(GameObject critter)
    {
        var weapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponPrimary);
        if (weapon == null || !GameSystems.Item.IsCrossbow(weapon))
        {
            return;
        }

        if (weapon.WeaponFlags.HasFlag(WeaponFlag.WEAPON_LOADED))
        {
            // It's already loaded
            return;
        }

        if (GameSystems.Critter.IsDeadOrUnconscious(critter))
        {
            return;
        }

        if (GameSystems.Item.AmmoMatchesItemAtSlot(critter, EquipSlot.WeaponPrimary))
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                if (GameSystems.Item.ReloadEquippedWeapon(critter))
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(2, critter, null);
                    GameSystems.D20.Combat.FloatCombatLine(critter, D20CombatMessage.reloaded);
                }
            }
        }
        else
        {
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0, critter, null);
            GameSystems.D20.Combat.FloatCombatLine(critter, D20CombatMessage.out_of_ammo);
        }
    }

    [TempleDllLocation(0x100570c0)]
    public bool HasLineOfAttack(GameObject obj, GameObject target)
    {
        using var objIt = new RaycastPacket();
        objIt.origin = obj.GetLocationFull();
        LocAndOffsets tgtLoc = target.GetLocationFull();
        objIt.targetLoc = tgtLoc;
        objIt.flags = RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects |
                      RaycastFlag.HasTargetObj | RaycastFlag.HasSourceObj | RaycastFlag.HasRadius;
        objIt.radius = 0.1f;
        bool blockerFound = false;
        if (objIt.Raycast() > 0)
        {
            foreach (var resultItem in objIt)
            {
                var resultObj = resultItem.obj;
                if (resultObj == null)
                {
                    if (resultItem.flags.HasFlag(RaycastResultFlag.BlockerSubtile))
                    {
                        blockerFound = true;
                    }

                    continue;
                }

                if (resultObj.type == ObjectType.portal)
                {
                    if (!resultObj.IsPortalOpen())
                    {
                        blockerFound = true;
                    }

                    continue;
                }

                if (resultObj.IsCritter())
                {
                    if (GameSystems.Critter.IsDeadOrUnconscious(resultObj)
                        || GameSystems.D20.D20Query(resultObj, D20DispatcherKey.QUE_Prone))
                    {
                        continue;
                    }

                    // TODO: flag for Cover
                }
            }
        }

        return !blockerFound;
    }

    [TempleDllLocation(0x1004e730)]
    public void DispatchBeginRound(GameObject obj, int numRounds)
    {
        var dispatcher = obj.GetDispatcher();
        if (dispatcher != null)
        {
            var dispIo = new DispIoD20Signal();
            dispIo.data1 = numRounds;
            dispatcher.Process(DispatcherType.BeginRound, D20DispatcherKey.NONE, dispIo);
            GameSystems.Spell.ObjOnSpellBeginRound(obj);
        }
    }

    [TempleDllLocation(0x10062720)]
    public bool IsCombatModeActive(GameObject obj)
    {
        return obj.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE);
    }

    [TempleDllLocation(0x100624c0)]
    public GameObject GetMainHandWeapon(GameObject obj)
    {
        return GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
    }

    [TempleDllLocation(0x10062df0)]
    public bool IsGameConfigAutoAttack()
    {
        if (IsCombatActive())
            return false;

        return Globals.Config.AutoAttack;
    }

    [TempleDllLocation(0x10063010)]
    public void ThrowItem(GameObject critter, GameObject item, locXY targetLocation)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100629b0)]
    private bool IsCloseEnoughForCombat(GameObject handle)
    {
        if (handle.type == ObjectType.pc)
        {
            return true;
        }

        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            if (handle.DistanceToObjInFeet(partyMember) < MathF.Sqrt(1800.0f))
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x10062740)]
    private void CritterEnterCombat(GameObject critter)
    {
        var result = GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_EnterCombat);
        if (result)
        {
            var flags = critter.GetCritterFlags();
            critter.SetCritterFlags(flags | CritterFlag.COMBAT_MODE_ACTIVE);
            GameSystems.AI.ForceSpreadOut(critter);
        }
    }

    [TempleDllLocation(0x100631e0)]
    public void EnterCombat(GameObject handle)
    {
        if (GameSystems.D20.D20Query(handle, D20DispatcherKey.QUE_EnterCombat))
        {
            if (IsCloseEnoughForCombat(handle))
            {
                if (GameSystems.Party.IsPlayerControlled(handle))
                {
                    foreach (var partyMember in GameSystems.Party.PartyMembers)
                    {
                        if (!partyMember.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                        {
                            CritterEnterCombat(partyMember);
                        }
                    }
                }
                else if (!handle.GetCritterFlags().HasFlag(CritterFlag.COMBAT_MODE_ACTIVE))
                {
                    var partyLeader = GameSystems.Party.GetLeader();
                    GameSystems.Item.WieldBestAll(handle, partyLeader);
                    CritterEnterCombat(handle);
                    GameSystems.SoundGame.StartCombatMusic(handle);
                }
            }
        }
    }

    [TempleDllLocation(0x10062fd0)]
    public void ProjectileCleanup2(GameObject projectile, GameObject actor)
    {
        ThrownItemCleanup(projectile, actor, null);
    }

    [TempleDllLocation(0x10062560)]
    private void ThrownItemCleanup(GameObject projectile, GameObject actor,
        GameObject? target, bool recursed = false)
    {
        var projectileFlags = projectile.ProjectileFlags;
        if (projectileFlags.HasFlag(ProjectileFlag.UNK_40))
        {
            var thrownWeapon = projectile.GetObject(obj_f.projectile_parent_weapon);
            GameSystems.MapObject.Move(thrownWeapon, projectile.GetLocationFull());

            GameSystems.MapObject.ClearFlags(thrownWeapon, ObjectFlag.OFF);
            GameSystems.Object.Destroy(projectile);
        }
        else if (projectileFlags.HasFlag(ProjectileFlag.UNK_1000))
        {
            if (!recursed || projectileFlags.HasFlag(ProjectileFlag.UNK_2000))
            {
                actor.SetCritterFlags2(actor.GetCritterFlags2() & ~CritterFlag2.USING_BOOMERANG);
                GameSystems.Object.Destroy(projectile);
                GameSystems.AI.sub_10057790(actor, target);
            }
            else
            {
                projectile.ProjectileFlags |= ProjectileFlag.UNK_2000;
                var returnTo = actor.GetLocationFull();
                if (!GameSystems.Anim.ReturnProjectile(projectile, returnTo, target))
                {
                    ThrownItemCleanup(projectile, actor, target, true);
                }
            }
        }
        else
        {
            GameSystems.Object.Destroy(projectile);
        }
    }

    [TempleDllLocation(0x1007eb30)]
    public bool AffiliationSame(GameObject critterA, GameObject critterB)
    {
        return GameSystems.Party.IsInParty(critterA) == GameSystems.Party.IsInParty(critterB);
    }

    public bool IsUnarmed(GameObject critter)
    {
        if (GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponPrimary) != null)
            return false;
        if (GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponSecondary) != null)
            return false;
        return true;
    }

    public bool DisarmCheck(GameObject attacker, GameObject defender)
    {
        GameObject attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
        if (attackerWeapon == null)
        {
            attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
        }

        GameObject defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponPrimary);
        if (defenderWeapon == null)
        {
            defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponSecondary);
        }

        int attackerRoll = Dice.D20.Roll();
        int attackerSize = attacker.GetStat(Stat.size);
        DispIoAttackBonus dispIoAtkBonus = DispIoAttackBonus.Default;
        if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_DISARM) != 0)
        {
            var featName = GameSystems.Feat.GetFeatName(FeatId.IMPROVED_DISARM);
            dispIoAtkBonus.bonlist.AddBonus(4, 0, 114, featName); // Feat Improved Disarm
        }

        dispIoAtkBonus.bonlist.AddBonus((attackerSize - 5) * 4, 0, 316);

        if (attackerWeapon != null)
        {
            int attackerWieldType = GameSystems.Item.GetWieldType(attacker, attackerWeapon);
            if (attackerWieldType == 0)
                dispIoAtkBonus.bonlist.AddBonus(-4, 0, 340); // Light Weapon
            else if (attackerWieldType == 2)
                dispIoAtkBonus.bonlist.AddBonus(4, 0, 341); // Two Handed Weapon
            var weaponType = attackerWeapon.GetWeaponType();
            if (weaponType == WeaponType.spike_chain || weaponType == WeaponType.nunchaku ||
                weaponType == WeaponType.light_flail || weaponType == WeaponType.heavy_flail ||
                weaponType == WeaponType.dire_flail || weaponType == WeaponType.ranseur ||
                weaponType == WeaponType.halfling_nunchaku)
                dispIoAtkBonus.bonlist.AddBonus(2, 0, 343); // Weapon Special Bonus
        }
        else
        {
            if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_UNARMED_STRIKE) == 0)
                dispIoAtkBonus.bonlist.AddBonus(-4, 0, 342); // Disarming While Unarmed
            else
                dispIoAtkBonus.bonlist.AddBonus(-4, 0, 340); // Light Weapon
        }

        dispIoAtkBonus.attackPacket.weaponUsed = attackerWeapon;
        GameSystems.Stat.DispatchAttackBonus(attacker, defender, ref dispIoAtkBonus,
            DispatcherType.BucklerAcPenalty, 0); // buckler penalty
        GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoAtkBonus, DispatcherType.ToHitBonus2,
            0); // to hit bonus2
        int atkToHitBonus = GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoAtkBonus,
            DispatcherType.ToHitBonusFromDefenderCondition, 0);
        int attackerResult = attackerRoll + dispIoAtkBonus.bonlist.OverallBonus;

        int defenderRoll = Dice.D20.Roll();
        int defenderSize = defender.GetStat(Stat.size);
        DispIoAttackBonus dispIoDefBonus = DispIoAttackBonus.Default;
        dispIoDefBonus.bonlist.AddBonus((defenderSize - 5) * 4, 0, 316);
        if (defenderWeapon != null)
        {
            int wieldType = GameSystems.Item.GetWieldType(defender, defenderWeapon);
            if (wieldType == 0)
                dispIoDefBonus.bonlist.AddBonus(-4, 0, 340); // Light Off-hand Weapon
            else if (wieldType == 2)
                dispIoDefBonus.bonlist.AddBonus(4, 0, 341); // Two Handed Weapon
            var weaponType = defenderWeapon.GetWeaponType();
            if (weaponType == WeaponType.spike_chain || weaponType == WeaponType.nunchaku ||
                weaponType == WeaponType.light_flail || weaponType == WeaponType.heavy_flail ||
                weaponType == WeaponType.dire_flail || weaponType == WeaponType.ranseur ||
                weaponType == WeaponType.halfling_nunchaku)
                dispIoAtkBonus.bonlist.AddBonus(2, 0, 343); // Weapon Special Bonus
        }

        dispIoDefBonus.attackPacket.weaponUsed = attackerWeapon;
        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.BucklerAcPenalty,
            0); // buckler penalty
        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.ToHitBonus2,
            0); // to hit bonus2
        int defToHitBonus = GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoDefBonus,
            DispatcherType.ToHitBonusFromDefenderCondition, 0);
        int defenderResult = defenderRoll + dispIoAtkBonus.bonlist.OverallBonus;

        bool attackerSucceeded = attackerResult > defenderResult;
        var mesLineResult = attackerSucceeded ? D20CombatMessage.attempt_succeeds : D20CombatMessage.attempt_fails;
        var rollHistId = GameSystems.RollHistory.AddOpposedCheck(attacker, defender, attackerRoll,
            defenderRoll, dispIoAtkBonus.bonlist, dispIoDefBonus.bonlist, 5109, mesLineResult, 1);
        GameSystems.RollHistory.CreateRollHistoryString(rollHistId);

        return attackerSucceeded;
    }

    public bool SunderCheck(GameObject attacker, GameObject defender)
    {
        GameObject attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
        if (attackerWeapon == null)
            attackerWeapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponSecondary);
        GameObject defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponPrimary);
        if (defenderWeapon == null)
            defenderWeapon = GameSystems.Item.ItemWornAt(defender, EquipSlot.WeaponSecondary);
        int attackerRoll = Dice.D20.Roll();
        int attackerSize = attacker.GetStat(Stat.size);
        DispIoAttackBonus dispIoAtkBonus = DispIoAttackBonus.Default;
        if (GameSystems.Feat.HasFeatCountByClass(attacker, FeatId.IMPROVED_SUNDER) != 0)
        {
            string featName = GameSystems.Feat.GetFeatName(FeatId.IMPROVED_SUNDER);
            dispIoAtkBonus.bonlist.AddBonus(4, 0, 114, featName); // Feat Improved Sunder
        }

        dispIoAtkBonus.bonlist.AddBonus((attackerSize - 5) * 4, 0, 316);
        if (attackerWeapon != null)
        {
            int attackerWieldType = GameSystems.Item.GetWieldType(attacker, attackerWeapon);
            if (attackerWieldType == 0)
                dispIoAtkBonus.bonlist.AddBonus(-4, 0, 340); // Light Weapon
            else if (attackerWieldType == 2)
                dispIoAtkBonus.bonlist.AddBonus(4, 0, 341); // Two Handed Weapon
        }
        else
        {
            dispIoAtkBonus.bonlist.AddBonus(-4, 0, 342); // Disarming While Unarmed
        }


        dispIoAtkBonus.attackPacket.weaponUsed = attackerWeapon;

        GameSystems.Stat.DispatchAttackBonus(attacker, defender, ref dispIoAtkBonus,
            DispatcherType.BucklerAcPenalty, 0); // buckler penalty
        GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoAtkBonus, DispatcherType.ToHitBonus2,
            0); // to hit bonus2
        int atkToHitBonus = GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoAtkBonus,
            DispatcherType.ToHitBonusFromDefenderCondition, 0);
        int attackerResult = attackerRoll + dispIoAtkBonus.bonlist.OverallBonus;

        int defenderRoll = Dice.D20.Roll();
        int defenderSize = defender.GetStat(Stat.size);
        DispIoAttackBonus dispIoDefBonus = DispIoAttackBonus.Default;
        dispIoDefBonus.bonlist.AddBonus((defenderSize - 5) * 4, 0, 316);
        if (defenderWeapon != null)
        {
            int wieldType = GameSystems.Item.GetWieldType(defender, defenderWeapon);
            if (wieldType == 0)
                dispIoDefBonus.bonlist.AddBonus(-4, 0, 340); // Light Off-hand Weapon
            else if (wieldType == 2)
                dispIoDefBonus.bonlist.AddBonus(4, 0, 341); // Two Handed Weapon
        }

        dispIoDefBonus.attackPacket.weaponUsed = attackerWeapon;
        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.BucklerAcPenalty,
            0); // buckler penalty
        GameSystems.Stat.DispatchAttackBonus(defender, null, ref dispIoDefBonus, DispatcherType.ToHitBonus2,
            0); // to hit bonus2
        int defToHitBonus = GameSystems.Stat.DispatchAttackBonus(attacker, null, ref dispIoDefBonus,
            DispatcherType.ToHitBonusFromDefenderCondition, 0);
        int defenderResult = defenderRoll + dispIoDefBonus.bonlist.OverallBonus;

        bool attackerSucceeded = attackerResult > defenderResult;
        var resultMesLine = attackerSucceeded ? D20CombatMessage.attempt_succeeds : D20CombatMessage.attempt_fails;
        int rollHistId = GameSystems.RollHistory.AddOpposedCheck(attacker, defender, attackerRoll,
            defenderRoll, dispIoAtkBonus.bonlist, dispIoDefBonus.bonlist, 5109, resultMesLine, 1);
        GameSystems.RollHistory.CreateRollHistoryString(rollHistId);

        return attackerSucceeded;
    }

    [TempleDllLocation(0x100b6230)]
    public bool TripCheck(GameObject attacker, GameObject target)
    {
        if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Untripable))
        {
            GameSystems.RollHistory.CreateFromFreeText(GameSystems.D20.Combat.GetCombatMesLine(171));
            return false;
        }

        void AbilityScoreCheckModDispatch(GameObject obj, GameObject opponent, Stat statUsed,
            ref BonusList bonlist, SkillCheckFlags flags)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
                return;
            var dispIo = DispIoObjBonus.Default;
            dispIo.bonlist = bonlist;
            dispIo.flags = flags;
            dispIo.obj = opponent;
            dispatcher.Process(DispatcherType.AbilityCheckModifier, D20DispatcherKey.STAT_STRENGTH + (int) statUsed,
                dispIo);
            bonlist = dispIo.bonlist;
        }

        var attackerRoll = Dice.D20.Roll();
        var attackerBon = BonusList.Default;
        var attackerStrMod = attacker.GetStat(Stat.str_mod);
        attackerBon.AddBonus(attackerStrMod, 0, 103);
        AbilityScoreCheckModDispatch(attacker, target, Stat.strength, ref attackerBon, SkillCheckFlags.UnderDuress);
        var attackerSize = attacker.GetStat(Stat.size);
        if (attackerSize != 5)
        {
            attackerBon.AddBonus(4 * (attackerSize - 5), 0, 316);
        }

        var attackerResult = attackerRoll + attackerBon.OverallBonus;

        var defenderRoll = Dice.D20.Roll();
        BonusList defenderBon = BonusList.Default;
        var defenderStr = target.GetStat(Stat.strength);
        var defenderDex = target.GetStat(Stat.dexterity);
        Stat defenderStat = Stat.strength;
        if (defenderDex > defenderStr)
        {
            defenderStat = Stat.dexterity;
            var defenderMod = D20StatSystem.GetModifierForAbilityScore(defenderDex);
            defenderBon.AddBonus(defenderMod, 0, 104);
        }
        else
        {
            var defenderMod = D20StatSystem.GetModifierForAbilityScore(defenderStr);
            defenderBon.AddBonus(defenderMod, 0, 103);
        }

        AbilityScoreCheckModDispatch(target, attacker, defenderStat, ref defenderBon,
            SkillCheckFlags.UnderDuress | SkillCheckFlags.Unk2);
        var defenderSize = target.GetStat(Stat.size);
        if (defenderSize != 5)
        {
            defenderBon.AddBonus(4 * (defenderSize - 5), 0, 316);
        }

        var defenderResult = defenderRoll + defenderBon.OverallBonus;


        var succeeded = attackerResult > defenderResult;
        var resultMesLine = succeeded ? D20CombatMessage.attempt_succeeds : D20CombatMessage.attempt_fails;
        var rollId = GameSystems.RollHistory.AddOpposedCheck(attacker, target, attackerRoll,
            defenderRoll, attackerBon, defenderBon, 5062, resultMesLine, 1);
        GameSystems.RollHistory.CreateRollHistoryString(rollId);

        return succeeded;
    }

    [TempleDllLocation(0x10AA841C)]
    private int combatRoundCount = 0;

    [TempleDllLocation(0x100639a0)]
    public bool StartCombat(GameObject combatInitiator, bool setToFirstInitiativeFlag)
    {
        if (IsCombatActive())
        {
            return true;
        }

        if (AllPcsUnconscious())
        {
            return false;
        }

        combatRoundCount = 0;
        GameSystems.Anim.InterruptAllForTbCombat();

        GameSystems.Anim.SetAllGoalsClearedCallback(TbCombatScheduleEventAndAiSthg);
        GameSystems.D20.Initiative.CreateForParty();
        _active = true;

        // Add within rect party to initiative
        foreach (var partyMember in GameSystems.Party.PartyMembers)
        {
            AddToInitiativeWithinRect(partyMember);
        }

        GameSystems.D20.Initiative.AddToInitiative(combatInitiator);

        if (setToFirstInitiativeFlag)
        {
            GameSystems.D20.Actions.ResetAll(combatInitiator);
        }
        else
        {
            GameSystems.D20.Actions.ResetAll(null);
        }

        GameUiBridge.LogbookStartCombat();

        GameSystems.D20.Actions.TurnStart(GameSystems.D20.Initiative.CurrentActor);

        if (GameSystems.Party.GetConsciousLeader() != null)
        {
            ++combatRoundCount;
            combatActor = null;
            combatTimeEventSthg = 0;
            combatTimeEventIndicator = false;
        }

        if (GameSystems.D20.Actions.SimulsAdvance())
        {
            Logger.Info("Combat for {0} ending turn (simul)...", combatInitiator);
            AdvanceTurn(combatInitiator);
        }

        TurnStart2(0);

        GameUiBridge.CombatSthCallback();

        GameSystems.SoundGame.StartCombatMusic(combatInitiator);

        OnCombatStatusChanged?.Invoke(true);
        return true;
    }

    [TempleDllLocation(0x100624a0)]
    public void OnAfterActionsLoaded()
    {
        if (_active)
        {
            GameUiBridge.CombatSthCallback();
            GameUiBridge.RefreshInitiativePortraits();
        }
    }
    
    [TempleDllLocation(0x10062ac0)]
    private void AddToInitiativeWithinRect(GameObject partyMember)
    {
        var loc = partyMember.GetLocation();
        TileRect trect;
        trect.x1 = loc.locx - 18;
        trect.x2 = loc.locx + 18;
        trect.y1 = loc.locy - 18;
        trect.y2 = loc.locy + 18;

        using var objlist = ObjList.ListRect(trect, ObjectListFilter.OLC_CRITTERS);

        foreach (var resHandle in objlist)
        {
            // check if the object is ok to act (not dead, ObjectFlag.OFF, ObjectFlag.DONTDRAW
            // (to prevent the naughty Co8 critters from getting into combat), destroyed , unconscious)
            if ((resHandle.GetFlags() & (ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.DONTDRAW)) != default)
            {
                continue;
            }

            if (GameSystems.Critter.IsDeadOrUnconscious(resHandle))
            {
                continue;
            }

            if (!GameSystems.D20.Initiative.Contains(resHandle))
            {
                if (!IsCombatModeActive(resHandle) && !GameSystems.Party.IsInParty(resHandle))
                {
                    // todo: originally there was a dangling IsPerforming check in the function, should it be added to the conditions??
                    GameSystems.AI.AiProcess(resHandle);
                }
            }

            if (IsCombatModeActive(resHandle))
            {
                GameSystems.D20.Initiative.AddToInitiative(resHandle);
            }
        }
    }

    [TempleDllLocation(0x100628F0)]
    private void TbCombatScheduleEventAndAiSthg()
    {
        if (GameSystems.Combat.IsCombatActive() && !GameSystems.IsResetting())
        {
            var currentActor = GameSystems.D20.Initiative.CurrentActor;
            if (currentActor != null && (!currentActor.IsPC() || combatSubturnTimeEvent <= 0))
            {
                if (currentActor == combatActor)
                {
                    if (combatSubturnTimeEvent == combatTimeEventSthg)
                        combatTimeEventIndicator = true;
                    combatTimeEventSthg = combatSubturnTimeEvent;
                }
                else
                {
                    combatActor = currentActor;
                }
            }

            // TBCombat
            if (!GameSystems.TimeEvent.IsScheduled(TimeEventType.TBCombat))
            {
                var evt = new TimeEvent(TimeEventType.TBCombat);
                GameSystems.TimeEvent.ScheduleAbsolute(evt, null, 2, out _);
            }
        }
    }

    [TempleDllLocation(0x10062d60)]
    public bool AllPcsUnconscious()
    {
        foreach (var playerCharacter in GameSystems.Party.PlayerCharacters)
        {
            if (!GameSystems.Critter.IsDeadOrUnconscious(playerCharacter))
            {
                return false;
            }
        }

        return true;
    }

    [TempleDllLocation(0x100635e0)]
    public void TurnProcessAi(GameObject obj)
    {
        var actor = GameSystems.D20.Initiative.CurrentActor;
        if (obj != actor && obj != GameSystems.D20.Actions.getNextSimulsPerformer())
        {
            Logger.Warn("Not AI processing {0} (wrong turn...)", GameSystems.MapObject.GetDisplayName(obj));
            return;
        }

        if (GameSystems.Party.IsPlayerControlled(obj))
        {
            if (GameSystems.Critter.IsDeadOrUnconscious(obj))
            {
                Logger.Info("Combat for {0} ending turn (unconscious)", GameSystems.MapObject.GetDisplayName(obj));
                GameSystems.Combat.AdvanceTurn(obj);
            }
            // TODO: bug? they probably meant to do an OR

            return;
        }

        if (Globals.GameLib.IsLoading)
        {
            return;
        }

        if (GameSystems.AI.IsPcUnderAiControl(obj))
        {
            // tutorial shite
            if (GameSystems.Map.GetCurrentMapId() == 5118 && GameSystems.Script.GetGlobalFlag(7))
            {
                GameUiBridge.EnableTutorial();
                GameUiBridge.ShowTutorialTopic(TutorialTopic.Room8Overview);
                GameSystems.Script.SetGlobalFlag(7, false);
            }

            GameSystems.AI.AiProcessPc(obj);
            // TODO: possibly bugged if there's no "Advance Turn"?
            return;
        }

        if (GameSystems.Script.ExecuteObjectScript(obj, obj, ObjScriptEvent.Heartbeat) == 0)
        {
            Logger.Info("Combat for {0} ending turn (script).", GameSystems.MapObject.GetDisplayName(obj));
            GameSystems.Combat.AdvanceTurn(obj);
            return;
        }

        if (obj.HasFlag(ObjectFlag.OFF))
        {
            Logger.Info("Combat for {0} ending turn (ObjectFlag.OFF).", GameSystems.MapObject.GetDisplayName(obj));
            GameSystems.Combat.AdvanceTurn(obj);
            return;
        }

        GameSystems.AI.AiProcess(obj);
        if (obj.HasFlag(ObjectFlag.OFF))
        {
            GameSystems.Combat.AdvanceTurn(obj);
        }
    }

    [TempleDllLocation(0x100b90c0)]
    public List<GameObject> GetEnemiesCanMelee(GameObject critter)
    {
        var result = new List<GameObject>();

        foreach (var combatant in GameSystems.D20.Initiative)
        {
            if (combatant == critter || GameSystems.Critter.IsFriendly(critter, combatant))
                continue;
            if (!GameSystems.Combat.CanMeleeTarget(combatant, critter))
                continue;
            result.Add(combatant);
        }

        return result;
    }

    [TempleDllLocation(0x100b8740)]
    public bool CanMeleeTarget(GameObject attacker, GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        if (attacker.IsOffOrDestroyed)
        {
            return false;
        }

        var targetObjFlags = target.GetFlags();
        if ((targetObjFlags & (ObjectFlag.OFF | ObjectFlag.DESTROYED | ObjectFlag.INVULNERABLE)) != 0)
        {
            return false;
        }

        if (GameSystems.Critter.IsDeadOrUnconscious(attacker))
        {
            return false;
        }

        if (GameSystems.D20.D20Query(target, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                WellKnownSpells.Sanctuary, 0)
           ) // spell_sanctuary
        {
            return false;
        }

        if (GameSystems.D20.D20Query(attacker, D20DispatcherKey.QUE_Critter_Has_Spell_Active,
                WellKnownSpells.Sanctuary, 0)
           ) // presumably so the AI doesn't break its sanctuary protection?
        {
            return false;
        }

        var weapon = GameSystems.Critter.GetWornItem(attacker, EquipSlot.WeaponPrimary);
        if (CanMeleeTargetRegardWeapon(attacker, weapon, target))
        {
            return true;
        }

        var offhandWeapon = GameSystems.Critter.GetWornItem(attacker, EquipSlot.WeaponSecondary);
        if (CanMeleeTargetRegardWeapon(attacker, offhandWeapon, target))
        {
            return true;
        }

        if (!attacker.HasNaturalAttacks())
        {
            // Improved unarmed strike is checked earlier
            return false;
        }

        var objReach = attacker.GetReach();
        var distance = MathF.Max(0.0f, attacker.DistanceToObjInFeet(target));
        return objReach >= distance;
    }

    public bool CanMeleeTargetRegardWeapon(GameObject attacker, GameObject weapon, GameObject target)
    {
        if (weapon != null)
        {
            if (weapon.type != ObjectType.weapon)
                return false;
            if (GameSystems.Item.IsRangedWeapon(weapon))
                return false;
        }
        else
        {
            if (!attacker.GetCritterFlags().HasFlag(CritterFlag.MONSTER)
                && !GameSystems.Feat.HasFeat(attacker, FeatId.IMPROVED_UNARMED_STRIKE))
                return false;
        }

        var objReach = attacker.GetReach();
        var distToTgt = MathF.Max(0.0f, attacker.DistanceToObjInFeet(target));
        return objReach >= distToTgt;
    }

    public bool IsWithinReach(GameObject attacker, GameObject target)
    {
        var reach = attacker.GetReach();
        var dist = attacker.DistanceToObjInFeet(target);
        return dist <= reach;
    }

    [TempleDllLocation(0x100b7df0)]
    public void Heal(GameObject critter, GameObject healer, Dice dice, D20ActionType actionType)
    {
        var prone = GameSystems.Critter.IsProne(critter) || GameSystems.Critter.IsDeadOrUnconscious(critter);

        var dispIo = DispIoDamage.CreateWithWeapon(healer, critter);
        dispIo.attackPacket.d20ActnType = actionType;
        dispIo.attackPacket.dispKey = 1;
        dispIo.attackPacket.flags = D20CAF.HIT;

        dispIo.damage.AddDamageDice(dice, DamageType.Unspecified, 103);
        critter.DispatchHealing(dispIo);
        HealDamage(critter, dispIo);

        if (prone && !GameSystems.Combat.IsCombatActive() && !GameSystems.Critter.IsDeadOrUnconscious(critter)
            && !GameSystems.Critter.IsDeadNullDestroyed(critter))
        {
            GameSystems.D20.Actions.TurnBasedStatusInit(critter);
            GameSystems.D20.Actions.CurSeqReset(critter);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.STAND_UP, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            GameSystems.D20.Actions.sequencePerform();
        }
    }

    [TempleDllLocation(0x100b81d0)]
    public void SpellHeal(GameObject target, GameObject healer, Dice dice, D20ActionType actionType,
        int spellId)
    {
        if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
        {
            return;
        }

        var dispIo = DispIoDamage.Create(healer, target);
        dispIo.attackPacket.d20ActnType = actionType;
        dispIo.attackPacket.dispKey = 1;
        dispIo.attackPacket.flags = D20CAF.HIT;
        dispIo.damage.AddDamageDice(dice, DamageType.Unspecified, 103);

        if (spellPkt.metaMagicData.metaMagicEmpowerSpellCount > 0)
        {
            dispIo.damage.AddModFactor(1.5f, dispIo.damage.dice[0].type, 122);
        }

        if (spellPkt.metaMagicData.IsMaximize)
        {
            dispIo.damage.Maximized = true;
        }

        target.DispatchHealing(dispIo);
        HealDamage(target, dispIo);
    }

    [TempleDllLocation(0x100b6ee0)]
    public void HealDamage(GameObject target, DispIoDamage dispIo)
    {
        if (target.IsCritter() && !GameSystems.Critter.IsDeadNullDestroyed(target))
        {
            dispIo.damage.CalcFinalDamage();
            var healAmount = dispIo.damage.GetOverallLethalDamage();
            if (healAmount < 0)
            {
                healAmount = 0;
            }

            var damage = target.GetInt32(obj_f.hp_damage);

            var remainingDamage = damage - healAmount < 0 ? 0 : damage - healAmount;
            GameSystems.MapObject.ChangeTotalDamage(target, remainingDamage);
            GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_HP_Changed, healAmount);

            var subdualDamage = target.GetInt32(obj_f.critter_subdual_damage);
            var remainingSubdualDamage = subdualDamage - healAmount < 0 ? 0 : subdualDamage - healAmount;
            GameSystems.MapObject.ChangeSubdualDamage(target, remainingSubdualDamage);

            if (damage > remainingDamage)
            {
                // We actually did heal
                var suffix = GameSystems.D20.Combat.GetCombatMesLine(32);
                var text = $"{healAmount} {suffix}";
                GameSystems.TextFloater.FloatLine(target, TextFloaterCategory.Damage, TextFloaterColor.LightBlue,
                    text);
            }

            if (subdualDamage > remainingSubdualDamage)
            {
                // We healed subdual damage
                var suffix = GameSystems.D20.Combat.GetCombatMesLine(33);
                var text = $"{healAmount} {suffix}";
                GameSystems.TextFloater.FloatLine(target, TextFloaterCategory.Damage, TextFloaterColor.LightBlue,
                    text);
            }
        }
    }

    [TemplePlusLocation("combat.cpp:590")]
    public bool HasLineOfAttackFromPosition(LocAndOffsets fromPosition, GameObject target)
    {
        using var objIt = new RaycastPacket();
        objIt.origin = fromPosition;
        objIt.targetLoc = target.GetLocationFull();
        objIt.flags = RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects |
                      RaycastFlag.HasRadius;
        objIt.radius = 0.1f;
        return objIt.Raycast() <= 0 || !objIt.HasBlockerOrClosedDoor();
    }

    [TempleDllLocation(0x100ebc70)]
    public void LoadBrawlState(SavedBrawlState savedState)
    {
        IsBrawling = savedState.InProgress;
        BrawlStatus = savedState.Status;
        _brawlPlayer = GameSystems.Object.GetObject(savedState.PlayerId);
        if (!savedState.PlayerId.IsNull)
        {
            throw new CorruptSaveException($"Failed to restore brawl player {savedState.PlayerId}");
        }
        _brawlOpponent = GameSystems.Object.GetObject(savedState.OpponentId);
        if (!savedState.OpponentId.IsNull)
        {
            throw new CorruptSaveException($"Failed to restore brawl opponent {savedState.PlayerId}");
        }
    }

    [TempleDllLocation(0x100ebba0)]
    public SavedBrawlState SaveBrawlState()
    {
        return new SavedBrawlState
        {
            InProgress = IsBrawling,
            Status = BrawlStatus,
            PlayerId = _brawlPlayer?.id ?? ObjectId.CreateNull(),
            OpponentId = _brawlOpponent?.id ?? ObjectId.CreateNull()
        };
    }

}