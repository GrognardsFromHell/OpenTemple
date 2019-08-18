using System.Text;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public class HistoryMiscCheck : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public int dc;
        public SavingThrowType saveType;
        public string text;
        public BonusList bonlist;

        [TempleDllLocation(0x100487b0)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}