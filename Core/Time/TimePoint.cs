
using System;
using System.Diagnostics;

namespace SpicyTemple.Core.Time
{
    public readonly struct TimePoint : IComparable<TimePoint>
    {
        public readonly long Time;

        public static readonly long TicksPerSecond = Stopwatch.Frequency;

        public static readonly long TicksPerMillisecond = Stopwatch.Frequency / 1000;

        public TimePoint(long time)
        {
            Time = time;
        }

        public static TimePoint Now => new TimePoint(Stopwatch.GetTimestamp());

        public double Seconds => Time / (double) Stopwatch.Frequency;

        public double Milliseconds => Seconds * 1000.0f;

        public bool Equals(TimePoint other)
        {
            return Time == other.Time;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TimePoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode();
        }

        public static bool operator ==(TimePoint left, TimePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimePoint left, TimePoint right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(TimePoint other)
        {
            return Time.CompareTo(other.Time);
        }

        public static TimeSpan operator -(TimePoint left, TimePoint right)
        {
            return new TimeSpan(left.Time - right.Time);
        }

        public static TimePoint operator -(TimePoint left, TimeSpan right)
        {
            return new TimePoint(left.Time - right.Ticks);
        }

        public static TimePoint operator +(TimePoint left, TimeSpan right)
        {
            return new TimePoint(left.Time + right.Ticks);
        }

        public static bool operator <(TimePoint left, TimePoint right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(TimePoint left, TimePoint right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(TimePoint left, TimePoint right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(TimePoint left, TimePoint right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}