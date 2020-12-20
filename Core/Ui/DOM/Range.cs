using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenTemple.Core.Ui.DOM
{
    public class Range : AbstractRange
    {
        public Range(BoundaryPoint start, BoundaryPoint end) : base(start, end)
        {
        }

        // https://dom.spec.whatwg.org/#dom-range-commonancestorcontainer
        public Node CommonAncestorContainer
        {
            get
            {
                foreach (var container in _start.Node.AncestorsIterator())
                {
                    if (container.IsInclusiveAncestor(_end.Node))
                    {
                        return container;
                    }
                }

                return null;
            }
        }

        // https://dom.spec.whatwg.org/#dom-range-setstart
        public void SetStart(Node node, int offset)
        {
            SetBoundaryPointStart(this, node, offset);
        }

        // https://dom.spec.whatwg.org/#dom-range-setend
        public void SetEnd(Node node, int offset)
        {
            SetBoundaryPointEnd(this, node, offset);
        }

        // https://dom.spec.whatwg.org/#dom-range-setstartbefore
        public void SetStartBefore(Node node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                throw new DOMException("The given Node has no parent.", "InvalidNodeTypeError");
            }

            SetBoundaryPointStart(this, parent, node.Index);
        }

        // https://dom.spec.whatwg.org/#dom-range-setstartafter
        public void SetStartAfter(Node node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                throw new DOMException("The given Node has no parent.", "InvalidNodeTypeError");
            }

            SetBoundaryPointStart(this, parent, node.Index + 1);
        }

        // https://dom.spec.whatwg.org/#dom-range-setendbefore
        public void SetEndBefore(Node node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                throw new DOMException("The given Node has no parent.", "InvalidNodeTypeError");
            }

            SetBoundaryPointEnd(this, parent, node.Index);
        }

        // https://dom.spec.whatwg.org/#dom-range-setendafter
        public void SetEndAfter(Node node)
        {
            var parent = node.Parent;

            if (parent == null)
            {
                throw new DOMException("The given Node has no parent.", "InvalidNodeTypeError");
            }

            SetBoundaryPointEnd(this, parent, node.Index + 1);
        }

        // https://dom.spec.whatwg.org/#dom-range-collapse
        public void Collapse(bool toStart)
        {
            if (toStart)
            {
                SetLiveRangeEnd(this._start.Node, this._start.Offset);
            }
            else
            {
                SetLiveRangeStart(this._end.Node, this._end.Offset);
            }
        }

        // https://dom.spec.whatwg.org/#dom-range-selectnode
        public void SelectNode(Node node)
        {
            SelectNodeWithinRange(node, this);
        }

        // https://dom.spec.whatwg.org/#dom-range-selectnodecontents
        public void SelectNodeContents(Node node)
        {
            var length = node.GetNodeLength();

            SetLiveRangeStart(node, 0);
            SetLiveRangeEnd(node, length);
        }

        // https://dom.spec.whatwg.org/#dom-range-compareboundarypoints
        public int CompareBoundaryPoints(RangeComparison how, Range sourceRange)
        {
            if (this._root != sourceRange._root)
            {
                throw new DOMException("The two Ranges are not in the same tree.", "WrongDocumentError");
            }

            BoundaryPoint thisPoint;
            BoundaryPoint otherPoint;
            if (how == RangeComparison.START_TO_START)
            {
                thisPoint = this._start;
                otherPoint = sourceRange._start;
            }
            else if (how == RangeComparison.START_TO_END)
            {
                thisPoint = this._end;
                otherPoint = sourceRange._start;
            }
            else if (how == RangeComparison.END_TO_END)
            {
                thisPoint = this._end;
                otherPoint = sourceRange._end;
            }
            else
            {
                thisPoint = this._start;
                otherPoint = sourceRange._end;
            }

            return BoundaryPoint.ComparePosition(thisPoint, otherPoint);
        }

        // https://dom.spec.whatwg.org/#dom-range-deletecontents
        public void DeleteContents()
        {
            if (this.IsCollapsed)
            {
                return;
            }

            var originalStart = _start;
            var originalEnd = _end;

            if (
                originalStart.Node == originalEnd.Node &&
                originalStart.Node is CharacterData
            )
            {
                ((CharacterData) originalStart.Node).ReplaceData(originalStart.Offset,
                    originalEnd.Offset - originalStart.Offset, "");
                return;
            }

            var nodesToRemove = new List<Node>();
            var currentNode = this._start.Node;
            var endNode = NextNodeDescendant(this._end.Node);
            while (currentNode != null && currentNode != endNode)
            {
                if (
                    IsContained(currentNode, this) &&
                    !IsContained(currentNode.Parent, this)
                )
                {
                    nodesToRemove.Add(currentNode);
                }

                currentNode = currentNode.Following();
            }

            Node newNode;
            int newOffset;
            if (originalStart.Node.IsInclusiveAncestor(originalEnd.Node))
            {
                newNode = originalStart.Node;
                newOffset = originalStart.Offset;
            }
            else
            {
                var referenceNode = originalStart.Node;

                while (
                    referenceNode != null &&
                    !referenceNode.Parent.IsInclusiveAncestor(originalEnd.Node)
                )
                {
                    referenceNode = referenceNode.Parent;
                }

                newNode = referenceNode.Parent;
                newOffset = referenceNode.Index + 1;
            }

            if (
                originalStart.Node is CharacterData
            )
            {
                ((CharacterData) originalStart.Node).ReplaceData(originalStart.Offset,
                    originalStart.Node.GetNodeLength() - originalStart.Offset, "");
            }

            foreach (var node in nodesToRemove)
            {
                node.Parent.RemoveChild(node);
            }

            if (
                originalEnd.Node is CharacterData
            )
            {
                ((CharacterData) originalEnd.Node).ReplaceData(0, originalEnd.Offset, "");
            }

            this.SetLiveRangeStart(newNode, newOffset);
            this.SetLiveRangeEnd(newNode, newOffset);
        }

        // https://dom.spec.whatwg.org/#contained
        private static bool IsContained(Node node, AbstractRange range)
        {
            return BoundaryPoint.ComparePosition(new BoundaryPoint(node, 0), range.Start) == 1 &&
                   BoundaryPoint.ComparePosition(new BoundaryPoint(node, node.GetNodeLength()), range.End) == -1;
        }

        [return: MaybeNull]
        private static Node NextNodeDescendant(Node node)
        {
            while (node != null && node.NextSibling == null)
            {
                node = node.Parent;
            }

            return node?.NextSibling;
        }

        // https://dom.spec.whatwg.org/#dom-range-extractcontents
        public DocumentFragment ExtractContents()
        {
            return ExtractRange(this);
        }

        // https://dom.spec.whatwg.org/#dom-range-clonecontents
        public DocumentFragment cloneContents()
        {
            return CloneRange(this);
        }

        // https://dom.spec.whatwg.org/#dom-range-insertnode
        public void InsertNode(Node node)
        {
            InsertNodeInRange(node, this);
        }

        // https://dom.spec.whatwg.org/#concept-range-insert
        private static void InsertNodeInRange(Node node, Range range) {
            var startNode = range._start.Node;
            var startOffset = range._start.Offset;

            if (

                                       startNode.NodeType == NodeType.COMMENT_NODE ||
                                                              startNode.NodeType == NodeType.TEXT_NODE && startNode.Parent == null ||
                                                              node == startNode
            ) {
                throw new DOMException("Invalid start node.", "HierarchyRequestError");
            }

            var referenceNode = startNode.NodeType == NodeType.TEXT_NODE
                ? startNode
                : startNode.ChildrenToArray()[startOffset];
            var parent = (referenceNode == null) ?
                startNode :
                referenceNode.Parent;

            parent._preInsertValidity(node, referenceNode);

            if (startNode is Text startTextNode) {
                referenceNode = startTextNode.SplitText(startOffset);
            }

            if (node == referenceNode) {
                referenceNode = referenceNode.NextSibling;
            }

            var nodeParent = node.Parent;
            nodeParent?.RemoveChild(node);

            var newOffset = referenceNode?.Index ?? parent.GetNodeLength();
            newOffset += node.NodeType == NodeType.DOCUMENT_FRAGMENT_NODE ? node.GetNodeLength() : 1;

            parent.InsertBefore(node, referenceNode);

            if (range.IsCollapsed) {
                range.SetLiveRangeEnd(parent, newOffset);
            }
        }

        // https://dom.spec.whatwg.org/#dom-range-surroundcontents
        public void SurroundContents(Node newParent)
        {
            var node = this.CommonAncestorContainer;
            var endNode = NextNodeDescendant(node);
            while (node != endNode)
            {
                if (!(node is Text) && IsPartiallyContained(node, this))
                {
                    throw new DOMException(
                        "The Range has partially contains a non-Text node.",
                        "InvalidStateError"
                    );
                }

                node = node.Following();
            }

            if (
                newParent is Document ||
                newParent is DocumentFragment
            )
            {
                throw new DOMException("Invalid element type.", "InvalidNodeTypeError");
            }

            var fragment = ExtractRange(this);

            while (newParent.FirstChild != null)
            {
                newParent.RemoveChild(newParent.FirstChild);
            }

            InsertNodeInRange(newParent, this);

            newParent.AppendChild(fragment);

            SelectNodeWithinRange(newParent, this);
        }

        // https://dom.spec.whatwg.org/#dom-range-clonerange
        public Range CloneRange()
        {
            return new Range(_start, _end);
        }

        // https://dom.spec.whatwg.org/#dom-range-ispointinrange
        private bool isPointInRange(Node node, int offset)
        {
            if (node.GetNodeRoot() != this._root)
            {
                return false;
            }

            ValidateSetBoundaryPoint(node, offset);

            var bp = new BoundaryPoint(node, offset);

            if (
                BoundaryPoint.ComparePosition(bp, this._start) == -1 ||
                BoundaryPoint.ComparePosition(bp, this._end) == 1
            )
            {
                return false;
            }

            return true;
        }

        // https://dom.spec.whatwg.org/#concept-range-select
        private static void SelectNodeWithinRange(Node node, Range range)
        {
            var parent = node.Parent;

            if (parent == null) {
                throw new DOMException("The given Node has no parent.", "InvalidNodeTypeError");
            }

            var index = node.Index;

            range.SetLiveRangeStart(parent, index);
            range.SetLiveRangeEnd(parent, index + 1);
        }

        // https://dom.spec.whatwg.org/#dom-range-comparepoint
        public int comparePoint(Node node, int offset)
        {
            if (node.GetNodeRoot() != this._root)
            {
                throw new DOMException(
                    "The given Node and the Range are not in the same tree.",
                    "WrongDocumentError"
                );
            }

            ValidateSetBoundaryPoint(node, offset);

            var bp = new BoundaryPoint(node, offset);
            if (BoundaryPoint.ComparePosition(bp, this._start) == -1)
            {
                return -1;
            }
            else if (BoundaryPoint.ComparePosition(bp, this._end) == 1)
            {
                return 1;
            }

            return 0;
        }

        // https://dom.spec.whatwg.org/#dom-range-intersectsnode
        public bool intersectsNode(Node node)
        {
            if (node.GetRoot() != this._root)
            {
                return false;
            }

            var parent = node.Parent;
            if (parent == null)
            {
                return true;
            }

            var offset = node.Index;

            return (
                BoundaryPoint.ComparePosition(new BoundaryPoint(parent, offset), this._end) == -1 &&
                BoundaryPoint.ComparePosition(new BoundaryPoint(parent, offset + 1), this._start) == 1
            );
        }

        // https://dom.spec.whatwg.org/#dom-range-stringifier
        public string toString()
        {
            var s = "";

            if (_start.Node is Text startText)
            {
                if (_start.Node == _end.Node)
                {
                    return startText.Data.Substring(_start.Offset, _end.Offset);
                }

                s += startText.Data.Substring(_start.Offset);
            }

            var currentNode = _start.Node;
            var endNode = NextNodeDescendant(_end.Node);
            while (currentNode != null && currentNode != endNode)
            {
                if (currentNode is Text currentText && IsContained(currentNode, this))
                {
                    s += currentText.Data;
                }

                currentNode = currentNode.Following();
            }

            if (_end.Node is Text endText)
            {
                s += endText.Data.Substring(0, _end.Offset);
            }

            return s;
        }

        // https://dom.spec.whatwg.org/#concept-range-root
        public Node _root => _start.Node.GetNodeRoot();

        internal void SetLiveRangeStart(Node node, int offset)
        {
            if (this._start.Node != node)
            {
                this._start.Node._referencedRanges.Remove(this);
            }

            node._referencedRanges.Add(this);

            this._start = new BoundaryPoint(
                node,
                offset
            );
        }

        internal void SetLiveRangeEnd(Node node, int offset)
        {
            if (this._end.Node != node)
            {
                this._end.Node._referencedRanges.Remove(this);
            }

            node._referencedRanges.Add(this);

            this._end = new BoundaryPoint(
                node,
                offset
            );
        }


// https://dom.spec.whatwg.org/#concept-range-clone
private static DocumentFragment CloneRange(AbstractRange range) {
  var originalStart = range.Start;
  var originalEnd = range.End;

  var fragment = new DocumentFragment(originalStart.Node.OwnerDocument);

  if (range.IsCollapsed) {
    return fragment;
  }

  if (
    originalStart.Node == originalEnd.Node &&
    (
      originalStart.Node.NodeType == NodeType.TEXT_NODE ||
      originalStart.Node.NodeType == NodeType.COMMENT_NODE
    )
  ) {
    var cloned = Node.Clone(originalStart.Node);
    var characterData = (CharacterData) cloned;
    characterData.Data = characterData.SubstringData(originalStart.Offset, originalEnd.Offset - originalStart.Offset);

    fragment.AppendChild(cloned);

    return fragment;
  }

  var commonAncestor = originalStart.Node;
  while (!commonAncestor.IsInclusiveAncestor(originalEnd.Node)) {
    commonAncestor = commonAncestor.Parent;
  }

  Node firstPartialContainedChild = null;
  if (!originalStart.Node.IsInclusiveAncestor(originalEnd.Node)) {
    var candidate = commonAncestor.FirstChild;
    while (firstPartialContainedChild == null) {
      if (IsPartiallyContained(candidate, range)) {
        firstPartialContainedChild = candidate;
      }

      candidate = candidate.NextSibling;
    }
  }

  Node lastPartiallyContainedChild = null;
  if (!originalEnd.Node.IsInclusiveAncestor(originalStart.Node)) {
    var candidate = commonAncestor.LastChild;
    while (lastPartiallyContainedChild == null) {
      if (IsPartiallyContained(candidate, range)) {
        lastPartiallyContainedChild = candidate;
      }

      candidate = candidate.PreviousSibling;
    }
  }

  var containedChildren = commonAncestor.ChildrenToArray()
    .Where(node => IsContained(node, range));

  if (
    firstPartialContainedChild != null &&
    (
      firstPartialContainedChild.NodeType == NodeType.TEXT_NODE ||
      firstPartialContainedChild.NodeType == NodeType.COMMENT_NODE
    )
  ) {
    var cloned = Node.Clone(originalStart.Node);
    var characterData = (CharacterData) cloned;
    characterData.Data = characterData.SubstringData(originalStart.Offset, originalStart.Node.GetNodeLength() - originalStart.Offset);

    fragment.AppendChild(cloned);
  } else if (firstPartialContainedChild != null) {
    var cloned = Node.Clone(firstPartialContainedChild);
    fragment.AppendChild(cloned);

    var subrange = new Range(
        originalStart,
        new BoundaryPoint(firstPartialContainedChild, firstPartialContainedChild.GetNodeLength())
    );

    var subfragment = CloneRange(subrange);
    cloned.AppendChild(subfragment);
  }

  foreach (var containedChild in containedChildren) {
    var cloned = Node.Clone(containedChild, null, true);
    fragment.AppendChild(cloned);
  }

  if (
    lastPartiallyContainedChild != null &&
    (
      lastPartiallyContainedChild.NodeType == NodeType.TEXT_NODE ||
      lastPartiallyContainedChild.NodeType == NodeType.COMMENT_NODE
    )
  ) {
    var cloned = Node.Clone(originalEnd.Node);
    var characterData = (CharacterData) cloned;
    characterData.Data = characterData.SubstringData(0, originalEnd.Offset);

    fragment.AppendChild(cloned);
  } else if (lastPartiallyContainedChild != null) {
    var cloned = Node.Clone(lastPartiallyContainedChild);
    fragment.AppendChild(cloned);

    var subrange = new Range(
    new BoundaryPoint(lastPartiallyContainedChild, 0),
      originalEnd
    );

    var subfragment = CloneRange(subrange);
    cloned.AppendChild(subfragment);
  }

  return fragment;
}

        // https://dom.spec.whatwg.org/#concept-range-extract
        private DocumentFragment ExtractRange(Range range)
        {
            var originalStart = range.Start;
            var originalEnd = range.End;

            var fragment = new DocumentFragment(originalStart.Node.OwnerDocument);

            if (range.IsCollapsed)
            {
                return fragment;
            }

            if (
                originalStart.Node == originalEnd.Node &&
                (
                    originalStart.Node is CharacterData
                )
            )
            {
                var cloned = Node.Clone(originalStart.Node);
                var clonedCharacterData = (CharacterData) cloned;
                clonedCharacterData.Data =
                    clonedCharacterData.SubstringData(originalStart.Offset, originalEnd.Offset - originalStart.Offset);

                fragment.AppendChild(cloned);
                ((CharacterData) originalStart.Node).ReplaceData(originalStart.Offset,
                    originalEnd.Offset - originalStart.Offset, "");

                return fragment;
            }

            var commonAncestor = originalStart.Node;
            while (!commonAncestor.IsInclusiveAncestor(originalEnd.Node))
            {
                commonAncestor = commonAncestor.Parent;
            }

            Node firstPartialContainedChild = null;
            if (!originalStart.Node.IsInclusiveAncestor(originalEnd.Node))
            {
                var candidate = commonAncestor.FirstChild;
                while (firstPartialContainedChild == null)
                {
                    if (IsPartiallyContained(candidate, range))
                    {
                        firstPartialContainedChild = candidate;
                    }

                    candidate = candidate.NextSibling;
                }
            }

            Node lastPartiallyContainedChild = null;
            if (!originalEnd.Node.IsInclusiveAncestor(originalStart.Node))
            {
                var candidate = commonAncestor.LastChild;
                while (lastPartiallyContainedChild == null)
                {
                    if (IsPartiallyContained(candidate, range))
                    {
                        lastPartiallyContainedChild = candidate;
                    }

                    candidate = candidate.PreviousSibling;
                }
            }

            var containedChildren = commonAncestor
                .ChildrenToArray(filter: node => IsContained(node, range));

            Node newNode;
            int newOffset;
            if (originalStart.Node.IsInclusiveAncestor(originalEnd.Node))
            {
                newNode = originalStart.Node;
                newOffset = originalStart.Offset;
            }
            else
            {
                var referenceNode = originalStart.Node;

                while (
                    referenceNode != null &&
                    !referenceNode.Parent.IsInclusiveAncestor(originalEnd.Node)
                )
                {
                    referenceNode = referenceNode.Parent;
                }

                newNode = referenceNode.Parent;
                newOffset = referenceNode.Index + 1;
            }

            if (
                firstPartialContainedChild is CharacterData
            )
            {
                var cloned = Node.Clone(originalStart.Node);
                var clonedCharacterData = (CharacterData) cloned;
                clonedCharacterData.Data = clonedCharacterData.SubstringData(originalStart.Offset,
                    originalStart.Node.GetNodeLength() - originalStart.Offset);

                fragment.AppendChild(cloned);

                ((CharacterData) originalStart.Node).ReplaceData(originalStart.Offset,
                    originalStart.Node.GetNodeLength() - originalStart.Offset, "");
            }
            else if (firstPartialContainedChild != null)
            {
                var cloned = Node.Clone(firstPartialContainedChild);
                fragment.AppendChild(cloned);

                var subrange = new Range(
                    originalStart,
                    new BoundaryPoint(firstPartialContainedChild, firstPartialContainedChild.GetNodeLength())
                );

                var subfragment = ExtractRange(subrange);
                cloned.AppendChild(subfragment);
            }

            foreach (var containedChild in containedChildren)
            {
                fragment.AppendChild(containedChild);
            }

            if (
                lastPartiallyContainedChild != null &&
                (
                    lastPartiallyContainedChild is CharacterData
                )
            )
            {
                var cloned = Node.Clone(originalEnd.Node);
                var clonedCharacterData = (CharacterData) cloned;
                clonedCharacterData.Data = clonedCharacterData.SubstringData(0, originalEnd.Offset);

                fragment.AppendChild(cloned);

                ((CharacterData) originalEnd.Node).ReplaceData(0, originalEnd.Offset, "");
            }
            else if (lastPartiallyContainedChild != null)
            {
                var cloned = Node.Clone(lastPartiallyContainedChild);
                fragment.AppendChild(cloned);

                var subrange = new Range(
                    new BoundaryPoint(lastPartiallyContainedChild, 0),
                    originalEnd
                );

                var subfragment = ExtractRange(subrange);
                cloned.AppendChild(subfragment);
            }

            range.SetLiveRangeStart(newNode, newOffset);
            range.SetLiveRangeEnd(newNode, newOffset);

            return fragment;
        }

        // https://dom.spec.whatwg.org/#partially-contained
        private static bool IsPartiallyContained(Node node, AbstractRange range)
        {
            return (
                (node.IsInclusiveAncestor(range.StartContainer) && !node.IsInclusiveAncestor(range.EndContainer)) ||
                (!node.IsInclusiveAncestor(range.StartContainer) && node.IsInclusiveAncestor(range.EndContainer))
            );
        }

        // https://dom.spec.whatwg.org/#concept-range-bp-set
        private static void ValidateSetBoundaryPoint(Node node, int offset)
        {
            if (offset > node.GetNodeLength())
            {
                throw new DOMException("Offset out of bound.", "IndexSizeError");
            }
        }

        private void SetBoundaryPointStart(Range range, Node node, int offset)
        {
            ValidateSetBoundaryPoint(node, offset);

            var bp = new BoundaryPoint(node, offset);
            if (
                node.GetNodeRoot() != range._root ||
                BoundaryPoint.ComparePosition(bp, range._end) == 1
            )
            {
                range.SetLiveRangeEnd(node, offset);
            }

            range.SetLiveRangeStart(node, offset);
        }

        private void SetBoundaryPointEnd(Range range, Node node, int offset)
        {
            ValidateSetBoundaryPoint(node, offset);

            var bp = new BoundaryPoint(node, offset);
            if (
                node.GetNodeRoot() != range._root ||
                BoundaryPoint.ComparePosition(bp, range._start) == -1
            )
            {
                range.SetLiveRangeStart(node, offset);
            }

            range.SetLiveRangeEnd(node, offset);
        }
    };

    public enum RangeComparison : ushort
    {
        START_TO_START = 0,
        START_TO_END = 1,
        END_TO_END = 2,
        END_TO_START = 3
    }
}