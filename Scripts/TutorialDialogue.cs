
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
    [ObjectScript(246)]
    public class TutorialDialogue : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((!Utilities.critter_is_unconscious(obj)))
                {
                    if (!GetGlobalFlag(0))
                    {
                        if (!UiSystems.HelpManager.IsTutorialActive)
                        {
                            UiSystems.HelpManager.ToggleTutorial();
                        }

                        UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.Dialogue);
                        SetGlobalFlag(0, true);
                        SetGlobalFlag(10, true);
                    }
                    else if (GetGlobalFlag(10))
                    {
                        // if not game.tutorial_is_active():
                        obj.BeginDialog(attachee, 1);
                        DetachScript();
                        SetGlobalFlag(10, false);
                    }

                }

            }

            return RunDefault;
        }

    }
}
