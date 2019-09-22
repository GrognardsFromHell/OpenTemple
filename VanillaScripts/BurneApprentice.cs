
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
    [ObjectScript(262)]
    public class BurneApprentice : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public static bool destroy_orb(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(326, true);
            StartTimer(345600000, () => return_Zuggtmoy(attachee, triggerer));
            return RunDefault;
        }
        public static bool play_effect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            AttachParticles("orb-destroy", attachee);
            return RunDefault;
        }
        public static bool return_Zuggtmoy(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Fade(0, 0, 301, 0);
            Utilities.set_end_slides(attachee, triggerer);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return SkipDefault;
        }


    }
}
