using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using OpenTemple.Core;
using OpenTemple.Core.DebugUI;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.IO.SaveGames.Archive;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Interop;

namespace OpenTemple.Windows
{
    public static class Launcher
    {
        public static void Main(string[] args)
        {
            // When a debugger is attached, immediately rethrow unobserved exceptions from asynchronous tasks
            if (Debugger.IsAttached)
            {
                TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
                {
                    if (!eventArgs.Observed)
                    {
                        throw eventArgs.Exception;
                    }
                };
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, e)
                    => HandleException(e.ExceptionObject as Exception);
            }

            if (JumpListHandler.Handle(args))
            {
                return;
            }

            if (args.Length > 0 && args[0] == "--extract-save")
            {
                ExtractSaveArchive.Main(args.Skip(1).ToArray());
                return;
            }

            if (args.Length == 2 && args[0] == "--mes-to-json")
            {
                var mesContent = MesFile.Read(args[1]);
                var newFile = Path.ChangeExtension(args[1], ".json");
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                var jsonContent = JsonSerializer.Serialize(mesContent.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ), options);
                File.WriteAllText(newFile, jsonContent);
                return;
            }

            if (args.Length > 0 && args[0] == "--dump-addresses")
            {
                var dumper = new AddressDumper();
                dumper.DumpAddresses();
                return;
            }

            using var mainGame = new MainGame();

            var gameTask = Task.Run(() => InitializeGame(mainGame));

            mainGame.RunGameLoop();

            gameTask.Wait();
        }

        private static async void InitializeGame(MainGame mainGame)
        {
            try
            {
                await mainGame.Initialize();

                var camera = Tig.RenderingDevice.GetCamera();
                camera.CenterOn(0, 0, 0);

                // TODO Globals.ConfigManager.OnConfigChanged += () => gameviews.UpdateConfig(Globals.Config.Rendering);

                // Run console commands from "startup.txt" (working dir)
                Tig.DynamicScripting.RunStartupScripts();

                // gameLoop.Run();
            }
            catch (Exception e)
            {
                HandleException(e);
                mainGame.MainWindow.Quit();
            }
        }

        private static void HandleException(Exception e)
        {
            var errorHeader = "Oops! A fatal error occurred.";

            var errorDetails = "Error Details:\n";
            errorDetails += e;

            try
            {
                NativePlatform.ShowMessage(
                    true,
                    "Fatal Error",
                    errorHeader,
                    errorDetails
                );
            }
            catch (Exception)
            {
                // In case the entire native library cant be loaded, the above call will fail
                // In those cases, we'll fall back to a super super low level MessageBox call,
                // which really shouldn't fail!
                var message = errorHeader + "\n\n" + errorDetails;
                message += "\n\nNOTE: You can press Ctrl+C to copy the content of this message box to your clipboard.";
                MessageBox(IntPtr.Zero, message, "OpenTemple - Fatal Error", 0x10);
            }

            Environment.Exit(-1);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern void MessageBox(IntPtr hwnd, string message, string title, int buttons);
    }
}