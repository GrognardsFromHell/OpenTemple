namespace OpenTemple.Core.Systems.TimeEvents
{
    /// <summary>
    /// There are three game time clocks that advance under different conditions.
    /// </summary>
    public enum GameClockType
    {
        /// <summary>
        /// Always increased every timestep.
        /// </summary>
        RealTime = 0,

        /// <summary>
        /// Only advances while out of combat, [0x10AA83D8] is 0 and the dialog UI is not visible.
        /// I suspect the 0x10AA83D8 is an end of turn flag
        /// </summary>
        GameTime,

        /// <summary>
        /// Advances as long as the dialog UI is not visible, but not while the
        /// game is otherwise paused.
        /// </summary>
        GameTimeAnims,

        ClockTypeCount // number of clock types
    }
}