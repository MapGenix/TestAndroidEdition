using System;
using System.Globalization;
using System.IO;
using Mapgenix.Utils;
using NetTopologySuite.Geometries;

namespace Mapgenix.Shapes
{
    /// <summary>Shape made of a single point.</summary>
    [Serializable]
    public class PointShape : BasePoint
    {
        private double pointX, pointY, pointZ;

        public PointShape()
            : this(0, 0, 0)
        {
        }

        public PointShape(double x, double y)
            : this(x, y, 0)
        {
        }

       
        public PointShape(double x, double y, double z)
        {
            pointX = x;
            pointY = y;
            pointZ = z;
        }

     
        public PointShape(Vertex vertex)
            : this(vertex.X, vertex.Y, 0)
        {
        }

       
        public PointShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

       
        public PointShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

      
        public double X
        {
            get
            {
                return pointX;
            }
            set
            {
                pointX = value;
            }
        }

     
        public double Y
        {
            get
            {
                return pointY;
            }
            set
            {
                pointY = value;
            }
        }

        public double Z
        {
            get
            {
                return pointZ;
            }
            set
            {
                pointZ = value;
            }
        }

       
        public bool Equal2D(PointShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");

            if ((X == targetShape.X) && (Y == targetShape.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public bool Equal2D(Feature targetPointFeature)
        {
            BaseShape targetBaseShape = targetPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(targetBaseShape);

            return Equal2D((PointShape)targetBaseShape);
        }


        protected override BaseShape CloneDeepCore()
        {
            PointShape point = new PointShape(X, Y);

            return point;
        }

       
        protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");

            PointShape resultPointShape = new PointShape();
            double longitudeDif = 0;
            double latitudeDif = 0;
            double xDif = 0;
            double yDif = 0;
            double destX = toPoint.X;
            double destY = toPoint.Y;

            if (toUnit == GeographyUnit.DecimalDegree)
            {
                xDif = this.X - fromPoint.X;
                yDif = this.Y - fromPoint.Y;
                longitudeDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xDif, fromUnit, toPoint.Y);
                latitudeDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yDif, fromUnit, toPoint.X);
                if (double.IsNaN(longitudeDif) || Math.Abs(longitudeDif) == 360)
                {
                    longitudeDif = 0;
                }
                if (double.IsNaN(latitudeDif))
                {
                    latitudeDif = 0;
                }

                if (longitudeDif != 0 || latitudeDif != 0)
                {
                    resultPointShape = new PointShape(destX + longitudeDif, destY + latitudeDif);
                }
            }
            else
            {
                DistanceUnit toDistanceUnit = Conversion.ConvertGeographyUnitToDistanceUnit(toUnit);
                xDif = Conversion.ConvertMeasureUnits(this.X - fromPoint.X, fromUnit, toDistanceUnit);
                yDif = Conversion.ConvertMeasureUnits(this.Y - fromPoint.Y, fromUnit, toDistanceUnit);
                resultPointShape = new PointShape(destX + xDif, destY + yDif);
            }

            return resultPointShape;
        }

       
        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThisShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                X = X + Conversion.ConvertMeasureUnits(xOffsetDistance, distanceUnit, distanceUnitOfThisShape);
                Y = Y + Conversion.ConvertMeasureUnits(yOffsetDistance, distanceUnit, distanceUnitOfThisShape);
            }
            else
            {
                double latDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yOffsetDistance, distanceUnit, X);
                double longDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xOffsetDistance, distanceUnit, Y);
                X = X + longDif;
                Y = Y + latDif;
            }
        }

        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);

            PointShape newPointShape = new PointShape();

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThisShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                double offset = Conversion.ConvertMeasureUnits(distance, distanceUnit, distanceUnitOfThisShape);
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
            X = X + xOffset;
            Y = Y + yOffset;
        }

        
        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            double radiantAngle = Conversion.DegreesToRadians(degreeAngle);
            double distance = GetDistanceFromPointShape(pivotPoint);
            if (distance != 0)
            {
                double tangentBeta = (Y - pivotPoint.Y) / (X - pivotPoint.X);
                double beta = Math.Atan(tangentBeta);
                if (beta <= 0 || Y < pivotPoint.Y)
                {
                    if (X < pivotPoint.X)
                    {
                        beta = Math.PI + beta;
                    }
                }

                double xValue = (distance * Math.Cos(radiantAngle + beta)) + pivotPoint.X;
                double yValue = (distance * Math.Sin(radiantAngle + beta)) + pivotPoint.Y;
                X = xValue;
                Y = yValue;
            }
        }

       
        public override bool CanRotate
        {
            get
            {
                return true;
            }
        }

       
        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            if (targetShape.Intersects(this))
            {
                return null;
            }

            PointShape retPointShape = new PointShape(X, Y);
            return retPointShape;
        }

        
        protected override string GetWellKnownTextCore()
        {
            string wellKnownText = string.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", X, Y);
            return wellKnownText;
        }

        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Point;
        }

       
        protected override void LoadFromWellKnownDataCore(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            PointShape point = (PointShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            this.X = point.X;
            this.Y = point.Y;
        }

        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            PointShape point = (PointShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            this.X = point.X;
            this.Y = point.Y;
        }


        protected override byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);
            MemoryStream wkbStream = null;
            BinaryWriter wkbWriter = null;

            byte[] wkbArray = null;

            try
            {
                wkbStream = new MemoryStream();
                wkbWriter = new BinaryWriter(wkbStream);

                if (byteOrder == WkbByteOrder.LittleEndian)
                {
                    wkbWriter.Write((byte)1);
                }
                else
                {
                    wkbWriter.Write((byte)0);
                }
                ShapeConverter.WriteWkb(WkbShapeType.Point, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(this.pointX, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(this.pointY, byteOrder, wkbWriter);

                wkbWriter.Flush();
                wkbArray = wkbStream.ToArray();
            }
            finally
            {
                if (wkbWriter != null) { wkbWriter.Close(); }
                if (wkbStream != null) { wkbStream.Close(); }
            }

            return wkbArray;
        }

        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validationResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    break;
                case ShapeValidationMode.Advanced:
                    break;
                default:
                    break;
            }

            return validationResult;
        }

      
        public MultilineShape GreatCircle(PointShape pointShape)
        {
            RectangleShape rectangle = new RectangleShape(-180, 90, 180, -90);
            if (rectangle.Contains(pointShape) && rectangle.Contains(this))
            {
                return DecimalDegreesHelper.GetGreatCircle(this, pointShape, 1000);
            }
            else
            {
                return null;
            }
        }

        public MultilineShape GreatCircle(Feature pointFeature)
        {
            BaseShape baseShape = pointFeature.GetShape();
            Validators.CheckShapeIsPointShape(baseShape);

            return GreatCircle((PointShape)baseShape);
        }

        
        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "unifOfThisShape");
    
            try
            {
                if (targetShape.GetType().Name == "EllipseShape")
                {
                    if (targetShape.Intersects(this))
                    {
                        return 0;
                    }
                }
                else
                {
                    if (this.Intersects(targetShape))
                    {
                        return 0;
                    }
                }
            }
            catch (TopologyException)
            {
                return 0;
            }


            switch (targetShape.GetType().Name)
            {
                case "PointShape":
                    return GetDistanceToPointShape((PointShape)targetShape, shapeUnit, distanceUnit);

                case "MultipointShape":
                    return GetDistanceToMultiPoint((MultipointShape)targetShape, shapeUnit, distanceUnit);

                case "LineShape":
                    return GetDistanceToLineShape((LineShape)targetShape, shapeUnit, distanceUnit);

                case "MultilineShape":
                    return GetDistanceToMultiLine((MultilineShape)targetShape, shapeUnit, distanceUnit);

                case "RingShape":
                    return GetDistanceToRingShape((RingShape)targetShape, shapeUnit, distanceUnit);

                case "PolygonShape":
                    return GetDistanceToPolygon((PolygonShape)targetShape, shapeUnit, distanceUnit);

                case "MultipolygonShape":
                    return GetDistanceToMultiPolygonShape((MultipolygonShape)targetShape, shapeUnit, distanceUnit);

                case "RectangleShape":
                    return GetDistanceToRectangle((RectangleShape)targetShape, shapeUnit, distanceUnit);

                case "EllipseShape":
                    return ((EllipseShape)targetShape).GetDistanceTo(this, shapeUnit, distanceUnit);

                default:
                    return base.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
            }
        }

        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            PointShape targetPointShape = targetShape as PointShape;

            if (targetPointShape != null)
            {

                if (targetPointShape.X == X && targetPointShape.Y == Y)
                {
                    resultMultiPointShape.Points.Add(targetPointShape);
                }
            }
            else
            {
                MultipointShape targetMultipointShape = targetShape as MultipointShape;
                if (targetMultipointShape != null)
                {
                    for (int i = 0; i < targetMultipointShape.Points.Count; i++)
                    {
                        if (targetMultipointShape.Points[i].X == X && targetMultipointShape.Points[i].Y == Y)
                        {
                            resultMultiPointShape.Points.Add(this);
                            break;
                        }
                    }
                }
                else
                {
                    resultMultiPointShape = targetShape.GetCrossing(this);
                }
            }

            return resultMultiPointShape;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", X, Y, Z);
        }

        protected override bool IsWithinCore(BaseShape targetShape)
        {
            bool returnValue = true;
            if (targetShape is RectangleShape)
            {
                RectangleShape rectangleShape = (RectangleShape)targetShape;

                if (this.X < rectangleShape.UpperLeftPoint.X)
                {
                    returnValue = false;
                    return returnValue;
                }

                if (this.X > rectangleShape.LowerRightPoint.X)
                {
                    returnValue = false;
                    return returnValue;
                }

                if (this.Y > rectangleShape.UpperLeftPoint.Y)
                {
                    returnValue = false;
                    return returnValue;
                }

                if (this.Y < rectangleShape.LowerRightPoint.Y)
                {
                    returnValue = false;
                    return returnValue;
                }
            }
            else
            {
                returnValue = base.IsWithinCore(targetShape);
            }

            return returnValue;
        }

        private double GetDistanceToRingShape(RingShape ringShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            return GetDistanceTo(ringShape.ToPolygon(), shapeUnit, distanceUnit);
        }

        private double GetDistanceToMultiPolygonShape(MultipolygonShape multipolygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double minDistance = double.MaxValue;
            double currentDistance = 0;

            foreach (PolygonShape polygon in multipolygonShape.Polygons)
            {
                currentDistance = GetDistanceTo(polygon, shapeUnit, distanceUnit);

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            return minDistance;
        }

        private double GetDistanceToMultiPoint(MultipointShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double minDistance = double.MaxValue;
            double currentDistance = 0;

            foreach (PointShape pointShape in targetShape.Points)
            {
                currentDistance = GetDistanceTo(pointShape, shapeUnit, distanceUnit);

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            return minDistance;
        }

        private double GetDistanceToMultiLine(MultilineShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double minDistance = double.MaxValue;
            double currentDistance = 0;

            foreach (LineShape lineShape in targetShape.Lines)
            {
                currentDistance = GetDistanceTo(lineShape, shapeUnit, distanceUnit);

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            return minDistance;
        }

        private double GetDistanceToLineShape(LineShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double minDistance = double.MaxValue;
            double currentDistance = 0;

            for (int i = 0; i < targetShape.Vertices.Count - 1; i++)
            {
                Vertex lineStartVertex = targetShape.Vertices[i];
                Vertex lineEndVertex = targetShape.Vertices[i + 1];

                if (shapeUnit == GeographyUnit.DecimalDegree)
                {
                    currentDistance = DecimalDegreesHelper.GetDistanceFromDecimalDegreesLine(lineStartVertex.X, lineStartVertex.Y, lineEndVertex.X, lineEndVertex.Y, this, distanceUnit);
                }
                else
                {
                    currentDistance = GetDistanceFromLineSegment(lineStartVertex.X, lineStartVertex.Y, lineEndVertex.X, lineEndVertex.Y);
                }

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit convertShapeUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                minDistance = Conversion.ConvertMeasureUnits(minDistance, convertShapeUnit, distanceUnit);
            }

            return minDistance;
        }

        private double GetDistanceToRectangle(RectangleShape rectangleShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            return GetDistanceTo(rectangleShape.ToPolygon(), shapeUnit, distanceUnit);
        }

        private double GetDistanceToPointShape(PointShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double distance = 0;
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                distance = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(this, targetShape, distanceUnit);
            }
            else
            {
                distance = GetDistanceFromPointShape(targetShape);
                DistanceUnit convertUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                distance = Conversion.ConvertMeasureUnits(distance, convertUnit, distanceUnit);
            }

            return distance;
        }

        private double GetDistanceToPolygon(PolygonShape polygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double currentDistance = 0;
            double minDistance = double.MaxValue;

            MultilineShape multilineShape = PolygonShape.ToLineShapes(polygonShape);
            foreach (LineShape lineShape in multilineShape.Lines)
            {
                currentDistance = GetDistanceTo(lineShape, shapeUnit, distanceUnit);

                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            return minDistance;
        }

        private double GetDistanceFromPointShape(PointShape toPointShape)
        {
            Validators.CheckParameterIsNotNull(this, "fromPointShape");
            Validators.CheckParameterIsNotNull(toPointShape, "toPointShape");

            return Math.Sqrt(Math.Pow((this.X - toPointShape.X), 2) + Math.Pow((this.Y - toPointShape.Y), 2));
        }

        internal double GetDistanceFromLineSegment(double fromPointX, double fromPointY, double toPointX, double toPointY)
        {
            PointShape NearestPointShape = GetNearestPointShapeFromLineSegment(fromPointX, fromPointY, toPointX, toPointY);
            return GetDistanceFromPointShape(NearestPointShape);
        }

        internal PointShape GetNearestPointShapeFromLineSegment(double fromX, double fromY, double toX, double toY)
        {
            double a;
            double b;
            double ap;
            double bp;
            double X;
            double Y;
            double xp;
            double yp;
            double x1;
            double y1;
            double x2;
            double y2;
            PointShape PointShapeI = new PointShape();

            if ((fromX > toX))
            {
                x1 = toX;
                y1 = toY;
                x2 = fromX;
                y2 = fromY;
            }
            else
            {
                x1 = fromX;
                y1 = fromY;
                x2 = toX;
                y2 = toY;
            }
            xp = this.X;
            yp = this.Y;

            if ((x2 != x1) && (y2 != y1))
            {
                a = (y2 - y1) / (x2 - x1);
                b = y1 - (a * x1);
                ap = -1 / a;
                bp = yp - (ap * xp);
                X = (bp - b) / (a - ap);
                Y = (a * X) + b;
                if (X > x1 & X < x2)
                {
                    PointShapeI.X = X;
                    PointShapeI.Y = Y;
                }
                else if (X <= x1)
                {
                    PointShapeI.X = x1;
                    PointShapeI.Y = y1;
                }
                else
                {
                    PointShapeI.X = x2;
                    PointShapeI.Y = y2;
                }
            }
            else if (x1 == x2)
            {
                if ((yp > y1) && (yp < y2) || (yp > y2) && (yp < y1))
                {
                    PointShapeI.X = x1;
                    PointShapeI.Y = yp;
                }
                else if (((y2 < y1) && (yp >= y1)) || ((y1 < y2) && (yp < y1)))
                {
                    PointShapeI.X = x1;
                    PointShapeI.Y = y1;
                }
                else
                {
                    PointShapeI.X = x2;
                    PointShapeI.Y = y2;
                }
            }
            else
            {
                if ((xp > x1) && (xp < x2))
                {
                    PointShapeI.X = xp;
                    PointShapeI.Y = y1;
                }
                else if (xp <= x1)
                {
                    PointShapeI.X = x1;
                    PointShapeI.Y = y1;
                }
                else
                {
                    PointShapeI.X = x2;
                    PointShapeI.Y = y2;
                }
            }
            return PointShapeI;
        }

        private PointShape GetPointShapeFromDistanceAndDegree(double offset, float degree)
        {
            PointShape newPointShape = new PointShape();

            if (degree >= 360)
            {
                degree = degree - 360;
            }
            else if (degree < 0)
            {
                degree = 360 - Math.Abs(degree);
            }

            if (degree != 0 && degree != 180)
            {
                double a = Math.Tan(Conversion.DegreesToRadians(90) - Conversion.DegreesToRadians(degree));
                double b = this.Y - (a * this.X);
                double bigA = Math.Pow(a, 2) + 1;
                double bigB = (2 * a * b) - (2 * this.X) - (2 * a * this.Y);
                double bigC = Math.Pow(this.X, 2) + Math.Pow(b, 2) - (2 * b * this.Y) + Math.Pow(this.Y, 2) - Math.Pow(offset, 2);
                double delta = Math.Pow(bigB, 2) - (4 * bigA * bigC);

                if (degree > 0 && degree < 180)
                {
                    newPointShape.X = (-bigB + Math.Sqrt(Math.Abs(delta))) / (2 * bigA);
                }
                else if (degree > 180 && degree < 360)
                {
                    newPointShape.X = (-bigB - Math.Sqrt(Math.Abs(delta))) / (2 * bigA);
                }

                newPointShape.Y = (a * newPointShape.X) + b;
            }

            else if (degree == 0)
            {
                newPointShape.X = this.X;
                newPointShape.Y = this.Y + offset;
            }
            else if (degree == 180)
            {
                newPointShape.X = this.X;
                newPointShape.Y = this.Y - offset;
            }

            return newPointShape;
        }
    }
}
