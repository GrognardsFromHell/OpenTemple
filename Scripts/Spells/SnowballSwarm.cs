
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
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Snowball Swarm OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Snowball Swarm-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Snowball Swarm-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Snowball Swarm OnEndProjectile");
            var remove_list = new List<GameObjectBody>();
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
}
