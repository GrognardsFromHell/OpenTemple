
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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
    [ObjectScript(3)]
    public class AverageFarmer : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else if ((attachee.GetMap() == 5001) && (GetGlobalVar(4) <= 4))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else if ((attachee.GetMap() == 5001) && (GetGlobalVar(4) == 6))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5031))
            {
                if ((GetQuestState(9) == QuestState.Accepted))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }
                    else
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((GetGlobalVar(4) == 3))
                {
                    SetGlobalVar(4, 4);
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalFlag(99, true);
                    SetGlobalVar(24, GetGlobalVar(24) + 1);
                    if ((!PartyLeader.HasReputation(5)))
                    {
                        PartyLeader.AddReputation(5);
                    }

                    if (((GetGlobalVar(24) >= 3) && (!PartyLeader.HasReputation(6))))
                    {
                        PartyLeader.AddReputation(6);
                    }

                }
                else if ((GetGlobalVar(4) == 5))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5001))
            {
                if ((GetQuestState(9) == QuestState.Accepted))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }
                    else
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((GetQuestState(9) >= QuestState.Accepted))
            {
                SetGlobalVar(4, 1);
            }

            if ((!PartyLeader.HasReputation(9)))
            {
                PartyLeader.AddReputation(9);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }

    }
}
