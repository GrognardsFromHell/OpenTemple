using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenTemple.Core.Ui.DOM
{
    public abstract class Node : EventTargetImpl
    {
        public Document OwnerDocument { get; internal set; }

        public abstract NodeType NodeType { get; }

        public abstract string NodeName { get; }

        public Node Parent { get; private set; }

        internal readonly ISet<Range> _referencedRanges = new HashSet<Range>();

        internal struct ObserverRegistration
        {
            public readonly MutationObserver observer;
            public readonly MutationObserverInit options;
        }

        internal readonly List<ObserverRegistration> _registeredObserverList = new List<ObserverRegistration>();

        private NodeList _childNodesList = null;
        private NodeList _childrenList = null;
        private readonly Dictionary<object, object> _memoizedQueries = new Dictionary<object, object>(); // TODO

        private int _version;

        public Node(Document ownerDocument)
        {
            OwnerDocument = (Document) ownerDocument;
        }

        [MaybeNull]
        public Node ParentNode
        {
            get => Parent;
        }

        [MaybeNull]
        public virtual Element ClosestElement => ParentElement;

        [MaybeNull]
        public Element ParentElement
        {
            get
            {
                if (Parent is Element parentEl)
                {
                    return parentEl;
                }

                return null;
            }
        }

        [MaybeNull]
        public Node FirstChild { get; private set; }

        [MaybeNull]
        public Node LastChild { get; private set; }

        [MaybeNull]
        public Node PreviousSibling { get; private set; }

        [MaybeNull]
        public Node NextSibling { get; private set; }

        protected override EventTargetImpl _getTheParent(EventImpl evt)
        {
            return Parent;
        }

        // https://dom.spec.whatwg.org/#concept-node-ensure-pre-insertion-validity
        internal void _preInsertValidity(Node nodeImpl, [MaybeNull]
            Node childImpl)
        {
            var nodeType = nodeImpl.NodeType;
            var nodeName = nodeImpl.NodeName;
            var parentType = nodeType;
            var parentName = NodeName;

            if (
                parentType != NodeType.DOCUMENT_NODE &&
                parentType != NodeType.DOCUMENT_FRAGMENT_NODE &&
                parentType != NodeType.ELEMENT_NODE
            )
            {
                throw new DOMException(
                    $"Node can't be inserted in a {parentName} parent.",
                    "HierarchyRequestError"
                );
            }

            if (nodeImpl.IsInclusiveAncestor(this))
            {
                throw new DOMException(
                    "The operation would yield an incorrect node tree.",
                    "HierarchyRequestError"
                );
            }

            if (childImpl != null && childImpl.Parent != this)
            {
                throw new DOMException(
                    "The child can not be found in the parent.",
                    "NotFoundError"
                );
            }

            if (
                nodeType != NodeType.DOCUMENT_FRAGMENT_NODE &&
                nodeType != NodeType.ELEMENT_NODE &&
                nodeType != NodeType.TEXT_NODE &&
                nodeType != NodeType.CDATA_SECTION_NODE && // CData section extends from Text
                nodeType != NodeType.COMMENT_NODE
            )
            {
                throw new DOMException(
                    $"{nodeName} node can't be inserted in parent node.",
                    "HierarchyRequestError"
                );
            }

            if (
                (nodeType == NodeType.TEXT_NODE && parentType == NodeType.DOCUMENT_NODE)
            )
            {
                throw new DOMException(
                    $"{nodeName} node can't be inserted in {parentName} parent.",
                    "HierarchyRequestError"
                );
            }

            if (parentType == NodeType.DOCUMENT_NODE)
            {
                var nodeChildren = nodeImpl.ChildrenToArray();
                var parentChildren = this.ChildrenToArray();

                switch (nodeType)
                {
                    case NodeType.DOCUMENT_FRAGMENT_NODE:
                    {
                        var nodeChildrenElements = nodeChildren.Count(child => child.NodeType == NodeType.ELEMENT_NODE);
                        if (nodeChildrenElements > 1)
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        var hasNodeTextChildren = nodeChildren.Any(child => child.NodeType == NodeType.TEXT_NODE);
                        if (hasNodeTextChildren)
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        if (
                            nodeChildrenElements == 1 &&
                            (
                                parentChildren.Any(child => child.NodeType == NodeType.ELEMENT_NODE)
                            )
                        )
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        break;
                    }

                    case NodeType.ELEMENT_NODE:
                        if (
                            parentChildren.Any(child => child.NodeType == NodeType.ELEMENT_NODE)
                        )
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        break;
                }
            }
        }

        public Node InsertBefore(Node node, [MaybeNull]
            Node child)
        {
            _preInsertValidity(node, child);

            var referenceChild = child;
            if (referenceChild == node)
            {
                referenceChild = node.NextSibling;
            }

            OwnerDocument._adoptNode(node);

            this._insert(node, referenceChild);

            return node;
        }

        // https://dom.spec.whatwg.org/#concept-node-insert
        internal void _insert(Node node, Node beforeNode, bool suppressObservers = false)
        {
            var documentFragment = node as DocumentFragment;
            var count = documentFragment?.ChildrenCount ?? 1;

            if (beforeNode != null)
            {
                var childIndex = beforeNode.Index;

                foreach (var range in this._referencedRanges)
                {
                    var start = range.Start;
                    var end = range.End;

                    if (start.Offset > childIndex)
                    {
                        range.SetLiveRangeStart(this, start.Offset + count);
                    }

                    if (end.Offset > childIndex)
                    {
                        range.SetLiveRangeEnd(this, end.Offset + count);
                    }
                }
            }

            var nodesImpl = node.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE
                ? node.ChildrenToArray()
                : new List<Node>() {node};

            if (node.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE)
            {
                Node grandChildImpl;
                while ((grandChildImpl = node.FirstChild) != null)
                {
                    node._remove(grandChildImpl, true);
                }
            }

            if (node.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE)
            {
                MutationObserverImpl.QueueTreeMutationRecord(node, new List<Node>(), nodesImpl, null, null);
            }

            var previousChildImpl = (beforeNode != null) ? beforeNode.PreviousSibling : LastChild;

            foreach (var childNode in nodesImpl)
            {
                if (childNode.IsAttached)
                {
                    throw new Exception("Given object is already present in this SymbolTree, remove it first");
                }

                childNode.Parent = this;
                if (beforeNode == null)
                {
                    // Simply insert at the end of the tree
                    if (LastChild != null)
                    {
                        childNode.PreviousSibling = LastChild;
                        LastChild.NextSibling = childNode;
                    }
                    else
                    {
                        FirstChild = childNode;
                    }

                    LastChild = childNode;
                }
                else
                {
                    var prevNode = beforeNode.PreviousSibling;

                    childNode.PreviousSibling = prevNode;
                    childNode.NextSibling = beforeNode;
                    beforeNode.PreviousSibling = childNode;

                    if (prevNode != null)
                    {
                        prevNode.NextSibling = childNode;
                    }

                    if (FirstChild == beforeNode)
                    {
                        FirstChild = childNode;
                    }
                }

                ChildrenChanged();

                this._modified();

                if (childNode.NodeType == NodeType.TEXT_NODE ||
                    childNode.NodeType == NodeType.CDATA_SECTION_NODE)
                {
                    this._childTextContentChangeSteps();
                }

                if (this._attached)
                {
                    childNode._attach();
                }

                this._descendantAdded(this, childNode);
            }

            if (!suppressObservers)
            {
                MutationObserverImpl.QueueTreeMutationRecord(this, nodesImpl, new List<Node>(), previousChildImpl,
                    beforeNode);
            }
        }


        // https://dom.spec.whatwg.org/#concept-node-replace
        private Node _replace(Node nodeImpl, Node childImpl)
        {
            var nodeType = nodeImpl.NodeType;
            var nodeName = nodeImpl.NodeName;
            var parentType = NodeType;
            var parentName = NodeName;

            // Note: This section differs from the pre-insert validation algorithm.
            if (
                parentType != NodeType.DOCUMENT_NODE &&
                parentType != NodeType.DOCUMENT_FRAGMENT_NODE &&
                parentType != NodeType.ELEMENT_NODE
            )
            {
                throw new DOMException(
                    $"Node can't be inserted in a {parentName} parent.",
                    "HierarchyRequestError"
                );
            }

            if (nodeImpl.IsInclusiveAncestor(this))
            {
                throw new DOMException(
                    "The operation would yield an incorrect node tree.",
                    "HierarchyRequestError"
                );
            }

            if (childImpl.Parent != this)
            {
                throw new DOMException(
                    "The child can not be found in the parent.",
                    "NotFoundError"
                );
            }

            if (
                nodeType != NodeType.DOCUMENT_FRAGMENT_NODE &&
                nodeType != NodeType.ELEMENT_NODE &&
                nodeType != NodeType.TEXT_NODE &&
                nodeType != NodeType.CDATA_SECTION_NODE && // CData section extends from Text
                nodeType != NodeType.COMMENT_NODE
            )
            {
                throw new DOMException(
                    $"{nodeName} node can't be inserted in parent node.",
                    "HierarchyRequestError"
                );
            }

            if (
                nodeType == NodeType.TEXT_NODE && parentType == NodeType.DOCUMENT_NODE
            )
            {
                throw new DOMException(
                    $"{nodeName} node can't be inserted in {parentName} parent.",
                    "HierarchyRequestError"
                );
            }

            if (parentType == NodeType.DOCUMENT_NODE)
            {
                var nodeChildren = nodeImpl.ChildrenToArray();
                var parentChildren = ChildrenToArray();

                switch (nodeType)
                {
                    case NodeType.DOCUMENT_FRAGMENT_NODE:
                    {
                        var nodeChildrenElements = nodeChildren.Where(child => child.NodeType == NodeType.ELEMENT_NODE)
                            .ToArray();
                        if (nodeChildrenElements.Length > 1)
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        var hasNodeTextChildren = nodeChildren.Any(child => child.NodeType == NodeType.TEXT_NODE);
                        if (hasNodeTextChildren)
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }


                        var parentChildElements = parentChildren.Where(child => child.NodeType == NodeType.ELEMENT_NODE)
                            .ToArray();
                        if (
                            nodeChildrenElements.Length == 1 &&
                            (
                                (parentChildElements.Length == 1 && parentChildElements[0] != childImpl)
                            )
                        )
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        break;
                    }

                    case NodeType.ELEMENT_NODE:
                        if (
                            parentChildren.Any(child => child.NodeType == NodeType.ELEMENT_NODE && child != childImpl)
                        )
                        {
                            throw new DOMException(
                                $"Invalid insertion of {nodeName} node in {parentName} node.",
                                "HierarchyRequestError"
                            );
                        }

                        break;
                }
            }

            var referenceChildImpl = childImpl.NextSibling;
            if (referenceChildImpl == nodeImpl)
            {
                referenceChildImpl = nodeImpl.NextSibling;
            }

            var previousSiblingImpl = childImpl.PreviousSibling;

            this.OwnerDocument._adoptNode(nodeImpl);

            var removedNodesImpl = new List<Node>();

            if (childImpl.Parent != null)
            {
                removedNodesImpl.Add(childImpl);
                this._remove(childImpl, true);
            }

            var nodesImpl = nodeImpl.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE
                ? nodeImpl.ChildrenToArray()
                : new List<Node>() {nodeImpl};

            this._insert(nodeImpl, referenceChildImpl, true);

            MutationObserverImpl.QueueTreeMutationRecord(this, nodesImpl, removedNodesImpl, previousSiblingImpl,
                referenceChildImpl);

            return childImpl;
        }

        // https://dom.spec.whatwg.org/#concept-node-replace-all
        private void _replaceAll(Node nodeImpl)
        {
            if (nodeImpl != null)
            {
                this.OwnerDocument._adoptNode(nodeImpl);
            }

            var removedNodesImpl = ChildrenToArray();

            List<Node> addedNodesImpl;
            if (nodeImpl == null)
            {
                addedNodesImpl = new List<Node>();
            }
            else if (nodeImpl.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE)
            {
                addedNodesImpl = nodeImpl.ChildrenToArray();
            }
            else
            {
                addedNodesImpl = new List<Node>() {nodeImpl};
            }

            foreach (var childImpl in ChildrenIterator())
            {
                this._remove(childImpl, true);
            }

            if (nodeImpl != null)
            {
                this._insert(nodeImpl, null, true);
            }

            if (addedNodesImpl.Count > 0 || removedNodesImpl.Count > 0)
            {
                MutationObserverImpl.QueueTreeMutationRecord(this, addedNodesImpl, removedNodesImpl, null, null);
            }
        }


        // https://dom.spec.whatwg.org/#concept-node-remove
        internal void _remove(Node nodeImpl, bool suppressObservers = false)
        {
            var index = nodeImpl.Index;

            foreach (var descendant in nodeImpl.TreeIterator())
            {
                foreach (var range in descendant._referencedRanges)
                {
                    if (range.StartContainer == descendant)
                    {
                        range.SetLiveRangeStart(this, index);
                    }

                    if (range.EndContainer == descendant)
                    {
                        range.SetLiveRangeEnd(this, index);
                    }
                }
            }

            foreach (var range in this._referencedRanges)
            {
                var _start = range.Start;
                var _end = range.End;

                if (_start.Node == this && _start.Offset > index)
                {
                    range.SetLiveRangeStart(this, _start.Offset - 1);
                }

                if (_end.Node == this && _end.Offset > index)
                {
                    range.SetLiveRangeEnd(this, _end.Offset - 1);
                }
            }

            OwnerDocument?._runPreRemovingSteps(nodeImpl);

            var oldPreviousSiblingImpl = nodeImpl.PreviousSibling;
            var oldNextSiblingImpl = nodeImpl.NextSibling;

            nodeImpl.remove();

            this._modified();
            nodeImpl._detach();
            this._descendantRemoved(this, nodeImpl);

            if (!suppressObservers)
            {
                MutationObserverImpl.QueueTreeMutationRecord(this, new List<Node>(), new List<Node>() {nodeImpl},
                    oldPreviousSiblingImpl, oldNextSiblingImpl);
            }

            if (nodeImpl.NodeType == NodeType.TEXT_NODE)
            {
                this._childTextContentChangeSteps();
            }
        }

        protected virtual void _modified()
        {
            foreach (var ancestor in AncestorsIterator())
            {
                ancestor._version++;
            }

            _childrenList?._update();
            _childNodesList?._update();
            _clearMemoizedQueries();
        }

        protected virtual void _childTextContentChangeSteps()
        {
            // Default: do nothing
        }

        private void _clearMemoizedQueries()
        {
            this._memoizedQueries.Clear();
            Parent?._clearMemoizedQueries();
        }

        protected virtual void _descendantRemoved(Node parent, Node child)
        {
            Parent?._descendantRemoved(parent, child);
        }

        protected virtual void _descendantAdded(Node parent, Node child)
        {
            Parent?._descendantAdded(parent, child);
        }

        private bool _attached;

        protected virtual void _attach()
        {
            this._attached = true;

            foreach (var child in ChildrenIterator())
            {
                child._attach();
            }
        }

        protected virtual void _detach()
        {
            this._attached = false;

            foreach (var child in ChildrenIterator())
            {
                child._detach();
            }
        }

        public Node AppendChild(Node node) => InsertBefore(node, null);

        public Node ReplaceChild(Node node, Node child)
        {
            return _replace(node, child);
        }

        // https://dom.spec.whatwg.org/#concept-node-pre-remove
        public Node RemoveChild(Node child)
        {
            if (child.Parent != this)
            {
                throw new DOMException(
                    "The node to be removed is not a child of this node.",
                    "NotFoundError"
                );
            }

            this._remove(child);
            return child;
        }

        public static T Clone<T>(T node, Document document = null, bool cloneChildren = false) where T : Node
        {
            if (document == null)
            {
                document = node.OwnerDocument;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Find the inclusive descendant that is last in tree order of the given object.
        /// `O(n)` (worst case) where `n` is the depth of the subtree of `object`
        /// </summary>
        private Node LastInclusiveDescendant()
        {
            Node lastChild;
            var current = this;

            while ((lastChild = current.LastChild) != null)
            {
                current = lastChild;
            }

            return current;
        }

        /// <summary>
        /// Find the preceding object (A) of the given object (B).
        /// An object A is preceding an object B if A and B are in the same tree
        ///     and A comes before B in tree order.
        /// * `O(n)` (worst case)
        ///     * `O(1)` (amortized when walking the entire tree)
        /// </summary>
        /// <param name="root">If set, `root` must be an inclusive ancestor
        ///      of the return value (or else null is returned). This check _assumes_
        ///        that `root` is also an inclusive ancestor of the given `object`</param>
        public Node Preceding(Node root = null)
        {
            if (this == root)
            {
                return null;
            }

            if (PreviousSibling != null)
            {
                return PreviousSibling.LastInclusiveDescendant();
            }

            // if there is no previous sibling return the parent (might be null)
            return Parent;
        }

        /// <summary>
        /// Find the following object (A) of the given object (B).
        /// An object A is following an object B if A and B are in the same tree
        /// and A comes after B in tree order.
        ///
        /// `O(n)` (worst case) where `n` is the amount of objects in the entire tree
        /// `O(1)` (amortized when walking the entire tree)
        /// </summary>
        internal Node Following(Node treeRoot = null, bool skipChildren = false)
        {
            if (!skipChildren && FirstChild != null)
            {
                return FirstChild;
            }

            var current = this;
            do
            {
                if (current == treeRoot)
                {
                    return null;
                }

                var nextSibling = current.NextSibling;
                if (nextSibling != null)
                {
                    return nextSibling;
                }

                current = current.Parent;
            } while (current != null);

            return null;
        }

        /// <summary>
        /// Append all children of the given object to an array.
        /// `O(n)` where `n` is the amount of children of the given `parent`
        /// </summary>
        /// <param name="list">If given, use this as the list to add nodes to.</param>
        /// <param name="filter">If given, only return nodes that this predicate returns true for.</param>
        internal List<Node> ChildrenToArray(List<Node> list = null, Predicate<Node> filter = null)
        {
            list ??= new List<Node>();

            var parentNode = this;
            var node = parentNode.FirstChild;
            var index = 0;

            while (node != null)
            {
                node.SetCachedIndex(parentNode, index);

                if (filter == null || filter(node))
                {
                    list.Add(node);
                }

                node = node.NextSibling;
                ++index;
            }

            return list;
        }

        /// <summary>
        /// Append all inclusive ancestors of the given object to an array.
        /// `O(n)` where `n` is the amount of ancestors of the given `object`
        /// </summary>
        /// <param name="list">If given, use this as the list to add nodes to.</param>
        /// <param name="filter">If given, only return nodes that this predicate returns true for.</param>
        public List<Node> AncestorsToArray(List<Node> list = null, Predicate<Node> filter = null)
        {
            list ??= new List<Node>();

            var ancestor = this;

            while (ancestor != null)
            {
                if (filter == null || filter(ancestor))
                {
                    list.Add(ancestor);
                }

                ancestor = ancestor.Parent;
            }

            return list;
        }

        /// <summary>
        /// Append all descendants of the given object to an array (in tree order).
        /// `O(n)` where `n` is the amount of objects in the sub-tree of the given `object`
        /// </summary>
        /// <param name="list">If given, use this as the list to add nodes to.</param>
        /// <param name="filter">If given, only return nodes that this predicate returns true for.</param>
        public List<Node> TreeToArray(List<Node> list = null,
            Predicate<Node> filter = null)
        {
            list ??= new List<Node>();

            var root = this;
            var obj = this;

            while (obj != null)
            {
                if (filter == null || filter.Invoke(obj))
                {
                    list.Add(obj);
                }

                obj = obj.Following(root);
            }

            return list;
        }

        public int ChildrenCount
        {
            get
            {
                if (LastChild == null)
                {
                    return 0;
                }

                return LastChild.Index + 1;
            }
        }

        [Flags]
        enum TreePosition
        {
            DISCONNECTED = 1,
            PRECEDING = 2,
            FOLLOWING = 4,
            CONTAINS = 8,
            CONTAINED_BY = 16
        }


        /**
         * Compare the position of an object relative to another object. A bit set is returned:
         *
         * <ul>
         *     <li>DISCONNECTED : 1</li>
         *     <li>PRECEDING : 2</li>
         *     <li>FOLLOWING : 4</li>
         *     <li>CONTAINS : 8</li>
         *     <li>CONTAINED_BY : 16</li>
         * </ul>
         *
         * The semantics are the same as compareDocumentPosition in DOM, with the exception that
         * DISCONNECTED never occurs with any other bit.
         *
         * where `n` and `m` are the amount of ancestors of `left` and `right`;
         * where `o` is the amount of children of the lowest common ancestor of `left` and `right`:
         *
         * * `O(n + m + o)` (worst case)
         * * `O(n + m)` (amortized, if the tree is not modified)
         *
         * @method compareTreePosition
         * @memberOf module:symbol-tree#
         * @param {Object} left
         * @param {Object} right
         * @return {Number}
         */
        private TreePosition compareTreePosition(Node left, Node right)
        {
            // In DOM terms:
            // left = reference / context object
            // right = other

            if (left == right)
            {
                return 0;
            }

            var leftAncestors = new List<Node>(); // TODO Pooled
            {
                // inclusive
                var leftAncestor = left;

                while (leftAncestor != null)
                {
                    if (leftAncestor == right)
                    {
                        return TreePosition.CONTAINS | TreePosition.PRECEDING;
                        // other is ancestor of reference
                    }

                    leftAncestors.Add(leftAncestor);
                    leftAncestor = leftAncestor.Parent;
                }
            }


            var rightAncestors = new List<Node>(); // TODO Pooled;
            {
                var rightAncestor = right;

                while (rightAncestor != null)
                {
                    if (rightAncestor == left)
                    {
                        return TreePosition.CONTAINED_BY | TreePosition.FOLLOWING;
                    }

                    rightAncestors.Add(rightAncestor);
                    rightAncestor = rightAncestor.Parent;
                }
            }

            static Node ReverseArrayIndex(List<Node> array, int reverseIndex)
            {
                return array[array.Count - 1 - reverseIndex]; // no need to check `index >= 0`
            }

            var root = ReverseArrayIndex(leftAncestors, 0);

            if (root == null || root != ReverseArrayIndex(rightAncestors, 0))
            {
                // note: unlike DOM, preceding / following is not set here
                return TreePosition.DISCONNECTED;
            }

            // find the lowest common ancestor
            var commonAncestorIndex = 0;
            var ancestorsMinLength = Math.Min(leftAncestors.Count, rightAncestors.Count);

            for (var i = 0; i < ancestorsMinLength; ++i)
            {
                var leftAncestor = ReverseArrayIndex(leftAncestors, i);
                var rightAncestor = ReverseArrayIndex(rightAncestors, i);

                if (leftAncestor != rightAncestor)
                {
                    break;
                }

                commonAncestorIndex = i;
            }

            // indexes within the common ancestor
            var leftIndex = ReverseArrayIndex(leftAncestors, commonAncestorIndex + 1).Index;
            var rightIndex = ReverseArrayIndex(rightAncestors, commonAncestorIndex + 1).Index;

            return rightIndex < leftIndex
                ? TreePosition.PRECEDING
                : TreePosition.FOLLOWING;
        }

        /// <summary>
        /// Remove the object from this tree.
        /// Has no effect if already removed.
        /// </summary>
        private void remove()
        {
            var parentNode = this.Parent;
            var prevNode = this.PreviousSibling;
            var nextNode = this.NextSibling;

            if (parentNode != null)
            {
                if (parentNode.FirstChild == this)
                {
                    parentNode.FirstChild = NextSibling;
                }

                if (parentNode.LastChild == this)
                {
                    parentNode.LastChild = PreviousSibling;
                }
            }

            if (prevNode != null)
            {
                prevNode.NextSibling = this.NextSibling;
            }

            if (nextNode != null)
            {
                nextNode.PreviousSibling = this.PreviousSibling;
            }

            this.Parent = null;
            this.PreviousSibling = null;
            this.NextSibling = null;
            this.cachedIndex = -1;
            this.cachedIndexVersion = -1;

            parentNode?.ChildrenChanged();
        }

        private bool HasChildren => FirstChild != null;

        private bool IsAttached => Parent != null || PreviousSibling != null || NextSibling != null;

        private void ChildrenChanged()
        {
            // integer wrap around
            this.childrenVersion = (this.childrenVersion + 1) & 0x7FFFFFFF;
            this.childIndexCachedUpTo = null;
        }

        /// <summary>
        /// Iterate over all direct children of this node.
        /// </summary>
        /// <param name="reverse">Iterate from last child to first.</param>
        public TreeIterator ChildrenIterator(bool reverse = false)
        {
            return new TreeIterator(
                this,
                reverse ? LastChild : FirstChild,
                reverse ? IterationOrder.PREV : IterationOrder.NEXT
            );
        }

        /// <summary>
        /// Iterate over all the previous siblings of the given object. (in reverse tree order)
        /// </summary>
        public TreeIterator PreviousSiblingsIterator()
        {
            return new TreeIterator(
                this,
                PreviousSibling,
                IterationOrder.PREV
            );
        }

        /// <summary>
        /// Iterate over all the next siblings of the given object. (in tree order)
        /// </summary>
        public TreeIterator NextSiblingsIterator()
        {
            return new TreeIterator(
                this,
                NextSibling,
                IterationOrder.NEXT
            );
        }

        /// <summary>
        /// Iterate over all inclusive ancestors of the given object
        /// </summary>
        public TreeIterator AncestorsIterator()
        {
            return new TreeIterator(
                this,
                this,
                IterationOrder.PARENT
            );
        }

        /// <summary>
        /// Iterate over all inclusive descendants of the given object
        /// </summary>
        public IEnumerable<Node> DescendantsIterator()
        {
            // TODO: OPTIMIZE
            yield return this;

            foreach (var child in ChildrenIterator())
            {
                foreach (var descendant in child.DescendantsIterator())
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// Iterate over all descendants of the given object (in tree order).
        /// </summary>
        public TreeIterator TreeIterator(bool reverse = false)
        {
            return new TreeIterator(
                this,
                reverse ? LastInclusiveDescendant() : this,
                reverse ? IterationOrder.PRECEDING : IterationOrder.FOLLOWING
            );
        }

        /// <summary>
        /// Find the index of the given object (the number of preceding siblings).
        /// </summary>
        public int Index
        {
            get
            {
                var parentNode = Parent;

                if (parentNode == null)
                {
                    // In principal, you could also find out the number of preceding siblings
                    // for objects that do not have a parent. This method limits itself only to
                    // objects that have a parent because that lets us optimize more.
                    return -1;
                }

                var currentIndex = GetCachedIndex(parentNode);
                if (currentIndex >= 0)
                {
                    return currentIndex;
                }

                currentIndex = 0;
                var current = parentNode.FirstChild;

                if (parentNode.childIndexCachedUpTo != null)
                {
                    var cachedUpToNode = parentNode.childIndexCachedUpTo;
                    current = cachedUpToNode.NextSibling;
                    currentIndex = cachedUpToNode.GetCachedIndex(parentNode) + 1;
                }

                for (; current != null; current = current.NextSibling)
                {
                    current.SetCachedIndex(parentNode, currentIndex);

                    if (current == this)
                    {
                        break;
                    }

                    ++currentIndex;
                }

                parentNode.childIndexCachedUpTo = this;
                return currentIndex;
            }
        }

        #region Cached Tree Properties

        private int cachedIndex;
        private int cachedIndexVersion;
        private int childrenVersion;
        private Node childIndexCachedUpTo;

        private int GetCachedIndex(Node parentNode)
        {
            // (assumes parentNode is actually the parent)
            if (this.cachedIndexVersion != parentNode.childrenVersion)
            {
                this.cachedIndexVersion = -1;
                // cachedIndex is no longer valid
                return -1;
            }

            return this.cachedIndex; // -1 if not cached
        }

        private void SetCachedIndex(Node parentNode, int index)
        {
            // (assumes parentNode is actually the parent)
            this.cachedIndexVersion = parentNode.childrenVersion;
            this.cachedIndex = index;
        }

        #endregion

        public virtual void _adoptingSteps(Document oldDocument)
        {
        }

        // https://dom.spec.whatwg.org/#converting-nodes-into-a-node
        // create a fragment (or just return a node for one item)
        internal static Node ConvertNodesIntoNode(Document document, object[] nodes) {
            static Node ConvertToNode(Document document, object obj)
            {
                if (obj is string stringValue)
                {
                    return document.CreateTextNode(stringValue);
                }
                else if (obj is Node node)
                {
                    return node;
                }

                throw new ArgumentException($"Only supports Node or string arguments: {obj}");
            }

            if (nodes.Length == 1)
            {
                return ConvertToNode(document, nodes[0]);
            }

            var fragment = document.CreateDocumentFragment();
            foreach (var obj in nodes)
            {
                fragment.AppendChild(ConvertToNode(document, obj));
            }
            return fragment;
        }

        // Currently everything is positioned
        public bool IsPositioned => true;

        public Node ClosestPositionedAncestor()
        {
            return ParentElement;
        }

    }
}