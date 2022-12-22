using System;

namespace OpenTemple.Core.Systems.Sound;

public class SoundSchemeElement
{
    public SoundSchemeElementType Type = SoundSchemeElementType.Ambient;
    public bool Music;
    public bool Loop;
    public bool Over;

    /// <summary>
    /// 1/1000th chance every 250ms to trigger this event. Only makes sense for non-music events.
    /// </summary>
    public int Freq = 5;

    /// <summary>
    /// Limits the hour of the day that this element will occur after. 
    /// </summary>
    public int TimeFrom;

    /// <summary>
    /// Limits the hour of the day that this element will occur before. 
    /// </summary>
    public int TimeTo = 23;

    public int BalanceFrom = 50;
    public int BalanceTo = 50;
    public int VolFrom = 100;
    public int VolTo = 100;
    public bool Scatter;

    public required string Filename { get; init; }

    // Set to the current stream playing this element.
    public int StreamId = -1;

    [TempleDllLocation(0x1003bfd0)]
    public static SoundSchemeElement Parse(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.TrimEntries);
        var filename = parts[0];
        var result = new SoundSchemeElement {Filename = filename};

        static bool ParseRangePart(string part, string prefix, out int from, out int to)
        {
            if (!part.StartsWith(prefix))
            {
                from = default;
                to = default;
                return false;
            }

            var subparts = part.Substring(prefix.Length).Split("-");
            from = int.Parse(subparts[0]);
            to = subparts.Length >= 2 ? int.Parse(subparts[1]) : from;
            return true;
        }

        static bool ParseValuePart(string part, string prefix, out int value)
        {
            if (!part.StartsWith(prefix))
            {
                value = default;
                return false;
            }

            value = int.Parse(part.Substring(prefix.Length));
            return true;
        }

        for (var i = 1; i < parts.Length; i++)
        {
            var part = parts[i].ToLowerInvariant();
            if (ParseRangePart(part, "/vol:", out var volFrom, out var volTo))
            {
                result.VolFrom = volFrom;
                result.VolTo = volTo;
            }
            else if (part == "/combatmusic")
            {
                if (result.Type != SoundSchemeElementType.Ambient)
                {
                    throw new Exception("Cannot combine different option types in one line.");
                }

                result.Type = SoundSchemeElementType.CombatLoop;
            }
            else if (part == "/combatintro")
            {
                if (result.Type != SoundSchemeElementType.Ambient)
                {
                    throw new Exception("Cannot combine different option types in one line.");
                }

                result.Type = SoundSchemeElementType.CombatIntro;
            }
            else if (part == "/anchor")
            {
                if (result.Type != SoundSchemeElementType.Ambient)
                {
                    throw new Exception("Cannot combine different option types in one line.");
                }

                result.Type = SoundSchemeElementType.Anchor;
                result.Music = true;
            }
            else if (part == "/over")
            {
                if (result.Type != SoundSchemeElementType.Ambient)
                {
                    throw new Exception("Cannot combine different option types in one line.");
                }

                result.Type = SoundSchemeElementType.Over;
                result.Music = true;
                result.Over = true;
            }
            else if (part == "/loop")
            {
                if (result.Type != SoundSchemeElementType.Ambient)
                {
                    throw new Exception("Cannot combine different option types in one line.");
                }

                result.Type = SoundSchemeElementType.Loop;
                result.Music = true;
                result.Loop = true;
            }
            else if (ParseRangePart(part, "/time:", out var timeFrom, out var timeTo))
            {
                // Only valid for loops _or_ sound effects
                result.TimeFrom = timeFrom;
                result.TimeTo = timeTo;
            }
            else if (ParseValuePart(part, "/freq:", out var freq))
            {
                // Only valid for sound effects
                result.Freq = freq;
            }
            else if (ParseRangePart(part, "/bal:", out var balanceFrom, out var balanceTo))
            {
                // Only valid for sound effects
                result.BalanceFrom = balanceFrom;
                result.BalanceTo = balanceTo;
            }
            else if (part == "/scatter")
            {
                // Only valid for sound effects
                result.Scatter = true;
            }
        }

        if (result.VolTo < result.VolFrom)
        {
            result.VolTo = result.VolFrom;
        }

        if (result.BalanceTo < result.BalanceFrom)
        {
            result.BalanceTo = result.BalanceFrom;
        }

        static void PercentageToSByte(ref int value)
        {
            value = Math.Clamp(127 * value / 100, 0, 127);
        }

        PercentageToSByte(ref result.VolFrom);
        PercentageToSByte(ref result.VolTo);
        PercentageToSByte(ref result.BalanceFrom);
        PercentageToSByte(ref result.BalanceTo);

        return result;
    }
}