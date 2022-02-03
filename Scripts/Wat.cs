
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
    [ObjectScript(113)]
    public class Wat : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((Utilities.find_npc_near(attachee, 14773) == null))
            {
                triggerer.BeginDialog(attachee, 270);
            }
            else
            {
                triggerer.BeginDialog(attachee, 70);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetQuestState(39, QuestState.Botched);
            return RunDefault;
        }
        public static bool kill_dick(GameObject attachee)
        {
            StartTimer(14400000, () => kill_dick_callback(attachee)); // call kill_dick_callback in 4 hours
            return SkipDefault;
        }
        public static bool kill_dick_callback(GameObject attachee)
        {
            var npc = Utilities.find_npc_near(attachee, 8018);
            if ((npc != null))
            {
                npc.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(88, true);
            }

            return SkipDefault;
        }

    }
}
