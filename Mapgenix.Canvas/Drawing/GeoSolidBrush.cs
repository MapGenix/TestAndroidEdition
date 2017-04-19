using System;

namespace Mapgenix.Canvas
{
    /// <summary>
    ///GeoBrush of a single color tofill area shapes on a GeoCanvas.
    /// </summary>
    [Serializable]
    public class GeoSolidBrush : BaseGeoBrush
    {
        /// <summary>Creates a new GeoSolidBrush.</summary>
        /// <overloads>Creates a GeoSolidBrush in transparent.</overloads>
        public GeoSolidBrush()
            : this(GeoColor.StandardColors.Transparent)
        {
        }

        /// <summary>Creates a new GeoSolidBrush.</summary>
        /// <overloads>Creates a GeoSolidBrush by passing in a GeoColor.</overloads>
        /// <param name="color">Color of the GeoSolidBrush.</param>
        public GeoSolidBrush(GeoColor color)
        {
            Color = color;
        }

        /// <summary>Gets or sets the color of the GeoSolidBrush.</summary>
        /// <decimalDegreesValue>Gets the color of the GeoSolidBrush.</decimalDegreesValue>
        public GeoColor Color { get; set; }
        
    }
}