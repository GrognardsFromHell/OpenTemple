using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Fade;

public class GFadeSystem : IGameSystem, ITimeAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10AA32CC)]
    private FadeArgs _currentFade;

    [TempleDllLocation(0x10AA32E8)]
    private TimePoint _lastFadeTime;

    [TempleDllLocation(0x10AA32C8)]
    private int _timeOverflow;

    [TempleDllLocation(0x10AA32C4)]
    private int _fadeStepsDone;

    // Used to notify code that GFade has completed
    private TaskCompletionSource<bool> _activeTask;

    [TempleDllLocation(0x10D25118)]
    public bool IsOverlayEnabled { get; private set; }

    [TempleDllLocation(0x10D24A28)]
    public LinearColorA OverlayColor { get; private set; }

    public void Dispose()
    {
    }

    [TempleDllLocation(0x10051a10)]
    public void AdvanceTime(TimePoint time)
    {
        if (_fadeStepsDone < _currentFade.fadeSteps)
        {
            if (_currentFade.flags.HasFlag(FadeFlag.FadeIn))
            {
                SetGameOpacity(GetFadeProgress());
            }
            else
            {
                SetGameOpacity(1.0f - GetFadeProgress());
            }

            _lastFadeTime = TimePoint.Now;

            if (_fadeStepsDone >= _currentFade.fadeSteps)
            {
                _activeTask?.TrySetResult(true);
                _activeTask = null;
            }
        }
    }

    [TempleDllLocation(0x10051C00)]
    [TempleDllLocation(0x10051ba0)]
    public Task PerformFade(ref FadeArgs fadeArgs)
    {
        _activeTask?.TrySetCanceled();
        _activeTask = new TaskCompletionSource<bool>();

        _currentFade = fadeArgs;

        _timeOverflow = 0;
        _fadeStepsDone = -1;
        if (!fadeArgs.flags.HasFlag(FadeFlag.FadeIn))
            Logger.Info("gfade fade out");
        else
            Logger.Info("gfade fade in");

        _lastFadeTime = TimePoint.Now;
        AdvanceTime(_lastFadeTime);
        // If transition time is 0, this might actually complete _immediately_!
        if (_activeTask == null)
        {
            return Task.CompletedTask;
        }
        return _activeTask.Task;
    }

    [TempleDllLocation(0x101da370)]
    public void SetGameOpacity(float gameOpacity)
    {
        if ( gameOpacity >= 1.0 )
        {
            IsOverlayEnabled = false;
            OverlayColor = LinearColorA.Transparent;
            return;
        }

        IsOverlayEnabled = true;
        var alpha = Math.Clamp(1.0f - gameOpacity, 0.0f, 1.0f);
        OverlayColor = new LinearColorA(0, 0, 0, alpha);
    }

    private float GetFadeProgress()
    {
        if (_fadeStepsDone < 0)
        {
            _fadeStepsDone = 0;
        }

        if (_currentFade.fadeSteps < 1 || _currentFade.transitionTime < 1.0)
        {
            return 1.0f;
        }
        var timePerStep = (int) (_currentFade.transitionTime * 600.0f / _currentFade.fadeSteps);
        var elapsedMs = (int) (TimePoint.Now - _lastFadeTime).TotalMilliseconds;
        _fadeStepsDone += (elapsedMs + _timeOverflow) / timePerStep;
        _timeOverflow = (elapsedMs + _timeOverflow) % timePerStep;
        if (_fadeStepsDone > _currentFade.fadeSteps)
        {
            _fadeStepsDone = _currentFade.fadeSteps;
        }

        return (float) _fadeStepsDone / _currentFade.fadeSteps;
    }

    public void ScheduleFade(ref FadeArgs fadeArgs, TimeSpan delay)
    {
        var evt = new TimeEvent(TimeEventType.Fade);
        evt.arg1.int32 = (int) fadeArgs.flags;
        evt.arg2.int32 = unchecked((int) fadeArgs.color.Pack());
        evt.arg3.float32 = 2.0f;
        evt.arg4.int32 = 48;
        GameSystems.TimeEvent.Schedule(evt, delay, out _);
    }
}