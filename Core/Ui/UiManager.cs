using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
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
        private List<WidgetContainer> _topLevelWidgets = new List<WidgetContainer>();

        private int maxZIndex = 0;

        public Size ScreenSize => Tig.RenderingDevice.GetCamera().ScreenSize;

        public event Action<Size> OnScreenSizeChanged;

        public IEnumerable<WidgetContainer> ActiveWindows => _topLevelWidgets;

        public UiManagerDebug Debug { get; }

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

        // TODO: Deregister!
        private int _resizeListenerId;

        public UiManager()
        {
            _renderTooltipCallback = RenderTooltip;
            Debug = new UiManagerDebug(this);
            _resizeListenerId = Tig.RenderingDevice.AddResizeListener((width, height) =>
            {
                var newSize = new Size(width, height);
                OnScreenSizeChanged?.Invoke(newSize);
            });
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

        public void RemoveWidget(WidgetBase widget)
        {
            if (widget is WidgetContainer container)
            {
                RemoveWindow(container);
            }

            // Invalidate any fields that may still hold a reference to the now invalid widget id
            if (mMouseButtonId == widget)
            {
                mMouseButtonId = null;
                RefreshMouseOverState();
            }

            if (mMouseCaptureWidgetId == widget)
            {
                mMouseCaptureWidgetId = null;
                RefreshMouseOverState();
            }

            if (_currentMouseOverWidget == widget)
            {
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

        public WidgetBase GetAdvancedWidgetAt(int x, int y)
        {
            return GetWidgetAt(x, y);
        }

        public WidgetBase GetWidgetAt(int x, int y)
        {
            WidgetBase result = null;

            // Backwards because of render order (rendered last is really on top)
            for (int i = _topLevelWidgets.Count - 1; i >= 0; --i)
            {
                var window = _topLevelWidgets[i];
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

        public WidgetBase GetMouseCaptureWidget()
        {
            return mMouseCaptureWidgetId;
        }

        public void SetMouseCaptureWidget(WidgetBase widget)
        {
            mMouseCaptureWidgetId = widget;
        }

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
                    else if (globalWid is WidgetButtonBase buttonWid && IsVisible(globalWid))
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
                    else if (widAtCursor is WidgetButtonBase buttonWid)
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
                    if (widIdAtCursor2 is WidgetButtonBase button)
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
                if (mMouseButtonId is WidgetButtonBase button)
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

    }
}