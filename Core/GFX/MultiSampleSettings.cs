using System;

namespace OpenTemple.Core.GFX;

public readonly struct MultiSampleSettings : IEquatable<MultiSampleSettings>
{
    public bool IsEnabled { get; }
    public int Samples { get; }
    public int Quality { get; }

    public MultiSampleSettings(bool isEnabled, int samples, int quality)
    {
        IsEnabled = isEnabled;
        Samples = samples;
        Quality = quality;
    }

    public bool Equals(MultiSampleSettings other)
    {
        return IsEnabled == other.IsEnabled && Samples == other.Samples && Quality == other.Quality;
    }

    public override bool Equals(object obj)
    {
        return obj is MultiSampleSettings other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsEnabled, Samples, Quality);
    }

    public static bool operator ==(MultiSampleSettings left, MultiSampleSettings right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MultiSampleSettings left, MultiSampleSettings right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        if (IsEnabled)
        {
            return $"On, {Samples} samples, quality={Quality}";
        }
        else
        {
            return "Off";
        }
    }
}