using System;
using System.Drawing.Imaging;

namespace Mapgenix.Layers
{
    public static class GoogleMapAdapter
	{
		public static string GetMapType(GoogleMapsMapType mapType)
		{
			switch (mapType)
			{
				case GoogleMapsMapType.RoadMap:
					return "roadmap";

				case GoogleMapsMapType.Mobile:
					return "mobile";

				case GoogleMapsMapType.Satellite:
					return "satellite";

				case GoogleMapsMapType.Terrain:
					return "terrain";

				case GoogleMapsMapType.Hybrid:
					return "hybrid";

				default:
					return "roadmap";
			}
		}

		public static string GetPictureFormat(GoogleMapsPictureFormat pictureFormat)
		{
			switch (pictureFormat)
			{
				case GoogleMapsPictureFormat.Jpeg:
					return "jpg-baseline";

				case GoogleMapsPictureFormat.Gif:
					return "gif";

				case GoogleMapsPictureFormat.Png8:
					return "png8";

				case GoogleMapsPictureFormat.Png32:
					return "png32";

				default:
					return "jpg-baseline";
			}
		}



		public static ImageFormat GetImageFormat(GoogleMapsPictureFormat pictureFormat)
		{
			switch (pictureFormat)
			{
				case GoogleMapsPictureFormat.Jpeg:
					return ImageFormat.Jpeg;

				case GoogleMapsPictureFormat.Gif:
					return ImageFormat.Gif;

				case GoogleMapsPictureFormat.Png8:
					return ImageFormat.Png;

				case GoogleMapsPictureFormat.Png32:
					return ImageFormat.Png;

				default:
					return ImageFormat.Jpeg;
			}
		}

		public static double FromLongitudetoGooglePixel(double longitude, double ru, double uu)
		{
			return ru + (longitude * uu);
		}

		public static double FromGooglePixelToLongitude(double xPixel, double ru, double uu)
		{
			return (xPixel - ru) / uu;
		}

		public static double FromLatitudeToGooglePixel(double latitude, double ru, double vu)
		{
			double f = Math.Sin(latitude * (Math.PI / 180.0));
			return ru + (0.5 * Math.Log((1 + f) / (1 - f)) * (-1 * vu));
		}

		public static double FromGooglePixelToLatitude(double yPixel, double ru, double vu)
		{
			double g = (yPixel - ru) / (-1 * vu);
			return (2.0 * Math.Atan(Math.Exp(g)) - (Math.PI / 2.0)) / (Math.PI / 180.0);
		}

       
	}
}
