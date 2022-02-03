
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

[ObjectScript(362)]
public class VerboboncMage : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        return RunDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            attachee.CastSpell(WellKnownSpells.Shield, attachee);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            pc.AddCondition("fallen_paladin");
        }

        if ((attachee.GetMap() == 5093))
        {
            VerboboncGuard.ditch_rings(attachee, triggerer);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(704) == 8))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                Co8.StopCombat(attachee, 0);
                var wilfrick = Utilities.find_npc_near(attachee, 8703);
                attachee.TurnTowards(wilfrick);
                obj.BeginDialog(attachee, 1);
                SetGlobalVar(704, 9);
                return RunDefault;
            }

        }

        return RunDefault;
    }
    public static bool ass_out(GameObject attachee, GameObject triggerer)
    {
        var wilfrick = Utilities.find_npc_near(attachee, 8703);
        AttachParticles("sp-Teleport", attachee);
        AttachParticles("sp-Teleport", wilfrick);
        Sound(4035, 1);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        wilfrick.SetObjectFlag(ObjectFlag.OFF);
        PartyLeader.AddReputation(42);
        resume_fighting(attachee, triggerer);
        return RunDefault;
    }
    public static void resume_fighting(GameObject attachee, GameObject triggerer)
    {
        var samson = Utilities.find_npc_near(attachee, 8724);
        var goliath = Utilities.find_npc_near(attachee, 8725);
        var bathsheba = Utilities.find_npc_near(attachee, 8726);
        var mage = Utilities.find_npc_near(attachee, 14658);
        var priest = Utilities.find_npc_near(attachee, 14471);
        var guard1 = Utilities.find_npc_near(attachee, 14716);
        var guard2 = Utilities.find_npc_near(attachee, 14719);
        samson.Attack(triggerer);
        goliath.Attack(triggerer);
        bathsheba.Attack(triggerer);
        mage.Attack(triggerer);
        priest.Attack(triggerer);
        guard1.Attack(triggerer);
        guard2.Attack(triggerer);
        return;
    }

}