
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
    [SpellScript(171)]
    public class Fireball : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Fireball OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Fireball OnSpellEffect");
            AttachParticles("sp-Fireball-conjure", spell.caster);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Fireball OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Fireball OnBeginProjectile");
            // spell.proj_partsys_id = game.particles( 'sp-Fireball-proj', projectile )
            SetProjectileParticles(projectile, AttachParticles("sp-Fireball-proj", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Fireball OnEndProjectile");
            var remove_list = new List<GameObjectBody>();
            spell.duration = 0;
            var dam = Dice.D6;
            dam = dam.WithCount(Math.Min(10, spell.casterLevel));
            if (Co8Settings.ElementalSpellsAtElementalNodes)
            {
                if (SelectedPartyLeader.GetMap() == 5083) // Fire node - fire spells do double damage
                {
                    dam = dam.WithCount(dam.Count * 2);
                }
                else if (SelectedPartyLeader.GetMap() == 5084) // Water node - fire spells do half damage
                {
                    dam = dam.WithCount(dam.Count / 2);
                }

            }

            EndProjectileParticles(projectile);
            var (xx, yy) = spell.aoeCenter.location;
            if (SelectedPartyLeader.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
            {
                // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                // This is for the projectile hitting inside the chamber - total fizzle
                var tro = GameSystems.MapObject.CreateObject(14070, spell.aoeCenter);
                SpawnParticles("swirled gas", spell.aoeCenter);
                tro.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                tro.Destroy();
                Sound(7581, 1);
                Sound(7581, 1);
                foreach (var target_item in spell.Targets)
                {
                    remove_list.Add(target_item.Object);
                }

            }
            else
            {
                // else: # suppose the fireball projectile lands outside the chamber, check individual targets
                SpawnParticles("sp-Fireball-Hit", spell.aoeCenter);
                var soundfizzle = 0;
                foreach (var target_item in spell.Targets)
                {
                    (xx, yy) = target_item.Object.GetLocation();
                    if (target_item.Object.GetMap() == 5067 && (xx >= 521 && xx <= 555) && (yy >= 560 && yy <= 610))
                    {
                        // Water Temple Pool Enchantment prevents fire spells from working inside the chamber, according to the module -SA
                        SpawnParticles("swirled gas", target_item.Object.GetLocation());
                        target_item.Object.FloatMesFileLine("mes/skill_ui.mes", 2000, TextFloaterColor.Red);
                        soundfizzle = 1;
                        spell.RemoveTarget(target_item.Object);
                    }
                    else
                    {
                        if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.Fire, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
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

                    remove_list.Add(target_item.Object);
                }

                if (soundfizzle == 1)
                {
                    Sound(7581, 1);
                    Sound(7581, 1);
                }

            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Fireball OnEndSpellCast");
        }

    }
}
