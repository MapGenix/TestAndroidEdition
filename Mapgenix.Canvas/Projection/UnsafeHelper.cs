using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Mapgenix.Canvas.Properties;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Static class as a wrapper for unsafe native methods
    /// </summary>
    public static class UnsafeHelper
    {
        /// <summary>Returns a boolean value for a specified key pressed or not.</summary>
        /// <param name="keys">Enum for the key.</param>
        /// <returns>Boolean value representing if a specified key is pressed or not.</returns>
        /// <remarks>It will call User32.dll API GetAsyncKeyState internally.</remarks>
        public static bool IsKeyPressed(Keys keys)
        {
            var result = false;

            var state = UnsafeNativeMethods.GetAsyncKeyState(keys);
            switch (state)
            {
                case 0:
                case 1:
                    break;
                case -32767:
                case -32768:
                    result = true;
                    break;
                default:
                    break;
            }

            return result;
        }

        
        public static Bitmap FastLoadImageFromFile(string pathFileName)
        {
            pathFileName = Path.GetFullPath(pathFileName);

            var loadingImage = IntPtr.Zero;


            if (UnsafeNativeMethods.GdipLoadImageFromFile(pathFileName, out loadingImage) != 0)
            {
                throw new InvalidOperationException(ExceptionDescription.GdipLoadImageFromFileError);
            }

            var imageType = typeof (Bitmap);

            return
                (Bitmap)
                    imageType.InvokeMember("FromGDIplus",
                        BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null,
                        new object[] {loadingImage}, CultureInfo.InvariantCulture);
        }
    }
}