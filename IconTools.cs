using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GetFileIcons
{
    public static class IconTools
    {
        internal const uint SHGFI_ICON = 0x100;
        internal const uint SHGFI_LARGEICON = 0x0;
        internal const uint SHGFI_SMALLICON = 0x1;
        const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        private class NativeMethods
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref SHFILEINFO psfi,
                uint cbSizeFileInfo,
                ShellIconSize uFlags
            );

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public extern static bool DestroyIcon(IntPtr handle);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;

            public IntPtr iIcon;

            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        public static ImageSource GetImageSourceForExtension(string extension, ShellIconSize iconsize = ShellIconSize.SmallIcon)
        {
            if (GetIconForExtension(extension, iconsize) is Icon icon)
            {
                var result = Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                result.Freeze();
                return result;
            }
            return null;
        }

        public static Icon GetIconForExtension(string extension, ShellIconSize size)
        {
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".xd2"; // unexistent extension for a default file icon
            }
            // repeat the process used for files, but instruct the API not to access the file
            size |= (ShellIconSize)SHGFI_USEFILEATTRIBUTES;
            return GetIconForFile(extension, size);
        }

        private static Icon GetIconForFile(string filename, ShellIconSize size)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            NativeMethods.SHGetFileInfo(filename, 0, ref shinfo, (uint) Marshal.SizeOf(shinfo), size);

            Icon icon = null;

            if (shinfo.hIcon != IntPtr.Zero)
            {
                // create the icon from the native handle and make a managed copy of it
                icon = (Icon) Icon.FromHandle(shinfo.hIcon).Clone();

                // release the native handle
                NativeMethods.DestroyIcon(shinfo.hIcon);
            }

            return icon;
        }
    }

    public enum ShellIconSize : uint
    {
        SmallIcon = IconTools.SHGFI_ICON | IconTools.SHGFI_SMALLICON,
        LargeIcon = IconTools.SHGFI_ICON | IconTools.SHGFI_LARGEICON
    }
}
