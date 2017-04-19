namespace Mapgenix.Canvas
{
    /// <summary>Formats of the map image tile.</summary>
    /// <remarks>Recommendation: 
    /// -With low bandwidth network use JPEG image format.
    /// -To overlay on top of other images, use PNG image format which supports transparency.
    /// </remarks>
    public enum TileImageFormat
    {
        /// <summary>Image in PNG image format.</summary>
        Png = 0,
        /// <summary>Image in JPEG image format.</summary>
        Jpeg = 1,
    }
}