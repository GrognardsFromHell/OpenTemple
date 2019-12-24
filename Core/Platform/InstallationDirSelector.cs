using System;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTemple.Core.Startup;

namespace OpenTemple.Core.Platform
{
    public static class InstallationDirSelector
    {
        /// <summary>
        /// Tries to detect where Temple of Elemental Evil has been installed on this machine.
        /// </summary>
        public static bool TryFind(out string directory)
        {
            // TODO Just try out some "well known" places it could have been installed
            // These are used as the default location by the Retail installer:
            // C:\Program Files (x86)\Atari\Temple of Elemental Evil

            if (FindInstallDirectory(out var directoryPtr))
            {
                directory = Marshal.PtrToStringUni(directoryPtr);
                Marshal.FreeCoTaskMem(directoryPtr);
                return true;
            }
            else
            {
                directory = null;
                return false;
            }
        }

        /// <summary>
        /// Informs the user about an issue with the selected installation directory and allows them
        /// to select the actual directory.
        /// </summary>
        public static bool Select(
            ValidationReport validationErrors,
            string currentDirectory,
            out string selectedPath
        )
        {
            var errorIcon = false;
            var promptTitle = "Temple of Elemental Evil Files";
            var promptEmphasized = "Choose Temple of Elemental Evil Installation";
            var promptDetailed = "The Temple of Elemental Evil data files are required to run OpenTemple.\n\n"
                                 + "Please selected the folder where Temple of Elemental Evil is installed to continue.";
            var pickerTitle = "Choose Temple of Elemental Evil Folder";

            // In case a directory was selected, but it did not contain a valid ToEE installation, show an actual error
            // rather an an informational message
            if (validationErrors != null && !validationErrors.IsValid)
            {
                promptEmphasized = "Incomplete Temple of Elemental Evil Installation";
                errorIcon = true;
                promptDetailed = "The Temple of Elemental Evil data files are required to run OpenTemple.\n\n"
                                 + "Currently selected:\n"
                                 + currentDirectory + "\n\n"
                                 + "Problems found:\n"
                                 + string.Join("\n", validationErrors.Messages.Select(message => " - " + message))
                                 + "\n\nPlease selected the folder where Temple of Elemental Evil is installed to continue.";
            }

            var result = SelectInstallationDirectory(
                errorIcon,
                promptTitle,
                promptEmphasized,
                promptDetailed,
                pickerTitle,
                currentDirectory,
                out var newDirectoryPtr
            );
            if (!result || newDirectoryPtr == IntPtr.Zero)
            {
                selectedPath = null;
                return false;
            }

            selectedPath = Marshal.PtrToStringUni(newDirectoryPtr);
            FreeSelectInstallationDirectory(newDirectoryPtr);
            return true;
        }

        [DllImport("OpenTemple.Native", EntryPoint = "FindInstallDirectory")]
        private static extern bool FindInstallDirectory(out IntPtr directory);

        [DllImport("OpenTemple.Native", EntryPoint = "SelectInstallationDirectory")]
        private static extern bool SelectInstallationDirectory(
            bool errorIcon,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptTitle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptEmphasized,
            [MarshalAs(UnmanagedType.LPWStr)]
            string promptDetailed,
            [MarshalAs(UnmanagedType.LPWStr)]
            string folderPickerTitle,
            [MarshalAs(UnmanagedType.LPWStr)]
            string currentDirectory,
            out IntPtr newDirectory);

        [DllImport("OpenTemple.Native")]
        private static extern void FreeSelectInstallationDirectory(IntPtr dir);
    }
}