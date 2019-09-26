
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
    [ObjectScript(394)]
    public class VerboboncCitizenNarc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5121))
            {
                if ((GetGlobalVar(944) == 1))
                {
                    StartTimer(3600000, () => one_hour_delay_to_pick_up_darlia_for_robbery()); // 1 hour
                    SetGlobalVar(944, 4);
                }

                if ((GetGlobalVar(944) == 2))
                {
                    StartTimer(3600000, () => one_hour_delay_to_pick_up_darlia_for_framework()); // 1 hour
                    SetGlobalVar(944, 4);
                }

                if ((GetGlobalVar(944) == 3))
                {
                    if (ScriptDaemon.tpsts("achan_off_to_arrest", 1 * 60 * 60))
                    {
                        SetGlobalVar(946, 3);
                    }

                    if (ScriptDaemon.tpsts("absalom_off_to_arrest", 1 * 60 * 60))
                    {
                        SetGlobalVar(947, 3);
                    }

                    if (ScriptDaemon.tpsts("abiram_off_to_arrest", 1 * 60 * 60))
                    {
                        SetGlobalVar(948, 3);
                    }

                    // setting the vars may be considered redundant with use of the time stamps, but it never hurts to bulletproof things in ToEE ;-) -SA
                    StartTimer(3600000, () => one_hour_delay_to_pick_up_darlia_for_wilfrick()); // 1 hour
                    SetGlobalVar(944, 4);
                }

            }

            return RunDefault;
        }
        public static bool one_hour_delay_to_pick_up_darlia_for_robbery()
        {
            SetGlobalVar(945, 1);
            return RunDefault;
        }
        public static bool one_hour_delay_to_pick_up_darlia_for_framework()
        {
            SetGlobalVar(945, 2);
            return RunDefault;
        }
        public static bool one_hour_delay_to_pick_up_darlia_for_wilfrick()
        {
            SetGlobalVar(945, 3);
            if ((GetGlobalVar(946) == 2)) // Achan
            {
                SetGlobalVar(946, 3);
            }

            if ((GetGlobalVar(947) == 2)) // Absalom
            {
                SetGlobalVar(947, 3);
            }

            if ((GetGlobalVar(948) == 2)) // Abiram
            {
                SetGlobalVar(948, 3);
            }

            return RunDefault;
        }

    }
}
