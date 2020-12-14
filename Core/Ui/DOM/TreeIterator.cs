using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTemple.Core.Ui.DOM
{
    public struct TreeEnumerator : IEnumerator<Node>
    {
        private readonly Node _root;
        private readonly IterationOrder _order;
        private Node _next;
        private bool _initialized;

        public TreeEnumerator(Node root, Node firstResult, IterationOrder order)
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
                IterationOrder.PREV => _next.PreviousSibling,
                IterationOrder.NEXT => _next.NextSibling,
                IterationOrder.PARENT => _next.Parent,
                IterationOrder.PRECEDING => _next.Preceding(_root),
                IterationOrder.FOLLOWING => _next.Following(_root),
                _ => throw new ArgumentOutOfRangeException()
            };

            return _next != null;
        }

        public void Reset()
        {
            throw new NotSupportedException ();
        }

        public Node Current => _next;

        object IEnumerator.Current => _next;

        public void Dispose()
        {
        }
    }

    public enum IterationOrder
    {
        PREV,
        NEXT,
        PARENT,
        PRECEDING,
        FOLLOWING
    }

    public readonly struct TreeIterator
    {
        private readonly Node _root;
        private readonly Node _firstResult;
        private readonly IterationOrder _order;

        public TreeIterator(Node root, Node firstResult, IterationOrder order)
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

}