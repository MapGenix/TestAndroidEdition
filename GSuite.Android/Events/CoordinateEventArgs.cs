using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class CoordinateEventArgs : EventArgs
    {

        public CoordinateEventArgs(PointShape worldCoordinate)
        {
            WorldCoordinate = worldCoordinate;
        }

        public PointShape WorldCoordinate { get; set; }

        public string Result { get; set; }
    }
}