using System.Security.Cryptography.X509Certificates;
using System.Text;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // This is used for linking to a miscellaneous bonus list from other entries and such, formerly type 7
    public class HistoryBonusDetail : HistoryEntry
    {
        public BonusList BonusList { get; }

        public int MesLineId { get; }

        public int RollResult { get; }

        public override string Title => GameSystems.RollHistory.GetTranslation(64); // Bonus Detail

        public HistoryBonusDetail(BonusList bonusList, int mesLineId, int rollResult)
        {
            BonusList = bonusList;
            MesLineId = mesLineId;
            RollResult = rollResult;
        }


        internal override void Format(StringBuilder builder)
        {
        }
    }
}