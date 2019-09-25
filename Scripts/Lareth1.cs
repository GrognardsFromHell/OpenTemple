
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
    [ObjectScript(60)]
    public class Lareth1 : BaseObjectScript
    {
        // pad3: internal flags
        // 2**1 - have cast Dispell

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(834, true);
            if ((triggerer.GetPartyMembers().Any(o => o.HasItemByName(11003)) && attachee.FindItemByName(11003) == null))
            {
                if ((GetGlobalFlag(198)))
                {
                    triggerer.BeginDialog(attachee, 1000);
                }
                else if ((attachee.GetLeader() != null))
                {
                    if ((GetGlobalFlag(53)))
                    {
                        triggerer.BeginDialog(attachee, 1100);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 1200);
                    }

                }
                else if ((GetGlobalFlag(52)))
                {
                    triggerer.BeginDialog(attachee, 1300);
                }
                else if ((GetGlobalFlag(48)))
                {
                    triggerer.BeginDialog(attachee, 1400);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1500);
                }

            }
            else if ((GetGlobalFlag(198)))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if ((attachee.GetLeader() != null))
            {
                if ((GetGlobalFlag(53)))
                {
                    triggerer.BeginDialog(attachee, 320);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 220);
                }

            }
            else if ((GetGlobalFlag(52)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if ((GetGlobalFlag(48)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null && !attachee.IsAffectedBySpell() && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL)))
            {
                attachee.CastSpell(WellKnownSpells.DetectLaw, attachee);
                attachee.PendingSpellsToMemorized();
                SetGlobalVar(726, 2);
            }

            if ((attachee.GetLeader() == null && !attachee.IsAffectedBySpell() && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_EVIL)))
            {
                attachee.CastSpell(WellKnownSpells.DetectChaos, attachee);
                attachee.PendingSpellsToMemorized();
                SetGlobalVar(726, 2);
            }

            if ((PartyAlignment == Alignment.NEUTRAL_GOOD && attachee.GetLeader() == null && !attachee.IsAffectedBySpell()))
            {
                attachee.CastSpell(WellKnownSpells.DetectGood, attachee);
                attachee.PendingSpellsToMemorized();
                SetGlobalVar(726, 2);
            }

            if ((PartyAlignment == Alignment.NEUTRAL_EVIL && attachee.GetLeader() == null && !attachee.IsAffectedBySpell()))
            {
                attachee.CastSpell(WellKnownSpells.DetectEvil, attachee);
                attachee.PendingSpellsToMemorized();
                SetGlobalVar(726, 2);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            // if game.global_flags[277] == 0:
            // Kalshane's idea for Raimol ratting the party out to the traders and triggering the assassination - put on hold
            // for obj in game.party:
            // if obj.name == 8050:
            // a = game.encounter_queue
            // b = 1
            // for enc_id in a:
            // if enc_id == 3000:
            // b = 0
            // if b == 1:
            // game.encounter_queue.append(3000)
            // game.global_flags[420] = 1
            ScriptDaemon.record_time_stamp(425);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
                SetGlobalFlag(37, true);
                if ((StoryState <= 1))
                {
                    StoryState = 2;
                }

                return RunDefault;
            }

            attachee.FloatLine(12014, triggerer);
            SetGlobalFlag(37, true);
            if ((StoryState <= 1))
            {
                StoryState = 2;
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.HasReputation(18)))
                {
                    pc.RemoveReputation(18);
                }

            }

            attachee.FloatLine(12014, triggerer);
            PartyLeader.AddReputation(15);
            if ((GetGlobalFlag(340)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        SetGlobalFlag(834, true);
                        obj.BeginDialog(attachee, 370);
                        attachee.FloatLine(12014, triggerer);
                    }

                }

            }
            else if ((!GetGlobalFlag(62)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        SetGlobalFlag(834, true);
                        obj.BeginDialog(attachee, 390);
                        attachee.FloatLine(12014, triggerer);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0)
        {
            attachee.FloatLine(12058, triggerer); // "You have earned my wrath!"
            if ((attachee.GetLeader() == null && (!attachee.HasEquippedByName(1) || !attachee.HasEquippedByName(4071))))
            {
                attachee.WieldBestInAllSlots();
                DetachScript();
            }

            if (attachee.GetMap() == 5005)
            {
                var ggv400 = GetGlobalVar(400);
                if ((ggv400 & (Math.Pow(2, 0))) == 0)
                {
                    ggv400 |= Math.Pow(2, 5);
                    SetGlobalVar(400, ggv400);
                }

                foreach (var obj in ObjList.ListVicinity(new locXY(487, 537), ObjectListFilter.OLC_NPC))
                {
                    if ((range(14074, 14078)).Contains(obj.GetNameId()) && obj.GetLeader() == null)
                    {
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                        if (triggerer.type == ObjectType.pc || triggerer.GetLeader() != null)
                        {
                            obj.Attack(triggerer);
                        }
                        else
                        {
                            obj.Attack(SelectedPartyLeader);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0, GameObjectBody talk_stage)
        {
            if (attachee.IsUnconscious())
            {
                return RunDefault;
            }

            // if generated_from_timed_event_call == 0 and attachee.distance_to( party_closest(attachee) ) > 45:
            // attachee.move( party_closest(attachee).location, 0 , 0)
            // for pp in range(0, 41):
            // attachee.scripts[pp] = 998
            // game.leader.ai_follower_add(attachee)
            // #game.timevent_add( lareth_abandon, (attachee, triggerer), 20, 1)
            var curr = attachee.GetStat(Stat.hp_current) - attachee.GetStat(Stat.subdual_damage);
            var maxx = attachee.GetStat(Stat.hp_max);
            var xx = attachee.GetLocation() & (65535);
            var hp_percent_lareth = 100 * curr / maxx;
            var ggv400 = GetGlobalVar(400);
            var ggv401 = GetGlobalVar(401);
            var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
            if ((attachee.GetMap() == 5005) && (party_too_far_from_lareth(attachee)) && (generated_from_timed_event_call == 0))
            {
                if (((pad3 & (Math.Pow(2, 2))) == 0))
                {
                    // Delay for one round, letting him cast Shield of Faith - he'll need it :)
                    pad3 |= (Math.Pow(2, 2));
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    return;
                }

                // Party is too far from Lareth, gotta nudge him in the right direction
                // spawn a beacon and change Lareth's strat to approach it
                if (xx > 478)
                {
                    var beacon_loc = 498 + ((550) << 32);
                }
                else
                {
                    var beacon_loc = 487 + ((540) << 32);
                }

                var obj_beacon = GameSystems.MapObject.CreateObject(14074, beacon_loc);
                obj_beacon.SetObjectFlag(ObjectFlag.DONTDRAW);
                obj_beacon.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
                obj_beacon.Move(beacon_loc, 0, 0);
                obj_beacon.SetBaseStat(Stat.dexterity, -30);
                obj_beacon.SetInt(obj_f.npc_pad_i_3, Math.Pow(2, 8));
                obj_beacon.Attack(SelectedPartyLeader);
                obj_beacon.AddToInitiative();
                attachee.SetInt(obj_f.pad_i_0, attachee.GetInt(obj_f.critter_strategy)); // Record original strategy
                attachee.SetInt(obj_f.critter_strategy, 80); // set Lareth's strat to "seek beacon"
                var grease_detected = 0;
                foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 40, 0, 360))
                {
                    // Check for active GREASE spell object
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 200 + (1 << 15))
                    {
                        grease_detected = 1;
                        var grease_obj = spell_obj;
                    }

                }

                if (grease_detected)
                {
                    // In case Lareth slips and doesn't execute his san_end_combat (wherein the beacon object is destroyed) - spawn a couple of timed events to guarantee the beacon doesn't survive
                    StartTimer(3700, () => kill_beacon_obj(obj_beacon, attachee), true);
                    StartTimer(3900, () => kill_beacon_obj(obj_beacon, attachee), true);
                }

                return RunDefault;
            }

            // strategy 81 - Approach Party strategy
            if (attachee.GetInt(obj_f.critter_strategy) == 81 && generated_from_timed_event_call == 0)
            {
                if (ScriptDaemon.can_see_party(attachee))
                {
                    attachee.SetInt(obj_f.critter_strategy, 82);
                }

            }

            if (attachee.GetInt(obj_f.critter_strategy) != 81 && generated_from_timed_event_call == 0)
            {
                // Should Lareth cast Obscuring Mist?
                // First, find closest party member - the most likely target for an archer
                var closest_pc_1 = SelectedPartyLeader;
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    if (pc.DistanceTo(attachee) < closest_pc_1.DistanceTo(attachee))
                    {
                        closest_pc_1 = pc;
                    }

                }

                // Then, check for spell objects with the Obscuring Mist ID, which are also identified as active
                var player_in_obscuring_mist = 0;
                foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 30, 0, 360))
                {
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 333 + (1 << 15) && spell_obj.DistanceTo(closest_pc_1) <= 17.5f)
                    {
                        player_in_obscuring_mist = 1;
                    }

                }

                var player_cast_web = 0;
                var player_cast_entangle = 0;
                foreach (var spell_obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_GENERIC))
                {
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 531 + (1 << 15))
                    {
                        player_cast_web = 1;
                    }

                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 153 + (1 << 15))
                    {
                        player_cast_entangle = 1;
                    }

                }

                // Assess level of ranged weapon threat
                var ranged_threat = 0;
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    var pc_weap = pc.ItemWornAt(EquipSlot.WeaponPrimary).GetInt(obj_f.weapon_type);
                    if ((new[] { 14, 17, 46, 48, 68 }).Contains(pc_weap) && !pc.IsUnconscious())
                    {
                        // 14 - light crossbow
                        // 17 - heavy crossbow
                        // 46 - shortbow
                        // 48 - longbow
                        // 68 - repeating crossbow
                        if (ranged_threat == 0)
                        {
                            ranged_threat = 1;
                        }

                        if (pc.HasFeat(FeatId.POINT_BLANK_SHOT) || (pc.GetStat(Stat.level_fighter) + pc.GetStat(Stat.level_ranger)) >= 1)
                        {
                            if (ranged_threat < 2)
                            {
                                ranged_threat = 2;
                            }

                        }

                        if (pc.HasFeat(FeatId.PRECISE_SHOT) && (pc.GetStat(Stat.level_fighter) + pc.GetStat(Stat.level_ranger)) >= 1)
                        {
                            if (ranged_threat < 3)
                            {
                                ranged_threat = 3;
                            }

                        }

                    }

                }

                if ((attachee.GetMap() == 5005 && xx > 478) && (((ggv401 >> 25) & 3) == 0) && ((ranged_threat == 3) || (ranged_threat > 1 && player_in_obscuring_mist == 1) || (ranged_threat > 0 && (player_cast_entangle || player_cast_web))))
                {
                    // Cast Obscuring Mist, if:
                    // 1. Haven't cast it yet  - (ggv401 >> 25) & 3
                    // 2. Ranged threat exists (emphasized when player casts web or is in obscuring mist)
                    // Give him a potion of Obscuring Mist, to simulate him having that scroll (just like I do...)
                    if (attachee.FindItemByProto(8899) == null)
                    {
                        attachee.GetItem(GameSystems.MapObject.CreateObject(8899, attachee.GetLocation()));
                    }

                    ggv401 += 1 << 25;
                    SetGlobalVar(401, ggv401);
                    var lareth_is_threatened = 0;
                    if (closest_pc_1.DistanceTo(attachee) <= 3)
                    {
                        lareth_is_threatened = 1;
                    }

                    if (lareth_is_threatened == 1)
                    {
                        attachee.SetInt(obj_f.critter_strategy, 85); // Obscuring Mist + 5ft step
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 86); // Just Obscuring Mist
                    }

                }
                else if (((pad3 & (Math.Pow(2, 1))) == 0) && (player_cast_entangle || player_cast_web))
                {
                    attachee.SetInt(obj_f.critter_strategy, 87); // Dispel strat
                    pad3 |= (Math.Pow(2, 1));
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                }
                else if (attachee.GetMap() == 5005 && player_entrenched_in_corridor(attachee))
                {
                    attachee.SetInt(obj_f.critter_strategy, 89);
                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 82);
                }

            }

            if ((hp_percent_lareth < 50) && (generated_from_timed_event_call == 0 || generated_from_timed_event_call == 1 && talk_stage == 667))
            {
                if ((ggv400 & (Math.Pow(2, 6))) == 0)
                {
                    GameObjectBody found_pc = null;
                    var obj_list = ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC);
                    // Extending the range a little...
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation() - 35, ObjectListFilter.OLC_NPC))
                    {
                        if (!((obj_list).Contains(obj)))
                        {
                            obj_list += (obj);
                        }

                    }

                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation() + 35, ObjectListFilter.OLC_NPC))
                    {
                        if (!((obj_list).Contains(obj)))
                        {
                            obj_list += (obj);
                        }

                    }

                    foreach (var obj in obj_list)
                    {
                        foreach (var pc in SelectedPartyLeader.GetPartyMembers())
                        {
                            if (pc.type == ObjectType.pc && !pc.IsUnconscious())
                            {
                                found_pc = pc;
                            }

                            // if obj.name in ([attachee.name] + range(14074, 14078)):
                            obj.AIRemoveFromShitlist(pc);
                            obj.SetReaction(pc, 50);
                            obj.RemoveFromInitiative(obj);
                            if (pc.type == ObjectType.npc)
                            {
                                pc.AIRemoveFromShitlist(obj);
                            }

                        }

                        foreach (var obj2 in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_ALL))
                        {
                            if (obj2.type == ObjectType.pc || obj2.type == ObjectType.npc)
                            {
                                obj2.SetReaction(obj, 50);
                                try
                                {
                                    obj2.AIRemoveFromShitlist(obj);
                                }
                                finally
                                {
                                    var dummy = 1;
                                }

                                obj2.RemoveFromInitiative(obj2);
                            }

                        }

                    }

                    if (generated_from_timed_event_call == 0)
                    {
                        StartTimer(100, () => san_start_combat(attachee, triggerer, 1, 667), true);
                    }
                    else if (found_pc != null)
                    {
                        ggv400 |= Math.Pow(2, 6);
                        SetGlobalVar(400, ggv400);
                        SetGlobalFlag(834, true);
                        found_pc.BeginDialog(attachee, 160);
                        return SkipDefault;
                    }

                }

            }
            else if (generated_from_timed_event_call == 0 && !GetGlobalFlag(834))
            {
                if (((ggv401 >> 15) & 7) == 0)
                {
                    ggv401 += 1 << 15;
                    SetGlobalVar(401, ggv401);
                }
                else if (((ggv401 >> 15) & 7) == 1)
                {
                    var closest_distance_1 = SelectedPartyLeader.DistanceTo(attachee);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        closest_distance_1 = Math.Min(closest_distance_1, pc.DistanceTo(attachee));
                    }

                    if (closest_distance_1 < 45)
                    {
                        for (var ppq = 3; ppq < 26; ppq++)
                        {
                            StartTimer(ppq * 2500 + RandomRange(0, 20), () => san_start_combat(attachee, triggerer, 1, ppq), true);
                        }

                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }

                }

            }
            else if (generated_from_timed_event_call == 1 && !GetGlobalFlag(834))
            {
                if ((hp_percent_lareth > 75) && (ggv400 & (Math.Pow(2, 4))) == 0 && !attachee.D20Query(D20DispatcherKey.QUE_Prone))
                {
                    if (talk_stage >= 3 && ((ggv401 >> 15) & 31) == 2)
                    {
                        attachee.FloatLine(6000, triggerer);
                        Sound(4201, 1);
                        Sound(4201, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 3 && ((ggv401 >> 15) & 31) == 3)
                    {
                        Sound(4202, 1);
                        Sound(4202, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 3 && ((ggv401 >> 15) & 31) == 4)
                    {
                        Sound(4203, 1);
                        Sound(4203, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 8 && ((ggv401 >> 15) & 31) == 5)
                    {
                        attachee.FloatLine(6001, triggerer);
                        Sound(4204, 1);
                        Sound(4204, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 8 && ((ggv401 >> 15) & 31) == 6)
                    {
                        Sound(4205, 1);
                        Sound(4205, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 13 && ((ggv401 >> 15) & 31) == 7)
                    {
                        attachee.FloatLine(6002, triggerer);
                        Sound(4206, 1);
                        Sound(4206, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 13 && ((ggv401 >> 15) & 31) == 8)
                    {
                        Sound(4207, 1);
                        Sound(4207, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 18 && ((ggv401 >> 15) & 31) == 9)
                    {
                        attachee.FloatLine(6003, triggerer);
                        Sound(4208, 1);
                        Sound(4208, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 18 && ((ggv401 >> 15) & 31) == 10)
                    {
                        Sound(4209, 1);
                        Sound(4209, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 22 && ((ggv401 >> 15) & 31) == 11)
                    {
                        attachee.FloatLine(6004, triggerer);
                        Sound(4210, 1);
                        Sound(4210, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 22 && ((ggv401 >> 15) & 31) == 12)
                    {
                        attachee.FloatLine(6004, triggerer);
                        Sound(4211, 1);
                        Sound(4211, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }
                    else if (talk_stage >= 22 && ((ggv401 >> 15) & 31) == 13)
                    {
                        attachee.FloatLine(6004, triggerer);
                        Sound(4212, 1);
                        Sound(4212, 1);
                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }

                }
                else if ((hp_percent_lareth <= 75) && (ggv400 & (Math.Pow(2, 4))) == 0)
                {
                    if (((ggv401 >> 15) & 31) > 2)
                    {
                        attachee.FloatLine(6005, triggerer);
                        Sound(4200, 1);
                        Sound(4200, 1);
                    }

                    StartTimer(5500, () => san_start_combat(attachee, triggerer, 1, 667), true);
                    ggv400 |= Math.Pow(2, 4);
                    SetGlobalVar(400, ggv400);
                }

            }

            // Spiritual Weapon Shenanigens	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0)
        {
            if (attachee.IsUnconscious())
            {
                return RunDefault;
            }

            var curr = attachee.GetStat(Stat.hp_current) - attachee.GetStat(Stat.subdual_damage);
            var maxx = attachee.GetStat(Stat.hp_max);
            var xx = attachee.GetLocation() & (65535);
            var hp_percent_lareth = 100 * curr / maxx;
            var ggv400 = GetGlobalVar(400);
            var ggv401 = GetGlobalVar(401);
            if (generated_from_timed_event_call == 0 && !GetGlobalFlag(834))
            {
                if (((ggv401 >> 15) & 7) == 1)
                {
                    var closest_distance_1 = SelectedPartyLeader.DistanceTo(attachee);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        closest_distance_1 = Math.Min(closest_distance_1, pc.DistanceTo(attachee));
                    }

                    if (closest_distance_1 <= 25 || xx > 480)
                    {
                        for (var ppq = 3; ppq < 26; ppq++)
                        {
                            StartTimer(ppq * 2500 + RandomRange(0, 20), () => san_start_combat(attachee, triggerer, 1, ppq), true);
                        }

                        ggv401 += 1 << 15;
                        SetGlobalVar(401, ggv401);
                    }

                }

            }

            if (generated_from_timed_event_call == 0)
            {
                // Wake up sleeping guy script
                var bbb = attachee.GetInt(obj_f.critter_strategy);
                if (bbb == 80) // if using the 'seek beacon' strategy
                {
                    bbb = attachee.GetInt(obj_f.pad_i_0); // retrieve previous strat
                    attachee.SetInt(obj_f.critter_strategy, bbb);
                    foreach (var obj in ObjList.ListCone(attachee, ObjectListFilter.OLC_NPC, 20, 0, 360))
                    {
                        if ((range(14074, 14078)).Contains(obj.GetNameId()) && obj != attachee)
                        {
                            var obj_pad3 = obj.GetInt(obj_f.npc_pad_i_3);
                            if ((obj_pad3 & (Math.Pow(2, 8))) != 0) // is a beacon object
                            {
                                obj.Destroy();
                            }

                        }

                    }

                }

            }

            // if can_see_party(attachee):
            // flash_signal(10)
            // attachee.obj_set_int(obj_f_critter_strategy, 82)
            // game.timevent_add(san_end_combat, ( attachee, triggerer, 1), 300, 1)
            // else:
            // if can_see_party(attachee):
            // flash_signal(10)
            // attachee.obj_set_int(obj_f_critter_strategy, 82)
            var dummy = 1;
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(37, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.HasReputation(15)))
                {
                    pc.RemoveReputation(15);
                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((!pc.HasReputation(18)))
                {
                    pc.AddReputation(18);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(726, GetGlobalVar(726) + 1);
            if ((GameSystems.Combat.IsCombatActive()))
            {
                return RunDefault;
            }

            if ((attachee.GetLeader() == null && (!attachee.HasEquippedByName(1) || !attachee.HasEquippedByName(4071))))
            {
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }

            if ((GetGlobalVar(726) == 1 && attachee.GetLeader() == null && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL)))
            {
                attachee.CastSpell(WellKnownSpells.DetectLaw, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(726) == 1 && attachee.GetLeader() == null && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_EVIL)))
            {
                attachee.CastSpell(WellKnownSpells.DetectChaos, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(726) == 1 && PartyAlignment == Alignment.NEUTRAL_GOOD && attachee.GetLeader() == null))
            {
                attachee.CastSpell(WellKnownSpells.DetectGood, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(726) == 1 && PartyAlignment == Alignment.NEUTRAL_EVIL && attachee.GetLeader() == null))
            {
                attachee.CastSpell(WellKnownSpells.DetectEvil, attachee);
                attachee.PendingSpellsToMemorized();
            }

            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((attachee.DistanceTo(obj) <= 10 && GetGlobalVar(726) == 5))
                {
                    if ((attachee.GetLeader() == null && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL)))
                    {
                        attachee.CastSpell(WellKnownSpells.MagicCircleAgainstLaw, attachee);
                        attachee.PendingSpellsToMemorized();
                    }

                }

            }

            // return RUN_DEFAULT
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((attachee.DistanceTo(obj) <= 10 && GetGlobalVar(726) == 5))
                {
                    if ((attachee.GetLeader() == null && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_EVIL)))
                    {
                        attachee.CastSpell(WellKnownSpells.MagicCircleAgainstChaos, attachee);
                        attachee.PendingSpellsToMemorized();
                    }

                }

            }

            // return RUN_DEFAULT
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((attachee.DistanceTo(obj) <= 10 && GetGlobalVar(726) == 5))
                {
                    if ((PartyAlignment == Alignment.NEUTRAL_GOOD && attachee.GetLeader() == null))
                    {
                        attachee.CastSpell(WellKnownSpells.MagicCircleAgainstGood, attachee);
                        attachee.PendingSpellsToMemorized();
                    }

                }

            }

            // return RUN_DEFAULT
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((attachee.DistanceTo(obj) <= 10 && GetGlobalVar(726) == 5))
                {
                    if ((PartyAlignment == Alignment.NEUTRAL_EVIL && attachee.GetLeader() == null))
                    {
                        attachee.CastSpell(WellKnownSpells.MagicCircleAgainstEvil, attachee);
                        attachee.PendingSpellsToMemorized();
                    }

                }

            }

            // return RUN_DEFAULT
            if ((GetGlobalVar(726) >= 200))
            {
                SetGlobalVar(726, 0);
            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((StoryState <= 1 && !GetGlobalFlag(806)))
            {
                StoryState = 2;
            }

            if ((GetGlobalFlag(340) && !GetGlobalFlag(806)))
            {
                // Playing demo version
                SetGlobalFlag(834, true);
                triggerer.BeginDialog(attachee, 380);
            }

            // Make his surviving fellows disappear
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (((range(14074, 14078))).Contains(obj.GetNameId()) && obj.GetStat(Stat.hp_current) >= 0 && obj.GetLeader() == null)
                {
                    obj.RunOff();
                    GetGlobalVar(756) += 1;
                    StartTimer(1000 + RandomRange(0, 200), () => destroy(obj), true);
                }

            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(806)))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    attachee.AIRemoveFromShitlist(pc);
                    attachee.SetReaction(pc, 50);
                }

                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        npc.AIRemoveFromShitlist(pc);
                        npc.SetReaction(pc, 50);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var randy1 = RandomRange(1, 12); // added by ShiningTed
                                             // if (attachee.map == 5065):				## Removed by Livonya
                                             // leader = attachee.leader_get()		## Removed by Livonya
                                             // if (leader != OBJ_HANDLE_NULL):		## Removed by Livonya
                                             // leader.begin_dialog( attachee, 270 )## Removed by Livonya
            if (((attachee.GetMap() == 5111) && (!GetGlobalFlag(200))))
            {
                SetGlobalFlag(200, true);
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 290);
                }

            }

            if ((attachee.GetMap() == 5066))
            {
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 300);
                }

            }

            if ((!GetGlobalFlag(833) && (attachee.GetMap() == 5001 || attachee.GetMap() == 5051)))
            {
                var leader = attachee.GetLeader();
                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 600);
                }

            }

            if (((attachee.GetMap() == 5013) && randy1 >= 6)) // added by ShiningTed
            {
                attachee.FloatLine(12029, triggerer);
            }

            if (((attachee.GetMap() == 5042) && randy1 >= 8)) // added by ShiningTed
            {
                attachee.FloatLine(12060, triggerer);
            }

            return RunDefault;
        }
        public static bool buff_npc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 14424 && obj.GetLeader() == null))
                {
                    obj.TurnTowards(attachee);
                    obj.CastSpell(WellKnownSpells.MageArmor, obj);
                }

                if ((obj.GetNameId() == 14425 && obj.GetLeader() == null))
                {
                    obj.TurnTowards(attachee);
                    obj.CastSpell(WellKnownSpells.ShieldOfFaith, obj);
                }

            }

            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
            return RunDefault;
        }
        public static bool buff_npc_two(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 14424 && obj.GetLeader() == null))
                {
                    obj.CastSpell(WellKnownSpells.MirrorImage, obj);
                }

                if ((obj.GetNameId() == 14425 && obj.GetLeader() == null))
                {
                    obj.CastSpell(WellKnownSpells.OwlsWisdom, obj);
                }

            }

            attachee.CastSpell(WellKnownSpells.BullsStrength, attachee);
            return RunDefault;
        }
        public static bool buff_npc_three(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasEquippedByName(1) || !attachee.HasEquippedByName(4071)))
            {
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            ScriptDaemon.record_time_stamp(425);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            attachee.RunOff();
            var obj_list = ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC);
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation() - 35, ObjectListFilter.OLC_NPC))
            {
                if (!((obj_list).Contains(obj)))
                {
                    obj_list += (obj);
                }

            }

            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation() + 35, ObjectListFilter.OLC_NPC))
            {
                if (!((obj_list).Contains(obj)))
                {
                    obj_list += (obj);
                }

            }

            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation() + 60, ObjectListFilter.OLC_NPC))
            {
                if (!((obj_list).Contains(obj)))
                {
                    obj_list += (obj);
                }

            }

            foreach (var obj in obj_list)
            {
                if (((range(14074, 14078))).Contains(obj.GetNameId()) && obj.GetStat(Stat.hp_current) >= 0 && obj.GetLeader() == null)
                {
                    obj.RunOff();
                    GetGlobalVar(756) += 1;
                    StartTimer(1000 + RandomRange(0, 200), () => destroy(obj), true);
                }

            }

            // if game.global_flags[277] == 0:
            // Raimol rats the party out to the traders
            // for obj in game.party:
            // if obj.name == 8050:
            // a = game.encounter_queue
            // b = 1
            // for enc_id in a:
            // if enc_id == 3000:
            // b = 0
            // if b == 1:
            // game.global_flags[420] = 1
            // game.encounter_queue.append(3000)
            return RunDefault;
        }
        public static bool demo_end_game(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // play slides and end game
            return RunDefault;
        }
        // added by ShiningTed

        public static bool argue_ron(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            // def argue_ron( attachee, triggerer, line): # added by ShiningTed
            var npc = Utilities.find_npc_near(attachee, 14681);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
                triggerer.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 260);
            }

            return SkipDefault;
        }
        public static bool equip_transfer(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemA = attachee.FindItemByName(4071);
            if ((itemA != null))
            {
                itemA.Destroy();
                Utilities.create_item_in_inventory(4071, triggerer);
            }

            Utilities.create_item_in_inventory(7001, attachee);
            return RunDefault;
        }
        // added by ShiningTed

        public static bool create_spiders(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def create_spiders( attachee, triggerer ): # added by ShiningTed
            // Modified by Sitra Achara -
            // Checks no. of troop casualties
            // If less than 5 - do not spawn spiders
            var ggv401 = GetGlobalVar(401);
            var troop_casualties = ((ggv401 >> 3) & 15) + ((ggv401 >> 7) & 15);
            if (troop_casualties < 5)
            {
                return;
            }

            var spider1 = GameSystems.MapObject.CreateObject(14397, new locXY(474, 535));
            AttachParticles("sp-summon monster I", spider1);
            Sound(4033, 1);
            spider1.TurnTowards(PartyLeader);
            var spider2 = GameSystems.MapObject.CreateObject(14398, new locXY(481, 536));
            AttachParticles("sp-summon monster I", spider2);
            spider2.TurnTowards(PartyLeader);
            var spider3 = GameSystems.MapObject.CreateObject(14398, new locXY(470, 536));
            AttachParticles("sp-summon monster I", spider3);
            spider3.TurnTowards(PartyLeader);
            var spider4 = GameSystems.MapObject.CreateObject(14417, new locXY(529, 544));
            var spider5 = GameSystems.MapObject.CreateObject(14417, new locXY(532, 555));
            return RunDefault;
        }
        public static int lareth_troops_state()
        {
            GameObjectBody seleucas = null;
            GameObjectBody lareth_sarge_1 = null;
            GameObjectBody lareth_sarge_2 = null;
            var troop_count = 0;
            foreach (var obj in ObjList.ListVicinity(new locXY(490, 535), ObjectListFilter.OLC_NPC))
            {
                if (obj.GetNameId() == 14074)
                {
                    if (!obj.IsUnconscious())
                    {
                        troop_count += 1;
                    }
                    else
                    {
                        var curr = obj.GetStat(Stat.hp_current);
                        var maxx = obj.GetStat(Stat.hp_max);
                    }

                }
                else if (obj.GetNameId() == 14077)
                {
                    seleucas = obj;
                }
                else if (obj.GetNameId() == 14075)
                {
                    lareth_sarge_1 = obj;
                }
                else if (obj.GetNameId() == 14076)
                {
                    lareth_sarge_2 = obj;
                }

            }

            return 0;
        }
        // Destroys object.  Neccessary for time event destruction to work.

        public static int destroy(GameObjectBody obj)
        {
            // def destroy(obj): # Destroys object.  Neccessary for time event destruction to work.
            obj.Destroy();
            return 1;
        }
        public static void lareth_abandon(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SelectedPartyLeader.RemoveFollower(attachee);
            return;
        }
        public static int party_too_far_from_lareth(FIXME lareth)
        {
            foreach (var pc in SelectedPartyLeader.GetPartyMembers())
            {
                var xx = pc.GetLocation() & (65535);
                if (xx <= 495 || lareth.distance_to/*Unknown*/(pc) <= 42)
                {
                    return 0;
                }

            }

            return 1;
        }
        public static int player_entrenched_in_corridor(GameObjectBody attachee)
        {
            var outside_corridor_count = 0;
            var pc_count = 0;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc_count += 1;
                var xx = pc.GetLocation() & (65535);
                var yy = pc.GetLocation() >> 32;
                if (xx < 495 || yy < 546)
                {
                    outside_corridor_count += 1;
                    return 0;
                }

            }

            if (outside_corridor_count >= 2)
            {
                return 0;
            }
            else if (pc_count <= 2 && outside_corridor_count == 1)
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
        public static void kill_beacon_obj(GameObjectBody obj_beacon, GameObjectBody attachee)
        {
            if (attachee.D20Query(D20DispatcherKey.QUE_Prone))
            {
                obj_beacon.Destroy();
            }

        }

    }
}
