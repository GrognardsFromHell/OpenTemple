
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
    [SpellScript(247)]
    public class InflictLightWounds : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Inflict Light Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Inflict Light Wounds OnSpellEffect");
            var dice = Dice.D8;

            dice = dice.WithModifier(Math.Min(5, spell.casterLevel));
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
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

            }

            AttachParticles("sp-Inflict Light Wounds", target.Object);
            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Inflict Light Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Inflict Light Wounds OnEndSpellCast");
        }


    }
}
