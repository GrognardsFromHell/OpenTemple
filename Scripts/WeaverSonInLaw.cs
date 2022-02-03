
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

[ObjectScript(85)]
public class WeaverSonInLaw : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
        {
            attachee.FloatLine(11002, triggerer);
        }
        else
        {
            if ((triggerer.GetStat(Stat.charisma) >= 8 && triggerer.GetGender() == Gender.Female))
            {
                if ((attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 70);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

        }

        return SkipDefault;
    }
    public static bool argue(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8006);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 10);
        }

        return SkipDefault;
    }

}