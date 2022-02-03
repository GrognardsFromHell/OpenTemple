
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
    [SpellScript(80)]
    public class ControlUndead : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Undead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Control Undead OnSpellEffect");
            var remove_list = new List<GameObject>();
            spell.duration = 10 * spell.casterLevel;
            var hitDiceAmount = 2 * spell.casterLevel;
            var controlled_undead = 0;

            foreach (var target_item in spell.Targets)
            {
                // check critter hit dice
                var targetHitDice = GameSystems.Critter.GetHitDiceNum(target_item.Object);
                // check if target does not exceed the amount allowed
                if (hitDiceAmount >= targetHitDice)
                {
                    // check if target is undead
                    if (target_item.Object.IsMonsterCategory(MonsterCategory.undead))
                    {
                        // subtract the target's hit dice from the amount allowed
                        hitDiceAmount = hitDiceAmount - targetHitDice;

                        int spell_dc = spell.dc;
                        var lich_number = false;
                        // check if target is Mathel or Angra Mainyu (a lich has +4 turn resistance)
                        if ((target_item.Object.GetNameId() == 14785) || (target_item.Object.GetNameId() == 8812) || (target_item.Object.GetNameId() == 14984) || (target_item.Object.GetNameId() == 8893))
                        {
                            spell.dc -= 4;
                            lich_number = true;
                        }

                        // check if target is not already rebuked
                        if (target_item.Object.D20Query(D20DispatcherKey.QUE_Commanded))
                        {
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 20045);
                            AttachParticles("Fizzle", target_item.Object);
                        }
                        else if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            // saving throw unsuccessful
                            spell.caster.AddAIFollower(target_item.Object);
                            target_item.Object.AddCondition("Commanded", spell.spellId, spell.duration, 0);
                            AttachParticles("sp-Feeblemind", target_item.Object);
                            // add target to initiative, just in case
                            target_item.Object.AddToInitiative();
                            UiSystems.Combat.Initiative.UpdateIfNeeded();
                            // add time event
                            StartTimer(spell.duration * 6000, () => removeControlUndead(spell.caster, target_item.Object));
                            controlled_undead = 1;
                        }
                        else
                        {
                            // saving throw successful
                            target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                            AttachParticles("Fizzle", target_item.Object);
                        }

                        // reset Lich turn resistance bonus
                        if (lich_number)
                        {
                            spell.dc = spell_dc;
                            lich_number = false;
                        }

                    }
                    else
                    {
                        // not an undead
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 31008);
                        AttachParticles("Fizzle", target_item.Object);
                    }

                }
                else
                {
                    // ran out of allowed HD
                    AttachParticles("Fizzle", target_item.Object);
                }

                remove_list.Add(target_item.Object);
            }

            if (controlled_undead == 1)
            {
                spell.caster.AddCondition("sp-Owls Wisdom", spell.spellId, spell.duration, 0);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Control Undead OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Control Undead OnEndSpellCast");
        }
        public void removeControlUndead(GameObject caster, GameObject target)
        {
            Logger.Info("Control Undead - removing control. {0}{1}", caster, target);
            var old_undead = target.GetNameId();
            var old_undead_hp = target.GetInt(obj_f.hp_pts);
            var old_undead_dam = target.GetInt(obj_f.hp_damage);
            var old_undead_IQ = target.GetStat(Stat.intelligence);
            var undead_loc = target.GetLocation();
            var (xx, yy) = target.GetLocation();
            var undead_rot = target.Rotation;
            var old_weapon = target.ItemWornAt(EquipSlot.WeaponPrimary);
            var old_shield1 = target.ItemWornAt(EquipSlot.WeaponSecondary);
            var old_shield2 = target.ItemWornAt(EquipSlot.Shield);
            var old_ammo = target.ItemWornAt(EquipSlot.Ammo);
            int old_quantity = 0;
            if ((old_ammo != null))
            {
                old_quantity = old_ammo.GetInt(obj_f.ammo_quantity);
            }

            caster.RemoveAIFollower(target);
            caster.RemoveFollower(target);
            if (!target.D20Query(D20DispatcherKey.QUE_Dead))
            {
                target.Destroy();
                if (old_undead == 8812) // Mathel fix
                {
                    old_undead = 14785;
                }

                if (old_undead == 8893) // Angra Mainyu fix
                {
                    old_undead = 14984;
                }

                // added to stop Skeleton crossbowmen from spawning more ammo on their first heartbeat
                if ((old_undead == 14107) || (old_undead == 14600))
                {
                    old_undead = 14603;
                }

                var new_undead = GameSystems.MapObject.CreateObject(old_undead, undead_loc);
                new_undead.SetInt(obj_f.hp_pts, old_undead_hp);
                new_undead.SetInt(obj_f.hp_damage, old_undead_dam);
                new_undead.Move(new locXY(xx, yy));
                new_undead.Rotation = undead_rot;
                if ((new_undead.ItemWornAt(EquipSlot.WeaponPrimary) != null))
                {
                    new_undead.ItemWornAt(EquipSlot.WeaponPrimary).Destroy();
                }

                if ((new_undead.ItemWornAt(EquipSlot.WeaponSecondary) != null))
                {
                    new_undead.ItemWornAt(EquipSlot.WeaponSecondary).Destroy();
                }

                if ((new_undead.ItemWornAt(EquipSlot.Shield) != null))
                {
                    new_undead.ItemWornAt(EquipSlot.Shield).Destroy();
                }

                if ((new_undead.ItemWornAt(EquipSlot.Ammo) != null))
                {
                    new_undead.ItemWornAt(EquipSlot.Ammo).Destroy();
                }

                if ((old_weapon != null))
                {
                    Utilities.create_item_in_inventory(old_weapon.GetNameId(), new_undead);
                }

                if ((old_shield1 != null))
                {
                    Utilities.create_item_in_inventory(old_shield1.GetNameId(), new_undead);
                }

                if ((old_shield2 != null))
                {
                    Utilities.create_item_in_inventory(old_shield2.GetNameId(), new_undead);
                }

                if ((old_ammo != null))
                {
                    var new_ammo = GameSystems.MapObject.CreateObject(old_ammo.GetNameId(), undead_loc);
                    new_ammo.SetInt(obj_f.ammo_quantity, old_quantity);
                    new_undead.GetItem(new_ammo);
                }

                new_undead.WieldBestInAllSlots();
                if ((old_undead_IQ > 0))
                {
                    if ((old_undead == 14785) || (old_undead == 14984)) // Lich attack
                    {
                        new_undead.FloatMesFileLine("mes/gd_cls_m2m.mes", 19013, TextFloaterColor.Red);
                        new_undead.Attack(caster);
                    }
                    else
                    {
                        var new_undead_strat = new_undead.GetInt(obj_f.critter_strategy); // critter strategy
                        if ((new_undead.FindItemByName(4096) != null) || (new_undead.FindItemByName(4097) != null))
                        {
                            new_undead.SetInt(obj_f.critter_strategy, 183); // Mage-Sniper strategy
                        }
                        else if ((new_undead.FindItemByName(4194) != null))
                        {
                            new_undead.SetInt(obj_f.critter_strategy, 234); // Skeleton Longspear (target dying) strategy
                        }
                        else
                        {
                            new_undead.SetInt(obj_f.critter_strategy, 180); // Assassin strategy
                        }

                        StartTimer(1000, () => resetControlUndead(new_undead, new_undead_strat));
                        new_undead.Attack(caster);
                    }

                }

            }

        }

        public void resetControlUndead(GameObject target, int strategy)
        {
            Logger.Info("Control Undead - resetting strategy. {0}{1}", target, strategy);
            target.SetInt32(obj_f.critter_strategy, strategy);
        }

    }
}
