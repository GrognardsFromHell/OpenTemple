using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Widgets.TextField;

namespace OpenTemple.Tests.Ui.Widgets.Text;

public class TextFieldWidgetTest
{
    [Test]
    public void TextInsertionWorks()
    {
        var widget = new TextFieldWidget();
        var e = new TextInputEvent
        {
            Text = "A"
        };
        widget.DispatchTextInput(e);
        widget.Text.Should().Be("A");
    }

    [Test]
    public void LargeTextInsertionRespectMaxLength()
    {
        var widget = new TextFieldWidget();
        var e = new TextInputEvent
        {
            Text = new string('X', widget.MaxLength + 1)
        };
        widget.DispatchTextInput(e);
        widget.Text.Length.Should().Be(widget.MaxLength);
    }

    [Test]
    public void ManySmallInsertionsRespectMaxLength()
    {
        var widget = new TextFieldWidget();
        for (var i = 0; i < 1001; i++)
        {
            var e = new TextInputEvent
            {
                Text = "X"
            };
            widget.DispatchTextInput(e);
        }
        widget.Text.Length.Should().Be(widget.MaxLength);
    }
}