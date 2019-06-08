using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Utils
{
    public static class Angles
    {
        public static float ToRadians(float degrees) => degrees * MathF.PI / 180.0f;

        public static float ToDegrees(float radians) => radians * 180.0f / MathF.PI;

        public static float RotationTo(this GameObjectBody from, GameObjectBody to)
        {
            return from.GetLocationFull().RotationTo(to.GetLocationFull());
        }

        public static float RotationTo(this LocAndOffsets from, LocAndOffsets to)
        {
            var fromLoc = from.ToInches2D();
            var toLoc = to.ToInches2D();

            var angle = MathF.Atan2(toLoc.Y - fromLoc.Y, toLoc.X - fromLoc.X) + ToRadians(135);
            if (angle < 0)
            {
                angle += MathF.PI * 2;
            }

            return angle;
        }
    }
}