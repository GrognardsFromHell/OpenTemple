
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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

namespace Scripts.Spells;

[SpellScript(513)]
public class UnholyBlight : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Unholy Blight OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Unholy Blight OnSpellEffect");
        var remove_list = new List<GameObject>();
        var damage_dice = Dice.D8;
        damage_dice = damage_dice.WithCount(Math.Min(5, spell.casterLevel / 2));
        var damage2_dice = Dice.D6;
        damage2_dice = damage2_dice.WithCount(Math.Min(10, spell.casterLevel));
        SpawnParticles("sp-Unholy Blight", spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            // roll possible sicken duration
            var sicken_dice = Dice.D4;
            spell.duration = sicken_dice.Roll();
            // check if target is EVIL
            var alignment = target_item.Object.GetAlignment();
            if (!(alignment.IsEvil()))
            {
                // check if target is GOOD or NEUTRAL
                if (((alignment.IsGood())))
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

                        // sicken the victim
                        target_item.Object.AddCondition("sp-Unholy Blight", spell.spellId, spell.duration, 0);
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
                // do NOT sicken the victim
                // target_item.obj.condition_add_with_args( 'sp-Unholy Blight', spell.id, spell.duration, 0 )
                // don't affect EVIL
                AttachParticles("Fizzle", target_item.Object);
            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Unholy Blight OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Unholy Blight OnEndSpellCast");
    }

}