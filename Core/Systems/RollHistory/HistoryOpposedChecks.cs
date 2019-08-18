using System.Text;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public class HistoryOpposedChecks : HistoryEntry
    {

        [TempleDllLocation(0x100488b0)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}