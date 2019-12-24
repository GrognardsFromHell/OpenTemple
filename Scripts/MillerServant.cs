
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

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
