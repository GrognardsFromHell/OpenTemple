
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
    [ObjectScript(154)]
    public class FarmerPrisoner : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001 && (SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29))))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else
            {
                if ((!attachee.HasMet(triggerer)))
                {
                    if ((!GetGlobalFlag(169)))
                    {
                        triggerer.BeginDialog(attachee, 1);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 40);
                    }

                }
                else if ((GetGlobalFlag(169)))
                {
                    triggerer.BeginDialog(attachee, 100);
                }
                else if ((GetGlobalFlag(170)))
                {
                    triggerer.BeginDialog(attachee, 110);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 120);
                }

            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static bool banter(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8037);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(427, 406);
            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool eat_in_three(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(86400000, () => mandy_eaten(attachee));
            StartTimer(259200000, () => whitman_eaten(attachee));
            return RunDefault;
        }
        public static bool mandy_eaten(GameObjectBody attachee)
        {
            SetGlobalFlag(170, true);
            return RunDefault;
        }
        public static bool whitman_eaten(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}
