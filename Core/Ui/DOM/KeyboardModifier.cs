using System;

namespace OpenTemple.Core.Ui.DOM
{
    [Flags]
    public enum KeyboardModifier
    {
        Control = 1,
        Shift = 2,
        Alt = 4,
        /// <summary>
        /// On Windows, this will be the Windows key.
        /// </summary>
        Meta = 8
    }
}