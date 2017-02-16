using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ScaleChangingEventArgs : EventArgs
    {
        private double _currentScale;
        private double _newScale;
        private GeographyUnit _mapUnit;
        private float _mapWidth;
        private float _mapHeight;
        private bool _cancel;
        private RectangleShape _currentExtent;

        public ScaleChangingEventArgs()
            : this(new RectangleShape(), 0, 0,GeographyUnit.Unknown, 0,0,false)
        { }

        public ScaleChangingEventArgs(RectangleShape currentExtent, double currentScale, double newScale, GeographyUnit mapUnit, float mapWidth, float mapHeight,bool cancel)
        {
            this._currentExtent = currentExtent;
            this._currentScale = currentScale;
            this._newScale = newScale;
            this._mapUnit = mapUnit;
            this._mapWidth = mapWidth;
            this._mapHeight = mapHeight;
            this._newScale = newScale;
            this._cancel = cancel;
        }

        public double Scale
        {
            get { return _currentScale; }
            set { _currentScale = value; }
        }

        public double NewScale
        {
            get { return _newScale; }
            set { _newScale = value; }
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

        public RectangleShape Extent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
