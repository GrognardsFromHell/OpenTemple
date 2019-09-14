using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;
using Size = System.Drawing.Size;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
    public class WidgetBase : IDisposable
    {
        public string Name { get; set; }

        protected WidgetBase([CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1)
        {
            if (filePath != null)
            {
                mSourceURI = $"{Path.GetFileName(filePath)}:{lineNumber}";
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (mWidget != null && mWidget.parentId != -1)
                {
                    Globals.UiManager.RemoveChildWidget(GetWidgetId());
                }

                mParent?.Remove(this);

                Globals.UiManager.RemoveWidget(GetWidgetId());
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event Action OnBeforeRender;

        public event Func<Message, bool> OnHandleMessage;

        public virtual void Render()
        {
            if (!IsVisible())
            {
                return;
            }

            OnBeforeRender?.Invoke();

            ApplyAutomaticSizing();

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

            foreach (var content in mContent)
            {
                if (!content.Visible)
                {
                    continue;
                }

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

                // If fixed width and height are used, the content area's width/height are overridden
                if (content.GetFixedWidth() != 0)
                {
                    specificContentArea.Width = content.GetFixedWidth();
                }

                if (content.GetFixedHeight() != 0)
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

        protected void ApplyAutomaticSizing()
        {
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

            if (mCenterHorizontally)
            {
                int containerWidth = mParent != null
                    ? mParent.GetWidth()
                    : (int) Tig.RenderingDevice.GetCamera().GetScreenWidth();
                int x = (containerWidth - GetWidth()) / 2;
                if (x != GetX())
                {
                    SetX(x);
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
                }
            }
        }

        public virtual bool HandleMessage(Message msg)
        {
            if (OnHandleMessage != null)
            {
                if (OnHandleMessage(msg))
                {
                    return true;
                }
            }

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

        public void ClearContent()
        {
            mContent.Clear();
        }

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
            var parent = mParent;
            if (parent != null)
            {
                parent.Remove(this);
                parent.Add(this);
            }
            else
            {
                Globals.UiManager.BringToFront(mWidget.widgetId);
            }
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

        public Rectangle Rectangle
        {
            get => new Rectangle(GetPos(), GetSize());
            set
            {
                SetPos(value.Location);
                SetSize(value.Size);
            }
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
            if (enable)
            {
                SetAutoSizeHeight(false);
                SetAutoSizeWidth(false);
            }
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
                    res.Y += mMargins.Top;
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

        protected virtual void InvokeOnBeforeRender()
        {
            OnBeforeRender?.Invoke();
        }
    };
}