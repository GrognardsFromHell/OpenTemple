#nullable enable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public enum LgcyWindowMouseState
{
    Outside = 0,
    Hovered = 6,

    // I have not actually found any place where this is ever set
    Pressed = 7,
    PressedOutside = 8
}

public enum LgcyButtonState
{
    Normal = 0,
    Hovered = 1,
    Down = 2,
    Released = 3,
    Disabled = 4
}

public class UiManager
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private List<WidgetContainer> _topLevelWidgets = new();

    private int _maxZIndex = 0;

    /// <summary>
    /// The size in "virtual pixels" of the UI canvas.
    /// </summary>
    public Size CanvasSize => new(
        (int)_mainWindow.UiCanvasSize.Width,
        (int)_mainWindow.UiCanvasSize.Height
    );

    public event Action<Size>? OnCanvasSizeChanged;

    public IEnumerable<WidgetContainer> ActiveWindows => _topLevelWidgets;

    public UiManagerDebug Debug { get; }

    [TempleDllLocation(0x11E74384)] private WidgetBase? _mouseCaptureWidget;

    [TempleDllLocation(0x10301324)] private WidgetBase? _currentMouseOverWidget;

    public WidgetBase? CurrentMouseOverWidget => _currentMouseOverWidget;

    [TempleDllLocation(0x10301328)] private WidgetBase? mMouseButtonId; // TODO = temple.GetRef<int>(0x10301328);

    // Hang on to the delegate
    private readonly CursorDrawCallback? _renderTooltipCallback;

    [TempleDllLocation(0x10EF97C4)]
    [TempleDllLocation(0x101f97d0)]
    [TempleDllLocation(0x101f97e0)]
    public bool IsDragging => DraggedObject != null;

    /// <summary>
    /// Represents an arbitrary object that is currently dragged by the user with their mouse.
    /// </summary>
    public object? DraggedObject { get; set; }

    public WidgetContainer? Modal { get; set; }

    private readonly IMainWindow _mainWindow;

    private readonly MouseController _mouse = new();

    public MouseController Mouse => _mouse;

    public CursorController Cursor { get; }

    public UiManager(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        Cursor = new CursorController(_mouse, _mainWindow);
        _renderTooltipCallback = RenderTooltip;
        Debug = new UiManagerDebug(this);
        mainWindow.UiCanvasSizeChanged += () => OnCanvasSizeChanged?.Invoke(CanvasSize);

        _mainWindow.OnInput += HandleInputEvent;
    }

    /// <inheritdoc cref="IMainWindow.DragStartDistance"/>
    public Size DragStartDistance => _mainWindow.DragStartDistance;

    /*
    Add something to the list of active windows on top of all existing windows.
    */
    public void AddWindow(WidgetContainer window)
    {
        if (_topLevelWidgets.Contains(window))
        {
            // Window is already in the list
            return;
        }

        // Don't add it, if it's hidden
        if (!window.Visible)
        {
            return;
        }

        _topLevelWidgets.Add(window);
    }

    public void RemoveWindow(WidgetContainer window)
    {
        _topLevelWidgets.Remove(window);
        SortWindows();
    }

    public void BringToFront(WidgetContainer window)
    {
        window.ZIndex = _topLevelWidgets
            .Where(otherWindow => otherWindow != window)
            .Max(otherWindow => otherWindow.ZIndex) + 1;
        SortWindows();
    }

    public void SendToBack(WidgetContainer window)
    {
        window.ZIndex = int.MinValue;
        SortWindows();
    }

    public void SetVisible(WidgetBase widget, bool visible)
    {
        if (widget is WidgetContainer container && container.GetParent() == null)
        {
            if (visible)
            {
                AddWindow(container);
            }
            else
            {
                RemoveWindow(container);
            }
        }

        RefreshMouseOverState();
    }

    private static bool IsAncestor(WidgetBase? widget, WidgetBase parent)
    {
        while (widget != null)
        {
            if (widget == parent)
            {
                return true;
            }

            widget = widget.GetParent();
        }

        return false;
    }

    public void RemoveWidget(WidgetBase widget)
    {
        if (widget is WidgetContainer container)
        {
            RemoveWindow(container);
        }

        // Invalidate any fields that may still hold a reference to the now invalid widget id
        if (IsAncestor(mMouseButtonId, widget))
        {
            mMouseButtonId = null;
            RefreshMouseOverState();
        }

        // Release mouse capture if the widget is becoming unavailable
        if (IsAncestor(_mouseCaptureWidget, widget))
        {
            ReleaseMouseCapture();
            RefreshMouseOverState();
        }

        if (IsAncestor(_currentMouseOverWidget, widget))
        {
            if (_currentMouseOverWidget is WidgetButton button && !button.IsDisabled())
            {
                button.ButtonState = LgcyButtonState.Normal;
            }

            _currentMouseOverWidget = null;
            RefreshMouseOverState();
        }
    }

    private void ReleaseMouseCapture()
    {
        var currentCaptor = _mouseCaptureWidget;
        if (currentCaptor != null)
        {
            _mouseCaptureWidget = null;
            // Notify the current captor that it has lost the capture
            currentCaptor.NotifyMouseCaptureLost();
        }
    }

    [TempleDllLocation(0x101F8D10)]
    public void Render()
    {
        // Make a copy here since some vanilla logic will show/hide windows in their render callbacks
        var activeWindows = _topLevelWidgets;

        foreach (var windowId in activeWindows)
        {
            // Our new widget system handles rendering itself
            windowId.Render();
        }

        Debug.AfterRenderWidgets();
    }

    public WidgetBase GetWidgetAt(int x, int y)
    {
        WidgetBase result = null;

        // Backwards because of render order (rendered last is really on top)
        for (int i = _topLevelWidgets.Count - 1; i >= 0; --i)
        {
            var window = _topLevelWidgets[i];

            if (Modal != null && Modal != window)
            {
                continue;
            }

            if (window.Visible && DoesWidgetContain(window, x, y))
            {
                result = window;

                int localX = x - window.X;
                int localY = y - window.Y;

                var widgetIn = window.PickWidget(localX, localY);
                if (widgetIn != null)
                {
                    return widgetIn;
                }
            }
        }

        return result;
    }

    private static bool DoesWidgetContain(WidgetBase widget, int x, int y)
    {
        var rect = widget.GetContentArea();
        return x >= rect.X
               && y >= rect.Y
               && x < rect.X + rect.Width
               && y < rect.Y + rect.Height;
    }

    /**
        * Uses the current mouse position to refresh which widget is being moused over.
        * Useful if a widget is hidden, shown or added to update the mouse-over state
        * without actually moving the mouse.
        */
    public void RefreshMouseOverState()
    {
        var pos = _mouse.GetPos();
        var args = new MessageMouseArgs
        {
            X = pos.X,
            Y = pos.Y,
            flags = MouseEventFlag.PosChange | MouseEventFlag.PosChangeSlow
        };
        TranslateMouseMessage(args);
    }

    public WidgetBase? GetMouseCaptureWidget()
    {
        return _mouseCaptureWidget;
    }

    /// <summary>
    /// Tries to capture mouse input for the given widget. Only succeeds if no other widget currently has
    /// captured the mouse.
    /// </summary>
    [TempleDllLocation(0x101f9830)]
    public bool TryCaptureMouse(WidgetBase? widget)
    {
        // Only widgets that are part of the UI tree can capture the mouse
        var topMostParent = widget.TopMostParent;
        if (topMostParent == null || !_topLevelWidgets.Contains(topMostParent))
        {
            return false;
        }

        if (_mouseCaptureWidget == null)
        {
            _mouseCaptureWidget = widget;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Releases mouse capture for a given widget, if the mouse is currently captured by that widget.
    /// Otherwise does nothing.
    /// </summary>
    [TempleDllLocation(0x101f9850)]
    public void ReleaseMouseCapture(WidgetBase? widget)
    {
        if (_mouseCaptureWidget == widget)
        {
            ReleaseMouseCapture();
        }
    }

    /*
    This will sort the windows using their z-order in the order in which
    they should be rendered.
    */
    private void SortWindows()
    {
        // Sort Windows by Z-Index
        _topLevelWidgets.Sort((windowA, windowB) => { return windowA.ZIndex.CompareTo(windowB.ZIndex); });

        // Reassign a zindex in monotonous order to those windows that dont have one
        for (var i = 0; i < _topLevelWidgets.Count; ++i)
        {
            var window = _topLevelWidgets[i];
            if (window.ZIndex == 0)
            {
                window.ZIndex = i * 100;
            }
        }
    }

    private void RenderTooltip(int x, int y, object userArg)
    {
        _currentMouseOverWidget?.RenderTooltip(x, y);
    }

    /// <summary>
    /// Handles a mouse message and produces higher level mouse messages based on it.
    /// </summary>
    [TempleDllLocation(0x101f9970)]
    public bool TranslateMouseMessage(MessageMouseArgs mouseMsg)
    {
        var flags = mouseMsg.flags;
        var x = mouseMsg.X;
        var y = mouseMsg.Y;

        var newTigMsg = new MessageWidgetArgs();
        newTigMsg.x = x;
        newTigMsg.y = y;

        var widAtCursor = GetWidgetAt(x, y);

        var globalWid = _currentMouseOverWidget;

        // moused widget changed
        if ((flags & MouseEventFlag.PosChange) != 0 && widAtCursor != globalWid)
        {
            if (widAtCursor != null && CursorDrawCallback == _renderTooltipCallback)
            {
                SetCursorDrawCallback(null, 0);
            }

            if (globalWid != null)
            {
                bool enqueueExited = false;
                // if window
                if (globalWid is WidgetContainer prevHoveredWindow)
                {
                    if (prevHoveredWindow.MouseState == LgcyWindowMouseState.Pressed)
                    {
                        prevHoveredWindow.MouseState = LgcyWindowMouseState.PressedOutside;
                    }
                    else if (prevHoveredWindow.MouseState != LgcyWindowMouseState.PressedOutside)
                    {
                        prevHoveredWindow.MouseState = LgcyWindowMouseState.Outside;
                    }
                }
                // button
                else if (globalWid is WidgetButtonBase buttonWid && !buttonWid.IsDisabled())
                {
                    switch (buttonWid.ButtonState)
                    {
                        case LgcyButtonState.Hovered:
                            // Unhover
                            buttonWid.ButtonState = LgcyButtonState.Normal;
                            Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOff);
                            break;
                        case LgcyButtonState.Down:
                            // Down . Released without click event
                            buttonWid.ButtonState = LgcyButtonState.Released;
                            break;
                    }
                }

                if (IsVisible(globalWid))
                {
                    newTigMsg.widgetId = globalWid;
                    newTigMsg.widgetEventType = TigMsgWidgetEvent.Exited;
                    Tig.MessageQueue.Enqueue(new Message(newTigMsg));
                }
            }

            if (widAtCursor != null)
            {
                if (widAtCursor is WidgetContainer widAtCursorWindow)
                {
                    if (widAtCursorWindow.MouseState == LgcyWindowMouseState.PressedOutside)
                    {
                        widAtCursorWindow.MouseState = LgcyWindowMouseState.Pressed;
                    }
                    else if (widAtCursorWindow.MouseState != LgcyWindowMouseState.Pressed)
                    {
                        widAtCursorWindow.MouseState = LgcyWindowMouseState.Hovered;
                    }
                }
                else if (widAtCursor is WidgetButtonBase buttonWid && !buttonWid.IsDisabled())
                {
                    if (buttonWid.ButtonState != LgcyButtonState.Normal)
                    {
                        if (buttonWid.ButtonState == LgcyButtonState.Released)
                        {
                            buttonWid.ButtonState = LgcyButtonState.Down;
                        }
                    }
                    else
                    {
                        buttonWid.ButtonState = LgcyButtonState.Hovered;
                        Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOn);
                    }
                }

                newTigMsg.widgetId = widAtCursor;
                newTigMsg.widgetEventType = TigMsgWidgetEvent.Entered;
                Tig.MessageQueue.Enqueue(new Message(newTigMsg));
            }

            globalWid = _currentMouseOverWidget = widAtCursor;
        }

        if ((mouseMsg.flags & MouseEventFlag.PosChangeSlow) != 0
            && globalWid != null
            && CursorDrawCallback == null)
        {
            SetCursorDrawCallback(_renderTooltipCallback);
        }

        if ((mouseMsg.flags & MouseEventFlag.LeftClick) != 0)
        {
            // probably redundant to do again, but just to be safe...
            var widIdAtCursor2 = GetWidgetAt(mouseMsg.X, mouseMsg.Y);
            if (widIdAtCursor2 != null)
            {
                if (widIdAtCursor2 is WidgetButtonBase button && !button.IsDisabled())
                {
                    switch (button.ButtonState)
                    {
                        case LgcyButtonState.Hovered:
                            button.ButtonState = LgcyButtonState.Down;
                            Tig.Sound.PlaySoundEffect(button.sndDown);
                            break;
                        case LgcyButtonState.Disabled:
                            return false;
                    }
                }

                newTigMsg.widgetEventType = TigMsgWidgetEvent.Clicked;
                newTigMsg.widgetId = widIdAtCursor2;
                mMouseButtonId = widIdAtCursor2;
                Tig.MessageQueue.Enqueue(new Message(newTigMsg));
            }
        }

        if ((mouseMsg.flags & MouseEventFlag.LeftReleased) != 0 && mMouseButtonId != null)
        {
            if (mMouseButtonId is WidgetButtonBase button && !button.IsDisabled())
            {
                switch (button.ButtonState)
                {
                    case LgcyButtonState.Down:
                        button.ButtonState = LgcyButtonState.Hovered;
                        Tig.Sound.PlaySoundEffect(button.sndClick);
                        break;
                    case LgcyButtonState.Released:
                        button.ButtonState = LgcyButtonState.Normal;
                        Tig.Sound.PlaySoundEffect(button.sndClick);
                        break;
                    case LgcyButtonState.Disabled:
                        return false;
                }
            }

            // probably redundant to do again, but just to be safe...
            var widIdAtCursor2 = GetWidgetAt(mouseMsg.X, mouseMsg.Y);
            newTigMsg.widgetId = mMouseButtonId;
            newTigMsg.widgetEventType = (widIdAtCursor2 != mMouseButtonId)
                ? TigMsgWidgetEvent.MouseReleasedAtDifferentButton
                : TigMsgWidgetEvent.MouseReleased;
            Tig.MessageQueue.Enqueue(new Message(newTigMsg));
            mMouseButtonId = null;
        }

        return false;
    }

    /// <summary>
    /// Checks if a widget is really on screen by checking all of it's parents as well for visibility.
    /// </summary>
    public bool IsVisible(WidgetBase widget)
    {
        while (true)
        {
            if (!widget.Visible)
            {
                return false;
            }

            if (widget.GetParent() == null)
            {
                // It must be a top-level window to be visible
                return ActiveWindows.Contains(widget);
            }
            else
            {
                widget = widget.GetParent();
            }
        }
    }

    [TempleDllLocation(0x101f8a80)]
    public bool ProcessMessage(Message msg)
    {
        if (msg.type == MessageType.UPDATE_TIME)
        {
            Mouse.AdvanceTime();

            // Dispatch time update messages continuously to all advanced widgets
            foreach (var entry in _topLevelWidgets)
            {
                entry.OnUpdateTime(msg.created);
            }
        }

        switch (msg.type)
        {
            case MessageType.MOUSE:
                return ProcessMouseMessage(msg);
            case MessageType.WIDGET:
                return ProcessWidgetMessage(msg);
            default:
                // In order from top to bottom (back is top)
                for (var i = _topLevelWidgets.Count - 1; i >= 0; i--)
                {
                    var window = _topLevelWidgets[i];

                    // Skip the top-level window if there's a modal and the modal is a different window
                    if (Modal != null && Modal != window)
                    {
                        continue;
                    }

                    if (window.HandleMessage(msg))
                    {
                        return true;
                    }
                }

                return false;
        }
    }

    private bool ProcessWidgetMessage(Message msg)
    {
        var widgetArgs = msg.WidgetArgs;

        var dispatchTo = widgetArgs.widgetId;
        while (dispatchTo != null)
        {
            var parent = dispatchTo.GetParent();
            if ((parent == null || parent.Visible) && dispatchTo.Visible)
            {
                if (dispatchTo.HandleMessage(msg))
                {
                    return true;
                }
            }

            // Bubble up the msg if the widget didn't handle it
            dispatchTo = dispatchTo.GetParent();
        }

        return false;
    }

    private bool ProcessMouseMessage(Message msg)
    {
        // Handle if a widget requested mouse capture
        if (_mouseCaptureWidget != null)
        {
            _mouseCaptureWidget.HandleMessage(msg);
            return true;
        }

        var mouseArgs = msg.MouseArgs;

        for (var i = _topLevelWidgets.Count - 1; i >= 0; i--)
        {
            var window = _topLevelWidgets[i];

            // Skip the top-level window if there's a modal and the modal is a different window
            if (Modal != null && Modal != window)
            {
                continue;
            }

            if (!window.Visible || !DoesWidgetContain(window, mouseArgs.X, mouseArgs.Y))
            {
                continue;
            }

            // Try dispatching the msg to all children of the window that are also under the mouse cursor, in reverse order of their
            // own insertion into the children list
            for (var j = window.GetChildren().Count - 1; j >= 0; j--)
            {
                var childWidget = window.GetChildren()[j];

                if (DoesWidgetContain(childWidget, mouseArgs.X, mouseArgs.Y))
                {
                    if (childWidget.Visible && childWidget.HandleMouseMessage(mouseArgs))
                    {
                        return true;
                    }
                }
            }

            // After checking with all children, dispatch the msg to the window itself
            if (window.Visible && window.HandleMouseMessage(mouseArgs))
            {
                return true;
            }
        }

        if (Modal != null)
        {
            // Always swallow mouse messages when a modal is visible
            return true;
        }

        return false;
    }

    private void HandleInputEvent(WindowEvent e)
    {
        switch (e.Type)
        {
            case WindowEventType.MouseEnter:
                _mouse.IsMouseOutsideWindow = false;
                break;
            case WindowEventType.MouseLeave:
                _mouse.IsMouseOutsideWindow = true;
                break;
            case WindowEventType.MouseMove:
                HandleMouseMoveEvent((MouseWindowEvent)e);
                break;
            case WindowEventType.MouseDown:
            case WindowEventType.MouseUp:
                HandleMouseButtonEvent((MouseWindowEvent)e);
                break;
            case WindowEventType.Wheel:
                HandleMouseWheelEvent((MouseWheelWindowEvent)e);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleMouseMoveEvent(MouseWindowEvent evt)
    {
        _mouse.SetPos((int)evt.UiPos.X, (int)evt.UiPos.Y, 0);
    }

    private void HandleMouseButtonEvent(MouseWindowEvent evt)
    {
        // Otherwise we might not get the actual position that was clicked if we use the last mouse move position..
        if (evt.Type == WindowEventType.MouseDown)
        {
            HandleMouseMoveEvent(evt);
        }

        _mouse.SetButtonState(evt.Button, evt.Type == WindowEventType.MouseDown);
    }

    private void HandleMouseWheelEvent(MouseWheelWindowEvent evt)
    {
        _mouse.SetPos((int)evt.UiPos.X, (int)evt.UiPos.Y, (int)(evt.Delta * 120));
    }

    private void OnMouseMove(PointF pos, int wheelDelta)
    {
        var x = pos.X;
        var y = pos.Y;
        var width = CanvasSize.Width;
        var height = CanvasSize.Height;

        // Account for a resized screen
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            if (Globals.Config.Window.Windowed)
            {
                if ((x > -7 && x < width + 7 && x > -7 && y < height + 7))
                {
                    if (x < 0)
                        x = 0;
                    else if (x > width)
                        x = width;
                    if (y < 0)
                        y = 0;
                    else if (y > height)
                        y = height;
                    _mouse.IsMouseOutsideWindow = false;
                    // TODO: Switch all mouse positioning to floats
                    _mouse.SetPos((int)x, (int)y, wheelDelta);
                    return;
                }
                else
                {
                    _mouse.IsMouseOutsideWindow = true;
                }
            }
            else
            {
                Logger.Info("Mouse outside resized window: {0},{1}, wheel: {2}", x, y, wheelDelta);
            }

            return;
        }

        _mouse.IsMouseOutsideWindow = false;
        // TODO: Switch all mouse positioning to floats
        _mouse.SetPos((int)pos.X, (int)pos.Y, wheelDelta);
    }

    // This is sometimes queried by ToEE to check which callback is active
    // It contains the callback function's address
    [TempleDllLocation(0x101dd5e0)] public CursorDrawCallback? CursorDrawCallback { get; private set; }

    // Extra argument passed to cursor draw callback.
    public object? CursorDrawCallbackArg { get; private set; }

    [TempleDllLocation(0x101DD5C0)]
    public void SetCursorDrawCallback(CursorDrawCallback? callback, object? arg = null)
    {
        CursorDrawCallback = callback;
        CursorDrawCallbackArg = arg;
    }

    [TempleDllLocation(0x101dd330, true)]
    public void DrawTooltip()
    {
        var pos = _mouse.GetPos();
        CursorDrawCallback?.Invoke(pos.X, pos.Y, CursorDrawCallbackArg);
    }
}

// This was previously known as "Tig Mouse", but since we are now moving all mouse input to be under
// the UI subsystem, we made it a private implementation detail of UiManager
public class MouseController
{
    /// <summary>
    /// The time span in which the mouse must not move after being last moved until we send
    /// a message with the <see cref="MouseEventFlag.PosChangeSlow"/> flag.
    /// </summary>
    private static readonly TimeSpan DelayUntilMousePosStable = TimeSpan.FromMilliseconds(35);

    // Millisecond interval in which mouse button held messages are sent
    private static readonly TimeSpan HeldMessageInterval = TimeSpan.FromMilliseconds(250);

    private static readonly int[] buttonStatePressed1 =
    {
        0x02, // Left button
        0x10, // Right button
        0x80 // Middle button
    };

    private static readonly int[] buttonStatePressed2 =
    {
        0x004, // Left button
        0x020, // Right button
        0x100 // Middle button
    };

    private static readonly int[] buttonStateReleased =
    {
        0x008, // Left button
        0x040, // Right button
        0x200 // Middle button
    };

    private enum MouseEventAction
    {
        Released,
        Pressed,
        Held
    }

    private int _flags;
    private int _x;
    private int _y;

    private readonly TimePoint[] _mouseButtonTime = new TimePoint[3];

    private bool _mouseStoppedMoving;

    private TimePoint _lastMousePosChange = TimePoint.Now;

    /// <summary>
    /// The cursor can be locked in place to enable relative movement (i.e. when dragging around maps).
    /// The mouse will automatically be captured as long as a mouse button is held.
    /// </summary>
    public bool IsRelative { get; private set; }

    /// <summary>
    /// The mouse position where relative mode was entered. The mouse will be held in place here.
    /// </summary>
    [TempleDllLocation(0x10d251c0), TempleDllLocation(0x10d25580)]
    private Point _relativeModeEnteredPos;

    public event Action OnIsRelativeChanged;

    public MouseController()
    {
        // Move the mouse so it's initially outside the window
        _x = -1;
        _y = -1;
    }

    public void SetButtonState(MouseButton button, bool pressed)
    {
        var buttonIndex = (int)button;
        var newFlags = 0;
        var now = TimePoint.Now;

        if (pressed)
        {
            if (!IsPressed(button))
            {
                newFlags = buttonStatePressed1[buttonIndex];
                _mouseButtonTime[buttonIndex] = now;
                QueueMouseButtonMessage(button, MouseEventAction.Pressed);
            }
        }
        else
        {
            if (IsPressed(button))
            {
                newFlags = buttonStateReleased[buttonIndex];
                _mouseButtonTime[buttonIndex] = TimePoint.Now;

                QueueMouseButtonMessage(button, MouseEventAction.Released);
            }
        }

        _flags = newFlags;
    }

    private bool IsPressed(MouseButton button)
    {
        var buttonIndex = (int)button;
        return (_flags & (buttonStatePressed1[buttonIndex] | buttonStatePressed2[buttonIndex])) != 0;
    }

    private void QueueMouseButtonMessage(MouseButton button, MouseEventAction action)
    {
        Tig.MessageQueue.Enqueue(new Message(
            new MessageMouseArgs(_x, _y, 0, GetMouseEventFlag(button, action)))
        );
    }

    private static MouseEventFlag GetMouseEventFlag(MouseButton button, MouseEventAction action)
    {
        switch (button)
        {
            case MouseButton.LEFT:
                switch (action)
                {
                    case MouseEventAction.Released:
                        return MouseEventFlag.LeftReleased;
                    case MouseEventAction.Pressed:
                        return MouseEventFlag.LeftClick;
                    case MouseEventAction.Held:
                        return MouseEventFlag.LeftHeld;
                }

                break;
            case MouseButton.RIGHT:
                switch (action)
                {
                    case MouseEventAction.Released:
                        return MouseEventFlag.RightReleased;
                    case MouseEventAction.Pressed:
                        return MouseEventFlag.RightClick;
                    case MouseEventAction.Held:
                        return MouseEventFlag.RightHeld;
                }

                break;
            case MouseButton.MIDDLE:
                switch (action)
                {
                    case MouseEventAction.Released:
                        return MouseEventFlag.MiddleReleased;
                    case MouseEventAction.Pressed:
                        return MouseEventFlag.MiddleClick;
                    case MouseEventAction.Held:
                        return MouseEventFlag.MiddleHeld;
                }

                break;
        }

        throw new ArgumentOutOfRangeException();
    }

    public Point GetPos() => new(_x, _y);

    public bool IsMouseOutsideWindow { get; set; } = true;

    /// <summary>
    /// we no longer call SetPos if nothing has changed, so we need this function to trigger the PosChangeSlow
    /// event.
    /// </summary>
    [TempleDllLocation(0x101dd7c0)]
    public void AdvanceTime()
    {
        var now = TimePoint.Now;

        if (!_mouseStoppedMoving && now - _lastMousePosChange > DelayUntilMousePosStable)
        {
            var eventFlags = MouseEventFlag.PosChangeSlow | GetEventFlagsFromButtonState();
            _mouseStoppedMoving = true;
            var args = new MessageMouseArgs(
                _x, _y, 0, eventFlags
            );
            Tig.MessageQueue.Enqueue(new Message(args));
        }

        // This sends messages if buttons are held for 250ms or longer.
        // This was previously handled by the DirectInput polling code in 0x101dd7c0
        void TrySendHeldMessage(MouseButton button)
        {
            if (IsPressed(button))
            {
                var buttonIndex = (int)button;
                _flags = buttonStatePressed2[buttonIndex];
                // Send a mouse button held message every 250ms after it was initially pressed
                if (now - _mouseButtonTime[buttonIndex] > HeldMessageInterval)
                {
                    _flags |= buttonStatePressed1[buttonIndex];
                    _mouseButtonTime[buttonIndex] = now;

                    QueueMouseButtonMessage(button, MouseEventAction.Held);
                }
            }
        }

        TrySendHeldMessage(MouseButton.LEFT);
        TrySendHeldMessage(MouseButton.RIGHT);
        TrySendHeldMessage(MouseButton.MIDDLE);
    }

    [TempleDllLocation(0x101DD070)]
    public void SetPos(int x, int y, int wheelDelta)
    {
        MouseEventFlag eventFlags = 0;

        if (x != _x || y != _y)
        {
            _mouseStoppedMoving = false;
            _lastMousePosChange = TimePoint.Now;
            eventFlags = MouseEventFlag.PosChange;
        }
        else if (!_mouseStoppedMoving && TimePoint.Now - _lastMousePosChange > DelayUntilMousePosStable)
        {
            _mouseStoppedMoving = true;
            eventFlags |= MouseEventFlag.PosChangeSlow;
        }

        if (wheelDelta != 0)
        {
            eventFlags |= MouseEventFlag.ScrollWheelChange;
        }

        _x = x;
        _y = y;

        eventFlags |= GetEventFlagsFromButtonState();

        if (eventFlags != 0)
        {
            var args = new MessageMouseArgs(
                x, y, wheelDelta, eventFlags
            );
            Tig.MessageQueue.Enqueue(new Message(args));
        }

        // Return the cursor to the lock position such that the next mouse movement message is
        // likely to be relative to this position again.
        if (IsRelative)
        {
            SetCursorPos(_relativeModeEnteredPos.X, _relativeModeEnteredPos.Y);
        }
    }

    private MouseEventFlag GetEventFlagsFromButtonState()
    {
        MouseEventFlag eventFlags = default;

        if (((buttonStatePressed1[0] | buttonStatePressed2[0]) & _flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.LEFT, MouseEventAction.Held);
        }

        if (((buttonStatePressed1[1] | buttonStatePressed2[1]) & _flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.RIGHT, MouseEventAction.Held);
        }

        if (((buttonStatePressed1[2] | buttonStatePressed2[2]) & _flags) != 0)
        {
            eventFlags |= GetMouseEventFlag(MouseButton.MIDDLE, MouseEventAction.Held);
        }

        return eventFlags;
    }

    [TempleDllLocation(0x101ddee0)]
    public void PushCursorLock()
    {
        GetCursorPos(out _relativeModeEnteredPos);
        IsRelative = true;
        OnIsRelativeChanged?.Invoke();
    }

    [TempleDllLocation(0x101dd470)]
    public void PopCursorLock()
    {
        if (IsRelative)
        {
            IsRelative = false;
            SetCursorPos(_relativeModeEnteredPos.X, _relativeModeEnteredPos.Y);
            OnIsRelativeChanged?.Invoke();
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Point point);
}

public class CursorController
{
    private readonly MouseController _mouse;

    private readonly IMainWindow _mainWindow;

    // whether the mouse cursor should currently be hidden (i.e. for cutscenes, while dragging, etc.)
    private bool _hidden;

    [TempleDllLocation(0x10D2558C)] private ResourceRef<ITexture> _iconUnderCursor;

    private Point _iconUnderCursorCenter;

    private Size _iconUnderCursorSize;

    private readonly Stack<string> _cursorStash = new();

    public CursorController(MouseController mouse, IMainWindow mainWindow)
    {
        _mouse = mouse;
        _mainWindow = mainWindow;
        _mouse.OnIsRelativeChanged += UpdateCursorVisibility;
    }

    public void Show()
    {
        _hidden = false;
        UpdateCursorVisibility();
    }

    public void Hide()
    {
        _hidden = true;
        UpdateCursorVisibility();
    }

    public void SetCursor(string cursorPath)
    {
        _cursorStash.Push(cursorPath);
        SetCursorInternal(cursorPath);
    }

    private void SetCursorInternal(string cursorPath)
    {
        int hotspotX = 0;
        int hotspotY = 0;

        // TODO: Introduce special "cursor" file type that includes hotspot info to remove these hardcoded values
        // Special handling for cursors that don't have their hotspot on 0,0
        if (cursorPath.Contains("Map_GrabHand_Closed.tga")
            || cursorPath.Contains("Map_GrabHand_Open.tga")
            || cursorPath.Contains("SlidePortraits.tga"))
        {
            var data = Tig.FS.ReadBinaryFile(cursorPath);
            var info = IO.Images.ImageIO.DetectImageFormat(data);

            hotspotX = info.width / 2;
            hotspotY = info.height / 2;
        }
        else if (cursorPath.Contains("ZoomCursor.tga"))
        {
            // This was previously set from the townmap UI via function @ 0x101dd4a0
            hotspotX = 10;
            hotspotY = 11;
        }

        Tig.MainWindow.SetCursor(hotspotX, hotspotY, cursorPath);
    }

    [TempleDllLocation(0x101DD770)]
    [TemplePlusLocation("tig_mouse.cpp:180")]
    public void Reset()
    {
        // The back is the one on screen
        if (_cursorStash.Count > 0)
        {
            _cursorStash.Pop();
        }

        // The back is the one on screen
        if (_cursorStash.TryPeek(out var texturePath))
        {
            SetCursorInternal(texturePath);
        }
    }

    [TempleDllLocation(0x101dd500)]
    public void SetDraggedIcon(string texturePath, Point center, Size size = default)
    {
        using var texture = Tig.Textures.Resolve(texturePath, false);
        SetDraggedIcon(texture.Resource, center, size);
    }

    public void SetDraggedIcon(ITexture texture, Point center, Size size = default)
    {
        if (texture != null)
        {
            if (size.IsEmpty)
            {
                _iconUnderCursorSize = texture.GetSize();
            }
            else
            {
                _iconUnderCursorSize = size;
            }

            _iconUnderCursorCenter = center;
            _iconUnderCursor.Dispose();
            _iconUnderCursor = texture.Ref();
        }
        else
        {
            _iconUnderCursor.Dispose();
        }
    }

    [TempleDllLocation(0x101dd500, true)]
    public void ClearDraggedIcon()
    {
        _iconUnderCursor.Dispose();
        _iconUnderCursorCenter = Point.Empty;
        _iconUnderCursorSize = Size.Empty;
    }

    [TempleDllLocation(0x101dd330, true)]
    public void DrawItemUnderCursor()
    {
        if (!_iconUnderCursor.IsValid)
        {
            return;
        }

        var mousePos = _mouse.GetPos();

        Tig.ShapeRenderer2d.DrawRectangle(
            mousePos.X + _iconUnderCursorCenter.X,
            mousePos.Y + _iconUnderCursorCenter.Y,
            _iconUnderCursorSize.Width,
            _iconUnderCursorSize.Height,
            _iconUnderCursor.Resource
        );
    }

    private void UpdateCursorVisibility()
    {
        _mainWindow.IsCursorVisible = !_hidden && !_mouse.IsRelative;
    }
}