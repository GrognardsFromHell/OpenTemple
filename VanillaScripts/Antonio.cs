
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(139)]
    public class Antonio : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(139)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else if ((GetQuestState(52) == QuestState.Unknown))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                triggerer.BeginDialog(attachee, 40);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(115, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(115, false);
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(311)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(115, true);
            }

            return RunDefault;
        }
        public static bool kill_tubal(GameObjectBody attachee)
        {
            StartTimer(172800000, () => tubal_dead(attachee));
            return RunDefault;
        }
        public static bool tubal_dead(GameObjectBody attachee)
        {
            SetGlobalFlag(310, true);
            return RunDefault;
        }


    }
}
