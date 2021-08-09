using OpenTemple.Core;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Tests.TestUtils;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace OpenTemple.Tests.Game
{
    [NeedsRealGame]
    public class MainMenuTest : HeadlessGameTest
    {
        [Test]
        public void CanStartAndReachMainMenu()
        {
            UiSystems.MainMenu.Show(MainMenuPage.MainMenu);

            Game.RenderFrame();
            ImageComparison.AssertImagesEqual(Game.TakeScreenshot(), "Game/MainMenu.png");
        }
    }
}