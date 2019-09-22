
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
    [SpellScript(178)]
    public class FlameStrike : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnSpellEffect");
            var remove_list = new List<GameObjectBody>();

            var dam = Dice.D6;

            dam = dam.WithCount(Math.Min(15, spell.casterLevel));
            var dam_over2 = Dice.Constant(0);

            dam_over2 = dam_over2.WithModifier(dam.Roll() / 2);
            SpawnParticles("sp-Flame Strike", spell.aoeCenter);
            foreach (var target_item in spell.Targets)
            {
                if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam_over2, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    if ((!target_item.Object.HasFeat(FeatId.EVASION)) && (!target_item.Object.HasFeat(FeatId.IMPROVED_EVASION)))
                    {
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    if (!target_item.Object.HasFeat(FeatId.IMPROVED_EVASION))
                    {
                        target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
                    }
                    else
                    {
                        target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, dam_over2, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Flame Strike OnEndSpellCast");
        }


    }
}
