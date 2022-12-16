using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetContainerTest
{
    private WidgetContainer _container;
    private WidgetBase _a;
    private WidgetBase _b;

    [SetUp]
    public void Setup()
    {
        _container = new WidgetContainer();
        _a = new WidgetBase();
        _a.ZIndex = 1000;
        _container.Add(_a);
        _b = new WidgetBase();
        _container.Add(_b);
    }
    
    [Test]
    public void InsertionAndZIndexOrderAreSeparate()
    {
        _container.Children.Should().Equal(_a, _b);
        _container.ZOrderChildren.Should().Equal(_b, _a);
    }
    
    [Test]
    public void ChildrenAreRemovedOnDispose()
    {
        _a.Dispose();
        _a.Parent.Should().BeNull();
        _container.Children.Should().Equal(_b);
        _container.ZOrderChildren.Should().Equal(_b);
    }
    
    [Test]
    public void ChildrenAreRemovedProperly()
    {
        _container.Remove(_a);
        _a.Parent.Should().BeNull();
        _container.Children.Should().Equal(_b);
        _container.ZOrderChildren.Should().Equal(_b);
    }
    
    [Test]
    public void SendToBack()
    {
        _container.SendToBack(_a);
        _container.Children.Should().Equal(_a, _b);
        _container.ZOrderChildren.Should().Equal(_a, _b);
    }
    
    [Test]
    public void SendToFront()
    {
        _container.BringToFront(_b);
        _container.Children.Should().Equal(_a, _b);
        _container.ZOrderChildren.Should().Equal(_a, _b);
    }
    
    [Test]
    public void SendToFrontIsIdempotent()
    {
        var originalZIndexA = _a.ZIndex;
        var originalZIndexB = _b.ZIndex;
        
        _container.BringToFront(_a);
        _container.Children.Should().Equal(_a, _b);
        _container.ZOrderChildren.Should().Equal(_b, _a);
        _a.ZIndex.Should().Be(originalZIndexA);
        _b.ZIndex.Should().Be(originalZIndexB);
    }
}