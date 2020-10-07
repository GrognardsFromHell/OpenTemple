using System;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options
{
    public class SliderOption : Option
    {
        private readonly Action<int> _setter;

        private readonly Func<int> _getter;

        public int MinValue { get; }

        public int MaxValue { get; }

        public virtual bool IsPercentage { get; }

        public SliderOption(string label, Func<int> getter, Action<int> setter, int minValue, int maxValue) :
            base(label)
        {
            _getter = getter;
            _setter = setter;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public Action<int> OnChanged { get; set; }

        public int Value
        {
            get => _getter();
            set => _setter(value);
        }
    }
}