using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Zoom level based on map scale to control drawing of features.</summary>
    [Serializable]
    public class ZoomLevel
    {
        private readonly Collection<BaseStyle> _customStyles;
        private ApplyUntilZoomLevel _applyUntilZoomLevel;
        private AreaStyle _defaultAreaStyle;
        private LineStyle _defaultLineStyle;
        private PointStyle _defaultPointStyle;
        private LabelStyle _defaultTextStyle;
        private bool _isActive;
        private string _name;
        private double _scale;

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Default constructor. Set the properties manually.</overloads>
        /// <returns>None</returns>
        public ZoomLevel()
            : this(0)
        {
        }

        /// <summary>Constructor of the class.</summary>
        /// <overloads>Provides the scale for this ZoomLevel.</overloads>
        /// <returns>None</returns>
        /// <param name="scale">Scale for the ZoomLevel.</param>
        public ZoomLevel(double scale)
        {
            _scale = scale;
            _isActive = true;
            _customStyles = new Collection<BaseStyle>();
        }

        /// <summary>Gets and sets whether the ZoomLevel is active and should draw.</summary>
        /// <value>Whether the ZoomLevel is active and draws.</value>
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        /// <summary>Gets and sets the scale for the ZoomLevel.</summary>
        /// <value>Scale for the ZoomLevel.</value>
        /// <remarks>The definition of scale is: The ratio or relationship between a distance or area on a map and the corresponding distance or area
        /// on the ground, commonly expressed as a fraction or ratio. A map scale of 1/100,000 or 1:100,000 means that one unit of measure on the map
        /// equals 100,000 of the same unit on the earth.</remarks>
        public double Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        /// <summary>Gets and sets the default AreaStyle for the zoom level.</summary>
        /// <value>Default AreaStyle for the zoom level.</value>
        public AreaStyle DefaultAreaStyle
        {
            get
            {
                if (_defaultAreaStyle == null)
                {
                    _defaultAreaStyle = new AreaStyle();
                }
                return _defaultAreaStyle;
            }
            set { _defaultAreaStyle = value; }
        }

        /// <summary>Gets and sets the default LineStyle for the zoom level.</summary>
        /// <value>Default LineStyle for the zoom level.</value>
        public LineStyle DefaultLineStyle
        {
            get
            {
                if (_defaultLineStyle == null)
                {
                    _defaultLineStyle = new LineStyle();
                }
                return _defaultLineStyle;
            }
            set { _defaultLineStyle = value; }
        }

        /// <summary>Gets and sets the default PointStyle for the zoom level.</summary>
        /// <value>Default PointStyle for the zoom level.</value>
        public PointStyle DefaultPointStyle
        {
            get
            {
                if (_defaultPointStyle == null)
                {
                    _defaultPointStyle = new PointStyle();
                }
                return _defaultPointStyle;
            }
            set { _defaultPointStyle = value; }
        }

        /// <summary>Gets and sets the default LabelStyle (TextStyle) for the zoom level.</summary>
        /// <value>Default LabelStyle for the zoom level.</value>
        public LabelStyle DefaultTextStyle
        {
            get
            {
                if (_defaultTextStyle == null)
                {
                    _defaultTextStyle = new LabelStyle();
                }
                return _defaultTextStyle;
            }
            set { _defaultTextStyle = value; }
        }

        /// <summary>Gets and sets the zoom level until which this zoom level's styles applies.</summary>
        /// <value>Zoom level until which this zoom level's styles applies.</value>
        /// <remarks>Zoom level 01 is at the world level. Zoom level 20 is at the street level.
        /// Use this property, for example, to avoid drawing local street at high zoom levels.</remarks>
        public ApplyUntilZoomLevel ApplyUntilZoomLevel
        {
            get { return _applyUntilZoomLevel; }
            set { _applyUntilZoomLevel = value; }
        }

        /// <summary>Gets and sets the name of the ZoomLevel.</summary>
        /// <value>Name of the ZoomLevel.</value>
        /// <remarks>Usefull for legend etc.</remarks>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>Gets a collection of custom styles for the zoom level.</summary>
        /// <value>Collection of custom styles for the zoom level.</value>
        /// <remarks>It recommended to use DefaultAreaStyle, DefaultLineStyle etc it you need to draw only one style.</remarks>
        public Collection<BaseStyle> CustomStyles
        {
            get { return _customStyles; }
        }

        internal bool IsDefault
        {
            get
            {
                var isDefault = !((_customStyles != null && _customStyles.Count != 0) ||
                                  (_defaultAreaStyle != null && !_defaultAreaStyle.IsDefault) ||
                                  (_defaultLineStyle != null && !_defaultLineStyle.IsDefault) ||
                                  (_defaultPointStyle != null && !_defaultPointStyle.IsDefault) ||
                                  (_defaultTextStyle != null && !_defaultTextStyle.IsDefault));

                var allIsInActive = true;
                if (_customStyles.Count != 0)
                {
                    foreach (var style in _customStyles)
                    {
                        if (style.IsActive)
                        {
                            allIsInActive = false;
                            break;
                        }
                    }

                    isDefault = allIsInActive;
                }

                return isDefault;
            }
        }

       
        /// <summary>Returns the column data for each feature that is required for the styles to draw.</summary>
        /// <returns>Collection containing the required column names.</returns>
        public Collection<string> GetRequiredColumnNames()
        {
            var requiredColumnNames = new Collection<string>();

            if (_customStyles.Count == 0)
            {
                if (_defaultAreaStyle != null)
                {
                    GetColumnNames(_defaultAreaStyle, requiredColumnNames);
                }

                if (_defaultLineStyle != null)
                {
                    GetColumnNames(_defaultLineStyle, requiredColumnNames);
                }

                if (_defaultPointStyle != null)
                {
                    GetColumnNames(_defaultPointStyle, requiredColumnNames);
                }

                if (_defaultTextStyle != null)
                {
                    GetColumnNames(_defaultTextStyle, requiredColumnNames);
                }
            }
            else
            {
                foreach (var style in _customStyles)
                {
                    GetColumnNames(style, requiredColumnNames);
                }
            }

            return requiredColumnNames;
        }

        /// <summary>Draws a collection of features of the ZoomLevel.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of abstract method DrawCore.<br/><br/>
        ///     As a concrete public method that wraps a Core method, Mapegenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</para>
        /// </remarks>
        /// <param name="canvas">Canvas used to draw the features.</param>
        /// <param name="features">Collection of features to draw.</param>
        /// <param name="currentLayerLabels">Collection of labels in the current layer.</param>
        /// <param name="allLayerLabels">Collection of labels in all layers.</param>
        public void Draw(BaseGeoCanvas canvas, IEnumerable<Feature> features, Collection<SimpleCandidate> currentLayerLabels,
            Collection<SimpleCandidate> allLayerLabels)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
           
            if (IsActive)
            {
                DrawCore(canvas, features, currentLayerLabels, allLayerLabels);
            }
        }

        /// <summary>Draws a collection of shapes of the ZoomLevel.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of abstract method DrawCore.<br/><br/>
        ///     As a concrete public method that wraps a Core method, Mapegenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</para>
        /// </remarks>
        /// <param name="canvas">Canvas used to draw the features.</param>
        /// <param name="shapes">Collection of shapes to draw.</param>
        /// <param name="currentLayerLabels">Collection of labels in the current layer.</param>
        /// <param name="allLayerLabels">Collection of labels in all layers.</param>
        public void Draw(BaseGeoCanvas canvas, IEnumerable<BaseShape> shapes, Collection<SimpleCandidate> currentLayerLabels,
            Collection<SimpleCandidate> allLayerLabels)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(shapes, "shapes");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.DefaultAndCustomStyleDuplicateForZoomLevel(this);

            var features = new Collection<Feature>();
            foreach (var shape in shapes)
            {
                features.Add(new Feature(shape));
            }

            Draw(canvas, features, currentLayerLabels, allLayerLabels);
        }

        /// <summary>Draws the features on the canvas passed in.</summary>
        /// <remarks>Called from the concrete public method Draw. 
        /// When overriding this method, consider each feature and its column data values.If column data for a
        /// feature is needed, be sure to override GetRequiredColumnNamesCore and add the columns to the collection.</remarks>
        /// <returns>None</returns>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to drawn in all layers.</param>
        protected virtual void DrawCore(BaseGeoCanvas canvas, IEnumerable<Feature> features,
            Collection<SimpleCandidate> currentLayerLabels, Collection<SimpleCandidate> allLayerLabels)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(features, "features");
            Validators.CheckArrayItemsBigerThan0(features, "features");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            if (_customStyles.Count == 0)
            {
                var featuresCollection = GetFeaturesByStyles(features);
                foreach (var featureCollection in featuresCollection)
                {
                    switch (featureCollection[0].GetWellKnownType())
                    {
                        case WellKnownType.Point:
                        case WellKnownType.Multipoint:
                            if (_defaultPointStyle != null)
                            {
                                _defaultPointStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            break;
                        case WellKnownType.Line:
                        case WellKnownType.Multiline:
                            if (_defaultLineStyle != null)
                            {
                                _defaultLineStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            break;
                        case WellKnownType.Polygon:
                        case WellKnownType.Multipolygon:
                            if (_defaultAreaStyle != null)
                            {
                                _defaultAreaStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            break;
                        case WellKnownType.GeometryCollection:
                            if (_defaultAreaStyle != null)
                            {
                                _defaultAreaStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            if (_defaultLineStyle != null)
                            {
                                _defaultLineStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            if (_defaultPointStyle != null)
                            {
                                _defaultPointStyle.Draw(featureCollection, canvas, currentLayerLabels, allLayerLabels);
                            }
                            break;
                    }
                }


                if (_defaultTextStyle != null && !string.IsNullOrEmpty(_defaultTextStyle.TextColumnName))
                {
                    _defaultTextStyle.Draw(features, canvas, currentLayerLabels, allLayerLabels);
                }
            }
            else
            {
                foreach (var style in _customStyles)
                {
                    style.Draw(features, canvas, currentLayerLabels, allLayerLabels);
                }
            }
        }

        private static void GetColumnNames(BaseStyle style, Collection<string> columnNames)
        {
            var tmpRequiredFieldNames = style.GetRequiredColumnNames();

            foreach (var name in tmpRequiredFieldNames)
            {
                if (columnNames.Count == 0)
                {
                    columnNames.Add(name);
                }
                else
                {
                    foreach (var item in columnNames)
                    {
                        if (!item.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            columnNames.Add(name);
                            break;
                        }
                    }
                }
            }
        }

        private static Collection<Collection<Feature>> GetFeaturesByStyles(IEnumerable<Feature> allFeatures)
        {
            var featuresCollection = new Collection<Collection<Feature>>();
            var features = new Collection<Feature>();
            var bufferType = 0;
            foreach (var feature in allFeatures)
            {
                var featureType = feature.GetWellKnownType();
                if (bufferType == 0)
                {
                    switch (featureType)
                    {
                        case WellKnownType.Point:
                        case WellKnownType.Multipoint:
                            bufferType = 1;
                            break;
                        case WellKnownType.Line:
                        case WellKnownType.Multiline:
                            bufferType = 2;
                            break;
                        case WellKnownType.Polygon:
                        case WellKnownType.Multipolygon:
                            bufferType = 3;
                            break;
                        case WellKnownType.GeometryCollection:
                            bufferType = 4;
                            break;
                        default:
                            break;
                    }
                }
                switch (featureType)
                {
                    case WellKnownType.Point:
                    case WellKnownType.Multipoint:
                        if (bufferType != 1)
                        {
                            featuresCollection.Add(features);
                            features = new Collection<Feature>();
                            bufferType = 1;
                        }
                        break;
                    case WellKnownType.Line:
                    case WellKnownType.Multiline:
                        if (bufferType != 2)
                        {
                            featuresCollection.Add(features);
                            features = new Collection<Feature>();
                            bufferType = 2;
                        }
                        break;
                    case WellKnownType.Polygon:
                    case WellKnownType.Multipolygon:
                        if (bufferType != 3)
                        {
                            featuresCollection.Add(features);
                            features = new Collection<Feature>();
                            bufferType = 3;
                        }
                        break;
                    case WellKnownType.GeometryCollection:
                        if (bufferType != 4)
                        {
                            featuresCollection.Add(features);
                            features = new Collection<Feature>();
                            bufferType = 4;
                        }
                        break;
                    default:
                        break;
                }
                features.Add(feature);
            }
            featuresCollection.Add(features);

            return featuresCollection;
        }
    }
}