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
    private bool _needsTextUpdate;
    private bool _needsCaretUpdate = true;
    private TextLayout _textLayout;
    private RectangleF _caretRect;

    private RectangleF[] _selectionRects;

    // Rectangle containing the text input area inside of this widget
    private RectangleF _innerTextAreaRect;

    // By how much is the text layout shifted to the left
    private float _lineXShift;

    private readonly Caret _caret;

    private PackedLinearColorA _selectionColor = new (255, 255, 255, 64);

    private PackedLinearColorA _caretColor = PackedLinearColorA.White;

    private bool _undecorated;

    public bool Undecorated
    {
        get => _undecorated;
        set
        {
            _undecorated = value;
            InvalidateCaret();
        }
    }

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
        get => _placeholderLabel.Text;
        set => _placeholderLabel.Text = value;
    }

    public TextFieldWidget([CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber =
        -1) :
        base(filePath, lineNumber)
    {
        FocusMode = FocusMode.User;
        _caret = new Caret(_buffer);
        _caret.OnChangeText += InvalidateText;
        _caret.OnChangeCaret += InvalidateCaret;
        LocalStyles.WordWrap = WordWrap.NoWrap;

        _placeholderLabel.LocalStyles.Color = PackedLinearColorA.FromHex("#666666");
        _textLayout = Tig.RenderingDevice.TextEngine.CreateTextLayout(ComputedStyles, "", float.MaxValue, float.MaxValue);
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

    public override void Render(UiRenderContext context)
    {
        base.Render(context);
        if (!Visible)
        {
            return;
        }

        // Draw border
        if (!_undecorated)
        {
            var decorationRect = GetContentArea();
            PackedLinearColorA outlineColor;
            if (HasFocus || ContainsMouse)
            {
                outlineColor = PackedLinearColorA.White;
            }
            else
            {
                outlineColor = PackedLinearColorA.FromHex("#1AC3FF");
            }

            // Draw the background first
            Tig.ShapeRenderer2d.DrawRectangle(decorationRect, PackedLinearColorA.FromHex("#000000"));

            // Then the borders around it
            Tig.ShapeRenderer2d.DrawRectangleOutline(decorationRect, outlineColor);
            decorationRect.Inflate(-1, -1);
            Tig.ShapeRenderer2d.DrawRectangleOutline(decorationRect, PackedLinearColorA.FromHex("#000000"));
            decorationRect.Inflate(-1, -1);
            Tig.ShapeRenderer2d.DrawRectangleOutline(decorationRect, PackedLinearColorA.FromHex("#5D5D5D"));
        }

        UpdateIfNeeded();

        var textAreaRect = _innerTextAreaRect;
        var contentArea = GetContentArea();
        textAreaRect.Offset(contentArea.Location);

        // We use the full content rectangle vertically because the baseline / caret doesn't include the descend below the baseline
        context.PushScissorRect(textAreaRect);

        if (!HasFocus && _buffer.Length == 0)
        {
            _placeholderLabel.SetBounds(textAreaRect);
            _placeholderLabel.Render();
        }
        else
        {
            RenderTextLine(textAreaRect);
        }

        context.PopScissorRect();
    }

    private void RenderTextLine(RectangleF textAreaRect)
    {
        var textLayoutLeft = textAreaRect.X - _lineXShift;
        
        Tig.RenderingDevice.TextEngine.RenderTextLayout(textLayoutLeft, textAreaRect.Y, _textLayout);

        // Caret & Selection
        if (HasFocus)
        {
            // Selection
            for (var index = 0; index < _selectionRects.Length; index++)
            {
                var selectionRect = _selectionRects[index];
                selectionRect.Offset(textLayoutLeft, textAreaRect.Y);
                UiManager?.SnapToPhysicalPixelGrid(ref selectionRect);

                Tig.ShapeRenderer2d.DrawRectangle(
                    selectionRect,
                    null,
                    _selectionColor
                );
            }

            var caretRect = _caretRect;
            caretRect.Offset(textLayoutLeft, textAreaRect.Y);

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
            UpdateIfNeeded();

            var pos = e.GetLocalPos(this);
            TransformToTextArea(ref pos);
            pos.X += _lineXShift;
            if (pos.X < 0)
            {
                _caret.MoveBackwards(CaretMove.All, e.IsShiftHeld);
            }
            else if (pos.X >= _textLayout!.OverallWidth)
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
            UpdateIfNeeded();

            var pos = e.GetLocalPos(this);
            TransformToTextArea(ref pos);
            pos.X += _lineXShift;

            var y = Math.Clamp(pos.Y, 0, _caretRect.Height);

            if (pos.X < 0)
            {
                _caret.MoveBackwards(CaretMove.All, true);
            }
            else if (pos.X >= _textLayout!.OverallWidth)
            {
                _caret.MoveForward(CaretMove.All, true);
            }
            else if (_textLayout.TryHitTest(pos.X, y, out var position, out _))
            {
                _caret.Set(position, true);
            }
        }
    }

    protected override void DefaultDoubleClickAction(MouseEvent e)
    {
        _caret.SelectAll();
    }

    private void TransformToTextArea(ref PointF pos)
    {
        pos.X -= _innerTextAreaRect.X;
        pos.Y -= _innerTextAreaRect.Y;
    }

    private void UpdateIfNeeded()
    {
        if (_needsTextUpdate)
        {
            var style = ComputedStyles;
            _textLayout.Dispose();
            _textLayout = Tig.RenderingDevice.TextEngine.CreateTextLayout(style, _buffer.ToString(), float.MaxValue, float.MaxValue);
            _needsTextUpdate = false;
            _needsCaretUpdate = true;
        }

        if (!_needsCaretUpdate)
        {
            return;
        }
        
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

        // Update content rectangle
        _innerTextAreaRect = ContentArea;
        if (!_undecorated)
        {
            _innerTextAreaRect.Inflate(-4, -4);
        }

        // How much of the total text width is overflowing the available bounds
        var overflowingWidth = Math.Max(0, _textLayout.OverallWidth - _innerTextAreaRect.Width + _caretRect.Width);
        // Clamp the current shift, but account for the visibility of the caret rect
        _lineXShift = Math.Clamp(_lineXShift, 0, overflowingWidth);
        
        // Check if the caret - accounting for current shift - 
        // is to the left or right of the visible area
        var caretInViewport = _caretRect.X - _lineXShift;
        if (caretInViewport < 0)
        {
            // The caret is at least partially to the left of the viewport
            // Shift the viewport such that the caret is roughly
            // viewportWidth/4 away from the left of the viewport
            _lineXShift -= - caretInViewport + _innerTextAreaRect.Width / 4f;
        }
        else if (caretInViewport + _caretRect.Width >= _innerTextAreaRect.Width)
        {
            // The caret is at least partially to the right of the viewport
            // Shift the viewport such that the caret is roughly
            // viewportWidth/4 away from the right of the viewport
            var distFromRightEdge = caretInViewport - (_innerTextAreaRect.Width - _caretRect.Width);
            _lineXShift += distFromRightEdge + _innerTextAreaRect.Width / 4f;
        }
        
        // Clamp the shift again after potentially adjusting it
        _lineXShift = Math.Clamp(_lineXShift, 0, overflowingWidth);

        // Vertically center the text line
        var lineYOffset = (int) MathF.Floor((_innerTextAreaRect.Height - _caretRect.Height) / 2);
        _innerTextAreaRect.Inflate(0, - lineYOffset);

        _needsCaretUpdate = false;
    }

    private void InvalidateCaret()
    {
        _needsCaretUpdate = true;
    }
    
    private void InvalidateText()
    {
        _needsTextUpdate = true;
    }
}