using NUnit.Framework;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetPickingTest
{
    private WidgetBase _widget;

    [SetUp]
    public void Setup()
    {
        _widget = new WidgetBase();
        _widget.X = 50;
        _widget.Width = Dimension.Pixels(30);
        _widget.Y = 150;
        _widget.Height = Dimension.Pixels(50);
    }

    [TestCase(-0.1f, -0.1f, ExpectedResult = false, TestName = "Just outside top-left corner")]
    [TestCase(30f, -0.1f, ExpectedResult = false, TestName = "Just outside top-right corner")]
    [TestCase(30f, 50f, ExpectedResult = false, TestName = "Just outside bottom-right corner")]
    [TestCase(-0.1f, 50f, ExpectedResult = false, TestName = "Just outside bottom-left corner")]
    [TestCase(0f, 0f, ExpectedResult = true, TestName = "Just inside top-left corner")]
    [TestCase(29.9f, 0f, ExpectedResult = true, TestName = "Just inside top-right corner")]
    [TestCase(29.9f, 49.9f, ExpectedResult = true, TestName = "Just inside bottom-right corner")]
    [TestCase(0f, 49.9f, ExpectedResult = true, TestName = "Just inside bottom-left corner")]
    public bool PickWithoutMargin(float relX, float relY)
    {
        var globalPick = _widget.PickWidgetGlobal(relX + _widget.X, relY + _widget.Y);
        var localPick = _widget.PickWidget(relX, relY);
        globalPick.Should().BeSameAs(localPick);
        return globalPick == _widget;
    }

    [TestCase(0.9f, 1.9f, ExpectedResult = false, TestName = "Just outside top-left corner (with margin)")]
    [TestCase(27f, 1.9f, ExpectedResult = false, TestName = "Just outside top-right corner (with margin)")]
    [TestCase(27f, 46f, ExpectedResult = false, TestName = "Just outside bottom-right corner (with margin)")]
    [TestCase(0.9f, 46f, ExpectedResult = false, TestName = "Just outside bottom-left corner (with margin)")]
    [TestCase(1.0f, 2.0f, ExpectedResult = true, TestName = "Just inside top-left corner (with margin)")]
    [TestCase(26.9f, 2.0f, ExpectedResult = true, TestName = "Just inside top-right corner (with margin)")]
    [TestCase(26.9f, 45.9f, ExpectedResult = true, TestName = "Just inside bottom-right corner (with margin)")]
    [TestCase(1.0f, 45.9f, ExpectedResult = true, TestName = "Just inside bottom-left corner (with margin)")]
    public bool PickWithMargin(float relX, float relY)
    {
        _widget.LocalStyles.SetMargins(1, 2, 3, 4);

        var globalPick = _widget.PickWidgetGlobal(relX + _widget.X, relY + _widget.Y);
        var localPick = _widget.PickWidget(relX, relY);
        globalPick.Should().BeSameAs(localPick);
        return globalPick == _widget;
    }
}