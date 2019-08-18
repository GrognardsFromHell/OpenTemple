using System.Text;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public class HistoryPercentageCheck : HistoryEntry
    {
        public int failureChance;
        public int rollResult;
        public int combatMesFailureReason; // description for the cause of the roll (e.g.
        public int combatMesResult; // descrition for the result
        public int combatMesTitle;

        [TempleDllLocation(0x10047780)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}