using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    /// <summary>Style using GradientBrush specifying LowerValue and UpperValue and its color.
    /// The color of each feature is based on a column value using linear interpolation.
    /// </summary>
    [Serializable]
    public class GradientStyle : BaseStyle
    {
        private string _columnName;
        private GeoSolidBrush _lowerGeoSolidBrush;
        private double _lowerValue;
        private GeoSolidBrush _noDataSolidBrush;
        private double _noDataValue;
        private GeoSolidBrush _upperGeoSolidBrush;
        private double _upperValue;

       
        public GradientStyle(string columnName, double lowerValue, GeoSolidBrush lowerGeoSolidBrush, double upperValue, GeoSolidBrush upperGeoSolidBrush)
            : this(columnName, lowerValue, lowerGeoSolidBrush, upperValue, upperGeoSolidBrush, -9999, new GeoSolidBrush(GeoColor.StandardColors.Transparent))
        {
        }

        public GradientStyle(string columnName, double lowerValue, GeoSolidBrush lowerGeoSolidBrush, double upperValue, GeoSolidBrush upperGeoSolidBrush, double noDataValue, GeoSolidBrush noDataSolidBrush)
        {
            _columnName = columnName;
            _lowerValue = lowerValue;
            _lowerGeoSolidBrush = lowerGeoSolidBrush;
            _upperValue = upperValue;
            _upperGeoSolidBrush = upperGeoSolidBrush;
            _noDataValue = noDataValue;
            _noDataSolidBrush = noDataSolidBrush;
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public double LowerValue
        {
            get { return _lowerValue; }
            set { _lowerValue = value; }
        }

        public GeoSolidBrush LowerGeoSolidBrush
        {
            get { return _lowerGeoSolidBrush; }
            set { _lowerGeoSolidBrush = value; }
        }

        public double UpperValue
        {
            get { return _upperValue; }
            set { _upperValue = value; }
        }

        public GeoSolidBrush UpperGeoSolidBrush
        {
            get { return _upperGeoSolidBrush; }
            set { _upperGeoSolidBrush = value; }
        }

        public double NoDataValue
        {
            get { return _noDataValue; }
            set { _noDataValue = value; }
        }

        public GeoSolidBrush NoDataSolidBrush
        {
            get { return _noDataSolidBrush; }
            set { _noDataSolidBrush = value; }
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");
            Validators.CheckColumnNameIsInFeature(_columnName, features);

            GeoColor lowerColor = _lowerGeoSolidBrush.Color;
            GeoColor upperColor = _upperGeoSolidBrush.Color;
            foreach (Feature feature in features)
            {
                double fieldValue = double.Parse(feature.ColumnValues[_columnName].Trim(), CultureInfo.CurrentCulture);
                GeoColor featureColor = GetColor(lowerColor, upperColor, fieldValue);
                AreaStyle areaStyle = new AreaStyle(new GeoSolidBrush(featureColor));
                Feature[] tmpFeatures = new Feature[1] { feature };
                areaStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
            }
        }

        private GeoColor GetColor(GeoColor lowerColor, GeoColor upperColor, double value)
        {
            GeoColor geoColor;
            if (value == _noDataValue || double.IsNaN(value))
            {
                geoColor = _noDataSolidBrush.Color;
            }
            else
            {
                double ratio = (value - _lowerValue) / (_upperValue - _lowerValue);
                int differenceA = Convert.ToInt32((Convert.ToInt32(upperColor.AlphaComponent) - Convert.ToInt32(lowerColor.AlphaComponent)) * ratio);
                int differenceR = Convert.ToInt32((Convert.ToInt32(upperColor.RedComponent) - Convert.ToInt32(lowerColor.RedComponent)) * ratio);
                int differenceG = Convert.ToInt32((Convert.ToInt32(upperColor.GreenComponent) - Convert.ToInt32(lowerColor.GreenComponent)) * ratio);
                int differenceB = Convert.ToInt32((Convert.ToInt32(upperColor.BlueComponent) - Convert.ToInt32(lowerColor.BlueComponent)) * ratio);

                geoColor = GeoColor.FromArgb(lowerColor.AlphaComponent + differenceA, lowerColor.RedComponent + differenceR, lowerColor.GreenComponent + differenceG, lowerColor.BlueComponent + differenceB);
            }

            return geoColor;
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");

            Collection<string> requiredFieldNames = new Collection<string>();

            requiredFieldNames.Add(_columnName);

            return requiredFieldNames;
        }
    }
}