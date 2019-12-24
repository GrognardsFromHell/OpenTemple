
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
    [ObjectScript(351)]
    public class CaptainAbsalom : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5170 || attachee.GetMap() == 5135))
            {
                if ((GetGlobalVar(947) == 1))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(947) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5171))
            {
                if ((GetGlobalVar(947) == 2) && !ScriptDaemon.tpsts("absalom_off_to_arrest", 1 * 60 * 60))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(947) == 3) || ScriptDaemon.tpsts("absalom_off_to_arrest", 1 * 60 * 60))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            if (attachee.GetMap() == 5121 || attachee.GetMap() == 5135 || attachee.GetMap() == 5169 || attachee.GetMap() == 5170 || attachee.GetMap() == 5171 || attachee.GetMap() == 5172)
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
                    StartTimer(86400000, () => VerboboncGuard.new_entry_guard(attachee, triggerer));
                }

                StartTimer(60000, () => VerboboncGuard.go_away(attachee));
            }

            SetGlobalFlag(419, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(419, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5170 || attachee.GetMap() == 5135) && GetGlobalVar(947) == 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_groovier_to_talk(attachee, obj)))
                        {
                            StartTimer(2000, () => start_talking(attachee, triggerer));
                            SetGlobalVar(947, 2);
                        }

                    }

                }

            }
            else if ((PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46)))
            {
                if (((GetGlobalVar(969) == 0) && (!GetGlobalFlag(955))))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 180);
                                SetGlobalVar(969, 1);
                            }

                        }

                    }

                }

            }
            else if ((GetGlobalVar(993) == 5 && GetGlobalFlag(870)))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            obj.BeginDialog(attachee, 200);
                            SetGlobalVar(993, 7);
                        }

                    }

                }

            }
            else if ((GetGlobalVar(993) == 5 && !GetGlobalFlag(870)))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            obj.BeginDialog(attachee, 210);
                            SetGlobalVar(993, 8);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 35))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool is_groovier_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 40))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool start_talking(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8703);
            attachee.TurnTowards(npc);
            PartyLeader.BeginDialog(attachee, 320);
            return RunDefault;
        }
        public static bool switch_to_wilfrick(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8703);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }

    }
}
