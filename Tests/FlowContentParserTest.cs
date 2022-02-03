using FluentAssertions;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class FlowContentParserTest
{
    [Test]
    public void CanParseSequenceWithColorSwitches()
    {
        var text = FlowContentParser.ParseLegacyText("text@1other@tcolor@0and back",
            new[] {PackedLinearColorA.Black});

        var complexElement = text.Should().BeOfType<ComplexInlineElement>().Which;
        complexElement.Children.Should().SatisfyRespectively(
            first =>
            {
                var inline = first.Should().BeOfType<SimpleInlineElement>().Which;
                inline.Text.Should().Be("text");
                inline.LocalStyles.Color.Should().BeNull();
            },
            second =>
            {
                var inline = second.Should().BeOfType<SimpleInlineElement>().Which;
                inline.Text.Should().Be("other\tcolor");
                inline.LocalStyles.Should().BeEquivalentTo(new StyleDefinition()
                {
                    Color = PackedLinearColorA.Black
                });
            },
            third =>
            {
                var inline = third.Should().BeOfType<SimpleInlineElement>().Which;
                inline.Text.Should().Be("and back");
                inline.LocalStyles.Color.Should().BeNull();
            });
    }
}