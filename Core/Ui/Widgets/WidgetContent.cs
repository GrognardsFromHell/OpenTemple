using System.Drawing;

namespace SpicyTemple.Core.Ui.Widgets
{
    public abstract class WidgetContent
    {

        public bool Visible { get; set; } = true;

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

        public Size FixedSize
        {
            get => new Size(mFixedWidth, mFixedHeight);
            set
            {
                mFixedWidth = value.Width;
                mFixedHeight = value.Height;
            }
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
}