using System.Collections.ObjectModel;
using Mapgenix.Canvas;

namespace Mapgenix.Layers
{
    public class CompassPrinterLayer : BasePrinterLayer
    {
        public GeoImage NeedleImage { get; set; }

        public GeoImage FrameImage { get; set; }

        public float RotateAngle { get; set; }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            PrinterDrawHelper.DrawPrinterCore(this, canvas, labelsInAllLayers);
            PrinterDrawHelper.DrawCompassPrintersCore(this,canvas);


        }
    }
}