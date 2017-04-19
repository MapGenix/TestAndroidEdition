using System.Collections.Generic;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles;
using Mapgenix.Utils;

namespace Mapgenix.Layers
{
    /// <summary>
    /// Factory class for layers.
    /// </summary>
    public static class LayerFactory
    {
        public static BackgroundLayer CreateBackgroundLayer()
        {
            return CreateBackgroundLayer(new GeoSolidBrush(GeoColor.StandardColors.Transparent));
        }

        public static BackgroundLayer CreateBackgroundLayer(BaseGeoBrush backgroundBrush)
        {
            return new BackgroundLayer
            {
                BackgroundBrush = backgroundBrush,
                Name = string.Empty,
                IsVisible = true
            };
        }

       

        public static HeatLayer CreateHeatLayer(FeatureSource.BaseFeatureSource featureSource)
        {
            return new HeatLayer
            {
                HeatStyle = new HeatStyle(),
                FeatureSource = featureSource,
                UpperScale = double.MaxValue,
                LowerScale = double.MinValue,
                Name = string.Empty,
                IsVisible = true
            };

        }

        public static RestrictionLayer CreateRestrictionLayer(IEnumerable<BaseAreaShape> zones)
        {
            return CreateRestrictionLayer(zones, RestrictionMode.HideZones, double.MaxValue, double.MinValue);
        }

        public static RestrictionLayer CreateRestrictionLayer(IEnumerable<BaseAreaShape> zones, RestrictionMode zonesToShowOrHide)
        {
            return CreateRestrictionLayer(zones, zonesToShowOrHide, double.MaxValue, double.MinValue);
        }

        public static RestrictionLayer CreateRestrictionLayer(IEnumerable<BaseAreaShape> zones, RestrictionMode zonesToShowOrHide, double upperScale, double lowerScale)
        {
            Validators.CheckParameterIsNotNull(zones, "zones");

            SafeCollection<BaseAreaShape> list = new SafeCollection<BaseAreaShape>();
            foreach (BaseAreaShape zone in zones)
            {
                list.Add(zone);
            }
            return new RestrictionLayer
            {
                UpperScale = upperScale,
                LowerScale = lowerScale,
                RestrictionMode = zonesToShowOrHide,
                DefaultStyle = AreaStyles.CreateHatchStyle(GeoHatchStyle.Percent80, GeoColor.StandardColors.LightGray, GeoColor.StandardColors.Black, GeoColor.StandardColors.Transparent, 0, LineDashStyle.Solid, 0, 0),
                CustomStyles = new SafeCollection<BaseStyle>(),
                Zones = list,
                Name = string.Empty,
                IsVisible = true
            };
        }

    }
}
