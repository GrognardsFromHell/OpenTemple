using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenTemple.Core.Config;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Styles;

[assembly: InternalsVisibleTo("OpenTemple.Tests")]

namespace OpenTemple.Core
{
    public class MainGame : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly SingleInstanceCheck _singleInstanceCheck = new SingleInstanceCheck();

        public static MainGame Instance { get; private set; }

        public GameFolders Folders { get; private set; }

        public GameConfig Config { get; }

        private readonly NativeMainWindow _mainWindow;

        public IMainWindow MainWindow => _mainWindow;

        public DebugUiSystem DebugUi { get; private set; }

        public MainGame()
        {
            Instance = this;

            Folders = new GameFolders();
            Globals.GameFolders = Folders;

            // If a debugger is attached, do not log to file, rather continue logging to the console
            if (!Debugger.IsAttached)
            {
                LoggingSystem.ChangeLogger(new FileLogger(Path.Join(Folders.UserDataFolder, "OpenTemple.log")));
            }

            Logger.Info("Starting OpenTemple - {0:u}", DateTime.Now);

            // Load the game configuration and - if necessary - write a default file
            Config = LoadConfig(Folders);

            var dataDirectory = Path.Join(Tig.GuessDataDirectory(), "ui");
            NativeMainWindow.AddUiSearchPath(dataDirectory);

            _mainWindow = new NativeMainWindow(Config.Window);
            _mainWindow.BaseUrl = "file:/" + dataDirectory.Replace('\\', '/') + "/";
            _mainWindow.OnClose += () => IsRunning = false;

            // Install a synchronization context that will allow us to dispatch tasks to the main thread
            UiSynchronizationContext.Install(_mainWindow);

            _mainWindow.Show();
        }

        public async Task Initialize()
        {
            if (!ValidateOrPickInstallation(Config))
            {
                MainWindow.Quit();
                return;
            }

            Tig.Startup(MainWindow, Config);

            Globals.ConfigManager.OnConfigChanged += () => Tig.UpdateConfig(Globals.ConfigManager.Config);

            // Hides the cursor during loading
            Tig.Mouse.HideCursor();

            await GameSystems.Init(MainWindow);

            new GameViews(_mainWindow, Config.Rendering);

            Globals.UiManager = new UiManager();
            Globals.UiAssets = new UiAssets();
            Globals.WidgetTextStyles = new WidgetTextStyles();
            Globals.WidgetButtonStyles = new WidgetButtonStyles();

            UiSystems.Startup(Config);

            DebugUi = new DebugUiSystem(MainWindow);

            GameSystems.LoadModule("ToEE");

            if (!Config.SkipIntro)
            {
                GameSystems.Movies.PlayMovie("movies/introcinematic.bik", 0, 0, 0);
            }

            await MainWindow.PostTask(() => Tig.Mouse.SetCursor("art/interface/cursors/MainCursor.tga"));

            // Show the main menu
            Tig.Mouse.ShowCursor();

            SceneManager.Instance = new SceneManager(MainWindow);

            await UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
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
            if (Instance == this)
            {
                Instance = null;
            }

            DebugUi?.Dispose();
            DebugUi = null;

            _mainWindow.Dispose();

            _singleInstanceCheck.Dispose();

            Logger.Info("Stopping OpenTemple - {0:u}", DateTime.Now);
        }

        private bool _running = true;

        public bool IsRunning
        {
            get
            {
                lock (this)
                {
                    return _running;
                }
            }
            set
            {
                lock (this)
                {
                    _running = value;
                }
            }
        }

        public void RunGameLoop()
        {
            while (IsRunning)
            {
                MainWindow.QueueUpdate();
                MainWindow.ProcessEvents();
            }
        }
    }
}