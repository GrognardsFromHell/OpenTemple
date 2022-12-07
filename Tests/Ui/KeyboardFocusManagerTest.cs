#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Tests.Ui;

[TestFixture]
public class KeyboardFocusManagerTest
{
    public class BaseTest : BaseWidgetTest
    {
        protected KeyboardFocusManager Manager;

        [SetUp]
        public void SetUp()
        {
            Manager = new KeyboardFocusManager(UiManager.TopLevelWidgets);
        }

        protected List<string?> GetFocusChain(bool backwards)
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

    public class NoWidgets : BaseTest
    {
        [Test]
        public void Forward()
        {
            Manager.MoveFocusByKeyboard(false);
            Manager.KeyboardFocus.Should().BeNull();
        }

        [Test]
        public void Backward()
        {
            Manager.MoveFocusByKeyboard(true);
            Manager.KeyboardFocus.Should().BeNull();
        }
    }

    public class NoFocusableWidgets : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            TreeNode("nf", focusMode: FocusMode.None);
        }

        [Test]
        public void Forward()
        {
            Manager.MoveFocusByKeyboard(false);
            Manager.KeyboardFocus.Should().BeNull();
        }

        [Test]
        public void Backwards()
        {
            Manager.MoveFocusByKeyboard(true);
            Manager.KeyboardFocus.Should().BeNull();
        }
    }

    [TestFixture]
    public class SkipOverUnfocusableElementAtStartAndEnd : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            TreeNode("frontInvisible", visible: false);
            TreeNode("frontDisabled", disabled: true);
            TreeNode("frontUnfocusable", focusMode: FocusMode.None);
            TreeNode("w");
            TreeNode("backInvisible", visible: false);
            TreeNode("backDisabled", disabled: true);
            TreeNode("backUnfocusable", focusMode: FocusMode.None);
        }

        [Test]
        public void Forward()
        {
            GetFocusChain(false).Should().Equal("w", null, "w");
        }


        [Test]
        public void Backward()
        {
            GetFocusChain(true).Should().Equal("w", null, "w");
        }
    }

    public class NoChildren : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            TreeNode("1");
            TreeNode("2");
            TreeNode("3");
        }

        [Test]
        public void Forward()
        {
            GetFocusChain(false).Should().Equal(
                "1", "2", "3", null, "1"
            );
        }

        [Test]
        public void Backward()
        {
            GetFocusChain(true).Should().Equal(
                "3", "2", "1", null, "3"
            );
        }
    }

    public class MultiLevelTree : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            TreeNode("1");
            TreeNode("2");
            TreeNode("2.1", "2");
            TreeNode("2.2", "2");
            TreeNode("2.2.1", "2.2");
            TreeNode("2.3", "2");
            TreeNode("3");
        }

        [Test]
        public void Forward()
        {
            GetFocusChain(false).Should().Equal(
                "1", "2", "2.1", "2.2", "2.2.1", "2.3", "3", null, "1"
            );
        }

        [Test]
        public void Backward()
        {
            GetFocusChain(true).Should().Equal(
                "3", "2.3", "2.2.1", "2.2", "2.1", "2", "1", null, "3"
            );
        }
    }
}