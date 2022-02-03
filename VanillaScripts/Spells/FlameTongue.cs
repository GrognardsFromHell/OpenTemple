
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

[SpellScript(713)]
public class FlameTongue : BaseSpellScript
{

    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Flame tongue OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Flame tongue OnSpellEffect");
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Flame tongue OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Flame tongue OnBeginProjectile");
        SetProjectileParticles(projectile, AttachParticles("sp-Flame Tongue-proj", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Flame tongue OnEndProjectile");
        var damage_dice = Dice.Parse("4d6");

        spell.duration = 0;

        EndProjectileParticles(projectile);
        var target_item = spell.Targets[0];

        var return_val = spell.caster.PerformTouchAttack(target_item.Object);

        if (return_val == D20CAF.HIT)
        {
            AttachParticles("sp-Flame Tongue-Hit", target_item.Object);
            target_item.Object.DealSpellDamage(spell.caster, DamageType.Fire, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
        }
        else if (return_val == D20CAF.CRITICAL)
        {
            AttachParticles("sp-Flame Tongue-Hit", target_item.Object);
            damage_dice = damage_dice.WithCount(8);
            target_item.Object.DealSpellDamage(spell.caster, DamageType.Fire, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
        }
        else
        {
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
            AttachParticles("Fizzle", target_item.Object);
        }

        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Flame tongue OnEndSpellCast");
    }


}