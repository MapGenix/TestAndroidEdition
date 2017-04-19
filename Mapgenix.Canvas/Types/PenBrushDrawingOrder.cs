namespace Mapgenix.Canvas
{
    /// <summary>Drawing orders with pen and brush.</summary>
    public enum PenBrushDrawingOrder
    {
        /// <summary>Fill brush draws first.</summary>
        BrushFirst = 0,
        /// <summary>Outline pen draws first.</summary>
        PenFirst = 1,
    }
}