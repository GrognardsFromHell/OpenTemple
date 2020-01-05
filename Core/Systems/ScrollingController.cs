using System.Numerics;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems
{
    /// <summary>
    /// Controls scrolling the game view.
    /// </summary>
    public class ScrollingController
    {
        private const float MaxClockJump = 0.1f;

        private Vector2 _currentAcceleration;

        private Vector2 _currentVelocity;

        private TimePoint _lastUpdate;

        public void Update()
        {
            var elapsedSeconds = CalcTimeSinceLastUpdate();



        }

        private float CalcTimeSinceLastUpdate()
        {
            var now = TimePoint.Now;
            var elapsedSeconds = (float) (now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;
            if (elapsedSeconds > MaxClockJump)
            {
                elapsedSeconds = MaxClockJump;
            }

            return elapsedSeconds;
        }
    }
}