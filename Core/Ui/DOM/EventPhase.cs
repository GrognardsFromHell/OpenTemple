namespace OpenTemple.Core.Ui.DOM
{
    public enum EventPhase : ushort
    {
        NONE = 0,
        CAPTURING_PHASE = 1,
        AT_TARGET = 2,
        BUBBLING_PHASE = 3
    }
}