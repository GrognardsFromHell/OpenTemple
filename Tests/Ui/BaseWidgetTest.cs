
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Ui;

public abstract class BaseWidgetTest : RealGameFiles
{
    protected List<WidgetContainer> Widgets => UiManager.Root.Children.Cast<WidgetContainer>().ToList();
    protected Dictionary<string, WidgetContainer> WidgetsById;
    protected UiManager UiManager;

    [SetUp]
    public void SetUpWidgets()
    {
        Globals.UiStyles = new UiStyles(FS);
        UiManager = new UiManager(new HeadlessMainWindow(), FS);
        WidgetsById = new Dictionary<string, WidgetContainer>();
    }

    [TearDown]
    public void TearDown()
    {
        Globals.UiStyles = null;
    }

    protected WidgetBase TreeNode(string id, string? parent = null, bool visible = true,
        bool disabled = false, FocusMode focusMode = FocusMode.User)
    {
        var widget = new WidgetContainer(Rectangle.Empty)
        {
            Id = id,
            Visible = visible,
            Disabled = disabled,
            FocusMode = focusMode,
        };
        
        WidgetsById[id] = widget;
        if (parent == null)
        {
            UiManager.AddWindow(widget);
        }
        else
        {
            WidgetsById[parent].Add(widget);
        }

        return widget;
    }
    
    protected IEnumerable<string> EnumerateNavigator(WidgetBase startAt, Func<WidgetBase, WidgetBase> navigationFunction)
    {
        var visited = new HashSet<WidgetBase>();
        var current = startAt;
        do
        {
            if (!visited.Add(current))
            {
                Assert.Fail("Loop in navigation function @ " + current.Id);
            }
            yield return current.Id;
            current = navigationFunction(current);
        } while (current != null && startAt != current);
    }
}