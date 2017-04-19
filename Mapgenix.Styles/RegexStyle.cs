using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    /// <summary>
    /// Style for the use of regular expression matching.
    /// </summary>
    [Serializable]
    public class RegexStyle : BaseStyle
    {
        private string _columnName;
        private Collection<RegexItem> _regexItems;
        private RegexMatching _regexMatchingRule;

      
        public RegexStyle()
            : this(string.Empty, new Collection<RegexItem>(), RegexMatching.MatchFirstOnly)
        {
        }

        public RegexStyle(string columnName, Collection<RegexItem> regexItems)
            : this(columnName, regexItems, RegexMatching.MatchFirstOnly)
        {
        }

        public RegexStyle(string columnName, Collection<RegexItem> regexItems, RegexMatching regexMatching)
        {
            _columnName = columnName;
            _regexItems = regexItems;
            _regexMatchingRule = regexMatching;
        }

        public RegexMatching RegexMatchingRule
        {
            get { return _regexMatchingRule; }
            set { _regexMatchingRule = value; }
        }

        
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

       
        public Collection<RegexItem> RegexItems
        {
            get { return _regexItems; }
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

            foreach (Feature feature in features)
            {
                string fieldValue = feature.ColumnValues[_columnName].Trim();
                Collection<RegexItem> regexItemCollection = GetRegexItems(fieldValue, _regexMatchingRule);

                Feature[] tmpFeatures = new Feature[1] { feature };
                foreach (RegexItem regexItem in regexItemCollection)
                {
                    if (regexItem.CustomStyles.Count == 0)
                    {
                        regexItem.DefaultAreaStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                        regexItem.DefaultLineStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                        regexItem.DefaultPointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                        regexItem.DefaultTextStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                    else
                    {
                        foreach (BaseStyle style in regexItem.CustomStyles)
                        {
                            style.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                        }
                    }
                }
            }
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");

            Collection<string> requiredFieldNames = new Collection<string>();

            requiredFieldNames.Add(_columnName);
            foreach (RegexItem regexItem in _regexItems)
            {
                foreach (BaseStyle style in regexItem.CustomStyles)
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
            }
            return requiredFieldNames;
        }

        private Collection<RegexItem> GetRegexItems(string columnValue, RegexMatching regexMatching)
        {
            Collection<RegexItem> results = new Collection<RegexItem>();
            foreach (RegexItem regexItem in _regexItems)
            {
                Regex regex = new Regex(regexItem.RegularExpression);
                bool isMatch = regex.IsMatch(columnValue);
                if (isMatch)
                {
                    results.Add(regexItem);
                    if (regexMatching == RegexMatching.MatchFirstOnly)
                    {
                        break;
                    }
                }
            }

            return results;
        }
    }
}