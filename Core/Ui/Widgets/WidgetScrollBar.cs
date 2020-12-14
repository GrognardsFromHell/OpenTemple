using System;
using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.Widgets
{
    class WidgetScrollBar : WidgetContainer
    {
        public WidgetScrollBar(Rectangle rectangle) : this()
        {
            SetPos(rectangle.Location);
            Height = rectangle.Height;
        }

        public int LargeChange { get; set; } = 1;

        public int SmallChange { get; set; } = 1;

        public WidgetScrollBar() : base(0, 0)
        {
            var upButton = new WidgetButton();
            upButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-up"));
            upButton.OnClick = e => SetValue(GetValue() - SmallChange);
            upButton.SetRepeat(true);

            var downButton = new WidgetButton();
            downButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-down"));
            upButton.OnClick = e => SetValue(GetValue() + SmallChange);
            downButton.SetRepeat(true);

            var track = new WidgetButton();
            track.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-track"));
            track.AddEventListener(SystemEventType.MouseDown, e =>
            {
                var mouseEvent = (MouseEvent) e;
                var y = mouseEvent.ClientY;

                // The y value is in relation to the track, we need to add it's own Y value,
                // and compare against the current position of the handle
                y += mTrack.Y;
                if (y < mHandleButton.Y)
                {
                    SetValue(GetValue() - 5);
                }
                else if (y >= mHandleButton.Y + mHandleButton.Height)
                {
                    SetValue(GetValue() + 5);
                }
            });
            track.SetRepeat(true);

            var handle = new WidgetScrollBarHandle(this);
            handle.Height = 100;

            Width = Math.Max(upButton.Width, downButton.Width);

            mUpButton = upButton;
            mDownButton = downButton;
            mTrack = track;
            mHandleButton = handle;

            Add(track);
            Add(upButton);
            Add(downButton);
            Add(handle);

            AddEventListener(SystemEventType.Wheel, msg =>
            {
                var evt = (WheelEvent) msg;
                if (evt.WheelTicksY > 0)
                {
                    SetValue(GetValue() - LargeChange);
                }
                else if (evt.WheelTicksY < 0)
                {
                    SetValue(GetValue() + LargeChange);
                }
            });
        }

        public int GetMin()
        {
            return mMin;
        }

        [TempleDllLocation(0x101f9d80)]
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

        [TempleDllLocation(0x101f9dd0)]
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

        [TempleDllLocation(0x101f9e20)]
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

            if (value != mValue)
            {
                mValue = value;
                mValueChanged?.Invoke(mValue);
            }
        }

        public override void Render()
        {
            mDownButton.Y = Height - mDownButton.Height;

            // Update the track position
            mTrack.Width = Width;
            mTrack.Y = mUpButton.Height;
            mTrack.Height = Height - mUpButton.Height - mDownButton.Height;

            var scrollRange = GetScrollRange();
            int handleOffset = (int) (((mValue - mMin) / (float) mMax) * scrollRange);
            mHandleButton.Y = mUpButton.Height + handleOffset;
            mHandleButton.Height = GetHandleHeight();

            base.Render();
        }

        public void SetValueChangeHandler(Action<int> handler)
        {
            mValueChanged = handler;
        }

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
            return Height - mUpButton.Height - mDownButton.Height;
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
                Width = mHandle.GetPreferredSize().Width;

                AddEventListener(SystemEventType.MouseMove, msg =>
                {
                    if (HasPointerCapture())
                    {
                        var evt = (MouseEvent) msg;

                        var curY = mDragY + evt.ClientY - mDragGrabPoint;

                        int scrollRange = mScrollBar.GetScrollRange();
                        var vPercent = (curY - mScrollBar.mUpButton.Height) / (float) scrollRange;
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
                });
                AddEventListener(SystemEventType.MouseUp, msg =>
                {
                    var evt = (MouseEvent) msg;
                    if (evt.Button == 0)
                    {
                        ReleasePointerCapture();
                    }
                });
                AddEventListener(SystemEventType.MouseDown, msg =>
                {
                    var evt = (MouseEvent) msg;
                    if (!HasPointerCapture() && evt.Button == 0)
                    {
                        SetPointerCapture();
                        mDragGrabPoint = evt.ClientY;
                        mDragY = Y;
                    }
                });
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

            private WidgetScrollBar mScrollBar;
            private WidgetImage mTop;
            private WidgetImage mTopClicked;
            private WidgetImage mHandle;
            private WidgetImage mHandleClicked;
            private WidgetImage mBottom;
            private WidgetImage mBottomClicked;

            private int mDragY = 0;
            private float mDragGrabPoint = 0;
        };
    };
}