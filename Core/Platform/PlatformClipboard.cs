using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Interop;

namespace OpenTemple.Core.Platform
{
    public class PlatformClipboard : IClipboard
    {
        public void SetText(string text)
        {
            NativePlatform.CopyToClipboard(Tig.MainWindow.NativeHandle, text);
        }

        public bool TryGetText(out string text)
        {
            return false;
        }
    }
}