namespace Mapgenix.Canvas
{
    /// <summary>Modes for resizing map.</summary>
    public enum MapResizeMode
    {
        /// <summary>Preserves scale of the map.</summary>
        PreserveScale = 0,
        /// <summary>Preserves scale and center of the map.</summary>
        PreserveScaleAndCenter = 1,
        /// <summary>Preserves extent of the map.</summary>
        PreserveExtent = 3
    }
}