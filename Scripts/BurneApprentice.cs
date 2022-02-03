
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

[ObjectScript(262)]
public class BurneApprentice : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 250); // pishella in party
        }
        else if ((attachee.GetMap() == 5014))
        {
            triggerer.BeginDialog(attachee, 330); // WotGS Hommlet under attack
        }
        else if ((GetGlobalVar(911) == 32 && attachee.GetMap() != 5016 && attachee.GetMap() != 5019))
        {
            triggerer.BeginDialog(attachee, 350); // have attacked 3 or more farm animals with pishella in party and not in castle main hall or parapet interior
        }
        else if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
        {
            attachee.FloatLine(12014, triggerer); // have lawbreaker or convict or banished from hommlet reps
        }
        else if ((GetGlobalFlag(694)))
        {
            triggerer.BeginDialog(attachee, 400); // declined to share information on who is altering construction orders
        }
        else if ((PartyLeader.HasReputation(27)))
        {
            triggerer.BeginDialog(attachee, 180); // have rabble-rouser reputation
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
            if ((attachee.GetMap() == 5016 || attachee.GetMap() == 5019))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5014))
            {
                if ((GetGlobalVar(510) != 2))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
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
        if ((!GetGlobalFlag(239)))
        {
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if (GetGlobalVar(23) >= 2)
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
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.FloatLine(12057, triggerer);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(911) >= 3))
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
        SetGlobalFlag(239, true);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(239, false);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        var randy1 = RandomRange(1, 16);
        if (((attachee.GetMap() == 5012) && randy1 >= 14))
        {
            attachee.FloatLine(500, triggerer);
        }
        else if (((attachee.GetMap() == 5058) && randy1 >= 13))
        {
            attachee.FloatLine(510, triggerer);
        }

        return RunDefault;
    }
    public static bool destroy_orb(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(326, true);
        // set timer for 4 days and then end game or go to verbo
        StartTimer(345600000, () => return_Zuggtmoy(attachee, triggerer));
        ScriptDaemon.record_time_stamp("s_zuggtmoy_banishment_initiate");
        return RunDefault;
    }
    public static void pishella_destroy_skull_while_party_npc(GameObject pc, GameObject npc)
    {
        var pisha = GameSystems.MapObject.CreateObject(14447, npc.GetLocation());
        pisha.SetObjectFlag(ObjectFlag.DONTDRAW);
        Utilities.party_transfer_to(pisha, 2208);
        Utilities.party_transfer_to(pisha, 4003);
        Utilities.party_transfer_to(pisha, 4004);
        Utilities.party_transfer_to(pisha, 3603);
        Utilities.party_transfer_to(pisha, 2203);
        pisha.SetObjectFlag(ObjectFlag.OFF);
    }
    public static bool play_effect(GameObject attachee, GameObject triggerer)
    {
        // play particle effect to destroy the orb
        AttachParticles("orb-destroy", attachee);
        Sound(4036, 1);
        return RunDefault;
    }
    public static bool return_Zuggtmoy(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(188) && !GetGlobalFlag(189)))
        {
            ScriptDaemon.set_f("s_zuggtmoy_gone");
            // play banishment movie
            Fade(0, 0, 301, 0);
            // play slides and end game or go to verbo
            if ((GetGlobalFlag(500)))
            {
                Utilities.set_end_slides_nc(attachee, triggerer);
                GameSystems.Movies.MovieQueuePlay();
                MakeAreaKnown(14);
                StartTimer(1500, () => go_to_verbobonc());
                return RunDefault;
            }
            else
            {
                Utilities.set_end_slides(attachee, triggerer);
                GameSystems.Movies.MovieQueuePlayAndEndGame();
                return SkipDefault;
            }

        }
        return SkipDefault;
    }
    public static void go_to_verbobonc()
    {
        FadeAndTeleport(0, 0, 0, 5121, 228, 507);
    }
    public static bool switch_to_tarah(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8805);
        var pishella = Utilities.find_npc_near(attachee, 8069);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(pishella);
            pishella.TurnTowards(npc);
        }

        return SkipDefault;
    }

}