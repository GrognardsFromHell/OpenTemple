
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

namespace VanillaScripts
{
    [ObjectScript(90)]
    public class BrauApprentice3 : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(86)))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((GetQuestState(60) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 210);
            }
            else if ((attachee.GetArea() == 1))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 60);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(86, true);
            if (!PartyLeader.HasReputation(9))
            {
                PartyLeader.AddReputation(9);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(86, false);
            return RunDefault;
        }
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            attachee.SetStandpoint(StandPointType.Night, 255);
            var loc = new locXY(506, 493);

            attachee.RunOff(loc);
            return RunDefault;
        }


    }
}
