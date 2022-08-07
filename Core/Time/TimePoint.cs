
using System;
using System.Diagnostics;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Time;

public readonly struct TimePoint : IComparable<TimePoint>
{
    public readonly long Time;

    public static readonly long TicksPerHour = Stopwatch.Frequency * 60 * 60;

    public static readonly long TicksPerSecond = Stopwatch.Frequency;

    public static readonly long TicksPerMillisecond = Stopwatch.Frequency / 1000;

    /// <summary>
    /// Conversion from the ticks in this structure to the ticks found in TimeSpan.
    /// </summary>
    private static long ToTimeSpanTicks(long ticks)
    {
        return ticks * TimeSpan.TicksPerMillisecond / TicksPerMillisecond;
    }

    /// <summary>
    /// Conversion from the ticks found in TimeSpan to the ticks found in this structure.
    /// </summary>
    private static long FromTimeSpanTicks(long ticks)
    {
        return ticks / TimeSpan.TicksPerMillisecond * TicksPerMillisecond;
    }

    private const int SecondsPerDay = 24 * 60 * 60;

    private static long _fakeTime = -1;

    public TimePoint(long time)
    {
        Time = time;
    }

    public static TimePoint Now
    {
        get
        {
            if (_fakeTime >= 0)
            {
                return new TimePoint(_fakeTime);
            }
            return new TimePoint(Stopwatch.GetTimestamp());
        }
    }

    public static void SetFakeTime(TimePoint timePoint)
    {
        _fakeTime = timePoint.Time;
    }

    public static void ClearFakeTime()
    {
        _fakeTime = -1;
    }

    public double Seconds => Time / (double) TicksPerSecond;

    public double Milliseconds => Time / (double) TicksPerMillisecond;

    public bool Equals(TimePoint other)
    {
        return Time == other.Time;
    }

    public override bool Equals(object? obj)
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
        return new TimeSpan(ToTimeSpanTicks(left.Time - right.Time));
    }

    public static TimePoint operator -(TimePoint left, TimeSpan right)
    {
        return new TimePoint(left.Time - FromTimeSpanTicks(right.Ticks));
    }

    public static TimePoint operator +(TimePoint left, TimeSpan right)
    {
        return new TimePoint(left.Time + FromTimeSpanTicks(right.Ticks));
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

    public GameTime ToGameTime()
    {
        var ms = (long) Milliseconds;
        var msecs = ms % (SecondsPerDay * 1000);
        var days = ms / (SecondsPerDay * 1000);
        return new GameTime((int) days, (int) msecs);
    }
}