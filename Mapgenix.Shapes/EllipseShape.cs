using System;
using System.Collections.ObjectModel;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>
    /// Ellipse defined with a center point, a height and a width.
    /// </summary>
    [Serializable]
    public class EllipseShape : BaseAreaShape
    {
        private const int defaultQuaterNumber = 96;
        private PointShape centerPoint;
        private double width;
        private double height;

        /// <summary>Creates an ellipse.</summary>
        public EllipseShape()
            : this(new PointShape(), 0)
        {
        }

        /// <summary>Creates an ellipse with a center and a radius.</summary>
        public EllipseShape(PointShape center, double radius)
        {
            Validators.CheckParameterIsNotNull(center, "center");
            Validators.CheckIfInputValueIsBiggerThan(radius, "radius", 0, RangeCheckingInclusion.IncludeValue);

            centerPoint = center;
            width = radius * 2;
            height = radius * 2;
        }

        /// <summary>Creates an ellipse with the center of a feature and a radius.</summary>
        public EllipseShape(Feature centerPointFeature, double radius)
        {
            Validators.CheckIfInputValueIsBiggerThan(radius, "radius", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape centerShape = centerPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(centerShape);

            centerPoint = (PointShape)centerShape;
            width = radius * 2;
            height = radius * 2;
        }

        /// <summary>Creates an ellipse with a center, a horizontal radius and a vertical radius.</summary>
        public EllipseShape(PointShape center, double horizontalRadius, double verticalRadius)
        {
            Validators.CheckParameterIsNotNull(center, "center");
            Validators.CheckIfInputValueIsBiggerThan(horizontalRadius, "horizontalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(verticalRadius, "verticalRadius", 0, RangeCheckingInclusion.IncludeValue);

            centerPoint = center;
            width = horizontalRadius * 2;
            height = verticalRadius * 2;
        }

        /// <summary>Creates an ellipse with the center of a feature, a horizontal radius and a vertical radius.</summary>
        public EllipseShape(Feature centerPointFeature, double horizontalRadius, double verticalRadius)
        {
            Validators.CheckIfInputValueIsBiggerThan(horizontalRadius, "horizontalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(verticalRadius, "verticalRadius", 0, RangeCheckingInclusion.IncludeValue);

            BaseShape centerShape = centerPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(centerShape);

            centerPoint = (PointShape)centerShape;
            width = horizontalRadius * 2;
            height = verticalRadius * 2;
        }

        /// <summary>Creates an ellipse with a WKT.</summary>
        public EllipseShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

        /// <summary>Creates an ellipse with a WKB.</summary>
        public EllipseShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

        /// <summary>Creates an ellipse.</summary>
        public EllipseShape(PointShape center, double radius, GeographyUnit shapeUnit, DistanceUnit unitOfRadius)
        {
            Validators.CheckParameterIsNotNull(center, "center");
            Validators.CheckIfInputValueIsBiggerThan(radius, "radius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
   
            SetSize(center, radius * 2, radius * 2, shapeUnit, unitOfRadius);
            centerPoint = center;
        }

        /// <summary>Creates an ellipse.</summary>
        public EllipseShape(Feature centerPointFeature, double radius, GeographyUnit shapeUnit, DistanceUnit unitOfRadius)
        {
            Validators.CheckIfInputValueIsBiggerThan(radius, "radius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            BaseShape centerShape = centerPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(centerShape);

            PointShape centerPointShape = (PointShape)centerShape;
            SetSize(centerPointShape, radius * 2, radius * 2, shapeUnit, unitOfRadius);
            centerPoint = centerPointShape;
        }

        /// <summary>Creates an ellipse.</summary>
        public EllipseShape(PointShape center, double horizontalRadius, double verticalRadius, GeographyUnit shapeUnit, DistanceUnit unitOfRadius)
        {
            Validators.CheckParameterIsNotNull(center, "center");
            Validators.CheckIfInputValueIsBiggerThan(horizontalRadius, "horizontalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(verticalRadius, "verticalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            SetSize(center, horizontalRadius * 2, verticalRadius * 2, shapeUnit, unitOfRadius);
            centerPoint = center;
        }

        /// <summary>Creates an ellipse.</summary>
        public EllipseShape(Feature centerPointFeature, double horizontalRadius, double verticalRadius, GeographyUnit shapeUnit, DistanceUnit unitOfRadius)
        {
            Validators.CheckIfInputValueIsBiggerThan(horizontalRadius, "horizontalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(verticalRadius, "verticalRadius", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            BaseShape centerShape = centerPointFeature.GetShape();
            Validators.CheckShapeIsPointShape(centerShape);

            PointShape centerPointShape = (PointShape)centerShape;
            SetSize(centerPointShape, horizontalRadius * 2, verticalRadius * 2, shapeUnit, unitOfRadius);
            centerPoint = centerPointShape;
        }

        /// <summary>Returns a complete copy of the shape.</summary>
        protected override BaseShape CloneDeepCore()
        {
            EllipseShape ellipse = new EllipseShape();
            ellipse.centerPoint = (PointShape)this.centerPoint.CloneDeep();
            ellipse.height = this.height;
            ellipse.width = this.width;

            return ellipse;
        }

        /// <summary>Returns the well-known type of the shape.</summary>
        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Polygon;
        }

        /// <summary>Gets the width of the ellipse in the unit of the shape.</summary>
        public double Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>Sets the width of the ellipse.</summary>
        public void SetWidthByUnit(double newWidth, GeographyUnit shapeUnit, DistanceUnit unitOfWidth)
        {
            Validators.CheckIfInputValueIsBiggerThan(newWidth, "width", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThiShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                width = Conversion.ConvertMeasureUnits(newWidth, unitOfWidth, distanceUnitOfThiShape);
            }
            else
            {
                double longDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(newWidth / 2, unitOfWidth, centerPoint.Y);
                width = longDif * 2;
            }
        }

        /// <summary>Gets the width of the ellipse.</summary>
        public double GetWidthByUnit(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            double returnValue = 0;
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThiShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                returnValue = Conversion.ConvertMeasureUnits(width, distanceUnitOfThiShape, returningUnit);
            }
            else
            {
                RectangleShape boundingBox = GetBoundingBox();
                returnValue = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(boundingBox.UpperLeftPoint.X, boundingBox.UpperLeftPoint.Y - boundingBox.Height / 2, boundingBox.LowerRightPoint.X, boundingBox.LowerRightPoint.Y + boundingBox.Height / 2, returningUnit);
            }
            return returnValue;
        }

        /// <summary>Gets the height of the ellipse in the unit of the shape.</summary>
        public double Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>Sets the height of the ellipse.</summary>
        public void SetHeightByUnit(double newHeight, GeographyUnit shapeUnit, DistanceUnit unitOfHeight)
        {
            Validators.CheckIfInputValueIsBiggerThan(newHeight, "height", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThiShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                height = Conversion.ConvertMeasureUnits(newHeight, unitOfHeight, distanceUnitOfThiShape);
            }
            else
            {
                double latitudeDifference = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(newHeight / 2, unitOfHeight, centerPoint.X);
                height = latitudeDifference * 2;
            }
        }

        /// <summary>Gets the height of the ellipse.</summary>
        public double GetHeightByUnit(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
  
            double returnValue = 0;
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThiShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                returnValue = Conversion.ConvertMeasureUnits(height, distanceUnitOfThiShape, returningUnit);
            }
            else
            {
                RectangleShape rect = GetBoundingBox();
                returnValue = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(rect.UpperLeftPoint.X + rect.Width / 2, rect.UpperLeftPoint.Y, rect.LowerRightPoint.X - rect.Width / 2, rect.LowerRightPoint.Y, returningUnit);
            }
            return returnValue;
        }

        /// <summary>Gets or sets the center PointShape of the EllipseShape.</summary>
        public PointShape Center
        {
            get
            {
                return centerPoint;
            }
            set
            {
                centerPoint = value;
            }
        }

        /// <summary>Returns the ellipse as a PolygonShape.</summary>
        public PolygonShape ToPolygon()
        {
            Validators.CheckShapeIsValidForOperation(this);

            PolygonShape resultPolygon = BuildPolygon(defaultQuaterNumber);
            return resultPolygon;
        }

        /// <summary>Returns the ellipse as a PolygonShape.</summary>
        public PolygonShape ToPolygon(int vertexCountInQuarter)
        {
            Validators.CheckIfInputValueIsBiggerThan(vertexCountInQuarter, "vertexCountInQuarter", 1, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return BuildPolygon(vertexCountInQuarter);
        }

        /// <summary>Returns the tangent points of the ellipse in relation to a target ellipse.</summary>
        public Collection<PointShape> GetTangents(EllipseShape targetEllipse)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsNotNull(targetEllipse, "targetEllipse");
            Validators.CheckParameterIsValid(targetEllipse, "targetEllipse");

            return GetTangentsFromEllipse(targetEllipse);
        }

        /// <summary>Returns the perimeter of the ellipse.</summary>
        protected override double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double perimeter = 0;
            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                perimeter = this.ToPolygon().GetPerimeter(shapeUnit, returningUnit);
            }
            else
            {
                perimeter = 2 * Math.PI * Math.Sqrt(0.125 * (width * width + height * height));
                DistanceUnit fromUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                perimeter = Conversion.ConvertMeasureUnits(perimeter, fromUnit, returningUnit);
            }

            return perimeter;
        }

        /// <summary>Returns the area the ellipse.</summary>
        protected override double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            double area = 0;

            if (shapeUnit == GeographyUnit.DecimalDegree)
            {
                area = this.ToPolygon().GetArea(shapeUnit, returningUnit);
            }
            else
            {
                area = Math.PI * width * height / 4.0;
                DistanceUnit formDisUnit = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                AreaUnit fromAreaUnit = GetAreaUnitFromLengthUnit(formDisUnit);
                area = Conversion.ConvertMeasureUnits(area, fromAreaUnit, returningUnit);
            }
            return area;
        }

        /// <summary>Increases the size of the ellipse by a percentage.</summary>
        /// <param name="percentage">Percentage by which to increase the ellipse's size.</param>
        protected override void ScaleUpCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 + percentage / 100.0;
            Scale(factor);
        }

        /// <summary>Decreases the size of the ellipse by a percentage.</summary>
        /// <param name="percentage">Percentage by which to decrease the ellipse's size.</param>
        protected override void ScaleDownCore(double percentage)
        {
            Validators.CheckIfInputValueIsBiggerThan(percentage, "percentage", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 - percentage / 100.0;
            Scale(factor);
        }

        /// <summary>Returns the rectangle encompassing the ellipse.</summary>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            PointShape upperLeftPoint = new PointShape(centerPoint.X - width / 2, centerPoint.Y + height / 2);
            PointShape lowerRightPoint = new PointShape(centerPoint.X + width / 2, centerPoint.Y - height / 2);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }

        /// <summary>Returns a shape registered from its original coordinate system to another based on two anchor points.</summary>
        protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsNotNull(fromPoint, "fromPoint");
            Validators.CheckParameterIsNotNull(toPoint, "toPoint");

            EllipseShape resultEllipse = new EllipseShape();
            double destX = toPoint.X;
            double destY = toPoint.Y;

            if (toUnit == GeographyUnit.DecimalDegree)
            {
                double xDiff = centerPoint.X - fromPoint.X;
                double yDiff = centerPoint.Y - fromPoint.Y;
                double longitudeDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xDiff, fromUnit, destY);
                double latitudeDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yDiff, fromUnit, destX);
                if (double.IsNaN(longitudeDif))
                {
                    longitudeDif = 0;
                }
                if (double.IsNaN(latitudeDif))
                {
                    latitudeDif = 0.0;
                }
                if (latitudeDif != 0 || latitudeDif != 0)
                {
                    resultEllipse.centerPoint = new PointShape(destX + longitudeDif, destY + latitudeDif);
                }
                resultEllipse.width = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(Width, fromUnit, resultEllipse.centerPoint.Y);
                resultEllipse.height = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(height, fromUnit, resultEllipse.centerPoint.X);
            }
            else
            {
                DistanceUnit distanceToUnit = Conversion.ConvertGeographyUnitToDistanceUnit(toUnit);
                double xDiff = Conversion.ConvertMeasureUnits(centerPoint.X - fromPoint.X, fromUnit, distanceToUnit);
                double yDiff = Conversion.ConvertMeasureUnits(centerPoint.Y - fromPoint.Y, fromUnit, distanceToUnit);
                resultEllipse.centerPoint = new PointShape(destX + xDiff, destY + yDiff);
                resultEllipse.width = Conversion.ConvertMeasureUnits(width, fromUnit, distanceToUnit);
                resultEllipse.height = Conversion.ConvertMeasureUnits(height, fromUnit, distanceToUnit);
            }

            return resultEllipse;
        }

        /// <summary>Moves the shape from one location to another based on an X and Y offset distance.</summary>
        /// <param name="xOffsetDistance">X offset distance.</param>
        /// <param name="yOffsetDistance">Y Offset distance.</param>
        /// <param name="shapeUnit">Geography unit of the feature.</param>
        /// <param name="distanceUnit">Distance unit.</param>
        protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            centerPoint.TranslateByOffset(xOffsetDistance, yOffsetDistance, shapeUnit, distanceUnit);
        }

        /// <summary>Moves a shape from one location to another based on a distance and a direction in degrees.</summary>
       protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "angleInDegrees", 0, 360);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);

            centerPoint.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
        }

       /// <summary>Gets whether the shape can be rotated.</summary>
        public override bool CanRotate
        {
            get
            {
                return false;
            }
        }

        /// <summary>Rotates the shape by a number of degrees based on a pivot point.</summary>
        protected override void RotateCore(PointShape pivotPoint, float degreeAngle)
        {
            Validators.CheckParameterIsNotNull(pivotPoint, "pivotPoint");
            Validators.CheckIfInputValueIsInRange(degreeAngle, "degreeAngle", 0, 360);
            Validators.CheckShapeIsValidForOperation(this);

            base.RotateCore(pivotPoint, degreeAngle);
        }

        /// <summary>Returns the point on  the shape closest to the target shape.</summary>
        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            if (Intersects(targetShape))
            {
                return null;
            }

            PointShape returnPointShape = null;
            double minDistance = double.MaxValue;

            EllipseShape ellipseShape = targetShape as EllipseShape;

            if (ellipseShape != null)
            {
                GetClosestInfoToEllipseShape(ellipseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                return returnPointShape;
            }

            WellKnownType targetShapeWellKnownType = targetShape.GetWellKnownType();
            BaseShape wktBaseShape = BaseShape.CreateShapeFromWellKnownData(targetShape.GetWellKnownText());

            switch (targetShapeWellKnownType)
            {
                case WellKnownType.Polygon:
                    GetClosestInfoToPolygonShape((PolygonShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                case WellKnownType.Multipolygon:
                    GetClosestInfoToMultipolygonShape((MultipolygonShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                case WellKnownType.Line:
                    GetClosestInfoToLineShape((LineShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                case WellKnownType.Multiline:
                    GetClosestInfoToMultilineShape((MultilineShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                case WellKnownType.Point:
                    GetClosestInfoToPointShape((PointShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                case WellKnownType.Multipoint:
                    GetClosestPointToMultipointShape((MultipointShape)wktBaseShape, shapeUnit, DistanceUnit.Meter, out returnPointShape, out minDistance);
                    break;
                default:
                    break;
            }
            return returnPointShape;
        }

        /// <summary>Returns the distance between the shape and a target shape.</summary>
        protected override double GetDistanceToCore(BaseShape targetShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            if (Intersects(targetShape))
            {
                return 0;
            }

            double returnDistance = 0;
            PointShape returnPointShape = null;

            switch (targetShape.GetType().Name)
            {
                case "PointShape":
                    GetClosestInfoToPointShape((PointShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "MultipointShape":
                    GetClosestPointToMultipointShape((MultipointShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "LineShape":
                    GetClosestInfoToLineShape((LineShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "MultilineShape":
                    GetClosestInfoToMultilineShape((MultilineShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "PolygonShape":
                    GetClosestInfoToPolygonShape((PolygonShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "MultipolygonShape":
                    GetClosestInfoToMultipolygonShape((MultipolygonShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "EllipseShape":
                    GetClosestInfoToEllipseShape((EllipseShape)targetShape, shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "RectangleShape":
                    GetClosestInfoToPolygonShape(((RectangleShape)targetShape).ToPolygon(), shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                case "RingShape":
                    GetClosestInfoToPolygonShape(((RingShape)targetShape).ToPolygon(), shapeUnit, distanceUnit, out returnPointShape, out returnDistance);
                    break;

                default:
                    returnDistance = base.GetDistanceToCore(targetShape, shapeUnit, distanceUnit);
                    break;
            }

            return returnDistance;
        }

        /// <summary>Returns the well-known text representation of the shape.</summary>
        /// <returns>String representing the shape in well-known text.</returns>
        protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);
            return GetWellKnownTextCore(defaultQuaterNumber);
        }

        /// <summary>Returns the well-known text representation of the shape.</summary>
        /// <returns>String representing the shape in well-known text.</returns>
        protected string GetWellKnownTextCore(int vertexCountInQuarter)
        {
            Validators.CheckIfInputValueIsBiggerThan(vertexCountInQuarter, "vertexCountInQuarter", 0, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon(vertexCountInQuarter).GetWellKnownText();
        }

        /// <summary>Returns a byte array representing the shape in well-known binary.</summary>
        /// <returns>Byte array representing the shape in well-known binary.</returns>
        /// <param name="byteOrder">Byte order to encode the well-known binary.</param>
        protected override byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);
  
            return GetWellKnownBinaryCore(byteOrder, defaultQuaterNumber);
        }

        /// <summary>Returns a byte array representing the shape in well-known binary.</summary>
        /// <returns>Byte array representing the shape in well-known binary.</returns>
        /// <param name="byteOrder">Byte order to encode the well-known binary.</param>
        protected byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder, int vertexCountInQuarter)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckIfInputValueIsBiggerThan(vertexCountInQuarter, "vertexCountInQuarter", 0, RangeCheckingInclusion.ExcludeValue);

            return ToPolygon(vertexCountInQuarter).GetWellKnownBinary(byteOrder);
        }

        /// <summary>Hydrates the shape with its data from well-known text.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownText">Well-known text to hydrate the shape.</param>
        protected override void LoadFromWellKnownDataCore(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            RectangleShape sourceRectangle = baseShape.GetBoundingBox();

            this.centerPoint = sourceRectangle.GetCenterPoint();
            this.width = sourceRectangle.Width;
            this.height = sourceRectangle.Height;
        }

        /// <summary>Hydrates the shape with its data from well-known text.</summary>
        /// <returns>None</returns>
        /// <param name="wellKnownText">Well-known text to hydrate the shape.</param>
        protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            RectangleShape sourceRectangle = baseShape.GetBoundingBox();

            this.centerPoint = sourceRectangle.GetCenterPoint();
            this.width = sourceRectangle.Width;
            this.height = sourceRectangle.Height;
        }

        /// <summary>Returns a ShapeValidationResult based on a validation tests.</summary>
        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (centerPoint == null || Width <= 0 || Height <= 0)
                    {
                        validateResult = new ShapeValidationResult(false, ExceptionDescription.ShapeIsInvalidForOperation);
                    }
                    break;

                case ShapeValidationMode.Advanced:
                    break;

                default:
                    break;
            }

            return validateResult;
        }

        /// <summary>Returns true if a target shape lies within the interior of the shape.</summary>
        protected override bool ContainsCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            bool isContain = false;
            string type = targetShape.GetType().Name;
            switch (type)
            {
                case "PolygonShape":
                    isContain = ContainsPolgyonShape((PolygonShape)targetShape);
                    break;
                case "MultipolygonShape":
                    isContain = ContainsMultipolygonShape((MultipolygonShape)targetShape);
                    break;
                case "EllipseShape":
                    isContain = ContainsEllipseShape((EllipseShape)targetShape);
                    break;
                case "RectangleShape":
                    isContain = ContainsRectangleShape((RectangleShape)targetShape);
                    break;
                case "RingShape":
                    isContain = ContainsRingShape((RingShape)targetShape);
                    break;
                case "LineShape":
                    isContain = ContainsLineShape((LineShape)targetShape);
                    break;
                case "MultilineShape":
                    isContain = ContainsMultiLineShape((MultilineShape)targetShape);
                    break;
                case "PointShape":
                    isContain = ContainsPointShape((PointShape)targetShape);
                    break;
                case "MultipointShape":
                    isContain = ContainsMultiPointShape((MultipointShape)targetShape);
                    break;
                default:
                    base.ContainsCore(targetShape);
                    break;
            }
            return isContain;
        }

        /// <summary>Returns true if the shape and a target shape have at least one point in common.</summary>
        protected override bool IntersectsCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            bool isIntersects = false;
            string type = targetShape.GetType().Name;
            switch (type)
            {
                case "PolygonShape":
                    isIntersects = IntersectsPolgyonShape((PolygonShape)targetShape);
                    break;
                case "MultipolygonShape":
                    isIntersects = IntersectsMultipolygonShape((MultipolygonShape)targetShape);
                    break;
                case "EllipseShape":
                    isIntersects = IntersectsEllipseShape((EllipseShape)targetShape);
                    break;
                case "RectangleShape":
                    isIntersects = IntersectsRectangleShape((RectangleShape)targetShape);
                    break;
                case "RingShape":
                    isIntersects = IntersectsRingShape((RingShape)targetShape);
                    break;
                case "LineShape":
                    isIntersects = IntersectsLineShape((LineShape)targetShape);
                    break;
                case "MultilineShape":
                    isIntersects = IntersectsMultiLineShape((MultilineShape)targetShape);
                    break;
                case "PointShape":
                    isIntersects = IntersectsPointShape((PointShape)targetShape);
                    break;
                case "MultipointShape":
                    isIntersects = IntersectsMultiPointShape((MultipointShape)targetShape);
                    break;
                default:
                    isIntersects = base.IntersectsCore(targetShape);
                    break;
            }
            return isIntersects;
        }

        /// <summary>Returns the crossing points between the shape and a target shape.</summary>
        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetCrossing(targetShape);
        }

        private void SetSize(PointShape center, double newWidth, double newHeight, GeographyUnit shapeUnit, DistanceUnit unitOfSize)
        {
            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThiShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                width = Conversion.ConvertMeasureUnits(newWidth, unitOfSize, distanceUnitOfThiShape);
                height = Conversion.ConvertMeasureUnits(newHeight, unitOfSize, distanceUnitOfThiShape);
            }
            else
            {
                double longDif = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(newWidth / 2, unitOfSize, center.Y);
                double latDif = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(newHeight / 2, unitOfSize, center.X);
                width = longDif * 2;
                height = latDif * 2;
            }
        }

        private PolygonShape BuildPolygon(int quaterVertexCount)
        {
            RingShape ringShape = new RingShape();

            int sidesCount = quaterVertexCount * 4;
            double radius = Math.Max(Width, Height) / 2;
            double angle = Math.PI * (1 / sidesCount - 1 / 2);
            double rotatedAngle = 0;

            for (int i = 0; i < sidesCount; i++)
            {
                rotatedAngle = angle + (i * 2 * Math.PI / sidesCount);
                double x = this.centerPoint.X + radius * Math.Cos(rotatedAngle);
                double y = this.centerPoint.Y + radius * Math.Sin(rotatedAngle);
                ringShape.Vertices.Add(new Vertex(x, y));
            }

            ringShape.Vertices.Add(ringShape.Vertices[0]);

            if (Width < Height)
            {
                double ratio = Width / Height;
                for (int i = 0; i < ringShape.Vertices.Count; i++)
                {
                    double x = centerPoint.X + ratio * (ringShape.Vertices[i].X - centerPoint.X);
                    double y = ringShape.Vertices[i].Y;

                    ringShape.Vertices[i] = new Vertex(x, y);
                }
            }
            else if (Width > Height)
            {
                double ratio = Height / Width;
                for (int i = 0; i < ringShape.Vertices.Count; i++)
                {
                    double x = ringShape.Vertices[i].X;
                    double y = centerPoint.Y + ratio * (ringShape.Vertices[i].Y - centerPoint.Y);

                    ringShape.Vertices[i] = new Vertex(x, y);
                }
            }

            return new PolygonShape(ringShape);
        }

        private void Scale(double factor)
        {
            width = width * factor;
            height = height * factor;
        }

        private void GetClosestInfoToPointShape(PointShape targetPointShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = GeShortestPointFromOutsidePoint(targetPointShape);

            minDistance = double.MaxValue;
            if (closestPoint != null)
            {
                minDistance = closestPoint.GetDistanceTo(targetPointShape, shapeUnit, distanceUnit);
            }
        }

        private void GetClosestPointToMultipointShape(MultipointShape targetMultiPoint, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;
            minDistance = double.MaxValue;

            foreach (PointShape pointShape in targetMultiPoint.Points)
            {
                double currentDist = double.MaxValue;
                PointShape tmpCloestPointShape = new PointShape();
                GetClosestInfoToPointShape(pointShape, shapeUnit, distanceUnit, out tmpCloestPointShape, out currentDist);
                if (currentDist < minDistance)
                {
                    minDistance = currentDist;
                    closestPoint = new PointShape(tmpCloestPointShape.X, tmpCloestPointShape.Y);
                }
            }
        }

        private void GetClosestInfoToLineShape(LineShape lineShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;
            minDistance = double.MaxValue;

            PointShape ellipseCenter = centerPoint;
            for (int i = 0; i < lineShape.Vertices.Count - 1; i++)
            {
                Vertex fromPoint = lineShape.Vertices[i];
                Vertex toPoint = lineShape.Vertices[i + 1];
                PointShape nearestPoint = ellipseCenter.GetNearestPointShapeFromLineSegment(fromPoint.X, fromPoint.Y, toPoint.X, toPoint.Y);
                PointShape interPointShape = GeShortestPointFromOutsidePoint(nearestPoint);
                if (interPointShape != null)
                {
                    double currentDistance = interPointShape.GetDistanceTo(nearestPoint, shapeUnit, distanceUnit);
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        closestPoint = interPointShape;
                    }
                }
            }
        }

        private void GetClosestInfoToMultilineShape(MultilineShape multiLineShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;

            minDistance = double.MaxValue;
            foreach (LineShape lineShape in multiLineShape.Lines)
            {
                double currentDist = double.MaxValue;
                PointShape tmpCloestPointShape = new PointShape();
                GetClosestInfoToLineShape(lineShape, shapeUnit, distanceUnit, out tmpCloestPointShape, out currentDist);
                if (currentDist < minDistance)
                {
                    minDistance = currentDist;
                    closestPoint = new PointShape(tmpCloestPointShape.X, tmpCloestPointShape.Y);
                }
            }
        }

        private void GetClosestInfoToPolygonShape(PolygonShape polygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;
            PointShape ellipseCenter = centerPoint;
            minDistance = double.MaxValue;

            for (int i = 0; i < polygonShape.OuterRing.Vertices.Count - 1; i++)
            {
                Vertex lineStartPoint = polygonShape.OuterRing.Vertices[i];
                Vertex lineEndPoint = polygonShape.OuterRing.Vertices[i + 1];

                PointShape lineSegmentPointShape = ellipseCenter.GetNearestPointShapeFromLineSegment(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y);
                PointShape interPointShape = GeShortestPointFromOutsidePoint(lineSegmentPointShape);
                if (interPointShape != null)
                {
                    double currentDisatance = interPointShape.GetDistanceTo(lineSegmentPointShape, shapeUnit, distanceUnit);
                    if (currentDisatance < minDistance)
                    {
                        minDistance = currentDisatance;
                        closestPoint = interPointShape;
                    }
                }
            }

            foreach (RingShape ringShape in polygonShape.InnerRings)
            {
                for (int i = 0; i < ringShape.Vertices.Count - 1; i++)
                {
                    Vertex lineStartPoint = ringShape.Vertices[i];
                    Vertex lineEndPoint = ringShape.Vertices[i + 1];

                    PointShape lineSegmentPointShape = ellipseCenter.GetNearestPointShapeFromLineSegment(lineStartPoint.X, lineStartPoint.Y, lineEndPoint.X, lineEndPoint.Y);
                    PointShape interPointShape = GeShortestPointFromOutsidePoint(lineSegmentPointShape);

                    if (interPointShape != null)
                    {
                        double currentDisatance = interPointShape.GetDistanceTo(lineSegmentPointShape, shapeUnit, distanceUnit);
                        if (currentDisatance < minDistance)
                        {
                            minDistance = currentDisatance;
                            closestPoint = interPointShape;
                        }
                    }
                }
            }
        }

        private PointShape GetClosestInfoToMultipolygonShape(MultipolygonShape multiPolygonShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;

            minDistance = double.MaxValue;
            foreach (PolygonShape polygon in multiPolygonShape.Polygons)
            {
                double currentDist = double.MaxValue;
                PointShape tmpCloestPointShape = new PointShape();
                GetClosestInfoToPolygonShape(polygon, shapeUnit, distanceUnit, out tmpCloestPointShape, out currentDist);
                if (currentDist < minDistance && tmpCloestPointShape != null)
                {
                    minDistance = currentDist;
                    closestPoint = new PointShape(tmpCloestPointShape.X, tmpCloestPointShape.Y);
                }
            }

            return closestPoint;
        }

        private void GetClosestInfoToEllipseShape(EllipseShape ellipseShape, GeographyUnit shapeUnit, DistanceUnit distanceUnit, out PointShape closestPoint, out double minDistance)
        {
            closestPoint = null;
            PointShape thisPoint = GeShortestPointFromOutsidePoint(ellipseShape.centerPoint);
            PointShape toPoint = ellipseShape.GeShortestPointFromOutsidePoint(centerPoint);

            minDistance = double.MaxValue;
            if (thisPoint != null && toPoint != null)
            {
                minDistance = thisPoint.GetDistanceTo(toPoint, shapeUnit, distanceUnit);
                closestPoint = thisPoint;
            }
        }

        private bool ContainsMultiPointShape(MultipointShape multiPointShape)
        {
            bool result = true;

            foreach (PointShape point in multiPointShape.Points)
            {
                if (!ContainsPointShape(point))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool ContainsLineShape(LineShape lineShape)
        {
            bool result = true;

            foreach (Vertex vertex in lineShape.Vertices)
            {
                if (!ContainsPointShape(vertex.X, vertex.Y))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool ContainsMultiLineShape(MultilineShape multiLineShape)
        {
            bool result = true;

            foreach (LineShape lineShape in multiLineShape.Lines)
            {
                if (!ContainsLineShape(lineShape))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool ContainsRingShape(RingShape ringShape)
        {
            bool result = true;

            foreach (Vertex pointShape in ringShape.Vertices)
            {
                if (!ContainsPointShape(pointShape.X, pointShape.Y))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool ContainsRectangleShape(RectangleShape rectangleShape)
        {
            bool result = true;

            if (!ContainsPointShape(rectangleShape.UpperLeftPoint) ||
                !ContainsPointShape(rectangleShape.UpperRightPoint) ||
                !ContainsPointShape(rectangleShape.LowerLeftPoint) ||
                !ContainsPointShape(rectangleShape.LowerRightPoint))
            {
                result = false;
            }
            return result;
        }

        private bool ContainsEllipseShape(EllipseShape ellipseShape)
        {
            RectangleShape rect = ellipseShape.GetBoundingBox();
            PointShape leftPointShape = new PointShape(rect.UpperLeftPoint.X, ellipseShape.centerPoint.Y);
            PointShape topPointShape = new PointShape(ellipseShape.centerPoint.X, rect.UpperLeftPoint.Y);
            PointShape rightPointShape = new PointShape(rect.LowerRightPoint.X, ellipseShape.centerPoint.Y);
            PointShape bottomPointShape = new PointShape(ellipseShape.centerPoint.X, rect.LowerRightPoint.Y);

            bool result = true;

            if (!ContainsPointShape(leftPointShape) ||
                !ContainsPointShape(topPointShape) ||
                !ContainsPointShape(rightPointShape) ||
                !ContainsPointShape(bottomPointShape))
            {
                result = false;
            }
            return result;
        }

        private bool ContainsPolgyonShape(PolygonShape polygonShape)
        {
            bool result = true;

            foreach (RingShape innerRing in polygonShape.InnerRings)
            {
                if (!ContainsRingShape(innerRing))
                {
                    result = false;
                    break;
                }
            }

            if (!Contains(polygonShape.OuterRing))
            {
                result = false;
            }
            return result;
        }

        private bool ContainsMultipolygonShape(MultipolygonShape multipolygonShape)
        {
            bool result = true;

            foreach (PolygonShape polygon in multipolygonShape.Polygons)
            {
                if (!ContainsPolgyonShape(polygon))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private bool IntersectsPointShape(PointShape pointShape)
        {
            bool result = false;

            if (ContainsPointShape(pointShape))
            {
                result = true;
            }
            return result;
        }

        private bool IntersectsMultiPointShape(MultipointShape multiPointShape)
        {
            bool hasPointWithInEllipse = false;

            foreach (PointShape point in multiPointShape.Points)
            {
                if (ContainsPointShape(point))
                {
                    hasPointWithInEllipse = true;
                }
            }
            return hasPointWithInEllipse;
        }

        private bool IntersectsLineShape(LineShape lineShape)
        {
            bool hasPointWithInEllipse = false;

            foreach (Vertex vertex in lineShape.Vertices)
            {
                if (ContainsPointShape(vertex.X, vertex.Y))
                {
                    hasPointWithInEllipse = true;
                }
            }

            return hasPointWithInEllipse;
        }

        private bool IntersectsMultiLineShape(MultilineShape multiLineShape)
        {
            bool result = false;

            foreach (LineShape lineShape in multiLineShape.Lines)
            {
                if (IntersectsLineShape(lineShape))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool IntersectsRingShape(RingShape ringShape)
        {
            bool hasPointWithInEllipse = false;

            foreach (Vertex vertex in ringShape.Vertices)
            {
                if (ContainsPointShape(vertex.X, vertex.Y))
                {
                    hasPointWithInEllipse = true;
                }
            }
            return hasPointWithInEllipse;
        }

        private bool IntersectsRectangleShape(RectangleShape rectangleShape)
        {
            bool hasPointWithInEllipse = false;

            if (ContainsPointShape(rectangleShape.UpperLeftPoint))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(rectangleShape.UpperRightPoint))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(rectangleShape.LowerLeftPoint))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(rectangleShape.LowerRightPoint))
            {
                hasPointWithInEllipse = true;
            }

            return hasPointWithInEllipse;
        }

        private bool IntersectsEllipseShape(EllipseShape ellipseShape)
        {
            RectangleShape rect = ellipseShape.GetBoundingBox();
            PointShape leftPointShape = new PointShape(rect.UpperLeftPoint.X, ellipseShape.centerPoint.Y);
            PointShape topPointShape = new PointShape(ellipseShape.centerPoint.X, rect.UpperLeftPoint.Y);
            PointShape rightPointShape = new PointShape(rect.LowerRightPoint.X, ellipseShape.centerPoint.Y);
            PointShape bottomPointShape = new PointShape(ellipseShape.centerPoint.X, rect.LowerRightPoint.Y);

            bool hasPointWithInEllipse = false;

            if (ContainsPointShape(leftPointShape))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(topPointShape))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(rightPointShape))
            {
                hasPointWithInEllipse = true;
            }

            if (ContainsPointShape(bottomPointShape))
            {
                hasPointWithInEllipse = true;
            }

            return hasPointWithInEllipse;
        }

        private bool IntersectsPolgyonShape(PolygonShape polygonShape)
        {
            bool result = false;

            foreach (RingShape innerRing in polygonShape.InnerRings)
            {
                if (IntersectsRingShape(innerRing))
                {
                    result = true;
                    break;
                }
            }

            if (IntersectsRingShape(polygonShape.OuterRing))
            {
                result = true;
            }
            return result;
        }

        private bool IntersectsMultipolygonShape(MultipolygonShape multipolygonShape)
        {
            bool result = false;

            foreach (PolygonShape polygon in multipolygonShape.Polygons)
            {
                if (IntersectsPolgyonShape(polygon))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private Collection<PointShape> GetTangentsFromEllipse(EllipseShape secondEllipse)
        {
            Validators.CheckParameterIsNotNull(this, "firstEllipse");
            Validators.CheckParameterIsNotNull(secondEllipse, "secondEllipse");
            Validators.CheckParameterIsValid(this, "firstEllipse");
            Validators.CheckParameterIsValid(secondEllipse, "secondEllipse");
            
            double Alpha;
            double Beta;
            double a;
            double b;
            double Alphap;
            double Betap;
            double ap;
            double bp;
            double Gamma;
            double X1;
            double Y1;
            double X1p;
            double Y1p;
            double X2;
            double Y2;
            double X2p;
            double Y2p;
            double k;
            double e;
            double f;
            double fp;
            double a1;
            double b1;
            double D1;
            double D2;
            double D3;
            double D;
            double X = 0;
            double Y = 0;
            double r1;
            double r2;
            double Rho1;
            double Rho2;
            double gA1;
            double gB1;
            double ga2;
            double gB2;
            double gA1p;
            double gB1p;
            double gA2p;
            double gB2p;
            double gA;
            double gB;
            double gC;
            double gDelta;
            double gAp;
            double gBp;
            double gCp;
            double gDeltap;
            Collection<PointShape> ResultPoints = new Collection<PointShape>();
            const int DIGITS = 6;

            double C1x = Math.Round(this.Center.X, DIGITS);
            double C1y = Math.Round(this.Center.Y, DIGITS);
            double C2x = Math.Round(secondEllipse.Center.X, DIGITS);
            double C2y = Math.Round(secondEllipse.Center.Y, DIGITS);

            const double RADIANTFACTOR = Math.PI / 180;

            if (C1x < C2x)
            {
                Alpha = C1x;
                Beta = C1y;
                a = (Math.Round(this.Width, DIGITS) / 2);
                b = (Math.Round(this.Height, DIGITS) / 2);

                Alphap = C2x;
                Betap = C2y;
                ap = (Math.Round(secondEllipse.Width, DIGITS) / 2);
                bp = (Math.Round(secondEllipse.Height, DIGITS) / 2);
            }
            else
            {
                Alpha = C2x;
                Beta = C2y;
                a = (Math.Round(secondEllipse.Width, DIGITS) / 2);
                b = (Math.Round(secondEllipse.Height, DIGITS) / 2);

                Alphap = C1x;
                Betap = C1y;
                ap = (Math.Round(this.Width, DIGITS) / 2);
                bp = (Math.Round(this.Height, DIGITS) / 2);
            }

            if ((Alpha != Alphap) & (Beta != Betap))
            {
                a1 = (Betap - Beta) / (Alphap - Alpha);
                Gamma = Math.Atan(a1);
                if ((ap != a) | (bp != b))
                {
                    k = ((ap * Math.Pow(Math.Sin(Gamma), 2)) / a) + ((bp * Math.Pow(Math.Cos(Gamma), 2)) / b);
                }
                else
                {
                    k = 1;
                }
                if (k != 1)
                {
                    b1 = Beta - (a1 * Alpha);
                    D1 = (1 + Math.Pow(a1, 2)) * (Math.Pow(k, 2) - 1);
                    D2 = (Math.Pow(k, 2) * (2 * a1 * (b1 - Beta) - (2 * Alpha))) - ((2 * a1) * (b1 - Betap) - (2 * Alphap));
                    D3 = (Math.Pow(k, 2) * (Math.Pow(Alpha, 2) + Math.Pow((b1 - Beta), 2)) - (Math.Pow(Alphap, 2) + Math.Pow((b1 - Betap), 2)));
                    D = Math.Pow(D2, 2) - (4 * D1 * D3);

                    if (k > 1)
                    {
                        X = (-D2 - Math.Sqrt(Math.Abs(D))) / (2 * D1);
                        Y = (a1 * X) + b1;
                    }
                    else if (k < 1)
                    {
                        X = (-D2 - Math.Sqrt(Math.Abs(D))) / (2 * D1);
                        Y = (a1 * X) + b1;
                    }

                    gA = 4 * (Math.Pow(a, 2) * Math.Pow(b, 2)) * ((2 * Alpha * X) + Math.Pow(a, 2) - Math.Pow(X, 2) - Math.Pow(Alpha, 2));
                    gB = (8 * Math.Pow(a, 2) * Math.Pow(b, 2)) * ((Alpha * Beta) + (X * Y) - (Alpha * Y) - (Beta * X));
                    gC = 4 * (Math.Pow(a, 2) * Math.Pow(b, 2)) * ((2 * Beta * Y) + Math.Pow(b, 2) - Math.Pow(Y, 2) - Math.Pow(Beta, 2));
                    gDelta = Math.Pow(gB, 2) - (4 * gA * gC);

                    r1 = (-gB + Math.Sqrt(Math.Abs(gDelta))) / (2 * gA);
                    r2 = (-gB - Math.Sqrt(Math.Abs(gDelta))) / (2 * gA);
                    Rho1 = Y - (r1 * X);
                    Rho2 = Y - (r2 * X);

                    gA1 = Math.Pow(b, 2) + (Math.Pow(a, 2) * Math.Pow(r1, 2));
                    gB1 = (2 * Math.Pow(a, 2) * r1 * Rho1) - (2 * Math.Pow(a, 2) * Beta * r1) - (2 * Math.Pow(b, 2) * Alpha);
                    X1 = -gB1 / (2 * gA1);
                    Y1 = (r1 * X1) + Rho1;

                    ga2 = Math.Pow(b, 2) + (Math.Pow(a, 2) * Math.Pow(r2, 2));
                    gB2 = (2 * Math.Pow(a, 2) * r2 * Rho2) - (2 * Math.Pow(a, 2) * Beta * r2) - (2 * Math.Pow(b, 2) * Alpha);
                    X2 = -gB2 / (2 * ga2);
                    Y2 = (r2 * X2) + Rho2;

                    gA1p = Math.Pow(bp, 2) + (Math.Pow(ap, 2) * Math.Pow(r1, 2));
                    gB1p = (2 * Math.Pow(ap, 2) * r1 * Rho1) - (2 * Math.Pow(ap, 2) * Betap * r1) - (2 * Math.Pow(bp, 2) * Alphap);
                    X1p = -gB1p / (2 * gA1p);
                    Y1p = (r1 * X1p) + Rho1;

                    gA2p = Math.Pow(bp, 2) + (Math.Pow(ap, 2) * Math.Pow(r2, 2));
                    gB2p = (2 * Math.Pow(ap, 2) * r2 * Rho2) - (2 * Math.Pow(ap, 2) * Betap * r2) - (2 * Math.Pow(bp, 2) * Alphap);
                    X2p = -gB2p / (2 * gA2p);
                    Y2p = (r2 * X2p) + Rho2;
                }

                else
                {
                    //k = 1 
                    e = Math.Tan((90 * RADIANTFACTOR) + Gamma);
                    f = Beta - (e * Alpha);
                    gA = Math.Pow(b, 2) + (Math.Pow(a, 2) * Math.Pow(e, 2));
                    gB = ((2 * Math.Pow(a, 2) * e) * (f - Beta)) - (2 * Alpha * Math.Pow(b, 2));
                    gC = (Math.Pow(b, 2) * Math.Pow(Alpha, 2)) + (Math.Pow(a, 2) * Math.Pow((f - Beta), 2)) - (Math.Pow(a, 2) * Math.Pow(b, 2));
                    gDelta = Math.Pow(gB, 2) - (4 * gA * gC);

                    X1 = (-gB + Math.Sqrt(Math.Abs(gDelta))) / (2 * gA);
                    Y1 = (e * X1) + f;

                    X2 = (-gB - Math.Sqrt(Math.Abs(gDelta))) / (2 * gA);
                    Y2 = (e * X2) + f;

                    fp = Betap - (e * Alphap);
                    gAp = gA;
                    gBp = ((2 * Math.Pow(a, 2) * e) * (fp - Betap)) - (2 * Alphap * Math.Pow(b, 2));
                    gCp = (Math.Pow(b, 2) * Math.Pow(Alphap, 2)) + (Math.Pow(a, 2) * Math.Pow((fp - Betap), 2)) - (Math.Pow(a, 2) * Math.Pow(b, 2));
                    gDeltap = Math.Pow(gBp, 2) - (4 * gAp * gCp);

                    X1p = (-gBp + Math.Sqrt(Math.Abs(gDeltap))) / (2 * gAp);
                    Y1p = (e * X1p) + fp;

                    X2p = (-gBp - Math.Sqrt(Math.Abs(gDeltap))) / (2 * gAp);
                    Y2p = (e * X2p) + fp;

                }
            }
            else
            {
                if (Alpha == Alphap)
                {
                    X1 = Alpha - a;
                    Y1 = Beta;
                    X2 = Alphap - ap;
                    Y2 = Betap;
                    X1p = Alpha + a;
                    Y1p = Beta;
                    X2p = Alphap + ap;
                    Y2p = Betap;
                }
                else
                {
                    X1 = Alpha;
                    Y1 = Beta + b;
                    X2 = Alphap;
                    Y2 = Betap + bp;
                    X1p = Alpha;
                    Y1p = Beta - b;
                    X2p = Alphap;
                    Y2p = Betap - bp;
                }
            }
            ResultPoints.Add(new PointShape(X1, Y1));
            ResultPoints.Add(new PointShape(X1p, Y1p));
            ResultPoints.Add(new PointShape(X2p, Y2p));
            ResultPoints.Add(new PointShape(X2, Y2));

            return ResultPoints;
        }

        private bool ContainsPointShape(PointShape pointShape)
        {
            return ContainsPointShape(pointShape.X, pointShape.Y);
        }

        private bool ContainsPointShape(double pointX, double pointY)
        {
            double B;
            double C;
            bool result = false;

            double Aaxe = this.Width / 2;
            double Baxe = this.Height / 2;
            if (this.Width > this.Height)
            {
                B = -2 * this.Center.Y;
                C = Math.Pow(this.Center.Y, 2) + (Math.Pow(Baxe, 2) * Math.Pow((pointX - this.Center.X), 2) / Math.Pow(Aaxe, 2)) - Math.Pow(Baxe, 2);
                result = DeterminEllipseContainsPoint(pointY, B, C);
            }
            else
            {
                B = -2 * this.Center.X;
                C = Math.Pow(this.Center.X, 2) + (Math.Pow(Aaxe, 2) * Math.Pow((pointY - this.Center.Y), 2) / Math.Pow(Baxe, 2)) - Math.Pow(Aaxe, 2);
                result = DeterminEllipseContainsPoint(pointX, B, C);
            }

            return result;
        }

        private static bool DeterminEllipseContainsPoint(double p, double B, double C)
        {
            bool result = false;
            double delta = Math.Pow(B, 2) - (4 * C);
            if (delta < 0)
            {
                result = false;
            }
            if (delta >= 0)
            {
                double valueP = (-B + Math.Sqrt(delta)) / 2;
                double valueS = (-B - Math.Sqrt(delta)) / 2;
                if ((p > valueP && p < valueS) || (p >= valueS && p <= valueP))
                {
                    result = true;
                }
            }

            return result;
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

        internal PointShape GeShortestPointFromOutsidePoint(PointShape pointShape)
        {
            PointShape toPointShape = this.centerPoint;
            double yp;
            double ys;
            PointShape pointShapeIp = new PointShape();
            PointShape pointShapeIs = new PointShape();
            PointShape returnPointShape = new PointShape();
            double axeEllipseWidth = this.Width / 2;
            double axeEllipseHeight = this.Height / 2;
            double ap = (toPointShape.Y - pointShape.Y) / (toPointShape.X - pointShape.X);
            double bp = pointShape.Y - (ap * pointShape.X);
            double delta = 0;
            double A = Math.Pow(ap, 2) + (Math.Pow(axeEllipseHeight, 2) / Math.Pow(axeEllipseWidth, 2));
            double B = 0;
            double C = 0;

            if (pointShape.X != toPointShape.X)
            {
                B = (2 * ap * bp) - (2 * this.Center.Y * ap) - (2 * this.Center.X * (Math.Pow(axeEllipseHeight, 2) / Math.Pow(axeEllipseWidth, 2)));
                C = Math.Pow(this.Center.Y, 2) + Math.Pow(bp, 2);
                C = C + (Math.Pow(this.Center.X, 2) * (Math.Pow(axeEllipseHeight, 2) / Math.Pow(axeEllipseWidth, 2)));
                C = C - (2 * this.Center.Y * bp) - Math.Pow(axeEllipseHeight, 2);
                delta = Math.Pow(B, 2) - (4 * A * C);

                if (delta >= 0)
                {
                    double xp = (-B + Math.Sqrt(delta)) / (2 * A);
                    double xs = (-B - Math.Sqrt(delta)) / (2 * A);
                    if (((xp >= pointShape.X & xp <= toPointShape.X) | (xp <= pointShape.X & xp >= toPointShape.X)) | ((xs >= pointShape.X & xs <= toPointShape.X) | (xs <= pointShape.X & xs >= toPointShape.X)))
                    {
                        pointShapeIp.X = xp;
                        pointShapeIp.Y = ap * xp + bp;
                        pointShapeIs.X = xs;
                        pointShapeIs.Y = ap * xs + bp;

                        if (((xp >= pointShape.X & xp <= toPointShape.X) | (xp <= pointShape.X & xp >= toPointShape.X)) & ((xs > Math.Max(pointShape.X, toPointShape.X)) | (xs < Math.Min(pointShape.X, toPointShape.X))))
                        {
                            returnPointShape = pointShapeIp;
                        }

                        else if (((xs >= pointShape.X & xs <= toPointShape.X) | (xs <= pointShape.X & xs >= toPointShape.X)) & ((xp > Math.Max(pointShape.X, toPointShape.X)) | (xp < Math.Min(pointShape.X, toPointShape.X))))
                        {
                            returnPointShape = pointShapeIs;
                        }

                    }
                }
            }
            else
            {
                A = Math.Pow(axeEllipseWidth, 2);
                B = -2 * Math.Pow(axeEllipseWidth, 2) * this.Center.Y;
                C = (Math.Pow(axeEllipseWidth, 2) * Math.Pow(this.Center.Y, 2)) + (Math.Pow(axeEllipseHeight, 2) * Math.Pow((pointShape.X - this.Center.X), 2)) - (Math.Pow(axeEllipseWidth, 2) * Math.Pow(axeEllipseHeight, 2));
                delta = Math.Pow(B, 2) - (4 * A * C);
                if (delta >= 0)
                {
                    yp = (-B + Math.Sqrt(delta)) / (2 * A);
                    ys = (-B - Math.Sqrt(delta)) / (2 * A);
                    pointShapeIp.X = pointShape.X;
                    pointShapeIp.Y = yp;
                    pointShapeIs.X = pointShape.X;
                    pointShapeIs.Y = ys;
                    if (yp <= Math.Max(pointShape.Y, toPointShape.Y) & yp >= Math.Min(pointShape.Y, toPointShape.Y))
                    {
                        returnPointShape = pointShapeIp;
                    }
                    if (ys <= Math.Max(pointShape.Y, toPointShape.Y) & ys >= Math.Min(pointShape.Y, toPointShape.Y))
                    {
                        returnPointShape = pointShapeIs;
                    }
                }
            }

            return returnPointShape;
        }
    }
}
