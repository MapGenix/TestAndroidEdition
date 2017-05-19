using System;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Layers;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Overlay requesting tiles from OpenStreetMap Web Service.
    /// </summary>
    [Serializable]
    public class OpenStreetMapOverlay : BaseTileOverlay
    {
        private const int DefaultTileWidth = 64;
        private const int DefaultTileHeight = 64;
        private OpenStreetMapAndroidLayer _osmLayer;
        private Collection<Uri> _serverUris;
        private int _timeoutInSeconds;

        public OpenStreetMapOverlay(Context context)
            : this(context, null, string.Empty)
        { }

        public OpenStreetMapOverlay(Context context, WebProxy webProxy, string cacheDirectory)
            :base(context)
        {
            _osmLayer = new OpenStreetMapAndroidLayer(webProxy, cacheDirectory);

            _osmLayer.TimeoutInSeconds = 10;
            //TileWidth = (int)LayoutUnitsUtil.convertDpToPixel(DefaultTileWidth, Context.Resources.DisplayMetrics.Xdpi);
            //TileHeight = (int)LayoutUnitsUtil.convertDpToPixel(DefaultTileHeight, Context.Resources.DisplayMetrics.Xdpi);
            TileWidth = DefaultTileWidth;
            TileHeight = DefaultTileHeight;
            _serverUris = new Collection<Uri>();
            this._serverUris.Add(new Uri("http://a.tile.openstreetmap.org"));
            this._serverUris.Add(new Uri("http://b.tile.openstreetmap.org"));
            this._serverUris.Add(new Uri("http://c.tile.openstreetmap.org"));
        }

        public WebProxy WebProxy
        {
            get { return _osmLayer.WebProxy; }
            set { _osmLayer.WebProxy = value; }
        }

        /*public String CacheDirectory
        {
            get { return _osmLayer.CacheDirectory; }
            set
            {
                Validators.CheckParameterIsNotNullOrEmpty(value, "CacheDirectory");
                _osmLayer.CacheDirectory = value;
            }
        }*/

        public String CacheDirectory
        {
            get; set;
        }

        protected override Tile GetTileCore()
        {
            /*UriTile tile = new UriTile(Context);
            tile.UriTileMode = UriTileMode.Custom;
            tile.ThreadLocker = new object();

            if (TileType == TileType.SingleTile)
            {
                tile.IsAsync = false;
            }

            return tile;*/
            LayerTile tile = new LayerTile(Context);

            if (TileType == TileType.SingleTile)
            {
                tile.IsAsync = false;
            }

            return tile;
        }

        public Collection<Uri> CustomServerUris
        {
            get { return this._serverUris; }
        }

        public int TimeoutInSeconds
        {
            get { return this._timeoutInSeconds; }
            set { this._timeoutInSeconds = value; }
        }

        protected override void DrawTileCore(Tile tile, RectangleShape targetExtent, Action<Tile> callback)
        {
            //UriTile layerTile = tile as UriTile;
            LayerTile layerTile = tile as LayerTile;

            if (layerTile != null)
            {
                layerTile.TileCache = TileCache;
                layerTile.DrawingLayers.Clear();
                layerTile.DrawingLayers.Add(_osmLayer);

                Bitmap nativeImage = Bitmap.CreateBitmap((int)tile.LayoutParameters.Width, (int)tile.LayoutParameters.Height, Bitmap.Config.Argb8888);
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                //layerTile.Uri = GetTileUri(layerTile.RowIndex, layerTile.ColumnIndex, layerTile.ZoomLevelIndex);

                if (tile.IsAsync)
                {
                    layerTile.DrawAsync(geoCanvas, callback);
                }
                else
                {
                    layerTile.Draw(geoCanvas);
                    geoCanvas.EndDrawing();
                    layerTile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
                }
            }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            return ExtentHelper.ZoomIn(new RectangleShape(-20001365, 20001365, 20001365, -20001365), 50);
        }

        private Uri GetTileUri(long rowIndex, long columnIndex, int zoomLevelIndex)
        {
            return new Uri(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0}/{1}/{2}/{3}.png", (object)this._serverUris[(int)columnIndex % this._serverUris.Count].AbsoluteUri.TrimEnd('/'), (object)(zoomLevelIndex + 1), (object)columnIndex, (object)rowIndex));
        }
    }
}
