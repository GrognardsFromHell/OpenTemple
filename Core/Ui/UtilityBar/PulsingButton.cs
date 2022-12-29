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
        }

        base.Dispose(disposing);
    }

    protected override void LayoutChildren()
    {
        base.LayoutChildren();

        _pulseImage.SetBounds(new RectangleF(
            Point.Empty,
            PaddingArea.Size
        ));
    }

    public override void Render(UiRenderContext context)
    {
        base.Render(context);

        if (PulseColor.A != 0)
        {
            _pulseImage.Color = PulseColor;
            _pulseImage.Render(GetViewportPaddingArea().Location);
        }
    }
}