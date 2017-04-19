using System;
using Mapgenix.Canvas;


namespace Mapgenix.Styles
{
    /// <summary>Factory for creating different types of LabelStyles.</summary>
    [Serializable]
    public static class TextStyles
    {
        ///<summary>Returns simple LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        public static LabelStyle CreateSimpleTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);

            return result;
        }

        ///<summary>Returns simple LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="xOffset">XOffset of the font  in pixel</param>
        /// <param name="yOffset">YOffset of the font  in pixel</param>
        public static LabelStyle CreateSimpleTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, float xOffset, float yOffset)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.XOffsetInPixel = xOffset;
            result.YOffsetInPixel = yOffset;

            return result;
        }

        ///<summary>Returns simple LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="haloPenColor">GeoColor to set the halopen color.</param>
        /// <param name="haloPenWidth">Halopen width.</param>
        public static LabelStyle CreateSimpleTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor haloPenColor, float haloPenWidth)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.HaloPen = new GeoPen(haloPenColor, haloPenWidth);

            return result;
        }

        ///<summary>Returns simple LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="haloPenColor">GeoColor to set the halopen color.</param>
        /// <param name="haloPenWidth">Halopen width.</param>
        /// <param name="xOffset">XOffset of the font  in pixel</param>
        /// <param name="yOffset">YOffset of the font  in pixel</param>
        public static LabelStyle CreateSimpleTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor haloPenColor, float haloPenWidth, float xOffset, float yOffset)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.HaloPen = new GeoPen(haloPenColor, haloPenWidth);
            result.XOffsetInPixel = xOffset;
            result.YOffsetInPixel = yOffset;

            return result;
        }

        ///<summary>Returns mask LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="maskFillColor">GeoColor to set the mask fill color.</param>
        public static LabelStyle CreateMaskTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor maskFillColor)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.Mask = new AreaStyle(new GeoSolidBrush(maskFillColor));

            return result;
        }

        ///<summary>Returns mask LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="maskFillColor">GeoColor to set the mask fill color.</param>
        /// <param name="xOffset">XOffset of the font  in pixel</param>
        /// <param name="yOffset">YOffset of the font  in pixel</param>
        public static LabelStyle CreateMaskTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor maskFillColor, float xOffset, float yOffset)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.Mask = new AreaStyle(new GeoSolidBrush(maskFillColor));
            result.XOffsetInPixel = xOffset;
            result.YOffsetInPixel = yOffset;

            return result;
        }

        ///<summary>Returns mask LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="textFont">GeoFont to set the font of the text.</param>
        /// <param name="textBrush">GeoSolidBrush to set the brush of the text.</param>
        /// <param name="areaStyle">areaStyle as mask of of the TextStyle.</param>
        /// <param name="xOffset">XOffset of the font  in pixel</param>
        /// <param name="yOffset">YOffset of the font  in pixel</param>
        public static LabelStyle CreateMaskTextStyle(string textColumnName, GeoFont textFont, GeoSolidBrush textBrush, AreaStyle areaStyle, float xOffset, float yOffset)
        {
            LabelStyle result = new LabelStyle(textColumnName, textFont, textBrush);
            result.Mask = areaStyle;
            result.XOffsetInPixel = xOffset;
            result.YOffsetInPixel = yOffset;

            return result;
        }

        ///<summary>Returns mask LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="maskPenColor">GeoColor to set the mask pen color.</param>
        /// <param name="maskPenSize">Mask pen size.</param>
        public static LabelStyle CreateMaskTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor maskPenColor, float maskPenSize)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.Mask = new AreaStyle(new GeoPen(maskPenColor, maskPenSize));

            return result;
        }

        ///<summary>Returns mask LabelStyle.</summary>
        ///<returns>LabelStyle.</returns>
        /// <param name="textColumnName">Column name.</param>
        /// <param name="fontFamilyName">Font family name.</param>
        /// <param name="fontSize">Font size.</param>
        /// <param name="drawingFontStyle">DrawingFontStyles to set the style of the font.</param>
        /// <param name="fontColor">GeoColor to set the font color.</param>
        /// <param name="maskPenColor">GeoColor to set the mask pen color.</param>
        /// <param name="maskPenSize">float value to set the mask pen size.</param>
        /// <param name="xOffset">XOffset of the font in pixel</param>
        /// <param name="yOffset">YOffset of the font in pixel</param>
        public static LabelStyle CreateMaskTextStyle(string textColumnName, string fontFamilyName, float fontSize, DrawingFontStyles drawingFontStyle, GeoColor fontColor, GeoColor maskPenColor, float maskPenSize, float xOffset, float yOffset)
        {
            GeoFont font = new GeoFont(fontFamilyName, fontSize, drawingFontStyle);
            GeoSolidBrush txtBrush = new GeoSolidBrush(fontColor);
            LabelStyle result = new LabelStyle(textColumnName, font, txtBrush);
            result.Mask = new AreaStyle(new GeoPen(maskPenColor, maskPenSize));
            result.XOffsetInPixel = xOffset;
            result.YOffsetInPixel = yOffset;

            return result;
        }
    }
}