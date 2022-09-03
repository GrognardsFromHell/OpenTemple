using System;

namespace OpenTemple.Core.Ui.Styles;

[Flags]
public enum StylingState : uint
{
    Hover = 1,
    Pressed = 2,
    Disabled = 4
}