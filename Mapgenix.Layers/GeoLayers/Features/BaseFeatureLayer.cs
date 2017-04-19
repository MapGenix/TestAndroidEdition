using System;
using System.Collections.ObjectModel;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using Mapgenix.FeatureSource;


namespace Mapgenix.Layers
{
    /// <summary>Represents a layer made of feature data.</summary>
    /// <remarks>Abstract class for all feature-based layers. Feature layers deal with the visualization of the underlying feature data source.<br/>
    /// 	<br/>
    /// To create one's own FeatureLayer, it is necessary to set a FeatureSource. </remarks>
    [Serializable]
    public abstract class BaseFeatureLayer : BaseLayer
    {
        double _drawingMarginPercentage = 10;

        EditTools _editTools;
        QueryTools _queryTools;
        BaseFeatureSource _featureSource;
        ZoomLevelSet _zoomLevelSet;
        DrawingQuality _drawingQuality;

        /// <summary>Raised right before drawing the features in the layer.</summary>
        /// <remarks>In event argument, there is a collection of features to draw. Features can be added or removed.</remarks>
        public event EventHandler<FeaturesEventArgs> DrawingFeatures;

        public event EventHandler<FeaturesEventArgs> DrawingWrappingFeatures;

        public RectangleShape WrappingExtent
        {
            get; set;
        }

        /// <summary>
        /// Thie property gets or sets whether allow wrap date line.
        /// </summary>
        public WrappingMode WrappingMode
        {
            get; set;
        }

        /// <summary>Constructor of the class.</summary>
        /// <returns>None</returns>
        protected BaseFeatureLayer()
        {
            WrappingMode = WrappingMode.Default;
            _zoomLevelSet = new ZoomLevelSet();
        }

        /// <summary>Raises the DrawingFeatures event.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<list type="bullet">
        /// 		<item>call from a sub class to raise the event.<br/>
        /// 			<br/>
        ///         To add or remove features to be drawn.</item>
        /// 	</list>
        /// </remarks>
        /// <param name="e">Event arguments for the event.</param>
        protected virtual void OnDrawingFeatures(FeaturesEventArgs e)
        {
            EventHandler<FeaturesEventArgs> handler = DrawingFeatures;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnDrawingWrappingFeatures(FeaturesEventArgs e)
        {
            EventHandler<FeaturesEventArgs> handler = DrawingWrappingFeatures;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>Gets the EditTools to easily edit features in the Feature Layer.</summary>
        /// <value>EditTools to easily edit features in the Feature Layer.</value>
        /// <remarks>EditTools is a wrapper for editing.</remarks>
        public EditTools EditTools
        {
            get
            {
                Validators.CheckLayerIsOpened(IsOpen);

                return _editTools;
            }
            protected set { _editTools = value; }
        }

        /// <summary>Returns a collection of strings of record id of features to exclude from the Layer.</summary>
        public Collection<string> FeatureIdsToExclude
        {
            get { return FeatureSource.FeatureIdsToExclude; }
        }

        /// <summary>Gets the QueryTools to easily query Features of the Feature Layer.</summary>
        /// <value>QueryTools to easily query Features of the Feature Layer.</value>
        /// <remarks>QueryTools is a wrapper for querying.</remarks>
        public QueryTools QueryTools
        {
            get
            {
                Validators.CheckLayerIsOpened(IsOpen);

                return _queryTools;
            }

            protected set { _queryTools = value; }
        }

        /// <summary>Sets up the EditTools and QueryTools objects.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// 	<para>Concrete wrapper of abstract method SetupToolsCore.<br/>
        /// 		<br/>
        ///     As a concrete public method that wraps a Core method, Mapgenix reserves the right
        ///     to add events and other logic returned by the Core version of the method.</para>
        /// </remarks>
        public void SetupTools()
        {
            SetupToolsCore();
        }

        /// <summary>Sets up the EditTools and QueryTools objects.</summary>
        /// <returns>None</returns>
        /// <remarks>
        /// <para>Concrete wrapper of abstract method SetupTools.</para>
        /// </remarks>
        protected virtual void SetupToolsCore()
        {
            _editTools = new EditTools(_featureSource);
            _queryTools = new QueryTools(_featureSource);
        }

        /// <summary>Opens the feature layer to get it ready to use.</summary>
        /// <remarks>Abstract method called from the concrete public method Open.</remarks>
        /// <returns>None</returns>
        protected override void OpenCore()
        {
            if (!_featureSource.IsOpen)
            {
                _featureSource.Open();
            }
            SetupTools();
        }

        /// <summary>Closes the feature layer and releases any resources it was using.</summary>
        /// <returns>None</returns>
        /// <remarks>Protected virtual method is called from concrete public method Close.</remarks>
        protected override void CloseCore()
        {
            if (_featureSource.IsOpen)
            {
                _featureSource.Close();
            }
        }

        /// <summary>Returns the bounding box of the feature layer.</summary>
        /// <returns>Bounding box of the feature layer..</returns>
        /// <remarks>Called from concrete public method GetBoundingBox.</remarks>
        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);

            return _featureSource.GetBoundingBox();
        }

        /// <summary>Gets the feature source for the feature layer.</summary>
        /// <value>Feature source for the feature layer.</value>
        /// <remarks>Feature source is the provider of data to the feature layer. 
        /// When you inherit from the FeatureLayer, set in the constructor the feature source to use.</remarks>
        public BaseFeatureSource FeatureSource
        {
            get { return _featureSource; }
            set { _featureSource = value; }
        }

        /// <summary>Gets and sets the drawing quality for the canvas where the feature layer draws.</summary>
        /// <value>Drawing quality for the canvas where the feature layer draws.</value>
        public DrawingQuality DrawingQuality
        {
            get { return _drawingQuality; }
            set { _drawingQuality = value; }
        }

        /// <summary>Gets and sets the ZoomLevelSet containing the specific zoom levels for the feature layer.</summary>
        /// <value>ZoomLevelSet containing the specific zoom levels for the feature layer.</value>
        /// <remarks>Each ZoomLevel contains the styles used to determine how to draw the features.</remarks>
        public ZoomLevelSet ZoomLevelSet
        {
            get { return _zoomLevelSet; }
            set { _zoomLevelSet = value; }
        }

        /// <summary>Gets and sets a drawing margin as a percentage of the size of the map
        /// to ensure the labeling for features on the edge is displayed.</summary>
        /// <value>Drawing margin as a percentage of the size of the map
        /// to ensure the labeling for features on the edge is displayed.</value>
        public double DrawingMarginPercentage
        {
            get { return _drawingMarginPercentage; }
            set { _drawingMarginPercentage = value; }
        }

        /// <summary>Returns true if the feature layer is open and false if it is not.</summary>
        /// <value>True if the feature layer is open and false if it is not.</value>
        protected override bool IsOpenCore
        {
            get { return _featureSource.IsOpen; }
        }

        /// <summary>Draws the features of the feature layer based on the parameters passed in.</summary>
        /// <returns>None</returns>
        /// <remarks>DrawCore is called when the layer is being drawn. It checks if the features are
        /// within the extent and whether it is within a defined zoom level. Then it applies the styles of the proper zoom level 
        /// to the features for drawing. Finally, it draws the features on the GeoImage or native image passed in.</remarks>
        /// <param name="canvas">GeoCanvas to draw the layer.</param>
        /// <param name="labelsInAllLayers">Labels in all the layers.</param>
        protected override void DrawCore(BaseGeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
        {
            Validators.CheckParameterIsNotNull(canvas, "canvas");
            Validators.CheckParameterIsNotNull(labelsInAllLayers, "labeledInLayers");
            Validators.CheckGeoCanvasIsInDrawing(canvas.IsDrawing);
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);

            DrawingQuality tempDrawingQuality = canvas.DrawingQuality;
            canvas.DrawingQuality = _drawingQuality;

            try
            {
                ZoomLevel currentZoomLevel = _zoomLevelSet.GetZoomLevelForDrawing(canvas.CurrentWorldExtent, canvas.Width, canvas.MapUnit, canvas.Dpi);
                _featureSource.DrawingProgressChanged += featureSource_ProgressDrawing;

                if (currentZoomLevel != null)
                {
                    Collection<string> columnNames = currentZoomLevel.GetRequiredColumnNames();
                    RectangleShape marginWorldExtent = MarginAffectWorldExtent(canvas.CurrentWorldExtent);



                    #region sync draw
                    Collection<Feature> features = _featureSource.GetFeaturesForDrawing(marginWorldExtent, canvas.Width, canvas.Height, columnNames);
                    FeaturesEventArgs featuresEventArgs = new FeaturesEventArgs(features);
                    OnDrawingFeatures(featuresEventArgs);

                    if (features.Count != 0)
                    {
                        Collection<SimpleCandidate> labeledFeatures = new Collection<SimpleCandidate>();
                        currentZoomLevel.Draw(canvas, features, labeledFeatures, labelsInAllLayers);
                    }

                    features.Clear();
                    #endregion


                   

                    if (WrappingMode == WrappingMode.WrapDateline)
                    {
                        Collection<Feature> wrappingFeaturesLeft = FeatureSource.GetWrappingFeaturesLeft(marginWorldExtent, canvas.Width, canvas.Height, columnNames, WrappingExtent);
                        if (wrappingFeaturesLeft.Count != 0)
                        {
                            while (canvas.CurrentWorldExtent.LowerLeftPoint.X < WrappingExtent.LowerLeftPoint.X)
                            {
                                canvas.CurrentWorldExtent.TranslateByOffset(WrappingExtent.Width, 0);
                                currentZoomLevel.Draw(canvas, wrappingFeaturesLeft, new Collection<SimpleCandidate>(), labelsInAllLayers);
                            }
                        }

                        Collection<Feature> wrappingFeaturesRight = FeatureSource.GetWrappingFeaturesRight(marginWorldExtent, canvas.Width, canvas.Height, columnNames, WrappingExtent);
                        if (wrappingFeaturesRight.Count != 0)
                        {
                            while (canvas.CurrentWorldExtent.LowerRightPoint.X > WrappingExtent.LowerRightPoint.X)
                            {
                                canvas.CurrentWorldExtent.TranslateByOffset(-WrappingExtent.Width, 0);
                                currentZoomLevel.Draw(canvas, wrappingFeaturesRight, new Collection<SimpleCandidate>(), labelsInAllLayers);
                            }
                        }
                    }
                }
            }
            finally
            {
                canvas.DrawingQuality = tempDrawingQuality;
                _featureSource.DrawingProgressChanged -= featureSource_ProgressDrawing;
            }
        }

        private RectangleShape MarginAffectWorldExtent(RectangleShape worldExtent)
        {
            RectangleShape newRectangle = (RectangleShape)worldExtent.CloneDeep();

            newRectangle.ScaleUp(_drawingMarginPercentage);

            return newRectangle;
        }

        private void featureSource_ProgressDrawing(object sender, ProgressEventArgs e)
        {
            OnDrawingProgressChanged(e);
        }
    }
}
