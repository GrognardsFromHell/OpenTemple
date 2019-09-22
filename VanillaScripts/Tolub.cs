
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(116)]
    public class Tolub : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5052))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(97, true);
            if ((GetQuestState(36) == QuestState.Accepted))
            {
                SetQuestState(36, QuestState.Completed);
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
                        SetGlobalVar(31, 0);
                        var n = RandomRange(190, 194);

                        attachee.FloatLine(n, triggerer);
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
        public static bool brawl_end(GameObjectBody attachee, GameObjectBody triggerer, int brawl_state)
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
                attachee.FloatLine(290, triggerer);
            }

            return RunDefault;
        }


    }
}
