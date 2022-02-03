
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells;

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
        var remove_list = new List<GameObject>();

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