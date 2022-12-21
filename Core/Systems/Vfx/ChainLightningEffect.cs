using System.Collections.Generic;
using System.Numerics;
using OpenTemple.Core.Time;
using OpenTemple.Core.Ui;



namespace OpenTemple.Core.Systems.Vfx;

/// <summary>
/// Models an ongoing chain lightning visual effect.
/// </summary>
public class ChainLightningEffect
{
    private readonly ChainLightningRenderer _renderer;

    [TempleDllLocation(0x10ab7e50)]
    private readonly TimePoint _start;

    [TempleDllLocation(0x10ab7e58)]
    private readonly Vector3 _source;

    [TempleDllLocation(0x10ab7e68)] [TempleDllLocation(0x10ab7e64)]
    private readonly List<ChainLightningTarget> _targets;

    public ChainLightningEffect(ChainLightningRenderer renderer,
        TimePoint start,
        Vector3 source,
        List<ChainLightningTarget> targets)
    {
        _renderer = renderer;
        _start = start;
        _source = source;
        _targets = targets;
    }

    public bool AtEnd { get; private set; }

    [TempleDllLocation(0x10087e60)]
    public void Render(IGameViewport viewport)
    {
        if (AtEnd)
        {
            return;
        }

        var elapsed = (int)(TimePoint.Now - _start).TotalMilliseconds;

        for (var i = 0; i < _targets.Count; i++)
        {
            // The elapsed time relative to the time-window where the arc for this target should be shown
            // Target arcs are staggered 256ms from the effect start
            var targetElapsed = elapsed - i * ChainLightningRenderer.ChainDelay;
            if (targetElapsed is >= 0 and < ChainLightningRenderer.Duration)
            {
                var target = _targets[i];
                // Trigger the effect as soon as the arc to the target becomes visible
                target.TriggerEffect();

                var from = i > 0 ? _targets[i - 1].Location : _source;
                _renderer.Render(viewport.Camera, i, targetElapsed, from, target.Location);
            }
        }

        // The effect ends after the delay for every target past the first and then the arc duration has elapsed.
        var totalDuration = (_targets.Count - 1) * ChainLightningRenderer.ChainDelay
                            + ChainLightningRenderer.Duration;
        if (elapsed > totalDuration)
        {
            AtEnd = true;
        }
    }
}