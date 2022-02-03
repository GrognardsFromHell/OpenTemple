#nullable enable
namespace OpenTemple.Core.Ui.FlowModel;

/// <summary>
/// A simple inline element can only contain text, but no other inline elements.
/// </summary>
public class SimpleInlineElement : InlineElement
{
    public SimpleInlineElement()
    {
    }

    public SimpleInlineElement(string text)
    {
        Text = text;
    }

    public string? Text { get; init; }

    public override string TextContent => Text ?? "";
}