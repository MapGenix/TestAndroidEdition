using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ciloci.Flee;
using Mapgenix.Canvas;
using Mapgenix.Shapes;

namespace Mapgenix.Styles
{
    public class FleeBooleanStyle : BaseStyle
    {
        private string fleeExpression;
        private Collection<BaseStyle> customTrueStyles;
        private Collection<BaseStyle> customFalseStyles;
        private Dictionary<string, object> userVariables;
        private Collection<string> columnVariables;
        private Collection<Type> staticTypes;

        public FleeBooleanStyle()
            : this(string.Empty, new Collection<BaseStyle>(), new Collection<BaseStyle>())
        {
        }

        public FleeBooleanStyle(string fleeExpression)
            : this(fleeExpression, new Collection<BaseStyle>(), new Collection<BaseStyle>())
        {
        }

        private FleeBooleanStyle(string fleeExpression, Collection<BaseStyle> trueCustomStyles, Collection<BaseStyle> falseCustomStyles)
        {
            this.fleeExpression = fleeExpression;
            this.customTrueStyles = trueCustomStyles;
            this.customFalseStyles = falseCustomStyles;
            userVariables = new Dictionary<string, object>();
            columnVariables = new Collection<string>();
            staticTypes = new Collection<Type>();
        }

        public string FleeExpression
        {
            get { return fleeExpression; }
            set { fleeExpression = value; }
        }

        public Collection<Type> StaticTypes
        {
            get { return staticTypes; }            
        }

        public Collection<string> ColumnVariables
        {
            get { return columnVariables; }            
        }

        public Dictionary<string, object> UserVariables
        {
            get { return userVariables; }            
        }

        public Collection<BaseStyle> CustomTrueStyles
        {
            get { return customTrueStyles; }
        }

        public Collection<BaseStyle> CustomFalseStyles
        {
            get { return customFalseStyles; }
        }

        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            ExpressionContext context = new ExpressionContext();

            foreach (string customVariableKey in userVariables.Keys)
            {
                context.Variables.Add(customVariableKey, userVariables[customVariableKey]);
            }

            foreach (string columnVariable in columnVariables)
            {
                context.Variables.Add(columnVariable, string.Empty);
            }

            foreach (Type staticType in staticTypes)
            {
                context.Imports.AddType(staticType);
            }

            IGenericExpression<bool> e = context.CompileGeneric<bool>(fleeExpression);

            foreach (Feature feature in features)
            {
                foreach (string columnName in columnVariables)
                {
                    context.Variables[columnName] = feature.ColumnValues[columnName];
                }

                bool evaluatedTrue = e.Evaluate();

                if (evaluatedTrue)
                {
                    foreach (BaseStyle style in customTrueStyles)
                    {
                        style.Draw(new Collection<Feature>() { feature }, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
                else
                {
                    foreach (BaseStyle style in customFalseStyles)
                    {
                        style.Draw(new Collection<Feature>() { feature }, canvas, labelsInThisLayer, labelsInAllLayers);
                    }
                }
            }
        }

        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            Collection<string> requiredFieldNames = new Collection<string>();

            foreach (string columnName in columnVariables)
            {
                if (!requiredFieldNames.Contains(columnName))
                {
                    requiredFieldNames.Add(columnName);
                }
            }

            // Custom True Styles
            foreach (BaseStyle style in customTrueStyles)
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

            foreach (BaseStyle style in customFalseStyles)
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

            return requiredFieldNames;
        }
    }
}
