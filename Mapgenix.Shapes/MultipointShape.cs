using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;

using Mapgenix.Utils;
using NetTopologySuite.Geometries;

namespace Mapgenix.Shapes
{
    /// <summary>Shape made of one or more points.</summary>
    [Serializable]
    public class MultipointShape : BasePoint
    {
        private Collection<PointShape> points;

       
        public MultipointShape()
            : this(new PointShape[] { })
        { }

      
        public MultipointShape(IEnumerable<PointShape> points)
        {
            Validators.CheckParameterIsNotNull(points, "points");

            this.points = new Collection<PointShape>();
            foreach (PointShape pointShape in points)
            {
                this.points.Add(pointShape);
            }
        }

      
        public MultipointShape(IEnumerable<Feature> pointFeatures)
        {
            Validators.CheckParameterIsNotNull(pointFeatures, "pointFeatures");

            points = new Collection<PointShape>();
            foreach (Feature pointFeature in pointFeatures)
            {
                BaseShape baseShape = pointFeature.GetShape();
                Validators.CheckShapeIsPointShape(baseShape);

                points.Add((PointShape)baseShape);
            }
        }

       
        public MultipointShape(Feature multipointFeature)
        {
            BaseShape shape = multipointFeature.GetShape();
            Validators.CheckShapeIsMultipointShape(shape);

            points = ((MultipointShape)shape).Points;
            Id = multipointFeature.Id;
        }

        
        public MultipointShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

       
        public MultipointShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

       
        public override bool CanRotate
        {
            get
            {
                return true;
            }
        }

       
        public Collection<PointShape> Points
        {
            get
            {
                return points;
            }
        }

       
        protected override BaseShape CloneDeepCore()
        {
            MultipointShape multipoint = new MultipointShape();

            for (int i = 0; i < points.Count; i++)
            {
                PointShape point = new PointShape(points[i].X, points[i].Y);
                multipoint.points.Add(point);
            }

            return multipoint;
        }

       
        public void ScaleUp(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            ScaleUpCore(percentage);
        }

      
        protected virtual void ScaleUpCore(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            float factor = (float)(1 + (percentage / 100));
            Scale(factor);
        }

        
        public void ScaleDown(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            ScaleDownCore(percentage);
        }

       
        protected virtual void ScaleDownCore(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            float factor = (float)(1 - (percentage / 100));
            Scale(factor);
        }

        private void Scale(float multiplicator)
        {
            double totalX = 0;
            double totalY = 0;
            int pointCount = Points.Count;
            for (int i = 0; i <= pointCount - 1; i++)
            {
                totalX = totalX + points[i].X;
                totalY = totalY + points[i].Y;
            }

            double xValue = 0;
            double yValue = 0;
            double alpha = totalX / pointCount;
            double beta = totalY / pointCount;
            foreach (PointShape point in points)
            {
                xValue = ((point.X - alpha) * multiplicator) + alpha;
                yValue = ((point.Y - beta) * multiplicator) + beta;
                point.X = xValue;
                point.Y = yValue;
            }
        }

        public RingShape ConvexHull()
        {
            Validators.CheckShapeIsValidForOperation(this);
            return ConvexHullCore();
        }

        protected virtual RingShape ConvexHullCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Geometry jtsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry geometry = (Geometry)jtsGeometry.ConvexHull();
            BaseShape resultBaseShape = ShapeConverter.JtsShapeToShape(geometry);

            Validators.CheckIfTypeIsCorrect(resultBaseShape, typeof(PolygonShape), "ConvexHull");

            return ((PolygonShape)resultBaseShape).OuterRing;
        }

        protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");

            MultipointShape resultMultiPoint = new MultipointShape();
            PointShape newPointShape = new PointShape();

            for (int i = 0; i <= this.Points.Count - 1; i++)
            {
                if (i == 0)
                {
                    newPointShape = (PointShape)(this.Points[i].Register(fromPoint, toPoint, fromUnit, toUnit));
                }
                else
                {
                    if (toUnit == GeographyUnit.DecimalDegree)
                    {
                        double xDif = this.Points[i].X - this.Points[i - 1].X;
                        double yDif = this.Points[i].Y - this.Points[i - 1].Y;
                        double longitudeDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xDif, fromUnit, toPoint.Y);
                        double latitudeDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yDif, fromUnit, toPoint.X);

                        if (double.IsNaN(longitudeDif))
                        {
                            longitudeDif = 0;
                        }

                        if (double.IsNaN(latitudeDif))
                        {
                            latitudeDif = 0;
                        }

                        if (longitudeDif != 0 || latitudeDif != 0)
                        {
                            newPointShape = new PointShape(resultMultiPoint.Points[resultMultiPoint.Points.Count - 1].X + longitudeDif, resultMultiPoint.Points[resultMultiPoint.Points.Count - 1].Y + latitudeDif);
                        }
                    }
                    else
                    {
                        newPointShape = (PointShape)(this.Points[i].Register(this.Points[i - 1], resultMultiPoint.Points[resultMultiPoint.Points.Count - 1], fromUnit, toUnit));
                    }
                }

                resultMultiPoint.Points.Add(newPointShape);
            }

            return resultMultiPoint;
        }

       
        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            foreach (PointShape pointShape in points)
            {
                pointShape.TranslateByOffset(xOffsetDistance, yOffsetDistance,shapeUnit, distanceUnit);
            }
        }

       
        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            foreach (PointShape pointShape in points)
            {
                pointShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            }
        }

        
        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            foreach (PointShape point in points)
            {
                point.Rotate(pivotPoint, degreeAngle);
            }
        }

        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            double minDistance = double.MaxValue;
            PointShape closestPointShape = null;

            if (targetShape.GetType().Name == "EllipseShape")
            {
                if (targetShape.Intersects(this))
                {
                    return closestPointShape;
                }
            }
            else
            {
                if (this.Intersects(targetShape))
                {
                    return closestPointShape;
                }
            }

            foreach (PointShape pointShape in this.Points)
            {
                double currentDistance = pointShape.GetDistanceTo(targetShape, shapeUnit, DistanceUnit.Meter);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = pointShape;
                }
            }

            return closestPointShape;
        }

       protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetBoundingBoxFromPoints(points);
        }

        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
      
            double minDistance = double.MaxValue;

            foreach (PointShape point in points)
            {
                double currentDistance = point.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                }
            }

            return minDistance;
        }

        protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            StringBuilder wellKnownText = new StringBuilder();
            wellKnownText.Append("MULTIPOINT(");
            string wktString = string.Empty;

            for (int i = 0; i < this.Points.Count; i++)
            {
                wktString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", Points[i].X, Points[i].Y);
                wellKnownText.Append(wktString);
                if (i < (this.Points.Count - 1))
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");

            return wellKnownText.ToString();
        }

        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Multipoint;
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
                ShapeConverter.WriteWkb(WkbShapeType.Multipoint, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(points.Count, byteOrder, wkbWriter);

                foreach (PointShape point in points)
                {
                    if (byteOrder == WkbByteOrder.LittleEndian)
                    {
                        wkbWriter.Write((byte)1);
                    }
                    else
                    {
                        wkbWriter.Write((byte)0);
                    }
                    ShapeConverter.WriteWkb(WkbShapeType.Point, byteOrder, wkbWriter);
                    ShapeConverter.WriteWkb(point.X, byteOrder, wkbWriter);
                    ShapeConverter.WriteWkb(point.Y, byteOrder, wkbWriter);
                }

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

            MultipointShape multipoint = (MultipointShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOnePointToAnother(multipoint, this);
        }

       
        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            MultipointShape multipoint = (MultipointShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneOnePointToAnother(multipoint, this);
        }

        private static void CloneOnePointToAnother(MultipointShape fromMultipoint, MultipointShape toMultipoint)
        {
            toMultipoint.points = new Collection<PointShape>();

            for (int i = 0; i < fromMultipoint.points.Count; i++)
            {
                toMultipoint.points.Add(fromMultipoint.points[i]);
            }
        }
      
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);
            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (Points.Count == 0)
                    {
                        validateResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForValidation);
                    }
                    break;
            }

            return validateResult;
        }

        
        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            PointShape targetPointShape = targetShape as PointShape;

            if (targetPointShape != null)
            {
                resultMultiPointShape = targetPointShape.GetCrossing(this);
            }
            else
            {
                MultipointShape multipointShape = targetShape as MultipointShape;
                if (multipointShape != null)
                {
                    for (int i = 0; i < multipointShape.Points.Count; i++)
                    {
                        for (int j = 0; j < Points.Count; j++)
                        {
                            if (multipointShape.Points[i].X == Points[j].X && multipointShape.Points[i].Y == Points[j].Y)
                            {
                                resultMultiPointShape.Points.Add(multipointShape.Points[i]);
                                break;
                            }
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

       
        public static bool RemoveVertex(Vertex selectedVertex, MultipointShape multipointShape)
        {
            Validators.CheckParameterIsNotNull(multipointShape, "");

            return multipointShape.RemoveVertex(selectedVertex);
        }

        
        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = false;
            if (this.Points.Count > 1)
            {
                for (int i = 0; i < this.Points.Count; i++)
                {
                    if (this.Points[i].X == selectedVertex.X && this.Points[i].Y == selectedVertex.Y)
                    {
                        deleteSucceed = this.Points.Remove(this.Points[i]);
                    }
                }
            }
            return deleteSucceed;
        }

        private static RectangleShape GetBoundingBoxFromPoints(IEnumerable<PointShape> points)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (PointShape point in points)
            {
                if (minX > point.X) { minX = point.X; }
                if (minY > point.Y) { minY = point.Y; }
                if (maxX < point.X) { maxX = point.X; }
                if (maxY < point.Y) { maxY = point.Y; }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }
    }
}
