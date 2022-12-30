using System;
using System.Drawing;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class GameViewScrollingController
{
    private readonly WidgetContainer _widget;
    private readonly IGameViewport _viewport;
    private TimePoint _lastScrolling;
    private static readonly TimeSpan ScrollButterDelay = TimeSpan.FromMilliseconds(16);
    private bool _grabMoving;
    private PointF _grabMoveRef;
    private PointF? _lastMousePos;
    public bool IsMouseScrolling { get; set; } = true;

    public GameViewScrollingController(WidgetContainer widget, IGameViewport viewport)
    {
        _widget = widget;
        _viewport = viewport;

        widget.AddHeldHotkey(InGameHotKey.ScrollLeft, condition: () => viewport.IsInteractive);
        widget.AddHeldHotkey(InGameHotKey.ScrollUp, condition: () => viewport.IsInteractive);
        widget.AddHeldHotkey(InGameHotKey.ScrollRight, condition: () => viewport.IsInteractive);
        widget.AddHeldHotkey(InGameHotKey.ScrollDown, condition: () => viewport.IsInteractive);
    }

    public bool MiddleMouseDown(PointF pos)
    {
        _grabMoving = _widget.SetMouseCapture();
        _grabMoveRef = pos;
        return true;
    }

    public bool MiddleMouseUp()
    {
        if (_grabMoving)
        {
            _widget.ReleaseMouseCapture();
            _grabMoving = false;
            _grabMoveRef = Point.Empty;
            return true;
        }

        return false;
    }

    public bool MouseMoved(PointF pos)
    {
        _lastMousePos = pos;

        if (!_grabMoving)
        {
            return false;
        }

        var dx = pos.X - _grabMoveRef.X;
        var dy = pos.Y - _grabMoveRef.Y;
        dx = (int) (dx / _viewport.Zoom);
        dy = (int) (dy / _viewport.Zoom);

        GameSystems.Scroll.ScrollBy(_viewport, (int) dx, (int) dy);

        _grabMoveRef = pos;
        return true;
    }

    [TempleDllLocation(0x10113fb0)]
    private void DoKeyboardScrolling()
    {
        if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollUp))
        {
            if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollLeft))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP_LEFT);
            }
            else if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollRight))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP_RIGHT);
            }
            else
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.UP);
            }
        }
        else if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollDown))
        {
            if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollLeft))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN_LEFT);
            }
            else if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollRight))
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN_RIGHT);
            }
            else
            {
                GameSystems.Scroll.SetScrollDirection(ScrollDirection.DOWN);
            }
        }
        else if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollLeft))
        {
            GameSystems.Scroll.SetScrollDirection(ScrollDirection.LEFT);
        }
        else if (_widget.IsHeldHotkeyPressed(InGameHotKey.ScrollRight))
        {
            GameSystems.Scroll.SetScrollDirection(ScrollDirection.RIGHT);
        }
    }

    [TempleDllLocation(0x10001010)]
    public void UpdateTime(TimePoint time)
    {
        DoKeyboardScrolling();

        // When we're grab-moving, do not do border-scrolling
        if (_grabMoving)
        {
            return;
        }

        // Wait until the mouse has moved at least once to avoid the issue of the mouse at 0,0
        // causing the screen to move directly on startup.
        if (_lastMousePos == null)
        {
            return;
        }

        var config = Globals.Config.Window;
        if (config.Windowed && _widget.UiManager?.ContainsMouse != true)
        {
            return;
        }

        if (_lastScrolling.Time != 0 && time - _lastScrolling < ScrollButterDelay)
        {
            if (!Globals.Config.ScrollAcceleration)
            {
                return;
            }
        }

        _lastScrolling = time;

        if (TryGetMouseScrollingDirection(_lastMousePos.Value, out var scrollDir))
        {
            GameSystems.Scroll.SetScrollDirection(scrollDir);
        }
    }

    /// <summary>
    /// Tries to get the direction the view will be scrolled in when the mouse is at the given position.
    /// Returns true if there is such a position and false if the mouse is not in a mouse-scrolling zone
    /// or mouse scrolling is disabled.
    /// </summary>
    public bool TryGetMouseScrollingDirection(PointF mousePos, out ScrollDirection direction)
    {
        if (!IsMouseScrolling)
        {
            direction = default;
            return false;
        }
        
        int scrollMarginTop = 2;
        int scrollMarginBottom = 2;
        int scrollMarginLeft = 3;
        int scrollMarginRight = 3;
        if (Globals.Config.Window.Windowed)
        {
            scrollMarginTop = 7;
            scrollMarginBottom = 7;
            scrollMarginLeft = 7;
            scrollMarginRight = 7;
        }
        if (Globals.Config.EnableDebugUI)
        {
            // The debug main menu overlaps the top scroll area
            scrollMarginTop += DebugUiSystem.ReservedVerticalSpace;
        }

        // TODO This should be the size of the game view
        var size = _widget.GetSize();
        var renderWidth = size.Width;
        var renderHeight = size.Height;

        if (mousePos.X <= scrollMarginLeft) // scroll left
        {
            if (mousePos.Y <= scrollMarginTop) // scroll upper left
                direction = ScrollDirection.UP_LEFT;
            else if (mousePos.Y >= renderHeight - scrollMarginBottom) // scroll bottom left
                direction = ScrollDirection.DOWN_LEFT;
            else
                direction = ScrollDirection.LEFT;
        }
        else if (mousePos.X >= renderWidth - scrollMarginRight) // scroll right
        {
            if (mousePos.Y <= scrollMarginTop) // scroll top right
                direction = ScrollDirection.UP_RIGHT;
            else if (mousePos.Y >= renderHeight - scrollMarginBottom) // scroll bottom right
                direction = ScrollDirection.DOWN_RIGHT;
            else
                direction = ScrollDirection.RIGHT;
        }
        else if (mousePos.Y <= scrollMarginTop) // scroll up
        {
            direction = ScrollDirection.UP;
        }
        else if (mousePos.Y >= renderHeight - scrollMarginBottom) // scroll down
        {
            direction = ScrollDirection.DOWN;
        }
        else
        {
            direction = default;
            return false;
        }

        return true;
    }
}