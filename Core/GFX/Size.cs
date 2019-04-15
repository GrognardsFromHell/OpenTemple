namespace SpicyTemple.Core.GFX
{
    public struct Size {
        public int width;
        public int height;

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool Equals(Size other)
        {
            return width == other.width && height == other.height;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Size other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (width * 397) ^ height;
            }
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size left, Size right)
        {
            return !left.Equals(right);
        }
    }
}