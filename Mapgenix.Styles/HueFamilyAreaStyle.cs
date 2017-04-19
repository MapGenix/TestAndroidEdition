using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles.Properties;

namespace Mapgenix.Styles
{
    /// <summary>Style to draw area features.</summary>
    [Serializable]
    public class HueFamilyAreaStyle : AreaStyle
    {
        private Collection<AreaStyle> _areaStyles;
        private Dictionary<string, AreaStyle> _cache;
        private int _numberOfColors;

        public HueFamilyAreaStyle()
            : this(new GeoPen(), new GeoColor(), 1, PenBrushDrawingOrder.BrushFirst)
        {
        }

       
        public HueFamilyAreaStyle(GeoPen outlinePen, GeoColor baseColor, int numberOfColors)
            : this(outlinePen, baseColor, numberOfColors, PenBrushDrawingOrder.BrushFirst)
        {
        }

        public HueFamilyAreaStyle(GeoPen outlinePen, GeoColor baseColor, int numberOfColors, PenBrushDrawingOrder penBrushDrawingOrder)
        {
            FillSolidBrush.Color = baseColor;
            OutlinePen = outlinePen;
            PenBrushDrawingOrder = penBrushDrawingOrder;
            _numberOfColors = numberOfColors;

            ClearCache();
        }

        public int NumberOfColors
        {
            get { return _numberOfColors; }
            set { _numberOfColors = value; }
        }

        public Dictionary<string, AreaStyle> Cache
        {
            get { return _cache; }
        }

        public GeoColor BaseColor
        {
            get { return FillSolidBrush.Color; }
            set { FillSolidBrush.Color = value; }
        }

        public void ClearCache()
        {
            _cache = new Dictionary<string, AreaStyle>();
            _areaStyles = new Collection<AreaStyle>();
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");

            foreach (Feature feature in features)
            {
                WellKnownType wellKnownType = feature.GetWellKnownType();
                if (wellKnownType == WellKnownType.Polygon || wellKnownType == WellKnownType.Multipolygon)
                {
                    break;
                }
                throw new ArgumentException(ExceptionDescription.FeatureIsInvalidInThisStyle, "features");
            }

            if (_areaStyles.Count == 0)
            {
                Collection<GeoColor> colors = GeoColor.GetColorsInHueFamily(BaseColor, _numberOfColors);
                foreach (GeoColor geoColor in colors)
                {
                    AreaStyle areaStyle = new AreaStyle(OutlinePen, new GeoSolidBrush(geoColor));
                    areaStyle.XOffsetInPixel = XOffsetInPixel;
                    areaStyle.YOffsetInPixel = YOffsetInPixel;
                    foreach (AreaStyle item in CustomAreaStyles)
                    {
                        areaStyle.CustomAreaStyles.Add(item);
                    }
                    areaStyle.AreaStyleCustom.FillCustomBrush = AreaStyleCustom.FillCustomBrush;
                    areaStyle.PenBrushDrawingOrder = PenBrushDrawingOrder;

                    _areaStyles.Add(areaStyle);
                }
            }

            int styleNumber = 0;
            foreach (Feature feature in features)
            {
                string id = feature.Id + feature.GetHashCode().ToString(CultureInfo.InvariantCulture);
                if (!_cache.ContainsKey(id))
                {
                    _cache.Add(id, _areaStyles[styleNumber]);
                }
                _cache[id].Draw(new[] { feature }, canvas, labelsInThisLayer, labelsInAllLayers);

                if (styleNumber == _areaStyles.Count - 1)
                {
                    styleNumber = 0;
                }
                else
                {
                    styleNumber++;
                }
            }
        }
    }
}