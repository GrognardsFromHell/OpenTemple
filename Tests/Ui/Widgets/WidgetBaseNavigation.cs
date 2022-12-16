using NUnit.Framework;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui.Widgets;

public class WidgetBaseNavigation : BaseWidgetTest
{
    [Test]
    public void TestFollowingWhereFollowingIsParentWithChild()
    {
        var root = new WidgetContainer();
        var w1 = new WidgetBase();
        root.Add(w1);
        var w2 = new WidgetContainer();
        root.Add(w2);
        var w2Child = new WidgetBase();
        w2.Add(w2Child);

        w1.Following().Should().BeSameAs(w2);
        w2.Following().Should().BeSameAs(w2Child);
    }
}