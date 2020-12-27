using System.Drawing;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Stats
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

            Label.ContentArea = GetContentArea();
            Label?.Render();
        }
    }
}