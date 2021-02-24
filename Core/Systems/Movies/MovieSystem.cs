using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Scenes;
using OpenTemple.Core.TigSubsystems;

#nullable enable

namespace OpenTemple.Core.Systems.Movies
{
    public class MovieSystem : IGameSystem, IModuleAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x102ad0a8)]
        private readonly Dictionary<int, MovieDefinition> _movies = new();

        [TempleDllLocation(0x108ec6b8)]
        private readonly List<int> _movieQueue = new();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10033d90)]
        public void LoadModule()
        {
            _movies.Clear();

            var movieEntries = Tig.FS.ReadMesFile("movies/movies.mes");
            foreach (var (movieId, specLine) in movieEntries)
            {
                if (movieId != 0 && !string.IsNullOrWhiteSpace(specLine))
                {
                    _movies[movieId] = MovieDefinition.ParseLine(movieId, specLine);
                }
            }
        }

        [TempleDllLocation(0x10033dc0)]
        public void UnloadModule()
        {
            _movies.Clear();
        }

        [TempleDllLocation(0x10034100)]
        public static void PlayMovie(string moviePath, string? subtitleFile)
        {
            var subtitles = LoadSubtitles(subtitleFile);

            var scene = new MoviePlayerScene(moviePath, subtitles);

            GameSystems.SoundGame?.StashSchemes();
            Globals.Stage.PushScene(scene);
            try
            {
                while (!scene.IsAtEnd)
                {
                    Globals.GameLoop.RunOneIteration(false);
                    Thread.Sleep(1);
                }
            }
            finally
            {
                Globals.Stage.TryPopScene(scene);
                GameSystems.SoundGame?.UnstashSchemes();
            }
        }

        [TempleDllLocation(0x10034190)]
        public void PlayMovieSlide(string slidePath, string? musicPath, string? subtitleFile,
            int soundtrackId)
        {
            GameSystems.SoundGame.StashSchemes();
            try
            {
                var subtitles = LoadSubtitles(subtitleFile);

                Stub.TODO();
            }
            finally
            {
                GameSystems.SoundGame.UnstashSchemes();
            }
        }

        private static MovieSubtitles?  LoadSubtitles(string? subtitleFile)
        {
            return subtitleFile != null ? MovieSubtitles.Load(subtitleFile) : null;
        }

        /// <summary>
        /// Plays a movie from movies.mes, which could either be a slide or binkw movie.
        /// The soundtrack id is used for BinkW movies with multiple soundtracks.
        /// As far as we know, this is not used at all in ToEE.
        /// </summary>
        [TempleDllLocation(0x100341f0)]
        public void PlayMovieId(int movieId, int soundtrackId)
        {
            if (!_movies.TryGetValue(movieId, out var movieDefinition))
            {
                Logger.Warn("Cannot play unknown movie: {0}", movieId);
                return;
            }

            if (movieDefinition.MovieType == MovieType.BinkVideo)
            {
                PlayMovie(movieDefinition.MoviePath, movieDefinition.SubtitleFile);
            }
            else if (movieDefinition.MovieType == MovieType.Slide)
            {
                PlayMovieSlide(movieDefinition.MoviePath, movieDefinition.MusicPath, movieDefinition.SubtitleFile, soundtrackId);
            }
        }

        [TempleDllLocation(0x10033de0)]
        public void MovieQueueAdd(int movieId)
        {
            _movieQueue.Add(movieId);
        }

        [TempleDllLocation(0x100345a0)]
        public void MovieQueuePlay()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10034670)]
        public void MovieQueuePlayAndEndGame()
        {
            Stub.TODO();
        }
    }
}
