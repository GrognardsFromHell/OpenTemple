using OpenTemple.Core.Hotkeys;
using SDL2;

namespace OpenTemple.Core.Ui;

/// <summary>
/// Default UI Hotkeys.
/// </summary>
public static class UiHotkeys
{

    public static readonly Hotkey CloseWindow = Hotkey.Build("close_window")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)
        .Build();

}
