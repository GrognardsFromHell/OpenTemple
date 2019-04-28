using System;

namespace SpicyTemple.Core.Utils
{
    public static class Angles
    {
        public static float ToRadians(float degrees) => degrees * MathF.PI / 180.0f;

        public static float ToDegrees(float radians) => radians * 180.0f / MathF.PI;
    }
}