
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
    [ObjectScript(262)]
    public class BurneApprentice : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool destroy_orb(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(326, true);
            StartTimer(345600000, () => return_Zuggtmoy(attachee, triggerer));
            return RunDefault;
        }
        public static bool play_effect(GameObject attachee, GameObject triggerer)
        {
            AttachParticles("orb-destroy", attachee);
            return RunDefault;
        }
        public static bool return_Zuggtmoy(GameObject attachee, GameObject triggerer)
        {
            Fade(0, 0, 301, 0);
            Utilities.set_end_slides(attachee, triggerer);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return SkipDefault;
        }


    }
}
