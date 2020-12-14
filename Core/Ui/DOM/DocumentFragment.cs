namespace OpenTemple.Core.Ui.DOM
{
    public class DocumentFragment : ParentNode
    {
        public DocumentFragment(Document ownerDocument) :  base(ownerDocument)
        {
        }

        public override NodeType NodeType => NodeType.DOCUMENT_FRAGMENT_NODE;
        public override string NodeName => "#document-fragment";
    }
}