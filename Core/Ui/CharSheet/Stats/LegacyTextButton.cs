using System.Drawing;
using SpicyTemple.Core.Ui.Widgets;

namespace SpicyTemple.Core.Ui.CharSheet.Stats
{
    public class LegacyTextButton : WidgetButton
    {
        public LegacyTextButton()
        {
        }

        public LegacyTextButton(Rectangle rect) : base(rect)
        {
        }

        public WidgetLegacyText Label { get; set; }

        public override void Render()
        {
            base.Render();

            Label?.SetContentArea(GetContentArea());
            Label?.Render();
        }
    }
}