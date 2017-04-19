using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;
using Mapgenix.Canvas;

namespace Mapgenix.Styles
{
    /// <summary>Style for a single value to match.</summary>
    [Serializable]
    public class ValueStyle : BaseStyle
    {
        private string _columnName;
        private Collection<ValueItem> _valueItems;

       
        public ValueStyle()
            : this(string.Empty, new Collection<ValueItem>())
        {
        }

      public ValueStyle(string columnName, Collection<ValueItem> valueItems)
        {
            _columnName = columnName;
            _valueItems = valueItems;
        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public Collection<ValueItem> ValueItems
        {
            get { return _valueItems; }
        }

       
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");
            Validators.CheckColumnNameIsInFeature(_columnName, features);

            foreach (Feature feature in features)
            {
                string fieldValue = feature.ColumnValues[_columnName].Trim();
                ValueItem valueItem = GetValueItem(fieldValue);

                Feature[] tmpFeatures = new Feature[1] { feature };
                if (valueItem.CustomStyles.Count == 0)
                {
                    if (valueItem.DefaultAreaStyle != null)
                    {
                        valueItem.DefaultAreaStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                    if (valueItem.DefaultLineStyle != null)
                    {
                        valueItem.DefaultLineStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                    if (valueItem.DefaultPointStyle != null)
                    {
                        valueItem.DefaultPointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                    if (valueItem.DefaultTextStyle != null)
                    {
                        valueItem.DefaultTextStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
                else
                {
                    foreach (BaseStyle style in valueItem.CustomStyles)
                    {
                        style.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

            
        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");

            Collection<string> requiredFieldNames = new Collection<string>();

            requiredFieldNames.Add(_columnName);

            foreach (ValueItem valueItem in _valueItems)
            {
                foreach (BaseStyle style in valueItem.CustomStyles)
                {
                    Collection<string> tmpCollection = style.GetRequiredColumnNames();

                    foreach (string name in tmpCollection)
                    {
                        if (!requiredFieldNames.Contains(name))
                        {
                            requiredFieldNames.Add(name);
                        }
                    }
                }
                if (valueItem.DefaultTextStyle != null)
                {
                    Collection<string> fieldsInTextStyle = valueItem.DefaultTextStyle.GetRequiredColumnNames();
                    foreach (string fieldName in fieldsInTextStyle)
                    {
                        if (!requiredFieldNames.Contains(fieldName))
                        {
                            requiredFieldNames.Add(fieldName);
                        }
                    }
                }
            }

            return requiredFieldNames;
        }

        private ValueItem GetValueItem(string columnValue)
        {
            ValueItem result = null;

            foreach (ValueItem valueItem in _valueItems)
            {
                if (string.Compare(columnValue, valueItem.Value, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    result = valueItem;
                    break;
                }
            }

            if (result == null)
            {
                result = new ValueItem();
            }

            return result;
        }
    }
}