
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

[ObjectScript(400)]
public class CastlePortal : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14581)) // red portal
        {
            if ((GetGlobalVar(931) != 0))
            {
                triggerer.BeginDialog(attachee, 710);
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
            }

        }
        else if ((attachee.GetNameId() == 14582)) // orange portal
        {
            if ((GetGlobalVar(932) != 0))
            {
                triggerer.BeginDialog(attachee, 810);
            }
            else
            {
                triggerer.BeginDialog(attachee, 210);
            }

        }
        else if ((attachee.GetNameId() == 14583)) // yellow portal
        {
            if ((GetGlobalVar(933) != 0))
            {
                triggerer.BeginDialog(attachee, 910);
            }
            else
            {
                triggerer.BeginDialog(attachee, 310);
            }

        }
        else if ((attachee.GetNameId() == 14584)) // green portal
        {
            if ((GetGlobalVar(934) != 0))
            {
                triggerer.BeginDialog(attachee, 1010);
            }
            else
            {
                triggerer.BeginDialog(attachee, 410);
            }

        }
        else if ((attachee.GetNameId() == 14585)) // blue portal
        {
            if ((GetGlobalVar(935) != 0))
            {
                triggerer.BeginDialog(attachee, 1110);
            }
            else
            {
                triggerer.BeginDialog(attachee, 510);
            }

        }
        else if ((attachee.GetNameId() == 14586)) // violet portal
        {
            if ((GetGlobalVar(936) != 0))
            {
                triggerer.BeginDialog(attachee, 1210);
            }
            else
            {
                triggerer.BeginDialog(attachee, 610);
            }

        }

        return SkipDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        var leader = PartyLeader;
        Co8.StopCombat(attachee, 0);
        leader.BeginDialog(attachee, 1);
        return RunDefault;
    }
    public static bool play_sparkles(GameObject attachee)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            AttachParticles("hit-BANE-medium", pc);
        }

        return RunDefault;
    }
    public static bool teleport_hommlet(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_hommlet(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_hommlet(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5001, 623, 418);
        return RunDefault;
    }
    public static bool teleport_nulb(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_nulb(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_nulb(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5051, 507, 361);
        return RunDefault;
    }
    public static bool teleport_verbobonc(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_verbobonc(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_verbobonc(GameObject attachee)
    {
        SetGlobalFlag(260, true);
        FadeAndTeleport(0, 0, 0, 5121, 252, 506);
        return RunDefault;
    }
    public static bool teleport_tower(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_tower(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_tower(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5016, 478, 486);
        return RunDefault;
    }
    public static bool teleport_cuthbert(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_cuthbert(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_cuthbert(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5012, 484, 485);
        return RunDefault;
    }
    public static bool teleport_grove(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_grove(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_grove(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5042, 492, 477);
        return RunDefault;
    }
    public static bool teleport_wench(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_wench(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_wench(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5007, 484, 486);
        return RunDefault;
    }
    public static bool teleport_smyth(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_smyth(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_smyth(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5001, 574, 440);
        return RunDefault;
    }
    public static bool teleport_trader(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_trader(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_trader(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5010, 481, 480);
        return RunDefault;
    }
    public static bool teleport_residence(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_residence(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_residence(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5085, 473, 495);
        return RunDefault;
    }
    public static bool teleport_hostel(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_hostel(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_hostel(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5060, 483, 500);
        return RunDefault;
    }
    public static bool teleport_otis(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_otis(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_otis(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5056, 492, 487);
        return RunDefault;
    }
    public static bool teleport_fong(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_fong(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_fong(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5088, 473, 492);
        return RunDefault;
    }
    public static bool teleport_cityhall(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_cityhall(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_cityhall(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5169, 490, 490);
        return RunDefault;
    }
    public static bool teleport_constabulary(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_constabulary(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_constabulary(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5171, 489, 495);
        return RunDefault;
    }
    public static bool teleport_pelor(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_pelor(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_pelor(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5137, 484, 485);
        return RunDefault;
    }
    public static bool teleport_goose(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_goose(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_goose(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5151, 497, 490);
        return RunDefault;
    }
    public static bool teleport_archers(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_archers(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_archers(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5161, 475, 472);
        return RunDefault;
    }
    public static bool teleport_bazaar(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_bazaar(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_bazaar(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5180, 516, 502);
        return RunDefault;
    }
    public static bool teleport_emridy(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_emridy(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_emridy(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5094, 488, 488);
        return RunDefault;
    }
    public static bool teleport_hickory(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_hickory(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_hickory(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5095, 462, 462);
        return RunDefault;
    }
    public static bool teleport_welkwood(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_welkwood(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_welkwood(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5093, 517, 324);
        return RunDefault;
    }
    public static bool teleport_moathouse(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_moathouse(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_moathouse(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5004, 476, 478);
        return RunDefault;
    }
    public static bool teleport_moatdung(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_moatdung(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_moatdung(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5005, 424, 414);
        return RunDefault;
    }
    public static bool teleport_toee(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_toee(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_toee(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5064, 490, 499);
        return RunDefault;
    }
    public static bool teleport_toee1(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_toee1(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_toee1(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5066, 484, 517);
        return RunDefault;
    }
    public static bool teleport_toee2(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_toee2(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_toee2(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5067, 487, 455);
        return RunDefault;
    }
    public static bool teleport_toee3(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_toee3(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_toee3(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5105, 491, 519);
        return RunDefault;
    }
    public static bool teleport_toee4(GameObject attachee, GameObject triggerer)
    {
        Sound(4121, 1);
        AttachParticles("sp-Teleport", attachee);
        StartTimer(2500, () => telly_toee4(attachee));
        StartTimer(2000, () => play_sparkles(attachee));
        return RunDefault;
    }
    public static bool telly_toee4(GameObject attachee)
    {
        FadeAndTeleport(0, 0, 0, 5080, 479, 590);
        return RunDefault;
    }

}