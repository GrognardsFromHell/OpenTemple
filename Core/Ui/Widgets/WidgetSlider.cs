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

    private Action<int> _valueChanged;

    private int _value;
    private int _min;
    private int _max = 150;

    private WidgetSliderHandle _handleButton;

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

        _handleButton = handle;

        Add(track);
        Add(leftButton);
        Add(rightButton);
        Add(handle);
        
        track.OnClick += e =>
        {
            var handleArea = _handleButton.GetViewportBorderArea();
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
        return _min;
    }

    [TempleDllLocation(0x101f9d80)]
    public void SetMin(int value)
    {
        _min = value;
        if (_min > _max)
        {
            _min = _max;
        }

        if (_value < _min)
        {
            SetValue(_min);
        }
    }

    public int GetMax()
    {
        return _max;
    }

    [TempleDllLocation(0x101f9dd0)]
    public void SetMax(int value)
    {
        _max = value;
        if (_max < _min)
        {
            _max = _min;
        }

        if (_value > _max)
        {
            SetValue(_max);
        }
    }

    public int GetValue()
    {
        return _value;
    }

    [TempleDllLocation(0x101f9e20)]
    public void SetValue(int value)
    {
        if (value < _min)
        {
            value = _min;
        }

        if (value > _max)
        {
            value = _max;
        }

        if (value != _value)
        {
            _value = value;
            _valueChanged?.Invoke(_value);
        }
    }

    public override void Render(UiRenderContext context)
    {
        var scrollRange = GetTrackWidth();
        int handleOffset = (int) (((_value - _min) / (float) (_max - _min)) * scrollRange);
        _handleButton.X = TrackStart + handleOffset;

        base.Render(context);
    }

    public void SetValueChangeHandler(Action<int> handler)
    {
        _valueChanged = handler;
    }

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
            _dragGrabPoint = e.X;
            _dragX = X;
        }

        protected override void HandleMouseMove(MouseEvent e)
        {
            if (HasMouseCapture)
            {
                var curX = _dragX + e.X - _dragGrabPoint;

                var hPercent = (curX - TrackStart) / _slider.GetTrackWidth();
                if (hPercent < 0)
                {
                    hPercent = 0;
                }
                else if (hPercent > 1)
                {
                    hPercent = 1;
                }

                var newVal = _slider._min + (_slider._max - _slider._min) * hPercent;

                _slider.SetValue((int) Math.Round(newVal));
            }
        }
    };
};