using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class MapPrinterLayer : BasePrinterLayer
    {
        readonly Collection<BaseLayer> _layers = new Collection<BaseLayer>();
 
        public RectangleShape MapExtent { get; set; }

        public RectangleShape LastMapExtent { get; set; }

        public GeoImage MapImageCache{ get; set; }

        public Collection<BaseLayer> Layers
        {
            get { return _layers; }
        }

        protected override void OpenCore()
        {
            foreach (BaseLayer layer in Layers)
            {
                layer.Open();
            }

            base.OpenCore();
        }

        protected override void CloseCore()
        {
            foreach (BaseLayer layer in Layers)
            {
                layer.Close();
            }
            base.CloseCore();
        }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            PrinterDrawHelper.DrawPrinterCore(this, canvas, labelsInAllLayers);
            PrinterDrawHelper.DrawMapPrinterCore(this, canvas, labelsInAllLayers);
        }

        public GeoImage GetCacheImage(PageGeoCanvas pageGeoCanvas, GeographyUnit unit, RectangleShape adjustedWorldExtent, Collection<SimpleCandidate> labelsInAllLayers)
        {
            RectangleShape currentBoundingBox = GetBoundingBox();

            if (MapImageCache == null)
            {
                int imageSize = 800;
                GeoImage cacheImage = new GeoImage(imageSize, imageSize);
                GdiPlusGeoCanvas tmpCanvas = new GdiPlusGeoCanvas();
                tmpCanvas.BeginDrawing(cacheImage, currentBoundingBox, unit);

                pageGeoCanvas.BeginDrawing(tmpCanvas, adjustedWorldExtent, GeographyUnit.Feet);

                pageGeoCanvas.EnableCliping = true;
                pageGeoCanvas.ClipingArea = adjustedWorldExtent;

                foreach (BaseLayer layer in Layers)
                {
                    layer.Open();
                    layer.Draw(pageGeoCanvas, labelsInAllLayers);
                    layer.Close();
                }

                pageGeoCanvas.EnableCliping = false;
                pageGeoCanvas.EndDrawing();
                tmpCanvas.EndDrawing();

                MapImageCache = cacheImage;
            }

            return MapImageCache;
        }
    }
}
