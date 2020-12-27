using System.Drawing;

namespace OpenTemple.Core.Ui.Widgets
{
    public abstract class WidgetContent
    {

        public bool Visible { get; set; } = true;

        public abstract void Render();

        public RectangleF ContentArea
        {
            set
            {
                mContentArea = value;
                mDirty = true;
            }
            get => mContentArea;
        }

        public SizeF GetPreferredSize()
        {
            return mPreferredSize;
        }

        public float X { get; set; } = 0;

        public float Y { get; set; } = 0;

        public SizeF FixedSize
        {
            get => new(FixedWidth, FixedHeight);
            set
            {
                FixedWidth = value.Width;
                FixedHeight = value.Height;
            }
        }

        public float FixedWidth { set; get; } = 0;

        public float FixedHeight { set; get; } = 0;

        protected RectangleF mContentArea;
        protected SizeF mPreferredSize;
        protected bool mDirty = true;
    };
}