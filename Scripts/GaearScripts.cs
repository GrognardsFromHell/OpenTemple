
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
    [ObjectScript(429)]
    public class GaearScripts : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalFlag(164)) && (attachee.GetMap() == 5105))) // turns on falrinth bugbear guards
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((GetGlobalVar(959) == 3) && (attachee.GetMap() == 5068))) // turns on bearded devils at Imeryds Run
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5121))) // turns off Verbobonc citizens at night
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5121))) // turns on Verbobonc citizens during the day
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5124))) // turns off Spruce Goose patrons during the day
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5124))) // turns on Spruce Goose patrons at night
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5125))) // turns off Spruce Goose housekeeping at night
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5125))) // turns on Spruce Goose housekeeping during the day
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5163))) // turns off Castle basement staff at night
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5163))) // turns on Castle basement staff during the day
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5164))) // turns off Castle first floor staff at night
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5164))) // turns on Castle first floor staff during the day
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((!Utilities.is_daytime()) && (attachee.GetMap() == 5165))) // turns off Castle second floor staff at night
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if (((Utilities.is_daytime()) && (attachee.GetMap() == 5165))) // turns on Castle second floor staff during the day
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((GetGlobalVar(696) == 6) && (attachee.GetMap() == 5002))) // turns on Moathouse exterior water snake
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }

    }
}
