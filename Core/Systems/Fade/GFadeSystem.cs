using System;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.Fade
{
    public class GFadeSystem : IGameSystem, ITimeAwareSystem
    {
        public void Dispose()
        {
        }

        public void AdvanceTime(TimePoint time)
        {
            // TODO
        }

        [TempleDllLocation(0x10051C00)]
        public void PerformFade(ref FadeArgs fadeArgs)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10084A50)]
        public void FadeAndTeleport(ref FadeAndTeleportArgs fadeTp)
        {

        }
    }
}