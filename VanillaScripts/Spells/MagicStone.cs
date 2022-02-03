
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

[SpellScript(290)]
public class MagicStone : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Magic Stone OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-transmutation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Magic Stone OnSpellEffect");
        AttachParticles("sp-Magic Stone", spell.caster);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Magic Stone OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Magic Stone OnBeginProjectile");
        SetProjectileParticles(projectile, AttachParticles("sp-Melfs Acid Arrow Projectile", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Magic Stone OnEndProjectile");
        EndProjectileParticles(projectile);
        var dice = Dice.D6;

        dice = dice.WithModifier(1);
        var target = spell.Targets[0];

        var attack_successful = spell.caster.PerformTouchAttack(target.Object);

        if (attack_successful == D20CAF.HIT)
        {
            AttachParticles("Fizzle", target.Object);
            if (target.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                dice = dice.WithCount(2);
                dice = dice.WithModifier(2);
                target.Object.DealSpellDamage(spell.caster, DamageType.Force, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }
            else
            {
                target.Object.DealSpellDamage(spell.caster, DamageType.Force, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }

        }
        else if (attack_successful == D20CAF.CRITICAL)
        {
            AttachParticles("Fizzle", target.Object);
            dice = dice.WithCount(2);
            if (target.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                dice = dice.WithCount(4);
                dice = dice.WithModifier(4);
                target.Object.DealSpellDamage(spell.caster, DamageType.Magic, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }
            else
            {
                target.Object.DealSpellDamage(spell.caster, DamageType.Magic, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            }

        }
        else
        {
            target.Object.FloatMesFileLine("mes/spell.mes", 30007);
            AttachParticles("Fizzle", target.Object);
        }

        spell.RemoveTarget(target.Object);
        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Magic Stone OnEndSpellCast");
    }


}