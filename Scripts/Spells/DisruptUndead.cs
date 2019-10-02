
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

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

            if (has_it == 1)
            {
                spell.caster.SetBaseStat(Stat.dexterity, x);
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