using System.Text;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public class HistorySavingThrow : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public int dc;
        public SavingThrowType saveType;
        public D20SavingThrowFlag saveFlags;
        public BonusList bonlist;

        private void AppendAttempt(StringBuilder builder)
        {
            var objName = GameSystems.MapObject.GetDisplayNameForParty(obj);
            var translation = GameSystems.RollHistory.GetTranslation(17); // attempts
            var saveTypeText = GameSystems.D20.Combat.GetSaveTypeName(saveType);
            var saveText = GameSystems.RollHistory.GetTranslation(18); // save
            builder.AppendFormat("{0} {1} {2}  {3}", objName, translation, saveTypeText, saveText);
        }

        [TempleDllLocation(0x10048670)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            AppendAttempt(builder);
            var overallBon = bonlist.OverallBonus;
            AppendSuccessOrFailureWithLink(builder, rollResult + overallBon >= dc);
            builder.Append('\n');
        }

    }
}