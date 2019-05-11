using System;

namespace SpicyTemple.Core.GameObject
{
    [Flags]
    public enum AiFlag : ulong {
        FindingHelp = 0x1,
        WaypointDelay = 0x2,
        WaypointDelayed = 0x4,
        RunningOff = 0x8,
        Fighting = 0x10,
        CheckGrenade = 0x20,
        CheckWield = 0x40,
        CheckWeapon = 0x80,
        LookForWeapon = 0x100,
        LookForArmor = 0x200,
        LookForAmmo = 0x400,
        HasSpokenFlee = 0x800,
    }
}