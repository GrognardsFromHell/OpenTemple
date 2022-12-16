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

    public bool IsRepeat { get; init; }
}
