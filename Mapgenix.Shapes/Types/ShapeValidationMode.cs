namespace Mapgenix.Shapes
{
    /// <summary>Types of validation test to run on a geometric shape.</summary>
    /// <remarks>Simple tests are done internally before any method call to a shape in order to have a minimum
    /// level of confidence for a successfull operation. 
    /// It is recommended to do an advanced validation on shape from an unconfirmed source.</remarks>
    public enum ShapeValidationMode
    {
        Simple = 0,
        Advanced = 1
    }
}