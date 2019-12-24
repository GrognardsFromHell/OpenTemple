
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
    [SpellScript(202)]
    public class GreaterDispelling : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dispel Magic OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            spell.casterLevel = Math.Min(20, spell.casterLevel);
            // check if we are targetting an object or an area
            if (spell.IsObjectSelected())
            {
                var target = spell.Targets[0];
                // support dispel on critters
                if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
                {
                    target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);
                    target.Object.AddCondition("sp-Dispel Magic", spell.spellId, 0, 0);
                }
                // support dispel on portals and containers
                else if ((target.Object.type == ObjectType.portal) || (target.Object.type == ObjectType.container))
                {
                    if ((target.Object.GetPortalFlags() & PortalFlag.MAGICALLY_HELD) != 0)
                    {
                        target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);
                        target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
                        spell.RemoveTarget(target.Object);
                    }

                }

            }
            else
            {
                // support dispel on these obj_types: weapon, ammo, armor, scroll
                // NO support for: money, food, key, written, generic, scenery, trap, bag
                // elif (target.obj.type == obj_t_weapon) or (target.obj.type == obj_t_ammo) or (target.obj.type == obj_t_armor) or (target.obj.type == obj_t_scroll):
                // print "[dispel magic] - items not supported yet!"
                // game.particles( 'Fizzle', target.obj )
                // spell.target_list.remove_target( target.obj )
                // draw area effect particles
                SpawnParticles("sp-Dispel Magic - Area", spell.aoeCenter);
                foreach (var target in spell.Targets)
                {
                    if ((target.Object.type == ObjectType.pc) || (target.Object.type == ObjectType.npc))
                    {
                        target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);
                        target.Object.AddCondition("sp-Dispel Magic", spell.spellId, 0, 1);
                    }
                    // support dispel on portals and containers
                    else if ((target.Object.type == ObjectType.portal) || (target.Object.type == ObjectType.container))
                    {
                        if ((target.Object.GetPortalFlags() & PortalFlag.MAGICALLY_HELD) != 0)
                        {
                            target.ParticleSystem = AttachParticles("sp-Dispel Magic - Targeted", target.Object);
                            target.Object.ClearPortalFlag(PortalFlag.MAGICALLY_HELD);
                            spell.RemoveTarget(target.Object);
                        }

                    }

                }

            }

            // support dispel on these obj_types: weapon, ammo, armor, scroll
            // NO support for: money, food, key, written, generic, scenery, trap, bag
            // elif (target.obj.type == obj_t_weapon) or (target.obj.type == obj_t_ammo) or (target.obj.type == obj_t_armor) or (target.obj.type == obj_t_scroll):
            // print "[dispel magic] - items not supported yet!"
            // game.particles( 'Fizzle', target.obj )
            // spell.target_list.remove_target( target.obj )
            spell.EndSpell(true);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            spell.EndSpell(true);
            Logger.Info("Dispel Magic OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dispel Magic OnEndSpellCast");
        }

    }
}
