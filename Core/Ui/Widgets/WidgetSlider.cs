using System;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

// This works essentially like a horizontal scrollbar
public class WidgetSlider : WidgetContainer
{
    public int Quantum { get; set; } = 1;

    // Horizontal start and end of the track within the slider background image
    private const int TrackStart = 24;

    public WidgetSlider([CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1) : base(filePath, lineNumber)
    {
        // Size of the slider itself
        Width = Dimension.Pixels(177);
        Height = Dimension.Pixels(27);

        var leftButton = new WidgetButton();
        leftButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-left"));
        leftButton.AddClickListener(() => { SetValue(GetValue() - 1); });
        leftButton.IsRepeat = true;
        leftButton.Width = Dimension.Pixels(27);
        leftButton.Height = Dimension.Pixels(27);

        var rightButton = new WidgetButton();
        rightButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-right"));
        rightButton.AddClickListener(() => { SetValue(GetValue() + 1); });
        rightButton.IsRepeat = true;
        rightButton.Width = Dimension.Pixels(27);
        rightButton.Height = Dimension.Pixels(27);
        rightButton.X = Width.Value - rightButton.Width.Value;

        var track = new WidgetButton();
        track.SetStyle(Globals.WidgetButtonStyles.GetStyle("slider-track"));
        track.IsRepeat = true;

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
        
        track.OnClick += e =>
        {
            var handleArea = _handleButton.GetContentArea();
            if (e.X < handleArea.Left)
            {
                SetValue(GetValue() - Quantum);
            }
            else if (e.X >= handleArea.Right)
            {
                SetValue(GetValue() + Quantum);
            }
        };
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

    public override void Render(UiRenderContext context)
    {
        var scrollRange = GetTrackWidth();
        int handleOffset = (int) (((mValue - mMin) / (float) (mMax - mMin)) * scrollRange);
        _handleButton.X = TrackStart + handleOffset;

        base.Render(context);
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

    protected override void HandleMouseWheel(WheelEvent e)
    {
        if (e.DeltaY > 0)
        {
            SetValue(GetValue() - Quantum);
        }
        else if (e.DeltaY < 0)
        {
            SetValue(GetValue() + Quantum);
        }
        e.StopPropagation();
    }

    private class WidgetSliderHandle : WidgetButton
    {
        private readonly WidgetSlider _slider;

        private float _dragX;
        private float _dragGrabPoint;
        
        public WidgetSliderHandle(WidgetSlider slider)
        {
            SetStyle("slider-handle");
            _slider = slider;
        }

        protected override void HandleMouseDown(MouseEvent e)
        {
            SetMouseCapture();
            _dragGrabPoint = (int) e.X;
            _dragX = X;
        }

        protected override void HandleMouseMove(MouseEvent e)
        {
            if (HasMouseCapture)
            {
                var curX = _dragX + e.X - _dragGrabPoint;

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
        }
    };
};