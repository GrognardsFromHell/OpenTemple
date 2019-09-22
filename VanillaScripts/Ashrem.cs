
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
    [ObjectScript(175)]
    public class Ashrem : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 170);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else
            {
                triggerer.BeginDialog(attachee, 150);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var alrrem = Utilities.find_npc_near(attachee, 8047);

                if ((alrrem != null))
                {
                    if ((!GetGlobalFlag(192)))
                    {
                        triggerer.BeginDialog(attachee, 230);
                    }

                }

            }

            return RunDefault;
        }
        public static bool talk_Taki(GameObjectBody attachee, GameObjectBody triggerer, int line, int alternate_line)
        {
            var taki = Utilities.find_npc_near(attachee, 8039);

            if ((taki != null))
            {
                triggerer.BeginDialog(taki, line);
                taki.TurnTowards(attachee);
                attachee.TurnTowards(taki);
            }
            else
            {
                triggerer.BeginDialog(attachee, alternate_line);
            }

            return SkipDefault;
        }
        public static bool talk_Alrrem(GameObjectBody attachee, GameObjectBody triggerer, int line, int alternate_line)
        {
            var alrrem = Utilities.find_npc_near(attachee, 8047);

            if ((alrrem != null))
            {
                triggerer.BeginDialog(alrrem, line);
                alrrem.TurnTowards(attachee);
                attachee.TurnTowards(alrrem);
            }
            else
            {
                triggerer.BeginDialog(attachee, alternate_line);
            }

            return SkipDefault;
        }


    }
}
