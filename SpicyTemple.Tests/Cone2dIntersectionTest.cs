using System;
using System.Numerics;
using FluentAssertions;
using SpicyTemple.Core.Systems.GameObjects;
using Xunit;

namespace SpicyTemple.Tests
{
    public class Cone2dIntersectionTest
    {
        [Fact]
        public void TestNonIntersection()
        {
            var tester = new Cone2dIntersectionTester(new Vector2(1, 5), 1, 0, MathF.PI * 0.25f);
            tester.Intersects(new Vector2(1, 7), 1.1f).Should().BeTrue();
        }
    }
}