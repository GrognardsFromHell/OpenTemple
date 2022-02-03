
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

[SpellScript(412)]
public class SearingLight : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("Searing Light OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("Searing Light OnSpellEffect");
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("Searing Light OnBeginRound");
    }
    public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Searing Light OnBeginProjectile");
        // spell.proj_partsys_id = game.particles( 'sp-Searing Light', projectile )
        SetProjectileParticles(projectile, AttachParticles("sp-Searing Light", projectile));
    }
    public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
    {
        Logger.Info("Searing Light OnEndProjectile");
        var damage_dice = Dice.D8;
        damage_dice = damage_dice.WithCount(Math.Min(5, spell.casterLevel / 2));
        spell.duration = 0;
        EndProjectileParticles(projectile);
        var target_item = spell.Targets[0];
        var npc = spell.caster;
        if (npc.GetNameId() == 20003)
        {
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.GetNameId() == 8072 && obj.GetLeader() != null)
                {
                    var curr = obj.GetStat(Stat.hp_current);
                    if (curr >= -9)
                    {
                        target_item.Object = obj;
                    }

                }

            }

        }

            
        // adding fix for an undead creature particularly vulnerable to bright light (& six lines below)
        // check if undead has a vulnerability to sunlight, an aversion to daylight, or a sunlight or daylight powerlessness
        // current creatures: Bodak, Nightwalker
        var undead_list = new[] { 14328, 14958 };
        bool undead_vulnerable = false;
        if ((target_item.Object.IsMonsterCategory(MonsterCategory.undead)))
        {
            undead_vulnerable = false;
            foreach (var undead in undead_list)
            {
                if (undead == target_item.Object.GetNameId())
                {
                    undead_vulnerable = true;
                }

            }

        }

        var attack_successful = spell.caster.PerformTouchAttack(target_item.Object);
        if ((attack_successful & D20CAF.HIT) != D20CAF.NONE)
        {
            if (index_of_target > 0)
            {
                attack_successful |= D20CAF.NO_PRECISION_DAMAGE;
            }

            AttachParticles("sp-Searing Light-Hit", target_item.Object);
            // hit
            if ((target_item.Object.IsMonsterCategory(MonsterCategory.undead)))
            {
                if (undead_vulnerable)
                {
                    damage_dice = damage_dice.WithSides(8);
                }
                else
                {
                    damage_dice = damage_dice.WithSides(6);
                }

                damage_dice = damage_dice.WithCount(Math.Min(10, spell.casterLevel));
                target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, attack_successful, index_of_target);
            }
            else
            {
                if ((target_item.Object.IsMonsterCategory(MonsterCategory.construct)))
                {
                    damage_dice = damage_dice.WithSides(6);
                }

                target_item.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, attack_successful, index_of_target);
            }

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
        Logger.Info("Searing Light OnEndSpellCast");
    }

}