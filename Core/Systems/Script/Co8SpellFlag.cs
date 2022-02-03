using System;

namespace OpenTemple.Core.Systems.Script;

// This should actually work via conditions or D20 queries respectively
[Flags]
public enum Co8SpellFlag
{
    IronBody = 0x1,
    TensersTransformation = 0x2,
    AnalyzeDweomer = 0x4,
    HolySword = 0x8,
    ProtectionFromSpells = 0x10,
    MordenkainensSword = 0x20,
    FlamingSphere = 0x40,
    Summoned = 0x80,
    HezrouStench = 0x100,
    Tongues = 0x200,
    DisguiseSelf = 0x400,
    DeathWard = 0x800
}