
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
    [ObjectScript(597)]
    public class CaveBlockage : BaseObjectScript
    {
        private static readonly string HB_BLOCKAGE_KEY = "HB_BLOCKAGE_SERIAL";
        public override bool OnUse(GameObjectBody door, GameObjectBody triggerer)
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

        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalVar(980) == 2) || (GetGlobalVar(981) == 2) || (GetGlobalVar(982) == 2) || (GetGlobalVar(983) == 2) || (GetGlobalVar(984) == 2) && !GetGlobalFlag(531)))
            {
                // turns on cave blockage
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(531, true);
                SetGlobalFlag(572, true);
                DetachScript();
            }

            var hb_blockage_serial = derefHandle(attachee);
            Co8PersistentData.setData/*Unknown*/(HB_BLOCKAGE_KEY, hb_blockage_serial);
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var target in PartyLeader.GetPartyMembers())
            {
                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
