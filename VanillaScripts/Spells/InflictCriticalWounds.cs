
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
    [SpellScript(246)]
    public class InflictCriticalWounds : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Inflict Critical Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Inflict Critical Wounds OnSpellEffect");
            var dice = Dice.Parse("4d8");

            dice = dice.WithModifier(Math.Min(20, spell.casterLevel));
            var target = spell.Targets[0];

            if (target.Object.IsFriendly(spell.caster))
            {
                if (target.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                }
                else
                {
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target.Object.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

            }
            else
            {
                if (target.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    if (target.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target.Object.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

            }

            AttachParticles("sp-Inflict Critical Wounds", target.Object);
            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Inflict Critical Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Inflict Critical Wounds OnEndSpellCast");
        }


    }
}
