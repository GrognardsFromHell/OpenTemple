
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [SpellScript(412)]
    public class SearingLight : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Searing Light OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Searing Light OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Searing Light OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Searing Light OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Searing Light", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Searing Light OnEndProjectile");
            var damage_dice = Dice.D8;

            damage_dice = damage_dice.WithCount(Math.Min(5, spell.casterLevel / 2));
            spell.duration = 0;

            EndProjectileParticles(projectile);
            var target_item = spell.Targets[0];

            var attack_successful = spell.caster.PerformTouchAttack(target_item.Object);

            if (attack_successful == D20CAF.HIT)
            {
                AttachParticles("sp-Searing Light-Hit", target_item.Object);
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.undead)))
                {
                    damage_dice = damage_dice.WithSides(6);
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    if ((target_item.Object.IsMonsterCategory(MonsterCategory.construct)))
                    {
                        damage_dice = damage_dice.WithSides(6);
                    }

                    target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }
            else if (attack_successful == D20CAF.CRITICAL)
            {
                AttachParticles("sp-Searing Light-Hit", target_item.Object);
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.undead)))
                {
                    damage_dice = damage_dice.WithSides(6);
                    damage_dice = damage_dice.WithCount(Math.Min(10, spell.casterLevel) * 2);
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    damage_dice = damage_dice.WithCount(damage_dice.Count * 2);
                    if ((target_item.Object.IsMonsterCategory(MonsterCategory.construct)))
                    {
                        damage_dice = damage_dice.WithSides(6);
                    }

                    target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Searing Light OnEndSpellCast");
        }


    }
}
