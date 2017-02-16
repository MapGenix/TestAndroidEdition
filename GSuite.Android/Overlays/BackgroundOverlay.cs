using System;
using Mapgenix.Layers;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Content;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from the Overlay abstract class. The overlay 
    /// specifies the background information of the Map Control.
    /// </summary>
    [Serializable]
    public class BackgroundOverlay : BaseOverlay
    {
        private BackgroundLayer _backgroundLayer;
        private LayerTile _tile;

        public BackgroundOverlay(Context context)
            :base(context)
        {
            _backgroundLayer = LayerFactory.CreateBackgroundLayer();

            _tile = new LayerTile(context);
            _tile.IsAsync = false;
            _tile.DrawingLayers.Add(_backgroundLayer);
            OverlayCanvas.AddView(_tile);
        }

        public BaseGeoBrush BackgroundBrush
        {
            get
            {
                if (_backgroundLayer.BackgroundBrush == null)
                {
                    _backgroundLayer.BackgroundBrush = new GeoSolidBrush(GeoColor.StandardColors.Transparent);
                }
                return _backgroundLayer.BackgroundBrush;
            }
            set
            {
                Validators.CheckParameterIsNotNull(value, "BackgroundBrush");
                _backgroundLayer.BackgroundBrush = value;
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
          
            if (refreshType == OverlayRefreshType.Pan) { return; }
            _tile.DrawingExceptionMode = DrawingExceptionMode;

            LayoutParams p = new LayoutParams(Convert.ToInt32(MapArguments.ActualWidth), Convert.ToInt32(MapArguments.ActualHeight));
            _tile.LayoutParameters = p;
            _tile.TargetExtent = targetExtent;

            GdiPlusGeoCanvas geoCanvas = new GdiPlusGeoCanvas();
            using (Bitmap nativeImage = Bitmap.CreateBitmap((int)MapArguments.ActualWidth, (int)MapArguments.ActualHeight, Bitmap.Config.Argb8888))
            //using (System.Drawing.Bitmap nativeImage = new System.Drawing.Bitmap((int)MapArguments.ActualWidth, (int)MapArguments.ActualHeight))
            {
                geoCanvas.DrawingQuality = DrawingQuality.HighSpeed;
                geoCanvas.BeginDrawing(nativeImage, targetExtent, MapArguments.MapUnit);
                _tile.Draw(geoCanvas);
                geoCanvas.EndDrawing();

                _tile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
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
    }
}
