using System;
using FluentAssertions;
using NUnit.Framework;

namespace OpenTemple.Tests.Ui;

public class WidgetNavigationTest : BaseWidgetTest
{
    [SetUp]
    public void SetUpTree()
    {
        TreeNode("1");
        TreeNode("2");
        TreeNode("2.1", "2");
        TreeNode("2.2", "2");
        TreeNode("2.2.1", "2.2");
        TreeNode("2.2.2", "2.2");
        TreeNode("2.3", "2");
        TreeNode("3");
    }

    [Test]
    public void TestNextSibling()
    {
        WidgetsById["1"].NextSibling.Should().HaveId("2");
        WidgetsById["2"].NextSibling.Should().HaveId("3");
        WidgetsById["3"].NextSibling.Should().BeNull();
        WidgetsById["2.1"].NextSibling.Should().HaveId("2.2");
        WidgetsById["2.2"].NextSibling.Should().HaveId("2.3");
        WidgetsById["2.2.1"].NextSibling.Should().HaveId("2.2.2");
        WidgetsById["2.2.2"].NextSibling.Should().BeNull();
        WidgetsById["2.3"].NextSibling.Should().BeNull();
    }

    [Test]
    public void TestPreviousSibling()
    {
        WidgetsById["1"].PreviousSibling.Should().BeNull();
        WidgetsById["2"].PreviousSibling.Should().HaveId("1");
        WidgetsById["3"].PreviousSibling.Should().HaveId("2");
        WidgetsById["2.1"].PreviousSibling.Should().BeNull();
        WidgetsById["2.2"].PreviousSibling.Should().HaveId("2.1");
        WidgetsById["2.2.1"].PreviousSibling.Should().BeNull();
        WidgetsById["2.2.2"].PreviousSibling.Should().HaveId("2.2.1");
        WidgetsById["2.3"].PreviousSibling.Should().HaveId("2.2");
    }
    
    [Test]
    public void TestFirstChild()
    {
        WidgetsById["1"].FirstChild.Should().BeNull();
        WidgetsById["2"].FirstChild.Should().HaveId("2.1");
        WidgetsById["2.1"].FirstChild.Should().BeNull();
        WidgetsById["2.2"].FirstChild.Should().HaveId("2.2.1");
        WidgetsById["2.2.1"].FirstChild.Should().BeNull();
        WidgetsById["2.2.2"].FirstChild.Should().BeNull();
        WidgetsById["2.3"].FirstChild.Should().BeNull();
        WidgetsById["3"].FirstChild.Should().BeNull();
    }

    [Test]
    public void TestLastChild()
    {
        WidgetsById["1"].LastChild.Should().BeNull();
        WidgetsById["2"].LastChild.Should().HaveId("2.3");
        WidgetsById["2.1"].LastChild.Should().BeNull();
        WidgetsById["2.2"].LastChild.Should().HaveId("2.2.2");
        WidgetsById["2.2.1"].LastChild.Should().BeNull();
        WidgetsById["2.2.2"].LastChild.Should().BeNull();
        WidgetsById["2.3"].LastChild.Should().BeNull();
        WidgetsById["3"].LastChild.Should().BeNull();
    }

    [Test]
    public void TestFollowingWithNoPredicate()
    {
        EnumerateNavigator(Widgets[0], w => w.Following())
            .Should()
            .BeEquivalentTo("1", "2", "2.1", "2.2", "2.2.1", "2.2.2", "2.3", "3");
    }
}