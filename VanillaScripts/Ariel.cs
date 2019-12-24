
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
    [ObjectScript(238)]
    public class Ariel : BaseObjectScript
    {

        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!GetGlobalFlag(11))
            {
                attachee.ForgetMemorizedSpells();
                SetGlobalFlag(11, true);
            }

            return RunDefault;
        }
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            UiSystems.HelpManager.ShowTutorialTopic(TutorialTopic.ArielKill);
            SetGlobalFlag(11, true);
            return RunDefault;
        }


    }
}
