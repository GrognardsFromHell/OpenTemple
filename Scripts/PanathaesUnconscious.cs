
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

[ObjectScript(464)]
public class PanathaesUnconscious : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(533)))
        {
            var pan1 = Utilities.find_npc_near(triggerer, 8791);
            if ((pan1 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan2 = Utilities.find_npc_near(triggerer, 8792);
            if ((pan2 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan3 = Utilities.find_npc_near(triggerer, 8793);
            if ((pan3 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan4 = Utilities.find_npc_near(triggerer, 8794);
            if ((pan4 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan5 = Utilities.find_npc_near(triggerer, 8795);
            if ((pan5 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan6 = Utilities.find_npc_near(triggerer, 8796);
            if ((pan6 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan7 = Utilities.find_npc_near(triggerer, 8797);
            if ((pan7 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

            var pan8 = Utilities.find_npc_near(triggerer, 8798);
            if ((pan8 != null))
            {
                var loc = triggerer.GetLocation();
                var npc = GameSystems.MapObject.CreateObject(14818, loc);
                triggerer.BeginDialog(npc, 200);
            }

        }

        return SkipDefault;
    }
    public static bool gather_panathaes(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(539) == 1))
        {
            var pan1 = Utilities.find_npc_near(triggerer, 8791);
            triggerer.AddFollower(pan1);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan1.AIRemoveFromShitlist(pc);
                pan1.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 2))
        {
            var pan2 = Utilities.find_npc_near(triggerer, 8792);
            triggerer.AddFollower(pan2);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan2.AIRemoveFromShitlist(pc);
                pan2.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 3))
        {
            var pan3 = Utilities.find_npc_near(triggerer, 8793);
            triggerer.AddFollower(pan3);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan3.AIRemoveFromShitlist(pc);
                pan3.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 4))
        {
            var pan4 = Utilities.find_npc_near(triggerer, 8794);
            triggerer.AddFollower(pan4);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan4.AIRemoveFromShitlist(pc);
                pan4.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 5))
        {
            var pan5 = Utilities.find_npc_near(triggerer, 8795);
            triggerer.AddFollower(pan5);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan5.AIRemoveFromShitlist(pc);
                pan5.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 6))
        {
            var pan6 = Utilities.find_npc_near(triggerer, 8796);
            triggerer.AddFollower(pan6);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan6.AIRemoveFromShitlist(pc);
                pan6.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 7))
        {
            var pan7 = Utilities.find_npc_near(triggerer, 8797);
            triggerer.AddFollower(pan7);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan7.AIRemoveFromShitlist(pc);
                pan7.SetReaction(pc, 50);
            }

        }
        else if ((GetGlobalVar(539) == 8))
        {
            var pan8 = Utilities.find_npc_near(triggerer, 8798);
            triggerer.AddFollower(pan8);
            SetGlobalVar(551, 1);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pan8.AIRemoveFromShitlist(pc);
                pan8.SetReaction(pc, 50);
            }

        }

        return RunDefault;
    }
    public static void increment_var_543(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(543, GetGlobalVar(543) + 1);
        return;
    }
    public static void increment_var_544(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(544, GetGlobalVar(544) + 1);
        return;
    }
    public static void increment_var_545(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(545, GetGlobalVar(545) + 1);
        return;
    }
    public void increment_var_556(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(556, GetGlobalVar(556) + 1);
        DetachScript();
        return;
    }
    public void increment_var_557(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(557, GetGlobalVar(557) + 1);
        DetachScript();
        return;
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