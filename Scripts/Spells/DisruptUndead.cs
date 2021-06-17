
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
    [SpellScript(135)]
    public class DisruptUndead : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disrupt Undead OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Disrupt Undead-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Disrupt Undead-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Disrupt Undead OnEndProjectile");
            spell.duration = 0;
            EndProjectileParticles(projectile);
            var target = spell.Targets[0];
            
            if (target.Object.IsMonsterCategory(MonsterCategory.undead))
            {
                // perform ranged touch attack
                var attack_successful = spell.caster.PerformTouchAttack(target.Object);
                if ((attack_successful & D20CAF.HIT) != D20CAF.NONE)
                {
                    var damage_dice = Dice.D6;
                    // hit
                    target.Object.DealSpellDamage(spell.caster, DamageType.PositiveEnergy, damage_dice, D20AttackPower.UNSPECIFIED, 100, D20ActionType.CAST_SPELL, spell.spellId, attack_successful, index_of_target);
                    target.ParticleSystem = AttachParticles("sp-Disrupt Undead-hit", target.Object);
                }
                else
                {
                    // missed
                    target.Object.FloatMesFileLine("mes/spell.mes", 30007);
                    AttachParticles("Fizzle", target.Object);
                }

            }
            else
            {
                // not undead!
                target.Object.FloatMesFileLine("mes/spell.mes", 31008);
                AttachParticles("Fizzle", target.Object);
            }

            
            spell.RemoveTarget(target.Object);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Disrupt Undead OnEndSpellCast");
        }

    }
}
