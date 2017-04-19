namespace Mapgenix.Canvas
{
    /// <summary>Types to join consecutive line or curve segments.</summary>
    public enum DrawingLineJoin
    {
        /// <summary>Beveled join producing a diagonal corner.</summary>
        Bevel = 1,
        /// <summary>Mitered join producing a sharp corner or a clipped corner,
        /// depending on whether the length of the miter exceeds the miter limit.</summary>
        Miter = 0,
        /// <summary>Mitered clipped join producing a sharp corner or a beveled corner,
        /// depending on whether the length of the miter exceeds the miter limit.</summary>
        MiterClipped = 2,
        /// <summary>Circular join producing a smooth, circular arc between the lines.</summary>
        Round = 3
    }
}