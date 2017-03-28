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
    /// Overlay requesting tiles from OpenStreetMap Web Service.
    /// </summary>
    [Serializable]
    public class OpenStreetMapOverlay : BaseTileOverlay
    {
        private const int DefaultTileWidth = 400;
        private const int DefaultTileHeight = 400;
        private OpenStreetMapAndroidLayer _osmLayer;

        public OpenStreetMapOverlay(Context context)
            : this(context, null, string.Empty)
        { }

        public OpenStreetMapOverlay(Context context, WebProxy webProxy, string cacheDirectory)
            :base(context)
        {
            _osmLayer = new OpenStreetMapAndroidLayer(webProxy, cacheDirectory);

            _osmLayer.TimeoutInSeconds = 10;
            TileWidth = DefaultTileWidth;
            TileHeight = DefaultTileHeight;
        }

        public int TimeoutInSeconds { get; set; }

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
                layerTile.DrawingLayers.Add(_osmLayer);

                Bitmap nativeImage = Bitmap.CreateBitmap((int)tile.LayoutParameters.Width, (int)tile.LayoutParameters.Height, Bitmap.Config.Argb8888);
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                //tile.IsAsync = false;
                if (tile.IsAsync)
                {
                    layerTile.DrawAsync(geoCanvas);
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


    }
}
