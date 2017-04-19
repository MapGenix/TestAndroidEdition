using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    public static class ResponseProcessor
	{

		public static void ProcessResponse(BaseGeoCanvas canvas, Bitmap image)
		{
			GdiPlusGeoCanvas gdiplusCanvas = canvas as GdiPlusGeoCanvas;
			if (gdiplusCanvas != null)
			{
				gdiplusCanvas.DrawScreenImageWithoutScaling(image, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
			}
			else
			{
				MemoryStream stream = new MemoryStream();
				image.Save(stream, ImageFormat.Png);
				stream.Seek(0, SeekOrigin.Begin);
				using (GeoImage geoImage = new GeoImage(stream))
				{
					canvas.DrawScreenImageWithoutScaling(geoImage, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
				}
			}
		}

		
		public static void ProcessResponseMultiTile(BaseGeoCanvas canvas, Bitmap resultImage)
		{
			GdiPlusGeoCanvas gdiplusCanvas = canvas as GdiPlusGeoCanvas;
			if (gdiplusCanvas != null)
			{
				gdiplusCanvas.DrawScreenImageWithoutScaling(resultImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
			}
			else
			{
				MemoryStream stream = new MemoryStream();
				resultImage.Save(stream, ImageFormat.Png);
				stream.Seek(0, SeekOrigin.Begin);
				using (GeoImage geoImage = new GeoImage(stream))
				{
					canvas.DrawScreenImageWithoutScaling(geoImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
				}
			}
		}
	}
}
