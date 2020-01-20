using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.ObjScript;

namespace ConvertMapToText
{
    public static class JsonWriterObjectExtensions
    {
        public static void WriteField(this Utf8JsonWriter writer, obj_f field, object value)
        {
            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

            writer.WritePropertyName(field.ToString());

            // Special cases for fields
            switch (field)
            {
                case obj_f.location:
                case obj_f.critter_teleport_dest:
                    writer.WriteTile(locXY.fromField(unchecked((ulong) (long) value)));
                    return;
                case obj_f.flags:
                    WriteFlags(writer, (ObjectFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.spell_flags:
                    WriteFlags(writer, (SpellFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.size:
                    WriteEnum<SizeCategory>(writer, value);
                    return;
                case obj_f.material:
                    WriteEnum<Material>(writer, value);
                    return;
                case obj_f.conditions:
                    break;
                case obj_f.condition_arg0:
                    break;
                case obj_f.secretdoor_flags:
                    WriteFlags(writer, (SecretDoorFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.attack_types_idx:
                    break;
                case obj_f.attack_bonus_idx:
                    break;
                case obj_f.strategy_state:
                    break;
                case obj_f.portal_flags:
                    WriteFlags(writer, (PortalFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.container_flags:
                    WriteFlags(writer, (ContainerFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.scenery_flags:
                    WriteFlags(writer, (SceneryFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.scenery_teleport_to:
                    break;
                case obj_f.projectile_flags_combat:
                    break;
                case obj_f.projectile_flags_combat_damage:
                    break;
                case obj_f.projectile_part_sys_id:
                    break;
                case obj_f.item_flags:
                    WriteFlags(writer, (ItemFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.item_spell_idx:
                    break;
                case obj_f.item_spell_idx_flags:
                    break;
                case obj_f.item_spell_charges_idx:
                    break;
                case obj_f.item_ai_action:
                    break;
                case obj_f.item_wear_flags:
                    WriteFlags(writer, (ItemWearFlag) (int) value);
                    return;
                case obj_f.item_material_slot:
                    break;
                case obj_f.item_quantity:
                    break;
                case obj_f.item_pad_wielder_condition_array:
                    break;
                case obj_f.item_pad_wielder_argument_array:
                    break;
                case obj_f.weapon_flags:
                    WriteFlags(writer, (WeaponFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.weapon_ammo_type:
                    break;
                case obj_f.weapon_ammo_consumption:
                    break;
                case obj_f.weapon_crit_hit_chart:
                    break;
                case obj_f.weapon_attacktype:
                    break;
                case obj_f.weapon_damage_dice:
                    break;
                case obj_f.weapon_animtype:
                    break;
                case obj_f.weapon_type:
                    break;
                case obj_f.weapon_crit_range:
                    break;
                case obj_f.ammo_flags:
                    WriteFlags(writer, (AmmoFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.ammo_type:
                    break;
                case obj_f.money_flags:
                    WriteFlags(writer, (MoneyFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.food_flags:
                    WriteFlags(writer, (FoodFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.generic_flags:
                    WriteFlags(writer, (GenericFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.critter_flags:
                    WriteFlags(writer, (CritterFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.critter_flags2:
                    WriteFlags(writer, (CritterFlag2) unchecked((uint) (int) value));
                    return;
                case obj_f.critter_abilities_idx:
                    break;
                case obj_f.critter_level_idx:
                    break;
                case obj_f.critter_race:
                    break;
                case obj_f.critter_gender:
                    break;
                case obj_f.critter_age:
                    break;
                case obj_f.critter_height:
                    break;
                case obj_f.critter_weight:
                    break;
                case obj_f.critter_experience:
                    break;
                case obj_f.critter_alignment:
                    break;
                case obj_f.critter_deity:
                    break;
                case obj_f.critter_domain_1:
                    break;
                case obj_f.critter_domain_2:
                    break;
                case obj_f.critter_alignment_choice:
                    break;
                case obj_f.critter_school_specialization:
                    break;
                case obj_f.critter_spells_known_idx:
                    break;
                case obj_f.critter_spells_memorized_idx:
                    break;
                case obj_f.critter_spells_cast_idx:
                    break;
                case obj_f.critter_feat_idx:
                    break;
                case obj_f.critter_feat_count_idx:
                    break;
                case obj_f.critter_fleeing_from:
                    break;
                case obj_f.critter_portrait:
                    break;
                case obj_f.critter_money_idx:
                    break;
                case obj_f.critter_inventory_num:
                    break;
                case obj_f.critter_inventory_list_idx:
                    break;
                case obj_f.critter_inventory_source:
                    break;
                case obj_f.critter_description_unknown:
                    break;
                case obj_f.critter_follower_idx:
                    break;
                case obj_f.critter_teleport_map:
                    break;
                case obj_f.critter_death_time:
                    break;
                case obj_f.critter_skill_idx:
                    break;
                case obj_f.critter_reach:
                    break;
                case obj_f.critter_subdual_damage:
                    break;
                case obj_f.critter_pad_i_4:
                    break;
                case obj_f.critter_pad_i_5:
                    break;
                case obj_f.critter_sequence:
                    break;
                case obj_f.critter_hair_style:
                    break;
                case obj_f.critter_strategy:
                    break;
                case obj_f.critter_monster_category:
                    break;
                case obj_f.critter_damage_idx:
                    break;
                case obj_f.critter_attacks_idx:
                    break;
                case obj_f.critter_seen_maplist:
                    break;
                case obj_f.critter_end:
                    break;
                case obj_f.pc_begin:
                    break;
                case obj_f.pc_flags:
                    break;
                case obj_f.pc_player_name:
                    break;
                case obj_f.pc_global_flags:
                    break;
                case obj_f.pc_global_variables:
                    break;
                case obj_f.pc_voice_idx:
                    break;
                case obj_f.pc_roll_count:
                    break;
                case obj_f.pc_weaponslots_idx:
                    break;
                case obj_f.npc_flags:
                    WriteFlags(writer, (NpcFlag) unchecked((uint) (int) value));
                    return;
                case obj_f.npc_ai_data:
                    break;
                case obj_f.npc_combat_focus:
                    break;
                case obj_f.npc_who_hit_me_last:
                    break;
                case obj_f.npc_waypoints_idx:
                    break;
                case obj_f.npc_waypoint_current:
                    break;
                case obj_f.npc_standpoint_day_INTERNAL_DO_NOT_USE:
                    break;
                case obj_f.npc_standpoint_night_INTERNAL_DO_NOT_USE:
                    break;
                case obj_f.npc_faction:
                    break;
                case obj_f.npc_retail_price_multiplier:
                    break;
                case obj_f.npc_substitute_inventory:
                    break;
                case obj_f.npc_reaction_base:
                    break;
                case obj_f.npc_challenge_rating:
                    break;
                case obj_f.npc_reaction_pc_idx:
                    break;
                case obj_f.npc_reaction_level_idx:
                    break;
                case obj_f.npc_reaction_time_idx:
                    break;
                case obj_f.npc_generator_data:
                    break;
                case obj_f.npc_ai_list_idx:
                    break;
                case obj_f.npc_save_reflexes_bonus:
                    break;
                case obj_f.npc_save_fortitude_bonus:
                    break;
                case obj_f.npc_save_willpower_bonus:
                    break;
                case obj_f.npc_ac_bonus:
                    break;
                case obj_f.npc_add_mesh:
                    break;
                case obj_f.npc_waypoint_anim:
                    break;
                case obj_f.npc_ai_flags64:
                    break;
                case obj_f.npc_hitdice_idx:
                    break;
                case obj_f.npc_ai_list_type_idx:
                    break;
                case obj_f.npc_standpoints:
                    break;
                case obj_f.trap_flags:
                    WriteFlags(writer, (TrapFlag) unchecked((uint) (int) value));
                    return;
            }

            switch (fieldDef.type)
            {
                case ObjectFieldType.Int32:
                    writer.WriteNumberValue((int) value);
                    break;
                case ObjectFieldType.Int64:
                    writer.WriteNumberValue((long) value);
                    break;
                case ObjectFieldType.AbilityArray:
                case ObjectFieldType.Int32Array:
                {
                    writer.WriteStartArray();
                    var sparseArray = (SparseArray<int>) value;
                    foreach (var t in sparseArray)
                    {
                        writer.WriteNumberValue(t);
                    }

                    writer.WriteEndArray();
                }
                    break;
                case ObjectFieldType.Int64Array:
                {
                    writer.WriteStartArray();
                    var sparseArray = (SparseArray<long>) value;
                    foreach (var t in sparseArray)
                    {
                        writer.WriteNumberValue(t);
                    }

                    writer.WriteEndArray();
                }
                    break;
                case ObjectFieldType.ScriptArray:
                    WriteScriptArray(writer, (SparseArray<ObjectScript>) value);
                    break;
                case ObjectFieldType.String:
                    writer.WriteStringValue((string) value);
                    break;
                case ObjectFieldType.Obj:
                    writer.WriteObjectIdValue((ObjectId) value);
                    break;
                case ObjectFieldType.ObjArray:
                    writer.WriteObjectIdArrayValue((List<ObjectId>) value);
                    break;
                case ObjectFieldType.SpellArray:
                {
                    var spells = (List<SpellStoreData>) value;
                    writer.WriteStartArray();
                    foreach (var spellStoreData in spells)
                    {
                        var spellName = GameSystems.Spell.GetSpellEnumName(spellStoreData.spellEnum);
                        writer.WriteStringValue(spellName);
                    }

                    writer.WriteEndArray();
                }
                    break;
                case ObjectFieldType.Float32:
                    writer.WriteNumberValue((float) value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteScriptArray(Utf8JsonWriter writer, SparseArray<ObjectScript> scriptArray)
        {
            writer.WriteStartObject();
            scriptArray.ForEachIndex(index =>
            {
                var evt = (ObjScriptEvent) index;
                var objectScript = scriptArray[index];
                if (objectScript.scriptId != 0 || objectScript.counters != 0)
                {
                    writer.WriteStartObject(evt.ToString());
                    if (objectScript.scriptId != 0)
                    {
                        writer.WriteNumber("script", objectScript.scriptId);
                    }

                    if (objectScript.counters != 0)
                    {
                        var counters = BitConverter.GetBytes(objectScript.counters);
                        writer.WriteStartArray("counters");
                        foreach (var counter in counters)
                        {
                            writer.WriteNumberValue(counter);
                        }

                        writer.WriteEndArray();
                    }

                    writer.WriteEndObject();
                }
            });

            writer.WriteEndObject();
        }

        private static void WriteFlags<T>(Utf8JsonWriter writer, T flags) where T : Enum
        {
            writer.WriteStartArray();
            foreach (var enumValue in (T[]) Enum.GetValues(typeof(T)))
            {
                if (flags.HasFlag(enumValue))
                {
                    writer.WriteStringValue(enumValue.ToString());
                }
            }

            writer.WriteEndArray();
        }

        private static void WriteEnum<T>(Utf8JsonWriter writer, object literal) where T : Enum
        {
            var enumVal = (T) literal;
            writer.WriteStringValue(enumVal.ToString());
        }

        public static void WriteObjectIdValue(this Utf8JsonWriter writer, ObjectId objectId)
        {
            if (objectId.IsNull)
            {
                writer.WriteNullValue();
                return;
            }

            if (!objectId.IsPermanent)
            {
                throw new ArgumentException("Found an Object-ID that is non-permanent: " + objectId);
            }

            writer.WriteStringValue(objectId.guid.ToString());
        }

        public static void WriteObjectIdArrayValue(this Utf8JsonWriter writer, List<ObjectId> objectIds)
        {
            writer.WriteStartArray();

            foreach (var objectId in objectIds)
            {
                writer.WriteObjectIdValue(objectId);
            }

            writer.WriteEndArray();
        }

        public static void WriteTile(this Utf8JsonWriter writer, locXY tile)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(tile.locx);
            writer.WriteNumberValue(tile.locy);
            writer.WriteEndArray();
        }

        public static void WriteTile(this Utf8JsonWriter writer, LocAndOffsets tile)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(tile.location.locx);
            writer.WriteNumberValue(tile.location.locy);
            writer.WriteNumberValue(tile.off_x);
            writer.WriteNumberValue(tile.off_y);
            writer.WriteEndArray();
        }
    }
}