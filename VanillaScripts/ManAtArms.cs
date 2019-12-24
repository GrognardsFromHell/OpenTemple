
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
