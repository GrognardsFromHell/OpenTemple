using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace OpenTemple.Core.Ui.FlowModel;

internal struct InlineChildren
{
    private List<InlineElement>? _children;

    public IReadOnlyList<InlineElement> Children
    {
        get
        {
            IReadOnlyList<InlineElement>? result = _children;
            return result ?? ImmutableList<InlineElement>.Empty;
        }
    }

    public void Append(IMutableInlineContainer parent, InlineElement inlineElement)
    {
        if (inlineElement.Parent != null)
        {
            inlineElement.Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);
            if (inlineElement.Paragraph is IMutableInlineContainer mutableContainer)
            {
                mutableContainer.RemoveContent(inlineElement);
            }
            else
            {
                throw new InvalidOperationException("Cannot remove an inline element from an immutable parent.");
            }
        }

        _children ??= new List<InlineElement>();
        _children.Add(inlineElement);

        inlineElement.Parent = parent;
        inlineElement.Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);
    }

    public void Remove(IMutableInlineContainer parent, InlineElement inlineElement)
    {
        if (inlineElement.Parent != parent)
        {
            throw new ArgumentException("Element is not in this parent");
        }

        inlineElement.Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);

        if (_children != null && _children.Remove(inlineElement))
        {
            if (_children.Count == 0)
            {
                _children = null;
            }
        }

        inlineElement.Parent = null;
    }

    public void Clear(IMutableInlineContainer parent)
    {
        for (var i = parent.Children.Count - 1; i >= 0; i--)
        {
            var child = parent.Children[i];
            parent.RemoveContent(child);
        }
    }
}