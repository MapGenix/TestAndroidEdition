using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using Mapgenix.Canvas;
using Mapgenix.Layers;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>Group of layers. Each layer can be of any type.</summary>
    [Serializable]
    public class LayerOverlay : BaseTileOverlay
    {
        private static readonly int LimitThreadCount = Environment.ProcessorCount;
        private int _limitThreadIndex;
        private SafeCollection<BaseLayer> _layers;
        private Collection<Collection<BaseLayer>> _copiedLayers;

        public LayerOverlay(Context context)
            : this(context, new Collection<BaseLayer>())
        { }

        public LayerOverlay(Context context, IEnumerable<BaseLayer> layers)
            :base(context)
        {
            _copiedLayers = new Collection<Collection<BaseLayer>>();
            this._layers = new SafeCollection<BaseLayer>();
            foreach (BaseLayer layer in layers)
            {
                this._layers.Add(layer);
            }
        }

        public RenderMode RenderMode { get; set; }

        public override bool IsEmpty
        {
            get
            {
                bool isEmpty = _layers.Count == 0;
                if (isEmpty)
                {
                    //ClearTiles(StretchCanvas);
                    //ClearTiles(DrawingCanvas);
                }
                return isEmpty;
            }
        }

        /*private static void ClearTiles(System.Windows.Controls.Canvas canvas)
        {
            foreach (Tile tile in canvas.Children)
            {
                tile.Dispose();
            }

            canvas.Children.Clear();
        }*/


        public SafeCollection<BaseLayer> Layers
        {
            get { return _layers; }
        }


        public LockLayerMode LockLayerMode { get; set; }


        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            if (refreshType == OverlayRefreshType.Redraw && LockLayerMode == LockLayerMode.DoNotLock)
            {
                lock (_copiedLayers)
                {
                    _copiedLayers.Clear();
                    for (int i = 0; i < LimitThreadCount; i++)
                    {
                        Collection<BaseLayer> oneCopies = new Collection<BaseLayer>();
                        foreach (BaseLayer layer in Layers)
                        {
                            lock (layer)
                            {
                                BaseLayer copiedLayer = layer;
                                if (TileType != TileType.SingleTile)
                                {
                                    copiedLayer = GetCopiedLayer(layer);
                                }
                                oneCopies.Add(copiedLayer);
                            }
                        }
                        _copiedLayers.Add(oneCopies);
                    }
                }
            }

            base.DrawCore(targetExtent, refreshType);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var layer in Layers)
                {
                    Monitor.Enter(layer);
                    layer.Close();
                    Monitor.Exit(layer);
                }
            }
        }

        private static BaseLayer GetCopiedLayer(BaseLayer sourceLayer)
        {
            bool isOpened = false;
            BaseFeatureLayer sourceFeatureLayer = sourceLayer as BaseFeatureLayer;
            if (sourceLayer.IsOpen)
            {
                sourceLayer.Close();
                if (sourceFeatureLayer != null &&
                    sourceFeatureLayer.FeatureSource.Projection != null &&
                    sourceFeatureLayer.FeatureSource.Projection.IsOpen)
                {
                    sourceFeatureLayer.FeatureSource.Projection.Close();
                }
                isOpened = true;
            }
            else if (sourceFeatureLayer != null && sourceFeatureLayer.FeatureSource.Projection != null && sourceFeatureLayer.FeatureSource.Projection.IsOpen)
            {
                sourceFeatureLayer.FeatureSource.Projection.Close();
            }
            
            BaseLayer copiedLayer = SerializationHelper.CloneDeep(sourceLayer);
            if (isOpened) sourceLayer.Open();
            return copiedLayer;
        }




        protected override Tile GetTileCore()
        {
            Tile tile = new LayerTile(Context);
            return tile;
        }

        protected override void DrawTileCore(Tile tile, RectangleShape targetExtent)
        {
            LayerTile layerTile = tile as LayerTile;
            lock (layerTile.DrawingLayers)
            {
                layerTile.DrawingLayers.Clear();
                if (LockLayerMode == LockLayerMode.DoNotLock)
                {
                    Collection<BaseLayer> oneCopies = _copiedLayers[_limitThreadIndex++];
                    foreach (BaseLayer layer in oneCopies)
                    {
                        layerTile.DrawingLayers.Add(layer);
                    }
                    if (_limitThreadIndex >= LimitThreadCount) _limitThreadIndex = 0;
                }
                else
                {
                    foreach (BaseLayer layer in _layers)
                    {
                        layerTile.DrawingLayers.Add(layer);
                    }
                }
            }

            int tw = (int)tile.LayoutParameters.Width;
            int th = (int)tile.LayoutParameters.Height;

            object nativeImage = null;
            
            if (TileCache != null) { layerTile.TileCache = TileCache; }
            GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context)
            {
                //CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed,
                DrawingQuality = DrawingQuality.HighSpeed,
                //SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed
            };
            nativeImage = Bitmap.CreateBitmap(tw, th, Bitmap.Config.Argb8888);
            geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
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

        protected override RectangleShape GetBoundingBoxCore()
        {
            double left = double.MaxValue;
            double right = double.MinValue;
            double top = double.MinValue;
            double bottom = double.MaxValue;

            foreach (BaseLayer layer in Layers)
            {
                if (layer.HasBoundingBox)
                {
                    if (!layer.IsOpen) { layer.Open(); }
                    RectangleShape layerBox = layer.GetBoundingBox();
                    left = left < layerBox.LowerLeftPoint.X ? left : layerBox.LowerLeftPoint.X;
                    right = right > layerBox.LowerRightPoint.X ? right : layerBox.LowerRightPoint.X;
                    top = top > layerBox.UpperLeftPoint.Y ? top : layerBox.UpperLeftPoint.Y;
                    bottom = bottom < layerBox.LowerRightPoint.Y ? bottom : layerBox.LowerRightPoint.Y;
                }
            }

            if (left > right || top < bottom)
            {
                return null;
            }
            else
            {
                return new RectangleShape(left, top, right, bottom);
            }
        }
    }
}
