using System.IO;
using OpenTemple.Interop;

namespace OpenTemple.Core.Config
{
    /// <summary>
    /// Specifies the file system folders where certain data is located.
    /// </summary>
    public class GameFolders
    {
        public GameFolders()
        {
            UserDataFolder = NativePlatform.UserDataFolder;
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

    }
}