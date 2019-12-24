
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
    [ObjectScript(185)]
    public class Knight : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.GetAlignment().IsGood())
            {
                GameObjectBody good_pc = null;
                foreach (var obj in GameSystems.Party.PartyMembers)
                {
                    if ((ScriptDaemon.is_safe_to_talk_rfv(attachee, obj, 45, false, false) && obj.GetAlignment().IsGood()))
                    {
                        good_pc = obj;
                    }

                }

                if (good_pc != null)
                {
                    attachee.TurnTowards(triggerer);
                    triggerer.BeginDialog(attachee, 200); // "I would prefer to speak to the good hearted one"
                }

            }
            else if ((attachee.FindItemByName(3014) != null))
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 50);
            }
            else
            {
                attachee.TurnTowards(triggerer);
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

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RemoveScript(ObjScriptEvent.Heartbeat); // to prevent the "popup after death" bug
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetScriptId(ObjScriptEvent.EnterCombat) == 0)
            {
                attachee.SetScriptId(ObjScriptEvent.EnterCombat, 185); // assigns san_enter_combat if one doesn't exist
            }

            // This script's execution of searching for a good aligned PC usually fails
            // The reason: it searches for a PC at a distance of 15
            // Very often, your good aligned PC would be standing slightly farther away
            // While another PC would be just at that distance
            // So the script would fail to find the good aligned PC, and find the nearer PC and initiate conversation with that PC
            // Suggested fix: search for a good PC slightly farther away (distance 25)
            // If one is not found, look for any PC at distance 15
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                GameObjectBody good_pc = null;
                GameObjectBody paladin_pc = null;
                GameObjectBody good_tank_pc = null;
                GameObjectBody good_cleric_pc = null;
                GameObjectBody faraway_good_pc = null;
                // near_pc = OBJ_HANDLE_NULL
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((ScriptDaemon.is_safe_to_talk_rfv(attachee, obj, 25)))
                    {
                        // near_pc = obj
                        if ((obj.GetAlignment().IsGood()))
                        {
                            good_pc = obj;
                            if ((obj.GetStat(Stat.level_paladin) > 1))
                            {
                                paladin_pc = obj;
                            }

                            if ((obj.GetStat(Stat.level_fighter) > 1) || (obj.GetStat(Stat.level_barbarian) > 1) || (obj.GetStat(Stat.level_ranger) > 1))
                            {
                                good_tank_pc = obj;
                            }

                            if ((obj.GetStat(Stat.level_cleric) > 1))
                            {
                                good_cleric_pc = obj;
                            }

                        }

                    }

                }

                // if (obj.item_find(3014) != OBJ_HANDLE_NULL ): # Prince Thrommel's Golden Amulet (name ID; shared with halfling sai)
                // obj.begin_dialog(attachee,50)
                // game.new_sid = 0
                // return RUN_DEFAULT
                if ((paladin_pc != null))
                {
                    attachee.TurnTowards(paladin_pc);
                    paladin_pc.BeginDialog(attachee, 1);
                    DetachScript();
                }
                else if ((good_tank_pc != null))
                {
                    attachee.TurnTowards(good_tank_pc);
                    good_tank_pc.BeginDialog(attachee, 1);
                    DetachScript();
                }
                else if ((good_cleric_pc != null))
                {
                    attachee.TurnTowards(good_cleric_pc);
                    good_cleric_pc.BeginDialog(attachee, 1);
                    DetachScript();
                }
                else if ((good_pc != null))
                {
                    attachee.TurnTowards(good_pc);
                    good_pc.BeginDialog(attachee, 1);
                    DetachScript();
                }
                else
                {
                    GameObjectBody near_pc = null;
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((ScriptDaemon.is_safe_to_talk_rfv(attachee, obj, 15)))
                        {
                            near_pc = obj;
                            if ((obj.GetAlignment().IsGood()))
                            {
                                good_pc = obj;
                            }

                            if ((obj.FindItemByName(3014) != null)) // Prince Thrommel's Golden Amulet (name ID; shared with halfling sai)
                            {
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 50);
                                DetachScript();
                                return RunDefault;
                            }

                        }

                        if ((ScriptDaemon.is_safe_to_talk_rfv(attachee, obj, 45, false, false) && obj.GetAlignment().IsGood()))
                        {
                            faraway_good_pc = obj;
                        }

                    }

                    if ((near_pc != null))
                    {
                        attachee.TurnTowards(near_pc);
                        if (faraway_good_pc == null)
                        {
                            near_pc.BeginDialog(attachee, 1);
                        }
                        else
                        {
                            near_pc.BeginDialog(attachee, 200);
                        }

                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public static bool distribute_magic_items(GameObjectBody npc, GameObjectBody pc)
        {
            foreach (var obj in pc.GetPartyMembers())
            {
                obj.AdjustMoney(2000000);
                Utilities.create_item_in_inventory(8007, obj);
                Utilities.create_item_in_inventory(6082, obj);
            }

            return RunDefault;
        }
        public static bool transfer_scrolls(GameObjectBody npc, GameObjectBody pc)
        {
            // give out 12 first level wizard scrolls to pc
            Utilities.create_item_in_inventory(9288, pc);
            Utilities.create_item_in_inventory(9280, pc);
            Utilities.create_item_in_inventory(9438, pc);
            Utilities.create_item_in_inventory(9431, pc);
            Utilities.create_item_in_inventory(9383, pc);
            Utilities.create_item_in_inventory(9509, pc);
            Utilities.create_item_in_inventory(9467, pc);
            Utilities.create_item_in_inventory(9333, pc);
            Utilities.create_item_in_inventory(9238, pc);
            Utilities.create_item_in_inventory(9229, pc);
            Utilities.create_item_in_inventory(9159, pc);
            Utilities.create_item_in_inventory(9056, pc);
            return RunDefault;
        }
        public static bool knight_party(GameObjectBody npc, GameObjectBody pc)
        {
            pc.AddReputation(22);
            foreach (var obj in pc.GetPartyMembers())
            {
                Utilities.create_item_in_inventory(6128, obj);
                Utilities.create_item_in_inventory(6129, obj);
            }

            return RunDefault;
        }
        public static void run_off(GameObjectBody npc, GameObjectBody pc)
        {
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() == null && !((SelectedPartyLeader.GetPartyMembers()).Contains(obj))))
                {
                    obj.RunOff();
                }

            }

            return;
        }
        public static void call_good_pc(GameObjectBody npc, GameObjectBody pc)
        {
            npc.RemoveScript(ObjScriptEvent.Heartbeat); // remove heartbeat so it doesn't interfere
            GameObjectBody good_pc = null;
            GameObjectBody paladin_pc = null;
            GameObjectBody good_tank_pc = null;
            GameObjectBody good_cleric_pc = null;
            GameObjectBody new_talker = null;
            // game.particles( "sp-summon monster I", pc ) # fired ok
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((ScriptDaemon.is_safe_to_talk_rfv(npc, obj, 45, false, false) && obj.GetAlignment().IsGood()))
                {
                    // game.particles( 'Orb-Summon-Fire-Elemental', pc )
                    good_pc = obj;
                    if ((obj.GetStat(Stat.level_paladin) > 1))
                    {
                        paladin_pc = obj;
                    }

                    if ((obj.GetStat(Stat.level_fighter) > 1) || (obj.GetStat(Stat.level_barbarian) > 1) || (obj.GetStat(Stat.level_ranger) > 1))
                    {
                        good_tank_pc = obj;
                    }

                    if ((obj.GetStat(Stat.level_cleric) > 1))
                    {
                        good_cleric_pc = obj;
                    }

                }

            }

            if ((paladin_pc != null))
            {
                new_talker = paladin_pc;
            }
            else if ((good_tank_pc != null))
            {
                new_talker = good_tank_pc;
            }
            else if ((good_cleric_pc != null))
            {
                new_talker = good_cleric_pc;
            }
            else if ((good_pc != null))
            {
                new_talker = good_pc;
            }

            if (new_talker == null) // failsafe
            {
                new_talker = pc;
                new_talker.BeginDialog(npc, 240);
            }
            else
            {
                new_talker.Move(pc.GetLocation().OffsetTiles(-2, 0));
                new_talker.TurnTowards(npc);
                npc.TurnTowards(new_talker);
                new_talker.BeginDialog(npc, 220);
                return;
            }

            return;
        }

    }
}
