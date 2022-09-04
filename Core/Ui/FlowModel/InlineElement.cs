using System;
using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.Ui.Styles;

#nullable enable

namespace OpenTemple.Core.Ui.FlowModel;

public abstract class InlineElement : Styleable
{
    private StylingState _stylingStates;

    public override IStyleable? StyleParent => Parent as IStyleable;

    public IInlineContainer? Parent { get; set; }

    public IFlowContentHost? Host => Paragraph?.Host;

    /// <summary>
    /// Returns the paragraph this inline element is a part of.
    /// </summary>
    public Paragraph? Paragraph
    {
        get
        {
            var parent = Parent;
            while (parent != null)
            {
                if (parent is Paragraph paragraph)
                {
                    return paragraph;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }

    public IEnumerable<InlineElement> EnumerateAncestors(bool includeSelf = true)
    {
        if (includeSelf)
        {
            yield return this;
        }

        for (var element = Parent; element != null; element = element.Parent)
        {
            if (element is InlineElement inlineElement)
            {
                yield return inlineElement;
            }
        }
    }

    public override StylingState PseudoClassState => _stylingStates;

    public void ToggleStylingState(StylingState stylingState, bool enable)
    {
        if (BitOperations.PopCount((uint) stylingState) != 1)
        {
            throw new ArgumentException(
                "Can only set or unset a single styling-state at once: " + (uint) stylingState);
        }

        var currentState = (_stylingStates & stylingState) != 0;
        if (currentState != enable)
        {
            if (enable)
            {
                _stylingStates |= stylingState;
            }
            else
            {
                _stylingStates &= ~stylingState;
            }

            InvalidateStyles();
        }
    }

    /// <summary>
    /// Finds the nearest ancestor of type T or returns this element if it's already of type T.
    /// </summary>
    public T? Closest<T>() where T : InlineElement
    {
        if (this is T)
        {
            return (T) this;
        }

        if (Parent is InlineElement inlineElementParent)
        {
            return inlineElementParent.Closest<T>();
        }
        else if (Parent is T parentT)
        {
            return parentT;
        }

        return null;
    }

    protected override void OnStylesInvalidated()
    {
        Host?.NotifyStyleChanged();
    }

    public abstract string TextContent { get; }
}