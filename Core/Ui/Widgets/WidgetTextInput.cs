using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.TextInput;

namespace OpenTemple.Core.Ui.Widgets
{
    public class WidgetTextInput : WidgetBase, ITextInputElement
    {
        private readonly TextBlock _textBlock = Tig.RenderingDevice.GetTextEngine().CreateTextBlock();

        private TextStyle _textStyle;

        private readonly TextInputHelper _textInput;

        public WidgetTextInput([CallerFilePath]
            string filePath = null,
            [CallerLineNumber]
            int lineNumber = -1) : base(filePath, lineNumber)
        {
            IsFocusable = true;
            _textInput = new TextInputHelper(this, false);
            new TextInputEventHandler(_textInput, this, _textBlock);

            _textStyle = new TextStyle()
            {
                foreground = Brush.Default,
                fontFace = "OFL Sorts Mill Goudy"
            };
            _textStyle.disableLigatures = true;
            _textBlock.DefaultStyle = _textStyle;

            SetSize(100, 20);
        }

        public override void Render()
        {
            base.Render();

            if (!Visible)
            {
                return;
            }

            UpdateTextBlock();

            var contentRect = GetContentArea();
            var outline = contentRect;
            outline.Inflate(1, 1);
            Tig.ShapeRenderer2d.DrawRectangleOutline(outline, new PackedLinearColorA(127, 127, 127, 255));

            if (IsFocused)
            {
                _textBlock.ShowCaret(_textInput.Caret);
            }
            else
            {
                _textBlock.HideCaret();
            }

            var clipRect = new RectangleF(contentRect.Left, contentRect.Top, contentRect.Width, contentRect.Height);
            _textBlock.Render(clipRect.X, clipRect.Y, clipRect);
        }

        private void UpdateTextBlock()
        {
            _textBlock.SetText(_textInput.Value);

            var (start, length) = _textInput.SelectionRange;
            if (length == 0)
            {
                _textBlock.ClearSelection();
            }
            else
            {
                _textBlock.SetSelection(start, length);
            }

            EnsureCaretVisibility();
        }

        private void EnsureCaretVisibility()
        {
            if (!IsFocused)
            {
                _textBlock.ScrollX = 0;
                return;
            }

            var caretRect = _textBlock.GetCaretRectangle(0, 0, _textInput.Caret);
            // Caret distance from the left or right edge of the input
            var rightDistance = caretRect.Right - _textBlock.ScrollX - Width;
            var leftDistance = -(caretRect.Left - _textBlock.ScrollX);
            if (rightDistance >= 0 && leftDistance <= 0)
            {
                // Caret is to the right
                _textBlock.ScrollX += rightDistance + Width / 3.0f;
            }
            else if (leftDistance > 0 && rightDistance <= 0)
            {
                // Caret is to the left
                _textBlock.ScrollX -= leftDistance + Width / 3.0f;
            }

            // Ensure the scrolling is capped
            var textWidth = MathF.Ceiling(_textBlock.BoundingBox.Width + caretRect.Width);
            var overhang = Math.Max(0, textWidth - Width);
            _textBlock.ScrollX = Math.Clamp(_textBlock.ScrollX, 0, overhang);
        }

        #region Delegate ITextInputElement to TextInputHelper
        public int SelectionStart { get => _textInput.SelectionStart; set => _textInput.SelectionStart = value; }
        public int SelectionEnd { get =>_textInput.SelectionEnd; set => _textInput.SelectionEnd = value; }
        public SelectionDirection SelectionDirection { get => _textInput.SelectionDirection; set => _textInput.SelectionDirection = value; }
        public void SetSelectionRange(int start, int end, SelectionDirection direction = default)
        {
            _textInput.SetSelectionRange(start, end, direction);
        }

        public void SetRangeText(string replacement, SelectionMode selectionMode = SelectionMode.Preserve)
        {
            _textInput.SetRangeText(replacement, selectionMode);
        }

        public void SetRangeText(string replacement, int start, int end, SelectionMode selectionMode = SelectionMode.Preserve)
        {
            _textInput.SetRangeText(replacement, start, end, selectionMode);
        }
        #endregion
    }
}