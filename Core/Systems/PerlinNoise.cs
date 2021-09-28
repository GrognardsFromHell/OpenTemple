using System;
using System.Numerics;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems
{
    /// <summary>
    /// This is essentially the same code as the original perlin noise code found at
    /// https://mrl.cs.nyu.edu/~perlin/doc/oscar.html#noise
    /// Vanilla ToEE also uses the initialization code for 3D noise, but then never makes use of it,
    /// so we do not inherit that.
    /// </summary>
    public class PerlinNoise
    {
        private const int N = 4096;
        private const int B = 256;
        private const int BM = 0xFF;

        [TempleDllLocation(0x10ab7648)] [TempleDllLocation(0x10ab7a48)]
        private readonly int[] _indices = new int[B + B + 2];

        [TempleDllLocation(0x10b3a120)] [TempleDllLocation(0x10b3a520)]
        private readonly float[] _grid1d = new float[B + B + 2];

        [TempleDllLocation(0x10b3a930)]
        private readonly Vector2[] _grid2d = new Vector2[B + B + 2];

        public PerlinNoise()
        {
            // Generate random normal vectors
            for (var i = 0; i < B; i++)
            {
                _grid1d[i] = (float)(ThreadSafeRandom.NextDouble() * 2 - 1);
                _indices[i] = i;
                _grid2d[i] = VectorUtils.Random2DNormal();
            }

            _indices.AsSpan(0, B).Shuffle();

            // Fill the latter half of the arrays with the front to wrap around
            for (var i = 0; i < B + 2; i++)
            {
                _indices[B + i] = _indices[i];
                _grid1d[B + i] = _grid1d[i];
                _grid2d[B + i] = _grid2d[i];
            }
        }

        // Ease curve
        private static float SCurve(float t) => 3 * t * t - 2 * t * t * t;
        private static double SCurve(double t) => 3 * t * t - 2 * t * t * t;

        private static float Lerp(float t, float a, float b) => a + t * (b - a);
        private static double Lerp(double t, double a, double b) => a + t * (b - a);

        [TempleDllLocation(0x100876c0)]
        private float Sample2D(float x, float y)
        {
            if (x < 0 || y < 0)
            {
                // This method does not work right for negative sample points.
                throw new ArgumentOutOfRangeException();
            }

            float t;
            t = x + N;
            var bx0 = (int)t % B;
            var bx1 = (bx0 + 1) % B;
            var rx0 = t - (int)t;
            var rx1 = rx0 - 1f;

            t = y + N;
            var by0 = (int)t % B;
            var by1 = (by0 + 1) % B;
            var ry0 = t - (int)t;
            var ry1 = ry0 - 1f;

            var i = _indices[bx0];
            var j = _indices[bx1];

            // Get the vertex-indices for each corner (00 to pleft, 11 bottom right, so to speak)
            var b00 = _indices[i + by0];
            var b10 = _indices[j + by0];
            var b01 = _indices[i + by1];
            var b11 = _indices[j + by1];

            var sx = SCurve(rx0);
            var sy = SCurve(ry0);

            static float Dot(ref Vector2 q, float rx, float ry) => rx * q.X + ry * q.Y;

            float u, v;
            u = Dot(ref _grid2d[b00], rx0, ry0);
            v = Dot(ref _grid2d[b10], rx1, ry0);
            var a = Lerp(sx, u, v);

            u = Dot(ref _grid2d[b01], rx0, ry1);
            v = Dot(ref _grid2d[b11], rx1, ry1);
            var b = Lerp(sx, u, v);

            return Lerp(sy, a, b);
        }

        [TempleDllLocation(0x10087ac0)]
        public float Noise2D(float x, float y, float persistence = 4, float lacunarity = 4, int octaves = 3)
        {
            var result = 0.0f;
            var curPersistence = 1.0f;

            var xa = x;
            var ya = y;
            for (var i = 0; i < octaves; i++)
            {
                result += Sample2D(xa, ya) / curPersistence;
                curPersistence *= persistence;
                xa *= lacunarity;
                ya *= lacunarity;
            }

            return result;
        }

        /// <summary>
        /// This method is actually inlined heavily in ToEE vanilla, and thus has been adapted from Ken Perlin's
        /// original function.
        /// </summary>
        private double Sample1D(double arg)
        {
            if (arg < 0)
            {
                // This method does not work right for negative sample points.
                throw new ArgumentOutOfRangeException();
            }

            var t = arg + N;
            var bx0 = (int)t % B;
            var bx1 = (bx0 + 1) % B;
            var rx0 = t - (int)t;
            var rx1 = rx0 - 1f;

            var sx = SCurve(rx0);

            var u = rx0 * _grid1d[_indices[bx0]];
            var v = rx1 * _grid1d[_indices[bx1]];

            return Lerp(sx, u, v);
        }

        [TempleDllLocation(0x10089ee0)]
        [TempleDllLocation(0x10087810)]
        public float Noise1D(double x, float persistence = 4, float lacunarity = 4, int octaves = 3)
        {
            var result = 0.0;
            var curPersistence = 1.0f;

            var xa = x;
            for (var i = 0; i < octaves; i++)
            {
                result += Sample1D(xa) / curPersistence;
                curPersistence *= persistence;
                xa *= lacunarity;
            }

            return (float) result;
        }
    }
}