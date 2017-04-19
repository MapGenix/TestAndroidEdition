using System;
using System.Collections;
using Mapgenix.Canvas.Properties;
using Mapgenix.Shapes;
using Mapgenix.Utils;

namespace Mapgenix.Canvas
{
    internal static class Validators
    {
        internal static void CheckProjectionIsOpen(bool isOpen)
        {
            if (!isOpen)
            {
                throw new InvalidOperationException(ExceptionDescription.ProjectionIsNotOpen);
            }
        }

        internal static void CheckFeatureIsValid(Feature feature)
        {
            if (!feature.IsValid())
            {
                throw new ArgumentException(ExceptionDescription.FeatureIsNotValid);
            }
        }


        internal static void CheckParameterIsNotNull(object objectToTest, string parameterName)
        {
            if (objectToTest == null)
            {
                var exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }

        public static void CheckPointTypeIsValid(PointType pointType, string parameterName)
        {
            switch (pointType)
            {
                case PointType.Symbol:
                    break;
                case PointType.Bitmap:
                    break;
                case PointType.Character:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckImageInPointStyleIsNotNull(GeoImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException("image", ExceptionDescription.ImageInPointStyleCanNotBeNull);
            }
        }

        internal static void CheckPointSymbolTypeIsValid(PointSymbolType symbolType, string parameterName)
        {
            switch (symbolType)
            {
                case PointSymbolType.Circle:
                    break;
                case PointSymbolType.Square:
                    break;
                case PointSymbolType.Triangle:
                    break;
                case PointSymbolType.Cross:
                    break;
                case PointSymbolType.Diamond:
                    break;
                case PointSymbolType.Diamond2:
                    break;
                case PointSymbolType.Star:
                    break;
                case PointSymbolType.Star2:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void DefaultAndCustomStyleDuplicateForZoomLevel(ZoomLevel zoomLevel)
        {
            if (zoomLevel.IsActive)
            {
                var isDefault =
                    !(!zoomLevel.DefaultAreaStyle.IsDefault || !zoomLevel.DefaultLineStyle.IsDefault ||
                      !zoomLevel.DefaultPointStyle.IsDefault || !zoomLevel.DefaultTextStyle.IsDefault);
                if (!isDefault && zoomLevel.CustomStyles.Count != 0)
                {
                    throw new ArgumentException(ExceptionDescription.DefaultAndCustomStyleDuplicate);
                }
            }
        }

        internal static void CheckGeoCanvasIsInDrawing(bool isDrawing)
        {
            if (!isDrawing)
            {
                throw new InvalidOperationException(ExceptionDescription.GeocanvasIsNotInDrawing);
            }
        }


        internal static void CheckParameterIsNull(GeoPen pen, string parameterName)
        {
            if (pen == null)
            {
                var exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }

        internal static void CheckScaleIsBiggerThanZero(double imageScale, string parameterName)
        {
            if (imageScale <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.ImageScaleShouldBiggerThanZero);
            }
        }

        internal static void CheckValueIsBiggerThanZero(double value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName,
                    ExceptionDescription.TheValueShouldBeGreaterThanZero);
            }
        }


        internal static void CheckShapeIsValidForOperation(BaseShape basheShapeToTest)
        {
            var validationResult = basheShapeToTest.Validate(ShapeValidationMode.Simple);
            if (!validationResult.IsValid)
            {
                var exceptionDescription = ExceptionDescription.ShapeIsInvalidForOperation;
                throw new InvalidOperationException(exceptionDescription + validationResult.ValidationErrors);
            }
        }


        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue,
            RangeCheckingInclusion includeMinValue, double maxValue, RangeCheckingInclusion includeMaxValue)
        {
            var bResult = false;
            if ((inputValue > minValue)
                || (inputValue == minValue && includeMinValue == RangeCheckingInclusion.IncludeValue))
            {
                if ((inputValue < maxValue)
                    || (inputValue == maxValue && includeMaxValue == RangeCheckingInclusion.IncludeValue))
                {
                    bResult = true;
                }
            }

            if (!bResult)
            {
                var errorDescription = ExceptionDescription.DoubleOutOfRange;
                throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue,
            double maxValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, RangeCheckingInclusion.IncludeValue,
                maxValue, RangeCheckingInclusion.IncludeValue);
        }

        internal static void CheckIfInputValueIsBiggerThan(double inputValue, string parameterName, double minValue,
            RangeCheckingInclusion includeMinValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, includeMinValue, double.MaxValue,
                RangeCheckingInclusion.IncludeValue);
        }

        internal static void CheckIfInputValueIsSmallerThan(double inputValue, string parameterName, double maxValue,
            RangeCheckingInclusion includeMaxValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, double.MinValue, RangeCheckingInclusion.IncludeValue,
                maxValue, includeMaxValue);
        }


        internal static void CheckParameterIsNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, ExceptionDescription.ParameterIsNull);
            }
            if (value.Trim() == string.Empty)
            {
                throw new ArgumentException(ExceptionDescription.ParameterIsEmpty, parameterName);
            }
        }


        internal static void CheckMapEngineExtentIsValid(RectangleShape worldExtent, string parameterName)
        {
            if (worldExtent == null)
            {
                throw new ArgumentNullException(ExceptionDescription.WorldExtentIsNotValid, parameterName);
            }
            var result = worldExtent.Validate(ShapeValidationMode.Simple);
            if (!result.IsValid)
            {
                throw new InvalidOperationException(ExceptionDescription.WorldExtentIsNotValid);
            }

            if (worldExtent.Width == 0 || worldExtent.Height == 0)
            {
                throw new InvalidOperationException(ExceptionDescription.CurrentExtentNotAssigned);
            }
        }

        internal static void CheckParameterIsNotBothNull(object firstObject, object secondObject,
            string firstParameterName, string secondParameterName)
        {
            if (firstObject == null && secondObject == null)
            {
                var exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(firstParameterName + " and " + secondParameterName, exceptionDescription);
            }
        }


        internal static void CheckDistanceUnitIsValid(DistanceUnit distanceUnit, string parameterName)
        {
            switch (distanceUnit)
            {
                case DistanceUnit.Feet:
                    break;
                case DistanceUnit.Kilometer:
                    break;
                case DistanceUnit.Meter:
                    break;
                case DistanceUnit.Mile:
                    break;
                case DistanceUnit.UsSurveyFeet:
                    break;
                case DistanceUnit.Yard:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckDrawingLineCapIsValid(DrawingLineCap drawingLineCap, string parameterName)
        {
            switch (drawingLineCap)
            {
                case DrawingLineCap.Round:
                    break;
                case DrawingLineCap.AnchorMask:
                    break;
                case DrawingLineCap.ArrowAnchor:
                    break;
                case DrawingLineCap.Custom:
                    break;
                case DrawingLineCap.DiamondAnchor:
                    break;
                case DrawingLineCap.Flat:
                    break;
                case DrawingLineCap.NoAnchor:
                    break;
                case DrawingLineCap.RoundAnchor:
                    break;
                case DrawingLineCap.Square:
                    break;
                case DrawingLineCap.SquareAnchor:
                    break;
                case DrawingLineCap.Triangle:
                    break;

                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckGeoDashCapIsValid(GeoDashCap geoDashCap, string parameterName)
        {
            switch (geoDashCap)
            {
                case GeoDashCap.Flat:
                    break;
                case GeoDashCap.Round:
                    break;
                case GeoDashCap.Triangle:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckDrawingLineJoinIsValid(DrawingLineJoin drawingLineJoin, string parameterName)
        {
            switch (drawingLineJoin)
            {
                case DrawingLineJoin.Bevel:
                    break;
                case DrawingLineJoin.Miter:
                    break;
                case DrawingLineJoin.MiterClipped:
                    break;
                case DrawingLineJoin.Round:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void ChecklineDashStyleIsValid(LineDashStyle lineDashStyle, string parameterName)
        {
            switch (lineDashStyle)
            {
                case LineDashStyle.Custom:
                    break;
                case LineDashStyle.Dash:
                    break;
                case LineDashStyle.DashDot:
                    break;
                case LineDashStyle.DashDotDot:
                    break;
                case LineDashStyle.Dot:
                    break;
                case LineDashStyle.Solid:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckDrawingLevelIsValid(DrawingLevel drawingLevel, string parameterName)
        {
            switch (drawingLevel)
            {
                case DrawingLevel.LevelFour:
                    break;
                case DrawingLevel.LevelOne:
                    break;
                case DrawingLevel.LevelThree:
                    break;
                case DrawingLevel.LevelTwo:
                    break;
                case DrawingLevel.LabelLevel:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckWkbIsValid(byte[] wkb, string parameterName)
        {
            var isValid = false;

            if (wkb.Length > 0)
            {
                if ((wkb[0] == 1 || wkb[0] == 0))
                {
                    if ((wkb[2] == 0 && wkb[3] == 0 && ((wkb[1] == 0 && wkb[4] != 0) || (wkb[1] != 0 && wkb[4] == 0))))
                    {
                        var shapeType = wkb[1] + wkb[4];
                        if (shapeType >= 1 && shapeType <= 7)
                        {
                            isValid = true;
                        }
                    }
                }
            }
            else
            {
                isValid = true;
            }

            if (!isValid)
            {
                var errorDescription = ExceptionDescription.WkbIsInvalid;
                throw new ArgumentException(errorDescription, parameterName);
            }
        }

        internal static void CheckHtmlColorIsValid(string htmlColor, string parameterName)
        {
            if ((htmlColor == null) || (htmlColor.Length == 0))
            {
                var errorDescription = ExceptionDescription.HtmlColorIsInvalid;
                throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckShapeIsAreaBaseShape(BaseShape baseShape)
        {
            if (!(baseShape is BaseAreaShape))
            {
                throw new ArgumentException(ExceptionDescription.TargetShapeIsNotValidType);
            }
        }


        internal static void CheckShapeIsPointShape(BaseShape baseShape)
        {
            if (!(baseShape is PointShape))
            {
                throw new ArgumentException(ExceptionDescription.TargetShapeIsNotValidType);
            }
        }


        internal static void CheckIsOpenedWhenCloneDeep(bool isOpenOrDrawung)
        {
            if (isOpenOrDrawung)
            {
                throw new InvalidOperationException(ExceptionDescription.CheckIsOpenedWhenCloneDeep);
            }
        }

        internal static void CheckGeographyUnitIsValid(GeographyUnit geographyUnit, string parameterName)
        {
            switch (geographyUnit)
            {
                case GeographyUnit.DecimalDegree:
                    break;
                case GeographyUnit.Feet:
                    break;
                case GeographyUnit.Meter:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckAreaSimplificationTypeIsValid(SimplificationType simplificationType,
            string parameterName)
        {
            switch (simplificationType)
            {
                case SimplificationType.DouglasPeucker:
                    break;
                case SimplificationType.TopologyPreserving:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckPanDirectionIsValid(PanDirection panDirection, string parameterName)
        {
            switch (panDirection)
            {
                case PanDirection.Up:
                    break;
                case PanDirection.UpperRight:
                    break;
                case PanDirection.Right:
                    break;
                case PanDirection.LowerRight:
                    break;
                case PanDirection.Down:
                    break;
                case PanDirection.LowerLeft:
                    break;
                case PanDirection.Left:
                    break;
                case PanDirection.UpperLeft:
                    break;
                default:
                    var errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }


        internal static void CheckArrayItemsBigerThan0(IEnumerable objects, string argumentName)
        {
            var i = 0;

            foreach (var obj in objects)
            {
                i++;
                break;
            }
            if (i == 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, "Array items should bigger than 0.");
            }
        }


        internal static void CheckRandomColorTypeIsValid(RandomColorType colorType, string parameterName)
        {
            switch (colorType)
            {
                case RandomColorType.All:
                    break;
                case RandomColorType.Bright:
                    break;
                case RandomColorType.Pastel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.EnumerationOutOfRange);
            }
        }

        internal static void CheckNumberIsByte(int number, string paramterName)
        {
            if (number < 0 || number > 255)
            {
                throw new ArgumentOutOfRangeException(paramterName, ExceptionDescription.NumberShouldBeByte);
            }
        }
    }
}