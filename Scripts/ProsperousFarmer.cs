
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
    [ObjectScript(16)]
    public class ProsperousFarmer : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(196)))
            {
                triggerer.BeginDialog(attachee, 320);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                if ((GetQuestState(6) == QuestState.Completed))
                {
                    triggerer.BeginDialog(attachee, 20);
                }
                else if ((GetQuestState(6) <= QuestState.Accepted))
                {
                    if ((!GetGlobalFlag(10)))
                    {
                        triggerer.BeginDialog(attachee, 120);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 150);
                    }

                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetQuestState(6, QuestState.Botched);
            SetGlobalFlag(333, true);
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(333, false);
            return RunDefault;
        }

    }
}
