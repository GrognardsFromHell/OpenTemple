using System;
using System.Numerics;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.Vfx;

public class LightningBoltEffect
{
    /// <summary>
    /// Time at which the effect started.
    /// </summary>
    [TempleDllLocation(0x10ab7f88)]
    private readonly TimePoint _start;

    /// <summary>
    /// Origin of the lightning bolt.
    /// </summary>
    [TempleDllLocation(0x10ab7f90)]
    private readonly Vector3 _source;

    /// <summary>
    /// Normalized vector pointing into the direction of the lightning bolt.
    /// </summary>
    [TempleDllLocation(0x10ab7f9c)]
    private readonly Vector3 _normal;

    private readonly LightningBoltRenderer _renderer;

    public LightningBoltEffect(LightningBoltRenderer renderer, TimePoint start, Vector3 source, Vector3 target)
    {
        _start = start;
        _source = source;
        _renderer = renderer;

        var direction = target - source;
        var length = direction.Length();
        if (length < 0.01f)
        {
            // Use a random direction if the source and target are nearly equal
            _normal = VectorUtils.Random3DNormal();
        }
        else
        {
            _normal = direction / length;
        }
    }

    public void Render(IGameViewport viewport)
    {
        if (AtEnd)
        {
            return;
        }

        var elapsedMs = (int)(TimePoint.Now - _start).TotalMilliseconds;
        _renderer.Render(viewport.Camera, elapsedMs, _source, _normal);

        if (elapsedMs > 768)
        {
            AtEnd = true;
        }
    }

    public bool AtEnd { get; set; }
}