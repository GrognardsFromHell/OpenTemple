
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

[SpellScript(307)]
public class MeteorSwarm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Meteor Swarm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-evocation-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Meteor Swarm OnSpellEffect");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Meteor Swarm OnBeginProjectile");
        var projectiles = 4;
        if (index_of_target < projectiles)
        {
            SetProjectileParticles(projectile, AttachParticles("sp-Spheres of Fire-proj", projectile));
        }

    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Meteor Swarm OnEndProjectile");
        var dam = Dice.Parse("2d6");
        var dam2 = Dice.Parse("6d6");
        var projectiles = 4;
        if (index_of_target < projectiles)
        {
            spell.duration = 0;
            EndProjectileParticles(projectile);
            var target_item = spell.Targets[index_of_target];
            var return_val = spell.caster.PerformTouchAttack(target_item.Object);
            var (xx, yy) = target_item.Object.GetLocation();
            if (target_item.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                AttachParticles("swirled gas", target_item.Object);
                Sound(7581, 1);
                Sound(7581, 1);
            }
            else
            {
                if (((return_val & D20CAF.HIT)) != D20CAF.NONE)
                {
                    // hit target
                    if (index_of_target > 0)
                    {
                        return_val |= D20CAF.NO_PRECISION_DAMAGE;
                    }

                    AttachParticles("sp-Spheres of Fire-hit", target_item.Object);
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Bludgeoning, dam, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
                    target_item.Object.DealSpellDamage(spell.caster, DamageType.Fire, dam2, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, return_val, index_of_target);
                }
                else
                {
                    // miss target
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                    AttachParticles("Fizzle", target_item.Object);
                    if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam2, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    }

                }

                AttachParticles("sp-Fireball-Hit", target_item.Object);
                foreach (var critter in ObjList.ListCone(target_item.Object, ObjectListFilter.OLC_CRITTERS, 40, -180, 360))
                {
                    if ((critter != target_item.Object) && (!critter.D20Query(D20DispatcherKey.QUE_Dead)))
                    {
                        (xx, yy) = critter.GetLocation();
                        if (critter.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                        {
                            critter.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                            AttachParticles("swirled gas", critter);
                            Sound(7581, 1);
                            Sound(7581, 1);
                        }
                        else
                        {
                            AttachParticles("hit-FIRE-burst", critter);
                            if (critter.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam2, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                            {
                                // saving throw successful
                                critter.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                // saving throw unsuccessful
                                critter.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }

                    }

                }

            }

            spell.RemoveProjectile(projectile);
        }

        if ((spell.projectiles.Length <= 0))
        {
            spell.EndSpell(true);
        }

    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Meteor Swarm OnEndSpellCast");
    }

}