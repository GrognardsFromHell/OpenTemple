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

    protected override void ApplyAutomaticSizing(LayoutContext context)
    {
        base.ApplyAutomaticSizing(context);
        
        if (AutoSizeWidth || AutoSizeHeight)
        {
            var preferredSize = _text.GetPreferredSize();
            if (AutoSizeWidth)
            {
                Width = Dimension.Pixels(preferredSize.Width);
            }
            if (AutoSizeHeight)
            {
                Height = Dimension.Pixels(preferredSize.Height);
            }
        }
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