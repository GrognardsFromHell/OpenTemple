using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui
{
    public enum LgcyWidgetType
    {
        Window = 1,
        Button = 2,
        Scrollbar = 3
    }

    public struct LgcyWidgetId
    {
        private readonly int id;

        public static readonly LgcyWidgetId Invalid = new LgcyWidgetId(-1);

        public LgcyWidgetId(int id)
        {
            this.id = id;
        }

        public static implicit operator int(LgcyWidgetId id) => id.id;
        public bool IsValid => id != -1;

        public bool Equals(LgcyWidgetId other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            return obj is LgcyWidgetId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return id;
        }

        public static bool operator ==(LgcyWidgetId left, LgcyWidgetId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LgcyWidgetId left, LgcyWidgetId right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => id.ToString();
    }

    /*
The base structure of all legacy widgets
*/
    public delegate void LgcyWidgetRenderFn(LgcyWidgetId widgetId);

    public delegate void LgcyWidgetRenderTooltipFn(int x, int y, LgcyWidgetId id);

    public delegate bool LgcyWidgetHandleMsgFn(LgcyWidgetId id, Message msg);

    public abstract class LgcyWidget
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public abstract LgcyWidgetType type { get; }
        public LgcyWidgetId parentId = LgcyWidgetId.Invalid;
        public LgcyWidgetId widgetId = LgcyWidgetId.Invalid;
        public string name;
        public int flags; // 1 = hidden
        public int x;
        public int y;
        public int xrelated;
        public int yrelated;
        public int width;
        public int height;
        public int field_6c;
        public LgcyWidgetRenderTooltipFn renderTooltip = null;
        public LgcyWidgetRenderFn render = null;
        public LgcyWidgetHandleMsgFn handleMessage = null;

        public bool IsWindow() => type == LgcyWidgetType.Window;

        public bool IsButton() => type == LgcyWidgetType.Button;

        public bool IsScrollBar() => type == LgcyWidgetType.Scrollbar;

        public bool IsHidden() => (flags & 1) != 0;

        public LgcyWidget Parent
        {
            get
            {
                if (parentId.IsValid)
                {
                    return Globals.UiManager.GetWidget(parentId);
                }

                return null;
            }
        }

        public bool CanHandleMessage()
        {
            if (widgetId == -1)
            {
                return false;
            }

            return handleMessage != null;
        }

        public bool HandleMessage(Message msg)
        {
            return handleMessage(widgetId, msg);
        }

        public static int WidgetIdIndexOf(LgcyWidgetId widgetId, IEnumerable<LgcyWidgetId> widgetlist)
        {
            var idx = 0;
            foreach (var id in widgetlist)
            {
                if (id == widgetId)
                {
                    return idx;
                }

                idx++;
            }

            return -1;
        }

        public static int WidgetIdIndexOf<T>(LgcyWidgetId widgetId, IEnumerable<T> widgetlist) where T : LgcyWidget
        {
            var idx = 0;
            foreach (var widget in widgetlist)
            {
                if (widget.widgetId == widgetId)
                {
                    return idx;
                }

                idx++;
            }

            return -1;
        }

        [TempleDllLocation(0x101F8830)]
        public bool DrawUiElement(ref Render2dArgs args)
        {
            if (args.textureId != 0)
            {
                var texture = Tig.Textures.GetById(args.textureId);
                if (texture == null)
                {
                    Logger.Warn("Failed to draw because texture {0} is unknown.", args.textureId);
                    return false;
                }

                // Clip the source rectangle to the texture being used
                var texSize = texture.GetContentRect();
                if (args.srcRect.Width > texSize.Width)
                {
                    args.srcRect.Width = texSize.Width;
                }

                if (args.srcRect.Height > texSize.Height)
                {
                    args.srcRect.Height = texSize.Height;
                }
            }
            else
            {
                if (args.srcRect.Width == 0)
                {
                    args.srcRect.Width = 1;
                }

                if (args.srcRect.Height == 0)
                {
                    args.srcRect.Height = 1;
                }
            }

            return Tig.ShapeRenderer2d.DrawRectangle(ref args);
        }
    }

    public enum LgcyWindowMouseState
    {
        Outside = 0,
        Hovered = 6,

        // I have not actually found any place where this is ever set
        Pressed = 7,
        PressedOutside = 8
    }

    public class LgcyWindow : LgcyWidget
    {
        public List<LgcyWidgetId> children = new List<LgcyWidgetId>();
        public int field_27c;
        public int zIndex;
        public LgcyWindowMouseState mouseState = LgcyWindowMouseState.Outside;
        public int field_28c;
        public int field_290;

        public LgcyWindow()
        {
        }

        public LgcyWindow(int x, int y, int w, int h)
        {
            this.x = x;
            this.y = y;
            this.xrelated = x;
            this.yrelated = y;
            this.width = w;
            this.height = h;
        }

        public override LgcyWidgetType type => LgcyWidgetType.Window;
    }

    public enum LgcyButtonState
    {
        Normal = 0,
        Hovered = 1,
        Down = 2,
        Released = 3,
        Disabled = 4
    }

    public class LgcyButton : LgcyWidget
    {
        public int field7c = -1;
        public int field80 = -1;
        public int field84 = -1;
        public int field88 = -1;
        public int field8C = -1;
        public int field90 = -1;
        public LgcyButtonState buttonState = LgcyButtonState.Normal; // 1 - hovered 2 - down  3 - released 4 - disabled
        public int field98;
        public int field9C;
        public int fieldA0;
        public int fieldA4;
        public int fieldA8;
        public int sndDown = -1;
        public int sndClick = -1;
        public int sndHoverOn = -1;
        public int sndHoverOff = -1;

        public LgcyButton()
        {
        }

        public LgcyButton(string ButtonName, LgcyWidgetId ParentId, int X, int Y, int Width, int Height)
        {
            this.name = ButtonName;
            this.x = X;
            this.y = Y;
            this.width = Width;
            this.height = Height;
            this.parentId = ParentId;
            this.yrelated = Y;
            this.xrelated = X;
        }

        public LgcyButton(string ButtonName, LgcyWidgetId ParentId, Rectangle rect)
            : this(ButtonName, ParentId, rect.X, rect.Y, rect.Width, rect.Height)
        {
        }

        public void SetDefaultSounds()
        {
            sndDown = 3012;
            sndClick = 3013;
            sndHoverOn = 3010;
            sndHoverOff = 3011;
        }

        public override LgcyWidgetType type => LgcyWidgetType.Button;
    }

    public class LgcyScrollBar : LgcyWidget
    {
        public int yMin;
        public int yMax;
        public int scrollbarY;
        public int scrollQuantum; //the amount of change per each scrollwheel roll
        public int field8C;
        public int field90;
        public int field94;
        public int field98; // scroll up button state (?)
        public int field9C; // scroll down button state (?)
        public int fieldA0;
        public int fieldA4;
        public int fieldA8;
        public int fieldAC;

        [TempleDllLocation(0x101FA150)]
        public int GetY()
        {
            int result;

            if (yMax > yMin)
            {
                if (field90 != 0)
                {
                    result = (yMax + field8C - yMin)
                             * ((height - 44) * scrollbarY / (yMax + field8C - yMin) - field90)
                             / (height - 44);
                }
                else
                {
                    result = scrollbarY;
                }
            }
            else
            {
                return 0;
            }

            if (result > yMax)
            {
                return yMax;
            }

            if (result < yMin)
            {
                return yMin;
            }

            return result;
        }

        public LgcyScrollBar(int X, int Y, int Height)
        {
            this.x = X;
            this.y = Y;
            flags = 0;
            width = 13;
            height = Height;
            yMax = 100;
            scrollQuantum = 1;
            field8C = 5;

            render = DefaultRender;
            handleMessage = null; // TODO temple.GetRef<BOOL(__cdecl)(int, TigMsg*)>(0x101FA410);
        }

        [TempleDllLocation(0x101FA1B0)]
        private void DefaultRender(LgcyWidgetId id)
        {
            var args = new Render2dArgs();

            var wftScrollbar = Tig.WftScrollbar;

            var srcRect1 = new Rectangle(0, 0, 13, 1);
            var srcRect2 = new Rectangle(0, 0, 13, 3);
            var srcRectArrows = new Rectangle(0, 0, 13, 12);

            args.flags = 0;
            args.destRect.X = this.x;
            args.destRect.Y = this.y + 12;
            args.destRect.Width = this.width;
            var v6 = this.y - this.field90 + 12;
            if (yMax > yMin)
                v6 += this.scrollbarY * (this.height - 44) / (yMax + this.field8C - yMin);
            args.destRect.Height = v6 - this.y + 12;
            args.srcRect = srcRect1;
            args.textureId = wftScrollbar.empty.Resource.GetId();
            Parent.DrawUiElement(ref args);

            args.destRect.Y += args.destRect.Height;
            args.destRect.Height = 3;
            args.textureId = wftScrollbar.top.Resource.GetId();
            args.srcRect = srcRect2;
            Parent.DrawUiElement(ref args);

            args.destRect.Y += args.destRect.Height;
            var v7 = this.yMax;
            var v8 = this.yMin;
            int v9;
            if (v7 <= v8)
                v9 = this.height - 24;
            else
                v9 = this.field8C * (this.height - 44) / (v7 + this.field8C - v8) + 20;
            args.destRect.Height = v9 - 6;
            args.srcRect = srcRect1;
            args.textureId = wftScrollbar.fill.Resource.GetId();
            Parent.DrawUiElement(ref args);

            args.destRect.Y += args.destRect.Height;
            args.destRect.Height = 3;
            args.srcRect = srcRect2;
            args.textureId = wftScrollbar.bottom.Resource.GetId();
            Parent.DrawUiElement(ref args);

            args.destRect.Y += args.destRect.Height;
            args.destRect.Height = this.height - args.destRect.Y + this.y - 12;
            args.srcRect = srcRect1;
            args.textureId = wftScrollbar.empty.Resource.GetId();
            Parent.DrawUiElement(ref args);

            args.destRect.X = this.x;
            args.destRect.Y = this.y;
            args.destRect.Width = this.width;
            args.destRect.Height = 12;
            if (this.field98 == 2)
                args.textureId = wftScrollbar.arrow_up_click.Resource.GetId();
            else
                args.textureId = wftScrollbar.arrow_up.Resource.GetId();
            args.srcRect = srcRectArrows;
            Parent.DrawUiElement(ref args);
            args.destRect.Y = this.height + this.y - 12;
            if (this.field9C == 2)
                args.textureId = wftScrollbar.arrow_down_click.Resource.GetId();
            else
                args.textureId = wftScrollbar.arrow_down.Resource.GetId();
            Parent.DrawUiElement(ref args);
        }

        public LgcyScrollBar(int x, int y, int height, LgcyWidgetId parentId) : this(x, y, height)
        {
            this.parentId = parentId;
            var p = Globals.UiManager.GetWindow(parentId);
            this.x += p.x;
            this.y += p.y;
        }

        public override LgcyWidgetType type => LgcyWidgetType.Scrollbar;
    }

    public class ActiveLegacyWidget
    {
        public string sourceFile;
        public int sourceLine;
        public LgcyWidget widget;
        public WidgetBase advancedWidget;
    }

    public class UiManager
    {
        private int mNextWidgetId = 0;
        private Dictionary<int, ActiveLegacyWidget> mActiveWidgets = new Dictionary<int, ActiveLegacyWidget>();
        private List<LgcyWidgetId> mActiveWindows = new List<LgcyWidgetId>();
        private int maxZIndex = 0;

        public Size ScreenSize => Tig.RenderingDevice.GetCamera().ScreenSize;

        public IEnumerable<WidgetContainer> ActiveWindows => mActiveWindows.Select(GetAdvancedWidget)
            .Cast<WidgetContainer>()
            .Where(x => x != null);

        public UiManagerDebug Debug { get; }


        [TempleDllLocation(0x11E74384)]
        private LgcyWidgetId
            mMouseCaptureWidgetId = LgcyWidgetId.Invalid; // TODO = temple.GetRef<LgcyWidgetId>(0x11E74384);

        [TempleDllLocation(0x10301324)]
        private LgcyWidgetId
            _currentMouseOverWidget = LgcyWidgetId.Invalid; // TODO = temple.GetRef<int>(0x10301324);

        public WidgetBase CurrentMouseOverWidget => GetAdvancedWidget(_currentMouseOverWidget);

        [TempleDllLocation(0x10301328)]
        private LgcyWidgetId mMouseButtonId; // TODO = temple.GetRef<int>(0x10301328);

        // TODO private void(*mMouseMsgHandlerRenderTooltipCallback)(int x, int y, void* data) = temple.GetPointer<void(int x, int y, void* data)>(0x101F9870);
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

        public UiManager()
        {
            _renderTooltipCallback = RenderTooltip;
            Debug = new UiManagerDebug(this);
        }

        public LgcyWidgetId AddWindow(LgcyWindow widget, [CallerFilePath]
            string file = "",
            [CallerLineNumber]
            int line = -1)
        {
            var widgetId = AddWidget(widget, file, line);
            AddWindow(widgetId);
            return widgetId;
        }

        public LgcyWidgetId AddButton(LgcyButton button, [CallerFilePath]
            string file = "",
            [CallerLineNumber]
            int line = -1)
        {
            return AddWidget(button, file, line);
        }

        public LgcyWidgetId AddButton(LgcyButton button, LgcyWidgetId parentId, [CallerFilePath]
            string file = "",
            [CallerLineNumber]
            int line = -1)
        {
            var buttonId = AddWidget(button, file, line);
            AddChild(parentId, buttonId);
            return buttonId;
        }

        public LgcyWidgetId AddScrollBar(LgcyScrollBar scrollBar, [CallerFilePath]
            string file = "",
            [CallerLineNumber]
            int line = -1)
        {
            return AddWidget(scrollBar, file, line);
        }

        public LgcyWidgetId AddScrollBar(LgcyScrollBar scrollBar, LgcyWidgetId parentId,
            [CallerFilePath]
            string file = "", [CallerLineNumber]
            int line = -1)
        {
            var scrollBarId = AddWidget(scrollBar, file, line);
            AddChild(parentId, scrollBarId);
            return scrollBarId;
        }

        /*
            sets the button's parent, and also does a bunch of mouse handling (haven't delved too deep there yet)
        */
        public void SetButtonState(LgcyWidgetId widgetId, LgcyButtonState newState)
        {
            var button = GetButton(widgetId);
            Trace.Assert(button != null);
            button.buttonState = newState;
        }

        public LgcyButtonState GetButtonState(LgcyWidgetId widgetId)
        {
            var button = GetButton(widgetId);
            Trace.Assert(button != null);
            return button.buttonState;
        }

        public void ScrollbarSetYmax(LgcyWidgetId widId, int yMax)
        {
            var widg = GetScrollBar(widId);
            Trace.Assert(widg != null);
            widg.yMax = yMax;
        }

        // I think? sets field84
        public void ScrollbarSetY(LgcyWidgetId widId, int value)
        {
            var scrollbar = GetScrollBar(widId);

            scrollbar.scrollbarY = value;

            var msg = new Message(MessageType.WIDGET);
            msg.arg1 = scrollbar.parentId;
            msg.arg2 = 5;
            Tig.MessageQueue.Enqueue(msg);
        }

        /**
            Adds a widget to the list of created widgets and returns the assigned widget id.
        */
        private LgcyWidgetId AddWidget(LgcyWidget widget, string sourceFile, int sourceLine)
        {
            // Assign a free id
            var assignedId = new LgcyWidgetId(mNextWidgetId++);

            // Make a private copy of the widget
            var activeWidget = new ActiveLegacyWidget();
            mActiveWidgets[assignedId] = activeWidget;

            activeWidget.sourceFile = sourceFile;
            activeWidget.sourceLine = sourceLine;

            // Make a copy and have it managed by the unique_ptr
            activeWidget.widget = widget;

            // Set some basic properties on the widget
            activeWidget.widget.widgetId = assignedId;

            return assignedId;
        }

        /*
        Add something to the list of active windows on top of all existing windows.
        */
        public void AddWindow(LgcyWidgetId id)
        {
            if (mActiveWindows.Contains(id))
            {
                // Window is already in the list
                return;
            }

            var window = GetWindow(id);
            if (window == null)
            {
                throw new Exception($"Trying to add widget {id} as a window which isn't a window.");
            }

            // Don't add it, if it's hidden
            if (window.IsHidden())
            {
                return;
            }

            mActiveWindows.Add(id);
        }

        public void RemoveWindow(LgcyWidgetId id)
        {
            mActiveWindows.Remove(id);
            SortWindows();
        }

        /*
        Gets a pointer to the widget with the given widget ID, null if it doesn't exist.
        */
        public LgcyWidget GetWidget(LgcyWidgetId id)
        {
            if (mActiveWidgets.TryGetValue(id, out var activeWidget))
            {
                return activeWidget.widget;
            }

            return null;
        }

        public LgcyWindow GetWindow(LgcyWidgetId widId)
        {
            var result = GetWidget(widId);
            if (result == null)
            {
                return null;
            }

            if (result.type == LgcyWidgetType.Window)
            {
                return (LgcyWindow) result;
            }

            return null;
        }

        public LgcyButton GetButton(LgcyWidgetId widId)
        {
            var result = GetWidget(widId);
            if (result == null || result.type != LgcyWidgetType.Button)
            {
                return null;
            }

            return (LgcyButton) result;
        }

        public LgcyScrollBar GetScrollBar(LgcyWidgetId widId)
        {
            var result = GetWidget(widId);
            if (result == null || result.type != LgcyWidgetType.Scrollbar)
                return null;
            return (LgcyScrollBar) result;
        }

        public void BringToFront(LgcyWidgetId id)
        {
            var window = GetWindow(id);
            if (window != null)
            {
                window.zIndex = mActiveWindows
                                    .Where(wId => wId != window.widgetId)
                                    .Max(wId => GetWindow(wId).zIndex) + 1;
                SortWindows();
            }
        }

        public void SendToBack(LgcyWidgetId id)
        {
            var window = GetWindow(id);
            if (window != null)
            {
                window.zIndex = int.MinValue;
                SortWindows();
            }
        }

        public WidgetBase GetAdvancedWidget(LgcyWidgetId id)
        {
            if (mActiveWidgets.TryGetValue(id, out var activeWidget))
            {
                return activeWidget.advancedWidget;
            }

            return null;
        }

        public void SetAdvancedWidget(LgcyWidgetId id, WidgetBase advancedWidget)
        {
            if (mActiveWidgets.TryGetValue(id, out var activeWidget))
            {
                if (!ReferenceEquals(activeWidget.advancedWidget, advancedWidget))
                {
                    activeWidget.advancedWidget?.Dispose();
                }

                activeWidget.advancedWidget = advancedWidget;
            }
        }

        public void SetHidden(LgcyWidgetId id, bool hidden)
        {
            var widget = GetWidget(id);
            if (hidden)
            {
                widget.flags |= 1;
            }
            else
            {
                widget.flags &= (~1);
            }

            // New widgets are rendered recursively and dont need to be
            // in the top-level window list to be rendered if they are nested
            // children
            var isNewWidget = GetAdvancedWidget(id) != null;

            // Update the top-level window list
            if (widget.IsWindow() && (!isNewWidget || widget.parentId == -1))
            {
                if (hidden)
                {
                    RemoveWindow(id);
                }
                else
                {
                    AddWindow(id);
                }
            }

            RefreshMouseOverState();
        }

        public bool IsHidden(LgcyWidgetId widId)
        {
            var widget = GetWidget(widId);
            return widget == null || widget.IsHidden();
        }

        public void RemoveWidget(LgcyWidgetId id)
        {
            if (mActiveWidgets.TryGetValue(id, out var activeWidget))
            {
                if (activeWidget.widget.IsWindow())
                {
                    RemoveWindow(id);
                }

                mActiveWidgets.Remove(id);

                // Invalidate any fields that may still hold a reference to the now invalid widget id
                if (mMouseButtonId == id)
                {
                    mMouseButtonId = LgcyWidgetId.Invalid;
                }

                if (mMouseCaptureWidgetId == id)
                {
                    mMouseCaptureWidgetId = LgcyWidgetId.Invalid;
                }

                if (_currentMouseOverWidget == id)
                {
                    _currentMouseOverWidget = LgcyWidgetId.Invalid;
                }
            }
        }

        public bool AddChild(LgcyWidgetId parentId, LgcyWidgetId childId)
        {
            var parent = GetWindow(parentId);
            if (parent != null)
            {
                var child = GetWidget(childId);
                if (child != null)
                {
                    child.parentId = parentId;
                }

                parent.children.Add(childId);
                RefreshMouseOverState();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveChildWidget(LgcyWidgetId id)
        {
            var widget = GetWidget(id);

            if (widget == null || !widget.parentId.IsValid)
            {
                return;
            }

            var parent = GetWidget(widget.parentId);
            if (parent == null)
            {
                return;
            }

            Trace.Assert(parent.IsWindow());
            var parentWindow = (LgcyWindow) parent;
            parentWindow.children.Remove(widget.widgetId);
        }

        [TempleDllLocation(0x101F8D10)]
        public void Render()
        {
            // Make a copy here since some vanilla logic will show/hide windows in their render callbacks
            var activeWindows = mActiveWindows;

            foreach (var windowId in activeWindows)
            {
                // Our new widget system handles rendering itself
                var advWidget = GetAdvancedWidget(windowId);
                if (advWidget != null)
                {
                    advWidget.Render();
                    continue;
                }

                var window = GetWindow(windowId);
                if (window.IsHidden())
                {
                    continue;
                }

                // Render the widget itself
                var renderFunc = window.render;
                renderFunc?.Invoke(windowId);

                // Render all child widgets
                foreach (var childId in window.children)
                {
                    var child = GetWidget(childId);
                    if (!child.IsHidden())
                    {
                        child.render?.Invoke(childId);
                    }
                }
            }

            Debug.AfterRenderWidgets();
        }

        public LgcyWindow GetWindowAt(int x, int y)
        {
            // Backwards because of render order (rendered last is really on top)
            for (int i = mActiveWindows.Count - 1; i >= 0; --i)
            {
                var windowId = mActiveWindows[i];
                var window = GetWindow(windowId);
                if (!window.IsHidden() && DoesWidgetContain(windowId, x, y))
                {
                    return window;
                }
            }

            return null;
        }

        public WidgetBase GetAdvancedWidgetAt(int x, int y)
        {
            var widgetId = GetWidgetAt(x, y);
            if (widgetId != -1)
            {
                return GetAdvancedWidget(widgetId);
            }

            return null;
        }

        public LgcyWidgetId GetWidgetAt(int x, int y)
        {
            LgcyWidgetId result = LgcyWidgetId.Invalid;

            // Backwards because of render order (rendered last is really on top)
            for (int i = mActiveWindows.Count - 1; i >= 0; --i)
            {
                var windowId = mActiveWindows[i];
                var window = GetWindow(windowId);
                if (!window.IsHidden() && DoesWidgetContain(windowId, x, y))
                {
                    result = windowId;

                    var advWidget = GetAdvancedWidget(windowId);
                    if (advWidget != null)
                    {
                        int localX = x - window.x;
                        int localY = y - window.y;

                        var widgetIn = advWidget.PickWidget(localX, localY);
                        if (widgetIn != null)
                        {
                            return widgetIn.GetWidgetId();
                        }

                        continue;
                    }

                    // Also in reverse order
                    for (int j = window.children.Count - 1; j >= 0; --j)
                    {
                        var childId = window.children[j];
                        var child = GetWidget(childId);
                        if (child != null && !child.IsHidden() && DoesWidgetContain(childId, x, y))
                        {
                            result = childId;
                            break;
                        }
                    }

                    break;
                }
            }

            return result;
        }

        public bool DoesWidgetContain(LgcyWidgetId id, int x, int y)
        {
            if (id == -1)
            {
                return false;
            }

            var widget = GetWidget(id);
            var rect = new Rectangle(widget.x, widget.y, widget.width, widget.height);

            var advWidget = GetAdvancedWidget(id);
            if (advWidget != null)
            {
                rect = advWidget.GetContentArea();
            }

            return widget != null
                   && x >= rect.X
                   && y >= rect.Y
                   && x < rect.X + rect.Width
                   && y < rect.Y + rect.Height;
        }

        public IReadOnlyList<LgcyWidgetId> GetActiveWindows()
        {
            return mActiveWindows;
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

        public LgcyWidgetId GetMouseCaptureWidgetId()
        {
            return mMouseCaptureWidgetId;
        }

        public void SetMouseCaptureWidgetId(LgcyWidgetId widgetId)
        {
            mMouseCaptureWidgetId = widgetId;
        }

        public void UnsetMouseCaptureWidgetId(LgcyWidgetId widgetId)
        {
            if (mMouseCaptureWidgetId == widgetId)
            {
                mMouseCaptureWidgetId = LgcyWidgetId.Invalid;
            }
        }

        /*
        This will sort the windows using their z-order in the order in which
        they should be rendered.
        */
        public void SortWindows()
        {
            // Sort Windows by Z-Index
            mActiveWindows.Sort((idA, idB) =>
            {
                var windowA = GetWindow(idA);
                var windowB = GetWindow(idB);
                return windowA.zIndex.CompareTo(windowB.zIndex);
            });

            // Reassign a zindex in monotonous order to those windows that dont have one
            for (var i = 0; i < mActiveWindows.Count; ++i)
            {
                var window = GetWindow(mActiveWindows[i]);
                if (window.zIndex == 0)
                {
                    window.zIndex = i * 100;
                }
            }
        }

        private void RenderTooltip(int x, int y, object userArg)
        {
            var advancedWidget = GetAdvancedWidget(_currentMouseOverWidget);
            if (advancedWidget != null)
            {
                advancedWidget.RenderTooltip(x, y);
                return;
            }

            var widget = this.GetWidget(_currentMouseOverWidget);
            widget?.renderTooltip?.Invoke(x, y, widget.widgetId);
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

            var widIdAtCursor = GetWidgetAt(x, y);
            var globalWidId = _currentMouseOverWidget;

            // moused widget changed
            if ((flags & MouseEventFlag.PosChange) != 0 && widIdAtCursor != globalWidId)
            {
                if (widIdAtCursor != -1 && Tig.Mouse.CursorDrawCallback == _renderTooltipCallback)
                {
                    Tig.Mouse.SetCursorDrawCallback(null, 0);
                }

                if (globalWidId != -1)
                {
                    bool enqueueExited = false;
                    var globalWid = GetWidget(globalWidId);
                    // if window
                    if (globalWid is LgcyWindow prevHoveredWindow)
                    {
                        if (prevHoveredWindow.mouseState == LgcyWindowMouseState.Pressed)
                        {
                            prevHoveredWindow.mouseState = LgcyWindowMouseState.PressedOutside;
                        }
                        else if (prevHoveredWindow.mouseState != LgcyWindowMouseState.PressedOutside)
                        {
                            prevHoveredWindow.mouseState = LgcyWindowMouseState.Outside;
                        }

                        enqueueExited = true;
                    }
                    // button
                    else if (globalWid.IsButton() && !globalWid.IsHidden())
                    {
                        var buttonWid = GetButton(globalWidId);
                        switch (buttonWid.buttonState)
                        {
                            case LgcyButtonState.Hovered:
                                // Unhover
                                buttonWid.buttonState = LgcyButtonState.Normal;
                                Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOff);
                                break;
                            case LgcyButtonState.Down:
                                // Down . Released without click event
                                buttonWid.buttonState = LgcyButtonState.Released;
                                break;
                        }

                        if (!GetWidget(globalWid.parentId).IsHidden())
                        {
                            enqueueExited = true;
                        }
                    }
                    // scrollbar
                    else if (globalWid.IsScrollBar())
                    {
                        if (globalWid.IsHidden() || GetWidget(globalWid.parentId).IsHidden())
                            enqueueExited = false;
                        else
                            enqueueExited = true;
                    }

                    if (enqueueExited)
                    {
                        newTigMsg.widgetId = globalWidId;
                        newTigMsg.widgetEventType = TigMsgWidgetEvent.Exited;
                        Tig.MessageQueue.Enqueue(new Message(newTigMsg));
                    }
                }

                if (widIdAtCursor != -1)
                {
                    var widAtCursor = GetWidget(widIdAtCursor);
                    if (widAtCursor is LgcyWindow widAtCursorWindow)
                    {
                        if (widAtCursorWindow.mouseState == LgcyWindowMouseState.PressedOutside)
                        {
                            widAtCursorWindow.mouseState = LgcyWindowMouseState.Pressed;
                        }
                        else if (widAtCursorWindow.mouseState != LgcyWindowMouseState.Pressed)
                        {
                            widAtCursorWindow.mouseState = LgcyWindowMouseState.Hovered;
                        }
                    }
                    else if (widAtCursor is LgcyButton buttonWid)
                    {
                        if (buttonWid.buttonState != LgcyButtonState.Normal)
                        {
                            if (buttonWid.buttonState == LgcyButtonState.Released)
                            {
                                buttonWid.buttonState = LgcyButtonState.Down;
                            }
                        }
                        else
                        {
                            buttonWid.buttonState = LgcyButtonState.Hovered;
                            Tig.Sound.PlaySoundEffect(buttonWid.sndHoverOn);
                        }
                    }

                    newTigMsg.widgetId = widIdAtCursor;
                    newTigMsg.widgetEventType = TigMsgWidgetEvent.Entered;
                    Tig.MessageQueue.Enqueue(new Message(newTigMsg));
                }

                globalWidId = _currentMouseOverWidget = widIdAtCursor;
            }

            if ((mouseMsg.flags & MouseEventFlag.PosChangeSlow) != 0
                && globalWidId != -1
                && Tig.Mouse.CursorDrawCallback == null)
            {
                Tig.Mouse.SetCursorDrawCallback(_renderTooltipCallback);
            }

            if ((mouseMsg.flags & MouseEventFlag.LeftClick) != 0)
            {
                // probably redundant to do again, but just to be safe...
                var widIdAtCursor2 = GetWidgetAt(mouseMsg.X, mouseMsg.Y);
                if (widIdAtCursor2.IsValid)
                {
                    var button = GetButton(widIdAtCursor2);
                    if (button != null)
                    {
                        switch (button.buttonState)
                        {
                            case LgcyButtonState.Hovered:
                                button.buttonState = LgcyButtonState.Down;
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

            if ((mouseMsg.flags & MouseEventFlag.LeftReleased) != 0 && mMouseButtonId.IsValid)
            {
                var button = GetButton(mMouseButtonId);
                if (button != null)
                {
                    switch (button.buttonState)
                    {
                        case LgcyButtonState.Down:
                            button.buttonState = LgcyButtonState.Hovered;
                            Tig.Sound.PlaySoundEffect(button.sndClick);
                            break;
                        case LgcyButtonState.Released:
                            button.buttonState = LgcyButtonState.Normal;
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
                mMouseButtonId = LgcyWidgetId.Invalid;
            }

            return false;
        }

        [TempleDllLocation(0x101f8a80)]
        public bool ProcessMessage(Message msg)
        {
            // Dispatch time update messages continuously to all advanced widgets
            if (msg.type == MessageType.UPDATE_TIME)
            {
                foreach (var entry in mActiveWidgets.Values)
                {
                    entry.advancedWidget?.OnUpdateTime(msg.created);
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
                    for (var i = mActiveWindows.Count - 1; i >= 0; i--)
                    {
                        var window = GetWidget(mActiveWindows[i]);

                        if (!window.IsHidden() && window.CanHandleMessage())
                        {
                            if (window.HandleMessage(msg))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
            }
        }

        private bool ProcessWidgetMessage(Message msg)
        {
            var widgetArgs = msg.WidgetArgs;

            var widgetId = widgetArgs.widgetId;
            LgcyWidget dispatchTo;
            while (widgetId != -1 && (dispatchTo = GetWidget(widgetId)) != null)
            {
                LgcyWidget parent = dispatchTo.parentId.IsValid ? GetWindow(dispatchTo.parentId) : null;
                if ((parent == null || !parent.IsHidden()) && !dispatchTo.IsHidden())
                {
                    if (dispatchTo.CanHandleMessage())
                    {
                        if (dispatchTo.HandleMessage(msg))
                        {
                            return true;
                        }
                    }
                }

                // Bubble up the msg if the widget didn't handle it
                widgetId = dispatchTo.parentId;
            }

            return false;
        }

        private bool ProcessMouseMessage(Message msg)
        {

            // Handle if a widget requested mouse capture
            if (mMouseCaptureWidgetId.IsValid)
            {
                var advWidget = GetAdvancedWidget(mMouseCaptureWidgetId);
                if (advWidget != null)
                {
                    advWidget.HandleMessage(msg);
                    return true;
                }
                else
                {
                    var widget = GetWidget(mMouseCaptureWidgetId);
                    if (widget != null && widget.CanHandleMessage())
                    {
                        widget.HandleMessage(msg);
                        return true;
                    }
                }

                return false;
            }

            var mouseArgs = msg.MouseArgs;

            for (var i = mActiveWindows.Count - 1; i >= 0; i--)
            {
                var windowId = mActiveWindows[i];
                var window = GetWindow(windowId);

                if (window == null || !window.IsWindow() || window.IsHidden() ||
                    !DoesWidgetContain(windowId, msg.arg1, msg.arg2))
                {
                    continue;
                }

                // Try dispatching the msg to all children of the window that are also under the mouse cursor, in reverse order of their
                // own insertion into the children list
                for (var j = window.children.Count - 1; j >= 0; j--)
                {
                    var childId = window.children[j];

                    if (DoesWidgetContain(childId, msg.arg1, msg.arg2))
                    {
                        var advancedChild = GetAdvancedWidget(childId);
                        if (advancedChild != null)
                        {
                            if (advancedChild.IsVisible() && advancedChild.HandleMouseMessage(mouseArgs))
                            {
                                return true;
                            }

                            continue;
                        }

                        var child = GetWidget(childId);
                        if (child != null && child.CanHandleMessage() && !child.IsHidden())
                        {
                            if (child.HandleMessage(msg))
                            {
                                return true;
                            }
                        }
                    }
                }

                // After checking with all children, dispatch the msg to the window itself
                if (window.CanHandleMessage() && !window.IsHidden() && window.HandleMessage(msg))
                {
                    return true;
                }
            }

            return false;
        }

    }
}