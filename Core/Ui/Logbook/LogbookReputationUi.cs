namespace OpenTemple.Core.Ui.Logbook;

public class LogbookReputationUi
{
    [TempleDllLocation(0x10c4bd78)]
    public bool IsVisible { get; set; }

    public void Show()
    {
        Update();
        Stub.TODO();
    }

    [TempleDllLocation(0x10192be0)]
    public void Hide()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x101949f0)]
    public void Update()
    {
        Stub.TODO();
    }

    public void Reset()
    {
        // NOTE: This actually doesn't exist in Vanilla (or rather, is a nullsub)
        Stub.TODO();
    }
}