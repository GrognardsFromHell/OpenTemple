using System;
using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Movies;

public class MovieSubtitles
{
    private static readonly Comparison<MovieSubtitleLine> StartTimeComparison =
        (a, b) => a.StartTime.CompareTo(b.StartTime);

    public List<MovieSubtitleLine> Lines { get; }

    public MovieSubtitles(List<MovieSubtitleLine> lines)
    {
        Lines = lines;
        Lines.Sort(StartTimeComparison);
    }

    public static MovieSubtitles Load(string subtitleFile)
    {
        var controlMes = Tig.FS.ReadMesFile($"rules/subtitles/{subtitleFile}");
        var textLines = Tig.FS.ReadMesFile($"mes/subtitles/{subtitleFile}");

        var lines = new List<MovieSubtitleLine>(controlMes.Count);
        foreach (var (_, controlLine) in controlMes)
        {
            var parts = controlLine.Split(';');
            if (parts.Length != 7)
            {
                throw new InvalidDataException($"Malformed subtitle line in {subtitleFile}: '{controlLine}'");
            }

            var startTime = int.Parse(parts[0]);
            var duration = int.Parse(parts[1]);
            var fontName = parts[2];
            var red = byte.Parse(parts[3]);
            var green = byte.Parse(parts[4]);
            var blue = byte.Parse(parts[5]);
            var textLineId = int.Parse(parts[6]);

            if (!textLines.TryGetValue(textLineId, out var textLine))
            {
                throw new InvalidDataException(
                    $"Subtitle file {subtitleFile} references unknown text-line {textLineId}");
            }

            lines.Add(new MovieSubtitleLine(
                startTime,
                duration,
                fontName,
                new PackedLinearColorA(red, green, blue, 255),
                textLine
            ));
        }

        return new MovieSubtitles(lines);
    }
}