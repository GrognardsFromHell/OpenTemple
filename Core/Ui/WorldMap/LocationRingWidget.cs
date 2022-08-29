using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.WorldMap;

public class LocationRingWidget : WidgetButtonBase
{
    private readonly int _radius;

    private readonly int _radiusSquared;

    private readonly WidgetImage _ringImage;

    private int _trend;

    private TimePoint _time;

    private float _animationPhase;

    private LgcyButtonState _lastButtonState;

    public bool ForceVisible { get; set; }

    public LocationRingWidget(Point position, int radius) : base(
        new Rectangle(position.X - radius, position.Y - radius, radius * 2, radius * 2)
    )
    {
        _radius = radius;
        _radiusSquared = radius * radius;

        _ringImage = new WidgetImage("art/interface/WORLDMAP_UI/Worldmap_Ring.tga");
        AddContent(_ringImage);
        Reset();
    }

    [TempleDllLocation(0x1015a710)]
    public override void Render()
    {
        var buttonState = ForceVisible ? LgcyButtonState.Hovered : ButtonState;

        if (buttonState != _lastButtonState)
        {
            _lastButtonState = buttonState;
            if (buttonState == LgcyButtonState.Hovered)
            {
                if (float.IsNaN(_animationPhase))
                {
                    _time = TimePoint.Now;
                    _time -= TimeSpan.FromMilliseconds(1);
                    _animationPhase = 0;
                }
                else
                {
                    _time = TimePoint.Now;
                    _time += TimeSpan.FromMilliseconds((int) (_animationPhase * -1000.0f));
                    _trend = 1;
                }
            }
            else if (buttonState == LgcyButtonState.Normal)
            {
                _time += TimeSpan.FromMilliseconds((int) (_animationPhase * -333.3f));
                _trend = -1;
                _time = TimePoint.Now;
            }
        }

        if (_animationPhase >= 0.0f)
        {
            if (_trend == 1)
            {
                _animationPhase = (float) (TimePoint.Now - _time).TotalMilliseconds / 333.3f;
            }
            else
            {
                _animationPhase = (1000.0f - (float) (TimePoint.Now - _time).TotalMilliseconds)
                                  * 0.001f;
            }

            _animationPhase = Math.Clamp(_animationPhase, 0, 1);
        }

        var currentSize = (int) (_animationPhase * 2 * _radius);
        var currentOffset = (int) ((1.0f - _animationPhase) * _radius);

        _ringImage.X = currentOffset;
        _ringImage.Y = currentOffset;
        _ringImage.FixedSize = new Size(currentSize, currentSize);

        if (_animationPhase > 0)
        {
            _ringImage.Color = PackedLinearColorA.OfFloats(1.0f, 1.0f, 1.0f, _animationPhase);
        }

        base.Render();
    }

    public override bool HitTest(float x, float y)
    {
        var dx = x - _radius;
        var dy = y - _radius;
        return dx * dx + dy * dy <= _radiusSquared;
    }

    public void Reset()
    {
        _lastButtonState = LgcyButtonState.Normal;
        _trend = 1;
        _time = default;
        _animationPhase = float.NaN;
        ForceVisible = false;
    }
}