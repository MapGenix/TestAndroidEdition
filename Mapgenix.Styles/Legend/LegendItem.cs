using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    [Serializable]
    public class LegendItem
    {
        public LegendItem()
        {
            Width = 160;
            Height = 25;
            ImageWidth = 16;
            ImageHeight = 16;
            ImageTopPadding = 5;
            ImageBottomPadding = 5;
            ImageLeftPadding = 10;
            ImageRightPadding = 5;
            TextTopPadding = 8;
            TextLeftPadding = 5;
        }

        public LegendItem(int width, int height, float imageWidth, float imageHeight, BaseStyle imageStyle, LabelStyle textStyle)
        {
            this.width = width;
            this.height = height;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.imageStyle = imageStyle;
            this.textStyle = textStyle;
        }

        private float width;
        private float height;
        private float imageWidth;
        private float imageHeight;

        private float topPadding;
        private float bottomPadding;
        private float leftPadding;
        private float rightPadding;
        private float imageTopPadding;
        private float imageBottomPadding;
        private float imageLeftPadding;
        private float imageRightPadding;
        private float textTopPadding;
        private float textBottomPadding;
        private float textLeftPadding;
        private float textRightPadding;

        private BaseStyle imageStyle;
        private LabelStyle textStyle;
        private AreaStyle backgroundMask;
        private AreaStyle textMask;
        private AreaStyle imageMask;

        private LegendImageJustificationMode imageJustificationMode;

        public float TopPadding
        {
            get { return topPadding; }
            set { topPadding = value; }
        }
        public float BottomPadding
        {
            get { return bottomPadding; }
            set { bottomPadding = value; }
        }
        public float LeftPadding
        {
            get { return leftPadding; }
            set { leftPadding = value; }
        }
        public float RightPadding
        {
            get { return rightPadding; }
            set { rightPadding = value; }
        }
        public float ImageTopPadding
        {
            get { return imageTopPadding; }
            set { imageTopPadding = value; }
        }
        public float ImageBottomPadding
        {
            get { return imageBottomPadding; }
            set { imageBottomPadding = value; }
        }
        public float ImageLeftPadding
        {
            get { return imageLeftPadding; }
            set { imageLeftPadding = value; }
        }
        public float ImageRightPadding
        {
            get { return imageRightPadding; }
            set { imageRightPadding = value; }
        }
        public float TextTopPadding
        {
            get { return textTopPadding; }
            set { textTopPadding = value; }
        }
        public float TextBottomPadding
        {
            get { return textBottomPadding; }
            set { textBottomPadding = value; }
        }
        public float TextLeftPadding
        {
            get { return textLeftPadding; }
            set { textLeftPadding = value; }
        }
        public float TextRightPadding
        {
            get { return textRightPadding; }
            set { textRightPadding = value; }
        }

        public float Width
        {
            get { return width; }
            set { width = value; }
        }
        public float Height
        {
            get { return height; }
            set { height = value; }
        }
        public float ImageWidth
        {
            get { return imageWidth; }
            set { imageWidth = value; }
        }
        public float ImageHeight
        {
            get { return imageHeight; }
            set { imageHeight = value; }
        }

        public BaseStyle ImageStyle
        {
            get { return imageStyle; }
            set { imageStyle = value; }
        }
        public LabelStyle TextStyle
        {
            get { return textStyle; }
            set { textStyle = value; }
        }
        public AreaStyle BackgroundMask
        {
            get { return backgroundMask; }
            set { backgroundMask = value; }
        }
        public AreaStyle TextMask
        {
            get { return textMask; }
            set { textMask = value; }
        }
        public AreaStyle ImageMask
        {
            get { return imageMask; }
            set { imageMask = value; }
        }

        public LegendImageJustificationMode ImageJustificationMode
        {
            get { return imageJustificationMode; }
            set { imageJustificationMode = value; }
        }

        public void Draw(BaseGeoCanvas adornmentGeoCanvas, Collection<SimpleCandidate> labelsInAllLayers, Offset offset)
        {
            Validators.CheckValueIsBiggerThanZero(width, "width");
            Validators.CheckValueIsBiggerThanZero(height, "height");
            Validators.CheckGeoCanvasIsInDrawing(adornmentGeoCanvas.IsDrawing);
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");

            DrawCore(adornmentGeoCanvas, labelsInAllLayers, offset);
        }

        protected virtual void DrawCore(BaseGeoCanvas adornmentGeoCanvas, Collection<SimpleCandidate> labelsInAllLayers, Offset offset)
        {
            float xOffset = offset.X;
            float yOffset = offset.Y;
            float totalImageLeftPadding = 0;
            float totalTextLeftPadding = 0;
            float totalImageTopPadding = yOffset + TopPadding + ImageTopPadding;
            float totalTextTopPadding = yOffset + TopPadding + TextTopPadding;

            DrawingRectangleF textLocation = new DrawingRectangleF();
            if (TextStyle != null)
            {
                int warplength = CustomWarpLength();
                string text = ChangeTheTextLengthByMaxTextLength(warplength);
                textLocation = adornmentGeoCanvas.MeasureText(text, TextStyle.Font);
            }

            switch (ImageJustificationMode)
            {
                case LegendImageJustificationMode.JustifyImageRight:
                    totalImageLeftPadding = xOffset + LeftPadding + TextLeftPadding + textLocation.Width + TextRightPadding + ImageLeftPadding;
                    totalTextLeftPadding = xOffset + LeftPadding + TextLeftPadding;
                    break;

                default:
                    totalImageLeftPadding = xOffset + LeftPadding + ImageLeftPadding;
                    totalTextLeftPadding = xOffset + LeftPadding + ImageLeftPadding + ImageWidth + ImageRightPadding + TextLeftPadding;
                    break;
            }

            if (BackgroundMask != null)
            {
                DrawingRectangleF backgroudExtent = new DrawingRectangleF(Width / 2 + xOffset + LeftPadding, Height / 2 + yOffset + TopPadding, Width, Height);
                BackgroundMask.DrawSample(adornmentGeoCanvas, backgroudExtent);
            }
            if (ImageStyle != null)
            {
                DrawingRectangleF imageExtent = new DrawingRectangleF(ImageWidth / 2 + totalImageLeftPadding, ImageHeight / 2 + totalImageTopPadding, ImageWidth, ImageHeight);
                ImageStyle.DrawSample(adornmentGeoCanvas, imageExtent);
                if (ImageMask != null)
                {
                    ImageMask.DrawSample(adornmentGeoCanvas, imageExtent);
                }
            }
            if (TextStyle != null)
            {
                DrawingRectangleF textExtent = new DrawingRectangleF(textLocation.CenterX + totalTextLeftPadding, textLocation.CenterY + totalTextTopPadding, textLocation.Width, textLocation.Height);
                TextStyle.DrawSample(adornmentGeoCanvas, textExtent);

                if (TextMask != null)
                {
                    TextMask.DrawSample(adornmentGeoCanvas, textExtent);
                }
            }
            
        }

        protected virtual int CustomWarpLength()
        {
            GdiPlusGeoCanvas textGeoCanvas = new GdiPlusGeoCanvas();
            int textWidth = (int)textGeoCanvas.MeasureText(TextStyle.TextColumnName, TextStyle.Font).Width;
            int warplength = 0;

            float textTotalWidth = Width - (ImageWidth + ImageLeftPadding + ImageRightPadding + TextLeftPadding + TextRightPadding);

            if (textTotalWidth < textWidth)
            {

            }

            return warplength;
        }

        private string ChangeTheTextLengthByMaxTextLength(int wordWrapLength)
        {
            string tempText = textStyle.TextColumnName;
            if (wordWrapLength != 0)
            {
                string cuttedText = tempText;
                string cuttingText = "";
                while (cuttedText.Length > wordWrapLength)
                {
                    if (cuttedText.Contains(" "))
                    {
                        cuttedText = Regex.Replace(cuttedText, @"\s+", " ");
                        if (cuttedText.IndexOf(" ", 0, wordWrapLength) + cuttedText.IndexOf(" ", wordWrapLength) < wordWrapLength * 2)
                        {
                            cuttingText = cuttingText + cuttedText.Substring(0, cuttedText.IndexOf(" ")) + "\r\n";
                            cuttedText = cuttedText.Substring(cuttedText.IndexOf(" ") + 1);
                        }
                        else
                        {
                            cuttingText = cuttingText + cuttedText.Substring(0, cuttedText.IndexOf(" ", wordWrapLength)) + "\r\n";
                            cuttedText = cuttedText.Substring(cuttedText.IndexOf(" ", wordWrapLength) + 1);
                        }
                    }
                    else
                        break;
                }
                tempText = cuttingText + cuttedText;
            }
            return tempText;
        }
    }
}
