using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System.Collections.Concurrent;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    /// <summary>Layer for OpenStreetMap web map service.</summary>
    [Serializable]
    public class OpenStreetMapLayer : BaseLayer
    {
        private const string serverUrl = "http://tile.openstreetmap.org/{0}/{1}/{2}.png";
        private const double divideDpi = 1.0 / 96.0;
        private const double openStreetMapLimitLatitud = 85.0;
        private const int openStreetMapLayerTileHeight = 256;
        private const double inchPerDecimalDegree = 4374754;
        private const int tileHeight = 256;
        private const int tileWidth = 256;

        [NonSerialized]
        private Exception exception;

        private OpenStreetMapLayerImageCache imageCache;

        [NonSerialized]
        private Collection<OpenStreetMapLayerTileInfo> imageInfoUncached;

        [NonSerialized]
        private ConcurrentDictionary<int, Bitmap> images;

        private int maxPixelInZoomLevel;

        private int pastOffsetX;
        private int pastOffsetY;
        private OpenStreetMapLayerPictureFormat pictureFormat;
        private Proj4Projection projection;

        private double ru;
        private int tileImageId;
        private int timeout;
        private double uu;
        private double vu;
        private int zoomlevelNumber;
        private WebProxy webProxy;

        private bool _openLayersTileEnable = false;

        public OpenStreetMapLayer()
            : this(null)
        {
        }

        public OpenStreetMapLayer(WebProxy webProxy)
        {
            imageCache = new OpenStreetMapLayerImageCache();
            imageCache.CacheDirectory = GetTemporaryFolder() + @"\\OpenStreetMapTmpTileCache";
            this.webProxy = webProxy;
            projection = new Proj4Projection();
            projection.InternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();
            projection.ExternalProjectionParametersString = Proj4Projection.GetEpsgParametersString(4326);
            timeout = 100;
            IsVisible = true;
        }

        public OpenStreetMapLayer(WebProxy webProxy, string cacheDirectory)
        {
            imageCache = new OpenStreetMapLayerImageCache();
            imageCache.CacheDirectory = cacheDirectory;
            this.webProxy = webProxy;
            projection = new Proj4Projection();
            projection.InternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();
            projection.ExternalProjectionParametersString = Proj4Projection.GetEpsgParametersString(4326);
            timeout = 100;
            IsVisible = true;
        }

        public OpenStreetMapLayerPictureFormat CachePictureFormat
        {
            get { return pictureFormat; }
            set { pictureFormat = value; }
        }

        public int TimeoutInSeconds
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public string CacheDirectory
        {
            get { return imageCache.CacheDirectory; }
            set { imageCache.CacheDirectory = value; }
        }

        public WebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }

        protected override void OpenCore()
        {
            projection.Open();
            base.OpenCore();
        }

        protected override void CloseCore()
        {
            projection.Close();
            base.CloseCore();
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            if (!_openLayersTileEnable)
            {
                Validators.CheckParameterIsNotNull(canvas, "canvas");
                Validators.CheckOpenStreetMapUnit(canvas.MapUnit, "canvas.MapUnit");

                ProcessMultiTileMode(canvas);
            }
        }

        private void ProcessMultiTileMode(BaseGeoCanvas canvas)
        {
            if (IsCompleteOutOfEarth(canvas.CurrentWorldExtent))
            {
                return;
            }

            tileImageId = 0;
            images = new ConcurrentDictionary<int, Bitmap>();
            imageInfoUncached = new Collection<OpenStreetMapLayerTileInfo>();
            zoomlevelNumber = GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit, canvas.Dpi);
            vu = openStreetMapLayerTileHeight * Math.Pow(2.0, zoomlevelNumber) / (2.0 * Math.PI);
            ru = openStreetMapLayerTileHeight * Math.Pow(2.0, zoomlevelNumber) / (2.0);
            uu = openStreetMapLayerTileHeight * Math.Pow(2.0, zoomlevelNumber) / (360.0);
            maxPixelInZoomLevel = (int)Math.Round(openStreetMapLayerTileHeight * Math.Pow(2.0, zoomlevelNumber));

            int renderWidth = (int)canvas.Width;
            int renderHeight = (int)canvas.Height;
            RectangleShape currentExtent = GetExtentInDecimalDegree(canvas);
            RectangleShape renderExtent = GetRenderExtent(currentExtent, ref renderHeight);

            var resultImage = GetTileImages(renderExtent, renderWidth, renderHeight);

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
                var geoImage = new GeoImage(stream);
                canvas.DrawScreenImageWithoutScaling(geoImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
            }

            if (exception != null)
            {
                Exception tempException = exception;
                exception = null;
                throw tempException;
            }
        }

        private bool IsCompleteOutOfEarth(RectangleShape utmExtent)
        {
            PointShape upperLeftPoint = (PointShape)projection.ConvertToInternalProjection(new PointShape(-180, 90));
            PointShape lowerRightPoint = (PointShape)projection.ConvertToInternalProjection(new PointShape(180, -90));

            if (utmExtent.UpperLeftPoint.X > lowerRightPoint.X ||
                utmExtent.LowerRightPoint.X < upperLeftPoint.X ||
                utmExtent.UpperLeftPoint.Y < lowerRightPoint.Y ||
                utmExtent.LowerRightPoint.Y > upperLeftPoint.Y)
            {
                return true;
            }
            return false;
        }

        private RectangleShape GetExtentInDecimalDegree(BaseGeoCanvas canvas)
        {
            PointShape centerPoint = GetCenterPointInDecimalDegree(canvas);
            float width = canvas.Width;
            float height = canvas.Height;

            OpenStreetMapsZoomLevelSet set = new OpenStreetMapsZoomLevelSet();
            RectangleShape newExtent = ExtentHelper.SnapToZoomLevel(canvas.CurrentWorldExtent, GeographyUnit.Meter, width, height, set);
            double currentScale = ExtentHelper.GetScale(newExtent, width, GeographyUnit.Meter, canvas.Dpi);
            double extentWidth = currentScale * width * divideDpi / inchPerDecimalDegree;
            double extentHeight = extentWidth * height / width;
            RectangleShape resultExtent = new RectangleShape();
            resultExtent.UpperLeftPoint.X = centerPoint.X - extentWidth * 0.5;
            resultExtent.UpperLeftPoint.Y = centerPoint.Y + extentHeight * 0.5;
            resultExtent.LowerRightPoint.X = centerPoint.X + extentWidth * 0.5;
            resultExtent.LowerRightPoint.Y = centerPoint.Y - extentHeight * 0.5;

            return resultExtent;
        }

        private static RectangleShape GetRenderExtent(RectangleShape fittedExtent, ref int expectHeight)
        {
            RectangleShape renderExtent = (RectangleShape)fittedExtent.CloneDeep();
            double extentCenterY = renderExtent.GetCenterPoint().Y;
            if (extentCenterY > openStreetMapLimitLatitud || extentCenterY < -openStreetMapLimitLatitud)
            {
                if (extentCenterY > openStreetMapLimitLatitud)
                {
                    renderExtent.UpperLeftPoint.Y = openStreetMapLimitLatitud + (openStreetMapLimitLatitud - renderExtent.LowerRightPoint.Y);
                    expectHeight = (int)(renderExtent.Height * expectHeight / fittedExtent.Height);
                }
                else if (extentCenterY < -openStreetMapLimitLatitud)
                {
                    renderExtent.LowerRightPoint.Y = -openStreetMapLimitLatitud - (renderExtent.UpperLeftPoint.Y + openStreetMapLimitLatitud);
                    expectHeight = (int)(renderExtent.Height * expectHeight / fittedExtent.Height);
                }
            }

            return renderExtent;
        }

        private Bitmap GetTileImages(RectangleShape renderExtent, int canvasWidth, int canvasHeight)
        {
            double renderExtentCenterLatitude = GetCenterY(renderExtent);
            double renderExtentCenterLongitude = GetCenterX(renderExtent);

            double centerPixelX = FromLongitudetoGooglePixel(renderExtentCenterLongitude);
            double centerPixelY = FromLatitudeToGooglePixel(renderExtentCenterLatitude);
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

            int startTileX = (int)Math.Floor(minPixelX / (double)tileWidth);
            int startTileY = (int)Math.Floor(minPixelY / (double)tileHeight);
            int endTileX = (int)Math.Ceiling(maxPixelX / (double)tileWidth);
            int endTileY = (int)Math.Ceiling(maxPixelY / (double)tileHeight);
            int imageWidth = (endTileX - startTileX) * tileWidth;
            int imageHeight = (endTileY - startTileY) * tileHeight;

            pastOffsetX = (int)Math.Round(centerPixelX - canvasWidth * 0.5) - startTileX * tileWidth;
            pastOffsetY = (int)Math.Round(centerPixelY - canvasHeight * 0.5) - startTileY * tileHeight;

            Bitmap bufferImage = GetBufferImage(imageWidth, imageHeight, startTileX, startTileY, endTileX, endTileY);
            Bitmap resultImage = new Bitmap(canvasWidth, canvasHeight);
            Graphics resultGraphics = Graphics.FromImage(resultImage);
            resultGraphics.DrawImageUnscaled(bufferImage, -pastOffsetX, -pastOffsetY);
            resultGraphics.Dispose();
            bufferImage.Dispose();
            return resultImage;
        }

        private Bitmap GetBufferImage(int imageWidth, int imageHeight, int startTileX, int startTileY, int endTileX, int endTileY)
        {
            GetImagesArray(startTileX, startTileY, endTileX, endTileY);

            CacheSave();

            Bitmap bufferImage = new Bitmap(imageWidth, imageHeight);
            Graphics bufferGraphics = Graphics.FromImage(bufferImage);
            int tileCountInRow = endTileX - startTileX;
            for (int j = startTileY; j < endTileY; j++)
            {
                for (int i = startTileX; i < endTileX; i++)
                {
                    int imageIndex = (j - startTileY) * tileCountInRow + (i - startTileX);
                    Bitmap tileImage = images[imageIndex];
                    int tempPositionX = (i - startTileX) * tileWidth;
                    int tempPositionY = (j - startTileY) * tileHeight;
                    bufferGraphics.DrawImageUnscaled(tileImage, tempPositionX, tempPositionY);
                    tileImage.Dispose();
                }
            }
            images.Clear();
            return bufferImage;
        }

        private void CacheSave()
        {
            if (imageCache.IsCacheEnabled)
            {
                foreach (OpenStreetMapLayerTileInfo tileInfo in imageInfoUncached)
                {
                    if (images[tileInfo.ImageId].Width != 1)
                    {
                        imageCache.SaveCacheImage(images[tileInfo.ImageId], zoomlevelNumber, tileInfo.TileX, tileInfo.TileY, pictureFormat);
                    }
                }
            }
            imageInfoUncached.Clear();
        }

        private ConcurrentDictionary<int, Bitmap> GetImagesArray(int startTileX, int startTileY, int endTileX, int endTileY)
        {
            Collection<Thread> threads = new Collection<Thread>();

            for (int j = startTileY; j < endTileY; j++)
            {
                for (int i = startTileX; i < endTileX; i++)
                {
                    int tileX = i;
                    int tileY = j;
                  
                    Bitmap image = null;
                    if (imageCache.IsCacheEnabled)
                    {
                        image = imageCache.GetCacheImage(zoomlevelNumber, tileX, tileY, pictureFormat);
                        if (image != null)
                        {
                            images.TryAdd(tileImageId, image);
                        }
                    }

                    if (image == null)
                    {
                        string requestString = String.Format(CultureInfo.InvariantCulture, serverUrl, zoomlevelNumber, tileX, tileY);
                        Thread thread = new Thread(FetchImageInThread);
                        thread.Start(new object[] { tileImageId, requestString });
                        threads.Add(thread);
                        imageInfoUncached.Add(new OpenStreetMapLayerTileInfo(tileImageId, tileX, tileY));
                    }

                    tileImageId++;
                }
            }

            foreach (Thread item in threads)
            {
                item.Join();
            }

            return images;
        }

       
        private void FetchImageInThread(object obj)
        {
            object[] objects = obj as object[];
            int imageId = (int)objects[0];
            string requestString = objects[1] as string;

            WebRequest request = HttpWebRequest.Create(requestString);
            request.Timeout = timeout * 1000;
            request.Proxy = webProxy;

            try
            {
                using (Stream imageStream = request.GetResponse().GetResponseStream())
                {
                    Bitmap image = new Bitmap(imageStream);
                    if (image == null)
                    {
                        image = new Bitmap(1, 1);
                    }

                    images.TryAdd(imageId, image);
                }
                    
            }
            catch (Exception ex)
            {
                images.TryAdd(imageId, new Bitmap(1, 1));

                if (exception == null)
                {
                    exception = ex;
                }
            }
        }

        

        private static int GetCurrentZoomLevelNumber(double newWidth, RectangleShape newTileExtent, GeographyUnit mapUnit, float dpi)
        {
            OpenStreetMapsZoomLevelSet zoomlevelSet = new OpenStreetMapsZoomLevelSet();
            ZoomLevel zoomlevel = zoomlevelSet.GetZoomLevel(newTileExtent, newWidth, mapUnit, dpi);
            Collection<ZoomLevel> zoomlevels = zoomlevelSet.GetZoomLevels();
            int zoomlevelNumber = 0;
            for (int i = 0; i < zoomlevels.Count; i++)
            {
                if (zoomlevel.Scale == zoomlevels[i].Scale)
                {
                    zoomlevelNumber = i;
                    break;
                }
            }
            return zoomlevelNumber;
        }

     
        private double FromLatitudeToGooglePixel(double latitude)
        {
            double f = Math.Sin(latitude * (Math.PI / 180.0));
            f = Math.Max(f, -0.9999);
            f = Math.Min(f, 0.9999);
            return ru + (0.5 * Math.Log((1 + f) / (1 - f)) * (-vu));
        }

        private double FromGooglePixelToLatitude(double yPixel)
        {
            double g = (yPixel - ru) / (-vu);
            return (2.0 * Math.Atan(Math.Exp(g)) - (Math.PI / 2.0)) / (Math.PI / 180.0);
        }

        private double FromLongitudetoGooglePixel(double longitude)
        {
            return ru + (longitude * uu);
        }

        private double FromGooglePixelToLongitude(double xPixel)
        {
            return (xPixel - ru) / uu;
        }

        private static double GetCenterX(RectangleShape renderExtent)
        {
            return (renderExtent.UpperLeftPoint.X + renderExtent.LowerRightPoint.X) * 0.5;
        }

        private static double GetCenterY(RectangleShape renderExtent)
        {
            return (renderExtent.UpperLeftPoint.Y + renderExtent.LowerRightPoint.Y) * 0.5;
        }

        private PointShape GetCenterPointInDecimalDegree(BaseGeoCanvas canvas)
        {
            PointShape utmCenterPoint = canvas.CurrentWorldExtent.GetCenterPoint();
            PointShape result = (PointShape)(projection.ConvertToExternalProjection(utmCenterPoint));

            if (result.X < 0 && utmCenterPoint.X > 0)
            {
                while (result.X < 0)
                {
                    result.X += 360;
                }
            }

            if (result.X > 0 && utmCenterPoint.X < 0)
            {
                while (result.X > 0)
                {
                    result.X -= 360;
                }
            }

            if (result.Y > 0 && utmCenterPoint.Y < 0)
            {
                while (result.Y > 0)
                {
                    result.Y -= 180;
                }
            }

            if (result.Y < 0 && utmCenterPoint.Y > 0)
            {
                while (result.Y < 0)
                {
                    result.Y += 180;
                }
            }

            return result;
        }



        private static string GetTemporaryFolder()
        {
            string returnValue = string.Empty;
            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = Environment.GetEnvironmentVariable("Temp");
            }

            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = Environment.GetEnvironmentVariable("Tmp");
            }

            if (string.IsNullOrEmpty(returnValue))
            {
                returnValue = @"c:\GSuiteTemp";
            }
            else
            {
                returnValue = returnValue + "\\" + "GSuite";
            }

            return returnValue;
        }
    }
}