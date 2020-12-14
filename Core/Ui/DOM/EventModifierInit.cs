namespace OpenTemple.Core.Ui.DOM
{
    // https://w3c.github.io/uievents/#event-modifier-initializers
    public class EventModifierInit : UiEventInit
    {
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
        public bool MetaKey { get; set; }
    }
}