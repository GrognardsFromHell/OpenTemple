
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

[ObjectScript(4)]
public class Burne : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 330); // burne in party
        }
        else if ((GetGlobalVar(909) == 32 && attachee.GetMap() != 5016 && attachee.GetMap() != 5018))
        {
            triggerer.BeginDialog(attachee, 1060); // have attacked 3 or more farm animals with burne in party and not in castle main hall or upper hall
        }
        else if ((GetGlobalFlag(839)))
        {
            triggerer.BeginDialog(attachee, 1160); // have liberated lareth
        }
        else if ((GetGlobalFlag(835) && !GetGlobalFlag(37) && GetGlobalFlag(842) && !GetGlobalFlag(839)))
        {
            triggerer.BeginDialog(attachee, 1000); // handled tower fight diplomatically and lareth is alive and have heard about prisoner lareth and have not liberated lareth
        }
        else if ((PartyLeader.HasReputation(28)))
        {
            triggerer.BeginDialog(attachee, 590); // have dominatrix reputation - burne will kiss your ass
        }
        else if ((PartyLeader.HasReputation(27)))
        {
            triggerer.BeginDialog(attachee, 11002); // have rabble-rouser reputation - burne won't talk to you
        }
        else
        {
            triggerer.BeginDialog(attachee, 1); // none of the above
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if (((GetGlobalVar(501) >= 2 && GetQuestState(97) != QuestState.Completed && GetQuestState(96) != QuestState.Completed) || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(730, 0);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer);
        SetGlobalFlag(336, true);
        SetGlobalFlag(282, true);
        if ((!GetGlobalFlag(231)))
        {
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

        }
        else
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(336, false);
        SetGlobalFlag(282, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(909) >= 3))
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

            if ((GetGlobalVar(730) == 0 && attachee.GetLeader() == null))
            {
                attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                attachee.PendingSpellsToMemorized();
                SetGlobalVar(730, 1);
            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(231, true);
        var diamond = attachee.FindItemByName(12036);
        diamond.SetItemFlag(ItemFlag.NO_TRANSFER);
        var amber = attachee.FindItemByName(12040);
        amber.SetItemFlag(ItemFlag.NO_TRANSFER);
        var silver_medallion_necklace = attachee.FindItemByName(6197);
        silver_medallion_necklace.SetItemFlag(ItemFlag.NO_TRANSFER);
        var emerald = attachee.FindItemByName(12010);
        emerald.SetItemFlag(ItemFlag.NO_TRANSFER);
        var silver_necklace = attachee.FindItemByName(6194);
        silver_necklace.SetItemFlag(ItemFlag.NO_TRANSFER);
        var dagger = attachee.FindItemByName(4058);
        dagger.SetItemFlag(ItemFlag.NO_TRANSFER);
        var wand = attachee.FindItemByName(12007);
        wand.SetItemFlag(ItemFlag.NO_TRANSFER);
        var chime = attachee.FindItemByName(12008);
        chime.SetItemFlag(ItemFlag.NO_TRANSFER);
        var ring = attachee.FindItemByName(6083);
        ring.SetItemFlag(ItemFlag.NO_TRANSFER);
        var kit = attachee.FindItemByName(12848);
        kit.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(231, false);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        var diamond = attachee.FindItemByName(12036);
        diamond.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var amber = attachee.FindItemByName(12040);
        amber.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var silver_medallion_necklace = attachee.FindItemByName(6197);
        silver_medallion_necklace.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var emerald = attachee.FindItemByName(12010);
        emerald.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var silver_necklace = attachee.FindItemByName(6194);
        silver_necklace.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var dagger = attachee.FindItemByName(4058);
        dagger.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var wand = attachee.FindItemByName(12007);
        wand.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var chime = attachee.FindItemByName(12008);
        chime.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var ring = attachee.FindItemByName(6083);
        ring.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var kit = attachee.FindItemByName(12848);
        kit.ClearItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(195)))
        {
            SelectedPartyLeader.BeginDialog(attachee, 480);
        }

        return SkipDefault;
    }

}