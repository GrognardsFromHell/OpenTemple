
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
    [ObjectScript(200)]
    public class Zaxis : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.GetLeader() == null && !GetGlobalFlag(877) && !attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((attachee.GetLeader() == null && GetGlobalFlag(877) && !attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((attachee.GetLeader() == null && attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 280);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5057))
            {
                if ((GetGlobalFlag(877) && !GetGlobalFlag(880)))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            return RunDefault;
        }
        public override bool OnNewMap(GameObject attachee, GameObject triggerer)
        {
            var randy1 = RandomRange(1, 12);
            if (((attachee.GetMap() == 5066) && randy1 >= 9))
            {
                attachee.FloatLine(12095, triggerer);
            }
            else if (((attachee.GetMap() == 5058) && randy1 >= 11))
            {
                attachee.FloatLine(12054, triggerer);
            }
            else if (((attachee.GetMap() == 5059) && randy1 >= 5))
            {
                attachee.FloatLine(12092, triggerer);
            }
            else if (((attachee.GetMap() == 5057) && randy1 >= 9))
            {
                attachee.FloatLine(12100, triggerer);
            }

            return RunDefault;
        }
        public static bool zaxis_runs_off(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(877, true);
            SetGlobalFlag(879, true);
            attachee.RunOff();
            return RunDefault;
        }
        public static bool zaxis_runs_off2(GameObject attachee, GameObject triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static bool equip_transfer(GameObject attachee, GameObject triggerer)
        {
            var itemA = attachee.FindItemByName(6011);
            if ((itemA != null))
            {
                itemA.Destroy();
                Utilities.create_item_in_inventory(6011, triggerer);
            }

            var itemB = attachee.FindItemByName(6012);
            if ((itemB != null))
            {
                itemB.Destroy();
                Utilities.create_item_in_inventory(6012, triggerer);
            }

            var itemC = attachee.FindItemByName(6045);
            if ((itemC != null))
            {
                itemC.Destroy();
                Utilities.create_item_in_inventory(6045, triggerer);
            }

            var itemD = attachee.FindItemByName(6091);
            if ((itemD != null))
            {
                itemD.Destroy();
                Utilities.create_item_in_inventory(6091, triggerer);
            }

            var itemE = attachee.FindItemByName(4009);
            if ((itemE != null))
            {
                itemE.Destroy();
                Utilities.create_item_in_inventory(4009, triggerer);
            }

            var itemF = attachee.FindItemByName(12562);
            if ((itemF != null))
            {
                itemF.Destroy();
                Utilities.create_item_in_inventory(12562, triggerer);
            }

            var itemG = attachee.FindItemByName(12561);
            if ((itemG != null))
            {
                itemG.Destroy();
                Utilities.create_item_in_inventory(12561, triggerer);
            }

            var itemH = attachee.FindItemByName(12563);
            if ((itemH != null))
            {
                itemH.Destroy();
                Utilities.create_item_in_inventory(12563, triggerer);
            }

            var itemI = attachee.FindItemByName(12564);
            if ((itemI != null))
            {
                itemI.Destroy();
                Utilities.create_item_in_inventory(12564, triggerer);
            }

            var itemJ = attachee.FindItemByName(12584);
            if ((itemJ != null))
            {
                itemJ.Destroy();
                Utilities.create_item_in_inventory(12584, triggerer);
            }

            var itemK = attachee.FindItemByName(12585);
            if ((itemK != null))
            {
                itemK.Destroy();
                Utilities.create_item_in_inventory(12585, triggerer);
            }

            var itemL = attachee.FindItemByName(12586);
            if ((itemL != null))
            {
                itemL.Destroy();
                Utilities.create_item_in_inventory(12586, triggerer);
            }

            var itemM = attachee.FindItemByName(12587);
            if ((itemM != null))
            {
                itemM.Destroy();
                Utilities.create_item_in_inventory(12587, triggerer);
            }

            Utilities.create_item_in_inventory(7003, attachee);
            Utilities.create_item_in_inventory(7003, attachee);
            Utilities.create_item_in_inventory(7003, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7002, attachee);
            return RunDefault;
        }

    }
}
