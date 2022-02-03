using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Systems.Movies;

public readonly struct MovieSubtitleLine
{
    public int StartTime { get; }
    public int Duration { get; }
    public string FontName { get; }
    public PackedLinearColorA Color { get; }
    public string Text { get; }

    public MovieSubtitleLine(int startTime, int duration, string fontName, PackedLinearColorA color, string text)
    {
        StartTime = startTime;
        Duration = duration;
        FontName = fontName;
        Color = color;
        Text = text;
    }
}