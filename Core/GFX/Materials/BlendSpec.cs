namespace SpicyTemple.Core.GFX.Materials
{
/**
	* Desribes the state of the render target blending stage.
	*/
    public class BlendSpec
    {
        // Enables alpha blending on the output target
        public bool blendEnable = false;

        // Defines how the incoming fragment color and the one in the
        // render target should be blended together
        public BlendOperand srcBlend = BlendOperand.One;
        public BlendOperand destBlend = BlendOperand.Zero;

        // By default use the destination alpha and keep it
        public BlendOperand srcAlphaBlend = BlendOperand.Zero;
        public BlendOperand destAlphaBlend = BlendOperand.One;

        // Write mask for writing to the render target
        public bool writeRed = true;
        public bool writeGreen = true;
        public bool writeBlue = true;
        public bool writeAlpha = true;

        public bool Equals(BlendSpec other)
        {
            return blendEnable == other.blendEnable && srcBlend == other.srcBlend && destBlend == other.destBlend &&
                   srcAlphaBlend == other.srcAlphaBlend && destAlphaBlend == other.destAlphaBlend &&
                   writeRed == other.writeRed && writeGreen == other.writeGreen && writeBlue == other.writeBlue &&
                   writeAlpha == other.writeAlpha;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((BlendSpec) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = blendEnable.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) srcBlend;
                hashCode = (hashCode * 397) ^ (int) destBlend;
                hashCode = (hashCode * 397) ^ (int) srcAlphaBlend;
                hashCode = (hashCode * 397) ^ (int) destAlphaBlend;
                hashCode = (hashCode * 397) ^ writeRed.GetHashCode();
                hashCode = (hashCode * 397) ^ writeGreen.GetHashCode();
                hashCode = (hashCode * 397) ^ writeBlue.GetHashCode();
                hashCode = (hashCode * 397) ^ writeAlpha.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BlendSpec left, BlendSpec right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BlendSpec left, BlendSpec right)
        {
            return !Equals(left, right);
        }
    }
}