using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTemple.Core.Ui.Widgets;

public struct TreeEnumerator : IEnumerator<WidgetBase>
{
    private readonly WidgetBase? _root;
    private readonly IterationOrder _order;
    private WidgetBase? _next;
    private bool _initialized;

    public TreeEnumerator(WidgetBase? root, WidgetBase firstResult, IterationOrder order)
    {
        _root = root;
        _next = firstResult;
        _order = order;
        _initialized = false;
    }

    public bool MoveNext()
    {
        if (!_initialized)
        {
            _initialized = true;
            return _next != null;
        }

        if (_next == null)
        {
            return false;
        }

        _next = _order switch
        {
            IterationOrder.Prev => _next.PreviousSibling,
            IterationOrder.Next => _next.NextSibling,
            IterationOrder.Parent => _next.Parent,
            IterationOrder.Preceding => _next.Preceding(_root),
            IterationOrder.Following => _next.Following(_root),
            _ => throw new ArgumentOutOfRangeException()
        };

        return _next != null;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public WidgetBase Current
    {
        get
        {
            Debug.Assert(_next != null);
            return _next;
        }
    }

    object IEnumerator.Current
    {
        get
        {
            Debug.Assert(_next != null);
            return _next;
        }
    }

    public void Dispose()
    {
    }
}

public enum IterationOrder
{
    Prev,
    Next,
    Parent,
    Preceding,
    Following
}

public readonly struct TreeIterator
{
    private readonly WidgetBase? _root;
    private readonly WidgetBase _firstResult;
    private readonly IterationOrder _order;

    public TreeIterator(WidgetBase? root, WidgetBase firstResult, IterationOrder order)
    {
        _root = root;
        _firstResult = firstResult;
        _order = order;
    }

    public TreeEnumerator GetEnumerator()
    {
        return new TreeEnumerator(_root, _firstResult, _order);
    }
}
