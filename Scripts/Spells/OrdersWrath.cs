
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
            damage_dice = damage_dice.WithCount(Math.Min(5, spell.casterLevel / 2));
            var damage2_dice = Dice.D6;
            damage2_dice = damage2_dice.WithCount(Math.Min(10, spell.casterLevel));
            spell.duration = 1;
            SpawnParticles("sp-Orders Wrath", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                // check if target is LAWFUL
                var alignment = target_item.Object.GetAlignment();
                if (!(alignment.IsLawful()))
                {
                    // check if target is CHAOTIC or NEUTRAL
                    if (((alignment.IsChaotic())))
                    {
                        // allow Will saving throw for half
                        if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // saving throw succesful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            // half damage
                            // check for outsiders
                            if (target_item.Object.IsMonsterCategory(MonsterCategory.outsider))
                            {
                                target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage2_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                            }
                            else
                            {
                                target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                            }

                        }
                        else
                        {
                            // saving throw unsuccesful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            // full damage
                            // check for outsiders
                            if (target_item.Object.IsMonsterCategory(MonsterCategory.outsider))
                            {
                                target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage2_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                            }
                            else
                            {
                                target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                            }

                            // daze the victim
                            target_item.Object.AddCondition("sp-Orders Wrath", spell.spellId, spell.duration, 0);
                        }

                    }
                    else
                    {
                        // allow Will saving throw for quarter
                        if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // saving throw succesful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            // quarter damage
                            target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_QUARTER, D20ActionType.CAST_SPELL, spell.spellId);
                        }
                        else
                        {
                            // saving throw unsuccesful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                            // half damage
                            target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                        }

                    }

                }
                else
                {
                    // do NOT daze the victim
                    // target_item.obj.condition_add_with_args( 'sp-Orders Wrath', spell.id, spell.duration, 0 )
                    // don't affect LAWFUL
                    AttachParticles("Fizzle", target_item.Object);
                }

                remove_list.Add(target_item.Object);
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
