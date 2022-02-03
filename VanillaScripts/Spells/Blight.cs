
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

[SpellScript(542)]
public class Blight : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blight OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-necromancy-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Blight OnSpellEffect");
        spell.duration = 0;

        var target_item = spell.Targets[0];

        var damage_dice = Dice.D6;

        damage_dice = damage_dice.WithCount(Math.Min(spell.casterLevel, 15));
        target_item.ParticleSystem = AttachParticles("sp-Blight", target_item.Object);

        if (target_item.Object.IsMonsterCategory(MonsterCategory.plant))
        {
            if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                target_item.Object.DealSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }
            else
            {
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                target_item.Object.DealReducedSpellDamage(spell.caster, DamageType.Magic, damage_dice, D20AttackPower.UNSPECIFIED, DAMAGE_REDUCTION_HALF, D20ActionType.CAST_SPELL, spell.spellId);
            }

        }
        else
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 31012);
            AttachParticles("Fizzle", target_item.Object);
        }

        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Blight OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Blight OnEndSpellCast");
    }


}