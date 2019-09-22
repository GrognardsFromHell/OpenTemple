
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
    [SpellScript(54)]
    public class ChaosHammer : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Chaos Hammer OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Chaos Hammer OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            var damage_dice = Dice.D8;

            if ((spell.casterLevel >= 10))
            {
                damage_dice = damage_dice.WithCount(5);
            }
            else if ((spell.casterLevel >= 8))
            {
                damage_dice = damage_dice.WithCount(4);
            }
            else if ((spell.casterLevel >= 6))
            {
                damage_dice = damage_dice.WithCount(3);
            }
            else if ((spell.casterLevel >= 4))
            {
                damage_dice = damage_dice.WithCount(2);
            }
            else
            {
                damage_dice = damage_dice.WithCount(1);
            }

            SpawnParticles("sp-Chaos Hammer", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                var stagger_dice = Dice.D6;

                spell.duration = stagger_dice.Roll();

                var alignment = target_item.Object.GetAlignment();

                if ((alignment.IsLawful()) || alignment == Alignment.NEUTRAL)
                {
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        remove_list.Add(target_item.Object);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        target_item.Object.AddCondition("sp-Chaos Hammer", spell.spellId, spell.duration, 0);
                    }

                }
                else if (!(alignment.IsLawful()) && !(alignment.IsChaotic()))
                {
                    remove_list.Add(target_item.Object);
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_QUARTER, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    AttachParticles("Fizzle", target_item.Object);
                    remove_list.Add(target_item.Object);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Chaos Hammer OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Chaos Hammer OnEndSpellCast");
        }


    }
}
