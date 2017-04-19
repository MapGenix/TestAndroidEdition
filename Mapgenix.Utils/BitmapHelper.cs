using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Mapgenix.FeatureSource.Filters
{
	public static class BitmapHelper
	{
		public static Bitmap ResizeImage(Bitmap bitmap, int newWidth, int newHeight)
		{
			Bitmap b = new Bitmap(newWidth, newHeight);
			using (Graphics g = Graphics.FromImage(b))
			{
				g.DrawImage(bitmap, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);

			}
			return b;
		}

		public static Bitmap StretchImage(Bitmap sourceBitmap, Rectangle sourceRectangle, int tempReturnWidth, int tempReturnHeight)
		{
			Bitmap returnImage = new Bitmap(tempReturnWidth, tempReturnHeight);

			Point[] targetImagePoints = new Point[3];
			targetImagePoints[0] = new Point(0, 0);
			targetImagePoints[1] = new Point(tempReturnWidth, 0);
			targetImagePoints[2] = new Point(0, tempReturnHeight);

			using(Graphics graphics = Graphics.FromImage(returnImage))
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.DrawImage(sourceBitmap, targetImagePoints, sourceRectangle, GraphicsUnit.Pixel);
			}
			return returnImage;
		}

		public static Stream GetStreamFromBitmap(Bitmap bitmap)
		{
			Stream returnStream = new MemoryStream();
			bitmap.Save(returnStream, ImageFormat.Png);
			returnStream.Seek(0, SeekOrigin.Begin);

			return returnStream;
		}

	}
}
