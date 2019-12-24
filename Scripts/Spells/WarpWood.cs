
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
    [SpellScript(528)]
    public class WarpWood : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Warp Wood OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Warp Wood OnSpellEffect");
            var number_items = spell.casterLevel;
            var remove_list = new List<GameObjectBody>();
            var itemref = 0;
            foreach (var target_item in spell.Targets)
            {
                if (number_items > 0)
                {
                    // First, check creatures for held wooden items
                    if ((target_item.Object.type == ObjectType.pc) || (target_item.Object.type == ObjectType.npc))
                    {
                        // Check for a wooden missile weapon and destroy that
                        var item_affected = target_item.Object.ItemWornAt(EquipSlot.WeaponPrimary);
                        if ((item_affected.GetInt(obj_f.weapon_ammo_type) < 2) && (number_items >= Warp_Size_by_Item_Size(item_affected)))
                        {
                            var FLAGS = item_affected.GetItemFlags();
                            // Only works on nonmagic wood, and the item gets its owner's Will save
                            if ((item_affected.GetMaterial() == Material.wood) && (FLAGS & ItemFlag.IS_MAGICAL) == 0 && !(target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId)))
                            {
                                target_item.Object.FloatMesFileLine("mes/combat.mes", 22);
                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                                AttachParticles("Blight", target_item.Object);
                                item_affected.Destroy();
                            }

                        }

                        // Check for a wooden melee weapon and warp that (-4 to attack rolls)
                        item_affected = target_item.Object.ItemWornAt(EquipSlot.WeaponPrimary);
                        if ((item_affected != null) && (number_items >= Warp_Size_by_Item_Size(item_affected)))
                        {
                            var FLAGS = item_affected.GetItemFlags();
                            // Only works on nonmagic wood, and the item gets its owner's Will save
                            if ((item_affected.GetMaterial() == Material.wood) && (FLAGS & ItemFlag.IS_MAGICAL) == 0 && !(target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId)))
                            {
                                if (!(Co8.is_spell_flag_set(item_affected, Co8SpellFlag.Summoned)))
                                {
                                    item_affected.AddConditionToItem("To Hit Bonus", -4);
                                    Co8.set_spell_flag(item_affected, Co8SpellFlag.Summoned);
                                    AttachParticles("Blight", target_item.Object);
                                    target_item.Object.FloatMesFileLine("mes/combat.mes", 8);
                                }

                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                            }

                        }

                    }
                    // Done with creature checks
                    // Check for unattended weapons targeted and warp, unwarp, or destroy them
                    else if ((target_item.Object.type == ObjectType.weapon))
                    {
                        var item_affected = target_item.Object;
                        var FLAGS = item_affected.GetItemFlags();
                        // Only works on nonmagical wooden items
                        if ((item_affected.GetMaterial() == Material.wood) && (FLAGS & ItemFlag.IS_MAGICAL) == 0 && (number_items >= Warp_Size_by_Item_Size(item_affected)))
                        {
                            // Missile weapons are destroyed
                            if ((item_affected.GetInt(obj_f.weapon_ammo_type) < 2))
                            {
                                target_item.Object.FloatMesFileLine("mes/combat.mes", 22);
                                AttachParticles("Blight", target_item.Object);
                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                                item_affected.Destroy();
                            }
                            // Melee weapons that are warped are replaced with an unwarped version
                            else if ((Co8.is_spell_flag_set(item_affected, Co8SpellFlag.Summoned)))
                            {
                                var proto = item_affected.GetNameId();
                                var holder = GameSystems.MapObject.CreateObject(proto, item_affected.GetLocation());
                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                                item_affected.Destroy();
                            }
                            else
                            {
                                // Melee weapons that are unwarped are warped (-4 to attack rolls)
                                item_affected.AddConditionToItem("To Hit Bonus", -4);
                                Co8.set_spell_flag(item_affected, Co8SpellFlag.Summoned);
                                AttachParticles("Blight", target_item.Object);
                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                                target_item.Object.FloatMesFileLine("mes/combat.mes", 8);
                            }

                        }

                        number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                    }
                    // Check for locked wooden chests, and unlock them
                    else if ((target_item.Object.type == ObjectType.container))
                    {
                        var item_affected = target_item.Object;
                        if ((target_item.Object.GetMaterial() == Material.wood))
                        {
                            if (((target_item.Object.GetContainerFlags() & ContainerFlag.LOCKED)) != 0 && (number_items >= Warp_Size_by_Item_Size(item_affected)))
                            {
                                target_item.Object.ClearContainerFlag(ContainerFlag.LOCKED);
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30004);
                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                            }

                        }
                        else
                        {
                            AttachParticles("Fizzle", target_item.Object);
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                        }

                    }
                    // Check for locked wooden doors, and unlock and open them
                    else if ((target_item.Object.type == ObjectType.portal))
                    {
                        var item_affected = target_item.Object;
                        if ((target_item.Object.GetMaterial() == Material.wood))
                        {
                            if (((target_item.Object.GetPortalFlags() & PortalFlag.LOCKED)) != 0 && (number_items >= Warp_Size_by_Item_Size(item_affected)))
                            {
                                target_item.Object.ClearPortalFlag(PortalFlag.LOCKED);
                                target_item.Object.FloatMesFileLine("mes/spell.mes", 30004);
                                if ((target_item.Object.GetPortalFlags() & PortalFlag.OPEN) == 0)
                                {
                                    target_item.Object.TogglePortalOpen();
                                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30013);
                                }

                                number_items = number_items - Warp_Size_by_Item_Size(item_affected);
                            }

                        }
                        else
                        {
                            AttachParticles("Fizzle", target_item.Object);
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                        }

                    }
                    else
                    {
                        AttachParticles("Fizzle", target_item.Object);
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30003);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Warp Wood OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Warp Wood OnEndSpellCast");
        }
        public static int Warp_Size_by_Item_Size(GameObjectBody itemref)
        {
            // This function returns the equivalent number of warp-items used up for the spell
            // based on the passed object's size. A Small or smaller object counts as 1 item.
            // A Medium object counts as 2 items, a Large object as 4 items, and so on
            // up to 32 items for a Colossal object.
            var size = itemref.GetInt(obj_f.size);
            int warp_size;
            if (size == 4)
            {
                warp_size = 2;
            }
            else if (size == 5)
            {
                warp_size = 4;
            }
            else if (size == 6)
            {
                warp_size = 8;
            }
            else if (size == 7)
            {
                warp_size = 16;
            }
            else if (size == 8)
            {
                warp_size = 32;
            }
            else
            {
                warp_size = 1;
            }

            return warp_size;
        }

    }
}
