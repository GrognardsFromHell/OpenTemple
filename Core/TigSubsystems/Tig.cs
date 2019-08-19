using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.DebugUI;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Scripting;
using SpicyTemple.Core.Startup;

namespace SpicyTemple.Core.TigSubsystems
{
    public static class Tig
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        /// <summary>
        /// This is development scripting.
        /// </summary>
        public static IDevScripting DevScripting { get; set; }

        public static IFileSystem FS { get; set; }

        public static TigMouse Mouse { get; set; }

        public static TigKeyboard Keyboard { get; set; }

        public static SystemEventPump SystemEventPump { get; set; }

        public static MainWindow MainWindow { get; set; }

        public static MessageQueue MessageQueue { get; set; }

        public static RenderingDevice RenderingDevice { get; set; }

        public static Textures Textures => RenderingDevice.GetTextures();

        public static DebugUiSystem DebugUI { get; set; }

        public static MdfMaterialFactory MdfFactory { get; set; }

        public static ShapeRenderer2d ShapeRenderer2d { get; set; }

        public static ShapeRenderer3d ShapeRenderer3d { get; set; }

        public static TextLayouter TextLayouter { get; set; }

        public static TigWftScrollbar WftScrollbar { get; set; }

        public static TigSound Sound { get; set; }

        public static TigFonts Fonts { get; set; }

        public static TigConsole Console { get; set; }

        public static void Startup(GameConfig config)
        {
            Logger.Info("Initializing TIG");

            FS = CreateFileSystem(config.InstallationFolder);

            DevScripting = TryLoadDevScripting();

            MainWindow = new MainWindow(config.Window);

            var configRendering = config.Rendering;
            RenderingDevice = new RenderingDevice(
                FS,
                MainWindow,
                configRendering.AdapterIndex,
                configRendering.DebugDevice);
            RenderingDevice.SetAntiAliasing(configRendering.IsAntiAliasing,
                configRendering.MSAASamples,
                configRendering.MSAAQuality);

            DebugUI = new DebugUiSystem(MainWindow, RenderingDevice, RenderingDevice.GetCamera());

            MdfFactory = new MdfMaterialFactory(FS, RenderingDevice);
            ShapeRenderer2d = new ShapeRenderer2d(RenderingDevice);
            ShapeRenderer3d = new ShapeRenderer3d(RenderingDevice);
            TextLayouter = new TextLayouter(RenderingDevice, ShapeRenderer2d);

            // TODO mStartedSystems.emplace_back(StartSystem("idxtable.c", 0x101EC400, 0x101ECAD0));
            // TODO mStartedSystems.emplace_back(StartSystem("trect.c", TigStartupNoop, 0x101E4E40));
            // TODO mStartedSystems.emplace_back(StartSystem("color.c", 0x101ECB20, 0x101ED070));

            // TODO mLegacyVideoSystem = std::make_unique<LegacyVideoSystem>(*mMainWindow, *mRenderingDevice);


            // mStartedSystems.emplace_back(StartSystem("video.c", 0x101DC6E0, 0x101D8540));

            // TODO mStartedSystems.emplace_back(StartSystem("shader", 0x101E3350, 0x101E2090));
            // TODO mStartedSystems.emplace_back(StartSystem("palette.c", 0x101EBE30, 0x101EBEB0));
            // TODO mStartedSystems.emplace_back(StartSystem("window.c", 0x101DED20, 0x101DF320));
            // TODO mStartedSystems.emplace_back(StartSystem("timer.c", 0x101E34E0, 0x101E34F0));
            // mStartedSystems.emplace_back(StartSystem("dxinput.c", 0x101FF910, 0x101FF950));
            // mStartedSystems.emplace_back(StartSystem("keyboard.c", 0x101DE430, 0x101DE2D0));
            Keyboard = new TigKeyboard();

            // mStartedSystems.emplace_back(StartSystem("texture.c", 0x101EDF60, 0x101EE0A0));
            Mouse = new TigMouse();
            // TODO mStartedSystems.emplace_back(StartSystem("mouse.c", 0x101DDF50, 0x101DDE30));
            // TODO mStartedSystems.emplace_back(StartSystem("message.c", 0x101DE460, 0x101DE4E0));
            MessageQueue = new MessageQueue();
            SystemEventPump = new SystemEventPump();

            // startedSystems.emplace_back(StartSystem("gfx.c", TigStartupNoop, TigShutdownNoop));
            // TODO mStartedSystems.emplace_back(StartSystem("strparse.c", 0x101EBF00, TigShutdownNoop));
            // TODO mStartedSystems.emplace_back(StartSystem("filecache.c", TigStartupNoop, TigShutdownNoop));
            // TODO if (!config.noSound) {
            // TODO     mStartedSystems.emplace_back(StartSystem("sound.c", 0x101E3FA0, 0x101E48A0));
            Sound = new TigSound();
            // TODO }
            // TODO mSoundSystem = std::make_unique<temple::SoundSystem>();
            // TODO mMovieSystem = std::make_unique<temple::MovieSystem>(*mSoundSystem);
            // mStartedSystems.emplace_back(StartSystem("movie.c", 0x101F1090, TigShutdownNoop));
            // NOTE: WFT -> UiManager
            // TODO mStartedSystems.emplace_back(StartSystem("wft.c", 0x101F98A0, 0x101F9770));
            WftScrollbar = new TigWftScrollbar();

            // TODO mStartedSystems.emplace_back(StartSystem("font.c", 0x101EAEC0, 0x101EA140));
            Fonts = new TigFonts();
            Fonts.LoadAllFrom("art/arial-10");
            Fonts.PushFont("arial-10", 10);

            // TODO mConsole = std::make_unique<Console>();
            // mStartedSystems.emplace_back(StartSystem("console.c", 0x101E0290, 0x101DFBC0));
            Console = new TigConsole(DevScripting);
            // TODO mStartedSystems.emplace_back(StartSystem("loadscreen.c", 0x101E8260, TigShutdownNoop));

            // TODO *tigInternal.consoleDisabled = false; // tig init disables console by default
        }

        private static IDevScripting TryLoadDevScripting()
        {
            // The dev scripting assembly is optional so it doesn't need to be loaded during normal gameplay
            try
            {
                var devScriptingAssembly = Assembly.Load("DevScripting");
                var devScriptingType = devScriptingAssembly.GetType("SpicyTemple.DevScripting.DevScripting");
                return (IDevScripting) Activator.CreateInstance(devScriptingType);
            }
            catch (Exception e)
            {
                Logger.Info("Unable to activate development scripting: {0}", e);
                return new DisabledDevScripting();
            }
        }

        private static IFileSystem CreateFileSystem(string installationFolder)
        {
            var report = ToEEInstallationValidator.Validate(installationFolder);
            if (!report.IsValid)
            {
                Logger.Error("Failed to validate ToEE installation directory:");
                foreach (var message in report.Messages)
                {
                    Logger.Error("  - " + message);
                }

                return null;
            }

            Logger.Info("Using ToEE installation from '{0}'", installationFolder);

            var vfs = TroikaVfs.CreateFromInstallationDir(installationFolder);
            vfs.AddDataDir(Path.GetFullPath(@"..\Data"));
            return vfs;
        }
    }
}