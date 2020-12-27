using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetScrollView : WidgetContainer
    {
        public WidgetScrollView(int width, int height) : base(width, height)
        {
            var scrollBar = new WidgetScrollBar();
            scrollBar.Height = height;
            scrollBar.X = width - scrollBar.Width;
            scrollBar.SetValueChangeHandler(newValue => { mContainer.ScrollTop = newValue; });
            mScrollBar = scrollBar;
            base.Add(scrollBar);

            var scrollView = new WidgetContainer(GetInnerWidth(), height);
            mContainer = scrollView;
            base.Add(scrollView);

            UpdateInnerContainer();
        }

        public override void Add(WidgetBase childWidget)
        {
            mContainer.Add(childWidget);
            UpdateInnerHeight();
        }

        public override void Clear(bool disposeChildren = false)
        {
            mContainer.Clear(disposeChildren);
            UpdateInnerHeight();
        }

        public int GetInnerWidth()
        {
            return Width - mScrollBar.Width - 2 * mPadding;
        }

        public int GetInnerHeight()
        {
            return Height - 2 * mPadding;
        }

        public override bool IsScrollView()
        {
            return true;
        }

        public void SetPadding(int padding)
        {
            mPadding = padding;

            UpdateInnerContainer();
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
                var childY = child.Y;
                var childH = child.Height;
                var bottom = childY + childH;
                if (bottom > innerHeight)
                {
                    innerHeight = bottom;
                }
            }

            mScrollBar.SetMax(innerHeight);
        }

        private void UpdateInnerContainer()
        {
            mContainer.X = mPadding;
            mContainer.Width = GetInnerWidth();

            mContainer.Y = mPadding;
            mContainer.Height = GetInnerHeight();
        }
    };
}