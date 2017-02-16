using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Mapgenix.GSuite.Wpf
{
    internal static class ResourceUtil
    {
        public static Stream GetResourceStream(Bitmap bitmap)
        {
            Stream result = new MemoryStream();
            if (bitmap != null)
            {
                try
                {
                    bitmap.Save(result, ImageFormat.Png);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }

            return result;
        }
    }
}
