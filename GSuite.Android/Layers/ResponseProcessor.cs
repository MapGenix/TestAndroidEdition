using System.IO;
using Mapgenix.Canvas;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    public static class ResponseProcessor
    {

        public static void ProcessResponse(BaseGeoCanvas canvas, Bitmap image)
        {
            GdiPlusAndroidGeoCanvas gdiplusCanvas = canvas as GdiPlusAndroidGeoCanvas;
            if (gdiplusCanvas != null)
            {
                gdiplusCanvas.DrawScreenImageWithoutScaling(image, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                image.Compress(Bitmap.CompressFormat.Png,0, stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (GeoImage geoImage = new GeoImage(stream))
                {
                    canvas.DrawScreenImageWithoutScaling(geoImage, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
            }
        }


        public static void ProcessResponseMultiTile(BaseGeoCanvas canvas, Bitmap resultImage)
        {
            GdiPlusAndroidGeoCanvas gdiplusCanvas = canvas as GdiPlusAndroidGeoCanvas;
            if (gdiplusCanvas != null)
            {
                gdiplusCanvas.DrawScreenImageWithoutScaling(resultImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                resultImage.Compress(Bitmap.CompressFormat.Png, 0, stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (GeoImage geoImage = new GeoImage(stream))
                {
                    canvas.DrawScreenImageWithoutScaling(geoImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
            }
        }
    }
}
