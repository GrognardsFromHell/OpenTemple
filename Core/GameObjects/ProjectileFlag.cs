using System;

namespace OpenTemple.Core.GameObjects;

[Flags]
public enum ProjectileFlag : uint
{
    UNK_40 = 0x40,
    UNK_1000 = 0x1000,
    UNK_2000 = 0x2000, // Possibly "returning"
}