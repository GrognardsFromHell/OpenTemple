
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

[ObjectScript(61)]
public class Gremag : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        var assassin_of_lodriss = triggerer.HasReputation(21);
        var butcher_of_hommlet = triggerer.HasReputation(1);
        var is_rogue = attachee.GetStat(Stat.level_rogue) >= 2;
        var is_evil = (triggerer.GetAlignment().IsEvil());
        var is_friend_of_lareth = triggerer.HasReputation(18);
        var is_beggar_maker = triggerer.HasReputation(5);
        if ((attachee.GetMap() == 5074)) // In the campsite
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 800);
        }

        // double crossed (need to add flag for actually getting a reward)
        // turned R&G in to Burne and confronted them about the spy as well:
        if ((GetQuestState(16) == QuestState.Completed && GetQuestState(15) == QuestState.Completed))
        {
            // received compensation for promising not to tell
            triggerer.BeginDialog(attachee, 20);
        }
        // suggest assassination quests
        else if ((!GetGlobalFlag(293) && attachee.HasMet(triggerer)) && (assassin_of_lodriss || butcher_of_hommlet || is_friend_of_lareth || (is_rogue && is_evil) || (is_beggar_maker && is_evil)))
        {
            triggerer.BeginDialog(attachee, 290);
        }
        // Ask about Lareth's whereabouts
        else if (((GetGlobalFlag(835) || GetGlobalFlag(837)) && !GetGlobalFlag(37) && attachee.HasMet(triggerer) && !(GetQuestState(16) == QuestState.Completed) && !(GetQuestState(15) == QuestState.Completed) && !GetGlobalFlag(843)))
        {
            triggerer.BeginDialog(attachee, 2500);
        }
        // Confronted about spy or courier, but have not ratted to Burne yet
        else if ((GetGlobalFlag(41) || GetQuestState(16) == QuestState.Completed || GetQuestState(64) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 260);
        }
        // Courier has spilled guts, Gremag now wary
        else if ((GetQuestState(17) == QuestState.Completed || GetQuestState(64) == QuestState.Botched))
        {
            triggerer.BeginDialog(attachee, 30);
        }
        // Exposed laborer spy
        else if ((GetGlobalFlag(31)))
        {
            triggerer.BeginDialog(attachee, 40);
        }
        // None of the above, but Man-At-Arms hired
        else if ((GetGlobalFlag(39)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        // completed 1st assassination
        else if ((GetGlobalFlag(297) && GetQuestState(91) == QuestState.Accepted))
        {
            triggerer.BeginDialog(attachee, 390);
        }
        // completed 2nd assassination
        else if ((GetGlobalFlag(298) && GetQuestState(92) == QuestState.Accepted))
        {
            triggerer.BeginDialog(attachee, 400);
        }
        // completed 3rd assassination
        else if ((GetGlobalFlag(299) && GetQuestState(93) == QuestState.Accepted))
        {
            triggerer.BeginDialog(attachee, 410);
        }
        else if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 500);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5010))
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(751) == 0))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((!GetGlobalFlag(907)))
                {
                    StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                    SetGlobalFlag(907, true);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(815, true);
        if ((GetGlobalFlag(814)))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.HasReputation(23)))
                {
                    pc.RemoveReputation(23);
                }

            }

        }

        if ((!PartyLeader.HasReputation(9)))
        {
            PartyLeader.AddReputation(9);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.type == ObjectType.pc && GetGlobalVar(751) == 0))
        {
            var raimol = Utilities.find_npc_near(attachee, 8050);
            if ((raimol != null))
            {
                attachee.FloatLine(280, triggerer);
                var leader = raimol.GetLeader();
                if ((leader != null))
                {
                    leader.RemoveFollower(raimol);
                }

                raimol.Attack(triggerer);
            }

        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(751) == 1 && attachee.GetMap() == 5010 && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            SetGlobalVar(751, 2);
            Utilities.create_item_in_inventory(8010, attachee);
            return RunDefault;
        }

        if ((GetGlobalVar(751) == 2 && attachee.GetMap() == 5010 && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            SetGlobalVar(751, 3);
            if ((!PartyLeader.HasReputation(23)))
            {
                PartyLeader.AddReputation(23);
            }

            attachee.RunOff();
            return SkipDefault;
        }

        if ((GetGlobalVar(751) == 0 && attachee.GetStat(Stat.hp_current) >= 0 && GetGlobalFlag(814) && attachee.GetMap() == 5010) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & (1 << 10)) == 0))
        {
            GameObject found_pc = null;
            var rannos = Utilities.find_npc_near(attachee, 8048);
            var raimol = Utilities.find_npc_near(attachee, 8050);
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.type == ObjectType.pc && !pc.IsUnconscious())
                {
                    found_pc = pc;
                    attachee.AIRemoveFromShitlist(pc);
                    rannos.AIRemoveFromShitlist(pc);
                    raimol.AIRemoveFromShitlist(pc);
                }
                else
                {
                    attachee.AIRemoveFromShitlist(pc);
                    rannos.AIRemoveFromShitlist(pc);
                    raimol.AIRemoveFromShitlist(pc);
                    pc.AIRemoveFromShitlist(attachee);
                    pc.AIRemoveFromShitlist(rannos);
                    pc.AIRemoveFromShitlist(raimol);
                }

            }

            if ((found_pc != null))
            {
                found_pc.BeginDialog(attachee, 1100);
                return SkipDefault;
            }

        }

        if ((Utilities.obj_percent_hp(attachee) < 95 && GetGlobalVar(751) == 0 && attachee.GetStat(Stat.hp_current) >= 0 && attachee.GetMap() == 5010) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & (1 << 10)) == 0))
        {
            GameObject found_pc = null;
            var rannos = Utilities.find_npc_near(attachee, 8048);
            var raimol = Utilities.find_npc_near(attachee, 8050);
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (pc.type == ObjectType.pc && !pc.IsUnconscious())
                {
                    found_pc = pc;
                    attachee.AIRemoveFromShitlist(pc);
                    rannos.AIRemoveFromShitlist(pc);
                    raimol.AIRemoveFromShitlist(pc);
                }
                else
                {
                    attachee.AIRemoveFromShitlist(pc);
                    rannos.AIRemoveFromShitlist(pc);
                    raimol.AIRemoveFromShitlist(pc);
                    pc.AIRemoveFromShitlist(attachee);
                    pc.AIRemoveFromShitlist(rannos);
                    pc.AIRemoveFromShitlist(raimol);
                }

            }

            if ((found_pc != null))
            {
                if ((!GetGlobalFlag(814) && rannos.GetStat(Stat.hp_current) >= 0))
                {
                    found_pc.BeginDialog(attachee, 1000);
                    return SkipDefault;
                }

                if ((GetGlobalFlag(814) || rannos.GetStat(Stat.hp_current) <= -1))
                {
                    found_pc.BeginDialog(attachee, 1100);
                }

                return SkipDefault;
            }

        }

        // THIS IS USED FOR BREAK FREE
        foreach (var obj in PartyLeader.GetPartyMembers())
        {
            if ((obj.DistanceTo(attachee) <= 3 && obj.GetStat(Stat.hp_current) >= -9))
            {
                return RunDefault;
            }

        }

        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        // attachee.d20_send_signal(S_BreakFree)
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(815, false);
        if ((PartyLeader.HasReputation(9)))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((!pc.HasReputation(23)))
                {
                    pc.AddReputation(23);
                }

            }

            PartyLeader.RemoveReputation(9);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(8010);
        if ((itemA != null && GetGlobalVar(751) == 0 && attachee.GetMap() == 5010))
        {
            itemA.Destroy();
            // create_item_in_inventory( 8021, attachee )
            DetachScript();
        }

        return RunDefault;
    }
    public static bool switch_to_rannos(GameObject gremag, GameObject pc)
    {
        var rannos = Utilities.find_npc_near(gremag, 8048);
        SetGlobalVar(750, 1);
        SetGlobalVar(751, 1);
        rannos.TurnTowards(gremag);
        gremag.TurnTowards(rannos);
        pc.BeginDialog(rannos, 1010);
        return SkipDefault;
    }
    public static void respawn(GameObject attachee)
    {
        var box = Utilities.find_container_near(attachee, 1004);
        InventoryRespawn.RespawnInventory(box);
        StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
        return;
    }
    public static bool buff_npc(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14607 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.Stoneskin, obj);
            }

            if ((obj.GetNameId() == 14609 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.ShieldOfFaith, obj);
            }

        }

        return RunDefault;
    }
    public static bool buff_npc_two(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14607 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.ImprovedInvisibility, obj);
            }

            if ((obj.GetNameId() == 14609 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.OwlsWisdom, obj);
            }

        }

        return RunDefault;
    }
    public static bool buff_npc_four(GameObject attachee, GameObject triggerer)
    {
        var target = Utilities.find_npc_near(attachee, 14606);
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14607 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.Endurance, attachee);
            }

            if ((obj.GetNameId() == 14609 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.Endurance, target);
            }

        }

        return RunDefault;
    }
    public static bool ishurt(GameObject attachee, int percent)
    {
        var maxhp = attachee.GetStat(Stat.hp_max);
        var curhp = attachee.GetStat(Stat.hp_current);
        var subdam = attachee.GetStat(Stat.subdual_damage);
        var nordam = maxhp - curhp;
        var percent_hurt = (nordam + subdam) * 100 / maxhp;
        return percent_hurt > percent;

    }
    public static bool lawful_ok()
    {
        // this script is used to determine if there's sufficient proof for a lawful party to attack R&G
        var a = 0;
        if ((GetGlobalFlag(31)))
        {
            // found out the laborer spy
            a = a + 1;
        }

        if ((GetGlobalFlag(428)))
        {
            // confronted about assassination attempt. only enabled if 292 == 1, so no need to check for it too
            a = a + 1;
        }

        if ((GetQuestState(17) == QuestState.Completed))
        {
            // found out the courier
            a = a + 1;
        }
        else if ((GetQuestState(17) != QuestState.Unknown && GetGlobalFlag(40) && GetGlobalFlag(4)))
        {
            // found out about Corl poisoning sheep, and teamster has attested to strange goings on in the barn, and you've met the courier
            a = a + 1;
        }

        if ((GetGlobalFlag(423) && GetGlobalFlag(292) && GetGlobalFlag(40)))
        {
            // found out about Corl having been assassinated, connected it with other assassinations and strange behavior of the barn courier
            a = a + 1;
        }

        if ((a > 1))
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public static int is_lawful_party()
    {
        if (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_EVIL)
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }
    public static int party_lawful()
    {
        // yeah yeah duplicity blah blah
        if ((PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL))
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }
    public static int party_good()
    {
        if ((PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }
    public static int party_bad()
    {
        if ((PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL))
        {
            return 1;
        }
        else
        {
            return 0;
        }

    }
    public static void burne_quest()
    {
        if (GetQuestState(64) == QuestState.Accepted || GetQuestState(64) == QuestState.Mentioned)
        {
            SetQuestState(64, QuestState.Completed);
        }

        return;
    }
    public static void rngfighttime_set()
    {
        if (!GetGlobalFlag(426))
        {
            ScriptDaemon.record_time_stamp(426);
            SetGlobalFlag(426, true);
        }

        return;
    }
    public static int traders_exposed()
    {
        // this script determines whether the traders surmise they are about to be exposed
        // it uses the time stamps to determine whether they knew this stuff BEFORE you assaulted them
        var a = 0;
        if ((GetQuestState(15) == QuestState.Completed && ScriptDaemon.tsc(427, 426)))
        {
            // laborer spy revealed to Burne
            a = a + 1;
        }

        if ((GetQuestState(16) == QuestState.Completed && ScriptDaemon.tsc(431, 426)))
        {
            // confronted traders about laborer spy
            a = a + 1;
        }

        if (GetGlobalFlag(444) || GetGlobalFlag(422) || (GetGlobalFlag(428) && (GetGlobalFlag(7) || (CurrentTimeSeconds >= GetGlobalVar(423) + 24 * 60 * 60 && GetGlobalFlag(4)))))
        {
            // confronted about assassination attempt, and either presented hard evidence or Corl was killed too
            a = a + 1;
        }

        if ((GetQuestState(17) == QuestState.Completed && ScriptDaemon.tsc(430, 426)))
        {
            // found out the courier
            a = a + 1;
        }

        return a;
    }
    // on cool list: add check for paladin in party, make him fall if there isn't sufficient evidence

    public static void q16()
    {
        if (GetQuestState(16) == QuestState.Accepted || GetQuestState(16) == QuestState.Mentioned)
        {
            SetQuestState(16, QuestState.Completed);
            ScriptDaemon.record_time_stamp(431);
        }

        return;
    }
    public static void f41()
    {
        if (!GetGlobalFlag(41))
        {
            SetGlobalFlag(41, true);
            ScriptDaemon.record_time_stamp(432);
        }

        return;
    }

}