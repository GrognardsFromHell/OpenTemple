
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

[ObjectScript(562)]
public class KobortAmbushCourtyard : BaseObjectScript
{
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
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        if ((is_better_to_talk(attachee, PartyLeader)))
                        {
                            StartTimer(2000, () => start_talking(attachee, triggerer));
                            DetachScript();
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 50))
        {
            return true;
        }

        return false;
    }
    public static bool start_talking(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 600);
        return RunDefault;
    }

}