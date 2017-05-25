using Android.Graphics;
using Mapgenix.Layers;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class GoogleMapsAndroidLayerImageCache
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

                    if (!cacheDirectory.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        cacheDirectory += "/";
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
            directory += "/" + tileX;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}-{3}-{4}.{5}", directory, GoogleMapAdapter.GetMapType(mapType), zoomlevel, tileX, tileY, GoogleMapAdapter.GetPictureFormat(pictureFormat));

        }

        public string GetFilename(GoogleMapsMapType mapType, int zoomlevel, string urlPath, GoogleMapsPictureFormat pictureFormat)
        {
            var directory = cacheDirectory + zoomlevel;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            directory += "/" + GoogleMapAdapter.GetMapType(mapType);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string md5String = ToMD5Hash(urlPath);

            return string.Format(CultureInfo.InvariantCulture, "{0}/{1}.{2}", directory, md5String, GetPictureFormat(pictureFormat));
        }

        public void SaveCacheImage(Bitmap image, GoogleMapsMapType mapType, int zoomlevel, int tileX, int tileY, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, tileX, tileY, pictureFormat);
            using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                image.Compress(GetImageFormat(pictureFormat), 5, file);
            }                
        }

        public void SaveCacheImage(Bitmap image, GoogleMapsMapType mapType, int zoomlevel, string urlPath, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, urlPath, pictureFormat);
            using (FileStream file = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
            {
                image.Compress(GetImageFormat(pictureFormat), 20, file);
            }                
        }

        public Bitmap GetCacheImage(GoogleMapsMapType mapType, int zoomlevel, int tileX, int tileY, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, tileX, tileY, pictureFormat);
            if (File.Exists(filename))
            {
                return BitmapFactory.DecodeFile(filename);
            }
            return null;
        }

        public Bitmap GetCacheImage(GoogleMapsMapType mapType, int zoomlevel, string urlPath, GoogleMapsPictureFormat pictureFormat)
        {
            string filename = GetFilename(mapType, zoomlevel, urlPath, pictureFormat);
            if (File.Exists(filename))
            {
                return BitmapFactory.DecodeFile(filename);
            }
            return null;
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

        public static Bitmap.CompressFormat GetImageFormat(GoogleMapsPictureFormat pictureFormat)
		{
			switch (pictureFormat)
			{
				case GoogleMapsPictureFormat.Jpeg:
					return Bitmap.CompressFormat.Jpeg;
				case GoogleMapsPictureFormat.Png8:
				case GoogleMapsPictureFormat.Png32:
					return Bitmap.CompressFormat.Png;

				default:
					return Bitmap.CompressFormat.Jpeg;
			}
		}

        public string GetPictureFormat(GoogleMapsPictureFormat pictureFormat)
        {
            switch (pictureFormat)
            {
                case GoogleMapsPictureFormat.Png8:
                case GoogleMapsPictureFormat.Png32:
                    return "png";

                case GoogleMapsPictureFormat.Jpeg:
                    return "jpg";

                default:
                    return "png";
            }
        }
    }
}