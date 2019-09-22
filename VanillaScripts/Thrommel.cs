
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
    [ObjectScript(149)]
    public class Thrommel : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(150, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(150, false);
            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5093) || (attachee.GetMap() == 5113) || (attachee.GetMap() == 5001)))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 130);
                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(151, true);
            attachee.RunOff();
            return RunDefault;
        }
        public static bool check_follower_thrommel_comments(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8014);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 490);
            }
            else
            {
                npc = Utilities.find_npc_near(attachee, 8000);

                if ((npc != null))
                {
                    triggerer.BeginDialog(npc, 550);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 10);
                }

            }

            return RunDefault;
        }
        public static bool schedule_reward(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(152, true);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            StartTimer(1209600000, () => give_reward());
            return RunDefault;
        }
        public static bool give_reward()
        {
            QueueRandomEncounter(3001);
            return RunDefault;
        }


    }
}
