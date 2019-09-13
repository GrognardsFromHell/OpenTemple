namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class ConditionAttachment
    {
        public readonly ConditionSpec condStruct;
        public int flags; // 1 - expired; 2 - got arg data from info stored in field
        public object[] args;

        public ConditionAttachment(ConditionSpec cond)
        {
            condStruct = cond;
            flags = 0;
            if (cond.numArgs > 0)
            {
                args = new object[cond.numArgs];
            }
            else
            {
                args = null;
            }
        }

        public bool IsExpired
        {
            get => (flags & 1) != 0;
            set
            {
                if (value)
                {
                    flags |= 1;
                }
                else
                {
                    flags &= ~1;
                }
            }
        }

        /// <summary>
        /// This flag is only used during D20 status init to make it possible to set the arguments for multiple
        /// instances of the same condition separately. Because the first instance will get this flag set to true,
        /// it'll be ignored for the second copy when its args are set.
        /// </summary>
        public bool ArgsFromField
        {
            get => (flags & 2) != 0;
            set
            {
                if (value)
                {
                    flags |= 2;
                }
                else
                {
                    flags &= ~2;
                }
            }
        }
    }
}