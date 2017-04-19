using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    /// <summary>A style based on class break values.</summary>
    [Serializable]
    public class ClassBreakStyle : BaseStyle
    {
        private BreakValueInclusion _breakValueInclusion;
        private Collection<ClassBreak> _classBreaks;
        private string _columnName;

        /// <summary>Constructor of the class.</summary>
        public ClassBreakStyle()
            : this(string.Empty, BreakValueInclusion.IncludeValue)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>To pass the column name of the column used for the break values.</overloads>
        /// <returns>None</returns>
        /// <param name="columnName">Column name of the column used for the break values.</param>
        public ClassBreakStyle(string columnName)
            : this(columnName, BreakValueInclusion.IncludeValue)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <param name="columnName">Column name of the column used for the break values.</param>
        /// <param name="breakValueInclusion">Whether the break value is included in the class break
        /// calculation.</param>
        public ClassBreakStyle(string columnName, BreakValueInclusion breakValueInclusion)
            : this(columnName, breakValueInclusion, new Collection<ClassBreak>())
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        /// <param name="columnName">Column name of the column used for the break values.</param>
        /// <param name="breakValueInclusion">Whether the break value is included in the class break
        /// calculation.</param>
        /// <param name="classBreaks">Class break determining which style to use on the break values.</param>
        public ClassBreakStyle(string columnName, BreakValueInclusion breakValueInclusion, Collection<ClassBreak> classBreaks)
        {
            _columnName = columnName;
            _breakValueInclusion = breakValueInclusion;
            _classBreaks = classBreaks;
        }

        /// <summary>Gets and sets the column name in the FeatureSource where the data
        /// is found for each feature.</summary>
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>Gets and sets the value of if the break values are included in the
        /// break calculation.</summary>
        public BreakValueInclusion BreakValueInclusion
        {
            get { return _breakValueInclusion; }
            set { _breakValueInclusion = value; }
        }

        /// <summary>Gets the collection of class breaks.</summary>
        public Collection<ClassBreak> ClassBreaks
        {
            get { return _classBreaks; }
        }

        /// <summary>Draws the features on the canvas.</summary>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels in all layers.</param>
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");
            Validators.CheckColumnNameIsInFeature(_columnName, features);
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            Collection<ClassBreak> sortedClassBreaks = GetSortedClassBreak(_classBreaks);
            Validators.CheckClassBreaksAreValid(sortedClassBreaks);

            foreach (Feature feature in features)
            {
                double fieldValue = double.Parse(feature.ColumnValues[_columnName].Trim(), CultureInfo.CurrentCulture);
                ClassBreak classBreak = GetClassBreak(fieldValue, _breakValueInclusion, sortedClassBreaks);
                if (classBreak == null)
                {
                    continue;
                }
                Feature[] tmpFeatures = { feature };
                if (classBreak.CustomStyles.Count == 0)
                {
                    classBreak.DefaultAreaStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    classBreak.DefaultLineStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    classBreak.DefaultPointStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    classBreak.DefaultTextStyle.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                }
                else
                {
                    foreach (BaseStyle style in classBreak.CustomStyles)
                    {
                        style.Draw(tmpFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

        private static Collection<ClassBreak> GetSortedClassBreak(Collection<ClassBreak> classBreaks)
        {
            List<double> breaks = new List<double>();
            Dictionary<double, ClassBreak> unsortedClassBreaks = new Dictionary<double, ClassBreak>();
            foreach (ClassBreak classBreak in classBreaks)
            {
                breaks.Add(classBreak.Value);
                unsortedClassBreaks.Add(classBreak.Value, classBreak);
            }

            breaks.Sort();

            Collection<ClassBreak> sortedClassBreaks = new Collection<ClassBreak>();

            for (int i = 0; i < breaks.Count; i++)
            {
                sortedClassBreaks.Add(unsortedClassBreaks[breaks[i]]);
            }

            return sortedClassBreaks;
        }

        private static ClassBreak GetClassBreak(double columnValue, BreakValueInclusion breakValueInclusion, Collection<ClassBreak> sortedClassBreaks)
        {
            ClassBreak result = sortedClassBreaks[sortedClassBreaks.Count - 1];
            if (breakValueInclusion == BreakValueInclusion.IncludeValue)
            {
                if (columnValue <= sortedClassBreaks[0].Value)
                {
                    return null;
                }

                for (int i = 1; i < sortedClassBreaks.Count; i++)
                {
                    if (columnValue < sortedClassBreaks[i].Value)
                    {
                        result = sortedClassBreaks[i - 1];
                        break;
                    }
                }
            }
            else
            {
                if (columnValue < sortedClassBreaks[0].Value)
                {
                    return null;
                }

                for (int i = 1; i < sortedClassBreaks.Count; i++)
                {
                    if (columnValue <= sortedClassBreaks[i].Value)
                    {
                        result = sortedClassBreaks[i - 1];
                        break;
                    }
                }
            }

            return result;
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Validators.CheckParameterIsNotNullOrEmpty(_columnName, "columnName");

            Collection<string> requiredFieldNames = new Collection<string>();

            requiredFieldNames.Add(_columnName);
            foreach (ClassBreak classBreak in _classBreaks)
            {
                foreach (BaseStyle style in classBreak.CustomStyles)
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
                Collection<string> fieldsInTextStyle = classBreak.DefaultTextStyle.GetRequiredColumnNames();
                foreach (string fieldName in fieldsInTextStyle)
                {
                    if (!requiredFieldNames.Contains(fieldName))
                    {
                        requiredFieldNames.Add(fieldName);
                    }
                }
            }
            return requiredFieldNames;
        }
    }
}