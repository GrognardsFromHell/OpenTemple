using System.Numerics;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.Vfx;

public class CallLightningEffect
{
    private readonly CallLightningRenderer _renderer;

    [TempleDllLocation(0x10b397b0)]
    private readonly TimePoint _start;

    [TempleDllLocation(0x10B397B8)]
    private readonly Vector3 _target;

    public bool AtEnd { get; private set; }

    public CallLightningEffect(CallLightningRenderer renderer, TimePoint start, Vector3 target)
    {
        _renderer = renderer;
        _start = start;
        _target = target;
    }

    public void Render(IGameViewport viewport)
    {
        if (AtEnd)
        {
            return;
        }

        var elapsedMs = (int)(TimePoint.Now - _start).TotalMilliseconds;
        _renderer.Render(viewport.Camera, elapsedMs, _target);

        if (elapsedMs > 768)
        {
            AtEnd = true;
        }
    }
}