using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Config;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Scenes;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Widgets;

[assembly: InternalsVisibleTo("OpenTemple.Tests")]

namespace OpenTemple.Core
{
    /// <summary>
    /// Manages the orderly startup of game systems, and loading data at startup.
    /// </summary>
    public class GameStartup : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly SingleInstanceCheck _singleInstanceCheck = new();

        public string DataFolder { get; init; }

        public bool Startup()
        {
            var gameFolders = new GameFolders();
            Globals.GameFolders = gameFolders;

            // If a debugger is attached, do not log to file, rather continue logging to the console
            if (!Debugger.IsAttached)
            {
                LoggingSystem.ChangeLogger(new FileLogger(Path.Join(gameFolders.UserDataFolder, "OpenTemple.log")));
            }

            Logger.Info("Starting OpenTemple - {0:u}", DateTime.Now);

            // Load the game configuration and - if necessary - write a default file
            var config = LoadConfig(gameFolders);

            if (!ValidateOrPickInstallation(config))
            {
                return false;
            }

            Tig.Startup(config, new TigSettings() {DataFolder = DataFolder});
            Globals.ConfigManager.OnConfigChanged += () => Tig.UpdateConfig(Globals.ConfigManager.Config);

            // Hides the cursor during loading
            Tig.Mouse.HideCursor();

            Globals.Stage = new StageManager(Tig.MainWindow);

            Globals.GameLoop = new GameLoop(
                Tig.MessageQueue,
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Globals.Config.Rendering,
                Tig.DebugUI
            );
            Tig.MainWindow.Closed += Globals.GameLoop.Stop;

            GameSystems.Init();

            Tig.Mouse.SetCursor("art/interface/cursors/MainCursor.tga");

            Globals.UiManager = new UiManager();
            Globals.UiAssets = new UiAssets();
            Globals.WidgetTextStyles = new WidgetTextStyles();
            Globals.WidgetButtonStyles = new WidgetButtonStyles();

            TranslationService.Translator = Globals.UiAssets.ApplyTranslation;

            UiSystems.Startup(config);

            return true;
        }

        private bool ValidateOrPickInstallation(GameConfig config)
        {
            var currentPath = config.InstallationFolder;

            // If the directory is initially not set, try auto-detection
            if (currentPath == null)
            {
                if (InstallationDirSelector.TryFind(out currentPath))
                {
                    Logger.Info("Auto-Detected ToEE installation directory: {0}", currentPath);
                }
            }

            while (true)
            {
                var validationReport = ToEEInstallationValidator.Validate(currentPath);
                if (validationReport.IsValid)
                {
                    break;
                }

                if (!InstallationDirSelector.Select(
                    validationReport,
                    currentPath,
                    out var selectedPath
                ))
                {
                    return false;
                }

                currentPath = selectedPath;
            }

            if (currentPath != config.InstallationFolder)
            {
                config.InstallationFolder = currentPath;
                Globals.ConfigManager.Save();
                Logger.Info("Set installation directory to {0}", currentPath);
            }

            return true;
        }

        private GameConfig LoadConfig(GameFolders gameFolders)
        {
            var configManager = new GameConfigManager(gameFolders);
            Globals.ConfigManager = configManager;
            return configManager.Config;
        }

        public void Dispose()
        {
            _singleInstanceCheck.Dispose();
        }

        public void EnterMainMenu()
        {
            GameSystems.LoadModule("ToEE");

            if (!Globals.Config.SkipIntro)
            {
                GameSystems.Movies.PlayMovie("movies/introcinematic.bik", 0, 0, 0);
            }

            // Show the main menu
            Globals.Stage.PushScene(new MainMenuScene());

            Tig.Mouse.ShowCursor();
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
        }
    }
}
