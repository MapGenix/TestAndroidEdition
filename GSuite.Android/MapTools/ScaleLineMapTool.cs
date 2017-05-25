using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Mapgenix.Shapes;
using Android.Content;
using NativeAndroid = Android;
using Android.Widget;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Map tool for scale line
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class ScaleLineMapTool : BaseMapTool
    {
        private const int MaxBarLength = 100;
        private double _scaleLineHeightPixel;
        private double _scaleLineWidthPixel;
        private Dictionary<string, string> _displayUnitString;
        private RelativeLayout _scaleLineCanvas;
        private ImageView _scaleLineImage;

        private TextView _topText;
        private TextView _bottomText;

        public ScaleLineMapTool(Context context)
            : this(context, false)
        { }
      
        public ScaleLineMapTool(Context context, bool isEnabled)
            : base(context, isEnabled)
        {
            _scaleLineHeightPixel = LayoutUnitsUtil.convertDpToPixel(12, Context.Resources.DisplayMetrics.Ydpi);
            _scaleLineWidthPixel = LayoutUnitsUtil.convertDpToPixel(100, Context.Resources.DisplayMetrics.Xdpi);

            RelativeLayout.LayoutParams scaleLineLayout = new RelativeLayout.LayoutParams((int)_scaleLineWidthPixel, 
                (int)_scaleLineHeightPixel * 2);
            scaleLineLayout.AddRule(LayoutRules.AlignParentBottom);
            scaleLineLayout.BottomMargin = (int)LayoutUnitsUtil.convertDpToPixel(5, Context.Resources.DisplayMetrics.Xdpi);
            scaleLineLayout.LeftMargin = 10;

            _scaleLineCanvas = new RelativeLayout(Context);
            _scaleLineCanvas.LayoutParameters = scaleLineLayout;

            _topText = new TextView(Context) { TextSize = 5, Text = "20" };
            _topText.SetTextColor(Color.Black);
            LayoutParams topLaout = new LayoutParams((int)100, 20);
            topLaout.LeftMargin = 4;
            _topText.LayoutParameters = topLaout;

            _bottomText = new TextView(Context) { TextSize = 5, Text = "10" };
            _bottomText.SetTextColor(Color.Black);
            LayoutParams bottomLayout = new LayoutParams((int)100, 20);
            bottomLayout.LeftMargin = 4;
            bottomLayout.TopMargin = (int)_scaleLineHeightPixel / 2;
            _bottomText.LayoutParameters = bottomLayout;

            _displayUnitString = new Dictionary<string, string>();
            _displayUnitString.Add("Meter", "m");
            _displayUnitString.Add("Feet", "ft");
            _displayUnitString.Add("Kilometer", "km");
            _displayUnitString.Add("Mile", "mi");
            _displayUnitString.Add("UsSurveyFeet", "usf");
            _displayUnitString.Add("Yard", "f");
        }

        protected override void InitializeCore(Map map)
        {
            base.InitializeCore(map);
            CurrentMap.CurrentExtentChanged -= MapExtentChanged;
            CurrentMap.CurrentExtentChanged += MapExtentChanged;
            UpdateBarItems(CurrentMap.CurrentExtent);
        }

        protected override void EnabledChangedCore(bool newValue)
        {
            base.EnabledChangedCore(newValue);
            if (_scaleLineCanvas != null)
            {
                if (Enabled)
                {
                    if(ChildCount == 0)
                        AddView(_scaleLineCanvas);

                    //_scaleLineCanvas.RemoveAllViews();
                    _scaleLineCanvas.AddView(_topText);
                    _scaleLineCanvas.AddView(_bottomText);
                }
            }
        }

        private void MapExtentChanged(object sender, ExtentChangedEventArgs e)
        {
            if (Enabled)
            {
                UpdateBarItems(CurrentMap.CurrentExtent);
            }
        }

        private void DrawScaleLine(float xTopLine, float xMiddleLine, float xBottomLine)
        {
            Path leftLine = new Path();
            leftLine.MoveTo(0, 2);
            leftLine.LineTo(0, (float)_scaleLineHeightPixel * 2 - 2);
            leftLine.Close();

            Path topLine = new Path();
            topLine.MoveTo(xTopLine, 2);
            topLine.LineTo(xTopLine, (float)_scaleLineHeightPixel / 2);
            topLine.Close();

            Path middleLine = new Path();
            middleLine.MoveTo(0, (float)_scaleLineHeightPixel / 2);
            middleLine.LineTo(xMiddleLine, (float)_scaleLineHeightPixel / 2);
            middleLine.Close();

            Path bottomLine = new Path();
            bottomLine.MoveTo(xBottomLine, (float)_scaleLineHeightPixel / 2);
            bottomLine.LineTo(xBottomLine, (float)_scaleLineHeightPixel * 2 - 2);
            bottomLine.Close();

            Paint paint = new Paint();
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = Color.Black;
            paint.StrokeWidth = 3;

            Bitmap background = null;
            NativeAndroid.Graphics.Canvas backGroundCanvas = null;

            try
            {
                background = Bitmap.CreateBitmap((int)_scaleLineWidthPixel, (int)_scaleLineHeightPixel, Bitmap.Config.Argb8888);
                backGroundCanvas = new NativeAndroid.Graphics.Canvas(background);
                backGroundCanvas.DrawPath(leftLine, paint);
                backGroundCanvas.DrawPath(topLine, paint);
                backGroundCanvas.DrawPath(middleLine, paint);
                backGroundCanvas.DrawPath(bottomLine, paint);

                if (_scaleLineImage == null)
                {
                    _scaleLineImage = new ImageView(Context);
                    _scaleLineCanvas.AddView(_scaleLineImage);
                }

                _scaleLineImage.SetImageBitmap(background);
            }
            catch(Exception ex)
            {
                Toast.MakeText(Context, this.GetType().ToString() + " ERROR:" + ex.Message, ToastLength.Short).Show();
            }
            finally
            {
                if(background != null)
                    background.Dispose();
                if(paint != null)
                    paint.Dispose();                
                if(leftLine != null)
                    leftLine.Dispose();
                if(topLine != null)
                    topLine.Dispose();
                if(middleLine != null)
                    middleLine.Dispose();
                if(bottomLine!= null)
                    bottomLine.Dispose();
                if(backGroundCanvas != null)
                    backGroundCanvas.Dispose();
            }
                        
        }

        private void UpdateBarItems(RectangleShape extent)
        {
            DistanceUnit topInUnits = DistanceUnit.Meter;
            DistanceUnit topOutUnits = DistanceUnit.Kilometer;
            DistanceUnit bottomOutUnits = DistanceUnit.Mile;
            DistanceUnit bottomInUnits = DistanceUnit.Feet;

            DistanceUnit bottomUnits;
            DistanceUnit topUnits;

            double resolution = extent.Width / CurrentMap.MapWidth;

            long maxSizeData = Convert.ToInt64(MaxBarLength * resolution * GetInchesPreUnit(CurrentMap.MapUnit));

            if (maxSizeData > 100000)
            {
                topUnits = topOutUnits;
                bottomUnits = bottomOutUnits;
            }
            else
            {
                topUnits = topInUnits;
                bottomUnits = bottomInUnits;
            }

            float topMax = Convert.ToInt32(maxSizeData / GetInchesPreUnit(topUnits));
            float bottomMax = Convert.ToInt32(maxSizeData / GetInchesPreUnit(bottomUnits));

          
            int topRounded = GetBarLength((int)topMax);
            int bottomRounded = GetBarLength((int)bottomMax);

          
            topMax = Convert.ToSingle(topRounded / GetInchesPreUnit(CurrentMap.MapUnit) * GetInchesPreUnit(topUnits));
            bottomMax = Convert.ToSingle(bottomRounded / GetInchesPreUnit(CurrentMap.MapUnit) * GetInchesPreUnit(bottomUnits));

          
            double topLengthPixel = Convert.ToInt32(topMax / resolution);
            if (topLengthPixel > _scaleLineWidthPixel) { topLengthPixel = _scaleLineWidthPixel; }

            double bottomLengthPixel = Convert.ToInt32(bottomMax / resolution);
            if (bottomLengthPixel > _scaleLineWidthPixel) { bottomLengthPixel = _scaleLineWidthPixel; }

            _topText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", topRounded, GetShortUnitString(topUnits));
            _bottomText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", bottomRounded, GetShortUnitString(bottomUnits));

            DrawScaleLine((float)topLengthPixel, (float)Math.Max(topLengthPixel, bottomLengthPixel), (float)bottomLengthPixel);
        }

        private string GetShortUnitString(DistanceUnit targetUnits)
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

        private static int GetBarLength(int maxLength)
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

        private static double GetInchesPreUnit(DistanceUnit targetUnit)
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

        private static double GetInchesPreUnit(GeographyUnit targetUnit)
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
