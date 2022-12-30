using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenTemple.Core.GameObjects;

public enum EquipSlot : uint
{
    Helmet = 0,
    Necklace = 1,
    Gloves = 2,
    WeaponPrimary = 3,
    WeaponSecondary = 4,
    Armor = 5,
    RingPrimary = 6,
    RingSecondary = 7,
    Boots = 8,
    Ammo = 9,
    Cloak = 10,
    Shield = 11,
    Robes = 12,
    Bracers = 13,
    BardicItem = 14,
    Lockpicks = 15,
    Count = 16,
    Invalid = 17,
    Bag1,
    Bag2,
    Bag3,
    Bag4,
}

public static class EquipSlots
{
    public static readonly IImmutableList<EquipSlot> Slots = ImmutableList.Create(
        EquipSlot.Helmet,
        EquipSlot.Necklace,
        EquipSlot.Gloves,
        EquipSlot.WeaponPrimary,
        EquipSlot.WeaponSecondary,
        EquipSlot.Armor,
        EquipSlot.RingPrimary,
        EquipSlot.RingSecondary,
        EquipSlot.Boots,
        EquipSlot.Ammo,
        EquipSlot.Cloak,
        EquipSlot.Shield,
        EquipSlot.Robes,
        EquipSlot.Bracers,
        EquipSlot.BardicItem,
        EquipSlot.Lockpicks
    );

    public static readonly IImmutableDictionary<string, EquipSlot> SlotsById = new Dictionary<string, EquipSlot>
    {
        {"helmet", EquipSlot.Helmet},
        {"necklace", EquipSlot.Necklace},
        {"gloves", EquipSlot.Gloves},
        {"main_hand", EquipSlot.WeaponPrimary},
        {"off_hand", EquipSlot.WeaponSecondary},
        {"armor", EquipSlot.Armor},
        {"ring1", EquipSlot.RingPrimary},
        {"ring2", EquipSlot.RingSecondary},
        {"boots", EquipSlot.Boots},
        {"ammo", EquipSlot.Ammo},
        {"cloak", EquipSlot.Cloak},
        {"shield", EquipSlot.Shield},
        {"robes", EquipSlot.Robes},
        {"bracers", EquipSlot.Bracers},
        {"bardic_instrument", EquipSlot.BardicItem},
        {"lockpicks", EquipSlot.Lockpicks},
    }.ToImmutableDictionary();
}
