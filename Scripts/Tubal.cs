
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
    [ObjectScript(140)]
    public class Tubal : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(310)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(116, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(116, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(116, false);
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
