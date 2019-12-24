
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
    [ObjectScript(133)]
    public class BrigandPrisoner : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if (((!attachee.HasMet(triggerer)) || (!GetGlobalFlag(130))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 130);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(343)))
            {
                SetGlobalFlag(343, false);
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(440, 416);

            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(16))
            {
                triggerer.AddReputation(16);
            }

            SetGlobalVar(26, GetGlobalVar(26) + 1);
            if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
            {
                triggerer.AddReputation(17);
            }

            return RunDefault;
        }
        public static bool move_wicked(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetStandpoint(StandPointType.Night, 258);
            attachee.SetStandpoint(StandPointType.Day, 258);
            SetGlobalFlag(343, true);
            attachee.RunOff();
            return RunDefault;
        }


    }
}
