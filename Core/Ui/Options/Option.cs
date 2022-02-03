using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Options;

public abstract class Option
{
    public string Label { get; }

    protected Option(string label)
    {
        Label = label;
    }

    public abstract void AddTo(WidgetContainer container);

    public virtual void Reset()
    {
    }

    public virtual void Apply()
    {
    }

    public virtual void Cancel()
    {
    }
}