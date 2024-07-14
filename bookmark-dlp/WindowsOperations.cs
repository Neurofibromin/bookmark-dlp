using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bookmark_dlp
{
    /// <summary>
    ///  Hides the console window on Windows, requires unsafe code to be enabled.
    /// </summary>
    public enum WindowMode : int { Hidden = 0, Visible = 5 }

    public static partial class WindowsOperations
    {
        private static readonly IntPtr _handle = GetConsoleWindow();

        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetConsoleWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr window_handle, int cmd_show_mode);

        public static void SetWindowMode(WindowMode mode)
        {
            ShowWindow(_handle, (int)mode);
        }
    }
}