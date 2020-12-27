using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells
{
    public class ClassTabButton : WidgetButtonBase
    {
        private const int HorizontalLabelPadding = 3;

        private readonly WidgetImage _normalLeft;
        private readonly WidgetImage _normalCenter;
        private readonly WidgetImage _normalRight;

        private readonly WidgetImage _selectedLeft;
        private readonly WidgetImage _selectedCenter;
        private readonly WidgetImage _selectedRight;

        private readonly WidgetText _label;

        public bool IsActive { get; set; }

        public ClassTabButton(string labelText, string labelStyle)
        {
            _normalLeft = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Unselected_Left.tga");
            AddContent(_normalLeft);
            _normalCenter = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Unselected_Center.tga");
            AddContent(_normalCenter);
            _normalRight = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Unselected_Right.tga");
            AddContent(_normalRight);
            _selectedLeft = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Selected_Left.tga");
            AddContent(_selectedLeft);
            _selectedCenter = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Selected_Center.tga");
            AddContent(_selectedCenter);
            _selectedRight = new WidgetImage("art/interface/CHAR_UI/CHAR_SPELLS_UI/SubTab_Selected_Right.tga");
            AddContent(_selectedRight);

            _label = new WidgetText(labelText, labelStyle);
            AddContent(_label);

            var leftSize = _normalLeft.GetPreferredSize();
            leftSize.Width -= 1; // The image contains a 1px transparent border for whatever reason
            var labelSize = _label.GetPreferredSize();
            var rightSize = _normalRight.GetPreferredSize();
            rightSize.Width -= 1; // The image contains a 1px transparent border for whatever reason

            _normalLeft.FixedSize = _normalLeft.GetPreferredSize();
            _normalRight.FixedSize = _normalRight.GetPreferredSize();
            _selectedLeft.FixedSize = _selectedLeft.GetPreferredSize();
            _selectedRight.FixedSize = _selectedRight.GetPreferredSize();

            // Position content so that the label is center
            _label.X = leftSize.Width + HorizontalLabelPadding;
            _label.Y = 1;

            // Stretch the center tab images out to fit the label width
            _normalCenter.X = leftSize.Width;
            _normalCenter.FixedSize = _normalCenter.GetPreferredSize();
            _normalCenter.FixedWidth = labelSize.Width + 2 * HorizontalLabelPadding;
            _selectedCenter.X = leftSize.Width;
            _selectedCenter.FixedSize = _selectedCenter.GetPreferredSize();
            _selectedCenter.FixedWidth = labelSize.Width + 2 * HorizontalLabelPadding;

            _normalRight.X = leftSize.Width + labelSize.Width - 1 + 2 * HorizontalLabelPadding;
            _selectedRight.X = leftSize.Width + labelSize.Width - 1 + 2 * HorizontalLabelPadding;

            Width = leftSize.Width + labelSize.Width + rightSize.Width;
            Height = leftSize.Height;
        }

        public override void Render()
        {
            _normalLeft.Visible = !IsActive;
            _normalCenter.Visible = !IsActive;
            _normalRight.Visible = !IsActive;
            _selectedLeft.Visible = IsActive;
            _selectedCenter.Visible = IsActive;
            _selectedRight.Visible = IsActive;
            base.Render();
        }
    }
}