
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

namespace VanillaScripts.Spells
{
    [SpellScript(89)]
    public class CureCriticalWounds : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Critical Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Cure Critical Wounds OnSpellEffect");
            var dice = Dice.Parse("4d8");

            dice = dice.WithModifier(Math.Min(20, spell.casterLevel));
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

            AttachParticles("sp-Cure Critical Wounds", target);
            spell.RemoveTarget(target);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Cure Critical Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Cure Critical Wounds OnEndSpellCast");
        }


    }
}
