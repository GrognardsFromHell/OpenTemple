namespace OpenTemple.Core.Ui.DOM
{
    internal static class EventTargetUtils
    {
        public static bool IsNode(this EventTarget tgt) => tgt is Node;

        public static EventTarget GetRoot(this EventTarget tgt)
        {
            return tgt is Node node ? node.GetNodeRoot() : tgt;
        }

        // https://dom.spec.whatwg.org/#concept-shadow-including-inclusive-ancestor
        public static bool IsShadowInclusiveAncestor(this EventTarget ancestor, Node node)
        {
            while (node != null)
            {
                if (node == ancestor)
                {
                    return true;
                }

                node = node.Parent;
            }

            return false;
        }
    }
}