

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTemple.Core.Ui.FlowModel;

/// <summary>
/// A container for inline content that allows the content to be modified.
/// </summary>
public interface IMutableInlineContainer : IInlineContainer
{
    void AppendContent(InlineElement inlineElement);

    void RemoveContent(InlineElement inlineElement);

    void ClearContent();
}
