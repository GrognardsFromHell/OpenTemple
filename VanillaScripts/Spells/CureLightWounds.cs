
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(90)]
    public class CureLightWounds : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Light Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cure Light Wounds OnSpellEffect");
            var dice = Dice.D8;

            dice = dice.WithModifier(Math.Min(5, spell.casterLevel));
            var target = spell.Targets[0].Object;

            if (target.IsFriendly(spell.caster))
            {
                if (target.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.FloatMesFileLine("mes/spell.mes", 30001);
                        target.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.FloatMesFileLine("mes/spell.mes", 30002);
                        target.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                }

            }
            else
            {
                if (target.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.FloatMesFileLine("mes/spell.mes", 30001);
                        target.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.FloatMesFileLine("mes/spell.mes", 30002);
                        target.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    if (target.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                        target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                        target.HealSubdual(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

            }

            AttachParticles("sp-Cure Light Wounds", target);
            spell.RemoveTarget(target);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cure Light Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Light Wounds OnEndSpellCast");
        }


    }
}
