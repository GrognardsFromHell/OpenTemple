using OpenTemple.Core.Hotkeys;
using SDL2;

namespace OpenTemple.Core.Ui;

/// <summary>
/// Default UI Hotkeys.
/// </summary>
public static class UiHotkeys
{

    public static readonly Hotkey Cancel = Hotkey.Build("cancel")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE)
        .Build();
    
    public static readonly Hotkey Confirm = Hotkey.Build("confirm")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_RETURN)
        .Build();
    
    public static readonly Hotkey Delete = Hotkey.Build("delete")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_DELETE)
        .Build();
    
    public static readonly Hotkey NavigateUp = Hotkey.Build("navigate_up")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_UP)
        .Secondary(SDL.SDL_Scancode.SDL_SCANCODE_KP_8)
        .Build();
    
    public static readonly Hotkey NavigateDown = Hotkey.Build("navigate_down")
        .Primary(SDL.SDL_Scancode.SDL_SCANCODE_DOWN)
        .Secondary(SDL.SDL_Scancode.SDL_SCANCODE_KP_2)
        .Build();

}
