using System;

namespace Mapgenix.Canvas
{
    /// <summary>Types of font style for drawing text.</summary>
    [Flags]
    public enum DrawingFontStyles
    {
        /// <summary>Standard text.</summary>
        Regular = 1,
        /// <summary>Bold text.</summary>
        Bold = 2,
        /// <summary>Italic text.</summary>
        Italic = 4,
        /// <summary>Underlined text.</summary>
        Underline = 8,
        /// <summary>Strikeout text.</summary>
        Strikeout = 16
    }
}