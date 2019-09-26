
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
    [ObjectScript(2)]
    public class BlackJay : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else if (((GetQuestState(3) == QuestState.Completed) || (GetQuestState(4) == QuestState.Completed)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if (((attachee.HasMet(triggerer)) || (GetQuestState(3) == QuestState.Accepted) || (GetQuestState(4) == QuestState.Accepted)))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

            return RunDefault;
        }
        // THIS IS DEFAULT SCRIPT TO GET TWO WEAPON FIGHTERS TO USE TWO WEAPONS (this is needed) ##

        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetNameId() == 14238 || attachee.GetNameId() == 14697 || attachee.GetNameId() == 14573 || attachee.GetNameId() == 14824) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
            {
                // Ettin
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14336 && (!attachee.HasEquippedByName(4081) || !attachee.HasEquippedByName(4126))))
            {
                // Elven Ranger
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14357 && (!attachee.HasEquippedByName(4156) || !attachee.HasEquippedByName(4159))))
            {
                // Grank's Bandit
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14747 && (!attachee.HasEquippedByName(4040) || !attachee.HasEquippedByName(4159))))
            {
                // Half Orc Assassin
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
            }

            if (((new[] { 14262, 14191, 14691 }).Contains(attachee.GetNameId()))) // trolls
            {
                if ((attachee.AddCondition("Rend")))
                {
                    Logger.Info("Added Rend");
                }

            }

            if (((new[] { 14050, 14391, 14352, 14406, 14408, 14610, 14994 }).Contains(attachee.GetNameId()))) // wolves
            {
                if ((attachee.AddCondition("Tripping Bite")))
                {
                    Logger.Info("Added Tripping Bite");
                }

            }

            if (((attachee.GetNameId() == 14238 || attachee.GetNameId() == 14697 || attachee.GetNameId() == 14573 || attachee.GetNameId() == 14824) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
            {
                // Ettin
                attachee.WieldBestInAllSlots();
            }
            // game.new_sid = 0
            else if ((attachee.GetNameId() == 14336 && (!attachee.HasEquippedByName(4081) || !attachee.HasEquippedByName(4126))))
            {
                // Elven Ranger
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }
            // game.new_sid = 0
            else if ((attachee.GetNameId() == 14357 && (!attachee.HasEquippedByName(4156) || !attachee.HasEquippedByName(4159))))
            {
                // Grank's Bandit
                attachee.WieldBestInAllSlots();
            }
            // game.new_sid = 0
            else if ((attachee.GetNameId() == 14747 && (!attachee.HasEquippedByName(4040) || !attachee.HasEquippedByName(4159))))
            {
                // Half Orc Assassin
                attachee.WieldBestInAllSlots();
            }
            else if (attachee.GetNameId() == 14057 && attachee.GetMap() == 5002)
            {
                // Giant Frogs of the Moathouse
                foreach (var obj in ObjList.ListVicinity(new locXY(492, 522), ObjectListFilter.OLC_NPC))
                {
                    if (obj.GetNameId() == 14057 && obj.GetLeader() == null)
                    {
                        obj.Attack(SelectedPartyLeader);
                    }

                }

                foreach (var obj in ObjList.ListVicinity(new locXY(470, 522), ObjectListFilter.OLC_NPC))
                {
                    if (obj.GetNameId() == 14057 && obj.GetLeader() == null)
                    {
                        obj.Attack(SelectedPartyLeader);
                    }

                }

                foreach (var obj in ObjList.ListVicinity(new locXY(473, 506), ObjectListFilter.OLC_NPC))
                {
                    if (obj.GetNameId() == 14057 && obj.GetLeader() == null)
                    {
                        obj.Attack(SelectedPartyLeader);
                    }

                }

            }
            else if (attachee.GetNameId() == 14337 && attachee.GetMap() == 5066)
            {
                // Earth Temple Guards
                if (attachee.GetMap() == 5066) // temple level 1 - guards near the corridor of bones - call in Earth Temple Fighters
                {
                    var (xx, yy) = attachee.GetLocation();
                    if ((Math.Pow((xx - 441), 2) + Math.Pow((yy - 504), 2)) < 200)
                    {
                        foreach (var npc in ObjList.ListVicinity(new locXY(441, 504), ObjectListFilter.OLC_NPC))
                        {
                            if ((new[] { 14337, 14338 }).Contains(npc.GetNameId()) && npc.GetLeader() == null && !npc.IsUnconscious())
                            {
                                var (xx2, yy2) = npc.GetLocation();
                                if ((Math.Pow((xx2 - 441), 2) + Math.Pow((yy2 - 504), 2)) < 300)
                                {
                                    // print str(xx) + " " + str(yy) + "  " + str(xx2) + " " + str(yy2)
                                    if (npc.GetScriptId(ObjScriptEvent.StartCombat) == 0)
                                    {
                                        npc.SetScriptId(ObjScriptEvent.StartCombat, 2);
                                    }

                                    npc.Attack(SelectedPartyLeader);
                                }

                            }

                        }

                    }

                }

            }
            else if (attachee.GetNameId() == 14338) // Earth Temple Fighters
            {
                if (attachee.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                {
                    attachee.WieldBestInAllSlots();
                }

            }
            else if (attachee.GetNameId() == 14888) // siren cultist screaching
            {
                Sound(4188, 1);
            }

            // ALSO, THIS IS USED FOR BREAK FREE
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            if (attachee.GetLeader() != null) // Don't wanna fuck up charmed enemies
            {
                return RunDefault;
            }

            // Special scripting		#
            if (attachee.GetNameId() == 14262 || attachee.GetNameId() == 14195 || attachee.GetNameId() == 14343 || attachee.GetNameId() == 14224 || attachee.GetNameId() == 14375)
            {
                // 14262 - Troll, 14195 - Oohlgrist, 14343 - Hydra, 14224 - Hydra Keeper
                // 14375 - Water Snake
                Logger.Info("{0}", "strategy chosen was " + attachee.GetInt(obj_f.critter_strategy).ToString());
            }
            else if (attachee.GetNameId() == 8091) // Belsornig
            {
                var goodguys = 0;
                var nonevilguys = 0;
                foreach (var dude in GameSystems.Party.PartyMembers)
                {
                    if (dude.GetAlignment().IsGood())
                    {
                        goodguys = goodguys + 1;
                    }

                    if ((dude.GetAlignment() & (Alignment.NEUTRAL_EVIL)) == 0)
                    {
                        nonevilguys = nonevilguys + 1;
                    }

                }

                if (goodguys <= 1 || (nonevilguys) <= 3)
                {
                    attachee.SetInt(obj_f.critter_strategy, 74); // Belsornig AI without Unholy Blight
                }

            }
            else if (attachee.GetNameId() == 14888) // Siren Cultist
            {
                if (Utilities.party_closest(attachee, mode_select:0) == null)
                {
                    attachee.SetInt(obj_f.critter_strategy, 573); // No PCs left to charm; charming NPCs causes them to be permanently hostile, essentially killing them unless charmed back or dominated, so I'll switch to an alternative spell list here
                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 572); // the charm tactic
                }

            }
            else if ((new[] { 14095, 14128, 14129 }).Contains(attachee.GetNameId()) && attachee.GetMap() == 5066)
            {
                var (xx, yy) = attachee.GetLocation();
                if ((yy <= 548 && yy >= 540 && xx >= 416 && xx <= 425) || (yy <= 540 && yy >= 530 && xx >= 421 && xx <= 425) || (xx <= 419 && yy <= 533 && yy >= 518)) // Room to the west of the harpy corridor
                {
                    if (attachee.D20Query(D20DispatcherKey.QUE_Turned))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PORTAL))
                        {
                            obj.SetObjectFlag(ObjectFlag.OFF);
                        }

                    }

                }

            }

            // Summoned Creaute/ Spiritual Weapon / Warded PC scripting	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            return RunDefault;
        }
        // THIS IS DEFAULT SCRIPT TO GET TWO WEAPON FIGHTERS TO USE TWO WEAPONS (this may not be needed) ##

        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GameSystems.Combat.IsCombatActive()))
            {
                return RunDefault;
            }

            if (((attachee.GetNameId() == 14238 || attachee.GetNameId() == 14697 || attachee.GetNameId() == 14573 || attachee.GetNameId() == 14824) && (!attachee.HasEquippedByName(4099) || !attachee.HasEquippedByName(4100))))
            {
                // Ettin
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14336 && attachee.FindItemByName(4058) != null))
            {
                // Elven Ranger
                var itemA = attachee.FindItemByName(4058);
                itemA.Destroy();
                Utilities.create_item_in_inventory(4126, attachee);
            }

            if ((attachee.GetNameId() == 14336 && (!attachee.HasEquippedByName(4081) || !attachee.HasEquippedByName(4126))))
            {
                // Elven Ranger
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14357 && attachee.FindItemByName(4087) != null))
            {
                // Grank's Bandit
                var itemA = attachee.FindItemByName(4087);
                itemA.Destroy();
                itemA = attachee.FindItemByName(5004);
                itemA.Destroy();
            }

            if ((attachee.GetNameId() == 14357 && (!attachee.HasEquippedByName(4156) || !attachee.HasEquippedByName(4159))))
            {
                // Grank's Bandit
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if ((attachee.GetNameId() == 14747 && (!attachee.HasEquippedByName(4040) || !attachee.HasEquippedByName(4159))))
            {
                // Half Orc Assassin
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }

    }
}
