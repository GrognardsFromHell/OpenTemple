
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

namespace Scripts
{
    [ObjectScript(287)]
    public class Ronald : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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

                var half_orc = false;
                var Grummshite = false;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.GetRace() == RaceId.half_orc)
                    {
                        half_orc = true;
                    }

                    if ((pc.GetDeity() == DeityId.GRUUMSH && pc.GetStat(Stat.level_cleric) >= 1))
                    {
                        Grummshite = true;
                    }

                }

                if (half_orc)
                {
                    SetGlobalVar(693, 1);
                }

                if (Grummshite)
                {
                    SetGlobalVar(693, 3);
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
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            attachee.FloatLine(12057, triggerer);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
        public static bool san_taking_damage(GameObject attachee, GameObject triggerer)
        {
            attachee.FloatLine(12054, triggerer);
            return RunDefault;
        }
        public override bool OnJoin(GameObject attachee, GameObject triggerer)
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
        public override bool OnDisband(GameObject attachee, GameObject triggerer)
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

        public override bool OnNewMap(GameObject attachee, GameObject triggerer)
        {
            // def san_new_map( attachee, triggerer ):		# after moathouse, sets to 283
            if ((attachee.GetMap() == 5004))
            {
                attachee.FloatLine(650, triggerer);
                ReplaceCurrentScript(283);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
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
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
            }

            attachee.RunOff();
            return RunDefault;
        }
        public static bool argue_lareth(GameObject attachee, GameObject triggerer, int line)
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
        public static bool war(GameObject attachee, GameObject triggerer)
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
        public static bool switch_to_cuthbert(GameObject attachee, GameObject triggerer, int line)
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
        public static bool argue_tuelk(GameObject attachee, GameObject triggerer, int line)
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
        public static bool reset_st(GameObject attachee, GameObject triggerer)
        {
            attachee.SetInt(obj_f.npc_pad_i_5, 1);
            return SkipDefault;
        }
        public static bool summon_parents(GameObject attachee, GameObject triggerer)
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
        public static bool argue_willi(GameObject attachee, GameObject triggerer, int line)
        {
            var willi = Utilities.find_npc_near(attachee, 14676);
            if ((willi != null))
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(willi, line);
                willi.TurnTowards(triggerer);
                triggerer.TurnTowards(willi);
            }
            else
            {
                triggerer.BeginDialog(attachee, 175);
            }

            return SkipDefault;
        }
        public static bool done(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(692, 10);
            attachee.SetInt(obj_f.npc_pad_i_5, 2);
            return SkipDefault;
        }
        public static bool reunion(GameObject attachee, GameObject triggerer)
        {
            var willi = Utilities.find_npc_near(attachee, 14676);
            willi.SetCritterFlag(CritterFlag.MUTE);
            StartTimer(88000000, () => rejoin(attachee, triggerer));
            attachee.SetCritterFlag(CritterFlag.MUTE);
            return SkipDefault;
        }
        public static bool rejoin(GameObject attachee, GameObject triggerer)
        {
            attachee.ClearCritterFlag(CritterFlag.MUTE);
            var willi = Utilities.find_npc_near(attachee, 14676);
            willi.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            var ivy = Utilities.find_npc_near(attachee, 14675);
            ivy.RunOff(attachee.GetLocation().OffsetTiles(-3, 0));
            SetGlobalVar(692, 11);
            return SkipDefault;
        }
        public static bool equip_all(GameObject attachee, GameObject triggerer)
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
        public static bool equip_leather(GameObject attachee, GameObject triggerer)
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
        public static bool equip_rest(GameObject attachee, GameObject triggerer)
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
