
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

[SpellScript(733)]
public class ScorchingRay : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Scorching Ray OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Scorching Ray OnSpellEffect");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Scorching Ray OnBeginProjectile");
        var projectiles = Math.Min(3, (spell.casterLevel + 1) / 4);
        if (index_of_target < projectiles)
        {
            SetProjectileParticles(projectile, AttachParticles("sp-Scorching Ray", projectile));
        }

    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Scorching Ray OnEndProjectile");
            
        var projectiles = Math.Min(3, (spell.casterLevel + 1) / 4);
        if (index_of_target < projectiles)
        {
            spell.duration = 0;
            EndProjectileParticles(projectile);
            var target = spell.Targets[index_of_target];
            var return_val = spell.caster.PerformTouchAttack(target.Object);
            var (xx, yy) = target.Object.GetLocation();
            if (target.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                target.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                AttachParticles("swirled gas", target.Object);
                Sound(7581, 1);
                Sound(7581, 1);
            }
            else if ((return_val & D20CAF.HIT) != D20CAF.NONE)
            {
                if (index_of_target > 0)
                {
                    return_val |= D20CAF.NO_PRECISION_DAMAGE;
                }

                var damage_dice = Dice.Parse("4d6");
                AttachParticles("sp-Scorching Ray-Hit", target.Object);
                target.Object.DealSpellDamage(spell.caster, DamageType.Fire, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
            }
            else
            {
                // missed
                target.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target.Object);
            }

        }

           
        spell.RemoveProjectile(projectile);
        if ((spell.projectiles.Length == 0))
        {
            spell.EndSpell(true);
        }

    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Scorching Ray OnEndSpellCast");
    }

}