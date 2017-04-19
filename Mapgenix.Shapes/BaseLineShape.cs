using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Utils;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Mapgenix.Shapes
{
    /// <summary>Root of all line-based shapes, such as LineShape and MultilineShape.</summary>
    [Serializable]
    public abstract class BaseLineShape : BaseShape
    {
        /// <summary>Default constructor for BaseLineShape.</summary>
        protected BaseLineShape()
        {
        }

        /// <summary>Returns the length of the shape.</summary>
        /// <returns>Length of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">DistanceUnit for the return value.</param>
        public double GetLength(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetLengthCore(shapeUnit, returningUnit);
        }

        /// <summary>Returns the length of the shape.</summary>
        /// <returns>Length of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">DistanceUnit for the return value.</param>
        protected virtual double GetLengthCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            string wkt = GetWellKnownText();
            BaseShape tmpBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);

            switch (tmpBaseShape.GetWellKnownType())
            {
                case WellKnownType.Line:
                    return ((LineShape)tmpBaseShape).GetLength(shapeUnit, returningUnit);

                case WellKnownType.Multiline:
                    return ((MultilineShape)tmpBaseShape).GetLength(shapeUnit, returningUnit);

                default:
                    return 0;
            }
        }

        /// <summary>Returns a new line shape scaled up by a percentage.</summary>
        /// <returns>New line shape scaled up by a percentage.</returns>
        /// <param name="sourceShape">Shape as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        public static BaseLineShape ScaleUp(BaseLineShape sourceShape, double percentage)
        {
            Validators.CheckParameterIsNotNull(sourceShape, "sourceLineBaseShape");
            Validators.CheckParameterIsValid(sourceShape, "sourceLineBaseShape");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            BaseLineShape returnLineBaseShape = (BaseLineShape)sourceShape.CloneDeepCore();
            returnLineBaseShape.ScaleUp(percentage);

            return returnLineBaseShape;
        }

        /// <summary>Returns a new feature scaled up by a percentage.</summary>
        /// <returns>New feature scaled up by a percentage.</returns>
        /// <param name="sourceLine">Feature as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to increase the feature shape's size.</param>
        public static Feature ScaleUp(Feature sourceLine, double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape baseShape = sourceLine.GetShape();
            Validators.CheckShapeIsLineBaseShape(baseShape);
            Validators.CheckParameterIsValid(baseShape, "sourceLineFeature");

            BaseLineShape resultLineBaseShape = ScaleUp((BaseLineShape)baseShape, percentage);
            Feature resultLineFeature = new Feature(resultLineBaseShape, sourceLine.ColumnValues);

            return resultLineFeature;
        }

        /// <summary>Increases the size of the line shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        public void ScaleUp(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            ScaleUpCore(percentage);
        }

        /// <summary>Increases the size of the area shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        protected virtual void ScaleUpCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 + percentage / 100.0;
            Scale(factor);
        }

        /// <summary>Returns a new line shape scaled down by a percentage.</summary>
        /// <returns>New line shape scaled down by a percentage.</returns>
        /// <param name="sourceLineBaseShape">Line shape as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        public static BaseLineShape ScaleDown(BaseLineShape sourceLineBaseShape, double percentage)
        {
            Validators.CheckParameterIsNotNull(sourceLineBaseShape, "sourceLineBaseShape");
            Validators.CheckParameterIsValid(sourceLineBaseShape, "sourceLineBaseShape");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            BaseLineShape returnShape = (BaseLineShape)sourceLineBaseShape.CloneDeepCore();
            returnShape.ScaleDown(percentage);

            return returnShape;
        }

        /// <summary>Returns a new feature scaled down by a percentage.</summary>
        /// <returns>New feature scaled down by a percentage.</returns>
        /// <param name="sourceLine">Feature as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to decrease the feature shape's size.</param>
        public static Feature ScaleDown(Feature sourceLine, double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape baseShape = sourceLine.GetShape();
            Validators.CheckShapeIsLineBaseShape(baseShape);
            Validators.CheckParameterIsValid(baseShape, "sourceLineFeature");

            BaseLineShape resultLineBaseShape = ScaleDown((BaseLineShape)baseShape, percentage);
            Feature resultFeature = new Feature(resultLineBaseShape, sourceLine.ColumnValues);

            return resultFeature;
        }

        /// <summary>Decreases the size of the line shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        public void ScaleDown(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            ScaleDownCore(percentage);
        }

        /// <summary>Decreases the size of the line shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        protected virtual void ScaleDownCore(double percentage)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            double factor = 1.0 - percentage / 100.0;
            Scale(factor);
        }

        /// <summary>Returns the convex hull of the shape.</summary>
        /// <returns>Convex hull of the shape.</returns>
        public RingShape ConvexHull()
        {
            Validators.CheckShapeIsValidForOperation(this);
            return ConvexHullCore();
        }

        /// <summary>Returns the convex hull of the shape.</summary>
        /// <returns>Convex hull of the shape.</returns>
        protected virtual RingShape ConvexHullCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Geometry jtsGeometry = ShapeConverter.ShapeToJtsShape(this);
            PolygonShape polygon = (PolygonShape)ShapeConverter.JtsShapeToShape((Geometry)jtsGeometry.ConvexHull());
            RingShape returnRingShape = polygon.OuterRing;

            return returnRingShape;
        }

        private void Scale(double factor)
        {
            string wkt = GetWellKnownText();
            BaseShape standandWktBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);

            switch (standandWktBaseShape.GetWellKnownType())
            {
                case WellKnownType.Line:
                    ((LineShape)standandWktBaseShape).Scale(factor);
                    break;

                case WellKnownType.Multiline:
                    ((MultilineShape)standandWktBaseShape).Scale(factor);
                    break;

                default:
                    break;
            }
            string scaleWkt = standandWktBaseShape.GetWellKnownText();
            this.LoadFromWellKnownData(scaleWkt);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="targetShape">Line shape to be simplified.</param>
        /// <param name="targetShapeUnit">Geography unit of the shape to perform the operation on.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="toleranceUnit">Distance unit of the tolerance.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multilineshape. </returns>
        public static MultilineShape Simplify(BaseLineShape targetShape, GeographyUnit targetShapeUnit, double tolerance, DistanceUnit toleranceUnit, SimplificationType simplificationType)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(targetShapeUnit, "targetShapeUnit");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
     
            double degreesDif = 0;
            if (targetShapeUnit == GeographyUnit.DecimalDegree)
            {
                degreesDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(tolerance, toleranceUnit, targetShape.GetBoundingBox().GetCenterPoint().Y);
            }
            else
            {
                degreesDif = Conversion.ConvertMeasureUnits(tolerance, toleranceUnit, Conversion.ConvertGeographyUnitToDistanceUnit(targetShapeUnit));
            }

            return Simplify(targetShape, degreesDif, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="targetShape">Line shape to be simplified.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multilineshape. </returns>
        public static MultilineShape Simplify(BaseLineShape targetShape, double tolerance, SimplificationType simplificationType)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
     
            BaseLineShape clonedShape = (BaseLineShape)targetShape.CloneDeep();
            return clonedShape.Simplify(tolerance, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="shapeUnit">Geography unit of the shape.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="toleranceUnit">Distance unit of the tolerance.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multilineshape. </returns>
        public MultilineShape Simplify(GeographyUnit shapeUnit, double tolerance, DistanceUnit toleranceUnit, SimplificationType simplificationType)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
     
            double degreesDif = 0;
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                degreesDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(tolerance, toleranceUnit, GetBoundingBox().GetCenterPoint().Y);
            }
            else
            {
                degreesDif = Conversion.ConvertMeasureUnits(tolerance, toleranceUnit, Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit));
            }

            return Simplify(degreesDif, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multilineshape. </returns>
        public MultilineShape Simplify(double tolerance, SimplificationType simplificationType)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
    
            return SimplifyCore(tolerance, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multilineshape. </returns>
        protected virtual MultilineShape SimplifyCore(double tolerance, SimplificationType simplificationType)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
           
            Geometry currentGeometry = ShapeConverter.ShapeToJtsShape(this);

            Geometry simplifiedGeometry = null;
            if (simplificationType == SimplificationType.DouglasPeucker)
            {
                simplifiedGeometry = (Geometry)DouglasPeuckerSimplifier.Simplify(currentGeometry, tolerance);
            }
            else
            {
                simplifiedGeometry = (Geometry)TopologyPreservingSimplifier.Simplify(currentGeometry, tolerance);
            }

            MultilineShape returnMultiLine = new MultilineShape();
            if (simplifiedGeometry is LineString || simplifiedGeometry is MultiLineString)
            {
                BaseLineShape simplifiedShape = (BaseLineShape)ShapeConverter.JtsShapeToShape(simplifiedGeometry);

                if (simplifiedShape.GetType() == typeof(LineShape))
                {
                    returnMultiLine.Lines.Add((LineShape)simplifiedShape);
                }
                else if (simplifiedShape.GetType() == typeof(MultilineShape))
                {
                    returnMultiLine = (MultilineShape)simplifiedShape;
                }
            }
            else if (simplifiedGeometry is GeometryCollection)
            {
                GeometryCollection geometryCollection = simplifiedGeometry as GeometryCollection;

                foreach (Geometry geometry in geometryCollection.Geometries)
                {
                    BaseShape resultShape = ShapeConverter.JtsShapeToShape(geometry);

                    if (geometry is LineString)
                    {
                        returnMultiLine.Lines.Add((LineShape)resultShape);
                    }
                    else if (geometry is MultiLineString)
                    {
                        MultilineShape multiLineShape = (MultilineShape)resultShape;
                        foreach (LineShape line in multiLineShape.Lines)
                        {
                            returnMultiLine.Lines.Add(line);
                        }
                    }
                }

                if (returnMultiLine.Lines.Count == 0)
                {
                    returnMultiLine = null;
                }
            }

            return returnMultiLine;
        }

        /// <summary>Returns the union of the shape and a target shape.</summary>
        /// <returns>MultilineShape as the union of the shape and a target shape.</returns>
        /// <param name="targetShape">Line shape to find the union with.</param>
        public MultilineShape Union(BaseLineShape targetShape)
        {
            return UnionCore(new BaseLineShape[] { targetShape });
        }

        /// <summary>Returns the union of the shape and target line shapes.</summary>
        /// <returns>MultilineShape as the union of the shape and target line shapes.</returns>
        /// <param name="targetShapes">Target line shapes to find the union with.</param>
        public static MultilineShape Union(IEnumerable<BaseLineShape> lineBaseShapes)
        {
            Validators.CheckParameterIsNotNull(lineBaseShapes, "lineBaseShapes");

            List<BaseLineShape> lines = new List<BaseLineShape>(lineBaseShapes);

            MultilineShape unionMultilineShape = null;
            BaseLineShape sourceShape = lines[0];
            for (int i = 1; i < lines.Count; i++)
            {
                if (unionMultilineShape == null)
                {
                    unionMultilineShape = sourceShape.Union(lines[i]);
                }
                else
                {
                    unionMultilineShape = unionMultilineShape.Union(lines[i]);
                }
            }
            if (unionMultilineShape == null)
            {
                if (sourceShape is MultilineShape)
                {
                    unionMultilineShape = sourceShape as MultilineShape;
                }
                else
                {
                    LineShape line = sourceShape as LineShape;
                    unionMultilineShape = new MultilineShape(new LineShape[] { line });
                }
            }

            return unionMultilineShape;
        }

        /// <summary>Returns the union of the feature and a target feature.</summary>
        /// <returns>MultilineShape as the union of the feature and a target feature.</returns>
        /// <param name="targetFeature">Feature to find the union with.</param>
        public MultilineShape Union(Feature targetFeature)
        {
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsLineBaseShape(targetShape);

            return Union((BaseLineShape)targetShape);
        }

        /// <summary>Returns the union of the shape and target features.</summary>
        /// <returns>MultilineShape as the union of the shape and target features.</returns>
        /// <param name="targetFeatures">Target features to find the union with.</param>
        public static MultilineShape Union(IEnumerable<Feature> targetFeatures)
        {
            Validators.CheckParameterIsNotNull(targetFeatures, "targetFeatures");

            Collection<BaseLineShape> lineBaseShapes = new Collection<BaseLineShape>();
            foreach (Feature feature in targetFeatures)
            {
                BaseLineShape lineBaseShape = (BaseLineShape)feature.GetShape();
                lineBaseShapes.Add(lineBaseShape);
            }

            return Union(lineBaseShapes);
        }

        /// <summary>Returns the union of the shape and target shapes.</summary>
        /// <returns>MultilineShape as the union of the shape and target shapes.</returns>
        /// <param name="lineBaseShapes">Line shapes to find the union with.</param>
        protected virtual MultilineShape UnionCore(IEnumerable<BaseLineShape> lineBaseShapes)
        {
            Validators.CheckParameterIsNotNull(lineBaseShapes, "lineBaseShapes");

            int count = 0;
            foreach (BaseLineShape shape in lineBaseShapes)
            {
                count++;
            }
            Geometry[] geometriesArray = new Geometry[count];

            int i = 0;
            foreach (BaseLineShape shape in lineBaseShapes)
            {
                geometriesArray[i] = ShapeConverter.ShapeToJtsShape(shape);
                i++;
            }

            Geometry geometry = ShapeConverter.ShapeToJtsShape(this);

            Geometry unionGeometry = null;
            foreach (Geometry tempGeo in geometriesArray)
            {
                if (unionGeometry == null)
                {
                    unionGeometry = geometry.Union(tempGeo) as Geometry;
                }
                else
                {
                    unionGeometry = unionGeometry.Union(tempGeo) as Geometry;
                }
            }

            BaseShape resultShape = ShapeConverter.JtsShapeToShape(unionGeometry);

            MultilineShape returnMultilineShape = null;
            if (resultShape.GetWellKnownType() == WellKnownType.Line)
            {
                returnMultilineShape = new MultilineShape();
                returnMultilineShape.Lines.Add((LineShape)resultShape);
            }
            else if (resultShape.GetWellKnownType() == WellKnownType.Multiline)
            {
                returnMultilineShape = (MultilineShape)resultShape;
            }
            return returnMultilineShape;
        }

        /// <summary>Returns the intersection of the shape and a target feature.</summary>
        /// <returns>MultilineShape as the intersection of the shape and a target feature.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetFeature">Feature to find the intersection with.</param>
        public MultilineShape GetIntersection(Feature targetFeature)
        {
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);
            Validators.CheckParameterIsValid(targetShape, "targetFeature");

            return GetIntersectionCore((BaseAreaShape)targetShape);
        }

        /// <summary>Returns the intersection of the shape and a target shape.</summary>
        /// <returns>MultilineShape as the intersection of the shape and a target shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Shape to find the intersection with.</param>
        public MultilineShape GetIntersection(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return GetIntersectionCore(targetShape);
        }

        /// <summary>Returns the intersection of the shape and a target shape.</summary>
        /// <returns>MultilineShape as the intersection of the shape and a target shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Shape to find the intersection with.</param>
        protected virtual MultilineShape GetIntersectionCore(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry jtsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            Geometry resultGeometry = (Geometry)jtsGeometry.Intersection(targetGeometry);
            MultilineShape intersectionMultiLineShape = new MultilineShape();
            if (resultGeometry != null)
            {
                BaseShape intersectionShape = BaseShape.CreateShapeFromWellKnownData(resultGeometry.AsText());
                WellKnownType type = intersectionShape.GetWellKnownType();

                if (type == WellKnownType.Multiline)
                {
                    intersectionMultiLineShape = (MultilineShape)intersectionShape;
                }
                else if (type == WellKnownType.Line)
                {
                    LineShape lineShape = (LineShape)intersectionShape;
                    intersectionMultiLineShape.Lines.Add(lineShape);
                }
            }

            return intersectionMultiLineShape;
        }

        internal static bool IsPointBetweenVerteces(Vertex vertex1, Vertex vertex2, PointShape point)
        {
            bool isBetween = false;
            double xOffset = Math.Abs(vertex2.X - vertex1.X);
            double yOffset = Math.Abs(vertex2.Y - vertex1.Y);

            if (xOffset <= 10E-5 && Math.Abs(vertex1.X - point.X) <= 10E-5)
            {
                if (point.Y >= Math.Min(vertex1.Y, vertex2.Y) && point.Y <= Math.Max(vertex1.Y, vertex2.Y))
                {
                    isBetween = true;
                }
            }
            else if (yOffset <= 10E-5 && Math.Abs(vertex1.Y - point.Y) <= 10E-5)
            {
                if (point.X >= Math.Min(vertex1.X, vertex2.X) && point.X <= Math.Max(vertex1.X, vertex2.X))
                {
                    isBetween = true;
                }
            }
            else
            {
                double xOffsetBetween = Math.Abs(point.X - vertex1.X);
                double yOffsetBetween = Math.Abs(point.Y - vertex1.Y);

                double xOffsetBetween2 = Math.Abs(point.X - vertex2.X);
                double yOffsetBetween2 = Math.Abs(point.Y - vertex2.Y);

                if (yOffsetBetween <= 10E-5 && xOffsetBetween <= 10E-5)
                {
                    isBetween = true;
                }
                else if (yOffsetBetween2 <= 10E-5 && xOffsetBetween2 <= 10E-5)
                {
                    isBetween = true;
                }
                else if (Math.Abs(yOffset / xOffset - yOffsetBetween / xOffsetBetween) <= 10E-5 || Math.Abs(xOffset / yOffset - xOffsetBetween / yOffsetBetween) <= 10E-5)
                {
                    if (point.X >= Math.Min(vertex1.X, vertex2.X) && point.X <= Math.Max(vertex1.X, vertex2.X))
                    {
                        isBetween = true;
                    }
                }
            }

            return isBetween;
        }

    }
}
