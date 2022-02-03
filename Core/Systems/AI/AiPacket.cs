using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.AI;

internal class AiPacket
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public GameObject obj;
    public GameObject target;
    public AiFightStatus aiFightStatus;
    public int aiState2; // 1 - cast spell, 3 - use skill,  4 - scout point
    public int spellEnum;
    public SkillId skillEnum;
    public GameObject scratchObj;
    public GameObject leader;
    public int soundMap;
    public D20SpellData spellData;
    public int field3C;
    public SpellPacketBody spellPktBod;

    [TempleDllLocation(0x1005af90)]
    public AiPacket(GameObject objIn)
    {
        obj = objIn;
        spellEnum = 10000;
        skillEnum = (SkillId) (-1);
        leader = GameSystems.Critter.GetLeader(objIn);
        GameSystems.AI.GetAiFightStatus(objIn, out aiFightStatus, out target);
        soundMap = -1;
    }

    [TempleDllLocation(0x1005e790)]
    public bool PacketCreate()
    {
        if (!WieldBestItem())
        {
            return false;
        }

        if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Afraid) )
        {
            var fearedObj = GameSystems.D20.D20QueryReturnObject(obj, D20DispatcherKey.QUE_Critter_Is_Afraid);
            target = fearedObj;
            aiFightStatus = GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FLEEING, fearedObj, ref soundMap);
            return true;
        }

        // select heal spell & target from: self, leader, leader's followers
        if (SelectHealSpell())
        {
            return true;
        }

        if (!LookForEquipment())
        {
            FightStatusUpdate();
        }

        return true;
    }

    [TempleDllLocation(0x10057550)]
    public bool WieldBestItem()
    {
        if (GameSystems.Critter.IsDeadOrUnconscious(obj))
            return false;

        if (!GameSystems.Critter.IsSleeping(obj)
            && (obj.GetFlags() & (ObjectFlag.DONTDRAW | ObjectFlag.OFF)) != default)
            return false;

        // TODO: Vanilla previously checked if the first party member had the "OFF" flag, which was even weirder. What is this about?
        if (GameSystems.Party.PartySize == 0)
        {
            return false;
        }

        if ((obj.GetSpellFlags() & SpellFlag.STONED) != default)
        {
            return false;
        }

        if ((obj.GetCritterFlags() & CritterFlag.STUNNED) != default)
        {
            return false;
        }

        if (GameSystems.Reaction.GetLastReactionPlayer(obj) != null)
        {
            return false;
        }

        var animPriority = GameSystems.Anim.GetCurrentPriority(obj);
        if (animPriority != AnimGoalPriority.AGP_7 && animPriority > AnimGoalPriority.AGP_2)
        {
            return false;
        }

        if (GameSystems.Critter.IsDeadOrUnconscious(obj))
            return false;

        var npcFlags = obj.GetNPCFlags();
        if ((npcFlags & NpcFlag.GENERATOR) != default)
            return false;

        var aiFlags = obj.AiFlags;
        if ((aiFlags & AiFlag.RunningOff) != default)
        {
            return false;
        }

        if ((aiFlags & AiFlag.CheckWield) != default && !GameSystems.Combat.IsCombatActive())
        {
            GameSystems.Item.WieldBestAll(obj, target);
            obj.AiFlags &= ~(AiFlag.CheckWeapon | AiFlag.CheckWield);
        }
        else if ((aiFlags & AiFlag.CheckWeapon) != default)
        {
            GameSystems.Item.WieldBest(obj, 203, target);
            obj.AiFlags &= ~AiFlag.CheckWeapon;
        }

        if ((npcFlags & NpcFlag.DEMAINTAIN_SPELLS) != default)
        {
            var leader = GameSystems.Critter.GetLeader(obj);
            if (leader == null || GameSystems.Critter.IsDeadNullDestroyed(leader) ||
                !GameSystems.Critter.IsCombatModeActive(leader))
            {
                obj.SetNPCFlags(obj.GetNPCFlags() & ~NpcFlag.DEMAINTAIN_SPELLS);
            }
        }

        return true;
    }

    [TempleDllLocation(0x1005c290)]
    public bool SelectHealSpell()
    {
        if (GameSystems.Critter.IsSleeping(obj) || aiFightStatus == AiFightStatus.FLEEING)
        {
            return false;
        }

        var doHealSpell = true;
        if (GameSystems.Critter.IsCombatModeActive(obj))
        {
            AiParamPacket aiParams = GameSystems.AI.GetAiParams(obj);
            if (GameSystems.Random.GetInt(1, 100) > aiParams.healSpellChance)
            {
                doHealSpell = false;
            }
        }

        if (ShouldUseHealSpellOn(obj, doHealSpell))
        {
            return true;
        }

        var leader = GameSystems.Critter.GetLeaderRecursive(obj);
        if (leader != null || (leader = this.leader) != null)
        {
            if (ShouldUseHealSpellOn(leader, doHealSpell))
                return true;
        }
        else
        {
            leader = obj;
        }

        foreach (var follower in leader.EnumerateFollowers(true))
        {
            if (follower != obj && ShouldUseHealSpellOn(follower, doHealSpell))
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x1005aff0)]
    public bool ShouldUseHealSpellOn(GameObject handle, bool healSpellRecommended)
    {
        target = handle;
        if (GameSystems.Critter.IsDeadNullDestroyed(handle))
        {
            GameSystems.AI.GetAiSpells(out var aiSpells, obj, AiSpellType.ai_action_resurrect);
            if (GameSystems.AI.ChooseRandomSpellFromList(this, ref aiSpells))
                return true;
        }

        var hpPct = GameSystems.Critter.GetHpPercent(handle);
        if (hpPct > 30 && !healSpellRecommended)
        {
            return false;
        }

        if (hpPct < 40)
        {
            GameSystems.AI.GetAiSpells(out var aiSpells, obj, AiSpellType.ai_action_heal_heavy);
            if (GameSystems.AI.ChooseRandomSpellFromList(this, ref aiSpells))
                return true;
        }

        if (hpPct < 55)
        {
            GameSystems.AI.GetAiSpells(out var aiSpells, obj, AiSpellType.ai_action_heal_medium);
            if (GameSystems.AI.ChooseRandomSpellFromList(this, ref aiSpells))
                return true;
        }

        if (GameSystems.D20.D20Query(handle, D20DispatcherKey.QUE_Critter_Is_Poisoned) )
        {
            GameSystems.AI.GetAiSpells(out var aiSpells, obj, AiSpellType.ai_action_cure_poison);
            if (GameSystems.AI.ChooseRandomSpellFromList(this, ref aiSpells))
                return true;
        }

        if (hpPct < 70 || aiFightStatus == AiFightStatus.NONE && hpPct < 90)
        {
            GameSystems.AI.GetAiSpells(out var aiSpells, obj, AiSpellType.ai_action_heal_light);
            if (GameSystems.AI.ChooseRandomSpellFromList(this, ref aiSpells))
                return true;
        }

        return false;
    }

    [TempleDllLocation(0x1005c3d0)]
    public bool LookForEquipment()
    {
        if (GameSystems.Critter.IsSleeping(obj))
            return false;
        var critterFlags = obj.GetCritterFlags();
        if ((critterFlags & CritterFlag.FATIGUE_LIMITING) != default)
        {
            return false;
        }

        var aiFlags = obj.AiFlags;
        if ((aiFlags & AiFlag.LookForAmmo) != default)
        {
            obj.AiFlags &= ~AiFlag.LookForAmmo;
            if (GameSystems.AI.LookForAndPickupItem(obj, ObjectType.ammo))
            {
                return true;
            }
        }

        if ((aiFlags & AiFlag.LookForWeapon) != default)
        {
            obj.AiFlags &= ~AiFlag.LookForWeapon;
            if (GameSystems.AI.LookForAndPickupItem(obj, ObjectType.weapon))
            {
                return true;
            }
        }

        if ((aiFlags & AiFlag.LookForArmor) != default)
        {
            obj.AiFlags &= ~AiFlag.LookForArmor;
            if (GameSystems.AI.LookForAndPickupItem(obj, ObjectType.armor))
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x1005e050)]
    public void FightStatusUpdate()
    {
        if (GameSystems.Critter.IsSleeping(obj))
            return;
        GameObject focus = null;
        switch (aiFightStatus)
        {
            case AiFightStatus.FINDING_HELP: // for AI with scout points
                focus = ConsiderCombatFocus();
                if (focus != null)
                {
                    target = focus;
                    if (!ScoutPointSetState())
                    {
                        aiFightStatus =
                            GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                        return;
                    }

                    break;
                }

                // slide into next case if focus == null
                goto case AiFightStatus.NONE;
            case AiFightStatus.NONE:
                focus = GameSystems.AI.FindSuitableTarget(obj);
                if (focus != null)
                {
                    target = focus;
                    if (HasScoutStandpoint())
                    {
                        ScoutPointSetState();
                        aiFightStatus =
                            GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                        return;
                    }

                    ChooseRandomSpell_RegardInvulnerableStatus();
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                }

                break;
            case AiFightStatus.FIGHTING:
                focus = ConsiderCombatFocus();
                if (focus != null)
                {
                    target = focus;
                    ChooseRandomSpell_RegardInvulnerableStatus();
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                    return;
                }

                focus = obj.GetObject(obj_f.npc_who_hit_me_last);
                target = focus;
                if (GameSystems.AI.ConsiderTarget(obj, focus))
                {
                    ChooseRandomSpell_RegardInvulnerableStatus();
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                    return;
                }

                focus = PickRandomFromAiList();
                target = focus;
                if (focus == null)
                {
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.NONE, null, ref soundMap);
                    return;
                }

                ChooseRandomSpell_RegardInvulnerableStatus();
                aiFightStatus =
                    GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
                return;
            case AiFightStatus.FLEEING:
                if (target == null)
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.NONE, null, ref soundMap);
                break;
            case AiFightStatus.SURRENDERED:
                if (GameSystems.Critter.GetHpPercent(obj) >= 80 && GameSystems.Random.GetInt(1, 500) == 1)
                {
                    aiFightStatus =
                        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.NONE, null, ref soundMap);
                }

                break;
            default:
                return;
        }
    }

    [TempleDllLocation(0x10057d00)]
    public bool HasScoutStandpoint()
    {
        obj.GetStandPoint(StandPointType.Scout, out var standPt);
        return standPt.location.location != locXY.Zero;
    }

    [TempleDllLocation(0x10057d40)]
    public bool ScoutPointSetState()
    {
        if (!HasScoutStandpoint())
            return false;

        obj.GetStandPoint(StandPointType.Scout, out var standPt);
        var objLoc = obj.GetLocationFull();
        if (objLoc.location.EstimateDistance(standPt.location.location) <= 3)
        {
            aiState2 = 5;
            return true;
        }

        aiState2 = 4;
        return true;
    }

    [TempleDllLocation(0x1005c6a0)]
    public void ChooseRandomSpell_RegardInvulnerableStatus()
    {
        var objBody = obj;
        if ((objBody.GetFlags() & ObjectFlag.INVULNERABLE) != default)
        {
            aiState2 = 0;
            return;
        }

        var critFlags2 = objBody.GetCritterFlags2();
        if ((critFlags2 & CritterFlag2.NIGH_INVULNERABLE) != default)
        {
            aiState2 = 0;
            return;
        }

        if (!GameSystems.AI.ChooseRandomSpell(this))
        {
            aiState2 = 0;
            return;
        }
    }

    [TempleDllLocation(0x1005d620)]
    public GameObject PickRandomFromAiList()
    {
        GameSystems.AI.AiListRemove(obj, null, 0);

        var objBody = obj;
        var aiListCount = objBody.GetObjectArray(obj_f.npc_ai_list_idx).Count;

        var randIdx = GameSystems.Random.GetInt(0, aiListCount - 1);
        for (var i = 0; i < aiListCount; i++, randIdx++)
        {
            var aiListItem = objBody.GetObject(obj_f.npc_ai_list_idx, randIdx % aiListCount);
            var aiListItemType = objBody.GetInt32(obj_f.npc_ai_list_type_idx, randIdx % aiListCount);
            if (aiListItemType == 0)
            {
                if (GameSystems.AI.ConsiderTarget(obj, aiListItem))
                {
                    return aiListItem;
                }
            }
        }

        return null;
    }

    [TempleDllLocation(0x1005d580)]
    public GameObject ConsiderCombatFocus()
    {
        var objBody = obj;
        var curCombatFocus = objBody.GetObject(obj_f.npc_combat_focus);
        if (GameSystems.AI.ConsiderTarget(obj, curCombatFocus))
        {
            return curCombatFocus;
        }

        GameSystems.AI.TargetLockUnset(obj);
        return GameSystems.AI.FindSuitableTarget(obj);
    }

    [TempleDllLocation(0x1005eec0)]
    public void ProcessCombat()
    {
        var isCombat = GameSystems.Combat.IsCombatActive();
        var curActor = GameSystems.D20.Initiative.CurrentActor;
        var nextSimuls = GameSystems.D20.Actions.getNextSimulsPerformer();

        if (!isCombat || curActor == obj || nextSimuls == obj)
        {
            var objBod = obj;
            if ((objBod.GetNPCFlags() & NpcFlag.BACKING_OFF) != default)
            {
                ProcessBackingOff();
            }
            else
            {
                switch (aiState2)
                {
                    case 1:
                        ThrowSpell();
                        break;
                    case 2:
                        UseItem();
                        break;
                    case 3:
                        GameSystems.Anim.PushUseSkillOn(obj, target, scratchObj, skillEnum);
                        break;
                    case 4:
                        MoveToScoutPoint();
                        break;
                    case 5:
                        ScoutPointAttack();
                        break;
                    default:
                        switch (aiFightStatus)
                        {
                            case AiFightStatus.NONE:
                                DoWaypoints();
                                break;
                            case AiFightStatus.FLEEING:
                                GameSystems.AI.FleeProcess(obj, target);
                                break;
                            case AiFightStatus.FIGHTING:
                                ProcessFighting();
                                break;
                            case AiFightStatus.SURRENDERED:
                                FleeingStatusRefresh();
                                break;
                        }
                        break;
                }
            }
        }

        if (!GameSystems.Combat.IsCombatActive())
            return;
        var curSeq = GameSystems.D20.Actions.CurrentSequence;
        if (curSeq == null)
        {
            Logger.Error("NULL STATUS???");
            return;
        }

        curActor = GameSystems.D20.Initiative.CurrentActor;
        if (curActor != obj || GameSystems.D20.Actions.IsCurrentlyPerforming(obj))
            return;
        if (!GameSystems.D20.Actions.IsSimulsCompleted())
            return;
        if (!GameSystems.D20.Actions.IsLastSimultPopped(obj))
        {
            Logger.Debug("AI for {0} ending turn...", obj);
            GameSystems.Combat.AdvanceTurn(obj);
        }
    }

    [TempleDllLocation(0x100581d0)]
    public void ProcessBackingOff()
    {
        var aiParams = GameSystems.AI.GetAiParams(obj);
        var critter = obj;
        var npcFlags = critter.GetNPCFlags();
        if ((npcFlags & NpcFlag.BACKING_OFF) == default)
            return;

        var minDistFeet = 12.0f;
        if (aiParams.combatMinDistanceFeet > 1){
            minDistFeet = aiParams.combatMinDistanceFeet;
        }

        if (obj.DistanceToObjInFeet(target) >= minDistFeet)
        {
            critter.SetNPCFlags(npcFlags & ~NpcFlag.BACKING_OFF);
            return;
        }

        AnimPathData animPathSpec;
        animPathSpec.handle = obj;
        animPathSpec.srcLoc = critter.GetLocation();
        animPathSpec.flags = AnimPathDataFlags.UNK800;
        animPathSpec.size = 100;
        animPathSpec.distTiles = (int)(minDistFeet * locXY.INCH_PER_FEET / locXY.INCH_PER_TILE);
        animPathSpec.deltas = new sbyte[200];
        animPathSpec.destLoc = target.GetLocation();
        if (GameSystems.PathX.AnimPathSearch(ref animPathSpec) == 0)
        {
            critter.SetNPCFlags(npcFlags & ~NpcFlag.BACKING_OFF);
            return;
        }

        if (GameSystems.D20.Actions.TurnBasedStatusInit(obj)) {
            GameSystems.D20.Actions.CurSeqReset(obj);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.UNSPECIFIED_MOVE, 0);
            GameSystems.D20.Actions.GlobD20ActnSetTarget(obj, null); // bug???? todo
            GameSystems.D20.Actions.ActionAddToSeq();
            GameSystems.D20.Actions.sequencePerform();
        }
    }

    [TempleDllLocation(0x10057dc0)]
    public void ThrowSpell()
    {

        if (GameSystems.Combat.IsCombatActive()){
            if (GameSystems.D20.Actions.TurnBasedStatusInit(obj)){
                GameSystems.D20.Actions.CurSeqReset(obj);
                GameSystems.AI.AiStrategDefaultCast(obj, target, spellData, spellPktBod);
            }
            return;
        }

        if (!GameSystems.Spell.IsSpellHarmful(spellData.spellEnumOrg, obj, target))
            return;


        GameSystems.Combat.EnterCombat(obj);

        if (!GameSystems.Party.IsInParty(obj) && !GameSystems.Party.IsInParty(target))
            return;

        GameSystems.Combat.EnterCombat(target);
        if (GameSystems.Critter.CanSense(target, obj)) {
            GameSystems.Combat.StartCombat(obj, false);
        }
        else{
            GameSystems.Combat.StartCombat(obj, true);
        }
    }

    [TempleDllLocation(0x10057ec0)]
    public void UseItem()
    {
        if (spellEnum == 0)
        {
            return;
        }

        if (!GameSystems.D20.Actions.TurnBasedStatusInit(obj))
        {
            return;
        }

        GameSystems.D20.Actions.CurSeqReset(obj);
        GameSystems.D20.Actions.GlobD20ActnInit();
        GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.USE_ITEM, 0);
        GameSystems.D20.Actions.GlobD20ActnSetSpellData(spellData);
        GameSystems.D20.Actions.ActionAddToSeq();
        GameSystems.D20.Actions.sequencePerform();
    }

    [TempleDllLocation(0x10057f10)]
    public void MoveToScoutPoint()
    {
        obj.GetStandPoint(StandPointType.Scout, out var standPt);
        if (!GameSystems.D20.Actions.TurnBasedStatusInit(obj))
            return;
        GameSystems.D20.Actions.CurSeqReset(obj);
        GameSystems.D20.Actions.GlobD20ActnInit();
        GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.UNSPECIFIED_MOVE, 0);
        GameSystems.D20.Actions.GlobD20ActnSetTarget(obj, standPt.location);
        GameSystems.D20.Actions.ActionAddToSeq();
        GameSystems.D20.Actions.sequencePerform();
    }

    [TempleDllLocation(0x1005ecd0)]
    public void ScoutPointAttack()
    {
        GameSystems.AI.ProvokeHostility(target, obj, 3, 0);
        obj.AiFlags &= ~AiFlag.FindingHelp;
        GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.FIGHTING, target, ref soundMap);
    }

    [TempleDllLocation(0x1005c6f0)]
    public void DoWaypoints()
    {
        GameSystems.Combat.CritterLeaveCombat(obj);
        if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Prone) &&
            !GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Unconscious)){
            GameSystems.D20.Actions.TurnBasedStatusInit(obj);
            GameSystems.D20.Actions.GlobD20ActnInit();
            GameSystems.D20.Actions.GlobD20ActnSetTypeAndData1(D20ActionType.STAND_UP, 0);
            GameSystems.D20.Actions.ActionAddToSeq();
            GameSystems.D20.Actions.sequencePerform();
            return;
        }

        var objBody = obj;
        var npcFlags = objBody.GetNPCFlags();

        if (leader == null) {
            if ((npcFlags& NpcFlag.USE_ALERTPOINTS) != default){

                if (!GameSystems.AI.NpcWander_1005BC00(obj, false)){
                    GameSystems.Anim.PushFidget(obj);
                    GameSystems.Anim.GetOffMyLawn(obj);
                }
            }
            else if (!GameSystems.AI.waypointsSthgsub_1005B950(obj, false)
                     && !GameSystems.AI.NpcWander_1005BC00(obj, false))
            {
                GameSystems.Anim.PushFidget(obj);
                GameSystems.Anim.GetOffMyLawn(obj);
            }
            return;
        }

        if ((npcFlags & NpcFlag.AI_WAIT_HERE) != default)
            return;

        if (!GameSystems.Anim.GetFirstRunSlotId(obj).IsNull)
            return;

        if (GameSystems.AI.RefuseFollowCheck(obj, leader)
            && GameSystems.Critter.RemoveFollower(obj, false))
        {
            GameUiBridge.UpdatePartyUi();
            npcFlags = objBody.GetNPCFlags();
            objBody.SetNPCFlags(npcFlags | NpcFlag.JILTED);
            // looks like there was some commented out code here for playing some sound
        }
        else if ((npcFlags & NpcFlag.CHECK_LEADER) != default){
            objBody.SetNPCFlags(npcFlags & ~NpcFlag.CHECK_LEADER);
            // looks like there was some commented out code here for playing some sound
        }
    }

    [TempleDllLocation(0x10058070)]
    public void ProcessFighting()
    {

        if (GameSystems.Combat.IsCombatActive()){
            if (GameSystems.D20.Actions.curSeqGetTurnBasedStatus() != null &&
                GameSystems.D20.Actions.TurnBasedStatusInit(obj))
            {
                GameSystems.D20.Actions.CurSeqReset(obj);
                GameSystems.AI.StrategyParse(obj, target);
            }

            return;
        }

        if (GameSystems.Party.IsAiFollower(obj) || obj.DistanceToObjInFeet(target) > 75.0f)
        {
            return;
        }

        if (!GameSystems.Party.IsInParty(obj) && GameSystems.Party.IsInParty(target)){
            target = GameSystems.PathX.CanPathToParty(obj);
            if (target == null)
            {
                return;
            }
        }
        GameSystems.Combat.EnterCombat(obj);
        if (GameSystems.Party.IsInParty(obj) || GameSystems.Party.IsInParty(target)){
            GameSystems.Combat.EnterCombat(target);
            if (GameSystems.Critter.CanSense(target, obj))
            {
                GameSystems.Combat.StartCombat(obj, false);
            }
            else
            {
                GameSystems.Combat.StartCombat(obj, true);
            }
        }

    }

    [TempleDllLocation(0x1005ddf0)]
    public void FleeingStatusRefresh()
    {
        var objBody = obj;
        var fleeingFrom = objBody.GetObject(obj_f.critter_fleeing_from);
        if (fleeingFrom == null
            || GameSystems.Critter.IsDeadNullDestroyed(fleeingFrom)
            || GameSystems.Critter.IsDeadOrUnconscious(fleeingFrom))
        {
            aiFightStatus = GameSystems.AI.UpdateAiFlags(obj, AiFightStatus.NONE, null);
        }
    }
}