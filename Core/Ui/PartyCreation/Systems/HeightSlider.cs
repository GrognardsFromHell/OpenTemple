using System;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    public class HeightSlider : WidgetButtonBase
    {
        private const string TrackImagePath = "art/interface/pc_creation/height_bar.tga";
        private const string ThumbImagePath = "art/interface/pc_creation/height_slider.tga";

        private const int MinThumbY = 16;
        private const int MaxThumbY = 173;

        private readonly WidgetImage _thumbImage;

        private float _value;
        // Value will converge towards this value if they differ
        private float _targetValue;
        private TimePoint _lastAnimationTime;

        public float Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, 0, 1);
                _targetValue = _value;
            }
        }
        public event Action<float> OnValueChanged;

        public int ThumbCenterY => Y + _thumbImage.Y + _thumbHeight / 2;

        private readonly int _thumbHeight;

        public override bool HitTest(float x, float y)
        {
            return true;
        }

        public HeightSlider()
        {
            var trackImage = new WidgetImage(TrackImagePath);
            trackImage.FixedSize = trackImage.GetPreferredSize();
            AddContent(trackImage);

            _thumbImage = new WidgetImage(ThumbImagePath);
            _thumbImage.FixedSize = _thumbImage.GetPreferredSize();
            _thumbImage.X = 6;
            _thumbImage.Y = MinThumbY;
            AddContent(_thumbImage);
            _thumbHeight = _thumbImage.FixedHeight;

            Width = _thumbImage.X + _thumbImage.GetPreferredSize().Width;
            Height = trackImage.GetPreferredSize().Height;
        }

        public override void Render()
        {
            var y = (int) (MinThumbY + (MaxThumbY - MinThumbY) * (1.0f - Value));
            y = Math.Clamp(y, MinThumbY, MaxThumbY);
            _thumbImage.Y = y;

            base.Render();
        }

        public override bool HandleMouseMessage(MessageMouseArgs msg)
        {
            var localY = msg.Y - GetContentArea().Y;

            if (Globals.UiManager.GetMouseCaptureWidget() == this)
            {
                // Reposition the thumb
                Value = GetValueFromTrackPos(localY);
                OnValueChanged?.Invoke(Value);

                if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
                {
                    Globals.UiManager.UnsetMouseCaptureWidget(this);
                }

                return true;
            }

            // Smoothly animate moving towards the clicked location
            if (localY < _thumbImage.Y || localY >= _thumbImage.Y + _thumbHeight)
            {
                if ((msg.flags & MouseEventFlag.LeftDown) != 0)
                {
                    _targetValue = GetValueFromTrackPos(localY);
                    _lastAnimationTime = TimePoint.Now;
                }

                return true;
            }

            if ((msg.flags & MouseEventFlag.LeftDown) != 0)
            {
                Globals.UiManager.SetMouseCaptureWidget(this);
            }

            return true;
        }

        private float GetValueFromTrackPos(int trackPos)
        {
            // MinThumbY / MaxThumbY are for the top of the slider image,
            // but for the user, the actual slider's center is relevant...
            var trackMin = MinThumbY + _thumbHeight / 2;
            var trackSize = (MaxThumbY + _thumbHeight / 2) - trackMin;

            // We have to invert it because the bottom is 0.f and the top is 1.f
            return 1.0f - (trackPos - trackMin) / (float) trackSize;
        }

        public override void OnUpdateTime(TimePoint timeMs)
        {
            if (Math.Abs(Value - _targetValue) < 0.001f)
            {
                return;
            }

            var elapsed = timeMs - _lastAnimationTime;
            // Ignores moves of less than 1%
            var amountMoved = (float)  elapsed.TotalSeconds;
            if (amountMoved < 0.01f)
            {
                return;
            }

            _lastAnimationTime = timeMs;
            var deltaRemaining = _targetValue - Value;
            _value += Math.Sign(deltaRemaining) * Math.Min(amountMoved, Math.Abs(deltaRemaining));
            OnValueChanged?.Invoke(_value);
        }
    }
}