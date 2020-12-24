namespace OpenTemple.Core.Ui
{
    public interface IClipboard
    {
        void SetText(string text);

        bool TryGetText(out string text);
    }
}