namespace Core.WidgetEventGenerator;

public record EventDefinition(
    string Name,
    string EventClass,
    bool Cancelable,
    bool Bubbles,
    bool HasDefaultAction
)
{
    public string EventClassShort => GetShortEventClass(EventClass);

    public string? EventClassNamespace => GetEventClassNamespace(EventClass);

    private string? GetEventClassNamespace(string eventClass)
    {
        var idx = eventClass.LastIndexOf('.');
        return idx == -1 ? null : eventClass[..idx];
    }

    private static string GetShortEventClass(string eventClass)
    {
        var idx = eventClass.LastIndexOf('.');
        return idx != -1 ? eventClass[(idx + 1)..] : eventClass;
    }
}
