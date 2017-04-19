using System;
using Mapgenix.Canvas;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    /// <summary>
    /// Factory for creating different types of PointStyles.
    /// </summary>
    [Serializable]
    public static class PointStyles
    {
        /// <summary>Returns a simple point style.</summary>
        /// <param name="pointStyle">PointStyle symbol type.</param>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimplePointStyle(PointSymbolType pointStyle, GeoColor fillColor, float size)
        {
            PointStyle returnStyle = new PointStyle();
            returnStyle.SymbolType = pointStyle;
            returnStyle.SymbolSolidBrush = new GeoSolidBrush(fillColor);
            returnStyle.SymbolSize = size;
            return returnStyle;
        }

        /// <summary>Returns a simple point style.</summary>
        /// <param name="pointStyle">PointStyle symbol type.</param>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="outlineWidth">PointStyle outline width.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimplePointStyle(PointSymbolType pointStyle, GeoColor fillColor, GeoColor outlineColor, float outlineWidth, float size)
        {
            PointStyle returnStyle = new PointStyle();
            returnStyle.SymbolType = pointStyle;
            returnStyle.SymbolSolidBrush = new GeoSolidBrush(fillColor);
            returnStyle.SymbolSize = size;
            returnStyle.SymbolPen = new GeoPen(outlineColor, outlineWidth);

            return returnStyle;
        }

        /// <summary>Returns a simple point style.</summary>
        /// <param name="pointStyle">PointStyle symbol type.</param>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimplePointStyle(PointSymbolType pointStyle, GeoColor fillColor, GeoColor outlineColor, float size)
        {
            PointStyle returnStyle = new PointStyle();
            returnStyle.SymbolType = pointStyle;
            returnStyle.SymbolSolidBrush = new GeoSolidBrush(fillColor);
            returnStyle.SymbolPen = new GeoPen(outlineColor);
            returnStyle.SymbolSize = size;

            return returnStyle;
        }

        /// <summary>Returns a circle point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleCircleStyle(GeoColor fillColor, float size)
        {
            return CreateSimplePointStyle(PointSymbolType.Circle, fillColor, size);
        }

        /// <summary>Returns a circle point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="outlineWidth">PointStyle outline width.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleCircleStyle(GeoColor fillColor, float size, GeoColor outlineColor, float outlineWidth)
        {
            return CreateSimplePointStyle(PointSymbolType.Circle, fillColor, outlineColor, outlineWidth, size);
        }

        /// <summary>Returns a circle point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleCircleStyle(GeoColor fillColor, float size, GeoColor outlineColor)
        {
            return CreateSimplePointStyle(PointSymbolType.Circle, fillColor, outlineColor, size);
        }

        /// <summary>Returns a square point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleSquareStyle(GeoColor fillColor, float size)
        {
            return CreateSimplePointStyle(PointSymbolType.Square, fillColor, size);
        }

        /// <summary>Returns a square point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleSquareStyle(GeoColor fillColor, float size, GeoColor outlineColor)
        {
            return CreateSimplePointStyle(PointSymbolType.Square, fillColor, outlineColor, size);
        }

        /// <summary>Returns a square point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="outlineWidth">PointStyle outline width.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleSquareStyle(GeoColor fillColor, float size, GeoColor outlineColor, float outlineWidth)
        {
            return CreateSimplePointStyle(PointSymbolType.Square, fillColor, outlineColor, outlineWidth, size);
        }

        /// <summary>Returns a star point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleStarStyle(GeoColor fillColor, float size)
        {
            return CreateSimplePointStyle(PointSymbolType.Star, fillColor, size);
        }

        /// <summary>Returns a star point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleStarStyle(GeoColor fillColor, float size, GeoColor outlineColor)
        {
            return CreateSimplePointStyle(PointSymbolType.Star, fillColor, outlineColor, size);
        }

        /// <summary>Returns a point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="outlineWidth">PointStyle outline width.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleStarStyle(GeoColor fillColor, float size, GeoColor outlineColor, float outlineWidth)
        {
            return CreateSimplePointStyle(PointSymbolType.Star, fillColor, outlineColor, outlineWidth, size);
        }

        /// <summary>Returns a triangle point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleTriangleStyle(GeoColor fillColor, float size)
        {
            return CreateSimplePointStyle(PointSymbolType.Triangle, fillColor, size);
        }

        /// <summary>Returns a triangle point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleTriangleStyle(GeoColor fillColor, float size, GeoColor outlineColor)
        {
            return CreateSimplePointStyle(PointSymbolType.Triangle, fillColor, outlineColor, size);
        }

        /// <summary>Returns a point style.</summary>
        /// <param name="fillColor">PointStyle fill color.</param>
        /// <param name="size">PointStyle size.</param>
        /// <param name="outlineColor">PointStyle outline color.</param>
        /// <param name="outlineWidth">PointStyle outline width.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateSimpleTriangleStyle(GeoColor fillColor, float size, GeoColor outlineColor, float outlineWidth)
        {
            return CreateSimplePointStyle(PointSymbolType.Triangle, fillColor, outlineColor, outlineWidth, size);
        }

        /// <summary>Returns a compound circle point style.</summary>
        /// <param name="fillColor1">Outer circle's PointStyle fill color.</param>
        /// <param name="size1">Outer circle's PointStyle size.</param>
        /// <param name="outlineColor1">Outer circle's PointStyle outline color.</param>
        /// <param name="outlineWidth1">Outer circle's PointStyle outline width.</param>
        /// <param name="fillColor2">Inner circle's PointStyle fill color.</param>
        /// <param name="size2">Inner circle's PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateCompoundCircleStyle(GeoColor fillColor1, float size1, GeoColor outlineColor1, float outlineWidth1, GeoColor fillColor2, float size2)
        {
            return CreateCompoundPointStyle(PointSymbolType.Circle, fillColor1, outlineColor1, outlineWidth1, size1, PointSymbolType.Circle, fillColor2, GeoColor.StandardColors.Transparent, 0F, size2);
        }

        /// <summary>Returns a compound circle point style.</summary>
        /// <param name="fillColor1">Outer circle's PointStyle fill color.</param>
        /// <param name="size1">Outer circle's PointStyle size.</param>
        /// <param name="outlineColor1">Outer circle's PointStyle outline color.</param>
        /// <param name="outlineWidth1">Outer circle's PointStyle outline width.</param>
        /// <param name="fillColor2">Inner circle's PointStyle fill color.</param>
        /// <param name="outlineColor2">Inner circle's PointStyle outline color.</param>
        /// <param name="outlineWidth2">Inner circle's PointStyle outline width.</param>
        /// <param name="size2">Inner circle's PointStyle size.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateCompoundCircleStyle(GeoColor fillColor1, float size1, GeoColor outlineColor1, float outlineWidth1, GeoColor fillColor2, float size2, GeoColor outlineColor2, float outlineWidth2)
        {
            return CreateCompoundPointStyle(PointSymbolType.Circle, fillColor1, outlineColor1, outlineWidth1, size1, PointSymbolType.Circle, fillColor2, outlineColor2, outlineWidth2, size2);
        }

        /// <summary>Returns a compound point style.</summary>
        /// <param name="pointStyle1">Outer circle's pointstyle symbol type.</param>
        /// <param name="fillColor1">Outer circle's PointStyle fill color.</param>
        /// <param name="size1">Outer circle's PointStyle size.</param>
        /// <param name="outlineColor1">Outer circle's PointStyle outline color.</param>
        /// <param name="outlineWidth1">Outer circle's PointStyle outline width.</param>
        /// <param name="pointStyle2">Inner circle's pointstyle symbol type.</param>
        /// <param name="fillColor2">Inner circle's PointStyle fill color.</param>
        /// <param name="size2">Inner circle PointStyle size.</param>
        /// <param name="outlineColor2">Inner circle's PointStyle outline color.</param>
        /// <param name="outlineWidth2">Inner circle's PointStyle outline width.</param>
        /// <returns>Point style.</returns>
        public static PointStyle CreateCompoundPointStyle(PointSymbolType pointStyle1, GeoColor fillColor1, GeoColor outlineColor1, float outlineWidth1, float size1, PointSymbolType pointStyle2, GeoColor fillColor2, GeoColor outlineColor2, float outlineWidth2, float size2)
        {
            PointStyle returnStyle = new PointStyle();
            returnStyle.SymbolType = pointStyle1;
            returnStyle.SymbolSolidBrush = new GeoSolidBrush(fillColor1);
            returnStyle.SymbolPen = new GeoPen(outlineColor1, outlineWidth1);
            returnStyle.SymbolSize = size1;

            PointStyle stackStyle = new PointStyle();
            stackStyle.SymbolType = pointStyle2;
            stackStyle.SymbolSolidBrush = new GeoSolidBrush(fillColor2);
            stackStyle.SymbolPen = new GeoPen(outlineColor2, outlineWidth2);
            stackStyle.SymbolSize = size2;

            returnStyle.CustomPointStyles.Add(stackStyle);

            return returnStyle;
        }
    }
}