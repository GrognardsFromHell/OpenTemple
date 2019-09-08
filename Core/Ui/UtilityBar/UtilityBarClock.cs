using System.Drawing;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.UtilityBar
{
    public class UtilityBarClock : WidgetButtonBase
    {
        public UtilityBarClock(Rectangle rect) : base(rect)
        {
        }

        [TempleDllLocation(0x10110400)]
        public override void Render()
        {
            base.Render();
        }

        [TempleDllLocation(0x101104a0)]
        public override void RenderTooltip(int x, int y)
        {
            base.RenderTooltip(x, y);
        }
    }
}