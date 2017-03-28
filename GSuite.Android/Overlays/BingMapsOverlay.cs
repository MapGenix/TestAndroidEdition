using Android.Content;
using Mapgenix.Canvas;
using Mapgenix.Layers;
using Mapgenix.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// BingMapsOverlay requesting tiles from BingMaps
    /// thru its Static API.
    /// </summary>
    [Serializable]
    public class BingMapsOverlay : BaseTileOverlay
    {
        private const int DefaultTileWidth = 128;
        private const int DefaultTileHeight = 128;
        private BingMapsAndroidLayer _bingLayer;

        public BingMapsOverlay(Context context) :
            this(context, String.Empty, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null)
        { }

        public BingMapsOverlay(Context context, string licenseKey) :
            this(context, licenseKey, BingMapsMapType.Road, String.Empty, BingMapsPictureFormat.Png, null)
        { }

        public BingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType) :
            this(context, licenseKey, mapType, String.Empty, BingMapsPictureFormat.Png, null)
        { }

        public BingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory) :
            this(context,licenseKey, mapType, cacheDirectory, BingMapsPictureFormat.Png, null)
        { }

        public BingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat) :
            this(context, licenseKey, mapType, cacheDirectory, pictureFormat, null)
        { }

        public BingMapsOverlay(Context context, string licenseKey, BingMapsMapType mapType, string cacheDirectory, BingMapsPictureFormat pictureFormat, WebProxy webProxy)
            : base(context)
        {
            //_bingLayer = WebLayerFactory.CreateBingMapsLayer(licenseKey, mapType, cacheDirectory, pictureFormat, webProxy);
            _bingLayer = new BingMapsAndroidLayer(licenseKey, mapType, cacheDirectory, pictureFormat, webProxy);
            TileWidth = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileWidth, Context.Resources.DisplayMetrics.Xdpi));
            TileHeight = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileHeight, Context.Resources.DisplayMetrics.Xdpi));
        }

        public BingMapsMapType MapType
        {
            get { return _bingLayer.MapType; }
            set
            {
                _bingLayer.MapType = value;
                if (TileType != TileType.SingleTile && DrawingCanvas != null && StretchCanvas != null)
                {
                    ClearTiles(ClearTilesMode.AllTiles);
                }
            }
        }

        public BingMapsPictureFormat PictureFormat
        {
            get { return _bingLayer.PictureFormat; }
            set
            {
                switch (value)
                {
                    case BingMapsPictureFormat.Png:
                    case BingMapsPictureFormat.Jpeg:
                    case BingMapsPictureFormat.Gif:
                        break;
                    default:
                        throw new NotSupportedException("PictureFormat:" + value + " not be supported.");
                }
                _bingLayer.PictureFormat = value;
                if (TileType != TileType.SingleTile && DrawingCanvas != null && StretchCanvas != null)
                {
                    ClearTiles(ClearTilesMode.AllTiles);
                }
            }
        }

        public string LicenseKey
        {
            get
            {
                return _bingLayer.LicenseKey;
            }
            set
            {
                _bingLayer.LicenseKey = value;
            }
        }

        public string CacheDirectory
        {
            get { return _bingLayer.CacheDirectory; }
            set { _bingLayer.CacheDirectory = value; }
        }

        public WebProxy WebProxy
        {
            get
            {
                return _bingLayer.WebProxy;
            }
            set
            {
                _bingLayer.WebProxy = value;
            }
        }

        protected override Tile GetTileCore()
        {
            LayerTile tile = new LayerTile(Context);
            if (TileType == TileType.SingleTile)
            {
                tile.IsAsync = false;
            }

            return tile;
        }

        protected override void DrawTileCore(Tile tile, RectangleShape targetExtent)
        {
            LayerTile layerTile = tile as LayerTile;

            if (layerTile != null)
            {
                layerTile.TileCache = TileCache;
                layerTile.DrawingLayers.Clear();
                layerTile.DrawingLayers.Add(_bingLayer);

                Bitmap nativeImage = Bitmap.CreateBitmap((int)tile.LayoutParameters.Width, (int)tile.LayoutParameters.Height, Bitmap.Config.Argb8888);
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                if (tile.IsAsync)
                {
                    layerTile.DrawAsync(geoCanvas);
                }
                else
                {
                    layerTile.Draw(geoCanvas);
                    geoCanvas.EndDrawing();

                    try
                    {
                        layerTile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
                    }
                    finally
                    {
                        nativeImage.Dispose();
                    }
                }
            }
        }

    }
}
