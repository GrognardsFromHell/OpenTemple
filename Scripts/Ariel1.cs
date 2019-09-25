
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
    [ObjectScript(238)]
    public class Ariel1 : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!GetGlobalFlag(11))
            {
                attachee.ForgetMemorizedSpells();
                SetGlobalFlag(11, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.ArielKill);
            SetGlobalFlag(11, true);
            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
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
