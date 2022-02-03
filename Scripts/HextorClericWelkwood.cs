
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

[ObjectScript(392)]
public class HextorClericWelkwood : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            StartTimer(3000, () => go_boom(attachee, triggerer));
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((attachee.GetMap() == 5093))
            {
                if ((is_cool_to_talk(attachee, PartyLeader)))
                {
                    if ((GetGlobalVar(960) == 3))
                    {
                        PartyLeader.TurnTowards(attachee);
                        PartyLeader.BeginDialog(attachee, 1);
                        SetGlobalVar(960, 4);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_cool_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 20))
        {
            return true;
        }

        return false;
    }
    public static bool run_off_gang(GameObject attachee, GameObject triggerer)
    {
        Sound(4031, 1);
        AttachParticles("sp-Invisibility", attachee);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        var sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        sb = Utilities.find_npc_near(attachee, 14720);
        AttachParticles("sp-Invisibility", sb);
        sb.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool go_boom(GameObject attachee, GameObject triggerer)
    {
        SpawnParticles("sp-Fireball-Hit", new locXY(484, 468));
        SpawnParticles("ef-fire-lazy", new locXY(484, 468));
        SpawnParticles("ef-Embers Small", new locXY(484, 468));
        SpawnParticles("sp-Fireball-Hit", new locXY(468, 452));
        SpawnParticles("ef-fire-lazy", new locXY(468, 452));
        SpawnParticles("ef-Embers Small", new locXY(468, 452));
        SpawnParticles("ef-fire-lazy", new locXY(468, 485));
        SpawnParticles("ef-Embers Small", new locXY(468, 485));
        SpawnParticles("ef-fire-lazy", new locXY(468, 464));
        SpawnParticles("ef-Embers Small", new locXY(468, 464));
        Sound(4111, 1);
        return RunDefault;
    }

}