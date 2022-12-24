using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace OpenTemple.Core.Ui.FlowModel;

/// <summary>
/// A simple inline element can only contain text, but no other inline elements.
/// </summary>
public class TranslatedInlineElement : InlineElement, IInlineContainer
{
    private IReadOnlyList<InlineElement>? _children;

    public TranslatedInlineElement(string translationId, params object[] args)
    {
        TranslationId = translationId;
        Args = args;
    }

    public string TranslationId { get; }

    public object[] Args { get; }

    public IReadOnlyList<InlineElement> Children
    {
        get
        {
            _children ??= BuildContent(TranslationId, Args);
            return _children;
        }
    }

    public bool IsEmpty => false;

    public override string TextContent
    {
        get
        {
            var result = new StringBuilder();
            ((IInlineContainer) this).VisitInFlowDirection(element => { result.Append(element.Text ?? ""); });
            return result.ToString();
        }
    }

    protected override void OnStylesInvalidated()
    {
        _children = null; // Fully reset content
    }

    private IReadOnlyList<InlineElement> BuildContent(string translationId, params object[] args)
    {
        IReadOnlyList<InlineElement> elements;
        if (args.Length == 0)
        {
            // Simplified version, no sub-elements are possible
            elements = ImmutableList.Create(new SimpleInlineElement(Globals.UiAssets.ApplyTranslation("#{" + translationId + "}")));
        }
        else
        {
            throw new InvalidOperationException("Formatting arguments for translations are not yet supported");
        }

        foreach (var element in elements)
        {
            element.Parent = this;
        }

        Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);
        return elements;
    }
}