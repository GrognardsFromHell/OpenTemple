using OpenTemple.Core.Ui.DOM;

namespace OpenTemple.Core.Ui.TextInput
{
    public static class EditCommandHandler
    {
        public static bool TryGetEditCommand(KeyboardEvent evt, out EditCommand command)
        {
            var key = evt.Key;
            var modifiers = evt.ActiveModifiers;
            var selecting = (modifiers & KeyboardModifier.Shift) != 0;
            if (selecting)
            {
                modifiers &= ~KeyboardModifier.Shift;
            }

            if (key == KeyboardKey.ArrowLeft && modifiers == default)
            {
                command = selecting
                    ? EditCommand.MoveSelectionBackwardsByCharacter
                    : EditCommand.MoveBackwardsByCharacter;
            }
            else if (key == KeyboardKey.ArrowRight && modifiers == default)
            {
                command = selecting
                    ? EditCommand.MoveSelectionForwardByCharacter
                    : EditCommand.MoveForwardByCharacter;
            }
            else if (key == KeyboardKey.ArrowLeft && modifiers == KeyboardModifier.Control)
            {
                command = selecting
                    ? EditCommand.MoveSelectionBackwardsByWord
                    : EditCommand.MoveBackwardsByWord;
            }
            else if (key == KeyboardKey.ArrowRight && modifiers == KeyboardModifier.Control)
            {
                command = selecting
                    ? EditCommand.MoveSelectionForwardByWord
                    : EditCommand.MoveForwardByWord;
            }
            else if (key == KeyboardKey.End)
            {
                command = selecting
                    ? EditCommand.MoveSelectionToEndOfLine
                    : EditCommand.MoveToEndOfLine;
            }
            else if (key == KeyboardKey.Home)
            {
                command = selecting
                    ? EditCommand.MoveSelectionToStartOfLine
                    : EditCommand.MoveToStartOfLine;
            }
            else if (key == KeyboardKey.Backspace && modifiers == default)
            {
                command = EditCommand.DeletePreviousCharacter;
            }
            else if (key == KeyboardKey.Backspace && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.DeletePreviousWord;
            }
            else if (key == KeyboardKey.Delete && modifiers == default)
            {
                command = EditCommand.DeleteNextCharacter;
            }
            else if (key == KeyboardKey.Delete && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.DeleteNextWord;
            }
            else if (key == KeyboardKey.A && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.SelectAll;
            }
            else if (key == KeyboardKey.V && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.Paste;
            }
            else if (key == KeyboardKey.C && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.Copy;
            }
            else if (key == KeyboardKey.X && modifiers == KeyboardModifier.Control)
            {
                command = EditCommand.Cut;
            }
            else
            {
                command = default;
                return false;
            }

            return true;
        }
    }
}