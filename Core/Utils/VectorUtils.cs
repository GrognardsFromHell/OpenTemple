using System;
using System.Diagnostics;
using System.Threading;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace OpenTemple.Core.Utils
{
    public static class VectorUtils
    {
        public static Vector2 Random2DNormal()
        {
            var angle = ThreadSafeRandom.NextDouble() * 2 * Math.PI; // Random angle (radians) in [0,360)
            var result = new Vector2(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle)
            );
            Debug.Assert(MathF.Abs(result.LengthSquared() - 1) < 0.001f);
            return result;
        }

        public static Vector3 Random3DNormal()
        {
            // See https://stackoverflow.com/questions/5408276/sampling-uniformly-distributed-random-points-inside-a-spherical-volume
            var phi = ThreadSafeRandom.NextDouble() * 2 * Math.PI; // Random angle (radians) in [0,360)
            var costheta = ThreadSafeRandom.NextDouble() * 2 - 1; // Random point in [-1,1)

            var theta = Math.Acos(costheta);
            var result = new Vector3(
                (float)(Math.Sin(theta) * Math.Cos(phi)),
                (float)(Math.Sin(theta) * Math.Sin(phi)),
                (float)Math.Cos(theta)
            );
            Debug.Assert(MathF.Abs(result.LengthSquared() - 1) < 0.001f);
            return result;
        }
    }
}