using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.Styles;
using SDL2;

namespace OpenTemple.Core.Ui.Widgets.TextField;

/// <summary>
/// A widget that displays an editable line of text.
/// </summary>
public class TextFieldWidget : WidgetBase
{
    private readonly WidgetText _placeholderLabel = new();
    private readonly StringBuilder _buffer = new();
    private bool _needsUpdate = true;
    private TextLayout? _textLayout;
    private RectangleF _caretRect;

    private RectangleF[] _selectionRects;

    // Inner Y position of the text line
    private int _lineYOffset;

    // By how much is the text layout shifted to the left
    private int _lineXShift;

    // Index in _buffer. Can be 0 to the size of the buffer (inclusive)
    private string _placeholder = "";
    private bool _undecorated;

    private readonly Caret _caret;

    private PackedLinearColorA _selectionColor = new PackedLinearColorA(255, 255, 255, 64);

    private PackedLinearColorA _caretColor = PackedLinearColorA.White;

    public bool Undecorated { get; set; }

    public int MaxLength { get; set; } = 100;

    public int CurrentLength => _buffer.Length;

    /// <summary>
    /// The current text value of this text field.
    /// </summary>
    public string Text
    {
        get => _buffer.ToString();
        set
        {
            _caret.SelectAll();
            ReplaceSelection(value);
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
        _caret = new Caret(_buffer);
        _caret.OnChange += Invalidate;
        LocalStyles.WordWrap = WordWrap.NoWrap;
    }

    protected override void DefaultTextInputAction(TextInputEvent e)
    {
        ReplaceSelection(e.Text);
    }

    protected override void DefaultKeyDownAction(KeyboardEvent e)
    {
        switch (e.VirtualKey)
        {
            case SDL.SDL_Keycode.SDLK_CUT:
            case SDL.SDL_Keycode.SDLK_x when e.IsCtrlHeld:
                if (_caret.SelectionLength > 0)
                {
                    Clipboard.SetText(_caret.SelectedText);
                    _caret.DeleteSelection();
                }

                break;
            case SDL.SDL_Keycode.SDLK_v when e.IsCtrlHeld:
                if (Clipboard.HasText)
                {
                    ReplaceSelection(Clipboard.GetText());
                }

                break;
            case SDL.SDL_Keycode.SDLK_c when e.IsCtrlHeld:
                if (_caret.SelectionLength > 0)
                {
                    Clipboard.SetText(_caret.SelectedText);
                }

                break;
            case SDL.SDL_Keycode.SDLK_a when e.IsCtrlHeld:
                _caret.SelectAll();
                break;
            case SDL.SDL_Keycode.SDLK_BACKSPACE:
            case SDL.SDL_Keycode.SDLK_KP_BACKSPACE:
                _caret.DeleteBackwards(e.IsCtrlHeld ? CaretMove.Word : CaretMove.Character);
                break;
            case SDL.SDL_Keycode.SDLK_DELETE:
                _caret.DeleteForward(e.IsCtrlHeld ? CaretMove.Word : CaretMove.Character);
                break;
            case SDL.SDL_Keycode.SDLK_LEFT:
                _caret.MoveBackwards(
                    e.IsCtrlHeld ? CaretMove.Word : CaretMove.Character,
                    e.IsShiftHeld
                );
                break;
            case SDL.SDL_Keycode.SDLK_RIGHT:
                _caret.MoveForward(
                    e.IsCtrlHeld ? CaretMove.Word : CaretMove.Character,
                    e.IsShiftHeld
                );
                break;
            case SDL.SDL_Keycode.SDLK_HOME:
                _caret.MoveBackwards(
                    CaretMove.All,
                    e.IsShiftHeld
                );
                break;
            case SDL.SDL_Keycode.SDLK_END:
                _caret.MoveForward(
                    CaretMove.All,
                    e.IsShiftHeld
                );
                break;
        }
    }

    // This is the primary way to actually insert text
    private void ReplaceSelection(ReadOnlySpan<char> text)
    {
        // We're adding selection length since it will be replaced
        var charsRemaining = Math.Max(0, MaxLength - CurrentLength + _caret.SelectionLength);

        Span<char> sanitized = stackalloc char[Math.Min(charsRemaining, text.Length)];
        var sanitizedLength = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch is '\r' or '\n')
            {
                break; // Do not accept line-feeds and skip the subsequent lines
            }

            // Handle UTF-16 surrogates
            bool keepTogetherWithNextChar = false;
            if (char.IsHighSurrogate(ch))
            {
                if (i + 1 >= text.Length)
                {
                    continue; // A high surrogate at the end of the string will just be ignored
                }

                var nextChar = text[i + 1];
                if (!char.IsLowSurrogate(nextChar))
                {
                    continue; // A high surrogate followed by something OTHER than a low surrogate will also be ignored
                }

                // Otherwise ensure high+low surrogate stay together
                if (sanitizedLength + 2 > sanitized.Length)
                {
                    break; // Exceeds max length
                }

                sanitized[sanitizedLength++] = ch;
                sanitized[sanitizedLength++] = nextChar;
            }
            else
            {
                if (sanitizedLength + 1 > sanitized.Length)
                {
                    break; // Exceeds max length
                }

                sanitized[sanitizedLength++] = ch;
            }
        }

        _caret.Replace(sanitized[..sanitizedLength]);
    }


    public override void Render()
    {
        base.Render();

        var contentRect = GetContentArea();

        Tig.ShapeRenderer2d.DrawRectangle(contentRect, null, PackedLinearColorA.FromHex("#333333"));

        // Focus outline
        if (HasFocus)
        {
            Tig.ShapeRenderer2d.DrawRectangleOutline(contentRect, PackedLinearColorA.White);
        }

        EnsureUpdated();

        var lineY = contentRect.Y + _lineYOffset;

        Tig.RenderingDevice.TextEngine.RenderTextLayout(contentRect.X, lineY, _textLayout);

        // Caret & Selection
        if (HasFocus)
        {
            // Selection
            for (var index = 0; index < _selectionRects.Length; index++)
            {
                var selectionRect = _selectionRects[index];
                selectionRect.Offset(contentRect.X, lineY);
                UiManager?.SnapToPhysicalPixelGrid(ref selectionRect);

                Tig.ShapeRenderer2d.DrawRectangle(
                    selectionRect,
                    null,
                    _selectionColor
                );
            }

            var caretRect = _caretRect;
            caretRect.Offset(contentRect.X, lineY);

            UiManager?.SnapToPhysicalPixelGrid(ref caretRect);

            Tig.ShapeRenderer2d.DrawRectangle(
                caretRect,
                null,
                _caretColor
            );
        }
    }

    protected override void HandleMouseDown(MouseEvent e)
    {
        if (e.Button == MouseButton.Left)
        {
            var pos = e.GetLocalPos(this);
            pos.X += _lineXShift;
            pos.Y -= _lineYOffset;
            EnsureUpdated();
            if (pos.X < 0)
            {
                _caret.MoveBackwards(CaretMove.All, e.IsShiftHeld);
            }
            else if (pos.X >= _textLayout.OverallWidth)
            {
                _caret.MoveForward(CaretMove.All, e.IsShiftHeld);
            }
            else if (_textLayout.TryHitTest(pos.X, pos.Y, out var position, out _))
            {
                _caret.Set(position, e.IsShiftHeld);
            }

            SetMouseCapture();
        }
    }

    protected override void HandleMouseMove(MouseEvent e)
    {
        if (HasMouseCapture)
        {
            var pos = e.GetLocalPos(this);
            pos.X += _lineXShift;
            pos.Y -= _lineYOffset;
            EnsureUpdated();
            if (pos.X < 0)
            {
                _caret.MoveBackwards(CaretMove.All, true);
            }
            else if (pos.X >= _textLayout.OverallWidth)
            {
                _caret.MoveForward(CaretMove.All, true);
            }
            else if (_textLayout.TryHitTest(pos.X, pos.Y, out var position, out _))
            {
                _caret.Set(position, true);
            }
        }
    }

    private void EnsureUpdated()
    {
        if (!_needsUpdate)
        {
            return;
        }

        var style = ComputedStyles;
        _textLayout = Tig.RenderingDevice.TextEngine.CreateTextLayout(style, _buffer.ToString(), Width, Height);

        _caretRect = _textLayout.HitTestPosition(_caret.Position, true);
        _caretRect.Width = 1;

        if (_caret.SelectionLength > 0)
        {
            _selectionRects = _textLayout.HitTestRange(_caret.SelectionStartIndex, _caret.SelectionLength);
        }
        else
        {
            _selectionRects = Array.Empty<RectangleF>();
        }

        _lineYOffset = (int) MathF.Ceiling((Height - _caretRect.Height) / 2);

        _needsUpdate = false;
    }

    private void Invalidate()
    {
        _needsUpdate = true;
    }
}