
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
    [ObjectScript(140)]
    public class Tubal : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(140)))
            {
                if ((GetGlobalVar(15) <= 4))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 10);
                }

            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if ((GetQuestState(52) >= QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 40);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(116, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(116, false);
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(310)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(116, true);
            }

            return RunDefault;
        }
        public static bool kill_antonio(GameObjectBody attachee)
        {
            StartTimer(86400000, () => antonio_dead(attachee));
            return RunDefault;
        }
        public static bool antonio_dead(GameObjectBody attachee)
        {
            SetGlobalFlag(311, true);
            return RunDefault;
        }
        public static bool kill_alrrem(GameObjectBody attachee)
        {
            StartTimer(86400000, () => alrrem_dead(attachee));
            return RunDefault;
        }
        public static bool alrrem_dead(GameObjectBody attachee)
        {
            SetGlobalFlag(312, true);
            return RunDefault;
        }


    }
}
