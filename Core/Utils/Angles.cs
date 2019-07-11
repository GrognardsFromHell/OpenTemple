using System;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Utils
{
    public static class Angles
    {
        public static readonly float OneDegreeInRadians = ToRadians(1.0f);

        public static float ToRadians(float degrees)
        {
            return degrees * MathF.PI / 180.0f;
        }

        public static float ToDegrees(float radians) => radians * 180.0f / MathF.PI;

        public static float RotationTo(this GameObjectBody from, GameObjectBody to)
        {
            return from.GetLocationFull().RotationTo(to.GetLocationFull());
        }

        public static float RotationTo(this LocAndOffsets from, LocAndOffsets to)
        {
            var fromLoc = from.ToInches2D();
            var toLoc = to.ToInches2D();
            return (toLoc - fromLoc).GetWorldRotation();
        }

        public static float ShortestAngleBetween(float angleFrom, float angleTo)
        {
            var neededRotation = NormalizeRadians(angleTo) - NormalizeRadians(angleFrom);

            if (neededRotation > MathF.PI)
            {
                neededRotation = -2 * MathF.PI + neededRotation;
            }
            else if (neededRotation < -MathF.PI)
            {
                neededRotation = 2 * MathF.PI + neededRotation;
            }

            return neededRotation;
        }

        public static float GetWorldRotation(this Vector2 directionalVector)
        {
            var angle = MathF.Atan2(directionalVector.X, directionalVector.Y) + ToRadians(135);
            angle = NormalizeRadians(angle);
            return angle;
        }

        public static float NormalizeRadians(float angleRadians)
        {
            while (angleRadians < 0)
            {
                angleRadians += 2 * MathF.PI;
            }

            while (angleRadians > 2 * MathF.PI)
            {
                angleRadians -= 2 * MathF.PI;
            }

            return angleRadians;
        }
    }
}