using System;

namespace OpenTemple.Core.Platform;

[Flags]
public enum KeyModifier
{
    Ctrl = 1,
    Alt = 2,
    Shift = 4,
    /// <summary>
    /// Windows Key, Command Key on Mac
    /// </summary>
    Meta = 8
}
