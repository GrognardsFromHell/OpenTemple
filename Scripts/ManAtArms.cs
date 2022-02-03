
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
    [ObjectScript(65)]
    public class ManAtArms : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
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
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static bool equip_transfer(GameObject attachee, GameObject triggerer)
        {
            var itemA = attachee.FindItemByName(4036);
            if ((itemA != null))
            {
                itemA.Destroy();
                Utilities.create_item_in_inventory(4036, triggerer);
            }

            var itemB = attachee.FindItemByName(4087);
            if ((itemB != null))
            {
                itemB.Destroy();
                Utilities.create_item_in_inventory(4087, triggerer);
            }

            var itemC = attachee.FindItemByName(6010);
            if ((itemC != null))
            {
                itemC.Destroy();
                Utilities.create_item_in_inventory(6010, triggerer);
            }

            var itemD = attachee.FindItemByName(6011);
            if ((itemD != null))
            {
                itemD.Destroy();
                Utilities.create_item_in_inventory(6011, triggerer);
            }

            var itemE = attachee.FindItemByName(6012);
            if ((itemE != null))
            {
                itemE.Destroy();
                Utilities.create_item_in_inventory(6012, triggerer);
            }

            var itemF = attachee.FindItemByName(6013);
            if ((itemF != null))
            {
                itemF.Destroy();
                Utilities.create_item_in_inventory(6013, triggerer);
            }

            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            return RunDefault;
        }

    }
}
