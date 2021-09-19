namespace OpenTemple.Core.Systems.Script.Hooks
{
    [HookInterface]
    public interface IRandomEncountersHook
    {
        /// <summary>
        /// Calculates whether the party can currently rest or not.
        /// </summary>
        SleepStatus CalculateSleepStatus();
    }
}