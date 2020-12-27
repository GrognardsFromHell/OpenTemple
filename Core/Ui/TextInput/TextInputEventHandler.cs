using System;
using System.Drawing;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.TextInput
{
    public class TextInputEventHandler
    {
        private enum DragMode
        {
            None,
            MovingCaret,
            Selecting
        }

        private readonly Element _host;

        private readonly TextInputHelper _textInput;

        private readonly TextBlock _textBlock;

        private DragMode _dragMode;

        public TextInputEventHandler(TextInputHelper textInput, Element host, TextBlock textBlock)
        {
            _textInput = textInput;
            _host = host;
            _textBlock = textBlock;

            _host.AddEventListener<KeyboardEvent>(SystemEventType.KeyDown, HandleShortcut);
            _host.AddEventListener<MouseEvent>(SystemEventType.MouseDown, HandleMouseDown);
            _host.AddEventListener<MouseEvent>(SystemEventType.MouseMove, HandleMouseMove);
            _host.AddEventListener<MouseEvent>(SystemEventType.MouseUp, HandleMouseUp);
        }

        private void HandleMouseDown(MouseEvent evt)
        {
            if (evt.Button == 0)
            {
                _dragMode = evt.ShiftKey ? DragMode.Selecting : DragMode.MovingCaret;
                _host.SetPointerCapture();

                HandleMouseMove(evt);
            }
        }

        private void HandleMouseMove(MouseEvent evt)
        {
            if (_dragMode == DragMode.None || !_host.HasPointerCapture())
            {
                return;
            }

            var rect = _host.GetContentArea();
            var x = evt.ClientX - rect.X;
            var y = evt.ClientY - rect.Y;
            _textBlock.HitTest(x, y, out var position);
            _textInput.MoveCaret(position, _dragMode == DragMode.Selecting);
        }

        private void HandleMouseUp(MouseEvent evt)
        {
            _dragMode = DragMode.None;
            if (evt.Button == 0)
            {
                _host.ReleasePointerCapture();
            }
        }

        private void HandleShortcut(KeyboardEvent evt)
        {
            if (EditCommandHandler.TryGetEditCommand(evt, out var command))
            {
                _textInput.ExecuteCommand(command);
            }
        }

    }
}