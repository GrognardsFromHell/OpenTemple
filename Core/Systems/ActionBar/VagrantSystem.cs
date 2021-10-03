using System;
using System.Collections.Generic;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.ActionBar
{
    public class VagrantSystem : IGameSystem, ITimeAwareSystem
    {
        [TempleDllLocation(0x10AB7588)]
        private readonly List<ActionBar> _activeBars = new();

        [TempleDllLocation(0x10ab7580)]
        private TimePoint _timeRef;

        [TempleDllLocation(0x10086ae0)]
        public VagrantSystem()
        {
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10086cb0)]
        public void AdvanceTime(TimePoint newsystime)
        {
            if (_timeRef == default)
            {
                _timeRef = GameSystems.TimeEvent.AnimTime;
                return;
            }

            var now = GameSystems.TimeEvent.AnimTime;
            var timeElapsedSec = (float) (now - _timeRef).TotalSeconds;
            _timeRef = now;

            if (timeElapsedSec <= 0)
            {
                return;
            }

            for (var i = _activeBars.Count - 1; i >= 0; i--)
            {
                var activeBar = _activeBars[i];
                switch (activeBar.Activity)
                {
                    case ActionBarActivity.Ramp:
                        AdvanceActionBarRamp(activeBar, timeElapsedSec);
                        break;
                    case ActionBarActivity.Pulse:
                        AdvanceActionBarPulse(activeBar, timeElapsedSec);
                        break;
                }
            }
        }

        [TempleDllLocation(0x10086dd0)]
        private void AdvanceActionBarRamp(ActionBar actionBar, float timeElapsedSec)
        {
            actionBar.Value += timeElapsedSec * actionBar.Speed;

            if (actionBar.Speed <= 0.0f)
            {
                if (actionBar.Value >= actionBar.ToValue)
                {
                    return;
                }

                actionBar.Value = actionBar.ToValue;
            }
            else
            {
                if (actionBar.Value <= actionBar.ToValue)
                {
                    return;
                }

                actionBar.Value = actionBar.ToValue;
            }

            actionBar.Activity = ActionBarActivity.None;
            actionBar.OnEndRampCallback?.Invoke();
        }

        [TempleDllLocation(0x10086c60)]
        private void AdvanceActionBarPulse(ActionBar actionBar, float timeElapsedSec)
        {
            var newRot = timeElapsedSec / actionBar.pulseTime + actionBar.pulsePhaseRadians;
            actionBar.pulsePhaseRadians = Angles.NormalizeRadians(newRot);
            actionBar.Value = MathF.Cos(actionBar.pulsePhaseRadians) * actionBar.pulseAmplitude + actionBar.pulseMean;
        }

        [TempleDllLocation(0x10086d10)]
        public ActionBar AllocateActionBar()
        {
            var result = new ActionBar();
            _activeBars.Add(result);
            return result;
        }

        [TempleDllLocation(0x10086c50)]
        public void ActionBarStopActivity(ActionBar actionBar)
        {
            actionBar.Activity = ActionBarActivity.None;
        }

        [TempleDllLocation(0x10086c40)]
        public bool ActionBarIsActive(ActionBar actionBar)
        {
            return actionBar.Activity != ActionBarActivity.None;
        }

        [TempleDllLocation(0x10086c30)]
        public float ActionBarGetValue(ActionBar actionBar)
        {
            return actionBar.Value;
        }

        [TempleDllLocation(0x10086b70)]
        public void ActionBarStartRamp(ActionBar bar, float fromValue, float toValue, float speed)
        {
            bar.Value = fromValue;
            bar.Activity = ActionBarActivity.Ramp;
            bar.FromValue = fromValue;
            bar.ToValue = toValue;
            if (fromValue <= toValue)
            {
                bar.Speed = speed;
            }
            else
            {
                bar.Speed = -speed;
            }
        }

        [TempleDllLocation(0x10086bc0)]
        public void ActionBarSetPulseValues(ActionBar pkt, float pulseMinVal, float pulseMaxVal, float pulseTime)
        {
            pkt.Value = pulseMinVal;
            pkt.Activity = ActionBarActivity.Pulse;
            pkt.pulseTime = pulseTime;
            pkt.pulseAmplitude = (pulseMaxVal - pulseMinVal) * 0.5f;
            pkt.pulseMean = pulseMinVal + pkt.pulseAmplitude;
        }
    }
}