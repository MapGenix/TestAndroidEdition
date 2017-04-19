using System;

namespace Mapgenix.FeatureSource
{
    [Serializable]
    public class GridCell
    {
        private readonly double _x;
        private readonly double _y;
        private readonly double _value;

        public GridCell(GridCell oldPoint)
            : this(oldPoint._x, oldPoint._y, oldPoint._value)
        {
        }

        public GridCell(double pointX, double pointY, double pointValue)
        {
            this._x = pointX;
            this._y = pointY;
            this._value = pointValue;
        }

        public double CenterX
        {
            get { return _x; }
        }

        public double CenterY
        {
            get { return _y; }
        }

        public double Value
        {
            get { return _value; }
        }
    }
}
