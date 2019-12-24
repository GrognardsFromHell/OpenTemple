
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
    [ObjectScript(113)]
    public class Wat : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 70);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(39, QuestState.Botched);
            return RunDefault;
        }
        public static bool kill_dick(GameObjectBody attachee)
        {
            StartTimer(14400000, () => kill_dick_callback(attachee));
            return SkipDefault;
        }
        public static bool kill_dick_callback(GameObjectBody attachee)
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
