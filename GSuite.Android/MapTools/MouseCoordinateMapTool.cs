using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Mapgenix.Shapes;
using Android.Content;
using Android.Widget;
using Android.Graphics;
using NativeAndroid = Android;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// World coordinates from the mouse pointer
    /// </summary>
    [ToolboxItem(false)]
    [DesignTimeVisible(false)]
    public class MouseCoordinateMapTool : BaseMapTool
    {
        //public static readonly DependencyProperty CoordinateTextProperty  = DependencyProperty.Register("CoordinateText", typeof(string), typeof(MouseCoordinateMapTool), new PropertyMetadata("--°--'--\"E  --°--'--\"N"));

        private float _coordinateWidth;
        private float _coordinateHeight;
        private TextView _coordinateText;
        public event EventHandler<CoordinateEventArgs> CustomFormatted;

        public MouseCoordinateMapTool(Context context)
            : base(context, false)
        {
            //DefaultStyleKey = typeof(MouseCoordinateMapTool);

            _coordinateWidth = LayoutUnitsUtil.convertDpToPixel(90, Context.Resources.DisplayMetrics.Xdpi);
            _coordinateHeight = LayoutUnitsUtil.convertDpToPixel(15, Context.Resources.DisplayMetrics.Ydpi);

            RelativeLayout.LayoutParams layout = new RelativeLayout.LayoutParams((int)_coordinateWidth, (int)_coordinateHeight);
            layout.AddRule(LayoutRules.AlignParentEnd);
            layout.AddRule(LayoutRules.AlignParentBottom);
            layout.RightMargin = 10;
            layout.BottomMargin = 0;

            LayoutParameters = layout;
            
            _coordinateText = new TextView(Context) { TextSize = 8 };
            _coordinateText.TextAlignment = NativeAndroid.Views.TextAlignment.TextEnd;
            _coordinateText.SetWidth((int)_coordinateWidth);
            _coordinateText.SetHeight((int)_coordinateHeight);
            _coordinateText.SetTextColor(Color.Black);

            AddView(_coordinateText);

            MouseCoordinateType = MouseCoordinateType.LongitudeLatitude;
            /*HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Bottom;
            Margin = new Thickness(0, 0, 4, 2);
            FontSize = 10.0;*/
        }

        public MouseCoordinateType MouseCoordinateType { get; set; }

        //public string CoordinateText { get { return (string)GetValue(CoordinateTextProperty); } set { SetValue(CoordinateTextProperty, value); } }

        /*public override void OnApplyTemplate()
        {
            InitEvent();
        }*/

        protected override void EnabledChangedCore(bool newValue)
        {
            base.EnabledChangedCore(newValue);
            
        }

        protected override void InitializeCore(Map map)
        {
            base.InitializeCore(map);
            InitEvent();
        }

        protected string OnCustomFormatted(CoordinateEventArgs args)
        {
            string result = String.Empty;
            EventHandler<CoordinateEventArgs> handler = CustomFormatted;
            if (handler != null)
            {
                handler(this, args);
                result = args.Result;
            }

            return result;
        }

        private void InitEvent()
        {
            if (CurrentMap == null) { return; }

            CurrentMap.MapMove -= CurrentMapMouseMove;

            if (Enabled)
            {
                CurrentMap.MapMove += CurrentMapMouseMove;
            }
        }

        private void CurrentMapMouseMove(object sender, MapMotionEventArgs e)
        {            
            _coordinateText.Text = GetMouseCoordinates(e.WorldX, e.WorldY);
        }

        private string GetMouseCoordinates(double lon, double lat)
        {
            switch (MouseCoordinateType)
            {
                case MouseCoordinateType.Default:
                case MouseCoordinateType.LongitudeLatitude:
                    return String.Format(CultureInfo.InvariantCulture, "{0},{1}", lon.ToString(CultureInfo.InvariantCulture), lat.ToString(CultureInfo.InvariantCulture));
                case MouseCoordinateType.LatitudeLongitude:
                    return String.Format(CultureInfo.InvariantCulture, "{0},{1}", lat.ToString(CultureInfo.InvariantCulture), lon.ToString(CultureInfo.InvariantCulture));
                case MouseCoordinateType.DegreesMinutesSeconds:
                    string latitude = CoordinateHelper.ToDms(lat);
                    string longtitude = CoordinateHelper.ToDms(lon);
                    if (lat < 0d)
                    {
                        latitude += "S";
                    }
                    else
                    {
                        latitude += "N";
                    }
                    if (lon < 0d)
                    {
                        longtitude += "W";
                    }
                    else
                    {
                        longtitude += "E";
                    }
                    return String.Format(CultureInfo.InvariantCulture, "{0}, {1}", longtitude, latitude);
                case MouseCoordinateType.Custom:
                    CoordinateEventArgs args = new CoordinateEventArgs(new PointShape(lon, lat));
                    return OnCustomFormatted(args);
                default:
                    return "--°--'--\"E  --°--'--\"N";
            }
        }

       
    }
}
