using System;
using System.Text;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public class HistoryEntryType8 : HistoryEntry
    {
        [TempleDllLocation(0x10048320)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}