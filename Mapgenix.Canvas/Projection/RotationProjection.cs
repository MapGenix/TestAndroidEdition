using System;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Projection class to apply rotation to a FeatureSource based layer.</summary>
    /// <remarks>Usefull for directional maps such as driving maps with a rotation according to a vehicle heading.</remarks>
    [Serializable]
    public class RotationProjection : BaseProjection
    {
        private Vertex _pivotVertex;
        private double _previousRotateAngle;
        private double _rotateAngle;
        private GeographyUnit _sourceUnit;

        /// <summary>Constructor of the class.</summary>
        /// <remarks>Default constructor. Sets the angle to 0 (North).</remarks>
        /// <returns>None</returns>
        public RotationProjection()
            : this(0, GeographyUnit.Unknown)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <remarks>Angle to change the rotation of the layer.</remarks>
        /// <returns>None</returns>
        /// <param name="angle">Angle to rotate the map to.</param>
        public RotationProjection(double angle)
            : this(angle, GeographyUnit.Unknown)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <param name="sourceUnit">Geography unit of the layer</param>
        public RotationProjection(GeographyUnit sourceUnit)
            : this(0, sourceUnit)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <param name="angle">Angle to rotate the layer to.</param>
        /// <param name="sourceUnit">Geography unit of the layer</param>
        public RotationProjection(double angle, GeographyUnit sourceUnit)
        {
            _rotateAngle = angle;
            _sourceUnit = sourceUnit;
        }

        /// <summary>Gets or sets the angle of rotation of the layer.</summary>
        public double Angle
        {
            get { return _rotateAngle; }
            set { _rotateAngle = value; }
        }

        /// <summary>Gets or sets the unit of the layer.</summary>
        public GeographyUnit SourceUnit
        {
            get { return _sourceUnit; }
            set { _sourceUnit = value; }
        }

        /// <summary>Returns an adjusted extend based on the angle of rotation.</summary>
        /// <returns>Adjusted extend based on the angle of rotation.</returns>
        /// <remarks>It is important to update the current extent every time the angle of the
        /// projection is changed.</remarks>
        /// <param name="worldExtent">Extent of the map before the rotation.</param>
        public RectangleShape GetUpdatedExtent(RectangleShape worldExtent)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");

            var centerPoint = worldExtent.GetCenterPoint();

            _pivotVertex = RotateVertex(centerPoint.X, centerPoint.Y, _previousRotateAngle*-1);

            _previousRotateAngle = _rotateAngle;

            var horizontalRadius = worldExtent.Width/2;
            var verticalRadius = worldExtent.Height/2;

            var upperLeftPoint = new PointShape(_pivotVertex.X - horizontalRadius, _pivotVertex.Y + verticalRadius);
            var lowerRightPoint = new PointShape(_pivotVertex.X + horizontalRadius, _pivotVertex.Y - verticalRadius);

            var result = new RectangleShape(upperLeftPoint, lowerRightPoint);
            result.Id = worldExtent.Id;
            return result;
        }

        /// <summary>Returns projected vertices based on the coordinates passed in.</summary>
        /// <returns>Projected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to project.</param>
        /// <param name="y">Y values of the points to project.</param>
        protected override Vertex[] ConvertToExternalProjectionCore(double[] x, double[] y)
        {
            Validators.CheckParameterIsNotNull(x, "x");
            Validators.CheckParameterIsNotNull(y, "y");

            var verteies = new Vertex[x.Length];

            for (var i = 0; i < verteies.Length; i++)
            {
                verteies[i] = RotateVertex(x[i], y[i], _rotateAngle);
            }

            return verteies;
        }

        /// <summary>Returns reprojected vertices based on the coordinates passed in.</summary>
        /// <returns>Reprojected vertices based on the coordinates passed in.</returns>
        /// <remarks>Returns a projected vertex to the internal projection based on the coordinates passed in. You need to override this method.</remarks>
        /// <param name="x">X values of the points to reproject.</param>
        /// <param name="y">Y values of the points to reproject.</param>
        protected override Vertex[] ConvertToInternalProjectionCore(double[] x, double[] y)
        {
            Validators.CheckParameterIsNotNull(x, "x");
            Validators.CheckParameterIsNotNull(y, "y");

            var vertices = new Vertex[x.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = RotateVertex(x[i], y[i], _rotateAngle*-1);
            }

            return vertices;
        }

        /// <summary>Returns the geographic unit of the internal projection.</summary>
        /// <returns>geographic unit of the internal projection.</returns>
        protected override GeographyUnit GetInternalGeographyUnitCore()
        {
            return _sourceUnit;
        }

        private Vertex RotateVertex(double x, double y, double angle)
        {
            var rotatedVertex = new Vertex(x, y);
            if ((angle%360) != 0)
            {
                var rotatedX = x;
                var rotatedY = y;

                var distanceToPivot = Math.Sqrt(Math.Pow((x - _pivotVertex.X), 2) + Math.Pow((y - _pivotVertex.Y), 2));
                if (distanceToPivot != 0)
                {
                    var beta = Math.Atan((y - _pivotVertex.Y)/(x - _pivotVertex.X));
                    if ((beta <= 0 | y < _pivotVertex.Y) && x < _pivotVertex.X)
                    {
                        beta = Math.PI + beta;
                    }

                    var radiantAngle = angle*Math.PI/180;
                    rotatedX = (distanceToPivot*Math.Cos(radiantAngle + beta)) + _pivotVertex.X;
                    rotatedY = (distanceToPivot*Math.Sin(radiantAngle + beta)) + _pivotVertex.Y;
                }

                rotatedVertex = new Vertex(rotatedX, rotatedY);
            }

            return rotatedVertex;
        }
    }
}