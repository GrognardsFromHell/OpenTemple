using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.DOM;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui
{
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
        private struct CapturingElementInfo
        {
            // capture should only be allowed during a mousedown event
            public Element mContent;
            public bool mAllowed;
            public bool mPointerLock;
            public bool mRetargetToElement;
            public bool mPreventDrag;
        }

        private CapturingElementInfo capturingElementInfo;

        private List<WidgetContainer> _topLevelWidgets = new List<WidgetContainer>();

        private int maxZIndex = 0;

        public Size ScreenSize => Tig.RenderingDevice.GetCamera().ScreenSize;

        public event Action<Size> OnScreenSizeChanged;

        public UiManagerDebug Debug { get; }

        public Document Document { get; }

        [TempleDllLocation(0x11E74384)]
        private WidgetBase mMouseCaptureWidgetId;

        [TempleDllLocation(0x10301324)]
        private WidgetBase _currentMouseOverWidget;

        public WidgetBase CurrentMouseOverWidget => _currentMouseOverWidget;

        [TempleDllLocation(0x10301328)]
        private WidgetBase mMouseButtonId; // TODO = temple.GetRef<int>(0x10301328);

        // Hang on to the delegate
        private readonly CursorDrawCallback _renderTooltipCallback;

        [TempleDllLocation(0x103012C4)]
        [TempleDllLocation(0x101f97b0)]
        [TempleDllLocation(0x101f97a0)]
        public bool IsMouseInputEnabled { get; set; } = true;

        [TempleDllLocation(0x10EF97C4)]
        [TempleDllLocation(0x101f97d0)]
        [TempleDllLocation(0x101f97e0)]
        public bool IsDragging { get; set; }

        private readonly UiWindowEventManager _uiWindowEventManager;

        // TODO: Deregister!
        private int _resizeListenerId;

        // TODO CACHE
        private IEnumerable<WidgetBase> RootWidgets => RootElement
            .ChildrenToArray(filter: node => node is WidgetBase widget && widget.Visible)
            .Cast<WidgetBase>();

        public UiManager()
        {
            _renderTooltipCallback = RenderTooltip;
            _resizeListenerId = Tig.RenderingDevice.AddResizeListener((width, height) =>
            {
                var newSize = new Size(width, height);
                OnScreenSizeChanged?.Invoke(newSize);
            });

            Document = new Document();
            Document.Append(Document.CreateElement("root"));

            _uiWindowEventManager = new UiWindowEventManager(Document);

            Tig.MainWindow.OnEvent += DispatchWindowEvent;

            Debug = new UiManagerDebug(this);
        }

        private void DispatchWindowEvent(IEvent evt)
        {
            if (evt is MouseEvent mouseEvent)
            {
                DispatchMouseEvent(mouseEvent);
            }
            else if (evt is KeyboardEvent keyboardEvent)
            {
                DispatchKeyboardEvent(keyboardEvent);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void BringToFront(WidgetContainer window)
        {
            // TODO: BAD
            var otherWindows = Document
                .ChildrenToArray()
                .Where(otherWindow => otherWindow != window && otherWindow is WidgetContainer)
                .ToList();
            if (otherWindows.Count == 0)
            {
                window.ZIndex = 1;
            }
            else
            {
                window.ZIndex = otherWindows
                    .Max(otherWindow => ((WidgetContainer) otherWindow).ZIndex) + 1;
            }

            SortWindows();
        }

        public void SendToBack(WidgetContainer window)
        {
            window.ZIndex = int.MinValue;
            SortWindows();
        }

        public void SetVisible(WidgetBase widget, bool visible)
        {
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
            // Invalidate any fields that may still hold a reference to the now invalid widget id
            if (IsAncestor(mMouseButtonId, widget))
            {
                mMouseButtonId = null;
                RefreshMouseOverState();
            }

            if (IsAncestor(mMouseCaptureWidgetId, widget))
            {
                mMouseCaptureWidgetId = null;
                RefreshMouseOverState();
            }

            if (IsAncestor(_currentMouseOverWidget, widget))
            {
                if (_currentMouseOverWidget is WidgetButton button && !button.IsDisabled())
                {
                    // TODO button.ButtonState = LgcyButtonState.Normal;
                }

                _currentMouseOverWidget = null;
                RefreshMouseOverState();
            }
        }

        [TempleDllLocation(0x101F8D10)]
        public void Render()
        {
            foreach (var widget in RootWidgets)
            {
                // Our new widget system handles rendering itself
                widget.Render();
            }

            Debug.AfterRenderWidgets();
        }

        public WidgetBase GetWidgetAt(int x, int y)
        {
            WidgetBase result = null;

            // Backwards because of render order (rendered last is really on top)
            foreach (var node in RootElement.ChildrenIterator(true))
            {
                if (!(node is WidgetContainer window))
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
                wheelDelta = state.field24,
                flags = MouseEventFlag.PosChange
            };
            TranslateMouseMessage(args);
        }

        /// <summary>
        /// When capturing content is set, it traps all mouse events and retargets
        /// them at this content node. If capturing is not allowed
        /// (gCaptureInfo.mAllowed is false), then capturing is not set. However, if
        /// the CaptureFlags.IgnoreAllowedState is set, the allowed state is ignored
        /// and capturing is set regardless. To disable capture, pass null for the
        /// value of aContent.
        /// If CaptureFlags.RetargetedToElement is set, all mouse events are
        /// targeted at aContent only. Otherwise, mouse events are targeted at
        /// aContent or its descendants. That is, descendants of aContent receive
        /// mouse events as they normally would, but mouse events outside of aContent
        /// are retargeted to aContent.
        /// If CaptureFlags.PreventDragStart is set then drags are prevented from
        /// starting while this capture is active.
        /// If CaptureFlags.PointerLock is set, similar to
        /// CaptureFlags.RetargetToElement, then events are targeted at aContent,
        /// but capturing is held more strongly (i.e., calls to SetCapturingContent()
        /// won't unlock unless CaptureFlags.PointerLock is set again).
        /// </summary>
        public void SetCapturingContent(Element element, CaptureFlags aFlags)
        {
            // If capture was set for pointer lock, don't unlock unless we are coming
            // out of pointer lock explicitly.
            if (element == null && capturingElementInfo.mPointerLock &&
                (aFlags & CaptureFlags.PointerLock) == 0)
            {
                return;
            }

            capturingElementInfo.mContent = null;

            // only set capturing content if allowed or the
            // CaptureFlags.IgnoreAllowedState or CaptureFlags.PointerLock are used.
            if ((aFlags & CaptureFlags.IgnoreAllowedState) != 0 ||
                capturingElementInfo.mAllowed || (aFlags & CaptureFlags.PointerLock) != 0)
            {
                if (element != null)
                {
                    capturingElementInfo.mContent = element;
                }

                // CaptureFlags.PointerLock is the same as
                // CaptureFlags.RetargetToElement & CaptureFlags.IgnoreAllowedState.
                capturingElementInfo.mRetargetToElement =
                    (aFlags & CaptureFlags.RetargetToElement) != 0 ||
                    (aFlags & CaptureFlags.PointerLock) != 0;
                capturingElementInfo.mPreventDrag =
                    (aFlags & CaptureFlags.PreventDragStart) != 0;
                capturingElementInfo.mPointerLock = (aFlags & CaptureFlags.PointerLock) != 0;
            }
        }

        /// <summary>
        /// Alias for SetCapturingContent(nullptr, CaptureFlags.None) for making
        /// callers what they do clearer.
        /// </summary>
        public void ReleaseCapturingContent()
        {
            SetCapturingContent(null, CaptureFlags.None);
        }

        /// <summary>
        /// Return the active content currently capturing the mouse if any.
        /// </summary>
        public Element GetCapturingContent()
        {
            return capturingElementInfo.mContent;
        }

        /// <summary>
        /// Allow or disallow mouse capturing.
        /// </summary>
        public void AllowMouseCapture(bool aAllowed)
        {
            capturingElementInfo.mAllowed = aAllowed;
        }

        /// <summary>
        /// Returns true if there is an active mouse capture that wants to prevent drags.
        /// </summary>
        public bool IsMouseCapturePreventingDrag()
        {
            return capturingElementInfo.mPreventDrag && capturingElementInfo.mContent != null;
        }

        public void ClearMouseCaptureOnView(Element aView)
        {
            capturingElementInfo.mContent = null;

            // disable mouse capture until the next mousedown as a dialog has opened
            // or a drag has started. Otherwise, someone could start capture during
            // the modal dialog or drag.
            capturingElementInfo.mAllowed = false;
        }

        // If a frame in the subtree rooted at aFrame is capturing the mouse then
        // clears that capture.
        public void ClearMouseCapture(Element aFrame)
        {
            if (capturingElementInfo.mContent == null)
            {
                capturingElementInfo.mAllowed = false;
                return;
            }

            // null frame argument means clear the capture
            if (aFrame == null
                || aFrame.IsInclusiveAncestor(capturingElementInfo.mContent))
            {
                capturingElementInfo.mContent = null;
                capturingElementInfo.mAllowed = false;
            }
        }

        public WidgetBase GetMouseCaptureWidget()
        {
            return mMouseCaptureWidgetId;
        }

        [TempleDllLocation(0x101f9830)]
        public bool SetMouseCaptureWidget(WidgetBase widget)
        {
            if (mMouseCaptureWidgetId == null)
            {
                mMouseCaptureWidgetId = widget;
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x101f9850)]
        public void UnsetMouseCaptureWidget(WidgetBase widget)
        {
            if (mMouseCaptureWidgetId == widget)
            {
                mMouseCaptureWidgetId = null;
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
                                // TODO buttonWid.ButtonState = LgcyButtonState.Normal;
                                Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOff);
                                break;
                            case LgcyButtonState.Down:
                                // Down . Released without click event
                                // TODO buttonWid.ButtonState = LgcyButtonState.Released;
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
                                // TODO buttonWid.ButtonState = LgcyButtonState.Down;
                            }
                        }
                        else
                        {
                            // TODO buttonWid.ButtonState = LgcyButtonState.Hovered;
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
                                // TODO button.ButtonState = LgcyButtonState.Down;
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
                            // TODO button.ButtonState = LgcyButtonState.Hovered;
                            Tig.Sound.PlaySoundEffect(button.sndClick);
                            break;
                        case LgcyButtonState.Released:
                            // TODO button.ButtonState = LgcyButtonState.Normal;
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
                    return RootElement == widget;
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
            if (mMouseCaptureWidgetId != null)
            {
                mMouseCaptureWidgetId.HandleMessage(msg);
                return true;
            }

            var mouseArgs = msg.MouseArgs;

            for (var i = _topLevelWidgets.Count - 1; i >= 0; i--)
            {
                var window = _topLevelWidgets[i];

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

            return false;
        }

        public void DispatchMouseEvent(MouseEvent evt)
        {
            if (evt.SystemType == SystemEventType.MouseDown)
            {
                AllowMouseCapture(true);
            }
            try
            {
                EventTargetImpl target = GetCapturingContent();
                target ??= Document.ElementFromPoint(evt.ClientX, evt.ClientY);
                target ??= Document;
                evt.Target = target;

                _uiWindowEventManager.PreHandleEvent(evt);

                target.Dispatch(evt);
    
                _uiWindowEventManager.PostHandleEvent(evt);
            }
            finally
            {
                AllowMouseCapture(false);
            }
        }

        private void DispatchKeyboardEvent(KeyboardEvent evt)
        {
            var focused = Document.FocusManager.Focused;
            var body = Document.DocumentElement;

            var target = focused ?? body;
            evt.Target = target;            
            target.Dispatch(evt);

            // https://www.w3.org/Bugs/Public/show_bug.cgi?id=27337
            if (!evt.DefaultPrevented)
            {
                if ((evt.Key == KeyboardKey.Enter || evt.Code == "Space") && evt.SystemType == SystemEventType.KeyUp)
                {
                    target.fire_synthetic_mouse_event_not_trusted(SystemEventType.Click);
                }
            }
        }

        public DOM.Element ElementFromPoint(double x, double y)
        {
            return GetWidgetAt((int) x, (int) y);
        }

        public IEnumerable<DOM.Element> ElementsFromPoint(double x, double y)
        {
            throw new NotImplementedException();
        }

        public CaretPosition CaretPositionFromPoint(double x, double y)
        {
            throw new NotImplementedException();
        }

        public Element ScrollingElement { get; set; }

        public Element RootElement => Document.DocumentElement;
    }

    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// Flags for <see cref="UiManager.SetCapturingContent"/>
    /// </summary>
    [Flags]
    public enum CaptureFlags
    {
        None = 0,

        // When assigning capture, ignore whether capture is allowed or not.
        IgnoreAllowedState = 1 << 0,

        // Set if events should be targeted at the capturing content or its children.
        RetargetToElement = 1 << 1,

        // Set if the current capture wants drags to be prevented.
        PreventDragStart = 1 << 2,

        // Set when the mouse is pointer locked, and events are sent to locked
        // element.
        PointerLock = 1 << 3,
    }
}