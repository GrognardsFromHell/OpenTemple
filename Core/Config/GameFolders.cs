using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Config
{
    /// <summary>
    /// Specifies the file system folders where certain data is located.
    /// </summary>
    public class GameFolders
    {
        public GameFolders()
        {
            // Determine where the user data folder is
            using var result = new WideStringResult();
            unsafe
            {
                GameFolders_GetUserDataFolder(&result);
            }

            UserDataFolder = Path.GetFullPath(result.String);

            if (!Directory.Exists(UserDataFolder))
            {
                Directory.CreateDirectory(UserDataFolder);
            }
        }

        /// <summary>
        /// Folder for user specific data such as save games, configuration files, and screenshots.
        /// </summary>
        public string UserDataFolder { get; }

        public string SaveFolder => Path.Join(UserDataFolder, "saves");

        public string CurrentSaveFolder => Path.Join(SaveFolder, "current");

        [SuppressUnmanagedCodeSecurity]
        [DllImport("SpicyTemple.Native")]
        private static extern unsafe bool GameFolders_GetUserDataFolder(WideStringResult* result);
    }
}