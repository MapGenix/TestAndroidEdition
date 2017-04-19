namespace Mapgenix.Canvas
{
    /// <summary>Types of Point.</summary>
    /// <remarks>Depending on the type, there are different properties that need to be set on the PointStyle.</remarks>
    public enum PointType
    {
        /// <summary>Symbol type.</summary>
        Symbol = 0,
        /// <summary>Bitmap type.</summary>
        Bitmap = 1,
        /// <summary>Font character type.</summary>
        Character = 2,
    }
}