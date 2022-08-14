using System;
using System.Drawing;
using OpenTemple.Core.Config;

namespace OpenTemple.Core.Platform;

public enum WindowEventType
{
    /// <summary>
    /// Mouse has entered the window.
    /// </summary>
    MouseEnter,
    /// <summary>
    /// Mouse has left the window.
    /// </summary>
    MouseLeave,
    MouseMove,
    MouseDown,
    MouseUp,
    Wheel,
}

/// <summary>
/// Base class for all events emitted by the operating system window the game is running in.
/// </summary>
public class WindowEvent
{
    public WindowEventType Type { get; }

    public IMainWindow Window { get; }

    public WindowEvent(WindowEventType type, IMainWindow window)
    {
        Type = type;
        Window = window;
    }
}

/// <summary>
/// Event raised when the mouse cursor enters or leaves the game window (in full-screen mode by tabbing out for example).
/// </summary>
public class MouseFocusEvent : WindowEvent
{
    public MouseFocusEvent(WindowEventType type, IMainWindow window) : base(type, window)
    {
    }
}

/// <summary>
/// Events related to mouse input use this base class.
/// </summary>
public class MouseWindowEvent : WindowEvent
{
    public Point WindowPos { get; }

    public PointF UiPos { get; }

    public MouseButton Button { get; init; }

    public MouseWindowEvent(WindowEventType type, IMainWindow window, Point windowPos, PointF uiPos) : base(type,
        window)
    {
        WindowPos = windowPos;
        UiPos = uiPos;
    }
}

/// <summary>
/// An event related to mouse wheel input.
/// </summary>
public class MouseWheelWindowEvent : MouseWindowEvent
{
    /// <summary>
    /// The delta the mouse-wheel was moved counted in "notches" of the mouse-wheel.
    /// </summary>
    public float Delta { get; }

    public MouseWheelWindowEvent(WindowEventType type, IMainWindow window, Point windowPos, PointF uiPos,
        float delta) : base(type, window, windowPos, uiPos)
    {
        Delta = delta;
    }
}

public interface IMainWindow : IDisposable
{
    void SetWindowMsgFilter(SDLEventFilter filter);

    /// <summary>
    /// Subscribe to this event to receive input-related window events.
    /// Return true to prevent the event from being passed to the next handler.
    /// </summary>
    event Action<WindowEvent> OnInput;

    /// <summary>
    /// Invoked when the window changes size. Given size will be the new client area of the window.
    /// </summary>
    event Action<Size> Resized;

    /// <summary>
    /// Invoked when the player closes the window.
    /// </summary>
    event Action Closed;

    WindowConfig WindowConfig { get; set; }

    SizeF UiCanvasSize { get; }

    event Action UiCanvasSizeChanged;

    float UiScale { get; }

    void SetCursor(int hotspotX, int hotspotY, string imagePath);

    bool IsCursorVisible { set; }
    
    void ProcessEvents();
}