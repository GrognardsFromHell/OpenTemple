#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.FlowModel;

public class Paragraph : Block, IInlineContainer
{
    private InlineChildren _children;

    private TextFlow? _textFlow;

    public IInlineContainer? Parent => null;

    public IReadOnlyList<InlineElement> Children => _children.Children;

    public void AppendContent(InlineElement inlineElement) => _children.Append(this, inlineElement);

    public void RemoveContent(InlineElement inlineElement) => _children.Remove(this, inlineElement);

    public void ClearContent()
    {
        _children.Clear(this);
    }

    public bool IsEmpty => _children.Children.Count == 0;

    public SimpleInlineElement? GetSourceElementAt(int index)
    {
        foreach (var element in TextFlow.Elements)
        {
            if (index >= element.Start && index < element.Start + element.Length)
            {
                return element.Source;
            }
        }

        return null;
    }

    public string TextContent
    {
        get
        {
            var result = new StringBuilder();
            ((IInlineContainer) this).VisitInFlowDirection(element => { result.Append(element.Text ?? ""); });
            return result.ToString();
        }
    }

    public void Invalidate(InvalidationFlags flags)
    {
        if ((flags & InvalidationFlags.TextFlow) != 0)
        {
            _textFlow = null;
            Host?.NotifyTextFlowChanged();
        }

        if ((flags & InvalidationFlags.Style) != 0)
        {
            Host?.NotifyStyleChanged();
        }
    }

    public TextFlow TextFlow
    {
        get
        {
            if (_textFlow == null)
            {
                var text = new StringBuilder();
                var flow = new List<TextFlow.Element>();
                ((IInlineContainer) this).VisitInFlowDirection(element =>
                {
                    var elementText = element.Text ?? "";
                    var flowElement = new TextFlow.Element(text.Length, elementText.Length, element);
                    flow.Add(flowElement);
                    text.Append(elementText);
                });

                _textFlow = new TextFlow(text.ToString(), flow);
            }

            return _textFlow;
        }
    }

    [Flags]
    public enum InvalidationFlags
    {
        TextFlow = 1,
        Style = 2
    }

    /// <summary>
    /// React to external changes to styles.
    /// </summary>
    protected override void OnStylesInvalidated()
    {
        base.OnStylesInvalidated();
        foreach (var child in _children.Children)
        {
            child.InvalidateStyles();
        }
    }
    
    // TODO: This is incorrect. Paragraphs should have their own pseudo-class state.
    public override StylingState PseudoClassState => default;
}
