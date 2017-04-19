using Mapgenix.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using System.Drawing;
using System.Net;
using System.Collections.Concurrent;
using System.IO;
using Mapgenix.Styles;
using System.Drawing.Imaging;
using System.Threading;

namespace Mapgenix.Layers
{
    /// <summary>Layer for Bing Maps</summary>
    public class BingMapsLayer : BaseLayer
    {

        private const double DIVIDE_DPI = 1.0 / 96.0;
        private const double INCH_PER_DECIAML_DEGREE = 4374754;
        private const int TILE_HEIGHT = 256;
        private const int TILE_WIDTH = 256;

        private BingMapsMapType _mapType;
        private BingMapsPictureFormat _pictureFormat;
        private BingMapsLayerImageCache _imageCache = new BingMapsLayerImageCache();
        private int _tileImageId;
        private int _maxPixelInZoomLevel;
        private double _uu;
        private double _vu;
        private double _ru;
        private int _pastOffsetX;
        private int _pastOffsetY;
        private int _zoomlevelNumber;
        private BingMapsTileMode _tileMode;
        [NonSerialized]
        private Collection<HEREMapsLayerTileInfo> _imageInfoUncached;

        [NonSerialized]
        private ConcurrentDictionary<int, Bitmap> _images;

        private bool _openLayersTileEnable = false;

        public BingMapsLayer() : 
            this(String.Empty, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null)
        { }
        public BingMapsLayer(string licenseKey) :
            this(licenseKey, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null)
        { }
        public BingMapsLayer(string licenseKey, BingMapsMapType MapType) : 
            this(licenseKey, MapType, String.Empty, BingMapsPictureFormat.Png, null)
        { }
        public BingMapsLayer(string licenseKey, BingMapsMapType MapType, string cacheDirectory) : 
            this(licenseKey, MapType, cacheDirectory, BingMapsPictureFormat.Png, null)
        { }
        public BingMapsLayer(string licenseKey, BingMapsMapType MapType, string cacheDirectory, BingMapsPictureFormat PictureFormat) : 
            this(licenseKey, MapType, cacheDirectory, PictureFormat, null)
        { }
        public BingMapsLayer(string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat, WebProxy webProxy)
        {
            this.IsVisible = true;
            this._mapType = mapType;
            this._tileMode = BingMapsTileMode.MultiTile;
            this._pictureFormat = pictureFormat;
            this.CacheDirectory = (cacheDirectory.Equals(String.Empty)) ? GetTemporaryFolder() + @"\BingMaps\" : cacheDirectory;
            Projection = new Proj4Projection
            {
                InternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString(),
                ExternalProjectionParametersString = Proj4Projection.GetWgs84ParametersString()
            };
            RequestBuilder = new BingMapRequestBuilder
            {
                LicenseKey = licenseKey,
                WebProxy = webProxy
            };
        }

        public BingMapRequestBuilder RequestBuilder { get; set; }

        public Proj4Projection Projection
        {
            set;
            get;
        }

        public BingMapsMapType MapType
        {
            get { return _mapType; }
            set { _mapType = value; }
        }

        public BingMapsPictureFormat PictureFormat
        {
            get { return _pictureFormat; }
            set { _pictureFormat = value; }
        }

        public string LicenseKey
        {
            get
            {
                return RequestBuilder.LicenseKey;
            }
            set
            {
                RequestBuilder.LicenseKey = value;
            }
        }

        public string CacheDirectory
        {
            get
            {
                return _imageCache.CacheDirectory;
            }
            set
            {
                _imageCache.CacheDirectory = value;
            }
        }

        public BingMapsTileMode TileMode
        {
            get
            {
                return _tileMode;
            }
            set
            {
                _tileMode = value;
            }
        }

        public bool IsTrafficFlowEnabled
        {
            get
            {
                return _mapType.HasFlag(BingMapsMapType.Traffic);
            }
        }

        public bool OpenLayersTileEnable
        {
            get { return _openLayersTileEnable; }
            set { _openLayersTileEnable = value; }
        }

        public string MapTypeAsString
        {
            get
            {
                if (_mapType.HasFlag(BingMapsMapType.Aerial))
                    return "aerial";
                if (_mapType.HasFlag(BingMapsMapType.AerialWithLabels))
                    return "aerialWithLabels";
                if (_mapType.HasFlag(BingMapsMapType.Road))
                    return "road";
                else
                    return "road";
            }
        }

        public WebProxy WebProxy
        {
            get
            {
                return RequestBuilder.WebProxy;
            }
            set
            {
                RequestBuilder.WebProxy = value;
            }
        }

        protected override void OpenCore()
        {
            Projection.Open();
        }

        protected override void CloseCore()
        {
            Projection.Close();
        }
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            if (!_openLayersTileEnable)
            {
                Validators.CheckParameterIsNotNull(canvas, "canvas");
                Validators.CheckGoogleMapUnit(canvas.MapUnit, "canvas.MapUnit");

                switch (_tileMode)
                {
                    case BingMapsTileMode.SingleTile:
                        SingleTileDrawCore(canvas, Projection, _pictureFormat);
                        break;

                    default:
                        MultiTileDrawCore(canvas, Projection, _pictureFormat);
                        break;
                }
            }
        }

        private void SingleTileDrawCore(BaseGeoCanvas canvas, BaseProjection projection, BingMapsPictureFormat pictureFormat)
        {
            _zoomlevelNumber = ZoomLevelHelper.GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit);

            PointShape center = (PointShape)projection.ConvertToExternalProjection(canvas.CurrentWorldExtent.GetCenterPoint());

            string rectangleInfoAndSize = RequestBuilder.CreateRectangleInfoAndSize(center, canvas, _zoomlevelNumber);
            string requestString = RequestBuilder.CreateRequestString(rectangleInfoAndSize, pictureFormat, _mapType, IsTrafficFlowEnabled);

            using (Bitmap image = RequestBuilder.FindImage(requestString))
            {
                ResponseProcessor.ProcessResponse(canvas, image);
            }
        }

        private void MultiTileDrawCore(BaseGeoCanvas canvas, BaseProjection projection, BingMapsPictureFormat pictureFormat)
        {
            Bitmap resultImage = null;
            GeoImage geoImage = null;
            try
            {
                _tileImageId = 0;
                _images = new ConcurrentDictionary<int, Bitmap>();
                _imageInfoUncached = new Collection<HEREMapsLayerTileInfo>();

                _zoomlevelNumber = ZoomLevelHelper.GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit, canvas.Dpi);
                _vu = TILE_HEIGHT * Math.Pow(2.0, _zoomlevelNumber) / (2.0 * Math.PI);
                _ru = TILE_HEIGHT * Math.Pow(2.0, _zoomlevelNumber) / (2.0);
                _uu = TILE_HEIGHT * Math.Pow(2.0, _zoomlevelNumber) / (360.0);
                _maxPixelInZoomLevel = (int)Math.Round(TILE_HEIGHT * Math.Pow(2.0, _zoomlevelNumber));


                int renderWidth = (int)canvas.Width;
                int renderHeight = (int)canvas.Height;
                RectangleShape currentExtent = GetExtentInDecimalDegree(canvas);

                resultImage = GetTileImages(currentExtent, (int)canvas.Width, (int)canvas.Height, canvas.Dpi);

                GdiPlusGeoCanvas gdiplusCanvas = canvas as GdiPlusGeoCanvas;
                if (gdiplusCanvas != null)
                {
                    gdiplusCanvas.DrawScreenImageWithoutScaling(resultImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    resultImage.Save(stream, GetMapImageFormat());
                    stream.Seek(0, SeekOrigin.Begin);
                    geoImage = new GeoImage(stream);
                    canvas.DrawScreenImageWithoutScaling(geoImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (resultImage != null)
                {
                    resultImage.Dispose();
                }
                if (geoImage != null)
                {
                    geoImage.Dispose();
                }
            }
        }

        private BingMapsMapType GetMapType()
        {
            if (_mapType.HasFlag(BingMapsMapType.Aerial))
                return BingMapsMapType.Aerial;
            if (_mapType.HasFlag(BingMapsMapType.AerialWithLabels))
                return BingMapsMapType.AerialWithLabels;
            if (_mapType.HasFlag(BingMapsMapType.Road))
                return BingMapsMapType.Road;
            else
                return BingMapsMapType.Road;
        }

        private ImageFormat GetMapImageFormat()
        {
            switch (_pictureFormat)
            {
                case BingMapsPictureFormat.Png:
                    return ImageFormat.Png;

                case BingMapsPictureFormat.Gif:
                    return ImageFormat.Gif;

                case BingMapsPictureFormat.Jpeg:
                    return ImageFormat.Jpeg;

                default:
                    return ImageFormat.Png;
            }
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

        private RectangleShape GetExtentInDecimalDegree(BaseGeoCanvas canvas)
        {
            PointShape centerPoint = GetCenterPointInDecimalDegree(canvas);
            float width = canvas.Width;
            float height = canvas.Height;

            BingMapsMapZoomLevel set = new BingMapsMapZoomLevel();
            RectangleShape newExtent = ExtentHelper.SnapToZoomLevel(canvas.CurrentWorldExtent, GeographyUnit.Meter, width, height, set);
            double currentScale = ExtentHelper.GetScale(newExtent, width, GeographyUnit.Meter, canvas.Dpi);
            double extentWidth = currentScale * width * DIVIDE_DPI / INCH_PER_DECIAML_DEGREE;
            double extentHeight = extentWidth * height / width;
            RectangleShape resultExtent = new RectangleShape();
            resultExtent.UpperLeftPoint.X = centerPoint.X - extentWidth * 0.5;
            resultExtent.UpperLeftPoint.Y = centerPoint.Y + extentHeight * 0.5;
            resultExtent.LowerRightPoint.X = centerPoint.X + extentWidth * 0.5;
            resultExtent.LowerRightPoint.Y = centerPoint.Y - extentHeight * 0.5;

            return resultExtent;
        }

        private PointShape GetCenterPointInDecimalDegree(BaseGeoCanvas canvas)
        {
            PointShape utmCenterPoint = canvas.CurrentWorldExtent.GetCenterPoint();
            PointShape result = (PointShape)(Projection.ConvertToExternalProjection(utmCenterPoint));

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

        private Bitmap GetTileImages(RectangleShape renderExtent, int canvasWidth, int canvasHeight, float dpi)
        {
            PointShape renderExtentCenter = renderExtent.GetCenterPoint();

            double centerPixelX = FromLongitudetoHerePixel(renderExtentCenter.X);
            double centerPixelY = FromLatitudeToHerePixel(renderExtentCenter.Y);
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
            maxPixelX = Math.Min(maxPixelX, _maxPixelInZoomLevel);
            maxPixelY = Math.Min(maxPixelY, _maxPixelInZoomLevel);

            int startTileX = (int)Math.Floor(minPixelX / (double)TILE_WIDTH);
            int startTileY = (int)Math.Floor(minPixelY / (double)TILE_HEIGHT);
            int endTileX = (int)Math.Ceiling(maxPixelX / (double)TILE_WIDTH);
            int endTileY = (int)Math.Ceiling(maxPixelY / (double)TILE_HEIGHT);
            int imageWidth = (endTileX - startTileX) * TILE_WIDTH;
            int imageHeight = (endTileY - startTileY) * TILE_HEIGHT;

            _pastOffsetX = (int)Math.Round(centerPixelX - canvasWidth * 0.5) - startTileX * TILE_WIDTH;
            _pastOffsetY = (int)Math.Round(centerPixelY - canvasHeight * 0.5) - startTileY * TILE_HEIGHT;

            Bitmap bufferImage = GetBufferImage(imageWidth, imageHeight, startTileX, startTileY, endTileX, endTileY);
            Bitmap resultImage = new Bitmap(canvasWidth, canvasHeight);
            Graphics resultGraphics = Graphics.FromImage(resultImage);
            resultGraphics.DrawImageUnscaled(bufferImage, -_pastOffsetX, -_pastOffsetY);
            resultGraphics.Dispose();
            bufferImage.Dispose();
            return resultImage;
        }

        private Bitmap GetBufferImage(int imageWidth, int imageHeight, int startTileX, int startTileY, int endTileX, int endTileY)
        {
            GetImagesArray(startTileX, startTileY, endTileX, endTileY);
            WaitUntilGetAllImage(_images, _tileImageId);
            CacheSave();

            Bitmap bufferImage = new Bitmap(imageWidth, imageHeight);
            Graphics bufferGraphics = Graphics.FromImage(bufferImage);
            int tileCountInRow = endTileX - startTileX;
            for (int j = startTileY; j < endTileY; j++)
            {
                for (int i = startTileX; i < endTileX; i++)
                {
                    int imageIndex = (j - startTileY) * tileCountInRow + (i - startTileX);
                    Bitmap tileImage = _images[imageIndex];
                    int tempPositionX = (i - startTileX) * TILE_WIDTH;
                    int tempPositionY = (j - startTileY) * TILE_HEIGHT;
                    bufferGraphics.DrawImageUnscaled(tileImage, tempPositionX, tempPositionY);
                    tileImage.Dispose();
                }
            }
            _images.Clear();
            return bufferImage;
        }

        private void CacheSave()
        {
            if (_imageCache.IsCacheEnabled && !IsTrafficFlowEnabled)
            {
                foreach (HEREMapsLayerTileInfo tileInfo in _imageInfoUncached)
                {
                    if (_images[tileInfo.ImageId].Width != 1)
                    {
                        _imageCache.SaveCacheImage(_images[tileInfo.ImageId], _zoomlevelNumber, tileInfo.TileX, tileInfo.TileY, _pictureFormat, MapTypeAsString);
                    }
                }
            }
            _imageInfoUncached.Clear();
        }

        private ConcurrentDictionary<int, Bitmap> GetImagesArray(int startTileX, int startTileY, int endTileX, int endTileY)
        {
            Collection<Thread> threads = new Collection<Thread>();

            for (int j = startTileY; j < endTileY; j++)
            {
                for (int i = startTileX; i < endTileX; i++)
                {
                    if (threads.Count >= 2)
                    {
                        foreach (Thread item in threads)
                        {
                            item.Join();
                        }
                        threads.Clear();
                    }

                    int tileX = i;
                    int tileY = j;
                    double pixelX = tileX * TILE_WIDTH + TILE_WIDTH * 0.5 - 1;
                    double pixelY = tileY * TILE_HEIGHT + TILE_HEIGHT * 0.5 - 1;
                    double latitude = FromHerePixelToLatitude(pixelY);
                    double longitude = FromHerePixelToLongitude(pixelX);

                    Bitmap image = null;
                    if (_imageCache.IsCacheEnabled && !IsTrafficFlowEnabled)
                    {
                        image = _imageCache.GetCacheImage(_zoomlevelNumber, tileX, tileY, _pictureFormat, MapTypeAsString);
                        if (image != null)
                        {
                            _images.TryAdd(_tileImageId, image);
                        }
                    }

                    if (image == null)
                    {
                        string rectangleInfoAndSize = RequestBuilder.CreateRectangleInfoAndSize(longitude, latitude, TILE_WIDTH, TILE_HEIGHT, _zoomlevelNumber);
                        string requestString = RequestBuilder.CreateRequestString(rectangleInfoAndSize, _pictureFormat, _mapType, IsTrafficFlowEnabled);
                        Thread thread = new Thread(new ParameterizedThreadStart(FetchImageInThread));
                        thread.Start(new object[] { requestString, _tileImageId });
                        threads.Add(thread);

                        if (threads.Count == 2)
                        {
                            threads[0].Join();
                            threads[1].Join();
                            threads.Clear();
                        }

                        _imageInfoUncached.Add(new HEREMapsLayerTileInfo(_tileImageId, tileX, tileY));
                    }

                    _tileImageId++;
                }
            }

            foreach (Thread item in threads)
            {
                item.Join();
            }

            return _images;
        }

        private void FetchImageInThread(object obj)
        {
            object[] objects = obj as object[];
            string requestString = (string)objects[0];
            int imageId = (int)objects[1];

            Bitmap requestedBitmap = GetImage(requestString);
            _images.TryAdd(imageId, requestedBitmap);
        }

        private Bitmap GetImage(string requestString)
        {
            return RequestBuilder.FindImage(requestString);
        }

        private double FromLatitudeToHerePixel(double latitude)
        {
            double f = Math.Sin(latitude * (Math.PI / 180.0));
            return _ru + (0.5 * Math.Log((1 + f) / (1 - f)) * (-_vu));
        }

        private double FromHerePixelToLatitude(double yPixel)
        {
            double g = (yPixel - _ru) / (-_vu);
            return (2.0 * Math.Atan(Math.Exp(g)) - (Math.PI / 2.0)) / (Math.PI / 180.0);
        }

        private double FromLongitudetoHerePixel(double longitude)
        {
            return _ru + (longitude * _uu);
        }

        private double FromHerePixelToLongitude(double xPixel)
        {
            return (xPixel - _ru) / _uu;
        }

        private static void WaitUntilGetAllImage(ConcurrentDictionary<int, Bitmap> bitmaps, int tileImageCount)
        {
            while (bitmaps.Count != tileImageCount)
            {
                Thread.Sleep(10);
            }
        }

    }
}
