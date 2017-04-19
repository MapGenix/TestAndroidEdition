using Mapgenix.Canvas;
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
    [Serializable]
    internal class BingMapsLayerImageCache
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

        public string GetFilename(int zoomlevel, int tileX, int tileY, BingMapsPictureFormat pictureFormat, string mapType)
        {
            var directory = _cacheDirectory + zoomlevel;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory += "\\" + tileX;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}{2}-{3}-{4}.{5}", directory, mapType,
                zoomlevel, tileX, tileY, GetPictureFormat(pictureFormat));
        }

        public void SaveCacheImage(Bitmap image, int zoomlevel, int tileX, int tileY,
            BingMapsPictureFormat pictureFormat, string mapType)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat, mapType);
            image.Save(filename, GetImageFormat(pictureFormat));
        }

        public Bitmap GetCacheImage(int zoomlevel, int tileX, int tileY, BingMapsPictureFormat pictureFormat, string mapType)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat, mapType);
            if (File.Exists(filename))
            {
                return new Bitmap(filename);
            }

            return null;
        }

        public ImageFormat GetImageFormat(BingMapsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case BingMapsPictureFormat.Png:
                    return ImageFormat.Png;

                case BingMapsPictureFormat.Gif:
                    return ImageFormat.Gif;

                case BingMapsPictureFormat.Jpeg:
                    return ImageFormat.Jpeg;

                default:
                    return ImageFormat.Png;
            }
        }

        public string GetPictureFormat(BingMapsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case BingMapsPictureFormat.Png:
                    return "png";

                case BingMapsPictureFormat.Gif:
                    return "gif";

                case BingMapsPictureFormat.Jpeg:
                    return "jpg";

                default:
                    return "png";
            }
        }
    }
}
