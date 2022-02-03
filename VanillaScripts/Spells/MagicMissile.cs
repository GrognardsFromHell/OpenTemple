
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(288)]
    public class MagicMissile : BaseSpellScript
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
        public override void OnBeginProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Magic Missile OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-magic missle-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObject projectile, int index_of_target)
        {
            Logger.Info("Magic Missile OnEndProjectile");
            var target = spell.Targets[index_of_target];

            var damage_dice = Dice.D4;

            damage_dice = damage_dice.WithModifier(1);
            target.Object.AddCondition("sp-Magic Missile", spell.spellId, spell.duration, damage_dice.Roll());
            target.ParticleSystem = AttachParticles("sp-magic missle-hit", target.Object);

            spell.RemoveProjectile(projectile);
            if (spell.projectiles.Length == 0)
            {
                spell.EndSpell(true);
            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnEndSpellCast");
        }


    }
}
