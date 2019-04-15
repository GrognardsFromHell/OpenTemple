using System;
using System.Diagnostics;

namespace SpicyTemple.Core.Time
{
    public readonly struct TimePoint : IComparable<TimePoint>
    {
        public readonly long Time;

        public TimePoint(long time)
        {
            Time = time;
        }

        public static TimePoint Now => new TimePoint(Stopwatch.GetTimestamp());

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
    }
}