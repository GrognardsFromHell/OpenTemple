using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetContainer : WidgetBase
    {
        public WidgetContainer(Size size, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1)
            : this(0, 0, size.Width, size.Height, filePath, lineNumber)
        {
        }

        public WidgetContainer(Rectangle rectangle, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : this(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, filePath,
            lineNumber)
        {
        }

        public WidgetContainer(int width, int height, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : this(0, 0, width, height, filePath, lineNumber)
        {
        }

        public WidgetContainer(int x, int y, int width, int height, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Containers are usually empty and should be click through where there is no content
            PreciseHitTest = true;
        }

        public bool ClipChildren { get; set; } = true;

        public virtual void Add(WidgetBase childWidget)
        {
            AppendChild(childWidget);
            Globals.UiManager.RefreshMouseOverState();
        }

        public LgcyWindowMouseState MouseState { get; internal set; }

        public int ZIndex { get; set; }

        public void Remove(WidgetBase childWidget)
        {
            Trace.Assert(childWidget.GetParent() == this);

            RemoveChild(childWidget);
            Globals.UiManager.RefreshMouseOverState();
        }

        public virtual void Clear(bool disposeChildren = false)
        {

            while (LastChild != null)
            {
                if (disposeChildren)
                {
                    // This will auto remove from the list
                    if (LastChild is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    else
                    {
                        RemoveChild(LastChild);
                    }
                }
                else
                {
                    RemoveChild(LastChild);
                }
            }
        }

        public override WidgetBase PickWidget(int x, int y)
        {
            foreach (var child in ChildrenIterator(true))
            {
                var widget = child as WidgetBase;

                if (widget == null || !widget.Visible)
                {
                    continue;
                }

                int localX = x - widget.X;
                int localY = y - widget.Y + mScrollOffsetY;
                if (localY < 0 || localY >= widget.Height)
                {
                    continue;
                }

                if (localX < 0 || localX >= widget.Width)
                {
                    continue;
                }

                var result = widget.PickWidget(localX, localY);
                if (result != null)
                {
                    return result;
                }
            }

            return base.PickWidget(x, y);
        }

        public override void BringToFront()
        {
            if (Parent == null)
            {
                Globals.UiManager.BringToFront(this);
            }
            else
            {
                base.BringToFront();
            }
        }

        public override bool IsContainer()
        {
            return true;
        }

        public List<WidgetBase> GetChildren()
        {
            return ChildrenToArray(filter: n => n is WidgetBase)
                .Cast<WidgetBase>()
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            while (LastChild != null)
            {
                var lastChild = LastChild;
                RemoveChild(lastChild);
                if (lastChild is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Child widgets should have removed themselves from this list
            Trace.Assert(LastChild == null);

            base.Dispose(disposing);
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }

            ContentOffset = new Point(0, mScrollOffsetY);

            base.Render();

            var visArea = GetVisibleArea();

            for (var child = FirstChild; child != null; child = child.NextSibling)
            {
                if (child is WidgetBase widget && widget.Visible)
                {
                    if (ClipChildren)
                    {
                        Tig.RenderingDevice.SetScissorRect(visArea.X, visArea.Y, visArea.Width, visArea.Height);
                    }

                    widget.Render();
                }
            }

            Tig.RenderingDevice.ResetScissorRect();
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            var area = GetContentArea();

            // Iterate in reverse order since this list is ordered in ascending z-order
            foreach (var child in ChildrenIterator(true))
            {
                var widget = child as WidgetBase;
                if (widget == null)
                {
                    continue;
                }

                int x = msg.X - area.X;
                int y = msg.Y - area.Y + GetScrollOffsetY();

                if (widget.Visible & x >= widget.X && y >= widget.Y && x < widget.X + widget.Width &&
                    y < widget.Y + widget.Height)
                {
                    if (widget.HandleMouseMessage(msg))
                    {
                        return true;
                    }
                }
            }

            return base.HandleMouseMessage(msg);
        }

        public override void OnUpdateTime(TimePoint timeMs)
        {
            base.OnUpdateTime(timeMs);

            foreach (var child in ChildrenIterator())
            {
                if (child is WidgetBase widget)
                {
                    widget.OnUpdateTime(timeMs);
                }
            }
        }

        public void SetScrollOffsetY(int scrollY)
        {
            mScrollOffsetY = scrollY;
            Globals.UiManager.RefreshMouseOverState();
        }

        [TempleDllLocation(0x101fa150)]
        public int GetScrollOffsetY()
        {
            return mScrollOffsetY;
        }

        private int mScrollOffsetY = 0;

        public void CenterOnScreen()
        {
            Trace.Assert(GetParent() == null);
            var screenSize = Globals.UiManager.ScreenSize;
            X = (screenSize.Width - Width) / 2;
            Y = (screenSize.Height - Height) / 2;
        }

        public bool IsInTree()
        {
            return OwnerDocument.DocumentElement.IsInclusiveAncestor(this);
        }
    };
}