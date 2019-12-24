namespace OpenTemple.Core.Systems.Anim
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

        public bool Equals(AnimSlotId other)
        {
            return slotIndex == other.slotIndex && uniqueId == other.uniqueId;
        }

        public override bool Equals(object obj)
        {
            return obj is AnimSlotId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (slotIndex * 397) ^ uniqueId;
            }
        }

        public static bool operator ==(AnimSlotId left, AnimSlotId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnimSlotId left, AnimSlotId right)
        {
            return !left.Equals(right);
        }
    }
}