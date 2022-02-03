
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

namespace Scripts;

[ObjectScript(149)]
public class Thrommel : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 120); // thrommel in party
        }
        else if ((GetGlobalVar(907) == 32))
        {
            triggerer.BeginDialog(attachee, 230); // have attacked 3 or more farm animals with thrommel in party - probably impossible
        }
        else
        {
            triggerer.BeginDialog(attachee, 1); // none of the above
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        var itemF = attachee.FindItemByName(2201);
        if ((itemF != null))
        {
            itemF.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(150, true);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(150, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(907) >= 3))
            {
                if ((attachee != null))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(attachee);
                        attachee.FloatLine(22000, triggerer);
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var itemD = attachee.FindItemByName(2201);
        if ((itemD != null))
        {
            itemD.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5111) || (attachee.GetMap() == 5112) || (attachee.GetMap() == 5001) || (attachee.GetMap() == 5121)))
        {
            // 5062 - Temple
            // 5111 - Temple Tower
            // 5112 - Ramshackle Farm (secret entrance to Temple Tower / level 3
            // 5001 - Hommlet
            // 5121 - Verbobonc
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                leader.BeginDialog(attachee, 130);
            }

        }

        return RunDefault;
    }
    public override bool OnTrueSeeing(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.HasLineOfSight(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return RunDefault;
    }
    public override bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
    {
        if ((spell.spellEnum == WellKnownSpells.DispelMagic))
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(151, true);
        attachee.RunOff();
        return RunDefault;
    }
    public static bool check_follower_thrommel_comments(GameObject attachee, GameObject triggerer)
    {
        var npc = Utilities.find_npc_near(attachee, 8014);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, 490);
        }
        else
        {
            npc = Utilities.find_npc_near(attachee, 8000);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 550);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

        }

        return RunDefault;
    }
    public static bool schedule_reward(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(152, true);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        StartTimer(1209600000, () => give_reward()); // 1209600000ms is 2 weeks
        ScriptDaemon.record_time_stamp("s_thrommel_reward");
        return RunDefault;
    }
    public static bool give_reward()
    {
        QueueRandomEncounter(3001);
        ScriptDaemon.set_f("s_thrommel_reward_scheduled");
        return RunDefault;
    }
    public static bool equip_transfer(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(3014);
        if ((itemA != null))
        {
            itemA.ClearItemFlag(ItemFlag.NO_TRANSFER);
            attachee.TransferItemByNameTo(triggerer, 3014);
        }

        return RunDefault;
    }
    public static bool equip_transfer2(GameObject attachee, GameObject triggerer)
    {
        if (SelectedPartyLeader.GetPartyMembers().Any(o => o.HasItemByName(2201)))
        {
            Utilities.party_transfer_to(attachee, 2201);
        }

        return RunDefault;
    }

}