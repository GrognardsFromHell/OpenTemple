using System;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.Widgets
{

    public interface ITextInputElement
    {
        public void InputText(string text);
    }

    public class WidgetTextInput : WidgetBase, ITextInputElement
    {
        private readonly WidgetText _valueText = new();

        private readonly StringBuilder _value = new();

        private bool _dirty = true;

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
            AddContent(_valueText);
        }

        public string Value
        {
            get => _value.ToString();
            set
            {
                _dirty = true;
                _value.Clear();
                _value.Append(value);
            }
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();

            if (!_dirty)
            {
                return;
            }

            _dirty = false;
            _valueText.SetText(_value.ToString());
        }

        public void InputText(string text)
        {
            _value.Append(text);
            _dirty = true;
        }
    }
}