using System;
using System.Drawing;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Camping
{
    public class CampingCheckbox : WidgetButtonBase
    {
        private readonly WidgetImage _checked;

        private readonly WidgetImage _unchecked;

        private readonly WidgetText _label;

        public bool Checked
        {
            get => _checked.Visible;
            set
            {
                _checked.Visible = value;
                _unchecked.Visible = !value;
            }
        }

        public event Action<bool> OnCheckedChange;

        public string Label
        {
            set => _label.SetText(value);
        }

        public CampingCheckbox(Rectangle rect, string labelText, string labelStyle) : base(rect)
        {
            _label = new WidgetText(labelText, labelStyle);
            _label.SetX(18);
            _label.SetY(1);
            AddContent(_label);

            _checked = new WidgetImage("art/interface/utility_bar_ui/checkbox_on.tga");
            _checked.FixedSize = new Size(15, 15);
            AddContent(_checked);

            _unchecked = new WidgetImage("art/interface/utility_bar_ui/checkbox_off.tga");
            _unchecked.FixedSize = new Size(15, 15);
            AddContent(_unchecked);

            Checked = false;
            SetClickHandler(OnClick);
        }

        private void OnClick()
        {
            Checked = !Checked;
            OnCheckedChange?.Invoke(Checked);
        }
    }
}