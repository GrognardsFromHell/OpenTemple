using System;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    [Flags]
    enum RadialMenuEntryFlags
    {
        // Irrelevant for .NET
        OwnsText = 0x1,
        HasMinArg = 0x2,
	    HasMaxArg = 0x4
    }

}