using System;
using System.Diagnostics;
using System.Numerics;

namespace SpicyTemple.Core.Utils
{
    public struct LineSegment
    {
        public readonly Vector2 From;

        public readonly Vector2 To;

        private readonly Vector2 _normDirection;

        private readonly float _length;

        // Cached
        private float _fromDotDirection;

        public LineSegment(Vector2 from, Vector2 to)
        {
            From = from;
            To = to;
            var direction = to - from;
            _length = direction.Length();
            if (_length > 0)
            {
                _normDirection = direction / _length;
            }
            else
            {
                _normDirection = default;
            }

            _fromDotDirection = float.NaN;
        }

        public LineSegment(Vector2 from, Vector2 direction, float length)
        {
            Trace.Assert(Math.Abs(direction.LengthSquared() - 1.0f) < 0.001f);
            From = from;
            To = from + direction * length;
            _normDirection = direction;
            _length = length;
            _fromDotDirection = float.NaN;
        }

        /// <summary>
        /// Compute the point on the line segment that is closest to the given point.
        /// </summary>
        public Vector2 GetClosesetPoint(Vector2 point)
        {
            // Compute and store the dot product on demand
            if (float.IsNaN(_fromDotDirection))
            {
                _fromDotDirection = Vector2.Dot(_normDirection, From);
            }

            var projection = Vector2.Dot(point, _normDirection) - _fromDotDirection;

            // Limit the closest point to the line semgent
            projection = Math.Clamp(projection, 0, _length);

            // Compute the point on the line
            return From + projection * _normDirection;
        }

        /// <summary>
        /// Gets the closest distance of the given point from any other point on this line segment.
        /// </summary>
        public float GetDistanceFromLine(Vector2 point)
        {
            var closestPoint = GetClosesetPoint(point);
            return (point - closestPoint).Length();
        }

        /// <summary>
        /// Gets the closest squared distance of the given point from any other point on this line segment.
        /// </summary>
        public float GetDistanceFromLineSquared(Vector2 point)
        {
            var closestPoint = GetClosesetPoint(point);
            return (point - closestPoint).LengthSquared();
        }

    }
}