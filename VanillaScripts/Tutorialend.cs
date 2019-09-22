
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(255)]
    public class Tutorialend : BaseObjectScript
    {

        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
}
