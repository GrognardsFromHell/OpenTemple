using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Movies;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.Styles;

[assembly: InternalsVisibleTo("OpenTemple.Tests")]

namespace OpenTemple.Core;

public class GameStartup : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly SingleInstanceCheck _singleInstanceCheck = new ();

    public string DataFolder { get; set; }

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

        Tig.Startup(config, new TigSettings {DataFolder = DataFolder});
        Globals.ConfigManager.OnConfigChanged += () => Tig.UpdateConfig(Globals.ConfigManager.Config);

        Globals.UiManager = new UiManager(Tig.MainWindow);

        // Hides the cursor during loading
#if !DEBUG
            Tig.Mouse.HideCursor();
#endif

        Globals.GameLoop = new GameLoop(
            Tig.MessageQueue,
            Tig.RenderingDevice,
            Tig.DebugUI
        );
        Tig.MainWindow.Closed += Globals.GameLoop.Stop;

        GameSystems.Init();

        Tig.Mouse.SetCursor("art/interface/cursors/MainCursor.tga");

        Globals.UiAssets = new UiAssets();
        Globals.UiStyles = new UiStyles();
        Globals.WidgetButtonStyles = new WidgetButtonStyles();

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

        GameSystems.Shutdown();
        UiSystems.DisposeAll();
        Tig.Shutdown();

        Globals.ConfigManager = null;
        Globals.GameFolders = null;
        Globals.GameLoop = null;
        Globals.UiAssets = null;
        Globals.UiManager = null;
        Globals.UiStyles = null;
        Globals.WidgetButtonStyles = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public void EnterMainMenu()
    {
        GameSystems.LoadModule("ToEE");

        if (!Globals.Config.SkipIntro)
        {
            MovieSystem.PlayMovie("movies/introcinematic.bik", null);
        }

        Tig.Mouse.ShowCursor();
        UiSystems.MainMenu.Show(MainMenuPage.MainMenu);
    }
}