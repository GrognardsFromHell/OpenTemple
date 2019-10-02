
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