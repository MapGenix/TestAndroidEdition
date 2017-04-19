using System;
using System.Collections.Generic;
using System.Drawing;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    /// <summary>Static class exposing methods dealing with map extents.</summary>
    [Serializable]
    public static class ExtentHelper
    {
        private const float StandardDpi = 96f;
        private const double InchToMeter = 0.0254;
        private const double InchPerDecimalDegree = 4374754;

        /// <summary>Returns an adjusted extent based on the ratio screen width / screen height.</summary>
        /// <returns>Adjusted extent based on the ratio screen width / screen height.</returns>
        /// <param name="worldExtent">Map extent to adjust.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape GetDrawingExtent(RectangleShape worldExtent, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            double screenRatio = screenHeight/screenWidth;
            var extentRatio = worldExtent.Height/worldExtent.Width;

            double newWidth;
            double newHeight;

            if (extentRatio > screenRatio)
            {
                newHeight = worldExtent.Height;
                newWidth = newHeight/screenRatio;
            }
            else
            {
                newWidth = worldExtent.Width;
                newHeight = newWidth*screenRatio;
            }

            var centerPoint = worldExtent.GetCenterPoint();
            var upperLeftPoint = new PointShape(centerPoint.X - newWidth*0.5, centerPoint.Y + newHeight*0.5);
            var lowerRightPoint = new PointShape(centerPoint.X + newWidth*0.5, centerPoint.Y - newHeight*0.5);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }

        /// <summary>Centers the rectangle based on a center point adjusting the rectangle's width / height ratio based on the map width / height ratio.</summary>
        /// <returns>Adjusted extent centered on a point.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Current map extent to center.</param>
        /// <param name="worldPoint">World point to center the map on.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape CenterAt(RectangleShape worldExtent, PointShape worldPoint, float screenWidth,
            float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var extent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);
            var centerPoint = extent.GetCenterPoint();

            var newExtent =
                (RectangleShape) extent.Register(centerPoint, worldPoint, DistanceUnit.Meter, GeographyUnit.Meter);

            return newExtent;
        }

        /// <summary>Centers the rectangle based on a center point of a feature adjusting the rectangle's width / height ratio based on the map width / height ratio.</summary>
        /// <returns>Adjusted extent centered on a point.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Current map extent to center.</param>
        /// <param name="centerFeature">Feature to center the map on.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape CenterAt(RectangleShape worldExtent, Feature centerFeature, float screenWidth,
            float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);

            var worldPoint = centerFeature.GetShape().GetCenterPoint();
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return CenterAt(worldExtent, worldPoint, screenWidth, screenHeight);
        }

        /// <summary>Returns an adjusted map extent centered on a point in screen coordinates.</summary>
        /// <returns>Adjusted extent centered on a point in screen coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Current map extent to center.</param>
        /// <param name="screenX">X in screen coordinates to center on.</param>
        /// <param name="screenY">Y in screen coordinates to center on.</param>
        /// <param name="screenWidth">Width in screen coordinates.</param>
        /// <param name="screenHeight">Height in screen coordinates.</param>
        public static RectangleShape CenterAt(RectangleShape worldExtent, float screenX, float screenY,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var extent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);
            var worldPoint = ToWorldCoordinate(extent, new ScreenPointF(screenX, screenY), screenWidth, screenHeight);

            var centerPoint = extent.GetCenterPoint();

            var newExtent =
                (RectangleShape) extent.Register(centerPoint, worldPoint, DistanceUnit.Meter, GeographyUnit.Meter);

            return newExtent;
        }


        /// <summary>Returns the distance in screen coordinates between two world points.</summary>
        /// <returns>Distance in screen coordinates between two world points.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="worldPoint1">First point in world coordinate to meassure from.</param>
        /// <param name="worldPoint2">Second point in world coordinate to measure to.</param>
        /// <param name="screenWidth">Width in screen coordinates.</param>
        /// <param name="screenHeight">Height in screen coordinates.</param>
        public static float GetScreenDistanceBetweenTwoWorldPoints(RectangleShape worldExtent, PointShape worldPoint1,
            PointShape worldPoint2, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckParameterIsNotNull(worldPoint1, "worldPoint1");
            Validators.CheckParameterIsNotNull(worldPoint2, "worldPoint2");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var extent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);

            var widthFactor = screenWidth/extent.Width;
            var heightFactor = screenHeight/extent.Height;
            var upperLeftX = extent.UpperLeftPoint.X;
            var upperLeftY = extent.UpperLeftPoint.Y;

            var point01X = (worldPoint1.X - upperLeftX)*widthFactor;
            var point01Y = (upperLeftY - worldPoint1.Y)*heightFactor;
            var point02X = (worldPoint2.X - upperLeftX)*widthFactor;
            var point02Y = (upperLeftY - worldPoint2.Y)*heightFactor;
            var differentX = point02X - point01X;
            var differentY = point02Y - point01Y;

            var distance = (float) Math.Sqrt(differentX*differentX + differentY*differentY);

            return distance;
        }

        /// <summary>Returns the distance in screen coordinates between two features.</summary>
        /// <returns>Distance in screen coordinates between two features.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="worldPointFeature1">First feature to meassure from.</param>
        /// <param name="worldPointFeature2">Second feature to measure to.</param>
        /// <param name="screenWidth">Width in screen coordinates.</param>
        /// <param name="screenHeight">Height in screen coordinates.</param>
        public static float GetScreenDistanceBetweenTwoWorldPoints(RectangleShape worldExtent,
            Feature worldPointFeature1, Feature worldPointFeature2, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var baseShape1 = worldPointFeature1.GetShape();
            var baseShape2 = worldPointFeature2.GetShape();
            Validators.CheckShapeIsPointShape(baseShape1);
            Validators.CheckShapeIsPointShape(baseShape2);

            return GetScreenDistanceBetweenTwoWorldPoints(worldExtent, (PointShape) baseShape1, (PointShape) baseShape2,
                screenWidth, screenHeight);
        }

        /// <summary>Returns the distance in world units between two screen points on the map.</summary>
        /// <returns>Distance in world units between two screen points on the map.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenPoint1">First screen point to measure from.</param>
        /// <param name="screenPoint2">Second screen point to measure to.</param>
        /// <param name="screenWidth">Width of the screen.</param>
        /// <param name="screenHeight">Height of the screen.</param>
        /// <param name="worldExtentUnit">Geographic unit of the world extent is in.</param>
        /// <param name="distanceUnit">Distance unit to get the result distance in.</param>
        public static double GetWorldDistanceBetweenTwoScreenPoints(RectangleShape worldExtent,
            ScreenPointF screenPoint1, ScreenPointF screenPoint2, float screenWidth, float screenHeight,
            GeographyUnit worldExtentUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "rectangleUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            var extent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);

            var widthFactor = extent.Width/screenWidth;
            var heightFactor = extent.Height/screenHeight;
            var upperLeftX = extent.UpperLeftPoint.X;
            var upperLeftY = extent.UpperLeftPoint.Y;

            var point01X = upperLeftX + screenPoint1.X*widthFactor;
            var point01Y = upperLeftY - screenPoint1.Y*heightFactor;
            var point02X = upperLeftX + screenPoint2.X*widthFactor;
            var point02Y = upperLeftY - screenPoint2.Y*heightFactor;

            double distance;

            if (worldExtentUnit == GeographyUnit.DecimalDegree)
            {
                distance = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(point01X, point01Y, point02X, point02Y,
                    distanceUnit);
            }
            else
            {
                var differentX = point02X - point01X;
                var differentY = point02Y - point01Y;
                distance = Math.Sqrt(differentX*differentX + differentY*differentY);
                var fromUnit = Conversion.ConvertGeographyUnitToDistanceUnit(worldExtentUnit);
                distance = Conversion.ConvertMeasureUnits(distance, fromUnit, distanceUnit);
            }

            return distance;
        }

        /// <summary>Returns the distance in world units between two screen points.</summary>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenPoint1X">X of the screen point to measure from.</param>
        /// <param name="screenPoint1Y">Y of the screen point to measure from.</param>
        /// <param name="screenPoint2X">X of the screen point to measure to.</param>
        /// <param name="screenPoint2Y">Y of the screen point to measure to.</param>
        /// <param name="screenWidth">Width of the screen.</param>
        /// <param name="screenHeight">Height of the screen.</param>
        /// <param name="worldExtentUnit">Geography unit of the world extent you passed in.</param>
        /// <param name="distanceUnit">Distance unit for the result distance.</param>
        public static double GetWorldDistanceBetweenTwoScreenPoints(RectangleShape worldExtent, float screenPoint1X,
            float screenPoint1Y, float screenPoint2X, float screenPoint2Y, float screenWidth, float screenHeight,
            GeographyUnit worldExtentUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "mapUnit");
            Validators.CheckDistanceUnitIsValid(distanceUnit, "distanceUnit");

            var screenPoint1 = new ScreenPointF(screenPoint1X, screenPoint1Y);
            var screenPoint2 = new ScreenPointF(screenPoint2X, screenPoint2Y);

            return GetWorldDistanceBetweenTwoScreenPoints(worldExtent, screenPoint1, screenPoint2, screenWidth,
                screenHeight, worldExtentUnit, distanceUnit);
        }

        /// <summary>Returns the scale of a map extent.</summary>
        /// <returns>Scale of a map extent.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="worldExtentUnit">Unit of the map extent.</param>
        public static double GetScale(RectangleShape worldExtent, float screenWidth, GeographyUnit worldExtentUnit)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");

            return GetScale(worldExtent, screenWidth, worldExtentUnit, StandardDpi);
        }

        /// <summary>Returns the current scale.</summary>
        /// <returns>Current scale.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="worldExtentUnit">Unit of the map extent.</param>
        /// <param name="dpi">Dpi (Dot per inch).</param>
        public static double GetScale(RectangleShape worldExtent, float screenWidth, GeographyUnit worldExtentUnit,
            float dpi)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(dpi, "dpi", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");

            double scale;
            if (worldExtentUnit == GeographyUnit.DecimalDegree)
            {
                scale = worldExtent.Width*InchPerDecimalDegree*dpi/screenWidth;
            }
            else
            {
                var distanceUnit = Conversion.ConvertGeographyUnitToDistanceUnit(worldExtentUnit);
                var distance = Conversion.ConvertMeasureUnits(worldExtent.Width, distanceUnit, DistanceUnit.Meter);
                var screenWidthInMeter = screenWidth*InchToMeter/dpi;
                scale = distance/screenWidthInMeter;
            }

            return scale;
        }


        /// <summary>Gets the BoundingBox of a group of BaseShapes.</summary>
        /// <param name="shapes">The BaseShapes to get the BoundingBox for.</param>
        /// <returns>BoundingBox containing the shapes passed in.</returns>
        public static RectangleShape GetBoundingBoxOfItems(IEnumerable<BaseShape> shapes)
        {
            Validators.CheckParameterIsNotNull(shapes, "shapes");

            RectangleShape boundingBox = null;

            foreach (var shape in shapes)
            {
                if (boundingBox == null)
                {
                    boundingBox = shape.GetBoundingBox();
                }
                else
                {
                    boundingBox.ExpandToInclude(shape.GetBoundingBox());
                }
            }

            return boundingBox;
        }

        /// <summary>Gets the BoundingBox of a group of features.</summary>
        /// <param name="shapes">The features to get the BoundingBox for.</param>
        /// <returns>BoundingBox containing the features passed in.</returns>
        public static RectangleShape GetBoundingBoxOfItems(IEnumerable<Feature> features)
        {
            Validators.CheckParameterIsNotNull(features, "features");

            RectangleShape boundingBox = null;

            foreach (var feature in features)
            {
                if (boundingBox == null)
                {
                    boundingBox = feature.GetBoundingBox();
                }
                else
                {
                    boundingBox.ExpandToInclude(feature.GetBoundingBox());
                }
            }

            return boundingBox;
        }

        /// <summary>Returns an extent zoomed in by the percentage passed in.</summary>
        /// <returns>extent zoomed in by the percentage passed in.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent to zoom to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        public static RectangleShape ZoomIn(RectangleShape worldExtent, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);

            var newExtent = new RectangleShape(worldExtent.UpperLeftPoint, worldExtent.LowerRightPoint);
            newExtent.ScaleDown(percentage);

            return newExtent;
        }

        /// <summary>Returns an extent centered and zoomed in.</summary>
        /// <returns>Extent centered and zoomed in.</returns>
        /// <param name="worldExtent">Map extent to centered and zoom to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        /// <param name="worldPoint">Point in world coordinate to center the extent on.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, PointShape worldPoint,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var extent = CenterAt(worldExtent, worldPoint, screenWidth, screenHeight);
            extent.ScaleDown(percentage);

            return extent;
        }

        /// <summary>Returns an extent centered and zoomed in.</summary>
        /// <returns>Extent centered and zoomed in.</returns>
        /// <param name="worldExtent">Map extent to centered and zoom to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        /// <param name="centerFeature">Feature to center the extent on.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, Feature centerFeature,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);

            var worldPoint = centerFeature.GetShape().GetCenterPoint();
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return ZoomIntoCenter(worldExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        /// <summary>Returns an extent centered and zoomed in.</summary>
        /// <returns>Extent centered and zoomed in.</returns>
        /// <param name="worldExtent">Map extent to center and zoom to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        /// <param name="screenX">X in screen coordinate to center on.</param>
        /// <param name="screenY">Y in screen coordinate to center on.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomIntoCenter(RectangleShape worldExtent, int percentage, float screenX,
            float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var widthFactor = worldExtent.Width/screenWidth;
            var heightFactor = worldExtent.Height/screenHeight;

            var pointX = worldExtent.UpperLeftPoint.X + screenX*widthFactor;
            var pointY = worldExtent.UpperLeftPoint.Y - screenY*heightFactor;

            return ZoomIntoCenter(worldExtent, percentage, new PointShape(pointX, pointY), screenWidth, screenHeight);
        }

        /// <summary>Returns extent zoomed out by a percentage.</summary>
        /// <returns>Extent zoomed out by a percentage.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent to zoom out to.</param>
        /// <param name="percentage">Percentage by which to zoom out.</param>
        public static RectangleShape ZoomOut(RectangleShape worldExtent, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            var newExtent = new RectangleShape(worldExtent.UpperLeftPoint, worldExtent.LowerRightPoint);
            newExtent.ScaleUp(percentage);

            return newExtent;
        }

        /// <summary>Returns an extent centered and zoomed out.</summary>
        /// <returns>Extent centered and zoomed out.</returns>
        /// <param name="worldExtent">Map extent to center and zoom out to.</param>
        /// <param name="percentage">Percentage by which to zoom out.</param>
        /// <param name="worldPoint">Point in world coordinates to center the extent on.</param>
        /// <param name="screenWidth">Map width in screen coordinates.</param>
        /// <param name="screenHeight">Map height in screen coordinates.</param>
        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, PointShape worldPoint,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var extent = CenterAt(worldExtent, worldPoint, screenWidth, screenHeight);
            extent.ScaleUp(percentage);

            return extent;
        }

        /// <summary>Returns map extent centered and zoomed out.</summary>
        /// <returns>map extent centered and zoomed out.</returns>
        /// <param name="worldExtent">Map extent to center and zoom out to.</param>
        /// <param name="percentage">Percentage by which to zoom out.</param>
        /// <param name="centerFeature">Feature to center the extent on.</param>
        /// <param name="screenWidth">Map width in screen coordinates.</param>
        /// <param name="screenHeight">Map height in screen coordinates.</param>
        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, Feature centerFeature,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            var worldPoint = centerFeature.GetShape().GetCenterPoint();
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return ZoomOutToCenter(worldExtent, percentage, worldPoint, screenWidth, screenHeight);
        }

        /// <summary>Returns a map extent centered and zoomed out.</summary>
        /// <returns>Map extent centered and zoomed out.</returns>
        /// <param name="worldExtent">Map extent to center and zoom out to.</param>
        /// <param name="percentage">Percentage by which to zoom out.</param>
        /// <param name="screenX">X in screen coordinate to center on.</param>
        /// <param name="screenY">Y in screen coordinate to center on.</param>
        /// <param name="screenWidth">Map width in screen coordinates.</param>
        /// <param name="screenHeight">Map height in screen coordinates.</param>
        public static RectangleShape ZoomOutToCenter(RectangleShape worldExtent, int percentage, float screenX,
            float screenY, float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var widthFactor = worldExtent.Width/screenWidth;
            var heightFactor = worldExtent.Height/screenHeight;

            var pointX = worldExtent.UpperLeftPoint.X + screenX*widthFactor;
            var pointY = worldExtent.UpperLeftPoint.Y - screenY*heightFactor;

            return ZoomOutToCenter(worldExtent, percentage, new PointShape(pointX, pointY), screenWidth, screenHeight);
        }

        /// <summary>Returns a panned extent.</summary>
        /// <overloads>Passes in a direction and a percentage by which to pan.</overloads>
        /// <returns>Panned extent.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent to pan.</param>
        /// <param name="direction">Direction in which to pan.</param>
        /// <param name="percentage">Percentage by which to pan.</param>
        public static RectangleShape Pan(RectangleShape worldExtent, PanDirection direction, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckParameterIsNotNull(direction, "direction");
            Validators.CheckPanDirectionIsValid(direction, "direction");
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            var newExtent = new RectangleShape(worldExtent.UpperLeftPoint, worldExtent.LowerRightPoint);
            PanExtent(newExtent, direction, percentage);

            return newExtent;
        }

        /// <summary>Returns a panned extent.</summary>
        /// <overloads>Passes in an angle and a percentage by which to pan.</overloads>
        /// <returns>Panned extent.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent to pan.</param>
        /// <param name="degree">Degree by which to pan.</param>
        /// <param name="percentage">Percentage by which to pan.</param>
        public static RectangleShape Pan(RectangleShape worldExtent, float degree, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsInRange(degree, "degree", 0, RangeCheckingInclusion.IncludeValue, 360,
                RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);

            double distance;

            if (degree > 305 || degree < 45 || (degree > 135 && degree < 225))
            {
                distance = worldExtent.Width*percentage*0.01;
            }
            else
            {
                distance = worldExtent.Height*percentage*0.01;
            }

            var newExtent = (RectangleShape) worldExtent.CloneDeep();
            newExtent.TranslateByDegree(distance, degree, GeographyUnit.Meter, DistanceUnit.Meter);

            return newExtent;
        }

        /// <summary>Returns screen coordinates from world coordinates.</summary>
        /// <returns>Screen coordinates from world coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent in world coordinates.</param>
        /// <param name="worldX">X in world coordinates to convert to screen points.</param>
        /// <param name="worldY">Y in world coordinates to convert to screen points.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, double worldX, double worldY,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            var widthFactor = screenWidth/worldExtent.Width;
            var heighFactor = screenHeight/worldExtent.Height;

            var pointX = (worldX - worldExtent.UpperLeftPoint.X)*widthFactor;
            var pointY = (worldExtent.UpperLeftPoint.Y - worldY)*heighFactor;

            return new ScreenPointF((float) pointX, (float) pointY);
        }

        /// <summary>Returns rectangle in screen coordinates from rectangle in world coordinates.</summary>
        /// <returns>Rectangle in screen coordinates from rectangle in world coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent in world coordinates.</param>
        /// <param name="targetExtent">Rectangle in world coordinates to convert to screen coordinates.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static Rectangle ToScreenCoordinate(RectangleShape worldExtent, RectangleShape targetExtent,
            float currentExtentWidth, float currentExtentHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(targetExtent, "targetExtent");

            var upperLeftX =
                (int)
                    Math.Round((targetExtent.UpperLeftPoint.X - worldExtent.UpperLeftPoint.X)/worldExtent.Width*
                               currentExtentWidth);
            var upperLeftY =
                (int)
                    Math.Round((worldExtent.UpperLeftPoint.Y - targetExtent.UpperLeftPoint.Y)/worldExtent.Height*
                               currentExtentHeight);

            var width = (int) Math.Round(targetExtent.Width/worldExtent.Width*currentExtentWidth);
            var height = (int) Math.Round(targetExtent.Height/worldExtent.Height*currentExtentHeight);

            return new Rectangle(upperLeftX, upperLeftY, width, height);
        }


        /// <summary>Returns screen coordinates from world coordinates.</summary>
        /// <returns>Screen coordinates from world coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent in world coordinates.</param>
        /// <param name="worldPoint">Point in world coordinates to convert to screen coordinates.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, PointShape worldPoint,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckParameterIsNotNull(worldPoint, "worldPoint");
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return ToScreenCoordinate(worldExtent, worldPoint.X, worldPoint.Y, screenWidth, screenHeight);
        }

        /// <summary>Returns the center of a feature in screen coordinates from world coordinates.</summary>
        /// <returns>Center of a feature in screen coordinates from world coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="worldPointFeature">Feature which center to convert to a screen point.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static ScreenPointF ToScreenCoordinate(RectangleShape worldExtent, Feature worldPointFeature,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);

            var baseShape = worldPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(baseShape);

            var worldPoint = (PointShape) baseShape;
            Validators.CheckShapeIsValidForOperation(worldPoint);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return ToScreenCoordinate(worldExtent, worldPoint, screenWidth, screenHeight);
        }

        /// <summary>Returns point in world coordinates from screen coordinates.</summary>
        /// <returns>Point in world coordinates from screen coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenX">X in screen coordinates of the point to convert to world coordinates.</param>
        /// <param name="screenY">Y in screen coordinates of the point to convert to world coordinates.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static PointShape ToWorldCoordinate(RectangleShape worldExtent, float screenX, float screenY,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            var widthFactor = worldExtent.Width/screenWidth;
            var heightFactor = worldExtent.Height/screenHeight;

            var pointX = worldExtent.UpperLeftPoint.X + screenX*widthFactor;
            var pointY = worldExtent.UpperLeftPoint.Y - screenY*heightFactor;

            return new PointShape(pointX, pointY);
        }

        /// <summary>Returns point in world coordinates from screen coordinates.</summary>
        /// <returns>Point in world coordinates from screen coordinates.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent.</param>
        /// <param name="screenPoint">Point in screen coordinates to convert to a world point.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static PointShape ToWorldCoordinate(RectangleShape worldExtent, ScreenPointF screenPoint,
            float screenWidth, float screenHeight)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckShapeIsValidForOperation(worldExtent);
            Validators.CheckIfInputValueIsBiggerThan(screenWidth, "screenWidth", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckIfInputValueIsBiggerThan(screenHeight, "screenHeight", 0,
                RangeCheckingInclusion.ExcludeValue);

            return ToWorldCoordinate(worldExtent, screenPoint.X, screenPoint.Y, screenWidth, screenHeight);
        }

        /// <summary>Returns an extent snapped to a zoom level in the zoom level set passed in.</summary>
        /// <returns>Extent snapped to a zoom level in the zoom level set passed in.</returns>
        /// <remarks>None</remarks>
        /// <param name="worldExtent">Map extent to snap.</param>
        /// <param name="worldExtentUnit">Geographic unit of the map extent.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        /// <param name="zoomLevelSet">Set of zoom levels to snap to.</param>
        public static RectangleShape SnapToZoomLevel(RectangleShape worldExtent, GeographyUnit worldExtentUnit,
            float screenWidth, float screenHeight, ZoomLevelSet zoomLevelSet)
        {
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckParameterIsNotNull(zoomLevelSet, "zoomLevelSet");

            worldExtent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);

            var scale = GetScale(worldExtent, screenWidth, worldExtentUnit);

            var nearestZoomLevel = zoomLevelSet.GetSnapedZoomLevel(scale);

            var newWidth = worldExtent.Width * nearestZoomLevel.Scale/scale;
            var newHeight = newWidth*screenHeight/screenWidth;

            var centerPoint = worldExtent.GetCenterPoint();
            var newUpperLeftPoint = new PointShape(centerPoint.X - newWidth*0.5, centerPoint.Y + newHeight*0.5);
            var newLowerRightPoint = new PointShape(centerPoint.X + newWidth*0.5, centerPoint.Y - newHeight*0.5);

            return new RectangleShape(newUpperLeftPoint, newLowerRightPoint);
        }

        /// <summary>Returns a extent zoomed based on a scale.</summary>
        /// <returns>Extent zoomed based on a scale.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetScale">Scale to zoom into.</param>
        /// <param name="worldExtent">Map extent to zoom into.</param>
        /// <param name="worldExtentUnit">Geography unit of the world extent.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomToScale(double targetScale, RectangleShape worldExtent,
            GeographyUnit worldExtentUnit, float screenWidth, float screenHeight)
        {
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");

            worldExtent = GetDrawingExtent(worldExtent, screenWidth, screenHeight);
            var sourceScale = GetScale(worldExtent, screenWidth, worldExtentUnit);

            var newWidth = worldExtent.Width*targetScale/sourceScale;
            var newHeight = newWidth*screenHeight/screenWidth;

            var centerPoint = worldExtent.GetCenterPoint();
            var newUpperLeftPoint = new PointShape(centerPoint.X - newWidth*0.5, centerPoint.Y + newHeight*0.5);
            var newLowerRightPoint = new PointShape(centerPoint.X + newWidth*0.5, centerPoint.Y - newHeight*0.5);

            return new RectangleShape(newUpperLeftPoint, newLowerRightPoint);
        }

        /// <summary>Returns a extent that has been zoomed into based on a scale.</summary>
        /// <returns>Extent zoomed based on a scale.</returns>
        /// <remarks>None</remarks>
        /// <param name="targetScale">Scale to zoom into.</param>
        /// <param name="worldExtent">World extent to zoom into.</param>
        /// <param name="worldExtentUnit">Geography unit of the map extent.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        /// <param name="offsetScreenPoint">Offset Point in screen coordinates.</param>
        public static RectangleShape ZoomToScale(double targetScale, RectangleShape worldExtent,
            GeographyUnit worldExtentUnit, float screenWidth, float screenHeight, ScreenPointF offsetScreenPoint)
        {
            Validators.CheckGeographyUnitIsValid(worldExtentUnit, "worldExtentUnit");
            Validators.CheckMapEngineExtentIsValid(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(offsetScreenPoint, "offsetScreenPoint");

            var worldPointBefore = ToWorldCoordinate(worldExtent, offsetScreenPoint, screenWidth, screenHeight);
            var resultExtent = ZoomToScale(targetScale, worldExtent, worldExtentUnit, screenWidth, screenHeight);
            var worldPointAfter = ToWorldCoordinate(resultExtent, offsetScreenPoint, screenWidth, screenHeight);

            resultExtent.TranslateByOffset(worldPointBefore.X - worldPointAfter.X,
                worldPointBefore.Y - worldPointAfter.Y, GeographyUnit.Meter, DistanceUnit.Meter);

            return resultExtent;
        }

        /// <summary>Returns a map extent zoomed in and keeping offsetScreenPoint constant.</summary>
        /// <returns>Map extent zoomed in and keeping offsetScreenPoint constant.</returns>
        /// <param name="worldExtent">Map extent to zoom in to and center to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        /// <param name="offsetScreenPoint">Point in screen coordinates to keep constant.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomIntoOffsetPoint(RectangleShape worldExtent, float screenWidth,
            float screenHeight, ScreenPointF offsetScreenPoint, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(offsetScreenPoint, "offsetScreenPoint");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(worldExtent);

            var worldPointBefore = ToWorldCoordinate(worldExtent, offsetScreenPoint, screenWidth, screenHeight);
            var resultExtent = ZoomIn(worldExtent, percentage);
            var worldPointAfter = ToWorldCoordinate(resultExtent, offsetScreenPoint, screenWidth, screenHeight);

            resultExtent.TranslateByOffset(worldPointBefore.X - worldPointAfter.X,
                worldPointBefore.Y - worldPointAfter.Y, GeographyUnit.Meter, DistanceUnit.Meter);

            return resultExtent;
        }

        /// <summary>Returns a map extent zoomed out and keeping offsetScreenPoint constant.</summary>
        /// <returns>Map extent zoomed out and keeping offsetScreenPoint constant.</returns>
        /// <param name="worldExtent">Map extent to zoom out to and center to.</param>
        /// <param name="percentage">Percentage by which to zoom in.</param>
        /// <param name="offsetScreenPoint">Point in screen coordinates to keep constant.</param>
        /// <param name="screenWidth">Width of the map in screen coordinates.</param>
        /// <param name="screenHeight">Height of the map in screen coordinates.</param>
        public static RectangleShape ZoomOutToOffsetPoint(RectangleShape worldExtent, float screenWidth,
            float screenHeight, ScreenPointF offsetScreenPoint, int percentage)
        {
            Validators.CheckParameterIsNotNull(worldExtent, "worldExtent");
            Validators.CheckParameterIsNotNull(offsetScreenPoint, "offsetScreenPoint");
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100,
                RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(worldExtent);

            var worldPointBefore = ToWorldCoordinate(worldExtent, offsetScreenPoint, screenWidth, screenHeight);
            var resultExtent = ZoomOut(worldExtent, percentage);
            var worldPointAfter = ToWorldCoordinate(resultExtent, offsetScreenPoint, screenWidth, screenHeight);

            resultExtent.TranslateByOffset(worldPointBefore.X - worldPointAfter.X,
                worldPointBefore.Y - worldPointAfter.Y, GeographyUnit.Meter, DistanceUnit.Meter);

            return resultExtent;
        }

        /// <summary>Returns a resized RectangleShape.</summary>
        /// <param name="sourceExtent">Rectangle to resize.</param>
        /// <param name="sourceScreenWidth">Width of the map in screen coordinate</param>
        /// <param name="targetScreenWidth">The width of the target map in screen coordinate</param>
        /// <param name="targetScreenHeight">The height of the target map in screen coordinate</param>
        /// <param name="mapUnit">Geography unit of the map</param>
        /// <param name="resizeType">Resize type.</param>
        /// <returns>Resized rectangle.</returns>
        public static RectangleShape ResizeExtent(RectangleShape sourceExtent, float sourceScreenWidth,
            float targetScreenWidth, float targetScreenHeight, GeographyUnit mapUnit, MapResizeMode resizeType)
        {
            RectangleShape resizedExtent = null;

            switch (resizeType)
            {
                case MapResizeMode.PreserveScale:
                    resizedExtent = GetExtentPreserveScale(sourceExtent, sourceScreenWidth, targetScreenWidth,
                        targetScreenHeight, mapUnit);
                    break;
                case MapResizeMode.PreserveScaleAndCenter:
                    resizedExtent = GetExtentPreserveScaleAndCenter(sourceExtent, sourceScreenWidth, targetScreenWidth,
                        targetScreenHeight, mapUnit);
                    break;
                case MapResizeMode.PreserveExtent:
                    resizedExtent = GetExtentPreserveExtent(sourceExtent, targetScreenWidth, targetScreenHeight);
                    break;
                default:
                    break;
            }

            return resizedExtent;
        }

        private static RectangleShape GetExtentPreserveScale(RectangleShape sourceExtent, float sourceScreenWidth,
            float targetScreenWidth, float targetScreenHeight, GeographyUnit mapUnit)
        {
            var scale = GetScale(sourceExtent, sourceScreenWidth, mapUnit);
            var resolution = ScaleResolutionConverter.GetResolutionFromScale(scale, mapUnit);

            var extentWidth = resolution*targetScreenWidth;
            var extentHeight = resolution*targetScreenHeight;

            return new RectangleShape(sourceExtent.UpperLeftPoint,
                new PointShape(sourceExtent.UpperLeftPoint.X + extentWidth, sourceExtent.UpperLeftPoint.Y - extentHeight));
        }

        private static RectangleShape GetExtentPreserveScaleAndCenter(RectangleShape sourceExtent,
            float sourceScreenWidth, float targetScreenWidth, float targetScreenHeight, GeographyUnit mapUnit)
        {
            var scale = GetScale(sourceExtent, sourceScreenWidth, mapUnit);
            var resolution = ScaleResolutionConverter.GetResolutionFromScale(scale, mapUnit);

            var extentWidth = resolution*targetScreenWidth;
            var extentHeight = resolution*targetScreenHeight;

            var centerPointShape = sourceExtent.GetCenterPoint();

            var upperLeftPointShape = new PointShape(centerPointShape.X - extentWidth/2,
                centerPointShape.Y + extentHeight/2);
            var lowerRightPointShape = new PointShape(centerPointShape.X + extentWidth/2,
                centerPointShape.Y - extentHeight/2);

            return new RectangleShape(upperLeftPointShape, lowerRightPointShape);
        }

        private static RectangleShape GetExtentPreserveExtent(RectangleShape sourceExtent, float targetScreenWidth,
            float targetScreenHeight)
        {
            return GetDrawingExtent(sourceExtent, targetScreenWidth, targetScreenHeight);
        }

        private static void PanExtent(RectangleShape worldExtent, PanDirection direction, int percentage)
        {
            var width = worldExtent.Width;
            var height = worldExtent.Height;
            var panPercentage = (double) percentage/100;

            switch (direction)
            {
                case PanDirection.Up:
                    worldExtent.TranslateByOffset(0, height*panPercentage, GeographyUnit.Meter, DistanceUnit.Meter);
                    break;
                case PanDirection.UpperRight:
                    worldExtent.TranslateByOffset(width*panPercentage, height*panPercentage, GeographyUnit.Meter,
                        DistanceUnit.Meter);
                    break;
                case PanDirection.Right:
                    worldExtent.TranslateByOffset(width*panPercentage, 0, GeographyUnit.Meter, DistanceUnit.Meter);
                    break;
                case PanDirection.LowerRight:
                    worldExtent.TranslateByOffset(width*panPercentage, -height*panPercentage, GeographyUnit.Meter,
                        DistanceUnit.Meter);
                    break;
                case PanDirection.Down:
                    worldExtent.TranslateByOffset(0, -height*panPercentage, GeographyUnit.Meter, DistanceUnit.Meter);
                    break;
                case PanDirection.LowerLeft:
                    worldExtent.TranslateByOffset(-width*panPercentage, -height*panPercentage, GeographyUnit.Meter,
                        DistanceUnit.Meter);
                    break;
                case PanDirection.Left:
                    worldExtent.TranslateByOffset(-width*panPercentage, 0, GeographyUnit.Meter, DistanceUnit.Meter);
                    break;
                case PanDirection.UpperLeft:
                    worldExtent.TranslateByOffset(-width*panPercentage, height*panPercentage, GeographyUnit.Meter,
                        DistanceUnit.Meter);
                    break;
                default:
                    break;
            }
        }
    }
}