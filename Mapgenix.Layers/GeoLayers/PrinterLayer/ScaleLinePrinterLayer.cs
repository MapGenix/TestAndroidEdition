using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    public class ScaleLinePrinterLayer : BasePrinterLayer
    {
        int _topLengthPixel;
        int _bottomLengthPixel;

        string _topString;
        string _bottomString;

        public int ScaleLineHeightPixel { get; set; }

        public int ScaleLineWidthPixel { get; set; }

        public GeoFont Font { get; set; }

        public BaseGeoBrush Brush { get; set; }

        public GeoPen Pen { get; set; }

        public GeoPen BackPen { get; set; }

        public MapPrinterLayer MapPrinterLayer { get; set; }

        public Dictionary<string, string> DisplayUnits { get; set; }

        public GeographyUnit MapUnit { get; set; }

        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            PrinterDrawHelper.DrawPrinterCore(this,canvas,labelsInAllLayers);
            PrinterDrawHelper.DrawScaleLinePrinterLayer(this,canvas);
         
        }

        public bool SetBarItems(GeographyUnit mapUnit)
        {
            bool scaleLineValid = true;

            double resolution = MapPrinterLayer.MapExtent.Width / MapPrinterLayer.GetBoundingBox().Width;

            int  maxBarLength = (int)GetBoundingBox().Width;
            long maxSizeData = Convert.ToInt64(maxBarLength * resolution * GetInchesPreUnit(mapUnit));

            DistanceUnit bottomUnits = DistanceUnit.Feet;
            DistanceUnit topUnits = DistanceUnit.Meter;
            if (maxSizeData > 100000)
            {
                topUnits = DistanceUnit.Kilometer;
                bottomUnits = DistanceUnit.Mile;
            }

            float topMax = Convert.ToInt32(maxSizeData / GetInchesPreUnit(topUnits));
            float bottomMax = Convert.ToInt32(maxSizeData / GetInchesPreUnit(bottomUnits));

            int topRounded = GetBarLength((int)topMax);
            int bottomRounded = GetBarLength((int)bottomMax);

            if (topRounded < 2 && topUnits == DistanceUnit.Meter)
            {
                scaleLineValid = false;
            }
            else
            {
                topMax = Convert.ToSingle(topRounded / GetInchesPreUnit(mapUnit) * GetInchesPreUnit(topUnits));
                bottomMax = Convert.ToSingle(bottomRounded / GetInchesPreUnit(mapUnit) * GetInchesPreUnit(bottomUnits));

                _topLengthPixel = Convert.ToInt32(topMax / resolution);
                if (_topLengthPixel > ScaleLineWidthPixel)
                {
                    _topLengthPixel = ScaleLineWidthPixel;
                }

                _bottomLengthPixel = Convert.ToInt32(bottomMax / resolution);
                if (_bottomLengthPixel > ScaleLineWidthPixel)
                {
                    _bottomLengthPixel = ScaleLineWidthPixel;
                }

                _topString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", topRounded, GetShortUnitString(topUnits));
                _bottomString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", bottomRounded, GetShortUnitString(bottomUnits));
            }

            return scaleLineValid;
        }

       public void DrawScaleLineImage(BaseGeoCanvas canvas, RectangleShape boundingBox, double ratio)
        {
            GeoFont drawingFont = new GeoFont(Font.FontName, (float)(Font.Size * ratio));
            GeoPen drawingPen = new GeoPen(GeoColor.StandardColors.Black, (float)(Pen.Width * ratio));
            GeoPen drawingBackPen = new GeoPen(GeoColor.StandardColors.White, (float)(BackPen.Width * ratio));

            int drawingScaleLineHeightPixel = (int)(ScaleLineHeightPixel * ratio);
            
            int drawingBottomLengthPixel = (int)(_bottomLengthPixel * ratio);
            int drawingTopLengthPixel = (int)(_topLengthPixel * ratio);

            int xOffset = (int)(Pen.Width * ratio);
            float bottomTextYOffset = (float)(BackPen.Width * ratio);

            ScreenPointF startPointF = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent, boundingBox.UpperLeftPoint, canvas.Width, canvas.Height);

            string trimmedTopString = TrimText(_topString, drawingFont, drawingTopLengthPixel, canvas);
            int topBarHeight = drawingScaleLineHeightPixel;
            if (trimmedTopString != _topString)
            {
                topBarHeight += Convert.ToInt32(0.5 * drawingScaleLineHeightPixel);
            }

            string trimmedBottomString = TrimText(_bottomString, drawingFont, drawingBottomLengthPixel, canvas);
            int bottomBarHeight = drawingScaleLineHeightPixel;
            if (trimmedBottomString != _bottomString)
            {
                bottomBarHeight += Convert.ToInt32(0.5 * drawingScaleLineHeightPixel);
            }

            float topTextHeight = canvas.MeasureText(trimmedTopString, drawingFont).Height / 2;
            float bottomTextHeight = canvas.MeasureText(trimmedBottomString, drawingFont).Height / 2;

            canvas.DrawText(trimmedTopString, drawingFont, Brush, new ScreenPointF[1] { new ScreenPointF(startPointF.X + drawingTopLengthPixel / 2, startPointF.Y + topTextHeight) }, DrawingLevel.LevelOne);

            ScreenPointF[] topPointFs = new ScreenPointF[4];
            topPointFs[0] = new ScreenPointF(1 + xOffset, 0);
            topPointFs[1] = new ScreenPointF(1 + xOffset, topBarHeight);
            topPointFs[2] = new ScreenPointF(drawingTopLengthPixel + xOffset, topBarHeight);
            topPointFs[3] = new ScreenPointF(drawingTopLengthPixel + xOffset, 0);
            canvas.DrawLine(topPointFs, drawingBackPen, DrawingLevel.LevelOne, startPointF.X, startPointF.Y);

            canvas.DrawText(trimmedBottomString, drawingFont, Brush, new ScreenPointF[1] { new ScreenPointF(startPointF.X + drawingBottomLengthPixel / 2, startPointF.Y + topBarHeight + bottomTextHeight + bottomTextYOffset) }, DrawingLevel.LevelOne);

            ScreenPointF[] bottomPointFs = new ScreenPointF[4];
            bottomPointFs[0] = new ScreenPointF(1 + xOffset, topBarHeight + bottomBarHeight);
            bottomPointFs[1] = new ScreenPointF(1 + xOffset, topBarHeight);
            bottomPointFs[2] = new ScreenPointF(drawingBottomLengthPixel + xOffset, topBarHeight);
            bottomPointFs[3] = new ScreenPointF(drawingBottomLengthPixel + xOffset, topBarHeight + bottomBarHeight);
            canvas.DrawLine(bottomPointFs, drawingBackPen, DrawingLevel.LevelOne, startPointF.X, startPointF.Y);

            topPointFs[0] = new ScreenPointF(1 + xOffset, 1);
            topPointFs[3] = new ScreenPointF(drawingTopLengthPixel + xOffset, 1);
            canvas.DrawLine(topPointFs, drawingPen, DrawingLevel.LevelTwo, startPointF.X, startPointF.Y);

            bottomPointFs[0] = new ScreenPointF(1 + xOffset, topBarHeight + bottomBarHeight - 1);
            bottomPointFs[3] = new ScreenPointF(drawingBottomLengthPixel + xOffset, topBarHeight + bottomBarHeight - 1);
            canvas.DrawLine(bottomPointFs, drawingPen, DrawingLevel.LevelTwo, startPointF.X, startPointF.Y);
        }

        static string TrimText(string text, GeoFont font, int lengthPixel, BaseGeoCanvas canvas)
        {
            float width = canvas.MeasureText(text, font).Width;
            if (width > lengthPixel - 4)
            {
                text = text.Replace(" ", Environment.NewLine + "  ");
            }

            return text;
        }

        string GetShortUnitString(DistanceUnit targetUnits)
        {
            string returnValue = string.Empty;

            switch (targetUnits)
            {
                case DistanceUnit.Meter:
                    returnValue = DisplayUnits["Meter"];
                    break;
                case DistanceUnit.Feet:
                    returnValue = DisplayUnits["Feet"];
                    break;
                case DistanceUnit.Kilometer:
                    returnValue = DisplayUnits["Kilometer"];
                    break;
                case DistanceUnit.Mile:
                    returnValue = DisplayUnits["Mile"];
                    break;
                case DistanceUnit.UsSurveyFeet:
                    returnValue = DisplayUnits["UsSurveyFeet"];
                    break;
                case DistanceUnit.Yard:
                    returnValue = DisplayUnits["Yard"];
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        static int GetBarLength(int maxLength)
        {
            string maxLengthString = maxLength.ToString(CultureInfo.InvariantCulture);

            int firstTwoChars;
            if (maxLengthString.Length > 1)
            {
                firstTwoChars = Convert.ToInt32(maxLengthString.Substring(0, 2), CultureInfo.InvariantCulture);
            }
            else
            {
                firstTwoChars = Convert.ToInt32(maxLengthString, CultureInfo.InvariantCulture);
                firstTwoChars *= 10;
            }
            
            string returnValueString = string.Empty;
            if (firstTwoChars > 45)
            {
                returnValueString = "5";
            }
            else if (firstTwoChars > 15)
            {
                returnValueString = "2";
            }
            else
            {
                returnValueString = "1";
            }

            for (int i = 0; i < maxLengthString.Length - 1; i++)
            {
                returnValueString += "0";
            }

            return Convert.ToInt32(returnValueString, CultureInfo.InvariantCulture);
        }

        static double GetInchesPreUnit(DistanceUnit targetUnit)
        {
            double returnValue = 0;

            switch (targetUnit)
            {
                case DistanceUnit.Meter:
                    returnValue = 39.3701;
                    break;
                case DistanceUnit.Feet:
                    returnValue = 12;
                    break;
                case DistanceUnit.Kilometer:
                    returnValue = 39370.1;
                    break;
                case DistanceUnit.Mile:
                    returnValue = 63360;
                    break;
                case DistanceUnit.UsSurveyFeet:
                    break;
                case DistanceUnit.Yard:
                    returnValue = 36;
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        static double GetInchesPreUnit(GeographyUnit targetUnit)
        {
            double returnValue = 0;

            switch (targetUnit)
            {
                case GeographyUnit.Unknown:
                    break;
                case GeographyUnit.DecimalDegree:
                    returnValue = 4374754;
                    break;
                case GeographyUnit.Feet:
                    returnValue = 12;
                    break;
                case GeographyUnit.Meter:
                    returnValue = 39.3701;
                    break;
                default:
                    break;
            }

            return returnValue;
        }
    }
}
