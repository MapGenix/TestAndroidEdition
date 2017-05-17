using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapgenix.Layers;
using System.Net;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// HereMapsOverlay requesting tiles from HereMaps
    /// thru its Static API.
    /// </summary>
    [Serializable]
    public class HEREMapsOverlay : BaseTileOverlay
    {
        private const int DefaultTileWidth = 128;
        private const int DefaultTileHeight = 128;
        private HEREMapsAndroidLayer _hereLayer;

        public HEREMapsOverlay(Context context) :
            this(context, String.Empty, String.Empty, HEREMapsMapType.Normal, String.Empty, HEREMapsPictureFormat.Png, null)
        { }

        public HEREMapsOverlay(Context context, string appID, string appCode) :
            this(context, appID, appCode, HEREMapsMapType.Normal, String.Empty, HEREMapsPictureFormat.Png, null)
        { }

        public HEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType) :
           this(context, appID, appCode, mapType, String.Empty, HEREMapsPictureFormat.Png, null)
        { }

        public HEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory) :
            this(context, appID, appCode, mapType, cacheDirectory, HEREMapsPictureFormat.Png, null)
        { }
        public HEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory, HEREMapsPictureFormat pictureFormat) :
            this(context, appID, appCode, mapType, cacheDirectory, pictureFormat, null)
        { }
        public HEREMapsOverlay(Context context, string appID, string appCode, HEREMapsMapType mapType, string cacheDirectory, HEREMapsPictureFormat pictureFormat, WebProxy webProxy)
            :base(context)
        {
            _hereLayer = new HEREMapsAndroidLayer(appID, appCode, mapType, cacheDirectory, pictureFormat, webProxy);
            TileWidth = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileWidth, Context.Resources.DisplayMetrics.Xdpi));
            TileHeight = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileHeight, Context.Resources.DisplayMetrics.Xdpi));
        }

        public HEREMapsMapType MapType
        {
            get { return _hereLayer.MapType; }
            set { _hereLayer.MapType = value; }
        }

        public HEREMapsPictureFormat CachePictureFormat
        {
            get { return _hereLayer.PictureFormat; }
            set { _hereLayer.PictureFormat = value; }
        }

        public string CacheDirectory
        {
            get { return _hereLayer.CacheDirectory; }
            set { _hereLayer.CacheDirectory = value; }
        }

        public string AppID
        {
            get
            {
                return _hereLayer.AppID;
            }
            set
            {
                _hereLayer.AppID = value;
            }
        }

        public string AppCode
        {
            get
            {
                return _hereLayer.AppCode;
            }
            set
            {
                _hereLayer.AppCode = value;
            }
        }

        public WebProxy WebProxy
        {
            get
            {
                return _hereLayer.WebProxy;
            }
            set
            {
                _hereLayer.WebProxy = value;
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

        protected override void DrawTileCore(Tile tile, RectangleShape targetExtent, Action<Tile> callback)
        {
            LayerTile layerTile = tile as LayerTile;

            if (layerTile != null)
            {
                layerTile.TileCache = TileCache;
                layerTile.DrawingLayers.Clear();
                layerTile.DrawingLayers.Add(_hereLayer);

                Bitmap nativeImage = Bitmap.CreateBitmap((int)tile.LayoutParameters.Width, (int)tile.LayoutParameters.Height, Bitmap.Config.Argb8888);
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                if (tile.IsAsync)
                {
                    layerTile.DrawAsync(geoCanvas, callback);
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

        public string GetPictureFormatString()
        {
            return CachePictureFormat.ToString();
        }

    }
}
