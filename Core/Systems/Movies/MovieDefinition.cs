using System.IO;
using OpenTemple.Core.Logging;



namespace OpenTemple.Core.Systems.Movies;

public class MovieDefinition
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public int Id { get; }

    public string MoviePath { get; }

    public MovieType MovieType { get; }

    public string? MusicPath { get; }

    /// <summary>
    /// Name (not path) of the subtitle file to use. May be null for no subtitles.
    /// Control file can be found in rules\subtitles\, while the actual text is in mes\subtitles\.
    /// </summary>
    public string? SubtitleFile { get; }

    public MovieDefinition(int id, string moviePath, MovieType movieType, string? musicPath, string? subtitleFile)
    {
        Id = id;
        MoviePath = moviePath;
        MovieType = movieType;
        MusicPath = musicPath;
        SubtitleFile = subtitleFile;
    }

    public static MovieDefinition ParseLine(int id, string specLine)
    {
        string? musicFile = null;
        string? subtitleFile = null;
        string? movieFile = null;
        MovieType movieType = default;

        // Ending slides 61 and 62 are broken in that they use , as separator
        var parts = specLine.Split(';', ',');
        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();
            if (trimmedPart.Length == 0)
            {
                continue;
            }

            if (trimmedPart.EndsWith(".mes"))
            {
                subtitleFile = trimmedPart;
            }
            else if (trimmedPart.EndsWith(".mp3"))
            {
                musicFile = trimmedPart;
            }
            else if (trimmedPart.EndsWith(".bmp") || trimmedPart.EndsWith(".tga") || trimmedPart.EndsWith(".jpg"))
            {
                movieFile = trimmedPart;
                movieType = MovieType.Slide;
            }
            else if (trimmedPart.EndsWith(".bik"))
            {
                movieFile = trimmedPart;
                movieType = MovieType.BinkVideo;
            }
            else
            {
                Logger.Warn("Unrecognized part in movie definition {0}: {1}", id, trimmedPart);
            }
        }

        if (movieFile == null)
        {
            throw new InvalidDataException($"Movie definition {id} is missing movie filename.");
        }

        // Adjust to full paths
        switch (movieType)
        {
            case MovieType.BinkVideo:
                movieFile = "movies/" + movieFile;
                break;
            case MovieType.Slide:
                movieFile = "slide/" + movieFile;
                if (musicFile != null)
                {
                    musicFile = "sound/speech/slide/" + musicFile;
                }
                break;
        }

        return new MovieDefinition(id, movieFile, movieType, musicFile, subtitleFile);
    }
}