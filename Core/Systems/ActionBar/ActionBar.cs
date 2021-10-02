using System;

namespace OpenTemple.Core.Systems.ActionBar
{
    public class ActionBar
    {
        public ActionBarActivity Activity = ActionBarActivity.None;
        public float Value;
        public Action OnEndRampCallback;
        public float FromValue;
        public float ToValue = 1.0f;
        /// <summary>
        /// Change per second.
        /// </summary>
        public float Speed = 1.0f;
        public float pulsePhaseRadians;
        public float pulseMean;
        public float pulseAmplitude;
        public float pulseTime;
    }

    public enum ActionBarActivity
    {
        None,
        Ramp,
        Pulse
    }
}