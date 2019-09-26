
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(62)]
    public class Rannosdavl : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5074))
            {
                // Found them in the wilderness
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 820);
                return SkipDefault;
            }

            if (((GetGlobalFlag(835) || GetGlobalFlag(837)) && !GetGlobalFlag(37) && attachee.HasMet(triggerer) && !(GetQuestState(16) == QuestState.Completed) && !(GetQuestState(15) == QuestState.Completed) && !GetGlobalFlag(843)))
            {
                // Lareth Prisoner subplot node (enables to ask about master)
                triggerer.BeginDialog(attachee, 2500);
            }

            if ((GetQuestState(16) == QuestState.Completed && GetQuestState(15) == QuestState.Completed))
            {
                // Ratted Traders to Burne
                triggerer.BeginDialog(attachee, 20);
            }
            else if ((GetGlobalFlag(41) || GetQuestState(16) == QuestState.Completed || GetQuestState(64) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 290);
            }
            else if ((GetQuestState(17) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else if ((GetGlobalFlag(31) || GetQuestState(64) == QuestState.Botched))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5010))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(750) == 0))
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetMap() == 5010))
            {
                rngfighttime_set();
                SetGlobalFlag(426, true);
            }

            SetGlobalFlag(814, true);
            if ((GetGlobalFlag(815)))
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
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc && GetGlobalVar(750) == 0))
            {
                var raimol = Utilities.find_npc_near(attachee, 8050);
                if ((raimol != null))
                {
                    attachee.FloatLine(380, triggerer);
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
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(750) == 1 && attachee.GetMap() == 5010 && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                SetGlobalVar(750, 2);
                Utilities.create_item_in_inventory(8010, attachee);
                return RunDefault;
            }

            if ((GetGlobalVar(750) == 2 && attachee.GetMap() == 5010 && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                SetGlobalVar(750, 3);
                if ((!PartyLeader.HasReputation(23)))
                {
                    PartyLeader.AddReputation(23);
                }

                attachee.RunOff();
                return SkipDefault;
            }

            if ((GetGlobalVar(751) == 0 && attachee.GetStat(Stat.hp_current) >= 0 && GetGlobalFlag(815) && attachee.GetMap() == 5010) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & (1 << 10)) == 0))
            {
                GameObjectBody found_pc = null;
                var gremag = Utilities.find_npc_near(attachee, 8049);
                var raimol = Utilities.find_npc_near(attachee, 8050);
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    if (pc.type == ObjectType.pc && !pc.IsUnconscious())
                    {
                        found_pc = pc;
                        attachee.AIRemoveFromShitlist(pc);
                        gremag.AIRemoveFromShitlist(pc);
                        raimol.AIRemoveFromShitlist(pc);
                    }
                    else
                    {
                        attachee.AIRemoveFromShitlist(pc);
                        gremag.AIRemoveFromShitlist(pc);
                        raimol.AIRemoveFromShitlist(pc);
                        pc.AIRemoveFromShitlist(attachee);
                        pc.AIRemoveFromShitlist(gremag);
                        pc.AIRemoveFromShitlist(raimol);
                    }

                }

                if ((found_pc != null))
                {
                    found_pc.BeginDialog(attachee, 1100);
                    return SkipDefault;
                }

            }

            if ((Utilities.obj_percent_hp(attachee) < 95 && GetGlobalVar(750) == 0 && attachee.GetStat(Stat.hp_current) >= 0 && attachee.GetMap() == 5010) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & (1 << 10)) == 0))
            {
                GameObjectBody found_pc = null;
                var gremag = Utilities.find_npc_near(attachee, 8049);
                var raimol = Utilities.find_npc_near(attachee, 8050);
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    if (pc.type == ObjectType.pc && !pc.IsUnconscious())
                    {
                        found_pc = pc;
                        attachee.AIRemoveFromShitlist(pc);
                        gremag.AIRemoveFromShitlist(pc);
                        raimol.AIRemoveFromShitlist(pc);
                    }
                    else
                    {
                        attachee.AIRemoveFromShitlist(pc);
                        gremag.AIRemoveFromShitlist(pc);
                        raimol.AIRemoveFromShitlist(pc);
                        pc.AIRemoveFromShitlist(attachee);
                        pc.AIRemoveFromShitlist(gremag);
                        pc.AIRemoveFromShitlist(raimol);
                    }

                }

                if ((found_pc != null))
                {
                    if ((!GetGlobalFlag(815) && gremag.GetStat(Stat.hp_current) >= 0))
                    {
                        found_pc.BeginDialog(attachee, 1000);
                        return SkipDefault;
                    }

                    if ((GetGlobalFlag(815) || gremag.GetStat(Stat.hp_current) <= -1))
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
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(814, false);
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(8010);
            if ((itemA != null && GetGlobalVar(750) == 0 && attachee.GetMap() == 5010))
            {
                itemA.Destroy();
                // create_item_in_inventory( 8021, attachee )
                DetachScript();
            }

            return RunDefault;
        }
        public static bool switch_to_gremag(GameObjectBody rannos, GameObjectBody pc)
        {
            var gremag = Utilities.find_npc_near(rannos, 8049);
            SetGlobalVar(750, 1);
            SetGlobalVar(751, 1);
            pc.BeginDialog(gremag, 1010);
            rannos.TurnTowards(gremag);
            gremag.TurnTowards(rannos);
            return SkipDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1004);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }
        public static bool buff_npc(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool buff_npc_two(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool buff_npc_three(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var target = Utilities.find_npc_near(attachee, 8049);
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
        public static bool buff_npc_four(GameObjectBody attachee, GameObjectBody triggerer)
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
}
