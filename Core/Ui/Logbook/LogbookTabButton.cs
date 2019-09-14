using System.Drawing;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Logbook
{
    public class LogbookTabButton : WidgetButtonBase
    {
        private readonly WidgetImage _normalLeft;
        private readonly WidgetImage _normalBg;
        private readonly WidgetImage _normalRight;

        private readonly WidgetImage _selectedLeft;
        private readonly WidgetImage _selectedBg;
        private readonly WidgetImage _selectedRight;

        private readonly WidgetLegacyText _label;

        private static readonly TigTextStyle LabelStyle = new TigTextStyle(new ColorRect(PackedLinearColorA.White))
        {
            shadowColor = new ColorRect(PackedLinearColorA.Black),
            flags = TigTextStyleFlag.TTSF_DROP_SHADOW,
            kerning = 1,
            tracking = 3
        };

        public LogbookTabButton(string label)
        {
            _normalLeft = new WidgetImage("art/interface/logbook_ui/Tab-Left.tga");
            _normalBg = new WidgetImage("art/interface/logbook_ui/Tab-Tiled.tga");
            _normalRight = new WidgetImage("art/interface/logbook_ui/Tab-Right.tga");
            _selectedLeft = new WidgetImage("art/interface/logbook_ui/Tab-Left-Selected.tga");
            _selectedBg = new WidgetImage("art/interface/logbook_ui/Tab-Tiled-Selected.tga");
            _selectedRight = new WidgetImage("art/interface/logbook_ui/Tab-Right-Selected.tga");

            _label = new WidgetLegacyText(label, PredefinedFont.ARIAL_12, LabelStyle);

            // Layout the content items
            LayoutContent(_selectedLeft, _selectedBg, _selectedRight, _label);
            var size = LayoutContent(_normalLeft, _normalBg, _normalRight, _label);
            SetSize(size);

            UpdateSelectedState();
        }

        public override void Render()
        {
            UpdateSelectedState();
            base.Render();
        }

        private static Size LayoutContent(WidgetContent left, WidgetContent bg, WidgetContent right,
            WidgetContent label)
        {
            left.FixedSize = left.GetPreferredSize();
            bg.FixedSize = new Size(label.GetPreferredSize().Width, bg.GetPreferredSize().Height);
            bg.SetX(left.FixedSize.Width);
            // The label offset was hardcoded before too
            label.SetX(bg.GetX() - 5);
            label.SetY(5);
            right.FixedSize = right.GetPreferredSize();
            right.SetX(bg.GetX() + bg.FixedSize.Width);

            return new Size(
                right.GetX() + right.FixedSize.Width,
                right.FixedSize.Height
            );
        }

        private void UpdateSelectedState()
        {
            mContent.Clear();
            if (mActive)
            {
                AddContent(_selectedLeft);
                AddContent(_selectedBg);
                AddContent(_selectedRight);
            }
            else
            {
                AddContent(_normalLeft);
                AddContent(_normalBg);
                AddContent(_normalRight);
            }

            AddContent(_label);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _normalLeft.Dispose();
                _normalBg.Dispose();
                _normalRight.Dispose();

                _selectedLeft.Dispose();
                _selectedBg.Dispose();
                _selectedRight.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}