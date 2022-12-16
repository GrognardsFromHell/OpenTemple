using System;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

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
            UpdateThumbPosition();
        }
    }
    public event Action<float>? OnValueChanged;

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

    protected override void HandleMouseDown(MouseEvent e)
    {
        SetMouseCapture();
        
        // Smoothly animate moving towards the clicked location
        var localY = (int) (e.Y - GetContentArea().Y);
        if (localY < _thumbImage.Y || localY >= _thumbImage.Y + _thumbHeight)
        {
            _targetValue = GetValueFromTrackPos(localY);
            _lastAnimationTime = TimePoint.Now;
        }
    }

    protected override void HandleMouseMove(MouseEvent e)
    {
        // Reposition the thumb immediately while dragging
        if (HasMouseCapture)
        {
            var localY = (int) (e.Y - GetContentArea().Y);
            Value = GetValueFromTrackPos(localY);
            OnValueChanged?.Invoke(Value);
        }
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

    public override void OnUpdateTime(TimePoint now)
    {
        if (Math.Abs(Value - _targetValue) < 0.001f)
        {
            return;
        }

        var elapsed = now - _lastAnimationTime;
        // Ignores moves of less than 1%
        var amountMoved = (float)  elapsed.TotalSeconds;
        if (amountMoved < 0.01f)
        {
            return;
        }

        _lastAnimationTime = now;
        var deltaRemaining = _targetValue - Value;
        _value += Math.Sign(deltaRemaining) * Math.Min(amountMoved, Math.Abs(deltaRemaining));
        UpdateThumbPosition();
        OnValueChanged?.Invoke(_value);
    }

    private void UpdateThumbPosition()
    {
        var y = (int) (MinThumbY + (MaxThumbY - MinThumbY) * (1.0f - Value));
        y = Math.Clamp(y, MinThumbY, MaxThumbY);
        _thumbImage.Y = y;
    }
}