
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
    [ObjectScript(70)]
    public class Furnok : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(61)))
            {
                triggerer.BeginDialog(attachee, 500);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 300);
            }
            else if ((GetGlobalFlag(51)))
            {
                if ((GetQuestState(18) == QuestState.Completed))
                {
                    triggerer.BeginDialog(attachee, 220);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 210);
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
            SetGlobalFlag(58, true);
            SetGlobalVar(29, GetGlobalVar(29) + 1);
            if ((attachee.GetLeader() == null))
            {
                SetGlobalVar(23, GetGlobalVar(23) + 1);
                if ((GetGlobalVar(23) >= 2))
                {
                    PartyLeader.AddReputation(1);
                }

            }
            else
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(58, false);
            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetArea() == 2) || (attachee.GetArea() == 4)))
            {
                SetGlobalFlag(60, true);
            }
            else if (((attachee.GetArea() == 1) && (GetGlobalFlag(60))))
            {
                SetGlobalFlag(60, false);
                if ((attachee.GetMoney() >= 200000))
                {
                    var leader = attachee.GetLeader();

                    if ((leader != null))
                    {
                        leader.BeginDialog(attachee, 400);
                    }

                }

            }

            return RunDefault;
        }


    }
}
