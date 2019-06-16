using System;
using System.Numerics;

namespace SpicyTemple.Core.AAS
{
    internal static class TriangleTests
    {
        private const float RayEpsilon = 1e-20f;

        /// <summary>
        /// This is ported from DirectXMath @
        /// https://github.com/microsoft/DirectXMath/blob/ecfb4754400dac581c2eeb6e849617cf5d210426/Inc/DirectXCollision.inl
        /// </summary>
        public static bool Intersects(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out float dist)
        {
            var e1 = v1 - v0;
            var e2 = v2 - v0;

            // p = Direction ^ e2;
            var p = Vector3.Cross( direction, e2 );

            // det = e1 * p;
            var det = Vector3.Dot( e1, p );

            float u, v, t;

            if( det >= RayEpsilon )
            {
                // Determinate is positive (front side of the triangle).
                var s = origin - v0;

                // u = s * p;
                u = Vector3.Dot( s, p );

                var noIntersection = u < 0 || u > det;

                // q = s ^ e1;
                Vector3 q = Vector3.Cross( s, e1 );

                // v = Direction * q;
                v = Vector3.Dot( direction, q );

                noIntersection |= v < 0 || u + v > det;

                // t = e2 * q;
                t = Vector3.Dot( e2, q );

                noIntersection |= t < 0;

                if( noIntersection )
                {
                    dist = 0;
                    return false;
                }
            }
            else if( det <= RayEpsilon )
            {
                // Determinate is negative (back side of the triangle).
                var s = origin - v0;

                // u = s * p;
                u = Vector3.Dot( s, p );

                var noIntersection = u > 0 || u < det;

                // q = s ^ e1;
                var q = Vector3.Cross( s, e1 );

                // v = Direction * q;
                v = Vector3.Dot( direction, q );

                noIntersection |= v > 0;
                noIntersection |= u + v < det;

                // t = e2 * q;
                t = Vector3.Dot( e2, q );

                noIntersection |= t > 0;

                if ( noIntersection )
                {
                    dist = 0;
                    return false;
                }
            }
            else
            {
                // Parallel ray.
                dist = 0;
                return false;
            }

            t /= det;

            // (u / det) and (v / dev) are the barycentric cooridinates of the intersection.

            // Store the x-component to *pDist
            dist = t;

            return true;
            
        }
    }
}