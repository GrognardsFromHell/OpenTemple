using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TabFiles;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.Protos
{
    internal static class ProtoColumns
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private static readonly Dictionary<int, IProtoColumnParser> Columns;

        static ProtoColumns()
        {
            Columns = new Dictionary<int, IProtoColumnParser>();

            Columns[0] = new IdParser();
            Columns[1] = new SkipParser(); // Redundant object_type since it's derived from the proto id
            Columns[2] = new Int32Parser(obj_f.shadow_art_2d);
            Columns[3] = new Int32Parser(obj_f.blit_flags);
            Columns[4] = new Int32Parser(obj_f.blit_color);
            Columns[5] = new Int32Parser(obj_f.transparency);
            Columns[6] = new Int32Parser(obj_f.model_scale);
            // Columns[7] = new Int32FlagsParser<uint>(obj_f.light_flags, lightFlagsMapping);
            Columns[8] = new Int32Parser(obj_f.light_material);
            Columns[9] = new CharParser(obj_f.light_color);
            Columns[10] = new FloatParser(obj_f.light_radius);
            Columns[11] = new FloatParser(obj_f.light_angle_start);
            Columns[12] = new FloatParser(obj_f.light_angle_end);
            Columns[13] = new Int32Parser(obj_f.light_type);
            Columns[14] = new FloatParser(obj_f.light_facing_X);
            Columns[15] = new FloatParser(obj_f.light_facing_Y);
            Columns[16] = new FloatParser(obj_f.light_facing_Z);
            Columns[17] = new FloatParser(obj_f.light_offset_X);
            Columns[18] = new FloatParser(obj_f.light_offset_Y);
            Columns[19] = new FloatParser(obj_f.light_offset_Z);
            var objectFlagMapping = new Dictionary<string, ObjectFlag>
            {
                {"OF_DESTROYED", ObjectFlag.DESTROYED},
                {"OF_OFF", ObjectFlag.OFF},
                {"OF_FLAT", ObjectFlag.FLAT},
                {"OF_TEXT", ObjectFlag.TEXT},
                {"OF_SEE_THROUGH", ObjectFlag.SEE_THROUGH},
                {"OF_SHOOT_THROUGH", ObjectFlag.SHOOT_THROUGH},
                {"OF_TRANSLUCENT", ObjectFlag.TRANSLUCENT},
                {"OF_SHRUNK", ObjectFlag.SHRUNK},
                {"OF_DONTDRAW", ObjectFlag.DONTDRAW},
                {"OF_INVISIBLE", ObjectFlag.INVISIBLE},
                {"OF_NO_BLOCK", ObjectFlag.NO_BLOCK},
                {"OF_CLICK_THROUGH", ObjectFlag.CLICK_THROUGH},
                {"OF_INVENTORY", ObjectFlag.INVENTORY},
                {"OF_DYNAMIC", ObjectFlag.DYNAMIC},
                {"OF_PROVIDES_COVER", ObjectFlag.PROVIDES_COVER},
                {"OF_RANDOM_SIZE", ObjectFlag.RANDOM_SIZE},
                {"OF_NOHEIGHT", ObjectFlag.NOHEIGHT},
                {"OF_WADING", ObjectFlag.WADING},
                {"OF_UNUSED_40000", ObjectFlag.UNUSED_40000},
                {"OF_STONED", ObjectFlag.STONED},
                {"OF_DONTLIGHT", ObjectFlag.DONTLIGHT},
                {"OF_TEXT_FLOATER", ObjectFlag.TEXT_FLOATER},
                {"OF_INVULNERABLE", ObjectFlag.INVULNERABLE},
                {"OF_EXTINCT", ObjectFlag.EXTINCT},
                {"OF_TRAP_PC", ObjectFlag.TRAP_PC},
                {"OF_TRAP_SPOTTED", ObjectFlag.TRAP_SPOTTED},
                {"OF_DISALLOW_WADING", ObjectFlag.DISALLOW_WADING},
                {"OF_UNUSED_08000000", ObjectFlag.UNUSED_08000000},
                {"OF_HEIGHT_SET", ObjectFlag.HEIGHT_SET},
                {"OF_ANIMATED_DEAD", ObjectFlag.ANIMATED_DEAD},
                {"OF_TELEPORTED", ObjectFlag.TELEPORTED},
                {"OF_RADIUS_SET", ObjectFlag.RADIUS_SET},
            };
            Columns[20] = new Int32FlagsParser<ObjectFlag>(obj_f.flags, objectFlagMapping);
            var spellFlagMapping = new Dictionary<string, SpellFlag>
            {
                {"OSF_INVISIBLE", SpellFlag.INVISIBLE},
                {"OSF_FLOATING", SpellFlag.FLOATING},
                {"OSF_BODY_OF_AIR", SpellFlag.BODY_OF_AIR},
                {"OSF_BODY_OF_EARTH", SpellFlag.BODY_OF_EARTH},
                {"OSF_BODY_OF_FIRE", SpellFlag.BODY_OF_FIRE},
                {"OSF_BODY_OF_WATER", SpellFlag.BODY_OF_WATER},
                {"OSF_DETECTING_MAGIC", SpellFlag.DETECTING_MAGIC},
                {"OSF_DETECTING_ALIGNMENT", SpellFlag.DETECTING_ALIGNMENT},
                {"OSF_DETECTING_TRAPS", SpellFlag.DETECTING_TRAPS},
                {"OSF_DETECTING_INVISIBLE", SpellFlag.DETECTING_INVISIBLE},
                {"OSF_SHIELDED", SpellFlag.SHIELDED},
                {"OSF_ANTI_MAGIC_SHELL", SpellFlag.ANTI_MAGIC_SHELL},
                {"OSF_BONDS_OF_MAGIC", SpellFlag.BONDS_OF_MAGIC},
                {"OSF_FULL_REFLECTION", SpellFlag.FULL_REFLECTION},
                {"OSF_SUMMONED", SpellFlag.SUMMONED},
                {"OSF_ILLUSION", SpellFlag.ILLUSION},
                {"OSF_STONED", SpellFlag.STONED},
                {"OSF_POLYMORPHED", SpellFlag.POLYMORPHED},
                {"OSF_MIRRORED", SpellFlag.MIRRORED},
                {"OSF_SHRUNK", SpellFlag.SHRUNK},
                {"OSF_PASSWALLED", SpellFlag.PASSWALLED},
                {"OSF_WATER_WALKING", SpellFlag.WATER_WALKING},
                {"OSF_MAGNETIC_INVERSION", SpellFlag.MAGNETIC_INVERSION},
                {"OSF_CHARMED", SpellFlag.CHARMED},
                {"OSF_ENTANGLED", SpellFlag.ENTANGLED},
                {"OSF_SPOKEN_WITH_DEAD", SpellFlag.SPOKEN_WITH_DEAD},
                {"OSF_TEMPUS_FUGIT", SpellFlag.TEMPUS_FUGIT},
                {"OSF_MIND_CONTROLLED", SpellFlag.MIND_CONTROLLED},
                {"OSF_DRUNK", SpellFlag.DRUNK},
                {"OSF_ENSHROUDED", SpellFlag.ENSHROUDED},
                {"OSF_FAMILIAR", SpellFlag.FAMILIAR},
                {"OSF_HARDENED_HANDS", SpellFlag.HARDENED_HANDS},
            };
            Columns[21] = new Int32FlagsParser<SpellFlag>(obj_f.spell_flags, spellFlagMapping);
            Columns[22] = new Int32Parser(obj_f.name);
            Columns[23] = new Int32Parser(obj_f.description);
            var sizeMapping = new Dictionary<string, SizeCategory>
            {
                {"size_none", SizeCategory.None},
                {"size_fine", SizeCategory.Fine},
                {"size_diminutive", SizeCategory.Diminutive},
                {"size_tiny", SizeCategory.Tiny},
                {"size_small", SizeCategory.Small},
                {"size_medium", SizeCategory.Medium},
                {"size_large", SizeCategory.Large},
                {"size_huge", SizeCategory.Huge},
                {"size_gargantuan", SizeCategory.Gargantuan},
                {"size_colossal", SizeCategory.Colossal},
            };
            Columns[24] = new Int32EnumParser<SizeCategory>(obj_f.size, sizeMapping);
            Columns[25] = new Int32Parser(obj_f.hp_pts);
            Columns[26] = new Int32Parser(obj_f.hp_damage);
            var materialMapping = new Dictionary<string, Material>
            {
                {"mat_stone", Material.stone},
                {"mat_brick", Material.brick},
                {"mat_wood", Material.wood},
                {"mat_plant", Material.plant},
                {"mat_flesh", Material.flesh},
                {"mat_metal", Material.metal},
                {"mat_glass", Material.glass},
                {"mat_cloth", Material.cloth},
                {"mat_liquid", Material.liquid},
                {"mat_paper", Material.paper},
                {"mat_gas", Material.gas},
                {"mat_force", Material.force},
                {"mat_fire", Material.fire},
                {"mat_powder", Material.powder},
            };
            Columns[27] = new Int32EnumParser<Material>(obj_f.material, materialMapping);
            Columns[28] = new SkipParser();
            Columns[29] = new Int32Parser(obj_f.sound_effect);
            Columns[30] = new Int32Parser(obj_f.category);
            Columns[31] = new FloatParser(obj_f.rotation);
            Columns[32] = new FloatParser(obj_f.speed_walk);
            Columns[33] = new FloatParser(obj_f.speed_run);
            Columns[34] = new Int32TwoFieldsParser(obj_f.base_anim, obj_f.base_mesh);
            Columns[35] = new FloatRadiusParser();
            Columns[36] = new FloatHeightParser();
            var portalFlagMapping = new Dictionary<string, PortalFlag>
            {
                {"OPF_LOCKED", PortalFlag.LOCKED},
                {"OPF_JAMMED", PortalFlag.JAMMED},
                {"OPF_MAGICALLY_HELD", PortalFlag.MAGICALLY_HELD},
                {"OPF_NEVER_LOCKED", PortalFlag.NEVER_LOCKED},
                {"OPF_ALWAYS_LOCKED", PortalFlag.ALWAYS_LOCKED},
                {"OPF_LOCKED_DAY", PortalFlag.LOCKED_DAY},
                {"OPF_LOCKED_NIGHT", PortalFlag.LOCKED_NIGHT},
                {"OPF_BUSTED", PortalFlag.BUSTED},
                {"OPF_NOT_STICKY", PortalFlag.NOT_STICKY},
                {"OPF_OPEN", PortalFlag.OPEN},
            };
            Columns[37] = new Int32FlagsParser<PortalFlag>(obj_f.portal_flags, portalFlagMapping);
            Columns[38] = new Int32Parser(obj_f.portal_lock_dc);
            Columns[39] = new Int32Parser(obj_f.portal_key_id);
            Columns[40] = new Int32Parser(obj_f.portal_notify_npc);
            var containerFlagMapping = new Dictionary<string, ContainerFlag>
            {
                {"OCOF_LOCKED", ContainerFlag.LOCKED},
                {"OCOF_JAMMED", ContainerFlag.JAMMED},
                {"OCOF_MAGICALLY_HELD", ContainerFlag.MAGICALLY_HELD},
                {"OCOF_NEVER_LOCKED", ContainerFlag.NEVER_LOCKED},
                {"OCOF_ALWAYS_LOCKED", ContainerFlag.ALWAYS_LOCKED},
                {"OCOF_LOCKED_DAY", ContainerFlag.LOCKED_DAY},
                {"OCOF_LOCKED_NIGHT", ContainerFlag.LOCKED_NIGHT},
                {"OCOF_BUSTED", ContainerFlag.BUSTED},
                {"OCOF_NOT_STICKY", ContainerFlag.NOT_STICKY},
                {"OCOF_INVEN_SPAWN_ONCE", ContainerFlag.INVEN_SPAWN_ONCE},
                {"OCOF_INVEN_SPAWN_INDEPENDENT", ContainerFlag.INVEN_SPAWN_INDEPENDENT},
                {"OCOF_OPEN", ContainerFlag.OPEN},
                {"OCOF_HAS_BEEN_OPENED", ContainerFlag.HAS_BEEN_OPENED},
            };
            Columns[41] = new Int32FlagsParser<ContainerFlag>(obj_f.container_flags, containerFlagMapping);
            Columns[42] = new Int32Parser(obj_f.container_lock_dc);
            Columns[43] = new Int32Parser(obj_f.container_key_id);
            Columns[44] = new InvenSrcParser();
            Columns[45] = new Int32Parser(obj_f.container_notify_npc);
            var sceneryFlagMapping = new Dictionary<string, SceneryFlag>
            {
                {"OSCF_NO_AUTO_ANIMATE", SceneryFlag.NO_AUTO_ANIMATE},
                {"OSCF_BUSTED", SceneryFlag.BUSTED},
                {"OSCF_NOCTURNAL", SceneryFlag.NOCTURNAL},
                {"OSCF_MARKS_TOWNMAP", SceneryFlag.MARKS_TOWNMAP},
                {"OSCF_IS_FIRE", SceneryFlag.IS_FIRE},
                {"OSCF_RESPAWNABLE", SceneryFlag.RESPAWNABLE},
                {"OSCF_SOUND_SMALL", SceneryFlag.SOUND_SMALL},
                {"OSCF_SOUND_MEDIUM", SceneryFlag.SOUND_MEDIUM},
                {"OSCF_SOUND_EXTRA_LARGE", SceneryFlag.SOUND_EXTRA_LARGE},
                {"OSCF_UNDER_ALL", SceneryFlag.UNDER_ALL},
                {"OSCF_RESPAWNING", SceneryFlag.RESPAWNING},
                {"OSCF_TAGGED_SCENERY", SceneryFlag.TAGGED_SCENERY},
                {"OSCF_USE_OPEN_WORLDMAP", SceneryFlag.USE_OPEN_WORLDMAP},
            };
            Columns[46] = new Int32FlagsParser<SceneryFlag>(obj_f.scenery_flags, sceneryFlagMapping);
            Columns[47] = new SkipParser();
            Columns[48] = new SkipParser();
            Columns[49] = new SkipParser();
            var itemFlagMapping = new Dictionary<string, ItemFlag>
            {
                {"OIF_IDENTIFIED", ItemFlag.IDENTIFIED},
                {"OIF_WONT_SELL", ItemFlag.WONT_SELL},
                {"OIF_IS_MAGICAL", ItemFlag.IS_MAGICAL},
                {"OIF_NO_PICKPOCKET", ItemFlag.NO_PICKPOCKET},
                {"OIF_NO_DISPLAY", ItemFlag.NO_DISPLAY},
                {"OIF_NO_DROP", ItemFlag.NO_DROP},
                {"OIF_NEEDS_SPELL", ItemFlag.NEEDS_SPELL},
                {"OIF_CAN_USE_BOX", ItemFlag.CAN_USE_BOX},
                {"OIF_NEEDS_TARGET", ItemFlag.NEEDS_TARGET},
                {"OIF_LIGHT_SMALL", ItemFlag.LIGHT_SMALL},
                {"OIF_LIGHT_MEDIUM", ItemFlag.LIGHT_MEDIUM},
                {"OIF_LIGHT_LARGE", ItemFlag.LIGHT_LARGE},
                {"OIF_LIGHT_XLARGE", ItemFlag.LIGHT_XLARGE},
                {"OIF_PERSISTENT", ItemFlag.PERSISTENT},
                {"OIF_MT_TRIGGERED", ItemFlag.MT_TRIGGERED},
                {"OIF_STOLEN", ItemFlag.STOLEN},
                {"OIF_USE_IS_THROW", ItemFlag.USE_IS_THROW},
                {"OIF_NO_DECAY", ItemFlag.NO_DECAY},
                {"OIF_UBER", ItemFlag.UBER},
                {"OIF_NO_NPC_PICKUP", ItemFlag.NO_NPC_PICKUP},
                {"OIF_NO_RANGED_USE", ItemFlag.NO_RANGED_USE},
                {"OIF_VALID_AI_ACTION", ItemFlag.VALID_AI_ACTION},
                {"OIF_DRAW_WHEN_PARENTED", ItemFlag.DRAW_WHEN_PARENTED},
                {"OIF_EXPIRES_AFTER_USE", ItemFlag.EXPIRES_AFTER_USE},
                {"OIF_NO_LOOT", ItemFlag.NO_LOOT},
                {"OIF_USES_WAND_ANIM", ItemFlag.USES_WAND_ANIM},
                {"OIF_NO_TRANSFER", ItemFlag.NO_TRANSFER},
            };
            Columns[50] = new Int32FlagsParser<ItemFlag>(obj_f.item_flags, itemFlagMapping);
            Columns[51] = new Int32Parser(obj_f.item_weight);
            Columns[52] = new Int32Parser(obj_f.item_worth);
            Columns[53] = new Int32Parser(obj_f.item_inv_aid);
            Columns[54] = new SkipParser();
            Columns[55] = new Int32Parser(obj_f.item_description_unknown);
            Columns[56] = new Int32Parser(obj_f.item_description_effects);
            Columns[57] = new SkipParser();
            Columns[58] = new SkipParser();
            Columns[59] = new SpellChargesIdxParser();
            Columns[60] = new Int32Parser(obj_f.item_ai_action);
            var itemWearFlagMapping = new Dictionary<string, ItemWearFlag>
            {
                {"OIF_WEAR_HELMET", ItemWearFlag.HELMET},
                {"OIF_WEAR_NECKLACE", ItemWearFlag.NECKLACE},
                {"OIF_WEAR_GLOVES", ItemWearFlag.GLOVES},
                {"OIF_WEAR_UNUSED_1", ItemWearFlag.UNUSED_1},
                {"OIF_WEAR_UNUSED_2", ItemWearFlag.UNUSED_2},
                {"OIF_WEAR_ARMOR", ItemWearFlag.ARMOR},
                {"OIF_WEAR_RING", ItemWearFlag.RING},
                {"OIF_WEAR_UNUSED_3", ItemWearFlag.UNUSED_3},
                {"OIF_WEAR_BOOTS", ItemWearFlag.BOOTS},
                {"OIF_WEAR_AMMO", ItemWearFlag.AMMO},
                {"OIF_WEAR_CLOAK", ItemWearFlag.CLOAK},
                {"OIF_WEAR_BUCKLER", ItemWearFlag.BUCKLER},
                {"OIF_WEAR_ROBES", ItemWearFlag.ROBES},
                {"OIF_WEAR_BRACERS", ItemWearFlag.BRACERS},
                {"OIF_WEAR_BARDIC_ITEM", ItemWearFlag.BARDIC_ITEM},
                {"OIF_WEAR_LOCKPICKS", ItemWearFlag.LOCKPICKS},
                {"OIF_WEAR_2HAND_REQUIRED", ItemWearFlag.TWOHANDED_REQUIRED},
            };
            Columns[61] = new Int32FlagsParser<ItemWearFlag>(obj_f.item_wear_flags, itemWearFlagMapping);
            Columns[62] = new Int32Parser(obj_f.item_material_slot);
            var weaponFlagMapping = new Dictionary<string, WeaponFlag>
            {
                {"OWF_LOUD", WeaponFlag.LOUD},
                {"OWF_SILENT", WeaponFlag.SILENT},
                {"OWF_UNUSED_1", WeaponFlag.UNUSED_1},
                {"OWF_UNUSED_2", WeaponFlag.UNUSED_2},
                {"OWF_THROWABLE", WeaponFlag.THROWABLE},
                {"OWF_TRANS_PROJECTILE", WeaponFlag.TRANS_PROJECTILE},
                {"OWF_BOOMERANGS", WeaponFlag.BOOMERANGS},
                {"OWF_IGNORE_RESISTANCE", WeaponFlag.IGNORE_RESISTANCE},
                {"OWF_DAMAGE_ARMOR", WeaponFlag.DAMAGE_ARMOR},
                {"OWF_DEFAULT_THROWS", WeaponFlag.DEFAULT_THROWS},
                {"OWF_RANGED_WEAPON", WeaponFlag.RANGED_WEAPON},
                {"OWF_WEAPON_LOADED", WeaponFlag.WEAPON_LOADED},
                {"OWF_MAGIC_STAFF", WeaponFlag.MAGIC_STAFF},
            };
            Columns[63] = new Int32FlagsParser<WeaponFlag>(obj_f.weapon_flags, weaponFlagMapping);
            Columns[64] = new Int32Parser(obj_f.weapon_range);
            var weaponAmmoTypeMapping = new Dictionary<string, WeaponAmmoType>
            {
                {"arrow", WeaponAmmoType.arrow},
                {"bolt", WeaponAmmoType.bolt},
                {"bullet", WeaponAmmoType.bullet},
                {"magic_missile", WeaponAmmoType.magic_missile},
                {"dagger", WeaponAmmoType.dagger},
                {"club", WeaponAmmoType.club},
                {"shortspear", WeaponAmmoType.shortspear},
                {"spear", WeaponAmmoType.spear},
                {"dart", WeaponAmmoType.dart},
                {"javelin", WeaponAmmoType.javelin},
                {"throwing_axe", WeaponAmmoType.throwing_axe},
                {"light_hammer", WeaponAmmoType.light_hammer},
                {"trident", WeaponAmmoType.trident},
                {"halfling_sai", WeaponAmmoType.halfling_sai},
                {"sai", WeaponAmmoType.sai},
                {"shuriken", WeaponAmmoType.shuriken},
                {"ball_of_fire", WeaponAmmoType.ball_of_fire},
                {"bottle", WeaponAmmoType.bottle},
            };
            Columns[65] = new Int32EnumParser<WeaponAmmoType>(obj_f.weapon_ammo_type, weaponAmmoTypeMapping);
            Columns[66] = new Int32Parser(obj_f.weapon_ammo_consumption);
            Columns[67] = new Int32Parser(obj_f.weapon_missile_aid);
            Columns[68] = new Int32Parser(obj_f.weapon_crit_hit_chart);
            Columns[69] = new D20DmgTypeParser(obj_f.weapon_attacktype);
            Columns[70] = new DiceParser(obj_f.weapon_damage_dice);
            Columns[71] = new Int32Parser(obj_f.weapon_animtype);
            var weaponTypeMapping = new Dictionary<string, WeaponType>
            {
                {"gauntlet", WeaponType.gauntlet},
                {"unarmed_strike_medium_sized_being", WeaponType.unarmed_strike_medium_sized_being},
                {"unarmed_strike_small_being", WeaponType.unarmed_strike_small_being},
                {"dagger", WeaponType.dagger},
                {"punching_dagger", WeaponType.punching_dagger},
                {"spiked_gauntlet", WeaponType.spiked_gauntlet},
                {"light_mace", WeaponType.light_mace},
                {"sickle", WeaponType.sickle},
                {"club", WeaponType.club},
                {"shortspear", WeaponType.shortspear},
                {"heavy_mace", WeaponType.heavy_mace},
                {"morningstar", WeaponType.morningstar},
                {"quarterstaff", WeaponType.quarterstaff},
                {"spear", WeaponType.spear},
                {"light_crossbow", WeaponType.light_crossbow},
                {"dart", WeaponType.dart},
                {"sling", WeaponType.sling},
                {"heavy_crossbow", WeaponType.heavy_crossbow},
                {"javelin", WeaponType.javelin},
                {"throwing_axe", WeaponType.throwing_axe},
                {"light_hammer", WeaponType.light_hammer},
                {"handaxe", WeaponType.handaxe},
                {"light_lance", WeaponType.light_lance},
                {"light_pick", WeaponType.light_pick},
                {"sap", WeaponType.sap},
                {"short_sword", WeaponType.short_sword},
                {"battleaxe", WeaponType.battleaxe},
                {"light_flail", WeaponType.light_flail},
                {"heavy_lance", WeaponType.heavy_lance},
                {"longsword", WeaponType.longsword},
                {"heavy_pick", WeaponType.heavy_pick},
                {"rapier", WeaponType.rapier},
                {"scimitar", WeaponType.scimitar},
                {"trident", WeaponType.trident},
                {"warhammer", WeaponType.warhammer},
                {"falchion", WeaponType.falchion},
                {"heavy_flail", WeaponType.heavy_flail},
                {"glaive", WeaponType.glaive},
                {"greataxe", WeaponType.greataxe},
                {"greatclub", WeaponType.greatclub},
                {"greatsword", WeaponType.greatsword},
                {"guisarme", WeaponType.guisarme},
                {"halberd", WeaponType.halberd},
                {"longspear", WeaponType.longspear},
                {"ranseur", WeaponType.ranseur},
                {"scythe", WeaponType.scythe},
                {"shortbow", WeaponType.shortbow},
                {"composite_shortbow", WeaponType.composite_shortbow},
                {"longbow", WeaponType.longbow},
                {"composite_longbow", WeaponType.composite_longbow},
                {"halfling_kama", WeaponType.halfling_kama},
                {"kukri", WeaponType.kukri},
                {"halfling_nunchaku", WeaponType.halfling_nunchaku},
                {"halfling_siangham", WeaponType.halfling_siangham},
                {"kama", WeaponType.kama},
                {"nunchaku", WeaponType.nunchaku},
                {"siangham", WeaponType.siangham},
                {"bastard_sword", WeaponType.bastard_sword},
                {"dwarven_waraxe", WeaponType.dwarven_waraxe},
                {"gnome_hooked_hammer", WeaponType.gnome_hooked_hammer},
                {"orc_double_axe", WeaponType.orc_double_axe},
                {"spike_chain", WeaponType.spike_chain},
                {"dire_flail", WeaponType.dire_flail},
                {"two_bladed_sword", WeaponType.two_bladed_sword},
                {"dwarven_urgrosh", WeaponType.dwarven_urgrosh},
                {"hand_crossbow", WeaponType.hand_crossbow},
                {"shuriken", WeaponType.shuriken},
                {"whip", WeaponType.whip},
                {"repeating_crossbow", WeaponType.repeating_crossbow},
                {"net", WeaponType.net},
                {"grapple", WeaponType.grapple},
                {"ray", WeaponType.ray},
                {"grenade", WeaponType.grenade},
            };
            Columns[72] = new Int32EnumParser<WeaponType>(obj_f.weapon_type, weaponTypeMapping, true);
            Columns[73] = new CritRangeParser(obj_f.weapon_crit_range);
            var ammoFlagMapping = new Dictionary<string, AmmoFlag>
            {
                {"OAF_NONE", AmmoFlag.NONE},
            };
            Columns[74] = new Int32FlagsParser<AmmoFlag>(obj_f.ammo_flags, ammoFlagMapping);
            Columns[75] = new Int32Parser(obj_f.ammo_quantity);
            Columns[76] = new Int32EnumParser<WeaponAmmoType>(obj_f.ammo_type, weaponAmmoTypeMapping);
            var armorTypeMapping = new Dictionary<string, ArmorTypeFlag>
            {
                {"OARF_ARMOR_TYPE_1", ArmorTypeFlag.ArmorType1},
                {"OARF_ARMOR_TYPE_2", ArmorTypeFlag.ArmorType2},
                {"OARF_HELM_TYPE_1", ArmorTypeFlag.HelmType1},
                {"OARF_HELM_TYPE_2", ArmorTypeFlag.HelmType2},
                {"OARF_ARMOR_NONE", ArmorTypeFlag.ArmorNone},
            };
            Columns[77] = new Int32FlagsParser<ArmorTypeFlag>(obj_f.armor_flags, armorTypeMapping);
            Columns[78] = new Int32Parser(obj_f.armor_ac_adj);
            Columns[79] = new Int32Parser(obj_f.armor_max_dex_bonus);
            Columns[80] = new Int32Parser(obj_f.armor_arcane_spell_failure);
            Columns[81] = new Int32Parser(obj_f.armor_armor_check_penalty);
            Columns[82] = new ArmorTypeParser();
            Columns[83] = new HelmSizeParser();
            var moneyFlagMapping = new Dictionary<string, MoneyFlag>
            {
                {"OGOF_NONE", MoneyFlag.None},
            };
            Columns[84] = new Int32FlagsParser<MoneyFlag>(obj_f.money_flags, moneyFlagMapping);
            Columns[85] = new Int32Parser(obj_f.money_quantity);
            var moneyTypeMapping = new Dictionary<string, MoneyType>
            {
                {"Copper", MoneyType.Copper},
                {"Silver", MoneyType.Silver},
                {"Gold", MoneyType.Gold},
                {"Platinum", MoneyType.Platinum},
            };
            Columns[86] = new Int32EnumParser<MoneyType>(obj_f.money_type, moneyTypeMapping);
            var foodFlagMapping = new Dictionary<string, FoodFlag>
            {
                {"OFF_NONE", FoodFlag.None},
            };
            Columns[87] = new Int32FlagsParser<FoodFlag>(obj_f.food_flags, foodFlagMapping);
// TODO: ToEE just used the spell flags here
            Columns[88] = new Int32FlagsParser<SpellFlag>(obj_f.scroll_flags, spellFlagMapping);
            Columns[89] = new Int32Parser(obj_f.key_key_id);
// TODO ToEE incorrectly used the weapon flags here
            Columns[90] = new Int32FlagsParser<WeaponFlag>(obj_f.written_flags, weaponFlagMapping);
            Columns[91] = new Int32Parser(obj_f.written_subtype);
            Columns[92] = new Int32Parser(obj_f.written_text_start_line);
            Columns[93] = new Int32Parser(obj_f.written_text_end_line);
            var bagFlagMapping = new Dictionary<string, BagFlag>
            {
                {"OBF_HOLDING_500", BagFlag.HOLDING_500},
                {"OBF_HOLDING_1000", BagFlag.HOLDING_1000},
            };
            Columns[94] = new Int32FlagsParser<BagFlag>(obj_f.bag_flags, bagFlagMapping);
            Columns[95] = new Int32Parser(obj_f.bag_size);
            var genericFlagMapping = new Dictionary<string, GenericFlag>
            {
                {"OGF_UNUSED_00000001", GenericFlag.UNUSED_00000001},
                {"OGF_IS_LOCKPICK", GenericFlag.IS_LOCKPICK},
                {"OGF_IS_TRAP_DEVICE", GenericFlag.IS_TRAP_DEVICE},
                {"OGF_UNUSED_00000008", GenericFlag.UNUSED_00000008},
                {"OGF_IS_GRENADE", GenericFlag.IS_GRENADE},
            };
            Columns[96] = new Int32FlagsParser<GenericFlag>(obj_f.generic_flags, genericFlagMapping);
            Columns[97] = new Int32Parser(obj_f.generic_usage_bonus);
            Columns[98] = new Int32Parser(obj_f.generic_usage_count_remaining);
            var critterFlagMapping = new Dictionary<string, CritterFlag>
            {
                {"OCF_IS_CONCEALED", CritterFlag.IS_CONCEALED},
                {"OCF_MOVING_SILENTLY", CritterFlag.MOVING_SILENTLY},
                {"OCF_EXPERIENCE_AWARDED", CritterFlag.EXPERIENCE_AWARDED},
                {"OCF_UNUSED_00000008", CritterFlag.UNUSED_00000008},
                {"OCF_FLEEING", CritterFlag.FLEEING},
                {"OCF_STUNNED", CritterFlag.STUNNED},
                {"OCF_PARALYZED", CritterFlag.PARALYZED},
                {"OCF_BLINDED", CritterFlag.BLINDED},
                {"OCF_HAS_ARCANE_ABILITY", CritterFlag.HAS_ARCANE_ABILITY},
                {"OCF_UNUSED_00000200", CritterFlag.UNUSED_00000200},
                {"OCF_UNUSED_00000400", CritterFlag.UNUSED_00000400},
                {"OCF_UNUSED_00000800", CritterFlag.UNUSED_00000800},
                {"OCF_SLEEPING", CritterFlag.SLEEPING},
                {"OCF_MUTE", CritterFlag.MUTE},
                {"OCF_SURRENDERED", CritterFlag.SURRENDERED},
                {"OCF_MONSTER", CritterFlag.MONSTER},
                {"OCF_SPELL_FLEE", CritterFlag.SPELL_FLEE},
                {"OCF_ENCOUNTER", CritterFlag.ENCOUNTER},
                {"OCF_COMBAT_MODE_ACTIVE", CritterFlag.COMBAT_MODE_ACTIVE},
                {"OCF_LIGHT_SMALL", CritterFlag.LIGHT_SMALL},
                {"OCF_LIGHT_MEDIUM", CritterFlag.LIGHT_MEDIUM},
                {"OCF_LIGHT_LARGE", CritterFlag.LIGHT_LARGE},
                {"OCF_LIGHT_XLARGE", CritterFlag.LIGHT_XLARGE},
                {"OCF_UNREVIVIFIABLE", CritterFlag.UNREVIVIFIABLE},
                {"OCF_UNRESSURECTABLE", CritterFlag.UNRESURRECTABLE}, // NOTE: Typo on the ToEE side
                {"OCF_UNUSED_02000000", CritterFlag.UNUSED_02000000},
                {"OCF_UNUSED_04000000", CritterFlag.UNUSED_04000000},
                {"OCF_NO_FLEE", CritterFlag.NO_FLEE},
                {"OCF_NON_LETHAL_COMBAT", CritterFlag.NON_LETHAL_COMBAT},
                {"OCF_MECHANICAL", CritterFlag.MECHANICAL},
                {"OCF_UNUSED_40000000", CritterFlag.UNUSED_40000000},
                {"OCF_FATIGUE_LIMITING", CritterFlag.FATIGUE_LIMITING},
            };
            Columns[99] = new Int32FlagsParser<CritterFlag>(obj_f.critter_flags, critterFlagMapping);
            var critterFlag2Mapping = new Dictionary<string, CritterFlag2>
            {
                {"OCF2_ITEM_STOLEN", CritterFlag2.ITEM_STOLEN},
                {"OCF2_AUTO_ANIMATES", CritterFlag2.AUTO_ANIMATES},
                {"OCF2_USING_BOOMERANG", CritterFlag2.USING_BOOMERANG},
                {"OCF2_FATIGUE_DRAINING", CritterFlag2.FATIGUE_DRAINING},
                {"OCF2_SLOW_PARTY", CritterFlag2.SLOW_PARTY},
                {"OCF2_UNUSED_00000020", CritterFlag2.UNUSED_00000020},
                {"OCF2_NO_DECAY", CritterFlag2.NO_DECAY},
                {"OCF2_NO_PICKPOCKET", CritterFlag2.NO_PICKPOCKET},
                {"OCF2_NO_BLOOD_SPLOTCHES", CritterFlag2.NO_BLOOD_SPLOTCHES},
                {"OCF2_NIGH_INVULNERABLE", CritterFlag2.NIGH_INVULNERABLE},
                {"OCF2_ELEMENTAL", CritterFlag2.ELEMENTAL},
                {"OCF2_DARK_SIGHT", CritterFlag2.DARK_SIGHT},
                {"OCF2_NO_SLIP", CritterFlag2.NO_SLIP},
                {"OCF2_NO_DISINTEGRATE", CritterFlag2.NO_DISINTEGRATE},
                {"OCF2_REACTION_0", CritterFlag2.REACTION_0},
                {"OCF2_REACTION_1", CritterFlag2.REACTION_1},
                {"OCF2_REACTION_2", CritterFlag2.REACTION_2},
                {"OCF2_REACTION_3", CritterFlag2.REACTION_3},
                {"OCF2_REACTION_4", CritterFlag2.REACTION_4},
                {"OCF2_REACTION_5", CritterFlag2.REACTION_5},
                {"OCF2_REACTION_6", CritterFlag2.REACTION_6},
                {"OCF2_TARGET_LOCK", CritterFlag2.TARGET_LOCK},
                {"OCF2_ACTION0_PAUSED", CritterFlag2.ACTION0_PAUSED},
                {"OCF2_ACTION1_PAUSED", CritterFlag2.ACTION1_PAUSED},
                {"OCF2_ACTION2_PAUSED", CritterFlag2.ACTION2_PAUSED},
                {"OCF2_ACTION3_PAUSED", CritterFlag2.ACTION3_PAUSED},
                {"OCF2_ACTION4_PAUSED", CritterFlag2.ACTION4_PAUSED},
                {"OCF2_ACTION5_PAUSED", CritterFlag2.ACTION5_PAUSED}
            };
            Columns[100] = new Int32FlagsParser<CritterFlag2>(obj_f.critter_flags2, critterFlag2Mapping);
            Columns[101] = new Int32IndexedParser(obj_f.critter_abilities_idx, 0);
            Columns[102] = new Int32IndexedParser(obj_f.critter_abilities_idx, 1);
            Columns[103] = new Int32IndexedParser(obj_f.critter_abilities_idx, 2);
            Columns[104] = new Int32IndexedParser(obj_f.critter_abilities_idx, 3);
            Columns[105] = new Int32IndexedParser(obj_f.critter_abilities_idx, 4);
            Columns[106] = new Int32IndexedParser(obj_f.critter_abilities_idx, 5);
            Columns[107] = new SkipParser();
            Columns[108] = new RaceParser();
            Columns[109] = new GenderParser();
            Columns[110] = new Int32Parser(obj_f.critter_age);
            Columns[111] = new Int32Parser(obj_f.critter_height);
            Columns[112] = new Int32Parser(obj_f.critter_weight);
            var alignmentMapping = new Dictionary<string, Alignment>
            {
                {"align_true_neutral", Alignment.TRUE_NEUTRAL},
                {"align_lawful_neutral", Alignment.LAWFUL_NEUTRAL},
                {"align_chaotic_neutral", Alignment.CHAOTIC_NEUTRAL},
                {"align_neutral_good", Alignment.NEUTRAL_GOOD},
                {"align_lawful_good", Alignment.LAWFUL_GOOD},
                {"align_chaotic_good", Alignment.CHAOTIC_GOOD},
                {"align_neutral_evil", Alignment.NEUTRAL_EVIL},
                {"align_lawful_evil", Alignment.LAWFUL_EVIL},
                {"align_chaotic_evil", Alignment.CHAOTIC_EVIL},
            };
            Columns[113] = new Int32EnumParser<Alignment>(obj_f.critter_alignment, alignmentMapping);
            Columns[114] = new DeityParser();
            var domainMapping = new Dictionary<string, DomainId>
            {
                {"NONE", DomainId.None},
                {"AIR", DomainId.Air},
                {"ANIMAL", DomainId.Animal},
                {"CHAOS", DomainId.Chaos},
                {"DEATH", DomainId.Death},
                {"DESTRUCTION", DomainId.Destruction},
                {"EARTH", DomainId.Earth},
                {"EVIL", DomainId.Evil},
                {"FIRE", DomainId.Fire},
                {"GOOD", DomainId.Good},
                {"HEALING", DomainId.Healing},
                {"KNOWLEDGE", DomainId.Knowledge},
                {"LAW", DomainId.Law},
                {"LUCK", DomainId.Luck},
                {"MAGIC", DomainId.Magic},
                {"PLANT", DomainId.Plant},
                {"PROTECTION", DomainId.Protection},
                {"STRENGTH", DomainId.Strength},
                {"SUN", DomainId.Sun},
                {"TRAVEL", DomainId.Travel},
                {"TRICKERY", DomainId.Trickery},
                {"WAR", DomainId.War},
                {"WATER", DomainId.Water},
            };
            Columns[115] = new Int32EnumParser<DomainId>(obj_f.critter_domain_1, domainMapping);
            Columns[116] = new Int32EnumParser<DomainId>(obj_f.critter_domain_2, domainMapping);
            var alignmentChoiceMapping = new Dictionary<string, AlignmentChoice>
            {
                {"Undecided", AlignmentChoice.Undecided},
                {"Positive", AlignmentChoice.Positive},
                {"Negative", AlignmentChoice.Negative},
            };
            Columns[117] = new Int32EnumParser<AlignmentChoice>(obj_f.critter_alignment_choice, alignmentChoiceMapping);
            Columns[118] = new Int32Parser(obj_f.critter_school_specialization);
            Columns[119] = new SkipParser();
            Columns[120] = new SkipParser();
            Columns[121] = new SkipParser();
            Columns[122] = new SkipParser();
            Columns[123] = new Int32Parser(obj_f.critter_portrait);
            Columns[124] = new Int32IndexedParser(obj_f.critter_money_idx, 0);
            Columns[125] = new Int32IndexedParser(obj_f.critter_money_idx, 1);
            Columns[126] = new Int32IndexedParser(obj_f.critter_money_idx, 2);
            Columns[127] = new Int32IndexedParser(obj_f.critter_money_idx, 3);
            Columns[128] = new Int32Parser(obj_f.critter_description_unknown);
            Columns[129] = new SkipParser();
            Columns[130] = new Int32Parser(obj_f.critter_reach);
            Columns[131] = new Int32Parser(obj_f.critter_subdual_damage);
            Columns[132] = new Int32IndexedParser(obj_f.critter_attacks_idx, 0);
            Columns[133] = new DiceIndexedParser(obj_f.critter_damage_idx, 0);
            var naturalAttackTypeMapping = new Dictionary<string, NaturalAttackType>
            {
                {"Bite", NaturalAttackType.Bite},
                {"Claw", NaturalAttackType.Claw},
                {"Rake", NaturalAttackType.Rake},
                {"Gore", NaturalAttackType.Gore},
                {"Slap", NaturalAttackType.Slap},
                {"Slam", NaturalAttackType.Slam},
                {"Sting", NaturalAttackType.Sting},
            };
            Columns[134] =
                new Int32EnumIndexedParser<NaturalAttackType>(obj_f.attack_types_idx, 0, naturalAttackTypeMapping);
            Columns[135] = new Int32IndexedParser(obj_f.attack_bonus_idx, 0);
            Columns[136] = new Int32IndexedParser(obj_f.critter_attacks_idx, 1);
            Columns[137] = new DiceIndexedParser(obj_f.critter_damage_idx, 1);
            Columns[138] =
                new Int32EnumIndexedParser<NaturalAttackType>(obj_f.attack_types_idx, 1, naturalAttackTypeMapping);
            Columns[139] = new Int32IndexedParser(obj_f.attack_bonus_idx, 1);
            Columns[140] = new Int32IndexedParser(obj_f.critter_attacks_idx, 2);
            Columns[141] = new DiceIndexedParser(obj_f.critter_damage_idx, 2);
            Columns[142] =
                new Int32EnumIndexedParser<NaturalAttackType>(obj_f.attack_types_idx, 2, naturalAttackTypeMapping);
            Columns[143] = new Int32IndexedParser(obj_f.attack_bonus_idx, 2);
            Columns[144] = new Int32IndexedParser(obj_f.critter_attacks_idx, 3);
            Columns[145] = new DiceIndexedParser(obj_f.critter_damage_idx, 3);
            Columns[146] =
                new Int32EnumIndexedParser<NaturalAttackType>(obj_f.attack_types_idx, 3, naturalAttackTypeMapping);
            Columns[147] = new Int32IndexedParser(obj_f.attack_bonus_idx, 3);
            Columns[148] = new HairColorParser();
            Columns[149] = new HairStyleParser();
            var pcFlagsMapping = new Dictionary<string, PcFlag>
            {
                {"OPCF_unused_1", PcFlag.unused_1},
                {"OPCF_unused_2", PcFlag.unused_2},
                {"OPCF_USE_ALT_DATA", PcFlag.USE_ALT_DATA},
                {"OPCF_unused_4", PcFlag.unused_4},
                {"OPCF_unused_5", PcFlag.unused_5},
                {"OPCF_FOLLOWER_SKILLS_ON", PcFlag.FOLLOWER_SKILLS_ON},
            };
            Columns[150] = new Int32FlagsParser<PcFlag>(obj_f.pc_flags, pcFlagsMapping);
            Columns[151] = new SkipParser();
            var npcFlagMapping = new Dictionary<string, NpcFlag>
            {
                {"ONF_EX_FOLLOWER", NpcFlag.EX_FOLLOWER},
                {"ONF_WAYPOINTS_DAY", NpcFlag.WAYPOINTS_DAY},
                {"ONF_WAYPOINTS_NIGHT", NpcFlag.WAYPOINTS_NIGHT},
                {"ONF_AI_WAIT_HERE", NpcFlag.AI_WAIT_HERE},
                {"ONF_AI_SPREAD_OUT", NpcFlag.AI_SPREAD_OUT},
                {"ONF_JILTED", NpcFlag.JILTED},
                {"ONF_LOGBOOK_IGNORES", NpcFlag.LOGBOOK_IGNORES},
                {"ONF_UNUSED_00000080", NpcFlag.UNUSED_00000080},
                {"ONF_KOS", NpcFlag.KOS},
                {"ONF_USE_ALERTPOINTS", NpcFlag.USE_ALERTPOINTS},
                {"ONF_FORCED_FOLLOWER", NpcFlag.FORCED_FOLLOWER},
                {"ONF_KOS_OVERRIDE", NpcFlag.KOS_OVERRIDE},
                {"ONF_WANDERS", NpcFlag.WANDERS},
                {"ONF_WANDERS_IN_DARK", NpcFlag.WANDERS_IN_DARK},
                {"ONF_FENCE", NpcFlag.FENCE},
                {"ONF_FAMILIAR", NpcFlag.FAMILIAR},
                {"ONF_CHECK_LEADER", NpcFlag.CHECK_LEADER},
                {"ONF_NO_EQUIP", NpcFlag.NO_EQUIP},
                {"ONF_CAST_HIGHEST", NpcFlag.CAST_HIGHEST},
                {"ONF_GENERATOR", NpcFlag.GENERATOR},
                {"ONF_GENERATED", NpcFlag.GENERATED},
                {"ONF_GENERATOR_RATE1", NpcFlag.GENERATOR_RATE1},
                {"ONF_GENERATOR_RATE2", NpcFlag.GENERATOR_RATE2},
                {"ONF_GENERATOR_RATE3", NpcFlag.GENERATOR_RATE3},
                {"ONF_DEMAINTAIN_SPELLS", NpcFlag.DEMAINTAIN_SPELLS},
                {"ONF_UNUSED_02000000", NpcFlag.UNUSED_02000000},
                {"ONF_UNUSED_04000000", NpcFlag.UNUSED_04000000},
                {"ONF_UNUSED_08000000", NpcFlag.UNUSED_08000000},
                {"ONF_BACKING_OFF", NpcFlag.BACKING_OFF},
                {"ONF_NO_ATTACK", NpcFlag.NO_ATTACK},
                {"ONF_BOSS_MONSTER", NpcFlag.BOSS_MONSTER},
                {"ONF_EXTRAPLANAR", NpcFlag.EXTRAPLANAR},
            };
            Columns[152] = new Int32FlagsParser<NpcFlag>(obj_f.npc_flags, npcFlagMapping);
            Columns[153] = new Int32Parser(obj_f.npc_ai_data);
            Columns[154] = new FactionsParser();
            Columns[155] = new Int32Parser(obj_f.npc_retail_price_multiplier);
            Columns[156] = new Int32Parser(obj_f.npc_reaction_base);
            Columns[157] = new ChallengeRatingParser(obj_f.npc_challenge_rating);
            Columns[158] = new Int32Parser(obj_f.npc_save_reflexes_bonus);
            Columns[159] = new Int32Parser(obj_f.npc_save_fortitude_bonus);
            Columns[160] = new Int32Parser(obj_f.npc_save_willpower_bonus);
            Columns[161] = new Int32Parser(obj_f.npc_ac_bonus);
            Columns[162] = new HitDiceParser(obj_f.npc_hitdice_idx);
            Columns[163] = new MonsterCategoryParser();
            Columns[164] = new MonsterSubtypesParser();
            var lootSharingTypeMapping = new Dictionary<string, LootSharingType>
            {
                {"normal", LootSharingType.normal},
                {"half_share_money_only", LootSharingType.half_share_money_only},
                {"all_arcane_scrolls_nothing_else", LootSharingType.all_arcane_scrolls_nothing_else},
                {"one_third_of_all", LootSharingType.one_third_of_all},
                {"one_fifth_of_all", LootSharingType.one_fifth_of_all},
                {"nothing", LootSharingType.nothing},
            };
            Columns[165] = new Int32EnumParser<LootSharingType>(obj_f.npc_pad_i_3, lootSharingTypeMapping);
            var trapFlagMapping = new Dictionary<string, TrapFlag>
            {
                {"OTF_UNUSED_01", TrapFlag.UNUSED_01},
                {"OTF_BUSTED", TrapFlag.BUSTED},
            };
            Columns[166] = new Int32FlagsParser<TrapFlag>(obj_f.trap_flags, trapFlagMapping);
            Columns[167] = new Int32Parser(obj_f.trap_difficulty);
            for (int i = 0; i < 20; i++)
            {
                int nameCol = 168 + i * 3;
                int arg1Col = 169 + i * 3;
                int arg2Col = 170 + i * 3;

                var condParser = new ConditionParser(nameCol, arg1Col, arg2Col);
                Columns[nameCol] = condParser;
                Columns[arg1Col] = condParser;
                Columns[arg2Col] = condParser;
            }

            for (int i = 0; i < 5; i++)
            {
                int classCol = 228 + i * 2;
                int levelCol = 229 + i * 2;
                var parser = new ClassLevelParser(classCol, levelCol);
                Columns[classCol] = parser;
                Columns[levelCol] = parser;
            }

            for (int i = 0; i < 10; i++)
            {
                int nameCol = 238 + i * 2;
                int rankCol = 239 + i * 2;
                var parser = new SkillParser(nameCol, rankCol);
                Columns[nameCol] = parser;
                Columns[rankCol] = parser;
            }

            Columns[258] = new FeatParser(0);
            Columns[259] = new FeatParser(1);
            Columns[260] = new FeatParser(2);
            Columns[261] = new FeatParser(3);
            Columns[262] = new FeatParser(4);
            Columns[263] = new FeatParser(5);
            Columns[264] = new FeatParser(6);
            Columns[265] = new FeatParser(7);
            Columns[266] = new FeatParser(8);
            Columns[267] = new FeatParser(9);
            Columns[268] = new ScriptParser(0);
            Columns[269] = new ScriptParser(1);
            Columns[270] = new ScriptParser(2);
            Columns[271] = new ScriptParser(3);
            Columns[272] = new ScriptParser(4);
            Columns[273] = new ScriptParser(5);
            Columns[274] = new ScriptParser(6);
            Columns[275] = new ScriptParser(7);
            Columns[276] = new ScriptParser(8);
            Columns[277] = new ScriptParser(9);
            Columns[278] = new ScriptParser(10);
            Columns[279] = new ScriptParser(11);
            Columns[280] = new ScriptParser(12);
            Columns[281] = new ScriptParser(13);
            Columns[282] = new ScriptParser(14);
            Columns[283] = new ScriptParser(15);
            Columns[284] = new ScriptParser(16);
            Columns[285] = new ScriptParser(17);
            Columns[286] = new ScriptParser(18);
            Columns[287] = new ScriptParser(19);
            Columns[288] = new ScriptParser(20);
            Columns[289] = new ScriptParser(21);
            Columns[290] = new ScriptParser(22);
            Columns[291] = new ScriptParser(23);
            Columns[292] = new ScriptParser(24);
            Columns[293] = new ScriptParser(25);
            Columns[294] = new ScriptParser(26);
            Columns[295] = new ScriptParser(27);
            Columns[296] = new ScriptParser(28);
            Columns[297] = new ScriptParser(29);
            Columns[298] = new ScriptParser(30);
            Columns[299] = new ScriptParser(31);
            Columns[300] = new ScriptParser(32);
            Columns[301] = new ScriptParser(33);
            Columns[302] = new ScriptParser(34);
            Columns[303] = new ScriptParser(35);
            Columns[304] = new ScriptParser(36);
            Columns[305] = new ScriptParser(37);
            Columns[306] = new ScriptParser(38);
            Columns[307] = new ScriptParser(39);
            Columns[308] = new ScriptParser(40);
            Columns[309] = new ScriptParser(41);
            Columns[310] = new ScriptParser(42);
            Columns[311] = new Int32Parser(obj_f.npc_add_mesh);
            Columns[312] = new SpellKnownParser();
            Columns[313] = new SpellKnownParser();
            Columns[314] = new SpellKnownParser();
            Columns[315] = new SpellKnownParser();
            Columns[316] = new SpellKnownParser();
            Columns[317] = new SpellKnownParser();
            Columns[318] = new SpellKnownParser();
            Columns[319] = new SpellKnownParser();
            Columns[320] = new SpellKnownParser();
            Columns[321] = new SpellKnownParser();
            Columns[322] = new SpellKnownParser();
            Columns[323] = new SpellKnownParser();
            Columns[324] = new SpellKnownParser();
            Columns[325] = new SpellKnownParser();
            Columns[326] = new SpellKnownParser();
            Columns[327] = new SpellKnownParser();
            Columns[328] = new SpellKnownParser();
            Columns[329] = new SpellKnownParser();
            Columns[330] = new SpellKnownParser();
            Columns[331] = new SpellKnownParser();
            Columns[332] = new CritterPadI4Parser();
            Columns[333] = new CritterStrategyParser();
        }

        public static void ParseColumns(int protoId, TabFileRecord record, GameObjectBody obj)
        {
            foreach (var (columnIndex, parser) in Columns)
            {
                var column = record[columnIndex];
                parser.Parse(protoId, column, obj);
            }
        }

        private interface IProtoColumnParser
        {
            void Parse(int protoId, TabFileColumn column, GameObjectBody obj);
        }

        [TempleDllLocation(0x10039360)]
        private struct IdParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
            }
        }

        [TempleDllLocation(0x101f5850)]
        private struct SkipParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                // Logger.Debug("Skipping proto {0} column '{1}'", protoId, column.AsString());
            }
        }

        [TempleDllLocation(0x10039380)]
        private struct Int32Parser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public Int32Parser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (column.TryGetInt(out var value))
                    {
                        obj.SetInt32(_field, value);
                    }
                    else
                    {
                        Logger.Warn($"Proto {protoId} has invalid value for {_field}: {column.AsString()}");
                    }
                }
            }
        }

        private delegate void TokenConsumer(ReadOnlySpan<byte> token);

        private static void IterateTokens(ReadOnlySpan<byte> columnText, TokenConsumer consumer)
        {
            // There can be at most 100 tokens
            Span<int> ranges = stackalloc int[100 * 2];
            // Delimiter is "vertical tab"
            var itemCount = TabFile.FindRangesInLine(columnText, 11, ranges, false, false);
            for (var i = 0; i < itemCount; i++)
            {
                var literal = columnText.Slice(ranges[i * 2], ranges[i * 2 + 1]);
                consumer(literal);
            }
        }

        [TempleDllLocation(0x10039560)]
        private struct Int32FlagsParser<T> : IProtoColumnParser where T : Enum, IConvertible
        {
            private readonly obj_f _field;

            private readonly EnumIntMapping _mapping;

            public Int32FlagsParser(obj_f field, Dictionary<string, T> mapping)
            {
                _field = field;
                _mapping = EnumIntMapping.Create(mapping);
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var field = _field;
                    var flags = obj.GetInt32(_field);
                    var mapping = _mapping;

                    IterateTokens(column, token =>
                    {
                        if (mapping.TryGetValue(token, out var flag))
                        {
                            flags |= flag;
                        }
                        else
                        {
                            var text = Encoding.Default.GetString(token);
                            Logger.Warn($"Proto {protoId} has invalid value for {field}: '{text}'");
                        }
                    });

                    obj.SetInt32(_field, flags);
                }
            }
        }

        [TempleDllLocation(0x10039640)]
        private struct CharParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public CharParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (column.EqualsIgnoreCase("#"))
                    {
                        obj.SetInt32(_field, 0);
                    }
                    else
                    {
                        throw new Exception($"The parser for field {_field} is broken.");
                    }
                }
            }
        }

        [TempleDllLocation(0x10039480)]
        private struct FloatParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public FloatParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var value = column.GetFloat();
                    obj.SetFloat(_field, value);
                }
            }
        }

        [TempleDllLocation(0x10039680)]
        private struct Int32EnumParser<T> : IProtoColumnParser where T : Enum
        {
            private readonly obj_f _field;
            private readonly EnumIntMapping _mapping;
            private readonly bool _ignoreCase;

            public Int32EnumParser(obj_f field, Dictionary<string, T> mapping, bool ignoreCase = false)
            {
                _field = field;
                _mapping = EnumIntMapping.Create(mapping);
                _ignoreCase = ignoreCase;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    int value;
                    if (!_ignoreCase && _mapping.TryGetValue(column, out value)
                        || _ignoreCase && _mapping.TryGetValueIgnoreCase(column, out value))
                    {
                        obj.SetInt32(_field, value);
                    }
                    else
                    {
                        Logger.Warn($"Proto {protoId} has invalid value for {_field}: '{column.AsString()}'");
                    }
                }
            }
        }

        [TempleDllLocation(0x100393c0)]
        private struct Int32TwoFieldsParser : IProtoColumnParser
        {
            private readonly obj_f _field1;
            private readonly obj_f _field2;

            public Int32TwoFieldsParser(obj_f field1, obj_f field2)
            {
                _field1 = field1;
                _field2 = field2;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    obj.SetInt32(_field1, column.GetInt());
                    obj.SetInt32(_field2, column.GetInt());
                }
            }
        }

        [TempleDllLocation(0x100394c0)]
        private struct FloatRadiusParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var radius = column.GetFloat();
                    obj.SetFloat(obj_f.radius, radius);
                    obj.SetFlag(ObjectFlag.RADIUS_SET, true);
                }
            }
        }

        [TempleDllLocation(0x10039510)]
        private struct FloatHeightParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var radius = column.GetFloat();
                    obj.SetFloat(obj_f.render_height_3d, radius);
                    obj.SetFlag(ObjectFlag.RADIUS_SET, true);
                }
            }
        }

        [TempleDllLocation(0x10039410)]
        private struct InvenSrcParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (obj.type == ObjectType.container)
                    {
                        obj.SetInt32(obj_f.container_inventory_source, column.GetInt());
                    }
                    else
                    {
                        obj.SetInt32(obj_f.critter_inventory_source, column.GetInt());
                    }
                }
            }
        }

        [TempleDllLocation(0x1003aa90)]
        private struct SpellChargesIdxParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    obj.SetInt32(obj_f.item_spell_charges_idx, column.GetInt());
                }
            }
        }

        [TempleDllLocation(0x100397d0)]
        private struct D20DmgTypeParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            [TempleDllLocation(0x100e0ae0)]
            private Int32EnumParser<DamageType> _enumParser;

            public D20DmgTypeParser(obj_f field)
            {
                _field = field;
                _enumParser = new Int32EnumParser<DamageType>(_field, new Dictionary<string, DamageType>
                {
                    {"D20DT_UNSPECIFIED", DamageType.Unspecified},
                    {"D20DT_BLUDGEONING", DamageType.Bludgeoning},
                    {"D20DT_PIERCING", DamageType.Piercing},
                    {"D20DT_SLASHING", DamageType.Slashing},
                    {"D20DT_BLUDGEONING_AND_PIERCING", DamageType.BludgeoningAndPiercing},
                    {"D20DT_PIERCING_AND_SLASHING", DamageType.PiercingAndSlashing},
                    {"D20DT_SLASHING_AND_BLUDGEONING", DamageType.SlashingAndBludgeoning},
                    {"D20DT_SLASHING_AND_BLUDGEONING_AND_PIERCING", DamageType.SlashingAndBludgeoningAndPiercing},
                    {"D20DT_ACID", DamageType.Acid},
                    {"D20DT_COLD", DamageType.Cold},
                    {"D20DT_ELECTRICITY", DamageType.Electricity},
                    {"D20DT_FIRE", DamageType.Fire},
                    {"D20DT_SONIC", DamageType.Sonic},
                    {"D20DT_NEGATIVE_ENERGY", DamageType.NegativeEnergy},
                    {"D20DT_SUBDUAL", DamageType.Subdual},
                    {"D20DT_POISON", DamageType.Poison},
                    {"D20DT_POSITIVE_ENERGY", DamageType.PositiveEnergy},
                    {"D20DT_FORCE", DamageType.Force},
                    {"D20DT_BLOOD_LOSS", DamageType.BloodLoss},
                    {"D20DT_MAGIC", DamageType.Magic},
                });
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (column.TryGetInt(out var type))
                    {
                        obj.SetInt32(_field, type);
                        throw new NotImplementedException();
                    }
                    else
                    {
                        _enumParser.Parse(protoId, column, obj);
                    }
                }
            }
        }

        [TempleDllLocation(0x10039b80)]
        private struct DiceParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public DiceParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (Dice.TryParse(column, out var dice))
                    {
                        obj.SetInt32(_field, dice.ToPacked());
                    }
                    else
                    {
                        Logger.Warn("Failed to parse dice spec {0} {1}", column.AsString(), column.Location);
                    }
                }
            }
        }

        [TempleDllLocation(0x10039ce0)]
        private struct CritRangeParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public CritRangeParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    obj.SetInt32(_field, 21 - column.GetInt());
                }
            }
        }

        [TempleDllLocation(0x1003a780)]
        private struct ArmorTypeParser : IProtoColumnParser
        {
            private static readonly Dictionary<string, ArmorFlag> Mapping = new Dictionary<string, ArmorFlag>
            {
                {"ARMOR_TYPE_LIGHT", ArmorFlag.TYPE_LIGHT},
                {"ARMOR_TYPE_MEDIUM", ArmorFlag.TYPE_MEDIUM},
                {"ARMOR_TYPE_HEAVY", ArmorFlag.TYPE_HEAVY},
                {"ARMOR_TYPE_SHIELD", ArmorFlag.TYPE_SHIELD},
            };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (EnumIntMapping.TryGetValue(column, out var flags))
                    {
                        var currentFlags = (ArmorFlag) obj.GetInt32(obj_f.armor_flags) & ~ArmorFlag.TYPE_BITMASK;
                        obj.SetInt32(obj_f.armor_flags, (int) (currentFlags | (ArmorFlag) flags));
                    }
                    else
                    {
                        throw new Exception($"Proto {protoId} has invalid armor flags '{column.AsString()}");
                    }
                }
                else if (obj.type == ObjectType.armor)
                {
                    var currentFlags = (ArmorFlag) obj.GetInt32(obj_f.armor_flags);
                    currentFlags |= ArmorFlag.TYPE_NONE;
                    obj.SetInt32(obj_f.armor_flags, (int) currentFlags);
                }
            }
        }

        [TempleDllLocation(0x1003a840)]
        private struct HelmSizeParser : IProtoColumnParser
        {
            // obj_f.armor_flags

            private static readonly Dictionary<string, ArmorFlag> Mapping = new Dictionary<string, ArmorFlag>
            {
                {"HELM_TYPE_SMALL", ArmorFlag.HELM_TYPE_SMALL},
                {"HELM_TYPE_MEDIUM", ArmorFlag.HELM_TYPE_MEDIUM},
                {"HELM_TYPE_LARGE", ArmorFlag.HELM_TYPE_LARGE},
            };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (EnumIntMapping.TryGetValue(column, out var flags))
                    {
                        var currentFlags = (ArmorFlag) obj.GetInt32(obj_f.armor_flags) & ~ArmorFlag.HELM_BITMASK;
                        obj.SetInt32(obj_f.armor_flags, (int) (currentFlags | (ArmorFlag) flags));
                    }
                    else
                    {
                        throw new Exception($"Proto {protoId} has invalid helmet size '{column.AsString()}");
                    }
                }
            }
        }

        [TempleDllLocation(0x10039d20)]
        private struct Int32IndexedParser : IProtoColumnParser
        {
            private readonly obj_f _field;
            private readonly int _index;

            public Int32IndexedParser(obj_f field, int index)
            {
                _field = field;
                _index = index;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var value = column.GetInt();
                    obj.SetInt32(_field, _index, value);
                }
            }
        }

        [TempleDllLocation(0x10039b40)]
        private struct RaceParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (GameSystems.Stat.GetRaceByEnumName(column.AsString(), out var raceId))
                    {
                        obj.SetInt32(obj_f.critter_race, (int) raceId);
                    }
                    else
                    {
                        throw new Exception($"Proto {protoId} is using invalid race '{column.AsString()}");
                    }
                }
            }
        }

        [TempleDllLocation(0x100396f0)]
        private struct GenderParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (column.EqualsIgnoreCase("female"))
                    {
                        obj.SetInt32(obj_f.critter_gender, (int) Gender.Female);
                    }
                    else if (column.EqualsIgnoreCase("male"))
                    {
                        obj.SetInt32(obj_f.critter_gender, (int) Gender.Male);
                    }
                    else
                    {
                        throw new Exception($"Invalid gender '{column.AsString()}' specified {column.Location}");
                    }
                }
            }
        }

        [TempleDllLocation(0x10039d60)]
        private struct DeityParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (DeitySystem.GetDeityFromEnglishName(column.AsString(), out var deityId))
                    {
                        obj.SetInt32(obj_f.critter_deity, (int) deityId);
                    }
                    else
                    {
                        Logger.Warn("Proto {0} has invalid deity '{1}'", protoId, column.AsString());
                    }
                }
            }
        }

        [TempleDllLocation(0x10039c70)]
        private struct DiceIndexedParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            private readonly int _index;

            public DiceIndexedParser(obj_f field, int index)
            {
                _field = field;
                _index = index;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty && Dice.TryParse(column, out var dice))
                {
                    obj.SetInt32(_field, _index, dice.ToPacked());
                }
            }
        }

        [TempleDllLocation(0x10039760)]
        private struct Int32EnumIndexedParser<T> : IProtoColumnParser where T : Enum
        {
            private readonly obj_f _field;
            private readonly int _index;
            private readonly EnumIntMapping _mapping;

            public Int32EnumIndexedParser(obj_f field, int index, Dictionary<string, T> mapping)
            {
                _field = field;
                _index = index;
                _mapping = EnumIntMapping.Create(mapping);
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (_mapping.TryGetValue(column, out var value))
                    {
                        obj.SetInt32(_field, _index, value);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        [TempleDllLocation(0x10039990)]
        private struct HairColorParser : IProtoColumnParser
        {
            private static readonly Dictionary<string, HairColor> Mapping = new Dictionary<string, HairColor>
            {
                {"Black", HairColor.Black},
                {"Blonde", HairColor.Blonde},
                {"Blue", HairColor.Blue},
                {"Brown", HairColor.Brown},
                {"Light Brown", HairColor.LightBrown},
                {"Pink", HairColor.Pink},
                {"Red", HairColor.Red},
                {"White", HairColor.White},
            };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (!EnumIntMapping.TryGetValueIgnoreCase(column, out var hairColor))
                    {
                        throw new Exception($"Proto {protoId} uses invalid hair color: '${column.AsString()}");
                    }

                    var hairSettings = HairSettings.Unpack(obj.GetInt32(obj_f.critter_hair_style));
                    hairSettings.Gender = (Gender) GameSystems.Stat.ObjStatBaseGet(obj, Stat.gender);
                    hairSettings.Race = (HairStyleRace) GameSystems.Stat.ObjStatBaseGet(obj, Stat.race);
                    hairSettings.Color = (HairColor) hairColor;

                    var packed = hairSettings.Pack();

                    obj.SetInt32(obj_f.critter_hair_style, packed);
                }
            }
        }

        [TempleDllLocation(0x10039a60)]
        private struct HairStyleParser : IProtoColumnParser
        {
            private static readonly Dictionary<string, HairStyle> Mapping = new Dictionary<string, HairStyle>
            {
                {"Longhair (m/f)", HairStyle.Longhair},
                {"Ponytail (m/f)", HairStyle.Ponytail},
                {"Shorthair (m/f)", HairStyle.Shorthair},
                {"Topknot (m/f)", HairStyle.Topknot},
                {"Mullet (m)", HairStyle.Mullet},
                {"Pigtails (f)", HairStyle.Mullet},
                {"Bald (m)", HairStyle.Bald},
                {"Braids (f)", HairStyle.Bald},
                {"Mohawk (m/f)", HairStyle.Mohawk},
                {"Medium (m)", HairStyle.Medium},
                {"Ponytail2 (f)", HairStyle.Medium},
            };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (!EnumIntMapping.TryGetValueIgnoreCase(column, out var hairStyle))
                    {
                        throw new Exception($"Proto {protoId} uses invalid hair style: '${column.AsString()}");
                    }

                    var hairSettings = HairSettings.Unpack(obj.GetInt32(obj_f.critter_hair_style));
                    hairSettings.Gender = (Gender) GameSystems.Stat.ObjStatBaseGet(obj, Stat.gender);
                    hairSettings.Race = (HairStyleRace) GameSystems.Stat.ObjStatBaseGet(obj, Stat.race);
                    hairSettings.Style = (HairStyle) hairStyle;

                    var packed = hairSettings.Pack();

                    obj.SetInt32(obj_f.critter_hair_style, packed);
                }
            }
        }

        [TempleDllLocation(0x1003a700)]
        private struct FactionsParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var tokenizer = new Tokenizer(column.AsString());
                    while (tokenizer.NextToken())
                    {
                        GameSystems.Critter.AddFaction(obj, tokenizer.TokenInt);
                    }
                }
            }
        }

        [TempleDllLocation(0x10039da0)]
        private struct ChallengeRatingParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public ChallengeRatingParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    int rating;
                    if (!column.EqualsIgnoreCase("1/4"))
                    {
                        rating = -2;
                    }
                    else if (!column.EqualsIgnoreCase("1/3"))
                    {
                        rating = -1;
                    }
                    else if (!column.EqualsIgnoreCase("1/2"))
                    {
                        rating = 0;
                    }
                    else
                    {
                        rating = column.GetInt();
                    }

                    obj.SetInt32(_field, rating);
                }
            }
        }

        [TempleDllLocation(0x10039bf0)]
        private struct HitDiceParser : IProtoColumnParser
        {
            private readonly obj_f _field;

            public HitDiceParser(obj_f field)
            {
                _field = field;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty && Dice.TryParse(column, out var dice))
                {
                    obj.SetInt32(_field, 0, dice.Count);
                    obj.SetInt32(_field, 1, dice.Sides);
                    obj.SetInt32(_field, 2, dice.Modifier);
                }
            }
        }

        [TempleDllLocation(0x10039850)]
        private struct MonsterCategoryParser : IProtoColumnParser
        {
            private static readonly Dictionary<string, MonsterCategory> Mapping =
                new Dictionary<string, MonsterCategory>
                {
                    {"mc_type_aberration", MonsterCategory.aberration},
                    {"mc_type_animal", MonsterCategory.animal},
                    {"mc_type_beast", MonsterCategory.beast},
                    {"mc_type_construct", MonsterCategory.construct},
                    {"mc_type_dragon", MonsterCategory.dragon},
                    {"mc_type_elemental", MonsterCategory.elemental},
                    {"mc_type_fey", MonsterCategory.fey},
                    {"mc_type_giant", MonsterCategory.giant},
                    {"mc_type_humanoid", MonsterCategory.humanoid},
                    {"mc_type_magical_beast", MonsterCategory.magical_beast},
                    {"mc_type_monstrous_humanoid", MonsterCategory.monstrous_humanoid},
                    {"mc_type_ooze", MonsterCategory.ooze},
                    {"mc_type_outsider", MonsterCategory.outsider},
                    {"mc_type_plant", MonsterCategory.plant},
                    {"mc_type_shapechanger", MonsterCategory.shapechanger},
                    {"mc_type_undead", MonsterCategory.undead},
                    {"mc_type_vermin", MonsterCategory.vermin},
                };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    if (EnumIntMapping.TryGetValue(column, out var value))
                    {
                        obj.SetInt64(obj_f.critter_monster_category, value);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        [TempleDllLocation(0x100398c0)]
        private struct MonsterSubtypesParser : IProtoColumnParser
        {
            // obj_f.critter_monster_category

            private static readonly Dictionary<string, MonsterSubtype> Mapping =
                new Dictionary<string, MonsterSubtype>
                {
                    {"mc_subtype_air", MonsterSubtype.air},
                    {"mc_subtype_aquatic", MonsterSubtype.aquatic},
                    {"mc_subtype_extraplanar", MonsterSubtype.extraplanar}, // TODO: Typo on the ToEE side
                    {"mc_subtype_extraplaner", MonsterSubtype.extraplanar}, // TODO: Typo on the ToEE side
                    {"mc_subtype_cold", MonsterSubtype.cold},
                    {"mc_subtype_chaotic", MonsterSubtype.chaotic},
                    {"mc_subtype_demon", MonsterSubtype.demon},
                    {"mc_subtype_devil", MonsterSubtype.devil},
                    {"mc_subtype_dwarf", MonsterSubtype.dwarf},
                    {"mc_subtype_earth", MonsterSubtype.earth},
                    {"mc_subtype_electricity", MonsterSubtype.electricity},
                    {"mc_subtype_elf", MonsterSubtype.elf},
                    {"mc_subtype_evil", MonsterSubtype.evil},
                    {"mc_subtype_fire", MonsterSubtype.fire},
                    {"mc_subtype_formian", MonsterSubtype.formian},
                    {"mc_subtype_gnoll", MonsterSubtype.gnoll},
                    {"mc_subtype_gnome", MonsterSubtype.gnome},
                    {"mc_subtype_goblinoid", MonsterSubtype.goblinoid},
                    {"mc_subtype_good", MonsterSubtype.good},
                    {"mc_subtype_guardinal", MonsterSubtype.guardinal},
                    {"mc_subtype_half_orc", MonsterSubtype.half_orc},
                    {"mc_subtype_halfling", MonsterSubtype.halfling},
                    {"mc_subtype_human", MonsterSubtype.human},
                    {"mc_subtype_lawful", MonsterSubtype.lawful},
                    {"mc_subtype_incorporeal", MonsterSubtype.incorporeal},
                    {"mc_subtype_orc", MonsterSubtype.orc},
                    {"mc_subtype_reptilian", MonsterSubtype.reptilian},
                    {"mc_subtype_slaadi", MonsterSubtype.slaadi},
                    {"mc_subtype_water", MonsterSubtype.water},
                };

            private static readonly EnumIntMapping EnumIntMapping = EnumIntMapping.Create(Mapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var category = obj.GetInt64(obj_f.critter_monster_category);
                    var flags = (int) (category >> 32);
                    IterateTokens(column, token =>
                    {
                        if (EnumIntMapping.TryGetValue(token, out var flag))
                        {
                            flags |= flag;
                        }
                        else
                        {
                            throw new Exception($"Proto {protoId} has invalid monster subtype " +
                                                $"'{Encoding.Default.GetString(token)}'");
                        }
                    });
                    category |= (flags << 32);
                    obj.SetInt64(obj_f.critter_monster_category, category);
                }
            }
        }

        [TempleDllLocation(0x10039e80)]
        private class ConditionParser : IProtoColumnParser
        {
            private readonly int _nameCol;
            private readonly int _arg1Col;
            private readonly int _arg2Col;

            private static readonly string[] AttributeNames =
            {
                "attribute_strength",
                "attribute_dexterity",
                "attribute_constitution",
                "attribute_intelligence",
                "attribute_wisdom",
                "attribute_charisma",
            };

            private static readonly string[] SavingThrowNames =
            {
                "saving_throw_fortitude",
                "saving_throw_reflex",
                "saving_throw_will",
            };

            private static readonly string[] Stuff =
            {
                "Nothing",
                "Potion",
                "Scroll",
                "Wand"
            };

            private static readonly string[] Stuff2 =
            {
                "type_animal",
                "type_beast",
                "type_construct",
                "type_dragon",
                "type_elemental",
                "type_fey",
                "type_giant",
                "type_magical_beast",
                "type_monstrous_humanoid",
                "type_ooze",
                "type_plant",
                "type_shapechanger",
                "type_undead",
                "type_vermin",
                "type_outsider_subtype_evil",
                "type_outsider_subtype_good",
                "type_outsider_subetype_lawful",
                "type_outsider_subtype_chaotic",
                "type_humanoid_subtype_goblinoid",
                "type_humanoid_subtype_reptilian",
                "type_humanoid_subtype_dwarf",
                "type_humanoid_subtype_elf",
                "type_humanoid_subtype_gnoll",
                "type_humanoid_subtype_gnome",
                "type_humanoid_subtype_halfling",
                "type_humanoid_subtype_orc",
                "type_humanoid_subtype_human",
                "type_humanoid_subtype_",
            };

            public ConditionParser(int nameCol, int arg1Col, int arg2Col)
            {
                _nameCol = nameCol;
                _arg1Col = arg1Col;
                _arg2Col = arg2Col;
            }

            private string _currentConditionName;
            private int _currentConditionArg1;
            private int _currentConditionArg2;

            private static readonly Dictionary<string, DamageType> DamageTypeMapping =
                new Dictionary<string, DamageType>
                {
                    {"Bludgeoning", DamageType.Bludgeoning},
                    {"Piercing", DamageType.Piercing},
                    {"Slashing", DamageType.Slashing},
                    {"Bludgeoning and Piercing", DamageType.BludgeoningAndPiercing},
                    {"Piercing and Slashing", DamageType.PiercingAndSlashing},
                    {"Slashing and Bludgeoning", DamageType.SlashingAndBludgeoning},
                    {"Slashing and Bludgeoning and Piercing", DamageType.SlashingAndBludgeoningAndPiercing},
                    {"Acid", DamageType.Acid},
                    {"Cold", DamageType.Cold},
                    {"Electricity", DamageType.Electricity},
                    {"Fire", DamageType.Fire},
                    {"Sonic", DamageType.Sonic},
                    {"Negative Energy", DamageType.NegativeEnergy},
                    {"Subdual", DamageType.Subdual},
                    {"Poison", DamageType.Poison},
                    {"Positive Energy", DamageType.PositiveEnergy},
                    {"Force", DamageType.Force},
                    {"Blood loss", DamageType.BloodLoss},
                    {"Magic", DamageType.Magic},
                };

            private static readonly EnumIntMapping DamageTypeIntMapping = EnumIntMapping.Create(DamageTypeMapping);

            private static readonly Dictionary<string, D20AttackPower> AttackPowerMapping =
                new Dictionary<string, D20AttackPower>
                {
                    {"Normal", D20AttackPower.NORMAL},
                    {"Unspecified", D20AttackPower.UNSPECIFIED},
                    {"Silver", D20AttackPower.SILVER},
                    {"Magic", D20AttackPower.MAGIC},
                    {"Holy", D20AttackPower.HOLY},
                    {"Unholy", D20AttackPower.UNHOLY},
                    {"Chaos", D20AttackPower.CHAOS},
                    {"Law", D20AttackPower.LAW},
                    {"Adamantium", D20AttackPower.ADAMANTIUM},
                    {"Bludgeoning", D20AttackPower.BLUDGEONING},
                    {"Piercing", D20AttackPower.PIERCING},
                    {"Slashing", D20AttackPower.SLASHING},
                    {"Mithril", D20AttackPower.MITHRIL},
                };

            private static readonly EnumIntMapping AttackPowerIntMapping = EnumIntMapping.Create(AttackPowerMapping);

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (column.ColumnIndex == _arg1Col)
                {
                    if (!column.IsEmpty)
                    {
                        for (int i = 0; i < AttributeNames.Length; i++)
                        {
                            if (column.EqualsIgnoreCase(AttributeNames[i]))
                            {
                                _currentConditionArg1 = i;
                            }
                        }

                        for (int i = 0; i < SavingThrowNames.Length; i++)
                        {
                            if (column.EqualsIgnoreCase(SavingThrowNames[i]))
                            {
                                _currentConditionArg1 = i;
                            }
                        }

                        for (int i = 0; i < Stuff2.Length; i++)
                        {
                            if (column.EqualsIgnoreCase(Stuff2[i]))
                            {
                                _currentConditionArg1 = i;
                            }
                        }

                        if (DamageTypeIntMapping.TryGetValueIgnoreCase(column, out _currentConditionArg1))
                        {
                            return;
                        }

                        if (AttackPowerIntMapping.TryGetValueIgnoreCase(column, out _currentConditionArg1))
                        {
                            // TODO: What is this 0x80000000 flag here ???
                            _currentConditionArg1 = (int) (_currentConditionArg1 | 0x80000000);
                            return;
                        }

                        if (GameSystems.Skill.GetSkillIdFromEnglishName(column.AsString(), out var skillId))
                        {
                            _currentConditionArg1 = (int) skillId;
                            return;
                        }

                        ReadOnlySpan<byte> columnText = column;
                        if (columnText[0] == '\'')
                        {
                            int start = 1;
                            int end;
                            for (end = start; end < columnText.Length; end++)
                            {
                                if (columnText[end] == '\'')
                                {
                                    break;
                                }
                            }

                            var condStringArg = columnText.Slice(start, end - start);
                            _currentConditionArg1 = ElfHash.Hash(condStringArg);
                            return;
                        }

                        if (!column.TryGetInt(out _currentConditionArg1))
                        {
                            Logger.Warn(
                                $"Proto {protoId} has bare text '{column.AsString()} as condition argument. Should use single quotes.");
                            _currentConditionArg1 = ElfHash.Hash(column.AsString());
                        }
                    }
                }
                else if (column.ColumnIndex == _arg2Col)
                {
                    if (!column.IsEmpty)
                    {
                        for (int i = 0; i < Stuff.Length; i++)
                        {
                            if (column.EqualsIgnoreCase(Stuff[i]))
                            {
                                _currentConditionArg2 = i;
                                return;
                            }
                        }

                        if (DamageTypeIntMapping.TryGetValueIgnoreCase(column, out _currentConditionArg2))
                        {
                            return;
                        }

                        if (Dice.TryParse(column, out var dice))
                            _currentConditionArg2 = dice.ToPacked();
                        else
                            _currentConditionArg2 = column.GetInt();
                    }

                    if (_currentConditionName != null)
                    {
                        obj_f conditionArrayField, argumentArrayField;
                        if (!obj.IsCritter())
                        {
                            conditionArrayField = obj_f.item_pad_wielder_condition_array;
                            argumentArrayField = obj_f.item_pad_wielder_argument_array;
                        }
                        else
                        {
                            conditionArrayField = obj_f.conditions;
                            argumentArrayField = obj_f.condition_arg0;
                        }

                        var condArray = obj.GetInt32Array(conditionArrayField);
                        var firstNumber = condArray.Count;
                        var argArray = obj.GetInt32Array(argumentArrayField);

                        var argIdx = argArray.Count;

                        var condSpec = GameSystems.D20.Conditions[_currentConditionName];
                        if (condSpec != null)
                        {
                            obj.SetInt32(conditionArrayField, firstNumber, ElfHash.Hash(_currentConditionName));

                            var argCount = condSpec.numArgs;
                            if (argCount > 0)
                                obj.SetInt32(argumentArrayField, argIdx++, _currentConditionArg1);

                            if (argCount > 1)
                                obj.SetInt32(argumentArrayField, argIdx++, _currentConditionArg2);

                            for (var i = 2; i < argCount; i++)
                            {
                                obj.SetInt32(argumentArrayField, argIdx++, 0);
                            }
                        }
                        else
                        {
                            Logger.Warn($"Proto {protoId} referenced unknown condition '{_currentConditionName}'");
                        }

                        _currentConditionName = null;
                    }
                }
                else if (column.ColumnIndex == _nameCol)
                {
                    if (!column.IsEmpty)
                    {
                        _currentConditionName = column.AsString();
                        _currentConditionArg1 = 0;
                        _currentConditionArg2 = 0;
                    }
                    else
                    {
                        _currentConditionName = null;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [TempleDllLocation(0x1003a430)]
        private struct ClassLevelParser : IProtoColumnParser
        {
            private readonly int _classColumn;
            private readonly int _levelColumn;

            public ClassLevelParser(int classColumn, int levelColumn)
            {
                _classColumn = classColumn;
                _levelColumn = levelColumn;
                _currentClass = null;
            }

            private Stat? _currentClass;

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (column.ColumnIndex == _classColumn)
                {
                    _currentClass = null; // Reset first

                    if (!column.IsEmpty)
                    {
                        for (var i = Stat.level_barbarian; i <= Stat.level_wizard; i++)
                        {
                            var name = GameSystems.Stat.GetStatRulesString(i);
                            if (column.EqualsIgnoreCase(name))
                            {
                                _currentClass = i;
                                return;
                            }
                        }

                        throw new Exception($"Proto {protoId} has invalid class '{column.AsString()}'");
                    }
                }
                else if (column.ColumnIndex == _levelColumn)
                {
                    if (!column.IsEmpty && _currentClass.HasValue)
                    {
                        var levels = column.GetInt();
                        for (int i = 0; i < levels; i++)
                        {
                            var packet = new LevelupPacket();
                            packet.flags = 1;
                            packet.classCode = (int) _currentClass.Value;
                            GameSystems.Level.LevelUpApply(obj, packet);
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [TempleDllLocation(0x1003a530)]
        private struct SkillParser : IProtoColumnParser
        {
            private readonly int _nameColumn;
            private readonly int _rankColumn;
            private SkillId? _currentSkillId;

            public SkillParser(int nameColumn, int rankColumn)
            {
                _nameColumn = nameColumn;
                _rankColumn = rankColumn;
                _currentSkillId = null;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (column.ColumnIndex == _nameColumn)
                {
                    _currentSkillId = null;

                    if (!column.IsEmpty)
                    {
                        for (var skill = SkillId.appraise; skill < SkillId.count; skill++)
                        {
                            var name = GameSystems.Skill.GetSkillEnglishName(skill);
                            if (column.EqualsIgnoreCase(name))
                            {
                                _currentSkillId = skill;
                                return;
                            }
                        }

                        throw new Exception($"Proto {protoId} uses invalid skill '{column.AsString()}'");
                    }
                }
                else if (column.ColumnIndex == _rankColumn)
                {
                    if (!column.IsEmpty && _currentSkillId.HasValue)
                    {
                        var ranks = column.GetInt();
                        GameSystems.Skill.AddSkillRanks(obj, _currentSkillId.Value, ranks);
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [TempleDllLocation(0x1003a600)]
        private struct FeatParser : IProtoColumnParser
        {
            private readonly int _index;

            public FeatParser(int index)
            {
                _index = index;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    for (var featId = FeatId.ACROBATIC; featId < FeatId.NONE; featId++)
                    {
                        var name = GameSystems.Feat.GetEnglishFeatName(featId);
                        if (column.EqualsIgnoreCase(name))
                        {
                            GameSystems.Feat.AddFeat(obj, featId);
                            return;
                        }
                    }

                    throw new Exception($"Proto {protoId} has invalid feat '{column.AsString()}'");
                }
            }
        }

        [TempleDllLocation(0x1003a660)]
        private struct ScriptParser : IProtoColumnParser
        {
            private readonly int _index;

            public ScriptParser(int index)
            {
                _index = index;
            }

            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var script = new ObjectScript();
                    var tokenizer = new Tokenizer(column.AsString());
                    if (tokenizer.NextToken())
                    {
                        script.scriptId = tokenizer.TokenInt;
                    }

                    Span<byte> counters = stackalloc byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        if (tokenizer.NextToken())
                        {
                            counters[i] = (byte) tokenizer.TokenInt;
                        }
                    }

                    script.counters = BitConverter.ToUInt32(counters);
                    script.unk1 = 0;

                    obj.SetScript(obj_f.scripts_idx, _index, script);
                }
            }
        }

        [TempleDllLocation(0x1003a8c0)]
        private struct SpellKnownParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (!column.IsEmpty)
                {
                    var tokenizer = new Tokenizer(column.AsString());

                    if (!tokenizer.NextToken() || !tokenizer.IsQuotedString)
                    {
                        return;
                    }

                    var spellName = tokenizer.TokenText;
                    if (!GameSystems.Spell.GetSpellEnumByEnglishName(spellName, out var spellId))
                    {
                        Logger.Warn($"Proto {protoId} references unknown spell '{spellName}");
                    }

                    if (!tokenizer.NextToken() || !tokenizer.IsIdentifier)
                    {
                        return;
                    }

                    var classCodeName = tokenizer.TokenText;
                    if (!GameSystems.Spell.GetSpellClassCode(classCodeName, out var classCode))
                    {
                        Logger.Warn($"Proto {protoId} references unknown spell class code '{classCodeName}");
                    }

                    if (!tokenizer.NextToken() || !tokenizer.IsNumber)
                    {
                        return;
                    }

                    var spellLevel = tokenizer.TokenInt;

                    if (!obj.IsCritter() || (classCode & 0x80) != 0 || (classCode & 0x7F) != 23)
                    {
                        bool knowsSpell = false;
                        if (obj.IsCritter())
                        {
                            knowsSpell = GameSystems.Spell.IsSpellKnown(obj, spellId, classCode, spellLevel);
                        }

                        if (!knowsSpell)
                        {
                            GameSystems.Spell.SpellKnownAdd(obj, spellId, classCode, spellLevel, 1, 0);
                        }

                        if (!obj.IsCritter())
                            return; // Only memoryize for critters
                    }

                    GameSystems.Spell.SpellMemorizeAdd(obj, spellId, classCode, spellLevel, 2, 0);
                }
            }
        }

        [TempleDllLocation(0x1003acc0)]
        private struct CritterPadI4Parser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (obj.IsCritter())
                {
                    obj.SetInt32(obj_f.critter_pad_i_4, ElfHash.Hash(column));
                }
            }
        }

        [TempleDllLocation(0x1003ad20)]
        private struct CritterStrategyParser : IProtoColumnParser
        {
            public void Parse(int protoId, TabFileColumn column, GameObjectBody obj)
            {
                if (obj.IsCritter())
                {
                    GameSystems.D20.SetCritterStrategy(obj, column.AsString());
                }
            }
        }
    }
}