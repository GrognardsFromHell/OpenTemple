
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
    [ObjectScript(323)]
    public class NulbVillageWoman : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(958) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                AttachParticles("ef-Node Portal", attachee);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
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
        public static bool get_sick(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static int all_die(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(958, 7);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Poison", pc);
            }

            return 1;
        }
        public static bool stop_watch(GameObjectBody attachee, GameObjectBody triggerer)
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
