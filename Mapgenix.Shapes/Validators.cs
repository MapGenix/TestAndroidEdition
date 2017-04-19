using System;
using Mapgenix.Utils;
using NetTopologySuite.IO;

namespace Mapgenix.Shapes
{
    internal static class Validators
    {
        internal static void CheckParameterIsNotNull(Object objectToTest, string parameterName)
        {
           
            if (objectToTest == null)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
            }
        }


        internal static void CheckValueIsBiggerThanZero(double value, string parameterName)
        {
          
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.TheValueShouldBeGreaterThanZero);
            }
        }

  
        internal static void CheckWktStringIsValid(string wkt, string parameterName)
        {
            WKTReader wr = new WKTReader();
            try
            {
                wr.Read(wkt);
            }
            catch (Exception ex)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsInvalid;
                throw new ArgumentException(parameterName, exceptionDescription, ex);
            }
        }

        internal static void CheckShapeIsValidForOperation(BaseShape BasheShapeToTest)
        {
            ShapeValidationResult validationResult = BasheShapeToTest.Validate(ShapeValidationMode.Simple);
            if (!validationResult.IsValid)
            {
                string exceptionDescription = ExceptionDescription.ShapeIsInvalidForOperation;
                throw new InvalidOperationException(exceptionDescription + validationResult.ValidationErrors);
            }
        }

        internal static void CheckParameterIsValid(BaseShape BasheShapeToTest, string parameterName)
        {
            ShapeValidationResult validationResult = BasheShapeToTest.Validate(ShapeValidationMode.Simple);
            if (!validationResult.IsValid)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsInvalid;
                throw new ArgumentException(exceptionDescription + validationResult.ValidationErrors, parameterName);
            }
        }

       
        internal static void CheckIfTypeIsCorrect(object objectToTest, Type type, string operationName)
        {
            if (objectToTest.GetType() != type)
            {
                string errorDescription = ExceptionDescription.ReturnTypeNotCorrect;
                throw new InvalidCastException(errorDescription + operationName);
            }
        }

        internal static void CheckDecimalDegreeLatitudeIsInRange(double latitude, string parameterName)
        {
            if (Math.Round((double)latitude, 4) > 90 || Math.Round((double)latitude, 4) < -90)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.DecimalDegreeLatitudeOutOfRange);
            }
        }

        internal static void CheckDecimalDegreeLongitudeIsInRange(double longitude, string parameterName)
        {
            if (Math.Round((double)longitude, 4) > 180 || Math.Round((double)longitude, 4) < -180)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.DecimalDegreeLongitudeOutOfRange);
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
                string ErrorDescription = ExceptionDescription.DoubleOutOfRange;
                throw new ArgumentOutOfRangeException(parameterName, ErrorDescription);
            }
        }

        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue, double maxValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, RangeCheckingInclusion.IncludeValue, maxValue, RangeCheckingInclusion.IncludeValue);
        }

        internal static void CheckIfInputValueIsBiggerThan(double inputValue, string parameterName, double minValue, RangeCheckingInclusion includeMinValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, includeMinValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
        }

        internal static void CheckIfInputValueIsSmallerThan(double inputValue, string parameterName, double maxValue, RangeCheckingInclusion includeMaxValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, double.MinValue, RangeCheckingInclusion.IncludeValue, maxValue, includeMaxValue);
        }


        internal static void CheckParameterIsNotNullOrEmpty(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, ExceptionDescription.ParameterIsNull);
            }
            else if (value.Trim() == string.Empty)
            {
                throw new ArgumentException(ExceptionDescription.ParameterIsEmpty, parameterName);
            }
        }
        
       

        internal static void CheckWkbIsValid(byte[] wkb, string parameterName)
        {
            bool isValid = false;

            if (wkb.Length > 0)
            {
                if ((wkb[0] == 1 || wkb[0] == 0))
                {
                    if ((wkb[2] == 0 && wkb[3] == 0 && ((wkb[1] == 0 && wkb[4] != 0) || (wkb[1] != 0 && wkb[4] == 0))))
                    {
                        int shapeType = wkb[1] + wkb[4];
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
                string errorDescription = ExceptionDescription.WkbIsInvalid;
                throw new ArgumentException(errorDescription, parameterName);
            }
        }





        internal static void CheckFeatureIsValid(Feature feature)
        {
            if (!feature.IsValid())
            {
                throw new ArgumentException(ExceptionDescription.FeatureIsNotValid);
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



        internal static void CheckShapeIsMultipointShape(BaseShape baseShape)
        {
            if (!(baseShape is MultipointShape))
            {
                throw new ArgumentException(ExceptionDescription.TargetShapeIsNotValidType);
            }
        }

        internal static void CheckShapeIsLineBaseShape(BaseShape baseShape)
        {
            if (!(baseShape is BaseLineShape))
            {
                throw new ArgumentException(ExceptionDescription.TargetShapeIsNotValidType);
            }
        }




        internal static void CheckGeographyUnitIsValid(GeographyUnit geographyUnit, string parameterName)
        {
            switch (geographyUnit)
            {
                case GeographyUnit.DecimalDegree: break;
                case GeographyUnit.Feet: break;
                case GeographyUnit.Meter: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckFeatureColumnValueContainsColon(string value, string parameterName)
        {
            if (!value.Contains(":"))
            {
                throw new ArgumentException(ExceptionDescription.FeatureColumnValueDoesNotContainColon, parameterName);
            }
        }


    }
}
