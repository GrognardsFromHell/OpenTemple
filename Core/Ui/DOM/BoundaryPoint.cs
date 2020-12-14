using System;

namespace OpenTemple.Core.Ui.DOM
{
    public readonly struct BoundaryPoint
    {
        public Node Node { get; }
        public int Offset { get; }

        public BoundaryPoint(Node node, int offset)
        {
            Node = node;
            Offset = offset;
        }

        // Returns 0 if equal, +1 for after and -1 for before
        // https://dom.spec.whatwg.org/#concept-range-bp-after
        public static int ComparePosition(in BoundaryPoint bpA, in BoundaryPoint bpB)
        {
            var nodeA = bpA.Node;
            var offsetA = bpA.Offset;
            var nodeB = bpB.Node;
            var offsetB = bpB.Offset;

            if (nodeA.GetNodeRoot() != nodeB.GetNodeRoot()) {
                throw new Exception("Boundary points should have the same root!");
            }

            if (nodeA == nodeB) {
                if (offsetA == offsetB) {
                    return 0;
                } else if (offsetA < offsetB) {
                    return -1;
                }

                return 1;
            }

            if (nodeA.IsFollowing(nodeB)) {
                return ComparePosition(bpB, bpA) == -1 ? 1 : -1;
            }

            if (nodeA.IsInclusiveAncestor(nodeB)) {
                var child = nodeB;

                while (child.Parent != nodeA) {
                    child = child.Parent;
                }

                if (child.Index < offsetA) {
                    return 1;
                }
            }

            return -1;
        }
    }
}