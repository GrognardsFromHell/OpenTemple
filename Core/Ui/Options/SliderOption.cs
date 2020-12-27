using System;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options
{
    public class SliderOption : Option
    {
        private readonly Action<int> _setter;

        private readonly Func<int> _getter;

        protected readonly WidgetSlider _slider;

        protected readonly WidgetText _minLabel;

        protected readonly WidgetText _maxLabel;

        protected readonly WidgetText _valueLabel;

        public SliderOption(string label, Func<int> getter, Action<int> setter, int minValue, int maxValue) :
            base(label)
        {
            _getter = getter;
            _setter = setter;

            _slider = new WidgetSlider(399, 9);
            _slider.SetMin(minValue);
            _slider.SetMax(maxValue);
            _slider.X = 178;
            _slider.Y = 2;

            // Display the slider's min value to the left of the slider
            _minLabel = new WidgetText(_slider.GetMin().ToString(), "options-label-muted");
            _minLabel.X = _slider.X - _minLabel.GetPreferredSize().Width - 5;
            _minLabel.SetCenterVertically(true);

            // Display the slider's max value to the left of the slider
            _maxLabel = new WidgetText(_slider.GetMax().ToString(), "options-label-muted");
            _maxLabel.X = _slider.X + _slider.Width + 5;
            _maxLabel.SetCenterVertically(true);

            // Display the current value to the far right
            _valueLabel = new WidgetText(_slider.GetValue().ToString(), "options-label");
            _valueLabel.X = _slider.X + _slider.Width + 40;
            _valueLabel.SetCenterVertically(true);
            _slider.SetValueChangeHandler(ValueChanged);
        }

        protected virtual void ValueChanged(int newValue)
        {
            _valueLabel.SetText(newValue.ToString());
        }

        public override void AddTo(WidgetContainer container)
        {
            container.Add(_slider);
            container.AddContent(_minLabel);
            container.AddContent(_maxLabel);
            container.AddContent(_valueLabel);
        }

        public override void Reset()
        {
            _slider.SetValue(_getter());
        }

        public override void Apply()
        {
            _setter(_slider.GetValue());
        }
    }
}