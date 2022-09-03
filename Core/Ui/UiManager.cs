using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui;

public class UiManager : IUiRoot
{
    // Tracks the state of the first button that caused a mouse-down event,
    // which has not been released yet.
    private class MouseDownState
    {
        // The first button that was pressed
        public MouseButton Button { get; init; }

        // The element it was pressed on
        public WidgetBase? Target { get; init; }

        // The UI position it was pressed at
        public PointF Pos { get; init; }

        // A bit-field with the buttons that are held, which might change
        // between the mouse down and mouse up event. Buttons pressed
        // after the first but before the first is released do not trigger
        // mouse down / up events.
        public MouseButtons Buttons { get; set; }
    }

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly List<WidgetBase> _topLevelWidgets = new();
    private readonly List<WidgetBase> _widgetsTemp = new();

    // Is the mouse currently in the main window?
    private bool _mouseOverUi = false;

    //  Last reported mouse position
    private PointF _mousePos = PointF.Empty;

    /// <summary>
    /// The size in "virtual pixels" of the UI canvas.
    /// </summary>
    public Size CanvasSize => new(
        (int) _mainWindow.UiCanvasSize.Width,
        (int) _mainWindow.UiCanvasSize.Height
    );

    /// <summary>
    /// The last widget to receive a mouse down event (separate for each button).
    /// </summary>
    private MouseDownState? _mouseDownState;

    public event Action<Size> OnCanvasSizeChanged;

    public IEnumerable<WidgetBase> ActiveWindows => _topLevelWidgets;

    public UiManagerDebug Debug { get; }

    [TempleDllLocation(0x11E74384)]
    public WidgetBase? MouseCaptureWidget { get; private set; }

    [TempleDllLocation(0x11E74384)]
    private WidgetBase? _pendingMouseCaptureWidget;

    [TempleDllLocation(0x10301324)]
    public WidgetBase? CurrentMouseOverWidget { get; private set; }

    [TempleDllLocation(0x10301328)]
    private WidgetBase mMouseButtonId; // TODO = temple.GetRef<int>(0x10301328);

    // Hang on to the delegate
    private readonly CursorDrawCallback _renderTooltipCallback;

    [TempleDllLocation(0x10EF97C4)]
    [TempleDllLocation(0x101f97d0)]
    [TempleDllLocation(0x101f97e0)]
    public bool IsDragging => DraggedObject != null;

    /// <summary>
    /// Represents an arbitrary object that is currently dragged by the user with their mouse.
    /// </summary>
    public object? DraggedObject { get; set; }

    public WidgetContainer Modal { get; set; }

    public PointF MousePos => _mousePos;

    private readonly IMainWindow _mainWindow;

    public UiManager(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _mainWindow.UiRoot = this;
        _renderTooltipCallback = RenderTooltip;
        Debug = new UiManagerDebug(this);
        mainWindow.UiCanvasSizeChanged += () => OnCanvasSizeChanged?.Invoke(CanvasSize);
    }

    /// <summary>
    /// Add something to the list of active windows on top of all existing windows.
    /// </summary>
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
        window.AttachToTree(this);
    }

    public void RemoveWindow(WidgetContainer window)
    {
        _topLevelWidgets.Remove(window);
        SortWindows();
    }

    public void BringToFront(WidgetContainer window)
    {
        window.ZIndex = _topLevelWidgets
            .Where(widget => widget != window)
            .Select(widget => widget.ZIndex)
            .DefaultIfEmpty()
            .Max() + 1;
        SortWindows();
    }

    public void SendToBack(WidgetContainer window)
    {
        window.ZIndex = int.MinValue;
        SortWindows();
    }

    public void SetVisible(WidgetBase widget, bool visible)
    {
        if (widget is WidgetContainer container && container.Parent == null)
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

            widget = widget.Parent;
        }

        return false;
    }

    public void RemoveWidget(WidgetBase widget)
    {
        if (widget is WidgetContainer container)
        {
            RemoveWindow(container);
        }

        var refreshMouseOverState = false;
        
        // Invalidate any fields that may still hold a reference to the now invalid widget id
        if (IsAncestor(mMouseButtonId, widget))
        {
            mMouseButtonId = null;
            refreshMouseOverState = true;
        }

        if (IsAncestor(MouseCaptureWidget, widget))
        {
            ReleaseMouseCapture(MouseCaptureWidget);
            refreshMouseOverState = true;
        }

        if (IsAncestor(CurrentMouseOverWidget, widget))
        {
            refreshMouseOverState = true;
        }

        if (refreshMouseOverState)
        {
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

    public WidgetBase? GetWidgetAt(float x, float y)
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

                var localX = x - window.X;
                var localY = y - window.Y;

                var widgetIn = window.PickWidget(localX, localY);
                if (widgetIn != null)
                {
                    return widgetIn;
                }
            }
        }

        return result;
    }

    private static bool DoesWidgetContain(WidgetBase widget, float x, float y)
    {
        var rect = widget.GetContentArea();
        return x >= rect.X
               && y >= rect.Y
               && x < rect.X + rect.Width
               && y < rect.Y + rect.Height;
    }

    /// <summary>
    /// Uses the current mouse position to refresh which widget is being moused over.
    /// Useful if a widget is hidden, shown or added to update the mouse-over state
    /// without actually moving the mouse.
    /// </summary>
    public void RefreshMouseOverState()
    {
        UpdateMouseOver();
    }

    [TempleDllLocation(0x101f9830)]
    public bool CaptureMouse(WidgetBase widget)
    {
        if (widget.UiManager != this)
        {
            throw new ArgumentException("Widget cannot capture pointer because it's not part of the UI tree.");
        }

        // Capturing is only possible while a button is held
        if (_mouseDownState == null)
        {
            return false;
        }

        _pendingMouseCaptureWidget = widget;
        return true;
    }

    [TempleDllLocation(0x101f9850)]
    public void ReleaseMouseCapture(WidgetBase? widget)
    {
        if (_pendingMouseCaptureWidget == widget)
        {
            _pendingMouseCaptureWidget = null;
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
        CurrentMouseOverWidget?.RenderTooltip(x, y);
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

        var globalWid = CurrentMouseOverWidget;

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
                    // if (prevHoveredWindow.MouseState == LgcyWindowMouseState.Pressed)
                    // {
                    //     prevHoveredWindow.MouseState = LgcyWindowMouseState.PressedOutside;
                    // }
                    // else if (prevHoveredWindow.MouseState != LgcyWindowMouseState.PressedOutside)
                    // {
                    //     prevHoveredWindow.MouseState = LgcyWindowMouseState.Outside;
                    // }
                }
                // button
                else if (globalWid is WidgetButtonBase buttonWid && !buttonWid.Disabled)
                {
                    // switch (buttonWid.ButtonState)
                    // {
                    //     case LgcyButtonState.Hovered:
                    //         // Unhover
                    //         buttonWid.ButtonState = LgcyButtonState.Normal;
                    //         Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOff);
                    //         break;
                    //     case LgcyButtonState.Down:
                    //         // Down . Released without click event
                    //         buttonWid.ButtonState = LgcyButtonState.Released;
                    //         break;
                    // }
                }

                if (IsVisible(globalWid))
                {
                    newTigMsg.widgetId = globalWid;
                    newTigMsg.widgetEventType = TigMsgWidgetEvent.Exited;
                    // TODO Tig.MessageQueue.Enqueue(new Message(newTigMsg));
                }
            }

            if (widAtCursor != null)
            {
                // if (widAtCursor is WidgetContainer widAtCursorWindow)
                // {
                //     if (widAtCursorWindow.MouseState == LgcyWindowMouseState.PressedOutside)
                //     {
                //         widAtCursorWindow.MouseState = LgcyWindowMouseState.Pressed;
                //     }
                //     else if (widAtCursorWindow.MouseState != LgcyWindowMouseState.Pressed)
                //     {
                //         widAtCursorWindow.MouseState = LgcyWindowMouseState.Hovered;
                //     }
                // }
                // else if (widAtCursor is WidgetButtonBase buttonWid && !buttonWid.Disabled)
                // {
                //     if (buttonWid.ButtonState != LgcyButtonState.Normal)
                //     {
                //         if (buttonWid.ButtonState == LgcyButtonState.Released)
                //         {
                //             buttonWid.ButtonState = LgcyButtonState.Down;
                //         }
                //     }
                //     else
                //     {
                //         buttonWid.ButtonState = LgcyButtonState.Hovered;
                //         Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOn);
                //     }
                // }

                newTigMsg.widgetId = widAtCursor;
                newTigMsg.widgetEventType = TigMsgWidgetEvent.Entered;
                // TODO Tig.MessageQueue.Enqueue(new Message(newTigMsg));
            }

            globalWid = CurrentMouseOverWidget = widAtCursor;
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
                if (widIdAtCursor2 is WidgetButtonBase button && !button.Disabled)
                {
                    // switch (button.ButtonState)
                    // {
                    //     case LgcyButtonState.Hovered:
                    //         button.ButtonState = LgcyButtonState.Down;
                    //         Tig.Sound.PlaySoundEffect(button.sndDown);
                    //         break;
                    //     case LgcyButtonState.Disabled:
                    //         return false;
                    // }
                }

                newTigMsg.widgetEventType = TigMsgWidgetEvent.Clicked;
                newTigMsg.widgetId = widIdAtCursor2;
                mMouseButtonId = widIdAtCursor2;
                // TODO Tig.MessageQueue.Enqueue(new Message(newTigMsg));
            }
        }

        if ((mouseMsg.flags & MouseEventFlag.LeftReleased) != 0 && mMouseButtonId != null)
        {
            if (mMouseButtonId is WidgetButtonBase button && !button.Disabled)
            {
                // switch (button.ButtonState)
                // {
                //     case LgcyButtonState.Down:
                //         button.ButtonState = LgcyButtonState.Hovered;
                //         Tig.Sound.PlaySoundEffect(button.sndClick);
                //         break;
                //     case LgcyButtonState.Released:
                //         button.ButtonState = LgcyButtonState.Normal;
                //         Tig.Sound.PlaySoundEffect(button.sndClick);
                //         break;
                //     case LgcyButtonState.Disabled:
                //         return false;
                // }
            }

            // probably redundant to do again, but just to be safe...
            var widIdAtCursor2 = GetWidgetAt(mouseMsg.X, mouseMsg.Y);
            newTigMsg.widgetId = mMouseButtonId;
            newTigMsg.widgetEventType = (widIdAtCursor2 != mMouseButtonId)
                ? TigMsgWidgetEvent.MouseReleasedAtDifferentButton
                : TigMsgWidgetEvent.MouseReleased;
            // TODO Tig.MessageQueue.Enqueue(new Message(newTigMsg));
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

            if (widget.Parent == null)
            {
                // It must be a top-level window to be visible
                return ActiveWindows.Contains(widget);
            }
            else
            {
                widget = widget.Parent;
            }
        }
    }

    // [TempleDllLocation(0x101f8a80)]
    // public bool ProcessMessage(Message msg)
    // {
    //     Globals.UiManager.TranslateMouseMessage(msg);
    //
    //     switch (msg.type)
    //     {
    //         case MessageType.MOUSE:
    //             return ProcessMouseMessage(msg);
    //         case MessageType.WIDGET:
    //             return ProcessWidgetMessage(msg);
    //         default:
    //             // In order from top to bottom (back is top)
    //             for (var i = _topLevelWidgets.Count - 1; i >= 0; i--)
    //             {
    //                 var window = _topLevelWidgets[i];
    //
    //                 // Skip the top-level window if there's a modal and the modal is a different window
    //                 if (Modal != null && Modal != window)
    //                 {
    //                     continue;
    //                 }
    //
    //                 if (window.HandleMessage(msg))
    //                 {
    //                     return true;
    //                 }
    //             }
    //
    //             return false;
    //     }
    // }

    // private bool ProcessWidgetMessage(Message msg)
    // {
    //     var widgetArgs = msg.WidgetArgs;
    //
    //     var dispatchTo = widgetArgs.widgetId;
    //     while (dispatchTo != null)
    //     {
    //         var parent = dispatchTo.GetParent();
    //         if ((parent == null || parent.Visible) && dispatchTo.Visible)
    //         {
    //             if (dispatchTo.HandleMessage(msg))
    //             {
    //                 return true;
    //             }
    //         }
    //
    //         // Bubble up the msg if the widget didn't handle it
    //         dispatchTo = dispatchTo.GetParent();
    //     }
    //
    //     return false;
    // }

    /// <summary>
    /// Works like Document.elementsFromPoint and enumerates all elements that intersect the given point on screen,
    /// from topmost to bottommost.
    /// </summary>
    public IEnumerable<WidgetBase> ElementsFromPoint(int x, int y)
    {
        foreach (var widget in _topLevelWidgets)
        {
            foreach (var el in ElementsFromPoint(widget, x, y))
            {
                yield return el;
            }
        }
    }

    private static IEnumerable<WidgetBase> ElementsFromPoint(WidgetBase context, int x, int y)
    {
        if (!context.Visible || !context.HitTest(x, y))
        {
            yield break;
        }

        if (context is WidgetContainer container)
        {
            foreach (var child in container.GetChildren())
            {
                foreach (var widgetBase in ElementsFromPoint(child, x, y))
                {
                    yield return widgetBase;
                }
            }
        }

        yield return context;
    }

    // private bool ProcessMouseMessage(Message msg)
    // {
    //     // Handle if a widget requested mouse capture
    //     if (MouseCaptureWidget != null)
    //     {
    //         MouseCaptureWidget.HandleMessage(msg);
    //         return true;
    //     }
    //
    //     var mouseArgs = msg.MouseArgs;
    //
    //     for (var i = _topLevelWidgets.Count - 1; i >= 0; i--)
    //     {
    //         var window = _topLevelWidgets[i];
    //
    //         // Skip the top-level window if there's a modal and the modal is a different window
    //         if (Modal != null && Modal != window)
    //         {
    //             continue;
    //         }
    //
    //         if (window == null || !window.Visible || !DoesWidgetContain(window, mouseArgs.X, mouseArgs.Y))
    //         {
    //             continue;
    //         }
    //
    //         // Try dispatching the msg to all children of the window that are also under the mouse cursor, in reverse order of their
    //         // own insertion into the children list
    //         for (var j = window.GetChildren().Count - 1; j >= 0; j--)
    //         {
    //             var childWidget = window.GetChildren()[j];
    //
    //             if (DoesWidgetContain(childWidget, mouseArgs.X, mouseArgs.Y))
    //             {
    //                 if (childWidget.Visible && childWidget.HandleMouseMessage(mouseArgs))
    //                 {
    //                     return true;
    //                 }
    //             }
    //         }
    //
    //         // After checking with all children, dispatch the msg to the window itself
    //         if (window.Visible && window.HandleMessage(msg))
    //         {
    //             return true;
    //         }
    //     }
    //
    //     if (Modal != null)
    //     {
    //         // Always swallow mouse messages when a modal is visible
    //         return true;
    //     }
    //
    //     return false;
    // }

    public void MouseEnter()
    {
        ProcessPendingMouseCapture();
        Tig.Mouse.IsMouseOutsideWindow = false;

        _mouseOverUi = true;
        UpdateMouseOver();
    }

    public void MouseLeave()
    {
        ProcessPendingMouseCapture();
        Tig.Mouse.IsMouseOutsideWindow = true;

        _mouseOverUi = false;
        UpdateMouseOver();
    }

    private void UpdateMousePosition(PointF uiPos)
    {
        if (_mousePos != uiPos)
        {
            _mousePos = uiPos;
            UpdateMouseOver();
        }
    }

    private void UpdateMouseOver()
    {
        ProcessPendingMouseCapture();
        
        WidgetBase? mouseOver;
        if (MouseCaptureWidget != null)
        {
            // Force the mouse-over widget to be the capture target
            mouseOver = MouseCaptureWidget;
        }
        else
        {
            mouseOver = _mouseOverUi ? GetWidgetAt(_mousePos.X, _mousePos.Y) : null;
        }

        var prevMouseOver = CurrentMouseOverWidget;

        if (mouseOver == prevMouseOver)
        {
            return; // Current mouse over widget has not changed
        }

        if (mouseOver != null && Tig.Mouse.CursorDrawCallback == _renderTooltipCallback)
        {
            Tig.Mouse.SetCursorDrawCallback(null, 0);
        }

        // When mouse over changes from one widget to another, we only need to propagate
        // this change up until the first common ancestor of both.
        WidgetBase? commonAncestor = null;
        if (prevMouseOver != null && mouseOver != null)
        {
            commonAncestor = prevMouseOver.GetCommonAncestor(mouseOver);
        }

        if (prevMouseOver != null)
        {
            DispatchMouseLeave(prevMouseOver, commonAncestor);
        }

        if (mouseOver != null)
        {
            DispatchMouseEnter(mouseOver, commonAncestor);
        }

        // Set the ContainsMouse state until we reach a potential common ancestor
        for (var node = mouseOver; node != null && node != commonAncestor; node = node.Parent)
        {
            node.ContainsMouse = true;
        }

        // Unset the ContainsMouse state until we reach a potential common ancestor
        for (var node = prevMouseOver; node != null && node != commonAncestor; node = node.Parent)
        {
            node.ContainsMouse = false;
        }

        CurrentMouseOverWidget = mouseOver;
    }

    public void MouseMove(Point windowPos, PointF uiPos)
    {
        ProcessPendingMouseCapture();
        UpdateMousePosition(uiPos);
    }

    private void DispatchMouseLeave(WidgetBase target, WidgetBase? stopAtParent)
    {
        for (var node = target; node != null && node != stopAtParent; node = node.Parent)
        {
            var e = new MouseEvent {InitialTarget = target};
            node.DispatchMouseLeave(e);
        }
    }

    private void DispatchMouseEnter(WidgetBase target, WidgetBase? stopAtParent)
    {
        for (var node = target; node != null && node != stopAtParent; node = node.Parent)
        {
            var e = new MouseEvent {InitialTarget = target};
            node.DispatchMouseLeave(e);
        }
    }

    public void MouseDown(Point windowPos, PointF uiPos, MouseButton button)
    {
        ProcessPendingMouseCapture();
        UpdateMousePosition(uiPos);

        if (_mouseDownState != null)
        {
            // We do not dispatch additional mouse down events if a button is already held
            _mouseDownState.Buttons |= GetMouseButtons(button);
            return;
        }

        // This should not really happen since capturing the mouse should only be allowed
        // if the mouse was already down, and it should be released, when the mouse button
        // is released. We'll handle it anyway.
        var dispatchTo = MouseCaptureWidget ?? GetWidgetAt(uiPos.X, uiPos.Y);

        _mouseDownState = new MouseDownState
        {
            Button = button,
            Buttons = GetMouseButtons(button),
            Pos = uiPos,
            Target = dispatchTo
        };

        if (dispatchTo != null)
        {
            var e = CreateMouseEvent(UiEventType.MouseDown, dispatchTo);
            dispatchTo.DispatchMouseDown(e);
        }
    }

    public void MouseUp(Point windowPos, PointF uiPos, MouseButton button)
    {
        ProcessPendingMouseCapture();
        UpdateMousePosition(uiPos);

        if (_mouseDownState == null)
        {
            // Ignore spurious mouse up event. Might happen if a secondary button is released 
            // after the initial mouse-down button was already released.
            return;
        }
        
        // Only emit a mouse up event if the initially pressed button is released
        if (_mouseDownState.Button != button)
        {
            _mouseDownState.Buttons &= ~GetMouseButtons(button);
            return;
        }

        var lastMouseDownAt = _mouseDownState.Target;

        var target = GetWidgetAt(uiPos.X, uiPos.Y);

        var dispatchTo = MouseCaptureWidget ?? target;
        if (dispatchTo == null)
        {
            return;
        }
        
        var e = CreateMouseEvent(UiEventType.MouseUp, dispatchTo);
        
        // Release the mouse down state here so that the mouse up event cannot recapture the mouse
        _mouseDownState = null;
        // Implicitly release mouse capture
        _pendingMouseCaptureWidget = null;
        
        dispatchTo.DispatchMouseUp(e);

        // We emit click events on the first common ancestor of the mouse-down target
        // and the mouse-up target.
        if (!e.IsDefaultPrevented && lastMouseDownAt != null)
        {
            var clickTarget = lastMouseDownAt.GetCommonAncestor(dispatchTo);
            if (clickTarget != null)
            {
                if (button == MouseButton.LEFT)
                {
                    clickTarget.DispatchClick(e);
                }
                else
                {
                    clickTarget.DispatchOtherClick(e);
                }
            }
        }

        ProcessPendingMouseCapture();
    }

    public void MouseWheel(Point windowPos, PointF uiPos, float units)
    {
        // TODO (int) (units * 120);
    }

    private MouseEvent CreateMouseEvent(UiEventType type, WidgetBase target)
    {
        var button = MouseButton.Unchanged;
        MouseButtons buttons = default;
        if (_mouseDownState != null)
        {
            // Only on mouse down do we actually get the button set 
            if (type is UiEventType.MouseDown or UiEventType.MouseUp)
            {
                button = _mouseDownState.Button;
            }

            buttons = _mouseDownState.Buttons;
        }

        return new MouseEvent
        {
            Type = type,
            InitialTarget = target,
            MouseOverWidget = GetWidgetAt(_mousePos.X, _mousePos.Y),
            Button = button,
            Buttons = buttons,
            X = _mousePos.X,
            Y = _mousePos.Y,
            IsAltHeld = Tig.Keyboard.IsAltPressed,
            IsCtrlHeld = Tig.Keyboard.IsCtrlPressed,
            IsShiftHeld = Tig.Keyboard.IsShiftPressed,
            IsMetaHeld = Tig.Keyboard.IsMetaHeld
        };
    }

    public void Tick()
    {
        // handle any queued tasks
        ProcessPendingMouseCapture();

        var now = TimePoint.Now;

        // Dispatch time update messages continuously to all advanced widgets
        _widgetsTemp.Clear();
        _widgetsTemp.AddRange(_topLevelWidgets);
        foreach (var entry in _widgetsTemp)
        {
            if (entry.UiManager == this)
            {
                entry.OnUpdateTime(now);
            }
        }
    }

    // See https://w3c.github.io/pointerevents/#process-pending-pointer-capture
    private void ProcessPendingMouseCapture()
    {
        if (_pendingMouseCaptureWidget != MouseCaptureWidget && MouseCaptureWidget != null)
        {
            MouseCaptureWidget.DispatchLostMouseCapture(CreateMouseEvent(UiEventType.LostMouseCapture, MouseCaptureWidget));
        }

        if (_pendingMouseCaptureWidget != MouseCaptureWidget && _pendingMouseCaptureWidget != null)
        {
            _pendingMouseCaptureWidget.DispatchGotMouseCapture(CreateMouseEvent(UiEventType.GotMouseCapture, _pendingMouseCaptureWidget));
        }

        if (MouseCaptureWidget != _pendingMouseCaptureWidget)
        {
            MouseCaptureWidget = _pendingMouseCaptureWidget;
            UpdateMouseOver();
        }
    }
    
    private static MouseButtons GetMouseButtons(MouseButton button)
    {
        return button switch
        {
            MouseButton.LEFT => MouseButtons.Left,
            MouseButton.RIGHT => MouseButtons.Right,
            MouseButton.MIDDLE => MouseButtons.Middle,
            MouseButton.EXTRA1 => MouseButtons.Extra1,
            MouseButton.EXTRA2 => MouseButtons.Extra2,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

}