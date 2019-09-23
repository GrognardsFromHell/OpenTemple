
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
    [ObjectScript(116)]
    public class Tolub : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5052))
            {
                if ((GetGlobalVar(10) == 4))
                {
                    triggerer.BeginDialog(attachee, 500);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 200);
                }

            }
            else
            {
                if ((GetGlobalVar(10) == 4))
                {
                    triggerer.BeginDialog(attachee, 500);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(97, true);
            if ((GetQuestState(36) == QuestState.Accepted))
            {
                GetQuestState(36) == QuestState.Completed;
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(40) != QuestState.Accepted))
            {
                Utilities.create_item_in_inventory(4189, attachee);
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(97, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(10) >= 2))
            {
                DetachScript();
            }
            else if ((attachee.GetMap() == 5052))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(31, GetGlobalVar(31) + 1);
                    if ((GetGlobalVar(31) > 25))
                    {
                        if ((attachee != null && Utilities.critter_is_unconscious(attachee) != 1 && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
                        {
                            SetGlobalVar(31, 0);
                            var n = RandomRange(190, 194);
                            attachee.FloatLine(n, triggerer);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool brawl(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetReaction(triggerer, -100);
            GameSystems.Combat.Brawl(triggerer, attachee);
            return RunDefault;
        }
        public static bool brawl_end(GameObjectBody attachee, GameObjectBody triggerer, FIXME brawl_state)
        {
            if ((brawl_state == 0))
            {
                attachee.SetReaction(triggerer, 50);
                triggerer.BeginDialog(attachee, 300);
            }
            else if ((brawl_state == 1))
            {
                attachee.SetReaction(triggerer, 50);
                triggerer.BeginDialog(attachee, 330);
            }
            else
            {
                Logger.Info("{0}", "Brawl State:" + brawl_state.ToString());
                attachee.SetReaction(triggerer, 50);
                Co8.StopCombat(attachee, 1);
                triggerer.BeginDialog(attachee, 400);
            }

            return RunDefault;
        }

    }
}
