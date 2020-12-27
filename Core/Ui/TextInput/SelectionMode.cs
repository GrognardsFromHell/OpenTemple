 namespace OpenTemple.Core.Ui.Widgets
{
    public enum SelectionMode
    {
        /// <summary>
        /// Make the newly inserted text the selected text.
        /// </summary>
        Select,

        /// <summary>
        /// Put the caret at the  start of the inserted text.
        /// </summary>
        Start,

        /// <summary>
        /// Put the caret at the end of the inserted text.
        /// </summary>
        End,
        Preserve
    }
}