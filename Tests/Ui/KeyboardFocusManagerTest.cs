#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui;

public class KeyboardFocusManagerTest : BaseWidgetTest
{
    public KeyboardFocusManager Manager;

    [SetUp]
    public void SetUp()
    {
        Manager = new KeyboardFocusManager(UiManager.TopLevelWidgets);
    }

    [Test]
    public void MoveFocusForwardWithNoWidgets()
    {
        Manager.MoveFocusByKeyboard(false);
        Manager.KeyboardFocus.Should().BeNull();
    }

    [Test]
    public void MoveFocusBackwardWithNoWidgets()
    {
        Manager.MoveFocusByKeyboard(true);
        Manager.KeyboardFocus.Should().BeNull();
    }

    [Test]
    public void MoveFocusWithNoFocusableWidgets()
    {
        TreeNode("nf", focusMode: FocusMode.None);

        Manager.MoveFocusByKeyboard(false);
        Manager.KeyboardFocus.Should().BeNull();
        Manager.MoveFocusByKeyboard(true);
        Manager.KeyboardFocus.Should().BeNull();
    }

    [Test]
    public void SkipOverUnfocusableElementAtStartAndEnd()
    {
        TreeNode("frontInvisible", visible: false);
        TreeNode("frontDisabled", disabled: true);
        TreeNode("frontUnfocusable", focusMode: FocusMode.None);
        TreeNode("w");
        TreeNode("backInvisible", visible: false);
        TreeNode("backDisabled", disabled: true);
        TreeNode("backUnfocusable", focusMode: FocusMode.None);

        GetFocusChain(false).Should().BeEquivalentTo("w", "w");
        GetFocusChain(true).Should().BeEquivalentTo("w", "w");
    }

    [Test]
    public void MoveFocusForwardWithoutChildren()
    {
        TreeNode("1");
        TreeNode("2");
        TreeNode("3");

        GetFocusChain(false).Should().BeEquivalentTo(
            "1", "2", "3", null, "1"
        );
    }

    [Test]
    public void MoveFocusBackwardsWithoutChildren()
    {
        TreeNode("1");
        TreeNode("2");
        TreeNode("3");

        GetFocusChain(false).Should().BeEquivalentTo(
            "3", "2", "1", "3"
        );
    }

    [Test]
    public void MoveFocusForwardThroughMultiLevelTree()
    {
        TreeNode("1");
        TreeNode("2");
        TreeNode("2.1", "2");
        TreeNode("2.2", "2");
        TreeNode("2.2.1", "2.1");
        TreeNode("2.3", "2");
        TreeNode("3");

        GetFocusChain(false).Should().ContainInOrder(
            "1", "2", "2.1", "2.2", "2.2.1", "2.3", "3", "1"
        );
    }

    private List<string?> GetFocusChain(bool backwards)
    {
        var focused = new List<string?>();

        Manager.MoveFocusByKeyboard(backwards);
        focused.Add(Manager.KeyboardFocus?.Id);
        do
        {
            Manager.MoveFocusByKeyboard(backwards);
            focused.Add(Manager.KeyboardFocus?.Id);
        } while (focused.Count < 100 && focused[0] != focused[^1]);

        return focused;
    }
}