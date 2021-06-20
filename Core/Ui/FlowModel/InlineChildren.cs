#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;

namespace OpenTemple.Core.Ui.FlowModel
{
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

        public void Append(IInlineContainer parent, InlineElement inlineElement)
        {
            if (inlineElement.Parent != null)
            {
                inlineElement.Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);
                inlineElement.Parent?.RemoveContent(inlineElement);
            }

            _children ??= new List<InlineElement>();
            _children.Add(inlineElement);

            inlineElement.Parent = parent;
            inlineElement.Paragraph?.Invalidate(Paragraph.InvalidationFlags.TextFlow);
        }

        public void Remove(IInlineContainer parent, InlineElement inlineElement)
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

        public void Clear(IInlineContainer parent)
        {
            for (var i = parent.Children.Count - 1; i >= 0; i--)
            {
                var child = parent.Children[i];
                parent.RemoveContent(child);
            }
        }
    }
}