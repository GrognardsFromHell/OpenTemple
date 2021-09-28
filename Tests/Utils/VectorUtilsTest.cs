using System;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Utils;

namespace OpenTemple.Tests.Utils
{
    public class VectorUtilsTest
    {
        [Test]
        public void TestRandom3DNormal()
        {
            // Ensures this test does not fail randomly
            ThreadSafeRandom.Seed = 1234;

            float minX = 0, maxX = 0, minY = 0, maxY = 0, minZ = 0, maxZ = 0;

            for (var i = 0; i < 10000; i++)
            {
                var v = VectorUtils.Random3DNormal();
                v.Length().Should().BeApproximately(1, 0.001f);

                minX = MathF.Min(minX, v.X);
                minY = MathF.Min(minY, v.Y);
                minZ = MathF.Min(minZ, v.Z);
                maxX = MathF.Max(maxX, v.X);
                maxY = MathF.Max(maxY, v.Y);
                maxZ = MathF.Max(maxZ, v.Z);
            }

            // This is just supposed to catch cases where the vectors only point into positive space or similar
            minX.Should().BeApproximately(-1, 0.01f);
            minY.Should().BeApproximately(-1, 0.01f);
            minZ.Should().BeApproximately(-1, 0.01f);
            maxX.Should().BeApproximately(1, 0.01f);
            maxY.Should().BeApproximately(1, 0.01f);
            maxZ.Should().BeApproximately(1, 0.01f);
        }

        [Test]
        public void TestRandom2DNormal()
        {
            // Ensures this test does not fail randomly
            ThreadSafeRandom.Seed = 1234;

            float minX = 0, maxX = 0, minY = 0, maxY = 0;

            for (var i = 0; i < 10000; i++)
            {
                var v = VectorUtils.Random2DNormal();
                v.Length().Should().BeApproximately(1, 0.001f);

                minX = MathF.Min(minX, v.X);
                minY = MathF.Min(minY, v.Y);
                maxX = MathF.Max(maxX, v.X);
                maxY = MathF.Max(maxY, v.Y);
            }

            // This is just supposed to catch cases where the vectors only point into positive space or similar
            minX.Should().BeApproximately(-1, 0.01f);
            minY.Should().BeApproximately(-1, 0.01f);
            maxX.Should().BeApproximately(1, 0.01f);
            maxY.Should().BeApproximately(1, 0.01f);
        }
    }
}