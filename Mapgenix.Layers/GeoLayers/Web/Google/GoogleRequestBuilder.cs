using System;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class GoogleRequestBuilder
	{
		public string ClientId { get; set; }
		public string PrivateKey { get; set; }
		public string LicenseKey { get; set; }
		public int TimeoutInSeconds { get; set; }
		public WebProxy WebProxy { get; set; }
		

		public WebRequest CreateWebRequest(Uri requestUri)
		{
			WebRequest request = HttpWebRequest.Create(requestUri);
			request.Timeout = TimeoutInSeconds * 1000;
			request.Proxy = WebProxy;
			return request;
		}


		public string CreateRectangleForQuery(BaseGeoCanvas canvas, PointShape center, int zoomLevel)
		{
			Size size = new Size
			{
				Width = Math.Min((int)canvas.Width, 4000),
				Height = Math.Min((int)canvas.Height, 4000)
			};

			if (string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(PrivateKey))
			{
				size.Width = Math.Min((int)canvas.Width, 640);
				size.Height = Math.Min((int)canvas.Height, 640);
			}

			if (zoomLevel <= 1)
			{
				size.Width = Math.Min((int)canvas.Width, 512);
				size.Height = Math.Min((int)canvas.Height, 512);
			}

			string rectangleInfoAndSize = string.Empty;
			rectangleInfoAndSize += "center=" + Math.Round(center.Y, 6).ToString(CultureInfo.InvariantCulture) + "," + Math.Round(center.X, 6).ToString(CultureInfo.InvariantCulture) + "&";
			rectangleInfoAndSize += "zoom=" + zoomLevel + "&";
			rectangleInfoAndSize += "size=" + size.Width + "x" + size.Height;
			return rectangleInfoAndSize;
		}

		public string GetRequestImageUrl(double lon, double lat, int zoomLevelNumber, double tileWidth, double tileHeight, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			string regionInfoParameters = GetRegionInfoParameters(tileWidth, tileHeight, zoomLevelNumber, lon, lat);
			string requestString = @"http://maps.google.com/maps/api/staticmap?";
			requestString += regionInfoParameters + "&";
			requestString += "maptype=" + GoogleMapAdapter.GetMapType(mapType) + "&";
			requestString += "format=" + GoogleMapAdapter.GetPictureFormat(pictureFormat) + "&";
			if (string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(PrivateKey))
			{
				requestString += "key=" + LicenseKey + "&";
			}
			else
			{
				requestString += "client=" + ClientId + "&";
			}
			requestString += "sensor=false";

			if (!string.IsNullOrEmpty(ClientId) || !string.IsNullOrEmpty(PrivateKey))
			{
				requestString += "&signature=" + GetSignature(requestString);
			}

			return requestString;
		}

		public string CreateRequestString(string rectangleInfoAndSize, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			string requestString = @"http://maps.google.com/maps/api/staticmap?";
			requestString += rectangleInfoAndSize + "&";
			requestString += "maptype=" + GoogleMapAdapter.GetMapType(mapType) + "&";
			requestString += "format=" + GoogleMapAdapter.GetPictureFormat(pictureFormat) + "&";

			if (string.IsNullOrEmpty(ClientId) && string.IsNullOrEmpty(PrivateKey))
			{
				requestString += "key=" + LicenseKey + "&";
			}
			else
			{
				requestString += "client=" + ClientId + "&";
			}
			requestString += "sensor=false";

			if (!string.IsNullOrEmpty(ClientId) || !string.IsNullOrEmpty(PrivateKey))
			{
				requestString += "&signature=" + GetSignature(requestString);
			}
			return requestString;
		}

		public string GetSignature(string url)
		{
			Validators.CheckParameterIsNotNull(PrivateKey, "PrivateKey");

			string usablePrivateKey = PrivateKey.Replace("-", "+").Replace("_", "/");
			byte[] privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

			Uri uri = new Uri(url);
			byte[] encodedPathAndQueryBytes = GetAsciiBytes(uri.LocalPath + uri.Query);

			HMACSHA1 algorithm = new HMACSHA1(privateKeyBytes);
			byte[] hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

			return Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");
		}

		static byte[] GetAsciiBytes(string value)
		{
			byte[] temp = Encoding.Unicode.GetBytes(value);
			byte[] result = new byte[temp.Length / 2];

			for (int i = 0; i < temp.Length; i += 2)
			{
				result[i / 2] = temp[i];
				if (result[i / 2] > 127)
				{
					result[i / 2] = 63;
				}
			}
			return result;
		}


        static string GetRegionInfoParameters(double newWidth, double newHeight, int zoomLevelNumber, double centerLongitude, double newLatitude)
        {
            string rectangleInfoAndSize = string.Empty;
            rectangleInfoAndSize += "center=" + Math.Round(newLatitude, 6).ToString(CultureInfo.InvariantCulture) + "," + Math.Round(centerLongitude, 6).ToString(CultureInfo.InvariantCulture) + "&";
            rectangleInfoAndSize += "zoom=" + zoomLevelNumber + "&";
            rectangleInfoAndSize += "size=" + (int)newWidth + "x" + (int)newHeight;
            return rectangleInfoAndSize;
        }
		
	}
}
