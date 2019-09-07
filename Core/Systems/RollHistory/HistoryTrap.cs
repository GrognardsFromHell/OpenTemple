using System;
using System.Text;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Traps, type 8
    public class HistoryTrap : HistoryEntry
    {
        public override string Title => GameSystems.RollHistory.GetTranslation(62); // Trap

        [TempleDllLocation(0x10048320)]
        internal override void Format(StringBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}