using System;
using System.Drawing;
using System.Windows.Media;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Utils;
using Mapgenix.Layers;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from the Overlay abstract class. This overlay stores
    /// the <strong>AdornmentLayer</strong> of the Map Control.
    /// </summary>
    [Serializable]
    public class AdornmentOverlay : BaseOverlay
    {
        private LogoAdornmentLayer _logoLayer;
        private SafeCollection<BaseAdornmentLayer> _layers;
        private LayerTile _tile;

        [NonSerialized]
        private TranslateTransform _panTransform;

        public AdornmentOverlay()
        {
            ShowLogo = false;
            _logoLayer = new LogoAdornmentLayer();
            _layers = new SafeCollection<BaseAdornmentLayer>();

            _panTransform = new TranslateTransform();
            OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.AdornmentOverlay);
            OverlayCanvas.RenderTransform = _panTransform;

            _tile = new LayerTile();
            _tile.HasWatermark = false;
            _tile.IsAsync = false;
            OverlayCanvas.Children.Add(_tile);
        }

        public SafeCollection<BaseAdornmentLayer> Layers { get { return _layers; } }

        public bool ShowLogo { get; set; }

        public override bool IsEmpty
        {
            get
            {
                bool isEmpty = (Layers.Count == 0 && !ShowLogo);
                if (isEmpty)
                {
                    foreach (Tile currentTile in OverlayCanvas.Children)
                    {
                        currentTile.Dispose();
                    }

                    OverlayCanvas.Children.Clear();
                }
                return isEmpty;
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            _panTransform.X = _panTransform.Y = 0;
            InitializeTile(targetExtent);

            if (!IsEmpty)
            {
                GdiPlusGeoCanvas geoCanvas = new GdiPlusGeoCanvas();
                using (Bitmap nativeImage = new Bitmap((int)MapArguments.ActualWidth, (int)MapArguments.ActualHeight))
                {
                    geoCanvas.DrawingQuality = DrawingQuality.Default;
                    geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                    _tile.Draw(geoCanvas);
                    geoCanvas.EndDrawing();
                    _tile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
                }
            }

            OnDrawn(new ExtentEventArgs(targetExtent));
        }

      
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _tile != null)
            {
                _tile.Dispose();
            }
        }

        private void InitializeTile(RectangleShape targetExtent)
        {
            if (OverlayCanvas.Children.Count == 0)
            {
                OverlayCanvas.Children.Add(_tile);
            }

            _tile.DrawingExceptionMode = DrawingExceptionMode;
            _tile.Width = MapArguments.ActualWidth;
            _tile.Height = MapArguments.ActualHeight;
            _tile.DrawingLayers.Clear();
            foreach (BaseAdornmentLayer layer in _layers)
            {
                _tile.DrawingLayers.Add(layer);
            }
            if (ShowLogo) { _tile.DrawingLayers.Add(_logoLayer); }
            _tile.TargetExtent = targetExtent;
        }
    }
}
