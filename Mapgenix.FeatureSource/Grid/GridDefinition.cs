using System.Collections.Generic;
using Mapgenix.Shapes;

namespace Mapgenix.FeatureSource
{
    public class GridDefinition
    {
        private RectangleShape _gridExtent;
        private readonly Dictionary<PointShape, double> _dataPoints;
        private double _cellSize;
        private double _noDataValue;

        public GridDefinition()
            : this(new RectangleShape(), 0, 0, new Dictionary<PointShape, double>())
        { }

        public GridDefinition(RectangleShape gridExtent, double cellSize, double noDataValue, Dictionary<PointShape, double> dataPoints)
        {
            this._gridExtent = gridExtent;
            this._cellSize = cellSize;
            this._noDataValue = noDataValue;
            this._dataPoints = dataPoints;
        }

        public RectangleShape GridExtent
        {
            get { return _gridExtent; }
            set { _gridExtent = value; }
        }

        public Dictionary<PointShape, double> DataPoints
        {
            get { return _dataPoints; }
        }

        public double CellSize
        {
            get { return _cellSize; }
            set { _cellSize = value; }
        }

        public double NoDataValue
        {
            get { return _noDataValue; }
            set { _noDataValue = value; }
        }
    }
}
