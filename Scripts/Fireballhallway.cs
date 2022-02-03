
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

namespace Scripts;

[ObjectScript(254)]
public class Fireballhallway : BaseObjectScript
{
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
        {
            if ((!Utilities.critter_is_unconscious(obj)))
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