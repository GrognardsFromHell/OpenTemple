
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
    [ObjectScript(117)]
    public class Lodriss : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(369, true);
            if (GetQuestState(42) >= QuestState.Mentioned)
            {
                SetQuestState(42, QuestState.Completed);
                triggerer.AddReputation(21);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetQuestState(42) == QuestState.Completed)
            {
                SetQuestState(42, QuestState.Botched);
                triggerer.RemoveReputation(21);
            }

            return RunDefault;
        }
        public static bool kill_lodriss(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool kill_skole(GameObjectBody attachee)
        {
            StartTimer(86400000, () => skole_dead(attachee));
            return RunDefault;
        }
        public static bool skole_dead(GameObjectBody attachee)
        {
            SetGlobalFlag(201, true);
            return RunDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 1);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }
        public static bool make_like(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetReaction(triggerer) <= 71))
            {
                attachee.SetReaction(triggerer, 71);
            }

            return SkipDefault;
        }


    }
}
