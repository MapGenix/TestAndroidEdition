using System;
using Mapgenix.Canvas;

namespace Mapgenix.GSuite.Android
{
    public class TileEventArgs : EventArgs
    {
        public TileEventArgs(Tile drawnTile, BaseGeoCanvas geoCanvas)
        {
            Tile = drawnTile;
            GeoCanvas = geoCanvas;
        }

        public Tile Tile { get; set; }

        public BaseGeoCanvas GeoCanvas { get; set; }
    }
}
