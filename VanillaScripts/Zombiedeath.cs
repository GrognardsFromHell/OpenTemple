
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(250)]
    public class Zombiedeath : BaseObjectScript
    {

        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(0, GetGlobalVar(0) + 1);
            Logger.Info("Zombies dead={0}", GetGlobalVar(0));
            if (GetGlobalVar(0) == 3)
            {
                if (!UiSystems.HelpManager.IsTutorialActive)
                {
                    UiSystems.HelpManager.ToggleTutorial();
                }

                UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.LootReminder);
                DetachScript();

            }

            return RunDefault;
        }


    }
}
