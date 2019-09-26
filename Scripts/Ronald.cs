
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
    [ObjectScript(287)]
    public class Ronald : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var st = attachee.GetInt(obj_f.npc_pad_i_5);
            attachee.TurnTowards(triggerer);
            if ((attachee.GetMap() == 5013))
            {
                attachee.FloatLine(23000, triggerer);
            }
            else if ((GetGlobalVar(915) == 32))
            {
                triggerer.BeginDialog(attachee, 570); // have attacked 3 or more farm animals with ronald in party
            }
            else if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                if (GetGlobalVar(692) == 2 && GetGlobalVar(693) == 3 && (attachee.GetLeader() == null))
                {
                    attachee.FloatLine(600, triggerer); // not in party, Gruumsh
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 1 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 260); // not in party, Lareth
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 3 && (GetGlobalVar(693) == 1 || GetGlobalVar(692) == 2) && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 90); // not in party, orcs present after issues
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 3 && GetGlobalVar(693) == 0 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 355); // not in party, orcs gone after issues
                    return SkipDefault;
                }
                else if ((GetGlobalVar(692) >= 5 && GetGlobalVar(692) <= 7) && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 100); // return with money
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 0 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 200); // return with no grudges
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 2 && GetGlobalVar(693) == 0 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 355); // not in party, returning after Gruumsh, Gruumsh gone
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 8 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 900); // not in party, left because party didn't pay for resurrection - not done yet!!!!
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 9 && (attachee.GetLeader() == null))
                {
                    attachee.FloatLine(600, triggerer); // left because hates party's guts, won't talk
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 10 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 2150); // done, left to talk to Dad
                    return SkipDefault;
                }
                else if (GetGlobalVar(692) == 11 && (attachee.GetLeader() == null))
                {
                    triggerer.BeginDialog(attachee, 2200); // chatted with Dad, ready to go
                    return SkipDefault;
                }
                else if ((attachee.GetLeader() != null && st == 1))
                {
                    triggerer.BeginDialog(attachee, 2060); // in party, wanting to resurrect
                    return SkipDefault;
                }
                else if ((attachee.GetLeader() != null && st == 0))
                {
                    triggerer.BeginDialog(attachee, 120); // in party, not resurrecting yet
                    return SkipDefault;
                }
                else if ((attachee.GetLeader() != null && st == 2))
                {
                    triggerer.BeginDialog(attachee, 2250); // back in party, post parents
                    return SkipDefault;
                }
                else
                {
                    attachee.FloatLine(600, triggerer); // anything else he just won't talk
                    return SkipDefault;
                }

            }
            else
            {
                if ((GetGlobalVar(5) > 7) && !triggerer.HasReputation(2))
                {
                    attachee.FloatLine(600, triggerer);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1); // none of the above
                }

            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(500)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetLeader() == null))
                {
                    if ((attachee.GetMap() == 5011))
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
                    else if ((attachee.GetMap() == 5013))
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

                var finale = attachee.GetInt(obj_f.npc_pad_i_5);
                if (((PartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8025))) || (PartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8026))))) // Tuelk and Pintark
                {
                    SetGlobalVar(693, 2);
                }

                var half_orc = 0;
                var Grummshite = 0;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.GetRace() == RaceId.half_orc)
                    {
                        half_orc = 1;
                    }

                    if ((pc.GetDeity() == 7 && pc.GetStat(Stat.level_cleric) >= 1))
                    {
                        Grummshite = 1;
                    }

                }

                if (half_orc == 1)
                {
                    SetGlobalVar(693, 1);
                }

                if (Grummshite == 1)
                {
                    SetGlobalVar(693, 3);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((!GetGlobalFlag(237)))
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
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalVar(915) >= 3))
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
        public static bool san_taking_damage(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.FloatLine(12054, triggerer);
            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(237, true);
            var itemD = attachee.FindItemByName(4068);
            if ((itemD != null))
            {
                itemD.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            var itemE = attachee.FindItemByName(4087);
            if ((itemE != null))
            {
                itemE.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            var itemA = attachee.FindItemByName(6056);
            if ((itemA != null))
            {
                itemA.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            var itemB = attachee.FindItemByName(6070);
            if ((itemB != null))
            {
                itemB.SetItemFlag(ItemFlag.NO_TRANSFER);
            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(237, false);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        // after moathouse, sets to 283

        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def san_new_map( attachee, triggerer ):		# after moathouse, sets to 283
            if ((attachee.GetMap() == 5004))
            {
                attachee.FloatLine(650, triggerer);
                ReplaceCurrentScript(283);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.CatsGrace || spell.spellEnum == WellKnownSpells.Invisibility))
            {
                attachee.FloatLine(630, triggerer);
            }
            else if ((spell.spellEnum == WellKnownSpells.Haste || spell.spellEnum == WellKnownSpells.GoodHope))
            {
                attachee.FloatLine(630, triggerer);
            }
            else if ((spell.spellEnum == WellKnownSpells.Stoneskin || spell.spellEnum == WellKnownSpells.Barkskin))
            {
                attachee.FloatLine(630, triggerer);
            }
            else if ((spell.spellEnum == WellKnownSpells.Blur || spell.spellEnum == WellKnownSpells.FoxsCunning))
            {
                attachee.FloatLine(630, triggerer);
            }
            else if ((spell.spellEnum == WellKnownSpells.Heroism || spell.spellEnum == WellKnownSpells.MageArmor))
            {
                attachee.FloatLine(630, triggerer);
            }
            else if ((spell.spellEnum == WellKnownSpells.MirrorImage || spell.spellEnum == WellKnownSpells.ProtectionFromArrows))
            {
                attachee.FloatLine(630, triggerer);
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
            }

            attachee.RunOff();
            return RunDefault;
        }
        public static bool argue_lareth(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8002);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
                triggerer.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 270);
            }

            return SkipDefault;
        }
        public static bool war(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var hedrack = Utilities.find_npc_near(attachee, 8032);
            var iuz = Utilities.find_npc_near(attachee, 8042);
            if ((hedrack != null))
            {
                hedrack.Attack(triggerer);
            }
            else if ((iuz != null))
            {
                iuz.Attack(triggerer);
            }

            return SkipDefault;
        }
        public static bool switch_to_cuthbert(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var cuthbert = Utilities.find_npc_near(attachee, 8043);
            if ((cuthbert != null))
            {
                triggerer.BeginDialog(cuthbert, line);
            }
            else
            {
                cuthbert.SetObjectFlag(ObjectFlag.OFF);
            }

            return SkipDefault;
        }
        public static bool argue_tuelk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8026);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(triggerer);
                triggerer.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 175);
            }

            return SkipDefault;
        }
        public static bool reset_st(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetInt(obj_f.npc_pad_i_5, 1);
            return SkipDefault;
        }
        public static bool summon_parents(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var willi = GameSystems.MapObject.CreateObject(14676, attachee.GetLocation().OffsetTiles(-3, 0));
            AttachParticles("sp-Raise Dead", willi);
            willi.TurnTowards(attachee);
            var ivy = GameSystems.MapObject.CreateObject(14675, attachee.GetLocation().OffsetTiles(-3, 0));
            AttachParticles("sp-Raise Dead", ivy);
            ivy.TurnTowards(attachee);
            attachee.TurnTowards(willi);
            return SkipDefault;
        }
        public static bool argue_willi(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var willi = Utilities.find_npc_near(attachee, 14676);
            if ((willi != null))
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(willi, line);
                willi.TurnTowards(triggerer);
                triggerer.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 175);
            }

            return SkipDefault;
        }
        public static bool done(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(692, 10);
            attachee.SetInt(obj_f.npc_pad_i_5, 2);
            return SkipDefault;
        }
        public static bool reunion(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var willi = Utilities.find_npc_near(attachee, 14676);
            willi.SetCritterFlag(CritterFlag.MUTE);
            StartTimer(88000000, () => rejoin(attachee, triggerer));
            attachee.SetCritterFlag(CritterFlag.MUTE);
            return SkipDefault;
        }
        public static bool rejoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.ClearCritterFlag(CritterFlag.MUTE);
            var willi = Utilities.find_npc_near(attachee, 14676);
            willi.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            var ivy = Utilities.find_npc_near(attachee, 14675);
            ivy.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            SetGlobalVar(692, 11);
            return SkipDefault;
        }
        public static bool equip_all(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            var itemB = attachee.FindItemByName(6070);
            if ((itemB != null))
            {
                itemB.Destroy();
                Utilities.create_item_in_inventory(6070, triggerer);
            }

            var itemD = attachee.FindItemByName(4068);
            if ((itemD != null))
            {
                itemD.Destroy();
                Utilities.create_item_in_inventory(4068, triggerer);
            }

            var itemE = attachee.FindItemByName(4087);
            if ((itemE != null))
            {
                itemE.Destroy();
                Utilities.create_item_in_inventory(4087, triggerer);
            }

            var itemA = attachee.FindItemByName(6056);
            if ((itemA != null))
            {
                itemA.Destroy();
                Utilities.create_item_in_inventory(6056, triggerer);
            }

            return RunDefault;
        }
        public static bool equip_leather(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var itemC = attachee.FindItemByName(6056);
            if ((itemC != null))
            {
                itemC.Destroy();
                Utilities.create_item_in_inventory(6056, triggerer);
            }

            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            return RunDefault;
        }
        public static bool equip_rest(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Utilities.create_item_in_inventory(7002, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            Utilities.create_item_in_inventory(7001, attachee);
            var itemB = attachee.FindItemByName(6070);
            if ((itemB != null))
            {
                itemB.Destroy();
                Utilities.create_item_in_inventory(6070, triggerer);
            }

            var itemD = attachee.FindItemByName(4068);
            if ((itemD != null))
            {
                itemD.Destroy();
                Utilities.create_item_in_inventory(4068, triggerer);
            }

            var itemE = attachee.FindItemByName(4087);
            if ((itemE != null))
            {
                itemE.Destroy();
                Utilities.create_item_in_inventory(4087, triggerer);
            }

            return RunDefault;
        }

    }
}
