using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Mapgenix.Canvas;
//using Mapgenix.RasterSource;
using System.Drawing;
using Mapgenix.Shapes;
using System.IO;
using System.Collections.Concurrent;
using Mapgenix.Styles;
using System.Threading;
using System.Drawing.Imaging;

namespace Mapgenix.Layers
{
    /// <summary>Layer for WMS Server.</summary>
    [Serializable]
    public class WmsLayer : BaseLayer
    {

        #region private properties

        private WmsPictureFormat _outputFormat;
        private BaseBitmapTileCache _tileCache;
        private WmsLayerImageCache _imageCache = new WmsLayerImageCache();
        private int _zoomlevelNumber;
        private WmsRequestBuilder _requestBuilder;
        private bool _wmsLoad;
        private bool _openLayersTileEnable = false;

        #endregion

        #region events

        //public event EventHandler<WebRequestEventArgs> SendingRequest;
        //public event EventHandler<WebResponseEventArgs> SentRequest;

        #endregion

        public WmsLayer()
        { }

        public WmsLayer(string serverUri, string layer)
            : this(serverUri, layer, "EPSG:4326", new Dictionary<string, string>())
        { }

        public WmsLayer(string serverUri, string layer, string srs)
            : this(serverUri, layer, srs, new Dictionary<string, string>())
        { }

        public WmsLayer(string serverUri, string layer, string srs, Dictionary<string, string> parameters)
        {
            Collection<string> layers = new Collection<string>();
            layers.Add(layer);
            Collection<GeoColor> keyColors = new Collection<GeoColor>();
            keyColors.Add(GeoColor.StandardColors.Black);

            RequestBuilder = new WmsRequestBuilder
            {
                Layers = layers,
                Parameters = parameters,
                WmsUrl = serverUri,
                Srs = srs
            };

            KeyColors = keyColors;
            IsVisible = true;

        }

        public WmsLayer(string serverUri, Collection<string> layers)
            : this(serverUri, layers, "EPSG:4326", new Dictionary<string, string>(), new Collection<string>(), WmsPictureFormat.Png, GeoColor.StandardColors.Black, String.Empty, String.Empty, null)
        { }

        public WmsLayer(string serverUri, Collection<string> layers, string srs)
            : this(serverUri, layers, srs, new Dictionary<string, string>(), new Collection<string>(), WmsPictureFormat.Png, GeoColor.StandardColors.Black, String.Empty, String.Empty, null)
        { }

        public WmsLayer(string serverUri, Collection<string> layers, string srs, Dictionary<string, string> parameters)
            : this(serverUri, layers, srs, parameters, new Collection<string>(), WmsPictureFormat.Png, GeoColor.StandardColors.Black, String.Empty, String.Empty, null)
        { }

        public WmsLayer(string serverUri, Collection<string> layers, string srs, Dictionary<string, string> parameters, Collection<string> styles,
                WmsPictureFormat pictureFormat, GeoColor keyColor, string clientID, string privateKey, WebProxy webProxy)
        {
            Collection<GeoColor> keyColors = new Collection<GeoColor>();
            keyColors.Add(keyColor);

            RequestBuilder = new WmsRequestBuilder
            {
                Layers = layers,
                Parameters = parameters,
                WmsUrl = serverUri,
                Srs = srs,
                PictureFormat =  pictureFormat,
                WebProxy = webProxy,
                Styles = styles
            };

            KeyColor = keyColor;
            KeyColors = keyColors;
            PrivateKey = privateKey;
            ClientId = clientID;
            IsVisible = true;
        }

        #region public properties

        public string ClientId
        {
            get;
            set;
        }

        public GeoColor KeyColor
        {
            get;
            set;
        }

        public Collection<GeoColor> KeyColors
        {
            get;
            set;
        }

        public string PrivateKey
        {
            get;
            set;
        }

        public BaseBitmapTileCache TileCache
        {
            get { return _tileCache; }
            set { _tileCache = value; }
        }

        public Collection<string> Layers
        {
            get { return RequestBuilder.Layers; }
            set
            {
                RequestBuilder.Layers = value;
            }
        }

        public Dictionary<string, string> Parameters
        {
            get { return RequestBuilder.Parameters; }
            set
            {
                RequestBuilder.Parameters = value;
            }
        }

        public string ServerUri
        {
            get { return RequestBuilder.WmsUrl; }
            set { RequestBuilder.WmsUrl = value; }
        }

        public string Srs
        {
            get { return RequestBuilder.Srs; }
            set { RequestBuilder.Srs = value; }
        }

        public string Version
        {
            get { return RequestBuilder.Version; }
            set { RequestBuilder.Version = value; }
        }

        public WebProxy WebProxy
        {
            get { return RequestBuilder.WebProxy; }
            set { RequestBuilder.WebProxy = value; }
        }

        public WmsPictureFormat OutputFormat
        {
            get { return this._outputFormat; }
            set
            {
                this._outputFormat = value;
                if (RequestBuilder != null)
                    RequestBuilder.PictureFormat = value;
            }
        }

        public string OutputFormatAsString
        {
            get
            {
                switch (this._outputFormat)
                {
                    case WmsPictureFormat.Gif:
                        return "IMAGE/GIF";
                    case WmsPictureFormat.Jpeg:
                        return "IMAGE/JPEG";
                    case WmsPictureFormat.Tiff:
                        return "IMAGE/TIFF";
                    case WmsPictureFormat.Png8:
                        return "IMAGE/PNG; MODE=8BIT";
                    default:
                        return "IMAGE/PNG";
                }
            }
        }

        public Collection<string> Styles
        {
            get { return RequestBuilder.Styles; }
            set { RequestBuilder.Styles = value; }
        }

        public bool IsTransparent
        {
            get { return RequestBuilder.IsTransparent; }
            set { RequestBuilder.IsTransparent = value; }
        }

        public string CacheDirectory
        {
            get { return _imageCache.CacheDirectory; }
            set { _imageCache.CacheDirectory = value; }
        }

        public bool OpenLayersTileEnable
        {
            get { return _openLayersTileEnable; }
            set { _openLayersTileEnable = value; }
        }

        #endregion

        #region internal properties
        internal WmsRequestBuilder RequestBuilder
        {
            get { return _requestBuilder; }
            set { _requestBuilder = value; }
        }
        #endregion

        #region public methods

        /*public void ImageSource_SentRequest(object sender, WebResponseEventArgs e)
        {
            OnSentRequest(e);
        }*/

        public void ClearCache()
        {
            if (_tileCache != null)
            {
                _tileCache.ClearCache();
            }
        }

        #endregion

        #region protected virtual methods

        /*protected virtual void OnSendingRequest(WebRequestEventArgs e)
        {
            EventHandler<WebRequestEventArgs> handler = SendingRequest;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSentRequest(WebResponseEventArgs e)
        {
            EventHandler<WebResponseEventArgs> handler = SentRequest;

            if (handler != null)
            {
                handler(this, e);
            }
        }*/

        #endregion

        #region protected override methods

        protected override void OpenCore()
        {
            if (!Parameters.ContainsKey("ClientId"))
            {
                if (ClientId != "" && ClientId != null)
                    Parameters.Add("ClientId", ClientId);
            }

            _wmsLoad = true;
        }

        protected override bool IsOpenCore
        {
            get { return _wmsLoad; }
        }

        protected override void CloseCore()
        {
            if (Parameters.ContainsKey("ClientId"))
            {
                Parameters.Remove("ClientId");
            }
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            if (!_openLayersTileEnable)
            {
                Bitmap image = null;
                MemoryStream imageStream = null;
                GeoImage result = null;

                try
                {
                    _zoomlevelNumber = ZoomLevelHelper.GetCurrentZoomLevelNumber(canvas.Width, canvas.CurrentWorldExtent, canvas.MapUnit);
                    string requestString = RequestBuilder.GetRequestUrl(canvas.CurrentWorldExtent, (int)canvas.Width, (int)canvas.Height);
                    image = RequestBuilder.FindImage(requestString);

                    imageStream = new MemoryStream();
                    image.Save(imageStream, _imageCache.GetImageFormat(_outputFormat));
                    result = new GeoImage(imageStream);

                    canvas.DrawScreenImage(result, canvas.Width * .5f, canvas.Height * .5f, canvas.Width, canvas.Height, DrawingLevel.LevelOne, 0f, 0f, 0f);

                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    if (image != null)
                        image.Dispose();

                    if (imageStream != null)
                        imageStream.Dispose();

                    if (result != null)
                        result.Dispose();
                }
            }
            
        }

        protected override void DrawExceptionCore(BaseGeoCanvas canvas, Exception e)
        {
            //WebDrawHelper.DrawException(canvas, e);
        }

        #endregion
    }
}
