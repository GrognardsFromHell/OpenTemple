#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTemple.Core.Ui.FlowModel;

/// <summary>
/// Container for inline elements (paragraphs or inlines themselves can contain other inlines).
/// </summary>
public interface IInlineContainer
{
    IInlineContainer? Parent { get; }

    IReadOnlyList<InlineElement> Children { get; }

    bool IsEmpty { get; }

    void AppendContent(InlineElement inlineElement);

    void RemoveContent(InlineElement inlineElement);

    void ClearContent();

    /// <summary>
    /// NOTE: No RTL support yet
    /// </summary>
    void VisitInFlowDirection(Action<SimpleInlineElement> visitor)
    {
        foreach (var child in Children)
        {
            if (child is SimpleInlineElement simpleInlineElement)
            {
                visitor(simpleInlineElement);
            }
            else if (child is IInlineContainer container)
            {
                container.VisitInFlowDirection(visitor);
            }
        }
    }
}