
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

namespace Scripts.Spells
{
    [SpellScript(432)]
    public class Shout : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shout OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Shout OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var damage_dice = Dice.Parse("5d6");
            var duration_dice = Dice.Parse("2d6");
            spell.duration = duration_dice.Roll();
            var earth_dam = Dice.D6;
            earth_dam = earth_dam.WithCount(Math.Min(15, spell.casterLevel));
            AttachParticles("sp-Shout", spell.caster);
            // get all targets in a 25ft + 2ft/level cone (60')
            var npc = spell.caster; // added so NPC's can target Shout
                                    // Caster is NOT in game party
            if (npc.type != ObjectType.pc && npc.GetLeader() == null)
            {
                // range = 25 + 2 * int(spell.caster_level/2)
                var range = 30;
                var target_list = ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, range, -30, 90);
                foreach (var obj in target_list)
                {
                    if (obj == spell.caster)
                    {
                        continue;
                    }

                    if (!obj.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw unsuccessful
                        obj.DealSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        obj.AddCondition("sp-Shout", spell.spellId, spell.duration, 0);
                        // obj.partsys_id = game.particles( 'sp-Shout-Hit', obj )
                        AttachParticles("sp-Shout-Hit", obj);
                    }
                    else
                    {
                        // saving throw successful, apply half damage
                        obj.FloatMesFileLine("mes/spell.mes", 30001);
                        obj.DealReducedSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        // obj.partsys_id = game.particles( 'sp-Shout-Hit', obj )
                        AttachParticles("sp-Shout-Hit", obj);
                        AttachParticles("Fizzle", obj);
                    }

                }

            }

            // Caster is in game party
            if (npc.type == ObjectType.pc || npc.GetLeader() != null)
            {
                foreach (var target_item in spell.Targets)
                {
                    if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw unsuccessful
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        target_item.Object.AddCondition("sp-Shout", spell.spellId, spell.duration, 0);
                        target_item.ParticleSystem = AttachParticles("sp-Shout-Hit", target_item.Object);
                    }
                    else
                    {
                        // saving throw successful, apply half damage
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Sonic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        target_item.ParticleSystem = AttachParticles("sp-Shout-Hit", target_item.Object);
                        AttachParticles("Fizzle", target_item.Object);
                        remove_list.Add(target_item.Object);
                    }

                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Shout OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Shout OnEndSpellCast");
        }

    }
}
