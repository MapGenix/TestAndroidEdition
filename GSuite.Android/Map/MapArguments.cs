using System;
using System.Collections.ObjectModel;
using System.Windows;
using Mapgenix.Shapes;
using Android.Graphics;

namespace Mapgenix.GSuite.Android
{
    [Serializable]
    public class MapArguments
    {
        private Collection<double> _zoomLevelScales;

        public MapArguments()
        {
            _zoomLevelScales = new Collection<double>();
        }

       
        public GeographyUnit MapUnit { get; set; }

        public double ActualWidth { get; set; }

       
        public double ActualHeight { get; set; }

      
        public Collection<double> ZoomLevelScales { get { return _zoomLevelScales; } }

       
        public double CurrentResolution { get; set; }

       
        public double CurrentScale { get; set; }

       
        public double MaximumScale { get; set; }

        public double MinimumScale { get; set; }

     
        public RectangleShape CurrentExtent { get; set; }
        
        public RectangleShape MaxExtent { get; set; }

        public PointF CurrentPointPosition { get; set; }
      
        public PointShape ToWorldCoordinate(PointShape screenCoordinate)
        {
            PointF newPoint = MapUtil.ToWorldCoordinate(CurrentExtent, screenCoordinate.X, screenCoordinate.Y, ActualWidth, ActualHeight);
            return new PointShape(newPoint.X, newPoint.Y);
        }

     
        public PointShape ToScreenCoordinate(PointShape worldCoordinate)
        {
            PointF newPoint = MapUtil.ToScreenCoordinate(CurrentExtent, worldCoordinate.X, worldCoordinate.Y, ActualWidth, ActualHeight);
            return new PointShape(newPoint.X, newPoint.Y);
        }

      
        public int GetSnappedZoomLevelIndex(RectangleShape extent)
        {
            return MapUtil.GetSnappedZoomLevelIndex(extent, MapUnit, ZoomLevelScales, ActualWidth, ActualHeight);
        }

        public int GetSnappedZoomLevelIndex(double scale)
        {
            return MapUtil.GetSnappedZoomLevelIndex(scale, ZoomLevelScales);
        }

       
        public RectangleShape GetSnappedExtent(RectangleShape extent)
        {
            int snappedLevel = GetSnappedZoomLevelIndex(extent);
            double newScale = ZoomLevelScales[snappedLevel];
            PointShape newCenter = extent.GetCenterPoint();
            return MapUtil.CalculateExtent(new PointF(Convert.ToSingle(newCenter.X), Convert.ToSingle(newCenter.Y)), newScale, MapUnit, ActualWidth, ActualHeight);
        }
    }
}
