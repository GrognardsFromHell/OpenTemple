
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
    [ObjectScript(487)]
    public class Fireforge : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.GetMap() == 5008))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5008))
            {
                if ((GetQuestState(98) != QuestState.Completed))
                {
                    if ((GetGlobalFlag(505)))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((GetQuestState(98) == QuestState.Completed))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5121))
            {
                if ((GetGlobalVar(507) == 1))
                {
                    if ((Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                        if ((!GetGlobalFlag(924)))
                        {
                            StartTimer(86400000, () => respawn_fireforge(attachee)); // 86400000ms is 24 hours
                            SetGlobalFlag(924, true);
                        }

                    }
                    else if ((!Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5163))
            {
                if ((GetGlobalVar(507) == 2))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((!GetGlobalFlag(924)))
                    {
                        StartTimer(86400000, () => respawn_fireforge(attachee)); // 86400000ms is 24 hours
                        SetGlobalFlag(924, true);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static void respawn_fireforge(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1005);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn_fireforge(attachee)); // 86400000ms is 24 hours
            return;
        }

    }
}
