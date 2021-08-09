using OpenTemple.Core;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Tests.TestUtils;
using NUnit.Framework;

namespace OpenTemple.Tests.Game
{
    public class MainMenuTest : HeadlessGameTest
    {

        [Test]
        public void CanStartAndReachMainMenu()
        {
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);

            Game.RenderFrame();
            Game.RenderFrame();
            Game.TakeScreenshot();
        }
    }
}