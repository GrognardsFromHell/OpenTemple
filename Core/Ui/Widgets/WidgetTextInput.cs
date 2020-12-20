using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets
{

    public interface ITextInputElement
    {
        public void InputText(string text);
    }

    public class WidgetTextInput : WidgetBase, ITextInputElement
    {
        private readonly TextBlock _textBlock = Tig.RenderingDevice.GetTextEngine().CreateTextBlock();

        private readonly StringBuilder _value = new();

        private bool _dirty = true;

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
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

        public override void Render()
        {
            base.Render();

            if (Visible)
            {
                if (_dirty)
                {
                    _dirty = false;
                    _textBlock.SetText(_value.ToString());
                }

                var contentRect = GetContentArea();
                _textBlock.Render(contentRect.X, contentRect.Y);
            }
        }

        public void InputText(string text)
        {
            _value.Append(text);
            _dirty = true;
        }
    }
}