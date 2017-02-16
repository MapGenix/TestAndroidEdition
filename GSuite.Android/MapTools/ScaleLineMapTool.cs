using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
/*using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;*/
using Mapgenix.Shapes;
using Android.Content;

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
        /*private System.Windows.Controls.Canvas _scaleLineCanvas;
        private TextBlock _topText;
        private TextBlock _bottomText;
        private DispatcherTimer _updateTimer;

        private Line _topLine;
        private Line _middleLine;
        private Line _bottomLine;*/

       
        public ScaleLineMapTool(Context context)
            : this(context, false)
        { }

      
        public ScaleLineMapTool(Context context, bool isEnabled)
            : base(context, isEnabled)
        {
            /*DefaultStyleKey = typeof(ScaleLineMapTool);

            Margin = new System.Windows.Thickness(5);
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromMilliseconds(50);
            _updateTimer.Tick += updateTimer_Tick;

            _scaleLineHeightPixel = 12;
            _scaleLineWidthPixel = 100;

            _topText = new TextBlock() { FontSize = 10 };
            _topText.SetValue(System.Windows.Controls.Canvas.LeftProperty, 3.0);

            _bottomText = new TextBlock() { FontSize = 10 };
            _bottomText.SetValue(System.Windows.Controls.Canvas.LeftProperty, 3.0);
            _bottomText.SetValue(System.Windows.Controls.Canvas.TopProperty, _scaleLineHeightPixel);

            _topLine = GetLine(0, 2, 0, _scaleLineHeightPixel);
            _middleLine = GetLine(0, _scaleLineHeightPixel, 0, _scaleLineHeightPixel);
            _bottomLine = GetLine(0, _scaleLineHeightPixel, 0, _scaleLineHeightPixel * 2 - 2);

            _displayUnitString = new Dictionary<string, string>();
            _displayUnitString.Add("Meter", "m");
            _displayUnitString.Add("Feet", "ft");
            _displayUnitString.Add("Kilometer", "km");
            _displayUnitString.Add("Mile", "mi");
            _displayUnitString.Add("UsSurveyFeet", "usf");
            _displayUnitString.Add("Yard", "f");*/
        }

       
        /*public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scaleLineCanvas = (System.Windows.Controls.Canvas)GetTemplateChild("Canvas");
            Name = "ScaleLine" + CurrentMap.Name;
            SetValue(Grid.ColumnProperty, 0);
            SetValue(Grid.RowProperty, 2);
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

            EnabledChangedCore(IsEnabled);
        }*/

      
        /*protected override void InitializeCore(Map wpfMap)
        {
            base.InitializeCore(wpfMap);
            CurrentMap.CurrentExtentChanged -= MapExtentChanged;
            CurrentMap.CurrentExtentChanged += MapExtentChanged;
            UpdateBarItems(CurrentMap.CurrentExtent);
        }*/

      
        protected override void EnabledChangedCore(bool newValue)
        {
            /*base.EnabledChangedCore(newValue);
            if (_scaleLineCanvas != null)
            {
                _scaleLineCanvas.Visibility = IsEnabled ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                if (IsEnabled)
                {
                    _scaleLineCanvas.Children.Clear();
                    _scaleLineCanvas.Width = _scaleLineWidthPixel;
                    _scaleLineCanvas.Height = _scaleLineHeightPixel * 2;

                    _scaleLineCanvas.Children.Add(_topText);
                    _scaleLineCanvas.Children.Add(_bottomText);
                    _scaleLineCanvas.Children.Add(GetLine(0, 2, 0, _scaleLineHeightPixel * 2 - 2));
                    _scaleLineCanvas.Children.Add(_topLine);
                    _scaleLineCanvas.Children.Add(_middleLine);
                    _scaleLineCanvas.Children.Add(_bottomLine);
                }
            }*/
        }

        private void MapExtentChanged(object sender, ExtentChangedEventArgs e)
        {
            /*if (IsEnabled)
            {
                if (_updateTimer.IsEnabled)
                {
                    _updateTimer.Stop();
                }

                _updateTimer.Start();
            }*/
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            /*UpdateBarItems(CurrentMap.CurrentExtent);
            _updateTimer.Stop();*/
        }

        private void UpdateBarItems(RectangleShape extent)
        {
            DistanceUnit topInUnits = DistanceUnit.Meter;
            DistanceUnit topOutUnits = DistanceUnit.Kilometer;
            DistanceUnit bottomOutUnits = DistanceUnit.Mile;
            DistanceUnit bottomInUnits = DistanceUnit.Feet;

            DistanceUnit bottomUnits;
            DistanceUnit topUnits;

            double resolution = extent.Width / CurrentMap.Width;

       
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

            /*_topText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", topRounded, GetShortUnitString(topUnits));
            _bottomText.Text = string.Format(CultureInfo.InvariantCulture, "{0} {1}", bottomRounded, GetShortUnitString(bottomUnits));

            _topLine.X1 = topLengthPixel;
            _topLine.X2 = topLengthPixel;

          
            _middleLine.X2 = Math.Max(topLengthPixel, bottomLengthPixel);

           
            _bottomLine.X1 = bottomLengthPixel;
            _bottomLine.X2 = bottomLengthPixel;*/
        }

        /*private static Line GetLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            line.X1 = x1;
            line.X2 = x2;
            line.Y1 = y1;
            line.Y2 = y2;

            return line;
        }*/

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
