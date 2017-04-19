using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    public class OpenStreetRequestBuilder
	{
		const string ServerUrl = "http://tile.openstreetmap.org/{0}/{1}/{2}.png";
		const double OpenStreetMapLimitLatitud = 85.0;
		const int OpenStreetMapLayerTileHeight = 256;
		const int TileHeight = 256;
		const int TileWidth = 256;

		public WebProxy WebProxy
		{
			get;
			set;
		}

		public int TimeoutInSeconds
		{
			get;
			set;
		}

		public Bitmap GetImage(string requestString)
		{
			Bitmap image = null;

			try
			{
			}

			catch (Exception ex)
			{
			}

			finally
			{
				if (image == null)
				{
					image = new Bitmap(1, 1);
				}
			}
			return image;
		}

		public Dictionary<int, Bitmap> GetImagesArray(int startTileX, int startTileY, int endTileX, int endTileY, int zoomlevelNumber)
		{
			Dictionary<int, Bitmap> images = new Dictionary<int, Bitmap>();
			int tileImageId = 0;
			for (int j = startTileY; j < endTileY; j++)
			{
				for (int i = startTileX; i < endTileX; i++)
				{
					int tileX = i;
					int tileY = j;

					string requestString = String.Format(CultureInfo.InvariantCulture, ServerUrl, zoomlevelNumber, tileX, tileY);
					WebRequest request = HttpWebRequest.Create(requestString);
					request.Timeout = TimeoutInSeconds * 1000;
					request.Proxy = WebProxy;
					
					using (Stream imageStream = request.GetResponse().GetResponseStream())
					{
						Bitmap image = new Bitmap(imageStream);
						images.Add(tileImageId, image);
					}
					tileImageId++;
				}
			}

			return images;
		}

		public Bitmap GetBufferImage(Size imageSize, int startTileX, int startTileY, int endTileX, int endTileY, int zoomLevel)
		{

			Dictionary<int, Bitmap> images = GetImagesArray(startTileX, startTileY, endTileX, endTileY, zoomLevel);

			Bitmap bufferImage = new Bitmap(imageSize.Width, imageSize.Height);
			using (Graphics bufferGraphics = Graphics.FromImage(bufferImage))
			{
				int tileCountInRow = endTileX - startTileX;
				for (int j = startTileY; j < endTileY; j++)
				{
					for (int i = startTileX; i < endTileX; i++)
					{
						int imageIndex = (j - startTileY) * tileCountInRow + (i - startTileX);
						using (Bitmap tileImage = images[imageIndex])
						{
							int tempPositionX = (i - startTileX) * TileWidth;
							int tempPositionY = (j - startTileY) * TileHeight;
							bufferGraphics.DrawImageUnscaled(tileImage, tempPositionX, tempPositionY);

						}
					}
				}
			}

			return bufferImage;
		}

		public Bitmap GetTileImages(RectangleShape renderExtent, int canvasWidth, int canvasHeight, int zoomLevel, int maxPixelInZoomLevel)
		{
			double renderExtentCenterLatitude = GeometryHelper.GetCenterY(renderExtent);
			double renderExtentCenterLongitude = GeometryHelper.GetCenterX(renderExtent);

			double vu = OpenStreetMapLayerTileHeight * Math.Pow(2.0, zoomLevel) / (2.0 * Math.PI);
			double ru = OpenStreetMapLayerTileHeight * Math.Pow(2.0, zoomLevel) / (2.0);
			double uu = OpenStreetMapLayerTileHeight * Math.Pow(2.0, zoomLevel) / (360.0);


			double centerPixelX = GoogleMapAdapter.FromLongitudetoGooglePixel(renderExtentCenterLongitude, ru, uu);
			double centerPixelY = FromLatitudeToGooglePixel(renderExtentCenterLatitude, ru, vu);
			int minPixelX = (int)Math.Round(centerPixelX - canvasWidth * 0.5);
			int minPixelY = (int)Math.Round(centerPixelY - canvasHeight * 0.5);
			int maxPixelX = minPixelX + canvasWidth;
			int maxPixelY = minPixelY + canvasHeight;

			if (minPixelX < 0)
			{
				minPixelX = 0;
			}
			if (minPixelY < 0)
			{
				minPixelY = 0;
			}
			maxPixelX = Math.Min(maxPixelX, maxPixelInZoomLevel);
			maxPixelY = Math.Min(maxPixelY, maxPixelInZoomLevel);

			int startTileX = (int)Math.Floor(minPixelX / (double)TileWidth);
			int startTileY = (int)Math.Floor(minPixelY / (double)TileHeight);
			int endTileX = (int)Math.Ceiling(maxPixelX / (double)TileWidth);
			int endTileY = (int)Math.Ceiling(maxPixelY / (double)TileHeight);
			Size imageSize = new Size
			{
				Width = (endTileX - startTileX) * TileWidth,
				Height = (endTileY - startTileY) * TileHeight
			};

			Point pastOffset = new Point
			{
				X = (int)Math.Round(centerPixelX - canvasWidth * 0.5) - startTileX * TileWidth,
				Y = (int)Math.Round(centerPixelY - canvasHeight * 0.5) - startTileY * TileHeight
			};

			Bitmap resultImage = new Bitmap(canvasWidth, canvasHeight);
			using (Bitmap bufferImage = GetBufferImage(imageSize, startTileX, startTileY, endTileX, endTileY, zoomLevel))
			{
				using (Graphics resultGraphics = Graphics.FromImage(resultImage))
				{
					resultGraphics.DrawImageUnscaled(bufferImage, -1 * pastOffset.X, -1 * pastOffset.Y);
				}
			}
			return resultImage;
		}

		double FromLatitudeToGooglePixel(double latitude, double ru, double vu)
		{
			double f = Math.Sin(latitude * (Math.PI / 180.0));
			f = Math.Max(f, -0.9999);
			f = Math.Min(f, 0.9999);
			return ru + (0.5 * Math.Log((1 + f) / (1 - f)) * (-1 * vu));
		}


		public void ProcessMultiTileMode(BaseGeoCanvas canvas, BaseProjection projection)
		{

			if (OpenStreetProjectionHelper.IsCompleteOutOfEarth(canvas.CurrentWorldExtent, projection))
			{
				return;
			}


			int zoomlevelNumber = ZoomLevelHelper.GetCurrentZoomLevelNumberOpenStreet(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit, canvas.Dpi);
			int maxPixelInZoomLevel = (int)Math.Round(OpenStreetMapLayerTileHeight * Math.Pow(2.0, zoomlevelNumber));


			RectangleShape currentExtent = OpenStreetProjectionHelper.GetExtentInDecimalDegree(canvas, projection);

			int renderWidth = (int)canvas.Width;
			int renderHeight = (int)canvas.Height;
			RectangleShape renderExtent = GetRenderExtent(currentExtent, ref renderHeight);

			using (Bitmap resultImage = GetTileImages(renderExtent, renderWidth, renderHeight, zoomlevelNumber, maxPixelInZoomLevel))
			{
				ResponseProcessor.ProcessResponseMultiTile(canvas, resultImage);
			}


		}



		static RectangleShape GetRenderExtent(RectangleShape fittedExtent, ref int expectHeight)
		{
			RectangleShape renderExtent = SerializationHelper.CloneDeep(fittedExtent);
			double extentCenterY = renderExtent.GetCenterPoint().Y;
			if (extentCenterY > OpenStreetMapLimitLatitud || extentCenterY < -1 * OpenStreetMapLimitLatitud)
			{
				if (extentCenterY > OpenStreetMapLimitLatitud)
				{
					renderExtent.UpperLeftPoint.Y = OpenStreetMapLimitLatitud + (OpenStreetMapLimitLatitud - renderExtent.LowerRightPoint.Y);
					expectHeight = (int)(renderExtent.Height * expectHeight / fittedExtent.Height);
				}
				else if (extentCenterY < -1 * OpenStreetMapLimitLatitud)
				{
					renderExtent.LowerRightPoint.Y = -1 * OpenStreetMapLimitLatitud - (renderExtent.UpperLeftPoint.Y + OpenStreetMapLimitLatitud);
					expectHeight = (int)(renderExtent.Height * expectHeight / fittedExtent.Height);
				}
			}

			return renderExtent;
		}


	}
}