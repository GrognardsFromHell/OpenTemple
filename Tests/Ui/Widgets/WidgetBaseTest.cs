using System.Drawing;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetBaseTest : BaseWidgetTest 
{
    private WidgetContainer _parent;
    private WidgetBase _child;
    private WidgetBase _childWithMargins;
    private WidgetBase _childWithBorder;
    private WidgetBase _childWithPadding;
    private WidgetBase _childWithBorderAndPadding;

    [SetUp]
    public void Setup()
    {
        _parent = new WidgetContainer
        {
            X = 100,
            Y = 101,
            PixelSize = new SizeF(33, 67)
        };
        UiManager.AddWindow(_parent);
        _child = new WidgetBase
        {
            X = 11,
            Y = 7,
            PixelSize = new SizeF(9, 13)
        };
        _parent.Add(_child);

        _childWithMargins = new WidgetBase
        {
            X = 11,
            Y = 7,
            PixelSize = new SizeF(9, 13)
        };
        _childWithMargins.LocalStyles.SetMargins(1, 2, 3, 4);
        _parent.Add(_childWithMargins);

        _childWithBorder = new WidgetBase
        {
            X = 11,
            Y = 7,
            PixelSize = new SizeF(9, 13)
        };
        _childWithBorder.LocalStyles.BorderWidth = 1;
        _parent.Add(_childWithBorder);

        _childWithPadding = new WidgetBase
        {
            X = 11,
            Y = 7,
            PixelSize = new SizeF(9, 13)
        };
        _childWithPadding.LocalStyles.SetPadding(1, 2, 3, 4);
        _parent.Add(_childWithPadding);

        _childWithBorderAndPadding = new WidgetBase
        {
            X = 11,
            Y = 7,
            PixelSize = new SizeF(9, 13)
        };
        _childWithBorderAndPadding.LocalStyles.SetPadding(1, 2, 3, 4);
        _childWithBorderAndPadding.LocalStyles.BorderWidth = 1;
        _parent.Add(_childWithBorderAndPadding);
    }

    [Test]
    public void ParentContentAreas()
    {
        // Parent has no margins and content area is equal in both cases
        _parent.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(100, 101, 33, 67));
        _parent.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(100, 101, 33, 67));
        _parent.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(100, 101, 33, 67));
    }

    [Test]
    public void ChildContentAreas()
    {
        // Child has no margins and content area is equal in both cases
        _child.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _child.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _child.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
    }

    [Test]
    public void ChildContentAreasWithMargin()
    {
        // Note that margins currently don't really matter in normal layout
        _childWithMargins.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithMargins.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithMargins.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
    }

    [Test]
    public void ChildContentAreasWithBorder()
    {
        _childWithBorder.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithBorder.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(112, 109, 7, 11));
        _childWithBorder.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(112, 109, 7, 11));
    }

    [Test]
    public void ChildContentAreasWithPadding()
    {
        _childWithPadding.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithPadding.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithPadding.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(112, 110, 5, 7));
    }

    [Test]
    public void ChildContentAreasWithBorderAndPadding()
    {
        _childWithPadding.GetViewportBorderArea().Should().BeEquivalentTo(new RectangleF(111, 108, 9, 13));
        _childWithBorderAndPadding.GetViewportPaddingArea().Should().BeEquivalentTo(new RectangleF(112, 109, 7, 11));
        _childWithBorderAndPadding.GetViewportContentArea().Should().BeEquivalentTo(new RectangleF(113, 111, 3, 5));
    }
}