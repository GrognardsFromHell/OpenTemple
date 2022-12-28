
using System;
using OpenTemple.Core.Ui.FlowModel;



namespace OpenTemple.Core.Ui.Widgets;

/// <summary>
/// This widget displays a text that can change anytime. It uses a supplier-function that is
/// queried every frame before rendering.
/// </summary>
public class WidgetDynamicLabel : WidgetBase
{
    private readonly WidgetText _text;

    private readonly Func<InlineElement?> _contentSupplier;

    private InlineElement? _previousContent;

    public WidgetDynamicLabel(Func<string?> contentSupplier)
        : this(() =>
        {
            var textContent = contentSupplier() ?? "";
            return textContent == "" ? null : new SimpleInlineElement(textContent);
        })
    {
    }

    public WidgetDynamicLabel(Func<InlineElement?> contentSupplier)
    {
        _text = new WidgetText();
        AddContent(_text);
        _contentSupplier = contentSupplier;
    }

    protected internal override void UpdateLayout(LayoutContext context)
    {
        var content = _contentSupplier();
        if (content == null)
        {
            _text.Content = _previousContent = null;
            return;
        }

        if (!content.Equals(_previousContent))
        {
            _text.Content = _previousContent = content;
        }

        base.UpdateLayout(context);
    }
}