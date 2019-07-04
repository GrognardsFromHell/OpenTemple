using System.Linq;
using SpicyTemple.Core;
using SpicyTemple.Core.IO.SaveGames.Archive;
using SpicyTemple.Core.TigSubsystems;

namespace Launcher
{
    public static class LauncherProgram
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--extract-save")
            {
                ExtractSaveArchive.Main(args.Skip(1).ToArray());
                return;
            }

            using var spicyTemple = new SpicyTemple.Core.SpicyTemple();

            spicyTemple.Run();

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