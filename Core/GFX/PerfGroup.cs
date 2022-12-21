namespace OpenTemple.Core.GFX;

public ref struct PerfGroup
{
    private RenderingDevice? _device;

    public PerfGroup(RenderingDevice device)
    {
        _device = device;
    }

    public void Dispose()
    {
        _device?.EndPerfGroup();
        _device = null;
    }
}