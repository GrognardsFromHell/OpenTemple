using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.Widgets;

/// <summary>
/// A widget that displays an editable line of text.
/// </summary>
public class TextFieldWidget : WidgetBase
{
    private readonly WidgetText _textLabel = new();
    private readonly WidgetText _placeholderLabel = new();
    private readonly StringBuilder _buffer = new();
    private string _placeholder = "";

    /// <summary>
    /// The current text value of this text field.
    /// </summary>
    public string Text
    {
        get => _buffer.ToString();
        set
        {
            _buffer.Clear().Append(value);
            _textLabel.Text = _buffer.ToString();
        }
    }

    /// <summary>
    /// Placeholder text to render when the text field has no <see cref="Text"/> and is not focused.
    /// </summary>
    public string Placeholder
    {
        get => _placeholder;
        set
        {
            _placeholder = value;
            _placeholderLabel.Text = value;
        }
    }

    public TextFieldWidget([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber =
        -1) :
        base(filePath, lineNumber)
    {
        FocusMode = FocusMode.User;
    }

    protected override void DefaultTextInputAction(TextInputEvent e)
    {
        base.DefaultTextInputAction(e);
    }

    protected override void DefaultKeyDownAction(KeyboardEvent e)
    {
        base.DefaultKeyDownAction(e);
    }

    protected override void DefaultKeyUpAction(KeyboardEvent e)
    {
        base.DefaultKeyUpAction(e);
    }

    public override void Render()
    {
        base.Render();
        
        // Focus outline
        if (HasFocus)
        {
            var contentRect = GetContentArea();
            Tig.ShapeRenderer2d.DrawRectangleOutline(contentRect, PackedLinearColorA.White);
        }
    }
}