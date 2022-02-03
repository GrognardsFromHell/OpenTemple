
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
    [ObjectScript(501)]
    public class Mathel : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.HasMet(triggerer)))
            {
                triggerer.TurnTowards(attachee);
                triggerer.BeginDialog(attachee, 30);
            }
            else
            {
                triggerer.TurnTowards(attachee);
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(976, true);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("ef-MinoCloud", attachee);
            AttachParticles("Brasier", attachee);
            Sound(4038, 1);
            var cloak = attachee.FindItemByName(6427);
            cloak.SetObjectFlag(ObjectFlag.OFF);
            var armor = attachee.FindItemByName(6475);
            armor.SetObjectFlag(ObjectFlag.OFF);
            var boots = attachee.FindItemByName(6045);
            boots.SetObjectFlag(ObjectFlag.OFF);
            var gloves = attachee.FindItemByName(6046);
            gloves.SetObjectFlag(ObjectFlag.OFF);
            var helm = attachee.FindItemByName(6209);
            helm.SetObjectFlag(ObjectFlag.OFF);
            var ring = attachee.FindItemByName(6083);
            ring.SetObjectFlag(ObjectFlag.OFF);
            var weapon = attachee.FindItemByName(4185);
            weapon.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((is_better_to_talk(attachee, PartyLeader)))
                {
                    attachee.TurnTowards(PartyLeader);
                    PartyLeader.BeginDialog(attachee, 1);
                    DetachScript();
                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 30))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
            }

            attachee.RunOff();
            return RunDefault;
        }

    }
}
