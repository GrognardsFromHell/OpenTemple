using System;
using System.IO;
using System.Threading.Tasks;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using QmlFiles.movies;

namespace OpenTemple.Core.Systems
{
    public class MovieSystem : IGameSystem, IModuleAwareSystem
    {
        private readonly IUserInterface _ui;

        public MovieSystem(IUserInterface ui)
        {
            _ui = ui;
        }

        public void Dispose()
        {
        }

        public void LoadModule()
        {
            // TODO movies
        }

        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10034100)]
        public async Task PlayMovie(string path, int p1 = 0, int p2 = 0, int p3 = 0)
        {
            path = Path.Join(Globals.Config.InstallationFolder, "data", path);

            var completionSource = new TaskCompletionSource<bool>();
            await _ui.PostTask(async () =>
            {
                var moviePlayer = await _ui.LoadView<MoviePlayerQml>("movies/MoviePlayer.qml");

                moviePlayer.OnEnded += () =>
                {
                    completionSource.SetResult(true);
                    moviePlayer.DeleteLater();
                };
                moviePlayer.Open(path);
            });

            await completionSource.Task;
        }

        /// <summary>
        /// Plays a movie from movies.mes, which could either be a slide or binkw movie.
        /// The soundtrack id is used for BinkW movies with multiple soundtracks.
        /// As far as we know, this is not used at all in ToEE.
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="flags"></param>
        /// <param name="soundtrackId"></param>
        [TempleDllLocation(0x100341f0)]
        public Task PlayMovieId(int movieId, int flags, int soundtrackId)
        {
            Stub.TODO();
            return Task.CompletedTask;
        }

        [TempleDllLocation(0x10033de0)]
        public void MovieQueueAdd(int movieId)
        {
            Stub.TODO();
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