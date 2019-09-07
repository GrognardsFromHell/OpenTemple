using System.Text;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Formerly type 4
    public class HistoryMiscCheck : HistoryEntry
    {
        public Dice dicePacked;
        public int rollResult;
        public int dc;
        public string text;
        public BonusList bonlist;

        public override string Title => GameSystems.RollHistory.GetTranslation(63); // Check

        private void AppendAttempt(StringBuilder builder)
        {
            var objName = GameSystems.MapObject.GetDisplayNameForParty(obj);
            var translation = GameSystems.RollHistory.GetTranslation(17); // attempts
            builder.AppendFormat("{0} {1} {2}", objName, translation, text);
        }

        [TempleDllLocation(0x100487b0)]
        internal override void Format(StringBuilder builder)
        {
            AppendAttempt(builder);
            var success = rollResult + bonlist.OverallBonus >= dc;
            AppendSuccessOrFailureWithLink(builder, success);
            builder.Append('\n');
        }
    }
}