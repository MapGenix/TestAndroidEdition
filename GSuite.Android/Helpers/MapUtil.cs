using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    internal static class MapUtil
    {
        private const int DotsPerInch = 96;
        public const double Feet = 12.0;
        public const double Meter = 39.3701;
        public const double DecimalDegree = 4374754;

        public static double GetResolutionFromScale(double scale, GeographyUnit unit)
        {
            return scale / (GetInchesByUnit(unit) * DotsPerInch);
        }

        public static double GetResolutionFromScale(double scale, GeographyUnit unit, int dpi)
        {
            return scale / (GetInchesByUnit(unit) * dpi);
        }

        public static double GetScaleFromResolution(double resolution, GeographyUnit unit)
        {
            return resolution * (GetInchesByUnit(unit) * DotsPerInch);
        }

        public static double GetScaleFromResolution(double resolution, GeographyUnit unit, int dpi)
        {
            return resolution * (GetInchesByUnit(unit) * dpi);
        }

        public static RectangleShape CalculateExtent(PointF center, double scale, GeographyUnit mapUnit, double mapWidth, double mapHeight)
        {
            if (Double.IsNaN(mapWidth) || Double.IsNaN(mapHeight))
            {
                return null;
            }

            double resolution = GetResolutionFromScale(scale, mapUnit);
            double widthInDegree = mapWidth * resolution;
            double heightInDegree = mapHeight * resolution;
            double left = center.X - widthInDegree * .5;
            double right = center.X + widthInDegree * .5;
            double top = center.Y + heightInDegree * .5;
            double bottom = center.Y - heightInDegree * .5;
            return new RectangleShape(left, top, right, bottom);
        }

        public static RectangleShape CalculateExtent(PointF center, double scale, GeographyUnit mapUnit, double mapWidth, double mapHeight, int dpi)
        {
            if (Double.IsNaN(mapWidth) || Double.IsNaN(mapHeight))
            {
                return null;
            }

            double resolution = GetResolutionFromScale(scale, mapUnit, 96);
            double widthInDegree = mapWidth * resolution;
            double heightInDegree = mapHeight * resolution;
            double left = center.X - widthInDegree * .5;
            double right = center.X + widthInDegree * .5;
            double top = center.Y + heightInDegree * .5;
            double bottom = center.Y - heightInDegree * .5;
            return new RectangleShape(left, top, right, bottom);
        }

        public static RectangleShape GetDefaultMaxExtent(GeographyUnit mapUnit)
        {
            RectangleShape maxExtent = new RectangleShape();
            switch (mapUnit)
            {
                case GeographyUnit.DecimalDegree: 
                    maxExtent = new RectangleShape(-180, 90, 180, -90);
                    break;
             
                case GeographyUnit.Meter:
                    FileNativeImageTileCache meterCache = new FileNativeImageTileCache();
                    meterCache.TileMatrix.BoundingBoxUnit = GeographyUnit.Meter;
                    meterCache.TileMatrix.BoundingBox = new RectangleShape(-1000000000, 1000000000, 1000000000, -1000000000);
                    maxExtent = meterCache.TileMatrix.BoundingBox;
                    break;
                case GeographyUnit.Feet:
                    FileNativeImageTileCache feetCache = new FileNativeImageTileCache();
                    feetCache.TileMatrix.BoundingBoxUnit = GeographyUnit.Feet;
                    feetCache.TileMatrix.BoundingBox = new RectangleShape(-1000000000, 1000000000, 1000000000, -1000000000);
                    maxExtent = feetCache.TileMatrix.BoundingBox;
                    break;
                default:
                    break;
            }
            return maxExtent;
        }

        public static double GetResolution(RectangleShape boundingBox, double widthInPixel, double heightInPixel)
        {
            return Math.Max(boundingBox.Width / widthInPixel, boundingBox.Height / heightInPixel);
        }

        public static double GetScale(GeographyUnit mapUnit, RectangleShape boundingBox, double widthInPixel, double heightInPixel)
        {
            double resolution = GetResolution(boundingBox, widthInPixel, heightInPixel);
            return GetScaleFromResolution(resolution, mapUnit);
        }

        public static PointF ToScreenCoordinate(RectangleShape currentExtent, double worldX, double worldY, double actualWidth, double actualHeight)
        {
            double widthFactor = actualWidth / currentExtent.Width;
            double heighFactor = actualHeight / currentExtent.Height;

            double pointX = (worldX - currentExtent.UpperLeftPoint.X) * widthFactor;
            double pointY = (currentExtent.UpperLeftPoint.Y - worldY) * heighFactor;

            return new PointF(Convert.ToSingle(pointX), Convert.ToSingle(pointY));
        }

        public static PointF ToWorldCoordinate(RectangleShape currentExtent, double screenX, double screenY, double screenWidth, double screenHeight)
        {
            double widthFactor = currentExtent.Width / screenWidth;
            double heightFactor = currentExtent.Height / screenHeight;

            double pointX = currentExtent.UpperLeftPoint.X + screenX * widthFactor;
            double pointY = currentExtent.UpperLeftPoint.Y - screenY * heightFactor;

            return new PointF(Convert.ToSingle(pointX), Convert.ToSingle(pointY));
        }

        public static double GetInchesByUnit(GeographyUnit unit)
        {
            switch (unit)
            {
                case GeographyUnit.Feet: return Feet;
                case GeographyUnit.Meter: return Meter;
                case GeographyUnit.DecimalDegree: return DecimalDegree;
                default: return double.NaN;
            }
        }

        public static double GetDistance(PointShape fromPoint, PointShape toPoint)
        {
            double horizenDistance = Math.Abs((fromPoint.X - toPoint.X));
            double verticalDistance = Math.Abs((fromPoint.Y - toPoint.Y));

            double result = Math.Sqrt(Math.Pow(horizenDistance, 2) + Math.Pow(verticalDistance, 2));

            return result;
        }

        internal static string GetBBoxString(RectangleShape rectangle)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", rectangle.LowerLeftPoint.X, rectangle.LowerLeftPoint.Y, rectangle.UpperRightPoint.X, rectangle.UpperRightPoint.Y);
        }

        internal static bool IsFuzzyEqual(double source, double target)
        {
            return Math.Round(source, 6) == Math.Round(target, 6);
        }

        internal static int GetSnappedZoomLevelIndex(RectangleShape extent, GeographyUnit mapUnit, Collection<double> zoomLevelScales, double actualWidth, double actualHeight)
        {
            double scale = GetScale(mapUnit, extent, actualWidth, actualHeight);
            return GetSnappedZoomLevelIndex(scale, zoomLevelScales);
        }

        internal static int GetSnappedZoomLevelIndex(double scale, Collection<double> zoomLevelScales)
        {
            return GetSnappedZoomLevelIndex(scale, zoomLevelScales, double.MinValue, double.MaxValue);
        }

        internal static int GetSnappedZoomLevelIndex(double scale, Collection<double> zoomLevelScales, double minimumScale, double maximumScale)
        {
            if (scale < minimumScale)
            {
                scale = minimumScale;
            }
            else if (scale > maximumScale)
            {
                scale = maximumScale;
            }

            int zoomLevel = -1;

            if (zoomLevelScales.Count > 0)
            {
                foreach (double tempScale in zoomLevelScales)
                {
                    if (tempScale >= scale || Math.Abs(tempScale - scale) < 0.1)
                    {
                        zoomLevel++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (zoomLevel >= zoomLevelScales.Count)
            {
                zoomLevel = zoomLevelScales.Count - 1;
            }

            return zoomLevel == -1 ? 0 : zoomLevel;
        }

        
        internal static object GetImageSourceFromNativeImage(object nativeImage)
        {
            object imageSource = nativeImage;
            if (nativeImage is System.Drawing.Bitmap)
            {
                System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)nativeImage;
                MemoryStream memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                imageSource = memoryStream;
                bitmap.Dispose();
            }

            return imageSource;
        }
    }
}
