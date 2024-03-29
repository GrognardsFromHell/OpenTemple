using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace OpenTemple.Core.GameObjects;

public enum WeaponType
{
    gauntlet = 0,
    unarmed_strike_medium_sized_being = 1,
    unarmed_strike_small_being = 2,
    dagger = 3,
    punching_dagger = 4,
    spiked_gauntlet = 5,
    light_mace = 6,
    sickle = 7,
    club = 8,
    shortspear = 9,
    heavy_mace = 10,
    morningstar = 11,
    quarterstaff = 12,
    spear = 13,
    light_crossbow = 14, // 14
    dart = 15,
    sling = 16,
    heavy_crossbow = 17, // 17
    javelin = 18,
    throwing_axe = 19,
    light_hammer = 20,
    handaxe = 21,
    light_lance = 22,
    light_pick = 23,
    sap = 24,
    short_sword = 25,
    battleaxe = 26,
    light_flail = 27,
    heavy_lance = 28,
    longsword = 29,
    heavy_pick = 30,
    rapier = 31,
    scimitar = 32,
    trident = 33,
    warhammer = 34,
    falchion = 35,
    heavy_flail = 36,
    glaive = 37,
    greataxe = 38,
    greatclub = 39,
    greatsword = 40,
    guisarme = 41,
    halberd = 42,
    longspear = 43, //43
    ranseur = 44,
    scythe = 45,
    shortbow = 46,
    composite_shortbow = 47,
    longbow = 48,
    composite_longbow = 49,
    halfling_kama = 50, // 50
    kukri = 51,
    halfling_nunchaku = 52, //52
    halfling_siangham = 53, //53
    kama = 54, //54
    nunchaku = 55, // 55
    siangham = 56, // 56
    bastard_sword = 57,
    dwarven_waraxe = 58, // racial weapons - 58 and on
    gnome_hooked_hammer = 59,
    orc_double_axe = 60,
    spike_chain = 61,
    dire_flail = 62,
    two_bladed_sword = 63,
    dwarven_urgrosh = 64,
    hand_crossbow = 65, // 65
    shuriken = 66,
    whip = 67,
    repeating_crossbow = 68,
    net = 69,
    grapple = 70,
    ray = 71,
    grenade = 72, // 72
    mindblade = 73,

    none = -1
}

public static class WeaponTypes
{
    public static readonly Dictionary<string, WeaponType> IdToType = new()
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

    public static readonly Dictionary<WeaponType, string> TypeToId = IdToType
        .ToDictionary(x => x.Value, x => x.Key);

    public static readonly Dictionary<WeaponType, WeaponType> FallbackSound = new()
    {
        {WeaponType.punching_dagger, WeaponType.gauntlet},
        {WeaponType.spiked_gauntlet, WeaponType.gauntlet},
        {WeaponType.light_mace, WeaponType.heavy_mace},
        {WeaponType.sickle, WeaponType.longsword},
        {WeaponType.shortspear, WeaponType.longspear},
        {WeaponType.morningstar, WeaponType.heavy_mace},
        {WeaponType.spear, WeaponType.longspear},
        {WeaponType.javelin, WeaponType.longspear},
        {WeaponType.throwing_axe, WeaponType.handaxe},
        {WeaponType.light_hammer, WeaponType.warhammer},
        {WeaponType.light_lance, WeaponType.longspear},
        {WeaponType.light_pick, WeaponType.longsword},
        {WeaponType.sap, WeaponType.light_mace},
        {WeaponType.short_sword, WeaponType.longsword},
        {WeaponType.light_flail, WeaponType.heavy_mace},
        {WeaponType.heavy_lance, WeaponType.longspear},
        {WeaponType.heavy_pick, WeaponType.longsword},
        {WeaponType.rapier, WeaponType.longsword},
        {WeaponType.scimitar, WeaponType.longsword},
        {WeaponType.trident, WeaponType.longspear},
        {WeaponType.warhammer, WeaponType.heavy_mace},
        {WeaponType.falchion, WeaponType.longsword},
        {WeaponType.heavy_flail, WeaponType.heavy_mace},
        {WeaponType.glaive, WeaponType.longspear},
        {WeaponType.greataxe, WeaponType.battleaxe},
        {WeaponType.greatclub, WeaponType.club},
        {WeaponType.greatsword, WeaponType.longsword},
        {WeaponType.guisarme, WeaponType.longspear},
        {WeaponType.ranseur, WeaponType.longspear},
        {WeaponType.scythe, WeaponType.sickle},
        {WeaponType.composite_shortbow, WeaponType.shortbow},
        {WeaponType.composite_longbow, WeaponType.longbow},
        {WeaponType.halfling_kama, WeaponType.kama},
        {WeaponType.kukri, WeaponType.dagger},
        {WeaponType.halfling_nunchaku, WeaponType.nunchaku},
        {WeaponType.halfling_siangham, WeaponType.siangham},
        {WeaponType.kama, WeaponType.sickle},
        {WeaponType.siangham, WeaponType.shortspear},
        {WeaponType.bastard_sword, WeaponType.longsword},
        {WeaponType.dwarven_waraxe, WeaponType.battleaxe},
        {WeaponType.gnome_hooked_hammer, WeaponType.warhammer},
        {WeaponType.orc_double_axe, WeaponType.battleaxe},
        {WeaponType.dire_flail, WeaponType.heavy_flail},
        {WeaponType.two_bladed_sword, WeaponType.longsword},
        {WeaponType.dwarven_urgrosh, WeaponType.battleaxe},
        {WeaponType.hand_crossbow, WeaponType.light_crossbow},
        {WeaponType.repeating_crossbow, WeaponType.light_crossbow},
        {WeaponType.grenade, WeaponType.gauntlet},
    };
}