
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

[SpellScript(754)]
public class UnholyBlightQuickened : BaseSpellScript
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
        var npc = spell.caster;
        if (npc.GetNameId() == 14958) // Nightwalker
        {
            spell.dc = 18;
            spell.casterLevel = 21;
        }

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

        SpawnParticles("sp-Unholy Blight", spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            // roll possible stagger duration
            var stagger_dice = Dice.D4;
            spell.duration = stagger_dice.Roll();
            // check if target is GOOD
            var alignment = target_item.Object.GetAlignment();
            if ((alignment.IsGood()) || (((alignment & Alignment.NEUTRAL)) != 0 && !(alignment.IsEvil())))
            {
                // allow Fortitude saving throw for half
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw succesful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    // half damage
                    target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    remove_list.Add(target_item.Object);
                }
                else
                {
                    // saving throw unsuccesful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // full damage
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    // stagger the victim
                    target_item.Object.AddCondition("sp-Unholy Blight", spell.spellId, spell.duration, 0);
                }

            }
            else if (!(alignment.IsGood()) && !(alignment.IsEvil()))
            {
                remove_list.Add(target_item.Object);
                // allow Fortitude saving throw for quarter
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
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
            else
            {
                // do NOT sicken the victim
                // target_item.obj.condition_add_with_args( 'sp-Unholy Blight', spell.id, spell.duration, 0 )
                // don't affect EVIL
                AttachParticles("Fizzle", target_item.Object);
                remove_list.Add(target_item.Object);
            }

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