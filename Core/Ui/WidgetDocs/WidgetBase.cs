using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;
using Size = System.Drawing.Size;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetBase : IDisposable
    {
        public WidgetBase()
        {
        }

        public WidgetBase(Rectangle rect)
        {
            SetPos(rect.Location);
            SetSize(rect.Size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mWidget != null && mWidget.parentId != -1)
                {
                    Globals.UiManager.RemoveChildWidget(GetWidgetId());
                }

                if (mParent != null)
                {
                    mParent.Remove(this);
                }

                Globals.UiManager.RemoveWidget(GetWidgetId());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Render()
        {
            if (!IsVisible())
            {
                return;
            }

            if (mSizeToParent)
            {
                int containerWidth = mParent != null
                    ? mParent.GetWidth()
                    : (int) Tig.RenderingDevice.GetCamera().GetScreenWidth();
                int containerHeight = mParent != null
                    ? mParent.GetHeight()
                    : (int) Tig.RenderingDevice.GetCamera().GetScreenHeight();
                SetSize(new Size(containerWidth, containerHeight));
            }

            var contentArea = GetContentArea();

            // Size to content
            if (contentArea.Width == 0 && contentArea.Height == 0)
            {
                foreach (var content in mContent)
                {
                    var preferred = content.GetPreferredSize();
                    contentArea.Width = Math.Max(contentArea.Width, preferred.Width);
                    contentArea.Height = Math.Max(contentArea.Height, preferred.Height);
                }

                // set widget size (adding up the margins in addition to the content dimensions, since the overall size should include the margins)
                if (contentArea.Width != 0 && contentArea.Height != 0)
                {
                    mWidget.width = contentArea.Width + mMargins.Left + mMargins.Right;
                    mWidget.height = contentArea.Height + mMargins.Top + mMargins.Bottom;
                }
            }

            if (mCenterHorizontally)
            {
                int containerWidth = mParent != null
                    ? mParent.GetWidth()
                    : (int) Tig.RenderingDevice.GetCamera().GetScreenWidth();
                int x = (containerWidth - GetWidth()) / 2;
                if (x != GetX())
                {
                    SetX(x);
                    contentArea = GetContentArea();
                }
            }

            if (mCenterVertically)
            {
                int containerHeight = mParent != null
                    ? mParent.GetHeight()
                    : (int) Tig.RenderingDevice.GetCamera().GetScreenHeight();
                int y = (containerHeight - GetHeight()) / 2;
                if (y != GetY())
                {
                    SetY(y);
                    contentArea = GetContentArea();
                }
            }

            foreach (var content in mContent)
            {
                Rectangle specificContentArea = contentArea;
                // Shift according to the content item positioning
                if (content.GetX() != 0)
                {
                    specificContentArea.X += content.GetX();
                    specificContentArea.Width -= content.GetX();
                }

                if (content.GetY() != 0)
                {
                    specificContentArea.Y += content.GetY();
                    specificContentArea.Height -= content.GetY();
                }

                // Constraint width if necessary
                if (content.GetFixedWidth() != 0 && content.GetFixedWidth() < specificContentArea.Width)
                {
                    specificContentArea.Width = content.GetFixedWidth();
                }

                if (content.GetFixedHeight() != 0 && content.GetFixedHeight() < specificContentArea.Height)
                {
                    specificContentArea.Height = content.GetFixedHeight();
                }

                if (content.GetContentArea() != specificContentArea)
                {
                    content.SetContentArea(specificContentArea);
                }

                content.Render();
            }
        }

        public virtual bool HandleMessage(Message msg)
        {
            if (msg.type == MessageType.WIDGET && mWidgetMsgHandler != null)
            {
                return mWidgetMsgHandler(msg.WidgetArgs);
            }
            else if (msg.type == MessageType.KEYSTATECHANGE && mKeyStateChangeHandler != null)
            {
                return mKeyStateChangeHandler(msg.KeyStateChangeArgs);
            }
            else if (msg.type == MessageType.CHAR && mCharHandler != null)
            {
                return mCharHandler(msg.CharArgs);
            }
            else if (msg.type == MessageType.MOUSE)
            {
                return HandleMouseMessage(msg.MouseArgs);
            }

            return false;
        }

        public virtual bool IsContainer()
        {
            return false;
        }

        public virtual bool IsButton()
        {
            return false;
        }

        public virtual bool IsScrollView()
        {
            return false;
        }

        /**
         * Picks the widget a the x,y coordinate local to this widget.
         * Null if the coordinates are outside of this widget. If no
         * other widget inside is at the given coordinate, will just return this.
         */
        public virtual WidgetBase PickWidget(int x, int y)
        {
            if (!IsVisible())
            {
                return null;
            }

            if (x >= mMargins.Left &
                y >= mMargins.Bottom &&
                x < (int) (mWidget.width - mMargins.Right)
                && y < (int) mWidget.height - mMargins.Top)
            {
                return this;
            }

            return null;
        }

        public void AddContent(WidgetContent content)
        {
            mContent.Add(content);
        }

        public void RemoveContent(WidgetContent content)
        {
            mContent.Remove(content);
        }

        public void ClearContent() => mContent.Clear();

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (visible != IsVisible())
            {
                Globals.UiManager.SetHidden(mWidget.widgetId, !visible);
            }
        }

        public bool IsVisible()
        {
            return !mWidget.IsHidden();
        }

        public void BringToFront()
        {
            Globals.UiManager.BringToFront(mWidget.widgetId);
        }

        public void SetParent(WidgetContainer parent)
        {
            Trace.Assert(mParent == null || mParent == parent || parent == null);
            mParent = parent;
        }

        public WidgetContainer GetParent()
        {
            return mParent;
        }

        public void SetPos(int x, int y)
        {
            mWidget.x = x;
            mWidget.y = y;
        }

        public void SetPos(Point point) => SetPos(point.X, point.Y);

        public Point GetPos()
        {
            return new Point(mWidget.x, mWidget.y);
        }

        public int GetX()
        {
            return GetPos().X;
        }

        public int GetY()
        {
            return GetPos().Y;
        }

        public void SetX(int x)
        {
            SetPos(x, GetY());
        }

        public void SetY(int y)
        {
            SetPos(GetX(), y);
        }

        public int GetWidth()
        {
            return GetSize().Width;
        }

        public int GetHeight()
        {
            return GetSize().Height;
        }

        public LgcyWidgetId GetWidgetId()
        {
            return mWidget.widgetId;
        }

        public void SetWidth(int width)
        {
            SetSize(new Size(width, GetHeight()));
        }

        public void SetHeight(int height)
        {
            SetSize(new Size(GetWidth(), height));
        }

        public void SetSize(Size size)
        {
            mWidget.width = size.Width;
            mWidget.height = size.Height;
        }

        public Size GetSize()
        {
            return new Size((int) mWidget.width, (int) mWidget.height);
        }

        /**
         * A unique id for this widget within the source URI (see below).
         */
        public string GetId()
        {
            return mId;
        }

        public void SetId(string id)
        {
            mId = id;
        }

        /**
         * If this widget was loaded from a file, indicates the URI to that file to more easily identify it.
         */
        public string GetSourceURI()
        {
            return mSourceURI;
        }

        public void SetSourceURI(string sourceUri)
        {
            mSourceURI = sourceUri;
        }

        public void SetCenterHorizontally(bool enable)
        {
            mCenterHorizontally = enable;
        }

        public void SetCenterVertically(bool enable)
        {
            mCenterVertically = enable;
        }

        public void SetSizeToParent(bool enable)
        {
            mSizeToParent = enable;
        }

        /**
         *	Basically gets a Rectangle of x,y,w,h.
         *	Can modify based on parent.
         */
        private static Rectangle GetContentArea(LgcyWidgetId id)
        {
            var widget = Globals.UiManager.GetWidget(id);
            var bounds = new Rectangle(widget.x, widget.y, (int) widget.width, (int) widget.height);

            var advWidget = Globals.UiManager.GetAdvancedWidget(id);

            // The content of an advanced widget container may be moved
            int scrollOffsetY = 0;
            if (advWidget.GetParent() != null)
            {
                var container = advWidget.GetParent();
                scrollOffsetY = container.GetScrollOffsetY();
            }

            if (widget.parentId != -1)
            {
                Rectangle parentBounds = GetContentArea(widget.parentId);
                bounds.X += parentBounds.X;
                bounds.Y += parentBounds.Y - scrollOffsetY;

                // Clamp width/height if necessary
                int parentRight = parentBounds.X + parentBounds.Width;
                int parentBottom = parentBounds.Y + parentBounds.Height;
                if (bounds.X >= parentRight)
                {
                    bounds.Width = 0;
                }

                if (bounds.Y >= parentBottom)
                {
                    bounds.Height = 0;
                }

                if (bounds.X + bounds.Width > parentRight)
                {
                    bounds.Width = Math.Max(0, parentRight - bounds.X);
                }

                if (bounds.Y + bounds.Height > parentBottom)
                {
                    bounds.Height = Math.Max(0, parentBottom - bounds.Y);
                }
            }

            return bounds;
        }

        /*
         Returns the {x,y,w,h} rect, but regards modification from parent and subtracts the margins.
         Content area controls:
         - Mouse handling active area
         - Rendering area
         */
        public Rectangle GetContentArea(bool includingMargins = false)
        {
            var res = GetContentArea(mWidget.widgetId);

            // if margins not included, subtract them
            if (!includingMargins)
            {
                if (res.Width != 0 & res.Height != 0)
                {
                    res.X += mMargins.Left;
                    res.Width -= mMargins.Left + mMargins.Right;
                    res.Y += mMargins.Bottom;
                    res.Height -= mMargins.Bottom + mMargins.Top;
                    if (res.Width < 0) res.Width = 0;
                    if (res.Height < 0) res.Height = 0;
                }
            }


            return res;
        }

        public Rectangle GetVisibleArea()
        {
            if (mParent != null)
            {
                Rectangle parentArea = mParent.GetVisibleArea();
                int parentLeft = parentArea.X;
                int parentTop = parentArea.Y;
                int parentRight = parentLeft + parentArea.Width;
                int parentBottom = parentTop + parentArea.Height;

                int clientLeft = parentArea.X + mWidget.x;
                int clientTop = parentArea.Y + mWidget.y - mParent.GetScrollOffsetY();
                int clientRight = clientLeft + mWidget.width;
                int clientBottom = clientTop + mWidget.height;

                clientLeft = Math.Max(parentLeft, clientLeft);
                clientTop = Math.Max(parentTop, clientTop);

                clientRight = Math.Min(parentRight, clientRight);
                clientBottom = Math.Min(parentBottom, clientBottom);

                if (clientRight <= clientLeft)
                {
                    clientRight = clientLeft;
                }

                if (clientBottom <= clientTop)
                {
                    clientBottom = clientTop;
                }

                return new Rectangle(
                    clientLeft,
                    clientTop,
                    clientRight - clientLeft,
                    clientBottom - clientTop
                );
            }
            else
            {
                return new Rectangle(mWidget.x, mWidget.y, (int) mWidget.width, (int) mWidget.height);
            }
        }

        public void SetMouseMsgHandler(Func<MessageMouseArgs, bool> handler)
        {
            mMouseMsgHandler = handler;
        }

        public void SetWidgetMsgHandler(Func<MessageWidgetArgs, bool> handler)
        {
            mWidgetMsgHandler = handler;
        }

        public void SetKeyStateChangeHandler(Func<MessageKeyStateChangeArgs, bool> handler)
        {
            mKeyStateChangeHandler = handler;
        }

        public void SetCharHandler(Func<MessageCharArgs, bool> handler)
        {
            mCharHandler = handler;
        }

        public virtual bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if (mMouseMsgHandler != null)
            {
                return mMouseMsgHandler(msg);
            }

            return false;
        }

        public virtual void OnUpdateTime(TimePoint timeMs)
        {
        }

        public void SetAutoSizeWidth(bool enable)
        {
            mAutoSizeWidth = enable;
        }

        public void SetAutoSizeHeight(bool enable)
        {
            mAutoSizeHeight = enable;
        }

        protected LgcyWidget mWidget = null;
        protected WidgetContainer mParent = null;
        protected string mSourceURI;
        protected string mId;
        protected bool mCenterHorizontally = false;
        protected bool mCenterVertically = false;
        protected bool mSizeToParent = false;
        protected bool mAutoSizeWidth = true;
        protected bool mAutoSizeHeight = true;
        protected Margins mMargins;
        protected Func<MessageMouseArgs, bool> mMouseMsgHandler;
        protected Func<MessageWidgetArgs, bool> mWidgetMsgHandler;
        protected Func<MessageKeyStateChangeArgs, bool> mKeyStateChangeHandler;
        protected Func<MessageCharArgs, bool> mCharHandler;

        protected List<WidgetContent> mContent = new List<WidgetContent>();

        public virtual void RenderTooltip(int x, int y)
        {
        }
    };

    public struct Margins
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Margins(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    public class WidgetContainer : WidgetBase
    {
        public WidgetContainer(Size size) : this(0, size.Width, 0, size.Height)
        {
        }

        public WidgetContainer(Rectangle rectangle) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height)
        {
        }

        public WidgetContainer(int width, int height) : this(0, 0, width, height)
        {
        }

        public WidgetContainer(int x, int y, int width, int height)
        {
            var window = new LgcyWindow(x, y, width, height);

            window.render = id => Render();
            window.handleMessage = (id, msg) => HandleMessage(msg);

            var widgetId = Globals.UiManager.AddWindow(window);
            Globals.UiManager.SetAdvancedWidget(widgetId, this);
            mWindow = Globals.UiManager.GetWindow(widgetId);
            mWidget = mWindow;
        }

        public virtual void Add(WidgetBase childWidget)
        {
            childWidget.SetParent(this);
            // If the child widget was a top-level window before, remove it
            Globals.UiManager.RemoveWindow(childWidget.GetWidgetId());
            mChildren.Add(childWidget);
            Globals.UiManager.AddChild(mWindow.widgetId, childWidget.GetWidgetId());
        }

        public void Remove(WidgetBase childWidget)
        {
            Trace.Assert(childWidget.GetParent() == this);

            childWidget.SetParent(null);
            mChildren.Remove(childWidget);
        }

        public virtual void Clear()
        {
            mChildren.Clear();
        }

        public override WidgetBase PickWidget(int x, int y)
        {
            for (var i = mChildren.Count - 1; i >= 0; i--)
            {
                var child = mChildren[i];

                if (!child.IsVisible())
                {
                    continue;
                }

                int localX = x - child.GetPos().X;
                int localY = y - child.GetPos().Y + mScrollOffsetY;
                if (localY < 0 || localY >= child.GetHeight())
                {
                    continue;
                }

                if (localX < 0 || localX >= child.GetWidth())
                {
                    continue;
                }

                var result = child.PickWidget(localX, localY);
                if (result != null)
                {
                    return result;
                }
            }

            return base.PickWidget(x, y);
        }

        public override bool IsContainer()
        {
            return true;
        }

        public List<WidgetBase> GetChildren()
        {
            return mChildren;
        }

        protected override void Dispose(bool disposing)
        {
            for (var i = mChildren.Count - 1; i >= 0; i--)
            {
                mChildren[i].Dispose();
            }

            // Child widgets should have removed themselves from this list
            Trace.Assert(mChildren.Count == 0);

            base.Dispose(disposing);
        }

        public override void Render()
        {
            if (!IsVisible())
            {
                return;
            }

            base.Render();

            var visArea = GetVisibleArea();

            foreach (var child in mChildren)
            {
                if (child.IsVisible())
                {
                    Tig.RenderingDevice.SetScissorRect(visArea.X, visArea.Y, visArea.Width, visArea.Height);
                    child.Render();
                }
            }

            Tig.RenderingDevice.ResetScissorRect();
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            var area = GetContentArea();

            // Iterate in reverse order since this list is ordered in ascending z-order
            for (var i = mChildren.Count - 1; i >= 0; i--)
            {
                var child = mChildren[i];

                int x = msg.X - area.X;
                int y = msg.Y - area.Y + GetScrollOffsetY();

                if (child.IsVisible() & x >= child.GetX() && y >= child.GetY() && x < child.GetX() + child.GetWidth() &&
                    y < child.GetY() + child.GetHeight())
                {
                    if (child.HandleMouseMessage(msg))
                    {
                        return true;
                    }
                }
            }

            return base.HandleMouseMessage(msg);
        }

        public void SetScrollOffsetY(int scrollY)
        {
            mScrollOffsetY = scrollY;
            Globals.UiManager.RefreshMouseOverState();
        }

        public int GetScrollOffsetY()
        {
            return mScrollOffsetY;
        }

        private LgcyWindow mWindow;
        private List<WidgetBase> mChildren = new List<WidgetBase>();

        private int mScrollOffsetY = 0;
    };

    public class WidgetButtonBase : WidgetBase
    {
        public WidgetButtonBase()
        {
            var button = new LgcyButton();

            // This is our special sauce...
            button.render = id => Render();
            button.handleMessage = (id, msg) => HandleMessage(msg);

            var widgetId = Globals.UiManager.AddButton(button);
            Globals.UiManager.SetAdvancedWidget(widgetId, this);
            mButton = Globals.UiManager.GetButton(widgetId);
            mWidget = mButton;
        }

        public WidgetButtonBase(Rectangle rect) : this()
        {
            SetPos(rect.Location);
            SetSize(rect.Size);
        }

        public event Action<MessageWidgetArgs> OnMouseEnter;

        public event Action<MessageWidgetArgs> OnMouseExit;

        public override bool HandleMessage(Message msg)
        {
            if (msg.type == MessageType.WIDGET)
            {
                MessageWidgetArgs widgetMsg = msg.WidgetArgs;
                if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Clicked)
                {
                    if (mClickHandler != null && !mDisabled)
                    {
                        var contentArea = GetContentArea();
                        int x = widgetMsg.x - contentArea.X;
                        int y = widgetMsg.y - contentArea.Y;
                        mClickHandler(x, y);
                        mLastClickTriggered = TimePoint.Now;
                    }

                    return true;
                }
                else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Entered)
                {
                    OnMouseEnter?.Invoke(widgetMsg);
                }
                else if (widgetMsg.widgetEventType == TigMsgWidgetEvent.Exited)
                {
                    OnMouseExit?.Invoke(widgetMsg);
                }
            }

            return base.HandleMessage(msg);
        }

        public LgcyButtonState ButtonState => mButton.buttonState;

        public void SetDisabled(bool disabled)
        {
            mDisabled = disabled;
        }

        public bool IsDisabled()
        {
            return mDisabled;
        }

        public void SetActive(bool isActive)
        {
            mActive = isActive;
        }

        public bool IsActive()
        {
            return mActive;
        }

        public void SetClickHandler(Action handler)
        {
            mClickHandler = (x, y) => handler();
        }

        public void SetClickHandler(ClickHandler handler)
        {
            mClickHandler = handler;
        }

        public override bool IsButton()
        {
            return true;
        }

        public bool IsRepeat()
        {
            return mRepeat;
        }

        public void SetRepeat(bool enable)
        {
            mRepeat = enable;
        }

        public TimeSpan GetRepeatInterval()
        {
            return mRepeatInterval;
        }

        public void SetRepeatInterval(TimeSpan interval)
        {
            mRepeatInterval = interval;
        }

        public override void OnUpdateTime(TimePoint timeMs)
        {
            if (mRepeat & mButton.buttonState == LgcyButtonState.Down)
            {
                var pos = Tig.Mouse.GetPos();
                if (mClickHandler != null && !mDisabled && mLastClickTriggered + mRepeatInterval < timeMs)
                {
                    var contentArea = GetContentArea();
                    int x = pos.X - contentArea.X;
                    int y = pos.Y - contentArea.Y;
                    mClickHandler(x, y);
                    mLastClickTriggered = TimePoint.Now;
                }
            }
        }

        protected LgcyButton mButton;
        protected bool mDisabled = false;

        protected bool
            mActive = false; // is the state associated with the button active? Note: this is separate from mDisabled, which determines if the button itself is disabled or not

        protected bool mRepeat = false;
        protected TimeSpan mRepeatInterval = TimeSpan.FromMilliseconds(200);
        protected TimePoint mLastClickTriggered;

        public delegate void ClickHandler(int x, int y);

        private ClickHandler mClickHandler;
    };

    public sealed class WidgetButtonStyle
    {
        public string normalImagePath;
        public string activatedImagePath;
        public string hoverImagePath;
        public string pressedImagePath;
        public string disabledImagePath;
        public string frameImagePath;

        public string textStyleId;
        public string hoverTextStyleId;
        public string pressedTextStyleId;
        public string disabledTextStyleId;
        public int soundEnter = -1;
        public int soundLeave = -1;
        public int soundDown = -1;
        public int soundClick = -1;

        public WidgetButtonStyle Copy()
        {
            return (WidgetButtonStyle) MemberwiseClone();
        }
    };

    public class WidgetButton : WidgetButtonBase
    {
        public WidgetButton()
        {
        }

        public WidgetButton(Rectangle rect) : base(rect)
        {
        }

        /*
         central style definitions:
         templeplus/button_styles.json
         */
        public void SetStyle(WidgetButtonStyle style)
        {
            mStyle = style;
            mButton.sndHoverOn = style.soundEnter;
            mButton.sndHoverOff = style.soundLeave;
            mButton.sndDown = style.soundDown;
            mButton.sndClick = style.soundClick;
            UpdateContent();
        }

        /*
         directly fetch style from Globals.WidgetButtonStyles
         */
        public void SetStyle(string styleName)
        {
            SetStyle(Globals.WidgetButtonStyles.GetStyle(styleName));
        }

        public WidgetButtonStyle GetStyle()
        {
            return mStyle;
        }

        public override void Render()
        {
            var contentArea = GetContentArea();

            // Always fall back to the default
            var image = mNormalImage;

            if (mDisabled)
            {
                if (mDisabledImage != null)
                {
                    image = mDisabledImage;
                }
                else
                {
                    image = mNormalImage;
                }

                if (mStyle.disabledTextStyleId != null)
                {
                    mLabel.SetStyleId(mStyle.disabledTextStyleId);
                }
                else
                {
                    mLabel.SetStyleId(mStyle.textStyleId);
                }
            }
            else
            {
                if (mButton.buttonState == LgcyButtonState.Down)
                {
                    if (mPressedImage != null)
                    {
                        image = mPressedImage;
                    }
                    else if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }
                    else
                    {
                        image = mNormalImage;
                    }

                    if (mStyle.pressedTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.pressedTextStyleId);
                    }
                    else if (mStyle.hoverTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.hoverTextStyleId);
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else if (IsActive())
                {
                    // Activated, else Pressed, else Hovered, (else Normal)
                    if (mActivatedImage != null)
                    {
                        image = mActivatedImage;
                    }
                    else if (mPressedImage != null)
                    {
                        image = mPressedImage;
                    }
                    else if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }


                    if (mButton.buttonState == LgcyButtonState.Hovered
                        || mButton.buttonState == LgcyButtonState.Released)
                    {
                        if (mStyle.hoverTextStyleId != null)
                        {
                            mLabel.SetStyleId(mStyle.hoverTextStyleId);
                        }
                        else
                        {
                            mLabel.SetStyleId(mStyle.textStyleId);
                        }
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else if (mButton.buttonState == LgcyButtonState.Hovered
                         || mButton.buttonState == LgcyButtonState.Released)
                {
                    if (mHoverImage != null)
                    {
                        image = mHoverImage;
                    }
                    else
                    {
                        image = mNormalImage;
                    }

                    if (mStyle.hoverTextStyleId != null)
                    {
                        mLabel.SetStyleId(mStyle.hoverTextStyleId);
                    }
                    else
                    {
                        mLabel.SetStyleId(mStyle.textStyleId);
                    }
                }
                else
                {
                    image = mNormalImage;
                    mLabel.SetStyleId(mStyle.textStyleId);
                }
            }

            var fr = mFrameImage;
            if (fr != null)
            {
                var contentAreaWithMargins = GetContentArea(true);
                fr.SetContentArea(contentAreaWithMargins);
                fr.Render();
            }

            if (image != null)
            {
                image.SetContentArea(contentArea);
                image.Render();
            }

            mLabel.SetContentArea(contentArea);
            mLabel.Render();
        }

        public void SetText(string text)
        {
            mLabel.SetText(text);
            UpdateAutoSize();
        }

        private WidgetButtonStyle mStyle;

        /*
          1. updates the WidgetImage pointers below, using WidgetButtonStyle file paths
          2. Updates mLabel
         */
        private void UpdateContent()
        {
            if (mStyle.normalImagePath != null)
            {
                mNormalImage = new WidgetImage(mStyle.normalImagePath);
            }
            else
            {
                mNormalImage?.Dispose();
                mNormalImage = null;
            }

            if (mStyle.activatedImagePath != null)
            {
                mActivatedImage = new WidgetImage(mStyle.activatedImagePath);
            }
            else
            {
                mActivatedImage?.Dispose();
                mActivatedImage = null;
            }

            if (mStyle.hoverImagePath != null)
            {
                mHoverImage = new WidgetImage(mStyle.hoverImagePath);
            }
            else
            {
                mHoverImage?.Dispose();
                mHoverImage = null;
            }

            if (mStyle.pressedImagePath != null)
            {
                mPressedImage = new WidgetImage(mStyle.pressedImagePath);
            }
            else
            {
                mPressedImage?.Dispose();
                mPressedImage = null;
            }

            if (mStyle.disabledImagePath != null)
            {
                mDisabledImage = new WidgetImage(mStyle.disabledImagePath);
            }
            else
            {
                mDisabledImage?.Dispose();
                mDisabledImage = null;
            }

            if (mStyle.frameImagePath != null)
            {
                mFrameImage = new WidgetImage(mStyle.frameImagePath);
            }
            else
            {
                mFrameImage?.Dispose();
                mFrameImage = null;
            }

            mLabel.SetStyleId(mStyle.textStyleId);
            mLabel.SetCenterVertically(true);
            UpdateAutoSize();
        }

        private void UpdateAutoSize()
        {
            // Try to var-size
            if (mAutoSizeWidth || mAutoSizeHeight)
            {
                Size prefSize;
                if (mNormalImage != null)
                {
                    prefSize = mNormalImage.GetPreferredSize();
                }
                else
                {
                    prefSize = mLabel.GetPreferredSize();
                }

                if (mFrameImage != null)
                {
                    // update margins from frame size
                    var framePrefSize = mFrameImage.GetPreferredSize();
                    var marginW = framePrefSize.Width - prefSize.Width;
                    var marginH = framePrefSize.Height - prefSize.Height;
                    if (marginW > 0)
                    {
                        mMargins.Right = marginW / 2;
                        mMargins.Left = marginW - mMargins.Right;
                    }

                    if (marginH > 0)
                    {
                        mMargins.Bottom = marginH / 2;
                        mMargins.Top = marginH - mMargins.Bottom;
                    }
                }

                prefSize.Height += mMargins.Bottom + mMargins.Top;
                prefSize.Width += mMargins.Left + mMargins.Right;

                if (mAutoSizeWidth && mAutoSizeHeight)
                {
                    SetSize(prefSize);
                }
                else if (mAutoSizeWidth)
                {
                    SetWidth(prefSize.Width);
                }
                else if (mAutoSizeHeight)
                {
                    SetHeight(prefSize.Height);
                }
            }
        }

        private WidgetImage mNormalImage;
        private WidgetImage mActivatedImage;
        private WidgetImage mHoverImage;
        private WidgetImage mPressedImage;
        private WidgetImage mDisabledImage;
        private WidgetImage mFrameImage;
        private WidgetText mLabel = new WidgetText();
    }


    class WidgetScrollBar : WidgetContainer
    {
        public WidgetScrollBar() : base(0, 0)
        {
            var upButton = new WidgetButton();
            upButton.SetParent(this);
            upButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-up"));
            upButton.SetClickHandler(() => { SetValue(GetValue() - 1); });
            upButton.SetRepeat(true);

            var downButton = new WidgetButton();
            downButton.SetParent(this);
            downButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-down"));
            downButton.SetClickHandler(() => { SetValue(GetValue() + 1); });
            downButton.SetRepeat(true);

            var track = new WidgetButton();
            track.SetParent(this);
            track.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-track"));
            track.SetClickHandler((x, y) =>
            {
                // The y value is in relation to the track, we need to add it's own Y value,
                // and compare against the current position of the handle
                y += mTrack.GetY();
                if (y < mHandleButton.GetY())
                {
                    SetValue(GetValue() - 5);
                }
                else if (y >= mHandleButton.GetY() + mHandleButton.GetHeight())
                {
                    SetValue(GetValue() + 5);
                }
            });
            track.SetRepeat(true);

            var handle = new WidgetScrollBarHandle(this);
            handle.SetParent(this);
            handle.SetHeight(100);

            SetWidth(Math.Max(upButton.GetWidth(), downButton.GetWidth()));

            mUpButton = upButton;
            mDownButton = downButton;
            mTrack = track;
            mHandleButton = handle;

            Add(track);
            Add(upButton);
            Add(downButton);
            Add(handle);
        }

        public int GetMin()
        {
            return mMin;
        }

        public void SetMin(int value)
        {
            mMin = value;
            if (mMin > mMax)
            {
                mMin = mMax;
            }

            if (mValue < mMin)
            {
                SetValue(mMin);
            }
        }

        public int GetMax()
        {
            return mMax;
        }

        public void SetMax(int value)
        {
            mMax = value;
            if (mMax < mMin)
            {
                mMax = mMin;
            }

            if (mValue > mMax)
            {
                SetValue(mMax);
            }
        }

        public int GetValue()
        {
            return mValue;
        }

        public void SetValue(int value)
        {
            if (value < mMin)
            {
                value = mMin;
            }

            if (value > mMax)
            {
                value = mMax;
            }

            mValue = value;
            if (mValueChanged != null)
            {
                mValueChanged(mValue);
            }
        }

        public override void Render()
        {
            mDownButton.SetY(GetHeight() - mDownButton.GetHeight());

            // Update the track position
            mTrack.SetWidth(GetWidth());
            mTrack.SetY(mUpButton.GetHeight());
            mTrack.SetHeight(GetHeight() - mUpButton.GetHeight() - mDownButton.GetHeight());

            var scrollRange = GetScrollRange();
            int handleOffset = (int) (((mValue - mMin) / (float) mMax) * scrollRange);
            mHandleButton.SetY(mUpButton.GetHeight() + handleOffset);
            mHandleButton.SetHeight(GetHandleHeight());

            base.Render();
        }

        public void SetValueChangeHandler(Action<int> handler)
        {
            mValueChanged = handler;
        }

        private LgcyScrollBar mScrollBar;

        private Action<int> mValueChanged;

        private int mValue = 0;
        private int mMin = 0;
        private int mMax = 150;

        private WidgetButton mUpButton;
        private WidgetButton mDownButton;
        private WidgetButton mTrack;
        private WidgetScrollBarHandle mHandleButton;

        private int GetHandleHeight()
        {
            return 5 * GetTrackHeight() / (5 + GetMax() - GetMin()) + 20;
        } // gets height of handle button (scaled according to Min/Max values)

        internal int GetScrollRange()
        {
            var trackHeight = GetTrackHeight();
            var handleHeight = GetHandleHeight();
            return trackHeight - handleHeight;
        } // gets range of possible values for Handle Button position

        private int GetTrackHeight()
        {
            return GetHeight() - mUpButton.GetHeight() - mDownButton.GetHeight();
        } // gets height of track area


        private class WidgetScrollBarHandle : WidgetButtonBase
        {
            public WidgetScrollBarHandle(WidgetScrollBar scrollBar)
            {
                mScrollBar = scrollBar;
                mTop = new WidgetImage("art/scrollbar/top.tga");
                mTopClicked = new WidgetImage("art/scrollbar/top_click.tga");
                mHandle = new WidgetImage("art/scrollbar/fill.tga");
                mHandleClicked = new WidgetImage("art/scrollbar/fill_click.tga");
                mBottom = new WidgetImage("art/scrollbar/bottom.tga");
                mBottomClicked = new WidgetImage("art/scrollbar/bottom_click.tga");
                SetWidth(mHandle.GetPreferredSize().Width);
            }

            public override void Render()
            {
                var contentArea = GetContentArea();

                var topArea = contentArea;
                topArea.Width = mTop.GetPreferredSize().Width;
                topArea.Height = mTop.GetPreferredSize().Height;
                mTop.SetContentArea(topArea);
                mTop.Render();

                var bottomArea = contentArea;
                bottomArea.Width = mBottom.GetPreferredSize().Width;
                bottomArea.Height = mBottom.GetPreferredSize().Height;
                bottomArea.Y = contentArea.Y + contentArea.Height - bottomArea.Height; // Align to bottom
                mBottom.SetContentArea(bottomArea);
                mBottom.Render();

                int inBetween = bottomArea.Y - topArea.Y - topArea.Height;
                if (inBetween > 0)
                {
                    var centerArea = contentArea;
                    centerArea.Y = topArea.Y + topArea.Height;
                    centerArea.Height = inBetween;
                    centerArea.Width = mHandle.GetPreferredSize().Width;
                    mHandle.SetContentArea(centerArea);
                    mHandle.Render();
                }
            }

            public override bool HandleMouseMessage(MessageMouseArgs msg)
            {
                if (Globals.UiManager.GetMouseCaptureWidgetId() == GetWidgetId())
                {
                    if (msg.flags.HasFlag(MouseEventFlag.PosChange))
                    {
                        int curY = mDragY + msg.Y - mDragGrabPoint;

                        int scrollRange = mScrollBar.GetScrollRange();
                        var vPercent = (curY - mScrollBar.mUpButton.GetHeight()) / (float) scrollRange;
                        if (vPercent < 0)
                        {
                            vPercent = 0;
                        }
                        else if (vPercent > 1)
                        {
                            vPercent = 1;
                        }

                        var newVal = mScrollBar.mMin + (mScrollBar.mMax - mScrollBar.mMin) * vPercent;

                        mScrollBar.SetValue((int) newVal);
                    }

                    if (msg.flags.HasFlag(MouseEventFlag.LeftReleased))
                    {
                        Globals.UiManager.UnsetMouseCaptureWidgetId(GetWidgetId());
                    }
                }
                else
                {
                    if (msg.flags.HasFlag(MouseEventFlag.LeftDown))
                    {
                        Globals.UiManager.SetMouseCaptureWidgetId(GetWidgetId());
                        mDragGrabPoint = msg.Y;
                        mDragY = GetY();
                    }
                }

                return true;
            }

            private WidgetScrollBar mScrollBar;
            private WidgetImage mTop;
            private WidgetImage mTopClicked;
            private WidgetImage mHandle;
            private WidgetImage mHandleClicked;
            private WidgetImage mBottom;
            private WidgetImage mBottomClicked;

            private int mDragY = 0;
            private int mDragGrabPoint = 0;
        };
    };

    class WidgetScrollView : WidgetContainer
    {
        public WidgetScrollView(int width, int height) : base(width, height)
        {
            var scrollBar = new WidgetScrollBar();
            scrollBar.SetHeight(height);
            scrollBar.SetX(width - scrollBar.GetWidth());
            scrollBar.SetValueChangeHandler(newValue => { mContainer.SetScrollOffsetY(newValue); });
            mScrollBar = scrollBar;
            base.Add(scrollBar);

            var scrollView = new WidgetContainer(GetInnerWidth(), height);
            mContainer = scrollView;
            base.Add(scrollView);

            UpdateLayout();
        }

        public override void Add(WidgetBase childWidget)
        {
            UpdateInnerHeight();
            mContainer.Add(childWidget);
        }

        public override void Clear()
        {
            mContainer.Clear();
        }

        public int GetInnerWidth()
        {
            return GetWidth() - mScrollBar.GetWidth() - 2 * mPadding;
        }

        public int GetInnerHeight()
        {
            return GetHeight() - 2 * mPadding;
        }

        public override bool IsScrollView()
        {
            return true;
        }

        public void SetPadding(int padding)
        {
            mPadding = padding;

            UpdateLayout();
        }

        public int GetPadding()
        {
            return mPadding;
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if (base.HandleMouseMessage(msg))
                return true;

            if (msg.flags.HasFlag(MouseEventFlag.ScrollWheelChange))
            {
                var curPos = mScrollBar.GetValue();
                var newPos = curPos - msg.wheelDelta / 10;
                mScrollBar.SetValue(newPos);
            }

            return true;
        }

        private WidgetContainer mContainer;
        private WidgetScrollBar mScrollBar;
        private int mPadding = 5;

        private void UpdateInnerHeight()
        {
            int innerHeight = 0;
            foreach (var child in mContainer.GetChildren())
            {
                var childY = child.GetY();
                var childH = child.GetHeight();
                var bottom = childY + childH;
                if (bottom > innerHeight)
                {
                    innerHeight = bottom;
                }
            }

            mScrollBar.SetMax(innerHeight);
        }

        private void UpdateLayout()
        {
            mContainer.SetX(mPadding);
            mContainer.SetWidth(GetInnerWidth());

            mContainer.SetY(mPadding);
            mContainer.SetHeight(GetInnerHeight());
        }
    };

    public abstract class WidgetContent
    {
        public abstract void Render();

        public void SetContentArea(Rectangle contentArea)
        {
            mContentArea = contentArea;
            mDirty = true;
        }

        public Rectangle GetContentArea()
        {
            return mContentArea;
        }

        public Size GetPreferredSize()
        {
            return mPreferredSize;
        }


        public void SetX(int x)
        {
            mX = x;
        }

        public int GetX()
        {
            return mX;
        }

        public void SetY(int y)
        {
            mY = y;
        }

        public int GetY()
        {
            return mY;
        }

        public void SetFixedWidth(int width)
        {
            mFixedWidth = width;
        }

        public int GetFixedWidth()
        {
            return mFixedWidth;
        }

        public void SetFixedHeight(int height)
        {
            mFixedHeight = height;
        }

        public int GetFixedHeight()
        {
            return mFixedHeight;
        }

        protected Rectangle mContentArea;
        protected Size mPreferredSize;
        protected bool mDirty = true;

        protected int mFixedWidth = 0;
        protected int mFixedHeight = 0;
        protected int mX = 0;
        protected int mY = 0;
    };

    class WidgetImage : WidgetContent, IDisposable
    {
        public WidgetImage(string path)
        {
            SetTexture(path);
        }

        public WidgetImage()
        {
        }

        public override void Render()
        {
            var renderer = Tig.ShapeRenderer2d;
            renderer.DrawRectangle(
                (float) mContentArea.X,
                (float) mContentArea.Y,
                (float) mContentArea.Width,
                (float) mContentArea.Height,
                mTexture.Resource
            );
        }

        public void SetTexture(string path)
        {
            mPath = path;
            mTexture.Dispose();
            if (path != null)
            {
                mTexture = Tig.RenderingDevice.GetTextures().Resolve(path, false);
                if (mTexture.Resource.IsValid())
                {
                    mPreferredSize = mTexture.Resource.GetSize();
                }
            }
        }

        public void SetTexture(ITexture texture)
        {
            mPath = texture.GetName();
            mTexture.Dispose();
            mTexture = texture.Ref();
            if (mTexture.Resource.IsValid())
            {
                mPreferredSize = mTexture.Resource.GetSize();
            }
        }

        private string mPath;
        private ResourceRef<ITexture> mTexture;

        public void Dispose()
        {
            mTexture.Dispose();
        }
    };
}