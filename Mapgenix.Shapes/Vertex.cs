using System;
using System.Globalization;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Vertex (point) making up a shape such as a polygon or a line.</summary>
    [Serializable]
    public struct Vertex
    {
        private double x;
        private double y;
       
        public Vertex(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        
        public Vertex(PointShape point)
        {
            Validators.CheckParameterIsNotNull(point, "point");

            this.x = point.X;
            this.y = point.Y;
        }

        public double X
        {
            get
            {
                return x;
            }
            set
            {
                this.x = value;
            }
        }

       
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                this.y = value;
            }
        }

        
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X: {0:F4}; Y: {1:F4}", x, y);
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode();
        }

       
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Vertex)
            {
                return Equals((Vertex)obj);
            }
            else
            {
                return false;
            }
        }

        private bool Equals(Vertex compareObj)
        {
            Vertex vertex = (Vertex)compareObj;
            return (x == vertex.x) && (y == vertex.y);
        }

       
        public Vertex Add(Vertex targetVertex)
        {
            return (this + targetVertex);
        }

      
        public static Vertex operator +(Vertex vertex1, Vertex vertex2)
        {
            return new Vertex(vertex1.x + vertex2.x, vertex1.y + vertex2.y);
        }

      
        public static bool operator ==(Vertex vertex1, Vertex vertex2)
        {
            return vertex1.Equals(vertex2);
        }

      
        public static bool operator !=(Vertex vertex1, Vertex vertex2)
        {
            return !(vertex1 == vertex2);
        }

       
        public static Vertex FindMiddleVertexBetweenTwoVertices(Vertex vertex1, Vertex vertex2)
        {
            double x, y;

            x = (vertex1.X + vertex2.X) / 2;
            y = (vertex1.Y + vertex2.Y) / 2;

            return new Vertex(x, y);
        }

        internal void TranslateByOffset(double xOffset, double yOffset, GeographyUnit shapeUnit, DistanceUnit unitOfOffset)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
      
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                double latDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yOffset, unitOfOffset, X);
                double longDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xOffset, unitOfOffset, Y);
                x = x + longDif;
                y = y + latDif;
            }
            else
            {
                DistanceUnit distanceUnitOfThisShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                x = x + Conversion.ConvertMeasureUnits(xOffset, distanceUnitOfThisShape, unitOfOffset);
                y = y + Conversion.ConvertMeasureUnits(yOffset, distanceUnitOfThisShape, unitOfOffset);
            }
        }

        internal void TranslateByDegree(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 1, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);

            PointShape newPointShape = new PointShape();

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThisShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                double offset = Conversion.ConvertMeasureUnits(distance, distanceUnitOfThisShape, distanceUnit);
                newPointShape = GetPointShapeFromDistanceAndDegree(offset, (Single)angleInDegrees);
            }
            else
            {
                double newLatitude = DecimalDegreesHelper.GetLatitudeFromDistanceAndDegree(X, Y, distance, distanceUnit, angleInDegrees);
                double newLongitude = DecimalDegreesHelper.GetLongitudeFromDistanceAndDegree(X, Y, distance, distanceUnit, angleInDegrees);
                newPointShape = new PointShape(newLongitude, newLatitude);
            }

            double xOffset = newPointShape.X - X;
            double yOffset = newPointShape.Y - Y;
            x = x + xOffset;
            y = y + yOffset;
        }

        internal void Rotate(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);

            double radiantAngle = Conversion.DegreesToRadians(degreeAngle);

            double distance = GetDistanceFromVertex(pivotPoint.X, pivotPoint.Y);
            if (distance != 0)
            {
                double tangentBeta = (y - pivotPoint.Y) / (x - pivotPoint.X);
                double beta = Math.Atan(tangentBeta);
                if ((beta <= 0 || y < pivotPoint.Y))
                {
                    if (x < pivotPoint.X)
                    {
                        beta = Math.PI + beta;
                    }
                }

                x = (distance * Math.Cos(radiantAngle + beta)) + pivotPoint.X;
                y = (distance * Math.Sin(radiantAngle + beta)) + pivotPoint.Y;
            }
        }

        internal double GetDistanceTo(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "unifOfThisShape");
      
            PointShape point = new PointShape(x, y);
            return point.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
        }

        internal double GetDistanceTo(Vertex targetVertex, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "unifOfThisShape");
            
            double distance = 0;
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                distance = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(x, y, targetVertex.x, targetVertex.y, distanceUnit);
            }
            else
            {
                distance = GetDistanceFromVertex(targetVertex.x, targetVertex.y);
                DistanceUnit convertUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                distance = Conversion.ConvertMeasureUnits(distance, convertUnit, distanceUnit);
            }

            return distance;
        }

        internal double GetDistanceFromVertex(double toX, double toY)
        {
            return Math.Sqrt(Math.Pow((this.X - toX), 2) + Math.Pow((this.Y - toY), 2));
        }

        private PointShape GetPointShapeFromDistanceAndDegree(double offset, float degree)
        {
            double newX = 0;
            double newY = 0;

            if (degree >= 360)
            {
                degree = degree - 360;
            }
            else if (degree < 0)
            {
                degree = 360 - Math.Abs(degree);
            }

            if (degree != 0 & degree != 180)
            {
                double a = Math.Tan(Conversion.DegreesToRadians(90) - Conversion.DegreesToRadians(degree));
                double b = this.Y - (a * this.X);
                double bigA = Math.Pow(a, 2) + 1;
                double bigB = (2 * a * b) - (2 * this.X) - (2 * a * this.Y);
                double bigC = Math.Pow(this.X, 2) + Math.Pow(b, 2) - (2 * b * this.Y) + Math.Pow(this.Y, 2) - Math.Pow(offset, 2);
                double delta = Math.Pow(bigB, 2) - (4 * bigA * bigC);

                if (degree > 0 && degree < 180)
                {
                    newX = (-bigB + Math.Sqrt(Math.Abs(delta))) / (2 * bigA);
                }
                else if (degree > 180 && degree < 360)
                {
                    newX = (-bigB - Math.Sqrt(Math.Abs(delta))) / (2 * bigA);
                }

                newY = (a * newX) + b;
            }

            else if (degree == 0)
            {
                newX = this.X;
                newY = this.Y + offset;
            }
            else if (degree == 180)
            {
                newX = this.X;
                newY = this.Y - offset;
            }

            return new PointShape(newX, newY);
        }
    }
}