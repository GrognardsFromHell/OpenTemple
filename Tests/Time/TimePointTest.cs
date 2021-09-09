using System;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Time;

namespace OpenTemple.Tests.Time
{
    public class TimePointTest
    {
        [Test]
        public void TestTimeSpanDelta()
        {
            var tp1 = new TimePoint(TimePoint.TicksPerMillisecond * 1250);
            var tp2 = new TimePoint(TimePoint.TicksPerMillisecond * 4000);
            (tp2 - tp1).TotalMilliseconds.Should().Be(2750);
        }

        [Test]
        public void TestTimeSpanSubtraction()
        {
            var tp = new TimePoint(TimePoint.TicksPerMillisecond * 1000);
            var ts = TimeSpan.FromMilliseconds(250);
            (tp - ts).Milliseconds.Should().Be(750);
        }

        [Test]
        public void TestTimeSpanAddition()
        {
            var tp = new TimePoint(TimePoint.TicksPerMillisecond * 1000);
            var ts = TimeSpan.FromMilliseconds(250);
            (tp + ts).Milliseconds.Should().Be(1250);
        }
    }
}