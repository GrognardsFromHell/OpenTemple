
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

[ObjectScript(397)]
public class WatchPostGuards : BaseObjectScript
{
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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

    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        return speaker.DistanceTo(listener) <= 40;
    }
}