
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

[SpellScript(586)]
public class PolarRay : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Polar Ray OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Polar Ray OnSpellEffect");
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Polar Ray OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Polar Ray OnBeginProjectile");
        // spell.proj_partsys_id = game.particles( 'sp-Ray of Frost', projectile )
        SetProjectileParticles(projectile, AttachParticles("sp-Ray of Frost", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Polar Ray OnEndProjectile");
        var damage_dice = Dice.D6;
        damage_dice = damage_dice.WithCount(Math.Min(25, spell.casterLevel));
        spell.duration = 0;
        EndProjectileParticles(projectile);
        var target_item = spell.Targets[0];
            
        var return_val = spell.caster.PerformTouchAttack(target_item.Object);
        if ((return_val & D20CAF.HIT) != D20CAF.NONE)
        {
            AttachParticles("sp-Ray of Frost-Hit", target_item.Object);
            // hit
            target_item.Object.DealSpellDamage(spell.caster, DamageType.Cold, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
        }
        else
        {
            // missed
            target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
            AttachParticles("Fizzle", target_item.Object);
        }

            
        spell.RemoveTarget(target_item.Object);
        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Polar Ray OnEndSpellCast");
    }

}