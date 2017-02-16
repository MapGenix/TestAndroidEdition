using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ScaleChangedEventArgs : EventArgs
    {
        private double _currentScale;
        private GeographyUnit _mapUnit;
        private float _mapWidth;
        private float _mapHeight;
        private RectangleShape _currentExtent;

        public ScaleChangedEventArgs()
            : this(new RectangleShape(), 0, GeographyUnit.Unknown, 0, 0)
        { }

        public ScaleChangedEventArgs(RectangleShape currentExtent, double currentScale, GeographyUnit mapUnit, float mapWidth, float mapHeight)
        {
            this._currentExtent = currentExtent;
            this._currentScale = currentScale;
            this._mapUnit = mapUnit;
            this._mapWidth = mapWidth;
            this._mapHeight = mapHeight;
        }

        public double CurrentScale
        {
            get { return _currentScale; }
            set { _currentScale = value; }
        }

        public GeographyUnit MapUnit
        {
            get { return _mapUnit; }
            set { _mapUnit = value; }
        }

        public float MapWidth
        {
            get { return _mapWidth; }
            set { _mapWidth = value; }
        }

        public float MapHeight
        {
            get { return _mapHeight; }
            set { _mapHeight = value; }
        }

        public RectangleShape CurrentExtent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }
    }
}
