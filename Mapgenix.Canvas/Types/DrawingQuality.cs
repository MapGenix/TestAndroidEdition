namespace Mapgenix.Canvas
{
    /// <summary>Types of drawing quality for the GeoCanvas.</summary>
    public enum DrawingQuality
    {
        /// <summary>Default drawing quality, balanced between speed and quality.</summary>
        Default = 0,
        /// <summary>High quality drawing quality (lower speed).</summary>
        HighQuality = 1,
        /// <summary>Low quality drawing quality (higher speed).</summary>
        HighSpeed = 2,
        /// <summary>Based on the canvas setting.</summary>
        CanvasSettings = 3
    }
}