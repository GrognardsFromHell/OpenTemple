using System;

namespace SpicyTemple.Core.Config
{
    public struct SeenMovie
    {
        public int MovieId { get; set; }

        public int SoundtrackId { get; set; }

        public SeenMovie(int movieId, int soundtrackId = -1)
        {
            MovieId = movieId;
            SoundtrackId = soundtrackId;
        }

        public bool Equals(SeenMovie other)
        {
            return MovieId == other.MovieId && SoundtrackId == other.SoundtrackId;
        }

        public override bool Equals(object obj)
        {
            return obj is SeenMovie other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MovieId, SoundtrackId);
        }

        public static bool operator ==(SeenMovie left, SeenMovie right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SeenMovie left, SeenMovie right)
        {
            return !left.Equals(right);
        }
    }
}