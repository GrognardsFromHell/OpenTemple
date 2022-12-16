using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ImGuiNET;
using OpenTemple.Core.Platform;
using SDL2;

namespace OpenTemple.Core.DebugUI;

using static SDL;

/// <summary>
/// This is based on https://github.com/ocornut/imgui/blob/master/backends/imgui_impl_sdl.cpp.
/// </summary>
public class ImGuiBackend : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly IntPtr _window;
    private readonly bool _mouseCanUseGlobalState;
    private readonly Dictionary<ImGuiMouseCursor, IntPtr> _mouseCursors = new();
    private int _mouseButtonsDown;
    private int _pendingMouseLeaveFrame;
    private IntPtr _clipboardTextData;
    private GCHandle _gcHandle;
    private bool _cursorOverridden;

    public ImGuiBackend(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _window = mainWindow.SDLWindow;

        var io = ImGui.GetIO();
        Trace.Assert(io.BackendPlatformUserData == IntPtr.Zero, "Already initialized a platform backend!");

        // Check and store if we are on a SDL backend that supports global mouse position
        // ("wayland" and "rpi" don't support it, but we chose to use a white-list instead of a black-list)
        var sdlBackend = SDL_GetCurrentVideoDriver();
        var globalMouseWhitelist = ImmutableHashSet.Create("windows", "cocoa", "x11", "dive", "vman");
        var mouseCanUseGlobalState = globalMouseWhitelist.Contains(sdlBackend.ToLowerInvariant());

        // Setup backend capabilities flags
        io.BackendPlatformUserData = new IntPtr(1);
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors; // We can honor GetMouseCursor() values (optional)
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos; // We can honor io.WantSetMousePos requests (optional, rarely used)

        _mouseCanUseGlobalState = mouseCanUseGlobalState;

        _gcHandle = GCHandle.Alloc(this);
        unsafe
        {
            io.ClipboardUserData = GCHandle.ToIntPtr(_gcHandle);
            io.SetClipboardTextFn = new IntPtr((delegate* unmanaged<IntPtr, IntPtr, void>) &SetClipboardTextCallback);
            io.GetClipboardTextFn = new IntPtr((delegate* unmanaged<IntPtr, IntPtr>) &GetClipboardTextCallback);
        }

        // Load mouse cursors
        _mouseCursors[ImGuiMouseCursor.Arrow] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
        _mouseCursors[ImGuiMouseCursor.TextInput] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM);
        _mouseCursors[ImGuiMouseCursor.ResizeAll] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL);
        _mouseCursors[ImGuiMouseCursor.ResizeNS] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS);
        _mouseCursors[ImGuiMouseCursor.ResizeEW] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE);
        _mouseCursors[ImGuiMouseCursor.ResizeNESW] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW);
        _mouseCursors[ImGuiMouseCursor.ResizeNWSE] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE);
        _mouseCursors[ImGuiMouseCursor.Hand] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND);
        _mouseCursors[ImGuiMouseCursor.NotAllowed] = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO);

        // Set platform dependent data in viewport
        var mainViewport = ImGui.GetMainViewport();
        mainViewport.PlatformHandleRaw = mainWindow.NativeHandle;

        // Set SDL hint to receive mouse click events on window focus, otherwise SDL doesn't emit the event.
        // Without this, when clicking to gain focus, our widgets wouldn't activate even though they showed as hovered.
        // (This is unfortunately a global SDL setting, so enabling it might have a side-effect on your application.
        // It is unlikely to make a difference, but if your app absolutely needs to ignore the initial on-focus click:
        // you can ignore SDL_MOUSEBUTTONDOWN events coming right after a SDL_WINDOWEVENT_FOCUS_GAINED)
        SDL_SetHint(SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
    }

    static void ImGui_ImplSDL2_UpdateKeyModifiers(SDL_Keymod keyMods)
    {
        var io = ImGui.GetIO();
        io.AddKeyEvent(ImGuiKey.ModCtrl, (keyMods & SDL_Keymod.KMOD_CTRL) != 0);
        io.AddKeyEvent(ImGuiKey.ModShift, (keyMods & SDL_Keymod.KMOD_SHIFT) != 0);
        io.AddKeyEvent(ImGuiKey.ModAlt, (keyMods & SDL_Keymod.KMOD_ALT) != 0);
        io.AddKeyEvent(ImGuiKey.ModSuper, (keyMods & SDL_Keymod.KMOD_GUI) != 0);
    }

    // You can read the io.WantCaptureMouse, io.WantCaptureKeyboard flags to tell if dear imgui wants to use your inputs.
    // - When io.WantCaptureMouse is true, do not dispatch mouse input data to your main application, or clear/overwrite your copy of the mouse data.
    // - When io.WantCaptureKeyboard is true, do not dispatch keyboard input data to your main application, or clear/overwrite your copy of the keyboard data.
    // Generally you may always pass all inputs to dear imgui, and hide them from your application based on those two flags.
    // If you have multiple SDL events and some of them are not meant to be used by dear imgui, you may need to filter events based on their windowID field.
    public bool ProcessEvent(ref SDL_Event e)
    {
        var io = ImGui.GetIO();

        switch (e.type)
        {
            case SDL_EventType.SDL_MOUSEMOTION:
            {
                io.AddMousePosEvent(e.motion.x, e.motion.y);
                return io.WantCaptureMouse;
            }
            case SDL_EventType.SDL_MOUSEWHEEL:
            {
                var wheelX = (e.wheel.x > 0) ? 1.0f : (e.wheel.x < 0) ? -1.0f : 0.0f;
                var wheelY = (e.wheel.y > 0) ? 1.0f : (e.wheel.y < 0) ? -1.0f : 0.0f;
                io.AddMouseWheelEvent(wheelX, wheelY);
                return io.WantCaptureMouse;
            }
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
            case SDL_EventType.SDL_MOUSEBUTTONUP:
            {
                int mouseButton;
                if (e.button.button == SDL_BUTTON_LEFT)
                {
                    mouseButton = 0;
                }
                else if (e.button.button == SDL_BUTTON_RIGHT)
                {
                    mouseButton = 1;
                }
                else if (e.button.button == SDL_BUTTON_MIDDLE)
                {
                    mouseButton = 2;
                }
                else if (e.button.button == SDL_BUTTON_X1)
                {
                    mouseButton = 3;
                }
                else if (e.button.button == SDL_BUTTON_X2)
                {
                    mouseButton = 4;
                }
                else
                {
                    break;
                }

                io.AddMouseButtonEvent(mouseButton, (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN));
                _mouseButtonsDown = (e.type == SDL_EventType.SDL_MOUSEBUTTONDOWN) ? (_mouseButtonsDown | (1 << mouseButton)) : (_mouseButtonsDown & ~(1 << mouseButton));
                return io.WantCaptureMouse;
            }
            case SDL_EventType.SDL_TEXTINPUT:
            {
                unsafe
                {
                    fixed (byte* bytes = e.text.text)
                    {
                        io.AddInputCharactersUTF8(Marshal.PtrToStringUTF8(new IntPtr(bytes)));
                    }
                }

                return io.WantCaptureKeyboard;
            }
            case SDL_EventType.SDL_KEYDOWN:
            case SDL_EventType.SDL_KEYUP:
            {
                ImGui_ImplSDL2_UpdateKeyModifiers(e.key.keysym.mod);
                ImGuiKey key = ImGui_ImplSDL2_KeycodeToImGuiKey(e.key.keysym.sym);
                io.AddKeyEvent(key, (e.type == SDL_EventType.SDL_KEYDOWN));
                // To support legacy indexing (<1.87 user code). Legacy backend uses SDLK_*** as indices to IsKeyXXX() functions.
                io.SetKeyEventNativeData(key, (int) e.key.keysym.sym, (int) e.key.keysym.scancode);
                return io.WantCaptureKeyboard;
            }
            case SDL_EventType.SDL_WINDOWEVENT:
            {
                // - When capturing mouse, SDL will send a bunch of conflicting LEAVE/ENTER event on every mouse move, but the final ENTER tends to be right.
                // - However we won't get a correct LEAVE @event for a captured window.
                // - In some cases, when detaching a window from main viewport SDL may send SDL_WINDOWEVENT_ENTER one frame too late,
                //   causing SDL_WINDOWEVENT_LEAVE on previous frame to interrupt drag operation by clear mouse position. This is why
                //   we delay process the SDL_WINDOWEVENT_LEAVE events by one frame. See issue #5012 for details.
                var windowEvent = e.window.windowEvent;
                switch (windowEvent)
                {
                    case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                        _pendingMouseLeaveFrame = 0;
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                        _pendingMouseLeaveFrame = ImGui.GetFrameCount() + 1;
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                        io.AddFocusEvent(true);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                        io.AddFocusEvent(false);
                        break;
                }

                break;
            }
        }

        return false;
    }

    // Functions
    [UnmanagedCallersOnly]
    private static IntPtr GetClipboardTextCallback(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var self = (ImGuiBackend) handle.Target;
        if (self != null)
        {
            if (self._clipboardTextData != IntPtr.Zero)
            {
                Marshal.ZeroFreeCoTaskMemUTF8(self._clipboardTextData);
                self._clipboardTextData = IntPtr.Zero;
            }

            self._clipboardTextData = Marshal.StringToCoTaskMemUTF8(Clipboard.GetText());

            return self._clipboardTextData;
        }

        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly]
    private static void SetClipboardTextCallback(IntPtr userData, IntPtr textPtr)
    {
        var text = Marshal.PtrToStringUTF8(textPtr);
        Clipboard.SetText(text);
    }

    private void UpdateMouseData()
    {
        var io = ImGui.GetIO();

        // We forward mouse input when hovered or captured (via SDL_MOUSEMOTION) or when focused (below)
        // SDL_CaptureMouse() let the OS know e.g. that our imgui drag outside the SDL window boundaries shouldn't e.g. trigger other operations outside
        SDL_CaptureMouse(_mouseButtonsDown != 0 ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE);
        var focusedWindow = SDL_GetKeyboardFocus();
        var isAppFocused = (_window == focusedWindow);

        if (isAppFocused)
        {
            // (Optional) Set OS mouse position from Dear ImGui if requested (rarely used, only when ImGuiConfigFlags_NavEnableSetMousePos is enabled by user)
            if (io.WantSetMousePos)
                SDL_WarpMouseInWindow(_window, (int) io.MousePos.X, (int) io.MousePos.Y);

            // (Optional) Fallback to provide mouse position when focused (SDL_MOUSEMOTION already provides this when hovered or captured)
            if (_mouseCanUseGlobalState && _mouseButtonsDown == 0)
            {
                SDL_GetGlobalMouseState(out var mouseXGlobal, out var mouseYGlobal);
                SDL_GetWindowPosition(_window, out var windowX, out var windowY);
                io.AddMousePosEvent(mouseXGlobal - windowX, mouseYGlobal - windowY);
            }
        }
    }

    private void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if ((io.ConfigFlags & ImGuiConfigFlags.NoMouseCursorChange) != 0)
            return;

        if (io.WantCaptureMouse)
        {
            _cursorOverridden = true;
            var imguiCursor = ImGui.GetMouseCursor();
            if (imguiCursor == ImGuiMouseCursor.None)
            {
                SDL_ShowCursor(SDL_DISABLE);
            }
            else
            {
                var cursor = _mouseCursors.GetValueOrDefault(imguiCursor, _mouseCursors[ImGuiMouseCursor.Arrow]);
                SDL_SetCursor(cursor);
                SDL_ShowCursor(SDL_ENABLE);
            }
        }
        else if (_cursorOverridden)
        {
            // Reset back to previous cursor
            _cursorOverridden = false;
            _mainWindow.UpdateCursor();
        }
    }

    private static void UpdateGamepads()
    {
        var io = ImGui.GetIO();
        // FIXME: Technically feeding gamepad shouldn't depend on this now that they are regular inputs.
        if ((io.ConfigFlags & ImGuiConfigFlags.NavEnableGamepad) == 0)
            return;

        // Get gamepad
        io.BackendFlags &= ~ImGuiBackendFlags.HasGamepad;
        var gameController = SDL_GameControllerOpen(0);
        if (gameController == IntPtr.Zero)
            return;
        io.BackendFlags |= ImGuiBackendFlags.HasGamepad;

        // Update gamepad inputs
        static float Saturate(float v)
        {
            return v < 0.0f ? 0.0f : v > 1.0f ? 1.0f : v;
        }

        void MapButton(ImGuiKey keyNo, SDL_GameControllerButton buttonNo)
        {
            io.AddKeyEvent(keyNo, SDL_GameControllerGetButton(gameController, buttonNo) != 0);
        }

        void MapAnalog(ImGuiKey keyNo, SDL_GameControllerAxis axisNo, int v0, int v1)
        {
            float vn = (SDL_GameControllerGetAxis(gameController, axisNo) - v0) / (float) (v1 - v0);
            vn = Saturate(vn);
            io.AddKeyAnalogEvent(keyNo, vn > 0.1f, vn);
        }

        const int thumbDeadZone = 8000; // SDL_gamecontroller.h suggests using this value.
        MapButton(ImGuiKey.GamepadStart, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);
        MapButton(ImGuiKey.GamepadBack, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
        MapButton(ImGuiKey.GamepadFaceLeft, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X); // Xbox X, PS Square
        MapButton(ImGuiKey.GamepadFaceRight, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B); // Xbox B, PS Circle
        MapButton(ImGuiKey.GamepadFaceUp, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y); // Xbox Y, PS Triangle
        MapButton(ImGuiKey.GamepadFaceDown, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A); // Xbox A, PS Cross
        MapButton(ImGuiKey.GamepadDpadLeft, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT);
        MapButton(ImGuiKey.GamepadDpadRight, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT);
        MapButton(ImGuiKey.GamepadDpadUp, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP);
        MapButton(ImGuiKey.GamepadDpadDown, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN);
        MapButton(ImGuiKey.GamepadL1, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
        MapButton(ImGuiKey.GamepadR1, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);
        MapAnalog(ImGuiKey.GamepadL2, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT, 0, 32767);
        MapAnalog(ImGuiKey.GamepadR2, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT, 0, 32767);
        MapButton(ImGuiKey.GamepadL3, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
        MapButton(ImGuiKey.GamepadR3, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);
        MapAnalog(ImGuiKey.GamepadLStickLeft, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX, -thumbDeadZone, -32768);
        MapAnalog(ImGuiKey.GamepadLStickRight, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX, +thumbDeadZone, +32767);
        MapAnalog(ImGuiKey.GamepadLStickUp, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY, -thumbDeadZone, -32768);
        MapAnalog(ImGuiKey.GamepadLStickDown, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY, +thumbDeadZone, +32767);
        MapAnalog(ImGuiKey.GamepadRStickLeft, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX, -thumbDeadZone, -32768);
        MapAnalog(ImGuiKey.GamepadRStickRight, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX, +thumbDeadZone, +32767);
        MapAnalog(ImGuiKey.GamepadRStickUp, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY, -thumbDeadZone, -32768);
        MapAnalog(ImGuiKey.GamepadRStickDown, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY, +thumbDeadZone, +32767);
    }

    public void NewFrame()
    {
        var io = ImGui.GetIO();

        if (_pendingMouseLeaveFrame != 0 && _pendingMouseLeaveFrame >= ImGui.GetFrameCount() && _mouseButtonsDown == 0)
        {
            io.AddMousePosEvent(-float.MaxValue, -float.MaxValue);
            _pendingMouseLeaveFrame = 0;
        }

        UpdateMouseData();
        UpdateMouseCursor();

        // Update game controllers (if enabled and available)
        UpdateGamepads();
    }

    private static ImGuiKey ImGui_ImplSDL2_KeycodeToImGuiKey(SDL_Keycode keycode)
    {
        switch (keycode)
        {
            case SDL_Keycode.SDLK_TAB: return ImGuiKey.Tab;
            case SDL_Keycode.SDLK_LEFT: return ImGuiKey.LeftArrow;
            case SDL_Keycode.SDLK_RIGHT: return ImGuiKey.RightArrow;
            case SDL_Keycode.SDLK_UP: return ImGuiKey.UpArrow;
            case SDL_Keycode.SDLK_DOWN: return ImGuiKey.DownArrow;
            case SDL_Keycode.SDLK_PAGEUP: return ImGuiKey.PageUp;
            case SDL_Keycode.SDLK_PAGEDOWN: return ImGuiKey.PageDown;
            case SDL_Keycode.SDLK_HOME: return ImGuiKey.Home;
            case SDL_Keycode.SDLK_END: return ImGuiKey.End;
            case SDL_Keycode.SDLK_INSERT: return ImGuiKey.Insert;
            case SDL_Keycode.SDLK_DELETE: return ImGuiKey.Delete;
            case SDL_Keycode.SDLK_BACKSPACE: return ImGuiKey.Backspace;
            case SDL_Keycode.SDLK_SPACE: return ImGuiKey.Space;
            case SDL_Keycode.SDLK_RETURN: return ImGuiKey.Enter;
            case SDL_Keycode.SDLK_ESCAPE: return ImGuiKey.Escape;
            case SDL_Keycode.SDLK_QUOTE: return ImGuiKey.Apostrophe;
            case SDL_Keycode.SDLK_COMMA: return ImGuiKey.Comma;
            case SDL_Keycode.SDLK_MINUS: return ImGuiKey.Minus;
            case SDL_Keycode.SDLK_PERIOD: return ImGuiKey.Period;
            case SDL_Keycode.SDLK_SLASH: return ImGuiKey.Slash;
            case SDL_Keycode.SDLK_SEMICOLON: return ImGuiKey.Semicolon;
            case SDL_Keycode.SDLK_EQUALS: return ImGuiKey.Equal;
            case SDL_Keycode.SDLK_LEFTBRACKET: return ImGuiKey.LeftBracket;
            case SDL_Keycode.SDLK_BACKSLASH: return ImGuiKey.Backslash;
            case SDL_Keycode.SDLK_RIGHTBRACKET: return ImGuiKey.RightBracket;
            case SDL_Keycode.SDLK_BACKQUOTE: return ImGuiKey.GraveAccent;
            case SDL_Keycode.SDLK_CAPSLOCK: return ImGuiKey.CapsLock;
            case SDL_Keycode.SDLK_SCROLLLOCK: return ImGuiKey.ScrollLock;
            case SDL_Keycode.SDLK_NUMLOCKCLEAR: return ImGuiKey.NumLock;
            case SDL_Keycode.SDLK_PRINTSCREEN: return ImGuiKey.PrintScreen;
            case SDL_Keycode.SDLK_PAUSE: return ImGuiKey.Pause;
            case SDL_Keycode.SDLK_KP_0: return ImGuiKey.Keypad0;
            case SDL_Keycode.SDLK_KP_1: return ImGuiKey.Keypad1;
            case SDL_Keycode.SDLK_KP_2: return ImGuiKey.Keypad2;
            case SDL_Keycode.SDLK_KP_3: return ImGuiKey.Keypad3;
            case SDL_Keycode.SDLK_KP_4: return ImGuiKey.Keypad4;
            case SDL_Keycode.SDLK_KP_5: return ImGuiKey.Keypad5;
            case SDL_Keycode.SDLK_KP_6: return ImGuiKey.Keypad6;
            case SDL_Keycode.SDLK_KP_7: return ImGuiKey.Keypad7;
            case SDL_Keycode.SDLK_KP_8: return ImGuiKey.Keypad8;
            case SDL_Keycode.SDLK_KP_9: return ImGuiKey.Keypad9;
            case SDL_Keycode.SDLK_KP_PERIOD: return ImGuiKey.KeypadDecimal;
            case SDL_Keycode.SDLK_KP_DIVIDE: return ImGuiKey.KeypadDivide;
            case SDL_Keycode.SDLK_KP_MULTIPLY: return ImGuiKey.KeypadMultiply;
            case SDL_Keycode.SDLK_KP_MINUS: return ImGuiKey.KeypadSubtract;
            case SDL_Keycode.SDLK_KP_PLUS: return ImGuiKey.KeypadAdd;
            case SDL_Keycode.SDLK_KP_ENTER: return ImGuiKey.KeypadEnter;
            case SDL_Keycode.SDLK_KP_EQUALS: return ImGuiKey.KeypadEqual;
            case SDL_Keycode.SDLK_LCTRL: return ImGuiKey.LeftCtrl;
            case SDL_Keycode.SDLK_LSHIFT: return ImGuiKey.LeftShift;
            case SDL_Keycode.SDLK_LALT: return ImGuiKey.LeftAlt;
            case SDL_Keycode.SDLK_LGUI: return ImGuiKey.LeftSuper;
            case SDL_Keycode.SDLK_RCTRL: return ImGuiKey.RightCtrl;
            case SDL_Keycode.SDLK_RSHIFT: return ImGuiKey.RightShift;
            case SDL_Keycode.SDLK_RALT: return ImGuiKey.RightAlt;
            case SDL_Keycode.SDLK_RGUI: return ImGuiKey.RightSuper;
            case SDL_Keycode.SDLK_APPLICATION: return ImGuiKey.Menu;
            case SDL_Keycode.SDLK_0: return ImGuiKey._0;
            case SDL_Keycode.SDLK_1: return ImGuiKey._1;
            case SDL_Keycode.SDLK_2: return ImGuiKey._2;
            case SDL_Keycode.SDLK_3: return ImGuiKey._3;
            case SDL_Keycode.SDLK_4: return ImGuiKey._4;
            case SDL_Keycode.SDLK_5: return ImGuiKey._5;
            case SDL_Keycode.SDLK_6: return ImGuiKey._6;
            case SDL_Keycode.SDLK_7: return ImGuiKey._7;
            case SDL_Keycode.SDLK_8: return ImGuiKey._8;
            case SDL_Keycode.SDLK_9: return ImGuiKey._9;
            case SDL_Keycode.SDLK_a: return ImGuiKey.A;
            case SDL_Keycode.SDLK_b: return ImGuiKey.B;
            case SDL_Keycode.SDLK_c: return ImGuiKey.C;
            case SDL_Keycode.SDLK_d: return ImGuiKey.D;
            case SDL_Keycode.SDLK_e: return ImGuiKey.E;
            case SDL_Keycode.SDLK_f: return ImGuiKey.F;
            case SDL_Keycode.SDLK_g: return ImGuiKey.G;
            case SDL_Keycode.SDLK_h: return ImGuiKey.H;
            case SDL_Keycode.SDLK_i: return ImGuiKey.I;
            case SDL_Keycode.SDLK_j: return ImGuiKey.J;
            case SDL_Keycode.SDLK_k: return ImGuiKey.K;
            case SDL_Keycode.SDLK_l: return ImGuiKey.L;
            case SDL_Keycode.SDLK_m: return ImGuiKey.M;
            case SDL_Keycode.SDLK_n: return ImGuiKey.N;
            case SDL_Keycode.SDLK_o: return ImGuiKey.O;
            case SDL_Keycode.SDLK_p: return ImGuiKey.P;
            case SDL_Keycode.SDLK_q: return ImGuiKey.Q;
            case SDL_Keycode.SDLK_r: return ImGuiKey.R;
            case SDL_Keycode.SDLK_s: return ImGuiKey.S;
            case SDL_Keycode.SDLK_t: return ImGuiKey.T;
            case SDL_Keycode.SDLK_u: return ImGuiKey.U;
            case SDL_Keycode.SDLK_v: return ImGuiKey.V;
            case SDL_Keycode.SDLK_w: return ImGuiKey.W;
            case SDL_Keycode.SDLK_x: return ImGuiKey.X;
            case SDL_Keycode.SDLK_y: return ImGuiKey.Y;
            case SDL_Keycode.SDLK_z: return ImGuiKey.Z;
            case SDL_Keycode.SDLK_F1: return ImGuiKey.F1;
            case SDL_Keycode.SDLK_F2: return ImGuiKey.F2;
            case SDL_Keycode.SDLK_F3: return ImGuiKey.F3;
            case SDL_Keycode.SDLK_F4: return ImGuiKey.F4;
            case SDL_Keycode.SDLK_F5: return ImGuiKey.F5;
            case SDL_Keycode.SDLK_F6: return ImGuiKey.F6;
            case SDL_Keycode.SDLK_F7: return ImGuiKey.F7;
            case SDL_Keycode.SDLK_F8: return ImGuiKey.F8;
            case SDL_Keycode.SDLK_F9: return ImGuiKey.F9;
            case SDL_Keycode.SDLK_F10: return ImGuiKey.F10;
            case SDL_Keycode.SDLK_F11: return ImGuiKey.F11;
            case SDL_Keycode.SDLK_F12: return ImGuiKey.F12;
        }

        return ImGuiKey.None;
    }

    public void Dispose()
    {
        if (_clipboardTextData != IntPtr.Zero)
        {
            Marshal.ZeroFreeCoTaskMemUTF8(_clipboardTextData);
            _clipboardTextData = IntPtr.Zero;
        }

        _gcHandle.Free();

        foreach (var cursor in _mouseCursors.Values)
        {
            SDL_FreeCursor(cursor);
        }

        _mouseCursors.Clear();

        var io = ImGui.GetIO();
        if (io.BackendPlatformUserData == new IntPtr(1))
        {
            io.SetClipboardTextFn = IntPtr.Zero;
            io.GetClipboardTextFn = IntPtr.Zero;
            io.ClipboardUserData = IntPtr.Zero;
            io.BackendPlatformUserData = IntPtr.Zero;
        }
    }
}