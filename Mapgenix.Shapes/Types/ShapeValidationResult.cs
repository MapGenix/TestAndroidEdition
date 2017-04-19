using System;

namespace Mapgenix.Shapes 
{
    /// <summary>Result of the validation method performed on a geometric shape.</summary>
    /// <remarks>To determine if a validation rule is violated by a shape with the description of the violation.</remarks>
    [Serializable]
    public struct ShapeValidationResult
    {
        private bool isValid;
        private string validationErrors;

        /// <summary>Sets the IsValid and ValidationErrors properties.</summary>
        /// <remarks>None</remarks>
        /// <param name="isValid">Whether the object has passed its validation method.</param>
        /// <param name="validationErrors">List of errors if the object did not pass the validation method. Errors separated by semicolons.</param>
        public ShapeValidationResult(bool isValid, string validationErrors)
        {
            this.isValid = isValid;
            this.validationErrors = validationErrors;
        }

        /// <summary>Whether the object has passed its validation method.</summary>
        /// <value>Either true or false, depending on whether the object has passed its validation method.</value>
        /// <remarks>None</remarks>
        public Boolean IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        /// <summary>Gets the list of errors if the object did not pass the validation method.</summary>
        /// <remarks>If errors,  separated by semicolons. Empty string if valid.</remarks>
        /// <value>List of errors if the object did not pass the validation method.</value>
        public string ValidationErrors
        {
            get
            {
                return this.validationErrors;
            }
        }

        /// <summary>Override of == operator.</summary>
        /// <remarks>None</remarks>
        public static bool operator ==(ShapeValidationResult sourceShapeValidationResult, ShapeValidationResult targetShapeValidationResult)
        {
            bool returnValue = false;
            if ((sourceShapeValidationResult.isValid == targetShapeValidationResult.isValid) && (sourceShapeValidationResult.ValidationErrors == targetShapeValidationResult.ValidationErrors))
            {
                returnValue = true;
            }
            return returnValue;
        }

        /// <summary>Override of != operator.</summary>
        /// <remarks>None</remarks>
        public static bool operator !=(ShapeValidationResult sourceShapeValidationResult, ShapeValidationResult targetShapeValidationResult)
        {
            return !(sourceShapeValidationResult == targetShapeValidationResult);
        }

        /// <summary>Override of Equals function.</summary>
        /// <remarks>None</remarks>
        public override bool Equals(object obj)
        {
            if (obj is ShapeValidationResult)
            {
                return this == (ShapeValidationResult)obj;
            }

            return false;
        }

        /// <summary>Override of GetHashCode method.</summary>
        /// <remarks>None</remarks>
        public override int GetHashCode()
        {
            return isValid.GetHashCode() ^ validationErrors.GetHashCode();
        }
    }
}
