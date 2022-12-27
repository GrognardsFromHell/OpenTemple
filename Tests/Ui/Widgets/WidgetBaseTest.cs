using System.Drawing;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetBaseTest
{
    private WidgetContainer _parent;
    private WidgetBase _child;
    private WidgetBase _childWithMargins;

    [SetUp]
    public void Setup()
    {
        _parent = new WidgetContainer
        {
            X = 100,
            Y = 101,
            Width = 33,
            Height = 67
        };
        _child = new WidgetBase
        {
            X = 11,
            Y = 7,
            Width = 9,
            Height = 13
        };
        _parent.Add(_child);

        _childWithMargins = new WidgetBase
        {
            X = 11,
            Y = 7,
            Width = 9,
            Height = 13
        };
        _childWithMargins.LocalStyles.SetMargins(1, 2, 3, 4);
        _parent.Add(_childWithMargins);
    }

    [Test]
    public void ParentContentAreas()
    {
        // Parent has no margins and content area is equal in both cases
        _parent.GetContentArea(false).Should().BeEquivalentTo(new Rectangle(100, 101, 33, 67));
        _parent.GetContentArea(true).Should().BeEquivalentTo(new Rectangle(100, 101, 33, 67));
    }

    [Test]
    public void ChildContentAreas()
    {
        // Child has no margins and content area is equal in both cases
        _child.GetContentArea(false).Should().BeEquivalentTo(new Rectangle(111, 108, 9, 13));
        _child.GetContentArea(true).Should().BeEquivalentTo(new Rectangle(111, 108, 9, 13));
    }

    [Test]
    public void ChildContentAreasWithMargin()
    {
        _childWithMargins.GetContentArea(false).Should().BeEquivalentTo(new Rectangle(112, 110, 5, 7));
        _childWithMargins.GetContentArea(true).Should().BeEquivalentTo(new Rectangle(111, 108, 9, 13));
    }
}