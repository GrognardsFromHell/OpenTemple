using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core.Ui.Widgets
{
    // This works essentially like a horizontal scrollbar
    public class WidgetSlider : WidgetContainer
    {
        public int Quantum { get; set; } = 1;

        // Horizontal start and end of the track within the slider background image
        private const int TrackStart = 24;

        public WidgetSlider(int x, int y, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(x, y, filePath, lineNumber)
        {
            // Size of the slider itself
            Width = 177;
            Height = 27;

            var leftButton = new WidgetButton();
            leftButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-left"));
            leftButton.SetClickHandler(() => { SetValue(GetValue() - 1); });
            leftButton.SetRepeat(true);
            leftButton.Width = 27;
            leftButton.Height = 27;

            var rightButton = new WidgetButton();
            rightButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-right"));
            rightButton.SetClickHandler(() => { SetValue(GetValue() + 1); });
            rightButton.SetRepeat(true);
            rightButton.Width = 27;
            rightButton.Height = 27;
            rightButton.X = Width - rightButton.Width;

            var track = new WidgetButton();
            track.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-track"));
            track.SetClickHandler((x, y) =>
            {
                if (x < _handleButton.X)
                {
                    SetValue(GetValue() - Quantum);
                }
                else if (x >= _handleButton.X + _handleButton.Width)
                {
                    SetValue(GetValue() + Quantum);
                }
            });
            track.SetRepeat(true);

            var handle = new WidgetSliderHandle(this);
            handle.Y = 2;

            _leftButton = leftButton;
            _rightButton = rightButton;
            _track = track;
            _handleButton = handle;

            Add(track);
            Add(leftButton);
            Add(rightButton);
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
            var scrollRange = GetTrackWidth();
            int handleOffset = (int) (((mValue - mMin) / (float) (mMax - mMin)) * scrollRange);
            _handleButton.X = TrackStart + handleOffset;

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

        private WidgetButton _leftButton;
        private WidgetButton _rightButton;
        private WidgetButton _track;
        private WidgetSliderHandle _handleButton;

        internal int GetTrackWidth()
        {
            return 116;
        } // gets width of usable track area

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

        private class WidgetSliderHandle : WidgetButton
        {
            public WidgetSliderHandle(WidgetSlider slider)
            {
                SetStyle("slider-handle");
                _slider = slider;
            }

            public override bool HandleMouseMessage(MessageMouseArgs msg)
            {
                if (Globals.UiManager.GetMouseCaptureWidget() == this)
                {
                    if (msg.flags.HasFlag(MouseEventFlag.PosChange))
                    {
                        int curX = _dragX + msg.X - _dragGrabPoint;

                        var hPercent = (curX - TrackStart) / (float) _slider.GetTrackWidth();
                        if (hPercent < 0)
                        {
                            hPercent = 0;
                        }
                        else if (hPercent > 1)
                        {
                            hPercent = 1;
                        }

                        var newVal = _slider.mMin + (_slider.mMax - _slider.mMin) * hPercent;

                        _slider.SetValue((int) Math.Round(newVal));
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
                        _dragGrabPoint = msg.X;
                        _dragX = X;
                    }
                    else if ((msg.flags & MouseEventFlag.ScrollWheelChange) != 0)
                    {
                        // Forward scroll wheel message to parent
                        _slider.HandleMouseMessage(msg);
                    }
                }

                return true;
            }

            private readonly WidgetSlider _slider;

            private int _dragX = 0;
            private int _dragGrabPoint = 0;
        };
    };
}