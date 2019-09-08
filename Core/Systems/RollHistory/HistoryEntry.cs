using System;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.RollHistory
{
    public abstract class HistoryEntry
    {
        public TimePoint recorded;
        public int histId;
        public int histType;
        public GameObjectBody obj;
        public ObjectId objId;
        public string objDescr;
        public GameObjectBody obj2;
        public ObjectId obj2Id;
        public string obj2Descr;
        public int prevId;
        public int nextId;

        [TempleDllLocation(0x1019b670)]
        public abstract string Title { get; }

        public abstract void FormatShort(StringBuilder builder);

        [TempleDllLocation(0x1019ce60)]
        public abstract void FormatLong(StringBuilder builder);

        protected void AppendSuccessOrFailureWithLink(StringBuilder builder, bool success)
        {
            string linkText;
            if (!success)
            {
                linkText = GameSystems.RollHistory.GetTranslation(21); // Failure
            }
            else
            {
                linkText = GameSystems.RollHistory.GetTranslation(20); // Success
            }

            builder.AppendFormat(" - ~{0}~[ROLL_{1}]", linkText, histId);
        }



    }
}