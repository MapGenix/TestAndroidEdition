using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class MultiTileHelper : ITileHelper
	{
		const int GoogleTileHeight = 256;
		const int TileHeight = 512;
		const int TileWidth = 512;

		public GoogleRequestBuilder RequestBuilder { private get; set; }

		public void Process(BaseGeoCanvas canvas, int zoomLevelNumber, BaseProjection projection, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			if (ProjectionHelper.IsCompleteOutOfEarth(canvas.CurrentWorldExtent, projection))
			{
				return;
			}

			double vu = GoogleTileHeight * Math.Pow(2.0, zoomLevelNumber) / (2.0 * Math.PI);
			double ru = GoogleTileHeight * Math.Pow(2.0, zoomLevelNumber) / (2.0);
			double uu = GoogleTileHeight * Math.Pow(2.0, zoomLevelNumber) / (360.0);
			int maxPixelInZoomLevel = (int)Math.Round(GoogleTileHeight * Math.Pow(2.0, zoomLevelNumber));
			RectangleShape currentExtent = ProjectionHelper.GetExtentInDecimalDegree(canvas, projection);

			if (currentExtent.UpperLeftPoint.X < -180 && currentExtent.UpperRightPoint.X > -180)
			{
				using (Bitmap rightResultImage = GetTileImages(currentExtent, (int)canvas.Width, (int)canvas.Height, ru, vu, uu, maxPixelInZoomLevel, zoomLevelNumber, mapType, pictureFormat))
				{
					RectangleShape r = new RectangleShape(currentExtent.LowerLeftPoint.X + 360, currentExtent.UpperRightPoint.Y, currentExtent.LowerRightPoint.X + 360, currentExtent.LowerRightPoint.Y);
					using (Bitmap leftResultImage = GetTileImages(r, (int)canvas.Width, (int)canvas.Height, ru, vu, uu, maxPixelInZoomLevel, zoomLevelNumber, mapType, pictureFormat))
					{
						double ratio = (-180 - currentExtent.LowerLeftPoint.X) / (currentExtent.LowerRightPoint.X - currentExtent.LowerLeftPoint.X);
					
						using (Bitmap resultImage = new Bitmap((int)canvas.Width, (int)canvas.Height))
						{
							using (Graphics graphics = Graphics.FromImage(resultImage))
							{
								Rectangle leftRectangle = new Rectangle(0, 0, (int)Math.Round(ratio * canvas.Width, 0), (int)canvas.Height);
								Rectangle rightRectangle = new Rectangle(leftRectangle.Width, 0, (int)(canvas.Width - leftRectangle.Width), (int)canvas.Height);
								graphics.DrawImage(leftResultImage, leftRectangle, leftRectangle, GraphicsUnit.Pixel);
								graphics.DrawImage(rightResultImage, rightRectangle, rightRectangle, GraphicsUnit.Pixel);
							}
							ResponseProcessor.ProcessResponseMultiTile(canvas, resultImage);
						}
					}
				}
			}
			else
			{
				using (Bitmap resultImage = GetTileImages(currentExtent, (int)canvas.Width, (int)canvas.Height, ru, vu, uu, maxPixelInZoomLevel, zoomLevelNumber, mapType, pictureFormat))
				{
					ResponseProcessor.ProcessResponseMultiTile(canvas, resultImage);
				}
			}
		}

		public Bitmap GetGoogleMapsImage(string requestString)
		{
			Bitmap image = null;
			try
			{
				Uri requestUri = new Uri(requestString);
				WebRequest request = RequestBuilder.CreateWebRequest(requestUri);
				using (Stream imageStream = request.GetResponse().GetResponseStream())
				{
					image = new Bitmap(imageStream);
				}
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

		public Dictionary<int, Bitmap> GetImagesArray(int startTileX, int startTileY, int endTileX, int endTileY, double ru, double vu, double uu, int zoomLevelNumber, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			Dictionary<int, Bitmap> _images = new Dictionary<int, Bitmap>();
			int tileImageId = 0;
			for (int j = startTileY; j < endTileY; j++)
			{
				for (int i = startTileX; i < endTileX; i++)
				{
					int tileX = i;
					int tileY = j;
					double pixelX = tileX * TileWidth + TileWidth * 0.5 - 1;
					double pixelY = tileY * TileHeight + TileHeight * 0.5 - 1;
					double latitude = GoogleMapAdapter.FromGooglePixelToLatitude(pixelY, ru, vu);
					double longitude = GoogleMapAdapter.FromGooglePixelToLongitude(pixelX, ru, uu);

					Bitmap image = null;
					if (image == null)
					{
						string requestString = RequestBuilder.GetRequestImageUrl(longitude, latitude, zoomLevelNumber, TileWidth, TileHeight, mapType, pictureFormat);
						image = GetGoogleMapsImage(requestString);
					}

					tileImageId++;
				}
			}
			return _images;
		}

		public Bitmap GetBufferImage(int imageWidth, int imageHeight, int startTileX, int startTileY, int endTileX, int endTileY, double ru, double vu, double uu, int zoomLevelNumber, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			Dictionary<int, Bitmap> images = GetImagesArray(startTileX, startTileY, endTileX, endTileY, ru, vu, uu, zoomLevelNumber, mapType, pictureFormat);

			Bitmap bufferImage = new Bitmap(imageWidth, imageHeight);
			Graphics bufferGraphics = Graphics.FromImage(bufferImage);
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
			images.Clear();
			return bufferImage;
		}

		public Bitmap GetTileImages(RectangleShape renderExtent, int canvasWidth, int canvasHeight, double ru, double vu, double uu, int maxPixel, int zoomLevelNumber, GoogleMapsMapType mapType, GoogleMapsPictureFormat pictureFormat)
		{
			PointShape renderExtentCenter = renderExtent.GetCenterPoint();

			double centerPixelX = GoogleMapAdapter.FromLongitudetoGooglePixel(renderExtentCenter.X, ru, uu);
			double centerPixelY = GoogleMapAdapter.FromLatitudeToGooglePixel(renderExtentCenter.Y, ru, vu);
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
			maxPixelX = Math.Min(maxPixelX, maxPixel);
			maxPixelY = Math.Min(maxPixelY, maxPixel);

			int startTileX = (int)Math.Floor(minPixelX / (double)TileWidth);
			int startTileY = (int)Math.Floor(minPixelY / (double)TileHeight);
			int endTileX = (int)Math.Ceiling(maxPixelX / (double)TileWidth);
			int endTileY = (int)Math.Ceiling(maxPixelY / (double)TileHeight);
			int imageWidth = (endTileX - startTileX) * TileWidth;
			int imageHeight = (endTileY - startTileY) * TileHeight;

			int pastOffsetX = (int)Math.Round(centerPixelX - canvasWidth * 0.5) - startTileX * TileWidth;
			int pastOffsetY = (int)Math.Round(centerPixelY - canvasHeight * 0.5) - startTileY * TileHeight;

			Bitmap resultImage = new Bitmap(canvasWidth, canvasHeight);
			using (Bitmap bufferImage = GetBufferImage(imageWidth, imageHeight, startTileX, startTileY, endTileX, endTileY, ru, vu, uu, zoomLevelNumber, mapType, pictureFormat))
			{
				using (Graphics resultGraphics = Graphics.FromImage(resultImage))
				{
					resultGraphics.DrawImageUnscaled(bufferImage, -1 * pastOffsetX, -1 * pastOffsetY);
				}
			}

			return resultImage;
		}

	}
}
