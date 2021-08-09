using System;
using OpenTemple.Core;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// Provides a running game instance that can render to an off-screen surface for tests.
    /// </summary>
    public class HeadlessGameFixture : IDisposable
    {
        private readonly TempDirectory _userData = new();

        private HeadlessGame Game { get; }

        public HeadlessGameFixture()
        {
            var toeeDir = Environment.GetEnvironmentVariable("TOEE_DIR");
            if (toeeDir == null)
            {
                throw new NotSupportedException(
                    "Cannot run a test based on real data because TOEE_DIR environment variable is not set."
                );
            }

            LoggingSystem.ChangeLogger(new TestLogger());

            var options = new HeadlessGameOptions(toeeDir)
            {
                UserDataFolder = _userData.Path,
                WithUserInterface = true
            };

            Game = HeadlessGame.Start(options);

            Globals.GameLoop = new GameLoop(
                Tig.MessageQueue,
                Tig.RenderingDevice,
                Tig.DebugUI
            );
        }

        public void RenderFrame()
        {
            Globals.GameLoop.RenderFrame();
        }

        public void TakeScreenshot()
        {
            Tig.RenderingDevice.TakeScaledScreenshot("screenshot.jpg");
        }

        public void Dispose()
        {
            Game.Dispose();
            _userData.Dispose();
        }
    }
}