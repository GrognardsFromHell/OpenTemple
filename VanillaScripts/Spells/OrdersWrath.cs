
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
    [SpellScript(335)]
    public class OrdersWrath : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Order's Wrath OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Order's Wrath OnSpellEffect");
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

            spell.duration = 1;

            SpawnParticles("sp-Orders Wrath", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                var alignment = target_item.Object.GetAlignment();

                if ((alignment.IsChaotic()) || (((alignment.IsChaotic())) && !(alignment.IsLawful())))
                {
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Reflex, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        remove_list.Add(target_item.Object);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                        target_item.Object.AddCondition("sp-Orders Wrath", spell.spellId, spell.duration, 0);
                    }

                }
                else if (!(alignment.IsLawful()) && !(alignment.IsChaotic()))
                {
                    remove_list.Add(target_item.Object);
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Reflex, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
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
            Logger.Info("Order's Wrath OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Order's Wrath OnEndSpellCast");
        }


    }
}
