using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Cursors;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui;

public class UiManager : IUiRoot
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly TimeSpan TooltipDelay = TimeSpan.FromMilliseconds(250);

    public RootWidget Root { get; }

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
    /// Tracking information about the last mouse down event to facilitate click events.
    /// </summary>
    private MouseDownState? _mouseDownState;

    /// <summary>
    /// Tracking information about the last click event to facilitate double click events.
    /// </summary>
    private ClickState? _clickState;

    public event Action<Size>? OnCanvasSizeChanged;

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

    public WidgetBase? Modal { get; set; }

    public PointF MousePos => _mousePos;

    /// <summary>
    /// The widget that is the keyboard focus.
    /// </summary>
    public WidgetBase? KeyboardFocus => _keyboardFocusManager.KeyboardFocus;

    private readonly KeyboardFocusManager _keyboardFocusManager;

    private readonly IMainWindow _mainWindow;

    private readonly WidgetTooltipRenderer _tooltipRenderer = new();

    private readonly CursorRegistry _cursorRegistry;

    private readonly List<ActiveActionHotkey> _actionHotkeys = new();
    private readonly List<(WidgetBase Owner, WidgetBase.HeldHotkeyState State)> _heldHotkeys = new();

    public IReadOnlyList<ActiveActionHotkey> ActiveHotkeys => _actionHotkeys;

    public UiManager(IMainWindow mainWindow, IFileSystem fs)
    {
        _mainWindow = mainWindow;
        _mainWindow.UiRoot = this;
        _renderTooltipCallback = RenderTooltip;
        Debug = new UiManagerDebug(this);
        mainWindow.UiCanvasSizeChanged += ResizeCanvas;

        Root = new RootWidget();
        Root.AttachToTree(this);
        Root.PixelSize = CanvasSize;
        _keyboardFocusManager = new KeyboardFocusManager(Root);

        _cursorRegistry = new CursorRegistry(fs);
    }

    private void ResizeCanvas()
    {
        // Resize the root element
        Root.PixelSize = CanvasSize;

        OnCanvasSizeChanged?.Invoke(CanvasSize);
    }

    /// <summary>
    /// Add something to the list of active windows on top of all existing windows.
    /// </summary>
    public void AddWindow(WidgetBase widget) => Root.Add(widget);

    public bool RemoveWindow(WidgetBase widget) => Root.Remove(widget);

    public void ShowModal(WidgetBase widget, bool centerOnScreen = true)
    {
        Root.Add(widget);
        Modal = widget;
        widget.BringToFront();
        if (centerOnScreen)
        {
            widget.CenterInParent();
        }

        // Move keyboard focus to the first focusable element in Modal
        if (KeyboardFocus != null)
        {
            _keyboardFocusManager.FocusFirstFocusableChild(Modal);
        }
    }

    public void OnAddedToTree(WidgetBase widget)
    {
        foreach (var hotkey in widget.ActionHotkeys)
        {
            _actionHotkeys.Add(new ActiveActionHotkey(
                hotkey.Hotkey,
                hotkey.Callback,
                hotkey.Condition,
                widget
            ));
        }

        foreach (var state in widget.HeldHotkeys)
        {
            _heldHotkeys.Add((widget, state));
        }
    }

    public void OnRemovedFromTree(WidgetBase widget)
    {
        _actionHotkeys.RemoveAll(h => ReferenceEquals(h.Owner, widget));
        for (var i = _heldHotkeys.Count - 1; i >= 0; i--)
        {
            var (owner, state) = _heldHotkeys[i];
            if (owner == widget)
            {
                if (state.Held)
                {
                    state.Held = false;
                    state.Callback(false);
                }

                _heldHotkeys.RemoveAt(i);
            }
        }

        var refreshMouseOverState = false;

        if (widget.IsInclusiveAncestorOf(MouseCaptureWidget))
        {
            ReleaseMouseCapture(MouseCaptureWidget);
            refreshMouseOverState = true;
        }

        if (widget.IsInclusiveAncestorOf(CurrentMouseOverWidget))
        {
            refreshMouseOverState = true;
        }

        if (refreshMouseOverState)
        {
            RefreshMouseOverState();
        }

        if (KeyboardFocus != null && widget.IsInclusiveAncestorOf(KeyboardFocus))
        {
            _keyboardFocusManager.Blur();
        }

        if (Modal != null && widget.IsInclusiveAncestorOf(Modal))
        {
            Modal = null;
        }
    }

    [TempleDllLocation(0x101F8D10)]
    public void Render()
    {
        UpdateCursor();
        
        Root.EnsureLayoutIsUpToDate();

        var context = new UiRenderContext(Tig.RenderingDevice, Tig.ShapeRenderer2d); // TODO CACHE
        Root.Render(context);

        Debug.AfterRenderWidgets();

        Tig.Mouse.DrawTooltip(_mousePos);
        Tig.Mouse.DrawItemUnderCursor(_mousePos);
    }

    private void UpdateCursor()
    {
        if (!_mouseOverUi)
        {
            return;
        }

        var visible = true;
        var cursor = CursorIds.Default;

        var target = MouseCaptureWidget ?? CurrentMouseOverWidget;

        if (target != null)
        {
            // Dispatch event to get the default cursor
            var e = CreateGetCursorEvent(UiEventType.GetCursor, target);

            target.DispatchGetCursor(e);

            visible = e.Visible;
            cursor = e.Cursor ?? cursor;
        }

        _mainWindow.IsCursorVisible = visible;
        if (visible)
        {
            if (_cursorRegistry.TryGetValue(cursor, out var cursorDefinition))
            {
                _mainWindow.SetCursor(cursorDefinition.HotspotX, cursorDefinition.HotspotY, cursorDefinition.TexturePath);
            }
            else
            {
                Logger.Error("Cursor {0} could not be found", cursor);
            }
        }
    }

    public WidgetBase? PickWidget(float x, float y)
    {
        return Modal != null ? Modal.PickWidgetGlobal(x, y) : Root.PickWidgetGlobal(x, y);
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

    /// <summary>
    /// Notifies the UI manager that the visibility of a widget has changed.
    /// </summary>
    public void VisibilityChanged(WidgetBase widget)
    {
        if (Modal == widget && !widget.Visible)
        {
            Modal = null;
        }

        // Handle the keyboard focus becoming invisible
        if (!widget.Visible && KeyboardFocus != null && widget.IsInclusiveAncestorOf(KeyboardFocus))
        {
            _keyboardFocusManager.Blur();
        }

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

    private void RenderTooltip(int x, int y, object userArg)
    {
        if (CurrentMouseOverWidget != null)
        {
            DrawTooltip(CurrentMouseOverWidget, x, y);
        }
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
            mouseOver = _mouseOverUi ? PickWidget(_mousePos.X, _mousePos.Y) : null;
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
        var dispatchTo = MouseCaptureWidget ?? PickWidget(uiPos.X, uiPos.Y);

        _mouseDownState = new MouseDownState
        {
            Timestamp = TimePoint.Now,
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

            if (e is {Button: MouseButton.Left, IsDefaultPrevented: false})
            {
                // Handle focus movement via mouse
                _keyboardFocusManager.MoveFocusByMouseDown(dispatchTo);
            }
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

        var target = PickWidget(uiPos.X, uiPos.Y);

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
        var lastMouseDownState = _mouseDownState;
        _mouseDownState = null;
        // Implicitly release mouse capture
        _pendingMouseCaptureWidget = null;

        dispatchTo.DispatchMouseUp(e);

        // We emit click events on the first common ancestor of the mouse-down target
        // and the mouse-up target.
        if (!e.IsDefaultPrevented && lastMouseDownAt != null)
        {
            var clickState = _clickState;
            _clickState = null; // Reset click state to avoid triple-clicks registering as a second double-clicks

            var clickTarget = lastMouseDownAt.GetCommonAncestor(dispatchTo);
            if (clickTarget != null)
            {
                if (button == MouseButton.Left)
                {
                    clickTarget.DispatchClick(CreateMouseEvent(UiEventType.Click, clickTarget));

                    // Check for a potential double-click
                    if (IsDoubleClick(clickState, lastMouseDownState, clickTarget))
                    {
                        clickTarget.DispatchDoubleClick(CreateMouseEvent(UiEventType.DoubleClick, clickTarget));
                    }
                    else
                    {
                        _clickState = new ClickState
                        {
                            MouseDownState = lastMouseDownState,
                            Target = clickTarget
                        };
                    }
                }
                else
                {
                    clickTarget.DispatchOtherClick(CreateMouseEvent(UiEventType.OtherClick, clickTarget));
                }
            }
        }

        ProcessPendingMouseCapture();
    }

    private bool IsDoubleClick(ClickState? clickState, MouseDownState mouseDownState, WidgetBase clickTarget)
    {
        if (clickState == null)
        {
            return false;
        }

        if (clickState.Target != clickTarget)
        {
            return false; // Click event targets a different element
        }

        // The deciding factor for emitting a double click event is the time between the mouse down events
        var elapsedMsSinceMouseDown = mouseDownState.Timestamp.Milliseconds - clickState.MouseDownState.Timestamp.Milliseconds;
        if (elapsedMsSinceMouseDown > 500)
        {
            return false; // Too much time between the mouse down events
        }

        // This is not quite correct. The mouse should not have moved outside of this rectangle, but we only
        // check the position at the times of the mouse down events.
        var firstMouseDownPos = clickState.MouseDownState.Pos;
        var mouseDownPos = mouseDownState.Pos;
        var distSq = MathF.Abs(firstMouseDownPos.X - mouseDownPos.X) + MathF.Abs(firstMouseDownPos.Y - mouseDownPos.Y);
        // We use 5 virtual pixels as the radius, but consider the user moves in physical pixels, convert it
        if (distSq > 5 * 5 * _mainWindow.UiScale * _mainWindow.UiScale)
        {
            return false;
        }

        return true;
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
            MouseOverWidget = PickWidget(_mousePos.X, _mousePos.Y),
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

    private GetCursorEvent CreateGetCursorEvent(UiEventType type, WidgetBase target)
    {
        MouseButtons buttons = default;
        if (_mouseDownState != null)
        {
            buttons = _mouseDownState.Buttons;
        }

        return new GetCursorEvent
        {
            Type = type,
            InitialTarget = target,
            MouseOverWidget = PickWidget(_mousePos.X, _mousePos.Y),
            Button = MouseButton.Unchanged,
            Buttons = buttons,
            X = _mousePos.X,
            Y = _mousePos.Y,
            IsAltHeld = Tig.Keyboard.IsAltPressed,
            IsCtrlHeld = Tig.Keyboard.IsCtrlPressed,
            IsShiftHeld = Tig.Keyboard.IsShiftPressed,
            IsMetaHeld = Tig.Keyboard.IsMetaHeld
        };
    }

    private TooltipEvent CreateTooltipEvent(UiEventType type, WidgetBase target)
    {
        MouseButtons buttons = default;
        if (_mouseDownState != null)
        {
            buttons = _mouseDownState.Buttons;
        }

        return new TooltipEvent
        {
            Type = type,
            InitialTarget = target,
            MouseOverWidget = PickWidget(_mousePos.X, _mousePos.Y),
            Button = MouseButton.Unchanged,
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
            MouseOverWidget = PickWidget(_mousePos.X, _mousePos.Y),
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
        var initialTarget = KeyboardFocus ?? Modal ?? Root;

        var e = CreateKeyboardEvent(UiEventType.KeyDown, initialTarget, virtualKey, physicalKey, modifiers, repeat);

        initialTarget.DispatchKeyDown(e);

        if (!e.IsDefaultPrevented && virtualKey == SDL_Keycode.SDLK_TAB)
        {
            _keyboardFocusManager.MoveFocusByKeyboard((modifiers & KeyModifier.Shift) != 0);
        }
        else
        {
            HandleHotkeyEvent(e);
        }
    }

    public void KeyUp(SDL_Keycode virtualKey, SDL_Scancode physicalKey, KeyModifier modifiers)
    {
        var initialTarget = KeyboardFocus ?? Modal ?? Root;

        var e = CreateKeyboardEvent(UiEventType.KeyUp, initialTarget, virtualKey, physicalKey, modifiers, false);

        initialTarget.DispatchKeyUp(e);

        HandleHotkeyEvent(e);

        // Clear any held hotkeys matching the key
        foreach (var (_, state) in _heldHotkeys)
        {
            if (state.Held && HotkeyMatchesEvent(state.Hotkey, e))
            {
                state.Held = false;
                state.Callback(false);
            }
        }
    }

    private bool HandleHotkeyEvent(KeyboardEvent e)
    {
        // If there's a focused element, we disable hotkeys
        // We also ignore hotkeys from keyboard events if the event has been handled in some way
        if (e.IsPropagationStopped || e.IsImmediatePropagationStopped || e.IsDefaultPrevented)
        {
            return false;
        }

        foreach (var activeHotkey in _actionHotkeys)
        {
            // If any widget has the keyboard focus, only process hotkeys defined by that widget or one of its children
            if (!e.InitialTarget.IsInclusiveAncestorOf(activeHotkey.Owner))
            {
                continue;
            }

            // "Held" hotkeys are handled differently
            if (activeHotkey.Hotkey.Trigger == HotkeyTrigger.Held)
            {
                continue;
            }

            if (!HotkeyMatchesEvent(activeHotkey.Hotkey, e) || !activeHotkey.ActiveCondition())
            {
                continue;
            }

            Logger.Debug("Triggering hotkey {0}", activeHotkey.Hotkey);
            activeHotkey.Trigger();
            return true; // Avoids triggering more than one
        }

        // Look for widgets reacting to "held hotkeys" if the hotkey was not handled by anything else
        if (e.Type == UiEventType.KeyDown)
        {
            foreach (var (_, state) in _heldHotkeys)
            {
                if (HotkeyMatchesEvent(state.Hotkey, e))
                {
                    state.Held = true;
                    state.Callback(true);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HotkeyMatchesEvent(Hotkey hotkey, KeyboardEvent e)
    {
        switch (hotkey.Trigger)
        {
            case HotkeyTrigger.Held:
                if (!(e is {Type: UiEventType.KeyDown, IsRepeat: false}
                      || e.Type == UiEventType.KeyUp))
                {
                    return false;
                }

                break;
            case HotkeyTrigger.KeyDown:
                if (e.Type != UiEventType.KeyDown || e.IsRepeat)
                {
                    return false;
                }

                break;
            case HotkeyTrigger.KeyUp:
                if (e.Type != UiEventType.KeyUp)
                {
                    return false;
                }

                break;
            case HotkeyTrigger.KeyDownAndRepeat:
                if (e.Type != UiEventType.KeyDown)
                {
                    return false;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return hotkey.Matches(e.VirtualKey, e.PhysicalKey, e.IsAltHeld, e.IsShiftHeld, e.IsCtrlHeld, e.IsMetaHeld);
    }

    private static KeyboardEvent CreateKeyboardEvent(UiEventType type, WidgetBase target, SDL_Keycode virtualKey, SDL_Scancode physicalKey, KeyModifier modifiers, bool repeat)
    {
        return new KeyboardEvent
        {
            Type = type,
            InitialTarget = target,
            VirtualKey = virtualKey,
            PhysicalKey = physicalKey,
            IsAltHeld = (modifiers & KeyModifier.Alt) != 0,
            IsCtrlHeld = (modifiers & KeyModifier.Ctrl) != 0,
            IsShiftHeld = (modifiers & KeyModifier.Shift) != 0,
            IsMetaHeld = (modifiers & KeyModifier.Meta) != 0,
            IsRepeat = repeat
        };
    }

    public void TextInput(string text)
    {
        var initialTarget = KeyboardFocus ?? Root;

        var e = new TextInputEvent
        {
            Type = UiEventType.TextInput,
            InitialTarget = initialTarget,
            Text = text
        };

        initialTarget.DispatchTextInput(e);
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

        Root.OnUpdateTime(now);
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
            MouseButton.Left => MouseButtons.Left,
            MouseButton.Right => MouseButtons.Right,
            MouseButton.Middle => MouseButtons.Middle,
            MouseButton.Extra1 => MouseButtons.Extra1,
            MouseButton.Extra2 => MouseButtons.Extra2,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }

    // Tracks the state of the last click that occured to facilitate double click tracking.
    private class ClickState
    {
        // A copy of the mouse down state that led to the click
        public MouseDownState MouseDownState { get; init; }

        // The element that received the click event
        public WidgetBase? Target { get; init; }
    }

    // Tracks the state of the first button that caused a mouse-down event,
    // which has not been released yet.
    private class MouseDownState
    {
        // When the event occured (using a monotonic clock)
        public TimePoint Timestamp { get; init; }

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

    public void Focus(WidgetBase widget)
    {
        _keyboardFocusManager.MoveFocus(widget);
    }

    public void SnapToPhysicalPixelGrid(ref RectangleF rect)
    {
        var scale = _mainWindow.UiScale;
        var right = rect.Right;
        var bottom = rect.Bottom;

        rect.X = MathF.Round(rect.X * scale) / scale;
        rect.Y = MathF.Round(rect.Y * scale) / scale;
        rect.Width = MathF.Round(right * scale) / scale - rect.X;
        rect.Height = MathF.Round(bottom * scale) / scale - rect.Y;
    }

    public void DrawTooltip(WidgetBase? widget, int x, int y)
    {
        if (widget == null)
        {
            return;
        }

        var e = CreateTooltipEvent(UiEventType.Tooltip, widget);

        widget.DispatchTooltip(e);

        if (!e.IsDefaultPrevented)
        {
            // TODO Introduce caching since this will be done every frame
            _tooltipRenderer.TooltipContent = e.Content;
            _tooltipRenderer.TooltipStyle = e.StyleId ?? WidgetTooltipRenderer.DefaultStyle;
            _tooltipRenderer.AlignLeft = e.AlignLeft;
            _tooltipRenderer.Render(x, y);
        }
    }
}