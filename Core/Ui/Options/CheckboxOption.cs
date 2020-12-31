using System;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Options
{
    /// <summary>
    /// A simple option to toggle a boolean value on or off.
    /// </summary>
    public class CheckboxOption : Option
    {
        private readonly Action<bool> _setter;

        private readonly Func<bool> _getter;

        private bool _currentValue;

        public CheckboxOption(string label, Func<bool> getter, Action<bool> setter) : base(label)
        {
            _getter = getter;
            _setter = setter;
        }

        public bool Value
        {
            get => _currentValue;
            set => this.RaiseAndSetIfChanged(ref _currentValue, value);
        }

        public override void Reset()
        {
            Value = _getter();
        }

        public override void Apply()
        {
            _setter(Value);
        }
    }
}