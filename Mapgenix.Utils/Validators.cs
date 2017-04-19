using System;
using System.Collections;

namespace Mapgenix.Utils
{
    internal static class Validators
    {
        internal static void CheckParameterIsNotNull(Object objectToTest, string parameterName)
        {
        
            if (objectToTest == null)
            {
                string exceptionDescription = CollectionExceptions.ParameterIsNull;
                throw new ArgumentNullException(parameterName, exceptionDescription);
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
                string errorDescription = CollectionExceptions.DoubleOutOfRange;
                throw new ArgumentOutOfRangeException(parameterName, errorDescription);
            }
        }

        internal static void CheckIfInputValueIsInRange(double inputValue, string parameterName, double minValue, double maxValue)
        {
            CheckIfInputValueIsInRange(inputValue, parameterName, minValue, RangeCheckingInclusion.IncludeValue, maxValue, RangeCheckingInclusion.IncludeValue);
        }
        
        
        

        internal static void CheckIEnumerableIsEmptyOrNull(IEnumerable values)
        {
        
            if (values != null)
            {
                foreach (object item in values)
                {
                    return;
                }
            }

            throw new InvalidOperationException(CollectionExceptions.IEnumerableIsEmptyOrNull);
        }
        
        
  
    }
}
