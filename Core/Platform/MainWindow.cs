using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTemple.Core.Config;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using static SDL2.SDL;

namespace OpenTemple.Core.Platform;

public delegate bool SDLEventFilter(ref SDL_Event e);

public class MainWindow : IMainWindow
{
    // The window class name used for RegisterClass and CreateWindow.
    private const string WindowClassName = "OpenTempleMainWnd";
    private const string WindowTitle = "OpenTemple";

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private WindowConfig _config;

    public IntPtr NativeHandle => _windowHandle;

    private SDLEventFilter _eventFilter;

    /// <summary>
    /// SDL2 Window Pointer.
    /// </summary>
    private IntPtr _window;

    public IntPtr SDLWindow => _window;

    private uint _windowId;

    // Used to determine whether a MouseEnter event should be emitted when a mouse event is received
    private bool _mouseFocus;

    public Size Size => new(_width, _height);

    private readonly IFileSystem _fs;
    private IntPtr _windowHandle;
    private int _width;
    private int _height;

    // Caches for cursors
    private readonly Dictionary<string, IntPtr> _cursorCache = new();
    private IntPtr _defaultCursor;
    private IntPtr _currentCursor = IntPtr.Zero;

    public IUiRoot? UiRoot
    {
        get => _uiRoot;
        set => _uiRoot = value;
    }

    public WindowConfig WindowConfig
    {
        get => _config;
        set
        {
            var changingFullscreen = _config.Windowed != value.Windowed;
            _config = value.Copy();

            if (changingFullscreen)
            {
                CreateWindowRectAndStyles(
                    out var x,
                    out var y,
                    out var width,
                    out var height,
                    out _
                );

                SDL_SetWindowFullscreen(
                    _window,
                    (uint) (_config.Windowed ? 0 : SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP)
                );

                if (_config.Windowed)
                {
                    SDL_SetWindowPosition(_window, x, y);
                    SDL_SetWindowSize(_window, width, height);
                    if (_config.Maximized)
                    {
                        SDL_MaximizeWindow(_window);
                    }
                }
            }
        }
    }
    
    public bool IsInForeground { get; set; } = true;

    public event Action<bool>? IsInForegroundChanged;

    public event Action<Size>? Resized;

    public event Action? Closed;

    public SizeF UiCanvasSize { get; private set; }

    private Size _uiCanvasTargetSize = new(1024, 768);
    
    private IUiRoot? _uiRoot;
    
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
                SDL_ShowCursor(SDL_ENABLE);
                UpdateCursor();
            }
            else
            {
                SDL_ShowCursor(SDL_DISABLE);
            }
        }
    }

    public MainWindow(WindowConfig config, IFileSystem fs)
    {
        try
        {
            WindowsPlatform.RegisterWindowClass(WindowClassName);
        }
        catch (EntryPointNotFoundException)
        {
        }

        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            throw new SDLException("Failed to initialize video subsystem.");
        }

        _defaultCursor = SDL_CreateSystemCursor(SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW);
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

        CreateWindow();
        UpdateUiCanvasSize();
    }

    public void Dispose()
    {
        if (_window != IntPtr.Zero)
        {
            if (_config.DisableScreenSaver)
            {
                SDL_EnableScreenSaver();
            }

            SDL_DestroyWindow(_window);
            try
            {
                WindowsPlatform.UnregisterWindowClass();
            }
            catch (EntryPointNotFoundException)
            {
            }

            SDL_FreeCursor(_defaultCursor);
            _defaultCursor = IntPtr.Zero;
            foreach (var cursor in _cursorCache.Values)
            {
                SDL_FreeCursor(cursor);
            }

            _cursorCache.Clear();
            _currentCursor = IntPtr.Zero;

            _window = IntPtr.Zero;
            _windowId = 0;
        }
    }

    [TempleDllLocation(0x101de880)]
    public void ProcessEvents()
    {
        while (SDL_PollEvent(out var e) != 0)
        {
            if (_eventFilter != null && _eventFilter(ref e))
            {
                continue;
            }

            switch (e.type)
            {
                case SDL_EventType.SDL_APP_TERMINATING:
                case SDL_EventType.SDL_QUIT:
                    Closed?.Invoke();
                    return;
                case SDL_EventType.SDL_WINDOWEVENT:
                    HandleWindowEvent(ref e.window);
                    return;

                case SDL_EventType.SDL_KEYDOWN:
                case SDL_EventType.SDL_KEYUP:
                    HandleKeyEvent(ref e.key);
                    break;

                case SDL_EventType.SDL_TEXTINPUT:
                    HandleTextInputEvent(ref e.text);
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    HandleMouseMoveEvent(ref e.motion);
                    break;
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    HandleMouseButtonEvent(true, ref e.button);
                    break;
                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    HandleMouseButtonEvent(false, ref e.button);
                    break;
                case SDL_EventType.SDL_MOUSEWHEEL:
                    HandleMouseWheelEvent(ref e.wheel);
                    break;
            }
        }

        UiRoot?.Tick();
    }

    private unsafe void HandleTextInputEvent(ref SDL_TextInputEvent e)
    {
        string text;
        fixed (byte* bytes = e.text)
        {
            text = Marshal.PtrToStringUTF8((IntPtr) bytes);
        }

        if (text is {Length: > 0})
        {
            _uiRoot?.TextInput(text);
        }
    }

    private void HandleKeyEvent(ref SDL_KeyboardEvent e)
    {
        // Ignore keyboard events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        var down = e.state == SDL_PRESSED;
        var repeat = e.repeat != 0;

        var keysym = e.keysym;
        var modifiers = GetKeyModifiers(keysym);

        Logger.Debug("key {0} scan_code={1}, key={2}, repeat={3}",
            down ? "down" : "up", SDL_GetScancodeName(keysym.scancode), SDL_GetKeyName(keysym.sym), repeat);

        // Handle Alt+Enter here
        if (modifiers == KeyModifier.Alt && keysym.scancode == SDL_Scancode.SDL_SCANCODE_RETURN)
        {
            // Ignore repeats, trigger on down
            if (!repeat && down)
            {
                var config = _config.Copy();
                config.Windowed = !config.Windowed;
                WindowConfig = config;
            }

            return;
        }

        if (down)
        {
            _uiRoot?.KeyDown(
                keysym.sym,
                keysym.scancode,
                modifiers,
                repeat
            );
        }
        else
        {
            _uiRoot?.KeyUp(
                keysym.sym,
                keysym.scancode,
                modifiers
            );
        }
    }

    private static KeyModifier GetKeyModifiers(SDL_Keysym keysym)
    {
        KeyModifier modifiers = default;
        if ((keysym.mod & SDL_Keymod.KMOD_ALT) != 0)
        {
            modifiers |= KeyModifier.Alt;
        }

        if ((keysym.mod & SDL_Keymod.KMOD_CTRL) != 0)
        {
            modifiers |= KeyModifier.Ctrl;
        }

        if ((keysym.mod & SDL_Keymod.KMOD_SHIFT) != 0)
        {
            modifiers |= KeyModifier.Shift;
        }

        if ((keysym.mod & SDL_Keymod.KMOD_GUI) != 0)
        {
            modifiers |= KeyModifier.Meta;
        }

        return modifiers;
    }

    private void HandleWindowEvent(ref SDL_WindowEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        switch (e.windowEvent)
        {
            case SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
            {
                var width = e.data1;
                var height = e.data2;

                // Ignore resizes to 0 (i.e. due to being minimized)
                if (width == 0 || height == 0)
                {
                    break;
                }

                // Update width/height with window client size
                if (width != _width || height != _height)
                {
                    _width = width;
                    _height = height;

                    Resized?.Invoke(Size);
                    UpdateUiCanvasSize();
                }

                // Persist changes to window size in window mode
                if (_config.Windowed && (_config.Width != width || _config.Height != height))
                {
                    _config.Width = width;
                    _config.Height = height;
                    Globals.Config.Window.Width = _config.Width;
                    Globals.Config.Window.Height = _config.Height;
                    Globals.ConfigManager.Save();
                }

                break;
            }
            case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
            case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                if (_config.Windowed)
                {
                    // If the window was maximized by the user, store that in the config
                    _config.Maximized = e.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED;
                    Globals.Config.Window.Maximized = _config.Maximized;
                    Globals.ConfigManager.Save();
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                HandleMouseFocusEvent(true);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                HandleMouseFocusEvent(false);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                Logger.Debug("Main window gained keyboard focus.");

                if (!IsInForeground)
                {
                    IsInForeground = true;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                Logger.Debug("Main window lost keyboard focus.");

                if (IsInForeground)
                {
                    IsInForeground = false;
                    IsInForegroundChanged?.Invoke(IsInForeground);
                }

                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                Closed?.Invoke();
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
                Logger.Debug("Main window is being offered keyboard focus.");
                SDL_SetWindowInputFocus(_window);
                SDL_RaiseWindow(_window);
                break;
            case SDL_WindowEventID.SDL_WINDOWEVENT_DISPLAY_CHANGED:
                break;
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

        SDL_SetCursor(cursor);
        _currentCursor = cursor;
    }

    private void UpdateUiCanvasSize()
    {
        // Attempt to fit 1024x768 onto the backbuffer
        var horScale = MathF.Max(1, _width / (float) _uiCanvasTargetSize.Width);
        var verScale = MathF.Max(1, _height / (float) _uiCanvasTargetSize.Height);
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
        SDL_Rect rect = default;
        rect.x = x;
        rect.y = y;
        rect.w = w;
        rect.h = h;

        if (SDL_SetWindowMouseRect(_window, ref rect) < 0)
        {
            Logger.Warn("Failed to confine cursor to window: {0}", SDL_GetError());
        }
    }

    // Sets a filter that receives a chance at intercepting all window messages
    public void SetWindowMsgFilter(SDLEventFilter filter)
    {
        _eventFilter = filter;
    }

    private void CreateWindow()
    {
        CreateWindowRectAndStyles(
            out var x,
            out var y,
            out var width,
            out var height,
            out var flags
        );

        flags |= SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

        // Show initially maximized in window mode, if the game was previously maximized
        if (_config.Windowed && _config.Maximized)
        {
            flags |= SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;
        }

        Logger.Info("Creating window with dimensions {0}x{1}", width, height);

        // Create our window
        _window = SDL_CreateWindow(WindowTitle, x, y, width, height, flags);

        // Make sure creating the window succeeded
        if (_window == IntPtr.Zero)
        {
            throw new SDLException("Failed to create main window");
        }

        _windowId = SDL_GetWindowID(_window);

        SDL_SysWMinfo wmInfo = default;
        SDL_VERSION(out wmInfo.version);
        if (SDL_GetWindowWMInfo(_window, ref wmInfo) != SDL_bool.SDL_TRUE)
        {
            throw new SDLException("Couldn't get HWND from Window");
        }

        _windowHandle = wmInfo.info.win.window;

        SDL_GetWindowSize(_window, out var actualWidth, out var actualHeight);
        // The returned size should never be zero, unless the window was forced by some hook to be minimized
        if (actualWidth > 0 && actualHeight > 0)
        {
            _width = actualWidth;
            _height = actualHeight;
            Logger.Info("Actual window dimensions {0}x{1}", width, height);
        }

        SDL_SetWindowMinimumSize(_window, _config.MinWidth, _config.MinHeight);

        if (_config.DisableScreenSaver)
        {
            SDL_DisableScreenSaver();
        }
    }

    private void CreateWindowRectAndStyles(out int x, out int y, out int width, out int height, out SDL_WindowFlags flags)
    {
        flags = default;

        if (!_config.Windowed)
        {
            x = SDL_WINDOWPOS_CENTERED;
            y = SDL_WINDOWPOS_CENTERED;
            // According to SDL2, this is ignored in fullscreen mode
            width = 1024;
            height = 768;
            flags = SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
        }
        else
        {
            x = SDL_WINDOWPOS_CENTERED;
            y = SDL_WINDOWPOS_CENTERED;
            width = _config.Width;
            height = _config.Height;
            flags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        }

        _width = width;
        _height = height;
    }

    private void HandleMouseWheelEvent(ref SDL_MouseWheelEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        HandleMouseFocusEvent(true);

        SDL_GetMouseState(out var x, out var y);

        var windowPos = new Point(x, y);
        var uiPos = TranslateToUiCanvas(windowPos);

        var units = e.preciseY;
        if (e.direction == (uint) SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED)
        {
            units *= -1;
        }

        UiRoot?.MouseWheel(
            windowPos,
            uiPos,
            units
        );
    }

    private void HandleMouseMoveEvent(ref SDL_MouseMotionEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        HandleMouseFocusEvent(true);
        var windowPos = new Point(e.x, e.y);
        var uiPos = TranslateToUiCanvas(windowPos);
        UiRoot?.MouseMove(
            windowPos,
            uiPos
        );
    }

    private void HandleMouseButtonEvent(bool down, ref SDL_MouseButtonEvent e)
    {
        // Ignore events for other windows
        if (e.windowID != _windowId)
        {
            return;
        }

        MouseButton button;
        if (e.button == SDL_BUTTON_LEFT)
        {
            button = MouseButton.LEFT;
        }
        else if (e.button == SDL_BUTTON_MIDDLE)
        {
            button = MouseButton.MIDDLE;
        }
        else if (e.button == SDL_BUTTON_RIGHT)
        {
            button = MouseButton.RIGHT;
        }
        else if (e.button == SDL_BUTTON_X1)
        {
            button = MouseButton.EXTRA1;
        }
        else if (e.button == SDL_BUTTON_X2)
        {
            button = MouseButton.EXTRA2;
        }
        else
        {
            Logger.Info("Ignoring event for unknown mouse button {0}", e.button);
            return;
        }

        HandleMouseFocusEvent(true);
        var windowPos = new Point(e.x, e.y);
        var uiPos = TranslateToUiCanvas(windowPos);
        if (down)
        {
            UiRoot?.MouseDown(windowPos, uiPos, button);
        }
        else
        {
            UiRoot?.MouseUp(windowPos, uiPos, button);
        }
    }

    private void HandleMouseFocusEvent(bool focus)
    {
        if (focus == _mouseFocus)
        {
            return;
        }

        _mouseFocus = focus;

        if (focus)
        {
            UiRoot?.MouseEnter();
        }
        else
        {
            UiRoot?.MouseLeave();
        }
    }

    private PointF TranslateToUiCanvas(int x, int y) => new(x / UiScale, y / UiScale);

    private PointF TranslateToUiCanvas(Point p) => TranslateToUiCanvas(p.X, p.Y);

    public void UpdateCursor()
    {
        SDL_SetCursor(_currentCursor != IntPtr.Zero ? _currentCursor : _defaultCursor);
    }
}