
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(255)]
public class Tutorialend : BaseObjectScript
{

    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
        {
            if ((Utilities.critter_is_unconscious(obj) == 0))
            {
                if (attachee.DistanceTo(obj) < 10)
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (((pc.GetNameId() == 8066) && (pc.GetStat(Stat.hp_current) >= 0)))
                        {
                            GameSystems.Movies.MovieQueueAdd(1028);
                            GameSystems.Movies.MovieQueuePlayAndEndGame();
                            return RunDefault;
                        }

                    }

                    GameSystems.Movies.MovieQueueAdd(1029);
                    GameSystems.Movies.MovieQueuePlayAndEndGame();
                    return RunDefault;
                }

            }

        }

        return RunDefault;
    }


}