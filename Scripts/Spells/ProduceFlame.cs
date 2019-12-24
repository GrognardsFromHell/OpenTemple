
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
    [SpellScript(364)]
    public class ProduceFlame : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnSpellEffect");
            var (xx, yy) = spell.caster.GetLocation();
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                AttachParticles("swirled gas", spell.caster);
                spell.caster.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                Sound(7581, 1);
                Sound(7581, 1);
            }
            else
            {
                spell.duration = 10 * spell.casterLevel;
                if (spell.casterLevel > 5)
                {
                    spell.casterLevel = 5;
                }

                var target = spell.Targets[0];
                target.Object.AddCondition("sp-Produce Flame", spell.spellId, spell.duration, 0);
                target.ParticleSystem = AttachParticles("sp-Produce Flame", target.Object);
            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Produce Flame OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Produce Flame-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Produce Flame-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Produce Flame OnEndProjectile");
            var targg364 = spell.Targets[index_of_target].Object;
            var (xx, yy) = targg364.GetLocation();
            if (targg364.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                targg364.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                Sound(7581, 1);
                Sound(7581, 1);
                AttachParticles("swirled gas", targg364);
            }
            else
            {
                // return_val = spell.caster.perform_touch_attack( targg364 )
                // if return_val >= 1:
                // damage_dice = dice_new( '4d6' )
                // game.particles( 'sp-Produce Flame-Hit', targg364 )
                // target.obj.spell_damage( spell.caster, D20DT_FIRE, damage_dice, D20DAP_UNSPECIFIED, D20A_CAST_SPELL, spell.id )
                spell.caster.D20SendSignalEx(D20DispatcherKey.SIG_TouchAttack, spell.Targets[index_of_target].Object);
            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Produce Flame OnEndSpellCast");
        }

    }
}
