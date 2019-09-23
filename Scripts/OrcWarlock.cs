
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(599)]
    public class OrcWarlock : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var webbed = break_free(attachee, 3);
            if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 8))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(6)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(7)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(6)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(7)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 7))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(6)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(6)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 6))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(5)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 5))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(4)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 4))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(3)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 3))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(2)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 2))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15 || attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16 && attachee.DistanceTo(GameSystems.Party.GetPartyGroupMemberN(1)) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }
            else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 1))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 15))
                {
                    if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }
                else if ((attachee.DistanceTo(PartyLeader) >= 16))
                {
                    if ((GetGlobalVar(971) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 391);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 498);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 499);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 500);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 501);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 502);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 503);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(971) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 504);
                        SetGlobalVar(500, 2);
                    }
                    else if ((GetGlobalVar(784) == 0))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 505);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 506);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 507);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 508);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 509);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 5))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 510);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 6))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 511);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 7))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 512);
                        SetGlobalVar(500, 1);
                    }
                    else if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 513);
                        SetGlobalVar(500, 3);
                    }
                    else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 514);
                        SetGlobalVar(500, 3);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(500) == 1))
            {
                if ((GetGlobalVar(784) == 0))
                {
                    SetGlobalVar(784, 1);
                }
                else if ((GetGlobalVar(784) == 1))
                {
                    SetGlobalVar(784, 2);
                }
                else if ((GetGlobalVar(784) == 2))
                {
                    SetGlobalVar(784, 3);
                }
                else if ((GetGlobalVar(784) == 3))
                {
                    SetGlobalVar(784, 4);
                }
                else if ((GetGlobalVar(784) == 4))
                {
                    SetGlobalVar(784, 5);
                }
                else if ((GetGlobalVar(784) == 5))
                {
                    SetGlobalVar(784, 6);
                }
                else if ((GetGlobalVar(784) == 6))
                {
                    SetGlobalVar(784, 7);
                }
                else if ((GetGlobalVar(784) == 7))
                {
                    SetGlobalVar(784, 8);
                }

            }
            else if ((GetGlobalVar(500) == 2))
            {
                if ((GetGlobalVar(971) == 0))
                {
                    SetGlobalVar(971, 1);
                }
                else if ((GetGlobalVar(971) == 1))
                {
                    SetGlobalVar(971, 2);
                }
                else if ((GetGlobalVar(971) == 2))
                {
                    SetGlobalVar(971, 3);
                }
                else if ((GetGlobalVar(971) == 3))
                {
                    SetGlobalVar(971, 4);
                }
                else if ((GetGlobalVar(971) == 4))
                {
                    SetGlobalVar(971, 5);
                }
                else if ((GetGlobalVar(971) == 5))
                {
                    SetGlobalVar(971, 6);
                }
                else if ((GetGlobalVar(971) == 6))
                {
                    SetGlobalVar(971, 7);
                }
                else if ((GetGlobalVar(971) == 7))
                {
                    SetGlobalVar(971, 8);
                }

            }
            else if ((GetGlobalVar(500) == 3))
            {
                if ((GetGlobalVar(784) == 8 && GetGlobalVar(971) == 8))
                {
                    SetGlobalVar(784, 9);
                    SetGlobalVar(971, 9);
                }
                else if ((GetGlobalVar(784) == 9 && GetGlobalVar(971) == 9))
                {
                    SetGlobalVar(784, 10);
                    SetGlobalVar(971, 10);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(571)))
            {
                attachee.Unconceal();
                StartTimer(4000, () => cast_buff(attachee, triggerer));
                DetachScript();
            }

            return RunDefault;
        }
        public static void cast_buff(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
            return;
        }

    }
}
