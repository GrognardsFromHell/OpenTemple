using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.GFX.TextRendering;

public class TextLayoutTest : RenderingTest
{
    [Test]
    public void TestOverallWidth()
    {
        using var layout = CreateHelloWorldLayout();
        // This value comes from the default font
        layout.OverallWidth.Should().BeApproximately(51.5f, 0.1f);

        // Changes when line-breaking is involved
        layout.LayoutWidth = 40;
        layout.OverallWidth.Should().BeApproximately(25.9f, 0.1f);
    }

    [Test]
    public void TestOverallHeight()
    {
        using var layout = CreateHelloWorldLayout();
        // This value comes from the default font
        layout.OverallHeight.Should().BeApproximately(11.5f, 0.1f);

        // Changes when line-breaking is involved
        layout.LayoutWidth = 40;
        layout.OverallHeight.Should().BeApproximately(23.0f, 0.1f);
    }

    [Test]
    public void TestIsTrimmed()
    {
        var paragraph = new Paragraph();
        paragraph.LocalStyles.TrimMode = TrimMode.Character;
        paragraph.AppendContent("Hello World");

        using var layout = Device.TextEngine.CreateTextLayout(paragraph, 200, 200);
        layout.IsTrimmed.Should().BeFalse();

        // Reduce space to force trimming
        layout.LayoutWidth = layout.OverallWidth - 30;
        layout.IsTrimmed.Should().BeTrue();
    }

    [Test]
    public void TestSetLayoutWidth()
    {
        var paragraph = new Paragraph();
        paragraph.AppendContent("Hello World");
        paragraph.LocalStyles.WordWrap = WordWrap.WholeWord;

        using var layout = Device.TextEngine.CreateTextLayout(paragraph, 100, 200);
        // We have enough space to fit both words in one line
        layout.LineCount.Should().Be(1);

        // Set layout width to less than the actual width to force wrapping
        layout.LayoutWidth = layout.OverallWidth - 10;
        layout.LineCount.Should().Be(2);
    }

    [Test]
    public void TestGetLayoutWidth()
    {
        using var layout = CreateHelloWorldLayout();
        layout.LayoutWidth.Should().Be(100);

        // Should change after setting it
        layout.LayoutWidth = 150;
        layout.LayoutWidth.Should().Be(150);
    }

    [Test]
    public void TestGetLayoutHeight()
    {
        using var layout = CreateHelloWorldLayout();
        layout.LayoutHeight.Should().Be(200);

        // Should change after setting it
        layout.LayoutHeight = 150;
        layout.LayoutHeight.Should().Be(150);
    }

    [Test]
    public void TestBoundingRectangle()
    {
        var paragraph = new Paragraph();
        paragraph.LocalStyles.ParagraphAlignment = ParagraphAlign.Center;
        paragraph.LocalStyles.TextAlignment = TextAlign.Center;
        paragraph.AppendContent("Hello World");
        using var layout = Device.TextEngine.CreateTextLayout(paragraph, 500, 500);

        // This reflects the centered position of the text in the layout rectangle.
        layout.BoundingRectangle.Left.Should().BeApproximately(224.2F, 0.1f);
        layout.BoundingRectangle.Top.Should().BeApproximately(244.2f, 0.1f);
        layout.BoundingRectangle.Width.Should().BeApproximately(51.5f, 0.1f);
        layout.BoundingRectangle.Height.Should().BeApproximately(11.5f, 0.1f);
    }

    [Test]
    public void TestHitTest()
    {
        using var layout = CreateHelloWorldLayout();
        layout.TryHitTest(-1, -1, out _, out _).Should().BeFalse();

        // Simple hit at the start
        int start, length;
        layout.TryHitTest(0, 5, out start, out length).Should().BeTrue();
        start.Should().Be(0);
        length.Should().Be(1);

        // Hit at the end
        layout.TryHitTest(layout.OverallWidth - 0.1f, 5, out start, out length).Should().BeTrue();
        // 10 is the index of the last char
        start.Should().Be(10);
        length.Should().Be(1);
    }

    private TextLayout CreateHelloWorldLayout()
    {
        var paragraph = new Paragraph();
        paragraph.AppendContent("Hello World");
        return Device.TextEngine.CreateTextLayout(paragraph, 100, 200);
    }
}