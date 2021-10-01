using System;
using System.Drawing;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetScrollBar : WidgetContainer
    {
        public WidgetScrollBar(Rectangle rectangle) : this()
        {
            SetPos(rectangle.Location);
            Height = rectangle.Height;
        }

        public int Quantum { get; set; } = 1;

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
            handle.SetParent(this);
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
        }

        public int GetMin()
        {
            return mMin;
        }

        [TempleDllLocation(0x101f9d80)]
        public void SetMin(int value)
        {
            mMin = value;
            if (mMin > _max)
            {
                mMin = _max;
            }

            if (mValue < mMin)
            {
                SetValue(mMin);
            }
        }

        public int Max
        {
            get => _max;
            [TempleDllLocation(0x101f9dd0)]
            set
            {
                _max = value;
                if (_max < mMin)
                {
                    _max = mMin;
                }

                if (mValue > _max)
                {
                    SetValue(_max);
                }
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

            if (value > _max)
            {
                value = _max;
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
            int handleOffset = (int) (((mValue - mMin) / (float) _max) * scrollRange);
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
        private int _max = 150;

        private WidgetButton mUpButton;
        private WidgetButton mDownButton;
        private WidgetButton mTrack;
        private WidgetScrollBarHandle mHandleButton;

        private int GetHandleHeight()
        {
            return 5 * GetTrackHeight() / (5 + Max - GetMin()) + 20;
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

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
            {
                if (msg.wheelDelta > 0)
                {
                    SetValue(GetValue() - Quantum);
                }
                else if (msg.wheelDelta < 0)
                {
                    SetValue(GetValue() + Quantum);
                }

                return true;
            }

            return base.HandleMouseMessage(msg);
        }

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
            }

            public override void Render()
            {
                var contentArea = GetContentArea();

                var topArea = contentArea;
                topArea.Width = mTop.GetPreferredSize().Width;
                topArea.Height = mTop.GetPreferredSize().Height;
                mTop.SetBounds(topArea);
                mTop.Render();

                var bottomArea = contentArea;
                bottomArea.Width = mBottom.GetPreferredSize().Width;
                bottomArea.Height = mBottom.GetPreferredSize().Height;
                bottomArea.Y = contentArea.Y + contentArea.Height - bottomArea.Height; // Align to bottom
                mBottom.SetBounds(bottomArea);
                mBottom.Render();

                int inBetween = bottomArea.Y - topArea.Y - topArea.Height;
                if (inBetween > 0)
                {
                    var centerArea = contentArea;
                    centerArea.Y = topArea.Y + topArea.Height;
                    centerArea.Height = inBetween;
                    centerArea.Width = mHandle.GetPreferredSize().Width;
                    mHandle.SetBounds(centerArea);
                    mHandle.Render();
                }
            }

            public override bool HandleMouseMessage(MessageMouseArgs msg)
            {
                if (Globals.UiManager.GetMouseCaptureWidget() == this)
                {
                    if (msg.flags.HasFlag(MouseEventFlag.PosChange))
                    {
                        int curY = mDragY + msg.Y - mDragGrabPoint;

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

                        var newVal = mScrollBar.mMin + (mScrollBar._max - mScrollBar.mMin) * vPercent;

                        mScrollBar.SetValue((int) newVal);
                    }

                    if (msg.flags.HasFlag(MouseEventFlag.LeftReleased))
                    {
                        Globals.UiManager.UnsetMouseCaptureWidget(this);
                    }
                }
                else
                {
                    if (msg.flags.HasFlag(MouseEventFlag.LeftHeld))
                    {
                        Globals.UiManager.SetMouseCaptureWidget(this);
                        mDragGrabPoint = msg.Y;
                        mDragY = Y;
                    }
                    else if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
                    {
                        // Forward scroll wheel message to parent
                        mScrollBar.HandleMouseMessage(msg);
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
}