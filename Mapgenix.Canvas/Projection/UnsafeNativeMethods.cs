using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mapgenix.Canvas
{
    public static class UnsafeNativeMethods
    {
        [DllImport("kernel32", EntryPoint = "GetShortPathNameA", ExactSpelling = true, CharSet = CharSet.Ansi,
            SetLastError = true)]
        public static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode)]
        public static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        [DllImport("GDI32.dll")]
        public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc,
            int nXSrc, int nYSrc, int dwRop);

        [DllImport("GDI32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("GDI32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("GDI32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("GDI32.dll")]
        public static extern int SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
    }
}