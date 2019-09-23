
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
    [ObjectScript(41)]
    public class AnimalCompanion : BaseObjectScript
    {
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null && !ScriptDaemon.npc_get(attachee, 2)))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_pc_percent_hp(attachee, leader) <= 40))
                {
                    attachee.SetInt(obj_f.critter_strategy, 462);
                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 8))
                {
                    for (var pp = 0; pp < 8; pp++)
                    {
                        if ((GameSystems.Party.GetPartyGroupMemberN(pp) != null))
                        {
                            if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(pp)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(pp).GetStat(Stat.hp_current) >= -9))
                            {
                                SetGlobalFlag(250 + pp, true);
                                SetGlobalFlag(258, true);
                            }

                        }

                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(253)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(3))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(254)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(4))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(255)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(5))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(256)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(6))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(257)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(7))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 7))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(2)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(252, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(3)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(253, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(4)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(254, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(5)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(5).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(255, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(6)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(6).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(256, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(253)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(3))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(254)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(4))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(255)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(5))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(256)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(6))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 6))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(2)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(252, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(3)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(253, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(4)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(254, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(5)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(5).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(255, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(253)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(3))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(254)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(4))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(255)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(5))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 5))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(2)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(252, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(3)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(253, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(4)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(254, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(253)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(3))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(254)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(4))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 4))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(2)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(252, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(3)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(253, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(253)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(3))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 3))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(2)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(252, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(252)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(2))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.NPCFollowersSize + GameSystems.Party.PlayerCharactersSize == 2))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((Utilities.obj_percent_hp(GameSystems.Party.GetPartyGroupMemberN(1)) <= 50 && GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(251, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(251)))
                    {
                        if ((adjacent(attachee, GameSystems.Party.GetPartyGroupMemberN(1))))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }
                else if ((GameSystems.Party.PlayerCharactersSize == 1))
                {
                    if ((Utilities.obj_percent_hp(PartyLeader) <= 50 && PartyLeader.GetStat(Stat.hp_current) >= -9))
                    {
                        SetGlobalFlag(250, true);
                        SetGlobalFlag(258, true);
                    }

                    if ((GetGlobalFlag(250)))
                    {
                        if ((adjacent(attachee, PartyLeader)))
                        {
                            SetGlobalFlag(259, true);
                        }

                    }

                    if ((GetGlobalFlag(258)))
                    {
                        if ((GetGlobalFlag(259)))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 464);
                            if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                            {
                                attachee.TurnTowards(triggerer);
                            }
                            else
                            {
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                    {
                                        attachee.TurnTowards(pc);
                                    }
                                    else
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                    }

                                }

                            }

                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 463);
                        }

                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 464);
                        if ((triggerer.type == ObjectType.npc && triggerer.GetLeader() == null))
                        {
                            attachee.TurnTowards(triggerer);
                        }
                        else
                        {
                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
                                {
                                    attachee.TurnTowards(pc);
                                }
                                else
                                {
                                    attachee.TurnTowards(PartyLeader);
                                }

                            }

                        }

                    }

                }

            }

            SetGlobalFlag(250, false);
            SetGlobalFlag(251, false);
            SetGlobalFlag(252, false);
            SetGlobalFlag(253, false);
            SetGlobalFlag(254, false);
            SetGlobalFlag(255, false);
            SetGlobalFlag(256, false);
            SetGlobalFlag(257, false);
            SetGlobalFlag(258, false);
            SetGlobalFlag(259, false);
            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((ScriptDaemon.npc_get(attachee, 1)))
            {
                ScriptDaemon.npc_set(attachee, 2);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.CharmPersonOrAnimal || spell.spellEnum == WellKnownSpells.CharmMonster))
            {
                ScriptDaemon.npc_set(attachee, 1);
            }

            return RunDefault;
        }
        public static bool not_adjacent(GameObjectBody companion, GameObjectBody target)
        {
            if ((companion.DistanceTo(target) >= 5))
            {
                return true;
            }

            return false;
        }
        public static bool adjacent(GameObjectBody companion, GameObjectBody target)
        {
            if ((companion.DistanceTo(target) <= 5))
            {
                return true;
            }

            return false;
        }

    }
}
