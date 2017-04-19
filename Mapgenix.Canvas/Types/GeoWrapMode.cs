namespace Mapgenix.Canvas
{
    /// <summary>Wrap mode for filling texture.</summary>
    public enum GeoWrapMode
    {
        /// <summary>Texture tiled to fill the entire area.</summary>
        Tile = 0,
        /// <summary>Texture not be tiled.</summary>
        Clamp = 1,
        /// <summary>Texture reversed horizontally.</summary>
        TileFlipX = 2,
        /// <summary>Texture reversed vertically.</summary>
        TileFlipY = 3,
        /// <summary>Texture reversed horizontally and vertically.</summary>
        TileFlipXY = 4
    }
}