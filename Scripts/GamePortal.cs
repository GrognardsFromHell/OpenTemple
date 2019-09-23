
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
    [ObjectScript(406)]
    public class GamePortal : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Co8.get_Co8_options_from_ini();
            if ((GetGlobalFlag(601)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = PartyLeader;
            Co8.StopCombat(attachee, 0);
            leader.BeginDialog(attachee, 4000);
            return RunDefault;
        }
        public static void intro_movie_setup(GameObjectBody attachee, GameObjectBody triggerer)
        {
            set_intro_slides();
            return;
        }
        public static bool set_intro_slides()
        {
            GameSystems.Movies.MovieQueueAdd(428);
            GameSystems.Movies.MovieQueueAdd(420);
            GameSystems.Movies.MovieQueueAdd(421);
            GameSystems.Movies.MovieQueueAdd(422);
            GameSystems.Movies.MovieQueueAdd(423);
            GameSystems.Movies.MovieQueueAdd(424);
            GameSystems.Movies.MovieQueueAdd(425);
            GameSystems.Movies.MovieQueueAdd(426);
            GameSystems.Movies.MovieQueueAdd(427);
            GameSystems.Movies.MovieQueueAdd(429);
            if ((PartyAlignment == Alignment.LAWFUL_GOOD))
            {
                GameSystems.Movies.MovieQueueAdd(1000);
            }

            if ((PartyAlignment == Alignment.NEUTRAL_GOOD))
            {
                GameSystems.Movies.MovieQueueAdd(1005);
            }

            if ((PartyAlignment == Alignment.CHAOTIC_GOOD))
            {
                GameSystems.Movies.MovieQueueAdd(1001);
            }

            if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL))
            {
                GameSystems.Movies.MovieQueueAdd(1007);
            }

            if ((PartyAlignment == Alignment.NEUTRAL))
            {
                GameSystems.Movies.MovieQueueAdd(1004);
            }

            if ((PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
            {
                GameSystems.Movies.MovieQueueAdd(1008);
            }

            if ((PartyAlignment == Alignment.LAWFUL_EVIL))
            {
                GameSystems.Movies.MovieQueueAdd(1002);
            }

            if ((PartyAlignment == Alignment.NEUTRAL_EVIL))
            {
                GameSystems.Movies.MovieQueueAdd(1006);
            }

            if ((PartyAlignment == Alignment.CHAOTIC_EVIL))
            {
                GameSystems.Movies.MovieQueueAdd(1003);
            }

            GameSystems.Movies.MovieQueuePlay();
            return RunDefault;
        }

    }
}
