
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

[SpellScript(803)]
public class SnowballSwarm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Snowball Swarm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Snowball Swarm OnSpellEffect");
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Snowball Swarm OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Snowball Swarm OnBeginProjectile");
        // spell.proj_partsys_id = game.particles( 'sp-Snowball Swarm-proj', projectile )
        SetProjectileParticles(projectile, AttachParticles("sp-Snowball Swarm-proj", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Snowball Swarm OnEndProjectile");
        var remove_list = new List<GameObject>();
        spell.duration = 0;
        var dam = Dice.D6;
        // calculate dice rolled
        if ((spell.casterLevel >= 3) && (spell.casterLevel <= 4))
        {
            dam = dam.WithCount(2);
        }
        else if ((spell.casterLevel >= 5) && (spell.casterLevel <= 6))
        {
            dam = dam.WithCount(3);
        }
        else if ((spell.casterLevel >= 7) && (spell.casterLevel <= 8))
        {
            dam = dam.WithCount(4);
        }
        else if ((spell.casterLevel >= 9) && (spell.casterLevel <= 20))
        {
            dam = dam.WithCount(5);
        }

        EndProjectileParticles(projectile);
        SpawnParticles("sp-Snowball Swarm-Hit", spell.aoeCenter);
        foreach (var target_item in spell.Targets)
        {
            if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Cold, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
            {
                // saving throw successful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                // saving throw unsuccessful
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
            }

            remove_list.Add(target_item.Object);
        }

        spell.RemoveTargets(remove_list);
        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Snowball Swarm OnEndSpellCast");
    }

}