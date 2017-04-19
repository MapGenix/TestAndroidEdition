using System;

namespace Mapgenix.Canvas
{
    /// <summary>Brush for filling area based features with various patterns.</summary>
    [Serializable]
    public class GeoHatchBrush : BaseGeoBrush
    {
        private GeoColor _backgroundColor;
        private GeoColor _foregroundColor;
        private GeoHatchStyle _hatchStyle;

        private GeoHatchBrush()
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Provides a foreground color for pattern fill.</overloads>
        /// <returns>None</returns>
        /// <remarks>To specify only the foreground color of the pattern and leave the background transparent.</remarks>
        /// <param name="hatchStyle">Hatch pattern to fill the area.</param>
        /// <param name="foregroundColor">Foreground color for the pattern.</param>
        public GeoHatchBrush(GeoHatchStyle hatchStyle, GeoColor foregroundColor)
            : this(hatchStyle, foregroundColor, GeoColor.StandardColors.Black)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Provides a foreground and background color for pattern fill.</overloads>
        /// <returns>None</returns>
        /// <remarks>To specify both the background and foreground color for the pattern.</remarks>
        /// <param name="hatchStyle">Hatch pattern to fill the area.</param>
        /// <param name="foregroundColor">Foreground color for the pattern.</param>
        /// <param name="backgroundColor">Background color for the pattern.</param>
        public GeoHatchBrush(GeoHatchStyle hatchStyle, GeoColor foregroundColor, GeoColor backgroundColor)
        {
            _hatchStyle = hatchStyle;
            _foregroundColor = foregroundColor;
            _backgroundColor = backgroundColor;
        }

        /// <summary>Gets and sets the pattern to use for the fill.</summary>
        /// <value>Pattern to use for the fill.</value>
        public GeoHatchStyle HatchStyle
        {
            get { return _hatchStyle; }
            set { _hatchStyle = value; }
        }

        /// <summary>Gets and sets the foreground color for the fill pattern.</summary>
        /// <value>Foreground color for the fill pattern.</value>
        public GeoColor ForegroundColor
        {
            get { return _foregroundColor; }
            set { _foregroundColor = value; }
        }

        /// <summary>Gets and sets the background color for the fill pattern.</summary>
        /// <value>Background color for the fill pattern.</value>
        public GeoColor BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }
    }
}