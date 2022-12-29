using System.Runtime.CompilerServices;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetLabel : WidgetBase
{
    private readonly WidgetText _text = new();

    public WidgetLabel([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = -1) : base(filePath, lineNumber)
    {
        AddContent(_text);
    }

    public new InlineElement? Content
    {
        set { _text.Content = value; }
    }

    public string Text
    {
        set => _text.Text = value;
    }
}