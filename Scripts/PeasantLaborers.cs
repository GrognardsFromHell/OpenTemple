
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
    [ObjectScript(87)]
    public class PeasantLaborers : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetQuestState(96) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetNameId() != 8073 && attachee.GetNameId() != 8074 && attachee.GetNameId() != 14019))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else if ((attachee.GetNameId() == 8073))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }
                    else if ((Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((attachee.GetNameId() == 8074))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }
                    else if ((Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((attachee.GetNameId() == 14019)) // otello of the field
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

            }

            return RunDefault;
        }

    }
}
