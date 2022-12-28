using System;
using System.Drawing;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

public sealed class WidgetScrollBar : WidgetContainer
{
    /// <summary>
    /// How often will the scrollbar move into the designated direction when a user is holding the
    /// mouse on the scrollbar track (above or below the handle).
    /// </summary>
    private static readonly TimeSpan RepeatedScrollingTimeSpan = TimeSpan.FromMilliseconds(250);  
    
    private readonly WidgetButton _upButton;
    private readonly WidgetButton _downButton;
    private readonly WidgetScrollBarHandle _handle;
    private readonly WidgetButton _track;

    public int Quantum { get; set; } = 1;

    private DeclaredInterval? _repeatedScrolling;
    
    private Action<int>? _valueChanged;

    private int _value;
    private int _min;
    private int _max = 150;

    public WidgetScrollBar(Rectangle rectangle) : this()
    {
        Pos = rectangle.Location;
        Height = Dimension.Pixels(rectangle.Height);
    }

    public WidgetScrollBar()
    {
        var upButton = new WidgetButton();
        upButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-up"));
        upButton.AddClickListener(() => { SetValue(GetValue() - 1); });
        upButton.IsRepeat = true;

        var downButton = new WidgetButton();
        downButton.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-down"));
        downButton.AddClickListener(() => { SetValue(GetValue() + 1); });
        downButton.IsRepeat = true;

        var track = new WidgetButton();
        track.SetStyle(Globals.WidgetButtonStyles.GetStyle("scrollbar-track"));
        track.IsRepeat = true;

        var handle = new WidgetScrollBarHandle(this);
        handle.Height = Dimension.Pixels(100);

        var upButtonWidth = upButton.ComputePreferredBorderAreaSize().Width;
        var downButtonWidth = downButton.ComputePreferredBorderAreaSize().Width;

        Width = Dimension.Pixels(MathF.Max(upButtonWidth, downButtonWidth));

        _track = track;
        _upButton = upButton;
        _downButton = downButton;
        _handle = handle;

        Add(track);
        Add(upButton);
        Add(downButton);
        Add(handle);
        
        // Scroll up or down repeatedly if the mouse is held on the track above/below the handle
        OnMouseDown += e => {
            if (e.Button == MouseButton.Left && e.InitialTarget == track && SetMouseCapture())
            {
                if (_repeatedScrolling != null)
                {
                    StopInterval(_repeatedScrolling);
                    _repeatedScrolling = null;
                }
                var handleArea = _handle.GetContentArea();
                if (e.Y < handleArea.Top)
                {
                    SetValue(GetValue() - 5);
                    _repeatedScrolling = AddInterval(() => SetValue(GetValue() - 5), RepeatedScrollingTimeSpan);
                }
                else if (e.Y >= handleArea.Bottom)
                {
                    SetValue(GetValue() + 5);
                    _repeatedScrolling = AddInterval(() => SetValue(GetValue() + 5), RepeatedScrollingTimeSpan);
                }
            }
        };
    }

    protected override void HandleLostMouseCapture(MouseEvent e)
    {
        if (_repeatedScrolling != null)
        {
            StopInterval(_repeatedScrolling);
            _repeatedScrolling = null;
        }
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

    public int Max
    {
        get => _max;
        [TempleDllLocation(0x101f9dd0)]
        set
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
        _downButton.Y = PaddingArea.Height - _downButton.BorderArea.Height;

        // Update the track position
        _track.Y = _upButton.BorderArea.Height;
        _track.PixelSize = new SizeF(
            PaddingArea.Width,
            PaddingArea.Height - _upButton.BorderArea.Height - _downButton.BorderArea.Height
        );

        var scrollRange = GetScrollRange();
        int handleOffset = (int) ((_value - _min) / (float) _max * scrollRange);
        _handle.Y = _upButton.BorderArea.Height + handleOffset;
        _handle.Height = Dimension.Pixels(GetHandleHeight());

        base.Render(context);
    }

    public void SetValueChangeHandler(Action<int> handler)
    {
        _valueChanged = handler;
    }

    private float GetHandleHeight()
    {
        return 5 * GetTrackHeight() / (5 + Max - GetMin()) + 20;
    } // gets height of handle button (scaled according to Min/Max values)

    internal float GetScrollRange()
    {
        var trackHeight = GetTrackHeight();
        var handleHeight = GetHandleHeight();
        return trackHeight - handleHeight;
    } // gets range of possible values for Handle Button position

    private float GetTrackHeight()
    {
        return BorderArea.Height - _upButton.BorderArea.Height - _downButton.BorderArea.Height;
    } // gets height of track area

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

    private class WidgetScrollBarHandle : WidgetButtonBase
    {
        private readonly WidgetScrollBar _scrollBar;
        private readonly WidgetImage _top;
        private readonly WidgetImage _topClicked;
        private readonly WidgetImage _handle;
        private readonly WidgetImage _handleClicked;
        private readonly WidgetImage _bottom;
        private readonly WidgetImage _bottomClicked;

        private float _dragY;
        private float _dragGrabPoint;
        
        public WidgetScrollBarHandle(WidgetScrollBar scrollBar)
        {
            _scrollBar = scrollBar;
            _top = new WidgetImage("art/scrollbar/top.tga");
            _topClicked = new WidgetImage("art/scrollbar/top_click.tga");
            _handle = new WidgetImage("art/scrollbar/fill.tga");
            _handleClicked = new WidgetImage("art/scrollbar/fill_click.tga");
            _bottom = new WidgetImage("art/scrollbar/bottom.tga");
            _bottomClicked = new WidgetImage("art/scrollbar/bottom_click.tga");
            Width = Dimension.Pixels(_handle.GetPreferredSize().Width);
        }

        public override void Render(UiRenderContext context)
        {
            var contentArea = GetContentArea();

            WidgetImage top, handle, bottom;
            if (Globals.UiManager.MouseCaptureWidget == this)
            {
                top = _topClicked;
                handle = _handleClicked;
                bottom = _bottomClicked;
            }
            else
            {
                top = _top;
                handle = _handle;
                bottom = _bottom;
            }
            
            var topArea = contentArea;
            topArea.Width = top.GetPreferredSize().Width;
            topArea.Height = top.GetPreferredSize().Height;
            top.SetBounds(topArea);
            top.Render();

            var bottomArea = contentArea;
            bottomArea.Width = bottom.GetPreferredSize().Width;
            bottomArea.Height = bottom.GetPreferredSize().Height;
            bottomArea.Y = contentArea.Y + contentArea.Height - bottomArea.Height; // Align to bottom
            bottom.SetBounds(bottomArea);
            bottom.Render();

            var inBetween = bottomArea.Y - topArea.Y - topArea.Height;
            if (inBetween > 0)
            {
                var centerArea = contentArea;
                centerArea.Y = topArea.Y + topArea.Height;
                centerArea.Height = inBetween;
                centerArea.Width = handle.GetPreferredSize().Width;
                handle.SetBounds(centerArea);
                handle.Render();
            }
        }

        protected override void HandleMouseDown(MouseEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                SetMouseCapture();
                _dragGrabPoint = (int) e.Y;
                _dragY = Y;
            }
        }

        protected override void HandleMouseMove(MouseEvent e)
        {
            if (HasMouseCapture)
            {
                var curY = _dragY + (int) e.Y - _dragGrabPoint;

                var scrollRange = _scrollBar.GetScrollRange();
                var vPercent = (curY - _scrollBar._upButton.BorderArea.Height) / scrollRange;
                if (vPercent < 0)
                {
                    vPercent = 0;
                }
                else if (vPercent > 1)
                {
                    vPercent = 1;
                }

                var newVal = _scrollBar._min + (_scrollBar._max - _scrollBar._min) * vPercent;

                _scrollBar.SetValue((int) newVal);
            }
        }
    };
};