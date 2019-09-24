
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
    [SpellScript(487)]
    public class Sunburst : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sunburst OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-evocation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Sunburst OnSpellEffect");
            AttachParticles("sp-Fireball-conjure", spell.caster);
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Sunburst OnBeginRound");
        }
        public override void OnBeginProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Sunburst OnBeginProjectile");
            SetProjectileParticles(projectile, AttachParticles("sp-Searing Light", projectile));
        }
        public override void OnEndProjectile(SpellPacketBody spell, GameObjectBody projectile, int index_of_target)
        {
            Logger.Info("Sunburst OnEndProjectile");
            SpawnParticles("sp-Sunburst", spell.aoeCenter);
            var remove_list = new List<GameObjectBody>();
            spell.duration = 100 * spell.casterLevel;
            var dam = Dice.D6;
            var dam2 = Dice.Parse("6d6");
            dam = dam.WithCount(Math.Min(25, spell.casterLevel));
            // game.particles_end( projectile.obj_get_int( obj_f_projectile_part_sys_id ) )
            foreach (var target_item in spell.Targets)
            {
                AttachParticles("sp-Searing Light-Hit", target_item.Object);
                if (target_item.Object.IsMonsterCategory(MonsterCategory.undead) || target_item.Object.IsMonsterCategory(MonsterCategory.plant) || target_item.Object.IsMonsterCategory(MonsterCategory.ooze))
                {
                    if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam, DamageType.PositiveEnergy, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.AddCondition("sp-Glitterdust Blindness", spell.spellId, spell.duration, 0);
                        // destruction of any undead creature specifically harmed by bright light if it fails its save
                        // vulnerability to sunlight: Bodak	aversion to daylight: Nightwalker	sunlight or daylight powerlessness: none
                        var undead_list = new[] { 14328, 14958 };
                        foreach (var undead in undead_list)
                        {
                            if (undead == target_item.Object.GetNameId())
                            {
                                target_item.Object.FloatMesFileLine("mes/combat.mes", 7000);
                                target_item.Object.KillWithDeathEffect();
                            }

                        }

                    }

                }
                else
                {
                    var light_sensitive = 0;
                    var light_damage = 0;
                    // a creature to which sunlight is harmful or unnatural takes double damage
                    // light sensitivity: orcs, half-orcs and kobolds	light blindness: drow (and that drow ranger too!)
                    if (target_item.Object.IsMonsterSubtype(MonsterSubtype.orc) || target_item.Object.IsMonsterSubtype(MonsterSubtype.half_orc) || (is_kobold(target_item.Object)) || (is_drow(target_item.Object)))
                    {
                        light_damage = dam2.Count;
                        dam2 = dam2.WithCount(dam2.Count * 2);
                        light_sensitive = 1;
                    }

                    if (target_item.Object.ReflexSaveAndDamage(spell.caster, spell.dc, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, dam2, DamageType.Magic, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId))
                    {
                        // saving throw successful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                    }
                    else
                    {
                        // saving throw unsuccessful
                        target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                        target_item.Object.AddCondition("sp-Glitterdust Blindness", spell.spellId, spell.duration, 0);
                    }

                    if (light_sensitive == 1)
                    {
                        dam2 = dam2.WithCount(light_damage);
                    }

                }

                remove_list.Add(target_item.Object);
            }

            spell.RemoveTargets(remove_list);
            spell.EndSpell();
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Sunburst OnEndSpellCast");
        }
        public bool is_kobold(GameObjectBody target)
        {
            Logger.Info("is_kobold");
            // all kobolds are the humanoid type, the reptilian subtype, are small, and have the alertness feat
            if (target.IsMonsterCategory(MonsterCategory.humanoid) && target.IsMonsterSubtype(MonsterSubtype.reptilian))
            {
                if (GameSystems.Stat.DispatchGetSizeCategory(target) == SizeCategory.Small)
                {
                    if (target.HasFeat(FeatId.ALERTNESS))
                    {
                        return true;
                    }

                }

            }

            return false;
        }
        public bool is_drow(GameObjectBody target)
        {
            Logger.Info("is_drow");
            // all drow are elvish npcs with white hair, with spell resistance, that usually worship Lolth, and that usually are evil
            var hair_list = new[] { 906, 922, 938, 954, 970, 986, 1002, 1018, 898, 914, 930, 946, 962, 978, 994, 1010 }; // white hair (for elves)
            var hair_style = target.GetInt(obj_f.critter_hair_style);
            var alignment = target.GetAlignment();
            if (target.type == ObjectType.npc)
            {
                if ((target.GetRace() == RaceId.elf) || target.IsMonsterSubtype(MonsterSubtype.elf)) // that drow ranger does NOT have a subtype of 'mc_subtype_elf'!
                {
                    foreach (var hair_color in hair_list)
                    {
                        if (hair_color == hair_style)
                        {
                            if (target.GetDeity() == DeityId.LOLTH) // Lolth (but of course!)
                            {
                                return true;
                            }
                            else if (target.D20Query(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance)) // check critter Spell Resistance (drow have 'Monster Spell Resistance')
                            {
                                if (!SR_status(target)) // check if Spell Resistance is from a condition or item (not really necessary for NPCs, but check just in case!)
                                {
                                    return true;
                                }

                            }
                            else if (((alignment.IsEvil()))) // not perfect, but evil elvish npcs with white hair should count for something!
                            {
                                return true;
                            }

                        }

                    }

                }

            }

            return false;
        }
        public bool SR_status(GameObjectBody target)
        {
            Logger.Info("SR_status");
            // conditions that give Spell Resistance
            // if target.d20_query_has_spell_condition(sp_Spell_Resistance) == 1:	## check for Spell Resistance spell:  DOES NOT WORK!  Needs a fix.
            // return 1
            // if target.has_feat(feat_diamond_soul):			## check for Diamond Soul feat for monks:  DOES NOT WORK!
            // return 1
            if (target.GetStat(Stat.level_monk) >= 13) // Monk fix
            {
                return true;
            }
            else if ((target.ItemWornAt(EquipSlot.WeaponPrimary).GetNameId() == 4999) || (target.ItemWornAt(EquipSlot.WeaponSecondary).GetNameId() == 4999)) // Paladin Holy Sword
            {
                return true;
            }
            else if (target.ItemWornAt(EquipSlot.Necklace).GetNameId() == 12669) // Scarab of Protection
            {
                return true;
            }
            else if (target.ItemWornAt(EquipSlot.Robes) != null)
            {
                var robe_list = new[] { 6219, 6401, 6402, 6403 }; // Senshock's Robes and Robes of the Archmagi
                var robe_name = target.ItemWornAt(EquipSlot.Robes).GetNameId();
                foreach (var robe in robe_list)
                {
                    if (robe == robe_name)
                    {
                        return true;
                    }

                }

            }
            else if (target.ItemWornAt(EquipSlot.Cloak) != null)
            {
                var cape_list = new[] { 6286, 6345, 6714, 6715, 6716, 6717 }; // Mantles of Spell Resistance
                var cape_name = target.ItemWornAt(EquipSlot.Cloak).GetNameId();
                foreach (var cape in cape_list)
                {
                    if (cape == cape_name)
                    {
                        return true;
                    }

                }

            }

            // elif target.item_worn_at(5) != OBJ_HANDLE_NULL:		## Need help to add armor crafted with Spell Resistance!
            // armor_worn = target.item_worn_at(5)
            // return 1
            return false;
        }

    }
}
