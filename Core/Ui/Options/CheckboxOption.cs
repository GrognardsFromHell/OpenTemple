using System;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options
{
    /// <summary>
    /// A simple option to toggle a boolean value on or off.
    /// </summary>
    public class CheckboxOption : Option
    {
        private readonly Action<bool> _setter;

        private readonly Func<bool> _getter;

        private readonly WidgetCheckbox _checkbox;

        public CheckboxOption(string label, Func<bool> getter, Action<bool> setter) : base(label)
        {
            _checkbox = new WidgetCheckbox(399, 9);
            _getter = getter;
            _setter = setter;
        }

        public override void AddTo(WidgetContainer container)
        {
            container.Add(_checkbox);
        }

        public override void Reset()
        {
            _checkbox.Checked = _getter();
        }

        public override void Apply()
        {
            _setter(_checkbox.Checked);
        }
    }
}