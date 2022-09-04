using System.Runtime.CompilerServices;

namespace OpenTemple.Core.Systems.Movies;

public class PlayMovieEvent
{
    public int MovieId { get; }
    public int SoundtrackId { get; }

    public bool Cancelled { get; private set; }
    public string? CancelledFilePath { get; private set; }
    public int CancelledLineNumber { get; private set; }

    public PlayMovieEvent(int movieId, int soundtrackId)
    {
        MovieId = movieId;
        SoundtrackId = soundtrackId;
    }

    public void Cancel([CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = -1)
    {
        Cancelled = true;
        CancelledFilePath = filePath;
        CancelledLineNumber = lineNumber;
    }
}
