using System.Security.Cryptography.X509Certificates;
using System.Text;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // This is used for linking to a miscellaneous bonus list from other entries and such
    public class HistoryMiscBonus : HistoryEntry
    {
        public BonusList BonusList { get; }

        public int MesLineId { get; }

        public int RollResult { get; }

        public HistoryMiscBonus(BonusList bonusList, int mesLineId, int rollResult)
        {
            BonusList = bonusList;
            MesLineId = mesLineId;
            RollResult = rollResult;
        }

        internal override void PrintToConsole(StringBuilder builder)
        {
        }
    }
}