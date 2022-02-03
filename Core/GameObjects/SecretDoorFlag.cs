using System;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum SecretDoorFlag : uint
    {
        DC_0 = 0x1,
        DC_1 = 0x2,
        DC_2 = 0x4,
        DC_3 = 0x8,
        DC_4 = 0x10,
        DC_5 = 0x20,
        DC_6 = 0x40,
        RANK_0 = 0x80,
        RANK_1 = 0x100,
        RANK_2 = 0x200,
        RANK_3 = 0x400,
        RANK_4 = 0x800,
        RANK_5 = 0x1000,
        RANK_6 = 0x2000,
        UNUSED = 0x4000,
        UNUSED2 = 0x8000,
        SECRET_DOOR = 0x10000,
        SECRET_DOOR_FOUND = 0x20000
    }
}