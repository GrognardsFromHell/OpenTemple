using OpenTemple.Core.Platform;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui.Events;

public class KeyboardEvent : UiEvent
{
    public SDL_Keycode VirtualKey { get; init; }
    public SDL_Scancode PhysicalKey { get; init; }

    public bool IsCtrlHeld { get; init; }
    public bool IsAltHeld { get; init; }
    public bool IsShiftHeld { get; init; }
    public bool IsMetaHeld { get; init; }

    public KeyModifier HeldModifiers
    {
        get
        {
            KeyModifier modifiers = default;
            if (IsCtrlHeld)
            {
                modifiers |= KeyModifier.Ctrl;
            }

            if (IsAltHeld)
            {
                modifiers |= KeyModifier.Alt;
            }

            if (IsShiftHeld)
            {
                modifiers |= KeyModifier.Shift;
            }

            if (IsMetaHeld)
            {
                modifiers |= KeyModifier.Meta;
            }

            return modifiers;
        }
    }

    public bool IsRepeat { get; init; }
}
