using System.Diagnostics;

namespace OpenTemple.Core.Ui.DOM
{
    /// <summary>
    /// A high precision timestamp used in the UI.
    /// </summary>
    public readonly struct UiTimestamp
    {
        private readonly long ticks;

        // TODO Fill out

        private UiTimestamp(long ticks)
        {
            this.ticks = ticks;
        }

        public static UiTimestamp Now => new UiTimestamp(Stopwatch.GetTimestamp());
    }
}