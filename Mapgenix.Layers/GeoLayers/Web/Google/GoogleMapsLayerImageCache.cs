using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Mapgenix.Layers
{
    [Serializable]
    public class GoogleMapsLayerImageCache
    {
        private string cacheDirectory;

        public string CacheDirectory
        {
            get { return cacheDirectory; }
            set
            {
                cacheDirectory = value;
                if (!string.IsNullOrEmpty(cacheDirectory))
                {
                    if (!Directory.Exists(cacheDirectory))
                    {
                        Directory.CreateDirectory(cacheDirectory);
                    }
                    cacheDirectory = value;

                    if (!cacheDirectory.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
                    {
                        cacheDirectory += "\\";
                    }
                }
            }
        }

        public bool IsCacheEnabled
        {
            get { return !string.IsNullOrEmpty(cacheDirectory); }
        }

        private string GetFilename(GoogleMapsMapType mapType, int zoomlevel, int tileX, int tileY, GoogleMapsPictureFormat pictureFormat)
        {
            string directory = cacheDirectory + zoomlevel;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory += "\\" + tileX;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}{2}-{3}-{4}.{5}", directory, GoogleMapAdapter.GetMapType(mapType), zoomlevel, tileX, tileY, GoogleMapAdapter.GetPictureFormat(pictureFormat));

        }

        public void SaveCacheImage(Bitmap image, GoogleMapsMapType mapType, int zoomlevel, int tileX, int tileY, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, tileX, tileY, pictureFormat);
			image.Save(filename, GoogleMapAdapter.GetImageFormat(pictureFormat));
        }

        public Bitmap GetCacheImage(GoogleMapsMapType mapType, int zoomlevel, int tileX, int tileY, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, tileX, tileY, pictureFormat);
            if (File.Exists(filename))
            {
                return new Bitmap(filename);
            }
            return null;
        }
    }
}