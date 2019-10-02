
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
    [SpellScript(579)]
    public class MassCureCriticalWounds : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dice = Dice.Parse("4d8");
            // dice.bonus = min( 40, spell.caster.stat_level_get( spell.caster_class ) )
            dice = dice.WithModifier(Math.Min(40, spell.casterLevel));
            foreach (var target_item in spell.Targets)
            {
                var target = target_item.Object;
                // check if target is friendly (willing target)
                if (target.IsFriendly(spell.caster))
                {
                    // check if target is undead
                    if (target.IsMonsterCategory(MonsterCategory.undead))
                    {
                        // check saving throw, damage target
                        if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            target.FloatMesFileLine("mes/spell.mes", 30001);
                            // saving throw succesful, damage target, 1/2 damage
                            target.DealReducedSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        }
                        else
                        {
                            target.FloatMesFileLine("mes/spell.mes", 30002);
                            // saving throw unsuccesful, damage target, full damage
                            target.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        }

                    }
                    else
                    {
                        // heal target
                        target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                        target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    // check if target is undead
                    if (target.IsMonsterCategory(MonsterCategory.undead))
                    {
                        // check saving throw, damage target
                        if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            target.FloatMesFileLine("mes/spell.mes", 30001);
                            // saving throw succesful, damage target, 1/2 damage
                            target.DealReducedSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        }
                        else
                        {
                            target.FloatMesFileLine("mes/spell.mes", 30002);
                            // saving throw unsuccesful, damage target, full damage
                            target.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        }

                    }
                    else
                    {
                        // check saving throw
                        if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // target.float_mesfile_line( 'mes\\spell.mes', 30001 )
                            // saving throw succesful, heal target, 1/2 heal
                            target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                            target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                        }
                        else
                        {
                            // target.float_mesfile_line( 'mes\\spell.mes', 30002 )
                            // saving throw unsuccesful, heal target, full heal
                            target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                            target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                        }

                    }

                }

                AttachParticles("sp-Cure Critical Wounds", target);
                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Healing Circle OnEndSpellCast");
        }

    }
}