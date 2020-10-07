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

        public CheckboxOption(string label, Func<bool> getter, Action<bool> setter) : base(label)
        {
            _getter = getter;
            _setter = setter;
        }

        public Action OnChanged { get; set; }

        public bool Value
        {
            get => _getter();
            set => _setter(value);
        }
    }
}