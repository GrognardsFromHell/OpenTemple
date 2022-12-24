
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.FlowModel;

public static class InlineContainerExtensions
{
    public static SimpleInlineElement AppendContent(this IMutableInlineContainer container, string text,
        IStyleDefinition? localStyle = null)
    {
        var element = new SimpleInlineElement
        {
            Text = text
        };
        localStyle?.MergeInto(element.LocalStyles);
        container.AppendContent(element);
        return element;
    }

    public static SimpleInlineElement AppendContent(this IMutableInlineContainer container, string text,
        params string[] styles)
    {
        var element = AppendContent(container, text);
        foreach (var style in styles)
        {
            element.AddStyle(style);
        }

        return element;
    }
    
    public static void AppendBreak(this IMutableInlineContainer container)
    {
        container.AppendContent("\n");
    }
    
    public static InlineElement AppendTranslation(this IMutableInlineContainer container, string translationId, params object[] args)
    {
        var element = new TranslatedInlineElement(translationId, args);
        container.AppendContent(element);
        return element;
    }
}
