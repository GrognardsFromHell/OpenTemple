using System.Numerics;

namespace SpicyTemple.Core.Utils
{
    public static class Vector3Extensions
    {
        public static Vector4 ToVector4(this Vector3 vector, float w)
        {
            return new Vector4(vector, w);
        }
    }
    public static class Vector4Extensions
    {
        public static Vector3 ToVector3(this Vector4 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}