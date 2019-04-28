using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    /// <summary>
    /// Can be implemented by game systems to receive time updates on each frame.
    /// </summary>
    public interface ITimeAwareSystem
    {
        void AdvanceTime(TimePoint time);
    }
}