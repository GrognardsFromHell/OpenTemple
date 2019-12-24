
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
    [ObjectScript(110)]
    public class Dick : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if ((GetGlobalFlag(91)))
            {
                triggerer.BeginDialog(attachee, 230);
            }
            else
            {
                triggerer.BeginDialog(attachee, 200);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(39, QuestState.Botched);
            SetGlobalFlag(88, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(88, false);
            return RunDefault;
        }
        public static bool set_hostel_flag(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(289, true);
            StartTimer(86400000, () => hostel_room_no_longer_available());
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool hostel_room_no_longer_available()
        {
            SetGlobalFlag(289, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }


    }
}
