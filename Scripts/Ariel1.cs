
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
    [ObjectScript(238)]
    public class Ariel1 : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(10, false);
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if (!GetGlobalFlag(11))
            {
                attachee.ForgetMemorizedSpells();
                SetGlobalFlag(11, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.ArielKill);
            SetGlobalFlag(11, true);
            return RunDefault;
        }
        public override bool OnJoin(GameObject attachee, GameObject triggerer)
        {
            if (!UiSystems.HelpManager.IsTutorialActive)
            {
                UiSystems.HelpManager.ToggleTutorial();
            }

            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.MemorizeSpells);
            SetGlobalFlag(1, true);
            return SkipDefault;
        }

    }
}
