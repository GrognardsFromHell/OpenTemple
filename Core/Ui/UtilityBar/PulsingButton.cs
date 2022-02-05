using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.UtilityBar;

public class PulsingButton : WidgetButton
{
    private WidgetImage _pulseImage;

    public PackedLinearColorA PulseColor { get; set; } = new(0, 0, 0, 0);

    public PulsingButton(string pulseTexture, Rectangle rect) : base(rect)
    {
        _pulseImage = new WidgetImage(pulseTexture);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pulseImage.Dispose();
            _pulseImage = null;
        }
        base.Dispose(disposing);
    }

    public override void Render()
    {
        base.Render();

        if (PulseColor.A != 0)
        {
            _pulseImage.Color = PulseColor;
            _pulseImage.SetBounds(GetContentArea());
            _pulseImage.Render();
        }
    }
}