using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Ui.WidgetDocs
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
            var window = new LgcyWindow(x, y, width, height);

            window.render = id => Render();
            window.handleMessage = (id, msg) => HandleMessage(msg);

            var widgetId = Globals.UiManager.AddWindow(window);
            Globals.UiManager.SetAdvancedWidget(widgetId, this);
            mWindow = Globals.UiManager.GetWindow(widgetId);
            mWidget = mWindow;
        }

        public bool ClipChildren { get; set; } = true;

        public virtual void Add(WidgetBase childWidget)
        {
            childWidget.SetParent(this);
            // If the child widget was a top-level window before, remove it
            Globals.UiManager.RemoveWindow(childWidget.GetWidgetId());
            mChildren.Add(childWidget);
            Globals.UiManager.AddChild(mWindow.widgetId, childWidget.GetWidgetId());
        }

        protected LgcyWindowMouseState MouseState => mWindow.mouseState;

        public int ZIndex
        {
            get => mWindow.zIndex;
            set => mWindow.zIndex = value;
        }

        public void Remove(WidgetBase childWidget)
        {
            Trace.Assert(childWidget.GetParent() == this);

            childWidget.SetParent(null);
            mChildren.Remove(childWidget);
            Globals.UiManager.RemoveChildWidget(childWidget.GetWidgetId());
        }

        public virtual void Clear()
        {
            for (var i = mChildren.Count - 1; i >= 0; i--)
            {
                Remove(mChildren[i]);
            }
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
                    if (ClipChildren)
                    {
                        Tig.RenderingDevice.SetScissorRect(visArea.X, visArea.Y, visArea.Width, visArea.Height);
                    }

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

        [TempleDllLocation(0x101fa150)]
        public int GetScrollOffsetY()
        {
            return mScrollOffsetY;
        }

        private LgcyWindow mWindow;
        private List<WidgetBase> mChildren = new List<WidgetBase>();

        private int mScrollOffsetY = 0;

        public void CenterOnScreen()
        {
            Trace.Assert(GetParent() == null);
            var screenSize = Tig.RenderingDevice.GetCamera().ScreenSize;
            SetX((screenSize.Width - GetWidth()) / 2);
            SetY((screenSize.Height - GetHeight()) / 2);
        }
    };
}