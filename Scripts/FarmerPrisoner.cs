
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
    [ObjectScript(154)]
    public class FarmerPrisoner : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static bool banter(GameObject attachee, GameObject triggerer, int line)
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
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            var loc = new locXY(427, 406);
            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool eat_in_three(GameObject attachee, GameObject triggerer)
        {
            StartTimer(86400000, () => mandy_eaten(attachee));
            StartTimer(259200000, () => whitman_eaten(attachee));
            return RunDefault;
        }
        public static bool mandy_eaten(GameObject attachee)
        {
            SetGlobalFlag(170, true);
            return RunDefault;
        }
        public static bool whitman_eaten(GameObject attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }

    }
}
