using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class LabelPrinterLayer : BasePrinterLayer
    {
        PrinterWrapMode _printerWrapMode = PrinterWrapMode.AutoSizeText;

        float _minFontSize = 1;
        float _maxFontSize = 200;

        public GeoFont Font { get; set; }

        public BaseGeoBrush FontBrush { get; set; }

        public string Text { get; set; }

        public GeoFont DrawingFont { get; set; }

        public RectangleShape LastBoundingBox { get; set; }

        public PrinterWrapMode PrinterWrapMode
        {
            get { return _printerWrapMode; }
            set { _printerWrapMode = value; }
        }


        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            PrinterDrawHelper.DrawPrinterCore(this, canvas, labelsInAllLayers);
            PrinterDrawHelper.DrawLabelPrinterCore(this,canvas);
        }

        
        public string WrapText(BaseGeoCanvas canvas, RectangleShape drawingBoundingBox, GeoFont drawingFont, string text)
        {
            StringBuilder sb = new StringBuilder();

            DrawingRectangleF drawingRect = canvas.MeasureText(text, drawingFont);
            ScreenPointF upperLeftOnScreen = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, drawingBoundingBox.UpperLeftPoint, canvas.Width, canvas.Height);
            ScreenPointF lowerRightOnScreen = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, drawingBoundingBox.UpperRightPoint, canvas.Width, canvas.Height);
            int drawingRectWidthOnScreen = (int)(lowerRightOnScreen.X - upperLeftOnScreen.X);

            if (drawingRect.Width > drawingRectWidthOnScreen)
            {
                text = text.Replace("\n", " ");
                text = text.Replace("\r", " ");
                text = text.Replace(".", ". ");
                text = text.Replace(">", "> ");
                text = text.Replace("\t", " ");
                text = text.Replace(",", ", ");
                text = text.Replace(";", "; ");
                text = text.Replace("<br>", " ");

                int maxStringLength = GetMaxStringLength(text, drawingRect.Width, drawingRectWidthOnScreen);
                string[] texts = Wrap(text, maxStringLength);
                foreach (string item in texts)
                {
                    if (item == "")
                    {
                        sb.Append(item);
                    }
                    else
                    {
                        sb.AppendLine(item);
                    }
                }
            }
            else
            {
                sb.Append(text);
            }

            return sb.ToString();
        }

        int GetMaxStringLength(string text, float currentWidth, double boundingBoxScreenWidth)
        {
            int result = (int)(text.Length * boundingBoxScreenWidth / currentWidth);
            if (result <= 0) { result = 1; }
            return result;
        }

        string[] Wrap(string text, int maxLength)
        {
            string[] words = text.Split(' ');
            int currentLineLength = 0;
            ArrayList lines = new ArrayList(text.Length / maxLength);
            string currentLine = "";
            bool inTag = false;

            foreach (string currentWord in words)
            {
                if (currentWord.Length > 0)
                {
                    if (currentWord.Substring(0, 1) == "<")
                        inTag = true;

                    if (inTag)
                    {
                        if (currentLine.EndsWith("."))
                        {
                            currentLine += currentWord;
                        }
                        else
                            currentLine += " " + currentWord;

                        if (currentWord.IndexOf(">") > -1)
                            inTag = false;
                    }
                    else
                    {
                        if (currentLineLength + currentWord.Length + 1 < maxLength)
                        {
                            currentLine += " " + currentWord;
                            currentLineLength += (currentWord.Length + 1);
                        }
                        else
                        {
                            lines.Add(currentLine);
                            currentLine = currentWord;
                            currentLineLength = currentWord.Length;
                        }
                    }
                }

            }
            if (currentLine != "")
                lines.Add(currentLine);

            string[] textLinesStr = new string[lines.Count];
            lines.CopyTo(textLinesStr, 0);
            return textLinesStr;
        }

        public float GetFontSizeByBoundingBox(BaseGeoCanvas canvas, GeoFont font, string drawingText, RectangleShape boundingBox)
        {
            float rtn = font.Size;

            ScreenPointF boundingBoxPointFul = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, boundingBox.UpperLeftPoint, canvas.Width, canvas.Height);
            ScreenPointF boundingBoxPointFur = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, boundingBox.UpperRightPoint, canvas.Width, canvas.Height);
            ScreenPointF boundingBoxPointFll = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, boundingBox.LowerLeftPoint, canvas.Width, canvas.Height);

            double widthInScreen = boundingBoxPointFur.X - boundingBoxPointFul.X;
            double heightInScreen = boundingBoxPointFll.Y - boundingBoxPointFul.Y;

            DrawingRectangleF textRectInScreen = canvas.MeasureText(drawingText, font);

            if (textRectInScreen.Width > widthInScreen || textRectInScreen.Height > heightInScreen)
            {
                while (textRectInScreen.Width > widthInScreen || textRectInScreen.Height > heightInScreen)
                {
                    rtn--;
                    textRectInScreen = canvas.MeasureText(drawingText, new GeoFont(font.FontName, rtn, font.Style));
                }
            }
            else
            {
                while (textRectInScreen.Width < widthInScreen && textRectInScreen.Height < heightInScreen)
                {
                    rtn++;
                    textRectInScreen = canvas.MeasureText(drawingText, new GeoFont(font.FontName, rtn, font.Style));
                }
                rtn--;
            }

            if (rtn < _minFontSize)
            {
                rtn = _minFontSize;
            }
            else if (rtn > _maxFontSize)
            {
                rtn = _maxFontSize;
            }

            return rtn;
        }
    }
}
