
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
    [ObjectScript(118)]
    public class Skole : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if (((GetGlobalFlag(202)) && (GetQuestState(42) != QuestState.Completed)))
            {
                triggerer.BeginDialog(attachee, 360);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(201)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(202, false);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(202, false);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(202, true);
            return RunDefault;
        }
        public static bool prepare_goons(GameObject attachee)
        {
            StartTimer(259200000, () => goons_attack(attachee));
            return RunDefault;
        }
        public static bool goons_attack(GameObject attachee)
        {
            if (GetQuestState(42) != QuestState.Completed)
            {
                SetQuestState(42, QuestState.Botched);
                SetGlobalFlag(202, true);
                QueueRandomEncounter(3004);
            }

            return RunDefault;
        }


    }
}
