using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlackIL
{
    public static class ExtensionMethods
    {
        // To support flashing.
        [DllImport("user32.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        //Flash both the window caption and taskbar button.
        //This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
        private const uint FLASHW_ALL = 3;

        private const uint FLASHW_STOP = 0;

        // Flash continuously until the window comes to the foreground. 
        private const uint FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        /// <summary>
        /// Send form taskbar notification, the Window will flash until get's focus
        /// <remarks>
        /// This method allows to Flash a Window, signifying to the user that some major event occurred within the application that requires their attention. 
        /// </remarks>
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static bool FlashNotification(this Form form, bool stop)
        {
            IntPtr hWnd = form.Handle;
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;

            if (!stop)
            {
                fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
                fInfo.uCount = uint.MaxValue;
            }
            else
            {
                fInfo.dwFlags = FLASHW_STOP;
                fInfo.uCount = 0;
            }

            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }


        public static Icon ToIcon(this Stream data)
        {
            Image image = Image.FromStream(data);
            Bitmap bitmap = new Bitmap(image);
            bitmap.SetResolution(72, 72);
            return System.Drawing.Icon.FromHandle(bitmap.GetHicon());
        }

        public static Icon ToIcon(this byte[] data)
        {
            Icon icon = null;
            using (var stream = new MemoryStream(data, 0, data.Length))
            {
                icon = stream.ToIcon();
            }

            return icon;
        }
    }
}
