using Mapgenix.Canvas;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace Mapgenix.Layers
{
    [Serializable]
    internal class OpenStreetMapLayerImageCache
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

        public string GetFilename(int zoomlevel, int tileX, int tileY, OpenStreetMapLayerPictureFormat pictureFormat)
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
            var filename = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}-{2}-{3}.{4}", directory, zoomlevel,
                tileX, tileY, GetPictureFormat(pictureFormat));

            return filename;
        }

        public void SaveCacheImage(Bitmap image, int zoomlevel, int tileX, int tileY,
            OpenStreetMapLayerPictureFormat pictureFormat)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat);
            image.Save(filename, GetImageFormat(pictureFormat));
        }

        public Bitmap GetCacheImage(int zoomlevel, int tileX, int tileY, OpenStreetMapLayerPictureFormat pictureFormat)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat);
            if (File.Exists(filename))
            {
                return new Bitmap(filename);
            }
            return null;
        }

        public static ImageFormat GetImageFormat(OpenStreetMapLayerPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case OpenStreetMapLayerPictureFormat.Jpeg:
                    return ImageFormat.Jpeg;

                case OpenStreetMapLayerPictureFormat.Gif:
                    return ImageFormat.Gif;

                case OpenStreetMapLayerPictureFormat.Png8:
                    return ImageFormat.Png;

                case OpenStreetMapLayerPictureFormat.Png32:
                    return ImageFormat.Png;

                default:
                    return ImageFormat.Jpeg;
            }
        }

        public static string GetPictureFormat(OpenStreetMapLayerPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case OpenStreetMapLayerPictureFormat.Jpeg:
                    return "jpg";

                case OpenStreetMapLayerPictureFormat.Gif:
                    return "gif";

                case OpenStreetMapLayerPictureFormat.Png8:
                    return "png";

                case OpenStreetMapLayerPictureFormat.Png32:
                    return "png";

                default:
                    return "jpg";
            }
        }
    }
}