
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace Scripts.Spells
{
    [SpellScript(773)]
    public class Avasculate : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Avasculate OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Avasculate OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Avasculate OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Avasculate OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Disintegrate", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Avasculate OnEndProjectile");
            var target_item = spell.Targets[0];
            spell.duration = 0;
            EndProjectileParticles(projectile);
            // WF Ray fix added by Shiningted (& two lines below)
            var has_it = 0;
            var x = 0;
            var y = 0;
            if (spell.caster.HasFeat(FeatId.WEAPON_FOCUS_RAY))
            {
                // game.particles( "sp-summon monster I", game.party[0] )
                has_it = 1;
                x = spell.caster.GetBaseStat(Stat.dexterity);
                y = x + 2;
                if (spell.caster.HasFeat(FeatId.GREATER_WEAPON_FOCUS_RAY))
                {
                    y = y + 2;
                }

                spell.caster.SetBaseStat(Stat.dexterity, y);
            }

            var return_val = spell.caster.PerformTouchAttack(target_item.Object);
            if ((return_val & D20CAF.HIT) != D20CAF.NONE)
            {
                AttachParticles("sp-Curse Water", target_item.Object);
                AttachParticles("hit-BLUDGEONING_AND_PIERCING-medium", target_item.Object);
                AttachParticles("hit-BLUDGEONING-medium", target_item.Object);
                AttachParticles("hit-PIERCING_AND_SLASHING-medium", target_item.Object);
                AttachParticles("hit-SLASHING_AND_BLUDGEONING_AND_PIERCING-medium", target_item.Object);
                AttachParticles("hit-SLASHING-medium", target_item.Object);
                var damage_dice = (target_item.Object.GetStat(Stat.hp_current) / 2);
                var dam = Dice.Parse("1d1");
                dam = dam.WithCount(Math.Min(500, damage_dice));
                target_item.Object.DealSpellDamage(spell.caster, DamageType.BloodLoss, dam, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId); // Avasculate doesn't really do damage so not using the weaponlike rule here
                if (target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }
                else
                {
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    spell.duration = 1;
                    target_item.Object.AddCondition("sp-Sound Burst", spell.spellId, spell.duration, 0);
                }

            }
            else
            {
                // missed
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30007);
                AttachParticles("Fizzle", target_item.Object);
            }

            if (has_it == 1)
            {
                spell.caster.SetBaseStat(Stat.dexterity, x);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Avasculate OnEndSpellCast");
        }

    }
}
