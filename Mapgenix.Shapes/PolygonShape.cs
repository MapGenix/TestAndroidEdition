using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Shape defined as a polygon with a single outer ring with zero-to-many inner rings.</summary>
    [Serializable]
    public class PolygonShape : BaseAreaShape
    {
        private RingShape outerRing;
        private Collection<RingShape> innerRings;

     
        public PolygonShape()
            : this(new RingShape(), new RingShape[] { })
        { }

       
        public PolygonShape(RingShape outerRing)
            : this(outerRing, new RingShape[] { })
        { }

       
        public PolygonShape(RingShape outerRing, IEnumerable<RingShape> innerRings)
        {
            Validators.CheckParameterIsNotNull(outerRing, "outerRing");
            Validators.CheckParameterIsNotNull(innerRings, "innerRings");

            this.innerRings = new Collection<RingShape>();
            this.outerRing = outerRing;

            foreach (RingShape ringShape in innerRings)
            {
                this.innerRings.Add(ringShape);
            }
        }

       
        public PolygonShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        public PolygonShape(byte[] wellKnownBinary)
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

        public RingShape OuterRing
        {
            get
            {
                return outerRing;
            }
            set
            {
                outerRing = value;
            }
        }

       
        public Collection<RingShape> InnerRings
        {
            get
            {
                return innerRings;
            }
        }

        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Polygon;
        }

       
        protected override BaseShape CloneDeepCore()
        {
            PolygonShape polygon = new PolygonShape();
            polygon.outerRing = (RingShape)this.outerRing.CloneDeep();

            foreach (RingShape ringSahpe in innerRings)
            {
                polygon.innerRings.Add((RingShape)ringSahpe.CloneDeep());
            }

            return polygon;
        }

        
        protected override double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
        
            double perimeter = 0;
            perimeter = outerRing.GetPerimeter(shapeUnit, returningUnit);
          
            foreach (RingShape ringShape in innerRings)
            {
                perimeter = perimeter + ringShape.GetPerimeter(shapeUnit, returningUnit);
            }

            return perimeter;
        }

        
        protected override double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
   
            double areaFill = 0;
            double areaHoll = 0;

            PolygonShape tmpPolygon = (PolygonShape)this.CloneDeepCore();

            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                TranslateShapeToOneHemiSphere(tmpPolygon);
            }

            areaFill = tmpPolygon.outerRing.GetArea(shapeUnit, returningUnit);
           
            foreach (RingShape ringShape in tmpPolygon.innerRings)
            {
                areaHoll = areaHoll + ringShape.GetArea(shapeUnit, returningUnit);
            }

            return areaFill - areaHoll;
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

            PolygonShape resultPolygon = new PolygonShape();

            resultPolygon.OuterRing = (RingShape)outerRing.Register(fromPoint, toPoint, fromUnit, toUnit);

            foreach (RingShape ringShape in innerRings)
            {
                RingShape tmpRingShape = (RingShape)ringShape.Register(fromPoint, toPoint, fromUnit, toUnit);
                resultPolygon.innerRings.Add(tmpRingShape);
            }

            return resultPolygon;
        }

        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
   
            outerRing.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);

            foreach (RingShape ringShape in innerRings)
            {
                ringShape.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
            }
        }

       
        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
   
            outerRing.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);

            foreach (RingShape ringShape in innerRings)
            {
                ringShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            }
        }

        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            outerRing.Rotate(pivotPoint, degreeAngle);

            foreach (RingShape ringShape in innerRings)
            {
                ringShape.Rotate(pivotPoint, degreeAngle);
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

            MultilineShape multilineShape = PolygonShape.ToLineShapes(this);
            foreach (LineShape lineShape in multilineShape.Lines)
            {
                double currentDistance = lineShape.GetDistanceTo(targetShape, shapeUnit, DistanceUnit.Meter);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = lineShape.GetClosestPointTo(targetShape, shapeUnit);
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

            MultilineShape multilineShape = PolygonShape.ToLineShapes(this);
            foreach (LineShape lineShape in multilineShape.Lines)
            {
                double currectDistance = lineShape.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
                if (currectDistance < minDistance)
                {
                    minDistance = currectDistance;
                }
            }

            return minDistance;
        }

       protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            StringBuilder wellKnownText = new StringBuilder();

            wellKnownText.Append("POLYGON(");

            GetARingWellKnownText(outerRing, wellKnownText);

            foreach (RingShape ring in innerRings)
            {
                wellKnownText.Append(",");
                GetARingWellKnownText(ring, wellKnownText);
            }

            wellKnownText.Append(")");

            return wellKnownText.ToString();
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

                ShapeConverter.WriteWkb(WkbShapeType.Polygon, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(1 + innerRings.Count, byteOrder, wkbWriter);
                GetARingWellKnownBinary(outerRing, byteOrder, wkbWriter);

                foreach (RingShape ringShape in innerRings)
                {
                    GetARingWellKnownBinary(ringShape, byteOrder, wkbWriter);
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

            PolygonShape polygon = (PolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOnePolygonToAnother(polygon, this);
        }

       
        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            PolygonShape polygon = (PolygonShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneOnePolygonToAnother(polygon, this);
        }

      
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validationResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    validationResult = outerRing.Validate(validationMode);
                    if (!validationResult.IsValid)
                    {
                        break;
                    }

                    foreach (RingShape innerRing in innerRings)
                    {
                        validationResult = innerRing.Validate(validationMode);
                        if (!validationResult.IsValid)
                        {
                            break;
                        }
                    }
                    break;
                case ShapeValidationMode.Advanced:
                    break;

                default:
                    break;
            }

            return validationResult;
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return outerRing.GetBoundingBox();
        }

       
        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            LineShape outerRingLineShape = new LineShape(outerRing.Vertices);
            MultipointShape outerRingMultipoint = outerRingLineShape.GetCrossing(targetShape);
            for (int i = 0; i < outerRingMultipoint.Points.Count; i++)
            {
                resultMultiPointShape.Points.Add(outerRingMultipoint.Points[i]);
            }

            for (int i = 0; i < innerRings.Count; i++)
            {
                LineShape innerRingLineShape = new LineShape(innerRings[i].Vertices);
                MultipointShape innerRingMultipoint = innerRingLineShape.GetCrossing(targetShape);
                for (int j = 0; j < innerRingMultipoint.Points.Count; j++)
                {
                    resultMultiPointShape.Points.Add(innerRingMultipoint.Points[j]);
                }
            }

            return resultMultiPointShape;
        }

      
        public static bool RemoveVertex(Vertex selectedVertex, PolygonShape polygonShape)
        {
            Validators.CheckParameterIsNotNull(polygonShape, "polygonShape");

            return polygonShape.RemoveVertex(selectedVertex);
        }

      
        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = this.OuterRing.RemoveVertex(selectedVertex);
            if (!deleteSucceed)
            {
                foreach (RingShape ringShape in this.InnerRings)
                {
                    deleteSucceed = ringShape.RemoveVertex(selectedVertex);
                    if (deleteSucceed)
                    {
                        break;
                    }
                }
            }
            return deleteSucceed;
        }

        internal void Scale(double multiplicator)
        {
            double totalX = 0;
            double totalY = 0;
            double totalPoints = 0;

            for (int i = 0; i < outerRing.Vertices.Count - 1; i++)
            {
                totalX = totalX + outerRing.Vertices[i].X;
                totalY = totalY + outerRing.Vertices[i].Y;
                totalPoints++;
            }

            foreach (RingShape ringShape in innerRings)
            {
                for (int i = 0; i < ringShape.Vertices.Count - 1; i++)
                {
                    totalX = totalX + ringShape.Vertices[i].X;
                    totalY = totalY + ringShape.Vertices[i].Y;
                    totalPoints++;
                }
            }

            double centerX = totalX / totalPoints;
            double centerY = totalY / totalPoints;

            RingShape.ScaleOneRing(outerRing, multiplicator, centerX, centerY);
           
            foreach (RingShape ringShape in innerRings)
            {
                RingShape.ScaleOneRing(ringShape, multiplicator, centerX, centerY);
            }
        }

        private static void GetARingWellKnownBinary(RingShape targetRingShape, WkbByteOrder byteOrder, BinaryWriter writer)
        {
            Collection<Vertex> vertices = targetRingShape.Vertices;
            ShapeConverter.WriteWkb(vertices.Count, byteOrder, writer);
            for (int i = 0; i < vertices.Count; i++)
            {
                ShapeConverter.WriteWkb(vertices[i].X, byteOrder, writer);
                ShapeConverter.WriteWkb(vertices[i].Y, byteOrder, writer);
            }
        }

        private static void GetARingWellKnownText(RingShape ringShape, StringBuilder wellKnownText)
        {
            wellKnownText.Append("(");

            int counter = 0;
            foreach (Vertex vertex in ringShape.Vertices)
            {
                wellKnownText.Append(String.Format(CultureInfo.InvariantCulture, "{0} {1}", vertex.X, vertex.Y));

                if (++counter < ringShape.Vertices.Count)
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");
        }

        private static void TranslateShapeToOneHemiSphere(PolygonShape targetPolygon)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (Vertex vertex in targetPolygon.outerRing.Vertices)
            {
                if (vertex.X < minX)
                {
                    minX = vertex.X;
                }
                if (vertex.Y < minY)
                {
                    minY = vertex.Y;
                }
                if (vertex.Y > maxY)
                {
                    maxY = vertex.Y;
                }
            }
          
            if (minX < 0)
            {
                targetPolygon.TranslateByOffset(Math.Abs(minX), 0, GeographyUnit.Meter, DistanceUnit.Meter);
            }
            if (minY < 0 && maxY > 0)
            {
                if (Math.Abs(minY) > Math.Abs(maxY))
                {
                    targetPolygon.TranslateByOffset(0, -maxY, GeographyUnit.Meter, DistanceUnit.Meter);
                }
                else
                {
                    targetPolygon.TranslateByOffset(0, Math.Abs(minY), GeographyUnit.Meter, DistanceUnit.Meter);
                }
            }
        }

        private static void CloneOnePolygonToAnother(PolygonShape fromPolygon, PolygonShape toPolygon)
        {
            toPolygon.innerRings = new Collection<RingShape>();

            toPolygon.outerRing = fromPolygon.outerRing;

            for (int i = 0; i < fromPolygon.innerRings.Count; i++)
            {
                toPolygon.innerRings.Add(fromPolygon.innerRings[i]);
            }
        }

        internal static MultilineShape ToLineShapes(PolygonShape polygonShape)
        {
            MultilineShape multilineShape = new MultilineShape();

            LineShape lineOuter = new LineShape();
            for (int i = 0; i < polygonShape.outerRing.Vertices.Count; i++)
            {
                Vertex tempVertex = new Vertex(polygonShape.outerRing.Vertices[i].X, polygonShape.outerRing.Vertices[i].Y);
                lineOuter.Vertices.Add(tempVertex);
            }
            multilineShape.Lines.Add(lineOuter);

            foreach (RingShape ringShape in polygonShape.innerRings)
            {
                LineShape lineInner = new LineShape();
                for (int i = 0; i < ringShape.Vertices.Count; i++)
                {
                    Vertex tempVertex = new Vertex(ringShape.Vertices[i].X, ringShape.Vertices[i].Y);
                    lineInner.Vertices.Add(tempVertex);
                }
                multilineShape.Lines.Add(lineInner);
            }

            return multilineShape;
        }

        internal bool IsSelSelfIntersecting()
        {
            bool isSelSelfIntersecting = outerRing.IsSelSelfIntersecting();

            if (!isSelSelfIntersecting)
            {
                foreach (RingShape ringShape in innerRings)
                {
                    isSelSelfIntersecting = ringShape.IsSelSelfIntersecting();
                    if (isSelSelfIntersecting)
                    {
                        break;
                    }
                }
            }

            return isSelSelfIntersecting;
        }
    }
}
