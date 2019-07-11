using System.Numerics;
using System.Runtime.InteropServices;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Particles.Render
{
    internal struct SpriteVertex {
        public Vector4 pos;
        public PackedLinearColorA diffuse;
        public float u;
        public float v;

        public static readonly int Size = Marshal.SizeOf<SpriteVertex>();
    }
}