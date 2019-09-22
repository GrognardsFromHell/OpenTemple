
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
    [ObjectScript(65)]
    public class ManAtArms : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.RemoveFollower(attachee);
                }

                if ((GetGlobalFlag(47)))
                {
                    if ((leader != null))
                    {
                        if ((Utilities.obj_percent_hp(attachee) > 70))
                        {
                            if ((Utilities.group_percent_hp(leader) < 30))
                            {
                                attachee.FloatLine(110, triggerer);
                                attachee.Attack(leader);
                            }

                        }

                    }

                }

                if ((Utilities.obj_percent_hp(attachee) < 30))
                {
                    leader = attachee.GetLeader();

                    if ((leader != null))
                    {
                        attachee.FloatLine(120, leader);
                        leader.RemoveFollower(attachee);
                        attachee.RunOff();
                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }


    }
}
