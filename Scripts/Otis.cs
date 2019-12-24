
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace Scripts
{
    [ObjectScript(97)]
    public class Otis : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(70, true);
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 350); // otis in party
            }
            else if ((GetGlobalVar(903) == 32 && attachee.GetMap() != 5051 && attachee.GetMap() != 5056))
            {
                triggerer.BeginDialog(attachee, 560); // have attacked 3 or more farm animals with otis in party and not in nulb exterior or nulb smithy
            }
            else if (((triggerer.FindItemByName(2202) != null) || (triggerer.FindItemByName(3008) != null)))
            {
                triggerer.BeginDialog(attachee, 330); // you have otis chainmail and-or longsword in inventory
            }
            else if (((GetQuestState(32) == QuestState.Completed) && (!GetGlobalFlag(74))))
            {
                triggerer.BeginDialog(attachee, 270); // have completed bribery for profit quest and have not revealed otis secret
            }
            else if ((GetQuestState(31) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 200); // have completed a second trip for otis quest
            }
            else if ((GetGlobalFlag(73)))
            {
                triggerer.BeginDialog(attachee, 120); // otis has been to toee with you
            }
            else if ((GetQuestState(63) == QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 700); // bribery for justice quest accepted
            }
            else
            {
                triggerer.BeginDialog(attachee, 1); // none of the above
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Logger.Info("Otis Enter Combat");
            if ((triggerer.type == ObjectType.pc))
            {
                Logger.Info("Triggered!!!");
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.RemoveFollower(attachee);
                }

                var elmo = Utilities.find_npc_near(attachee, 8000);
                if ((elmo != null))
                {
                    attachee.FloatLine(380, triggerer);
                    leader = elmo.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(elmo);
                    }

                    elmo.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // print "Otis heartbeat"
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                // if (game.global_vars[903] >= 3): #removed buggy animal farm scripting
                // if (attachee != OBJ_HANDLE_NULL):
                // leader = attachee.leader_get()
                // if (leader != OBJ_HANDLE_NULL):
                // leader.follower_remove(attachee)
                // attachee.float_line(22000,triggerer)
                if ((!GetGlobalFlag(362)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            if (((obj.FindItemByName(2202) != null) || (obj.FindItemByName(3008) != null)))
                            {
                                obj.BeginDialog(attachee, 330);
                                SetGlobalFlag(362, true);
                                return RunDefault;
                            }

                        }

                    }

                }

                if ((!GetGlobalFlag(72)))
                {
                    // print "elmo script"
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((obj.GetNameId() == 8000))
                        {
                            // print "elmo found"
                            var delPc = Utilities.GetDelegatePc(attachee, 10);
                            if ((delPc != null))
                            {
                                delPc.BeginDialog(attachee, 400);
                                return RunDefault;
                            }

                        }

                    }

                }

                if ((GetGlobalFlag(366)))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        attachee.TurnTowards(leader);
                        attachee.FloatLine(12023, leader);
                        leader.RemoveFollower(attachee);
                        attachee.Attack(leader);
                        SetGlobalFlag(366, false);
                        return RunDefault;
                    }

                }

                if ((GetGlobalFlag(367)))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        attachee.TurnTowards(leader);
                        attachee.FloatLine(10014, leader);
                        leader.RemoveFollower(attachee);
                        SetGlobalFlag(367, false);
                        return RunDefault;
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5051))
            {
                foreach (var chest in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CONTAINER))
                {
                    if ((chest.GetNameId() == 1202))
                    {
                        chest.TransferItemByNameTo(attachee, 2202);
                        chest.TransferItemByNameTo(attachee, 3008);
                        chest.TransferItemByNameTo(attachee, 12038);
                        chest.TransferItemByNameTo(attachee, 12040);
                        var reg_warhammer = attachee.FindItemByName(4077);
                        if ((reg_warhammer != null))
                        {
                            reg_warhammer.Destroy();
                        }

                        var reg_chainmail = attachee.FindItemByName(6019);
                        if ((reg_chainmail != null))
                        {
                            reg_chainmail.Destroy();
                        }

                        attachee.WieldBestInAllSlots();
                    }

                }

                var mag_longsword = attachee.FindItemByName(2202);
                if (mag_longsword != null)
                {
                    mag_longsword.SetItemFlag(ItemFlag.NO_TRANSFER);
                }

                var mag_chainmail = attachee.FindItemByName(3008);
                if (mag_chainmail != null)
                {
                    mag_chainmail.SetItemFlag(ItemFlag.NO_TRANSFER);
                }

                var blu_sapph = attachee.FindItemByName(12038);
                blu_sapph.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber = attachee.FindItemByName(12040);
                amber.SetItemFlag(ItemFlag.NO_TRANSFER);
            }
            else
            {
                var itemA = attachee.FindItemByName(4077);
                if ((itemA != null))
                {
                    itemA.Destroy();
                }

                var itemB = attachee.FindItemByName(6019);
                if ((itemB != null))
                {
                    itemB.Destroy();
                }

                var mag_sword = Utilities.create_item_in_inventory(4122, attachee);
                mag_sword.SetItemFlag(ItemFlag.NO_TRANSFER);
                var mag_armor = Utilities.create_item_in_inventory(6102, attachee);
                mag_armor.SetItemFlag(ItemFlag.NO_TRANSFER);
                var blu_sapp1 = Utilities.create_item_in_inventory(12038, attachee);
                blu_sapp1.SetItemFlag(ItemFlag.NO_TRANSFER);
                var blu_sapp2 = Utilities.create_item_in_inventory(12038, attachee);
                blu_sapp2.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber1 = Utilities.create_item_in_inventory(12040, attachee);
                amber1.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber2 = Utilities.create_item_in_inventory(12040, attachee);
                amber2.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber3 = Utilities.create_item_in_inventory(12040, attachee);
                amber3.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber4 = Utilities.create_item_in_inventory(12040, attachee);
                amber4.SetItemFlag(ItemFlag.NO_TRANSFER);
                var amber5 = Utilities.create_item_in_inventory(12040, attachee);
                amber5.SetItemFlag(ItemFlag.NO_TRANSFER);
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8021))
                {
                    triggerer.RemoveFollower(obj);
                }

                if ((obj.GetNameId() == 8022))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5066) || (attachee.GetMap() == 5067) || (attachee.GetMap() == 5105) || (attachee.GetMap() == 5079) || (attachee.GetMap() == 5080)))
            {
                SetGlobalFlag(73, true);
                if ((GetQuestState(31) == QuestState.Accepted))
                {
                    SetQuestState(31, QuestState.Completed);
                }

            }
            else if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5112)))
            {
                SetGlobalFlag(73, true);
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    if (((leader.GetAlignment() == Alignment.LAWFUL_EVIL) || (leader.GetAlignment() == Alignment.CHAOTIC_EVIL) || (leader.GetAlignment() == Alignment.NEUTRAL_EVIL)))
                    {
                        var percent = Utilities.group_percent_hp(leader);
                        if (((percent < 30) || (GetGlobalFlag(74))))
                        {
                            SetGlobalFlag(366, true);
                        }

                    }

                }

            }
            else if ((attachee.GetMap() == 5051))
            {
                if ((((GetGlobalFlag(73)) && (GetQuestState(31) == QuestState.Unknown)) || (GetQuestState(31) == QuestState.Completed)))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        SetGlobalFlag(367, true);
                    }

                }

            }

            return RunDefault;
        }
        public static bool make_elmo_talk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8000);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 410);
            }

            return SkipDefault;
        }
        public static bool make_saduj_talk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14689);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }

            return SkipDefault;
        }
        public static bool talk_to_screng(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8021);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 470);
            }

            return SkipDefault;
        }
        public static bool make_lila_talk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 14001);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 610);
            }

            return SkipDefault;
        }
        public static bool switch_to_thrommel(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8031);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 40);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 510);
            }

            return SkipDefault;
        }
        public static void chain_it(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(2202);
            if ((itemA != null))
            {
                itemA.ClearItemFlag(ItemFlag.NO_TRANSFER);
            }

            var itemB = attachee.FindItemByName(3008);
            if ((itemB != null))
            {
                itemB.ClearItemFlag(ItemFlag.NO_TRANSFER);
            }

            return;
        }

    }
}
