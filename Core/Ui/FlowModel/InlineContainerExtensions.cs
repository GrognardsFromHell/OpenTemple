
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.FlowModel;

public static class InlineContainerExtensions
{
    public static SimpleInlineElement AppendContent(this IInlineContainer container, string text,
        IStyleDefinition? localStyle = null)
    {
        var element = new SimpleInlineElement()
        {
            Text = text
        };
        localStyle?.MergeInto(element.LocalStyles);
        container.AppendContent(element);
        return element;
    }

    public static SimpleInlineElement AppendContent(this IInlineContainer container, string text,
        params string[] styles)
    {
        var element = AppendContent(container, text);
        foreach (var style in styles)
        {
            element.AddStyle(style);
        }

        return element;
    }
}