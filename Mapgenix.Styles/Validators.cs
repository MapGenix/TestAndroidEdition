using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles.Properties;
using Mapgenix.Utils;

namespace Mapgenix.Styles
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

      
        internal static void CheckGeoCanvasIsInDrawing(bool isDrawing)
        {
            if (!isDrawing)
            {
                throw new InvalidOperationException(ExceptionDescription.GeocanvasIsNotInDrawing);
            }
        }

        
    
        internal static void CheckScaleIsBiggerThanZero(double imageScale, string parameterName)
        {
            if (imageScale <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.ImageScaleShouldBiggerThanZero);
            }
        }

       
        internal static void CheckClassBreaksAreValid(Collection<ClassBreak> classBreaks)
        {
            double tmp = double.MinValue;
            for (int i = 1; i < classBreaks.Count - 1; i++)
            {
                if (classBreaks[i].Value <= tmp)
                {
                    throw new ArgumentException(ExceptionDescription.ClassBreaksIsValid);
                }
                tmp = classBreaks[i].Value;
            }
        }

        internal static void CheckColumnNameIsInFeature(string columnName, IEnumerable<Feature> features)
        {
            bool exist = false;

            int featureCount = 0;
            foreach (Feature feature in features)
            {
                featureCount++;
                Dictionary<string, string> dict = feature.ColumnValues;

                foreach (KeyValuePair<string, string> item in dict)
                {
                    string name = item.Key;
                    if (columnName.ToUpperInvariant().Trim().Equals(name.ToUpperInvariant().Trim(), StringComparison.CurrentCulture))
                    {
                        exist = true;
                        break;
                    }
                }
            }

            if (featureCount == 0)
            {
                exist = true;
            }

            if (!exist)
            {
                throw new ArgumentException(ExceptionDescription.FieldNameIsNotInFeature);
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
                string errorDescription = ExceptionDescription.DoubleOutOfRange;
                throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

       

        internal static void CheckIfInputValueIsBiggerThan(double inputValue, string parameterName, double minValue, RangeCheckingInclusion includeMinValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, includeMinValue, double.MaxValue, RangeCheckingInclusion.IncludeValue);
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


       

        internal static void CheckParameterIsNotBothNull(Object firstObject, Object secondObject, string firstParameterName, string secondParameterName)
        {
            if (firstObject == null && secondObject == null)
            {
                string exceptionDescription = ExceptionDescription.ParameterIsNull;
                throw new ArgumentNullException(firstParameterName + " and " + secondParameterName, exceptionDescription);
            }
        }



        internal static void CheckDrawingLevelIsValid(DrawingLevel drawingLevel, string parameterName)
        {
            switch (drawingLevel)
            {
                case DrawingLevel.LevelFour: break;
                case DrawingLevel.LevelOne: break;
                case DrawingLevel.LevelThree: break;
                case DrawingLevel.LevelTwo: break;
                case DrawingLevel.LabelLevel: break;
                default:
                    string errorDescription = ExceptionDescription.EnumerationOutOfRange;
                    throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

      
        internal static void CheckIconImageAndIconFilePathAreInvalid(string iconFilePath, GeoImage iconImage)
        {
            if (string.IsNullOrEmpty(iconFilePath) && iconImage == null)
            {
                throw new InvalidOperationException();
            }
        }
        
        
        internal static void CheckValueIsBiggerThanZero(double value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ExceptionDescription.TheValueShouldBeGreaterThanZero);
            }
        }



    }
}
