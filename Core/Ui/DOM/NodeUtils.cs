namespace OpenTemple.Core.Ui.DOM
{
    internal static class NodeUtils
    {
        // https://dom.spec.whatwg.org/#concept-node-length
        public static int GetNodeLength(this Node node)
        {
            switch (node.NodeType)
            {
                case NodeType.TEXT_NODE:
                case NodeType.COMMENT_NODE:
                    return ((CharacterData) node).Data.Length;

                default:
                    return node.ChildrenCount;
            }
        }

        // https://dom.spec.whatwg.org/#concept-tree-root
        public static Node GetNodeRoot(this Node node)
        {
            while (node.Parent != null)
            {
                node = node.Parent;
            }

            return node;
        }

        // https://dom.spec.whatwg.org/#concept-tree-inclusive-ancestor
        public static bool IsInclusiveAncestor(this Node ancestorNode, Node node)
        {
            while (node != null)
            {
                if (ancestorNode == node)
                {
                    return true;
                }

                node = node.Parent;
            }

            return false;
        }

        // https://dom.spec.whatwg.org/#concept-tree-host-including-inclusive-ancestor
        public static bool IsHostIncludingInclusiveAncestor(this Node a, Node b)
        {
            if (a.IsInclusiveAncestor(b))
            {
                return true;
            }

            // TODO
            return false;
        }

        // https://dom.spec.whatwg.org/#concept-tree-following
        public static bool IsFollowing(this Node nodeA, Node nodeB)
        {
            if (nodeA == nodeB)
            {
                return false;
            }

            var current = nodeB;
            while (current != null)
            {
                if (current == nodeA)
                {
                    return true;
                }

                current = current.Following();
            }

            return false;
        }
    }
}