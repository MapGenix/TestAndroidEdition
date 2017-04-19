using System;
using Mapgenix.FeatureSource;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    [Serializable]
    public class InverseDistanceWeightedGridInterpolationModel : BaseGridInterpolationModel
    {
        private double _power;
        private double _searchRadius;

        public InverseDistanceWeightedGridInterpolationModel()
            : this(2, double.MaxValue)
        {
        }

        public InverseDistanceWeightedGridInterpolationModel(double power, double searchRadius)
        {
            this._power = power;
            this._searchRadius = searchRadius;
        }

        public double Power
        {
            get { return _power; }
            set { _power = value; }
        }

        public double SearchRadius
        {
            get { return _searchRadius; }
            set { _searchRadius = value; }
        }

        protected override double InterpolateCore(RectangleShape cellExtent, GridDefinition gridDefinition)
        {
            double cellCenterX = cellExtent.GetCenterPoint().X;
            double cellCenterY = cellExtent.GetCenterPoint().Y;
            double cellValue = gridDefinition.NoDataValue;
            double zx1 = 0;
            double zx2 = 0;

            foreach (PointShape closePoint in gridDefinition.DataPoints.Keys)
            {
                double distance = Math.Sqrt(Math.Pow((cellCenterX - closePoint.X), 2) + Math.Pow((cellCenterY - closePoint.Y), 2));
                if (distance < _searchRadius)
                {
                    double alpha = Math.Pow((1 / distance), _power);
                    zx1 = zx1 + alpha * (gridDefinition.DataPoints[closePoint]);
                    zx2 = zx2 + alpha;
                }
            }

            if (zx1 != 0 && zx2 != 0)
            {
                cellValue = (zx1 / zx2);
            }

            return cellValue;
        }
    }
}
