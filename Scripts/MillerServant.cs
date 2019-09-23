
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(13)]
    public class MillerServant : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool beggar_soon(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.AddCondition("Fallen_Paladin", 0, 0);
            StartTimer(86400000, () => beggar_now(attachee, triggerer));
            return RunDefault;
        }
        public static bool beggar_now(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(199, true);
            SetGlobalVar(24, GetGlobalVar(24) + 1);
            if ((!triggerer.HasReputation(5)))
            {
                triggerer.AddReputation(5);
            }

            if (((GetGlobalVar(24) >= 3) && (!triggerer.HasReputation(6))))
            {
                triggerer.AddReputation(6);
            }

            return RunDefault;
        }
        public static bool complete_quest(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(10, QuestState.Completed);
            attachee.AdjustReaction(triggerer, +30);
            StartTimer(90000000, () => visited_church(attachee, triggerer));
            return RunDefault;
        }
        public static bool visited_church(GameObjectBody attachee, GameObjectBody triggerer)
        {
            GetGlobalFlag(302);
            return RunDefault;
        }

    }
}
