
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
    [ObjectScript(342)]
    public class Tarah : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(993) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(993) == 3))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(949, true);
            SetGlobalVar(993, 5);
            Sound(4112, 1);
            if ((GetGlobalFlag(948) && GetGlobalFlag(950) && GetGlobalFlag(951) && GetGlobalFlag(952) && GetGlobalFlag(953) && GetGlobalFlag(954)))
            {
                PartyLeader.AddReputation(40);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(949, false);
            PartyLeader.RemoveReputation(40);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 55))
            {
                return true;
            }

            return false;
        }
        public static bool switch_to_kenan(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8804);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool switch_to_sharar(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8806);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool switch_to_gadham(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8807);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool switch_to_abaddon(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8808);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool switch_to_gershom(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8810);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool switch_to_daniel(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8720);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_meleny(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8015);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_riana(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8058);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_fruella(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8067);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_serena(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8056);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_pishella(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8069);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool switch_to_kella(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8070);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
            }

            return SkipDefault;
        }
        public static bool pick_to_grope(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8015)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8058)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8067)))
            {
                triggerer.BeginDialog(attachee, 80);
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8056)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8069)))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8070)))
            {
                triggerer.BeginDialog(attachee, 270);
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
            }

            return SkipDefault;
        }
        public static bool create_skel(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var skel1 = GameSystems.MapObject.CreateObject(14602, new locXY(498, 575));
            skel1.Rotation = 2.5f;
            skel1.SetConcealed(true);
            skel1.Unconceal();
            AttachParticles("Trap-Spores", skel1);
            var skel2 = GameSystems.MapObject.CreateObject(14092, new locXY(500, 572));
            skel2.Rotation = 2.5f;
            skel2.SetConcealed(true);
            skel2.Unconceal();
            AttachParticles("Trap-Spores", skel2);
            var skel3 = GameSystems.MapObject.CreateObject(14092, new locXY(502, 571));
            skel3.Rotation = 2.5f;
            skel3.SetConcealed(true);
            skel3.Unconceal();
            AttachParticles("Trap-Spores", skel3);
            var skel4 = GameSystems.MapObject.CreateObject(14092, new locXY(495, 575));
            skel4.Rotation = 3f;
            skel4.SetConcealed(true);
            skel4.Unconceal();
            AttachParticles("Trap-Spores", skel4);
            var skel5 = GameSystems.MapObject.CreateObject(14092, new locXY(492, 576));
            skel5.Rotation = 3f;
            skel5.SetConcealed(true);
            skel5.Unconceal();
            AttachParticles("Trap-Spores", skel5);
            Sound(4015, 1);
            return RunDefault;
        }
        public static bool destroy_skel(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var skel = Utilities.find_npc_near(attachee, 14602);
            var abby = Utilities.find_npc_near(attachee, 8808);
            abby.TurnTowards(skel);
            AttachParticles("cast-Necromancy-cast", abby);
            AttachParticles("hit-UNHOLY-medium", skel);
            skel.SetObjectFlag(ObjectFlag.OFF);
            var zomb = Utilities.find_npc_near(attachee, 14092);
            AttachParticles("hit-UNHOLY-medium", zomb);
            zomb.SetObjectFlag(ObjectFlag.OFF);
            zomb = Utilities.find_npc_near(attachee, 14092);
            AttachParticles("hit-UNHOLY-medium", zomb);
            zomb.SetObjectFlag(ObjectFlag.OFF);
            zomb = Utilities.find_npc_near(attachee, 14092);
            AttachParticles("hit-UNHOLY-medium", zomb);
            zomb.SetObjectFlag(ObjectFlag.OFF);
            zomb = Utilities.find_npc_near(attachee, 14092);
            AttachParticles("hit-UNHOLY-medium", zomb);
            zomb.SetObjectFlag(ObjectFlag.OFF);
            Sound(4113, 1);
            return RunDefault;
        }
        // doesn't work reliably

        public static void pick_random_four(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def pick_random_four( attachee, triggerer ):	##doesn't work reliably
            var rr = RandomRange(3, 4);
            var pc = GameSystems.Party.GetPartyGroupMemberN(rr);
            pc.KillWithDeathEffect();
            AttachParticles("sp-Slay Living", pc);
            AttachParticles("ef-MinoCloud", pc);
            return;
        }
        // doesn't work reliably

        public static void pick_random_five(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def pick_random_five( attachee, triggerer ):	##doesn't work reliably
            var rr = RandomRange(3, 5);
            var pc = GameSystems.Party.GetPartyGroupMemberN(rr);
            pc.KillWithDeathEffect();
            AttachParticles("sp-Slay Living", pc);
            AttachParticles("ef-MinoCloud", pc);
            return;
        }
        // doesn't work reliably

        public static void pick_random_six(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def pick_random_six( attachee, triggerer ):	##doesn't work reliably
            var rr = RandomRange(3, 6);
            var pc = GameSystems.Party.GetPartyGroupMemberN(rr);
            pc.KillWithDeathEffect();
            AttachParticles("sp-Slay Living", pc);
            AttachParticles("ef-MinoCloud", pc);
            return;
        }
        // doesn't work reliably

        public static void pick_random_seven(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def pick_random_seven( attachee, triggerer ):	##doesn't work reliably
            var rr = RandomRange(3, 7);
            var pc = GameSystems.Party.GetPartyGroupMemberN(rr);
            pc.KillWithDeathEffect();
            AttachParticles("sp-Slay Living", pc);
            AttachParticles("ef-MinoCloud", pc);
            return;
        }
        // doesn't work reliably

        public static void pick_random_eight(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def pick_random_eight( attachee, triggerer ):	##doesn't work reliably
            var rr = RandomRange(3, 8);
            var pc = GameSystems.Party.GetPartyGroupMemberN(rr);
            pc.KillWithDeathEffect();
            AttachParticles("sp-Slay Living", pc);
            AttachParticles("ef-MinoCloud", pc);
            return;
        }
        public static int kill_pc_3(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = GameSystems.Party.GetPartyGroupMemberN(2);
            if ((pc.type == ObjectType.pc))
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Slay Living", pc);
                AttachParticles("ef-MinoCloud", pc);
                pc.ClearCritterFlag(CritterFlag.PARALYZED);
            }
            else
            {
                kill_pc_4(attachee, triggerer);
            }

            return 1;
        }
        public static int kill_pc_4(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = GameSystems.Party.GetPartyGroupMemberN(3);
            if ((pc.type == ObjectType.pc))
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Slay Living", pc);
                AttachParticles("ef-MinoCloud", pc);
                pc.ClearCritterFlag(CritterFlag.PARALYZED);
            }
            else
            {
                kill_pc_5(attachee, triggerer);
            }

            return 1;
        }
        public static int kill_pc_5(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = GameSystems.Party.GetPartyGroupMemberN(4);
            if ((pc.type == ObjectType.pc))
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Slay Living", pc);
                AttachParticles("ef-MinoCloud", pc);
                pc.ClearCritterFlag(CritterFlag.PARALYZED);
            }
            else
            {
                kill_pc_6(attachee, triggerer);
            }

            return 1;
        }
        public static int kill_pc_6(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = GameSystems.Party.GetPartyGroupMemberN(5);
            if ((pc.type == ObjectType.pc))
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Slay Living", pc);
                AttachParticles("ef-MinoCloud", pc);
                pc.ClearCritterFlag(CritterFlag.PARALYZED);
            }
            else
            {
                kill_pc_7(attachee, triggerer);
            }

            return 1;
        }
        public static int kill_pc_7(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = GameSystems.Party.GetPartyGroupMemberN(6);
            if ((pc.type == ObjectType.pc))
            {
                pc.KillWithDeathEffect();
                AttachParticles("sp-Slay Living", pc);
                AttachParticles("ef-MinoCloud", pc);
                pc.ClearCritterFlag(CritterFlag.PARALYZED);
            }
            else
            {
                kill_pc_3(attachee, triggerer);
            }

            return 1;
        }
        public static bool dom_mon(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = PartyLeader;
            AttachParticles("sp-Charm Monster", leader);
            AttachParticles("swirled gas", leader);
            AttachParticles("cast-Enchantment-cast", attachee);
            foreach (var dude in GameSystems.Party.PartyMembers)
            {
                dude.AddCondition("Paralyzed", 4, 0);
            }

            var kenan = Utilities.find_npc_near(attachee, 8804);
            var sharar = Utilities.find_npc_near(attachee, 8806);
            var gadham = Utilities.find_npc_near(attachee, 8807);
            var abaddon = Utilities.find_npc_near(attachee, 8808);
            var gershom = Utilities.find_npc_near(attachee, 8810);
            kenan.TurnTowards(attachee);
            sharar.TurnTowards(attachee);
            gadham.TurnTowards(attachee);
            abaddon.TurnTowards(attachee);
            gershom.TurnTowards(attachee);
            return RunDefault;
        }
        public static bool dom_mon_end(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var pc = PartyLeader;
            AttachParticles("Fizzle", pc);
            AttachParticles("Gaseous Swirly", pc);
            AttachParticles("Fizzle", attachee);
            return RunDefault;
        }
        public static void daniel_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var daniel = Utilities.find_npc_near(attachee, 8720);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", daniel);
            daniel.Move(new locXY(507, 587));
            daniel.TurnTowards(tarah);
            tarah.TurnTowards(daniel);
            return;
        }
        public static void kella_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var kella = Utilities.find_npc_near(attachee, 8070);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", kella);
            kella.Move(new locXY(506, 588));
            kella.TurnTowards(tarah);
            tarah.TurnTowards(kella);
            return;
        }
        public static void meleny_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var meleny = Utilities.find_npc_near(attachee, 8015);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", meleny);
            meleny.Move(new locXY(506, 588));
            meleny.TurnTowards(tarah);
            tarah.TurnTowards(meleny);
            return;
        }
        public static void fruella_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var fruella = Utilities.find_npc_near(attachee, 8067);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", fruella);
            fruella.Move(new locXY(506, 588));
            fruella.TurnTowards(tarah);
            tarah.TurnTowards(fruella);
            return;
        }
        public static void riana_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var riana = Utilities.find_npc_near(attachee, 8058);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", riana);
            riana.Move(new locXY(506, 588));
            riana.TurnTowards(tarah);
            tarah.TurnTowards(riana);
            return;
        }
        public static void serena_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var serena = Utilities.find_npc_near(attachee, 8056);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", serena);
            serena.Move(new locXY(506, 588));
            serena.TurnTowards(tarah);
            tarah.TurnTowards(serena);
            return;
        }
        public static void pishella_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var pishella = Utilities.find_npc_near(attachee, 8069);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", pishella);
            pishella.Move(new locXY(506, 588));
            pishella.TurnTowards(tarah);
            tarah.TurnTowards(pishella);
            return;
        }
        public static void pc2_see_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            var pc2 = GameSystems.Party.GetPartyGroupMemberN(1);
            AttachParticles("cast-Conjuration-cast", tarah);
            AttachParticles("sp-Dimension Door", pc2);
            pc2.Move(new locXY(506, 588));
            pc2.TurnTowards(tarah);
            tarah.TurnTowards(pc2);
            return;
        }
        public static void start_fight(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.Attack(triggerer);
            var kenan = Utilities.find_npc_near(attachee, 8804);
            var sharar = Utilities.find_npc_near(attachee, 8806);
            var gadham = Utilities.find_npc_near(attachee, 8807);
            var abaddon = Utilities.find_npc_near(attachee, 8808);
            var gershom = Utilities.find_npc_near(attachee, 8810);
            var persis = Utilities.find_npc_near(attachee, 8811);
            kenan.Attack(triggerer);
            sharar.Attack(triggerer);
            gadham.Attack(triggerer);
            abaddon.Attack(triggerer);
            gershom.Attack(triggerer);
            persis.Attack(triggerer);
            return;
        }

    }
}
