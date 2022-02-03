
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

namespace Scripts.Spells
{
    [SpellScript(101)]
    public class DeathWard : BaseSpellScript
    {
        private static readonly int SPELL_OBJ = 6400;
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Death Ward OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public static void do_particles(SpellTarget target)
        {
            var part_str = "sp-Death Ward";
            if (GameSystems.Stat.DispatchGetSizeCategory(target.Object) <= SizeCategory.Medium)
            {
                part_str = "sp-Death Ward-MED";
            }
            else if (GameSystems.Stat.DispatchGetSizeCategory(target.Object) == SizeCategory.Large)
            {
                part_str = "sp-Death Ward-LARGE";
            }
            else
            {
                part_str = "sp-Death Ward-HUGE";
            }

            target.ParticleSystem = AttachParticles(part_str, target.Object);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Death Ward OnSpellEffect");
            // Dar's level check no longer needed thanks to Spellslinger's dll fix
            // if spell.caster_class == 13: #added to check for proper paladin slot level (darmagon)
            // if spell.spell_level < 4:
            // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
            // return
            spell.duration = 10 * spell.casterLevel;
            var npc = spell.caster; // added so NPC's can pre-buff
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && !GameSystems.Combat.IsCombatActive())
            {
                spell.duration = 2000 * spell.casterLevel;
            }

            var target = spell.Targets[0];
            if (Co8.find_spell_obj_with_flag(target.Object, SPELL_OBJ, Co8SpellFlag.DeathWard) == null)
            {
                var spell_obj = GameSystems.MapObject.CreateObject(SPELL_OBJ, target.Object.GetLocation());
                spell_obj.SetItemFlag(ItemFlag.NO_DROP);
                spell_obj.SetItemFlag(ItemFlag.NO_LOOT);
                Co8.set_spell_flag(spell_obj, Co8SpellFlag.DeathWard);
                spell_obj.AddConditionToItem("Monster Energy Immunity", (int) DamageType.NegativeEnergy, 0);
                target.Object.GetItem(spell_obj);
                target.Object.AddCondition("sp-Death Ward", spell.spellId, spell.duration, 0);
                do_particles(target);
                // This is evil, but we need to remember who the spell was
                // cast on, and the target_list gets blown away.
                spell.caster = target.Object;
            }
            else
            {
                target.Object.FloatMesFileLine("mes/spell.mes", 16007);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
                spell.EndSpell();
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Death Ward OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Death Ward OnEndSpellCast");
            Co8.destroy_spell_obj_with_flag(spell.caster, SPELL_OBJ, Co8SpellFlag.DeathWard);
        }

    }
}
