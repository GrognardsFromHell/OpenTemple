
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
    [ObjectScript(254)]
    public class Fireballhallway : BaseObjectScript
    {

        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.critter_is_unconscious(obj) == 0))
                {
                    if (attachee.DistanceTo(obj) < 10)
                    {
                        if (!GetGlobalFlag(11))
                        {
                            if (!UiSystems.HelpManager.IsTutorialActive)
                            {
                                UiSystems.HelpManager.ToggleTutorial();
                            }

                            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.WandUse);
                            SetGlobalFlag(9, true);
                            DetachScript();

                            return RunDefault;
                        }

                    }

                }

            }

            return RunDefault;
        }


    }
}
