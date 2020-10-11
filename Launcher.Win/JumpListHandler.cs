using System;
using System.Diagnostics;
using System.Reflection;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;
using OpenTemple.Interop;

namespace OpenTemple.Windows
{
    /// <summary>
    /// Jump lists are the extra items that are shown by Windows when right-clicking an App in the start
    /// menu or task bar. The items will launch the same application again and pass extra command line args.
    /// This class handles setting up such a jump list, and also handling the arguments.
    /// </summary>
    public static class JumpListHandler
    {
        private const string ChangeInstallationDirVerb = "--change-installation-dir";
        private const string OpenSaveGameFolderVerb = "--open-save-game-folder";

        public static bool Handle(string[] commandLineArgs)
        {
            if (commandLineArgs.Length > 0)
            {
                var verb = commandLineArgs[0];
                switch (verb)
                {
                    case ChangeInstallationDirVerb:
                        ChangeInstallationDir();
                        return true;
                    case OpenSaveGameFolderVerb:
                        OpenSaveGameFolder();
                        return true;
                }
            }

            UpdateJumpList();

            return false;
        }

        private static void ChangeInstallationDir()
        {
            var gameFolders = new GameFolders();
            var configManager = new GameConfigManager(gameFolders);

            var currentFolder = configManager.Config.InstallationFolder;

            ValidationReport validationReport = null;
            string selectedFolder;
            do
            {
                if (!InstallationDirSelector.Select(validationReport, currentFolder, out selectedFolder))
                {
                    return;
                }

                validationReport = ToEEInstallationValidator.Validate(selectedFolder);
            } while (selectedFolder == null || !validationReport.IsValid);

            configManager.Config.InstallationFolder = selectedFolder;
            configManager.Save();
        }

        private static void OpenSaveGameFolder()
        {
            var gameFolders = new GameFolders();
            NativePlatform.OpenFolder(gameFolders.SaveFolder);
        }

        private static void UpdateJumpList()
        {
            using var builder = new JumpListBuilder();
            builder.AddTask(
                ChangeInstallationDirVerb,
                "Switch ToEE Installation",
                "shell32.dll",
                3,
                "Changes the Temple of Elemental Evil installation directory used by OpenTemple"
            );
            builder.AddTask(
                OpenSaveGameFolderVerb,
                "Open Savegame Folder",
                "shell32.dll",
                3,
                "Opens the folder where save games are located"
            );
            builder.Commit();
        }
    }
}