using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Utils;
using NetTopologySuite.Geometries;
using NetTopologySuite.Simplify;

namespace Mapgenix.Shapes
{
    /// <summary>Root of all area-based shapes, such as RectangleShape, EllipseShape, PolygonShape.</summary>
    [Serializable]
    public abstract class BaseAreaShape : BaseShape
    {
        /// <summary>Default constructor for BaseAreaShape.</summary>
        protected BaseAreaShape()
        {
        }

        /// <summary>Returns the perimeter of the shape.</summary>
        /// <returns>Perimeter of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">DistanceUnit for the return value.</param>
        public double GetPerimeter(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetPerimeterCore(shapeUnit, returningUnit);
        }

        /// <summary>Returns the perimeter of the shape.</summary>
        /// <returns>Perimeter of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">DistanceUnit for the return value.</param>
        protected virtual double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            string wkt = GetWellKnownText();
            BaseShape tmpBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);

            switch (tmpBaseShape.GetWellKnownType())
            {
                case WellKnownType.Polygon:
                    return ((PolygonShape)tmpBaseShape).GetPerimeter(shapeUnit, returningUnit);

                case WellKnownType.Multipolygon:
                    return ((MultipolygonShape)tmpBaseShape).GetPerimeter(shapeUnit, returningUnit);

                default:
                    return 0;
            }
        }

        /// <summary>Returns the area of the shape.</summary>
        /// <returns>Area of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">AreaUnit for the return value.</param>
        public double GetArea(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return GetAreaCore(shapeUnit, returningUnit);
        }

        /// <summary>Returns the area of the shape.</summary>
        /// <returns>Area of the shape.</returns>
        /// <param name="shapeUnit">GeographyUnit of the shape.</param>
        /// <param name="returningUnit">AreaUnit for the return value.</param>
        protected virtual double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            string wkt = GetWellKnownText();
            BaseShape tmpBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);

            switch (tmpBaseShape.GetWellKnownType())
            {
                case WellKnownType.Polygon:
                    return ((PolygonShape)tmpBaseShape).GetArea(shapeUnit, returningUnit);

                case WellKnownType.Multipolygon:
                    return ((MultipolygonShape)tmpBaseShape).GetArea(shapeUnit, returningUnit);

                default:
                    return 0;
            }
        }

        /// <summary>Returns a new area shape scaled up by a percentage.</summary>
        /// <returns>New area shape scaled up by a percentage.</returns>
        /// <param name="targetShape">Shape as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        public static BaseAreaShape ScaleUp(BaseAreaShape targetShape, double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            BaseAreaShape returnAreaBaseShape = (BaseAreaShape)targetShape.CloneDeepCore();
            returnAreaBaseShape.ScaleUp(percentage);

            return returnAreaBaseShape;
        }

        /// <summary>Returns a new feature scaled up by a percentage.</summary>
        /// <returns>New feature scaled up by a percentage.</returns>
        /// <param name="targetFeature">Feature as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to increase the feature shape's size.</param>
        public static Feature ScaleUp(Feature targetFeature, double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            BaseShape resultBaseShape = ScaleUp((BaseAreaShape)targetShape, percentage);

            return new Feature(resultBaseShape);
        }

        /// <summary>Increases the size of the area shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        public void ScaleUp(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);
            ScaleUpCore(percentage);
        }

        /// <summary>Increases the size of the area shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to increase the shape's size.</param>
        protected virtual void ScaleUpCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = (double)1 + (percentage / 100);
            Scale(factor);
        }

        /// <summary>Returns a new area shape scaled down by a percentage.</summary>
        /// <returns>New area shape scaled down by a percentage.</returns>
        /// <param name="targetShape">Area shape as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        public static BaseAreaShape ScaleDown(BaseAreaShape targetShape, double percentage)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            BaseAreaShape returnAreaBaseShape = (BaseAreaShape)targetShape.CloneDeepCore();
            returnAreaBaseShape.ScaleDown(percentage);

            return returnAreaBaseShape;
        }

        /// <summary>Returns a new feature scaled down by a percentage.</summary>
        /// <returns>New feature scaled down by a percentage.</returns>
        /// <param name="targetFeature">Feature as the base for scaling.</param>
        /// <param name="percentage">Percentage by which to decrease the feature shape's size.</param>
        public static Feature ScaleDown(Feature targetFeature, double percentage)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);

            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            BaseShape resultShape = ScaleDown((BaseAreaShape)targetShape, percentage);
            return new Feature(resultShape);
        }

        /// <summary>Decreases the size of the area shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        public void ScaleDown(double percentage)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(this);
            ScaleDownCore(percentage);
        }

        /// <summary>Decreases the size of the area shape by a percentage.</summary>
        /// <param name="percentage">Percentage by which to decrease the shape's size.</param>
        protected virtual void ScaleDownCore(double percentage)
        {
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = (double)1 - (percentage / 100);
            Scale(factor);
        }

        /// <summary>Returns the convex hull of the shape.</summary>
        /// <returns>Convex hull of the shape.</returns>
        public RingShape GetConvexHull()
        {
            Validators.CheckShapeIsValidForOperation(this);
            return GetConvexHullCore();
        }

        /// <summary>Returns the convex hull of the shape.</summary>
        /// <returns>Convex hull of the shape.</returns>
        protected virtual RingShape GetConvexHullCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            Geometry jtsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry convexHullResult = (Geometry)jtsGeometry.ConvexHull();
            BaseShape baseShape = ShapeConverter.JtsShapeToShape(convexHullResult);

            Validators.CheckIfTypeIsCorrect(baseShape, typeof(PolygonShape), "ConvexHull");

            return ((PolygonShape)baseShape).OuterRing;
        }

        /// <summary>Returns the intersection of the shape and a target shape.</summary>
        /// <returns>MultiPolygonShape as the intersection of the shape and a target shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Shape to find the intersection with.</param>
       public MultipolygonShape GetIntersection(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return GetIntersectionCore(targetShape);
        }

       /// <summary>Returns the intersection of the shape and a target feature.</summary>
       /// <returns>MultiPolygonShape as the intersection of the shape and a target feature.</returns>
       /// <remarks>None</remarks>
       /// <param name="targetFeature">Feature to find the intersection with.</param>
        public MultipolygonShape GetIntersection(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            return GetIntersectionCore((BaseAreaShape)targetShape);
        }

        /// <summary>Returns the intersection of the shape and a target shape.</summary>
        /// <returns>MultiPolygonShape as the intersection of the shape and a target shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Shape to find the intersection with.</param>
        protected virtual MultipolygonShape GetIntersectionCore(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry jtsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            Geometry resultGeometry = (Geometry)jtsGeometry.Intersection(targetGeometry);

            MultipolygonShape returnMultipolygon = null;
            if (resultGeometry is GeometryCollection)
            {
                GeometryCollection geometryCollection = resultGeometry as GeometryCollection;
                returnMultipolygon = new MultipolygonShape();

                foreach (Geometry geometry in geometryCollection.Geometries)
                {
                    BaseShape resultShape = ShapeConverter.JtsShapeToShape(geometry);

                    if (geometry is Polygon)
                    {
                        returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
                    }
                    else if (geometry is MultiPolygon)
                    {
                        MultipolygonShape multiPolygonShape = (MultipolygonShape)resultShape;
                        foreach (PolygonShape polygon in multiPolygonShape.Polygons)
                        {
                            returnMultipolygon.Polygons.Add(polygon);
                        }
                    }
                }

                if (returnMultipolygon.Polygons.Count == 0)
                {
                    returnMultipolygon = null;
                }
            }
            else
            {
                if (resultGeometry != null)
                {
                    BaseShape resultShape = ShapeConverter.JtsShapeToShape(resultGeometry);
                    if (resultGeometry is Polygon)
                    {
                        returnMultipolygon = new MultipolygonShape();
                        returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
                    }
                    else if (resultGeometry is MultiPolygon)
                    {
                        returnMultipolygon = (MultipolygonShape)resultShape;
                    }
                }
            }
            return returnMultipolygon;
        }

        /// <summary>Returns the union of the shape and a target shape.</summary>
        /// <returns>MultiPolygonShape as the union of the shape and a target shape.</returns>
        /// <param name="targetShape">Area shape to find the union with.</param>
        public virtual MultipolygonShape Union(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return UnionCore(targetShape);
        }

        /// <summary>Returns the union of the feature and a target feature.</summary>
        /// <returns>MultiPolygonShape as the union of the feature and a target feature.</returns>
        /// <param name="targetFeature">Feature to find the union with.</param>
        public MultipolygonShape Union(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            return UnionCore((BaseAreaShape)targetShape);
        }

        /// <summary>Returns the union of the shape and a target shape.</summary>
        /// <returns>MultiPolygonShape as the union of the shape and a target shape.</returns>
        /// <param name="targetShape">Area shape to find the union with.</param>
        protected virtual MultipolygonShape UnionCore(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return BaseAreaShape.Union(new BaseAreaShape[2] { this, targetShape });
        }

        /// <summary>Returns the union of the shape and target features.</summary>
        /// <returns>MultipolygonShape as the union of the shape and target features.</returns>
        /// <param name="targetFeatures">Target features to find the union with.</param>
        public static MultipolygonShape Union(IEnumerable<Feature> targetFeatures)
        {
            Validators.CheckParameterIsNotNull(targetFeatures, "targetFeatures");

            Collection<BaseAreaShape> areaBaseShapes = new Collection<BaseAreaShape>();
            foreach (Feature feature in targetFeatures)
            {
                BaseAreaShape areaBaseShape = (BaseAreaShape)feature.GetShape();
                areaBaseShapes.Add(areaBaseShape);
            }

            return Union(areaBaseShapes);
        }

        /// <summary>Returns the union of the shape and target area shapes.</summary>
        /// <returns>MultipolygonShape as the union of the shape and target area shapes.</returns>
        /// <param name="targetShapes">Target area shapes to find the union with.</param>
       public static MultipolygonShape Union(IEnumerable<BaseAreaShape> areaShapes)
        {
            Validators.CheckParameterIsNotNull(areaShapes, "areaShapes");

            int count = 0;
            foreach (BaseAreaShape shape in areaShapes)
            {
                count++;
            }
            Geometry[] geometriesArray = new Geometry[count];

            int i = 0;
            foreach (BaseAreaShape shape in areaShapes)
            {
                geometriesArray[i] = ShapeConverter.ShapeToJtsShape(shape);
                i++;
            }

            GeometryCollection geometries = new GeometryCollection(geometriesArray);
            Geometry unionGeometry = (Geometry)geometries.Buffer(0);
            BaseShape resultShape = ShapeConverter.JtsShapeToShape(unionGeometry);

            MultipolygonShape returnMultipolygon = null;
            if (resultShape.GetType().Name == "PolygonShape")
            {
                returnMultipolygon = new MultipolygonShape();
                returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
            }
            else if (resultShape.GetType().Name == "MultipolygonShape")
            {
                returnMultipolygon = (MultipolygonShape)resultShape;
            }
            return returnMultipolygon;
        }

       /// <summary>Returns the difference between the shape and another shape.</summary>
       /// <returns>MultiPolygonShape as the difference between the shape and another shape.</returns>
       /// <remarks>None</remarks>
       /// <param name="targetShape">Area shape to find the difference with.</param>
        public MultipolygonShape GetDifference(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return GetDifferenceCore(targetShape);
        }

        /// <summary>Returns the difference between the shape and a feature.</summary>
        /// <returns>MultiPolygonShape as the difference between the shape and a feature.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetFeature">Feature to find the difference with.</param>
         public MultipolygonShape GetDifference(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);

            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            return GetDifferenceCore((BaseAreaShape)targetShape);
        }

         /// <summary>Returns the difference between the shape and another shape.</summary>
         /// <returns>MultiPolygonShape as the difference between the shape and another shape.</returns>
         /// <remarks>None</remarks>
         /// <param name="targetShape">Area shape to find the difference with.</param>
        protected virtual MultipolygonShape GetDifferenceCore(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry ntsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            Geometry resultGeometry = (Geometry)ntsGeometry.Difference(targetGeometry);
            BaseShape resultShape = ShapeConverter.JtsShapeToShape(resultGeometry);

            MultipolygonShape returnMultipolygon = null;
            if (resultShape.GetType().Name == "PolygonShape")
            {
                returnMultipolygon = new MultipolygonShape();
                returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
            }
            else if (resultShape.GetType().Name == "MultipolygonShape")
            {
                returnMultipolygon = (MultipolygonShape)resultShape;
            }
            return returnMultipolygon;
        }

        /// <summary>Returns the symmetrical difference between the shape and another shape.</summary>
        /// <returns>MultiPolygonShape as the symmetrical difference between the shape and another shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Area shape to find the symmetrical difference with.</param>
        public MultipolygonShape GetSymmetricalDifference(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            return GetSymmetricalDifferenceCore(targetShape);
        }

        /// <summary>Returns the symmetricsl difference between the shape and a feature.</summary>
        /// <returns>MultiPolygonShape as the symmetrical difference between the shape and a feature.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetFeature">Feature to find the symmetricsl difference with.</param>
        public MultipolygonShape GetSymmetricalDifference(Feature targetFeature)
        {
            Validators.CheckShapeIsValidForOperation(this);
            BaseShape targetShape = targetFeature.GetShape();
            Validators.CheckShapeIsAreaBaseShape(targetShape);

            return GetSymmetricalDifferenceCore((BaseAreaShape)targetShape);
        }

        /// <summary>Returns the symmetrical difference between the shape and another shape.</summary>
        /// <returns>MultiPolygonShape as the symmetrical difference between the shape and another shape.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetShape">Area shape to find the symmetrical difference with.</param>
        protected virtual MultipolygonShape GetSymmetricalDifferenceCore(BaseAreaShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            Geometry ntsGeometry = ShapeConverter.ShapeToJtsShape(this);
            Geometry targetGeometry = ShapeConverter.ShapeToJtsShape(targetShape);

            Geometry resultGeometry = (Geometry)ntsGeometry.SymmetricDifference(targetGeometry);
            BaseShape resultShape = ShapeConverter.JtsShapeToShape(resultGeometry);

            MultipolygonShape returnMultipolygon = null;
            if (resultShape.GetType().Name == "PolygonShape")
            {
                returnMultipolygon = new MultipolygonShape();
                returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
            }
            else if (resultShape.GetType().Name == "MultipolygonShape")
            {
                returnMultipolygon = (MultipolygonShape)resultShape;
            }
            return returnMultipolygon;
        }

        /// <summary>Returns a collection of MultiPolygonShapes split.</summary>
        /// <returns>Collection of MultiPolygonShape split.</returns>
        /// <remarks>None.</remarks>
        /// <param name="areaToSplit">Area shape to be split.</param>
        /// <param name="areaToSplitBy">Area shape to perform the split.</param>
        public static Collection<MultipolygonShape> Split(BaseAreaShape areaToSplit, BaseAreaShape areaToSplitBy)
        {
            Validators.CheckParameterIsNotNull(areaToSplit, "areaToSplit");
            Validators.CheckParameterIsNotNull(areaToSplitBy, "areaToSplitBy");

            Validators.CheckParameterIsValid(areaToSplit, "areaToSplit");
            Validators.CheckParameterIsValid(areaToSplitBy, "areaToSplitBy");

            Collection<MultipolygonShape> result = new Collection<MultipolygonShape>();

            result.Add(areaToSplit.GetIntersection(areaToSplitBy));
            result.Add(areaToSplit.GetDifference(areaToSplitBy));

            return result;
        }

        /// <summary>Returns a collection of features split.</summary>
        /// <returns>Collection of features split.</returns>
        /// <remarks>None.</remarks>
        /// <param name="areaToSplit">Area shape to be split.</param>
        /// <param name="areaToSplitBy">Area shape to perform the split.</param>
        public static Collection<Feature> Split(Feature areaToSplit, Feature areaToSplitBy)
        {
            BaseShape areaToSplitBaseShape = areaToSplit.GetShape();
            BaseShape areaToSplitByBaseShape = areaToSplitBy.GetShape();

            Validators.CheckShapeIsAreaBaseShape(areaToSplitBaseShape);
            Validators.CheckShapeIsAreaBaseShape(areaToSplitByBaseShape);

            Collection<Feature> result = new Collection<Feature>();

            BaseAreaShape areaToSplitAreaBaseShape = (BaseAreaShape)areaToSplitBaseShape;
            BaseAreaShape areaToSplitByAreaBaseShape = (BaseAreaShape)areaToSplitByBaseShape;

            result.Add(new Feature(areaToSplitAreaBaseShape.GetIntersection(areaToSplitByAreaBaseShape)));
            result.Add(new Feature(areaToSplitAreaBaseShape.GetDifference(areaToSplitByAreaBaseShape)));

            return result;
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="targetShape">Area shape to be simplified.</param>
        /// <param name="targetShapeUnit">Geography unit of the shape to perform the operation on.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="toleranceUnit">Distance unit of the tolerance.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multipolygonshape. </returns>
        public static MultipolygonShape Simplify(BaseAreaShape targetShape, GeographyUnit targetShapeUnit, double tolerance, DistanceUnit toleranceUnit, SimplificationType simplificationType)
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
        /// <param name="targetShape">Area shape to be simplified.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multipolygonshape. </returns>
        public static MultipolygonShape Simplify(BaseAreaShape targetShape, double tolerance, SimplificationType simplificationType)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
    
            BaseAreaShape clonedShape = (BaseAreaShape)targetShape.CloneDeep();
            return clonedShape.Simplify(tolerance, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="shapeUnit">Geography unit of the shape.</param>
        /// <param name="tolerance">Tolerance distance for the simplification.</param>
        /// <param name="toleranceUnit">Distance unit of the tolerance.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multipolygonshape. </returns>
        public MultipolygonShape Simplify(GeographyUnit shapeUnit, double tolerance, DistanceUnit toleranceUnit, SimplificationType simplificationType)
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
        /// <param name="tolerance">Tolerance diatance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multipolygonshape. </returns>
        public MultipolygonShape Simplify(double tolerance, SimplificationType simplificationType)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(tolerance, "tolerance", 0, RangeCheckingInclusion.ExcludeValue);
    
            return SimplifyCore(tolerance, simplificationType);
        }

        /// <summary>Performs a simplification operation.</summary>
        /// <param name="tolerance">Tolerance diatance for the simplification.</param>
        /// <param name="simplificationType">Simplification type for the operation.</param>
        /// <returns>Simplified Multipolygonshape. </returns>
        protected virtual MultipolygonShape SimplifyCore(double tolerance, SimplificationType simplificationType)
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

            MultipolygonShape returnMultipolygon = null;
            if (simplifiedGeometry is Polygon || simplifiedGeometry is MultiPolygon)
            {
                BaseAreaShape simplifiedShape = (BaseAreaShape)ShapeConverter.JtsShapeToShape(simplifiedGeometry);

                if (simplifiedShape.GetType().Name == "PolygonShape")
                {
                    returnMultipolygon = new MultipolygonShape();
                    returnMultipolygon.Polygons.Add((PolygonShape)simplifiedShape);
                }
                else if (simplifiedShape.GetType().Name == "MultipolygonShape")
                {
                    returnMultipolygon = (MultipolygonShape)simplifiedShape;
                }
            }
            else if (simplifiedGeometry is GeometryCollection)
            {
                GeometryCollection geometryCollection = simplifiedGeometry as GeometryCollection;
                returnMultipolygon = new MultipolygonShape();

                foreach (Geometry geometry in geometryCollection.Geometries)
                {
                    BaseShape resultShape = ShapeConverter.JtsShapeToShape(geometry);

                    if (geometry is Polygon)
                    {
                        returnMultipolygon.Polygons.Add((PolygonShape)resultShape);
                    }
                    else if (geometry is MultiPolygon)
                    {
                        MultipolygonShape multiPolygonShape = (MultipolygonShape)resultShape;
                        foreach (PolygonShape polygon in multiPolygonShape.Polygons)
                        {
                            returnMultipolygon.Polygons.Add(polygon);
                        }
                    }
                }

                if (returnMultipolygon.Polygons.Count == 0)
                {
                    returnMultipolygon = null;
                }
            }

            return returnMultipolygon;
        }

        private void Scale(double factor)
        {
            string wkt = GetWellKnownText();
            BaseShape standandWktBaseShape = BaseShape.CreateShapeFromWellKnownData(wkt);

            switch (standandWktBaseShape.GetWellKnownType())
            {
                case WellKnownType.Polygon:
                    ((PolygonShape)standandWktBaseShape).Scale(factor);
                    break;

                case WellKnownType.Multipolygon:
                    ((MultipolygonShape)standandWktBaseShape).Scale(factor);
                    break;

                default:
                    break;
            }
            string scaleWkt = standandWktBaseShape.GetWellKnownText();
            this.LoadFromWellKnownData(scaleWkt);
        }
    }
}
