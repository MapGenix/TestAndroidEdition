using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Shape made of one or more polygons.</summary>
    [Serializable]
    public class MultipolygonShape : BaseAreaShape
    {
        private Collection<PolygonShape> polygons;

        public MultipolygonShape()
            : this(new PolygonShape[] { })
        {
        }

        public MultipolygonShape(IEnumerable<PolygonShape> polygons)
        {
            Validators.CheckParameterIsNotNull(polygons, "polygons");

            this.polygons = new Collection<PolygonShape>();
            foreach (PolygonShape polygon in polygons)
            {
                this.polygons.Add(polygon);
            }
        }

        public MultipolygonShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            MultipolygonFromWkt(wellKnownText);
        }

        public MultipolygonShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            MultipolygonFromWkb(wellKnownBinary);
        }

        public Collection<PolygonShape> Polygons
        {
            get
            {
                return polygons;
            }
        }

        public override bool CanRotate
        {
            get
            {
                return true;
            }
        }

        protected override BaseShape CloneDeepCore()
        {
            MultipolygonShape multipolygon = new MultipolygonShape();

            foreach (PolygonShape polygon in polygons)
            {
                multipolygon.polygons.Add((PolygonShape)polygon.CloneDeep());
            }

            return multipolygon;
        }

        
        protected override double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
           
            double perimeter = 0;

            foreach (PolygonShape polygonShape in polygons)
            {
                perimeter = perimeter + polygonShape.GetPerimeter(shapeUnit, returningUnit);
            }

            return perimeter;
        }

        
        protected override double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
           
            double totalArea = 0;

            foreach (PolygonShape polygon in polygons)
            {
                totalArea = totalArea + polygon.GetArea(shapeUnit, returningUnit);
            }

            return totalArea;
        }

        protected override void ScaleUpCore(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            double factor = 1.0 + percentage / 100;
            Scale(factor);
        }

        protected override void ScaleDownCore(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue);

            double factor = 1.0 - percentage / 100;
            Scale(factor);
        }

        protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");

            MultipolygonShape resultMultipolygon = new MultipolygonShape();

            foreach (PolygonShape polygon in polygons)
            {
                PolygonShape tmpPolygon = (PolygonShape)polygon.Register(fromPoint, toPoint, fromUnit, toUnit);
                resultMultipolygon.polygons.Add(tmpPolygon);
            }

            return resultMultipolygon;
        }

        
        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
           
            foreach (PolygonShape polygon in polygons)
            {
                polygon.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
            }
        }

       
        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
     
            foreach (PolygonShape polygon in polygons)
            {
                polygon.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            }
        }

        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            foreach (PolygonShape polygon in polygons)
            {
                polygon.Rotate(pivotPoint, degreeAngle);
            }
        }

        
        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

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

            foreach (PolygonShape polygon in polygons)
            {
                double currentDistance = polygon.GetDistanceTo(targetShape, shapeUnit, DistanceUnit.Meter);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = polygon.GetClosestPointTo(targetShape, shapeUnit);
                }
            }
            return closestPointShape;
        }

        
        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double minDistance = double.MaxValue;

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

            foreach (PolygonShape polygon in polygons)
            {
                double currentDistance = polygon.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
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

            StringBuilder wktStringText = new StringBuilder();
            wktStringText.Append("MULTIPOLYGON()");

            for (int i = 0; i < polygons.Count; i++)
            {
                string polygonString = polygons[i].GetWellKnownText();
                polygonString = polygonString.Substring(7);

                if (i == 0)
                {
                    wktStringText.Insert(wktStringText.Length - 1, polygonString);
                }
                else
                {
                    wktStringText.Insert(wktStringText.Length - 1, "," + polygonString);
                }
            }
            return wktStringText.ToString();
        }

        
        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Multipolygon;
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

                byte wkbByteOrder = (byteOrder == WkbByteOrder.LittleEndian) ? (byte)1 : (byte)0;

                wkbWriter.Write(wkbByteOrder);

                ShapeConverter.WriteWkb(WkbShapeType.Multipolygon, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(polygons.Count, byteOrder, wkbWriter);
                foreach (PolygonShape polygon in polygons)
                {
                    wkbWriter.Write(wkbByteOrder);

                    ShapeConverter.WriteWkb(WkbShapeType.Polygon, byteOrder, wkbWriter);
                    ShapeConverter.WriteWkb(1 + polygon.InnerRings.Count, byteOrder, wkbWriter);
                    GetARingWellKnownBinary(polygon.OuterRing, byteOrder, wkbWriter);
                    foreach (RingShape ringShape in polygon.InnerRings)
                    {
                        GetARingWellKnownBinary(ringShape, byteOrder, wkbWriter);
                    }
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

            MultipolygonFromWkt(wellKnownText);
        }

       
        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            MultipolygonFromWkb(wellKnownBinary);
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Collection<Vertex> vertexs = new Collection<Vertex>();
            foreach (PolygonShape polygon in polygons)
            {
                foreach (Vertex vertex in polygon.OuterRing.Vertices)
                {
                    vertexs.Add(vertex);
                }
            }

            return GetBoundingBoxFromVertices(vertexs);
        }

        private static void CloneOnePolygonsToAnother(MultipolygonShape fromMultipolygon, MultipolygonShape toMultipolygon)
        {
            toMultipolygon.polygons = new Collection<PolygonShape>();

            foreach (PolygonShape polygon in fromMultipolygon.polygons)
            {
                toMultipolygon.polygons.Add((PolygonShape)polygon.CloneDeep());
            }
        }

       
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (polygons.Count == 0)
                    {
                        validateResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    foreach (PolygonShape polygon in polygons)
                    {
                        validateResult = polygon.Validate(validationMode);
                        if (!validateResult.IsValid)
                        {
                            break;
                        }
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

            for (int i = 0; i < polygons.Count; i++)
            {
                MultipointShape multipointShape = polygons[i].GetCrossing(targetShape);

                for (int j = 0; j < multipointShape.Points.Count; j++)
                {
                    resultMultiPointShape.Points.Add(multipointShape.Points[j]);
                }
            }

            return resultMultiPointShape;
        }

        protected override bool ContainsCore(BaseShape targetShape)
        {
            bool result = false;

            foreach (PolygonShape polygon in polygons)
            {
                if (polygon.Contains(targetShape))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

     
        public static bool RemoveVertex(Vertex selectedVertex, MultipolygonShape multipolygonShape)
        {
            Validators.CheckParameterIsNotNull(multipolygonShape, "multipolygonShape");

            return multipolygonShape.RemoveVertex(selectedVertex);
        }

       
        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = false;
            foreach (PolygonShape polygonShape in this.Polygons)
            {
                deleteSucceed = polygonShape.RemoveVertex(selectedVertex);
                if (deleteSucceed)
                {
                    break;
                }
            }

            return deleteSucceed;
        }

        private static void GetARingWellKnownBinary(RingShape targetRingShape, WkbByteOrder byteOrder, BinaryWriter writer)
        {
            ShapeConverter.WriteWkb(targetRingShape.Vertices.Count, byteOrder, writer);
            for (int i = 0; i < targetRingShape.Vertices.Count; i++)
            {
                ShapeConverter.WriteWkb(targetRingShape.Vertices[i].X, byteOrder, writer);
                ShapeConverter.WriteWkb(targetRingShape.Vertices[i].Y, byteOrder, writer);
            }
        }

        private void MultipolygonFromWkt(string wellKnownText)
        {
            MultipolygonShape multipolygon = (MultipolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOnePolygonsToAnother(multipolygon, this);
        }

        private void MultipolygonFromWkb(byte[] wellKnownBinary)
        {
            MultipolygonShape multipolygon = (MultipolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneOnePolygonsToAnother(multipolygon, this);
        }

        internal void Scale(double multiplicator)
        {
            double totalX = 0;
            double totalY = 0;
            double totalPoints = 0;

            foreach (PolygonShape polygon in polygons)
            {
                for (int i = 0; i < polygon.OuterRing.Vertices.Count - 1; i++)
                {
                    totalX = totalX + polygon.OuterRing.Vertices[i].X;
                    totalY = totalY + polygon.OuterRing.Vertices[i].Y;
                    totalPoints++;
                }

                foreach (RingShape ringShape in polygon.InnerRings)
                {
                    for (int j = 0; j < ringShape.Vertices.Count - 1; j++)
                    {
                        totalX = totalX + ringShape.Vertices[j].X;
                        totalY = totalY + ringShape.Vertices[j].Y;
                        totalPoints++;
                    }
                }
            }

            double centerX = totalX / totalPoints;
            double centerY = totalY / totalPoints;

            foreach (PolygonShape polygon in polygons)
            {
                RingShape.ScaleOneRing(polygon.OuterRing, multiplicator, centerX, centerY);

                foreach (RingShape ringShape in polygon.InnerRings)
                {
                    RingShape.ScaleOneRing(ringShape, multiplicator, centerX, centerY);
                }
            }
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
    }
}
