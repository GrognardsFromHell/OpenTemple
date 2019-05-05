namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class ConditionAttachment
    {
        public readonly ConditionSpec condStruct;
        public uint flags; // 1 - expired; 2 - got arg data from info stored in field
        public int[] args;

        public ConditionAttachment(ConditionSpec cond)
        {
            condStruct = cond;
            flags = 0;
            args = null;
        }

        public bool IsExpired => (flags & 1) != 0;
    }
}