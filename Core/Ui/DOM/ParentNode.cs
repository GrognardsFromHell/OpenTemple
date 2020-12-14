using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenTemple.Core.Ui.DOM
{
    public abstract class ParentNode : Node
    {

        private HTMLCollection _childrenList;

        public ParentNode(Document ownerDocument) : base(ownerDocument)
        {
        }

        public HTMLCollection Children
        {
            get
            {
                if (this._childrenList == null) {
                    this._childrenList = new HTMLCollection(
                        this,
                        () => ChildrenToArray(
                            filter: node => node.NodeType == NodeType.ELEMENT_NODE
                        )
                    );
                } else {
                    this._childrenList._update();
                }
                return this._childrenList;
            }
        }

        [MaybeNull]
        public Element FirstElementChild {
            get
            {
                var child = FirstChild;
                while (child != null && !(child is Element))
                {
                    child = child.NextSibling;
                }

                return (Element) child;
            }
        }

        [MaybeNull]
        public Element LastElementChild {
            get
            {
                var child = LastChild;
                while (child != null && !(child is Element))
                {
                    child = child.PreviousSibling;
                }

                return (Element) child;
            }
        }

        public int ChildElementCount => this.Children.Length;

        public void Prepend(params object[] nodesOrText)
        {
            this.InsertBefore(ConvertNodesIntoNode(OwnerDocument, nodesOrText), this.FirstChild);
        }

        public void Append(params object[] nodesOrText)
        {
            this.AppendChild(ConvertNodesIntoNode(OwnerDocument, nodesOrText));
        }

        public void ReplaceChildren(params object[] nodesOrText)
        {
            throw new NotImplementedException();
        }

        private static bool ShouldAlwaysSelectNothing(Node node) =>
            node == node.OwnerDocument && node.OwnerDocument.DocumentElement == null;

        [return: MaybeNull]
        public Element QuerySelector(string selectors)
        {
            if (ShouldAlwaysSelectNothing(this)) {
                return null;
            }

            throw new NotImplementedException();
        }

        public NodeList QuerySelectorAll(string selectors)
        {
            if (ShouldAlwaysSelectNothing(this))
            {
                return new NodeList();
            }

            throw new NotImplementedException();
        }
    }
}