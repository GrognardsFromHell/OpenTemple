
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
    [SpellScript(569)]
    public class CujosMagicMissile : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnSpellEffect");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Magic Missile OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-magic missle-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Magic Missile OnEndProjectile");
            var target = spell.Targets[index_of_target];
            var damage_dice = Dice.Parse("5d12");
            damage_dice = damage_dice.WithModifier(1);
            // always hits
            target.Object.DealSpellDamage(spell.caster, DamageType.Force, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            target.ParticleSystem = AttachParticles("sp-magic missle-hit", target.Object);
            // spell.target_list.remove_target_by_index( index_of_target )

            spell.RemoveProjectile(projectile);
            if (spell.projectiles.Length == 0)
            {
                // loc = target.obj.location
                // target.obj.destroy()
                // mxcr = game.obj_create( 12021, loc )
                // game.global_vars[30] = game.global_vars[30] + 1
                spell.EndSpell(true);
            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnEndSpellCast");
        }

    }
}
