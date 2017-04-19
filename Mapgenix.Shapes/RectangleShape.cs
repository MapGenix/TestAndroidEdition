using System;
using System.Collections.Generic;
using System.Globalization;
using Mapgenix.Utils;

namespace Mapgenix.Shapes
{
    /// <summary>
    /// Shape represented as a rectangle, which is defined as an upper left point and a
    /// lower right point.
    /// </summary>
    [Serializable]
    public class RectangleShape : BaseAreaShape
    {
        private PointShape upperLeftPoint;
        private PointShape lowerRightPoint;

        protected static double tolerance = 0.00001;

       
        public RectangleShape()
            : this(0, 0, 0, 0)
        {
        }

       
        public RectangleShape(PointShape upperLeftPoint, PointShape lowerRightPoint)
        {
            Validators.CheckParameterIsNotNull(upperLeftPoint, "upperLeftPoint");
            Validators.CheckParameterIsNotNull(lowerRightPoint, "lowerRightPoint");

            Validators.CheckIfInputValueIsBiggerThan(lowerRightPoint.X, "maxX", upperLeftPoint.X, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(upperLeftPoint.Y, "maxY", lowerRightPoint.Y, RangeCheckingInclusion.IncludeValue);

            this.upperLeftPoint = new PointShape(upperLeftPoint.X, upperLeftPoint.Y);
            this.lowerRightPoint = new PointShape(lowerRightPoint.X, lowerRightPoint.Y);

            if (this.upperLeftPoint.X == this.lowerRightPoint.X)
            {
                this.upperLeftPoint.X -= tolerance / 2;
                this.lowerRightPoint.X += tolerance / 2;
            }
            if (this.upperLeftPoint.Y == this.lowerRightPoint.Y)
            {
                this.upperLeftPoint.Y += tolerance / 2;
                this.lowerRightPoint.Y -= tolerance / 2;
            }
          
        }

       
        public RectangleShape(double minX, double maxY, double maxX, double minY)
        {
            Validators.CheckIfInputValueIsBiggerThan(maxX, "maxX", minX, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsBiggerThan(maxY, "maxY", minY, RangeCheckingInclusion.IncludeValue);

            upperLeftPoint = new PointShape(minX, maxY);
            lowerRightPoint = new PointShape(maxX, minY);

            if (upperLeftPoint.X == lowerRightPoint.X)
            {
                upperLeftPoint.X -= tolerance / 2;
                lowerRightPoint.X += tolerance / 2;
            }
            if (upperLeftPoint.Y == lowerRightPoint.Y)
            {
                upperLeftPoint.Y += tolerance / 2;
                lowerRightPoint.Y -= tolerance / 2;
            }
        }

        public RectangleShape(string wellKnownText)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            LoadFromWellKnownData(wellKnownText);
        }

     
        public RectangleShape(byte[] wellKnownBinary)
            : this()
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            LoadFromWellKnownData(wellKnownBinary);
        }

        private RectangleShapeRelationship CheckContains(RectangleShape targetShape)
        {
            RectangleShapeRelationship result = RectangleShapeRelationship.RectangleIntersect;

            bool validWidth = targetShape.Width <= this.Width;
            bool validHeight = targetShape.Height <= this.Height;
            double targetCenterX = (targetShape.UpperLeftPoint.X + targetShape.LowerRightPoint.X) / 2;
            double targetCenterY = (targetShape.UpperLeftPoint.Y + targetShape.LowerRightPoint.Y) / 2;
            double sourceCenterX = (this.UpperLeftPoint.X + this.LowerRightPoint.X) / 2;
            double sourceCenterY = (this.UpperLeftPoint.Y + this.LowerRightPoint.Y) / 2;
            double distanceX = Math.Abs(targetCenterX - sourceCenterX);
            double distanceY = Math.Abs(targetCenterY - sourceCenterY);

            if (validWidth && validHeight)
            {
                double minDistanceWidth = Math.Abs((targetShape.Width - this.Width) / 2);
                double minDistanceHeight = Math.Abs((targetShape.Height - this.Height) / 2);
                if (distanceX <= minDistanceWidth && distanceY <= minDistanceHeight)
                {
                    result = RectangleShapeRelationship.RectangleContaining;
                }
            }
            else if (!validWidth && !validHeight)
            {
                double minDistanceWidth = Math.Abs((targetShape.Width - this.Width) / 2);
                double minDistanceHeight = Math.Abs((targetShape.Height - this.Height) / 2);
                if (distanceX <= minDistanceWidth && distanceY <= minDistanceHeight)
                {
                    result = RectangleShapeRelationship.RectangleContained;
                }
            }
            else
            {
                double maxDistanceWidth = Math.Abs((targetShape.Width + this.Width) / 2);
                double maxDistanceHeight = Math.Abs((targetShape.Height + this.Height) / 2);
                if (distanceX >= maxDistanceWidth || distanceY >= maxDistanceHeight)
                {
                    return RectangleShapeRelationship.RectangleApartFrom;
                }
            }

            return result;
        }

        private RectangleShape GetIntersectionShape(RectangleShape targetShape)
        {
            double resultUpperLeftX = double.MaxValue;
            double resultUpperLeftY = double.MinValue;
            double resultLowerRightX = double.MinValue;
            double resultLowerRightY = double.MaxValue;

            double targetUpperLeftX = targetShape.UpperLeftPoint.X;
            double targetUpperLeftY = targetShape.UpperLeftPoint.Y;
            double targetLowerRightX = targetShape.LowerRightPoint.X;
            double targetLowerRightY = targetShape.LowerRightPoint.Y;

            double sourceUpperLeftX = this.UpperLeftPoint.X;
            double sourceUpperLeftY = this.UpperLeftPoint.Y;
            double sourceLowerRightX = this.LowerRightPoint.X;
            double sourceLowerRightY = this.LowerRightPoint.Y;

            resultUpperLeftX = (targetUpperLeftX > sourceUpperLeftX && targetUpperLeftX < sourceLowerRightX) ? targetUpperLeftX : sourceUpperLeftX;
            resultUpperLeftY = (targetUpperLeftY > sourceLowerRightY && targetUpperLeftY < sourceUpperLeftY) ? targetUpperLeftY : sourceUpperLeftY;
            resultLowerRightX = (targetLowerRightX > sourceUpperLeftX && targetLowerRightX < sourceLowerRightX) ? targetLowerRightX : sourceLowerRightX;
            resultLowerRightY = (targetLowerRightY > sourceLowerRightY && targetLowerRightY < sourceUpperLeftY) ? targetLowerRightY : sourceLowerRightY;

            RectangleShape result = new RectangleShape(resultUpperLeftX, resultUpperLeftY, resultLowerRightX, resultLowerRightY);
            return result;
        }

     
        public RectangleShape GetIntersection(RectangleShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckParameterIsValid(targetShape, "targetShape");

            RectangleShapeRelationship result = this.CheckContains(targetShape);

            switch (result)
            {
                case RectangleShapeRelationship.RectangleApartFrom:
                    return new RectangleShape();
                case RectangleShapeRelationship.RectangleContaining:
                    return (RectangleShape)targetShape.CloneDeep();
                case RectangleShapeRelationship.RectangleContained:
                    return (RectangleShape)this.CloneDeep();
                default:
                    return this.GetIntersectionShape(targetShape);
            }
        }


        protected override WellKnownType GetWellKnownTypeCore()
        {
            return WellKnownType.Polygon;
        }

       
        public PointShape UpperLeftPoint
        {
            get
            {
                return upperLeftPoint;
            }
        }

     
        public PointShape UpperRightPoint
        {
            get
            {
                return new PointShape(lowerRightPoint.X, upperLeftPoint.Y);
            }
        }

        public PointShape LowerRightPoint
        {
            get
            {
                return lowerRightPoint;
            }
        }

      
        public PointShape LowerLeftPoint
        {
            get
            {
                return new PointShape(upperLeftPoint.X, lowerRightPoint.Y);
            }
        }
     
        public double Height
        {
            get
            {
                return upperLeftPoint.Y - lowerRightPoint.Y;
            }
        }

      
        public double Width
        {
            get
            {
                return lowerRightPoint.X - upperLeftPoint.X;
            }
        }

     
        protected override BaseShape CloneDeepCore()
        {
            RectangleShape rectangle = new RectangleShape();
            rectangle.upperLeftPoint = (PointShape)this.upperLeftPoint.CloneDeep();
            rectangle.lowerRightPoint = (PointShape)this.lowerRightPoint.CloneDeep();

            return rectangle;
        }

        public PolygonShape ToPolygon()
        {
            Validators.CheckShapeIsValidForOperation(this);

            PolygonShape polygon = new PolygonShape();
            polygon.OuterRing.Vertices.Add(new Vertex(upperLeftPoint.X, upperLeftPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(lowerRightPoint.X, upperLeftPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(lowerRightPoint.X, lowerRightPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(upperLeftPoint.X, lowerRightPoint.Y));
            polygon.OuterRing.Vertices.Add(new Vertex(upperLeftPoint.X, upperLeftPoint.Y));

            return polygon;
        }

        private bool IsDisjointed(RectangleShape targetShape)
        {
            bool isDisjointed = false;

            if (upperLeftPoint.X > targetShape.lowerRightPoint.X ||
                lowerRightPoint.X < targetShape.upperLeftPoint.X ||
                lowerRightPoint.Y > targetShape.upperLeftPoint.Y ||
                upperLeftPoint.Y < targetShape.lowerRightPoint.Y)
            {
                isDisjointed = true;
            }

            return isDisjointed;
        }

        public void ExpandToInclude(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            ExpandToIncludeCore(targetShape);
        }

        public void ExpandToInclude(Feature targetFeature)
        {
            Validators.CheckFeatureIsValid(targetFeature);

            ExpandToIncludeCore(targetFeature.GetShape());
        }

       public void ExpandToInclude(IEnumerable<BaseShape> targetShapes)
        {
            Validators.CheckParameterIsNotNull(targetShapes, "targetShapes");
            Validators.CheckShapeIsValidForOperation(this);

            foreach (BaseShape targetShape in targetShapes)
            {
                ExpandToIncludeCore(targetShape);
            }
        }

        public void ExpandToInclude(IEnumerable<Feature> targetFeatures)
        {
            Validators.CheckParameterIsNotNull(targetFeatures, "targetFeatures");
            Validators.CheckShapeIsValidForOperation(this);

            foreach (Feature targetFeature in targetFeatures)
            {
                ExpandToIncludeCore(targetFeature.GetShape());
            }
        }

        protected override PointShape GetCenterPointCore()
        {
            PointShape pointShape = new PointShape((upperLeftPoint.X + lowerRightPoint.X) / 2, (upperLeftPoint.Y + lowerRightPoint.Y) / 2);
            return pointShape;
        }

       protected virtual void ExpandToIncludeCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            RectangleShape boundingBox = targetShape.GetBoundingBox();

            if (upperLeftPoint.X > boundingBox.upperLeftPoint.X) { upperLeftPoint.X = boundingBox.upperLeftPoint.X; }
            if (upperLeftPoint.Y < boundingBox.upperLeftPoint.Y) { upperLeftPoint.Y = boundingBox.upperLeftPoint.Y; }
            if (lowerRightPoint.X < boundingBox.lowerRightPoint.X) { lowerRightPoint.X = boundingBox.lowerRightPoint.X; }
            if (lowerRightPoint.Y > boundingBox.lowerRightPoint.Y) { lowerRightPoint.Y = boundingBox.lowerRightPoint.Y; }
        }

       
        protected override double GetPerimeterCore(GeographyUnit shapeUnit, DistanceUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetPerimeter(shapeUnit, returningUnit);
        }

      
        protected override double GetAreaCore(GeographyUnit shapeUnit, AreaUnit returningUnit)
        {
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetArea(shapeUnit, returningUnit);
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
            Validators.CheckIfInputValueIsInRange(percentage, "percentage", 0, RangeCheckingInclusion.ExcludeValue, 100, RangeCheckingInclusion.ExcludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            double factor = 1.0 - percentage / 100;
            Scale(factor);
        }

       protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return new RectangleShape(upperLeftPoint, lowerRightPoint);
        }

       protected override BaseShape RegisterCore(PointShape fromPoint, PointShape toPoint, DistanceUnit fromUnit, GeographyUnit toUnit)
        {
            Validators.CheckGeographyUnitIsValid(toUnit, "toUnit");
            Validators.CheckShapeIsValidForOperation(this);

            RectangleShape rectangle = new RectangleShape();

            rectangle.upperLeftPoint = (PointShape)upperLeftPoint.Register(fromPoint, toPoint, fromUnit, toUnit);
            rectangle.lowerRightPoint = (PointShape)lowerRightPoint.Register(fromPoint, toPoint, fromUnit, toUnit);

            return rectangle;
        }

       protected override void TranslateByOffsetCore(double xOffsetDistance, double yOffsetDistance, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckShapeIsValidForOperation(this);
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");

            double differentX = 0;
            double differentY = 0;

            if (shapeUnit != GeographyUnit.DecimalDegree)
            {
                DistanceUnit distanceUnitOfThisShape = Conversion.ConvertGeographyUnitToDistanceUnit(shapeUnit);
                differentX = Conversion.ConvertMeasureUnits(xOffsetDistance, distanceUnitOfThisShape, distanceUnit);
                differentY = Conversion.ConvertMeasureUnits(yOffsetDistance, distanceUnitOfThisShape, distanceUnit);
            }
            else
            {
                differentX = DecimalDegreesHelper.GetLongitudeDifferenceFromDistance(xOffsetDistance, distanceUnit, this.upperLeftPoint.Y);
                differentY = DecimalDegreesHelper.GetLatitudeDifferenceFromDistance(yOffsetDistance, distanceUnit, this.upperLeftPoint.X);
            }

            upperLeftPoint.X += differentX;
            upperLeftPoint.Y += differentY;

            lowerRightPoint.X += differentX;
            lowerRightPoint.Y += differentY;
        }

      
        protected override void TranslateByDegreeCore(double distance, double angleInDegrees, GeographyUnit shapeUnit, DistanceUnit distanceUnit)
        {
            Validators.CheckIfInputValueIsBiggerThan(distance, "distance", 0, RangeCheckingInclusion.IncludeValue);
            Validators.CheckIfInputValueIsInRange(angleInDegrees, "distance", 0, RangeCheckingInclusion.IncludeValue, 360, RangeCheckingInclusion.IncludeValue);
            Validators.CheckShapeIsValidForOperation(this);

            upperLeftPoint.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
            lowerRightPoint.TranslateByDegree(distance, angleInDegrees, shapeUnit, distanceUnit);
        }

        protected override PointShape GetClosestPointToCore(BaseShape targetShape, GeographyUnit shapeUnit)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckParameterIsValid(targetShape, "targetShape");
            Validators.CheckGeographyUnitIsValid(shapeUnit, "shapeUnit");
            Validators.CheckShapeIsValidForOperation(this);

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

        protected override string GetWellKnownTextCore()
        {
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetWellKnownText();
        }

        protected override byte[] GetWellKnownBinaryCore(WkbByteOrder byteOrder)
        {
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetWellKnownBinary(byteOrder);
        }

        protected override void LoadFromWellKnownDataCore(string wellKnownText)
        {
            Validators.CheckParameterIsNotNull(wellKnownText, "wellKnownText");
            Validators.CheckWktStringIsValid(wellKnownText, "wellKnownText");

            BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wellKnownText);
            RectangleShape sourceRectangle = baseShape.GetBoundingBox();
            CloneOneRectangleToAnother(sourceRectangle, this);
        }

       protected override void LoadFromWellKnownDataCore(byte[] wellKnownBinary)
        {
            Validators.CheckParameterIsNotNull(wellKnownBinary, "wellKnownBinary");
            Validators.CheckWkbIsValid(wellKnownBinary, "wellKnownBinary");

            BaseShape baseShape = BaseShape.CreateShapeFromWellKnownData(wellKnownBinary);
            RectangleShape sourceRectangle = baseShape.GetBoundingBox();
            CloneOneRectangleToAnother(sourceRectangle, this);
        }

        protected override bool IsDisjointedCore(BaseShape targetShape)
        {
            bool isDisjointed = true;

            RectangleShape targetRectangle = targetShape as RectangleShape;

            if (targetRectangle != null)
            {
                isDisjointed = IsDisjointed(targetRectangle);
            }
            else
            {
                isDisjointed = base.IsDisjointedCore(targetShape);
            }

            return isDisjointed;
        }

        private static void CloneOneRectangleToAnother(RectangleShape fromRectangle, RectangleShape toRectangle)
        {

            toRectangle.upperLeftPoint = fromRectangle.upperLeftPoint;
            toRectangle.lowerRightPoint = fromRectangle.lowerRightPoint;
        }

        protected override ShapeValidationResult ValidateCore(ShapeValidationMode validationMode)
        {
            ShapeValidationResult validateResult = new ShapeValidationResult(true, String.Empty);

            switch (validationMode)
            {
                case ShapeValidationMode.Simple:
                    if (!(upperLeftPoint.X <= lowerRightPoint.X && upperLeftPoint.Y >= lowerRightPoint.Y))
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

        protected override MultipointShape GetCrossingCore(BaseShape targetShape)
        {
            Validators.CheckParameterIsNotNull(targetShape, "targetShape");
            Validators.CheckShapeIsValidForOperation(this);

            return ToPolygon().GetCrossing(targetShape);
        }

        protected override bool ContainsCore(BaseShape targetShape)
        {
            bool result = false;
            RectangleShape rectangleTargetShape = targetShape as RectangleShape;
            if (rectangleTargetShape == null)
            {
                result = base.ContainsCore(targetShape);
            }
            else
            {
                if (this.LowerLeftPoint.X <= rectangleTargetShape.LowerLeftPoint.X
                    && this.LowerLeftPoint.Y <= rectangleTargetShape.LowerLeftPoint.Y
                    && this.UpperRightPoint.X >= rectangleTargetShape.UpperRightPoint.X
                    && this.UpperRightPoint.Y >= rectangleTargetShape.UpperRightPoint.Y)
                {
                    result = true;
                }
            }
            return result;
        }

       public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", UpperLeftPoint.X, UpperLeftPoint.Y, LowerRightPoint.X, LowerRightPoint.Y);
        }

        private void Scale(double multiplicator)
        {
            double totalX = (upperLeftPoint.X + lowerRightPoint.X) * 2;
            double totalY = (upperLeftPoint.Y + lowerRightPoint.Y) * 2;

            double alpha = totalX / 4;
            double beta = totalY / 4;

            upperLeftPoint.X = ((upperLeftPoint.X - alpha) * multiplicator) + alpha;
            upperLeftPoint.Y = ((upperLeftPoint.Y - beta) * multiplicator) + beta;
            lowerRightPoint.X = ((lowerRightPoint.X - alpha) * multiplicator) + alpha;
            lowerRightPoint.Y = ((lowerRightPoint.Y - beta) * multiplicator) + beta;
        }
    }

     enum RectangleShapeRelationship
    {
        RectangleApartFrom = 0,
        RectangleContaining = 1, 
        RectangleContained = 2, 
        RectangleIntersect = 3
    }
}
