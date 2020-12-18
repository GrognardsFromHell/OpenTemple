using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.DOM
{
    // https://drafts.csswg.org/cssom-view/#extensions-to-the-document-interface
    public class Document : ParentNode
    {
        private Node body;
        private List<object> _workingNodeIterators;

        private FocusManager _focusManager;

        public FocusManager FocusManager => _focusManager ??= new FocusManager(this);

        public IDocumentHost Host { get; set; }

        public Document() : base(null)
        {
            OwnerDocument = this;
        }

        public override NodeType NodeType => NodeType.DOCUMENT_NODE;

        public override string NodeName => "#document";

        public Element DocumentElement => FirstElementChild;

        [return: MaybeNull]
        public Element ElementFromPoint(double x, double y)
        {
            if (x < 0 || y < 0)
            {
                return null;
            }

            foreach (var node in DocumentElement.TreeIterator(true))
            {
                if (node is WidgetBase widget && widget.IsEffectivelyVisible())
                {
                    var rect = widget.GetContentArea();
                    if (x >= rect.X
                        && y >= rect.Y
                        && x < rect.X + rect.Width
                        && y < rect.Y + rect.Height)
                    {
                        return widget;
                    }
                }
            }

            return null;
        }

        public IEnumerable<Element> ElementsFromPoint(double x, double y)
        {
            throw new System.NotImplementedException();
        }

        public CaretPosition CaretPositionFromPoint(double x, double y)
        {
            throw new System.NotImplementedException();
        }

        public Element ScrollingElement { get; set; }

        public Text CreateTextNode(string text)
        {
            throw new System.NotImplementedException();
        }

        public DocumentFragment CreateDocumentFragment()
        {
            return new DocumentFragment(this);
        }

        public Element CreateElement(string tagName)
        {
            return new Element(this, tagName);
        }

        internal void _runPreRemovingSteps(Node oldNode)
        {
            FocusManager.NotifyElementRemoval(oldNode);
        }

        // https://dom.spec.whatwg.org/#concept-node-adopt
        internal void _adoptNode(Node node)
        {
            var newDocument = this;
            var oldDocument = node.OwnerDocument;

            var parent = node.Parent;
            parent?._remove(node);

            if (oldDocument != newDocument)
            {
                foreach (var inclusiveDescendant in node.DescendantsIterator())
                {
                    inclusiveDescendant.OwnerDocument = newDocument;
                }

                foreach (var inclusiveDescendant in node.DescendantsIterator())
                {
                    inclusiveDescendant._adoptingSteps(oldDocument);
                }
            }
        }

        internal void ContentStateChanged(Element element, EventState states)
        {
            OnContentStateChanged?.Invoke(element, states);
        }

        internal event Action<Element, EventState> OnContentStateChanged;

        public void ReleaseCapture()
        {
            // only release the capture if the caller can access it. This prevents a
            // page from stopping a scrollbar grab for example.
            var node = Globals.UiManager.GetCapturingContent();
            if (node != null)
            {
                Globals.UiManager.ReleaseCapturingContent();
            }
        }

    }
}