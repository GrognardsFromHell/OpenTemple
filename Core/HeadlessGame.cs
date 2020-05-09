using System;
using System.IO;
using OpenTemple.Core.Config;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core
{
    public sealed class HeadlessGame : IDisposable
    {
        private HeadlessGame(string toeeDir)
        {
            var config = new GameConfig
            {
                InstallationFolder = toeeDir
            };
            var settings = new TigSettings
            {
                DataFolder = FindDataFolder(),
                OffScreen = true
            };
            Globals.ConfigManager = new GameConfigManager(config);

            Tig.Startup(null, config, settings);

            GameSystems.InitializeSystems(new DummyLoadingProgress(), Tig.MainWindow);
            GameSystems.GameInit.IsEditor = true; // Prevent shopmap from opening
            GameSystems.LoadModule("ToEE", true);
        }

        public static HeadlessGame Start(string toeeDir)
        {
            return new HeadlessGame(toeeDir);
        }

        public void Dispose()
        {
        }

        private static string FindDataFolder()
        {
            // We usually assume that the Data directory is right below our executable location
            var assembly = typeof(HeadlessGame).Assembly;
            var location = Directory.GetParent(assembly.Location);

            do
            {
                var dataDirectory = Path.Join(location.FullName, "Data");
                if (Directory.Exists(dataDirectory))
                {
                    return dataDirectory;
                }

                location = location.Parent;
            } while (location != null);

            throw new InvalidOperationException("Failed to find data directory.");
        }
    }
}