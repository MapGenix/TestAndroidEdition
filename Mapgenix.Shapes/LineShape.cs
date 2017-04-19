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
    /// <summary>Line defined as a single line with two or more vertices.</summary>
    [Serializable]
    public class LineShape : BaseLineShape
    {
        private Collection<Vertex> vertices;

        /// <summary>To create the LineShape.</summary>
        public LineShape()
            : this(new Vertex[] { })
        {
        }

        /// <summary>To create the LineShape.</summary>
        public LineShape(IEnumerable<Vertex> points)
        {
            Validators.CheckParameterIsNotNull(points, "points");

            vertices = new Collection<Vertex>();

            foreach (Vertex tempVertex in points)
            {
                vertices.Add(tempVertex);
            }
        }

        /// <summary>To create the LineShape.</summary>
        public LineShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        /// <summary>To create the LineShape.</summary>
        public LineShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

        /// <summary>Gets the collection of points making up the LineShape.</summary>
        public Collection<Vertex> Vertices
        {
            get
            {
                return vertices;
            }
        }

        /// <summary>Reverses the order of the points in the line.</summary>
        public void ReversePoints()
        {
            Validators.CheckShapeIsValidForOperation(this);

            ReversePointsCore();
        }

        /// <summary>Reverses the order of the points in the line.</summary>
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

        /// <summary>Returns a complete copy of the shape.</summary>
        protected override BaseShape CloneDeepCore()
        {
            LineShape line = new LineShape();

            foreach (Vertex vertex in vertices)
            {
                line.vertices.Add(vertex);
            }

            return line;
        }

        /// <summary>Returns true if the line is closed, meaning the last point and
        /// first point have the same X and Y values.</summary>
        public bool IsClosed()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return IsClosedCore();
        }

        /// <summary>Returns true if the line is closed, meaning the last point and
        /// first point have the same X and Y values.</summary>
       protected virtual bool IsClosedCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            bool isLineShapeClosed = false;

            if (vertices[0] == vertices[vertices.Count - 1])
            {
                isLineShapeClosed = true;
            }

            return isLineShapeClosed;
        }

       /// <summary>Returns the length of the shape.</summary>
       /// <returns>Length of the shape.</returns>
       /// <param name="shapeUnit">GeographyUnit of the shape.</param>
       /// <param name="returningUnit">DistanceUnit for the return value.</param>
        protected override double GetLengthCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double totalDistance = 0.0;

            double distance = 0.0;
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                for (int i = 0; i <= vertices.Count - 2; i++)
                {
                    Vertex currentStartVertex = vertices[i];
                    Vertex currentEndVertex = vertices[i + 1];
                    distance = currentStartVertex.GetDistanceFromVertex(currentEndVertex.X, currentEndVertex.Y);
                    totalDistance += distance;
                }
                DistanceUnit distanceShapeUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                totalDistance = Conversion.ConvertMeasureUnits(totalDistance, distanceShapeUnit, returningUnit);
            }
            else
            {
                for (int i = 0; i <= vertices.Count - 2; i++)
                {
                    Vertex pt1 = vertices[i];
                    Vertex pt2 = vertices[i + 1];
                    distance = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(pt1.X, pt1.Y, pt2.X, pt2.Y, returningUnit);
                    totalDistance += distance;
                }
            }
            return totalDistance;
        }

        /// <summary>Returns a PointShape on the line, based on a percentage of the
        ///     length of the line from the first or last vertex (Dynamic segmentation).
        /// </summary>
        public PointShape GetPointOnALine(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return GetPointOnALineCore(startingPoint, percentageOfLine);
        }

        /// <summary>Returns a PointShape on the line, based on a percentage of the
        ///     length of the line from the first or last vertex (Dynamic segmentation).
        /// </summary>
        protected virtual PointShape GetPointOnALineCore(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsInRange(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double length = GetLength(GeographyUnit.Meter, DistanceUnit.Meter);
            double percentageLength = (percentageOfLine / 100.0) * length;

            return GetPointOnALine(startingPoint, percentageLength, GeographyUnit.Meter, DistanceUnit.Meter);
        }

        /// <summary>Dynamic segmentation</summary>
        public PointShape GetPointOnALine(StartingPoint startingPoint, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetPointOnALineCore(startingPoint, distance, shapeUnit, distanceUnit);
        }

        /// <summary>Dynamic segmentation</summary>
        protected virtual PointShape GetPointOnALineCore(StartingPoint startingPoint, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            PointShape returnPointShape = null;
            if (distance == 0)
            {
                if (startingPoint == StartingPoint.FirstPoint)
                {
                    returnPointShape = new PointShape(vertices[0].X, vertices[0].Y);
                }
                else
                {
                    returnPointShape = new PointShape(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y);
                }
            }
            else
            {
                double lengthInUnit = GetLength(shapeUnit, distanceUnit);
                if (lengthInUnit == distance)
                {
                    if (startingPoint == StartingPoint.FirstPoint)
                    {
                        returnPointShape = new PointShape(vertices[vertices.Count - 1].X, vertices[vertices.Count - 1].Y);
                    }
                    else
                    {
                        returnPointShape = new PointShape(vertices[0].X, vertices[0].Y);
                    }
                }
                else if (distance <= lengthInUnit)
                {
                    double realDistance = double.MinValue;
                    if (shapeUnit == GeographyUnit.DecimalDegree)
                    {
                        if (startingPoint == StartingPoint.FirstPoint)
                        {
                            return GetPointByDistance(distance);
                        }
                        else
                        {
                            double reverseDist = GetLength(shapeUnit, distanceUnit) - distance;
                            return GetPointByDistance(reverseDist);
                        }
                    }
                    else
                    {
                        DistanceUnit distanceShapeUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                        realDistance = Conversion.ConvertMeasureUnits(distance, distanceUnit, distanceShapeUnit);
                    }
                    if (startingPoint == StartingPoint.FirstPoint)
                    {
                        returnPointShape = GetPointShapeBasedOnDistance(realDistance);
                    }
                    else
                    {
                        double reverseDist = GetLength(shapeUnit, distanceUnit) - realDistance;
                        returnPointShape = GetPointShapeBasedOnDistance(reverseDist);
                    }
                }
                else
                {
                    LineShape line = new LineShape(Vertices);
                    line.ScaleUp(10);
                    returnPointShape = line.GetPointOnALine(startingPoint, distance, shapeUnit, distanceUnit);
                }
            }
            return returnPointShape;
        }

        /// <summary>Dynamic segmentation</summary>
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, float startingPercentageOfTheLine, float percentageOfTheLine)
        {
            Validators.CheckIfInputValueIsInRange(startingPercentageOfTheLine, "startingPercentageOfTheLine", 0, RangeCheckingInclusion.IncludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsInRange(percentageOfTheLine, "percentageOfTheLine", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.IncludeValue);
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
                result = ((LineShape)lineBaseShape).GetLineOnALine(StartingPoint.LastPoint, percentageOfTheLine);
            }

            return result;
        }

        /// <summary>Dynamic segmentation</summary>
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, percentageOfLine);
        }

        /// <summary>Dynamic segmentation</summary>
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, float percentageOfLine)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentageOfLine, "percentageOfLine", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double lengthOfThisShape = GetLength(GeographyUnit.Meter, DistanceUnit.Meter);
            double percentageLength = lengthOfThisShape * percentageOfLine / 100;

            return GetLineOnALineCore(startingPoint, 0, percentageLength, GeographyUnit.Meter, DistanceUnit.Meter);
        }

        /// <summary>Dynamic segmentation</summary>
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, double startingDistance, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(startingDistance, "startingDistance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, startingDistance, distance, shapeUnit, distanceUnit);
        }

        /// <summary>Dynamic segmentation</summary>
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, double startingDistance, double distance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(startingDistance, "startingDistance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double endingDistance = startingDistance + distance;
            if (startingDistance >= endingDistance)
            {
                return null;
            }

            PointShape startPointShape = GetPointOnALine(startingPoint, startingDistance, shapeUnit, distanceUnit);
            PointShape endingPointShape = GetPointOnALine(startingPoint, endingDistance, shapeUnit, distanceUnit);

            LineShape resultLineShape = new LineShape();
            if (startPointShape == null)
            {
                return null;
            }
            resultLineShape.vertices.Add(new Vertex(startPointShape.X, startPointShape.Y));

            double totalDistance = 0.0;
            if (startingPoint == StartingPoint.FirstPoint)
            {
                for (int i = 1; i < vertices.Count; i++)
                {
                    totalDistance = totalDistance + vertices[i - 1].GetDistanceTo(vertices[i], shapeUnit, distanceUnit);
                    if ((startingDistance < totalDistance) && (endingDistance > totalDistance))
                    {
                        resultLineShape.vertices.Add(new Vertex(vertices[i].X, vertices[i].Y));
                    }
                    else if (totalDistance > endingDistance)
                    {
                        break;
                    }
                }
            }
            else
            {
                int i = vertices.Count - 1;
                while (i > 0)
                {
                    totalDistance = totalDistance + vertices[i].GetDistanceTo(vertices[i - 1], shapeUnit, distanceUnit);
                    if ((startingDistance < totalDistance) && (endingDistance > totalDistance))
                    {
                        resultLineShape.vertices.Add(new Vertex(vertices[i - 1].X, vertices[i - 1].Y));
                    }
                    else if (totalDistance > endingDistance)
                    {
                        break;
                    }
                    i--;
                }
            }

            if (endingPointShape != null)
            {
                resultLineShape.vertices.Add(new Vertex(endingPointShape.X, endingPointShape.Y));
            }
            else
            {
                if (startingPoint == StartingPoint.FirstPoint)
                {
                    resultLineShape.vertices.Add(vertices[vertices.Count - 1]);
                }
                else
                {
                    resultLineShape.vertices.Add(vertices[0]);
                }
            }

            return resultLineShape;
        }

        /// <summary>Dynamic segmentation</summary>
        public BaseLineShape GetLineOnALine(StartingPoint startingPoint, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startingPoint, endPointShape);
        }

        /// <summary>Dynamic segmentation</summary>
        protected virtual BaseLineShape GetLineOnALineCore(StartingPoint startingPoint, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            LineShape resultLineShape = new LineShape();

            if (startingPoint == StartingPoint.FirstPoint)
            {
                resultLineShape = (LineShape)GetLineOnALine(new PointShape(Vertices[0]), endPointShape);
            }
            else
            {
                resultLineShape = (LineShape)GetLineOnALine(endPointShape, new PointShape(Vertices[Vertices.Count - 1]));
            }

            return resultLineShape;
        }

        /// <summary>Dynamic segmentation</summary>
        public BaseLineShape GetLineOnALine(PointShape startPointShape, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(startPointShape, "startPointShape");
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLineOnALineCore(startPointShape, endPointShape);
        }

        /// <summary>Dynamic segmentation</summary>
        protected virtual BaseLineShape GetLineOnALineCore(PointShape startPointShape, PointShape endPointShape)
        {
            Validators.CheckParameterIsNotNull(startPointShape, "startPointShape");
            Validators.CheckParameterIsNotNull(endPointShape, "endPointShape");
            Validators.CheckShapeIsValidForOperation(this);

            PointShape realStartPoint = GetClosestPointTo(startPointShape, GeographyUnit.Meter);
            PointShape realEndPoint = GetClosestPointTo(endPointShape, GeographyUnit.Meter);

            LineShape resultLineShape = new LineShape();

            if (realStartPoint.X != realEndPoint.X || realStartPoint.Y != realEndPoint.Y)
            {
                double startDistance = GetDistanceToFirstVertex(realStartPoint);
                double endDistance = GetDistanceToFirstVertex(realEndPoint);

                if (startDistance <= endDistance)
                {
                    resultLineShape = (LineShape)GetLineOnALine(StartingPoint.FirstPoint, startDistance, endDistance - startDistance, GeographyUnit.Meter, DistanceUnit.Meter);
                }
                else
                {
                    if (endDistance == 0)
                    {
                        resultLineShape = GetResultLineShapeForSameStartAndEndPoints(startPointShape, endPointShape);
                    }
                    else
                    {
                        resultLineShape = (LineShape)GetLineOnALine(StartingPoint.FirstPoint, endDistance, startDistance - endDistance, GeographyUnit.Meter, DistanceUnit.Meter);
                    }
                }
            }

            return resultLineShape;
        }

        private LineShape GetResultLineShapeForSameStartAndEndPoints(PointShape startPointShape, PointShape endPointShape)
        {
            Collection<Vertex> points = new Collection<Vertex>();
            bool isStart = false;
            foreach (Vertex vertex in this.Vertices)
            {
                if (isStart)
                {
                    if ((vertex.X == endPointShape.X) && (vertex.Y == endPointShape.Y))
                    {
                        points.Add(vertex);
                        break;
                    }
                    else
                    {
                        points.Add(vertex);
                        continue;
                    }
                }

                if ((vertex.X == startPointShape.X && vertex.Y == startPointShape.Y) || (vertex.X == endPointShape.X && vertex.Y == endPointShape.Y))
                {
                    points.Add(vertex);
                    isStart = true;
                }
            }

            LineShape line = new LineShape(points);
            return line;
        }

        private double GetDistanceToFirstVertex(PointShape pointShape)
        {
            double distance = 0.0;

            for (int i = 1; i <= Vertices.Count - 1; i++)
            {
                if (IsPointBetweenVerteces(Vertices[i], Vertices[i - 1], pointShape))
                {
                    double distanceToBeforeVertex = Vertices[i - 1].GetDistanceTo(pointShape, GeographyUnit.Meter, DistanceUnit.Meter);
                    distance += distanceToBeforeVertex;
                    break;
                }
                else
                {
                    double vertexDistance = Vertices[i].GetDistanceTo(Vertices[i - 1], GeographyUnit.Meter, DistanceUnit.Meter);
                    distance += vertexDistance;
                }
            }

            return distance;
        }

      
        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double minDistance = double.MaxValue;
            PointShape closestPoint = null;

            switch (targetShape.GetType().Name)
            {
                case "PointShape":
                    GetClosestInfoToPointShape((PointShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "MultipointShape":
                    GetClosestInfoToMutliPointShape((MultipointShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "LineShape":
                    GetClosestInfoToLineShape((LineShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "MultilineShape":
                    GetCloestInfoToMultilineShape((MultilineShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "PolygonShape":
                    GetCloestInfoToPolygon((PolygonShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "MultipolygonShape":
                    GetClosestInfoToMultiPolygon((MultipolygonShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "EllipseShape":
                    GetCloestInfoToEllipse((EllipseShape)targetShape, shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "RectangleShape":
                    GetCloestInfoToPolygon(((RectangleShape)targetShape).ToPolygon(), shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                case "RingShape":
                    GetCloestInfoToPolygon(((RingShape)targetShape).ToPolygon(), shapeUnit, distanceUnit, out closestPoint, out minDistance);
                    break;

                default:
                    minDistance = base.GetDistanceToCore(targetShape, shapeUnit, distanceUnit);
                    break;
            }
            return minDistance;
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

            LineShape resultLineShape = new LineShape();
            double longitudeDif, latitudeDif;
            double destX = toPoint.X;
            double destY = toPoint.Y;

            if (toUnit == GeographyUnit.DecimalDegree)
            {
                for (int i = 0; i <= vertices.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        double xDif = vertices[i].X - fromPoint.X;
                        double yDif = vertices[i].Y - fromPoint.Y;
                        DistanceUnit fromDistanceUnit = fromUnit;
                        longitudeDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xDif, fromDistanceUnit, toPoint.Y);
                        latitudeDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yDif, fromDistanceUnit, toPoint.X);

                        if (double.IsNaN(longitudeDif)) { longitudeDif = 0; }
                        if (double.IsNaN(latitudeDif)) { latitudeDif = 0; }

                        if (longitudeDif != 0 || latitudeDif != 0)
                        {
                            double newX = destX + longitudeDif;
                            double newY = destY + latitudeDif;
                            resultLineShape.vertices.Add(new Vertex(newX, newY));
                        }
                    }
                    else
                    {
                        double xDif = vertices[i].X - vertices[i - 1].X;
                        double yDif = vertices[i].Y - vertices[i - 1].Y;
                        longitudeDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xDif, fromUnit, toPoint.Y);
                        latitudeDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yDif, fromUnit, toPoint.X);

                        if (double.IsNaN(longitudeDif)) { longitudeDif = 0; }
                        if (double.IsNaN(latitudeDif)) { latitudeDif = 0; }

                        if (longitudeDif != 0 || latitudeDif != 0)
                        {
                            double newX = resultLineShape.vertices[resultLineShape.vertices.Count - 1].X + longitudeDif;
                            double newY = resultLineShape.vertices[resultLineShape.vertices.Count - 1].Y + latitudeDif;
                            resultLineShape.vertices.Add(new Vertex(newX, newY));
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i <= vertices.Count - 1; i++)
                {
                    DistanceUnit toDistanceUnit = Conversion.ConvertGeographyUnitToDistanceUnit(toUnit);
                    if (i == 0)
                    {
                        double xDif = Conversion.ConvertMeasureUnits(vertices[i].X - fromPoint.X, fromUnit, toDistanceUnit);
                        double yDif = Conversion.ConvertMeasureUnits(vertices[i].Y - fromPoint.Y, fromUnit, toDistanceUnit);

                        double newX = destX + xDif;
                        double newY = destY + yDif;
                        resultLineShape.vertices.Add(new Vertex(newX, newY));
                    }
                    else
                    {
                        double xDif = Conversion.ConvertMeasureUnits(vertices[i].X - vertices[i - 1].X, fromUnit, toDistanceUnit);
                        double yDif = Conversion.ConvertMeasureUnits(vertices[i].Y - vertices[i - 1].Y, fromUnit, toDistanceUnit);

                        double newX = resultLineShape.vertices[resultLineShape.vertices.Count - 1].X + xDif;
                        double newY = resultLineShape.vertices[resultLineShape.vertices.Count - 1].Y + yDif;
                        resultLineShape.vertices.Add(new Vertex(newX, newY));
                    }
                }
            }
            return resultLineShape;
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
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

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
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex tempVertex = vertices[i];
                tempVertex.Rotate(pivotPoint, degreeAngle);
                vertices[i] = tempVertex;
            }
        }

        public override bool CanRotate
        {
            get
            {
                return true;
            }
        }

       protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return GetBoundingBoxFromVertices(vertices);
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
                if (this.Intersects(targetShape))
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

            switch (targetShape.GetType().Name)
            {
                case "PointShape":
                    GetClosestInfoToPointShape((PointShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "MultipointShape":
                    GetClosestInfoToMutliPointShape((MultipointShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "LineShape":
                    GetClosestInfoToLineShape((LineShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "MultilineShape":
                    GetCloestInfoToMultilineShape((MultilineShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "PolygonShape":
                    GetCloestInfoToPolygon((PolygonShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "MultipolygonShape":
                    GetClosestInfoToMultiPolygon((MultipolygonShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "RingShape":
                    GetCloestInfoToPolygon(((RingShape)targetShape).ToPolygon(), shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "RectangleShape":
                    GetCloestInfoToPolygon(((RectangleShape)targetShape).ToPolygon(), shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                case "EllipseShape":
                    GetCloestInfoToEllipse((EllipseShape)targetShape, shapeUnit, DistanceUnit.Meter, out closestPoint, out minDistance);
                    break;

                default:
                    closestPoint = base.GetClosestPointTo(targetShape, shapeUnit);
                    break;
            }
            return closestPoint;
        }

       protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            StringBuilder wellKnownText = new StringBuilder();
            wellKnownText.Append("LINESTRING(");

            for (int i = 0; i < vertices.Count; i++)
            {
                wellKnownText.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1}", vertices[i].X, vertices[i].Y));
                if (i < (vertices.Count - 1))
                {
                    wellKnownText.Append(",");
                }
            }

            wellKnownText.Append(")");

            return wellKnownText.ToString();
        }

        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Line;
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

                ShapeConverter.WriteWkb(WkbShapeType.LineString, byteOrder, wkbWriter);
                ShapeConverter.WriteWkb(vertices.Count, byteOrder, wkbWriter);

                for (int i = 0; i < vertices.Count; i++)
                {
                    ShapeConverter.WriteWkb(vertices[i].X, byteOrder, wkbWriter);
                    ShapeConverter.WriteWkb(vertices[i].Y, byteOrder, wkbWriter);
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

            LineShape line = (LineShape)BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            CloneOneLineToAnother(line, this);
        }

       protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LineShape line = (LineShape)BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            CloneOneLineToAnother(line, this);
        }

        private static void CloneOneLineToAnother(LineShape fromLine, LineShape toLine)
        {
            toLine.Vertices.Clear();

            for (int i = 0; i < fromLine.Vertices.Count; i++)
            {
                toLine.vertices.Add(fromLine.vertices[i]);
            }
        }

       
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validationResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (vertices.Count < 2)
                    {
                        validationResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    break;

                case ShapeValidationMode.Advanced:
                    break;

                default:
                    break;
            }

            return validationResult;
        }

       
        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            MultipointShape resultMultiPointShape = new MultipointShape();

            if (targetShape is BaseAreaShape)
            {
                if (targetShape is MultipolygonShape)
                {
                    resultMultiPointShape = GetCrossingForMultipolygonShape(targetShape);
                }
                else
                {
                    resultMultiPointShape = GetCrossingForAreaBaseShape(targetShape);
                }
            }
            else
            {
                resultMultiPointShape = GetCrossingForSimpleBaseShape(targetShape);
            }

            return resultMultiPointShape;
        }

      
        public static bool RemoveVertex(Vertex selectedVertex, LineShape lineShape)
        {
            Validators.CheckParameterIsNotNull(lineShape, "lineShape");

            return lineShape.RemoveVertex(selectedVertex);
        }

     
        public bool RemoveVertex(Vertex selectedVertex)
        {
            bool deleteSucceed = false;
            if (this.Vertices.Count > 2)
            {
                deleteSucceed = this.Vertices.Remove(selectedVertex);
            }
            return deleteSucceed;
        }

        private MultipointShape GetCrossingForAreaBaseShape(BaseShape targetShape)
        {
            MultipointShape resultMultiPointShape = new MultipointShape();



            PolygonShape targetPolygon = BaseShape.CreateShapeFromWellKnownData(targetShape.GetWellKnownBinary()) as PolygonShape;

            if (targetPolygon != null)
            {
                LineShape outerLine = new LineShape(targetPolygon.OuterRing.Vertices);
                MultipointShape outerRingResult = GetCrossingForSimpleBaseShape(outerLine);

                for (int i = 0; i < outerRingResult.Points.Count; i++)
                {
                    resultMultiPointShape.Points.Add(outerRingResult.Points[i]);
                }

                for (int i = 0; i < targetPolygon.InnerRings.Count; i++)
                {
                    LineShape innerLine = new LineShape(targetPolygon.InnerRings[i].Vertices);
                    MultipointShape innerRingResult = GetCrossingForSimpleBaseShape(innerLine);

                    for (int j = 0; j < innerRingResult.Points.Count; j++)
                    {
                        resultMultiPointShape.Points.Add(innerRingResult.Points[j]);
                    }
                }
            }

            return resultMultiPointShape;
        }

        private MultipointShape GetCrossingForMultipolygonShape(BaseShape targetShape)
        {
            MultipointShape resultMultiPointShape = new MultipointShape();
            MultipolygonShape targetPolygon = targetShape as MultipolygonShape;

            if (targetPolygon != null)
            {
                for (int i = 0; i < targetPolygon.Polygons.Count; i++)
                {
                    MultipointShape multipointShape = GetCrossingForAreaBaseShape(targetPolygon.Polygons[i]);

                    for (int j = 0; j < multipointShape.Points.Count; j++)
                    {
                        resultMultiPointShape.Points.Add(multipointShape.Points[j]);
                    }
                }
            }

            return resultMultiPointShape;
        }

        private MultipointShape GetCrossingForSimpleBaseShape(BaseShape targetShape)
        {
            MultipointShape resultMultiPointShape = new MultipointShape();

            Geometry source = ShapeConverter.ShapeToJtsShape(this);
            Geometry target = ShapeConverter.ShapeToJtsShape(targetShape);

            Geometry result = source.Intersection(target) as Geometry;

            if (result != null)
            {
                GeometryCollection resultCollection = result as GeometryCollection;

                if (resultCollection != null)
                {
                    foreach (Geometry resultGeometry in resultCollection.Geometries)
                    {
                        AddResultToMultiPointShape(resultGeometry, resultMultiPointShape);
                    }
                }
                else
                {
                    AddResultToMultiPointShape(result, resultMultiPointShape);
                }
            }

            return resultMultiPointShape;
        }

        private static void AddResultToMultiPointShape(Geometry result, MultipointShape resultMultiPointShape)
        {
            BaseShape resultShape = ShapeConverter.JtsShapeToShape(result);
            switch (resultShape.GetWellKnownType())
            {
                case WellKnownType.Point:
                    resultMultiPointShape.Points.Add((PointShape)resultShape);

                    break;
                case WellKnownType.Multipoint:
                    resultMultiPointShape = (MultipointShape)resultShape;

                    break;
                case WellKnownType.Line:
                    LineShape resultLineShape = (LineShape)resultShape;
                    for (int i = 0; i < resultLineShape.Vertices.Count; i++)
                    {
                        resultMultiPointShape.Points.Add(new PointShape(resultLineShape.Vertices[i]));
                    }

                    break;
                case WellKnownType.Multiline:
                    MultilineShape resultMultilineShape = (MultilineShape)resultShape;
                    for (int i = 0; i < resultMultilineShape.Lines.Count; i++)
                    {
                        for (int j = 0; j < resultMultilineShape.Lines[i].Vertices.Count; j++)
                        {
                            resultMultiPointShape.Points.Add(new PointShape(resultMultilineShape.Lines[i].Vertices[j]));
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        internal void Scale(double factor)
        {
            double Alpha, Beta, TotalX, TotalY, Xvalue, Yvalue;
            TotalX = 0;
            TotalY = 0;

            for (int i = 0; i < vertices.Count; i++)
            {
                TotalX = TotalX + vertices[i].X;
                TotalY = TotalY + vertices[i].Y;
            }
            Alpha = TotalX / vertices.Count;
            Beta = TotalY / vertices.Count;

            for (int i = 0; i < vertices.Count; i++)
            {
                Xvalue = ((vertices[i].X - Alpha) * factor) + Alpha;
                Yvalue = ((vertices[i].Y - Beta) * factor) + Beta;
                vertices[i] = new Vertex(Xvalue, Yvalue);
            }
        }

        private void GetCloestInfoToEllipse(EllipseShape ellipse, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            minDistance = double.MaxValue;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vertex pt1 = vertices[i];
                Vertex pt2 = vertices[i + 1];
                PointShape ellipseCenter = ellipse.GetBoundingBox().GetCenterPoint();
                PointShape lineSegmentPointShape = ellipseCenter.GetNearestPointShapeFromLineSegment(pt1.X, pt1.Y, pt2.X, pt2.Y);

                PointShape interPointShape = ellipse.GeShortestPointFromOutsidePoint(lineSegmentPointShape);
                double distance = 0.0;

                if (interPointShape != null)
                {
                    distance = interPointShape.GetDistanceTo(lineSegmentPointShape, shapeUnit, distanceUnit);
                }
                if (i == 0 || distance < minDistance)
                {
                    minDistance = distance;
                    closestPointShape = lineSegmentPointShape;
                }
            }
        }

        private void GetCloestInfoToPolygon(PolygonShape polygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            minDistance = double.MaxValue;
            closestPointShape = null;

            MultilineShape multilineShape = PolygonShape.ToLineShapes(polygonShape);
            foreach (LineShape lineShape in multilineShape.Lines)
            {
                double currentDistance = GetDistanceTo(lineShape, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = GetClosestPointToCore(lineShape, shapeUnit);
                }
            }
        }

        private void GetClosestInfoToMultiPolygon(MultipolygonShape multiPolygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            minDistance = double.MaxValue;
            closestPointShape = null;

            foreach (PolygonShape polygon in multiPolygonShape.Polygons)
            {
                double currentDistance = GetDistanceTo(polygon, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = GetClosestPointToCore(polygon, shapeUnit);
                }
            }
        }

        private void GetClosestInfoToPointShape(PointShape pointShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                Vertex lineStartPoint = vertices[i];
                Vertex lineEndPoint = vertices[i + 1];

                if (shapeUnit != GeographyUnit.DecimalDegree)
                {
                    double currentDistance = pointShape.GetDistanceFromLineSegment(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        closestPointShape = pointShape.GetNearestPointShapeFromLineSegment(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y);
                    }
                }
                else
                {
                    double currentDistance = DecimalDegreesHelper.GetDistanceFromDecimalDegreesLine(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y, pointShape, distanceUnit);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        closestPointShape = DecimalDegreesHelper.GetNearestPointFromPointShapeDecimalDegreesLine(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y, pointShape);
                    }
                }
            }

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit convertShapeUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                minDistance = Conversion.ConvertMeasureUnits(minDistance, convertShapeUnit, distanceUnit);
            }
        }

        private void GetClosestInfoToMutliPointShape(MultipointShape multiPointShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            foreach (PointShape point in multiPointShape.Points)
            {
                double currentDistance = GetDistanceTo(point, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = GetClosestPointToCore(point, shapeUnit);
                }
            }
        }

        private void GetClosestInfoToLineShape(LineShape lineShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            Vertex minDistanceVertex = new Vertex();
            closestPointShape = null;

            minDistance = double.MaxValue;
            minDistanceVertex = new Vertex();
            closestPointShape = null;

            foreach (Vertex targetVertex in lineShape.vertices)
            {
                double currentDistance = targetVertex.GetDistanceTo(this, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    minDistanceVertex = targetVertex;
                }
            }

            PointShape minPoint = new PointShape(minDistanceVertex.X, minDistanceVertex.Y);
            closestPointShape = GetClosestPointToCore(minPoint, shapeUnit);

            foreach (Vertex sourceVertex in vertices)
            {
                double currentDistance = sourceVertex.GetDistanceTo(lineShape, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = new PointShape(sourceVertex.X, sourceVertex.Y);
                }
            }
        }

        private void GetCloestInfoToMultilineShape(MultilineShape multilineShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPointShape, out double minDistance)
        {
            minDistance = double.MaxValue;
            closestPointShape = null;

            foreach (LineShape lineSahpe in multilineShape.Lines)
            {
                double currentDistance = GetDistanceTo(lineSahpe, shapeUnit, distanceUnit);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointShape = GetClosestPointToCore(lineSahpe, shapeUnit);
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

        private PointShape GetPointShapeBasedOnDistance(double distance)
        {
            int i;
            double dn, an, bn;
            double xn, yn;
            double xn1, yn1;
            double TotalDist = 0;
            double TotalDistB = 0;

            PointShape returnPointShape = null;
            if (distance != 0)
            {
                for (i = 0; i <= this.Vertices.Count - 2; i++)
                {
                    Vertex pt1 = this.Vertices[i];
                    Vertex pt2 = this.Vertices[i + 1];
                    xn = pt1.X;
                    yn = pt1.Y;
                    xn1 = pt2.X;
                    yn1 = pt2.Y;
                    dn = pt1.GetDistanceFromVertex(pt2.X, pt2.Y);
                    TotalDistB = TotalDistB + dn;
                    if (distance <= TotalDistB)
                    {
                        returnPointShape = new PointShape(this.Vertices[0]);
                        if (Math.Round(xn, 6) != Math.Round(xn1, 6))
                        {
                            an = (yn1 - yn) / (xn1 - xn);
                            bn = yn - (an * xn);
                            double scale = (distance - TotalDist) / dn;
                            double xDistance = xn1 - xn;
                            double resultX = scale * xDistance + xn;
                            double resultY = an * resultX + bn;

                            returnPointShape.X = resultX;
                            returnPointShape.Y = resultY;
                            break;
                        }
                        else
                        {
                            returnPointShape.X = xn;
                            if (yn <= yn1)
                            {
                                returnPointShape.Y = yn + (distance - (TotalDist));
                            }
                            else
                            {
                                returnPointShape.Y = yn - (distance - (TotalDist));
                            }
                            break;
                        }
                    }
                    TotalDist = TotalDist + dn;
                }
            }
            return returnPointShape;
        }

        private PointShape GetPointByDistance(double distance)
        {
            PointShape returnPoint = new PointShape();
            double totalDistance = 0;
            for (int i = 0; i < Vertices.Count - 1; i++)
            {
                double segmentLength = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(new Feature(Vertices[i]), new Feature(Vertices[i + 1]), DistanceUnit.Kilometer);
                totalDistance += segmentLength;
                if (totalDistance < distance)
                {
                    continue;
                }
                else if (totalDistance == distance)
                {
                    returnPoint = new PointShape(Vertices[i + 1]);
                    break;
                }
                else
                {
                    double circleRadius = segmentLength - (totalDistance - distance);
                    EllipseShape circle = new EllipseShape(new Feature(Vertices[i]), circleRadius);
                    LineShape segment = new LineShape(new Collection<Vertex> { Vertices[i], Vertices[i + 1] });

                    double yn = Vertices[i].Y - (Vertices[i].Y - Vertices[i + 1].Y) * (circleRadius / segmentLength);
                    double xn = Vertices[i].X - (Vertices[i].Y - yn) * (Vertices[i].X - Vertices[i + 1].X) / (Vertices[i].Y - Vertices[i + 1].Y);

                    returnPoint = new PointShape(xn, yn);
                    break;
                }
            }
            return returnPoint;
        }
    }
}
