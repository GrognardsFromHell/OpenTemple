using System;
using System.Diagnostics;
using static SDL2.SDL;

namespace OpenTemple.Core.TigSubsystems;

public unsafe class TigKeyboard
{
    // The indices are SDL scan codes
    private readonly byte* _keyState;
    private readonly int _keyStateLength;

    public TigKeyboard()
    {
        _keyState = (byte*) SDL_GetKeyboardState(out _keyStateLength);
    }
    
    public bool IsCtrlPressed { get; private set; }

    public bool IsAltPressed { get; private set; }
    
    public bool IsShiftPressed { get; private set; }
    
    public bool IsMetaHeld { get; private set; }

    [TempleDllLocation(0x101DE050)]
    private bool IsPressed(SDL_Scancode scanCode)
    {
        var idx = (int) scanCode;
        Trace.Assert(idx >= 0 && idx < _keyStateLength);
        return _keyState[idx] != 0;
    }
    
    [TempleDllLocation(0x101DE0D0)]
    public void Update()
    {
        IsCtrlPressed = IsPressed(SDL_Scancode.SDL_SCANCODE_LCTRL) || IsPressed(SDL_Scancode.SDL_SCANCODE_RCTRL);
        IsShiftPressed = IsPressed(SDL_Scancode.SDL_SCANCODE_LSHIFT) || IsPressed(SDL_Scancode.SDL_SCANCODE_RSHIFT);
        IsAltPressed = IsPressed(SDL_Scancode.SDL_SCANCODE_LALT) || IsPressed(SDL_Scancode.SDL_SCANCODE_RALT);
        IsMetaHeld = IsPressed(SDL_Scancode.SDL_SCANCODE_LGUI) || IsPressed(SDL_Scancode.SDL_SCANCODE_RGUI);
    }
}
