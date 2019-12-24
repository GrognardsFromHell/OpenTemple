using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using OpenTemple.Core.Config;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Startup;

namespace Launcher
{
    /// <summary>
    /// Jump lists are the extra items that are shown by Windows when right-clicking an App in the start
    /// menu or task bar. The items will launch the same application again and pass extra command line args.
    /// This class handles setting up such a jump list, and also handling the arguments.
    /// </summary>
    public static class JumpListHandler
    {
        private const string ChangeInstallationDirVerb = "jumplist:change-installation-dir";
        private const string OpenSaveGameFolderVerb = "jumplist:open-save-game-folder";

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
                        OpenSaveGameFolder().Wait();
                        return true;
                }
            }

            if (JumpList.IsSupported())
            {
                Task.Run(UpdateJumpList);
            }

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

        private static async Task OpenSaveGameFolder()
        {
            var gameFolders = new GameFolders();
            await Windows.System.Launcher.LaunchFolderPathAsync(gameFolders.SaveFolder);
        }

        private static async Task UpdateJumpList()
        {
            var currentJumpList = await JumpList.LoadCurrentAsync();

            // As a game, we dont care about the "frequent" stuff,
            // although at a later date, we could add save games as "recently used files"
            currentJumpList.SystemGroupKind = JumpListSystemGroupKind.None;

            // Recreate our jump list entries
            currentJumpList.Items.Clear();
            currentJumpList.Items.Add(CreateChangeInstallationDirItem());
            currentJumpList.Items.Add(CreateOpenSaveGameFolderItem());

            await currentJumpList.SaveAsync();
        }

        private static JumpListItem CreateChangeInstallationDirItem()
        {
            var item = JumpListItem.CreateWithArguments(ChangeInstallationDirVerb, "Switch ToEE Installation");
            item.Description = "Changes the Temple of Elemental Evil installation directory used by OpenTemple";
            return item;
        }

        private static JumpListItem CreateOpenSaveGameFolderItem()
        {
            var item = JumpListItem.CreateWithArguments(OpenSaveGameFolderVerb, "Open Savegame Folder");
            item.Description = "Opens the folder where save games are located";
            return item;
        }
    }
}