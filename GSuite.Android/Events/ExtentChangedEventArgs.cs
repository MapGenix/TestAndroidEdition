using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ExtentChangedEventArgs : EventArgs
    {
        private RectangleShape _currentExtent;
        private GeographyUnit _mapUnit;
        private float _mapWidth;
        private float _mapHeight;

        public ExtentChangedEventArgs(RectangleShape currentExtent, GeographyUnit mapUnit, float mapWidth, float mapHeight)
        {
            this._currentExtent = currentExtent;
            this._mapUnit = mapUnit;
            this._mapWidth = mapWidth;
            this._mapHeight = mapHeight;
        }

        public RectangleShape Extent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
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
    }
}
