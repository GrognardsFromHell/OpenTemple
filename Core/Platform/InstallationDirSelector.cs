using System;
using System.Runtime.InteropServices;

namespace SpicyTemple.Core.Platform
{
    public static class InstallationDirSelector
    {
        /// <summary>
        /// Tries to detect where Temple of Elemental Evil has been installed on this machine.
        /// </summary>
        public static bool TryFind(out string directory)
        {
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
        /// <param name="errorIcon"></param>
        /// <param name="promptTitle"></param>
        /// <param name="promptEmphasized"></param>
        /// <param name="promptDetailed"></param>
        /// <param name="pickerTitle"></param>
        /// <param name="currentDirectory"></param>
        /// <param name="selectedPath"></param>
        public static bool Select(
            bool errorIcon,
            string promptTitle,
            string promptEmphasized,
            string promptDetailed,
            string pickerTitle,
            string currentDirectory,
            out string selectedPath
        )
        {
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

        [DllImport("SpicyTemple.Native", EntryPoint = "FindInstallDirectory")]
        private static extern bool FindInstallDirectory(out IntPtr directory);

        [DllImport("SpicyTemple.Native", EntryPoint = "SelectInstallationDirectory")]
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

        [DllImport("SpicyTemple.Native")]
        private static extern void FreeSelectInstallationDirectory(IntPtr dir);
    }
}