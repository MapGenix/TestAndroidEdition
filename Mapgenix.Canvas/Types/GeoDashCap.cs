namespace Mapgenix.Canvas
{
    /// <summary>Types of graphic shape to use on both ends of each dash in a dashed line.</summary>
    public enum GeoDashCap
    {
        /// <summary>Square cap that squares off both ends of each dash.</summary>
        Flat = 0,
        /// <summary>Circular cap that rounds off both ends of each dash.</summary>
        Round = 1,
        /// <summary>Triangular cap that points both ends of each dash.</summary>
        Triangle = 2
    }
}