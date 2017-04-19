using System;

namespace Mapgenix.Canvas
{
    /// <summary>
    /// Font used to label features on a canvas.
    /// </summary>
    [Serializable]
    public class GeoFont
    {
        private static long _geoFontIdCounter;
        private readonly string _fontName;

        private readonly long _id;

        private readonly bool _isBold;
        private readonly bool _isItalic;
        private readonly bool _isStrikeout;
        private readonly bool _isUnderline;
        private readonly float _size;

        private readonly DrawingFontStyles _style;

        /// <overloads>Creates the GeoFont with default property values.</overloads>
        /// <remarks>None</remarks>
        /// <summary>Creates a GeoFont for labeling features on a canvas.</summary>
        public GeoFont()
            : this("Arial", 9, DrawingFontStyles.Regular)
        {
        }

        /// <summary>Creates a GeoFont for labeling features on a canvas.</summary>
        /// <overloads>Creates a GeoFont by specifying font name and font size.</overloads>
        /// <remarks>None</remarks>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="size">Size of the font.</param>
        public GeoFont(string fontName, float size)
            : this(fontName, size, DrawingFontStyles.Regular)
        {
        }

        /// <summary>Creates a GeoFont for labeling features on a canvas.</summary>
        /// <overloads>Creates a GeoFont by specifying font name, font size and font style.</overloads>
        /// <remarks>None</remarks>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="size">Size of the font.</param>
        /// <param name="style">Style of the font.</param>
       public GeoFont(string fontName, float size, DrawingFontStyles style)
        {
            _fontName = fontName;
            _size = size;
            _style = style;

            _isBold = ((style & DrawingFontStyles.Bold) != 0);
            _isItalic = ((style & DrawingFontStyles.Italic) != 0);
            _isStrikeout = ((style & DrawingFontStyles.Strikeout) != 0);
            _isUnderline = ((style & DrawingFontStyles.Underline) != 0);

            _geoFontIdCounter += 1;
            _id = _geoFontIdCounter;
        }

       /// <summary>Gets the bold property of the font.</summary>
       /// <value>Bold property of the font.</value>
       /// <remarks>None</remarks>
        public bool IsBold
        {
            get { return _isBold; }
        }

        /// <summary>Gets the strikeout property of the font.</summary>
        /// <value>Strikeout property of the font.</value>
        /// <remarks>None</remarks>
        public bool IsStrikeout
        {
            get { return _isStrikeout; }
        }

        /// <summary>Gets the italic property of the font.</summary>
        /// <value>Italic property of the font.</value>
        /// <remarks>None</remarks>
        public bool IsItalic
        {
            get { return _isItalic; }
        }

        /// <summary>Gets the underline property of the font.</summary>
        /// <value>Underline property of the font.</value>
        /// <remarks>None</remarks>
        public bool IsUnderline
        {
            get { return _isUnderline; }
        }

        /// <summary>Gets the font name property of the font.</summary>
        /// <value>Font name property of the font.</value>
        /// <remarks>None</remarks>
        public string FontName
        {
            get { return _fontName; }
        }

        /// <summary>Gets the font size property of the font.</summary>
        /// <value>Font size property of the font.</value>
        /// <remarks>None</remarks>
        public float Size
        {
            get { return _size; }
        }

        /// <summary>Gets the font style property of the font.</summary>
        /// <value>Font style property of the font.</value>
        /// <remarks>None</remarks>
        public DrawingFontStyles Style
        {
            get { return _style; }
        }

        /// <summary>Gets the Id property of the font.</summary>
        /// <value>Id property of the font.</value>
        /// <remarks>None</remarks>
        public long Id
        {
            get { return _id; }
        }
    }
}