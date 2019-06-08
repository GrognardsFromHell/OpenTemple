namespace SpicyTemple.Core.Systems.Anim
{
    public struct AnimSlotId
    {
        public int slotIndex;
        public int uniqueId;
        public int field_8;

        public AnimSlotId(int slotIndex, int uniqueId, int field8)
        {
            this.slotIndex = slotIndex;
            this.uniqueId = uniqueId;
            field_8 = field8;
        }

        public override string ToString()
        {
            return $"[{slotIndex}:{uniqueId}r{field_8}]";
        }

        public void Clear()
        {
            slotIndex = -1;
            uniqueId = -1;
            field_8 = 0;
        }

        public bool IsNull => slotIndex == -1;

        public static AnimSlotId Null => new AnimSlotId
        {
            slotIndex = -1,
            uniqueId = -1,
            field_8 = 0
        };

    }
}