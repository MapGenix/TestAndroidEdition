using System;
using Mapgenix.Shapes;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class ExtentChangingEventArgs : EventArgs
    {
        private RectangleShape _currentExtent;
        private RectangleShape _newExtent;
        private GeographyUnit _mapUnit;
        private float _mapWidth;
        private float _mapHeight;
        private bool _cancel;

        public ExtentChangingEventArgs(RectangleShape currentExtent, RectangleShape newExtent, GeographyUnit mapUnit, float mapWidth, float mapHeight, bool cancel)
        {
            this._currentExtent = currentExtent;
            this._mapUnit = mapUnit;
            this._mapWidth = mapWidth;
            this._mapHeight = mapHeight;
            this._newExtent = newExtent;
            this._cancel = cancel;
        }

        public RectangleShape Extent
        {
            get { return _currentExtent; }
            set { _currentExtent = value; }
        }

        public RectangleShape NewExtent
        {
            get { return _newExtent; }
            set { _newExtent = value; }
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

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }
    }
}
