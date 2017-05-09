using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// An overlay object that gets map tiles from the WMS (Web Mapping Services) server.
    /// </summary>
    [Serializable]
    public class WmsOverlayLite : BaseTileOverlay
    {
        private const int LockObjCount = 4;
        private object[] _lockObjs;
        private Dictionary<string, string> _requiredParameters;
        private Dictionary<string, string> _parameters;
        private Collection<Uri> _serverUris;

        public WmsOverlayLite(Context context)
            : this(context, new Collection<Uri>(), null)
        { }

        public WmsOverlayLite(Context context, Uri serverUri)
            : this(context, new Collection<Uri>() { serverUri }, null)
        { }

        public WmsOverlayLite(Context context, IEnumerable<Uri> serverUris)
            : this(context, serverUris, null)
        { }

        public WmsOverlayLite(Context context, IEnumerable<Uri> serverUris, WebProxy webProxy)
            :base(context)
        {
            Validators.CheckParameterIsNotNull(serverUris, "serverUris");

            InitThreadLockObject();

            this._serverUris = new Collection<Uri>();
            foreach (Uri uri in serverUris)
            {
                this._serverUris.Add(uri);
            }

            _requiredParameters = new Dictionary<string, string>();
            _requiredParameters.Add("SERVICE", "WMS");
            _requiredParameters.Add("VERSION", "1.1.1");
            _requiredParameters.Add("REQUEST", "GetMap");
            _requiredParameters.Add("EXCEPTIONS", "application/vnd.ogc.se_inimage");
            _requiredParameters.Add("FORMAT", "image/jpeg");
            _requiredParameters.Add("SRS", "EPSG:4326");

            _parameters = new Dictionary<string, string>();
            Projection = "EPSG:4326";
            TimeoutInSeconds = 10;

            WebProxy = webProxy;
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                _parameters = value;
            }
        }

        public Collection<Uri> ServerUris
        {
            get
            {
                return _serverUris;
            }
            set
            {
                _serverUris = value;
            }
        }

        public WebProxy WebProxy { get; set; }

        public int TimeoutInSeconds { get; set; }

        public string Projection
        {
            get
            {
                if (_requiredParameters.ContainsKey("SRS"))
                {
                    return _requiredParameters["SRS"];
                }
                else
                {
                    return "EPSG:4326";
                }
            }
            set
            {
                string projection = value;
                if (projection.IndexOf(':') == -1)
                {
                    projection = String.Format(CultureInfo.InvariantCulture, "EPSG:{0}", value);
                }

                if (_requiredParameters.ContainsKey("SRS"))
                {
                    _requiredParameters["SRS"] = projection;
                }
                else
                {
                    _requiredParameters.Add("SRS", projection);
                }
            }
        }

        protected override Tile GetTileCore()
        {
            return new UriTile(Context);
        }

        protected override void DrawTileCore(Tile tile, RectangleShape targetExtent)
        {
            UriTile uriTile = tile as UriTile;

            if (TileType == TileType.SingleTile)
            {
                uriTile.IsAsync = false;
            }

            if (uriTile != null)
            {
                int requestServerIndex = 0;
                if (_serverUris.Count > 1) { requestServerIndex = (int)Math.Abs(tile.ColumnIndex) % _serverUris.Count; }
                int lockObjIndex = (int)(Math.Abs(tile.ColumnIndex) % LockObjCount);

                uriTile.Uri = GetRequestUris(targetExtent)[requestServerIndex];
                uriTile.ThreadLocker = _lockObjs[lockObjIndex];
                uriTile.TimeoutInSeconds = TimeoutInSeconds;
                if (WebProxy != null) { uriTile.WebProxy = WebProxy; }
                if (TileCache != null) { uriTile.TileCache = TileCache; }

                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                Bitmap nativeImage = Bitmap.CreateBitmap((int)uriTile.LayoutParameters.Width, (int)uriTile.LayoutParameters.Width, Bitmap.Config.Argb8888);
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                if (uriTile.IsAsync)
                {
                    uriTile.DrawAsync(geoCanvas);
                }
                else
                {
                    uriTile.Draw(geoCanvas);
                    geoCanvas.EndDrawing();
                    uriTile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
                }
            }
        }

        
       
        public Collection<Uri> GetRequestUris(RectangleShape requestExtent)
        {
            return GetRequestUrisCore(requestExtent);
        }


        protected virtual Collection<Uri> GetRequestUrisCore(RectangleShape requestExtent)
        {
            Collection<Uri> requestUris = new Collection<Uri>();
            foreach (Uri requestUri in ServerUris)
            {
                requestUris.Add(GetRequestUri(requestUri, requestExtent));
            }
            return requestUris;
        }

        private Uri GetRequestUri(Uri serverUri, RectangleShape targetExtent)
        {
            UriBuilder uriBuilder = new UriBuilder(serverUri);

            Dictionary<string, string> tempParameters = new Dictionary<string, string>();
            foreach (string key in _requiredParameters.Keys)
            {
                tempParameters.Add(key, _requiredParameters[key]);
            }

            foreach (string key in _parameters.Keys)
            {
                if (tempParameters.ContainsKey(key))
                {
                    tempParameters[key] = _parameters[key];
                }
                else
                {
                    tempParameters.Add(key, _parameters[key]);
                }
            }

            StringBuilder paramBuilder = new StringBuilder();
            foreach (string key in tempParameters.Keys)
            {
                paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", key, tempParameters[key]);
            }

            paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", "BBOX", MapUtil.GetBBoxString(targetExtent));

            if (TileType == TileType.SingleTile)
            {
                paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", "WIDTH", MapArguments.ActualWidth);
                paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", "HEIGHT", MapArguments.ActualHeight);
            }
            else
            {
                paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", "WIDTH", TileWidth);
                paramBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", "HEIGHT", TileHeight);
            }

            uriBuilder.Query += "&" + paramBuilder.ToString().TrimEnd('&');
            uriBuilder.Query = uriBuilder.Query.TrimStart('?');
            return uriBuilder.Uri;
        }

        private void InitThreadLockObject()
        {
            _lockObjs = new object[LockObjCount];
            for (int i = 0; i < LockObjCount; i++)
            {
                _lockObjs[i] = new object();
            }
        }
    }
}
