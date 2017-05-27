using Android.Graphics;
using Mapgenix.Canvas;
using Mapgenix.Layers;
using Mapgenix.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mapgenix.GSuite.Android
{
    internal class BingMapAndroidRequestBuilder
    {
        public String LicenseKey
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
            rectangleInfoAndSize += startingLocationLatitude + "," + startingLocationLongitude + "/";
            rectangleInfoAndSize += zoomLevel + "?";
            rectangleInfoAndSize += "mapSize=" + newWidth + "," + newHeight;
            return rectangleInfoAndSize;
        }

        public string CreateRectangleInfoAndSize(double lon, double lat, int width, int height, int zoomLevel)
        {
            double startingLocationLatitude = (lat == 0) ? 0.000001 : lat;
            double startingLocationLongitude = (lon == 0) ? 0.000001 : lon;

            string rectangleInfoAndSize = string.Empty;
            rectangleInfoAndSize += startingLocationLatitude + "," + startingLocationLongitude + "/";
            rectangleInfoAndSize += zoomLevel + "?";
            rectangleInfoAndSize += "mapSize=" + width + "," + height;
            return rectangleInfoAndSize;
        }

        public string CreateRequestString(string rectangleInfoAndSize, BingMapsPictureFormat pictureFormat, BingMapsMapType mapType, bool trafficFlowEnable)
        {
            string requestString = @"http://dev.virtualearth.net/REST/v1/Imagery/Map/";
            requestString += GetMapType(mapType) + "/";
            requestString += rectangleInfoAndSize + "&";
            requestString += "format=" + GetMapPictureFormat(pictureFormat) + "&";

            if(trafficFlowEnable)
                requestString += "mapLayer=TrafficFlow&";

            requestString += "key=" + LicenseKey;
            return requestString;
        }

        public Bitmap FindImage(string requestString)
        {

            Bitmap image = null;
            WebRequest request = HttpWebRequest.Create(requestString);
            request.Proxy = WebProxy;
            WebResponse response01 = request.GetResponse();
            using (Stream imageStream01 = response01.GetResponseStream())
            {
                image = BitmapFactory.DecodeStream(imageStream01);
            }

            return image;
        }

        private string GetMapType(BingMapsMapType type)
        {
            if (type.HasFlag(BingMapsMapType.Aerial))
                return "Aerial";
            if (type.HasFlag(BingMapsMapType.AerialWithLabels))
                return "AerialWithLabels";
            if (type.HasFlag(BingMapsMapType.Road))
                return "Road";
            else
                return "Road";
        }

        private string GetMapPictureFormat(BingMapsPictureFormat format)
        {
            switch (format)
            {
                case BingMapsPictureFormat.Png:
                    return "png";
                case BingMapsPictureFormat.Jpeg:
                    return "jpeg";
                case BingMapsPictureFormat.Gif:
                    return "gif";
                default:
                    return "png";
            }
        }
    }
}
