using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui
{
    public class FocusManager
    {
        private readonly Document _document;

        private Element _pendingFocusChange;

        public Element Focused { get; private set; }

        public FocusManager(Document document)
        {
            _document = document;
            Focused = document.DocumentElement;
        }

        public void NotifyElementRemoval(Node node)
        {
            // https://html.spec.whatwg.org/#focus-fixup-rule
            if (node == Focused)
            {
                _pendingFocusChange = _document.DocumentElement;
            }

            // Something tried to .focus() an element, and then it got removed
            // before that focus took effect
            if (node == _pendingFocusChange)
            {
                _pendingFocusChange = null;
            }
        }

        public void RequestFocus(Element element)
        {
            _pendingFocusChange = element;
        }

        public void PerformPendingFocusChange()
        {
            throw new System.NotImplementedException();
        }
    }
}