using System.Collections.Generic;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Ui.DOM
{
    public class MutationObserverInit
    {
        public bool subtree { get; set; }
        public bool childList { get; set; }
        public bool attributes { get; set; }
        public List<string> attributeFilter { get; set; }
        public bool attributeOldValue { get; set; }
        public bool characterData { get; set; }
        public bool characterDataOldValue { get; set; }
    }

    public interface MutationObserver
    {

    }
}