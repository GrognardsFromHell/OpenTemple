
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(116)]
    public class Tolub : BaseObjectScript
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(97, true);
            if ((GetQuestState(36) == QuestState.Accepted))
            {
                SetQuestState(36, QuestState.Completed);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetQuestState(40) != QuestState.Accepted))
            {
                Utilities.create_item_in_inventory(4189, attachee);
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(97, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
                        if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
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
        public static bool brawl(GameObject attachee, GameObject triggerer)
        {
            attachee.SetReaction(triggerer, -100);
            GameSystems.Combat.Brawl(triggerer, attachee);
            return RunDefault;
        }
        public static bool brawl_end(GameObject attachee, GameObject triggerer, int brawl_state)
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
