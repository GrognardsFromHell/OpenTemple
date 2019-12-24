
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [SpellScript(93)]
    public class CureSeriousWounds : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Serious Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cure Serious Wounds OnSpellEffect");
            // Dar's level check no longer needed thanks to Spellslinger's dll fix
            // if spell.caster_class == 13: #added to check for proper paladin slot level (darmagon)
            // if spell.spell_level < 4:
            // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
            // spell.spell_end(spell.id)
            // return
            // if spell.caster_class == 14:
            // if spell.spell_level < 4:#added to check for proper ranger slot level (darmagon)
            // spell.caster.float_mesfile_line('mes\\spell.mes', 16008)
            // spell.spell_end(spell.id)
            // return
            var npc = spell.caster; // added so NPC's can use potion
            if (npc.type != ObjectType.pc && npc.GetLeader() == null && spell.casterLevel <= 0)
            {
                spell.casterLevel = 10;
            }

            var dice = Dice.Parse("3d8");
            // dice.bonus = min( 15, spell.caster.stat_level_get( spell.caster_class ) )
            dice = dice.WithModifier(Math.Min(15, spell.casterLevel));
            var target = spell.Targets[0].Object;
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

            AttachParticles("sp-Cure Serious Wounds", target);
            spell.RemoveTarget(target);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cure Serious Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Serious Wounds OnEndSpellCast");
        }

    }
}
