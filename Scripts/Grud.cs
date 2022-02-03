
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

namespace Scripts
{
    [ObjectScript(102)]
    public class Grud : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetQuestState(35) == QuestState.Mentioned))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else if ((GetQuestState(35) == QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if ((GetQuestState(35) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 170);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetQuestState(35, QuestState.Botched);
            SetGlobalFlag(329, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(329, false);
            return RunDefault;
        }

    }
}
