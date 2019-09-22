
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
    [ObjectScript(170)]
    public class Taki : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040))))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 120);
                }

            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8040))))
            {
                triggerer.BeginDialog(attachee, 170);
            }
            else
            {
                triggerer.BeginDialog(attachee, 190);
            }

            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = attachee.GetLeader();

            if ((leader != null))
            {
                if ((triggerer.type == ObjectType.npc))
                {
                    if (((triggerer.GetAlignment() == Alignment.LAWFUL_GOOD) || (triggerer.GetAlignment() == Alignment.NEUTRAL_GOOD) || (triggerer.GetAlignment() == Alignment.CHAOTIC_GOOD)))
                    {
                        attachee.FloatLine(RandomRange(220, 222), leader);
                        return SkipDefault;
                    }

                }

            }

            return RunDefault;
        }
        public static bool switch_to_ashrem(GameObjectBody attachee, GameObjectBody triggerer, int line, int alternate_line)
        {
            var ashrem = Utilities.find_npc_near(attachee, 8040);

            if ((ashrem != null))
            {
                triggerer.BeginDialog(ashrem, line);
                ashrem.TurnTowards(attachee);
                attachee.TurnTowards(ashrem);
            }
            else
            {
                triggerer.BeginDialog(attachee, alternate_line);
            }

            return SkipDefault;
        }


    }
}
