using System;
using System.Numerics;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Utils
{
    public struct Cylinder3d
    {
        public Vector3 baseCenter;
        public float radius;
        public float height;

        public bool HitTest(in Ray3d ray, out float distance)
        {
            distance = float.MaxValue;

            float t;
            // Intersect segment S(t)=sa+t(sb-sa), 0<=t<=1 against cylinder specifiedby p, q and r
            // int IntersectSegmentCylinder(Point sa, Point sb, Point p, Point q, float r, float &t)
            var sa = ray.origin;
            var sb = sa + ray.direction;
            var rayLength = ray.direction.Length();

            var p = baseCenter;

            // Fix issues with degenerated cylinders (height=0)
            var effectiveHeight = 1.0f;
            if (height > effectiveHeight)
            {
                effectiveHeight = height;
            }

            var q = p + new Vector3(0, effectiveHeight, 0);

            var d = q - p;
            var m = sa - p;
            var n = sb - sa;
            float md = Vector3.Dot(m, d);
            float nd = Vector3.Dot(n, d);
            float dd = Vector3.Dot(d, d);
            // Test if segment fully outside either endcap of cylinder
            if (md < 0.0f && md + nd < 0.0f) return false; // Segment outside ’p’ side of cylinder
            if (md > dd && md + nd > dd) return false; // Segment outside ’q’ side of cylinder
            float nn = Vector3.Dot(n, n);
            float mn = Vector3.Dot(m, n);
            float a = dd * nn - nd * nd;
            float k = Vector3.Dot(m, m) - radius * radius;
            float c = dd * k - md * md;
            if (MathF.Abs(a) < 0.0001f)
            {
                // Segment runs parallel to cylinder axis
                if (c > 0.0f) return false; // ’a’ and thus the segment lie outside cylinder
                // Now known that segment intersects cylinder; figure out how it intersects
                if (md < 0.0f) t = -mn / nn; // Intersect segment against ’p’ endcap
                else if (md > dd) t = (nd - mn) / nn; // Intersect segment against ’q’ endcap
                else t = 0.0f; // ’a’ lies inside cylinder
                distance = t * rayLength;
                return true;
            }

            float b = dd * mn - nd * md;
            float discr = b * b - a * c;
            if (discr < 0.0f) return false; // No real roots; no intersection
            t = (-b - MathF.Sqrt(discr)) / a;
            if (t < 0.0f || t > 1.0f) return false; // Intersection lies outside segment
            if (md + t * nd < 0.0f)
            {
                // Intersection outside cylinder on ’p’ side
                if (nd <= 0.0f) return false; // Segment pointing away from endcap
                t = -md / nd;
                distance = t * rayLength;
                // Keep intersection if Dot(S(t) - p, S(t) - p) <= r ? 2
                return k + 2 * t * (mn + t * nn) <= 0.0f;
            }
            else if (md + t * nd > dd)
            {
                // Intersection outside cylinder on ’q’ side
                if (nd >= 0.0f) return false; // Segment pointing away from endcap
                t = (dd - md) / nd;
                distance = t * rayLength;
                // Keep intersection if Dot(S(t) - q, S(t) - q) <= r ? 2
                return k + dd - 2 * md + t * (2 * (mn - nd) + t * nn) <= 0.0f;
            }

            distance = t * rayLength;
            // Segment intersects cylinder between the endcaps; t is correct
            return true;
        }
    }
}