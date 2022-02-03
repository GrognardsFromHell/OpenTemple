
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

[ObjectScript(465)]
public class PhysicalEvidence : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 11009))
        {
            SetGlobalVar(552, GetGlobalVar(552) + 1);
            if ((GetGlobalVar(552) <= 1))
            {
                SetGlobalVar(543, GetGlobalVar(543) + 1);
                check_evidence_rep_pan(attachee, triggerer);
            }

            DetachScript();
        }
        else if ((attachee.GetNameId() == 11058))
        {
            SetGlobalVar(553, GetGlobalVar(553) + 1);
            if ((GetGlobalVar(553) <= 1))
            {
                SetGlobalVar(544, GetGlobalVar(544) + 1);
                check_evidence_rep_bor(attachee, triggerer);
            }

            DetachScript();
        }
        else if ((attachee.GetNameId() == 11099))
        {
            SetGlobalVar(554, GetGlobalVar(554) + 1);
            if ((GetGlobalVar(554) <= 1))
            {
                SetGlobalVar(545, GetGlobalVar(545) + 1);
                check_evidence_rep_rak(attachee, triggerer);
            }

            DetachScript();
        }
        else if ((attachee.GetNameId() == 11060))
        {
            SetGlobalVar(553, GetGlobalVar(553) + 1);
            if ((GetGlobalVar(553) <= 1))
            {
                SetGlobalVar(544, GetGlobalVar(544) + 1);
                check_evidence_rep_bor(attachee, triggerer);
            }

            attachee.Destroy();
        }
        else if ((attachee.GetNameId() == 11061))
        {
            SetGlobalVar(554, GetGlobalVar(554) + 1);
            if ((GetGlobalVar(554) <= 1))
            {
                SetGlobalVar(545, GetGlobalVar(545) + 1);
                check_evidence_rep_rak(attachee, triggerer);
            }

            attachee.Destroy();
        }
        else if ((attachee.GetNameId() == 11062))
        {
            SetGlobalVar(552, GetGlobalVar(552) + 1);
            if ((GetGlobalVar(552) <= 1))
            {
                SetGlobalVar(543, GetGlobalVar(543) + 1);
                check_evidence_rep_pan(attachee, triggerer);
            }

            attachee.Destroy();
        }

        return RunDefault;
    }
    public static void check_evidence_rep_bor(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(72)))
        {
            PartyLeader.AddReputation(75);
            PartyLeader.RemoveReputation(72);
        }
        else if ((PartyLeader.HasReputation(69)))
        {
            PartyLeader.AddReputation(72);
            PartyLeader.RemoveReputation(69);
        }
        else if ((!PartyLeader.HasReputation(69)))
        {
            if ((!PartyLeader.HasReputation(72)))
            {
                if ((!PartyLeader.HasReputation(75)))
                {
                    PartyLeader.AddReputation(69);
                }

            }

        }

        return;
    }
    public static void check_evidence_rep_pan(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(73)))
        {
            PartyLeader.AddReputation(76);
            PartyLeader.RemoveReputation(73);
        }
        else if ((PartyLeader.HasReputation(70)))
        {
            PartyLeader.AddReputation(73);
            PartyLeader.RemoveReputation(70);
        }
        else if ((!PartyLeader.HasReputation(70)))
        {
            if ((!PartyLeader.HasReputation(73)))
            {
                if ((!PartyLeader.HasReputation(76)))
                {
                    PartyLeader.AddReputation(70);
                }

            }

        }

        return;
    }
    public static void check_evidence_rep_rak(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(74)))
        {
            PartyLeader.AddReputation(77);
            PartyLeader.RemoveReputation(74);
        }
        else if ((PartyLeader.HasReputation(71)))
        {
            PartyLeader.AddReputation(74);
            PartyLeader.RemoveReputation(71);
        }
        else if ((!PartyLeader.HasReputation(71)))
        {
            if ((!PartyLeader.HasReputation(74)))
            {
                if ((!PartyLeader.HasReputation(77)))
                {
                    PartyLeader.AddReputation(71);
                }

            }

        }

        return;
    }

}