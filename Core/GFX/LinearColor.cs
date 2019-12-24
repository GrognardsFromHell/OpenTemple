using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace OpenTemple.Core.GFX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LinearColor
    {
        public float R;
        public float G;
        public float B;

        public static LinearColor White => new LinearColor(1, 1, 1);

        public LinearColor(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public LinearColor(PackedLinearColorA color) : this(color.R * 255, color.G * 255, color.B * 255)
        {
        }

        public static LinearColor Lerp(LinearColor from, LinearColor to, float factor)
        {
            return new LinearColor(
                from.R + (to.R - from.R) * factor,
                from.G + (to.G - from.G) * factor,
                from.B + (to.B - from.B) * factor
            );
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LinearColorA
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public static LinearColorA White => new LinearColorA(1, 1, 1, 1);

        public static LinearColorA Transparent => new LinearColorA(0, 0, 0, 0);

        public LinearColorA(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static implicit operator RawColor4(LinearColorA color)
        {
            return new RawColor4(color.R, color.G, color.B, color.A);
        }

        public static implicit operator PackedLinearColorA(LinearColorA color)
        {
            return new PackedLinearColorA(color);
        }
    }

    /// <summary>
    /// A LinearColorA packed into a 32-bit BGRA integer.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 4, Pack = 1)]
    public struct PackedLinearColorA
    {

        [FieldOffset(0)]
        public byte B;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(2)]
        public byte R;

        [FieldOffset(3)]
        public byte A;

        public PackedLinearColorA(LinearColorA color) : this((byte) (color.R * 255.0f), (byte) (color.G * 255.0f), (byte) (color.B * 255.0f), (byte) (color.A * 255.0f))
        {
        }

        public PackedLinearColorA(LinearColor color, float alpha = 1.0f) : this((byte) (color.R * 255.0f), (byte) (color.G * 255.0f), (byte) (color.B * 255.0f), (byte) (alpha * 255.0f))
        {
        }

        public PackedLinearColorA(uint packed) : this()
        {
            B = (byte) (packed & 0xFF);
            G = (byte) ((packed >> 8) & 0xFF);
            R = (byte) ((packed >> 16) & 0xFF);
            A = (byte) ((packed >> 24) & 0xFF);
        }

        public uint Pack()
        {
            return (uint) (B | G << 8 | R << 16 | A << 24);
        }

        public PackedLinearColorA(byte r, byte g, byte b, byte a)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        public static PackedLinearColorA OfFloats(float r, float g, float b, float a)
        {
            return new PackedLinearColorA(
                (byte) (r * 255.0f),
                (byte) (g * 255.0f),
                (byte) (b * 255.0f),
                (byte) (a * 255.0f)
            );
        }

        public static PackedLinearColorA White => new PackedLinearColorA(255, 255, 255, 255);

        public static PackedLinearColorA Black => new PackedLinearColorA(0, 0, 0, 255);

        public Vector4 ToRGBA() => new Vector4(
            R / 255.0f,
            G / 255.0f,
            B / 255.0f,
            A / 255.0f
        );

        public bool Equals(PackedLinearColorA other)
        {
            return B == other.B && G == other.G && R == other.R && A == other.A;
        }

        public override bool Equals(object obj)
        {
            return obj is PackedLinearColorA other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = B.GetHashCode();
                hashCode = (hashCode * 397) ^ G.GetHashCode();
                hashCode = (hashCode * 397) ^ R.GetHashCode();
                hashCode = (hashCode * 397) ^ A.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PackedLinearColorA left, PackedLinearColorA right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PackedLinearColorA left, PackedLinearColorA right)
        {
            return !left.Equals(right);
        }
    }
}