using System;

namespace OpenTemple.Core.GameObjects;

[Flags]
public enum WeaponFlag : uint
{
    LOUD = 1,
    SILENT = 2,
    UNUSED_1 = 4,
    UNUSED_2 = 8,
    THROWABLE = 0x10,
    TRANS_PROJECTILE = 0x20,
    BOOMERANGS = 0x40,
    IGNORE_RESISTANCE = 0x80,
    DAMAGE_ARMOR = 0x100,
    DEFAULT_THROWS = 0x200,
    RANGED_WEAPON = 0x400,
    WEAPON_LOADED = 0x800,
    MAGIC_STAFF = 0x1000,
}