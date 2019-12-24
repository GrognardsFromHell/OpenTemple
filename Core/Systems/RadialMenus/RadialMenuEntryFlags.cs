using System;

namespace OpenTemple.Core.Systems.RadialMenus
{
    [Flags]
    public enum RadialMenuEntryFlags
    {
        // Irrelevant for .NET
        OwnsText = 0x1,
        HasMinArg = 0x2,
	    HasMaxArg = 0x4
    }

}