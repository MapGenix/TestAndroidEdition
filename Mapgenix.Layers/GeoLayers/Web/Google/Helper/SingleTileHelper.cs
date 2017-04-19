using System;
using System.Drawing;
using System.IO;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class SingleTileHelper : ITileHelper
	{
		public GoogleRequestBuilder RequestBuilder{private get; set;}
		
		public void Process(BaseGeoCanvas canvas, int zoomLevelNumber, BaseProjection projection, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{

			PointShape center = (PointShape)projection.ConvertToExternalProjection(canvas.CurrentWorldExtent.GetCenterPoint());

			string rectangleInfoAndSize = RequestBuilder.CreateRectangleForQuery(canvas, center, zoomLevelNumber);
			string requestString = RequestBuilder.CreateRequestString(rectangleInfoAndSize, mapType, pictureFormat);

			Uri requestUri = new Uri(requestString);
			
			WebRequest request = HttpWebRequest.Create(requestUri);
			using (Stream imageStream = request.GetResponse().GetResponseStream())
			{
				using (Bitmap image = new Bitmap(imageStream))
				{
					ResponseProcessor.ProcessResponse(canvas, image);
				}
			}
		}

	}
}
