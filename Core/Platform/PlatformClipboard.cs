using OpenTemple.Core.Ui;
using OpenTemple.Interop;

namespace OpenTemple.Core.Platform
{
    public class PlatformClipboard : IClipboard
    {
        private readonly IMainWindow _mainWindow;

        public PlatformClipboard(IMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void SetText(string text)
        {
            NativePlatform.SetClipboardText(_mainWindow.NativeHandle, text);
        }

        public bool TryGetText(out string text)
        {
            return NativePlatform.TryGetClipboardText(_mainWindow.NativeHandle, out text);
        }
    }
}