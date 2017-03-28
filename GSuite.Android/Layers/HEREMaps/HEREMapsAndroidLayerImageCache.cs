using Android.Graphics;
using Mapgenix.Canvas;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    internal class HEREMapsAndroidLayerImageCache
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

                    if (!_cacheDirectory.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        _cacheDirectory += "/";
                    }
                }
            }
        }

        public bool IsCacheEnabled
        {
            get { return !string.IsNullOrEmpty(_cacheDirectory); }
        }

        public string GetFilename(int zoomlevel, int tileX, int tileY, HEREMapsPictureFormat pictureFormat, string mapType)
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
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}-{3}-{4}.{5}", directory, mapType,
                zoomlevel, tileX, tileY, GetPictureFormat(pictureFormat));
        }

        public string GetFilename(int zoomlevel, string urlPath, HEREMapsPictureFormat pictureFormat, string mapType)
        {
            var directory = _cacheDirectory + zoomlevel;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory += "/" + mapType;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string md5String = ToMD5Hash(urlPath);

            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}.{2}", directory, md5String, GetPictureFormat(pictureFormat));
        }

        public void SaveCacheImage(Bitmap image, int zoomlevel, int tileX, int tileY, HEREMapsPictureFormat pictureFormat, string mapType)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat, mapType);
            FileStream file = new FileStream(filename.ToString(), FileMode.Create, FileAccess.ReadWrite);
            image.Compress(GetImageFormat(pictureFormat), 0, file);
        }

        public void SaveCacheImage(Bitmap image, int zoomlevel, string urlPath, HEREMapsPictureFormat pictureFormat, string mapType)
        {
            var filename = GetFilename(zoomlevel, urlPath, pictureFormat, mapType);
            FileStream file = new FileStream(filename.ToString(), FileMode.Create, FileAccess.ReadWrite);
            image.Compress(GetImageFormat(pictureFormat), 0, file);
        }

        public Bitmap GetCacheImage(int zoomlevel, int tileX, int tileY, HEREMapsPictureFormat pictureFormat, string mapType)
        {
            var filename = GetFilename(zoomlevel, tileX, tileY, pictureFormat, mapType);
            if (File.Exists(filename))
            {
                return BitmapFactory.DecodeFile(filename);
            }

            return null;
        }

        public Bitmap GetCacheImage(int zoomlevel, string urlPath, HEREMapsPictureFormat pictureFormat, string mapType)
        {
            try
            {
                var filename = GetFilename(zoomlevel, urlPath, pictureFormat, mapType);
                if (File.Exists(filename))
                {
                    return BitmapFactory.DecodeFile(filename);
                }
            }
            catch(Exception ex)
            {
                return null;
            }
            
            return null;
        }

        public Bitmap.CompressFormat GetImageFormat(HEREMapsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case HEREMapsPictureFormat.Png:
                case HEREMapsPictureFormat.PNG8:
                    return Bitmap.CompressFormat.Png;

                /*case HEREMapsPictureFormat.Gif:
                    return ImageFormat.Gif;

                case HEREMapsPictureFormat.BMP:
                    return ImageFormat.Bmp;*/

                case HEREMapsPictureFormat.Jpeg:
                    return Bitmap.CompressFormat.Jpeg;

                default:
                    return Bitmap.CompressFormat.Png;
            }
        }

        public string GetPictureFormat(HEREMapsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case HEREMapsPictureFormat.Png:
                    return "png";

                case HEREMapsPictureFormat.Gif:
                    return "gif";

                case HEREMapsPictureFormat.BMP:
                    return "bmp";

                case HEREMapsPictureFormat.Jpeg:
                    return "jpg";

                default:
                    return "png";
            }
        }

        public string ToMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

    }
}