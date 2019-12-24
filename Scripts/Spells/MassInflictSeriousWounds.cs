
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
    [SpellScript(582)]
    public class MassInflictSeriousWounds : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mass Inflict Serious Wounds OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            var check = Co8.check_for_protection_from_spells(spell.Targets, 0);
            Logger.Info("Mass Inflict Serious Wounds OnSpellEffect");
            var remove_list = new List<GameObjectBody>();
            var dice = Dice.Parse("3d8");
            dice = dice.WithModifier(Math.Min(35, spell.casterLevel));
            foreach (var target_item in spell.Targets)
            {
                AttachParticles("sp-Inflict Serious Wounds", target_item.Object);
                // hurt enemies, heal undead
                if (target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                {
                    // allow Fortitude saving throw for half
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        // saving throw succesful, damage target, 1/2 damage
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        target_item.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        // saving throw unsuccesful, damage target, full damage
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.HealFromSpell(spell.caster, dice, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    // check saving throw, damage target
                    if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                        // saving throw succesful, damage target, 1/2 damage
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        // saving throw unsuccesful, damage target, full damage
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.NegativeEnergy, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            if (check)
            {
                Co8.replace_protection_from_spells();
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Mass Inflict Serious Wounds OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Mass Inflict Serious Wounds OnEndSpellCast");
        }

    }
}
