using System;
using System.Net;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Layers;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// GoogleMapsOverlay requesting tiles from GoogleMaps
    /// thru its Static API.
    /// </summary>
    [Serializable]
    public class GoogleMapsOverlay : BaseTileOverlay
    {
        private const int DefaultTileWidth = 128;
        private const int DefaultTileHeight = 128;
        private GoogleMapsAndroidLayer _googleLayer;

        public GoogleMapsOverlay(Context context)
             : this(context, String.Empty, String.Empty, String.Empty, string.Empty, null)
        { }

        public GoogleMapsOverlay(Context context, string licenseKey)
            : this(context, licenseKey, String.Empty, String.Empty, String.Empty, null)
        { }
        
        public GoogleMapsOverlay(Context context, string licenseKey, string cacheDirectory, string clientId, string privateKey, WebProxy webProxy)
            : base(context)
        {
            _googleLayer = new GoogleMapsAndroidLayer(licenseKey, cacheDirectory, clientId, privateKey, webProxy);
            TileWidth = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileWidth, Context.Resources.DisplayMetrics.Xdpi));
            TileHeight = Convert.ToInt32(LayoutUnitsUtil.convertDpToPixel(DefaultTileHeight, Context.Resources.DisplayMetrics.Xdpi));
        }

        public GoogleMapsMapType MapType
        {
            get { return _googleLayer.MapType; }
            set
            {
                _googleLayer.MapType = value;
            }
        }

        public GoogleMapsPictureFormat CachePictureFormat
        {
            get { return _googleLayer.PictureFormat; }
            set
            {
                _googleLayer.PictureFormat = value;
            }
        }

        public String CacheDirectory
        {
            get { return _googleLayer.CacheDirectory; }
            set
            {
                _googleLayer.CacheDirectory = value;
            }
        }

        public string LicenseKey
        {
            set
            {
                _googleLayer.LicenseKey = value;
            }
        }

        public WebProxy WebProxy
        {
            get
            {
                return _googleLayer.WebProxy;
            }
            set
            {
                _googleLayer.WebProxy = value;
            }
        }

        public string ClientID
        {
            get
            {
                return _googleLayer.ClientId;
            }
            set
            {
                _googleLayer.ClientId = value;
            }
        }

        public string PrivateKey
        {
            get
            {
                return _googleLayer.PrivateKey;
            }
            set
            {
                _googleLayer.PrivateKey = value;
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
                layerTile.DrawingLayers.Add(_googleLayer);

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

        public string GetPictureFormatString()
        {
            switch (CachePictureFormat)
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

        protected override RectangleShape GetBoundingBoxCore()
        {
            return ExtentHelper.ZoomIn(new RectangleShape(-20001365, 20001365, 20001365, -20001365), 30);
        }


    }
}
