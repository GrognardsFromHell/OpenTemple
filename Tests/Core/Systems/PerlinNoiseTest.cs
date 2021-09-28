using System;
using System.Diagnostics;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Portability;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Tests.Core.Systems
{
    public class PerlinNoiseTest
    {
        [Test]
        public void TestNoise1D()
        {
            ThreadSafeRandom.Seed = 1234;
            var noise = new PerlinNoise();

            var samples = new double[10000];
            for (var i = 0; i < samples.Length; i++)
            {
                var v = (ushort)ThreadSafeRandom.Next() + ThreadSafeRandom.NextDouble();
                samples[i] = noise.Noise1D(v, 1, 1, 1);
                if (samples[i] > 1 || samples[i] < -1)
                {
                    Debugger.Break();
                }
            }

            var stats = new Statistics(samples);
            stats.Mean.Should().BeApproximately(0, 0.1f);
            // Vanilla's Perlin1 also seems to produce values in this range for some reason.
            stats.Min.Should().BeApproximately(-0.5f, 0.1f);
            stats.Max.Should().BeApproximately(0.5f, 0.1f);
        }

        [Test]
        public void TestNoise2D()
        {
            ThreadSafeRandom.Seed = 1234;
            var noise = new PerlinNoise();

            var samples = new double[10000];
            for (var i = 0; i < samples.Length; i++)
            {
                var v = (ushort)ThreadSafeRandom.Next() + (float) ThreadSafeRandom.NextDouble();
                samples[i] = noise.Noise2D(v, v, 1, 1, 1);
                if (samples[i] > 1 || samples[i] < -1)
                {
                    Debugger.Break();
                }
            }

            var stats = new Statistics(samples);
            stats.Mean.Should().BeApproximately(0, 0.1f);
            // Vanilla's Perlin1 also seems to produce values in this range for some reason.
            stats.Min.Should().BeApproximately(-0.6f, 0.1f);
            stats.Max.Should().BeApproximately(0.6f, 0.1f);
        }
    }
}