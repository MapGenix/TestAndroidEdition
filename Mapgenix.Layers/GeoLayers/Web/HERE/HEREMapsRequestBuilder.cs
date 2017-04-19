using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    internal class HEREMapsRequestBuilder
	{

		public String AppID
		{
			get;
			set;
		}

        public String AppCode
        {
            get;
            set;
        }

		public WebProxy WebProxy
		{
			get;
			set;
		}

		public string CreateRectangleInfoAndSize(PointShape center, BaseGeoCanvas canvas, int zoomLevel)
		{
			int newWidth = Math.Min((int)canvas.Width, 1024);
			int newHeight = Math.Min((int)canvas.Height, 1024);
			string rectangleInfoAndSize = string.Empty;
			string startingLocationLatitude = Math.Round(center.Y, 6).ToString(CultureInfo.InvariantCulture);
			string startingLocationLongitude = Math.Round(center.X, 6).ToString(CultureInfo.InvariantCulture);
			if (string.Equals(startingLocationLatitude, "0"))
			{
				startingLocationLatitude = "0.000001";
			}
			if (string.Equals(startingLocationLongitude, "0"))
			{
				startingLocationLongitude = "0.000001";
			}
			rectangleInfoAndSize += "ctr=" + startingLocationLatitude + "," + startingLocationLongitude + "&";
			rectangleInfoAndSize += "z=" + zoomLevel + "&";
			rectangleInfoAndSize += "w=" + newWidth + "&" + "h=" + newHeight;
			return rectangleInfoAndSize;
		}

        public string CreateRectangleInfoAndSize(double lon, double lat, int width, int height, int zoomLevel)
        {
            double startingLocationLatitude = (lat == 0) ? 0.000001 : lat;
            double startingLocationLongitude = (lon == 0) ? 0.000001 : lon;

            string rectangleInfoAndSize = string.Empty;
            rectangleInfoAndSize += "ctr=" + startingLocationLatitude + "," + startingLocationLongitude + "&";
            rectangleInfoAndSize += "z=" + zoomLevel + "&";
            rectangleInfoAndSize += "w=" + width + "&" + "h=" + height;
            return rectangleInfoAndSize;
        }


        public string CreateRequestString(string rectangleInfoAndSize, HEREMapsPictureFormat pictureFormat, HEREMapsMapType mapType)
		{
			string requestString = @"https://image.maps.cit.api.here.com/mia/1.6/mapview?";
			requestString += rectangleInfoAndSize + "&";
			requestString += "f=" + (int)pictureFormat + "&";
            requestString += "t=" + (int)mapType + "&";
            requestString += "app_id=" + AppID + "&";
            requestString += "app_code=" + AppCode;
            return requestString;
		}

		public Bitmap FindImage(string requestString)
		{

			Bitmap image = null;
			WebRequest request = HttpWebRequest.Create(requestString);
			request.Proxy = WebProxy;
			using (Stream imageStream01 = request.GetResponse().GetResponseStream())
			{
                image = new Bitmap(imageStream01);
			}

			return image;
		}
       
	}
}
