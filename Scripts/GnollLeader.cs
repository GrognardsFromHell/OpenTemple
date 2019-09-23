
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
    [ObjectScript(75)]
    public class GnollLeader : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(288) && GetGlobalFlag(856) && GetGlobalFlag(857)))
            {
                triggerer.BeginDialog(attachee, 200);
            }

            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
            {
                triggerer.BeginDialog(attachee, 100);
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

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetMap() == 5005)
            {
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14067, 14078, 14079, 14080 }).Contains(npc.GetNameId()) && npc.GetLeader() == null && npc.GetScriptId(ObjScriptEvent.StartCombat) == 0)
                    {
                        npc.SetScriptId(ObjScriptEvent.StartCombat, 75);
                    }

                }

            }

            if ((GetGlobalVar(709) == 1 && attachee.GetMap() == 5005))
            {
                SetGlobalVar(709, 2);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14079 || obj.GetNameId() == 14080 || obj.GetNameId() == 14067 || obj.GetNameId() == 14078 || obj.GetNameId() == 14066))
                    {
                        obj.TurnTowards(triggerer);
                        obj.Attack(triggerer);
                    }

                }

                DetachScript();
            }

            if ((attachee.GetMap() == 5066)) // change to ==
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14078 || obj.GetNameId() == 14079 || obj.GetNameId() == 14078 || obj.GetNameId() == 14631 || obj.GetNameId() == 14632 || obj.GetNameId() == 14633 || obj.GetNameId() == 14634 || obj.GetNameId() == 14067 || obj.GetNameId() == 14066 || obj.GetNameId() == 14635))
                    {
                        obj.TurnTowards(triggerer);
                        obj.Attack(triggerer);
                    }

                }

                DetachScript();
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetMap() == 5005) // Moathouse Dungeon
            {
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14066, 14067, 14078, 14079, 14080 }).Contains(npc.GetNameId()) && npc.GetLeader() == null && npc.GetScriptId(ObjScriptEvent.StartCombat) == 0)
                    {
                        npc.SetScriptId(ObjScriptEvent.StartCombat, 75);
                    }

                }

            }
            else if (attachee.GetMap() == 5066) // Temple level 1
            {
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (npc.GetLeader() == null && npc.GetScriptId(ObjScriptEvent.StartCombat) == 0)
                    {
                        npc.SetScriptId(ObjScriptEvent.StartCombat, 2);
                    }

                }

            }

            if ((GetGlobalVar(709) == 2 && attachee.GetMap() == 5005))
            {
                SetGlobalVar(709, 3);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14079 || obj.GetNameId() == 14080 || obj.GetNameId() == 14067 || obj.GetNameId() == 14078 || obj.GetNameId() == 14066))
                    {
                        obj.TurnTowards(triggerer);
                        obj.Attack(triggerer);
                    }

                }

            }

            if (attachee.GetMap() == 5066 && (!ScriptDaemon.get_f("j_ogre_temple_1"))) // temple level 1 - gnolls near southern stairs
            {
                var (xx, yy) = attachee.GetLocation();
                if ((Math.Pow((xx - 564), 2) + Math.Pow((yy - 599), 2)) < 200)
                {
                    ScriptDaemon.set_f("j_ogre_temple_1");
                    var xxp_min = 564;
                    var xxp_o = 564;
                    foreach (var pc in PartyLeader.GetPartyMembers())
                    {
                        var (xxp, yyp) = pc.GetLocation();
                        if (yyp >= 599)
                        {
                            if (xxp < xxp_min)
                            {
                                xxp_min = xxp;
                            }

                        }
                        else if (yyp < 599 && xxp >= 519 && xxp <= 546 && yyp >= 589)
                        {
                            if (xxp < xxp_o)
                            {
                                xxp_o = xxp;
                            }

                        }

                    }

                    var x_ogre = Math.Min(xxp_min, xxp_o) - 20;
                    x_ogre = Math.Max(507, x_ogre);
                    foreach (var npc in ObjList.ListVicinity(new locXY(507, 603), ObjectListFilter.OLC_NPC))
                    {
                        if (npc.GetNameId() == 14448 && npc.GetLeader() == null && !npc.IsUnconscious())
                        {
                            npc.Move(new locXY(x_ogre, 601), 0, 0);
                            npc.Attack(SelectedPartyLeader);
                        }

                    }

                }

            }

            // THIS IS USED FOR BREAK FREE
            var found_nearby = 0;
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if ((obj.DistanceTo(attachee) <= 3 && obj.GetStat(Stat.hp_current) >= -9))
                {
                    found_nearby = 1;
                }

            }

            if (found_nearby == 0)
            {
                while ((attachee.FindItemByName(8903) != null))
                {
                    attachee.FindItemByName(8903).Destroy();
                }

            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            // Spiritual Weapon Shenanigens	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(854) && attachee.GetMap() == 5066))
            {
                SetGlobalFlag(854, true);
                var newNPC = GameSystems.MapObject.CreateObject(14459, new locXY(551, 600));
                newNPC = GameSystems.MapObject.CreateObject(14458, new locXY(511, 540));
                newNPC = GameSystems.MapObject.CreateObject(14452, new locXY(455, 535));
            }

            if ((attachee.GetNameId() == 14078 && attachee.GetLeader() == null && attachee.GetMap() == 5005 && GetGlobalVar(760) == 1))
            {
                SetGlobalVar(760, 2);
                DetachScript();
                return RunDefault;
            }

            if ((attachee.GetNameId() == 14078 && attachee.GetLeader() == null && attachee.GetMap() == 5005))
            {
                if ((GetGlobalVar(760) == 2))
                {
                    attachee.SetStandpoint(StandPointType.Night, 272);
                    attachee.SetStandpoint(StandPointType.Day, 272);
                }

                if ((GetGlobalVar(760) == 13))
                {
                    attachee.SetStandpoint(StandPointType.Night, 271);
                    attachee.SetStandpoint(StandPointType.Day, 271);
                }

                SetGlobalVar(760, GetGlobalVar(760) + 1);
                if ((GetGlobalVar(760) >= 22))
                {
                    SetGlobalVar(760, 2);
                }

            }

            if ((attachee.GetNameId() == 14635 && attachee.GetLeader() == null && attachee.GetMap() == 5066))
            {
                // Make the barbarian gnolls patrol around Temple level 1 near Ogre Chief
                var rr = RandomRange(1, 30);
                if ((rr == 1))
                {
                    attachee.SetStandpoint(StandPointType.Night, 273);
                    attachee.SetStandpoint(StandPointType.Day, 273);
                }

                if ((rr == 2))
                {
                    attachee.SetStandpoint(StandPointType.Night, 274);
                    attachee.SetStandpoint(StandPointType.Day, 274);
                }

                if ((rr == 3))
                {
                    attachee.SetStandpoint(StandPointType.Night, 275);
                    attachee.SetStandpoint(StandPointType.Day, 275);
                }

                if ((rr == 4))
                {
                    attachee.SetStandpoint(StandPointType.Night, 276);
                    attachee.SetStandpoint(StandPointType.Day, 276);
                }

                if ((rr == 5))
                {
                    attachee.SetStandpoint(StandPointType.Night, 277);
                    attachee.SetStandpoint(StandPointType.Day, 277);
                }

                rr = 0;
            }

            if ((attachee.GetNameId() == 14636 && attachee.GetLeader() == null && attachee.GetMap() == 5066))
            {
                // Make the goblin patrol around Temple level 1 near Ogre Chief
                var rr = RandomRange(1, 30);
                if ((rr == 1))
                {
                    attachee.SetStandpoint(StandPointType.Night, 288);
                    attachee.SetStandpoint(StandPointType.Day, 288);
                }

                if ((rr == 2))
                {
                    attachee.SetStandpoint(StandPointType.Night, 289);
                    attachee.SetStandpoint(StandPointType.Day, 289);
                }

                if ((rr == 3))
                {
                    attachee.SetStandpoint(StandPointType.Night, 290);
                    attachee.SetStandpoint(StandPointType.Day, 290);
                }

                rr = 0;
            }

            if ((!GameSystems.Combat.IsCombatActive() && attachee.GetNameId() == 14066 && attachee.GetMap() == 5005))
            {
                if ((!attachee.HasMet(PartyLeader)))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        if ((!Utilities.critter_is_unconscious(PartyLeader)))
                        {
                            if ((PartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 100);
                            }
                            else
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                            }

                        }

                    }
                    else
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                if ((PartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002))))
                                {
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 100);
                                }
                                else
                                {
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 1);
                                }

                            }

                        }

                    }

                }

            }

            if ((GetGlobalFlag(288) && GetGlobalFlag(856) && GetGlobalFlag(857)))
            {
                DetachScript();
                if ((is_better_to_talk(attachee, PartyLeader)))
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 200);
                }
                else
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 200);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(709) >= 2 && attachee.GetMap() == 5005))
            {
                attachee.Attack(SelectedPartyLeader);
                DetachScript();
                return RunDefault;
            }

            if ((GetGlobalVar(709) <= 1 && attachee.GetMap() == 5005))
            {
                return SkipDefault;
            }

            if (attachee.GetMap() == 5091)
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody npc, GameObjectBody pc)
        {
            SetGlobalFlag(288, true);
            var location = new locXY(484, 490);
            npc.RunOff(location);
            Co8.Timed_Destroy(npc, 5000);
            // game.timevent_add( give_reward, (), 1209600000 )
            // game.timevent_add( give_reward, (), 720000 )
            QueueRandomEncounter(3605);
            foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 14079 || obj.GetNameId() == 14080 || obj.GetNameId() == 14067 || obj.GetNameId() == 14078))
                {
                    obj.RunOff(location);
                    Co8.Timed_Destroy(obj, 5000);
                }

            }

            return RunDefault;
        }
        public static bool give_reward()
        {
            QueueRandomEncounter(3579);
            return RunDefault;
        }
        public static bool give_item(GameObjectBody npc)
        {
            SetGlobalVar(769, 0);
            var item = GetGlobalVar(768);
            Utilities.create_item_in_inventory(item, npc);
            SetGlobalVar(768, 0);
            SetGlobalVar(776, GetGlobalVar(776) + 1);
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 20))
                {
                    return true;
                }

            }

            return false;
        }
        public static void call_leader(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation() - 2);
            leader.BeginDialog(npc, 1);
            return;
        }
        public static void call_leaderplease(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation() - 2);
            leader.BeginDialog(npc, 100);
            return;
        }
        public static void call_leadersvp(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation() - 2);
            leader.BeginDialog(npc, 200);
            return;
        }

    }
}
