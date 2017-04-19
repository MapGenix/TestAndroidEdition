using System;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    /// <summary>
    /// Factory for creating different types of AreaStyles.
    /// </summary>
    [Serializable]
    public static class AreaStyles
    {
        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor)
        {
            return CreateSimpleAreaStyle(fillBrushColor, new GeoColor(), 1, LineDashStyle.Solid, 0, 0);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline pen color of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor)
        {
            return CreateSimpleAreaStyle(fillBrushColor, outlinePenColor, 1, LineDashStyle.Solid, 0, 0);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline pen color of the area.</param>
        /// <param name="outlinePenWidth">Outline pen width of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor, int outlinePenWidth)
        {
            return CreateSimpleAreaStyle(fillBrushColor, outlinePenColor, outlinePenWidth, LineDashStyle.Solid, 0, 0);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline pen color of the area.</param>
        /// <param name="outlinePenWidth">Outline pen width of the area.</param>
        /// <param name="borderStyle">BorderStyle of the area style.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor, int outlinePenWidth, LineDashStyle borderStyle)
        {
            return CreateSimpleAreaStyle(fillBrushColor, outlinePenColor, outlinePenWidth, borderStyle, 0, 0);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="xOffsetInPixel">X pixels offset of the area.</param>
        /// <param name="yOffsetInPixel">Y pixels offset of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, float xOffsetInPixel, float yOffsetInPixel)
        {
            return CreateSimpleAreaStyle(fillBrushColor, new GeoColor(), 1, LineDashStyle.Solid, xOffsetInPixel, yOffsetInPixel);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline color of the area.</param>
        /// <param name="xOffsetInPixel">X pixels offset of the area.</param>
        /// <param name="yOffsetInPixel">Y pixels offset of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor, float xOffsetInPixel, float yOffsetInPixel)
        {
            return CreateSimpleAreaStyle(fillBrushColor, outlinePenColor, 1, LineDashStyle.Solid, xOffsetInPixel, yOffsetInPixel);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline pen color of the area.</param>
        /// <param name="outlinePenWidth">Outline pen width of the area.</param>
        /// <param name="xOffsetInPixel">X pixels offset of the area.</param>
        /// <param name="yOffsetInPixel">Y pixels offset of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor, int outlinePenWidth, float xOffsetInPixel, float yOffsetInPixel)
        {
            return CreateSimpleAreaStyle(fillBrushColor, outlinePenColor, outlinePenWidth, LineDashStyle.Solid, xOffsetInPixel, yOffsetInPixel);
        }

        /// <summary>Creates a simple area style.</summary>
        /// <returns>Simple area style.</returns>
        /// <remarks>None</remarks>
        /// <param name="fillBrushColor">Fill color of the area.</param>
        /// <param name="outlinePenColor">Outline pen color of the area.</param>
        /// <param name="outlinePenWidth">Outline pen width of the area.</param>
        ///<param name="borderStyle">BorderStyle of the area.</param>
        /// <param name="xOffsetInPixel">X pixels offset of the area.</param>
        /// <param name="yOffsetInPixel">Y pixels offset of the area.</param>
        public static AreaStyle CreateSimpleAreaStyle(GeoColor fillBrushColor, GeoColor outlinePenColor, int outlinePenWidth, LineDashStyle borderStyle, float xOffsetInPixel, float yOffsetInPixel)
        {
            AreaStyle returnStyle = new AreaStyle
            {
                FillSolidBrush = new GeoSolidBrush(fillBrushColor),
                OutlinePen = new GeoPen(outlinePenColor, outlinePenWidth) {DashStyle = borderStyle}
            };
            returnStyle.XOffsetInPixel = xOffsetInPixel;
            returnStyle.YOffsetInPixel = yOffsetInPixel;

            return returnStyle;
        }

        /// <summary>Creates an AreaStyle with a hatch pattern.</summary>
        /// <returns>AreaStyle with hatch pattern.</returns>
        /// <remarks>None</remarks>
        /// <param name="hatchStyle">Hatch pattern to be used.</param>
        /// <param name="foregroundBrushColor">Color of the foreground of the hatch pattern.</param>
        /// <param name="backgroundBrushColor">Color of the background of the hatch pattern.</param>
        public static AreaStyle CreateHatchStyle(GeoHatchStyle hatchStyle, GeoColor foregroundBrushColor, GeoColor backgroundBrushColor)
        {
            return CreateHatchStyle(hatchStyle, foregroundBrushColor, backgroundBrushColor, GeoColor.StandardColors.Transparent, 0, LineDashStyle.Solid, 0, 0);
        }

        /// <summary>Creates an AreaStyle with a hatch pattern.</summary>
        /// <returns>AreaStyle with a hatch pattern.</returns>
        /// <remarks>None</remarks>
        /// <param name="hatchStyle">Hatch pattern to be used.</param>
        /// <param name="foregroundBrushColor">Color of the foreground of the hatch pattern.</param>
        /// <param name="backgroundBrushColor">Color of the background of the hatch pattern.</param>
        /// <param name="outlinePenColor">Border color of the area.</param>
        public static AreaStyle CreateHatchStyle(GeoHatchStyle hatchStyle, GeoColor foregroundBrushColor, GeoColor backgroundBrushColor, GeoColor outlinePenColor)
        {
            return CreateHatchStyle(hatchStyle, foregroundBrushColor, backgroundBrushColor, outlinePenColor, 1, LineDashStyle.Solid, 0, 0);
        }

        /// <summary>Creates a GeoHatchStyle.</summary>
        /// <requirements>None</requirements>
        /// <returns>AreaStyle with a hatch pattern.</returns>
        /// <param name="hatchStyle">Hatch pattern.</param>
        /// <param name="foregroundBrushColor">Color of the foreground of the hatch pattern.</param>
        /// <param name="backgroundColor">Color of the background of the hatch pattern.</param>
        /// <param name="outlinePenColor">Border color of the area.</param>
        /// <param name="outlinePenWidth">Border width of the area.</param>
        /// <param name="outlineDashStyle">Dash style of the border.</param>
        /// <param name="xOffsetInPixel">Pixel offset for X.</param>
        /// <param name="yOffsetInPixel">Pixel offset for Y.</param>
        public static AreaStyle CreateHatchStyle(GeoHatchStyle hatchStyle, GeoColor foregroundBrushColor, GeoColor backgroundColor, GeoColor outlinePenColor, int outlinePenWidth, LineDashStyle outlineDashStyle, float xOffsetInPixel, float yOffsetInPixel)
        {
            AreaStyle returnStyle = new AreaStyle();

            GeoPen geoPen = new GeoPen(outlinePenColor, outlinePenWidth);
            geoPen.DashStyle = outlineDashStyle;
            returnStyle.OutlinePen = geoPen;

            GeoHatchBrush brush = new GeoHatchBrush(hatchStyle, foregroundBrushColor, backgroundColor);
            returnStyle.AreaStyleCustom.FillCustomBrush = brush;

            returnStyle.XOffsetInPixel = xOffsetInPixel;
            returnStyle.YOffsetInPixel = yOffsetInPixel;

            return returnStyle;
        }

        /// <summary>Creates a linear gradient AreaStyle.</summary>
        /// <returns>Linear gradient AreaStyle.</returns>
        /// <remarks>None</remarks>
        /// <param name="fromColor">
        /// Starting <strong>GeoColor</strong> of the
        /// gradient.
        /// </param>
        /// <param name="toColor">
        ///  Ending <strong>GeoColor</strong> of the
        /// gradient.
        /// </param>
        /// <param name="angle">
        ///  Angle of the color changing from start to
        /// end.
        /// </param>
        public static AreaStyle CreateLinearGradientStyle(GeoColor fromColor, GeoColor toColor, float angle)
        {
            return CreateLinearGradientStyle(fromColor, toColor, angle, GeoColor.StandardColors.Transparent);
        }

        /// <summary>Creates a linear gradient AreaStyle.</summary>
        /// <returns>Linear gradient AreaStyle.</returns>
        /// <remarks>None</remarks>
        /// <param name="fromColor">
        ///  Starting <strong>GeoColor</strong> of the
        /// gradient.
        /// </param>
        /// <param name="toColor">
        ///  Ending <strong>GeoColor</strong> of the
        /// gradient.
        /// </param>
        /// <param name="angle">
        ///  Angle of the color changing from start to
        /// end.
        /// </param>
        /// <param name="outlinePenColor">Outline pen color of the area style.</param>
        public static AreaStyle CreateLinearGradientStyle(GeoColor fromColor, GeoColor toColor, float angle, GeoColor outlinePenColor)
        {
            AreaStyle returnStyle = new AreaStyle();
            returnStyle.OutlinePen = new GeoPen(outlinePenColor);
            GeoLinearGradientBrush brush = new GeoLinearGradientBrush(fromColor, toColor, angle);
            returnStyle.AreaStyleCustom.FillCustomBrush = brush;
            return returnStyle;
        }

        /// <summary>Creates an AreaStyle.</summary>
        /// <returns>AreaStyle.</returns>
        /// <remarks>None.</remarks>
        /// <param name="outlinePenColor">GeoPen on the outline of the area style.</param>
        /// <param name="baseColor">Base <strong>GeoColor</strong> of the hue family colors.</param>
        /// <param name="numberOfColors">Number of GeoColors in hue family to construct the areastyle.</param>
        public static AreaStyle CreateHueFamilyAreaStyle(GeoColor outlinePenColor, GeoColor baseColor, int numberOfColors)
        {
            return new HueFamilyAreaStyle(new GeoPen(outlinePenColor), baseColor, numberOfColors);
        }

        /// <summary>Creates an AreaStyle in a family of hue-related colors drawn with a linear gradient.</summary>
        /// <returns>AreaStyle in a family of hue-related colors drawn with a linear gradient.</returns>
        /// <remarks>None.</remarks>
        /// <param name="outlinePenColor">GeoPen on the outline of the AreaStyle.
        /// </param>
        /// <param name="baseColor">Base <strong>GeoColor</strong> of the hue family of colors.</param>
        /// <param name="numberOfColors">Number of GeoColors in the hue family to construct the
        /// AreaStyle.</param>
        /// <param name="fromColor">
        /// Starting <strong>GeoColor</strong> of the
        /// gradient.
        /// </param>
        /// <param name="toColor">
        ///  Ending <strong>GeoColor</strong> of the gradient.</param>
        /// <param name="angle">Angle of the gradient.</param>
        public static AreaStyle CreateHueFamilyLinearGradientAreaStyle(GeoColor outlinePenColor, GeoColor baseColor, int numberOfColors, GeoColor fromColor, GeoColor toColor, float angle)
        {
            HueFamilyAreaStyle hueFamilyStyle = new HueFamilyAreaStyle(new GeoPen(outlinePenColor), baseColor, numberOfColors);
            hueFamilyStyle.AreaStyleCustom.FillCustomBrush = new GeoLinearGradientBrush(fromColor, toColor, angle);

            return hueFamilyStyle;
        }

        /// <summary>Creates an AreaStyle in a family of quality-realted colors.</summary>
        /// <returns>AreaStyle in a family of quality-realted colors.</returns>
        /// <param name="outlinePenColor">GeoPen on the outline of the AreaStyle.</param>
        /// <param name="baseColor">Base <strong>GeoColor</strong> of the quality family of colors.</param>
        /// <param name="numberOfColors">Number of GeoColors in the quality-based family to construct the AreaStyle.</param>
        public static AreaStyle CreateQualityFamilyAreaStyle(GeoColor outlinePenColor, GeoColor baseColor, int numberOfColors)
        {
            return new QualityFamilyAreaStyle(new GeoPen(outlinePenColor), baseColor, numberOfColors);
        }

        /// <summary>Creates an AreaStyle in a family of quality-related colors drawn with a linear gradient.</summary>
        /// <returns>AreaStyle in a family of quality-related colors drawn with a linear gradient.</returns>
        /// <remarks>None.</remarks>
        /// <param name="outlinePenColor">GeoPen on the outline of the AreaStyle.</param>
        /// <param name="baseColor">Base <strong>GeoColor</strong> of the quality family of colors.</param>
        /// <param name="numberOfColors">Number of GeoColors in quality-based family to construct the AreaStyle.</param>
        /// <param name="fromColor">Starting <strong>GeoColor</strong> of the gradient. </param>
        /// <param name="toColor">Ending <strong>GeoColor</strong> of the gradient.</param>
        /// <param name="angle">Angle of the gradient.</param>
        public static AreaStyle CreateQualityFamilyLinearGradientAreaStyle(GeoColor outlinePenColor, GeoColor baseColor, int numberOfColors, GeoColor fromColor, GeoColor toColor, float angle)
        {
            QualityFamilyAreaStyle qualityFamilyStyle = new QualityFamilyAreaStyle(new GeoPen(outlinePenColor), baseColor, numberOfColors);
            qualityFamilyStyle.AreaStyleCustom.FillCustomBrush = new GeoLinearGradientBrush(fromColor, toColor, angle);

            return qualityFamilyStyle;
        }

    }
}