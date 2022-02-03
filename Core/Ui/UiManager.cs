using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Styles;
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

    private int maxZIndex = 0;

    /// <summary>
    /// The size in "virtual pixels" of the UI canvas.
    /// </summary>
    public Size CanvasSize => new Size(
        (int) _mainWindow.UiCanvasSize.Width,
        (int) _mainWindow.UiCanvasSize.Height
    );

    public event Action<Size> OnCanvasSizeChanged;

    public IEnumerable<WidgetContainer> ActiveWindows => _topLevelWidgets;

    public UiManagerDebug Debug { get; }

    [TempleDllLocation(0x11E74384)]
    private WidgetBase _mouseCaptureWidget;

    [TempleDllLocation(0x10301324)]
    private WidgetBase _currentMouseOverWidget;

    public WidgetBase CurrentMouseOverWidget => _currentMouseOverWidget;

    [TempleDllLocation(0x10301328)]
    private WidgetBase mMouseButtonId; // TODO = temple.GetRef<int>(0x10301328);

    // Hang on to the delegate
    private readonly CursorDrawCallback _renderTooltipCallback;

    [TempleDllLocation(0x10EF97C4)]
    [TempleDllLocation(0x101f97d0)]
    [TempleDllLocation(0x101f97e0)]
    public bool IsDragging { get; set; }

    public WidgetContainer Modal { get; set; }

    private readonly IMainWindow _mainWindow;

    public UiManager(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _renderTooltipCallback = RenderTooltip;
        Debug = new UiManagerDebug(this);
        mainWindow.UiCanvasSizeChanged += () => OnCanvasSizeChanged?.Invoke(CanvasSize);

        _mainWindow.OnInput += HandleInputEvent;
    }

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

    private static bool IsAncestor(WidgetBase widget, WidgetBase parent)
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

        if (IsAncestor(_mouseCaptureWidget, widget))
        {
            _mouseCaptureWidget = null;
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
        Tig.Mouse.GetState(out var state);

        var args = new MessageMouseArgs
        {
            X = state.x,
            Y = state.y,
            flags = MouseEventFlag.PosChange|MouseEventFlag.PosChangeSlow
        };
        TranslateMouseMessage(args);
    }

    public WidgetBase GetMouseCaptureWidget()
    {
        return _mouseCaptureWidget;
    }

    [TempleDllLocation(0x101f9830)]
    public bool SetMouseCaptureWidget(WidgetBase widget)
    {
        if (_mouseCaptureWidget == null)
        {
            _mouseCaptureWidget = widget;
            return true;
        }

        return false;
    }

    [TempleDllLocation(0x101f9850)]
    public void UnsetMouseCaptureWidget(WidgetBase widget)
    {
        if (_mouseCaptureWidget == widget)
        {
            _mouseCaptureWidget = null;
        }
    }

    /*
    This will sort the windows using their z-order in the order in which
    they should be rendered.
    */
    private void SortWindows()
    {
        // Sort Windows by Z-Index
        _topLevelWidgets.Sort((windowA, windowB) =>
        {
            return windowA.ZIndex.CompareTo(windowB.ZIndex);
        });

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
            if (widAtCursor != null && Tig.Mouse.CursorDrawCallback == _renderTooltipCallback)
            {
                Tig.Mouse.SetCursorDrawCallback(null, 0);
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
            && Tig.Mouse.CursorDrawCallback == null)
        {
            Tig.Mouse.SetCursorDrawCallback(_renderTooltipCallback);
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
        // Dispatch time update messages continuously to all advanced widgets
        if (msg.type == MessageType.UPDATE_TIME)
        {
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

            if (window == null || !window.Visible || !DoesWidgetContain(window, mouseArgs.X, mouseArgs.Y))
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
            if (window.Visible && window.HandleMessage(msg))
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
                Tig.Mouse.IsMouseOutsideWindow = false;
                break;
            case WindowEventType.MouseLeave:
                Tig.Mouse.IsMouseOutsideWindow = true;
                break;
            case WindowEventType.MouseMove:
                HandleMouseMoveEvent((MouseWindowEvent) e);
                break;
            case WindowEventType.MouseDown:
            case WindowEventType.MouseUp:
                HandleMouseButtonEvent((MouseWindowEvent) e);
                break;
            case WindowEventType.Wheel:
                HandleMouseWheelEvent((MouseWheelWindowEvent) e);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleMouseMoveEvent(MouseWindowEvent evt)
    {
        Tig.Mouse.SetPos((int) evt.UiPos.X, (int) evt.UiPos.Y, 0);
    }

    private void HandleMouseButtonEvent(MouseWindowEvent evt)
    {
        // Otherwise we might not get the actual position that was clicked if we use the last mouse move position..
        if (evt.Type == WindowEventType.MouseDown)
        {
            HandleMouseMoveEvent(evt);
        }

        Tig.Mouse.SetButtonState(evt.Button, evt.Type == WindowEventType.MouseDown);
    }

    private void HandleMouseWheelEvent(MouseWheelWindowEvent evt)
    {
        Tig.Mouse.SetPos((int) evt.UiPos.X, (int) evt.UiPos.Y, (int) (evt.Delta * 120));
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
                    Tig.Mouse.IsMouseOutsideWindow = false;
                    // TODO: Switch all mouse positioning to floats
                    Tig.Mouse.SetPos((int) x, (int) y, wheelDelta);
                    return;
                }
                else
                {
                    Tig.Mouse.IsMouseOutsideWindow = true;
                }
            }
            else
            {
                Logger.Info("Mouse outside resized window: {0},{1}, wheel: {2}", x, y, wheelDelta);
            }

            return;
        }

        Tig.Mouse.IsMouseOutsideWindow = false;
        // TODO: Switch all mouse positioning to floats
        Tig.Mouse.SetPos((int) pos.X, (int) pos.Y, wheelDelta);
    }

}