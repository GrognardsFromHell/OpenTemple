
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

[ObjectScript(450)]
public class LarethTroops : BaseObjectScript
{
    // IMPORTANT NOTICE!
    // I have directly copied many scripts instead of using functions.
    // This is because when you call another script, it often fails to execute.
    // Thus I wished to cut back on them as much as possible.
    // Flags / Vars documentation:
    // ggv400: Bitwise flags
    // 2**0 - Seleucas engaged in battle
    // 2**1 - Seleucas dead
    // 2**2 - Lareth has been called upon by his men
    // 2**3 - Lareth has entered the fray
    // 2**4 - Abort Lareth's villain speech
    // 2**5 - Entered combat with Lareth without having entered combat with Seleucas
    // ggv401: Mini variables
    // 0-2: Lareth Float Comment Stage
    // 3-6: Seleucas Battle Death count
    // 7-10: Seleucas Battle Knockout count
    // 11-14: Seleucas Battle Charm count
    // ggv403: Moathouse  Reactive behavior flags
    // 0 - Moathouse regrouped;
    // 1 - New guard guy shouts out to Seleucas - enables his KOS;
    // 2 - KOS thing played out
    // pad3:
    // 2**1 - marked as having contributed to Seleucas Battle Knockout count
    // 2**2 - marked as having contributed to Seleucas Battle Charm count
    // 2**5 - marked as having contributed to Seleucas Battle Death count
    // 2**6 - solo guard who went to alert Seleucas
    // 2**7 - have had Sleep successfully cast upon (see Spell438 - Sleep.py)
    // 2**8 - Beacon designator
    // 2**9 - Archer guy put to sleep - skip turn when woken up to prevent approach, then reset

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(37) && (GetGlobalFlag(49) || !GetGlobalFlag(48))))
        {
            triggerer.BeginDialog(attachee, 40);
        }
        else if ((GetGlobalFlag(49)))
        {
            triggerer.BeginDialog(attachee, 60);
        }
        else if ((GetGlobalFlag(48)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }

    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        return OnDying(attachee, triggerer, false);
    }
        
    public bool OnDying(GameObject attachee, GameObject triggerer, bool generated_from_timed_event_call)
    {
        if (GetGlobalFlag(403) && GetGlobalFlag(405))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 12, TextFloaterColor.Red);
        }

        var ggv400 = GetGlobalVar(400);
        var ggv401 = GetGlobalVar(401);
        var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
        if (!generated_from_timed_event_call)
        {
            SetGlobalVar(756, GetGlobalVar(756) + 1);
            StartTimer(100 + RandomRange(0, 50), () => OnDying(attachee, triggerer, true), true);
        }

        if ((ggv400 & (1)) != 0) // Fighting Seleucas
        {
            if (attachee.IsUnconscious() && (pad3 & (0x20)) == 0)
            {
                pad3 += (0x20);
                attachee.SetInt(obj_f.npc_pad_i_3, pad3); // Mark him as having contributed to the dying count
                ggv401 += (1 << 3);
                SetGlobalVar(401, ggv401);
            }

            // Was this guy unconscious before dying?
            // (thus having contributed to the unconscious count)
            // Unmark his internal flag and reduce the count of unconscious guys
            if ((pad3 & (2)) != 0)
            {
                pad3 ^= (2);
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                if (((ggv401 >> 7) & 15) > 0)
                {
                    ggv401 -= (1 << 7);
                }

                SetGlobalVar(401, ggv401);
            }

        }

        if (attachee.GetNameId() == 14077)
        {
            // Seleucas dead
            ggv400 |= 2;
            SetGlobalVar(400, ggv400);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if (GetGlobalFlag(403) && GetGlobalFlag(405))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 13, TextFloaterColor.Red);
        }

        if (attachee.GetNameId() == 14077) // Seleucas
        {
            var ggv400 = GetGlobalVar(400);
            var ggv403 = GetGlobalVar(403);
            ggv400 = ggv400 | (1); // indicate that you have entered combat with Seleucas' group
            SetGlobalVar(400, ggv400);
            if ((ggv403 & (2)) != 0) // Moathouse regroup scenario; Seleucas is warned by guard of approaching party
            {
                ggv403 |= 0x4;
                SetGlobalVar(403, ggv403);
            }

            attachee.RemoveScript(ObjScriptEvent.Heartbeat); // His heartbeat script - disabled, in case hostilies momentarily cease / fight ends with him charmed
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                SelectedPartyLeader.FloatMesFileLine("mes/script_activated.mes", 10001, TextFloaterColor.Red);
            }

        }
        else if (attachee.GetNameId() == 14074)
        {
            var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
            if ((pad3 & (0x40)) != 0)
            {
                attachee.FloatLine(15002, triggerer);
                attachee.RemoveScript(ObjScriptEvent.Dialog);
                pad3 ^= 0x40;
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
            }

        }

        var a123 = attachee.ItemWornAt(EquipSlot.WeaponPrimary).GetInt(obj_f.weapon_type);
        if ((new[] { 14, 17, 46, 48, 68 }).Contains(a123))
        {
            // 14 - light crossbow
            // 17 - heavy crossbow
            // 46 - shortbow
            // 48 - longbow
            // 68 - repeating crossbow
            var dummy = 1;
        }
        else
        {
            Livonya.tag_strategy(attachee);
            if ((new[] { 37, 41, 43, 44, 61 }).Contains(a123))
            {
                // Reach Weapons
            }
            else
            {
                Livonya.get_melee_strategy(attachee);
            }

        }

        return RunDefault;
    }

    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if (GetGlobalFlag(403) && GetGlobalFlag(405))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 14, TextFloaterColor.Red);
        }

        return RunDefault;
    }

    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        return OnStartCombat(attachee, triggerer, false);
    }

    public bool OnStartCombat(GameObject attachee, GameObject triggerer, bool generated_from_timed_event_call)
    {
        if (attachee.GetBaseStat(Stat.dexterity) == -30)
        {
            // Beacon object - skip its turn in case it somehow survives
            attachee.Destroy();
            return SkipDefault;
        }

        if (GetGlobalFlag(403) && GetGlobalFlag(405))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
        }

        var ggv400 = GetGlobalVar(400);
        var ggv401 = GetGlobalVar(401);
        var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
        if ((ggv400 & (1)) != 0)
        {
            if (attachee.IsUnconscious() && (pad3 & (2)) == 0)
            {
                // Troop knocked unconscious - mark him as such and increase the KOed counter
                pad3 |= 2;
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                ggv401 += 1 << 7;
                SetGlobalVar(401, ggv401);
            }
            else if ((pad3 & (2)) != 0 && !attachee.IsUnconscious())
            {
                // Attachee has contributed to unconscious count, but is no longer unconscious
                // E.g. healed
                pad3 &= ~(2);
                if (((ggv401 >> 7) & 15) > 0)
                {
                    ggv401 -= (1 << 7);
                }

                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                SetGlobalVar(401, ggv401);
            }

            if ((new[] { 14074, 14075, 14076, 14077 }).Contains(attachee.GetNameId()))
            {
                // The "Call Lareth" section
                // Calculates how many troops are down etc. and decides whether to call Lareth
                GameObject seleucas = null;
                GameObject lareth_sarge = null;
                foreach (var obj1 in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (obj1.GetNameId() == 14077)
                    {
                        seleucas = obj1;
                    }
                    else if (obj1.GetNameId() == 14076)
                    {
                        lareth_sarge = obj1;
                    }

                }

                bool seleucas_down;
                if ((seleucas != null && seleucas.IsUnconscious()) || (ggv400 & (2)) != 0 || (seleucas.GetLeader() != null))
                {
                    seleucas_down = true;
                }
                else
                {
                    seleucas_down = false;
                }

                bool lareth_sarge_down;
                if ((lareth_sarge != null && lareth_sarge.IsUnconscious()))
                {
                    lareth_sarge_down = true;
                }
                else
                {
                    lareth_sarge_down = false;
                }

                var troops_down = ((ggv401 >> 3) & 15) + ((ggv401 >> 7) & 15);
                var should_call_lareth = false;
                if (seleucas_down && (troops_down >= 2)) // Seleucas + 1 other troop
                {
                    should_call_lareth = true;
                }
                else if (seleucas_down && lareth_sarge_down) // Seleucas + Sergeant
                {
                    should_call_lareth = true;
                }
                else if (troops_down >= 2)
                {
                    should_call_lareth = true;
                }

                if (should_call_lareth)
                {
                    if ((ggv400 & (0x4 + (0x20))) == 0)
                    {
                        // Lareth has not been called upon
                        // And have not initiated combat with Lareth
                        if (seleucas_down && !attachee.IsUnconscious() && (attachee.GetLeader() == null))
                        {
                            // Seleucas is down - the soldier calls Lareth
                            var temppp = attachee.GetScriptId(ObjScriptEvent.Dialog);
                            attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                            attachee.FloatLine(15001, triggerer);
                            attachee.SetScriptId(ObjScriptEvent.Dialog, temppp);
                            ggv400 |= 0x4;
                            SetGlobalVar(400, ggv400);
                        }
                        else if (!seleucas_down)
                        {
                            // Seleucas is alive - Seleucas calls Lareth
                            seleucas.FloatLine(15000, triggerer);
                            ggv400 |= 0x4;
                            SetGlobalVar(400, ggv400);
                        }

                    }
                    else if ((ggv400 & 0x28) == 0)
                    {
                        var lareth = Utilities.find_npc_near(attachee, 8002);
                        if (lareth != null && !lareth.IsUnconscious())
                        {
                            // if can_see_party(lareth) == 0:
                            lareth.SetInt(obj_f.critter_strategy, 81); // Lareth's "Join the Fray" strategy - start with shield of faith, then target hurt friendly, heal, and commence attack
                            lareth.SetScriptId(ObjScriptEvent.EndCombat, 60); // san_end_combat
                            lareth.Move(new locXY(476, 546), -60, 10); // This is to decrease the chances of Lareth skipping his turn
                            var closest_party_member = SelectedPartyLeader;
                            foreach (var pcc in GameSystems.Party.PartyMembers)
                            {
                                if (pcc.DistanceTo(attachee) < closest_party_member.DistanceTo(attachee))
                                {
                                    closest_party_member = pcc;
                                }

                            }

                            lareth.CastSpell(WellKnownSpells.ShieldOfFaith, lareth);
                            lareth.Attack(closest_party_member);
                            var init_current = -100;
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if (pc.GetInitiative() < attachee.GetInitiative() && (pc.GetInitiative() - 2) > init_current)
                                {
                                    init_current = pc.GetInitiative() - 2;
                                }

                            }

                            lareth.SetInitiative(init_current); // make sure he gets to act on the same round
                            ggv400 |= 8;
                            SetGlobalVar(400, ggv400);
                        }

                    }

                }

            }

            if (attachee.GetLeader() != null && (pad3 & 0x4) == 0)
            {
                // Attachee charmed
                pad3 |= 0x4;
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                ggv401 += (1 << 11);
                SetGlobalVar(401, ggv401);
            }

        }

        // Copied from script 310 :	#
        // THIS IS USED FOR BREAK FREE
        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        if ((attachee.D20Query(D20DispatcherKey.QUE_Is_BreakFree_Possible))) // workaround no longer necessary!
        {
            return RunDefault;
        }

        // create_item_in_inventory( 8903, attachee )
        // attachee.d20_send_signal(S_BreakFree)
        // Wake up from Sleep Scripting	#
        var a123 = attachee.ItemWornAt(EquipSlot.WeaponPrimary).GetInt(obj_f.weapon_type);
        if ((new[] { 14, 17, 46, 48, 68 }).Contains(a123)) // (Is archer)
        {
            // 14 - light crossbow
            // 17 - heavy crossbow
            // 46 - shortbow
            // 48 - longbow
            // 68 - repeating crossbow
            if (!generated_from_timed_event_call && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null)
            {
                var player_in_obscuring_mist = 0;
                var grease_detected = 0;
                var should_wake_up_comrade = 0;
                // First, find closest party member - the most likely target for an archer
                var closest_pc_1 = SelectedPartyLeader;
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    if (pc.DistanceTo(attachee) < closest_pc_1.DistanceTo(attachee))
                    {
                        closest_pc_1 = pc;
                    }

                }

                foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 30, 0, 360))
                {
                    // Check for active OBSCURING MIST spell objects
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 333 + (1 << 15) && spell_obj.DistanceTo(closest_pc_1) <= 17.5f)
                    {
                        player_in_obscuring_mist = 1;
                    }

                }

                foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 40, 0, 360))
                {
                    // Check for active GREASE spell object
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 200 + (1 << 15))
                    {
                        grease_detected = 1;
                        var grease_obj = spell_obj;
                    }

                }

                // Scripting for approaching sleeping friend
                if (player_in_obscuring_mist == 1 || RandomRange(0, 1) == 0)
                {
                    // Player staying back in Obscuring Mist - thus it's more useful to use the archer to wake someone up
                    // Otherwise, 50% chance
                    should_wake_up_comrade = 1;
                }

                if (closest_pc_1.DistanceTo(attachee) <= 8)
                {
                    // Player is close - Attachee will probably get hit trying to approach friend, so abort
                    should_wake_up_comrade = 0;
                }

                if (player_in_obscuring_mist == 1 && RandomRange(1, 12) == 1)
                {
                    // Player Cast Obscuring Mist -
                    // Float a comment about not being able to see the player (1 in 12 chance)
                    if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                    {
                        attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                    }

                    attachee.FloatLine(16520 + RandomRange(0, 2), attachee);
                }

            }

        }
        else
        {
            Livonya.tag_strategy(attachee);
            if ((new[] { 37, 41, 43, 44, 61 }).Contains(a123))
            {
                // Reach Weapons
            }
            else
            {
                SetGlobalVar(499, 19);
                Livonya.get_melee_strategy(attachee);
            }

        }

        // Spiritual Weapon Shenanigens	#
        CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
        return RunDefault;
    }

    public override bool OnEndCombat(GameObject attachee, GameObject triggerer)
    {
        return OnEndCombat(attachee, triggerer, false);
    }

    public bool OnEndCombat(GameObject attachee, GameObject triggerer, bool generated_from_timed_event_call)
    {
        var ggv400 = GetGlobalVar(400);
        var ggv401 = GetGlobalVar(401);
        var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
        if (GetGlobalFlag(403) && GetGlobalFlag(405))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 16, TextFloaterColor.Red);
        }

        if ((ggv400 & (1)) != 0)
        {
            if (attachee.IsUnconscious() && (pad3 & (2)) == 0)
            {
                pad3 |= 2;
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                ggv401 += 1 << 7;
                SetGlobalVar(401, ggv401);
            }
            else if ((pad3 & (2)) != 0 && !attachee.IsUnconscious())
            {
                // Attachee has contributed to unconscious count, but is no longer unconscious
                // E.g. healed
                pad3 &= ~(2);
                if (((ggv401 >> 7) & 15) > 0)
                {
                    ggv401 -= (1 << 7);
                }

                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                SetGlobalVar(401, ggv401);
            }

            if ((new[] { 14074, 14075, 14076, 14077 }).Contains(attachee.GetNameId()))
            {
                GameObject seleucas = null;
                GameObject lareth_sarge = null;
                foreach (var obj1 in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (obj1.GetNameId() == 14077)
                    {
                        seleucas = obj1;
                    }
                    else if (obj1.GetNameId() == 14076)
                    {
                        lareth_sarge = obj1;
                    }

                }

                bool seleucas_down;
                bool lareth_sarge_down;
                if ((seleucas != null && seleucas.IsUnconscious()) || (ggv400 & (2)) != 0 || seleucas.GetLeader() != null)
                {
                    seleucas_down = true;
                }
                else
                {
                    seleucas_down = false;
                }

                if ((lareth_sarge != null && lareth_sarge.IsUnconscious()))
                {
                    lareth_sarge_down = true;
                }
                else
                {
                    lareth_sarge_down = false;
                }

                var troops_down = ((ggv401 >> 3) & 15) + ((ggv401 >> 7) & 15);
                var should_call_lareth = false;
                if (seleucas_down && (troops_down >= 2)) // Seleucas + 1 other troop
                {
                    should_call_lareth = true;
                }
                else if (seleucas_down && lareth_sarge_down) // Seleucas + Sergeant
                {
                    should_call_lareth = true;
                }
                else if (troops_down >= 2)
                {
                    should_call_lareth = true;
                }

                if (should_call_lareth)
                {
                    if ((ggv400 & 0x24) == 0)
                    {
                        // Lareth has not been called upon
                        // And have not initiated combat with Lareth
                        if (seleucas_down && !attachee.IsUnconscious() && attachee.GetLeader() == null)
                        {
                            var temppp = attachee.GetScriptId(ObjScriptEvent.Dialog);
                            attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                            attachee.FloatLine(15001, triggerer);
                            attachee.SetScriptId(ObjScriptEvent.Dialog, temppp);
                            ggv400 |= 0x4;
                            SetGlobalVar(400, ggv400);
                        }
                        else if (!seleucas_down)
                        {
                            seleucas.FloatLine(15000, triggerer);
                            ggv400 |= 0x4;
                            SetGlobalVar(400, ggv400);
                        }

                    }
                    else if ((ggv400 & (0x8)) == 0)
                    {
                        var lareth = Utilities.find_npc_near(attachee, 8002);
                        if (lareth != null && !lareth.IsUnconscious())
                        {
                            // if can_see_party(lareth) == 0:
                            lareth.SetInt(obj_f.critter_strategy, 81); // Lareth's "Join the Fray" strategy - start with shield of faith, then target hurt friendly, heal, and commence attack
                            lareth.SetScriptId(ObjScriptEvent.EndCombat, 60); // san_end_combat
                            lareth.Move(new locXY(476, 546), -60, 10); // This is to decrease the chances of Lareth skipping his turn
                            var closest_party_member = SelectedPartyLeader;
                            foreach (var pcc in GameSystems.Party.PartyMembers)
                            {
                                if (pcc.DistanceTo(attachee) < closest_party_member.DistanceTo(attachee))
                                {
                                    closest_party_member = pcc;
                                }

                            }

                            lareth.CastSpell(WellKnownSpells.ShieldOfFaith, lareth);
                            lareth.Attack(closest_party_member);
                            var init_current = -100;
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if (pc.GetInitiative() < attachee.GetInitiative() && (pc.GetInitiative() - 2) > init_current)
                                {
                                    init_current = pc.GetInitiative() - 2;
                                }

                            }

                            lareth.SetInitiative(init_current); // make sure he gets to act on the same round
                            ggv400 |= 0x8;
                            SetGlobalVar(400, ggv400);
                        }

                    }

                }

            }

            if (attachee.GetLeader() != null && (pad3 & 0x4) == 0)
            {
                // Attachee charmed
                pad3 |= 0x4;
                attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                ggv401 += (1 << 11);
                SetGlobalVar(401, ggv401);
            }

        }

        if (!generated_from_timed_event_call && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone))
        {
            // Wake up sleeping guy script
            var bbb = attachee.GetInt(obj_f.critter_strategy);
            if (bbb == 84) // if using the 'seek beacon' strategy
            {
                bbb = attachee.GetInt(obj_f.pad_i_0); // retrieve previous strat
                attachee.SetInt(obj_f.critter_strategy, bbb);
                var has_woken_someone_up = 0;
                foreach (var obj in ObjList.ListCone(attachee, ObjectListFilter.OLC_NPC, 10, 0, 360))
                {
                    if (((14074..14078)).Contains(obj.GetNameId()) && obj != attachee)
                    {
                        var obj_pad3 = obj.GetInt(obj_f.npc_pad_i_3);
                        if ((obj_pad3 & (0x100)) != 0) // is a beacon object
                        {
                            obj.Destroy();
                        }
                        else if ((obj_pad3 & ((1 << 7))) != 0 && obj.DistanceTo(attachee) <= 4.2f && has_woken_someone_up == 0)
                        {
                            obj_pad3 &= ~((1 << 7)); // remove "sleepig flag"
                            obj.SetInt(obj_f.npc_pad_i_3, obj_pad3);
                            if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                            {
                                attachee.SetScriptId(ObjScriptEvent.Dialog, 450); // Set the correct san_dialog
                            }

                            attachee.TurnTowards(obj);
                            attachee.FloatLine(16500 + RandomRange(0, 3), attachee);
                            obj.Damage(null, DamageType.Subdual, Dice.Parse("1d1"));
                            has_woken_someone_up = 1;
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()) && (!GetGlobalFlag(363)) && (attachee.GetLeader() == null))
        {
            if ((attachee.DistanceTo(PartyLeader) <= 22 && attachee.HasLineOfSight(PartyLeader) && !Utilities.critter_is_unconscious(PartyLeader)))
            {
                if ((!attachee.HasMet(PartyLeader)))
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 1);
                    DetachScript();
                }
                else if ((!GetGlobalFlag(49) && GetGlobalFlag(48) && GetGlobalFlag(62)))
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 50);
                    DetachScript();
                }
                else if ((GetGlobalFlag(49)))
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 60);
                    DetachScript();
                }
                else
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 70);
                    DetachScript();
                }

            }
            else
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((attachee.DistanceTo(obj) <= 20 && attachee.HasLineOfSight(obj) && !Utilities.critter_is_unconscious(obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                        }
                        else if ((!GetGlobalFlag(49) && GetGlobalFlag(48) && GetGlobalFlag(62)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 50);
                        }
                        else if ((GetGlobalFlag(49)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 60);
                        }
                        else
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 70);
                        }

                    }

                }

            }

        }

        // Don't scrap the script - because of the "bring the leader here" treatment
        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if (attachee.DistanceTo(triggerer) <= 31 && (GetGlobalVar(403) & ((2) + 0x4)) == 2)
        {
            return RunDefault;
        }

        return SkipDefault;
    }
    public override bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
    {
        if (spell.spellEnum == WellKnownSpells.Fireball)
        {
            StartTimer(1250 + RandomRange(0, 1000), () => OnStartCombat(attachee, triggerer, true), true);
            if (RandomRange(0, 5) == 0 && attachee.GetNameId() == 14074)
            {
                if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                {
                    attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                }

                if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 450 && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    attachee.FloatLine(16000 + RandomRange(0, 6), triggerer);
                }

            }

        }
        else
        {
            StartTimer(4500 + RandomRange(0, 1000), () => OnStartCombat(attachee, triggerer, true), true);
        }

        return RunDefault;
    }
    public static void call_leader(GameObject npc, GameObject pc)
    {
        var leader = PartyLeader;
        leader.Move(pc.GetLocation().OffsetTiles(-2, 0));
        leader.BeginDialog(npc, 1);
        return;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        var loc = new locXY(526, 569);
        attachee.RunOff(loc);
        return RunDefault;
    }
    public static bool run_off_to_back(GameObject attachee, GameObject triggerer, bool generated_from_timevent = false)
    {
        if (!generated_from_timevent)
        {
            attachee.SetNpcFlag(NpcFlag.WAYPOINTS_DAY);
            attachee.SetNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
            attachee.RunOff(attachee.GetLocation().OffsetTiles(-10, 0));
            SetGlobalVar(403, GetGlobalVar(403) | 2);
            var obj = GameSystems.MapObject.CreateObject(14074, new locXY(485, 538));
            obj.SetScriptId(ObjScriptEvent.Dialog, 450);
            obj.SetScriptId(ObjScriptEvent.Dying, 450);
            obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
            obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
            obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
            obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
            obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
            var pad3 = obj.GetInt(obj_f.npc_pad_i_3);
            pad3 |= 0x40;
            obj.SetInt(obj_f.npc_pad_i_3, pad3);
            obj.Move(new locXY(485, 538), 0, 0);
            obj.TurnTowards(SelectedPartyLeader); // just to correct the animation glitch
            obj.Rotation = 3.3f;
            StartTimer(1000, () => run_off_to_back(attachee, triggerer, true), true);
        }
        else
        {
            attachee.Destroy();
        }

        return RunDefault;
    }
    public static bool move_pc(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5005, 537, 545);
        // triggerer.move( location_from_axis( 537, 545 ) )
        return RunDefault;
    }
    public static bool deliver_pc(GameObject attachee, GameObject triggerer)
    {
        triggerer.Move(new locXY(491, 541));
        return RunDefault;
    }

}