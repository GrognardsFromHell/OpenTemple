using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using OpenTemple.Core;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.IO.SaveGames.Archive;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;

namespace Launcher
{
    public static class LauncherProgram
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

            if (!mainGame.Run())
            {
                return;
            }

            var camera = Tig.RenderingDevice.GetCamera();
            camera.CenterOn(0, 0, 0);

            var gameLoop = new GameLoop(
                Tig.MessageQueue,
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Globals.Config.Rendering,
                Tig.DebugUI
            );
            gameLoop.Run();
        }
    }
}