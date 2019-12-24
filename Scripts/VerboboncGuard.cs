
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
    [ObjectScript(385)]
    public class VerboboncGuard : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5170))
            {
                triggerer.BeginDialog(attachee, 80);
            }
            else if ((Utilities.is_daytime()))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 20);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5156 && GetGlobalVar(704) == 3 && Utilities.is_daytime() && GetQuestState(76) != QuestState.Accepted))
            {
                // turns on warehouse Wilfrick escort
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
            {
                // turns on Welkwood Thaddeus escort
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5171 && GetGlobalVar(944) == 4 && GetGlobalFlag(861)))
            {
                // turns on Watch Post main floor replacements
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if (((PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42)) && (attachee.GetMap() == 5121)))
            {
                // turns on Verbobonc Exterior backup patrol
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if (((!PartyLeader.HasReputation(35)) && (attachee.GetMap() == 5121)))
            {
                // turns off Verbobonc Exterior backup patrol
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            if ((attachee.GetMap() == 5093))
            {
                ditch_rings(attachee, triggerer);
                if ((GetGlobalVar(956) == 0))
                {
                    SetGlobalVar(957, GetGlobalVar(957) + 1);
                }

            }
            else if ((attachee.GetMap() == 5121 || attachee.GetMap() == 5135 || attachee.GetMap() == 5169 || attachee.GetMap() == 5170 || attachee.GetMap() == 5171 || attachee.GetMap() == 5172))
            {
                SetGlobalVar(334, GetGlobalVar(334) + 1);
                if ((GetGlobalVar(334) >= 2))
                {
                    PartyLeader.AddReputation(35);
                }

                if ((GetQuestState(67) == QuestState.Accepted))
                {
                    SetGlobalFlag(964, true);
                }

                if ((GetGlobalFlag(942)))
                {
                    PartyLeader.AddReputation(35);
                }

                if ((attachee.GetNameId() == 8770))
                {
                    StartTimer(86400000, () => new_entry_guard(attachee, triggerer));
                }

                StartTimer(60000, () => go_away(attachee));
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = SelectedPartyLeader;
            if (((attachee.GetMap() == 5093) && (GetGlobalVar(956) == 1)))
            {
                attachee.SetInt(obj_f.critter_strategy, 21);
                return RunDefault;
            }
            else if ((attachee.GetMap() != 5093 && GetGlobalVar(969) == 1))
            {
                SetGlobalVar(969, 0);
            }
            else if (((GetQuestState(67) == QuestState.Accepted) && (!GetGlobalFlag(963))))
            {
                SetCounter(0, GetCounter(0) + 1);
                if ((GetCounter(0) >= 2))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.type == ObjectType.pc)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                    }

                    SetGlobalFlag(963, true);
                    leader.BeginDialog(attachee, 1);
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // get arrested for various reps
            if (((attachee.GetMap() != 5156) && (PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(45) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870)))))
            {
                if (((GetGlobalVar(969) == 0) && (!GetGlobalFlag(955))))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(2000, () => get_arrested(attachee, triggerer));
                            }

                        }

                    }

                }

            }
            // viscount guard talks about arrow to the knee
            else if ((attachee.GetNameId() == 8800))
            {
                if ((GetGlobalVar(829) == 0))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_peachy_to_talk(attachee, obj)))
                            {
                                StartTimer(2000, () => talk_arrow_knee(attachee, triggerer));
                                SetGlobalVar(829, 1);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(34)) || (PartyLeader.HasReputation(35)))
            {
                return RunDefault;
            }
            else if ((!GetGlobalFlag(992)) || (!GetGlobalFlag(975)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public static bool guard_backup(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var guard_1 = GameSystems.MapObject.CreateObject(14716, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_1.TurnTowards(PartyLeader);
            var guard_2 = GameSystems.MapObject.CreateObject(14716, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_2.TurnTowards(PartyLeader);
            var guard_3 = GameSystems.MapObject.CreateObject(14716, attachee.GetLocation().OffsetTiles(-4, 0));
            guard_3.TurnTowards(PartyLeader);
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 50))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool is_peachy_to_talk(GameObjectBody speaker, GameObjectBody listener)
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
        public static bool is_distant_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 100))
                {
                    return true;
                }

            }

            return false;
        }
        public static int execution(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cleric = Utilities.find_npc_near(attachee, 14471);
            AttachParticles("cast-Evocation-cast", cleric);
            Sound(4049, 1);
            var pc1 = PartyLeader;
            pc1.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc1);
            var pc2 = GameSystems.Party.GetPartyGroupMemberN(1);
            pc2.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc2);
            var pc3 = GameSystems.Party.GetPartyGroupMemberN(2);
            pc3.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc3);
            var pc4 = GameSystems.Party.GetPartyGroupMemberN(3);
            pc4.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc4);
            var pc5 = GameSystems.Party.GetPartyGroupMemberN(4);
            pc5.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc5);
            var pc6 = GameSystems.Party.GetPartyGroupMemberN(5);
            pc6.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc6);
            var pc7 = GameSystems.Party.GetPartyGroupMemberN(6);
            pc7.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc7);
            var pc8 = GameSystems.Party.GetPartyGroupMemberN(7);
            pc8.KillWithDeathEffect();
            AttachParticles("sp-Flame Strike", pc8);
            return 1;
        }
        public static bool go_away(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static void ditch_rings(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var acid_minor = attachee.FindItemByName(12630);
            var cold_minor = attachee.FindItemByName(12629);
            var electricity_minor = attachee.FindItemByName(12627);
            var fire_minor = attachee.FindItemByName(6101);
            var sonic_minor = attachee.FindItemByName(12628);
            acid_minor.Destroy();
            cold_minor.Destroy();
            electricity_minor.Destroy();
            fire_minor.Destroy();
            sonic_minor.Destroy();
            return;
        }
        public static bool get_arrested(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 30);
            return RunDefault;
        }
        public static bool talk_arrow_knee(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 140);
            return RunDefault;
        }
        public static bool new_entry_guard(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var entry_guard = GameSystems.MapObject.CreateObject(14817, new locXY(236, 490));
            entry_guard.Rotation = 3f;
            return RunDefault;
        }

    }
}
