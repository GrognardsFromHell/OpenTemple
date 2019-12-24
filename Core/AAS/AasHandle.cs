namespace OpenTemple.Core.AAS
{
    public readonly struct AasHandle
    {
        public readonly uint Handle;

        public static AasHandle Null => new AasHandle(0);

        public AasHandle(uint handle)
        {
            Handle = handle;
        }

        public static implicit operator int(AasHandle handle) => (int) handle.Handle;

        public static implicit operator bool(AasHandle handle) => handle.Handle != 0;

        public bool Equals(AasHandle other)
        {
            return Handle == other.Handle;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is AasHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Handle;
        }

        public static bool operator ==(AasHandle left, AasHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AasHandle left, AasHandle right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"AasHandle({Handle})";
        }
    }
}