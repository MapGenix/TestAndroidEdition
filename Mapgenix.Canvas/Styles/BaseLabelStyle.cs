using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Mapgenix.Shapes;


namespace Mapgenix.Canvas
{
    /// <summary>Abstract class encapsulating the labeling drawing and position logic.</summary>
    /// <remarks>Is inherited by other styles for labeling such as LabelStyle.</remarks>
    [Serializable]
    public abstract class BaseLabelStyle : BaseStyle
    {
        private readonly LabelStyleCustom _advanced;
        private readonly Collection<LabelStyle> _customTextStyles;
        private bool _allowLineCarriage;
        private bool _allowSpline;
        private bool _bestPlacement;
        private WellKnownType _currentShapeWellKnownType;
        private string _dateFormat;
        private DrawingLevel _drawingLevel;
        private bool _fittingPolygon;
        private double _fittingPolygonFactor;
        private GeoFont _font;
        private bool _forceHorizontalLabelForLine;
        private bool _forceLineCarriage;

        private int _gridSize;
        private GeoPen _haloPen;
        private bool _labelAllLineParts;
        private bool _labelAllPolygonParts;
        private LabelDuplicateRule _labelDuplicateRule;
        private LabelOverlappingRule _labelOverlappintRule;
        private Dictionary<string, WorldLabelingCandidate> _labelPositions;
        private AreaStyle _mask;
        private int _maskMargin;

        private string _numericFormat;
        private PointPlacement _pointPlacement;
        private PolygonLabelingLocationMode _polygonLabelingLocationMode;
        private bool _restrictLabelInScreen;
        private double _rotationAngle;
        private SplineType _splineType;
        private bool _suppressPartialLabels;
        private string _textColumnName;
        private string _textFormat;
        private double _textLineSegmentRatio;
        private GeoSolidBrush _textSolidBrush;
        private float _xOffsetInPixel;
        private float _yOffsetInPixel;

        /// <summary>Default constructor of the class.</summary>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        protected BaseLabelStyle()
        {
            _customTextStyles = new Collection<LabelStyle>();
            _advanced = new LabelStyleCustom();
            _haloPen = new GeoPen();
            _pointPlacement = PointPlacement.CenterRight;
            _splineType = SplineType.Default;
            _labelPositions = new Dictionary<string, WorldLabelingCandidate>();
            _drawingLevel = DrawingLevel.LabelLevel;
        }

        /// <summary>Returns a value representing a keyValuepair (feature id and label position of a features).</summary>
        protected Dictionary<string, WorldLabelingCandidate> LabelPositions
        {
            get
            {
                if (_labelPositions == null)
                {
                    _labelPositions = new Dictionary<string, WorldLabelingCandidate>();
                }
                return _labelPositions;
            }
        }

        /// <summary>Gets and sets the mode determining a polygon labeling location.</summary>
        /// <value>Mode determining a polygon labeling location.</value>
        /// <remarks>The two ways to establish a polygon's labeling location area:
        /// 1) Polygon's centroid 2) Polygon's boungdingbox center.</remarks>
        protected PolygonLabelingLocationMode PolygonLabelingLocationMode
        {
            get { return _polygonLabelingLocationMode; }
            set { _polygonLabelingLocationMode = value; }
        }

        /// <summary>Gets and sets the X offset in pixel for drawing labels.</summary>
        /// <value>X pixel offset in pixel for drawing each labels.</value>
        /// <remarks>Useful to adjust the position of a label, for example, in relation to the point it represents.</remarks>
        protected float XOffsetInPixel
        {
            get { return _xOffsetInPixel; }
            set { _xOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the Y offset in pixel for drawing labels.</summary>
        /// <value>Y pixel offset in pixel for drawing each labels.</value>
        /// <remarks>Useful to adjust the position of a label, for example, in relation to the point it represents.</remarks>
        protected float YOffsetInPixel
        {
            get { return _yOffsetInPixel; }
            set { _yOffsetInPixel = value; }
        }

        /// <summary>Gets and sets the grid size used for deterministic labeling.</summary>
        /// <value>Grid size for deterministic labeling.</value>
        /// <remarks>Grid size determines how many labels will be considered as candidates for drawing by grid. The
        /// smaller the grid size, the higher the density of candidates. The smaller the grid, 
        /// the more the drawing speed performance is negatively impacted.</remarks>
        protected int GridSize
        {
            get { return _gridSize; }
            set
            {
                _gridSize = value;
                Validators.CheckValueIsBiggerThanZero(_gridSize, "gridSize");
            }
        }

        /// <summary>Returns a collection of label styles to stack multiple label styles on top of each other.</summary>
        /// <value>collection of label styles.</value>
        protected Collection<LabelStyle> CustomTextStyles
        {
            get { return _customTextStyles; }
        }

        /// <summary>Gets and sets the rotation angle of the label being positioned.</summary>
        /// <value>Rotation angle of the item being positioned.</value>
        /// <remarks>None</remarks>
        protected double RotationAngle
        {
            get { return _rotationAngle; }
            set { _rotationAngle = value; }
        }

        ///<summary>Gets and sets whether the labeler intends to fit the label on the visible part of a line on the screen.</summary>
        ///<value>Whether the labeler intends to fit the label on the visible part of the line or not.</value>
        protected bool FittingLineInScreen
        {
            get { return _restrictLabelInScreen; }
            set { _restrictLabelInScreen = value; }
        }

        /// <summary>Gets and sets the format for the text of the label.</summary>
        /// <value>Format for the text of the label.</value>
        protected string TextFormat
        {
            get { return _textFormat; }
            set { _textFormat = value; }
        }

        /// <summary>Gets and sets the numeric format for the text of the label.</summary>
        /// <value>Numeric format for the text of the label.</value>
        protected string NumericFormat
        {
            get { return _numericFormat; }
            set { _numericFormat = value; }
        }

        /// <summary>Gets and sets the date format for the text of the label.</summary>
        /// <value>Date format for the text of the label.</value>
        protected string DateFormat
        {
            get { return _dateFormat; }
            set { _dateFormat = value; }
        }

        /// <summary>Gets and sets the SolidBrush to draw the text of the label.</summary>
        /// <value>This property gets the SolidBrush that will be used to draw the text.</value>
        /// <remarks>To draw a solid color. For other brushes, use Advanced property.</remarks>
        protected GeoSolidBrush TextSolidBrush
        {
            get { return _textSolidBrush; }
            set { _textSolidBrush = value; }
        }

        /// <summary>Gets and sets the font to draw the text of the label.</summary>
        /// <value>Font to draw the text of the label.</value>
        /// <remarks>None</remarks>
        protected GeoFont Font
        {
            get { return _font; }
            set { _font = value; }
        }

        /// <summary>Gets and sets a pen to draw a halo around the text of the label.</summary>
        /// <value>Pen to draw a halo around the text of the label</value>
        /// <remarks>Halo effect makes the text stand out.</remarks>
        protected GeoPen HaloPen
        {
            get { return _haloPen; }
            set { _haloPen = value; }
        }

        /// <summary>Gets and sets the column name in the data to get the text from.</summary>
        /// <value>Column name in the data to get the text from.</value>
        /// <remarks>To retrieve text from a feature specifying the name of the column that contains the text to draw.</remarks>
        protected string TextColumnName
        {
            get { return _textColumnName; }
            set { _textColumnName = value; }
        }

        /// <summary>Gets and sets the AreaStyle to draw a mask behind the text.</summary>
        /// <value>AreaStyle to draw a mask behind the text.</value>
        /// <remarks>Halo and mask are similar in their effect of making highlight the text.</remarks>
        protected AreaStyle Mask
        {
            get { return _mask; }
            set { _mask = value; }
        }

        /// <summary>Gets and sets the margin around the text for the mask.</summary>
        /// <value>Margin around the text for the mask.</value>
        /// <remarks>Determines how much larger the mask is than the text.</remarks>
        protected int MaskMargin
        {
            get { return _maskMargin; }
            set { _maskMargin = value; }
        }

        /// <summary>Gets the custom properties of LabelStyle.</summary>
        /// <value>Gets he custom properties of LabelStyle.</value>
        protected LabelStyleCustom Advanced
        {
            get { return _advanced; }
        }

        /// <summary>Gets and sets the rule determining how duplication of labels is handled.</summary>
        /// <value>Rule determining how duplication of labels is handled.</value>
        protected LabelDuplicateRule DuplicateRule
        {
            get { return _labelDuplicateRule; }
            set { _labelDuplicateRule = value; }
        }

        /// <summary>Gets and sets the rule determining how overlapping labels are handled.</summary>
        /// <value>Rule determining how overlapping labels are handled.</value>
        protected LabelOverlappingRule OverlappingRule
        {
            get { return _labelOverlappintRule; }
            set { _labelOverlappintRule = value; }
        }

        /// <summary>Gets and sets whether line labels spline around curved lines.</summary>
        /// <value>Whether line labels spline around curved lines.</value>
        /// <remarks>Useful for curved streets.</remarks>
        protected bool AllowSpline
        {
            get { return _allowSpline; }
            set { _allowSpline = value; }
        }

        /// <summary>Gets and sets whether the labeler allows carriage returns.</summary>
        /// <value>Whether the labeler allows carriage returns.</value>
        /// <remarks>Allows the labeler to split long labels into multiple lines.</remarks>
        protected bool AllowLineCarriage
        {
            get { return _allowLineCarriage; }
            set { _allowLineCarriage = value; }
        }

        /// <summary>Gets and sets whether a partial label in the current extent is drawn or not.</summary>
        /// <remarks>Provides a solution to the "cut off" label issue when multiple tiles exist.</remarks>
        protected bool SuppressPartialLabels
        {
            get { return _suppressPartialLabels; }
            set { _suppressPartialLabels = value; }
        }

        /// <summary>Gets and sets whether the labeler  forces carriage returns.</summary>
        /// <value>Whether the labeler forces carriage returns.</value>
        protected bool ForceLineCarriage
        {
            get { return _forceLineCarriage; }
            set { _forceLineCarriage = value; }
        }

        /// <summary>Gets and sets whether the labeler intends to fit the label within the boundary of polygon.</summary>
        /// <value>Whether the labeler intends to fit the label within the boundary of polygon.</value>
        /// <remarks>None</remarks>
        protected bool FittingPolygon
        {
            get { return _fittingPolygon; }
            set { _fittingPolygon = value; }
        }

        /// <summary>Gets and sets whether the labeler labels every part of a multi-part polygon.</summary>
        /// <value>Whether the labeler labels every part of a multi-part polygon.</value>
        protected bool LabelAllPolygonParts
        {
            get { return _labelAllPolygonParts; }
            set { _labelAllPolygonParts = value; }
        }

        /// <summary>Gets and sets whether the labeler lebels every part of a multi-part line.</summary>
        /// <value>Whether the labeler labels every part of a multi-part line.</value>
        protected bool LabelAllLineParts
        {
            get { return _labelAllLineParts; }
            set { _labelAllLineParts = value; }
        }

        /// <summary>Gets and sets whether labeling for lines is horizontal.</summary>
        /// <value>Whether labeling for lines is horizontal.</value>
        /// <remarks>Normally lines are labeled in the direction of the line.</remarks>
        protected bool ForceHorizontalLabelForLine
        {
            get { return _forceHorizontalLabelForLine; }
            set { _forceHorizontalLabelForLine = value; }
        }

        /// <summary>Gets and sets the factor keeping the label inside of the polygon.</summary>
        /// <value>Factor keeping the label inside of the polygon.</value>
        /// <remarks>None</remarks>
        protected double FittingPolygonFactor
        {
            get { return _fittingPolygonFactor; }
            set { _fittingPolygonFactor = value; }
        }

        /// <summary>Gets and sets the ratio label length / line length.</summary>
        /// <value>Ratio label length / line length.</value>
        /// <remarks>Allows to suppress labels to avoid label length exceeding greatly the line length.</remarks>
        protected double TextLineSegmentRatio
        {
            get { return _textLineSegmentRatio; }
            set { _textLineSegmentRatio = value; }
        }

        /// <summary>Gets and sets whether the labeler changes the label position to avoid overlapping for point-based features.</summary>
        /// <value>Whether the labeler changes the label position to avoid overlapping for point-based features.</value>
        protected bool BestPlacement
        {
            get { return _bestPlacement; }
            set { _bestPlacement = value; }
        }

        /// <summary>Gets and sets the location of the label for point features in relation to the point.</summary>
        /// <value>Location of the label for point features in relation to the point.</value>
        protected PointPlacement PointPlacement
        {
            get { return _pointPlacement; }
            set { _pointPlacement = value; }
        }

        /// <summary>Gets or sets the SplineType for labeling.</summary>
        protected SplineType SplineType
        {
            get { return _splineType; }
            set { _splineType = value; }
        }

        /// <summary>Gets or sets the DrawingLavel for the label.</summary>
        protected DrawingLevel DrawingLevel
        {
            get { return _drawingLevel; }
            set { _drawingLevel = value; }
        }

        public bool IsDefault
        {
            get
            {
                if (!IsActive)
                {
                    return true;
                }

                var isDefault = false;

                if (_textSolidBrush.Color.IsTransparent)
                {
                    isDefault = true;
                }

                if (isDefault && _advanced.TextCustomBrush != null)
                {
                    isDefault = false;
                }

                return isDefault;
            }
        }

        /// <summary>Draws the labels of the features on the canvas passed in.</summary>
        /// <remarks>Called from the concrete public method Draw. 
        /// When overriding this method, consider each feature and its column data values.If column data for a
        /// feature is needed, be sure to override GetRequiredColumnNamesCore and add the columns to the collection.</remarks>
        /// <returns>None</returns>
        /// <param name="features">Features to draw on the canvas.</param>
        /// <param name="canvas">Canvas to draw the features on.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to drawn in all layers.</param>
        protected override void DrawCore(IEnumerable<Feature> features, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(features, "feature");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            var candidateFeatures = FilterFeatures(features, canvas);

            if (!IsDefault)
            {
                var tempSuppressPartialLabels = _suppressPartialLabels;
                if (canvas.EnableCliping)
                {
                    _suppressPartialLabels = true;
                }

                foreach (var feature in candidateFeatures)
                {
                    DrawOneFeature(feature, canvas, labelsInThisLayer, labelsInAllLayers);
                }

                _suppressPartialLabels = tempSuppressPartialLabels;
            }

            foreach (var textStyle in _customTextStyles)
            {
                textStyle.Draw(features, canvas, labelsInThisLayer, labelsInAllLayers);
            }
        }

        /// <summary>Returns the column data required for the style to draw.</summary>
        /// <remarks>Abstract method called from the concrete public method GetRequiredFieldNames.</remarks>
        /// <returns>Returns a collection of column names required by the style.</returns>
        protected override Collection<string> GetRequiredColumnNamesCore()
        {
            var requiredFieldNames = new Collection<string>();

            if (!string.IsNullOrEmpty(_textColumnName))
            {
                requiredFieldNames.Add(_textColumnName);
            }

            foreach (var style in _customTextStyles)
            {
                var tmpCollection = style.GetRequiredColumnNames();

                foreach (var name in tmpCollection)
                {
                    if (!requiredFieldNames.Contains(name))
                    {
                        requiredFieldNames.Add(name);
                    }
                }
            }

            return requiredFieldNames;
        }

        /// <summary>Filters the features based on the grid size for deterministic labeling.</summary>
        /// <returns>Features that will be considered for labeling.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method. </remarks>
        /// <param name="features">Features to be filtered.</param>
        /// <param name="canvas">Canvas used for calculating font sizes.</param>
        protected internal Collection<Feature> FilterFeatures(IEnumerable<Feature> features, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(features, "feature");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            return FilterFeaturesCore(features, canvas);
        }

        /// <summary>Filters the features based on the grid size for deterministic labeling.</summary>
        /// <returns>Features that will be considered for labeling.</returns>
        /// <param name="features">Features to be filtered.</param>
        /// <param name="canvas">Canvas used for calculating font sizes.</param>
        protected virtual Collection<Feature> FilterFeaturesCore(IEnumerable<Feature> features, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(features, "feature");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            double virtualX = 0;
            double virtualY = 0;
            var canvasWidth = canvas.Width;
            var results = new Collection<Feature>();

            var currentExtent = canvas.CurrentWorldExtent;
            var gridSizeInWorld = (currentExtent.Width*_gridSize)/canvasWidth;

            RectangleShape allBoundingBox = null;
            var featureList = new List<Feature>(features);
            var boundingBoxList = new List<RectangleShape>();
            var indexOfFeatureListForLine = new List<int>();

            for (var i = 0; i < featureList.Count; i++)
            {
                var type = featureList[i].GetWellKnownType();
                if (type == WellKnownType.Line || type == WellKnownType.Multiline)
                {
                    var boundingBox = featureList[i].GetBoundingBox();
                    boundingBoxList.Add(boundingBox);

                    if (allBoundingBox == null)
                    {
                        allBoundingBox = (RectangleShape) boundingBox.CloneDeep();
                    }
                    else
                    {
                        allBoundingBox.ExpandToInclude(boundingBox);
                    }
                    indexOfFeatureListForLine.Add(i);
                }
                else
                {
                    results.Add(featureList[i]);
                }
            }

            if (boundingBoxList.Count != 0)
            {
                var allGridUpperLeftX = virtualX +
                                        Math.Floor((allBoundingBox.UpperLeftPoint.X - virtualX)/gridSizeInWorld)*
                                        gridSizeInWorld;
                var allGridUpperLeftY = virtualY -
                                        Math.Floor((virtualY - allBoundingBox.UpperLeftPoint.Y)/gridSizeInWorld)*
                                        gridSizeInWorld;

                var allGridLowerRightX = virtualX +
                                         Math.Ceiling((allBoundingBox.LowerRightPoint.X - virtualX)/gridSizeInWorld + 1)*
                                         gridSizeInWorld;
                var allGridLowerRightY = virtualY -
                                         Math.Ceiling((virtualY - allBoundingBox.LowerRightPoint.Y)/gridSizeInWorld + 1)*
                                         gridSizeInWorld;

                var gridCells = new Dictionary<int, LabelStyleGridCell>();

                for (var i = 0; i < boundingBoxList.Count; i++)
                {
                    var tempUpperLeftX = boundingBoxList[i].UpperLeftPoint.X;
                    var tempUpperLeftY = boundingBoxList[i].UpperLeftPoint.Y;
                    var tempLowerRightX = boundingBoxList[i].LowerRightPoint.X;
                    var tempLowerRightY = boundingBoxList[i].LowerRightPoint.Y;

                    var fromX = (int) Math.Floor((tempUpperLeftX - allGridUpperLeftX)/gridSizeInWorld);
                    var toX = (int) Math.Floor((tempLowerRightX - allGridUpperLeftX)/gridSizeInWorld);
                    var fromY = (int) Math.Floor((allGridUpperLeftY - tempUpperLeftY)/gridSizeInWorld);
                    var toY = (int) Math.Floor((allGridUpperLeftY - tempLowerRightY)/gridSizeInWorld);

                    var isFitTheGrid = false;

                    for (var k = fromX; k <= toX; k++)
                    {
                        for (var j = fromY; j <= toY; j++)
                        {
                            if (!gridCells.ContainsValue(new LabelStyleGridCell(k, j)))
                            {
                                gridCells.Add(i, new LabelStyleGridCell(k, j));
                                isFitTheGrid = true;
                                break;
                            }
                        }
                        if (isFitTheGrid)
                        {
                            break;
                        }
                    }
                }

                foreach (var index in gridCells.Keys)
                {
                    results.Add(featureList[indexOfFeatureListForLine[index]]);
                }
            }

            return results;
        }

        
        /// <returns>Collection of labeling candidates.</returns>
        /// <summary>Determines whether the specified feature is good candidate to be labeled according to the labeling properties.</summary>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method. </remarks>
        /// <param name="feature">Feature as a labeling candidate.</param>
        /// <param name="canvas">Canvas used to determine font size, etc.</param>
        protected Collection<LabelingCandidate> GetLabelingCandidates(Feature feature, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            Collection<LabelingCandidate> labelingCandidates;
            if (!LabelPositions.ContainsKey(feature.Id))
            {
                labelingCandidates = GetLabelingCandidateCore(feature, canvas);
            }
            else
            {
                var worldLabelingCandidate = LabelPositions[feature.Id];
                if (worldLabelingCandidate.CenterPointInWorldCoordinates == null)
                {
                    labelingCandidates = GetLabelingCandidateCore(feature, canvas);
                }
                else
                {
                    var centerPointShape = feature.GetShape().GetCenterPoint();
                    var xOffset = worldLabelingCandidate.CenterPointInWorldCoordinates.X - centerPointShape.X;
                    var yOffset = worldLabelingCandidate.CenterPointInWorldCoordinates.Y - centerPointShape.Y;
                    var tranlatedFeature = BaseShape.TranslateByOffset(feature, xOffset, yOffset, GeographyUnit.Meter,
                        DistanceUnit.Meter);

                    tranlatedFeature.ColumnValues[_textColumnName] = worldLabelingCandidate.OriginalText;
                    labelingCandidates = GetLabelingCandidateCore(tranlatedFeature, canvas);
                }
            }

            return labelingCandidates;
        }

        /// <returns>Collection of labeling candidates.</returns>
        /// <summary>Determines whether the specified feature is good candidate to be labeled according to the labeling properties.</summary>
        /// <param name="feature">Feature as a labeling candidate.</param>
        /// <param name="canvas">Canvas used to determine font size, etc.</param>
        protected virtual Collection<LabelingCandidate> GetLabelingCandidateCore(Feature feature, BaseGeoCanvas canvas)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            var candidatesCollection = new Collection<LabelingCandidate>();
            LabelingCandidate candidate;

            var shapeType = feature.GetWellKnownType();

            var shape = ConvertToScreenShape(feature, canvas);

            if (feature.ColumnValues == null || !feature.ColumnValues.ContainsKey(_textColumnName))
            {
                return candidatesCollection;
            }
            var text = feature.ColumnValues[_textColumnName];
            if (text == null || string.IsNullOrEmpty(text.Trim()))
            {
                return candidatesCollection;
            }

            switch (shapeType)
            {
                case WellKnownType.Point:
                    candidate = GetLabelingCandidateForOnePoint((PointShape) shape, text, canvas);
                    if (candidate != null)
                    {
                        candidatesCollection.Add(candidate);
                    }
                    break;

                case WellKnownType.Multipoint:
                    candidatesCollection = GetLabelingCandidateForMultipoint((MultipointShape) shape, text, canvas);
                    break;

                case WellKnownType.Line:
                    var multiLineShape = new MultilineShape();
                    multiLineShape.Lines.Add((LineShape) shape);
                    candidatesCollection = GetLabelingCandidatesForMultiline(multiLineShape, text, canvas);
                    break;

                case WellKnownType.Multiline:
                    candidatesCollection = GetLabelingCandidatesForMultiline((MultilineShape) shape, text, canvas);
                    break;

                case WellKnownType.Polygon:
                    var multipolygonShape = new MultipolygonShape();
                    multipolygonShape.Polygons.Add((PolygonShape) shape);
                    candidatesCollection = GetLabelingCandidateForMultipolygon(multipolygonShape, text, canvas);
                    break;

                case WellKnownType.Multipolygon:
                    candidatesCollection = GetLabelingCandidateForMultipolygon((MultipolygonShape) shape, text, canvas);
                    break;
                case WellKnownType.GeometryCollection:
                    candidatesCollection = GetLabelingCandidateForGeometryCollection(feature, canvas);
                    break;
            }

            return candidatesCollection;
        }

        /// <summary>Determines whether the label is suppressed because when duplicated.</summary>
        /// <returns>Whether the label is suppressed because when duplicated.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        /// <param name="labelingCandidate">Labeling candidate that will be checked to determine if it is a duplicate.</param>
        /// <param name="canvas">Canvas used for calculations.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to draw in all layers.</param>
        protected bool CheckDuplicate(LabelingCandidate labelingCandidate, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(labelingCandidate, "labelingCandidate");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInThisLayer, "labelsInThisLayer");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labelsInAllLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            return CheckDuplicateCore(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers);
        }

        /// <summary>Determines whether the label is suppressed because when duplicated.</summary>
        /// <returns>Whether the label is suppressed because when duplicated.</returns>
        /// <param name="labelingCandidate">Labeling candidate that will be checked to determine if it is a duplicate.</param>
        /// <param name="canvas">Canvas used for calculations.</param>
        /// <param name="labelsInThisLayer">Labels to draw in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels to draw in all layers.</param>
        protected virtual bool CheckDuplicateCore(LabelingCandidate labelingCandidate, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(labelingCandidate, "labelingCandidate");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            var isDuplicate = false;

            switch (_labelDuplicateRule)
            {
                case LabelDuplicateRule.OneDuplicateLabelPerQuadrant:
                    isDuplicate = IsLabelDuplicateByQuadrant(labelsInAllLayers, labelingCandidate,
                        new ScreenPointF(0, 0), new ScreenPointF(canvas.Width, canvas.Height));
                    break;

                case LabelDuplicateRule.NoDuplicateLabels:
                    isDuplicate = IsLabelDuplicate(labelsInAllLayers, labelingCandidate);
                    break;

                case LabelDuplicateRule.UnlimitedDuplicateLabels:
                    break;
                default:
                    break;
            }

            return isDuplicate;
        }

        private static bool IsLabelDuplicate(Collection<SimpleCandidate> labelCandidates,
            LabelingCandidate currentLabelCandidate)
        {
            if (labelCandidates == null)
            {
                return false;
            }

            var isDuplicate = false;

            foreach (var labelCandidate in labelCandidates)
            {
                if (string.CompareOrdinal(labelCandidate.OriginalText, currentLabelCandidate.OriginalText) == 0)
                {
                    isDuplicate = true;
                    break;
                }
            }

            return isDuplicate;
        }

        private static bool IsLabelDuplicateByQuadrant(Collection<SimpleCandidate> labelCandidates,
            LabelingCandidate currentLabelCandidate, ScreenPointF ulPointF, ScreenPointF lrPointF)
        {
            if (labelCandidates == null)
            {
                return false;
            }

            var isDuplicate = false;

            double width = lrPointF.X - ulPointF.X;
            double height = lrPointF.Y - ulPointF.Y;

            var quadrantCenterX = ulPointF.X + width*0.5;
            var quadrantCenterY = ulPointF.Y + height*0.5;

            foreach (var labelCandidate in labelCandidates)
            {
                var centerPointShape = GetCenterPointOfPolygon(labelCandidate.SimplePolygonInScreenCoordinate);

                if (string.CompareOrdinal(labelCandidate.OriginalText, currentLabelCandidate.OriginalText) == 0)
                {
                    var candidateCenterX = currentLabelCandidate.CenterPoint.X;
                    var candidateCenterY = currentLabelCandidate.CenterPoint.Y;

                    if ((candidateCenterX < quadrantCenterX) && (centerPointShape.X < quadrantCenterX) &&
                        (candidateCenterY < quadrantCenterY) && (centerPointShape.Y < quadrantCenterY))
                    {
                        isDuplicate = true;
                        break;
                    }
                    if ((candidateCenterX > quadrantCenterX) && (centerPointShape.X > quadrantCenterX) &&
                        (candidateCenterY < quadrantCenterY) && (centerPointShape.Y < quadrantCenterY))
                    {
                        isDuplicate = true;
                        break;
                    }
                    if ((candidateCenterX > quadrantCenterX) && (centerPointShape.X > quadrantCenterX) &&
                        (candidateCenterY > quadrantCenterY) && (centerPointShape.Y > quadrantCenterY))
                    {
                        isDuplicate = true;
                        break;
                    }
                    if ((candidateCenterX < quadrantCenterX) && (centerPointShape.X < quadrantCenterX) &&
                        (candidateCenterY > quadrantCenterY) && (centerPointShape.Y > quadrantCenterY))
                    {
                        isDuplicate = true;
                        break;
                    }
                }
            }

            return isDuplicate;
        }


        /// <summary>Determines whether the label overlaps over labels.</summary>
        /// <returns>Whether the label overlaps over labels.</returns>
        /// <remarks>As a concrete public method that wraps a Core method, Mapgenix reserves the right
        /// to add events and other logic returned by the Core version of the method.</remarks>
        /// <param name="labelingCandidate">Labeling candidate to chech it is overlapping.</param>
        /// <param name="canvas">Canvas used for calculations.</param>
        /// <param name="labelsInThisLayer">Labels drawn in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels drawn in all layers.</param>
        protected bool CheckOverlapping(LabelingCandidate labelingCandidate, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(labelingCandidate, "labelingCandidate");
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);

            var isOverlapping = CheckOverlappingCore(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers);

            if (isOverlapping && _bestPlacement && _rotationAngle == 0 &&
                _currentShapeWellKnownType == WellKnownType.Point)
            {
                var size = canvas.MeasureText(labelingCandidate.LabelInformation[0].Text, _font);
                var originalPoint = GetOrginalLabelPointShape(labelingCandidate.CenterPoint, size.Width, size.Height,
                    _pointPlacement, canvas.Dpi);
                var generalPointPlacement = _pointPlacement;

                for (var i = 0; i < 4; i++)
                {
                    _pointPlacement = SearchPlacement(i, _pointPlacement);

                    if (generalPointPlacement == _pointPlacement)
                    {
                        continue;
                    }

                    var tempLabelingCandidate = GetLabelingCandidateForOnePointStraight(originalPoint,
                        labelingCandidate.OriginalText, canvas);

                    isOverlapping = CheckOverlappingCore(tempLabelingCandidate, canvas, labelsInThisLayer,
                        labelsInAllLayers);

                    if (!isOverlapping)
                    {
                        CopyLabelingCandidate(tempLabelingCandidate, labelingCandidate);
                        break;
                    }
                }

                _pointPlacement = generalPointPlacement;
            }

            return isOverlapping;
        }

        /// <summary>Determines whether the label overlaps over labels.</summary>
        /// <returns>Whether the label overlaps over labels.</returns>
        /// <param name="labelingCandidate">Labeling candidate to chech it is overlapping.</param>
        /// <param name="canvas">Canvas used for calculations.</param>
        /// <param name="labelsInThisLayer">Labels drawn in the current layer only.</param>
        /// <param name="labelsInAllLayers">Labels drawn in all layers.</param>
        protected virtual bool CheckOverlappingCore(LabelingCandidate labelingCandidate, BaseGeoCanvas canvas,
            Collection<SimpleCandidate> labelsInThisLayer, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(labelingCandidate, "labelingCandidate");

            var isOverlapping = false;
            if (_labelOverlappintRule == LabelOverlappingRule.NoOverlapping)
            {
                isOverlapping = IsLabelOverlapping(labelsInAllLayers, labelingCandidate);
            }
            return isOverlapping;
        }

        
        /// <summary>Converts the shape of a feature in world coordinates to screen coordinates.</summary>
        /// <returns>Shape in screen coordinates.</returns>
        /// <param name="feature">Feature which shape is to be converted from world coordinates to screen coordinates.</param>
        /// <param name="canvas">Canvas for doing the conversion.</param>
        protected static BaseShape ConvertToScreenShape(Feature feature, BaseGeoCanvas canvas)
        {
            var shapeType = feature.GetWellKnownType();
            BaseShape shape = null;

            switch (shapeType)
            {
                case WellKnownType.Point:
                    shape = ConvertPoint(feature, canvas);
                    break;

                case WellKnownType.Line:
                    shape = ConvertLine(feature, canvas);
                    break;

                case WellKnownType.Polygon:
                    shape = ConvertPolygon(feature, canvas);
                    break;

                case WellKnownType.Multipoint:
                    shape = ConvertMultipoint(feature, canvas);
                    break;

                case WellKnownType.Multiline:
                    shape = ConvertMultiline(feature, canvas);
                    break;

                case WellKnownType.Multipolygon:
                    shape = ConvertMultipolygon(feature, canvas);
                    break;
                case WellKnownType.GeometryCollection:
                    shape = ConvertGeometryCollection(feature, canvas);
                    break;
                default:
                    break;
            }

            return shape;
        }

        private static bool IsLabelOverlapping(Collection<SimpleCandidate> labelCandidates,
            LabelingCandidate currentLabelCandidate)
        {
            if (labelCandidates == null)
            {
                return false;
            }

            var isOverlapping = false;

            foreach (var labelCandidate in labelCandidates)
            {
                if (!IsDisjointed(currentLabelCandidate.ScreenArea, labelCandidate.SimplePolygonInScreenCoordinate))
                {
                    isOverlapping = true;
                    break;
                }
            }

            return isOverlapping;
        }

        private void DrawOneFeature(Feature feature, BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInThisLayer,
            Collection<SimpleCandidate> labelsInAllLayers)
        {
            var tmpGeoBrush = _advanced.TextCustomBrush;
            if (tmpGeoBrush == null && !_textSolidBrush.Color.IsTransparent)
            {
                tmpGeoBrush = _textSolidBrush;
            }

            var textPathInScreen = new ScreenPointF[1];

            var labelingCandidates = GetLabelingCandidates(feature, canvas);

            _currentShapeWellKnownType = feature.GetWellKnownType();

            RectangleShape canvasScreenExtent = null;
            if (SuppressPartialLabels)
            {
                canvasScreenExtent =
                    ConvertToScreenShape(new Feature(canvas.CurrentWorldExtent), canvas).GetBoundingBox();
            }

            foreach (var labelingCandidate in labelingCandidates)
            {
                if (CheckDuplicate(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers))
                {
                    continue;
                }
                if (CheckOverlapping(labelingCandidate, canvas, labelsInThisLayer, labelsInAllLayers))
                {
                    continue;
                }

                if (SuppressPartialLabels)
                {
                    if (!canvasScreenExtent.Contains(labelingCandidate.ScreenArea))
                    {
                        continue;
                    }
                }

                var simpleCandidate = new SimpleCandidate(labelingCandidate.OriginalText, labelingCandidate.ScreenArea);
                if (labelsInAllLayers != null)
                {
                    labelsInAllLayers.Add(simpleCandidate);
                }
                if (labelsInThisLayer != null)
                {
                    labelsInThisLayer.Add(simpleCandidate);
                }

                if (_mask != null)
                {
                    Feature[] maskFeatures = {ConvertToWorldCoordinate(labelingCandidate.ScreenArea, canvas)};
                    _mask.Draw(maskFeatures, canvas, labelsInThisLayer, labelsInAllLayers);
                }

                foreach (var labelInfo in labelingCandidate.LabelInformation)
                {
                    textPathInScreen[0] = new ScreenPointF((float) labelInfo.PositionInScreenCoordinates.X,
                        (float) labelInfo.PositionInScreenCoordinates.Y);
                    canvas.DrawText(labelInfo.Text, _font, tmpGeoBrush, _haloPen, textPathInScreen, DrawingLevel, 0, 0,
                        (float) labelInfo.RotationAngle);
                }
            }
        }

        private LabelingCandidate GetLabelingCandidateForMultiline(MultilineShape multilineShape, string text,
            BaseGeoCanvas canvas)
        {
            LabelingCandidate labelingCandidate;

            var upperLeft = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent,
                canvas.CurrentWorldExtent.UpperLeftPoint, canvas.Width, canvas.Height);
            var lowerRight = ExtentHelper.ToScreenCoordinate(canvas.CurrentWorldExtent,
                canvas.CurrentWorldExtent.LowerRightPoint, canvas.Width, canvas.Height);
            var screenExtent = new RectangleShape(upperLeft.X, lowerRight.Y, lowerRight.X, upperLeft.Y);
            var lineLabelAdjust = GetLineLabelAdjuster(multilineShape, screenExtent, _restrictLabelInScreen);

            if (lineLabelAdjust.Angle == 0 || _forceHorizontalLabelForLine)
            {
                labelingCandidate = GetLabelCandidateForMultiLineStraight(lineLabelAdjust, text, canvas);
            }
            else
            {
                labelingCandidate = GetLabelCandidateForMultiLineIncline(lineLabelAdjust, text, canvas);
            }

            return labelingCandidate;
        }

        private LabelingCandidate GetLabelCandidateForMultiLineStraight(LineLabelAdjuster lineLabelAdjuster, string text,
            BaseGeoCanvas canvas)
        {
            var labelText = ApplyFormatOfString(text);

            var size = canvas.MeasureText(labelText, _font);
            double width = size.Width;
            double height = size.Height;

            if (size.Width > lineLabelAdjuster.LineSegmentLength*_textLineSegmentRatio)
            {
                return null;
            }

            var midLinePoint = lineLabelAdjuster.MidLinePoint;
            var centerPointX = midLinePoint.X + (_xOffsetInPixel*(canvas.Dpi/96.0));
            var centerPointY = midLinePoint.Y - (_yOffsetInPixel*(canvas.Dpi/96.0));

            var upperLeftPoint = new PointShape(centerPointX - width*0.5, centerPointY - height*0.5);

            var simplePolygon = GetStraightMask(upperLeftPoint, width, height);

            var centerPointForLabel = new PointShape(centerPointX, centerPointY);

            var labelInfo = new LabelInformation(centerPointForLabel, labelText, 0);
            var labelInformationCollection = new Collection<LabelInformation>();
            labelInformationCollection.Add(labelInfo);

            return new LabelingCandidate(text, simplePolygon, centerPointForLabel, labelInformationCollection);
        }

        private LabelingCandidate GetLabelCandidateForMultiLineIncline(LineLabelAdjuster lineLabelAdjuster, string text,
            BaseGeoCanvas canvas)
        {
            var labelText = ApplyFormatOfString(text);

            var midLinePoint = lineLabelAdjuster.MidLinePoint;
            var angle = lineLabelAdjuster.Angle;

            var size = canvas.MeasureText(labelText, _font);
            double width = size.Width;
            double height = size.Height;

            if (width > lineLabelAdjuster.LineSegmentLength*_textLineSegmentRatio)
            {
                return null;
            }

            var upperCenterPoint = GetPointFromDistanceAndDegree(midLinePoint,
                height*0.5 + _yOffsetInPixel*(canvas.Dpi/96.0), angle + 90);
            upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, _xOffsetInPixel*(canvas.Dpi/96.0), angle);

            var simplePolygon = GetInclineMask(upperCenterPoint, width, height, angle);

            var centerPointForLabel = GetPointFromDistanceAndDegree(upperCenterPoint, height*0.5, angle + 270);

            var labelInfo = new LabelInformation(centerPointForLabel, labelText, angle);
            var labelInformationCollection = new Collection<LabelInformation>();
            labelInformationCollection.Add(labelInfo);

            return new LabelingCandidate(text, simplePolygon, upperCenterPoint, labelInformationCollection);
        }

        private LabelingCandidate GetLabelingCandidateForMultilineSplineLabeling(MultilineShape multilineShape,
            string text, BaseGeoCanvas canvas)
        {
            LabelingCandidate candidate = null;

            var firstLineShape = multilineShape.Lines[0];
            var maxSegmentLength = double.MinValue;
            for (var i = 1; i < firstLineShape.Vertices.Count; i++)
            {
                var currentLength = GetDistanceBetweenTwoVertex(firstLineShape.Vertices[i - 1],
                    firstLineShape.Vertices[i]);

                if (currentLength > maxSegmentLength)
                {
                    maxSegmentLength = currentLength;
                }
            }

            var labelText = ApplyFormatOfString(text);
            var realTextForMeasure = labelText.Replace(' ', '_');

            var size = canvas.MeasureText(realTextForMeasure, _font);
            double width = size.Width + labelText.Length;

            var lineLength = firstLineShape.GetLength(GeographyUnit.Meter, DistanceUnit.Meter);

            if (_splineType == SplineType.ForceSplining && width > lineLength)
            {
                return candidate;
            }

            if ((width > maxSegmentLength && width < lineLength) || _splineType == SplineType.ForceSplining)
            {
                var coordinates = new Collection<PointShape>();
                var startingPoint = StartingPoint.FirstPoint;
                if (_splineType == SplineType.ForceSplining)
                {
                    if ((firstLineShape.Vertices[0].Y > firstLineShape.Vertices[firstLineShape.Vertices.Count - 1].Y))
                    {
                        startingPoint = StartingPoint.LastPoint;
                    }
                }
                else
                {
                    if ((firstLineShape.Vertices[0].X > firstLineShape.Vertices[firstLineShape.Vertices.Count - 1].X))
                    {
                        startingPoint = StartingPoint.LastPoint;
                    }
                }
                var firstPoint = firstLineShape.GetPointOnALine(startingPoint, (lineLength - width)/2,
                    GeographyUnit.Meter, DistanceUnit.Meter);
                coordinates.Add(firstPoint);

                double totalCharactorLength = 0;
                for (var i = 0; i < labelText.Length; i++)
                {
                    DrawingRectangleF charactorSize;
                    if (string.CompareOrdinal(labelText[i].ToString(), " ") == 0)
                    {
                        charactorSize = canvas.MeasureText("_", _font);
                        totalCharactorLength = totalCharactorLength + charactorSize.Width;
                    }
                    else
                    {
                        charactorSize = canvas.MeasureText(labelText[i].ToString(), _font);
                        totalCharactorLength = totalCharactorLength + charactorSize.Width + 1;
                    }

                    var point = firstLineShape.GetPointOnALine(startingPoint,
                        (lineLength - width)/2 + totalCharactorLength, GeographyUnit.Meter, DistanceUnit.Meter);
                    if (point != null)
                    {
                        coordinates.Add(point);
                    }
                }

                candidate = GetLabelingCandidateForMultilineSplineLabeling(coordinates, labelText, canvas);
            }
            else
            {
                candidate = GetLabelingCandidateForMultiline(multilineShape, text, canvas);
            }
            return candidate;
        }

        private LabelingCandidate GetLabelingCandidateForMultilineSplineLabeling(Collection<PointShape> coordinates,
            string text, BaseGeoCanvas canvas)
        {
            var labelInformationCollection = new Collection<LabelInformation>();
            var simplePolygons = new MultipolygonShape();
            double totalSlideLength = 0;

            for (var i = 0; i < coordinates.Count - 1; i++)
            {
                if (string.CompareOrdinal(text[i].ToString(), " ") != 0)
                {
                    var midLinePoint = GetMidPointFromTwoPoints(coordinates[i], coordinates[i + 1]);
                    var angle = GetAngleFromTwoVerticesForSpline(new Vertex(coordinates[i]),
                        new Vertex(coordinates[i + 1]));

                    var size = canvas.MeasureText(text[i].ToString(), _font);

                    double width = size.Width;
                    double height = size.Height;

                    var upperCenterPoint = GetPointFromDistanceAndDegree(midLinePoint,
                        height*0.5 + _yOffsetInPixel*(canvas.Dpi/96.0), angle + 90);
                    upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, _xOffsetInPixel*(canvas.Dpi/96.0),
                        angle);


                    if (totalSlideLength > 0)
                    {
                        upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, totalSlideLength, angle);
                    }

                    var tempSimplePolygon = GetInclineMask(upperCenterPoint, width, height, angle);

                    if (i > 0 &&
                        SimpleOverlaps(tempSimplePolygon, (simplePolygons.Polygons[simplePolygons.Polygons.Count - 1])))
                    {
                        totalSlideLength += 1;
                        upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, 1, angle);
                    }
                    var simplePolygon = GetInclineMask(upperCenterPoint, width, height, angle);

                    simplePolygons.Polygons.Add(simplePolygon);
                    var centerPointForLabel = GetPointFromDistanceAndDegree(upperCenterPoint, height*0.5, angle + 270);
                    var labelInformation = new LabelInformation(centerPointForLabel, text[i].ToString(), angle);
                    labelInformationCollection.Add(labelInformation);
                }
            }
            var totalPolygonShape = new PolygonShape(simplePolygons.GetConvexHull());

            return new LabelingCandidate(text, totalPolygonShape, totalPolygonShape.GetCenterPoint(),
                labelInformationCollection);
        }

        private static bool SimpleOverlaps(PolygonShape firstPolygon, PolygonShape secondPolygon)
        {
            var overlapResult = false;

            var firstBoundingBox = firstPolygon.GetBoundingBox();
            var secondBoundingBox = secondPolygon.GetBoundingBox();

            var firstCenterPointX = firstBoundingBox.UpperLeftPoint.X + firstBoundingBox.Width/2;
            var firstCenterPointY = firstBoundingBox.UpperLeftPoint.Y - firstBoundingBox.Height/2;
            var secondCenterPointX = secondBoundingBox.UpperLeftPoint.X + secondBoundingBox.Width/2;
            var secondCenterPointY = secondBoundingBox.UpperLeftPoint.Y - secondBoundingBox.Height/2;

            if (Math.Abs(firstCenterPointX - secondCenterPointX) <
                (firstBoundingBox.Width/2 + secondBoundingBox.Width/2) &&
                Math.Abs(firstCenterPointY - secondCenterPointY) <
                (firstBoundingBox.Height/2 + secondBoundingBox.Height/2))
            {
                overlapResult = true;
            }

            return overlapResult;
        }

        private static LineLabelAdjuster GetLineLabelAdjuster(MultilineShape multilineShape, RectangleShape screenExtent,
            bool allowLineLabelingMoving)
        {
            var maxLength = double.MinValue;
            var lineIndex = 0;
            var verticesIndex = 0;
            var isContained = screenExtent.Intersects(multilineShape);

            for (var j = 0; j < multilineShape.Lines.Count; j++)
            {
                var line = multilineShape.Lines[j];
                for (var i = 1; i < line.Vertices.Count; i++)
                {
                    var currentLength = GetDistanceBetweenTwoVertex(line.Vertices[i - 1], line.Vertices[i]);

                    if (currentLength > maxLength)
                    {
                        if (allowLineLabelingMoving && isContained)
                        {
                            var tempLine = new LineShape(new[] {line.Vertices[i - 1], line.Vertices[i]});
                            if (screenExtent.Intersects(tempLine))
                            {
                                maxLength = currentLength;
                                lineIndex = j;
                                verticesIndex = i;
                            }
                        }
                        else
                        {
                            maxLength = currentLength;
                            lineIndex = j;
                            verticesIndex = i;
                        }
                    }
                }
            }

            var vertexWest = multilineShape.Lines[lineIndex].Vertices[verticesIndex - 1];
            var vertexEast = multilineShape.Lines[lineIndex].Vertices[verticesIndex];

            if (allowLineLabelingMoving && isContained)
            {
                var line = new LineShape(new[] {vertexWest, vertexEast});
                if (screenExtent.Crosses(line))
                {
                    var containedLine = OverlappingForLine(line, screenExtent);
                    vertexWest = containedLine.Lines[0].Vertices[0];
                    vertexEast = containedLine.Lines[0].Vertices[1];
                    maxLength = GetDistanceBetweenTwoVertex(vertexWest, vertexEast);
                }
            }

            if (vertexWest.X > vertexEast.X)
            {
                var temp = vertexWest;
                vertexWest = vertexEast;
                vertexEast = temp;
            }

            var angle = GetAngleFromTwoVertices(vertexWest, vertexEast);
            var midLinePoint = GetMidPointFromTwoVertices(vertexWest, vertexEast);

            return new LineLabelAdjuster(midLinePoint, maxLength, angle);
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForMultipoint(MultipointShape multipoint, string text,
            BaseGeoCanvas canvas)
        {
            var candidatesCollection = new Collection<LabelingCandidate>();

            foreach (var point in multipoint.Points)
            {
                var candidate = GetLabelingCandidateForOnePoint(point, text, canvas);
                if (candidate != null)
                {
                    candidatesCollection.Add(candidate);
                }
            }

            return candidatesCollection;
        }

        private LabelingCandidate GetLabelingCandidateForOnePoint(PointShape point, string text, BaseGeoCanvas canvas)
        {
            LabelingCandidate labelingCandidate;

            if (_rotationAngle == 0)
            {
                labelingCandidate = GetLabelingCandidateForOnePointStraight(point, text, canvas);
            }
            else
            {
                labelingCandidate = GetLabelingCandidateForOnePointIncline(point, text, canvas);
            }

            return labelingCandidate;
        }

        private LabelingCandidate GetLabelingCandidateForOnePointIncline(PointShape point, string text, BaseGeoCanvas canvas)
        {
            var labelText = ApplyFormatOfString(text);

            var size = canvas.MeasureText(labelText, _font);
            double width = size.Width;
            double height = size.Height;

            var upperCenterPoint = new PointShape(point.X, point.Y);
            upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint,
                height*0.5 + _yOffsetInPixel*(canvas.Dpi/96.0), _rotationAngle + 90);
            upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, _xOffsetInPixel*(canvas.Dpi/96.0),
                _rotationAngle);

            var simplePolygon = GetInclineMask(upperCenterPoint, width, height, _rotationAngle);

            var centerPointForLabel = GetPointFromDistanceAndDegree(upperCenterPoint, height*0.5, _rotationAngle + 270);

            var labelInfo = new LabelInformation(centerPointForLabel, labelText, _rotationAngle);
            var labelInformationCollection = new Collection<LabelInformation>();
            labelInformationCollection.Add(labelInfo);

            return new LabelingCandidate(text, simplePolygon, centerPointForLabel, labelInformationCollection);
        }

        private LabelingCandidate GetLabelingCandidateForOnePointStraight(PointShape point, string text,
            BaseGeoCanvas canvas)
        {
            var labelText = ApplyFormatOfString(text);

            var size = canvas.MeasureText(labelText, _font);
            double width = size.Width;
            double height = size.Height;

            var upperLeftPoint = new PointShape(point.X + _xOffsetInPixel*(canvas.Dpi/96.0),
                point.Y - _yOffsetInPixel*(canvas.Dpi/96.0));
            upperLeftPoint = GetNewUpperLeftPointByPlacement(upperLeftPoint, width, height, _pointPlacement);

            var centerPointToLabel = new PointShape(upperLeftPoint.X + width*0.5, upperLeftPoint.Y + height*0.5);

            var simplePolygon = GetStraightMask(upperLeftPoint, width, height);

            var labelInfo = new LabelInformation(centerPointToLabel, labelText, 0);
            var labelInformationCollection = new Collection<LabelInformation>();
            labelInformationCollection.Add(labelInfo);

            return new LabelingCandidate(text, simplePolygon, centerPointToLabel, labelInformationCollection);
        }

        private Collection<LabelingCandidate> GetLabelingCandidatesForMultiline(MultilineShape multilineShape,
            string text, BaseGeoCanvas canvas)
        {
            Collection<LabelingCandidate> resultCandidate;

            if (_labelAllLineParts)
            {
                resultCandidate = GetLabelingCandidateForAllPartOfMultiline(multilineShape, text, canvas);
            }
            else
            {
                resultCandidate = GetLabelingCandidateForOnePartOfMultiline(multilineShape, text, canvas);
            }

            return resultCandidate;
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForAllPartOfMultiline(MultilineShape multilineShape,
            string text, BaseGeoCanvas canvas)
        {
            var candidatesCollection = new Collection<LabelingCandidate>();

            foreach (var line in multilineShape.Lines)
            {
                var candidate = GetLabelingCandidateForOneLine(line, text, canvas);
                if (candidate != null)
                {
                    candidatesCollection.Add(candidate);
                }
            }

            return candidatesCollection;
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForOnePartOfMultiline(MultilineShape multilineShape,
            string text, BaseGeoCanvas canvas)
        {
            var candidatesCollection = new Collection<LabelingCandidate>();


            var line = GetTheLongestLineInMultiline(multilineShape);


            var candidate = GetLabelingCandidateForOneLine(line, text, canvas);

            if (candidate != null)
            {
                candidatesCollection.Add(candidate);
            }

            return candidatesCollection;
        }

        private LineShape GetTheLongestLineInMultiline(MultilineShape multilineShape)
        {
            var count = multilineShape.Lines.Count;

            if (count == 1)
            {
                return multilineShape.Lines[0];
            }

            var index = 0;
            var maxLength = double.MinValue;

            for (var i = 0; i < count; i++)
            {
                var length = multilineShape.Lines[i].GetLength(GeographyUnit.Meter, DistanceUnit.Meter);
                if (length > maxLength)
                {
                    maxLength = length;
                    index = i;
                }
            }

            return multilineShape.Lines[index];
        }

        private LabelingCandidate GetLabelingCandidateForOneLine(LineShape line, string text, BaseGeoCanvas canvas)
        {
            LabelingCandidate candidate;

            var multiLineShape = new MultilineShape();
            multiLineShape.Lines.Add(line);
            if (_splineType == SplineType.StandardSplining || _splineType == SplineType.ForceSplining)
            {
                candidate = GetLabelingCandidateForMultilineSplineLabeling(multiLineShape, text, canvas);
            }
            else
            {
                candidate = GetLabelingCandidateForMultiline(multiLineShape, text, canvas);
            }

            return candidate;
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForMultipolygon(MultipolygonShape multipolygonShape,
            string text, BaseGeoCanvas canvas)
        {
            Collection<LabelingCandidate> resultCandidate;

            if (_labelAllPolygonParts)
            {
                resultCandidate = GetLabelingCandidateForAllPartOfMultipolygon(multipolygonShape, text, canvas);
            }
            else
            {
                resultCandidate = GetLabelingCandidateForOnePartOfMultipolygon(multipolygonShape, text, canvas);
            }

            return resultCandidate;
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForGeometryCollection(Feature feature,
            BaseGeoCanvas canvas)
        {
            Collection<LabelingCandidate> resultCandidate = null;

            var geometryCollectionShape = (GeometryCollectionShape) feature.GetShape();
            foreach (var baseShape in geometryCollectionShape.Shapes)
            {
                if (resultCandidate == null)
                {
                    resultCandidate = new Collection<LabelingCandidate>();
                }

                baseShape.Id = feature.Id;
                var subShapeFeature = new Feature(baseShape, feature.ColumnValues);
                var subLabelingCandidates = GetLabelingCandidateCore(subShapeFeature, canvas);
                foreach (var subLabelingCandidate in subLabelingCandidates)
                {
                    resultCandidate.Add(subLabelingCandidate);
                }
            }

            return resultCandidate;
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForOnePartOfMultipolygon(
            MultipolygonShape multipolygonShape, string text, BaseGeoCanvas canvas)
        {
            var largestPolygon = GetTheLargestPolygonInMultipolygon(multipolygonShape);

            var candidates = new Collection<LabelingCandidate>();

            var candidate = GetLabelingCandidateForOnePolygon(largestPolygon, text, canvas);

            if (candidate != null)
            {
                candidates.Add(candidate);
            }

            return candidates;
        }

        internal LabelingCandidate GetLabelingCandidateForOnePolygon(PolygonShape polygon, string text, BaseGeoCanvas canvas)
        {
            var labelText = ApplyFormatOfString(text);

            var labelTexts = GetLabelTextsForCarriage(polygon, labelText, canvas);

            var sizeList = new DrawingRectangleF[labelTexts.Length];

            for (var i = 0; i < sizeList.Length; i++)
            {
                sizeList[i] = canvas.MeasureText(labelTexts[i], _font);
            }

            double width = 0;
            double height = 0;

            for (var i = 0; i < sizeList.Length; i++)
            {
                if (sizeList[i].Width > width)
                {
                    width = sizeList[i].Width;
                }

                height += sizeList[i].Height;
            }

            if (_fittingPolygon)
            {
                var polygonWidth = polygon.GetBoundingBox().Width;
                if (width > polygonWidth*_fittingPolygonFactor)
                {
                    return null;
                }
            }

            var centerPoint = GetLabelingLocationForPolygon(polygon);

            var upperCenterPoint = GetPointFromDistanceAndDegree(centerPoint,
                height*0.5 + _yOffsetInPixel*(canvas.Dpi/96.0), _rotationAngle + 90);
            upperCenterPoint = GetPointFromDistanceAndDegree(upperCenterPoint, _xOffsetInPixel*(canvas.Dpi/96.0),
                _rotationAngle);

            double yOffset = 0;
            var labelInfos = new Collection<LabelInformation>();

            for (var i = 0; i < sizeList.Length; i++)
            {
                if (i != 0)
                {
                    yOffset += sizeList[i - 1].Height;
                }

                var labelInfo = GetLabelInfo(labelTexts[i], sizeList[i].Height, upperCenterPoint, yOffset);
                labelInfos.Add(labelInfo);
            }

            var simplePolygon = GetInclineMask(upperCenterPoint, width, height, _rotationAngle);

            return new LabelingCandidate(text, simplePolygon, centerPoint, labelInfos);
        }

        private Collection<LabelingCandidate> GetLabelingCandidateForAllPartOfMultipolygon(
            MultipolygonShape multipolygonShape, string text, BaseGeoCanvas canvas)
        {
            var candidatesCollection = new Collection<LabelingCandidate>();

            foreach (var polygon in multipolygonShape.Polygons)
            {
                var candidate = GetLabelingCandidateForOnePolygon(polygon, text, canvas);
                if (candidate != null)
                {
                    candidatesCollection.Add(candidate);
                }
            }

            return candidatesCollection;
        }

        private static double GetDistanceBetweenTwoVertex(Vertex fromVertex, Vertex toVertex)
        {
            var differentX = fromVertex.X - toVertex.X;
            var differentY = fromVertex.Y - toVertex.Y;

            return Math.Sqrt(differentX*differentX + differentY*differentY);
        }

        private static double GetAngleFromTwoVertices(Vertex fromVertex, Vertex toVertex)
        {
            double alpha;

            if (fromVertex.X != toVertex.X)
            {
                var tangentAlpha = (fromVertex.Y - toVertex.Y)/(toVertex.X - fromVertex.X);
                alpha = Math.Atan(tangentAlpha)*180/Math.PI;
                if (alpha < 0)
                {
                    alpha += 360;
                }
            }
            else
            {
                alpha = (fromVertex.Y > toVertex.Y) ? 90 : 270;
            }

            return alpha;
        }

        private static double GetAngleFromTwoVerticesForSpline(Vertex fromVertex, Vertex toVertex)
        {
            double alpha;

            if (fromVertex.X != toVertex.X)
            {
                var tangentAlpha = (fromVertex.Y - toVertex.Y)/(toVertex.X - fromVertex.X);
                alpha = Math.Atan(tangentAlpha)*180/Math.PI;
                if (alpha < 0)
                {
                    alpha += 360;
                }
                if (fromVertex.X > toVertex.X)
                {
                    alpha += 180;
                }
                alpha %= 360;
            }
            else
            {
                alpha = (fromVertex.Y > toVertex.Y) ? 90 : 270;
            }

            return alpha;
        }

        private string[] GetLabelTextsForCarriage(PolygonShape polygon, string text, BaseGeoCanvas canvas)
        {
            var labelTexts = new string[1] {text};

            if (_allowLineCarriage)
            {
                var extent = polygon.GetBoundingBox();
                var size = canvas.MeasureText(text, _font);

                if (size.Width > extent.Width)
                {
                    labelTexts = SplitString(text);
                }
            }
            else if (_forceLineCarriage)
            {
                labelTexts = SplitString(text);
            }

            return labelTexts;
        }

        private static string[] SplitString(string text)
        {
            var subTexts = text.Split(' ');

            var resultStrings = new List<string>();
            resultStrings.Add(subTexts[0]);

            for (var i = 1; i < subTexts.Length; i++)
            {
                if (subTexts[i].Length > 2 && (subTexts[i - 1].Length > 2 || (i > 1 && subTexts[i - 1].Length <= 2)))
                {
                    resultStrings.Add(subTexts[i]);
                }
                else
                {
                    resultStrings[resultStrings.Count - 1] += " " + subTexts[i];
                }
            }

            return resultStrings.ToArray();
        }

        private static PointShape GetMidPointFromTwoVertices(Vertex fromVertex, Vertex toVertex)
        {
            var pointX = (fromVertex.X + toVertex.X)*0.5;
            var pointY = (fromVertex.Y + toVertex.Y)*0.5;

            return new PointShape(pointX, pointY);
        }

        private static PointShape GetMidPointFromTwoPoints(PointShape fromVertex, PointShape toVertex)
        {
            var pointX = (fromVertex.X + toVertex.X)*0.5;
            var pointY = (fromVertex.Y + toVertex.Y)*0.5;

            return new PointShape(pointX, pointY);
        }

        private static PointShape GetPointFromDistanceAndDegree(PointShape point, double distance, double angle)
        {
            var radient = angle*Math.PI/180;

            var pointX = point.X + distance*Math.Cos(radient);
            var pointY = point.Y - distance*Math.Sin(radient);

            return new PointShape(pointX, pointY);
        }

        private PointShape GetLabelingLocationForPolygon(PolygonShape polygon)
        {
            PointShape centerPoint;
            switch (_polygonLabelingLocationMode)
            {
                case PolygonLabelingLocationMode.BoundingBoxCenter:
                    centerPoint = polygon.GetBoundingBox().GetCenterPoint();
                    break;
                default:
                    centerPoint = polygon.GetCenterPoint();
                    break;
            }

            if (double.IsNaN(centerPoint.X) || double.IsNaN(centerPoint.Y))
            {
                centerPoint = new PointShape(polygon.OuterRing.Vertices[0]);
            }
            var resultPoint = centerPoint;
            var extent = polygon.GetBoundingBox();

            if (!IsPointInsidePolygon(polygon, centerPoint, extent))
            {
                var upperLeftX = extent.UpperLeftPoint.X;
                var upperLeftY = extent.UpperLeftPoint.Y;
                var lowerRightX = extent.LowerRightPoint.X;
                var lowerRightY = extent.LowerRightPoint.Y;

                var northMidPoint = GetMidPointFromTwoPoints(new PointShape(centerPoint.X, upperLeftY), centerPoint);
                var eastMidPoint = GetMidPointFromTwoPoints(new PointShape(lowerRightX, centerPoint.Y), centerPoint);
                var southMidPoint = GetMidPointFromTwoPoints(new PointShape(centerPoint.X, lowerRightY), centerPoint);
                var westMidPoint = GetMidPointFromTwoPoints(new PointShape(upperLeftX, centerPoint.Y), centerPoint);

                if (IsPointInsidePolygon(polygon, northMidPoint, extent))
                {
                    resultPoint = northMidPoint;
                }
                else if (IsPointInsidePolygon(polygon, eastMidPoint, extent))
                {
                    resultPoint = eastMidPoint;
                }
                else if (IsPointInsidePolygon(polygon, southMidPoint, extent))
                {
                    resultPoint = southMidPoint;
                }
                else if (IsPointInsidePolygon(polygon, westMidPoint, extent))
                {
                    resultPoint = westMidPoint;
                }
            }

            return resultPoint;
        }

        private static PointShape GetNewUpperLeftPointByPlacement(PointShape upperLeftPoint, double width, double height,
            PointPlacement placementOfPoint)
        {
            var upperLeftPointX = upperLeftPoint.X;
            var upperLeftPointY = upperLeftPoint.Y;
            PointShape newUpperLeftPoint = null;

            switch (placementOfPoint)
            {
                case PointPlacement.UpperRight:
                    newUpperLeftPoint = new PointShape(upperLeftPointX, upperLeftPointY - height);
                    break;

                case PointPlacement.CenterRight:
                    newUpperLeftPoint = new PointShape(upperLeftPointX, upperLeftPointY - height*0.5);
                    break;

                case PointPlacement.LowerRight:
                    newUpperLeftPoint = new PointShape(upperLeftPointX, upperLeftPointY);
                    break;

                case PointPlacement.UpperLeft:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width, upperLeftPointY - height);
                    break;

                case PointPlacement.CenterLeft:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width, upperLeftPointY - height*0.5);
                    break;

                case PointPlacement.LowerLeft:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width, upperLeftPointY);
                    break;

                case PointPlacement.UpperCenter:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width*0.5, upperLeftPointY - height);
                    break;

                case PointPlacement.Center:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width*0.5, upperLeftPointY - height*0.5);
                    break;

                case PointPlacement.LowerCenter:
                    newUpperLeftPoint = new PointShape(upperLeftPointX - width*0.5, upperLeftPointY);
                    break;

                default:
                    break;
            }

            return newUpperLeftPoint;
        }

        private PointShape GetOrginalLabelPointShape(PointShape centerPointBePlacemented, double width, double height,
            PointPlacement placementOfLabel, float canvasDpi)
        {
            var upperLeftPointX = centerPointBePlacemented.X - width*0.5;
            var upperLeftPointY = centerPointBePlacemented.Y - height*0.5;

            var upperLeftPoint = new PointShape(upperLeftPointX, upperLeftPointY);
            var pointShape = GetNewUpperLeftPointByPlacement(upperLeftPoint, -width, -height, placementOfLabel);

            var oringalPoint = new PointShape(pointShape.X - _xOffsetInPixel*(canvasDpi/96.0),
                pointShape.Y + _yOffsetInPixel*(canvasDpi/96.0));

            return oringalPoint;
        }

        private PolygonShape GetTheLargestPolygonInMultipolygon(MultipolygonShape multipolygonShape)
        {
            var count = multipolygonShape.Polygons.Count;

            if (count == 1)
            {
                return multipolygonShape.Polygons[0];
            }

            var index = 0;
            var maxSize = double.MinValue;

            for (var i = 0; i < count; i++)
            {
                var boundingBox = multipolygonShape.Polygons[i].GetBoundingBox();
                var currentSize = boundingBox.Width + boundingBox.Height;
                if (currentSize > maxSize)
                {
                    maxSize = currentSize;
                    index = i;
                }
            }

            return multipolygonShape.Polygons[index];
        }

        private PolygonShape GetStraightMask(PointShape upperLeftPoint, double width, double height)
        {
            var points = new PointShape[5];
            points[0] = new PointShape(upperLeftPoint.X, upperLeftPoint.Y);
            points[1] = GetPointFromDistanceAndDegree(points[0], width, 0);
            points[2] = GetPointFromDistanceAndDegree(points[1], height, 270);
            points[3] = GetPointFromDistanceAndDegree(points[2], width, 180);

            if (_maskMargin != 0)
            {
                points[0].X -= _maskMargin;
                points[0].Y -= _maskMargin;

                points[1].X += _maskMargin;
                points[1].Y -= _maskMargin;

                points[2].X += _maskMargin;
                points[2].Y += _maskMargin;

                points[3].X -= _maskMargin;
                points[3].Y += _maskMargin;
            }

            points[4] = points[0];

            var polygon = new PolygonShape();
            for (var i = 0; i < points.Length; i++)
            {
                polygon.OuterRing.Vertices.Add(new Vertex(points[i].X, points[i].Y));
            }

            return polygon;
        }

        private PolygonShape GetInclineMask(PointShape upperCenterPoint, double width, double height, double angle)
        {
            var points = new PointShape[5];
            points[0] = GetPointFromDistanceAndDegree(upperCenterPoint, width*0.5, angle);
            points[1] = GetPointFromDistanceAndDegree(points[0], height, angle + 270);
            points[2] = GetPointFromDistanceAndDegree(points[1], width, angle + 180);
            points[3] = GetPointFromDistanceAndDegree(points[2], height, angle + 90);

            if (_maskMargin != 0)
            {
                points[0] = GetPointFromDistanceAndDegree(points[0], _maskMargin, angle);
                points[0] = GetPointFromDistanceAndDegree(points[0], _maskMargin, angle + 90);

                points[1] = GetPointFromDistanceAndDegree(points[1], _maskMargin, angle);
                points[1] = GetPointFromDistanceAndDegree(points[1], _maskMargin, angle + 270);

                points[2] = GetPointFromDistanceAndDegree(points[2], _maskMargin, angle + 180);
                points[2] = GetPointFromDistanceAndDegree(points[2], _maskMargin, angle + 270);

                points[3] = GetPointFromDistanceAndDegree(points[3], _maskMargin, angle + 180);
                points[3] = GetPointFromDistanceAndDegree(points[3], _maskMargin, angle + 90);
            }

            points[4] = points[0];

            var polygon = new PolygonShape();

            for (var i = 0; i < points.Length; i++)
            {
                polygon.OuterRing.Vertices.Add(new Vertex((float) (points[i].X), (float) (points[i].Y)));
            }

            return polygon;
        }

        private LabelInformation GetLabelInfo(string text, double height, PointShape centerPoint, double yOffset)
        {
            var center = GetPointFromDistanceAndDegree(centerPoint, height*0.5 + yOffset, _rotationAngle + 270);

            return new LabelInformation(center, text, _rotationAngle);
        }

        private static BaseShape ConvertPoint(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var startIndex = 0;

            return ConvertOnePoint(wellKnownBinary, ref startIndex, canvas);
        }

        private static BaseShape ConvertLine(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var startIndex = 0;

            return ConvertOneLine(wellKnownBinary, ref startIndex, canvas);
        }

        private static BaseShape ConvertPolygon(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var startIndex = 0;

            return ConvertOnePolygon(wellKnownBinary, ref startIndex, canvas);
        }

        private static BaseShape ConvertMultipoint(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var byteOrder = wellKnownBinary[0];

            var multipoint = new MultipointShape();

            var pointCount = GetIntegerFromArray(wellKnownBinary, 5, byteOrder);

            var index = 9;

            for (var i = 0; i < pointCount; i++)
            {
                var point = ConvertOnePoint(wellKnownBinary, ref index, canvas);
                multipoint.Points.Add(point);
            }

            return multipoint;
        }

        private static BaseShape ConvertMultiline(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var byteOrder = wellKnownBinary[0];

            var multiline = new MultilineShape();

            var lineCount = GetIntegerFromArray(wellKnownBinary, 5, byteOrder);

            var index = 9;

            for (var i = 0; i < lineCount; i++)
            {
                var line = ConvertOneLine(wellKnownBinary, ref index, canvas);
                multiline.Lines.Add(line);
            }

            return multiline;
        }

        private static BaseShape ConvertMultipolygon(Feature feature, BaseGeoCanvas canvas)
        {
            var wellKnownBinary = feature.GetWellKnownBinary();
            var byteOrder = wellKnownBinary[0];

            var multipolygon = new MultipolygonShape();

            var polygonCount = GetIntegerFromArray(wellKnownBinary, 5, byteOrder);

            var index = 9;

            for (var i = 0; i < polygonCount; i++)
            {
                var polygon = ConvertOnePolygon(wellKnownBinary, ref index, canvas);
                multipolygon.Polygons.Add(polygon);
            }

            return multipolygon;
        }

        private static BaseShape ConvertGeometryCollection(Feature feature, BaseGeoCanvas canvas)
        {
            var returnGeometryCollection = new GeometryCollectionShape();

            var geometryCollectionShape = (GeometryCollectionShape) feature.GetShape();
            foreach (var baseShape in geometryCollectionShape.Shapes)
            {
                baseShape.Id = feature.Id;
                var subShapeFeature = new Feature(baseShape, feature.ColumnValues);
                var subScreenShape = ConvertToScreenShape(subShapeFeature, canvas);
                returnGeometryCollection.Shapes.Add(subScreenShape);
            }

            return returnGeometryCollection;
        }

        private static PointShape ConvertOnePoint(byte[] wellKnownBinary, ref int startIndex, BaseGeoCanvas canvas)
        {
            var byteOrder = wellKnownBinary[startIndex];

            var index = startIndex + 5;

            var pointX = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
            index += 8;
            var pointY = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
            index += 8;

            var upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            var extentWidth = canvas.CurrentWorldExtent.Width;
            var extentHeight = canvas.CurrentWorldExtent.Height;

            var widthFactor = canvasWidth/extentWidth;
            var heightFactor = canvasHeight/extentHeight;

            var screenPointX = (pointX - upperLeftX)*widthFactor;
            var screenPointY = (upperLeftY - pointY)*heightFactor;

            startIndex = index;

            return new PointShape(screenPointX, screenPointY);
        }

        private static PolygonShape ConvertOnePolygon(byte[] wellKnownBinary, ref int startIndex, BaseGeoCanvas canvas)
        {
            var byteOrder = wellKnownBinary[startIndex];
            var ringCount = GetIntegerFromArray(wellKnownBinary, startIndex + 5, byteOrder);

            var upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            var extentWidth = canvas.CurrentWorldExtent.Width;
            var extentHeight = canvas.CurrentWorldExtent.Height;

            var widthFactor = canvasWidth/extentWidth;
            var heightFactor = canvasHeight/extentHeight;

            var polygonShape = new PolygonShape();
            var index = startIndex + 9;
            RingShape ringShape;

            for (var i = 0; i < ringCount; i++)
            {
                var vertexCount = GetIntegerFromArray(wellKnownBinary, index, byteOrder);
                index += 4;

                if (i == 0)
                {
                    ringShape = polygonShape.OuterRing;
                }
                else
                {
                    ringShape = new RingShape();
                    polygonShape.InnerRings.Add(ringShape);
                }

                for (var j = 0; j < vertexCount; j++)
                {
                    var pointX = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
                    index += 8;
                    var pointY = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
                    index += 8;

                    var screenPointX = (pointX - upperLeftX)*widthFactor;
                    var screenPointY = (upperLeftY - pointY)*heightFactor;

                    ringShape.Vertices.Add(new Vertex(screenPointX, screenPointY));
                }
            }

            startIndex = index;

            return polygonShape;
        }

        private static LineShape ConvertOneLine(byte[] wellKnownBinary, ref int startIndex, BaseGeoCanvas canvas)
        {
            var byteOrder = wellKnownBinary[startIndex];

            var count = GetIntegerFromArray(wellKnownBinary, startIndex + 5, byteOrder);

            var upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            var extentWidth = canvas.CurrentWorldExtent.Width;
            var extentHeight = canvas.CurrentWorldExtent.Height;

            var widthFactor = canvasWidth/extentWidth;
            var heightFactor = canvasHeight/extentHeight;

            var lineShape = new LineShape();
            var index = startIndex + 9;

            for (var i = 0; i < count; i++)
            {
                var pointX = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
                index += 8;
                var pointY = GetDoubleFromArray(wellKnownBinary, index, byteOrder);
                index += 8;

                var screenPointX = (pointX - upperLeftX)*widthFactor;
                var screenPointY = (upperLeftY - pointY)*heightFactor;

                lineShape.Vertices.Add(new Vertex(screenPointX, screenPointY));
            }

            startIndex = index;

            return lineShape;
        }

        internal static Feature ConvertToWorldCoordinate(PolygonShape simplyPolygon, BaseGeoCanvas canvas)
        {
            var upperLeftX = canvas.CurrentWorldExtent.UpperLeftPoint.X;
            var upperLeftY = canvas.CurrentWorldExtent.UpperLeftPoint.Y;
            double canvasWidth = canvas.Width;
            double canvasHeight = canvas.Height;
            var extentWidth = canvas.CurrentWorldExtent.Width;
            var extentHeight = canvas.CurrentWorldExtent.Height;

            var widthFactor = extentWidth/canvasWidth;
            var heightFactor = extentHeight/canvasHeight;

            var count = simplyPolygon.InnerRings.Count + 1;
            RingShape ringShape;

            var verticesCount = 0;

            for (var i = 0; i < count; i++)
            {
                ringShape = (i == 0) ? simplyPolygon.OuterRing : simplyPolygon.InnerRings[i - 1];

                verticesCount += ringShape.Vertices.Count;
            }

            var wellKnownBinary = new byte[9 + count*4 + verticesCount*16];
            var header = new byte[5] {1, 3, 0, 0, 0};
            CopyToArray(header, wellKnownBinary, 0);
            CopyToArray(BitConverter.GetBytes(count), wellKnownBinary, 5);
            var index = 9;

            for (var i = 0; i < count; i++)
            {
                ringShape = (i == 0) ? simplyPolygon.OuterRing : simplyPolygon.InnerRings[i - 1];

                CopyToArray(BitConverter.GetBytes(ringShape.Vertices.Count), wellKnownBinary, index);
                index += 4;

                for (var j = 0; j < ringShape.Vertices.Count; j++)
                {
                    var pointX = ringShape.Vertices[j].X;
                    var pointY = ringShape.Vertices[j].Y;

                    var worldPointX = pointX*widthFactor + upperLeftX;
                    var worldPointY = upperLeftY - pointY*heightFactor;

                    CopyToArray(BitConverter.GetBytes(worldPointX), wellKnownBinary, index);
                    index += 8;
                    CopyToArray(BitConverter.GetBytes(worldPointY), wellKnownBinary, index);
                    index += 8;
                }
            }

            return new Feature(wellKnownBinary, string.Empty, new Dictionary<string, string>());
        }

        private static PointPlacement SearchPlacement(int index, PointPlacement baseOfPointPlacement)
        {
            if (baseOfPointPlacement == PointPlacement.UpperRight || baseOfPointPlacement == PointPlacement.LowerRight ||
                baseOfPointPlacement == PointPlacement.UpperLeft || baseOfPointPlacement == PointPlacement.LowerLeft)
            {
                switch (index)
                {
                    case 0:
                        return PointPlacement.UpperRight;
                    case 1:
                        return PointPlacement.UpperLeft;
                    case 2:
                        return PointPlacement.LowerRight;
                    case 3:
                        return PointPlacement.LowerLeft;
                }
            }
            else
            {
                switch (index)
                {
                    case 0:
                        return PointPlacement.CenterRight;
                    case 1:
                        return PointPlacement.CenterLeft;
                    case 2:
                        return PointPlacement.UpperCenter;
                    case 3:
                        return PointPlacement.LowerCenter;
                }
            }

            return baseOfPointPlacement;
        }

        private string ApplyFormatOfString(string text)
        {
            string labelText = null;
            if (text == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(text.Trim()))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_textFormat) && string.IsNullOrEmpty(_numericFormat) &&
                string.IsNullOrEmpty(_dateFormat))
            {
                labelText = text.Trim();
            }
            else
            {
                if (!string.IsNullOrEmpty(_numericFormat))
                {
                    labelText =
                        string.Format(CultureInfo.CurrentCulture, _numericFormat,
                            double.Parse(text, CultureInfo.InvariantCulture)).Trim();
                }
                else if (!string.IsNullOrEmpty(_dateFormat))
                {
                    labelText =
                        string.Format(CultureInfo.CurrentCulture, _dateFormat,
                            DateTime.Parse(text, CultureInfo.InvariantCulture)).Trim();
                }
                else if (!string.IsNullOrEmpty(_textFormat))
                {
                    labelText = string.Format(CultureInfo.CurrentCulture, _textFormat, text).Trim();
                }
            }

            return labelText;
        }

        private static void CopyToArray(byte[] sourceArray, byte[] destinateArray, long destinateIndex)
        {
            for (var i = 0; i < sourceArray.Length; i++)
            {
                destinateArray[destinateIndex + i] = sourceArray[i];
            }
        }

        private static void CopyLabelingCandidate(LabelingCandidate sourceLabelingCandidate,
            LabelingCandidate destinateLabelingCandidate)
        {
            destinateLabelingCandidate.ScreenArea = sourceLabelingCandidate.ScreenArea;
            destinateLabelingCandidate.OriginalText = sourceLabelingCandidate.OriginalText;
            destinateLabelingCandidate.CenterPoint = sourceLabelingCandidate.CenterPoint;

            destinateLabelingCandidate.LabelInformation.Clear();

            foreach (var labelInfo in sourceLabelingCandidate.LabelInformation)
            {
                destinateLabelingCandidate.LabelInformation.Add(labelInfo);
            }
        }

        private static double GetDoubleFromArray(byte[] bytes, int startIndex, byte byteOrder)
        {
            if (byteOrder == 1)
            {
                return BitConverter.ToDouble(bytes, startIndex);
            }
            var tempArray = new byte[8];
            Array.Copy(bytes, startIndex, tempArray, 0, tempArray.Length);
            Array.Reverse(tempArray);

            return BitConverter.ToDouble(tempArray, 0);
        }

        private static int GetIntegerFromArray(byte[] bytes, int startIndex, byte byteOrder)
        {
            if (byteOrder == 1)
            {
                return BitConverter.ToInt32(bytes, startIndex);
            }
            var tempArray = new byte[4];
            Array.Copy(bytes, startIndex, tempArray, 0, tempArray.Length);
            Array.Reverse(tempArray);

            return BitConverter.ToInt32(tempArray, 0);
        }

        private static bool IsDisjointed(PolygonShape polygon01, PolygonShape polygon02)
        {
            var rectangle01 = GetBoundingBoxOfSpecitalPolygon(polygon01);
            var rectangle02 = GetBoundingBoxOfSpecitalPolygon(polygon02);

            var isDisjointed = false;
            if (rectangle01.UpperLeftPoint.X >= rectangle02.LowerRightPoint.X ||
                rectangle01.LowerRightPoint.X <= rectangle02.UpperLeftPoint.X ||
                rectangle01.LowerRightPoint.Y >= rectangle02.UpperLeftPoint.Y ||
                rectangle01.UpperLeftPoint.Y <= rectangle02.LowerRightPoint.Y)
            {
                isDisjointed = true;
            }

            return isDisjointed;
        }

        private static RectangleShape GetBoundingBoxOfSpecitalPolygon(PolygonShape polygon)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            var vertices = polygon.OuterRing.Vertices;
            foreach (var vertex in vertices)
            {
                if (minX > vertex.X)
                {
                    minX = vertex.X;
                }
                if (minY > vertex.Y)
                {
                    minY = vertex.Y;
                }
                if (maxX < vertex.X)
                {
                    maxX = vertex.X;
                }
                if (maxY < vertex.Y)
                {
                    maxY = vertex.Y;
                }
            }

            return new RectangleShape(minX, maxY, maxX, minY);
        }

        private static PointShape GetCenterPointOfPolygon(PolygonShape polygon)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            var vertices = polygon.OuterRing.Vertices;
            foreach (var vertex in vertices)
            {
                if (minX > vertex.X)
                {
                    minX = vertex.X;
                }
                if (minY > vertex.Y)
                {
                    minY = vertex.Y;
                }
                if (maxX < vertex.X)
                {
                    maxX = vertex.X;
                }
                if (maxY < vertex.Y)
                {
                    maxY = vertex.Y;
                }
            }

            var centerX = (minX + maxX)*0.5;
            var centerY = (minY + maxY)*0.5;

            return new PointShape(centerX, centerY);
        }

        private static bool IsPointInsidePolygon(PolygonShape polygon, PointShape point, RectangleShape boundingBox)
        {
            var pointX = point.X;
            var pointY = point.Y;

            return IsPointInsidePolygon(polygon, pointX, pointY, boundingBox);
        }

        private static bool IsPointInsidePolygon(PolygonShape polygon, double pointX, double pointY,
            RectangleShape boundingBox)
        {
            var upperLeftX = boundingBox.UpperLeftPoint.X;
            var upperLeftY = boundingBox.UpperLeftPoint.Y;
            var lowerRightX = boundingBox.LowerRightPoint.X;
            var lowerRightY = boundingBox.LowerRightPoint.Y;

            if (pointX < upperLeftX || pointX > lowerRightX || pointY > upperLeftY || pointY < lowerRightY)
            {
                return false;
            }

            var crossingPoints = new Collection<Vertex>();

            GetCrossingPoints(crossingPoints, polygon.OuterRing, pointX, pointY, pointX + boundingBox.Width);

            foreach (var ringShape in polygon.InnerRings)
            {
                GetCrossingPoints(crossingPoints, ringShape, pointX, pointY, pointX + boundingBox.Width);
            }

            if (crossingPoints.Count != 0)
            {
                return (Math.IEEERemainder(crossingPoints.Count, 2) != 0);
            }
            return false;
        }

        private static void GetCrossingPoints(Collection<Vertex> crossingPoints, RingShape ringShape, double pointX,
            double pointY, double secondLineEndPointX)
        {
            var outerVertices = ringShape.Vertices;

            for (var i = 1; i < outerVertices.Count; i++)
            {
                var startPoint = outerVertices[i - 1];
                var endPoint = outerVertices[i];
                var interPoint = GetPointFFromLineSegmentIntersection(startPoint, endPoint, pointX, pointY,
                    secondLineEndPointX);

                if (IsPointValid(interPoint) && IsPointNotDuplicate(crossingPoints, interPoint))
                {
                    crossingPoints.Add(interPoint);
                }
            }
        }

        private static Vertex GetPointFFromLineSegmentIntersection(Vertex startPoint, Vertex endPoint,
            double secondLineStartX, double secondLineStartY, double secondLineEndPointX)
        {
            var x1 = startPoint.X;
            var x2 = endPoint.X;
            var y1 = startPoint.Y;
            var y2 = endPoint.Y;
            var xp1 = secondLineStartX;
            var xp2 = secondLineEndPointX;

            if (y1 != y2)
            {
                var a = (x2 - x1)/(y2 - y1);
                var b = x1 - (a*y1);
                var yi = secondLineStartY;
                var xi = (a*yi) + b;

                if (IsInRange(yi, y1, y2) && IsInRange(xi, x1, x2) && IsInRange(xi, xp1, xp2))
                {
                    return new Vertex(xi, yi);
                }
            }

            return new Vertex(double.MinValue, double.MinValue);
        }

        private static bool IsPointNotDuplicate(Collection<Vertex> crossingPoints, Vertex vertex)
        {
            for (var i = 0; i < crossingPoints.Count; i++)
            {
                if (Math.Round(crossingPoints[i].X, 12) == Math.Round(vertex.X, 12) &&
                    Math.Round(crossingPoints[i].Y, 12) == Math.Round(vertex.Y, 12))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsInRange(double value, double start, double end)
        {
            return (value >= start && value <= end) || (value <= start && value >= end);
        }

        private static bool IsPointValid(Vertex vertex)
        {
            return (vertex.X != double.MinValue || vertex.Y != double.MinValue);
        }

        private static MultilineShape OverlappingForLine(LineShape line, RectangleShape rectangle)
        {
            var resultShape = new MultilineShape();

            if (line.Vertices.Count < 2)
            {
                return resultShape;
            }

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            var startVertex = line.Vertices[0];
            var newLine = new LineShape();
            var startVertexInRectangle = (startVertex.X >= upperLeftX && startVertex.Y <= upperLeftY &&
                                          startVertex.X <= lowerRightX && startVertex.Y >= lowerRightY);
            var isReCreateLineShape = false;

            for (var i = 1; i < line.Vertices.Count; i++)
            {
                var endVertex = line.Vertices[i];
                var endVertexInRectangle = (endVertex.X >= upperLeftX && endVertex.Y <= upperLeftY &&
                                            endVertex.X <= lowerRightX && endVertex.Y >= lowerRightY);

                if (!startVertexInRectangle && endVertexInRectangle)
                {
                    var crossingVertex = GetCrossingVertex(startVertex, endVertex, rectangle);
                    newLine.Vertices.Add(crossingVertex);
                    newLine.Vertices.Add(endVertex);
                    if (i == line.Vertices.Count - 1)
                    {
                        resultShape.Lines.Add(newLine);
                    }
                }
                else if (startVertexInRectangle && !endVertexInRectangle)
                {
                    var crossingVertex = GetCrossingVertex(startVertex, endVertex, rectangle);
                    if (newLine.Vertices.Count == 0)
                    {
                        newLine.Vertices.Add(startVertex);
                    }
                    newLine.Vertices.Add(crossingVertex);
                    resultShape.Lines.Add(newLine);
                    isReCreateLineShape = true;
                }
                else if (startVertexInRectangle)
                {
                    if (newLine.Vertices.Count == 0)
                    {
                        newLine.Vertices.Add(startVertex);
                    }
                    newLine.Vertices.Add(endVertex);
                }
                else
                {
                    var tempLine = OverlappingForLineWithTwoPoints(startVertex, endVertex, rectangle);

                    if (tempLine != null && tempLine.Vertices.Count == 2)
                    {
                        resultShape.Lines.Add(tempLine);
                        isReCreateLineShape = true;
                    }
                }

                if (isReCreateLineShape)
                {
                    isReCreateLineShape = false;
                    newLine = new LineShape();
                }

                startVertexInRectangle = endVertexInRectangle;
                startVertex = endVertex;
            }

            if ((resultShape.Lines.Count == 0 && newLine.Vertices.Count > 1))
            {
                resultShape.Lines.Add(newLine);
            }

            return resultShape;
        }

        private static LineShape OverlappingForLineWithTwoPoints(Vertex startPoint, Vertex endPoint,
            RectangleShape rectangle)
        {
            var startX = startPoint.X;
            var startY = startPoint.Y;
            var endX = endPoint.X;
            var endY = endPoint.Y;

            var isHorizontal = ((endY - startY) == 0);
            var isVertical = ((endX - startX) == 0);

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            LineShape lineShape = null;

            if (isHorizontal)
            {
                if (IsInRange(upperLeftX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(upperLeftX, endY));
                }
                else if (IsInRange(lowerRightX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(lowerRightX, endY));
                }
            }
            else if (isVertical)
            {
                if (IsInRange(upperLeftY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(endX, upperLeftY));
                }
                else if (IsInRange(lowerRightY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    lineShape = new LineShape();
                    lineShape.Vertices.Add(new Vertex(endX, lowerRightY));
                }
            }
            else
            {
                var tangent = (endY - startY)/(endX - startX);

                if (IsInRange(upperLeftY, startY, endY))
                {
                    var newX = startX + ((upperLeftY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        lineShape = new LineShape();
                        lineShape.Vertices.Add(new Vertex(newX, upperLeftY));
                    }
                }

                if (IsInRange(lowerRightY, startY, endY))
                {
                    var newX = startX + ((lowerRightY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(newX, lowerRightY));
                    }
                }

                if (IsInRange(upperLeftX, startX, endX))
                {
                    var newY = startY + ((upperLeftX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(upperLeftX, newY));
                    }
                }

                if (IsInRange(lowerRightX, startX, endX))
                {
                    var newY = startY + ((lowerRightX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        if (lineShape == null)
                        {
                            lineShape = new LineShape();
                        }
                        lineShape.Vertices.Add(new Vertex(lowerRightX, newY));
                    }
                }
            }

            return lineShape;
        }

        private static Vertex GetCrossingVertex(Vertex startPoint, Vertex endPoint, RectangleShape rectangle)
        {
            var startX = startPoint.X;
            var startY = startPoint.Y;
            var endX = endPoint.X;
            var endY = endPoint.Y;

            var isHorizontal = ((endY - startY) == 0);
            var isVertical = ((endX - startX) == 0);

            var upperLeftX = rectangle.UpperLeftPoint.X;
            var upperLeftY = rectangle.UpperLeftPoint.Y;
            var lowerRightX = rectangle.LowerRightPoint.X;
            var lowerRightY = rectangle.LowerRightPoint.Y;

            if (isHorizontal)
            {
                if (IsInRange(upperLeftX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    return new Vertex(upperLeftX, endY);
                }
                if (IsInRange(lowerRightX, startX, endX) && IsInRange(endY, upperLeftY, lowerRightY))
                {
                    return new Vertex(lowerRightX, endY);
                }
            }
            else if (isVertical)
            {
                if (IsInRange(upperLeftY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    return new Vertex(endX, upperLeftY);
                }
                if (IsInRange(lowerRightY, startY, endY) && IsInRange(endX, upperLeftX, lowerRightX))
                {
                    return new Vertex(endX, lowerRightY);
                }
            }
            else
            {
                var tangent = (endY - startY)/(endX - startX);

                if (IsInRange(upperLeftY, startY, endY))
                {
                    var newX = startX + ((upperLeftY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        return new Vertex(newX, upperLeftY);
                    }
                }

                if (IsInRange(lowerRightY, startY, endY))
                {
                    var newX = startX + ((lowerRightY - startY)/tangent);
                    if (IsInRange(newX, upperLeftX, lowerRightX) && IsInRange(newX, startX, endX))
                    {
                        return new Vertex(newX, lowerRightY);
                    }
                }

                if (IsInRange(upperLeftX, startX, endX))
                {
                    var newY = startY + ((upperLeftX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        return new Vertex(upperLeftX, newY);
                    }
                }

                if (IsInRange(lowerRightX, startX, endX))
                {
                    var newY = startY + ((lowerRightX - startX)*tangent);
                    if (IsInRange(newY, upperLeftY, lowerRightY) && IsInRange(newY, startY, endY))
                    {
                        return new Vertex(lowerRightX, newY);
                    }
                }
            }

            return new Vertex(double.MaxValue, double.MaxValue);
        }
    }
}