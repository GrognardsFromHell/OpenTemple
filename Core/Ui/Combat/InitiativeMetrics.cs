using System.Drawing;

namespace OpenTemple.Core.Ui.Combat
{
    public class InitiativeMetrics
    {

        public SizeF FrameSize { get; }

        public RectangleF Button { get; }

        public RectangleF HighlightFrame { get; }

        public SizeF FrameSelected { get; }

        public PointF PortraitOffset { get; }

        public InitiativeMetrics(SizeF size, RectangleF button, RectangleF highlightFrame, SizeF frameSelected, PointF portraitOffset)
        {
            FrameSize = size;
            Button = button;
            HighlightFrame = highlightFrame;
            FrameSelected = frameSelected;
            PortraitOffset = portraitOffset;
        }

        public static readonly InitiativeMetrics Normal = new InitiativeMetrics(
            new Size(62, 56),
            new Rectangle(3, 3, 51, 45),
            new Rectangle(-6, -6, 67, 72),
            new Size(51, 45),
            new Point(2, 2)
        );

        public static readonly InitiativeMetrics Small = new InitiativeMetrics(
            new Size(51, 46),
            new Rectangle(3, 3, 42, 37),
            new Rectangle(-6, -6, 56, 62),
            new Size(51, 45),
            new Point(2, 2)
        );

    }
}