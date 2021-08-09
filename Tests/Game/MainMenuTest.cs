using OpenTemple.Core;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Tests.TestUtils;
using NUnit.Framework;

namespace OpenTemple.Tests.Game
{
    public class MainMenuTest
    {
        private readonly HeadlessGameFixture _game;

        public MainMenuTest(HeadlessGameFixture game)
        {
            _game = game;
        }

        [Test]
        public void CanStartAndReachMainMenu()
        {
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);

            _game.RenderFrame();
            _game.TakeScreenshot();
        }
    }
}