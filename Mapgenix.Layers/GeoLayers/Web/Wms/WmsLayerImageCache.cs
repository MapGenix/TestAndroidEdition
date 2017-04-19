using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapgenix.Layers
{
    internal class WmsLayerImageCache
    {
        private string _cacheDirectory;

        public string CacheDirectory
        {
            get { return _cacheDirectory; }
            set
            {
                _cacheDirectory = value;
                if (!string.IsNullOrEmpty(_cacheDirectory))
                {
                    if (!Directory.Exists(_cacheDirectory))
                    {
                        Directory.CreateDirectory(_cacheDirectory);
                    }
                    _cacheDirectory = value;

                    if (!_cacheDirectory.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                    {
                        _cacheDirectory += "\\";
                    }
                }
            }
        }

        public bool IsCacheEnabled
        {
            get { return !string.IsNullOrEmpty(_cacheDirectory); }
        }

        public string GetFilename(int zoomlevel, RectangleShape worldExtent, WmsPictureFormat pictureFormat, string serviceUrl)
        {
            var directory = _cacheDirectory + "\\" + serviceUrl + "\\" + zoomlevel;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string extentString = worldExtent.GetWellKnownText();

            return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}-{2}.{3}", directory, zoomlevel, extentString, GetPictureFormat(pictureFormat));
        }

        public void SaveCacheImage(Bitmap image, int zoomlevel, RectangleShape worldExtent,
            WmsPictureFormat pictureFormat, string serviceUrl)
        {
            var filename = GetFilename(zoomlevel, worldExtent, pictureFormat, serviceUrl);
            image.Save(filename, GetImageFormat(pictureFormat));
        }

        public Bitmap GetCacheImage(int zoomlevel, RectangleShape worldExtent, WmsPictureFormat pictureFormat, string serviceUrl)
        {
            var filename = GetFilename(zoomlevel, worldExtent, pictureFormat, serviceUrl);
            if (File.Exists(filename))
            {
                return new Bitmap(filename);
            }

            return null;
        }

        public ImageFormat GetImageFormat(WmsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case WmsPictureFormat.Png:
                case WmsPictureFormat.Png8:
                    return ImageFormat.Png;

                case WmsPictureFormat.Gif:
                    return ImageFormat.Gif;

                case WmsPictureFormat.Tiff:
                    return ImageFormat.Tiff;

                case WmsPictureFormat.Jpeg:
                    return ImageFormat.Jpeg;

                default:
                    return ImageFormat.Png;
            }
        }

        public string GetPictureFormat(WmsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case WmsPictureFormat.Png:
                    return "png";

                case WmsPictureFormat.Png8:
                    return "png8";

                case WmsPictureFormat.Gif:
                    return "gif";

                case WmsPictureFormat.Tiff:
                    return "tiff";

                case WmsPictureFormat.Jpeg:
                    return "jpg";

                default:
                    return "png";
            }
        }

    }
}
