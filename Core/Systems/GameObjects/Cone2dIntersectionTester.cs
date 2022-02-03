using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace OpenTemple.Core.Systems.GameObjects
{
    public readonly struct Cone2dIntersectionTester
    {
        private readonly Vector2 origin;

        private readonly float radius;

        private readonly float radiusSquared;

        private readonly float NEGSIN_AngleMin;
        private readonly float COS_AngleMin;
        private readonly float SIN_AngleMax;
        private readonly float NEGCOS_AngleMax;

        private readonly bool coneLargerThanHalfCircle;

        public Cone2dIntersectionTester(Vector2 origin, float radius, float coneStartAngleRad, float coneArcRad)
        {
            this.origin = origin;
            this.radius = radius;
            radiusSquared = radius * radius;

            NEGSIN_AngleMin = -MathF.Sin(coneStartAngleRad);
            COS_AngleMin = MathF.Cos(coneStartAngleRad);
            SIN_AngleMax = MathF.Sin(coneArcRad);
            NEGCOS_AngleMax = -MathF.Cos(coneArcRad);

            coneLargerThanHalfCircle = coneArcRad >= MathF.PI;
        }

        [Pure]
        public bool Intersects(Vector2 position, float radius)
        {
            var relPos = position - origin;
            var distanceSquared = relPos.LengthSquared() - radius;
            if (distanceSquared > radiusSquared)
            {
                return false;
            }

            // Check the intersection against the cone angle only if the other object doesnt intersect
            // the origin.
            if (distanceSquared <= 0)
            {
                return true;
            }

            if (coneLargerThanHalfCircle)
            {
                if (relPos.Y * NEGSIN_AngleMin + relPos.X * COS_AngleMin + radius > 0.0f)
                {
                    return true;
                }

                return false;
            }
            else if (relPos.Y * NEGSIN_AngleMin + relPos.X * COS_AngleMin + radius <= 0.0f)
            {
                return false;
            }

            if (relPos.Y * SIN_AngleMax + relPos.X * NEGCOS_AngleMax + radius > 0.0f)
            {
                return true;
            }

            return false;
        }
    }
}