using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Platform
{
    [SuppressUnmanagedCodeSecurity]
    public static class NativePlatform
    {
        public const string LibraryName = "OpenTemple.Native";

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        static NativePlatform()
        {
            // Determine where the user data folder is
            using var result = new WideStringResult();
            unsafe
            {
                GameFolders_GetUserDataFolder(&result);
            }

            UserDataFolder = Path.GetFullPath(result.String);
        }

        /// <summary>
        /// A path to the folder where user facing documents may be stored.
        /// On Windows this will be the Documents library.
        /// </summary>
        public static string UserDataFolder { get; }

        /// <summary>
        /// Opens the user's preferred file browsing application at the given local folder.
        /// </summary>
        public static void OpenFolder(string path)
        {
            Shell_OpenFolder(path);
        }

        /// <summary>
        /// Opens the user's preferred browser for the given URL.
        /// </summary>
        public static void OpenUrl(string url)
        {
            Shell_OpenUrl(url);
        }

        /// <summary>
        /// Allows the user to select a folder on their computer.
        /// </summary>
        /// <param name="title">Shown as the title of the folder picker</param>
        /// <param name="startingPath">Optionally provide the starting point of the folder picker process.</param>
        /// <param name="pickedFolder">Receives the path to the picked folder.</param>
        /// <returns>True if the user picked a folder, false if they canceled.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool PickFolder(string title, string startingPath, out string pickedFolder)
        {
            using var wideStringResult = new WideStringResult();
            int result;
            unsafe
            {
                result = Shell_PickFolder(title, startingPath, &wideStringResult);
            }

            if (result == 0)
            {
                pickedFolder = wideStringResult.String;
                return true;
            }
            else if (result == 1)
            {
                pickedFolder = null;
                return false;
            }
            else
            {
                throw new InvalidOperationException($"Failed to show folder picker. Code: {result}");
            }
        }

        /// <summary>
        /// Shows a prompt to the user with detailed info that they can confirm or close.
        /// </summary>
        /// <param name="errorIcon">Should the prompt display an error icon?</param>
        /// <param name="title">The window title.</param>
        /// <param name="emphasizedText">Emphasized text at the beginning of the prompt.</param>
        /// <param name="detailedText">More detailed in-depth text.</param>
        /// <returns>True if the user confirmed.</returns>
        public static bool ShowPrompt(bool errorIcon,
            string title,
            string emphasizedText,
            string detailedText)
        {
            return Shell_ShowPrompt(errorIcon, title, emphasizedText, detailedText);
        }

        /// <summary>
        /// Shows a message to the user with detailed info.
        /// </summary>
        /// <param name="errorIcon">Should the prompt display an error icon?</param>
        /// <param name="title">The window title.</param>
        /// <param name="emphasizedText">Emphasized text at the beginning of the prompt.</param>
        /// <param name="detailedText">More detailed in-depth text.</param>
        public static void ShowMessage(bool errorIcon,
            string title,
            string emphasizedText,
            string detailedText)
        {
            Shell_ShowMessage(errorIcon, title, emphasizedText, detailedText);
        }

        /// <summary>
        /// Copies text to the system clipboard.
        /// </summary>
        public static void CopyToClipboard(IntPtr nativeWindowHandle, string text)
        {
            var errorCode = Shell_CopyToClipboard(nativeWindowHandle, text);
            if (errorCode != 0)
            {
                Logger.Error("Failed to copy text '{0}' to clipboard: {1}", text, errorCode);
            }
        }

        [DllImport(LibraryName)]
        private static extern unsafe bool GameFolders_GetUserDataFolder(WideStringResult* result);

        [DllImport(LibraryName)]
        private static extern void Shell_OpenFolder([In, MarshalAs(UnmanagedType.LPWStr)]
            string path);

        [DllImport(LibraryName)]
        private static extern void Shell_OpenUrl([In, MarshalAs(UnmanagedType.LPWStr)]
            string url);

        [DllImport(LibraryName)]
        private static extern unsafe int Shell_PickFolder(
            [In, MarshalAs(UnmanagedType.LPWStr)]
            string title,
            [In, Optional, MarshalAs(UnmanagedType.LPWStr)]
            string startingDirectory,
            WideStringResult* pickedFolder
        );

        [DllImport(LibraryName)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Shell_ShowPrompt(
            bool errorIcon,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptTitle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptEmphasized,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptDetailed);

        [DllImport(LibraryName)]
        private static extern void Shell_ShowMessage(
            bool errorIcon,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptTitle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptEmphasized,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptDetailed);

        [DllImport(LibraryName)]
        private static extern int Shell_CopyToClipboard(
            IntPtr nativeWindowHandle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string text
        );
    }
}