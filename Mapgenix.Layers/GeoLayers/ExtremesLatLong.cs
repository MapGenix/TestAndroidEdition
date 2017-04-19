
namespace Mapgenix.Layers
{

    public struct ExtremesLatLong
    {
        private double _maxLong;
        private double _minLong;
        private double _maxLat;
        private double _minLat;

        public ExtremesLatLong(double maxLat, double minLat, double maxLong, double minLong)
        {
            this._maxLat = maxLat; this._minLat = minLat; this._maxLong = maxLong; this._minLong = minLong;
        }

        public double MaxLong
        {
            get { return _maxLong; }
            set { _maxLong = value; }
        }

        public double MinLong
        {
            get { return _minLong; }
            set { _minLong = value; }
        }

        public double MaxLat
        {
            get { return _maxLat; }
            set { _maxLat = value; }
        }

        public double MinLat
        {
            get { return _minLat; }
            set { _minLat = value; }
        }
    }
}
