
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
    [ObjectScript(323)]
    public class NulbVillageWoman : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(958) == 1))
            {
                triggerer.BeginDialog(attachee, 1000);
            }
            else if ((GetGlobalVar(958) == 2))
            {
                triggerer.BeginDialog(attachee, 2000);
            }
            else if ((GetGlobalVar(958) == 10))
            {
                triggerer.BeginDialog(attachee, 5000);
            }
            else if ((GetGlobalVar(958) == 3))
            {
                triggerer.BeginDialog(attachee, 3000);
            }
            else
            {
                return SkipDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(958) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                AttachParticles("ef-Node Portal", attachee);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(958) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        PartyLeader.BeginDialog(attachee, 1000);
                    }

                }

            }
            else if ((GetGlobalVar(958) == 4))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        PartyLeader.BeginDialog(attachee, 4000);
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 20))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool get_sick(GameObject attachee, GameObject triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
            {
                obj.AddCondition("Poisoned", 15, 0);
                obj.AddCondition("Poisoned", 32, 0);
                obj.AddCondition("Poisoned", 18, 0);
                obj.AddCondition("Poisoned", 29, 0);
            }

            return RunDefault;
        }
        public static int all_die(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(958, 7);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc);
            }

            return 1;
        }
        public static bool stop_watch(GameObject attachee, GameObject triggerer)
        {
            StartTimer(36000000, () => all_dead()); // 10 hours
            SetGlobalVar(958, 6);
            return RunDefault;
        }
        public static int all_dead()
        {
            if ((GetGlobalVar(958) == 6))
            {
                SetGlobalVar(958, 7);
                var pc1 = PartyLeader;
                pc1.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc1);
                var pc2 = GameSystems.Party.GetPartyGroupMemberN(1);
                pc2.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc2);
                var pc3 = GameSystems.Party.GetPartyGroupMemberN(2);
                pc3.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc3);
                var pc4 = GameSystems.Party.GetPartyGroupMemberN(3);
                pc4.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc4);
                var pc5 = GameSystems.Party.GetPartyGroupMemberN(4);
                pc5.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc5);
                var pc6 = GameSystems.Party.GetPartyGroupMemberN(5);
                pc6.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc6);
                var pc7 = GameSystems.Party.GetPartyGroupMemberN(6);
                pc7.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc7);
                var pc8 = GameSystems.Party.GetPartyGroupMemberN(7);
                pc8.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc8);
            }

            return 1;
        }

    }
}
