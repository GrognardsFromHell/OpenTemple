using System.Runtime.InteropServices;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.TigSubsystems;

public class TigKeyboard
{

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly byte[] _keyState = new byte[0xFF];

    [TempleDllLocation(0x101DE050)]
    public bool IsPressed(DIK key)
    {
        var virtualKey = ToVirtualKey(key);

        return IsKeyPressed(virtualKey);
    }

    public bool IsKeyPressed(VirtualKey virtualKey)
    {
        var state = _keyState[(int) virtualKey];
        return (state & 0x80) != 0;
    }

    public bool IsModifierActive(VirtualKey virtualKey)
    {
        var state = _keyState[(int) virtualKey];
        return (state & 0x01) != 0;
    }

    [TempleDllLocation(0x101DE070)]
    public bool IsModifierActive(DIK key)
    {
        var virtualKey = ToVirtualKey(key);

        return IsModifierActive(virtualKey);
    }

    [TempleDllLocation(0x101DE0D0)]
    public void Update()
    {
        if (!GetKeyboardState(_keyState))
        {
            Logger.Error("Unable to retrieve keyboard state: {0}", Marshal.GetLastWin32Error());
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetKeyboardState([Out] byte[] keyState);

    // Translate dinput key to virtual key
    private static VirtualKey ToVirtualKey(DIK dinputKey)
    {
        switch (dinputKey)
        {
            case DIK.DIK_ESCAPE:
                return VirtualKey.VK_ESCAPE;
            case DIK.DIK_1:
                return VirtualKey.VK_1;
            case DIK.DIK_2:
                return VirtualKey.VK_2;
            case DIK.DIK_3:
                return VirtualKey.VK_3;
            case DIK.DIK_4:
                return VirtualKey.VK_4;
            case DIK.DIK_5:
                return VirtualKey.VK_5;
            case DIK.DIK_6:
                return VirtualKey.VK_6;
            case DIK.DIK_7:
                return VirtualKey.VK_7;
            case DIK.DIK_8:
                return VirtualKey.VK_8;
            case DIK.DIK_9:
                return VirtualKey.VK_9;
            case DIK.DIK_0:
                return VirtualKey.VK_0;
            case DIK.DIK_MINUS: /* - on main keyboard */
                return VirtualKey.VK_OEM_MINUS;
            case DIK.DIK_EQUALS:
                return VirtualKey.VK_OEM_PLUS;
            case DIK.DIK_BACK: /* backspace */
                return VirtualKey.VK_BACK;
            case DIK.DIK_TAB:
                return VirtualKey.VK_TAB;
            case DIK.DIK_Q:
                return VirtualKey.VK_Q;
            case DIK.DIK_W:
                return VirtualKey.VK_W;
            case DIK.DIK_E:
                return VirtualKey.VK_E;
            case DIK.DIK_R:
                return VirtualKey.VK_R;
            case DIK.DIK_T:
                return VirtualKey.VK_T;
            case DIK.DIK_Y:
                return VirtualKey.VK_Y;
            case DIK.DIK_U:
                return VirtualKey.VK_U;
            case DIK.DIK_I:
                return VirtualKey.VK_I;
            case DIK.DIK_O:
                return VirtualKey.VK_O;
            case DIK.DIK_P:
                return VirtualKey.VK_P;
            case DIK.DIK_LBRACKET:
                return VirtualKey.VK_OEM_4;
            case DIK.DIK_RBRACKET:
                return VirtualKey.VK_OEM_6;
            case DIK.DIK_RETURN: /* Enter on main keyboard */
                return VirtualKey.VK_RETURN;
            case DIK.DIK_LCONTROL:
                return VirtualKey.VK_LCONTROL;
            case DIK.DIK_A:
                return VirtualKey.VK_A;
            case DIK.DIK_S:
                return VirtualKey.VK_S;
            case DIK.DIK_D:
                return VirtualKey.VK_D;
            case DIK.DIK_F:
                return VirtualKey.VK_F;
            case DIK.DIK_G:
                return VirtualKey.VK_G;
            case DIK.DIK_H:
                return VirtualKey.VK_H;
            case DIK.DIK_J:
                return VirtualKey.VK_J;
            case DIK.DIK_K:
                return VirtualKey.VK_K;
            case DIK.DIK_L:
                return VirtualKey.VK_L;
            case DIK.DIK_SEMICOLON:
                return VirtualKey.VK_OEM_1;
            case DIK.DIK_APOSTROPHE:
                return VirtualKey.VK_OEM_7;
            case DIK.DIK_GRAVE: /* accent grave */
                return VirtualKey.VK_OEM_3;
            case DIK.DIK_LSHIFT:
                return VirtualKey.VK_LSHIFT;
            case DIK.DIK_BACKSLASH:
                return VirtualKey.VK_OEM_5;
            case DIK.DIK_Z:
                return VirtualKey.VK_Z;
            case DIK.DIK_X:
                return VirtualKey.VK_X;
            case DIK.DIK_C:
                return VirtualKey.VK_C;
            case DIK.DIK_V:
                return VirtualKey.VK_V;
            case DIK.DIK_B:
                return VirtualKey.VK_B;
            case DIK.DIK_N:
                return VirtualKey.VK_N;
            case DIK.DIK_M:
                return VirtualKey.VK_M;
            case DIK.DIK_COMMA:
                return VirtualKey.VK_OEM_COMMA;
            case DIK.DIK_PERIOD: /* . on main keyboard */
                return VirtualKey.VK_OEM_PERIOD;
            case DIK.DIK_SLASH: /* / on main keyboard */
                return VirtualKey.VK_OEM_2;
            case DIK.DIK_RSHIFT:
                return VirtualKey.VK_RSHIFT;
            case DIK.DIK_MULTIPLY: /* * on numeric keypad */
                return VirtualKey.VK_MULTIPLY;
            case DIK.DIK_LMENU: /* left Alt */
                return VirtualKey.VK_LMENU;
            case DIK.DIK_SPACE:
                return VirtualKey.VK_SPACE;
            case DIK.DIK_CAPITAL:
                return VirtualKey.VK_CAPITAL;
            case DIK.DIK_F1:
                return VirtualKey.VK_F1;
            case DIK.DIK_F2:
                return VirtualKey.VK_F2;
            case DIK.DIK_F3:
                return VirtualKey.VK_F3;
            case DIK.DIK_F4:
                return VirtualKey.VK_F4;
            case DIK.DIK_F5:
                return VirtualKey.VK_F5;
            case DIK.DIK_F6:
                return VirtualKey.VK_F6;
            case DIK.DIK_F7:
                return VirtualKey.VK_F7;
            case DIK.DIK_F8:
                return VirtualKey.VK_F8;
            case DIK.DIK_F9:
                return VirtualKey.VK_F9;
            case DIK.DIK_F10:
                return VirtualKey.VK_F10;
            case DIK.DIK_NUMLOCK:
                return VirtualKey.VK_NUMLOCK;
            case DIK.DIK_SCROLL: /* Scroll Lock */
                return VirtualKey.VK_SCROLL;
            case DIK.DIK_NUMPAD7:
                return VirtualKey.VK_NUMPAD7;
            case DIK.DIK_NUMPAD8:
                return VirtualKey.VK_NUMPAD8;
            case DIK.DIK_NUMPAD9:
                return VirtualKey.VK_NUMPAD9;
            case DIK.DIK_SUBTRACT: /* - on numeric keypad */
                return VirtualKey.VK_SUBTRACT;
            case DIK.DIK_NUMPAD4:
                return VirtualKey.VK_NUMPAD4;
            case DIK.DIK_NUMPAD5:
                return VirtualKey.VK_NUMPAD5;
            case DIK.DIK_NUMPAD6:
                return VirtualKey.VK_NUMPAD6;
            case DIK.DIK_ADD: /* + on numeric keypad */
                return VirtualKey.VK_ADD;
            case DIK.DIK_NUMPAD1:
                return VirtualKey.VK_NUMPAD1;
            case DIK.DIK_NUMPAD2:
                return VirtualKey.VK_NUMPAD2;
            case DIK.DIK_NUMPAD3:
                return VirtualKey.VK_NUMPAD3;
            case DIK.DIK_NUMPAD0:
                return VirtualKey.VK_NUMPAD0;
            case DIK.DIK_DECIMAL: /* . on numeric keypad */
                return VirtualKey.VK_DECIMAL;
            case DIK.DIK_F11:
                return VirtualKey.VK_F11;
            case DIK.DIK_F12:
                return VirtualKey.VK_F12;
            case DIK.DIK_F13: /*                     (NEC PC98) */
                return VirtualKey.VK_F13;
            case DIK.DIK_F14: /*                     (NEC PC98) */
                return VirtualKey.VK_F14;
            case DIK.DIK_F15: /*                     (NEC PC98) */
                return VirtualKey.VK_F15;
            case DIK.DIK_NUMPADENTER: /* Enter on numeric keypad */
                return VirtualKey.VK_RETURN;
            case DIK.DIK_RCONTROL:
                return VirtualKey.VK_RCONTROL;
            case DIK.DIK_DIVIDE: /* / on numeric keypad */
                return VirtualKey.VK_DIVIDE;
            case DIK.DIK_RMENU: /* right Alt */
                return VirtualKey.VK_RMENU;
            case DIK.DIK_HOME: /* Home on arrow keypad */
                return VirtualKey.VK_HOME;
            case DIK.DIK_UP: /* UpArrow on arrow keypad */
                return VirtualKey.VK_UP;
            case DIK.DIK_PRIOR: /* PgUp on arrow keypad */
                return VirtualKey.VK_PRIOR;
            case DIK.DIK_LEFT: /* LeftArrow on arrow keypad */
                return VirtualKey.VK_LEFT;
            case DIK.DIK_RIGHT: /* RightArrow on arrow keypad */
                return VirtualKey.VK_RIGHT;
            case DIK.DIK_END: /* End on arrow keypad */
                return VirtualKey.VK_END;
            case DIK.DIK_DOWN: /* DownArrow on arrow keypad */
                return VirtualKey.VK_DOWN;
            case DIK.DIK_NEXT: /* PgDn on arrow keypad */
                return VirtualKey.VK_NEXT;
            case DIK.DIK_INSERT: /* Insert on arrow keypad */
                return VirtualKey.VK_INSERT;
            case DIK.DIK_DELETE: /* Delete on arrow keypad */
                return VirtualKey.VK_DELETE;
            case DIK.DIK_LWIN: /* Left Windows key */
                return VirtualKey.VK_LWIN;
            case DIK.DIK_RWIN: /* Right Windows key */
                return VirtualKey.VK_RWIN;
            case DIK.DIK_APPS: /* AppMenu key */
                return VirtualKey.VK_APPS;
            case DIK.DIK_PAUSE:
                return VirtualKey.VK_PAUSE;
            case DIK.DIK_SYSRQ: // (print screen)
                return VirtualKey.VK_SNAPSHOT;
            default:
                throw new TigException($"Unmappable direct input key: {dinputKey}");
        }
    }
}