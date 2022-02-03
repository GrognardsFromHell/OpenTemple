
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

[SpellScript(304)]
public class MelfSAcidArrow : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Melf's Acid Arrow OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-conjuration-conjure", spell.caster);
    }
    // spell.variables = [0,0]

    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Melf's Acid Arrow OnSpellEffect");
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Melf's Acid Arrow OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Melf's Acid Arrow OnBeginProjectile");
        SetProjectileParticles(projectile, AttachParticles("sp-Melfs Acid Arrow Projectile", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Melf's Acid Arrow OnEndProjectile");
        EndProjectileParticles(projectile);
        // calculate spell.duration
        if ((spell.casterLevel >= 3) && (spell.casterLevel <= 5))
        {
            spell.duration = 2 - 1;
        }
        else if ((spell.casterLevel >= 6) && (spell.casterLevel <= 8))
        {
            spell.duration = 3 - 1;
        }
        else if ((spell.casterLevel >= 9) && (spell.casterLevel <= 11))
        {
            spell.duration = 4 - 1;
        }
        else if ((spell.casterLevel >= 12) && (spell.casterLevel <= 14))
        {
            spell.duration = 5 - 1;
        }
        else if ((spell.casterLevel >= 15) && (spell.casterLevel <= 17))
        {
            spell.duration = 6 - 1;
        }
        else if ((spell.casterLevel >= 18) && (spell.casterLevel <= 20))
        {
            spell.duration = 7 - 1;
        }
        else
        {
            spell.duration = 1 - 1;
        }

        var target = spell.Targets[0];
        if (!(target.Object == spell.caster))
        {
            var attack_successful = spell.caster.PerformTouchAttack(target.Object);
            // perform ranged touch attack
            if (attack_successful == D20CAF.HIT)
            {
                // hit
                target.Object.AddCondition("sp-Melfs Acid Arrow", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Melfs Acid Arrow Projectile Hit", target.Object);
            }
            else if (attack_successful == D20CAF.CRITICAL)
            {
                // critical hit
                target.Object.AddCondition("sp-Melfs Acid Arrow", spell.spellId, spell.duration, 1);
                target.ParticleSystem = AttachParticles("sp-Melfs Acid Arrow Projectile Hit", target.Object);
            }
            else
            {
                // missed
                target.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target.Object);
                spell.RemoveTarget(target.Object);
            }

        }
        else
        {
            Logger.Info("creating acid arrows not supported yet!");
        }

        spell.EndSpell();
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Melf's Acid Arrow OnEndSpellCast");
    }

}