namespace OpenTemple.Core.Ui.Widgets
{
    /// <summary>
    /// Roughly modeled after https://html.spec.whatwg.org/#textFieldSelection
    /// </summary>
    public interface ITextInputElement
    {
        int SelectionStart { get; set; }

        int SelectionEnd { get; set; }

        SelectionDirection SelectionDirection { get; set; }

        void SetSelectionRange(int start, int end, SelectionDirection direction = default);

        void SetRangeText(string replacement, SelectionMode selectionMode = SelectionMode.Preserve);

        void SetRangeText(string replacement, int start, int end, SelectionMode selectionMode = SelectionMode.Preserve);
    }
}