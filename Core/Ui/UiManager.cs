using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui;

public class UiManager : IUiRoot
{
    private static readonly TimeSpan TooltipDelay = TimeSpan.FromMilliseconds(250);

    private readonly List<WidgetBase> _topLevelWidgets = new();

    // Is the mouse currently in the main window?
    private bool _mouseOverUi;

    //  Last reported mouse position
    private PointF _mousePos = PointF.Empty;

    private TimePoint _mousePosLastChanged = TimePoint.Now;

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

    public IReadOnlyList<WidgetBase> TopLevelWidgets => _topLevelWidgets;

    public UiManagerDebug Debug { get; }

    [TempleDllLocation(0x11E74384)]
    public WidgetBase? MouseCaptureWidget { get; private set; }

    [TempleDllLocation(0x11E74384)]
    private WidgetBase? _pendingMouseCaptureWidget;

    [TempleDllLocation(0x10301324)]
    public WidgetBase? CurrentMouseOverWidget { get; private set; }

    // Hang on to the delegate
    private readonly CursorDrawCallback _renderTooltipCallback;

    private WidgetBase? _renderingTooltipFor;

    [TempleDllLocation(0x10EF97C4)]
    [TempleDllLocation(0x101f97d0)]
    [TempleDllLocation(0x101f97e0)]
    public bool IsDragging => DraggedObject != null;

    /// <summary>
    /// Represents an arbitrary object that is currently dragged by the user with their mouse.
    /// </summary>
    public object? DraggedObject { get; set; }

    public WidgetBase Modal { get; set; }

    public PointF MousePos => _mousePos;

    /// <summary>
    /// The widget that is the keyboard focus.
    /// </summary>
    public WidgetBase? KeyboardFocus => _keyboardFocusManager.KeyboardFocus;

    private readonly KeyboardFocusManager _keyboardFocusManager;

    private readonly IMainWindow _mainWindow;

    public UiManager(IMainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _mainWindow.UiRoot = this;
        _renderTooltipCallback = RenderTooltip;
        Debug = new UiManagerDebug(this);
        mainWindow.UiCanvasSizeChanged += () => OnCanvasSizeChanged?.Invoke(CanvasSize);
        _keyboardFocusManager = new KeyboardFocusManager(_topLevelWidgets);
    }

    /// <summary>
    /// Add something to the list of active windows on top of all existing windows.
    /// </summary>
    public void AddWindow(WidgetBase widget)
    {
        if (_topLevelWidgets.Contains(widget))
        {
            // Window is already in the list
            return;
        }

        if (widget.Parent != null)
        {
            throw new ArgumentException("Cannot show a parented widget as a window.");
        }

        if (widget.UiManager != null)
        {
            throw new ArgumentException("Widget already belongs to a different UI manager.");
        }

        _topLevelWidgets.Add(widget);
        widget.AttachToTree(this);
    }

    public bool RemoveWindow(WidgetContainer window)
    {
        if (Modal == window)
        {
            Modal = null;
        }

        if (_topLevelWidgets.Remove(window))
        {
            window.AttachToTree(null);
            return true;
        }
        else
        {
            return false;
        }
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

    public void ShowModal(WidgetBase widget, bool centerOnScreen = true)
    {
        AddWindow(widget);
        Modal = widget;
        widget.BringToFront();
        if (centerOnScreen)
        {
            widget.CenterOnScreen();
        }
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

        Tig.Mouse.DrawTooltip(_mousePos);
        Tig.Mouse.DrawItemUnderCursor(_mousePos);
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
        _topLevelWidgets.Sort((windowA, windowB) => windowA.ZIndex.CompareTo(windowB.ZIndex));

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
                return TopLevelWidgets.Contains(widget);
            }
            else
            {
                widget = widget.Parent;
            }
        }
    }

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
            _mousePosLastChanged = TimePoint.Now;
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

        if (CurrentMouseOverWidget != null)
        {
            var e = CreateMouseEvent(UiEventType.MouseMove, CurrentMouseOverWidget);
            CurrentMouseOverWidget.DispatchMouseMove(e);
        }
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
            node.DispatchMouseEnter(e);
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
            SetPressed(dispatchTo, true);

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

        // Unset the Pressed visual state
        if (lastMouseDownAt != null)
        {
            SetPressed(lastMouseDownAt, false);
        }

        var target = GetWidgetAt(uiPos.X, uiPos.Y);

        var dispatchTo = MouseCaptureWidget ?? target;
        if (dispatchTo == null)
        {
            // Release the mouse down state here so that the mouse up event cannot recapture the mouse
            _mouseDownState = null;
            // Implicitly release mouse capture
            _pendingMouseCaptureWidget = null;
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

    private static void SetPressed(WidgetBase widget, bool value)
    {
        foreach (var node in widget.EnumerateSelfAndAncestors())
        {
            node.Pressed = value;
        }
    }

    public void MouseWheel(Point windowPos, PointF uiPos, float units)
    {
        UpdateMousePosition(uiPos);

        var target = MouseCaptureWidget ?? CurrentMouseOverWidget;
        if (target != null)
        {
            // Old code assumes the wheel units to be windows wheel units, which usually are 120 * increments of the wheel
            var e = CreateWheelEvent(UiEventType.MouseWheel, target, 0, units * 120.0f, 0);
            target.DispatchMouseWheel(e);
        }
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

    private WheelEvent CreateWheelEvent(UiEventType type, WidgetBase target, float deltaX, float deltaY, float deltaZ)
    {
        return new WheelEvent
        {
            Type = type,
            InitialTarget = target,
            MouseOverWidget = GetWidgetAt(_mousePos.X, _mousePos.Y),
            Button = MouseButton.Unchanged,
            Buttons = _mouseDownState?.Buttons ?? default,
            X = _mousePos.X,
            Y = _mousePos.Y,
            IsAltHeld = Tig.Keyboard.IsAltPressed,
            IsCtrlHeld = Tig.Keyboard.IsCtrlPressed,
            IsShiftHeld = Tig.Keyboard.IsShiftPressed,
            IsMetaHeld = Tig.Keyboard.IsMetaHeld,
            DeltaX = deltaX,
            DeltaY = deltaY,
            DeltaZ = deltaZ,
        };
    }

    public void KeyDown(SDL_Keycode virtualKey, SDL_Scancode physicalKey, KeyModifier modifiers, bool repeat)
    {
        if (virtualKey == SDL_Keycode.SDLK_TAB)
        {
            _keyboardFocusManager.MoveFocusByKeyboard((modifiers & KeyModifier.Shift) != 0);
        }
    }

    public void Tick()
    {
        // handle any queued tasks
        ProcessPendingMouseCapture();

        var now = TimePoint.Now;
        if (Tig.Mouse.CursorDrawCallback == null && now - _mousePosLastChanged >= TooltipDelay)
        {
            Tig.Mouse.SetCursorDrawCallback(_renderTooltipCallback);
            _renderingTooltipFor = CurrentMouseOverWidget;
        }
        // If the mouse over widget changes, reset tooltip
        else if (_renderingTooltipFor != null
                 && _renderingTooltipFor != CurrentMouseOverWidget
                 && Tig.Mouse.CursorDrawCallback == _renderTooltipCallback)
        {
            Tig.Mouse.SetCursorDrawCallback(null);
            _renderingTooltipFor = null;
        }

        // Make a local copy of the widget list, because it could be modified by the event handlers
        var tempWidgets = ArrayPool<WidgetBase>.Shared.Rent(_topLevelWidgets.Count);
        try
        {
            _topLevelWidgets.CopyTo(tempWidgets);
            var widgets = tempWidgets.AsSpan(0, _topLevelWidgets.Count);

            // Dispatch time update messages continuously to all advanced widgets
            foreach (var entry in widgets)
            {
                if (entry.UiManager == this)
                {
                    entry.OnUpdateTime(now);
                }
            }
        }
        finally
        {
            ArrayPool<WidgetBase>.Shared.Return(tempWidgets);
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
}