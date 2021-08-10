using System;
using System.Drawing;
using System.IO;
using OpenTemple.Core.Config;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.Styles;

#nullable enable

namespace OpenTemple.Core
{
    public sealed class HeadlessGame : IDisposable
    {
        private HeadlessGame(HeadlessGameOptions options)
        {
            var config = new GameConfig
            {
                InstallationFolder = options.InstallationFolder,
                SkipIntro = true,
                SkipLegal = true,
                // Debug UI makes no sense in the headless client
                EnableDebugUI = false,
                Rendering =
                {
                    DebugDevice = options.UseDebugRenderer
                }
            };
            var settings = new TigSettings
            {
                DataFolder = options.OpenTempleDataPath ?? FindDataFolder(),
                OffScreen = true,
                DisableSound = true
            };
            Globals.ConfigManager = new GameConfigManager(config);
            Globals.GameFolders = new GameFolders(options.UserDataFolder);

            Tig.Startup(config, settings);

            GameSystems.InitializeFonts();
            GameSystems.InitializeSystems(new DummyLoadingProgress());

            if (options.WithUserInterface)
            {
                Globals.UiManager = new UiManager(Tig.MainWindow);
                Globals.UiAssets = new UiAssets();
                Globals.UiStyles = new UiStyles();
                Globals.WidgetButtonStyles = new WidgetButtonStyles();

                UiSystems.Startup(config);
            }

            GameSystems.GameInit.IsEditor = !options.OpenStartMap; // Prevent shopmap from opening
            GameSystems.LoadModule("ToEE", true);
            // If we don't open a start map, we still need to set the campaign start time
            if (!options.OpenStartMap)
            {
                GameSystems.GameInit.SetupStartingTime();
            }

        }

        public static HeadlessGame Start(HeadlessGameOptions options)
        {
            return new HeadlessGame(options);
        }

        public void Dispose()
        {
            // Reset all of the UI and GameSystems
            UiSystems.DisposeAll();
            GameSystems.Shutdown();
            Tig.Shutdown();
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

    public class HeadlessGameOptions
    {
        /// <summary>
        /// Where is ToEE installed. Used to find the original game's data files.
        /// </summary>
        public string InstallationFolder { get; }

        /// <summary>
        /// Path to where user data such as config and save-games is stored.
        /// The current user's default will be used if this is not specified.
        /// </summary>
        public string? UserDataFolder { get; init; }

        /// <summary>
        /// Where is OpenTemple's data directory located?
        /// An attempt at auto-detecting the location will be made when this is null.
        /// </summary>
        public string? OpenTempleDataPath { get; init; }

        /// <summary>
        /// The size of the off-screen rendering surface.
        /// </summary>
        public Size SurfaceSize { get; init; } = new(1024, 768);

        /// <summary>
        /// Enables initialization of the UI systems.
        /// </summary>
        public bool WithUserInterface { get; init; }

        /// <summary>
        /// Enable the debug layer for the offscreen rendering device to get more usable debug output.
        /// Will be much slower.
        /// </summary>
        public bool UseDebugRenderer { get; init; }

        /// <summary>
        /// If set to true, the shop map will be loaded upon startup.
        /// Keep in mind that working with game objects requires a map to be open.
        /// </summary>
        public bool OpenStartMap { get; init; }

        public HeadlessGameOptions(string installationFolder)
        {
            InstallationFolder = installationFolder;
        }
    }
}