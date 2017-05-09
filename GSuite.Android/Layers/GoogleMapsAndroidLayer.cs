using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using System.Collections.Concurrent;
using Mapgenix.Styles;
using Android.Graphics;
using Mapgenix.Layers;

namespace Mapgenix.GSuite.Android
{
    /// <summary>Layer for GoogleMaps web map service.</summary>
    public class GoogleMapsAndroidLayer : BaseLayer
    {
        private const double divideDpi = 1.0 / 96.0;
        private const double googleLimitLatitud = 85.0;
        private const int googleTileHeight = 256;
        private const double inchPerDecimalDegree = 4374754;
        private const double inchToMeter = 0.0254;
        private const int tileHeight = 512;
        private const int tileWidth = 512;
        private string clientId;

        [NonSerialized]
        private Exception exception;

        private GoogleMapsAndroidLayerImageCache imageCache;

        /*[NonSerialized]
        private Collection<GoogleMapsLayerTileInfo> imageInfoUncached;*/

        [NonSerialized]
        private ConcurrentDictionary<int, Bitmap> images;

        private string licenseKey;
        private GoogleMapsMapType mapType;
        private int maxPixelInZoomLevel;
        private Bitmap noDataTileImage;

        private int pastOffsetX;
        private int pastOffsetY;
        private GoogleMapsPictureFormat pictureFormat;
        private string privateKey;
        private Proj4Projection projection;

        private double ru;
        private int tileImageId;
        private GoogleMapsTileMode tileMode;
        private int timeout;
        private double uu;
        private double vu;
        private int zoomlevelNumber;
        private WebProxy webProxy;


        public event EventHandler<CreatingRequestGoogleMapsLayerEventArgs> CreatingRequest;

        public GoogleMapsAndroidLayer()
            : this(String.Empty, String.Empty, String.Empty, String.Empty, null)
        {
        }

        public GoogleMapsAndroidLayer(String licenseKey)
            : this(licenseKey, String.Empty, String.Empty, String.Empty, null)
        {
        }

        public GoogleMapsAndroidLayer(String cacheDirectory, String clientId, String privateKey)
            : this(string.Empty, cacheDirectory, clientId, privateKey, null)
        {
        }

        public GoogleMapsAndroidLayer(String cacheDirectory, String clientId, String privateKey, WebProxy webProxy)
            : this(string.Empty, cacheDirectory, clientId, privateKey, webProxy)
        {
        }

        public GoogleMapsAndroidLayer(String licenseKey, String cacheDirectory, String clientId, String privateKey, WebProxy webProxy)
            : base()
        {
            this.licenseKey = licenseKey;
            imageCache = new GoogleMapsAndroidLayerImageCache();
            imageCache.CacheDirectory = cacheDirectory;
            this.IsVisible = true;
            tileMode = GoogleMapsTileMode.SingleTile;
            this.clientId = clientId;
            this.privateKey = privateKey;

            projection = new Proj4Projection();
            projection.InternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();
            projection.ExternalProjectionParametersString = Proj4Projection.GetWgs84ParametersString();

            this.webProxy = webProxy;

            timeout = 20;
        }

        public string LicenseKey { set { licenseKey = value; } }


        public GoogleMapsTileMode TileMode
        {
            get { return tileMode; }
            set { tileMode = value; }
        }

        public GoogleMapsPictureFormat PictureFormat
        {
            get { return pictureFormat; }
            set { pictureFormat = value; }
        }

        public GoogleMapsMapType MapType
        {
            get { return mapType; }
            set { mapType = value; }
        }

        public int TimeoutInSeconds
        {
            get { return timeout; }
            set { timeout = value; }
        }

        public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }

        public string PrivateKey
        {
            get { return privateKey; }
            set { privateKey = value; }
        }

        public string CacheDirectory
        {
            get { return imageCache.CacheDirectory; }
            set { imageCache.CacheDirectory = value; }
        }

        public Bitmap NoDataTileImage
        {
            get
            {
                if (noDataTileImage == null)
                {
                    noDataTileImage = null; // GetCompressedOrStretchedImage(Resources.NoImageTile, tileWidth, tileHeight);
                }
                return Bitmap.CreateBitmap(noDataTileImage);
            }
            set { noDataTileImage = null; } // GetCompressedOrStretchedImage(value, tileWidth, tileHeight); }
        }

        public WebProxy WebProxy
        {
            get { return webProxy; }
            set { webProxy = value; }
        }

        protected virtual ConcurrentDictionary<int, Bitmap> BufferImages
        {
            get { return images; }
        }

        protected virtual void OnCreatingRequest(CreatingRequestGoogleMapsLayerEventArgs e)
        {
            EventHandler<CreatingRequestGoogleMapsLayerEventArgs> handler = CreatingRequest;

            if (handler != null)
            {
                handler(this, e);
            }
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
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckOpenStreetMapUnit(canvas.MapUnit, "canvas.MapUnit");

            switch (tileMode)
            {
                case GoogleMapsTileMode.MultiTile:
                    ProcessMultiTileMode(canvas);
                    break;
                case GoogleMapsTileMode.SingleTile:
                    ProcessSingleTileMode(canvas);
                    break;
                default:
                    break;
            }
        }

        private void ProcessSingleTileMode(BaseGeoCanvas canvas)
        {
            Bitmap image = null;

            try
            {
                zoomlevelNumber = GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit, canvas.Dpi);
                int zoomlevel = ZoomLevelHelper.GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit);
                PointShape center = (PointShape)projection.ConvertToExternalProjection(canvas.CurrentWorldExtent.GetCenterPoint());

                int newWidth = Math.Min((int)canvas.Width, 4000);
                int newHeight = Math.Min((int)canvas.Height, 4000);

                if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(privateKey))
                {
                    newWidth = Math.Min((int)canvas.Width, 1024);
                    newHeight = Math.Min((int)canvas.Height, 1024);
                }

                if (zoomlevel <= 1)
                {
                    newWidth = Math.Min((int)canvas.Width, 512);
                    newHeight = Math.Min((int)canvas.Height, 512);
                }

                string rectangleInfoAndSize = string.Empty;
                rectangleInfoAndSize += "center=" + Math.Round(center.Y, 6).ToString(CultureInfo.InvariantCulture) + "," + Math.Round(center.X, 6).ToString(CultureInfo.InvariantCulture) + "&";
                rectangleInfoAndSize += "zoom=" + zoomlevel + "&";
                rectangleInfoAndSize += "size=" + (int)newWidth + "x" + (int)newHeight;

                string requestString = @"http://maps.google.com/maps/api/staticmap?";
                requestString += rectangleInfoAndSize + "&";
                requestString += "maptype=" + GetMapType() + "&";
                requestString += "format=" + GetPictureFormat() + "&";

                if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(privateKey))
                {
                    requestString += "key=" + licenseKey + "&";
                }
                else
                {
                    requestString += "client=" + clientId + "&";
                }
                requestString += "sensor=false";

                if (!string.IsNullOrEmpty(clientId) || !string.IsNullOrEmpty(privateKey))
                {
                    requestString += "&signature=" + GetSignature(requestString);
                }

                WebRequest request = null;
                Stream imageStream = null;

                try
                {
                    if (imageCache.IsCacheEnabled)
                    {
                        image = imageCache.GetCacheImage(mapType, zoomlevel, requestString, pictureFormat);
                    }

                    if(image == null)
                    {
                        CreatingRequestGoogleMapsLayerEventArgs args = new CreatingRequestGoogleMapsLayerEventArgs(new Uri(requestString));
                        OnCreatingRequest(args);
                        request = HttpWebRequest.Create(args.RequestUri);
                        imageStream = request.GetResponse().GetResponseStream();
                        image = BitmapFactory.DecodeStream(imageStream);
                        imageCache.SaveCacheImage(image, mapType, zoomlevel, requestString, pictureFormat);
                    }
                }
                catch(Exception ex)
                {

                }
                finally
                {
                    if (imageStream != null)
                    {
                        imageStream.Dispose();
                    }
                }

                GdiPlusAndroidGeoCanvas gdiplusCanvas = canvas as GdiPlusAndroidGeoCanvas;
                if (gdiplusCanvas != null)
                {
                    gdiplusCanvas.DrawScreenImageWithoutScaling(image, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    //image.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    GeoImage geoImage = new GeoImage(stream);
                    canvas.DrawScreenImageWithoutScaling(geoImage, canvas.Width * 0.5f, canvas.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                    geoImage.Dispose();
                }
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
            }
        }

        private void ProcessMultiTileMode(BaseGeoCanvas canvas)
        {
            Bitmap resultImage = null;
            GeoImage geoImage = null;
            try
            {
                if (IsCompleteOutOfEarth(canvas.CurrentWorldExtent))
                {
                    return;
                }

                tileImageId = 0;
                images = new ConcurrentDictionary<int, Bitmap>();
                //imageInfoUncached = new Collection<GoogleMapsLayerTileInfo>();
                zoomlevelNumber = GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit, canvas.Dpi);
                vu = googleTileHeight * Math.Pow(2.0, zoomlevelNumber) / (2.0 * Math.PI);
                ru = googleTileHeight * Math.Pow(2.0, zoomlevelNumber) / (2.0);
                uu = googleTileHeight * Math.Pow(2.0, zoomlevelNumber) / (360.0);
                maxPixelInZoomLevel = (int)Math.Round(googleTileHeight * Math.Pow(2.0, zoomlevelNumber));
                RectangleShape currentExtent = GetExtentInDecimalDegree(canvas);

                if (currentExtent.UpperLeftPoint.X < -180 && currentExtent.UpperRightPoint.X > -180)
                {
                    // Handle the tile which cross -180
                    Bitmap rightResultImage = GetTileImages(currentExtent, (int)canvas.Width, (int)canvas.Height, canvas.Dpi);
                    tileImageId = 0;
                    Bitmap leftResultImage = GetTileImages(new RectangleShape(currentExtent.LowerLeftPoint.X + 360, currentExtent.UpperRightPoint.Y, currentExtent.LowerRightPoint.X + 360, currentExtent.LowerRightPoint.Y), (int)canvas.Width, (int)canvas.Height, canvas.Dpi);
                    double ratio = (-180 - currentExtent.LowerLeftPoint.X) / (currentExtent.LowerRightPoint.X - currentExtent.LowerLeftPoint.X);
                    // Merge two images
                    resultImage = Bitmap.CreateBitmap((int)canvas.Width, (int)canvas.Height, Bitmap.Config.Argb8888);
                    Graphics graphics = Graphics.FromImage(resultImage);
                    RectF leftRectangle = new RectF(0, 0, (float)Math.Round(ratio * canvas.Width, 0), canvas.Height);
                    RectF rightRectangle = new RectF(leftRectangle.Width(), 0, (int)(canvas.Width - leftRectangle.Width()), (int)canvas.Height);
                    //graphics.DrawImage(leftResultImage, leftRectangle, leftRectangle, GraphicsUnit.Pixel);
                    //graphics.DrawImage(rightResultImage, rightRectangle, rightRectangle, GraphicsUnit.Pixel);
                    graphics.DrawImage(leftResultImage, leftRectangle, leftRectangle);
                    graphics.DrawImage(rightResultImage, rightRectangle, rightRectangle);
                    graphics.Dispose();
                }
                else
                {
                    resultImage = GetTileImages(currentExtent, (int)canvas.Width, (int)canvas.Height, canvas.Dpi);
                }

                GdiPlusAndroidGeoCanvas gdiplusCanvas = canvas as GdiPlusAndroidGeoCanvas;
                if (gdiplusCanvas != null)
                {
                    gdiplusCanvas.DrawScreenImageWithoutScaling(resultImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
                else
                {
                    MemoryStream stream = new MemoryStream();
                    //resultImage.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    geoImage = new GeoImage(stream);
                    canvas.DrawScreenImageWithoutScaling(geoImage, resultImage.Width * 0.5f, resultImage.Height * 0.5f, DrawingLevel.LevelOne, 0, 0, 0);
                }
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

            if (exception != null)
            {
                Exception tempException = exception;
                exception = null;
                throw tempException;
            }
        }

        private bool IsCompleteOutOfEarth(RectangleShape utmExtent)
        {
            
            PointShape upperLeftPoint = (PointShape)projection.ConvertToExternalProjection(utmExtent.UpperLeftPoint);
            PointShape lowerRightPoint = (PointShape)projection.ConvertToExternalProjection(utmExtent.LowerRightPoint);

            if (-180 > lowerRightPoint.X ||
                180 < upperLeftPoint.X ||
                googleLimitLatitud < lowerRightPoint.Y ||
                -googleLimitLatitud > upperLeftPoint.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private RectangleShape GetExtentInDecimalDegree(BaseGeoCanvas canvas)
        {
            PointShape centerPoint = GetCenterPointInDecimalDegree(canvas);
            float width = canvas.Width;
            float height = canvas.Height;

            GoogleMapZoomLevelSet set = new GoogleMapZoomLevelSet();
            RectangleShape newExtent = ExtentHelper.SnapToZoomLevel(canvas.CurrentWorldExtent, GeographyUnit.Meter, width, height, set);
            double currentScale = ExtentHelper.GetScale(newExtent, width, GeographyUnit.Meter, canvas.Dpi);
            double extentWidth = currentScale * width * divideDpi / inchPerDecimalDegree;
            double extentHeight = extentWidth * height / (double)width;
            RectangleShape resultExtent = new RectangleShape();
            resultExtent.UpperLeftPoint.X = centerPoint.X - extentWidth * 0.5;
            resultExtent.UpperLeftPoint.Y = centerPoint.Y + extentHeight * 0.5;
            resultExtent.LowerRightPoint.X = centerPoint.X + extentWidth * 0.5;
            resultExtent.LowerRightPoint.Y = centerPoint.Y - extentHeight * 0.5;

            return resultExtent;
        }

        

        private Bitmap GetTileImages(RectangleShape renderExtent, int canvasWidth, int canvasHeight, float dpi)
        {
            PointShape renderExtentCenter = renderExtent.GetCenterPoint();

            double centerPixelX = FromLongitudetoGooglePixel(renderExtentCenter.X);
            double centerPixelY = FromLatitudeToGooglePixel(renderExtentCenter.Y);
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
            Bitmap resultImage = Bitmap.CreateBitmap(canvasWidth, canvasHeight, Bitmap.Config.Argb8888);
            Graphics resultGraphics = Graphics.FromImage(resultImage);
            resultGraphics.DrawImageUnscaled(bufferImage, -pastOffsetX, -pastOffsetY);
            resultGraphics.Dispose();
            bufferImage.Dispose();
            return resultImage;
        }

        private Bitmap GetBufferImage(int imageWidth, int imageHeight, int startTileX, int startTileY, int endTileX, int endTileY)
        {
            GetImagesArray(startTileX, startTileY, endTileX, endTileY);
            WaitUntilGetAllImage(images, tileImageId);
            CacheSave();

            Bitmap bufferImage = Bitmap.CreateBitmap(imageWidth, imageHeight, Bitmap.Config.Argb8888);
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
            /*if (imageCache.IsCacheEnabled)
            {
                foreach (GoogleMapsLayerTileInfo tileInfo in imageInfoUncached)
                {
                    if (images[tileInfo.ImageId].Width != 1)
                    {
                        imageCache.SaveCacheImage(images[tileInfo.ImageId], mapType, zoomlevelNumber, tileInfo.TileX, tileInfo.TileY, pictureFormat);
                    }
                }
            }
            imageInfoUncached.Clear();*/
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
                    double pixelX = tileX * tileWidth + tileWidth * 0.5 - 1;
                    double pixelY = tileY * tileHeight + tileHeight * 0.5 - 1;
                    double latitude = FromGooglePixelToLatitude(pixelY);
                    double longitude = FromGooglePixelToLongitude(pixelX);

                    Bitmap image = null;
                    /*if (imageCache.IsCacheEnabled)
                    {
                        image = imageCache.GetCacheImage(mapType, zoomlevelNumber, tileX, tileY, pictureFormat);
                        if (image != null)
                        {
                            images.TryAdd(tileImageId, image);
                        }
                    }*/

                    if (image == null)
                    {
                        string requestString = GetRequestImageUrl(longitude, latitude);//, tileWidth, tileHeight, zoomlevelNumber, longitude, latitude);
                        Thread thread = new Thread(new ParameterizedThreadStart(FetchImageInThread));
                        thread.Start(new object[] { tileImageId, longitude, latitude });
                        threads.Add(thread);

                        if (threads.Count == 2)
                        {
                            threads[0].Join();
                            threads[1].Join();
                            threads.Clear();
                        }

                        //imageInfoUncached.Add(new GoogleMapsLayerTileInfo(tileImageId, tileX, tileY));
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

        private string GetRequestImageUrl(double lon, double lat)//, double newWidth, double newHeight, int zoomLevelNumber)//, double centerLongitude, double newLatitude)
        {
            return GetRequestImageUrl(lon, lat, zoomlevelNumber, tileWidth, tileHeight);
        }

        private string GetRequestImageUrl(double lon, double lat, int zoomLevelNumber, double tileWidth, double tileHeight)
        {
            string regionInfoParameters = GetRegionInfoParameters(tileWidth, tileHeight, zoomLevelNumber, lon, lat);
            string requestString = @"http://maps.google.com/maps/api/staticmap?";
            requestString += regionInfoParameters + "&";
            requestString += "maptype=" + GetMapType() + "&";
            requestString += "format=" + GetPictureFormat() + "&";
            if (string.IsNullOrEmpty(clientId) && string.IsNullOrEmpty(privateKey))
            {
                requestString += "key=" + licenseKey + "&";
            }
            else
            {
                requestString += "client=" + clientId + "&";
            }
            requestString += "sensor=false";

            if (!string.IsNullOrEmpty(clientId) || !string.IsNullOrEmpty(privateKey))
            {
                requestString += "&signature=" + GetSignature(requestString);
            }

            return requestString;
        }

       
        private void FetchImageInThread(object obj)
        {
            object[] objects = obj as object[];
            int imageId = (int)objects[0];
            double longitude = (double)objects[1];
            double latitude = (double)objects[2];

            Bitmap requestedBitmap = GetGoogleMapsImage(longitude, latitude, zoomlevelNumber, tileWidth, tileHeight);
            images.TryAdd(imageId, requestedBitmap);
        }

        public Bitmap GetGoogleMapsImage(double longitude, double latitude, int zoomLevelNumber, double tileWidth, double tileHeight)
        {
            return GetGoogleMapsImageCore(longitude, latitude, zoomLevelNumber, tileWidth, tileHeight);
        }

        protected virtual Bitmap GetGoogleMapsImageCore(double longitude, double latitude, int zoomLevelNumber, double tileWidth, double tileHeight)
        {
            string requestString = GetRequestImageUrl(longitude, latitude, zoomLevelNumber, tileWidth, tileHeight);

            WebRequest request = null;
            Stream imageStream = null;
            Bitmap image = null;

            try
            {
                CreatingRequestGoogleMapsLayerEventArgs args = new CreatingRequestGoogleMapsLayerEventArgs(new Uri(requestString));
                OnCreatingRequest(args);
                request = HttpWebRequest.Create(args.RequestUri);
                request.Timeout = timeout * 1000;
                request.Proxy = webProxy;
                imageStream = request.GetResponse().GetResponseStream();
                image = BitmapFactory.DecodeStream(imageStream);
            }
            catch (Exception ex)
            {
                if (exception == null)
                {
                    exception = ex;
                }
            }
            finally
            {
                if (image == null)
                {
                    image = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
                }

                if (imageStream != null)
                {
                    imageStream.Dispose();
                }
            }

            return image;
        }

        private string GetSignature(string url)
        {
            Validators.CheckParameterIsNotNull(PrivateKey, "PrivateKey");

            // converting key to bytes will throw an exception, need to replace '-' and '_' characters first. 
            string usablePrivateKey = PrivateKey.Replace("-", "+").Replace("_", "/");
            byte[] privateKeyBytes = Convert.FromBase64String(usablePrivateKey);

            Uri uri = new Uri(url);
            byte[] encodedPathAndQueryBytes = GetAsciiBytes(uri.LocalPath + uri.Query);

            // compute the hash 
            HMACSHA1 algorithm = new HMACSHA1(privateKeyBytes);
            byte[] hash = algorithm.ComputeHash(encodedPathAndQueryBytes);

            // convert the bytes to string and make url-safe by replacing '+' and '/' characters 
            string signature = Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_");

            return signature;
        }

        private static byte[] GetAsciiBytes(string value)
        {
            byte[] temp = Encoding.Unicode.GetBytes(value);
            byte[] result = new byte[temp.Length / 2];

            for (int i = 0; i < temp.Length; i += 2)
            {
                result[i / 2] = temp[i];
                if (result[i / 2] > 127)
                {
                    result[i / 2] = 63;
                }
            }
            return result;
        }

        private static string GetRegionInfoParameters(double newWidth, double newHeight, int zoomLevelNumber, double centerLongitude, double newLatitude)
        {
            string rectangleInfoAndSize = string.Empty;
            rectangleInfoAndSize += "center=" + Math.Round(newLatitude, 6).ToString(CultureInfo.InvariantCulture) + "," + Math.Round(centerLongitude, 6).ToString(CultureInfo.InvariantCulture) + "&";
            rectangleInfoAndSize += "zoom=" + zoomLevelNumber + "&";
            rectangleInfoAndSize += "size=" + (int)newWidth + "x" + (int)newHeight;
            return rectangleInfoAndSize;
        }

        private static int GetCurrentZoomLevelNumber(double newWidth, RectangleShape newTileExtent, GeographyUnit mapUnit, float dpi)
        {
            GoogleMapZoomLevelSet zoomlevelSet = new GoogleMapZoomLevelSet();
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

        private static void WaitUntilGetAllImage(ConcurrentDictionary<int, Bitmap> bitmaps, int tileImageCount)
        {
            while (bitmaps.Count != tileImageCount)
            {
                Thread.Sleep(10);
            }
        }

        private double FromLatitudeToGooglePixel(double latitude)
        {
            double f = Math.Sin(latitude * (Math.PI / 180.0));
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

        private string GetMapType()
        {
            switch (mapType)
            {
                case GoogleMapsMapType.RoadMap:
                    return "roadmap";

                case GoogleMapsMapType.Mobile:
                    return "mobile";

                case GoogleMapsMapType.Satellite:
                    return "satellite";

                case GoogleMapsMapType.Terrain:
                    return "terrain";

                case GoogleMapsMapType.Hybrid:
                    return "hybrid";

                default:
                    return "roadmap";
            }
        }

       
        public string GetPictureFormat()
        {
            switch (pictureFormat)
            {
                case GoogleMapsPictureFormat.Jpeg:
                    return "jpg-baseline";

                case GoogleMapsPictureFormat.Gif:
                    return "gif";

                case GoogleMapsPictureFormat.Png8:
                    return "png8";

                case GoogleMapsPictureFormat.Png32:
                    return "png32";

                default:
                    return "jpg-baseline";
            }
        }


        /*private static Bitmap GetCompressedOrStretchedImage(Image offScaleImage, int tileWidth, int tileHeight)
        {
            Bitmap resultImage = null;
            if (offScaleImage != null)
            {
                Graphics graphics = null;

                try
                {
                    Rectangle sourceRectangle = new Rectangle(0, 0, offScaleImage.Width, offScaleImage.Height);
                    Rectangle destinationRectangle = new Rectangle(0, 0, tileWidth, tileHeight);

                    resultImage = new Bitmap(tileWidth, tileHeight);
                    graphics = Graphics.FromImage(resultImage);
                    graphics.DrawImage(offScaleImage, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
                }
                finally
                {
                    if (graphics != null)
                    {
                        graphics.Dispose();
                    }
                    if (offScaleImage != null)
                    {
                        offScaleImage.Dispose();
                    }
                }
            }

            return resultImage;
        }*/

       
    }
}