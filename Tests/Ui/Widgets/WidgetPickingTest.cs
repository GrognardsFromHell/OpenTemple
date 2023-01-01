using System.Drawing;
using NUnit.Framework;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetPickingTest : BaseWidgetTest
{
    private WidgetContainer _widget;
    private WidgetBase _childWidget;

    [SetUp]
    public void Setup()
    {
        _widget = new WidgetContainer();
        _widget.HitTesting = HitTestingMode.Area;
        _widget.SetLayout(RectangleF.FromLTRB(
            50,
            150,
            80,
            200
        ));
        // Border insets the padding area and will shift the relative offset for children
        _widget.LocalStyles.BorderWidth = 1;

        _childWidget = new WidgetBase();
        _childWidget.SetLayout(RectangleF.FromLTRB(
            10,
            20,
            11,
            22
        ));
        _widget.Add(_childWidget);
    }

    [TestCase(-0.1f, -0.1f, ExpectedResult = false, TestName = "Just outside top-left corner")]
    [TestCase(30f, -0.1f, ExpectedResult = false, TestName = "Just outside top-right corner")]
    [TestCase(30f, 50f, ExpectedResult = false, TestName = "Just outside bottom-right corner")]
    [TestCase(-0.1f, 50f, ExpectedResult = false, TestName = "Just outside bottom-left corner")]
    [TestCase(0f, 0f, ExpectedResult = true, TestName = "Just inside top-left corner")]
    [TestCase(29.9f, 0f, ExpectedResult = true, TestName = "Just inside top-right corner")]
    [TestCase(29.9f, 49.9f, ExpectedResult = true, TestName = "Just inside bottom-right corner")]
    [TestCase(0f, 49.9f, ExpectedResult = true, TestName = "Just inside bottom-left corner")]
    public bool PickParent(float relX, float relY)
    {
        var globalPick = _widget.PickWidgetGlobal(relX + 50, relY + 150);
        var localPick = _widget.PickWidget(relX, relY);
        globalPick.Should().BeSameAs(localPick);
        return globalPick == _widget;
    }

    [TestCase(-0.1f, -0.1f, ExpectedResult = false, TestName = "Just outside top-left corner (Child)")]
    [TestCase(1f, -0.1f, ExpectedResult = false, TestName = "Just outside top-right corner (Child)")]
    [TestCase(1f, 2f, ExpectedResult = false, TestName = "Just outside bottom-right corner (Child)")]
    [TestCase(-0.1f, 2f, ExpectedResult = false, TestName = "Just outside bottom-left corner (Child)")]
    [TestCase(0f, 0f, ExpectedResult = true, TestName = "Just inside top-left corner (Child)")]
    [TestCase(0.9f, 0f, ExpectedResult = true, TestName = "Just inside top-right corner (Child)")]
    [TestCase(0.9f, 1.9f, ExpectedResult = true, TestName = "Just inside bottom-right corner (Child)")]
    [TestCase(0f, 1.9f, ExpectedResult = true, TestName = "Just inside bottom-left corner (Child)")]
    public bool PickChild(float relX, float relY)
    {
        // Note the offset due to the parents border
        var globalPick = _childWidget.PickWidgetGlobal(relX + 61, relY + 171);
        var localPick = _childWidget.PickWidget(relX, relY);
        globalPick.Should().BeSameAs(localPick);
        return globalPick == _childWidget;
    }
}