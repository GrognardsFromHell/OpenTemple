
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

namespace Scripts.Spells
{
    [SpellScript(288)]
    public class MagicMissile : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Magic Missile OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        // spell.num_of_projectiles = spell.num_of_projectiles + 1
        // spell.target_list.push_target(spell.caster)     # didn't work :(
        // generally the sequence is: OnBeginSpellCast, OnBeginProjectile, OnSpellEffect,OnEndProjectile (OnBeginRound isn't called)

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
            EndProjectileParticles(projectile);
            var target = spell.Targets[index_of_target];
            var damage_dice = Dice.D4;
            damage_dice = damage_dice.WithModifier(1);
            var target_item_obj = target.Object;
            if ((!((PartyLeader.GetPartyMembers()).Contains(spell.caster))) && target_item_obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Charmed))
            {
                // NPC enemy is trying to cast on a charmed target - this is mostly meant for the Cult of the Siren encounter
                target_item_obj = Utilities.party_closest(spell.caster, exclude_warded:true); // select nearest conscious PC instead, who isn't already charmed
                if (target_item_obj == null)
                {
                    target_item_obj = target.Object;
                }

            }

            // always hits
            target_item_obj.AddCondition("sp-Magic Missile", spell.spellId, spell.duration, damage_dice.Roll());
            target.ParticleSystem = AttachParticles("sp-magic missle-hit", target_item_obj);
            // special scripting for NPCs no longer necessary - NPCs will launch multiple projectiles now
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
