using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.GameObject
{
    public enum ObjectFieldType : uint
    {
        None = 0,
        BeginSection = 1,
        EndSection = 2,
        Int32 = 3,
        Int64 = 4,
        AbilityArray = 5,
        UnkArray = 6, // Not used by anything in ToEE
        Int32Array = 7,
        Int64Array = 8,
        ScriptArray = 9,
        Unk2Array = 10, // Not used by anything in ToEE
        String = 11,
        Obj = 12,
        ObjArray = 13,
        SpellArray = 14,
        Float32 = 15
    }

    public struct ObjectFieldDef
    {
        // Name of this field
        public string name;

        // The index of this field in the property
        // collection of prototype objects
        public int protoPropIdx;

        // The idx of this array field (starting at 0 for the first array) or -1 if this is
        // not an array field
        public int arrayIdx;

        // The idx of the bitmap 32-bit block that contains the bit indicating whether
        // an object instance has this field or not
        public int bitmapBlockIdx;

        // The bit within the bitmap block identified by bitmapBlockIdx
        public byte bitmapBitIdx;

        // A bitmask to easily test for the bit identified by bitmapBitIdx
        public uint bitmapMask;

        // Number of entries in the properties collection for this field
        public int storedInPropColl;

        public ObjectFieldType type;
    }

    public static class ObjectFields
    {
        private const int sSectionCount = 20;

        private static readonly Dictionary<obj_f, ObjectFieldType> sFieldTypeMapping =
            new Dictionary<obj_f, ObjectFieldType>
            {
                {obj_f.begin, ObjectFieldType.BeginSection},
                {obj_f.location, ObjectFieldType.Int64},
                {obj_f.offset_x, ObjectFieldType.Float32},
                {obj_f.offset_y, ObjectFieldType.Float32},
                {obj_f.shadow_art_2d, ObjectFieldType.Int32},
                {obj_f.blit_flags, ObjectFieldType.Int32},
                {obj_f.blit_color, ObjectFieldType.Int32},
                {obj_f.transparency, ObjectFieldType.Int32},
                {obj_f.model_scale, ObjectFieldType.Int32},
                {obj_f.light_flags, ObjectFieldType.Int32},
                {obj_f.light_material, ObjectFieldType.Int32},
                {obj_f.light_color, ObjectFieldType.Int32},
                {obj_f.light_radius, ObjectFieldType.Float32},
                {obj_f.light_angle_start, ObjectFieldType.Float32},
                {obj_f.light_angle_end, ObjectFieldType.Float32},
                {obj_f.light_type, ObjectFieldType.Int32},
                {obj_f.light_facing_X, ObjectFieldType.Float32},
                {obj_f.light_facing_Y, ObjectFieldType.Float32},
                {obj_f.light_facing_Z, ObjectFieldType.Float32},
                {obj_f.light_offset_X, ObjectFieldType.Float32},
                {obj_f.light_offset_Y, ObjectFieldType.Float32},
                {obj_f.light_offset_Z, ObjectFieldType.Float32},
                {obj_f.flags, ObjectFieldType.Int32},
                {obj_f.spell_flags, ObjectFieldType.Int32},
                {obj_f.name, ObjectFieldType.Int32},
                {obj_f.description, ObjectFieldType.Int32},
                {obj_f.size, ObjectFieldType.Int32},
                {obj_f.hp_pts, ObjectFieldType.Int32},
                {obj_f.hp_adj, ObjectFieldType.Int32},
                {obj_f.hp_damage, ObjectFieldType.Int32},
                {obj_f.material, ObjectFieldType.Int32},
                {obj_f.scripts_idx, ObjectFieldType.ScriptArray},
                {obj_f.sound_effect, ObjectFieldType.Int32},
                {obj_f.category, ObjectFieldType.Int32},
                {obj_f.rotation, ObjectFieldType.Float32},
                {obj_f.speed_walk, ObjectFieldType.Float32},
                {obj_f.speed_run, ObjectFieldType.Float32},
                {obj_f.base_mesh, ObjectFieldType.Int32},
                {obj_f.base_anim, ObjectFieldType.Int32},
                {obj_f.radius, ObjectFieldType.Float32},
                {obj_f.render_height_3d, ObjectFieldType.Float32},
                {obj_f.conditions, ObjectFieldType.Int32Array},
                {obj_f.condition_arg0, ObjectFieldType.Int32Array},
                {obj_f.permanent_mods, ObjectFieldType.Int32Array},
                {obj_f.initiative, ObjectFieldType.Int32},
                {obj_f.dispatcher, ObjectFieldType.Int32},
                {obj_f.subinitiative, ObjectFieldType.Int32},
                {obj_f.secretdoor_flags, ObjectFieldType.Int32},
                {obj_f.secretdoor_effectname, ObjectFieldType.Int32},
                {obj_f.secretdoor_dc, ObjectFieldType.Int32},
                {obj_f.pad_i_7, ObjectFieldType.Int32},
                {obj_f.pad_i_8, ObjectFieldType.Int32},
                {obj_f.pad_i_9, ObjectFieldType.Int32},
                {obj_f.pad_i_0, ObjectFieldType.Int32},
                {obj_f.offset_z, ObjectFieldType.Float32},
                {obj_f.rotation_pitch, ObjectFieldType.Float32},
                {obj_f.pad_f_3, ObjectFieldType.Float32},
                {obj_f.pad_f_4, ObjectFieldType.Float32},
                {obj_f.pad_f_5, ObjectFieldType.Float32},
                {obj_f.pad_f_6, ObjectFieldType.Float32},
                {obj_f.pad_f_7, ObjectFieldType.Float32},
                {obj_f.pad_f_8, ObjectFieldType.Float32},
                {obj_f.pad_f_9, ObjectFieldType.Float32},
                {obj_f.pad_f_0, ObjectFieldType.Float32},
                {obj_f.pad_i64_0, ObjectFieldType.Int64},
                {obj_f.pad_i64_1, ObjectFieldType.Int64},
                {obj_f.pad_i64_2, ObjectFieldType.Int64},
                {obj_f.pad_i64_3, ObjectFieldType.Int64},
                {obj_f.pad_i64_4, ObjectFieldType.Int64},
                {obj_f.last_hit_by, ObjectFieldType.Obj},
                {obj_f.pad_obj_1, ObjectFieldType.Obj},
                {obj_f.pad_obj_2, ObjectFieldType.Obj},
                {obj_f.pad_obj_3, ObjectFieldType.Obj},
                {obj_f.pad_obj_4, ObjectFieldType.Obj},
                {obj_f.permanent_mod_data, ObjectFieldType.Int32Array},
                {obj_f.attack_types_idx, ObjectFieldType.Int32Array},
                {obj_f.attack_bonus_idx, ObjectFieldType.Int32Array},
                {obj_f.strategy_state, ObjectFieldType.Int32Array},
                {obj_f.pad_ias_4, ObjectFieldType.Int32Array},
                {obj_f.pad_i64as_0, ObjectFieldType.Int64Array},
                {obj_f.pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.pad_i64as_2, ObjectFieldType.Int64Array},
                {obj_f.pad_i64as_3, ObjectFieldType.Int64Array},
                {obj_f.pad_i64as_4, ObjectFieldType.Int64Array},
                {obj_f.pad_objas_0, ObjectFieldType.ObjArray},
                {obj_f.pad_objas_1, ObjectFieldType.ObjArray},
                {obj_f.pad_objas_2, ObjectFieldType.ObjArray},
                {obj_f.end, ObjectFieldType.EndSection},
                {obj_f.portal_begin, ObjectFieldType.BeginSection},
                {obj_f.portal_flags, ObjectFieldType.Int32},
                {obj_f.portal_lock_dc, ObjectFieldType.Int32},
                {obj_f.portal_key_id, ObjectFieldType.Int32},
                {obj_f.portal_notify_npc, ObjectFieldType.Int32},
                {obj_f.portal_pad_i_1, ObjectFieldType.Int32},
                {obj_f.portal_pad_i_2, ObjectFieldType.Int32},
                {obj_f.portal_pad_i_3, ObjectFieldType.Int32},
                {obj_f.portal_pad_i_4, ObjectFieldType.Int32},
                {obj_f.portal_pad_i_5, ObjectFieldType.Int32},
                {obj_f.portal_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.portal_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.portal_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.portal_end, ObjectFieldType.EndSection},
                {obj_f.container_begin, ObjectFieldType.BeginSection},
                {obj_f.container_flags, ObjectFieldType.Int32},
                {obj_f.container_lock_dc, ObjectFieldType.Int32},
                {obj_f.container_key_id, ObjectFieldType.Int32},
                {obj_f.container_inventory_num, ObjectFieldType.Int32},
                {obj_f.container_inventory_list_idx, ObjectFieldType.ObjArray},
                {obj_f.container_inventory_source, ObjectFieldType.Int32},
                {obj_f.container_notify_npc, ObjectFieldType.Int32},
                {obj_f.container_pad_i_1, ObjectFieldType.Int32},
                {obj_f.container_pad_i_2, ObjectFieldType.Int32},
                {obj_f.container_pad_i_3, ObjectFieldType.Int32},
                {obj_f.container_pad_i_4, ObjectFieldType.Int32},
                {obj_f.container_pad_i_5, ObjectFieldType.Int32},
                {obj_f.container_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.container_pad_obj_2, ObjectFieldType.Obj},
                {obj_f.container_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.container_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.container_pad_objas_1, ObjectFieldType.ObjArray},
                {obj_f.container_end, ObjectFieldType.EndSection},
                {obj_f.scenery_begin, ObjectFieldType.BeginSection},
                {obj_f.scenery_flags, ObjectFieldType.Int32},
                {obj_f.scenery_pad_obj_0, ObjectFieldType.Obj},
                {obj_f.scenery_respawn_delay, ObjectFieldType.Int32},
                {obj_f.scenery_pad_i_0, ObjectFieldType.Int32},
                {obj_f.scenery_pad_i_1, ObjectFieldType.Int32},
                {obj_f.scenery_teleport_to, ObjectFieldType.Int32},
                {obj_f.scenery_pad_i_4, ObjectFieldType.Int32},
                {obj_f.scenery_pad_i_5, ObjectFieldType.Int32},
                {obj_f.scenery_pad_obj_1, ObjectFieldType.Int32},
                {obj_f.scenery_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.scenery_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.scenery_end, ObjectFieldType.EndSection},
                {obj_f.projectile_begin, ObjectFieldType.BeginSection},
                {obj_f.projectile_flags_combat, ObjectFieldType.Int32},
                {obj_f.projectile_flags_combat_damage, ObjectFieldType.Int32},
                {obj_f.projectile_parent_weapon, ObjectFieldType.Obj},
                {obj_f.projectile_parent_ammo, ObjectFieldType.Obj},
                {obj_f.projectile_part_sys_id, ObjectFieldType.Int32},
                {obj_f.projectile_acceleration_x, ObjectFieldType.Float32},
                {obj_f.projectile_acceleration_y, ObjectFieldType.Float32},
                {obj_f.projectile_acceleration_z, ObjectFieldType.Float32},
                {obj_f.projectile_pad_i_4, ObjectFieldType.Int32},
                {obj_f.projectile_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.projectile_pad_obj_2, ObjectFieldType.Obj},
                {obj_f.projectile_pad_obj_3, ObjectFieldType.Obj},
                {obj_f.projectile_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.projectile_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.projectile_pad_objas_1, ObjectFieldType.ObjArray},
                {obj_f.projectile_end, ObjectFieldType.EndSection},
                {obj_f.item_begin, ObjectFieldType.BeginSection},
                {obj_f.item_flags, ObjectFieldType.Int32},
                {obj_f.item_parent, ObjectFieldType.Obj},
                {obj_f.item_weight, ObjectFieldType.Int32},
                {obj_f.item_worth, ObjectFieldType.Int32},
                {obj_f.item_inv_aid, ObjectFieldType.Int32},
                {obj_f.item_inv_location, ObjectFieldType.Int32},
                {obj_f.item_ground_mesh, ObjectFieldType.Int32},
                {obj_f.item_ground_anim, ObjectFieldType.Int32},
                {obj_f.item_description_unknown, ObjectFieldType.Int32},
                {obj_f.item_description_effects, ObjectFieldType.Int32},
                {obj_f.item_spell_idx, ObjectFieldType.SpellArray},
                {obj_f.item_spell_idx_flags, ObjectFieldType.Int32},
                {obj_f.item_spell_charges_idx, ObjectFieldType.Int32},
                {obj_f.item_ai_action, ObjectFieldType.Int32},
                {obj_f.item_wear_flags, ObjectFieldType.Int32},
                {obj_f.item_material_slot, ObjectFieldType.Int32},
                {obj_f.item_quantity, ObjectFieldType.Int32},
                {obj_f.item_pad_i_1, ObjectFieldType.Int32},
                {obj_f.item_pad_i_2, ObjectFieldType.Int32},
                {obj_f.item_pad_i_3, ObjectFieldType.Int32},
                {obj_f.item_pad_i_4, ObjectFieldType.Int32},
                {obj_f.item_pad_i_5, ObjectFieldType.Int32},
                {obj_f.item_pad_i_6, ObjectFieldType.Int32},
                {obj_f.item_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.item_pad_obj_2, ObjectFieldType.Obj},
                {obj_f.item_pad_obj_3, ObjectFieldType.Obj},
                {obj_f.item_pad_obj_4, ObjectFieldType.Obj},
                {obj_f.item_pad_obj_5, ObjectFieldType.Obj},
                {obj_f.item_pad_wielder_condition_array, ObjectFieldType.Int32Array},
                {obj_f.item_pad_wielder_argument_array, ObjectFieldType.Int32Array},
                {obj_f.item_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.item_pad_i64as_2, ObjectFieldType.Int64Array},
                {obj_f.item_pad_objas_1, ObjectFieldType.ObjArray},
                {obj_f.item_pad_objas_2, ObjectFieldType.ObjArray},
                {obj_f.item_end, ObjectFieldType.EndSection},
                {obj_f.weapon_begin, ObjectFieldType.BeginSection},
                {obj_f.weapon_flags, ObjectFieldType.Int32},
                {obj_f.weapon_range, ObjectFieldType.Int32},
                {obj_f.weapon_ammo_type, ObjectFieldType.Int32},
                {obj_f.weapon_ammo_consumption, ObjectFieldType.Int32},
                {obj_f.weapon_missile_aid, ObjectFieldType.Int32},
                {obj_f.weapon_crit_hit_chart, ObjectFieldType.Int32},
                {obj_f.weapon_attacktype, ObjectFieldType.Int32},
                {obj_f.weapon_damage_dice, ObjectFieldType.Int32},
                {obj_f.weapon_animtype, ObjectFieldType.Int32},
                {obj_f.weapon_type, ObjectFieldType.Int32},
                {obj_f.weapon_crit_range, ObjectFieldType.Int32},
                {obj_f.weapon_pad_i_1, ObjectFieldType.Int32},
                {obj_f.weapon_pad_i_2, ObjectFieldType.Int32},
                {obj_f.weapon_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.weapon_pad_obj_2, ObjectFieldType.Obj},
                {obj_f.weapon_pad_obj_3, ObjectFieldType.Obj},
                {obj_f.weapon_pad_obj_4, ObjectFieldType.Obj},
                {obj_f.weapon_pad_obj_5, ObjectFieldType.Obj},
                {obj_f.weapon_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.weapon_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.weapon_end, ObjectFieldType.EndSection},
                {obj_f.ammo_begin, ObjectFieldType.BeginSection},
                {obj_f.ammo_flags, ObjectFieldType.Int32},
                {obj_f.ammo_quantity, ObjectFieldType.Int32},
                {obj_f.ammo_type, ObjectFieldType.Int32},
                {obj_f.ammo_pad_i_1, ObjectFieldType.Int32},
                {obj_f.ammo_pad_i_2, ObjectFieldType.Int32},
                {obj_f.ammo_pad_obj_1, ObjectFieldType.Obj},
                {obj_f.ammo_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.ammo_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.ammo_end, ObjectFieldType.EndSection},
                {obj_f.armor_begin, ObjectFieldType.BeginSection},
                {obj_f.armor_flags, ObjectFieldType.Int32},
                {obj_f.armor_ac_adj, ObjectFieldType.Int32},
                {obj_f.armor_max_dex_bonus, ObjectFieldType.Int32},
                {obj_f.armor_arcane_spell_failure, ObjectFieldType.Int32},
                {obj_f.armor_armor_check_penalty, ObjectFieldType.Int32},
                {obj_f.armor_pad_i_1, ObjectFieldType.Int32},
                {obj_f.armor_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.armor_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.armor_end, ObjectFieldType.EndSection},
                {obj_f.money_begin, ObjectFieldType.BeginSection},
                {obj_f.money_flags, ObjectFieldType.Int32},
                {obj_f.money_quantity, ObjectFieldType.Int32},
                {obj_f.money_type, ObjectFieldType.Int32},
                {obj_f.money_pad_i_1, ObjectFieldType.Int32},
                {obj_f.money_pad_i_2, ObjectFieldType.Int32},
                {obj_f.money_pad_i_3, ObjectFieldType.Int32},
                {obj_f.money_pad_i_4, ObjectFieldType.Int32},
                {obj_f.money_pad_i_5, ObjectFieldType.Int32},
                {obj_f.money_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.money_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.money_end, ObjectFieldType.EndSection},
                {obj_f.food_begin, ObjectFieldType.BeginSection},
                {obj_f.food_flags, ObjectFieldType.Int32},
                {obj_f.food_pad_i_1, ObjectFieldType.Int32},
                {obj_f.food_pad_i_2, ObjectFieldType.Int32},
                {obj_f.food_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.food_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.food_end, ObjectFieldType.EndSection},
                {obj_f.scroll_begin, ObjectFieldType.BeginSection},
                {obj_f.scroll_flags, ObjectFieldType.Int32},
                {obj_f.scroll_pad_i_1, ObjectFieldType.Int32},
                {obj_f.scroll_pad_i_2, ObjectFieldType.Int32},
                {obj_f.scroll_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.scroll_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.scroll_end, ObjectFieldType.EndSection},
                {obj_f.key_begin, ObjectFieldType.BeginSection},
                {obj_f.key_key_id, ObjectFieldType.Int32},
                {obj_f.key_pad_i_1, ObjectFieldType.Int32},
                {obj_f.key_pad_i_2, ObjectFieldType.Int32},
                {obj_f.key_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.key_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.key_end, ObjectFieldType.EndSection},
                {obj_f.written_begin, ObjectFieldType.BeginSection},
                {obj_f.written_flags, ObjectFieldType.Int32},
                {obj_f.written_subtype, ObjectFieldType.Int32},
                {obj_f.written_text_start_line, ObjectFieldType.Int32},
                {obj_f.written_text_end_line, ObjectFieldType.Int32},
                {obj_f.written_pad_i_1, ObjectFieldType.Int32},
                {obj_f.written_pad_i_2, ObjectFieldType.Int32},
                {obj_f.written_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.written_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.written_end, ObjectFieldType.EndSection},
                {obj_f.bag_begin, ObjectFieldType.BeginSection},
                {obj_f.bag_flags, ObjectFieldType.Int32},
                {obj_f.bag_size, ObjectFieldType.Int32},
                {obj_f.bag_end, ObjectFieldType.EndSection},
                {obj_f.generic_begin, ObjectFieldType.BeginSection},
                {obj_f.generic_flags, ObjectFieldType.Int32},
                {obj_f.generic_usage_bonus, ObjectFieldType.Int32},
                {obj_f.generic_usage_count_remaining, ObjectFieldType.Int32},
                {obj_f.generic_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.generic_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.generic_end, ObjectFieldType.EndSection},
                {obj_f.critter_begin, ObjectFieldType.BeginSection},
                {obj_f.critter_flags, ObjectFieldType.Int32},
                {obj_f.critter_flags2, ObjectFieldType.Int32},
                {obj_f.critter_abilities_idx, ObjectFieldType.AbilityArray},
                {obj_f.critter_level_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_race, ObjectFieldType.Int32},
                {obj_f.critter_gender, ObjectFieldType.Int32},
                {obj_f.critter_age, ObjectFieldType.Int32},
                {obj_f.critter_height, ObjectFieldType.Int32},
                {obj_f.critter_weight, ObjectFieldType.Int32},
                {obj_f.critter_experience, ObjectFieldType.Int32},
                {obj_f.critter_pad_i_1, ObjectFieldType.Int32},
                {obj_f.critter_alignment, ObjectFieldType.Int32},
                {obj_f.critter_deity, ObjectFieldType.Int32},
                {obj_f.critter_domain_1, ObjectFieldType.Int32},
                {obj_f.critter_domain_2, ObjectFieldType.Int32},
                {obj_f.critter_alignment_choice, ObjectFieldType.Int32},
                {obj_f.critter_school_specialization, ObjectFieldType.Int32},
                {obj_f.critter_spells_known_idx, ObjectFieldType.SpellArray},
                {obj_f.critter_spells_memorized_idx, ObjectFieldType.SpellArray},
                {obj_f.critter_spells_cast_idx, ObjectFieldType.SpellArray},
                {obj_f.critter_feat_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_feat_count_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_fleeing_from, ObjectFieldType.Obj},
                {obj_f.critter_portrait, ObjectFieldType.Int32},
                {obj_f.critter_money_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_inventory_num, ObjectFieldType.Int32},
                {obj_f.critter_inventory_list_idx, ObjectFieldType.ObjArray},
                {obj_f.critter_inventory_source, ObjectFieldType.Int32},
                {obj_f.critter_description_unknown, ObjectFieldType.Int32},
                {obj_f.critter_follower_idx, ObjectFieldType.ObjArray},
                {obj_f.critter_teleport_dest, ObjectFieldType.Int64},
                {obj_f.critter_teleport_map, ObjectFieldType.Int32},
                {obj_f.critter_death_time, ObjectFieldType.Int32},
                {obj_f.critter_skill_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_reach, ObjectFieldType.Int32},
                {obj_f.critter_subdual_damage, ObjectFieldType.Int32},
                {obj_f.critter_pad_i_4, ObjectFieldType.Int32},
                {obj_f.critter_pad_i_5, ObjectFieldType.Int32},
                {obj_f.critter_sequence, ObjectFieldType.Int32},
                {obj_f.critter_hair_style, ObjectFieldType.Int32},
                {obj_f.critter_strategy, ObjectFieldType.Int32},
                {obj_f.critter_pad_i_3, ObjectFieldType.Int32},
                {obj_f.critter_monster_category, ObjectFieldType.Int64},
                {obj_f.critter_pad_i64_2, ObjectFieldType.Int64},
                {obj_f.critter_pad_i64_3, ObjectFieldType.Int64},
                {obj_f.critter_pad_i64_4, ObjectFieldType.Int64},
                {obj_f.critter_pad_i64_5, ObjectFieldType.Int64},
                {obj_f.critter_damage_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_attacks_idx, ObjectFieldType.Int32Array},
                {obj_f.critter_seen_maplist, ObjectFieldType.Int64Array},
                {obj_f.critter_pad_i64as_2, ObjectFieldType.Int64Array},
                {obj_f.critter_pad_i64as_3, ObjectFieldType.Int64Array},
                {obj_f.critter_pad_i64as_4, ObjectFieldType.Int64Array},
                {obj_f.critter_pad_i64as_5, ObjectFieldType.Int64Array},
                {obj_f.critter_end, ObjectFieldType.EndSection},
                {obj_f.pc_begin, ObjectFieldType.BeginSection},
                {obj_f.pc_flags, ObjectFieldType.Int32},
                {obj_f.pc_pad_ias_0, ObjectFieldType.Int32Array},
                {obj_f.pc_pad_i64as_0, ObjectFieldType.Int64Array},
                {obj_f.pc_player_name, ObjectFieldType.String},
                {obj_f.pc_global_flags, ObjectFieldType.Int32Array},
                {obj_f.pc_global_variables, ObjectFieldType.Int32Array},
                {obj_f.pc_voice_idx, ObjectFieldType.Int32},
                {obj_f.pc_roll_count, ObjectFieldType.Int32},
                {obj_f.pc_pad_i_2, ObjectFieldType.Int32},
                {obj_f.pc_weaponslots_idx, ObjectFieldType.Int32Array},
                {obj_f.pc_pad_ias_2, ObjectFieldType.Int32Array},
                {obj_f.pc_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.pc_end, ObjectFieldType.EndSection},
                {obj_f.npc_begin, ObjectFieldType.BeginSection},
                {obj_f.npc_flags, ObjectFieldType.Int32},
                {obj_f.npc_leader, ObjectFieldType.Obj},
                {obj_f.npc_ai_data, ObjectFieldType.Int32},
                {obj_f.npc_combat_focus, ObjectFieldType.Obj},
                {obj_f.npc_who_hit_me_last, ObjectFieldType.Obj},
                {obj_f.npc_waypoints_idx, ObjectFieldType.Int64Array},
                {obj_f.npc_waypoint_current, ObjectFieldType.Int32},
                {obj_f.npc_standpoint_day_INTERNAL_DO_NOT_USE, ObjectFieldType.Int64},
                {obj_f.npc_standpoint_night_INTERNAL_DO_NOT_USE, ObjectFieldType.Int64},
                {obj_f.npc_faction, ObjectFieldType.Int32Array},
                {obj_f.npc_retail_price_multiplier, ObjectFieldType.Int32},
                {obj_f.npc_substitute_inventory, ObjectFieldType.Obj},
                {obj_f.npc_reaction_base, ObjectFieldType.Int32},
                {obj_f.npc_challenge_rating, ObjectFieldType.Int32},
                {obj_f.npc_reaction_pc_idx, ObjectFieldType.ObjArray},
                {obj_f.npc_reaction_level_idx, ObjectFieldType.Int32Array},
                {obj_f.npc_reaction_time_idx, ObjectFieldType.Int32Array},
                {obj_f.npc_generator_data, ObjectFieldType.Int32},
                {obj_f.npc_ai_list_idx, ObjectFieldType.ObjArray},
                {obj_f.npc_save_reflexes_bonus, ObjectFieldType.Int32},
                {obj_f.npc_save_fortitude_bonus, ObjectFieldType.Int32},
                {obj_f.npc_save_willpower_bonus, ObjectFieldType.Int32},
                {obj_f.npc_ac_bonus, ObjectFieldType.Int32},
                {obj_f.npc_add_mesh, ObjectFieldType.Int32},
                {obj_f.npc_waypoint_anim, ObjectFieldType.Int32},
                {obj_f.npc_pad_i_3, ObjectFieldType.Int32},
                {obj_f.npc_pad_i_4, ObjectFieldType.Int32},
                {obj_f.npc_pad_i_5, ObjectFieldType.Int32},
                {obj_f.npc_ai_flags64, ObjectFieldType.Int64},
                {obj_f.npc_pad_i64_2, ObjectFieldType.Int64},
                {obj_f.npc_pad_i64_3, ObjectFieldType.Int64},
                {obj_f.npc_pad_i64_4, ObjectFieldType.Int64},
                {obj_f.npc_pad_i64_5, ObjectFieldType.Int64},
                {obj_f.npc_hitdice_idx, ObjectFieldType.Int32Array},
                {obj_f.npc_ai_list_type_idx, ObjectFieldType.Int32Array},
                {obj_f.npc_pad_ias_3, ObjectFieldType.Int32Array},
                {obj_f.npc_pad_ias_4, ObjectFieldType.Int32Array},
                {obj_f.npc_pad_ias_5, ObjectFieldType.Int32Array},
                {obj_f.npc_standpoints, ObjectFieldType.Int64Array},
                {obj_f.npc_pad_i64as_2, ObjectFieldType.Int64Array},
                {obj_f.npc_pad_i64as_3, ObjectFieldType.Int64Array},
                {obj_f.npc_pad_i64as_4, ObjectFieldType.Int64Array},
                {obj_f.npc_pad_i64as_5, ObjectFieldType.Int64Array},
                {obj_f.npc_end, ObjectFieldType.EndSection},
                {obj_f.trap_begin, ObjectFieldType.BeginSection},
                {obj_f.trap_flags, ObjectFieldType.Int32},
                {obj_f.trap_difficulty, ObjectFieldType.Int32},
                {obj_f.trap_pad_i_2, ObjectFieldType.Int32},
                {obj_f.trap_pad_ias_1, ObjectFieldType.Int32Array},
                {obj_f.trap_pad_i64as_1, ObjectFieldType.Int64Array},
                {obj_f.trap_end, ObjectFieldType.EndSection},
                {obj_f.total_normal, ObjectFieldType.None},
                {obj_f.transient_begin, ObjectFieldType.None},
                {obj_f.render_color, ObjectFieldType.Int32},
                {obj_f.render_colors, ObjectFieldType.Int32},
                {obj_f.render_palette, ObjectFieldType.Int32},
                {obj_f.render_scale, ObjectFieldType.Int32},
                {obj_f.render_alpha, ObjectFieldType.AbilityArray},
                {obj_f.render_x, ObjectFieldType.Int32},
                {obj_f.render_y, ObjectFieldType.Int32},
                {obj_f.render_width, ObjectFieldType.Int32},
                {obj_f.render_height, ObjectFieldType.Int32},
                {obj_f.palette, ObjectFieldType.Int32},
                {obj_f.color, ObjectFieldType.Int32},
                {obj_f.colors, ObjectFieldType.Int32},
                {obj_f.render_flags, ObjectFieldType.Int32},
                {obj_f.temp_id, ObjectFieldType.Int32},
                {obj_f.light_handle, ObjectFieldType.Int32},
                {obj_f.overlay_light_handles, ObjectFieldType.Int32Array},
                {obj_f.internal_flags, ObjectFieldType.Int32},
                {obj_f.find_node, ObjectFieldType.Int32},
                {obj_f.animation_handle, ObjectFieldType.Int32},
                {obj_f.grapple_state, ObjectFieldType.Int32},
                {obj_f.transient_end, ObjectFieldType.None},
                {obj_f.type, ObjectFieldType.Int32},
                {obj_f.prototype_handle, ObjectFieldType.Obj}
            };

        private static readonly obj_f[] sSectionStarts =
        {
            obj_f.begin,
            obj_f.portal_begin,
            obj_f.container_begin,
            obj_f.scenery_begin,
            obj_f.projectile_begin,
            obj_f.item_begin,
            obj_f.weapon_begin,
            obj_f.ammo_begin,
            obj_f.armor_begin,
            obj_f.money_begin,
            obj_f.food_begin,
            obj_f.scroll_begin,
            obj_f.key_begin,
            obj_f.written_begin,
            obj_f.bag_begin,
            obj_f.generic_begin,
            obj_f.critter_begin,
            obj_f.pc_begin,
            obj_f.npc_begin,
            obj_f.trap_begin
        };

        private static readonly ImmutableArray<obj_f>[] FieldsByType = new ImmutableArray<obj_f>[ObjectTypes.Count];

        static ObjectFields()
        {
            int curProtoPropIdx = 0, curArrayIdx = 0;
            int objProtoPropCount = 0, objArrayCount = 0;
            int critterArrayCount = 0, critterProtoPropCount = 0;
            int itemArrayCount = 0, itemProtoPropCount = 0;

            Span<int> sectionFirstProtoPropIdx = stackalloc int[sSectionCount];

            // Create the full property definitions
            for (var i = 0; i < mFieldDefs.Length; ++i)
            {
                var field = (obj_f) i;

                ref var objectFieldDef = ref mFieldDefs[i];
                // Initialize a few of the properties
                objectFieldDef.protoPropIdx = -1;
                objectFieldDef.arrayIdx = -1;
                objectFieldDef.bitmapBlockIdx = -1;

                // Determine the name
                objectFieldDef.name = field.ToString();

                // Determine the type
                if (!sFieldTypeMapping.TryGetValue(field, out var fieldType))
                {
                    throw new Exception($"No type for {field}");
                }

                objectFieldDef.type = fieldType;

                // Determine the width in the property collection
                objectFieldDef.storedInPropColl = GetPropCollSize(field, fieldType);
            }

            // For all normal fields, calculate property bitmap indices
            for (obj_f field = obj_f.begin; field < obj_f.total_normal; field = (obj_f) ((int) field + 1))
            {
                var fieldType = mFieldDefs[(int) field].type;

                // Handle section markers
                if (fieldType == ObjectFieldType.BeginSection)
                {
                    switch (field)
                    {
                        case obj_f.begin:
                            curProtoPropIdx = 0;
                            curArrayIdx = 0;
                            break;
                        case obj_f.portal_begin:
                        case obj_f.container_begin:
                        case obj_f.scenery_begin:
                        case obj_f.projectile_begin:
                        case obj_f.item_begin:
                        case obj_f.critter_begin:
                        case obj_f.trap_begin:
                            curProtoPropIdx = objProtoPropCount;
                            curArrayIdx = objArrayCount;
                            break;
                        case obj_f.pc_begin:
                        case obj_f.npc_begin:
                            curProtoPropIdx = critterProtoPropCount;
                            curArrayIdx = critterArrayCount;
                            break;
                        case obj_f.scroll_begin:
                        case obj_f.key_begin:
                        case obj_f.written_begin:
                        case obj_f.bag_begin:
                        case obj_f.generic_begin:
                        case obj_f.weapon_begin:
                        case obj_f.ammo_begin:
                        case obj_f.armor_begin:
                        case obj_f.money_begin:
                        case obj_f.food_begin:
                            curProtoPropIdx = itemProtoPropCount;
                            curArrayIdx = itemArrayCount;
                            break;
                    }

                    var idx = GetSectionStartIdx(field);
                    sectionFirstProtoPropIdx[idx] = curProtoPropIdx;
                }
                else if (fieldType == ObjectFieldType.EndSection)
                {
                    switch (field)
                    {
                        case obj_f.end:
                            objArrayCount = curArrayIdx;
                            objProtoPropCount = curProtoPropIdx;
                            break;
                        case obj_f.item_end:
                            itemArrayCount = curArrayIdx;
                            itemProtoPropCount = curProtoPropIdx;
                            break;
                        case obj_f.critter_end:
                            critterArrayCount = curArrayIdx;
                            critterProtoPropCount = curProtoPropIdx;
                            break;
                    }
                }
                else
                {
                    int fieldIdx = (int) field - 1;

                    //  Calculate the bitmap block that contains the bit for this field
                    int bitmapBlockIdx = fieldIdx / 32;
                    int bitmapBitIdx = fieldIdx % 32;

                    ref var fieldDef = ref mFieldDefs[(int) field];

                    fieldDef.bitmapBlockIdx = bitmapBlockIdx;
                    fieldDef.bitmapBitIdx = (byte) bitmapBitIdx;
                    fieldDef.bitmapMask = (uint) (1 << bitmapBitIdx);
                    fieldDef.protoPropIdx = curProtoPropIdx++;

                    if (IsArrayType(fieldType))
                    {
                        fieldDef.arrayIdx = curArrayIdx++;
                    }
                }
            }

            // For each object type, calculate the number of bitmap blocks that are needed
            foreach (var type in ObjectTypes.All)
            {
                // Search for the highest prop idx among the type's fields
                int highestIdx = -1;
                var count = 0;
                IterateTypeFields(type, field =>
                {
                    if (mFieldDefs[(int) field].bitmapBlockIdx > highestIdx)
                    {
                        highestIdx = mFieldDefs[(int) field].bitmapBlockIdx;
                    }

                    count++;
                    return true;
                });

                // Now find all fields belonging to the type and collect them in an immutable array
                var fieldList = ImmutableArray.CreateBuilder<obj_f>(count);
                IterateTypeFields(type, field =>
                {
                    fieldList.Add(field);
                    return true;
                });

                mPropCollSizePerType[(int) type] = count;
                mBitmapBlocksPerType[(int) type] = highestIdx + 1;
                FieldsByType[(int) type] = fieldList.MoveToImmutable();
            }
        }

        public static ref readonly ObjectFieldDef GetFieldDef(obj_f field) => ref mFieldDefs[(int) field];

        public static ReadOnlySpan<ObjectFieldDef> GetFieldDefs() => mFieldDefs;

        public static bool DoesTypeSupportField(ObjectType type, obj_f field)
        {
            var fieldType = mFieldDefs[(int) field].type;

            // Section markers are not supported for storage
            if (fieldType == ObjectFieldType.None
                || fieldType == ObjectFieldType.BeginSection
                || fieldType == ObjectFieldType.EndSection)
            {
                return false;
            }

            // Properties that are supported by all object types
            if (field > obj_f.begin & field < obj_f.end
                || field > obj_f.transient_begin && field < obj_f.transient_end
                || field == obj_f.type
                || field == obj_f.prototype_handle)
            {
                return true;
            }

            switch (type)
            {
                case ObjectType.portal:
                    return field > obj_f.portal_begin && field < obj_f.portal_end;
                case ObjectType.container:
                    return field > obj_f.container_begin && field < obj_f.container_end;
                case ObjectType.scenery:
                    return field > obj_f.scenery_begin && field < obj_f.scenery_end;
                case ObjectType.projectile:
                    return field > obj_f.projectile_begin && field < obj_f.projectile_end;
                case ObjectType.weapon:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.weapon_begin && field < obj_f.weapon_end;
                case ObjectType.ammo:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.ammo_begin && field < obj_f.ammo_end;
                case ObjectType.armor:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.armor_begin && field < obj_f.armor_end;
                case ObjectType.money:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.money_begin && field < obj_f.money_end;
                case ObjectType.food:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.food_begin && field < obj_f.food_end;
                case ObjectType.scroll:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.scroll_begin && field < obj_f.scroll_end;
                case ObjectType.key:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.key_begin && field < obj_f.key_end;
                case ObjectType.written:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.written_begin && field < obj_f.written_end;
                case ObjectType.bag:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.bag_begin && field < obj_f.bag_end;
                case ObjectType.generic:
                    if (field > obj_f.item_begin && field < obj_f.item_end)
                        return true;
                    return field > obj_f.generic_begin && field < obj_f.generic_end;
                case ObjectType.pc:
                    if (field > obj_f.critter_begin && field < obj_f.critter_end)
                        return true;
                    return field > obj_f.pc_begin && field < obj_f.pc_end;
                case ObjectType.npc:
                    if (field > obj_f.critter_begin && field < obj_f.critter_end)
                        return true;
                    return field > obj_f.npc_begin && field < obj_f.npc_end;
                case ObjectType.trap:
                    return field > obj_f.trap_begin && field < obj_f.trap_end;
                default:
                    return false;
            }
        }

        // Gets the type of the given field
        public static ObjectFieldType GetType(obj_f field)
        {
            return GetFieldDef(field).type;
        }

        public static bool IsTransient(obj_f field)
        {
            return field > obj_f.transient_begin & field < obj_f.transient_end;
        }

        /**
         * Returns the number of bitmap blocks needed for the given type.
         */
        public static int GetBitmapBlockCount(ObjectType type)
        {
            return mBitmapBlocksPerType[(int) type];
        }

        /**
         * Returns the number of properties supported by the given type.
         */
        public static int GetSupportedFieldCount(ObjectType type)
        {
            return mPropCollSizePerType[(int) type];
        }

        public static ImmutableArray<obj_f> GetTypeFields(ObjectType type)
        {
            return FieldsByType[(int) type];
        }

        public static bool IterateTypeFields(ObjectType type, Func<obj_f, bool> callback)
        {
            IterateFieldRange(obj_f.begin, obj_f.end, callback);

            switch (type)
            {
                case ObjectType.portal:
                    return IterateFieldRange(obj_f.portal_begin, obj_f.portal_end, callback);
                case ObjectType.container:
                    return IterateFieldRange(obj_f.container_begin, obj_f.container_end, callback);
                case ObjectType.scenery:
                    return IterateFieldRange(obj_f.scenery_begin, obj_f.scenery_end, callback);
                case ObjectType.projectile:
                    return IterateFieldRange(obj_f.projectile_begin, obj_f.projectile_end, callback);
                case ObjectType.weapon:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.weapon_begin, obj_f.weapon_end, callback);
                case ObjectType.ammo:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.ammo_begin, obj_f.ammo_end, callback);
                case ObjectType.armor:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.armor_begin, obj_f.armor_end, callback);
                case ObjectType.money:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.money_begin, obj_f.money_end, callback);
                case ObjectType.food:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.food_begin, obj_f.food_end, callback);
                case ObjectType.scroll:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.scroll_begin, obj_f.scroll_end, callback);
                case ObjectType.key:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.key_begin, obj_f.key_end, callback);
                case ObjectType.written:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.written_begin, obj_f.written_end, callback);
                case ObjectType.bag:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.bag_begin, obj_f.bag_end, callback);
                case ObjectType.generic:
                    if (!IterateFieldRange(obj_f.item_begin, obj_f.item_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.generic_begin, obj_f.generic_end, callback);
                case ObjectType.pc:
                    if (!IterateFieldRange(obj_f.critter_begin, obj_f.critter_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.pc_begin, obj_f.pc_end, callback);
                case ObjectType.npc:
                    if (!IterateFieldRange(obj_f.critter_begin, obj_f.critter_end, callback))
                        return false;
                    return IterateFieldRange(obj_f.npc_begin, obj_f.npc_end, callback);
                case ObjectType.trap:
                    return IterateFieldRange(obj_f.trap_begin, obj_f.trap_end, callback);
                default:
                    return false;
            }
        }

        public static bool IsArrayType(ObjectFieldType type)
        {
            return type == ObjectFieldType.AbilityArray
                   || type == ObjectFieldType.UnkArray
                   || type == ObjectFieldType.Int32Array
                   || type == ObjectFieldType.Int64Array
                   || type == ObjectFieldType.ScriptArray
                   || type == ObjectFieldType.Unk2Array
                   || type == ObjectFieldType.ObjArray
                   || type == ObjectFieldType.SpellArray;
        }

        private static readonly ObjectFieldDef[] mFieldDefs = new ObjectFieldDef[430];

        // The number of bitmap blocks needed to be allocated by type
        private static readonly int[] mBitmapBlocksPerType = new int[ObjectTypes.Count];

        // The number of possible property storage locations per type
        public static readonly int[] mPropCollSizePerType = new int[ObjectTypes.Count];

        private static int GetPropCollSize(obj_f field, ObjectFieldType type)
        {
            // Handle special cases
            if (field == obj_f.critter_abilities_idx)
            {
                return 6;
            }
            else if (field == obj_f.render_alpha)
            {
                return 4;
            }
            else if (field >= obj_f.transient_begin)
            {
                return 0;
            }

            // All other fields are just 1 entry, except the section markers
            if (type == ObjectFieldType.None
                || type == ObjectFieldType.BeginSection
                || type == ObjectFieldType.EndSection)
            {
                return 0;
            }

            return 1;
        }

        private static bool IterateFieldRange(obj_f rangeStart, obj_f rangeEnd, Func<obj_f, bool> callback)
        {
            // The iteration is exclusive of the start and end
            obj_f field = (obj_f) ((int) rangeStart + 1);

            while (field < rangeEnd)
            {
                if (!callback(field))
                {
                    return false;
                }

                field = (obj_f) ((int) field + 1);
            }

            return true;
        }

        private static int GetSectionStartIdx(obj_f sectionStart)
        {
            for (var i = 0; i < sSectionStarts.Length; ++i)
            {
                if (sSectionStarts[i] == sectionStart)
                {
                    return i;
                }
            }

            throw new Exception($"Unknown section start: {sectionStart}");
        }

        public static object ReadFieldValue(obj_f field, BinaryReader reader)
        {
            var type = GetType(field);
            return ReadFieldValue(type, reader);
        }

        public static object ReadFieldValue(ObjectFieldType type, BinaryReader reader)
        {
            byte dataPresent;
            switch (type)
            {
                case ObjectFieldType.Int32:
                    return reader.ReadInt32();
                case ObjectFieldType.Float32:
                    return reader.ReadSingle();
                case ObjectFieldType.Int64:
                    dataPresent = reader.ReadByte();

                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    return reader.ReadInt64();
                case ObjectFieldType.Obj:
                    dataPresent = reader.ReadByte();

                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    var objIdValue = reader.ReadObjectId();
                    if (!objIdValue.IsPersistable())
                    {
                        throw new Exception($"Read an invalid object id {objIdValue}.");
                    }

                    return objIdValue;
                case ObjectFieldType.String:
                    dataPresent = reader.ReadByte();
                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    // The string length excludes the null-byte, but the data does include it
                    var strLen = reader.ReadInt32();
                    Span<byte> str = stackalloc byte[strLen + 1];
                    reader.Read(str);
                    return Encoding.Default.GetString(str.Slice(0, str.Length - 1));
                case ObjectFieldType.AbilityArray:
                case ObjectFieldType.Int32Array:
                    return ReadSparseArray<int>(reader);
                case ObjectFieldType.Int64Array:
                    return ReadSparseArray<long>(reader);
                case ObjectFieldType.ScriptArray:
                    return ReadSparseArray<ObjectScript>(reader);
                case ObjectFieldType.ObjArray:
                    return ReadSparseArrayAsList(reader, 24, ReadObjectId, false);
                case ObjectFieldType.SpellArray:
                    return ReadSparseArrayAsList(reader, 32, ReadSpellStoreData, false);
                default:
                    throw new Exception($"Cannot deserialize field type {type}");
            }
        }

        private static SparseArray<T> ReadSparseArray<T>(BinaryReader reader) where T : struct
        {
            var dataPresent = reader.ReadByte();
            if (dataPresent == 0)
            {
                return null;
            }

            return SparseArray<T>.ReadFrom(reader);
        }

        private static List<T> ReadSparseArrayAsList<T>(BinaryReader reader, int itemSize,
            SparseArrayConverter.ItemReader<T> itemsReader, bool keepGaps)
        {
            var dataPresent = reader.ReadByte();
            if (dataPresent == 0)
            {
                return null;
            }

            return SparseArrayConverter.ReadFrom(reader, itemSize, itemsReader, keepGaps);
        }

        private static ObjectId ReadObjectId(BinaryReader reader) => reader.ReadObjectId();

        private static SpellStoreData ReadSpellStoreData(BinaryReader reader)
        {
            var item = new SpellStoreData();
            item.spellEnum = reader.ReadInt32();
            item.classCode = reader.ReadInt32();
            item.spellLevel = reader.ReadInt32();
            var state = reader.ReadInt32();
            item.spellStoreState.usedUp = (state & 0x100) != 0;
            item.spellStoreState.spellStoreType = (SpellStoreType) (state & 0xFF);
            item.metaMagicData = MetaMagicData.Unpack(reader.ReadUInt32());
            item.pad1 = reader.ReadUInt32();
            item.pad2 = reader.ReadUInt32();
            item.pad3 = reader.ReadUInt32();
            return item;
        }

    }
}