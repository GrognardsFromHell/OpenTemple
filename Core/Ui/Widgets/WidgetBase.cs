using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;
using Size = System.Drawing.Size;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetBase : IDisposable
    {
        public string Name { get; set; }

        // Horizontal position relative to parent
        public int X { get; set; }

        // Vertical position relative to parent
        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool Visible
        {
            get => _visible;
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    Globals.UiManager.SetVisible(this, value);
                }
            }
        }

        /// <summary>
        /// Content is shiftet by this offset within the viewport of the widget.
        /// </summary>
        protected Point ContentOffset { get; set; }

        public Margins Margins
        {
            get => mMargins;
            set => mMargins = value;
        }

        public WidgetBase([CallerFilePath]
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
                mParent?.Remove(this);
                Globals.UiManager.RemoveWidget(this);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event Action OnBeforeRender;

        public event Func<Message, bool> OnHandleMessage;

        /// <summary>
        /// Hit test the content of this widget instead of just checking against the content rectangle.
        /// </summary>
        public bool PreciseHitTest { get; set; } = false;

        public virtual bool HitTest(int x, int y)
        {
            var contentArea = GetContentArea();
            x += contentArea.X - mMargins.Left;
            y += contentArea.Y - mMargins.Top;

            if (!PreciseHitTest)
            {
                return contentArea.Contains(x, y);
            }

            UpdateLayout();

            foreach (var content in mContent)
            {
                if (!content.Visible)
                {
                    continue;
                }

                var contentRect = content.GetContentArea();
                contentRect.Intersect(contentArea);

                if (contentRect.Contains(x, y))
                {
                    return true;
                }
            }


            return false;
        }

        public virtual void Render()
        {
            if (!Visible)
            {
                return;
            }

            OnBeforeRender?.Invoke();

            UpdateLayout();

            var contentArea = GetContentArea();

            foreach (var content in mContent)
            {
                if (!content.Visible || !content.GetContentArea().IntersectsWith(contentArea))
                {
                    continue;
                }

                content.Render();
            }
        }

        protected virtual void UpdateLayout()
        {
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
                    Width = contentArea.Width + mMargins.Left + mMargins.Right;
                    Height = contentArea.Height + mMargins.Top + mMargins.Bottom;

                    ApplyAutomaticSizing();
                    contentArea = GetContentArea();
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
                    if (specificContentArea.Width < 0)
                    {
                        specificContentArea.Width = 0;
                    }
                }

                if (content.GetY() != 0)
                {
                    specificContentArea.Y += content.GetY();
                    specificContentArea.Height -= content.GetY();
                    if (specificContentArea.Height < 0)
                    {
                        specificContentArea.Height = 0;
                    }
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

                // Shift according to scroll offset for content
                if (ContentOffset != Point.Empty)
                {
                    specificContentArea.Offset(-ContentOffset.X, -ContentOffset.Y);
                    // Cull the item when it's no longer visible at all
                    if (!specificContentArea.IntersectsWith(contentArea))
                    {
                        continue;
                    }
                }

                if (content.GetContentArea() != specificContentArea)
                {
                    content.SetContentArea(specificContentArea);
                }
            }
        }

        protected void ApplyAutomaticSizing()
        {
            if (mSizeToParent)
            {
                int containerWidth = mParent != null
                    ? mParent.Width
                    : Globals.UiManager.CanvasSize.Width;
                int containerHeight = mParent != null
                    ? mParent.Height
                    : Globals.UiManager.CanvasSize.Height;
                SetSize(new Size(containerWidth, containerHeight));
            }

            if (mCenterHorizontally)
            {
                int containerWidth = mParent != null
                    ? mParent.Width
                    : Globals.UiManager.CanvasSize.Width;
                int x = (containerWidth - Width) / 2;
                if (x != X)
                {
                    X = x;
                }
            }

            if (mCenterVertically)
            {
                int containerHeight = mParent != null
                    ? mParent.Height
                    : Globals.UiManager.CanvasSize.Height;
                int y = (containerHeight - Height) / 2;
                if (y != Y)
                {
                    Y = y;
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
            if (!Visible)
            {
                return null;
            }

            if (x >= mMargins.Left &
                y >= mMargins.Bottom &&
                x < Width - mMargins.Right
                && y < Height - mMargins.Top
                && HitTest(x, y))
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
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public virtual void BringToFront()
        {
            var parent = mParent;
            if (parent != null)
            {
                parent.Remove(this);
                parent.Add(this);
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
            X = x;
            Y = y;
        }

        public void SetPos(Point point) => SetPos(point.X, point.Y);

        public Point GetPos()
        {
            return new Point(X, Y);
        }

        public void SetSize(Size size)
        {
            if (size.Width != Width || size.Height != Height)
            {
                Width = size.Width;
                Height = size.Height;
                OnSizeChanged();
            }
        }

        public Size GetSize()
        {
            return new Size(Width, Height);
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
        private static Rectangle GetContentArea(WidgetBase widget)
        {
            var bounds = new Rectangle(widget.GetPos(), widget.GetSize());

            // The content of an advanced widget container may be moved
            int scrollOffsetY = 0;
            if (widget.GetParent() != null)
            {
                var container = widget.GetParent();
                scrollOffsetY = container.GetScrollOffsetY();
            }

            if (widget.GetParent() != null)
            {
                var parentBounds = GetContentArea(widget.GetParent());
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
            var res = GetContentArea(this);

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

                int clientLeft = parentArea.X + X;
                int clientTop = parentArea.Y + Y - mParent.GetScrollOffsetY();
                int clientRight = clientLeft + Width;
                int clientBottom = clientTop + Height;

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
                return new Rectangle(X, Y, Width, Height);
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

        protected virtual void OnSizeChanged()
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
        private bool _visible = true;

        public virtual void RenderTooltip(int x, int y)
        {
        }

        protected virtual void InvokeOnBeforeRender()
        {
            OnBeforeRender?.Invoke();
        }
    };
}