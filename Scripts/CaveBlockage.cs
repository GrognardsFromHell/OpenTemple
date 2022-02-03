
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
    [ObjectScript(597)]
    public class CaveBlockage : BaseObjectScript
    {
        public override bool OnUse(GameObject door, GameObject triggerer)
        {
            if ((door.GetNameId() == 1620))
            {
                if ((GetGlobalFlag(531)))
                {
                    // if cave blockage is active, disable door portal
                    return SkipDefault;
                }
                else
                {
                    return RunDefault;
                }

            }
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if (((GetGlobalVar(980) == 2) || (GetGlobalVar(981) == 2) || (GetGlobalVar(982) == 2) || (GetGlobalVar(983) == 2) || (GetGlobalVar(984) == 2) && !GetGlobalFlag(531)))
            {
                // turns on cave blockage
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(531, true);
                SetGlobalFlag(572, true);
                DetachScript();
            }
            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            AttachParticles("ef-MinoCloud", attachee);
            AttachParticles("Orb-Summon-Earth Elemental", attachee);
            AttachParticles("Mon-EarthElem-Unconceal", attachee);
            Sound(4042, 1);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(531, false);
            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            return SkipDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            foreach (var target in PartyLeader.GetPartyMembers())
            {
                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
