using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Mapgenix.Shapes;
using Android.Content;

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

        //public event EventHandler<CoordinateEventArgs> CustomFormatted;

        public MouseCoordinateMapTool(Context context)
            : base(context, false)
        {
            //DefaultStyleKey = typeof(MouseCoordinateMapTool);
            
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
            InitEvent();
        }

        protected string OnCustomFormatted(CoordinateEventArgs args)
        {
            string result = String.Empty;
            /*EventHandler<CoordinateEventArgs> handler = CustomFormatted;
            if (handler != null)
            {
                handler(this, args);
                result = args.Result;
            }*/

            return result;
        }

        private void InitEvent()
        {
            if (CurrentMap == null) { return; }

            /*if (IsEnabled)
            {
                CurrentMap.MouseMove += CurrentMapMouseMove;
            }
            else
            {
                CurrentMap.MouseMove -= CurrentMapMouseMove;
            }*/
        }

        /*private void CurrentMapMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point screenCoordinate = e.GetPosition(CurrentMap);
            PointShape lonlat = CurrentMap.ToWorldCoordinate(screenCoordinate);
            CoordinateText = GetMouseCoordinates(lonlat.X, lonlat.Y);
        }*/

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
