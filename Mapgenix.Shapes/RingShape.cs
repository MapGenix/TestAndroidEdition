using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>A closed ring of points to make up polygon.</summary>
    [Serializable]
    public class RingShape : BaseAreaShape
    {
        private Collection<Vertex> vertices;
       
        public RingShape()
            : this(new Vertex[] { })
        { }

        public RingShape(IEnumerable<Vertex> points)
        {
            Validators.CheckParameterIsNotNull(points, "points");

            this.vertices = new Collection<Vertex>();
            foreach (Vertex vertex in points)
            {
                this.vertices.Add(vertex);
            }

            if (this.vertices.Count > 1)
            {
                if (vertices[0].X != vertices[vertices.Count - 1].X || vertices[0].Y != vertices[vertices.Count - 1].Y)
                {
                    this.vertices.Add(vertices[0]);
                }
            }
        }

       public RingShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        public RingShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }
    
        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Polygon;
        }

        public Collection<Vertex> Vertices
        {
            get
            {
                return vertices;
            }
        }

        protected override BaseShape CloneDeepCore()
        {
            RingShape ringShape = new RingShape();

            foreach (Vertex vertex in vertices)
            {
                ringShape.vertices.Add(vertex);
            }

            return ringShape;
        }

        public PolygonShape ToPolygon()
        {
            Validators.CheckShapeIsValidForOperation(this);

            PolygonShape polygon = new PolygonShape();
            Collection<Vertex> vertiesCollection = polygon.OuterRing.Vertices;

            for (int i = 0; i < vertices.Count; i++)
            {
                vertiesCollection.Add(vertices[i]);
            }

            return polygon;
        }

        protected override double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            LineShape lineShape = new LineShape();

            for (int i = 0; i < vertices.Count; i++)
            {
                lineShape.Vertices.Add(vertices[i]);
            }

            return lineShape.GetLength(shapeUnit, returningUnit);
        }

        protected override double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double sumAFill, sumBFill;
            double x1, x2, y1, y2;
            AreaUnit shapeAreaUnit;

            sumAFill = sumBFill = 0;
            x1 = x2 = y1 = y2 = 0;

            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                shapeAreaUnit = AreaUnit.SquareMeters;
            }
            else
            {
                DistanceUnit unit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                shapeAreaUnit = GetAreaUnitFromLengthUnit(unit);
            }

            RingShape newRingShape = new RingShape();
            for (int i = 0; i < vertices.Count; i++)
            {
                newRingShape.vertices.Add(vertices[i]);
            }

            MakeRingShapeInTheSameHemiSphere(newRingShape);

            for (int i = 0; i < newRingShape.vertices.Count - 1; i++)
            {
                double pt1x = newRingShape.vertices[i].X;
                double pt1y = newRingShape.vertices[i].Y;

                double pt2x = newRingShape.vertices[i + 1].X;
                double pt2y = newRingShape.vertices[i + 1].Y;

                if (shapeUnit != GeographyUnit.DecimalDegree)
                {
                    x1 = pt1x; x2 = pt2x;
                    y1 = pt1y; y2 = pt2y;
                }
                else
                {
                    x1 = DecimalDegreesHelper.GetXFromDegreeOnSphere(pt1x, pt1y, DistanceUnit.Meter);
                    x2 = DecimalDegreesHelper.GetXFromDegreeOnSphere(pt2x, pt2y, DistanceUnit.Meter);
                    y1 = DecimalDegreesHelper.GetYFromDegreeOnSphere(pt1y, DistanceUnit.Meter);
                    y2 = DecimalDegreesHelper.GetYFromDegreeOnSphere(pt2y, DistanceUnit.Meter);
                }

                sumAFill += (x1 * y2);
                sumBFill += (x2 * y1);
            }

            double areaFill = Math.Abs((sumAFill - sumBFill) / 2);
            areaFill = Conversion.ConvertMeasureUnits(areaFill, shapeAreaUnit, returningUnit);

            return areaFill;
        }

        protected override void ScaleUpCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 + percentage / 100;
            Scale(factor);
        }

        protected override void ScaleDownCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 - percentage / 100;
            Scale(factor);
        }

       protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetBoundingBoxFromVertices(vertices);
        }

       protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);

            RingShape newRingShape = new RingShape();
            Vertex targetVertex;

            if (toUnit == GeographyUnit.DecimalDegree)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    double point01X = (i != 0) ? vertices[i - 1].X : fromPoint.X;
                    double point01Y = (i != 0) ? vertices[i - 1].Y : fromPoint.Y;

                    double point02X = vertices[i].X;
                    double point02Y = vertices[i].Y;

                    double differentX = point02X - point01X;
                    double differentY = point02Y - point01Y;
                    double longitude = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(differentX, fromUnit, toPoint.Y);
                    double latitude = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(differentY, fromUnit, toPoint.X);

                    if (double.IsNaN(longitude)) { longitude = 0; }
                    if (double.IsNaN(latitude)) { latitude = 0; }

                    double newTargetLongitude = (i != 0) ? newRingShape.vertices[i - 1].X : toPoint.X;
                    double newTargetLatitude = (i != 0) ? newRingShape.vertices[i - 1].Y : toPoint.Y;

                    if ((longitude != 0) || (latitude != 0))
                    {
                        targetVertex = new Vertex(newTargetLongitude + longitude, newTargetLatitude + latitude);
                    }
                    else
                    {
                        targetVertex = new Vertex(newTargetLongitude, newTargetLatitude);
                    }

                    newRingShape.vertices.Add(targetVertex);
                }
            }
            else
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    double point01X = (i != 0) ? vertices[i - 1].X : fromPoint.X;
                    double point01Y = (i != 0) ? vertices[i - 1].Y : fromPoint.Y;

                    double point02X = vertices[i].X;
                    double point02Y = vertices[i].Y;

                    DistanceUnit toDistanceUnit = Conversion.ConvertGeographyUnitToDistanceUnit(toUnit);

                    double valueX = Conversion.ConvertMeasureUnits(point02X - point01X, fromUnit, toDistanceUnit);
                    double valueY = Conversion.ConvertMeasureUnits(point02Y - point01Y, fromUnit, toDistanceUnit);

                    double targetX = (i != 0) ? newRingShape.vertices[i - 1].X : toPoint.X;
                    double targetY = (i != 0) ? newRingShape.vertices[i - 1].Y : toPoint.Y;

                    newRingShape.vertices.Add(new Vertex(targetX + valueX, targetY + valueY));
                }
            }

            return newRingShape;
        }

       protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex tempVertex = vertices[i];
                tempVertex.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
                vertices[i] = tempVertex;
            }
        }

       protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex tempVertex = vertices[i];
                tempVertex.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
                vertices[i] = tempVertex;
            }
        }

        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex tempVertex = vertices[i];
                tempVertex.Rotate(pivotPoint, degreeAngle);
                vertices[i] = tempVertex;
            }
        }

        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

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

            return ToPolygon().GetDistanceTo(targetShape, shapeUnit, distanceUnit);
        }

       protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            if (targetShape.GetType().Name == "EllipseShape")
            {
                if (targetShape.Intersects(this))
                {
                    return null;
                }
            }
            else
            {
                if (this.Intersects(targetShape))
                {
                    return null;
                }
            }

            return ToPolygon().GetClosestPointTo(targetShape, shapeUnit);
        }

        protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetWellKnownText();
        }

        protected override byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);

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

                ShapeConverter.WriteWkb(WkbShapeType.Polygon, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(1, byteOrder, wkbWriter);
                GetARingWellKnownBinary(byteOrder, wkbWriter);

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

        protected override void LoadFromWellKnownDataCore(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            PolygonShape ringShape = (PolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneRingToAnother(ringShape, this);
        }

        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            PolygonShape ringShape = (PolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneRingToAnother(ringShape, this);
        }

        public static bool RemoveVertex(Vertex selectedVertex, RingShape ringShape)
        {
            Validators.CheckParameterIsNotNull(ringShape, "ringShape");

            return ringShape.RemoveVertex(selectedVertex);
        }

        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = false;
            if (this.Vertices.Count > 4)
            {
                Collection<Vertex> removingVertices = new Collection<Vertex>();
                foreach (Vertex vertex in this.Vertices)
                {
                    if (vertex == selectedVertex)
                    {
                        removingVertices.Add(vertex);
                    }
                }

                if (removingVertices.Count == 1)
                {
                    deleteSucceed = this.Vertices.Remove(selectedVertex);
                }

                if (removingVertices.Count == 2)
                {
                    deleteSucceed = this.Vertices.Remove(selectedVertex);
                    deleteSucceed = this.Vertices.Remove(selectedVertex);

                    if (this.Vertices[0] != this.Vertices[this.Vertices.Count - 1])
                    {
                        this.Vertices.Add(this.Vertices[0]);
                    }
                }
            }

            return deleteSucceed;
        }

        private static void CloneRingToAnother(PolygonShape fromPolygon, RingShape toRing)
        {
            toRing.vertices = new Collection<Vertex>();

            for (int i = 0; i < fromPolygon.OuterRing.vertices.Count; i++)
            {
                toRing.vertices.Add(fromPolygon.OuterRing.vertices[i]);
            }
        }

       protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (vertices.Count <= 3)
                    {
                        validateResult = new ShapeValidationResult(false, ExceptionDescription.RingShapeIsInvalidForOperationVertexCountLessThan4);
                    }
                    break;
                case ShapeValidationMode.Advanced:
                    break;
                default:
                    break;
            }

            return validateResult;
        }

        private static void MakeRingShapeInTheSameHemiSphere(RingShape ringShape)
        {
            double minX = ringShape.vertices[0].X;
            double minY = ringShape.vertices[0].Y;
            double maxY = ringShape.vertices[0].Y;

            foreach (Vertex vertex in ringShape.vertices)
            {
                minX = Math.Min(vertex.X, minX);
                minY = Math.Min(vertex.Y, minY);
                maxY = Math.Max(vertex.Y, maxY);
            }

            if (minX < 0)
            {
                ringShape.TranslateByOffset(Math.Abs(minX), 0, GeographyUnit.Meter, DistanceUnit.Meter);
            }

            if (minY < 0 && maxY > 0)
            {
                if (Math.Abs(minY) > Math.Abs(maxY))
                {
                    ringShape.TranslateByOffset(0, -maxY, GeographyUnit.Meter, DistanceUnit.Meter);
                }
                else
                {
                    ringShape.TranslateByOffset(0, Math.Abs(minY), GeographyUnit.Meter, DistanceUnit.Meter);
                }
            }

        }

        private void Scale(double multiplicator)
        {
            double totalX = 0;
            double totalY = 0;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                totalX += vertices[i].X;
                totalY += vertices[i].Y;
            }

            double alpha = totalX / (vertices.Count - 1);
            double beta = totalY / (vertices.Count - 1);

            ScaleOneRing(this, multiplicator, alpha, beta);
        }

        internal static void ScaleOneRing(RingShape targetRingShape, double multiplicator, double centerX, double centerY)
        {
            for (int i = 0; i < targetRingShape.Vertices.Count; i++)
            {
                double newX = ((targetRingShape.Vertices[i].X - centerX) * multiplicator) + centerX;
                double newY = ((targetRingShape.Vertices[i].Y - centerY) * multiplicator) + centerY;
                targetRingShape.Vertices[i] = new Vertex(newX, newY);
            }
        }

        private void GetARingWellKnownBinary(WkbByteOrder byteOrder, BinaryWriter writer)
        {
            ShapeConverter.WriteWkb(vertices.Count, byteOrder, writer);
            for (int i = 0; i < vertices.Count; i++)
            {
                ShapeConverter.WriteWkb(vertices[i].X, byteOrder, writer);
                ShapeConverter.WriteWkb(vertices[i].Y, byteOrder, writer);
            }
        }

         public bool IsCounterClockwise()
        {

            Vertex hightestPoint = new Vertex(double.MinValue, double.MinValue);
            int hightestPointNumber = 0;
            int count = vertices.Count;
            for (int i = 0; i < count; i++)
            {
                if (vertices[i].Y > hightestPoint.Y)
                {
                    hightestPoint = vertices[i];
                    hightestPointNumber = i;
                }
            }

            int previousPointNumber = hightestPointNumber - 1;
            if (previousPointNumber < 0)
            {
                previousPointNumber = count - 2;
            }
            Vertex previousPoint = vertices[previousPointNumber];

            int nextPointNumber = hightestPointNumber + 1;
            if (nextPointNumber >= count)
            {
                nextPointNumber = 1;
            }
            Vertex nextPoint = vertices[nextPointNumber];

            double prev2x = previousPoint.X - hightestPoint.X;
            double prev2y = previousPoint.Y - hightestPoint.Y;
            double next2x = nextPoint.X - hightestPoint.X;
            double next2y = nextPoint.Y - hightestPoint.Y;
            
            double disc = next2x * prev2y - next2y * prev2x;
           
            bool isCounterClockwise = false;

            if (disc == 0.0)
            {
                isCounterClockwise = previousPoint.X > nextPoint.X;
            }
            else
            {
                isCounterClockwise = disc > 0.0;
            }

            return isCounterClockwise;
        }

        public void ReversePoints()
        {
            Validators.CheckShapeIsValidForOperation(this);

            ReversePointsCore();
        }

        protected virtual void ReversePointsCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Collection<Vertex> tempPoints = new Collection<Vertex>();
            for (int i = vertices.Count - 1; i >= 0; i--)
            {
                tempPoints.Add(vertices[i]);
            }

            vertices = tempPoints;
        }

        internal bool IsSelSelfIntersecting()
        {
            bool isSelSelfIntersecting = false;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vertex vertex1a = vertices[i];
                Vertex vertex1b = vertices[i + 1];
                for (int j = (i + 2); j < vertices.Count - 1; j++)
                {
                    Vertex vertex2a = vertices[j];
                    Vertex vertex2b = vertices[j + 1];

                    if (DoesLineSegmentCompletelyIntersectLineSegment(vertex1a, vertex1b, vertex2a, vertex2b))
                    {
                        isSelSelfIntersecting = true;
                        break;
                    }
                }
                if (isSelSelfIntersecting) { break; }
            }
            return isSelSelfIntersecting;
        }

        private static bool DoesLineSegmentCompletelyIntersectLineSegment(Vertex Pt1, Vertex Pt2, Vertex CompPt1, Vertex CompPt2)
        {
            bool result = false;

            if (DoesLineSegmentIntersectLineSegment(Pt1, Pt2, CompPt1, CompPt2) == true)
            {
                if (DoesPointShapeBelongToLineSegment(Pt1, CompPt1, CompPt2) | DoesPointShapeBelongToLineSegment(Pt2, CompPt1, CompPt2) | DoesPointShapeBelongToLineSegment(CompPt1, Pt1, Pt2) | DoesPointShapeBelongToLineSegment(CompPt2, Pt1, Pt2))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private static bool DoesLineSegmentIntersectLineSegment(Vertex InPt1, Vertex InPt2, Vertex InCompPt1, Vertex InCompPt2)
        {
            double a;
            double b;
            double ap;
            double bp;
            double xi;
            double yi;
            double x1 = InPt1.X;
            double x2 = InPt2.X;
            double y1 = InPt1.Y;
            double y2 = InPt2.Y;
            double xp1 = InCompPt1.X;
            double xp2 = InCompPt2.X;
            double yp1 = InCompPt1.Y;
            double yp2 = InCompPt2.Y;
            bool Result = false;

           
            if (DoesPointBelongToLineSegment(x1, y1, xp1, yp1, xp2, yp2) | DoesPointBelongToLineSegment(x2, y2, xp1, yp1, xp2, yp2) | DoesPointBelongToLineSegment(xp1, yp1, x1, y1, x2, y2) | DoesPointBelongToLineSegment(xp2, yp2, x1, y1, x2, y2))
            {
                Result = true;
            }

            if (Result == false)
            {

                if ((x2 - x1 != 0) & (xp2 - xp1 != 0))
                {
                    a = (y2 - y1) / (x2 - x1);
                    b = y1 - (a * x1);
                    ap = (yp2 - yp1) / (xp2 - xp1);
                    bp = yp1 - (ap * xp1);
                    if (a - ap != 0)
                    {
                        xi = (bp - b) / (a - ap);
                        if (y1 - y2 == 0)
                        {
                            yi = y1;
                        }
                        else if (yp1 - yp2 == 0)
                        {
                            yi = yp1;
                        }
                        else
                        {
                            yi = (a * xi) + b;
                        }
                        if (IsBetweenInclusive(yi, y1, y2) == true & IsBetweenInclusive(yi, yp1, yp2) == true & IsBetweenInclusive(xi, x1, x2) == true & IsBetweenInclusive(xi, xp1, xp2) == true)
                        {
                            Result = true;
                        }
                    }
                    else
                    {
                        Result = false;
                    }
                }
                else
                {
                    if ((x2 - x1 == 0) & (xp2 - xp1 == 0))
                    {
                        Result = false;
                    }
                    else if (x2 - x1 == 0 & (xp2 - xp1 != 0))
                    {
                        ap = (yp2 - yp1) / (xp2 - xp1);
                        bp = yp1 - (ap * xp1);
                        xi = x1;
                        yi = (ap * xi) + bp;
                        if (IsBetweenInclusive(yi, y1, y2) == true & IsBetweenInclusive(yi, yp1, yp2) == true & IsBetweenInclusive(xi, x1, x2) == true & IsBetweenInclusive(xi, xp1, xp2) == true)
                        {
                            Result = true;
                        }
                    }
                    else if ((xp2 - xp1 == 0) & (x2 - x1 != 0))
                    {
                        a = (y2 - y1) / (x2 - x1);
                        b = y1 - (a * x1);
                        xi = xp1;
                        yi = (a * xi) + b;
                        if (IsBetweenInclusive(yi, y1, y2) == true & IsBetweenInclusive(yi, yp1, yp2) == true & IsBetweenInclusive(xi, x1, x2) == true & IsBetweenInclusive(xi, xp1, xp2) == true)
                        {
                            Result = true;
                        }

                    }
                }
            }

            return Result;
        }

        private static bool DoesPointBelongToLineSegment(double x1, double y1, double xp1, double yp1, double xp2, double yp2)
        {
            double a = 0;
            double b = 0;
            bool Result = false;
            if ((x1 == xp1 & y1 == yp1) | (x1 == xp2 & y1 == yp2))
            {
                Result = true;
            }
            else
            {
                if (xp1 != xp2)
                {
                    a = (yp2 - yp1) / (xp2 - xp1);
                    b = yp1 - (a * xp1);

                    if (Math.Round(y1, 8) == Math.Round((a * x1) + b, 8) & x1 >= Math.Min(xp1, xp2) & x1 <= Math.Max(xp1, xp2)) // 3
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
                else
                {
                    if (x1 == xp1 & (y1 >= Math.Min(yp1, yp2) & y1 <= Math.Max(yp1, yp2)))
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
            }
            return Result;
        }

        private static bool IsBetweenInclusive(double Value, double CompValue1, double CompValue2)
        {
            bool result = false;

            double IsBetweenInclusive_Lower = Math.Min(CompValue1, CompValue2);
            double IsBetweenInclusive_Upper = Math.Max(CompValue1, CompValue2);
            if (Value >= IsBetweenInclusive_Lower && Value <= IsBetweenInclusive_Upper)
            {
                result = true;
            }

            return result;
        }

        private static bool DoesPointShapeBelongToLineSegment(Vertex PointShape1, Vertex LinePointShape1, Vertex LinePointShape2)
        {
            bool Result = false;
            double a = 0;
            double b = 0;

            if ((PointShape1.X == LinePointShape1.X & PointShape1.Y == LinePointShape1.Y) | (PointShape1.X == LinePointShape2.X & PointShape1.Y == LinePointShape2.Y))
            {
                Result = true;
            }
            else
            {
                if (LinePointShape1.X != LinePointShape2.X)
                {
                    a = (LinePointShape2.Y - LinePointShape1.Y) / (LinePointShape2.X - LinePointShape1.X);
                    b = LinePointShape1.Y - (a * LinePointShape1.X);

                    if (Math.Round(PointShape1.Y, 10) == Math.Round((a * PointShape1.X) + b, 10) & PointShape1.X >= Math.Min(LinePointShape1.X, LinePointShape2.X) & PointShape1.X <= Math.Max(LinePointShape1.X, LinePointShape2.X))
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
                else
                {
                    if (PointShape1.X == LinePointShape1.X & (PointShape1.Y >= Math.Min(LinePointShape1.Y, LinePointShape2.Y) & PointShape1.Y <= Math.Max(LinePointShape1.Y, LinePointShape2.Y)))
                    {
                        Result = true;
                    }
                    else
                    {
                        Result = false;
                    }
                }
            }
            return Result;
        }

        private static RectangleShape GetBoundingBoxFromVertices(IEnumerable<Vertex> vertices)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (Vertex vertex in vertices)
            {
                if (minX > vertex.X) { minX = vertex.X; }
                if (minY > vertex.Y) { minY = vertex.Y; }
                if (maxX < vertex.X) { maxX = vertex.X; }
                if (maxY < vertex.Y) { maxY = vertex.Y; }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static AreaUnit GetAreaUnitFromLengthUnit(DistanceUnit distanceUnit)
        {
            AreaUnit areaUnit = AreaUnit.Acres;
            switch (distanceUnit)
            {
                case DistanceUnit.Feet:
                    areaUnit = AreaUnit.SquareFeet;
                    break;

                case DistanceUnit.Kilometer:
                    areaUnit = AreaUnit.SquareKilometers;
                    break;

                case DistanceUnit.Meter:
                    areaUnit = AreaUnit.SquareMeters;
                    break;

                case DistanceUnit.Mile:
                    areaUnit = AreaUnit.SquareMiles;
                    break;

                case DistanceUnit.Yard:
                    areaUnit = AreaUnit.SquareYards;
                    break;

                case DistanceUnit.UsSurveyFeet:
                    areaUnit = AreaUnit.SquareUsSurveyFeet;
                    break;

                default:
                    break;
            }

            return areaUnit;
        }

    }
}
