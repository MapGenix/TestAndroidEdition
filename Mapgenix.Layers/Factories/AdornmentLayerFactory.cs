using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Styles;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for adornment layers
    /// </summary>
    public static class AdornmentLayerFactory
    {
        public static LogoAdornmentLayer CreateLogoAdornmentLayer(GeoImage image)
        {
            return new LogoAdornmentLayer
            {
                Image = image,
                Location = AdornmentLocation.LowerRight,
                Name = string.Empty,
                IsVisible = true,
                BackgroundMask = new AreaStyle()
            };
            
        }

        public static RestrictionAdornmentLayer CreateRestrictionAdornmentLayer()
        {
            return new RestrictionAdornmentLayer
            {
                RestrictionAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.StandardColors.White),
                Height = 300,
                Width = 300,
                Name = string.Empty,
                IsVisible = true,
                BackgroundMask = new AreaStyle()

            };
        }

      
        public static ScaleLineAdornmentLayer CreateScaleLineAdornmentLayer(ScreenPointF startPoint)
        {
            ScaleLineAdornmentLayer layer = CreateScaleLineAdornmentLayer(AdornmentLocation.UseOffsets);
            layer.XOffsetInPixel = startPoint.X;
            layer.YOffsetInPixel = startPoint.Y;
            return layer;
        }

        public static ScaleLineAdornmentLayer CreateScaleLineAdornmentLayer(AdornmentLocation location)
        {
            Dictionary<string, string> displayUnitString = new Dictionary<string, string>();
            displayUnitString.Add("Meter", "m");
            displayUnitString.Add("Feet", "ft");
            displayUnitString.Add("Kilometer", "km");
            displayUnitString.Add("Mile", "mi");
            displayUnitString.Add("UsSurveyFeet", "usf");
            displayUnitString.Add("Yard", "f");

            return new ScaleLineAdornmentLayer
            {
                Location = location,
                ScaleLineHeightPixel = 14,
                ScaleLineWidthPixel = 150,
                DisplayUnitString = displayUnitString,
                Name = string.Empty,
                IsVisible = true,
                BackgroundMask = new AreaStyle()
            };
        }

        public static GraticuleAdornmentLayer CreateGraticuleAdornmentLayer()
        {
            return new GraticuleAdornmentLayer
            {
                Intervals = GetDefaultIntervals(),
                GraticuleLineStyle = new LineStyle(new GeoPen(GeoColor.FromArgb(255, GeoColor.StandardColors.DarkBlue), 0.5F)),
                GraticuleTextFont = new GeoFont("Arial", 10),
                GraticuleTextBrush = new GeoSolidBrush(GeoColor.FromArgb(255, GeoColor.StandardColors.DarkBlue)),
                GraticuleDensity = 10,
                Location = AdornmentLocation.UseOffsets,
                BackgroundMask = new AreaStyle()
            };
        }

        public static GraticuleAdornmentLayer CreateGraticuleAdornmentLayer(BaseProjection projection)
        {
            if(projection != null)
            {
                projection.Open();
            }
            return new GraticuleAdornmentLayer
            {
                Intervals = GetDefaultIntervals(),
                GraticuleLineStyle = new LineStyle(new GeoPen(GeoColor.FromArgb(255, GeoColor.StandardColors.DarkBlue), 0.5F)),
                GraticuleTextFont = new GeoFont("Arial", 10),
                GraticuleTextBrush = new GeoSolidBrush(GeoColor.FromArgb(255, GeoColor.StandardColors.DarkBlue)),
                GraticuleDensity = 10,
                Projection = projection,
                Location = AdornmentLocation.UseOffsets,
                BackgroundMask = new AreaStyle()
            };
        }

        public static Collection<double> GetDefaultIntervals()
        {
            Collection<double> intervals = new Collection<double>();
            intervals.Add(0.0005);
            intervals.Add(0.001);
            intervals.Add(0.002);
            intervals.Add(0.005);
            intervals.Add(0.01);
            intervals.Add(0.02);
            intervals.Add(0.05);
            intervals.Add(0.1);
            intervals.Add(0.2);
            intervals.Add(0.5);
            intervals.Add(1);
            intervals.Add(2);
            intervals.Add(5);
            intervals.Add(10);
            intervals.Add(20);
            intervals.Add(40);
            intervals.Add(50);
            return intervals;
        }

        public static LegendAdornmentLayer CreateLegendAdornmentLayer()
        {
            return new LegendAdornmentLayer
            {
                Width = 160,
                Height = 200,
                BackgroundMask = AreaStyles.CreateLinearGradientStyle(new GeoColor(255, 255, 255, 255), new GeoColor(255, 230, 230, 230), 90, GeoColor.StandardColors.Black),
                Location = AdornmentLocation.LowerLeft
            };
            
        }
    }
}
