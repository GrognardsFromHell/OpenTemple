using System.Numerics;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Game;

public class GameViewTest : HeadlessGameTest
{
    [SetUp]
    public void LoadIntoTutorial()
    {
        GameSystems.GameInit.OpenStartMap(true);
        UiSystems.MainMenu.LaunchTutorial();
        Game.RunFor(5000);

        UiSystems.Alert.Hide();
    }

    [Test]
    public void TestPrimaryView()
    {
        GameViews.Primary.Should().NotBeNull();
        GameViews.Primary.Should().Be(UiSystems.GameView);
    }

    [Test]
    public void TestMouseWheelZoom()
    {
        TakeScreenshot("before_zoom");

        IGameViewport gv = UiSystems.GameView;

        gv.Zoom.Should().Be(1.0f);
        var initialCenter = gv.CenteredOn.ToInches2D(); // Remember where it's centered now

        // One "pip" of scroll-wheel -> 10% zoom
        var e = new WheelEvent {DeltaY = 1};
        UiSystems.GameView.DispatchMouseWheel(e);
        TakeScreenshot("after_zoom1");
        gv.Zoom.Should().BeApproximately(1.1f, 0.001f);

        UiSystems.GameView.DispatchMouseWheel(e);
        TakeScreenshot("after_zoom2");
        gv.Zoom.Should().BeApproximately(1.2f, 0.001f);

        // Check that the camera stays centered on the original center
        var centerAfterZoom = gv.CenteredOn.ToInches2D();
        var distanceFromInitialCenter = Vector2.Subtract(initialCenter, centerAfterZoom).Length();
        distanceFromInitialCenter.Should().BeLessOrEqualTo(3);

        // Initiate and immediately release MMB scrolling because previously the state would get de-synched
        // and cause the view to jerk around
        UiSystems.GameView.DispatchMouseDown(new MouseEvent {Button = MouseButton.MIDDLE});
        UiSystems.GameView.DispatchMouseMove(new MouseEvent {X = 1});
        UiSystems.GameView.DispatchMouseMove(new MouseEvent {X = -1});
        UiSystems.GameView.DispatchMouseUp(new MouseEvent {Button = MouseButton.MIDDLE});
        var centerAfterMove = gv.CenteredOn.ToInches2D();
        Vector2.Subtract(centerAfterZoom, centerAfterMove).Length().Should().BeLessOrEqualTo(3);
    }
}