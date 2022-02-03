using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Platform
{
    public delegate bool WindowMsgFilter(uint msg, nuint wparam, nint lparam);

    public class MainWindow : IMainWindow
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private WindowConfig _config;

        private GCHandle _gcHandle;

        public IntPtr NativeHandle => _windowHandle;

        // Used to determine whether a MouseEnter event should be emitted when a mouse event is received
        private bool _mouseFocus;

        private int _mouseCaptureDepth;

        public MainWindow(WindowConfig config, IFileSystem fs)
        {
            _fs = fs;
            _config = config.Copy();
            if (_config.Width < _config.MinWidth)
            {
                _config.Width = _config.MinWidth;
            }

            if (_config.Height < _config.MinHeight)
            {
                _config.Height = _config.MinHeight;
            }

            _instanceHandle = GetModuleHandle(null);

            // This GC handle is attached to windows created with CreateWindow
            _gcHandle = GCHandle.Alloc(this);

            RegisterWndClass();
            CreateHwnd();
            UpdateUiCanvasSize();
        }

        public void Dispose()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                DestroyWindow(_windowHandle);
            }

            _gcHandle.Free();

            UnregisterWndClass();
        }

        public IntPtr WindowHandle => _windowHandle;

        public IntPtr InstanceHandle => _instanceHandle;

        public Size Size => new Size(_width, _height);

        public WindowConfig WindowConfig
        {
            get => _config;
            set
            {
                var changingFullscreen = _config.Windowed != value.Windowed;
                _config = value.Copy();

                if (changingFullscreen)
                {
                    CreateWindowRectAndStyles(out var windowRect, out var style, out var styleEx);

                    var currentStyles = unchecked((WindowStyles) (long) GetWindowLongPtr(_windowHandle, GWL_STYLE));
                    currentStyles &= ~(WindowStyles.WS_OVERLAPPEDWINDOW | WindowStyles.WS_POPUP |
                                       WindowStyles.WS_MAXIMIZE);
                    currentStyles |= style;

                    SetWindowLongPtr(_windowHandle, GWL_STYLE, (IntPtr) currentStyles);
                    SetWindowLongPtr(_windowHandle, GWL_EXSTYLE, (IntPtr) styleEx);
                    
                    var wasMaximized = _config.Maximized;

                    // Just update window styles first, this will clear the maximized flag and send a WM_SIZE though!
                    SetWindowPos(
                        _windowHandle,
                        IntPtr.Zero,
                        0,
                        0,
                        0,
                        0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED
                    );

                    if (!_config.Windowed || !wasMaximized)
                    {
                        // When going full-screen, this positions the window at full screen size. This uses
                        // screen coordinates, not "workspace"
                        SetWindowPos(
                            _windowHandle,
                            IntPtr.Zero,
                            windowRect.Left,
                            windowRect.Top,
                            windowRect.Width,
                            windowRect.Height,
                            SWP_NOZORDER
                        );
                    }
                    else
                    {
                        var placement = WINDOWPLACEMENT.Default;
                        GetWindowPlacement(_windowHandle, ref placement);
                        placement.ShowCmd = SW_MAXIMIZE;
                        SetWindowPlacement(_windowHandle, ref placement);
                    }
                }
            }
        }

        public bool IsInForeground { get; set; } = true;

        public event Action<bool> IsInForegroundChanged;

        private bool unsetClip = false;

        public event Action<WindowEvent> OnInput;

        public event Action<Size> Resized;

        public event Action Closed;

        public SizeF UiCanvasSize { get; private set; }

        private Size _uiCanvasTargetSize = new(1024, 768);

        public Size UiCanvasTargetSize
        {
            get => _uiCanvasTargetSize;
            set
            {
                if (value.Width <= 0 || value.Height <= 0)
                {
                    throw new ArgumentException("Cannot set target UI size to 0");
                }

                _uiCanvasTargetSize = value;
                UpdateUiCanvasSize();
            }
        }

        public event Action UiCanvasSizeChanged;

        public float UiScale { get; private set; }

        public bool IsCursorVisible
        {
            set
            {
                if (value)
                {
                    if (_currentCursor != IntPtr.Zero)
                    {
                        Cursor.SetCursor(_windowHandle, _currentCursor);
                    }
                }
                else
                {
                    Cursor.HideCursor(_windowHandle);
                }
            }
        }

        /// <summary>
        /// Changes the currently used cursor to the given surface.
        /// </summary>
        public void SetCursor(int hotspotX, int hotspotY, string imagePath)
        {
            if (!_cursorCache.TryGetValue(imagePath, out var cursor))
            {
                var textureData = _fs.ReadBinaryFile(imagePath);
                try
                {
                    cursor = IO.Images.ImageIO.LoadImageToCursor(textureData, hotspotX, hotspotY);
                }
                catch (Exception e)
                {
                    cursor = IntPtr.Zero;
                    Logger.Error("Failed to load cursor {0}: {1}", imagePath, e);
                }

                _cursorCache[imagePath] = cursor;
            }

            Cursor.SetCursor(_windowHandle, cursor);
            _currentCursor = cursor;
        }

        private void UpdateUiCanvasSize()
        {
            // Attempt to fit 1024x768 onto the backbuffer
            var horScale = MathF.Max(1, _width / (float)_uiCanvasTargetSize.Width);
            var verScale = MathF.Max(1, _height / (float)_uiCanvasTargetSize.Height);
            UiScale = Math.Min(horScale, verScale);

            UiCanvasSize = new SizeF(MathF.Floor(_width / UiScale), MathF.Floor(_height / UiScale));
            if (UiCanvasSize.IsEmpty)
            {
                Debugger.Break();
            }
            UiCanvasSizeChanged?.Invoke();
        }

        // Locks the mouse cursor to this window
        // if we're in the foreground
        public void ConfineCursor(int x, int y, int w, int h)
        {
            bool isForeground = GetForegroundWindow() == _windowHandle;

            if (isForeground)
            {
                RECT rect = new RECT { X = x, Y = y, Right = x + w, Bottom = y + h };

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

        // Sets a filter that receives a chance at intercepting all window messages
        public void SetWindowMsgFilter(WindowMsgFilter filter)
        {
            mWindowMsgFilter = filter;
        }

        private readonly IFileSystem _fs;
        private readonly IntPtr _instanceHandle;
        private IntPtr _windowHandle;
        private int _width;
        private int _height;

        // Caches for cursors
        private readonly Dictionary<string, IntPtr> _cursorCache = new();
        private IntPtr _currentCursor = IntPtr.Zero;

        // The window class name used for RegisterClass and CreateWindow.
        private const string WindowClassName = "OpenTempleMainWnd";
        private const string WindowTitle = "OpenTemple";

        private unsafe void RegisterWndClass()
        {
            var wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                // Make sure to use the delegate object here, otherwise .NET might create a temporary one,
                // but we need to ensure the native twin of this delegate will live as long as the App.
                lpfnWndProc =
                    (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, nuint, nint, IntPtr>)&WndProcTrampoline,
                hInstance = _instanceHandle,
                hIcon = LoadIcon(_instanceHandle, "icon"),
                hCursor = LoadCursor(IntPtr.Zero, IDC_ARROW),
                hbrBackground = GetStockObject(StockObjects.BLACK_BRUSH),
                lpszClassName = WindowClassName,
                cbWndExtra = Marshal.SizeOf<GCHandle>()
            };

            if (RegisterClassEx(ref wndClass) == 0)
            {
                throw new Exception("Unable to register window class: " + Marshal.GetLastWin32Error());
            }
        }

        private void UnregisterWndClass()
        {
            if (!UnregisterClass(WindowClassName, _instanceHandle))
            {
                Logger.Error("Unable to unregister window class: " + Marshal.GetLastWin32Error());
            }
        }

        private void CreateHwnd()
        {
            CreateWindowRectAndStyles(out var windowRect, out var style, out var styleEx);

            style |= WindowStyles.WS_VISIBLE;

            // Show initially maximized in window mode, if the game was previously maximized
            if (_config.Windowed && _config.Maximized)
            {
                style |= WindowStyles.WS_MAXIMIZE;
            }

            var width = windowRect.Width;
            var height = windowRect.Height;
            Logger.Info("Creating window with dimensions {0}x{1}", width, height);
            _windowHandle = CreateWindowEx(
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
                _instanceHandle,
                IntPtr.Zero);

            if (_windowHandle == IntPtr.Zero)
            {
                throw new Exception("Unable to create main window: " + Marshal.GetLastWin32Error());
            }
            
            // Retrieve the *actual* size, because we don't seem to get a WM_SIZE message for this,
            // especially in case we use WS_MAXIMIZE, the size can be wrong
            if (GetClientRect(_windowHandle, out var clientRect))
            {
                // The returned size should never be zero, unless the window was forced by some hook to be minimized
                if (clientRect.Width > 0 && clientRect.Height > 0)
                {
                    _width = clientRect.Width;
                    _height = clientRect.Height;
                    Logger.Info("Actual window dimensions {0}x{1}", width, height);
                }
            }

            // Store our this pointer in the window
            SetWindowLongPtr(_windowHandle, 0, GCHandle.ToIntPtr(_gcHandle));

            // Enable notifications for WM_MOUSELEAVE
            TrackWindowLeave(_windowHandle);
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
                _width = screenWidth;
                _height = screenHeight;
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

                style = WindowStyles.WS_OVERLAPPEDWINDOW;

                AdjustWindowRectEx(ref windowRect, style, false, styleEx);
                int extraWidth = (windowRect.Right - windowRect.Left) - _config.Width;
                int extraHeight = (windowRect.Bottom - windowRect.Top) - _config.Height;
                windowRect.Left = (screenWidth - _config.Width) / 2 - (extraWidth / 2);
                windowRect.Top = (screenHeight - _config.Height) / 2 - (extraHeight / 2);
                windowRect.Right = windowRect.Left + _config.Width + extraWidth;
                windowRect.Bottom = windowRect.Top + _config.Height + extraHeight;

                _width = _config.Width;
                _height = _config.Height;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static IntPtr WndProcTrampoline(IntPtr hWnd, uint msg, nuint wparam, nint lparam)
        {
            // Retrieve our this pointer from the wnd
            var handlePtr = GetWindowLongPtr(hWnd, 0);
            if (handlePtr != IntPtr.Zero)
            {
                var handle = GCHandle.FromIntPtr(handlePtr);

                if (handle.IsAllocated)
                {
                    var mainWindow = (MainWindow)handle.Target;
                    return mainWindow.WndProc(hWnd, msg, wparam, lparam);
                }
            }

            return DefWindowProc(hWnd, msg, wparam, lparam);
        }

        [TempleDllLocation(0x10D25CEC), TempleDllLocation(0x10D25CF0)]
        private PointF mousePos;

        private IntPtr WndProc(IntPtr hWnd, uint msg, nuint wParam, nint lParam)
        {
            if (hWnd != _windowHandle)
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
                    if ((IntPtr)(nint)wParam == _windowHandle && !IsDebuggerPresent())
                    {
                        SetWindowPos(_windowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_KILLFOCUS:
                    // Make our window topmost unless a debugger is attached
                    if ((IntPtr)(nint)wParam == _windowHandle && !IsDebuggerPresent())
                    {
                        SetWindowPos(_windowHandle, HWND_NOTOPMOST, 0, 0, 0, 0,
                            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                    }

                    break;
                case WM_SYSCOMMAND:
                    if (wParam == SC_KEYMENU || wParam == SC_SCREENSAVE || wParam == SC_MONITORPOWER)
                    {
                        return IntPtr.Zero;
                    }

                    break;
                case WM_GETMINMAXINFO:
                    unsafe
                    {
                        var minMaxInfo = (MinMaxInfo*)lParam;
                        minMaxInfo->ptMinTrackSize.X = _config.MinWidth;
                        minMaxInfo->ptMinTrackSize.Y = _config.MinHeight;
                    }

                    break;
                case WM_SIZE:
                    // Ignore resizes to 0 (i.e. due to being minimized)
                    if (WindowsMessageUtils.GetWidthParam(lParam) == 0
                        || WindowsMessageUtils.GetHeightParam(lParam) == 0)
                    {
                        break;
                    }
                    
                    // Update width/height with window client size
                    _width = WindowsMessageUtils.GetWidthParam(lParam);
                    _height = WindowsMessageUtils.GetHeightParam(lParam);
                    // If the window was maximized by the user, store that in the config
                    if (_config.Windowed)
                    {
                        if (wParam == SIZE_MAXIMIZED)
                        {
                            _config.Maximized = true;
                        }
                        else if (wParam == SIZE_RESTORED)
                        {
                            _config.Maximized = false;
                        }

                        Globals.Config.Window.Maximized = _config.Maximized;
                        Globals.ConfigManager.Save();
                    }
                    Resized?.Invoke(Size);
                    UpdateUiCanvasSize();
                    break;
                case WM_ACTIVATEAPP:
                    IsInForeground = wParam == 1;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                    break;
                case WM_ERASEBKGND:
                    return IntPtr.Zero;
                case WM_CLOSE:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs(1)));
                    Closed?.Invoke();
                    break;
                case WM_QUIT:
                    Tig.MessageQueue.Enqueue(new Message(new ExitMessageArgs((int)wParam)));
                    Closed?.Invoke();
                    break;
                case WM_LBUTTONDOWN:
                    HandleMouseButtonEvent(WindowEventType.MouseDown, MouseButton.LEFT, hWnd, wParam, lParam);
                    break;
                case WM_LBUTTONUP:
                    HandleMouseButtonEvent(WindowEventType.MouseUp, MouseButton.LEFT, hWnd, wParam, lParam);
                    break;
                case WM_RBUTTONDOWN:
                    HandleMouseButtonEvent(WindowEventType.MouseDown, MouseButton.RIGHT, hWnd, wParam, lParam);
                    break;
                case WM_RBUTTONUP:
                    HandleMouseButtonEvent(WindowEventType.MouseUp, MouseButton.RIGHT, hWnd, wParam, lParam);
                    break;
                case WM_MBUTTONDOWN:
                    HandleMouseButtonEvent(WindowEventType.MouseDown, MouseButton.MIDDLE, hWnd, wParam, lParam);
                    break;
                case WM_MBUTTONUP:
                    HandleMouseButtonEvent(WindowEventType.MouseUp, MouseButton.MIDDLE, hWnd, wParam, lParam);
                    break;
                case WM_XBUTTONDOWN:
                    switch (WindowsMessageUtils.GetXButton(wParam))
                    {
                        case 0:
                            HandleMouseButtonEvent(WindowEventType.MouseDown, MouseButton.EXTRA1, hWnd, wParam, lParam);
                            break;
                        case 1:
                            HandleMouseButtonEvent(WindowEventType.MouseDown, MouseButton.EXTRA2, hWnd, wParam, lParam);
                            break;
                    }

                    break;
                case WM_XBUTTONUP:
                    switch (WindowsMessageUtils.GetXButton(wParam))
                    {
                        case 0:
                            HandleMouseButtonEvent(WindowEventType.MouseUp, MouseButton.EXTRA1, hWnd, wParam, lParam);
                            break;
                        case 1:
                            HandleMouseButtonEvent(WindowEventType.MouseUp, MouseButton.EXTRA2, hWnd, wParam, lParam);
                            break;
                    }

                    break;
                case WM_SYSKEYDOWN:
                case WM_KEYDOWN:
                {
                    // Handle Alt+Enter here
                    if (IsAltEnter(wParam, lParam))
                    {
                        // Ignore repeats
                        if ((GetKeyMessageFlags(lParam) & KeyMessageFlag.KF_REPEAT) == 0)
                        {
                            var config = _config.Copy();
                            config.Windowed = !config.Windowed;
                            WindowConfig = config;
                        }
                        return IntPtr.Zero;
                    }
                    
                    var key = (DIK)ToDirectInputKey((VirtualKey)wParam);
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
                    if (IsAltEnter(wParam, lParam))
                    {
                        // Since we handle the key-down of this, we should not pass through the key-up
                        return IntPtr.Zero;
                    }

                    var key = (DIK)ToDirectInputKey((VirtualKey)wParam);
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
                    Tig.MessageQueue.Enqueue(new Message(new MessageCharArgs((char)wParam)));
                    break;
                case WM_MOUSEWHEEL:
                    HandleMouseWheelEvent(hWnd, wParam, lParam);
                    break;
                case WM_MOUSEMOVE:
                    HandleMouseMoveEvent(WindowEventType.MouseMove, hWnd, wParam, lParam);
                    break;
                case WM_MOUSELEAVE:
                    HandleMouseFocusEvent(false);
                    break;
            }

            // Previously, ToEE called a global window proc here but it did nothing useful.
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private static bool IsAltEnter(nuint wParam, nint lParam)
        {
            var flags = GetKeyMessageFlags(lParam);
            return (VirtualKey) wParam == VirtualKey.VK_RETURN
                   // And only if alt is being held
                   && (flags & KeyMessageFlag.KF_ALTDOWN) != 0;
        }

        private static KeyMessageFlag GetKeyMessageFlags(nint lParam)
        {
            return (KeyMessageFlag) ((lParam >> 16) & 0xFFFF);
        }

        private void HandleMouseWheelEvent(IntPtr hWnd, ulong wParam, long lParam)
        {
            HandleMouseFocusEvent(true);
            // Note that for mouse-wheel events, the mouse position is relative to the screen,
            // not the client area
            var windowPos = TranslateScreenToClient(
                hWnd,
                WindowsMessageUtils.GetXParam(lParam),
                WindowsMessageUtils.GetYParam(lParam)
            );
            var uiPos = TranslateToUiCanvas(windowPos);
            OnInput?.Invoke(new MouseWheelWindowEvent(
                WindowEventType.Wheel,
                this,
                windowPos,
                uiPos,
                // 120 units is the "default" of one scroll wheel notch
                WindowsMessageUtils.GetWheelDelta(wParam) / 120.0f
            ));
        }

        private void HandleMouseMoveEvent(WindowEventType type, IntPtr hWnd, ulong wParam, long lParam)
        {
            HandleMouseFocusEvent(true);
            var windowPos = new Point(
                WindowsMessageUtils.GetXParam(lParam),
                WindowsMessageUtils.GetYParam(lParam)
            );
            var uiPos = TranslateToUiCanvas(windowPos);
            OnInput?.Invoke(new MouseWindowEvent(
                type,
                this,
                windowPos,
                uiPos
            ));
        }

        private void HandleMouseButtonEvent(WindowEventType type, MouseButton button, IntPtr hWnd, ulong wParam,
            long lParam)
        {
            // When a mouse button is pressed, capture the mouse in order to guarantee receiving a mouse up event
            if (type == WindowEventType.MouseDown && _mouseCaptureDepth <= 0)
            {
                SetCapture(hWnd);
                _mouseCaptureDepth++;
            }
            else if (type == WindowEventType.MouseUp)
            {
                if (--_mouseCaptureDepth <= 0)
                {
                    ReleaseCapture();
                }
            }

            HandleMouseFocusEvent(true);
            var windowPos = new Point(
                WindowsMessageUtils.GetXParam(lParam),
                WindowsMessageUtils.GetYParam(lParam)
            );
            var uiPos = TranslateToUiCanvas(windowPos);
            OnInput?.Invoke(new MouseWindowEvent(
                type,
                this,
                windowPos,
                uiPos
            )
            {
                Button = button
            });
        }

        private void HandleMouseFocusEvent(bool focus)
        {
            if (focus == _mouseFocus)
            {
                return;
            }

            _mouseFocus = focus;

            var type = focus ? WindowEventType.MouseEnter : WindowEventType.MouseLeave;
            OnInput?.Invoke(new MouseFocusEvent(type, this));

            // Ensure we get a mouse-leave notification
            TrackWindowLeave(_windowHandle);
        }

        /// <summary>
        /// Translate from screen coordinates to the client coordinates of the window.
        /// </summary>
        private Point TranslateScreenToClient(IntPtr hWnd, int x, int y)
        {
            POINT p;
            p.X = x;
            p.Y = y;
            ScreenToClient(hWnd, ref p);
            return new Point(p.X, p.Y);
        }

        private void TrackWindowLeave(IntPtr hWnd)
        {
            var tme = new TRACKMOUSEEVENT(TMEFlags.TME_LEAVE, hWnd, 0);
            if (!TrackMouseEvent(ref tme))
            {
                var err = Marshal.GetLastWin32Error();
                Console.WriteLine(err);
            }
        }

        private PointF TranslateToUiCanvas(int x, int y) => new(x / UiScale, y / UiScale);

        private PointF TranslateToUiCanvas(Point p) => TranslateToUiCanvas(p.X, p.Y);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern bool IsDebuggerPresent();

        private const uint WM_ACTIVATEAPP = 0x001C;
        private const uint WM_SETFOCUS = 0x0007;
        private const uint WM_KILLFOCUS = 0x0008;
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint WM_SIZE = 0x0005;
        private const uint WM_ERASEBKGND = 0x0014;
        private const uint WM_GETMINMAXINFO = 0x0024;

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
        private const uint WM_XBUTTONDOWN = 0x020B;
        private const uint WM_XBUTTONUP = 0x020C;
        private const uint WM_MOUSELEAVE = 0x02A3;
        private const uint WM_CAPTURECHANGED = 0x0215;

        private const uint SC_KEYMENU = 0xF100;
        private const uint SC_SCREENSAVE = 0xF140;
        private const uint SC_MONITORPOWER = 0xF170;

        private const uint SIZE_RESTORED = 0;
        private const uint SIZE_MAXIMIZED = 2;

        private uint ToDirectInputKey(VirtualKey vk)
        {
            // Special case for keys that can be on the numpad
            switch (vk)
            {
                case VirtualKey.VK_HOME:
                    return (uint)DIK.DIK_HOME; /* Home on arrow keypad */
                case VirtualKey.VK_END:
                    return (uint)DIK.DIK_END; /* End on arrow keypad */
                case VirtualKey.VK_PRIOR:
                    return (uint)DIK.DIK_PRIOR; /* PgUp on arrow keypad */
                case VirtualKey.VK_NEXT:
                    return (uint)DIK.DIK_NEXT; /* PgDn on arrow keypad */
                case VirtualKey.VK_VOLUME_DOWN:
                    return (uint)DIK.DIK_VOLUMEDOWN;
                case VirtualKey.VK_VOLUME_UP:
                    return (uint)DIK.DIK_VOLUMEUP;
                case VirtualKey.VK_VOLUME_MUTE:
                    return (uint)DIK.DIK_MUTE;
            }

            // This seems to map using scan codes, which
            // actually look like original US keyboard ones
            var mapped = MapVirtualKey((uint)vk, MAPVK_VK_TO_VSC);
            if (mapped != 0)
            {
                return mapped;
            }

            switch (vk)
            {
                case VirtualKey.VK_ESCAPE:
                    return (byte)DIK.DIK_ESCAPE;
                case VirtualKey.VK_1:
                    return (byte)DIK.DIK_1;
                case VirtualKey.VK_2:
                    return (byte)DIK.DIK_2;
                case VirtualKey.VK_3:
                    return (byte)DIK.DIK_3;
                case VirtualKey.VK_4:
                    return (byte)DIK.DIK_4;
                case VirtualKey.VK_5:
                    return (byte)DIK.DIK_5;
                case VirtualKey.VK_6:
                    return (byte)DIK.DIK_6;
                case VirtualKey.VK_7:
                    return (byte)DIK.DIK_7;
                case VirtualKey.VK_8:
                    return (byte)DIK.DIK_8;
                case VirtualKey.VK_9:
                    return (byte)DIK.DIK_9;
                case VirtualKey.VK_0:
                    return (byte)DIK.DIK_0;
                case VirtualKey.VK_OEM_MINUS:
                    return (byte)DIK.DIK_MINUS /* - on main keyboard */;
                case VirtualKey.VK_OEM_PLUS:
                    return (byte)DIK.DIK_EQUALS;
                case VirtualKey.VK_BACK:
                    return (byte)DIK.DIK_BACK /* backspace */;
                case VirtualKey.VK_TAB:
                    return (byte)DIK.DIK_TAB;
                case VirtualKey.VK_Q:
                    return (byte)DIK.DIK_Q;
                case VirtualKey.VK_W:
                    return (byte)DIK.DIK_W;
                case VirtualKey.VK_E:
                    return (byte)DIK.DIK_E;
                case VirtualKey.VK_R:
                    return (byte)DIK.DIK_R;
                case VirtualKey.VK_T:
                    return (byte)DIK.DIK_T;
                case VirtualKey.VK_Y:
                    return (byte)DIK.DIK_Y;
                case VirtualKey.VK_U:
                    return (byte)DIK.DIK_U;
                case VirtualKey.VK_I:
                    return (byte)DIK.DIK_I;
                case VirtualKey.VK_O:
                    return (byte)DIK.DIK_O;
                case VirtualKey.VK_P:
                    return (byte)DIK.DIK_P;
                case VirtualKey.VK_OEM_4:
                    return (byte)DIK.DIK_LBRACKET;
                case VirtualKey.VK_OEM_6:
                    return (byte)DIK.DIK_RBRACKET;
                case VirtualKey.VK_RETURN:
                    return (byte)DIK.DIK_RETURN /* Enter on main keyboard */;
                case VirtualKey.VK_LCONTROL:
                    return (byte)DIK.DIK_LCONTROL;
                case VirtualKey.VK_A:
                    return (byte)DIK.DIK_A;
                case VirtualKey.VK_S:
                    return (byte)DIK.DIK_S;
                case VirtualKey.VK_D:
                    return (byte)DIK.DIK_D;
                case VirtualKey.VK_F:
                    return (byte)DIK.DIK_F;
                case VirtualKey.VK_G:
                    return (byte)DIK.DIK_G;
                case VirtualKey.VK_H:
                    return (byte)DIK.DIK_H;
                case VirtualKey.VK_J:
                    return (byte)DIK.DIK_J;
                case VirtualKey.VK_K:
                    return (byte)DIK.DIK_K;
                case VirtualKey.VK_L:
                    return (byte)DIK.DIK_L;
                case VirtualKey.VK_OEM_1:
                    return (byte)DIK.DIK_SEMICOLON;
                case VirtualKey.VK_OEM_7:
                    return (byte)DIK.DIK_APOSTROPHE;
                case VirtualKey.VK_OEM_3:
                    return (byte)DIK.DIK_GRAVE /* accent grave */;
                case VirtualKey.VK_LSHIFT:
                    return (byte)DIK.DIK_LSHIFT;
                case VirtualKey.VK_OEM_5:
                    return (byte)DIK.DIK_BACKSLASH;
                case VirtualKey.VK_Z:
                    return (byte)DIK.DIK_Z;
                case VirtualKey.VK_X:
                    return (byte)DIK.DIK_X;
                case VirtualKey.VK_C:
                    return (byte)DIK.DIK_C;
                case VirtualKey.VK_V:
                    return (byte)DIK.DIK_V;
                case VirtualKey.VK_B:
                    return (byte)DIK.DIK_B;
                case VirtualKey.VK_N:
                    return (byte)DIK.DIK_N;
                case VirtualKey.VK_M:
                    return (byte)DIK.DIK_M;
                case VirtualKey.VK_OEM_COMMA:
                    return (byte)DIK.DIK_COMMA;
                case VirtualKey.VK_OEM_PERIOD:
                    return (byte)DIK.DIK_PERIOD /* . on main keyboard */;
                case VirtualKey.VK_OEM_2:
                    return (byte)DIK.DIK_SLASH /* / on main keyboard */;
                case VirtualKey.VK_RSHIFT:
                    return (byte)DIK.DIK_RSHIFT;
                case VirtualKey.VK_MULTIPLY:
                    return (byte)DIK.DIK_MULTIPLY /* * on numeric keypad */;
                case VirtualKey.VK_LMENU:
                    return (byte)DIK.DIK_LMENU /* left Alt */;
                case VirtualKey.VK_SPACE:
                    return (byte)DIK.DIK_SPACE;
                case VirtualKey.VK_CAPITAL:
                    return (byte)DIK.DIK_CAPITAL;
                case VirtualKey.VK_F1:
                    return (byte)DIK.DIK_F1;
                case VirtualKey.VK_F2:
                    return (byte)DIK.DIK_F2;
                case VirtualKey.VK_F3:
                    return (byte)DIK.DIK_F3;
                case VirtualKey.VK_F4:
                    return (byte)DIK.DIK_F4;
                case VirtualKey.VK_F5:
                    return (byte)DIK.DIK_F5;
                case VirtualKey.VK_F6:
                    return (byte)DIK.DIK_F6;
                case VirtualKey.VK_F7:
                    return (byte)DIK.DIK_F7;
                case VirtualKey.VK_F8:
                    return (byte)DIK.DIK_F8;
                case VirtualKey.VK_F9:
                    return (byte)DIK.DIK_F9;
                case VirtualKey.VK_F10:
                    return (byte)DIK.DIK_F10;
                case VirtualKey.VK_NUMLOCK:
                    return (byte)DIK.DIK_NUMLOCK;
                case VirtualKey.VK_SCROLL:
                    return (byte)DIK.DIK_SCROLL /* Scroll Lock */;
                case VirtualKey.VK_NUMPAD7:
                    return (byte)DIK.DIK_NUMPAD7;
                case VirtualKey.VK_NUMPAD8:
                    return (byte)DIK.DIK_NUMPAD8;
                case VirtualKey.VK_NUMPAD9:
                    return (byte)DIK.DIK_NUMPAD9;
                case VirtualKey.VK_SUBTRACT:
                    return (byte)DIK.DIK_SUBTRACT /* - on numeric keypad */;
                case VirtualKey.VK_NUMPAD4:
                    return (byte)DIK.DIK_NUMPAD4;
                case VirtualKey.VK_NUMPAD5:
                    return (byte)DIK.DIK_NUMPAD5;
                case VirtualKey.VK_NUMPAD6:
                    return (byte)DIK.DIK_NUMPAD6;
                case VirtualKey.VK_ADD:
                    return (byte)DIK.DIK_ADD /* + on numeric keypad */;
                case VirtualKey.VK_NUMPAD1:
                    return (byte)DIK.DIK_NUMPAD1;
                case VirtualKey.VK_NUMPAD2:
                    return (byte)DIK.DIK_NUMPAD2;
                case VirtualKey.VK_NUMPAD3:
                    return (byte)DIK.DIK_NUMPAD3;
                case VirtualKey.VK_NUMPAD0:
                    return (byte)DIK.DIK_NUMPAD0;
                case VirtualKey.VK_DECIMAL:
                    return (byte)DIK.DIK_DECIMAL /* . on numeric keypad */;
                case VirtualKey.VK_F11:
                    return (byte)DIK.DIK_F11;
                case VirtualKey.VK_F12:
                    return (byte)DIK.DIK_F12;
                case VirtualKey.VK_F13:
                    return (byte)DIK.DIK_F13 /* (NEC PC98) */;
                case VirtualKey.VK_F14:
                    return (byte)DIK.DIK_F14 /* (NEC PC98) */;
                case VirtualKey.VK_F15:
                    return (byte)DIK.DIK_F15 /* (NEC PC98) */;
                case VirtualKey.VK_RCONTROL:
                    return (byte)DIK.DIK_RCONTROL;
                case VirtualKey.VK_DIVIDE:
                    return (byte)DIK.DIK_DIVIDE /* / on numeric keypad */;
                case VirtualKey.VK_RMENU:
                    return (byte)DIK.DIK_RMENU /* right Alt */;
                case VirtualKey.VK_HOME:
                    return (byte)DIK.DIK_HOME /* Home on arrow keypad */;
                case VirtualKey.VK_UP:
                    return (byte)DIK.DIK_UP /* UpArrow on arrow keypad */;
                case VirtualKey.VK_PRIOR:
                    return (byte)DIK.DIK_PRIOR /* PgUp on arrow keypad */;
                case VirtualKey.VK_LEFT:
                    return (byte)DIK.DIK_LEFT /* LeftArrow on arrow keypad */;
                case VirtualKey.VK_RIGHT:
                    return (byte)DIK.DIK_RIGHT /* RightArrow on arrow keypad */;
                case VirtualKey.VK_END:
                    return (byte)DIK.DIK_END /* End on arrow keypad */;
                case VirtualKey.VK_DOWN:
                    return (byte)DIK.DIK_DOWN /* DownArrow on arrow keypad */;
                case VirtualKey.VK_NEXT:
                    return (byte)DIK.DIK_NEXT /* PgDn on arrow keypad */;
                case VirtualKey.VK_INSERT:
                    return (byte)DIK.DIK_INSERT /* Insert on arrow keypad */;
                case VirtualKey.VK_DELETE:
                    return (byte)DIK.DIK_DELETE /* Delete on arrow keypad */;
                case VirtualKey.VK_LWIN:
                    return (byte)DIK.DIK_LWIN /* Left Windows key */;
                case VirtualKey.VK_RWIN:
                    return (byte)DIK.DIK_RWIN /* Right Windows key */;
                case VirtualKey.VK_APPS:
                    return (byte)DIK.DIK_APPS /* AppMenu key */;
                case VirtualKey.VK_PAUSE:
                    return (byte)DIK.DIK_PAUSE;
                case VirtualKey.VK_SNAPSHOT:
                    return (byte)DIK.DIK_SYSRQ; // (print screen);
                default:
                    return 0;
            }
        }

        private const uint MAPVK_VK_TO_VSC = 0x00;

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);
        
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        
        /// <summary>
        /// Contains information about the placement of a window on the screen.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            /// <summary>
            /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
            /// <para>
            /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
            /// </para>
            /// </summary>
            public int Length;

            /// <summary>
            /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The current show state of the window.
            /// </summary>
            public uint ShowCmd;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is minimized.
            /// </summary>
            public POINT MinPosition;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is maximized.
            /// </summary>
            public POINT MaxPosition;

            /// <summary>
            /// The window's coordinates when the window is in the restored position.
            /// </summary>
            public RECT NormalPosition;

            /// <summary>
            /// Gets the default (empty) value.
            /// </summary>
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf( result );
                    return result;
                }
            }    
        }
        
        /// <summary>
        /// SetWindowPos Flags
        /// </summary>
        private const uint
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x004,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        private WindowMsgFilter mWindowMsgFilter;

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, nuint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyles dwStyle,
            bool bMenu, WindowStylesEx dwExStyle);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U2)]
        private static extern short RegisterClassEx([In] ref WNDCLASSEX lpwcx);

        [DllImport("gdi32.dll")]
        private static extern IntPtr GetStockObject(StockObjects fnObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        private static int IDC_ARROW = 32512;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        private const int GWL_STYLE = -16;

        private const int GWL_EXSTYLE = -20;

        // Check https://stackoverflow.com/questions/54833997/i-keep-getting-unable-to-find-an-entry-point-named-getwindowlongptra-in-dll
        // for why this is required
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        
        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, nIndex);
            }
            else
            {
                return GetWindowLongPtr32(hWnd, nIndex);
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }
            else
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
        }
        
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public const uint SW_RESTORE = 9;
        public const uint SW_MAXIMIZE = 3;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, string lpIconName);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetric smIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetCapture(IntPtr hWnd);

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

        [Flags]
        private enum KeyMessageFlag
        {
            /// <summary>
            /// Manipulates the extended key flag.
            /// </summary>
            KF_EXTENDED =  0x0100,
            /// <summary>
            /// Manipulates the dialog mode flag, which indicates whether a dialog box is active.
            /// </summary>
            KF_DLGMODE =  0x0800,
            /// <summary>
            /// Manipulates the menu mode flag, which indicates whether a menu is active.
            /// </summary>
            KF_MENUMODE =  0x1000,
            /// <summary>
            /// Manipulates the ALT key flag, which indicated if the ALT key is pressed.
            /// </summary>
            KF_ALTDOWN =  0x2000,
            /// <summary>
            /// Manipulates the repeat count.
            /// </summary>
            KF_REPEAT =  0x4000,
            /// <summary>
            /// Manipulates the transition state flag.
            /// </summary>
            KF_UP =  0x8000
        }
    }
}