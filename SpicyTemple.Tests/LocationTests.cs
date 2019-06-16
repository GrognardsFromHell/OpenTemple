using System;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Utils;
using Xunit;

namespace SpicyTemple.Tests
{

    public class LocationTests
    {

        private static locXY Center => new locXY(480,480);

        private static void AssertAngles(float expectedDegrees, float actualRadians)
        {
            var expectedRadians = Angles.ToRadians(expectedDegrees);
            var delta = Angles.ShortestAngleBetween(expectedRadians, actualRadians);
            if (MathF.Abs(delta) < Angles.OneDegreeInRadians)
            {
                return; // Everything okay!
            }

            Assert.Equal(expectedRadians, actualRadians);
        }

        [Fact]
        public void RotationTowardsUpIsZero()
        {
            var top = new LocAndOffsets(Center.Offset(CompassDirection.Top));
            var rot = new LocAndOffsets(Center).RotationTo(top);
            AssertAngles(0.0f, rot);
        }

        [Fact]
        public void RotationTowardsRightIs90Degrees()
        {
            var right = new LocAndOffsets(Center.Offset(CompassDirection.Right));
            var rot = new LocAndOffsets(Center).RotationTo(right);
            AssertAngles(90, rot);
        }

        [Fact]
        public void RotationTowardsBottomIs180Degrees()
        {
            var bottom = new LocAndOffsets(Center.Offset(CompassDirection.Bottom));
            var rot = new LocAndOffsets(Center).RotationTo(bottom);
            AssertAngles(180, rot);
        }

        [Fact]
        public void RotationTowardsLeftIs270Degrees()
        {
            var left = new LocAndOffsets(Center.Offset(CompassDirection.Left));
            var rot = new LocAndOffsets(Center).RotationTo(left);
            AssertAngles(270, rot);
        }

    }
}