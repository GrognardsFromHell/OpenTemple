using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Platform
{
    public delegate void MouseMoveHandler(int x, int y, int wheelDelta);

    public delegate bool WindowMsgFilter(uint msg, ulong wparam, long lparam);

    internal delegate IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam);

    public class MainWindow : IMainWindow, IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly WindowConfig _config;

        private GCHandle _gcHandle;

        public IntPtr NativeHandle => mHwnd;

        public MainWindow(WindowConfig config)
        {
            _config = config;
            mHinstance = GetModuleHandle(null);

            // This GC handle is attached to windows created with CreateWindow
            _gcHandle = GCHandle.Alloc(this);

            RegisterWndClass();
            CreateHwnd();
        }

        public void Dispose()
        {
            if (mHwnd != IntPtr.Zero)
            {
                DestroyWindow(mHwnd);
            }

            _gcHandle.Free();

            UnregisterWndClass();
        }

        public IntPtr GetHwnd()
        {
            return mHwnd;
        }

        public IntPtr GetHinstance()
        {
            return mHinstance;
        }

        public int GetWidth()
        {
            return mWidth;
        }

        public int GetHeight()
        {
            return mHeight;
        }

        public bool IsInForeground { get; set; } = true;

        public event Action<bool> IsInForegroundChanged;

        private bool unsetClip = false;

        public event Action<Size> Resized;

        // Locks the mouse cursor to this window
        // if we're in the foreground
        public void LockCursor(int x, int y, int w, int h)
        {
            bool isForeground = GetForegroundWindow() == mHwnd;

            if (isForeground)
            {
                RECT rect = new RECT {X = x, Y = y, Right = x + w, Bottom = y + h};

                ClipCursor(ref rect);
                unsetClip = false;
            }
            else // Unset the CursorClip  (was supposed to happen naturally when alt-tabbing, but sometimes it requires hitting a keystroke before it takes effect so adding it here too)
            {
                // Check if already unset the CursorClip so it doesn't cause interference in case some other app is trying to clip the cursor...
                if (unsetClip)
                    return;
                unsetClip = true;

                ClipCursor(IntPtr.Zero);
            }
        }

        public void SetMouseMoveHandler(MouseMoveHandler handler)
        {
            mMouseMoveHandler = handler;
        }

        // Sets a filter that receives a chance at intercepting all window messages
        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
            mWindowMsgFilter = filter;
        }

        private IntPtr mHinstance;
        private IntPtr mHwnd;
        private int mWidth;
        private int mHeight;

        /*
	        The window class name used for RegisterClass
	        and CreateWindow.
        */
        private const string WindowClassName = "SpicyTempleMainWnd";
        private const string WindowTitle = "SPICY TEMPLE";

        private void RegisterWndClass()
        {
            var wndClass = new WNDCLASSEX();
            wndClass.cbSize = Marshal.SizeOf<WNDCLASSEX>();
            wndClass.lpfnWndProc = WndProcTrampolineDelegate; // Make sure to use the delegate object here
            wndClass.hInstance = mHinstance;
            wndClass.hIcon = LoadIcon(mHinstance, "icon");
            wndClass.hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW);
            wndClass.hbrBackground = GetStockObject(StockObjects.BLACK_BRUSH);
            wndClass.lpszClassName = WindowClassName;
            wndClass.cbWndExtra = Marshal.SizeOf<GCHandle>();

            if (RegisterClassEx(ref wndClass) == 0)
            {
                throw new Exception("Unable to register window class: " + Marshal.GetLastWin32Error());
            }
        }

        private void UnregisterWndClass()
        {
            if (!UnregisterClass(WindowClassName, mHinstance))
            {
                Logger.Error("Unable to unregister window class: " + Marshal.GetLastWin32Error());
            }
        }

        public void CreateHwnd()
        {
            CreateWindowRectAndStyles(out var windowRect, out var style, out var styleEx);

            style |= WindowStyles.WS_VISIBLE;

            var width = windowRect.Width;
            var height = windowRect.Height;
            Logger.Info("Creating window with dimensions {0}x{1}", width, height);
            mHwnd = CreateWindowEx(
                styleEx,
                WindowClassName,
                WindowTitle,
                style,
                windowRect.Left,
                windowRect.Top,
                width,
                height,
                IntPtr.Zero,
                IntPtr.Zero,
                mHinstance,
                IntPtr.Zero);

            if (mHwnd == IntPtr.Zero)
            {
                throw new Exception("Unable to create main window: " + Marshal.GetLastWin32Error());
            }

            // Store our this pointer in the window
            SetWindowLongPtr(mHwnd, 0, GCHandle.ToIntPtr(_gcHandle));
        }

        private void CreateWindowRectAndStyles(out RECT windowRect, out WindowStyles style, out WindowStylesEx styleEx)
        {
            var screenWidth = GetSystemMetrics(SystemMetric.SM_CXSCREEN);
            var screenHeight = GetSystemMetrics(SystemMetric.SM_CYSCREEN);

            styleEx = 0;

            if (!_config.Windowed)
            {
                windowRect = new RECT(0, 0, screenWidth, screenHeight);
                style = WindowStyles.WS_POPUP;
                mWidth = screenWidth;
                mHeight = screenHeight;
            }
            else
            {
                windowRect = new RECT(
                    (screenWidth - _config.Width) / 2,
                    (screenHeight - _config.Height) / 2,
                    0,
                    0);
                windowRect.Right = windowRect.Left + _config.Width;
                windowRect.Bottom = windowRect.Top + _config.Height;

                // style = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
                style = WindowStyles.WS_OVERLAPPEDWINDOW;

                AdjustWindowRectEx(ref windowRect, style, false, styleEx);
                int extraWidth = (windowRect.Right - windowRect.Left) - _config.Width;
                int extraHeight = (windowRect.Bottom - windowRect.Top) - _config.Height;
                windowRect.Left = (screenWidth - _config.Width) / 2 - (extraWidth / 2);
                windowRect.Top = (screenHeight - _config.Height) / 2 - (extraHeight / 2);
                windowRect.Right = windowRect.Left + _config.Width + extraWidth;
                windowRect.Bottom = windowRect.Top + _config.Height + extraHeight;

                mWidth = _config.Width;
                mHeight = _config.Height;
            }
        }

        // Keep a delegate object for the trampoline around to prevent it from being GC'd
        private static readonly WndProc WndProcTrampolineDelegate = WndProcTrampoline;

        private static IntPtr WndProcTrampoline(IntPtr hWnd, uint msg, ulong wparam, long lparam)
        {
            // Retrieve our this pointer from the wnd
            var handlePtr = GetWindowLongPtr(hWnd, 0);
            if (handlePtr != IntPtr.Zero)
            {
                var handle = GCHandle.FromIntPtr(handlePtr);

                if (handle.IsAllocated)
                {
                    var mainWindow = (MainWindow) handle.Target;
                    return mainWindow.WndProc(hWnd, msg, wparam, lparam);
                }
            }

            return DefWindowProc(hWnd, msg, wparam, lparam);
        }

        private int mousePosX = 0; // Replaces memory @ 10D25CEC
        private int mousePosY = 0; // Replaces memory @ 10D25CF0

        private IntPtr WndProc(IntPtr hWnd, uint msg, ulong wParam, long lParam)
        {
            if (hWnd != mHwnd)
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            if (mWindowMsgFilter != null && mWindowMsgFilter(msg, wParam, lParam))
            {
                return DefWindowProc(hWnd, msg, wParam, lParam);
            }

            switch (msg)
            {
                case WM_SETFOCUS:
                    // Make our window topmost unless a debugger is attached
                    if ((IntPtr) wParam == mHwnd && !IsDebuggerPresent())
                    {
                        SetWindowPos(mHwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_KILLFOCUS:
                    // Make our window topmost unless a debugger is attached
                    if ((IntPtr) wParam == mHwnd && !IsDebuggerPresent())
                    {
                        SetWindowPos(mHwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_SYSCOMMAND:
                    if (wParam == SC_KEYMENU || wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER)
                    {
                        return IntPtr.Zero;
                    }

                    break;
                case WM_SIZE:
                    Resized?.Invoke(new Size(
                        WindowsMessageUtils.GetXParam(lParam),
                        WindowsMessageUtils.GetYParam(lParam)
                    ));
                    break;
                case WM_ACTIVATEAPP:
                    IsInForeground = wParam == 1;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                    break;
                case WM_ERASEBKGND:
                    return IntPtr.Zero;
                case WM_CLOSE:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(1)));
                    break;
                case WM_QUIT:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs((int) wParam)));
                    break;
                case WM_LBUTTONDOWN:
                    Tig.Mouse.SetButtonState(MouseButton.LEFT, true);
                    break;
                case WM_LBUTTONUP:
                    Tig.Mouse.SetButtonState(MouseButton.LEFT, false);
                    break;
                case WM_RBUTTONDOWN:
                    Tig.Mouse.SetButtonState(MouseButton.RIGHT, true);
                    break;
                case WM_RBUTTONUP:
                    Tig.Mouse.SetButtonState(MouseButton.RIGHT, false);
                    break;
                case WM_MBUTTONDOWN:
                    Tig.Mouse.SetMmbReference();
                    Tig.Mouse.SetButtonState(MouseButton.MIDDLE, true);
                    break;
                case WM_MBUTTONUP:
                    Tig.Mouse.ResetMmbReference();
                    Tig.Mouse.SetButtonState(MouseButton.MIDDLE, false);
                    break;
                case WM_SYSKEYDOWN:
                case WM_KEYDOWN:
                {
                    var key = (DIK) ToDirectInputKey((VirtualKey) wParam);
                    if (key != 0)
                    {
                        Tig.MessageQueue.Enqueue(new Message(
                            new MessageKeyStateChangeArgs
                            {
                                key = key,
                                // Means it has changed to pressed
                                down = true
                            }
                        ));
                    }
                }
                    break;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                {
                    var key = (DIK) ToDirectInputKey((VirtualKey) wParam);
                    if (key != 0)
                    {
                        Tig.MessageQueue.Enqueue(new Message(
                            new MessageKeyStateChangeArgs
                            {
                                key = key,
                                // Means it has changed to up
                                down = false
                            }
                        ));
                    }
                }
                    break;
                case WM_CHAR:
                    Tig.MessageQueue.Enqueue(new Message(new CharMessageArgs((VirtualKey) wParam)));
                    break;
                case WM_MOUSEWHEEL:
                    UpdateMousePos(
                        mousePosX,
                        mousePosY,
                        WindowsMessageUtils.GetWheelDelta(wParam)
                    );
                    break;
                case WM_MOUSEMOVE:
                    mousePosX = WindowsMessageUtils.GetXParam(lParam);
                    mousePosY = WindowsMessageUtils.GetYParam(lParam);
                    UpdateMousePos(mousePosX, mousePosY, 0);
                    break;
            }

            if (msg != WM_KEYDOWN)
            {
                UpdateMousePos(mousePosX, mousePosY, 0);
            }

            // Previously, ToEE called a global window proc here but it did nothing useful.
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool IsDebuggerPresent();

        private const uint WM_ACTIVATEAPP = 0x001C;
        private const uint WM_SETFOCUS = 0x0007;
        private const uint WM_KILLFOCUS = 0x0008;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint WM_SIZE = 0x0005;
        private const uint WM_ERASEBKGND = 0x0014;

        private const uint WM_CHAR = 0x0102;
        private const uint WM_CLOSE = 0x0010;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MBUTTONDOWN = 0x0207;
        private const uint WM_MBUTTONUP = 0x0208;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_QUIT = 0x0012;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
        private const uint WM_SYSKEYDOWN = 0x0104;
        private const uint WM_SYSKEYUP = 0x0105;


        private const uint SC_KEYMENU = 0xF100;
        private const uint SC_SCREENSAVE = 0xF140;
        private const uint SC_MONITORPOWER = 0xF170;

        private uint ToDirectInputKey(VirtualKey vk)
        {
            // Special case for keys that can be on the numpad
            switch (vk)
            {
                case VirtualKey.VK_HOME:
                    return (uint) DIK.DIK_HOME; /* Home on arrow keypad */
                case VirtualKey.VK_END:
                    return (uint) DIK.DIK_END; /* End on arrow keypad */
                case VirtualKey.VK_PRIOR:
                    return (uint) DIK.DIK_PRIOR; /* PgUp on arrow keypad */
                case VirtualKey.VK_NEXT:
                    return (uint) DIK.DIK_NEXT; /* PgDn on arrow keypad */
                case VirtualKey.VK_VOLUME_DOWN:
                    return (uint) DIK.DIK_VOLUMEDOWN;
                case VirtualKey.VK_VOLUME_UP:
                    return (uint) DIK.DIK_VOLUMEUP;
                case VirtualKey.VK_VOLUME_MUTE:
                    return (uint) DIK.DIK_MUTE;
            }

            // This seems to map using scan codes, which
            // actually look like original US keyboard ones
            var mapped = MapVirtualKey((uint) vk, MAPVK_VK_TO_VSC);
            if (mapped != 0)
            {
                return mapped;
            }

            switch (vk)
            {
                case VirtualKey.VK_ESCAPE:
                    return (byte) DIK.DIK_ESCAPE;
                case VirtualKey.VK_1:
                    return (byte) DIK.DIK_1;
                case VirtualKey.VK_2:
                    return (byte) DIK.DIK_2;
                case VirtualKey.VK_3:
                    return (byte) DIK.DIK_3;
                case VirtualKey.VK_4:
                    return (byte) DIK.DIK_4;
                case VirtualKey.VK_5:
                    return (byte) DIK.DIK_5;
                case VirtualKey.VK_6:
                    return (byte) DIK.DIK_6;
                case VirtualKey.VK_7:
                    return (byte) DIK.DIK_7;
                case VirtualKey.VK_8:
                    return (byte) DIK.DIK_8;
                case VirtualKey.VK_9:
                    return (byte) DIK.DIK_9;
                case VirtualKey.VK_0:
                    return (byte) DIK.DIK_0;
                case VirtualKey.VK_OEM_MINUS:
                    return (byte) DIK.DIK_MINUS /* - on main keyboard */;
                case VirtualKey.VK_OEM_PLUS:
                    return (byte) DIK.DIK_EQUALS;
                case VirtualKey.VK_BACK:
                    return (byte) DIK.DIK_BACK /* backspace */;
                case VirtualKey.VK_TAB:
                    return (byte) DIK.DIK_TAB;
                case VirtualKey.VK_Q:
                    return (byte) DIK.DIK_Q;
                case VirtualKey.VK_W:
                    return (byte) DIK.DIK_W;
                case VirtualKey.VK_E:
                    return (byte) DIK.DIK_E;
                case VirtualKey.VK_R:
                    return (byte) DIK.DIK_R;
                case VirtualKey.VK_T:
                    return (byte) DIK.DIK_T;
                case VirtualKey.VK_Y:
                    return (byte) DIK.DIK_Y;
                case VirtualKey.VK_U:
                    return (byte) DIK.DIK_U;
                case VirtualKey.VK_I:
                    return (byte) DIK.DIK_I;
                case VirtualKey.VK_O:
                    return (byte) DIK.DIK_O;
                case VirtualKey.VK_P:
                    return (byte) DIK.DIK_P;
                case VirtualKey.VK_OEM_4:
                    return (byte) DIK.DIK_LBRACKET;
                case VirtualKey.VK_OEM_6:
                    return (byte) DIK.DIK_RBRACKET;
                case VirtualKey.VK_RETURN:
                    return (byte) DIK.DIK_RETURN /* Enter on main keyboard */;
                case VirtualKey.VK_LCONTROL:
                    return (byte) DIK.DIK_LCONTROL;
                case VirtualKey.VK_A:
                    return (byte) DIK.DIK_A;
                case VirtualKey.VK_S:
                    return (byte) DIK.DIK_S;
                case VirtualKey.VK_D:
                    return (byte) DIK.DIK_D;
                case VirtualKey.VK_F:
                    return (byte) DIK.DIK_F;
                case VirtualKey.VK_G:
                    return (byte) DIK.DIK_G;
                case VirtualKey.VK_H:
                    return (byte) DIK.DIK_H;
                case VirtualKey.VK_J:
                    return (byte) DIK.DIK_J;
                case VirtualKey.VK_K:
                    return (byte) DIK.DIK_K;
                case VirtualKey.VK_L:
                    return (byte) DIK.DIK_L;
                case VirtualKey.VK_OEM_1:
                    return (byte) DIK.DIK_SEMICOLON;
                case VirtualKey.VK_OEM_7:
                    return (byte) DIK.DIK_APOSTROPHE;
                case VirtualKey.VK_OEM_3:
                    return (byte) DIK.DIK_GRAVE /* accent grave */;
                case VirtualKey.VK_LSHIFT:
                    return (byte) DIK.DIK_LSHIFT;
                case VirtualKey.VK_OEM_5:
                    return (byte) DIK.DIK_BACKSLASH;
                case VirtualKey.VK_Z:
                    return (byte) DIK.DIK_Z;
                case VirtualKey.VK_X:
                    return (byte) DIK.DIK_X;
                case VirtualKey.VK_C:
                    return (byte) DIK.DIK_C;
                case VirtualKey.VK_V:
                    return (byte) DIK.DIK_V;
                case VirtualKey.VK_B:
                    return (byte) DIK.DIK_B;
                case VirtualKey.VK_N:
                    return (byte) DIK.DIK_N;
                case VirtualKey.VK_M:
                    return (byte) DIK.DIK_M;
                case VirtualKey.VK_OEM_COMMA:
                    return (byte) DIK.DIK_COMMA;
                case VirtualKey.VK_OEM_PERIOD:
                    return (byte) DIK.DIK_PERIOD /* . on main keyboard */;
                case VirtualKey.VK_OEM_2:
                    return (byte) DIK.DIK_SLASH /* / on main keyboard */;
                case VirtualKey.VK_RSHIFT:
                    return (byte) DIK.DIK_RSHIFT;
                case VirtualKey.VK_MULTIPLY:
                    return (byte) DIK.DIK_MULTIPLY /* * on numeric keypad */;
                case VirtualKey.VK_LMENU:
                    return (byte) DIK.DIK_LMENU /* left Alt */;
                case VirtualKey.VK_SPACE:
                    return (byte) DIK.DIK_SPACE;
                case VirtualKey.VK_CAPITAL:
                    return (byte) DIK.DIK_CAPITAL;
                case VirtualKey.VK_F1:
                    return (byte) DIK.DIK_F1;
                case VirtualKey.VK_F2:
                    return (byte) DIK.DIK_F2;
                case VirtualKey.VK_F3:
                    return (byte) DIK.DIK_F3;
                case VirtualKey.VK_F4:
                    return (byte) DIK.DIK_F4;
                case VirtualKey.VK_F5:
                    return (byte) DIK.DIK_F5;
                case VirtualKey.VK_F6:
                    return (byte) DIK.DIK_F6;
                case VirtualKey.VK_F7:
                    return (byte) DIK.DIK_F7;
                case VirtualKey.VK_F8:
                    return (byte) DIK.DIK_F8;
                case VirtualKey.VK_F9:
                    return (byte) DIK.DIK_F9;
                case VirtualKey.VK_F10:
                    return (byte) DIK.DIK_F10;
                case VirtualKey.VK_NUMLOCK:
                    return (byte) DIK.DIK_NUMLOCK;
                case VirtualKey.VK_SCROLL:
                    return (byte) DIK.DIK_SCROLL /* Scroll Lock */;
                case VirtualKey.VK_NUMPAD7:
                    return (byte) DIK.DIK_NUMPAD7;
                case VirtualKey.VK_NUMPAD8:
                    return (byte) DIK.DIK_NUMPAD8;
                case VirtualKey.VK_NUMPAD9:
                    return (byte) DIK.DIK_NUMPAD9;
                case VirtualKey.VK_SUBTRACT:
                    return (byte) DIK.DIK_SUBTRACT /* - on numeric keypad */;
                case VirtualKey.VK_NUMPAD4:
                    return (byte) DIK.DIK_NUMPAD4;
                case VirtualKey.VK_NUMPAD5:
                    return (byte) DIK.DIK_NUMPAD5;
                case VirtualKey.VK_NUMPAD6:
                    return (byte) DIK.DIK_NUMPAD6;
                case VirtualKey.VK_ADD:
                    return (byte) DIK.DIK_ADD /* + on numeric keypad */;
                case VirtualKey.VK_NUMPAD1:
                    return (byte) DIK.DIK_NUMPAD1;
                case VirtualKey.VK_NUMPAD2:
                    return (byte) DIK.DIK_NUMPAD2;
                case VirtualKey.VK_NUMPAD3:
                    return (byte) DIK.DIK_NUMPAD3;
                case VirtualKey.VK_NUMPAD0:
                    return (byte) DIK.DIK_NUMPAD0;
                case VirtualKey.VK_DECIMAL:
                    return (byte) DIK.DIK_DECIMAL /* . on numeric keypad */;
                case VirtualKey.VK_F11:
                    return (byte) DIK.DIK_F11;
                case VirtualKey.VK_F12:
                    return (byte) DIK.DIK_F12;
                case VirtualKey.VK_F13:
                    return (byte) DIK.DIK_F13 /* (NEC PC98) */;
                case VirtualKey.VK_F14:
                    return (byte) DIK.DIK_F14 /* (NEC PC98) */;
                case VirtualKey.VK_F15:
                    return (byte) DIK.DIK_F15 /* (NEC PC98) */;
                case VirtualKey.VK_RCONTROL:
                    return (byte) DIK.DIK_RCONTROL;
                case VirtualKey.VK_DIVIDE:
                    return (byte) DIK.DIK_DIVIDE /* / on numeric keypad */;
                case VirtualKey.VK_RMENU:
                    return (byte) DIK.DIK_RMENU /* right Alt */;
                case VirtualKey.VK_HOME:
                    return (byte) DIK.DIK_HOME /* Home on arrow keypad */;
                case VirtualKey.VK_UP:
                    return (byte) DIK.DIK_UP /* UpArrow on arrow keypad */;
                case VirtualKey.VK_PRIOR:
                    return (byte) DIK.DIK_PRIOR /* PgUp on arrow keypad */;
                case VirtualKey.VK_LEFT:
                    return (byte) DIK.DIK_LEFT /* LeftArrow on arrow keypad */;
                case VirtualKey.VK_RIGHT:
                    return (byte) DIK.DIK_RIGHT /* RightArrow on arrow keypad */;
                case VirtualKey.VK_END:
                    return (byte) DIK.DIK_END /* End on arrow keypad */;
                case VirtualKey.VK_DOWN:
                    return (byte) DIK.DIK_DOWN /* DownArrow on arrow keypad */;
                case VirtualKey.VK_NEXT:
                    return (byte) DIK.DIK_NEXT /* PgDn on arrow keypad */;
                case VirtualKey.VK_INSERT:
                    return (byte) DIK.DIK_INSERT /* Insert on arrow keypad */;
                case VirtualKey.VK_DELETE:
                    return (byte) DIK.DIK_DELETE /* Delete on arrow keypad */;
                case VirtualKey.VK_LWIN:
                    return (byte) DIK.DIK_LWIN /* Left Windows key */;
                case VirtualKey.VK_RWIN:
                    return (byte) DIK.DIK_RWIN /* Right Windows key */;
                case VirtualKey.VK_APPS:
                    return (byte) DIK.DIK_APPS /* AppMenu key */;
                case VirtualKey.VK_PAUSE:
                    return (byte) DIK.DIK_PAUSE;
                case VirtualKey.VK_SNAPSHOT:
                    return (byte) DIK.DIK_SYSRQ; // (print screen);
                default:
                    return 0;
            }
        }

        const uint MAPVK_VK_TO_VSC = 0x00;

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        private const uint
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOACTIVATE = 0x0010;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private void UpdateMousePos(int xAbs, int yAbs, int wheelDelta)
        {
            mMouseMoveHandler?.Invoke(xAbs, yAbs, wheelDelta);
        }


        private MouseMoveHandler mMouseMoveHandler;
        private WindowMsgFilter mWindowMsgFilter;

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, ulong wParam, long lParam);

        [DllImport("user32.dll")]
        static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle,
            bool bMenu, WindowStylesEx dwExStyle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("gdi32.dll")]
        private static extern IntPtr GetStockObject(StockObjects fnObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        private static int IDC_ARROW = 32512;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateWindowEx(
            WindowStylesEx dwExStyle,
            string lpClassName,
            string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);
    }
}