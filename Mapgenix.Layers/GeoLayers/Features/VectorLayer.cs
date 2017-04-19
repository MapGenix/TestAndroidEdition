using System;
using System.Collections.ObjectModel;
using AnnotationValidation.Attributes;
using Mapgenix.Canvas.Canvas;
using Mapgenix.Canvas.Events;
using Mapgenix.Canvas.Labels;
using Mapgenix.Canvas.Types;
using Mapgenix.Canvas.Zoom;
using Mapgenix.FeatureSource.Tools;
using Mapgenix.Layers.Events;
using Mapgenix.Layers.Types;
using Mapgenix.Shapes;

namespace Mapgenix.Layers.GeoLayers.Features
{

    [Serializable]
    public abstract class VectorLayer : Layer
    {
        double _drawingMarginPercentage = 10;

        EditTools _editTools;
        QueryTools _queryTools;
        FeatureSource.VectorSource _featureSource;
        ZoomLevelSet _zoomLevelSet;
        DrawingQuality _drawingQuality;
        RectangleShape _wrappingExtent;
        WrappingMode _wrappingMode;


        public event EventHandler<FeaturesEventArgs> DrawingFeatures;

        public event EventHandler<FeaturesEventArgs> DrawingWrappingFeatures;

        public RectangleShape WrappingExtent
        {
            get { return _wrappingExtent; }
            set { _wrappingExtent = value; }
        }

        public WrappingMode WrappingMode
        {
            get { return _wrappingMode; }
            set { _wrappingMode = value; }
        }


        protected VectorLayer()
        {
            WrappingMode = WrappingMode.Default;
            _zoomLevelSet = new ZoomLevelSet();
        }

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

        public EditTools EditTools
        {
            get
            {
                Validators.CheckLayerIsOpened(IsOpen);

                return _editTools;
            }
            protected set { _editTools = value; }
        }

        public Collection<string> FeatureIdsToExclude
        {
            get { return FeatureSource.FeatureIdsToExclude; }
        }

        public QueryTools QueryTools
        {
            get
            {
                Validators.CheckLayerIsOpened(IsOpen);

                return _queryTools;
            }

            protected set { _queryTools = value; }
        }

        protected void SetupTools()
        {
            SetupToolsCore();
        }

        protected virtual void SetupToolsCore()
        {
            _editTools = new EditTools(_featureSource);
            _queryTools = new QueryTools(_featureSource);
        }

        protected override void OpenCore()
        {
            if (!_featureSource.IsOpen)
            {
                _featureSource.Open();
            }
            SetupTools();
        }

        protected override void CloseCore()
        {
            if (_featureSource.IsOpen)
            {
                _featureSource.Close();
            }
        }

        protected override RectangleShape GetBoundingBoxCore()
        {
            Validators.CheckLayerIsOpened(IsOpen);
            Validators.CheckFeatureSourceIsOpen(_featureSource.IsOpen);

            return _featureSource.GetBoundingBox();
        }

		[Inject]
        public FeatureSource.VectorSource FeatureSource
        {
            get { return _featureSource; }
            set { _featureSource = value; }
        }

        public DrawingQuality DrawingQuality
        {
            get { return _drawingQuality; }
            set { _drawingQuality = value; }
        }

        public ZoomLevelSet ZoomLevelSet
        {
            get { return _zoomLevelSet; }
            set { _zoomLevelSet = value; }
        }

        public double DrawingMarginPercentage
        {
            get { return _drawingMarginPercentage; }
            set { _drawingMarginPercentage = value; }
        }

        protected override bool IsOpenCore
        {
            get { return _featureSource.IsOpen; }
        }

        protected override void DrawCore(GeoCanvas canvas, Collection<SimpleCandidate> labelsInAllLayers)
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

        RectangleShape MarginAffectWorldExtent(RectangleShape worldExtent)
        {
            RectangleShape newRectangle = (RectangleShape)worldExtent.CloneDeep();

            newRectangle.ScaleUp(_drawingMarginPercentage);

            return newRectangle;
        }

        void featureSource_ProgressDrawing(object sender, DrawingProgressEventArgs e)
        {
            OnDrawingProgressChanged(e);
        }
    }
}
