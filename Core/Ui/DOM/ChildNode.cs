using System;

namespace OpenTemple.Core.Ui.DOM
{
    public interface IChildNode
    {
    }

    public static class ChildNodeExtensions
    {
        /// <summary>
        /// Inserts a set of nodes or strings right before this node.
        /// </summary>
        public static void Before<T>(this T node, params object[] nodes) where T : Node, IChildNode
        {
            var parent = node.Parent;
            if (parent != null)
            {
                var viablePreviousSibling = node.PreviousSibling;
                var idx = viablePreviousSibling != null ? Array.IndexOf(nodes, viablePreviousSibling) : -1;

                while (idx != -1) {
                    viablePreviousSibling = viablePreviousSibling.PreviousSibling;
                    if (viablePreviousSibling == null) {
                        break;
                    }
                    idx = Array.IndexOf(nodes, viablePreviousSibling);
                }

                parent.InsertBefore(
                    Node.ConvertNodesIntoNode(node.OwnerDocument, nodes),
                    viablePreviousSibling?.NextSibling ?? parent.FirstChild
                );
            }
        }

        /// <summary>
        /// Inserts a set of nodes or strings right after this node.
        /// </summary>
        public static void After<T>(this T node, params object[] nodes) where T : Node, IChildNode
        {
            var parent = node.Parent;
            if (parent != null) {
                var viableNextSibling = node.NextSibling;
                var idx = viableNextSibling != null ? Array.IndexOf(nodes, viableNextSibling) : -1;

                while (idx != -1) {
                    viableNextSibling = viableNextSibling.NextSibling;
                    if (viableNextSibling == null) {
                        break;
                    }
                    idx = Array.IndexOf(nodes, viableNextSibling);
                }

                parent.InsertBefore(Node.ConvertNodesIntoNode(node.OwnerDocument, nodes), viableNextSibling);
            }
        }

        /// <summary>
        /// Replaces this node with a set of nodes or strings.
        /// </summary>
        public static void ReplaceWith<T>(this T node, params object[] nodes) where T : Node, IChildNode
        {
            var parent = node.Parent;
            if (parent != null) {
                var viableNextSibling = node.NextSibling;
                var idx = viableNextSibling != null ? Array.IndexOf(nodes, viableNextSibling) : -1;

                while (idx != -1) {
                    viableNextSibling = viableNextSibling.NextSibling;
                    if (viableNextSibling == null) {
                        break;
                    }
                    idx = Array.IndexOf(nodes, viableNextSibling);
                }

                var nodeToInsert = Node.ConvertNodesIntoNode(node.OwnerDocument, nodes);

                if (node.Parent == parent) {
                    parent.ReplaceChild(nodeToInsert, node);
                } else {
                    parent.InsertBefore(nodeToInsert, viableNextSibling);
                }
            }
        }

        public static void Remove<T>(this T node) where T : Node, IChildNode
        {
            node.Parent?._remove(node);
        }

    }
}