using System;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Options
{
    public class SliderOption : Option
    {
        private readonly Action<int> _setter;

        private readonly Func<int> _getter;

        private int _value;

        public int MinValue { get; }

        public int MaxValue { get; }

        public int Value { get => _value; set => this.RaiseAndSetIfChanged(ref _value, value); }

        public SliderOption(string label, Func<int> getter, Action<int> setter, int minValue, int maxValue) :
            base(label)
        {
            _getter = getter;
            _setter = setter;

            MinValue = minValue;
            MaxValue = maxValue;

          // _slider = new WidgetSlider(399, 9);
          // _slider.SetMin(minValue);
          // _slider.SetMax(maxValue);
          // _slider.X = 178;
          // _slider.Y = 2;

          // // Display the slider's min value to the left of the slider
          // _minLabel = new WidgetText(_slider.GetMin().ToString(), "options-label-muted");
          // _minLabel.SetX(_slider.X - _minLabel.GetPreferredSize().Width - 5);
          // _minLabel.SetCenterVertically(true);

          // // Display the slider's max value to the left of the slider
          // _maxLabel = new WidgetText(_slider.GetMax().ToString(), "options-label-muted");
          // _maxLabel.SetX(_slider.X + _slider.Width + 5);
          // _maxLabel.SetCenterVertically(true);

          // // Display the current value to the far right
          // _valueLabel = new WidgetText(_slider.GetValue().ToString(), "options-label");
          // _valueLabel.SetX(_slider.X + _slider.Width + 40);
          // _valueLabel.SetCenterVertically(true);
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