namespace OpenTemple.Core.Ui.DOM
{
    public delegate void EventListener(IEvent evt);

    public delegate void EventListener<in T>(T evt) where T : IEvent;
}