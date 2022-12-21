
using System.Collections.Generic;
using System.Text;

namespace OpenTemple.Core.Ui.FlowModel;

public class ComplexInlineElement : InlineElement, IInlineContainer
{
    private InlineChildren _children;

    public IReadOnlyList<InlineElement> Children => _children.Children;

    public bool IsEmpty => _children.Children.Count == 0;

    public void AppendContent(InlineElement inlineElement) => _children.Append(this, inlineElement);

    public void RemoveContent(InlineElement inlineElement) => _children.Remove(this, inlineElement);

    public void ClearContent() => _children.Clear(this);

    public override string TextContent
    {
        get
        {
            var result = new StringBuilder();
            ((IInlineContainer)this).VisitInFlowDirection(element => { result.Append(element.Text ?? ""); });
            return result.ToString();
        }
    }

    protected override void OnStylesInvalidated()
    {
        foreach (var child in _children.Children)
        {
            child.InvalidateStyles();
        }
    }
}