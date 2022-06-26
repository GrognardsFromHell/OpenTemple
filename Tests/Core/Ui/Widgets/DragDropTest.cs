using System.Drawing;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.Ui.Widgets;

public class DragDropTest
{
    private HeadlessMainWindow _mainWindow;
    private UiManager _uiManager;
    private WidgetBase _drag;
    private WidgetBase _drop;

    [SetUp]
    public void SetupMockUi()
    {
        _mainWindow = new HeadlessMainWindow();
        _uiManager = new UiManager(_mainWindow);
        Globals.UiManager = _uiManager;

        _drag = new WidgetBase();
        _drop = new WidgetBase();
        _drag.Rectangle = new Rectangle(0, 0, 10, 10);
        _drop.Rectangle = new Rectangle(100, 0, 10, 10);

        Tig.Mouse = new TigMouse();
    }

    [TearDown]
    public void TearDownMockUi()
    {
        Globals.UiManager = null;
        Tig.Mouse = null;
    }
    
    [Test]
    public void TestDragDrop()
    {
        _mainWindow.SendMouseEvent(WindowEventType.MouseDown, Point.Empty, MouseButton.LEFT);
    }
}
