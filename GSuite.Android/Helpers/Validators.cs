using System;
using System.Globalization;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using Mapgenix.GSuite.Android.Properties;

namespace Mapgenix.GSuite.Android
{
    internal static class Validators
    {
        internal static void CheckParameterIsNotNull(Object objectToTest, string parameterName)
        {
            if (objectToTest == null)
            {
                string exceptionDescription = ExceptionMessage.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }

        internal static void CheckParametersAreNotBothNull(Object object1, Object object2, string parameterName1, string parameterName2)
        {
            if (object1 == null && object2 == null)
            {
                string exceptionDescription = ExceptionMessage.ParametersCannotBeBothNull;
                throw new ArgumentNullException(String.Format(CultureInfo.InvariantCulture, "{0} or {1}", parameterName1, parameterName2), exceptionDescription);
            }
        }

        internal static void CheckValueIsGreaterOrEqualToZero(double value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionMessage.TheValueShouldBeGreaterOrEqualToZero);
            }
        }

        internal static void CheckValueIsGreaterThanZero(double value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionMessage.TheValueShouldBeGreaterThanZero);
            }
        }

        internal static void CheckMapUnitIsValid(GeographyUnit mapUnit)
        {
            switch (mapUnit)
            {
                case GeographyUnit.DecimalDegree: break;
                case GeographyUnit.Feet: break;
                case GeographyUnit.Meter: break;
                default: throw new ArgumentException(ExceptionMessage.MapUnitIsInvalid, "mapUnit");
            }
        }

        internal static void CheckMapUnitIsMeter(GeographyUnit mapUnit)
        {
            if (mapUnit != GeographyUnit.Meter)
            {
                throw new ArgumentException("Map Unit is not meter.", "mapUnit");
            }
        }


        internal static void CheckValueIsBiggerThanOrEqualToZero(double value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionMessage.TheValueShouldBeGreaterOrEqualToZero);
            }
        }

      

        internal static void CheckGeoCanvasIsInDrawing(bool isDrawing)
        {
            if (!isDrawing)
            {
                throw new InvalidOperationException(ExceptionMessage.GeocanvasIsNotInDrawing);
            }
        }

        internal static void CheckParameterIsNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, ExceptionMessage.ParameterIsNull);
            }
            else if (String.IsNullOrEmpty(value.Trim()))
            {
                throw new ArgumentException(ExceptionMessage.ParameterIsEmpty, parameterName);
            }
        }

        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue, RangeCheckingInclusion includeMinValue, double maxValue, RangeCheckingInclusion includeMaxValue)
        {
            bool bResult = false;
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
                throw new ArgumentOutOfRangeException(parameterName, ExceptionMessage.DoubleOutOfRange);
            }
        }

        
        internal static void CheckScaleIsValid(double scale, string parameterName)
        {
            if (scale < 1)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionMessage.ScaleIsOutOfRange);
            }
        }

        internal static void CheckTileSizeIsValid(int tileSize, string parameterName)
        {
            CheckValueIsGreaterThanZero(tileSize, parameterName);
        }

        internal static void CheckMapEngineExtentIsValid(RectangleShape worldExtent, string parameterName)
        {
            if (worldExtent == null)
            {
                throw new ArgumentNullException(ExceptionMessage.WorldExtentIsNotValid, parameterName);
            }
            var result = worldExtent.Validate(ShapeValidationMode.Simple);
            if (!result.IsValid)
            {
                throw new InvalidOperationException(ExceptionMessage.WorldExtentIsNotValid);
            }

            if (worldExtent.Width == 0 || worldExtent.Height == 0)
            {
                throw new InvalidOperationException(ExceptionMessage.CurrentExtentNotAssigned);
            }
        }

        internal static void CheckOpenStreetMapUnit(GeographyUnit fromUnit, string parameterName)
        {
            if (fromUnit != GeographyUnit.Meter)
            {
                throw new ArgumentException(ExceptionMessage.GeographicUnitNotValidWithGoogle, parameterName);
            }
        }

    }
}
