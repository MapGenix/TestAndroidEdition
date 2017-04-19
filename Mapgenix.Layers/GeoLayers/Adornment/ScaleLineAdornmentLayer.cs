using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Adornment layer to show scale line for the current map extent. 
    /// </summary>
    [Serializable]
    public class ScaleLineAdornmentLayer : BaseAdornmentLayer
    {
        const int MaxBarLength = 100;
        const int MaskMargin = 10;

        int _bottomLengthPixel;
        string _bottomString;
        Dictionary<string, string> _displayUnitString;
        public int ScaleLineHeightPixel { get; set; }
        public int ScaleLineWidthPixel { get; set; }
        int _topLengthPixel;
        string _topString;

        public ScaleLineAdornmentLayer() : base()
        {
            Location = AdornmentLocation.LowerLeft;
            ScaleLineHeightPixel = 14;
            ScaleLineWidthPixel = 150;

            _displayUnitString = new Dictionary<string, string>();
            _displayUnitString.Add("Meter", "m");
            _displayUnitString.Add("Feet", "ft");
            _displayUnitString.Add("Kilometer", "km");
            _displayUnitString.Add("Mile", "mi");
            _displayUnitString.Add("UsSurveyFeet", "usf");
            _displayUnitString.Add("Yard", "f");
        }

        public Dictionary<string, string> DisplayUnitString
        {
            get { return _displayUnitString; }
            set { _displayUnitString = value; }
        }

        
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
          AdornmentDrawHelper.DrawScaleLineAdornmentLayer(this, canvas, labelsInAllLayers);
        }

        public void DrawScaleLineImage(BaseGeoCanvas canvas)
        {
            GeoFont font = new GeoFont("Calibri", 9f, DrawingFontStyles.Bold);
            BaseGeoBrush brush = new GeoSolidBrush(GeoColor.StandardColors.Black);
            GeoPen pen = new GeoPen(GeoColor.StandardColors.Black, 2f);
            GeoPen backPen = new GeoPen(GeoColor.StandardColors.White, 4f);

            string trimmedTopString = TrimText(_topString, font, _topLengthPixel, canvas);
            int topBarHeight = ScaleLineHeightPixel;
            if (trimmedTopString != _topString)
            {
                topBarHeight += Convert.ToInt32(0.5 * ScaleLineHeightPixel);
            }

            string trimmedBottomString = TrimText(_bottomString, font, _bottomLengthPixel, canvas);
            int bottomBarHeight = ScaleLineHeightPixel;
            if (trimmedBottomString != _bottomString)
            {
                bottomBarHeight += Convert.ToInt32(0.5 * ScaleLineHeightPixel);
            }

            ScreenPointF startPointF = GetDrawingLocation(canvas, ScaleLineWidthPixel, topBarHeight + bottomBarHeight);

            DrawBackgroundMask(canvas, topBarHeight, bottomBarHeight, startPointF);

            canvas.DrawText(trimmedTopString, font, brush, new ScreenPointF[1] { new ScreenPointF(startPointF.X + _topLengthPixel / 2, startPointF.Y + canvas.MeasureText(trimmedTopString, font).Height / 2) }, DrawingLevel.LevelOne);

            ScreenPointF[] topPointFs = new ScreenPointF[4];
            topPointFs[0] = new ScreenPointF(1, 0);
            topPointFs[1] = new ScreenPointF(1, topBarHeight);
            topPointFs[2] = new ScreenPointF(_topLengthPixel, topBarHeight);
            topPointFs[3] = new ScreenPointF(_topLengthPixel, 0);
            canvas.DrawLine(topPointFs, backPen, DrawingLevel.LevelOne, startPointF.X, startPointF.Y);

            canvas.DrawText(trimmedBottomString, font, brush, new ScreenPointF[1] { new ScreenPointF(startPointF.X + _bottomLengthPixel / 2, startPointF.Y + topBarHeight + canvas.MeasureText(trimmedBottomString, font).Height / 2 + 2) }, DrawingLevel.LevelOne);

            ScreenPointF[] bottomPointFs = new ScreenPointF[4];
            bottomPointFs[0] = new ScreenPointF(1, topBarHeight + bottomBarHeight);
            bottomPointFs[1] = new ScreenPointF(1, topBarHeight);
            bottomPointFs[2] = new ScreenPointF(_bottomLengthPixel, topBarHeight);
            bottomPointFs[3] = new ScreenPointF(_bottomLengthPixel, topBarHeight + bottomBarHeight);
            canvas.DrawLine(bottomPointFs, backPen, DrawingLevel.LevelOne, startPointF.X, startPointF.Y);

            topPointFs[0] = new ScreenPointF(1, 1);
            topPointFs[3] = new ScreenPointF(_topLengthPixel, 1);
            canvas.DrawLine(topPointFs, pen, DrawingLevel.LevelTwo, startPointF.X, startPointF.Y);

            bottomPointFs[0] = new ScreenPointF(1, topBarHeight + bottomBarHeight - 1);
            bottomPointFs[3] = new ScreenPointF(_bottomLengthPixel, topBarHeight + bottomBarHeight - 1);
            canvas.DrawLine(bottomPointFs, pen, DrawingLevel.LevelTwo, startPointF.X, startPointF.Y);
        }

        void DrawBackgroundMask(BaseGeoCanvas canvas, int topBarHeight, int bottomBarHeight, ScreenPointF startPointF)
        {
            int width = Math.Max(_topLengthPixel, _bottomLengthPixel);
            int height = topBarHeight + bottomBarHeight;
            DrawingRectangleF backgroudExtent = new DrawingRectangleF(startPointF.X + width / 2, startPointF.Y + height / 2, width + MaskMargin, height + MaskMargin);
            BackgroundMask.DrawSample(canvas, backgroudExtent);
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

        public bool SetBarItems(BaseGeoCanvas canvas, GeographyUnit mapUnit)
        {
            bool scaleLineValid = true;

            DistanceUnit topInUnits = DistanceUnit.Meter;
            DistanceUnit topOutUnits = DistanceUnit.Kilometer;
            DistanceUnit bottomOutUnits = DistanceUnit.Mile;
            DistanceUnit bottomInUnits = DistanceUnit.Feet;

            DistanceUnit bottomUnits;
            DistanceUnit topUnits;

            ScreenPointF startPoint = new ScreenPointF(0, canvas.Height / 2);
            ScreenPointF stopPoint = new ScreenPointF(canvas.Width, canvas.Height / 2);
            PointShape leftPoint = ExtentHelper.ToWorldCoordinate(canvas.CurrentWorldExtent, startPoint.X, startPoint.Y, canvas.Width, canvas.Height);
            PointShape rightPoint = ExtentHelper.ToWorldCoordinate(canvas.CurrentWorldExtent, stopPoint.X, stopPoint.Y, canvas.Width, canvas.Height);

            double distance = 0;
            if (mapUnit == GeographyUnit.DecimalDegree)
            {
                if (leftPoint.X >= -180.0 && leftPoint.X <= 180 &&
                    leftPoint.Y >= -90.0 && leftPoint.Y <= 90 &&
                    rightPoint.X >= -180.0 && rightPoint.X <= 180 &&
                    rightPoint.Y >= -90.0 && rightPoint.Y <= 90)
                {
                    distance = DecimalDegreesHelper.GetDistanceFromDecimalDegrees(leftPoint.X, leftPoint.Y, rightPoint.X, rightPoint.Y, DistanceUnit.Meter);
                }
                else
                {
                    scaleLineValid = false;
                }
            }
            else if (mapUnit == GeographyUnit.Feet)
            {
                distance = (rightPoint.X - leftPoint.X) * GetInchesPreUnit(DistanceUnit.Meter) / GetInchesPreUnit(DistanceUnit.Feet);
            }
            else if (mapUnit == GeographyUnit.Meter)
            {
                distance = rightPoint.X - leftPoint.X;
            }

            long maxLength = Convert.ToInt64(distance * MaxBarLength / canvas.Width);

            if (maxLength > 2500)
            {
                topUnits = topOutUnits;
                bottomUnits = bottomOutUnits;
            }
            else
            {
                topUnits = topInUnits;
                bottomUnits = bottomInUnits;
            }

            float topMax = Convert.ToInt32(maxLength * GetInchesPreUnit(DistanceUnit.Meter) / GetInchesPreUnit(topUnits));
            float bottomMax = Convert.ToInt32(maxLength * GetInchesPreUnit(DistanceUnit.Meter) / GetInchesPreUnit(bottomUnits));

            int topRounded = GetBarLength((int)topMax);
            int bottomRounded = GetBarLength((int)bottomMax);

            if (topRounded < 2 && topUnits == DistanceUnit.Meter)
            {
                scaleLineValid = false;
            }
            else
            {
                _topLengthPixel = Convert.ToInt32(topRounded * MaxBarLength / topMax);
                if (_topLengthPixel > ScaleLineWidthPixel)
                {
                    _topLengthPixel = ScaleLineWidthPixel;
                }

                _bottomLengthPixel = Convert.ToInt32(bottomRounded * MaxBarLength / bottomMax);
                if (_bottomLengthPixel > ScaleLineWidthPixel)
                {
                    _bottomLengthPixel = ScaleLineWidthPixel;
                }

                _topString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", topRounded, GetShortUnitString(topUnits));
                _bottomString = string.Format(CultureInfo.InvariantCulture, "{0} {1}", bottomRounded, GetShortUnitString(bottomUnits));
            }

            return scaleLineValid;
        }

        string GetShortUnitString(DistanceUnit targetUnits)
        {
            string returnValue = string.Empty;

            switch (targetUnits)
            {
                case DistanceUnit.Meter:
                    returnValue = _displayUnitString["Meter"];
                    break;
                case DistanceUnit.Feet:
                    returnValue = _displayUnitString["Feet"];
                    break;
                case DistanceUnit.Kilometer:
                    returnValue = _displayUnitString["Kilometer"];
                    break;
                case DistanceUnit.Mile:
                    returnValue = _displayUnitString["Mile"];
                    break;
                case DistanceUnit.UsSurveyFeet:
                    returnValue = _displayUnitString["UsSurveyFeet"];
                    break;
                case DistanceUnit.Yard:
                    returnValue = _displayUnitString["Yard"];
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
            }

            return returnValue;
        }

       
    }
}