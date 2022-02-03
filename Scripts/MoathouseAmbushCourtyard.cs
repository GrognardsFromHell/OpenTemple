
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
    [ObjectScript(565)]
    public class MoathouseAmbushCourtyard : BaseObjectScript
    {
        public override bool OnUse(GameObject attachee, GameObject triggerer)
        {
            // used by door in Moathouse Interior to Moathouse Courtyard to tell which Ambush to turn on
            SetGlobalVar(710, 2);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(765) == 1 || GetGlobalVar(765) == 3 || GetGlobalFlag(283)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }
            else if (((attachee.GetMap() == 5002) && (GetGlobalVar(765) == 0) && (PartyAlignment != Alignment.LAWFUL_EVIL) && (GetGlobalFlag(977)) && (GetGlobalVar(710) == 2)) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & (1 << 11)) == 0))
            {
                if (!SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8004)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8005)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8010)))
                {
                    if (((!GetGlobalFlag(44)) && (!GetGlobalFlag(45)) && (!GetGlobalFlag(700)) && (GetGlobalFlag(37)) && (!GetGlobalFlag(283))))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

            attachee.SetScriptId(ObjScriptEvent.StartCombat, 2); // san_start_combat
            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(765, 2);
            return RunDefault;
        }

    }
}
