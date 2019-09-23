
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
    [SpellScript(133)]
    public class DispelMagic : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Dispel Magic OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-abjuration-conjure", spell.caster);
        }

        private LocAndOffsets AverageLocation(LocAndOffsets a, LocAndOffsets b)
        {
            var averaged = (a.ToInches3D() + b.ToInches3D()) / 2;
            return LocAndOffsets.FromInches(averaged);
        }

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            // Lareth Special scripting in the Moathouse
            if (spell.caster.GetNameId() == 8002 && spell.caster.GetMap() == 5005)
            {
                GameObjectBody player_cast_web_obj = null;
                GameObjectBody player_cast_entangle_obj = null;
                foreach (var spell_obj in ObjList.ListVicinity(spell.caster.GetLocation(), ObjectListFilter.OLC_GENERIC))
                {
                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 531 + (1 << 15))
                    {
                        player_cast_web_obj = spell_obj;
                    }

                    if (spell_obj.GetInt(obj_f.secretdoor_dc) == 153 + (1 << 15))
                    {
                        player_cast_entangle_obj = spell_obj;
                    }

                }

                LocAndOffsets locc_;
                if (player_cast_entangle_obj != null && player_cast_web_obj != null && player_cast_entangle_obj.DistanceTo(player_cast_web_obj) <= 18)
                {
                    locc_ = AverageLocation(player_cast_entangle_obj.GetLocationFull(), player_cast_web_obj.GetLocationFull());
                }
                else if (player_cast_web_obj != null)
                {
                    locc_ = player_cast_web_obj.GetLocationFull();
                }
                else if (player_cast_entangle_obj != null)
                {
                    locc_ = player_cast_entangle_obj.GetLocationFull();
                }
                else
                {
                    locc_ = spell.caster.GetLocationFull();
                }

                SpawnParticles("sp-Dispel Magic - Area", locc_);
                var dispel_obj = GameSystems.MapObject.CreateObject(OBJECT_SPELL_GENERIC, locc_);
                dispel_obj.Move(locc_);
                foreach (var target in ObjList.ListVicinity(dispel_obj, ObjectListFilter.OLC_GENERIC))
                {
                    if (target.DistanceTo(dispel_obj) <= 20)
                    {
                        var partsys_id = AttachParticles("sp-Dispel Magic - Targeted", target);
                        // aaa1 = game.party[0].damage( OBJ_HANDLE_NULL, 0, dice_new("1d3"))
                        target.AddCondition("sp-Dispel Magic", spell.spellId, 0, 1);
                    }

                }

                // game.party[0].damage( OBJ_HANDLE_NULL, 0, dice_new("1d4"))
                foreach (var target in ObjList.ListVicinity(dispel_obj, ObjectListFilter.OLC_NPC|ObjectListFilter.OLC_PC))
                {
                    if (target.DistanceTo(dispel_obj) <= 18)
                    {
                        if ((target.type == ObjectType.pc) || (target.type == ObjectType.npc) || (target.type == ObjectType.generic))
                        {
                            var partsys_id = AttachParticles("sp-Dispel Magic - Targeted", target);
                            target.AddCondition("sp-Dispel Magic", spell.spellId, 0, partsys_id);
                        }

                    }

                }

                dispel_obj.Destroy();
            }
            // check if we are targetting an object or an area
            else if (spell.IsObjectSelected())
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
            spell.EndSpell();
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
