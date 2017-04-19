using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>Shape defined as one or more lines, each with two or more points.</summary>
    [Serializable]
    public class MultilineShape : BaseLineShape
    {
        private Collection<LineShape> lines;

       
        public MultilineShape()
            : this(new LineShape[] { })
        { }

       public MultilineShape(IEnumerable<LineShape> lineShapes)
        {
            Validators.CheckParameterIsNotNull(lineShapes, "lineShapes");

            lines = new Collection<LineShape>();
            foreach (LineShape lineShape in lineShapes)
            {
                lines.Add(lineShape);
            }
        }

        public MultilineShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        public MultilineShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

        public Collection<LineShape> Lines
        {
            get { return lines; }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Collection<Vertex> vertexs = new Collection<Vertex>();
            foreach (LineShape lineShape in lines)
            {
                foreach (Vertex vertex in lineShape.Vertices)
                {
                    vertexs.Add(vertex);
                }
            }

            return GetBoundingBoxFromVertices(vertexs);
        }

        
        protected override BaseShape CloneDeepCore()
        {
            MultilineShape multiline = new MultilineShape();

            foreach (LineShape line in lines)
            {
                multiline.lines.Add((LineShape)line.CloneDeep());
            }

            return multiline;
        }

       
        public void Reorder(PointShape startPoint, double tolerance)
        {
            Validators.CheckParameterIsNotNull(startPoint, "startPoint");
            Validators.CheckParameterIsValid(startPoint, "startPoint");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            ReorderCore(startPoint, tolerance, GeographyUnit.Meter, DistanceUnit.Meter);
        }

       
        public void Reorder(PointShape startPoint, double tolerance, GeographyUnit shapeUnit, DistanceUnit unitOfTolerance)
        {
            Validators.CheckParameterIsNotNull(startPoint, "startPoint");
            Validators.CheckParameterIsValid(startPoint, "startPoint");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            ReorderCore(startPoint, tolerance, shapeUnit, unitOfTolerance);
        }

       
        protected virtual void ReorderCore(PointShape startPoint, double tolerance, GeographyUnit shapeUnit, DistanceUnit unitOfTolerance)
        {
            Validators.CheckParameterIsNotNull(startPoint, "startPoint");
            Validators.CheckParameterIsValid(startPoint, "startPoint");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            bool isMultilineValidForReOrder = CheckMutliLineIsValidForReOrder(tolerance, shapeUnit, unitOfTolerance);
            if (!isMultilineValidForReOrder)
            {
                throw new System.InvalidOperationException(ExceptionDescription.MultiLineInvalidForReorder);
            }

            bool isInputPointIsValidForReOrder = CheckInputPointIsValidForReOrder(startPoint, tolerance, shapeUnit, unitOfTolerance);
            if (!isInputPointIsValidForReOrder)
            {
                throw new ArgumentException(ExceptionDescription.MultiLineCannotReorderForInputParams);
            }

            MultilineShape resultMultline = new MultilineShape();
            Vertex tempPointShape = new Vertex(startPoint.X, startPoint.Y);

            while (lines.Count != 0)
            {
                LineShape newLineShape = GetLineShapeWithInTolerance(tempPointShape, tolerance, shapeUnit, unitOfTolerance);
                resultMultline.lines.Add(newLineShape);
                tempPointShape = newLineShape.Vertices[newLineShape.Vertices.Count - 1];
            }
            lines = resultMultline.lines;
        }

        
        protected override double GetLengthCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            double totalDistance = 0.0;
            foreach (LineShape lineShape in lines)
            {
                totalDistance = totalDistance + lineShape.GetLength(shapeUnit, returningUnit);
            }

            return totalDistance;
        }

       
        public PointShape GetPointOnALine(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return GetPointOnALineCore(startingPoint, percentageOfLine);
        }

        
        protected virtual PointShape GetPointOnALineCore(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double unitLength = GetLength(GeographyUnit.Meter, DistanceUnit.Meter);
            double percentatageDistance = (percentageOfLine / 100) * unitLength;
            PointShape resultPointShape = GetPointOnALineCore(startingPoint, percentatageDistance, GeographyUnit.Meter, DistanceUnit.Meter);

            return resultPointShape;
        }

       
        public PointShape GetPointOnALine(StartingPoint startingPoint, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

            return GetPointOnALineCore(startingPoint, distance, shapeUnit, distanceUnit);
        }

       
        protected virtual PointShape GetPointOnALineCore(StartingPoint startingPoint, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

            PointShape resultPointShape = new PointShape();
            if (distance == 0)
            {
                if (startingPoint == StartingPoint.FirstPoint)
                {
                    resultPointShape = new PointShape(lines[0].Vertices[0].X, lines[0].Vertices[0].Y);
                }
                else
                {
                    int lineCount = lines.Count;
                    int pointCount = lines[lineCount - 1].Vertices.Count;
                    resultPointShape = new PointShape(lines[lineCount - 1].Vertices[pointCount - 1].X, lines[lineCount - 1].Vertices[pointCount - 1].Y);
                }
            }
            else
            {
                double unitLength = GetLength(shapeUnit, distanceUnit);
                if (distance == unitLength)
                {
                    if (startingPoint == StartingPoint.FirstPoint)
                    {
                        int lineCount = lines.Count;
                        int pointCount = lines[lineCount - 1].Vertices.Count;
                        resultPointShape = new PointShape(lines[lineCount - 1].Vertices[pointCount - 1].X, lines[lineCount - 1].Vertices[pointCount - 1].Y);
                    }
                    else
                    {
                        resultPointShape = new PointShape(lines[0].Vertices[0].X, lines[0].Vertices[0].Y);
                    }
                }
                else if (distance <= unitLength)
                {
                    StartingPoint tmpFromVertex;
                    MultilineShape tmpMultiLine;
                    if (startingPoint == StartingPoint.FirstPoint)
                    {
                        tmpMultiLine = this;
                        tmpFromVertex = StartingPoint.FirstPoint;
                    }
                    else
                    {
                        tmpMultiLine = new MultilineShape();
                        int i = lines.Count - 1;
                        do
                        {
                            tmpMultiLine.lines.Add(lines[i]);
                            i = i - 1;
                        }
                        while (i >= 0);
                        tmpFromVertex = StartingPoint.LastPoint;
                    }

                    double totalUnitLengthLine = 0.0;
                    foreach (LineShape lineshape in tmpMultiLine.lines)
                    {
                        double unitLengthLine = lineshape.GetLength(shapeUnit, distanceUnit);
                        totalUnitLengthLine = totalUnitLengthLine + unitLengthLine;
                        if (distance <= totalUnitLengthLine)
                        {
                            double startingLength = totalUnitLengthLine - unitLengthLine;
                            double realDistance = distance - startingLength;
                            resultPointShape = lineshape.GetPointOnALine(tmpFromVertex, realDistance, shapeUnit, distanceUnit);
                            break;
                        }
                    }
                }
            }
            return resultPointShape;
        }

        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, float startingPercentageOfTheLine, float percentageOfTheLine)
        {
            Validators.CheckIfInputValueIsInRange(startingPercentageOfTheLine, "startingPercentageOfTheLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(percentageOfTheLine, "percentageOfTheLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            BaseLineShape result = null;

            StartingPoint point = StartingPoint.FirstPoint;
            if (startingPoint == StartingPoint.FirstPoint)
            {
                point = StartingPoint.LastPoint;
            }

            BaseLineShape lineBaseShape = GetLineOnALine(point, (1 - startingPercentageOfTheLine / 100) * 100);
            if (lineBaseShape != null)
            {
                result = ((MultilineShape)lineBaseShape).GetLineOnALine(StartingPoint.LastPoint, percentageOfTheLine);
            }

            return result;
        }

       
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, percentageOfLine);
        }

       
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double unitLength = GetLength(GeographyUnit.Meter, DistanceUnit.Meter);
            double percentatageDistance = (percentageOfLine / 100) * unitLength;
            BaseLineShape resultLineShape = GetLineOnALineCore(startingPoint, 0, percentatageDistance, GeographyUnit.Meter, DistanceUnit.Meter);

            return resultLineShape;
        }

       
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, double startingDistance, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(startingDistance, "startingDistance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, startingDistance, distance, shapeUnit, distanceUnit);
        }

       
     
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, double startingDistance, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(startingDistance, "startingDistance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            MultilineShape resultMultiline = new MultilineShape();
            double cumulDistanceEnd = 0;
            double endingDistance = startingDistance + distance;
            if (startingPoint == StartingPoint.FirstPoint)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    AddLineShapeForLineOnLine(resultMultiline, ref cumulDistanceEnd, lines[i], startingPoint, startingDistance, endingDistance, shapeUnit, distanceUnit);
                }
            }
            else
            {
                int i = lines.Count - 1;
                while (i >= 0)
                {
                    AddLineShapeForLineOnLine(resultMultiline, ref cumulDistanceEnd, lines[i], startingPoint, startingDistance, endingDistance, shapeUnit, distanceUnit);
                    i = i - 1;
                }
            }
            return resultMultiline;
        }

        
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, endPointShape);
        }

       
        public BaseLineShape GetLineOnALine(PointShape startPointShape, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(startPointShape, "startPointShape");
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startPointShape, endPointShape);
        }

       
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultilineShape resultLineShape = new MultilineShape();

            if (startingPoint == StartingPoint.FirstPoint)
            {
                resultLineShape = (MultilineShape)GetLineOnALine(new PointShape(Lines[0].Vertices[0]), endPointShape);
            }
            else
            {
                resultLineShape = (MultilineShape)GetLineOnALine(endPointShape, new PointShape(Lines[Lines.Count - 1].Vertices[Lines[Lines.Count - 1].Vertices.Count - 1]));
            }

            return resultLineShape;
        }

       
        protected virtual BaseLineShape GetLineOnALineCore(PointShape startPointShape, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(startPointShape, "startPointShape");
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            PointShape realStartPoint = GetClosestPointTo(startPointShape, GeographyUnit.Meter);
            PointShape realEndPoint = GetClosestPointTo(endPointShape, GeographyUnit.Meter);

            MultilineShape resultLineShape = new MultilineShape();

            if (realStartPoint.X != realEndPoint.X || realStartPoint.Y != realEndPoint.Y)
            {
                double startDistance = GetDistanceToFirstVertex(realStartPoint);
                double endDistance = GetDistanceToFirstVertex(realEndPoint);

                if (startDistance <= endDistance)
                {
                    resultLineShape = (MultilineShape)GetLineOnALine(StartingPoint.FirstPoint, startDistance, endDistance - startDistance, GeographyUnit.Meter, DistanceUnit.Meter);
                }
                else
                {
                    if (endDistance == 0)
                    {
                        resultLineShape = GetResultLineShapeForSameStartAndEndPoints(startPointShape, endPointShape);
                    }
                    else
                    {
                        resultLineShape = (MultilineShape)GetLineOnALine(StartingPoint.FirstPoint, endDistance, startDistance - endDistance, GeographyUnit.Meter, DistanceUnit.Meter);
                    }
                }
            }

            return resultLineShape;
        }

        private MultilineShape GetResultLineShapeForSameStartAndEndPoints(PointShape startPointShape, PointShape endPointShape)
        {
            Collection<Vertex> points = new Collection<Vertex>();
            Collection<LineShape> lines = new Collection<LineShape>();
            bool isStart = false;
            bool isEnd = false;
            foreach (LineShape line in this.Lines)
            {
                if (isEnd)
                {
                    break;
                }

                foreach (Vertex vertex in line.Vertices)
                {
                    if (isStart)
                    {
                        if ((vertex.X == endPointShape.X) && (vertex.Y == endPointShape.Y))
                        {
                            points.Add(vertex);
                            isEnd = true;
                            break;
                        }
                        else
                        {
                            points.Add(vertex);
                            continue;
                        }
                    }

                    if ((vertex.X == startPointShape.X && vertex.Y == startPointShape.Y) || (Math.Abs(1 - vertex.X / endPointShape.X) <= 1e-8 && Math.Abs(1 - vertex.Y / endPointShape.Y) <= 1e-8))
                    {
                        points.Add(vertex);
                        isStart = true;
                    }
                }
            }

            LineShape lineShape = new LineShape(points);
            lines.Add(lineShape);
            MultilineShape multiLine = new MultilineShape(lines);
            return multiLine;
        }

        private double GetDistanceToFirstVertex(PointShape pointShape)
        {
            double distance = 0.0;
            bool shouldStop = false;

            for (int j = 0; j < Lines.Count; j++)
            {
                if (!shouldStop)
                {
                    for (int i = 1; i <= Lines[j].Vertices.Count - 1; i++)
                    {
                        if (IsPointBetweenVerteces(Lines[j].Vertices[i], Lines[j].Vertices[i - 1], pointShape))
                        {
                            double distanceToBeforeVertex = Lines[j].Vertices[i - 1].GetDistanceTo(pointShape, GeographyUnit.Meter, DistanceUnit.Meter);
                            distance += distanceToBeforeVertex;
                            shouldStop = true;
                            break;
                        }
                        else
                        {
                            double vertexDistance = Lines[j].Vertices[i].GetDistanceTo(Lines[j].Vertices[i - 1], GeographyUnit.Meter, DistanceUnit.Meter);
                            distance += vertexDistance;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return distance;
        }

        private static void AddLineShapeForLineOnLine(MultilineShape ResultMultiLine, ref double CumulDistEnd, LineShape LineShape, StartingPoint startingPoint, double StartingDistance, double EndingDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            double LineStart;
            double LineEnd;
            double LineDist = LineShape.GetLength(shapeUnit, distanceUnit);
            CumulDistEnd = CumulDistEnd + LineDist;
            double CumulDistStart = CumulDistEnd - LineDist;
            LineShape NewLineShape;
            if ((StartingDistance >= CumulDistStart && StartingDistance <= CumulDistEnd) && (EndingDistance >= CumulDistEnd))
            {
                LineStart = StartingDistance - CumulDistStart;
                LineEnd = LineDist - LineStart;
                NewLineShape = (LineShape)LineShape.GetLineOnALine(startingPoint, LineStart, LineEnd, shapeUnit, distanceUnit);
                ResultMultiLine.lines.Add(NewLineShape);
            }

            else if ((StartingDistance <= CumulDistStart && EndingDistance >= CumulDistEnd))
            {
                LineStart = 0;
                LineEnd = LineDist;
                NewLineShape = (LineShape)LineShape.GetLineOnALine(startingPoint, LineStart, LineEnd, shapeUnit, distanceUnit);
                ResultMultiLine.lines.Add(NewLineShape);
            }

            else if ((EndingDistance > CumulDistStart && EndingDistance < CumulDistEnd) && (StartingDistance < CumulDistStart))
            {
                LineStart = 0;
                LineEnd = EndingDistance - CumulDistStart;
                NewLineShape = (LineShape)LineShape.GetLineOnALine(startingPoint, LineStart, LineEnd - LineStart, shapeUnit, distanceUnit);
                ResultMultiLine.lines.Add(NewLineShape);
                return;
            }
            else if ((StartingDistance >= CumulDistStart && StartingDistance < CumulDistEnd) && (EndingDistance > CumulDistStart && EndingDistance <= CumulDistEnd))
            {
                LineStart = StartingDistance - CumulDistStart;
                LineEnd = EndingDistance - CumulDistStart;
                NewLineShape = (LineShape)LineShape.GetLineOnALine(startingPoint, LineStart, LineEnd - LineStart, shapeUnit, distanceUnit);
                ResultMultiLine.lines.Add(NewLineShape);
                return;
            }
        }


        protected override void ScaleUpCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 + percentage / 100.0;
            Scale(factor);
        }

        protected override void ScaleDownCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 - percentage / 100.0;
            Scale(factor);
        }

        protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");

            MultilineShape returnMultiLineShape = new MultilineShape();

            foreach (LineShape lineShape in lines)
            {
                LineShape tmpLineShape = (LineShape)(lineShape.Register(fromPoint, toPoint, fromUnit, toUnit));
                returnMultiLineShape.lines.Add(tmpLineShape);
            }
            return returnMultiLineShape;
        }

       
        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            foreach (LineShape lineShape in lines)
            {
                lineShape.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
            }
        }

        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

            foreach (LineShape lineShape in lines)
            {
                lineShape.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            }
        }

      
        public override bool CanRotate
        {
            get
            {
                return true;
            }
        }

       
        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            foreach (LineShape lineShape in lines)
            {
                lineShape.Rotate(pivotPoint, degreeAngle);
            }
        }

        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            PointShape closestPoint = null;
            double minDistance = double.MaxValue;

            if (targetShape.GetType().Name == "EllipseShape")
            {
                if (targetShape.Intersects(this))
                {
                    return closestPoint;
                }
            }
            else if (targetShape.GetType().Name == "PointShape")
            {
                if (targetShape.Intersects(this))
                {
                    return (PointShape)targetShape;
                }
            }
            else
            {
                if (this.Intersects(targetShape))
                {
                    return closestPoint;
                }
            }

            foreach (LineShape lineShape in lines)
            {
                double currentDistance = lineShape.GetDistanceTo(targetShape, shapeUnit, DistanceUnit.Meter);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPoint = lineShape.GetClosestPointTo(targetShape, shapeUnit);
                }
            }
            return closestPoint;
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

            foreach (LineShape lineShape in lines)
            {
                double currentDistance = lineShape.GetDistanceTo(targetShape, shapeUnit, distanceUnit);
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
            wellKnownText.Append("MULTILINESTRING(");

            for (int i = 0; i < lines.Count; i++)
            {
                GetWktForALine(lines[i], wellKnownText);
                if (i < lines.Count - 1)
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");

            return wellKnownText.ToString();
        }

      
        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Multiline;
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

                byte EndianByte = (byte)0;
                if (byteOrder == WkbByteOrder.LittleEndian)
                {
                    EndianByte = (byte)1;
                }
                else
                {
                    EndianByte = (byte)0;
                }

                wkbWriter.Write(EndianByte);

                ShapeConverter.WriteWkb(WkbShapeType.Multiline, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(lines.Count, byteOrder, wkbWriter);
                foreach (LineShape line in lines)
                {
                    wkbWriter.Write(EndianByte);

                    ShapeConverter.WriteWkb(WkbShapeType.LineString, byteOrder, wkbWriter);
                    ShapeConverter.WriteWkb(line.Vertices.Count, byteOrder, wkbWriter);

                    for (int i = 0; i < line.Vertices.Count; i++)
                    {
                        ShapeConverter.WriteWkb(line.Vertices[i].X, byteOrder, wkbWriter);
                        ShapeConverter.WriteWkb(line.Vertices[i].Y, byteOrder, wkbWriter);
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

            MultilineShape multiline = (MultilineShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOneLinesToAnother(multiline, this);
        }

       
        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            MultilineShape multiline = (MultilineShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneOneLinesToAnother(multiline, this);
        }

        private static void CloneOneLinesToAnother(MultilineShape fromLines, MultilineShape toLines)
        {
            toLines.lines = new Collection<LineShape>();

            for (int i = 0; i < fromLines.lines.Count; i++)
            {
                toLines.lines.Add(fromLines.lines[i]);
            }
        }

        
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (lines.Count == 0)
                    {
                        validateResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    else
                    {
                        foreach (LineShape lineShape in lines)
                        {
                            validateResult = lineShape.Validate(validationMode);
                            if (!validateResult.IsValid)
                            {
                                break;
                            }
                        }
                    }

                    break;
                case ShapeValidationMode.Advanced:
                    break;
                default:
                    break;
            }

            return validateResult;
        }

       
        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            for (int i = 0; i < lines.Count; i++)
            {
                MultipointShape multipointShape = lines[i].GetCrossing(targetShape);

                for (int j = 0; j < multipointShape.Points.Count; j++)
                {
                    resultMultiPointShape.Points.Add(multipointShape.Points[j]);
                }
            }

            return resultMultiPointShape;
        }

       
        public static bool RemoveVertex(Vertex selectedVertex, MultilineShape multilineShape)
        {
            Validators.CheckParameterIsNotNull(multilineShape, "multilineShape");

            return multilineShape.RemoveVertex(selectedVertex);
        }

       
        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = false;
            foreach (LineShape lineShape in this.Lines)
            {
                deleteSucceed = lineShape.RemoveVertex(selectedVertex);
                if (deleteSucceed)
                {
                    break;
                }
            }

            return deleteSucceed;
        }

        internal void Scale(double factor)
        {
            int totalPointCount = 0;
            double totalX = 0;
            double totalY = 0;

            foreach (LineShape lineShape in lines)
            {
                totalPointCount += lineShape.Vertices.Count;

                for (int i = 0; i < lineShape.Vertices.Count; i++)
                {
                    totalX = totalX + lineShape.Vertices[i].X;
                    totalY = totalY + lineShape.Vertices[i].Y;
                }
            }

            double alpha = totalX / totalPointCount;
            double beta = totalY / totalPointCount;

            foreach (LineShape lineShape in lines)
            {
                for (int i = 0; i < lineShape.Vertices.Count; i++)
                {
                    double newX = (lineShape.Vertices[i].X - alpha) * factor + alpha;
                    double newY = (lineShape.Vertices[i].Y - beta) * factor + beta;

                    lineShape.Vertices[i] = new Vertex(newX, newY);
                }
            }
        }

        private static void GetWktForALine(LineShape lineShape, StringBuilder wellKnownText)
        {
            wellKnownText.Append("(");

            for (int i = 0; i < lineShape.Vertices.Count; i++)
            {
                wellKnownText.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", lineShape.Vertices[i].X, lineShape.Vertices[i].Y));
                if (i < (lineShape.Vertices.Count - 1))
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");
        }

        private LineShape GetLineShapeWithInTolerance(Vertex vertex, double Tolerance, GeographyUnit ShapesUnit, DistanceUnit ToleranceUnit)
        {
            LineShape returnLineShape = null;

            for (int i = 0; i < lines.Count; i++)
            {
                Vertex StartPoint = lines[i].Vertices[0];
                Vertex EndPoint = lines[i].Vertices[lines[i].Vertices.Count - 1];
                double DistancToStartPoint = vertex.GetDistanceTo(StartPoint, ShapesUnit, ToleranceUnit);
                double DistanceToEndPoint = vertex.GetDistanceTo(EndPoint, ShapesUnit, ToleranceUnit);

                if (DistancToStartPoint <= Tolerance)
                {
                    returnLineShape = lines[i];
                    lines.RemoveAt(i);
                    break;
                }
                if (DistanceToEndPoint <= Tolerance)
                {
                    lines[i].ReversePoints();
                    returnLineShape = lines[i];
                    lines.RemoveAt(i);
                    break;
                }
            }
            return returnLineShape;
        }

        
        private bool CheckMutliLineIsValidForReOrder(double tolerance, GeographyUnit shapeUnit, DistanceUnit unitOfTolerance)
        {
            bool IsValid = true;

            if ((lines.Count == 1))
            {
                return true;
            }

            for (int i = 0; i <= lines.Count - 1; i++)
            {
                Vertex StartPoint = lines[i].Vertices[0];
                Vertex EndPoint = lines[i].Vertices[lines[i].Vertices.Count - 1];
                int StartPointWithinToleranceCount = 0;
                int EndPointWithinToleranceCount = 0;
                for (int j = 0; j <= lines.Count - 1; j++)
                {
                    if ((j != i))
                    {
                        double DistancToStartPoint = StartPoint.GetDistanceTo(lines[j], shapeUnit, unitOfTolerance);
                        double DistanceToEndPoint = EndPoint.GetDistanceTo(lines[j], shapeUnit, unitOfTolerance);
                        if ((DistancToStartPoint <= tolerance))
                        {
                            StartPointWithinToleranceCount += 1;
                        }
                        if ((DistanceToEndPoint <= tolerance))
                        {
                            EndPointWithinToleranceCount += 1;
                        }
                    }
                }
                if ((StartPointWithinToleranceCount > 1 || EndPointWithinToleranceCount > 1))
                {
                    IsValid = false;
                    break;
                }
                if ((StartPointWithinToleranceCount == 0 && EndPointWithinToleranceCount == 0))
                {
                    IsValid = false;
                    break;
                }
            }

            return IsValid;
        }

       
        private bool CheckInputPointIsValidForReOrder(PointShape pointShape, double tolerance, GeographyUnit shapeUnit, DistanceUnit unitOfTolerance)
        {
            bool InputPointIsValid = true;

            int InputPointCount = 0;
            foreach (LineShape LineShape in lines)
            {
                Vertex StartPoint = LineShape.Vertices[0];
                Vertex EndPoint = LineShape.Vertices[LineShape.Vertices.Count - 1];
                double DistancToStartPoint = StartPoint.GetDistanceTo(pointShape, shapeUnit, unitOfTolerance);
                double DistanceToEndPoint = EndPoint.GetDistanceTo(pointShape, shapeUnit, unitOfTolerance);

                if (DistancToStartPoint <= tolerance)
                {
                    InputPointCount += 1;
                }
                if (DistanceToEndPoint <= tolerance)
                {
                    InputPointCount += 1;
                }
            }

            if ((InputPointCount != 1))
            {
                InputPointIsValid = false;
            }

            return InputPointIsValid;
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
