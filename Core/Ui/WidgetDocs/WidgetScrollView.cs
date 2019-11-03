using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Ui.WidgetDocs
{
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

            UpdateInnerContainer();
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

        private void UpdateInnerContainer()
        {
            mContainer.SetX(mPadding);
            mContainer.SetWidth(GetInnerWidth());

            mContainer.SetY(mPadding);
            mContainer.SetHeight(GetInnerHeight());
        }
    };
}