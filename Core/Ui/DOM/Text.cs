using System;

namespace OpenTemple.Core.Ui.DOM
{
    public class Text : Node, CharacterData
    {
        public string Data { get; set; }

        public override NodeType NodeType => NodeType.TEXT_NODE;

        public override string NodeName => "#text";

        public Text(Document ownerDocument) : base(ownerDocument)
        {
        }

        public string SubstringData(int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void AppendData(string data)
        {
            throw new NotImplementedException();
        }

        public void InsertData(int offset, string data)
        {
            throw new NotImplementedException();
        }

        public void DeleteData(int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void ReplaceData(int offset, int count, string data)
        {
            throw new NotImplementedException();
        }

        // https://dom.spec.whatwg.org/#dom-text-splittext
        // https://dom.spec.whatwg.org/#concept-text-split
        public Node SplitText(int offset)
        {
            var length = Data.Length;

            if (offset > length)
            {
                throw new DOMException("The index is not in the allowed range.", "IndexSizeError");
            }

            var count = length - offset;
            var newData = this.SubstringData(offset, count);

            var newNode = this.OwnerDocument.CreateTextNode(newData);

            var parent = Parent;
            if (parent != null)
            {
                parent._insert(newNode, this.NextSibling);

                foreach (var range in this._referencedRanges)
                {
                    var start = range.Start;
                    var end = range.End;

                    if (start.Node == this && start.Offset > offset)
                    {
                        range.SetLiveRangeStart(newNode, start.Offset - offset);
                    }

                    if (end.Node == this && end.Offset > offset)
                    {
                        range.SetLiveRangeEnd(newNode, end.Offset - offset);
                    }
                }

                var nodeIndex = Index;
                foreach (var range in parent._referencedRanges)
                {
                    var start = range.Start;
                    var end = range.End;

                    if (start.Node == parent && start.Offset == nodeIndex + 1)
                    {
                        range.SetLiveRangeStart(parent, start.Offset + 1);
                    }

                    if (end.Node == parent && end.Offset == nodeIndex + 1)
                    {
                        range.SetLiveRangeEnd(parent, end.Offset + 1);
                    }
                }
            }

            this.ReplaceData(offset, count, "");

            return newNode;
        }

        public Element PreviousElementSibling
        {
            get
            {
                var node = PreviousSibling;
                while (node != null && !(node is Element))
                {
                    node = node.PreviousSibling;
                }

                return (Element) node;
            }
        }

        public Element NextElementSibling
        {
            get
            {
                var node = NextSibling;
                while (node != null && !(node is Element))
                {
                    node = node.NextSibling;
                }

                return (Element) node;
            }
        }
    }
}