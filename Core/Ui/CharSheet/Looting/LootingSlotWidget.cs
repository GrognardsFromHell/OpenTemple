using System.Drawing;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Looting
{
    public class LootingSlotWidget : WidgetContainer
    {
        public static readonly Size Size = new Size(50, 51);
        public const int MarginRight = 3;
        public const int MarginBottom = 3;

        public LootingSlotWidget(Point position) : base(position.X, position.Y, Size.Width, Size.Height)
        {
            // slotWidget.OnHandleMessage += 0x101406d0;
            // slotWidget.OnBeforeRender += 0x1013faf0;
            // slotWidget.OnRenderTooltip += 0x1013fea0;
        }

    }
}