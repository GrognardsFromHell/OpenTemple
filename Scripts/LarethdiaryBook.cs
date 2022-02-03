
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

[ObjectScript(222)]
public class LarethdiaryBook : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
    {
        var loc = triggerer.GetLocation();
        var npc = GameSystems.MapObject.CreateObject(14413, loc);
        var obj = Utilities.find_npc_near(triggerer, 8002);
        if ((obj != null))
        {
            if ((obj.GetLeader() != null && obj.DistanceTo(triggerer) <= 15))
            {
                var where = triggerer.GetLocation().OffsetTiles(-1, 0);
                obj.Move(where);
                obj.TurnTowards(triggerer);
                triggerer.TurnTowards(obj);
                if ((GetGlobalFlag(198)))
                {
                    triggerer.BeginDialog(obj, 1000);
                }
                else if ((obj.GetLeader() != null))
                {
                    if ((GetGlobalFlag(53)))
                    {
                        triggerer.BeginDialog(obj, 1100);
                    }
                    else
                    {
                        triggerer.BeginDialog(obj, 1200);
                    }

                }
                else if ((GetGlobalFlag(52)))
                {
                    triggerer.BeginDialog(obj, 1300);
                }
                else if ((GetGlobalFlag(48)))
                {
                    triggerer.BeginDialog(obj, 1400);
                }
                else
                {
                    triggerer.BeginDialog(obj, 1500);
                }

            }
            else
            {
                // if (game.global_vars[701] == 2):
                // triggerer.begin_dialog( npc,1 )
                // else:
                triggerer.BeginDialog(npc, 1);
            }

        }
        else
        {
            // elif (game.global_vars[701] == 2):
            // triggerer.begin_dialog( npc,1 )
            triggerer.BeginDialog(npc, 1);
        }

        return SkipDefault;
    }

}