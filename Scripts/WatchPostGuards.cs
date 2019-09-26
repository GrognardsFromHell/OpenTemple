
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
    [ObjectScript(397)]
    public class WatchPostGuards : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() != 5171) && (PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(45) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870)))))
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
                                obj.BeginDialog(attachee, 30);
                            }

                        }

                    }

                }

            }
            else if (((attachee.GetMap() == 5149) && (GetGlobalVar(944) == 1 || GetGlobalVar(944) == 2) && (!GetGlobalFlag(861))))
            {
                attachee.RunOff();
                SetGlobalFlag(861, true);
            }

            // game.new_sid = 0
            return RunDefault;
        }

        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            return speaker.DistanceTo(listener) <= 40;
        }
    }
}
