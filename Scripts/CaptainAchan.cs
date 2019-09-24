
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
    [ObjectScript(352)]
    public class CaptainAchan : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetGlobalVar(962) == 1))
            {
                triggerer.BeginDialog(attachee, 250);
            }
            else if ((attachee.HasMet(triggerer)))
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
                if ((GetGlobalVar(946) == 1))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(946) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5172))
            {
                if ((GetGlobalVar(946) == 2) && !ScriptDaemon.tpsts("achan_off_to_arrest", 1 * 60 * 60))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalVar(946) == 3) || ScriptDaemon.tpsts("achan_off_to_arrest", 1 * 60 * 60))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5170 || attachee.GetMap() == 5135) && GetGlobalVar(946) == 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_groovier_to_talk(attachee, obj)))
                        {
                            StartTimer(2000, () => start_talking(attachee, triggerer));
                            DetachScript();
                        }

                    }

                }

            }
            else
            {
                if ((PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870))))
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
                                    obj.BeginDialog(attachee, 230);
                                    SetGlobalVar(969, 1);
                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 50))
            {
                return true;
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
            PartyLeader.BeginDialog(attachee, 460);
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
