using Mapgenix.Canvas;
using System;

namespace Mapgenix.GSuite.Android
{
    public class GeoCanvasEventArgs : EventArgs
    {
        public GeoCanvasEventArgs(BaseGeoCanvas geoCanvas)
            : this(geoCanvas, false)
        { }

        public GeoCanvasEventArgs(BaseGeoCanvas geoCanvas, bool cancel)
        {
            GeoCanvas = geoCanvas;
            Cancel = cancel;
        }

        public BaseGeoCanvas GeoCanvas { get; set; }

        public bool Cancel { get; set; }
    }
}
