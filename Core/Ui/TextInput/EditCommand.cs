namespace OpenTemple.Core.Ui.TextInput
{
    public enum EditCommand
    {
        DeleteNextCharacter,
        DeletePreviousCharacter,
        DeleteNextWord,
        DeletePreviousWord,
        MoveForwardByCharacter,
        MoveBackwardsByCharacter,
        MoveSelectionForwardByCharacter,
        MoveSelectionBackwardsByCharacter,
        MoveForwardByWord,
        MoveBackwardsByWord,
        MoveSelectionForwardByWord,
        MoveSelectionBackwardsByWord,
        MoveToEndOfLine,
        MoveToStartOfLine,
        MoveSelectionToEndOfLine,
        MoveSelectionToStartOfLine,
        SelectAll,
        Copy,
        Paste,
        Cut
    }
}