using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Mapgenix.Canvas;
using Mapgenix.Shapes;
using Mapgenix.Styles;
using Mapgenix.GSuite.Android.Properties;
using Mapgenix.FeatureSource;
using Mapgenix.Layers;
using Android.Content;
using Android.Graphics;
using NativeAndroid = Android;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// EditShape interative process with the Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class EditInteractiveOverlay : BaseInteractiveOverlay
    {
        public event EventHandler<FeatureEventArgs> FeatureDragged;
        public event EventHandler<FeatureEditEventArgs> FeatureDragging;
        public event EventHandler<FeatureEventArgs> FeatureResized;
        public event EventHandler<FeatureEditEventArgs> FeatureResizing;
        public event EventHandler<FeatureEventArgs> FeatureRotated;
        public event EventHandler<FeatureEditEventArgs> FeatureRotating;
        public event EventHandler<VertexFeatureEventArgs> VertexAdded;
        public event EventHandler<VertexFeatureEventArgs> VertexAdding;
        public event EventHandler<VertexFeatureEventArgs> VertexMoved;
        public event EventHandler<VertexEditEventArgs> VertexMoving;
        public event EventHandler<VertexFeatureEventArgs> VertexRemoved;
        public event EventHandler<VertexFeatureEventArgs> VertexRemoving;
        public event EventHandler<FeatureEventArgs> ControlPointSelected;
        public event EventHandler<PointShapeEventArgs> ControlPointSelecting;

        private const string ExistingFeatureColumnName = "state";
        private const string ExistingFeatureColumnValue = "selected";

        private Feature _originalEditingFeature;
        private ControlPointType _controlPointType;
        [NonSerialized]
        private TranslateTransform _translateTransform;

        private InMemoryFeatureLayer _editShapesLayer;
        private InMemoryFeatureLayer _dragControlPointsLayer;
        private InMemoryFeatureLayer _rotateControlPointsLayer;
        private InMemoryFeatureLayer _resizeControlPointsLayer;
        private InMemoryFeatureLayer _existingControlPointsLayer;

        private Feature _selectControlPointFeature;
        private GeographyUnit _mapUnit;
        private LayerTile _tile;

        private bool _canDrag;
        private bool _canReshape;
        private bool _canAddVertex;
        private bool _canResize;
        private bool _canRotate;
        private bool _canRemoveVertex;

       
        public EditInteractiveOverlay(Context context)
            : base(context)
        {
            //OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.EditInteractiveOverlay);
            OverlayCanvas.Elevation = ZIndexes.EditInteractiveOverlay;
            _translateTransform = new TranslateTransform(OverlayCanvas);
            RenderMode = RenderMode.GdiPlus;
            
            _editShapesLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            _dragControlPointsLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            _rotateControlPointsLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            _resizeControlPointsLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            Collection<FeatureSourceColumn> columns = new Collection<FeatureSourceColumn>() { new FeatureSourceColumn(ExistingFeatureColumnName) };
            _existingControlPointsLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer(columns, new Collection<Feature>());

            _editShapesLayer.Name = "EditShapesLayer";
            _dragControlPointsLayer.Name = "DragControlPointsLayer";
            _rotateControlPointsLayer.Name = "RotateControlPointLayer";
            _resizeControlPointsLayer.Name = "ResizeControlPointLayer";
            _existingControlPointsLayer.Name = "ExistingControlPointsLayer";
            _mapUnit = GeographyUnit.Meter;

            _canDrag = true;
            _canReshape = true;
            _canResize = true;
            _canRotate = true;
            _canAddVertex = true;
            _canRemoveVertex = true;

            SetDefaultStyle();

            _tile = GetLayerTile();
            OverlayCanvas.AddView(_tile);
        }

        public InMemoryFeatureLayer EditShapesLayer
        {
            get { return _editShapesLayer; }
        }

        public InMemoryFeatureLayer DragControlPointsLayer
        {
            get { return _dragControlPointsLayer; }
        }

        public InMemoryFeatureLayer RotateControlPointsLayer
        {
            get { return _rotateControlPointsLayer; }
        }

        public InMemoryFeatureLayer ResizeControlPointsLayer
        {
            get { return _resizeControlPointsLayer; }
        }

        public InMemoryFeatureLayer ExistingControlPointsLayer
        {
            get { return _existingControlPointsLayer; }
        }

        protected ControlPointType ControlPointType
        {
            get { return _controlPointType; }
            set { _controlPointType = value; }
        }

        protected Feature SelectControlPointFeature
        {
            get { return _selectControlPointFeature; }
        }

        protected Feature OriginalEditingFeature
        {
            get { return _originalEditingFeature; }
        }

        public bool CanDrag
        {
            get { return _canDrag; }
            set { _canDrag = value; }
        }

        public bool CanReshape
        {
            get { return _canReshape; }
            set { _canReshape = value; }
        }

        public bool CanResize
        {
            get { return _canResize; }
            set { _canResize = value; }
        }

        public bool CanRotate
        {
            get { return _canRotate; }
            set { _canRotate = value; }
        }

        public bool CanAddVertex
        {
            get { return _canAddVertex; }
            set { _canAddVertex = value; }
        }

        public bool CanRemoveVertex
        {
            get { return _canRemoveVertex; }
            set { _canRemoveVertex = value; }
        }

        public override bool IsEmpty
        {
            get
            {
                bool isEmpty = _editShapesLayer.IsEmpty();
                if (isEmpty)
                {
                    for(int i = 0; i < OverlayCanvas.ChildCount; i++)
                    {
                        Tile currentTile = (Tile)OverlayCanvas.GetChildAt(i);
                        currentTile.Dispose();
                    }

                    OverlayCanvas.RemoveAllViews();
                }

                return isEmpty;
            }
        }

        public RenderMode RenderMode { get; set; }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {

            Validators.CheckParameterIsNotNull(targetExtent, "targetExtent");
            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                if (PreviousExtent != null)
                {
                    double resolution = MapArguments.CurrentResolution;
                    double worldOffsetX = targetExtent.UpperLeftPoint.X - PreviousExtent.UpperLeftPoint.X;
                    double worldOffsetY = targetExtent.UpperLeftPoint.Y - PreviousExtent.UpperLeftPoint.Y;
                    double screenOffsetX = worldOffsetX / resolution;
                    double screenOffsetY = worldOffsetY / resolution;

                    _translateTransform.X -= (float)screenOffsetX;
                    _translateTransform.Y += (float)screenOffsetY;
                }
            }
            else
            {
                _translateTransform.X = 0;
                _translateTransform.Y = 0;

                if (OverlayCanvas.ChildCount == 0)
                {
                    OverlayCanvas.AddView(_tile);
                }

                _tile.TargetExtent = targetExtent;

                RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams(Convert.ToInt32(MapArguments.ActualWidth), Convert.ToInt32(MapArguments.ActualHeight));
                _tile.LayoutParameters = p;
                _tile.ZoomLevelIndex = MapArguments.GetSnappedZoomLevelIndex(targetExtent);
                DrawTile(_tile);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _tile != null)
            {
                _tile.Dispose();
            }
        }

        private void DrawTile(LayerTile tile)
        {
            int tileSw = (int)MapArguments.ActualWidth;
            int tileSh = (int)MapArguments.ActualHeight;
            using (Bitmap nativeImage = Bitmap.CreateBitmap(tileSw, tileSh, Bitmap.Config.Argb8888))
            {
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, tile.TargetExtent, MapArguments.MapUnit);
                DrawTileCore(geoCanvas);
                geoCanvas.EndDrawing();
                tile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
            }
        }

        protected virtual void DrawTileCore(GdiPlusAndroidGeoCanvas geoCanvas)
        {
            LayerTile layerTile = OverlayCanvas.GetChildAt(0) as LayerTile;
            if (layerTile != null)
            {
                layerTile.Draw(geoCanvas);
            }
        }

        private LayerTile GetLayerTile()
        {
            LayerTile layerTile = new LayerTile(Context);
            layerTile.IsAsync = false;
            layerTile.HasWatermark = false;
            layerTile.DrawingLayers.Add(EditShapesLayer);
            layerTile.DrawingLayers.Add(ExistingControlPointsLayer);
            layerTile.DrawingLayers.Add(ResizeControlPointsLayer);
            layerTile.DrawingLayers.Add(RotateControlPointsLayer);
            layerTile.DrawingLayers.Add(DragControlPointsLayer);
            return layerTile;
        }

        protected bool SetSelectedControlPoint(PointShape targetPointShape, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            bool result = false;

            Feature selectedFeature = SetSelectedControlPointCore(targetPointShape, searchingTolerance);
            if (_selectControlPointFeature.GetWellKnownBinary() == null)
            {
                _selectControlPointFeature = selectedFeature;
            }

            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                if (feature.Id == selectedFeature.Id)
                {
                    _originalEditingFeature = feature;
                    break;
                }
            }

            if (selectedFeature.ColumnValues != null && selectedFeature.ColumnValues.Count > 0)
            {
                if (selectedFeature.ColumnValues[ExistingFeatureColumnName] == ExistingFeatureColumnValue)
                {
                    result = true;
                }
            }

            return result;
        }

        protected virtual Feature SetSelectedControlPointCore(PointShape targetPointShape, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            PointShapeEventArgs selectedControlPointSettingEditInteractiveOverlayEventArgs =
                new PointShapeEventArgs(targetPointShape);
            OnControlPointSelecting(new PointShapeEventArgs());

            if (selectedControlPointSettingEditInteractiveOverlayEventArgs.Cancel)
            {
                return new Feature();
            }
            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);

            foreach (Feature feature in DragControlPointsLayer.GetAll())
            {
                if (feature.GetShape().IsWithin(searchingArea))
                {
                    ControlPointType = ControlPointType.Drag;

                    OnControlPointSelected(new FeatureEventArgs(feature));
                    return feature;
                }
            }

            foreach (Feature feature in RotateControlPointsLayer.GetAll())
            {
                if (feature.GetShape().IsWithin(searchingArea))
                {
                    ControlPointType = ControlPointType.Rotate;

                    OnControlPointSelected(new FeatureEventArgs(feature));
                    return feature;
                }
            }

            foreach (Feature feature in ResizeControlPointsLayer.GetAll())
            {
                if (feature.GetShape().IsWithin(searchingArea))
                {
                    ControlPointType = ControlPointType.Resize;

                    OnControlPointSelected(new FeatureEventArgs(feature));
                    return feature;
                }
            }

            foreach (Feature feature in ExistingControlPointsLayer.GetAll())
            {
                if (feature.GetShape().IsWithin(searchingArea))
                {
                    ControlPointType = ControlPointType.Vertex;
                    if (feature.ColumnValues.ContainsKey(ExistingFeatureColumnName))
                    {
                        feature.ColumnValues[ExistingFeatureColumnName] = ExistingFeatureColumnValue;
                    }
                    else
                    {
                        feature.ColumnValues.Add(ExistingFeatureColumnName, ExistingFeatureColumnValue);
                    }

                    OnControlPointSelected(new FeatureEventArgs(feature));
                    return feature;
                }
            }

            return new Feature();
        }

        protected void EndEditing(PointShape targetPointShape)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");

            EndEditingCore(targetPointShape);

            ControlPointType = ControlPointType.None;
            _selectControlPointFeature = new Feature();
        }

        protected virtual void EndEditingCore(PointShape targetPointShape)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");

            ClearAllControlPoints();
            CalculateAllControlPoints();
            ShowAllControlPointLayers(true);
        }

        protected void ClearAllControlPoints()
        {
            DragControlPointsLayer.Clear();
            RotateControlPointsLayer.Clear();
            ResizeControlPointsLayer.Clear();
            ExistingControlPointsLayer.Clear();
        }

        public void CalculateAllControlPoints()
        {
            try
            {
                ClearAllControlPoints();

                if (_canDrag) { CalculateDragControlPoints(); }
                if (_canRotate) { CalculateRotateControlPoints(); }
                if (_canResize) { CalculateResizeControlPoints(); }
                if (_canReshape) { CalculateVertexControlPoints(); }
            }
            catch (Exception ex)
            {
                NativeAndroid.Util.Log.Debug("MyApp", "ERROR CalculateDragControlPoints()!!!!!!!!");
                NativeAndroid.Util.Log.Debug("MyApp", ex.Message);
            }       
        }

        protected void CalculateDragControlPoints()
        {
            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                IEnumerable<Feature> dragControlPoints = CalculateDragControlPointsCore(feature);
                foreach (Feature dragControlPoint in dragControlPoints)
                {
                    Feature addedFeature = new Feature(dragControlPoint.GetWellKnownBinary(), feature.Id, dragControlPoint.ColumnValues);
                    DragControlPointsLayer.Add(addedFeature);
                }
            }
        }

        protected virtual IEnumerable<Feature> CalculateDragControlPointsCore(Feature feature)
        {
            BaseShape baseShape = feature.GetShape();
            PointShape centerPointShape = baseShape.GetCenterPoint();

            if (double.IsNaN(centerPointShape.X) || double.IsNaN(centerPointShape.Y) || double.IsInfinity(centerPointShape.X) || double.IsInfinity(centerPointShape.Y))
            {
                centerPointShape = baseShape.GetBoundingBox().UpperLeftPoint;
            }

            Feature dragPointFeature = new Feature(new Vertex(centerPointShape));
            Feature[] returnValues = new Feature[] { dragPointFeature };
            return returnValues;
        }

        protected void CalculateRotateControlPoints()
        {
            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                IEnumerable<Feature> rotateControlPoints = CalculateRotateControlPointsCore(feature);
                foreach (Feature rotateControlPoint in rotateControlPoints)
                {
                    Feature addedFeature = new Feature(rotateControlPoint.GetWellKnownBinary(), feature.Id, rotateControlPoint.ColumnValues);
                    RotateControlPointsLayer.Add(addedFeature);
                }
            }
        }

        protected virtual IEnumerable<Feature> CalculateRotateControlPointsCore(Feature feature)
        {
            RectangleShape boundingBox = feature.GetBoundingBox();
            double width = boundingBox.Width;
            double height = boundingBox.Height;

            PointShape rotatePointShape = new PointShape(boundingBox.LowerRightPoint.X + width * 0.1, boundingBox.LowerRightPoint.Y - height * 0.1);
            Feature rotatePointFeature = new Feature(new Vertex(rotatePointShape));

            Feature[] returnValues = new Feature[] { rotatePointFeature };
            return returnValues;
        }

        protected void CalculateResizeControlPoints()
        {
            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                IEnumerable<Feature> resizeControlPoints = CalculateResizeControlPointsCore(feature);
                foreach (Feature resizeControlPoint in resizeControlPoints)
                {
                    Feature addedFeature = new Feature(resizeControlPoint.GetWellKnownBinary(), feature.Id, resizeControlPoint.ColumnValues);
                    ResizeControlPointsLayer.Add(addedFeature);
                }
            }
        }

        protected virtual IEnumerable<Feature> CalculateResizeControlPointsCore(Feature feature)
        {
            RectangleShape boundingBox = feature.GetBoundingBox();
            double width = boundingBox.Width;
            double height = boundingBox.Height;

            PointShape resizePointShape = new PointShape(boundingBox.LowerLeftPoint.X - width * 0.1, boundingBox.LowerLeftPoint.Y - height * 0.1);
            Feature resizePointFeature = new Feature(new Vertex(resizePointShape));

            Feature[] returnValues = new Feature[] { resizePointFeature };
            return returnValues;
        }

        protected void CalculateVertexControlPoints()
        {
            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                IEnumerable<Feature> reShapeControlPoints = CalculateVertexControlPointsCore(feature);
                foreach (Feature resshapeControlPoint in reShapeControlPoints)
                {
                    Feature addedFeature = new Feature(resshapeControlPoint.GetWellKnownBinary(), feature.Id, resshapeControlPoint.ColumnValues);
                    addedFeature.ColumnValues.Add(ExistingFeatureColumnName, string.Empty);
                    ExistingControlPointsLayer.Add(addedFeature);
                }
            }
        }

        protected virtual IEnumerable<Feature> CalculateVertexControlPointsCore(Feature feature)
        {
            WellKnownType wellKnowType = feature.GetWellKnownType();
            IEnumerable<Feature> returnValues = new Collection<Feature>();
            switch (wellKnowType)
            {
                case WellKnownType.Multipoint:
                    returnValues = CaculateVertexControlPointsForMultipointTypeFeature(feature);
                    break;
                case WellKnownType.Line:
                    returnValues = CaculateVertexControlPointsForLineTypeFeature(feature);
                    break;
                case WellKnownType.Multiline:
                    returnValues = CaculateVertexControlPointsForMultilineTypeFeature(feature);
                    break;
                case WellKnownType.Polygon:
                    returnValues = CaculateVertexControlPointsForPolygonTypeFeature(feature);
                    break;
                case WellKnownType.Multipolygon:
                    returnValues = CaculateVertexControlPointsForMultipolygonTypeFeature(feature);
                    break;
                case WellKnownType.Point:
                case WellKnownType.Invalid:
                default:
                    break;
            }

            return returnValues;
        }

        protected override InteractiveResult MotionDownCore(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "interactionArguments");

            bool addSucceed = false;
            bool findSucceed = false;
            bool drawSelected = false;

            if (!EditShapesLayer.IsEmpty())
            {
                if (SetSelectedControlPoint(new PointShape(motionArgs.WorldX, motionArgs.WorldY), motionArgs.SearchingTolerance))
                {
                    drawSelected = true;
                }
            }

            if (_controlPointType != ControlPointType.None)
            {
                findSucceed = true;
            }
            else
            {
                addSucceed = this.AddVertex(new PointShape(motionArgs.WorldX, motionArgs.WorldY), motionArgs.SearchingTolerance);
                if (SetSelectedControlPoint(new PointShape(motionArgs.WorldX, motionArgs.WorldY), motionArgs.SearchingTolerance))
                {
                    drawSelected = true;
                }
            }
        
            InteractiveResult result = new InteractiveResult();
            if (addSucceed || drawSelected)
            {
                result.DrawThisOverlay = DrawType.Draw;
            }
            if (addSucceed || findSucceed)
            {
                result.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            }

            return result;
        }

        protected override InteractiveResult MotionMoveCore(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "interactionArguments");

            bool editSucceed = false;

            if (_controlPointType != ControlPointType.None)
            {
                PointShape sourceControlPoint = _selectControlPointFeature.GetShape() as PointShape;

                Feature editedFeature = new Feature();

                switch (_controlPointType)
                {
                    case ControlPointType.Drag:
                        editedFeature = DragFeature(_originalEditingFeature, sourceControlPoint, new PointShape(motionArgs.WorldX, motionArgs.WorldY));
                        break;
                    case ControlPointType.Rotate:
                        editedFeature = RotateFeature(_originalEditingFeature, sourceControlPoint, new PointShape(motionArgs.WorldX, motionArgs.WorldY));
                        break;
                    case ControlPointType.Resize:
                        editedFeature = ResizeFeature(_originalEditingFeature, sourceControlPoint, new PointShape(motionArgs.WorldX, motionArgs.WorldY));
                        break;
                    case ControlPointType.Vertex:
                        editedFeature = MoveVertex(_originalEditingFeature, sourceControlPoint, new PointShape(motionArgs.WorldX, motionArgs.WorldY));
                        break;
                    case ControlPointType.None:
                    default:
                        break;
                }

                bool existingNode = false;
                foreach (string key in EditShapesLayer.AllKeys())
                {
                    if (string.Equals(EditShapesLayer.Find(key).Id, _originalEditingFeature.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        EditShapesLayer.Replace(key, editedFeature);
                        foreach (string featureKey in _existingControlPointsLayer.AllKeys())
                        {
                            Feature feature = _existingControlPointsLayer.Find(featureKey);
                            if (feature.ColumnValues != null)
                            {
                                if (feature.ColumnValues[ExistingFeatureColumnName] == ExistingFeatureColumnValue)
                                {
                                    existingNode = true;
                                }
                            }
                        }
                        ShowAllControlPointLayers(false, existingNode);
                        editSucceed = true;
                        break;
                    }
                }
            }

            InteractiveResult result = new InteractiveResult();

            if (editSucceed)
            {
                result.DrawThisOverlay = DrawType.Draw;
                result.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            }
        
            return result;
        }

        protected override InteractiveResult MotionUpCore(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "interactionArguments");

            bool editSucceed = false;

            if (_controlPointType != ControlPointType.None)
            {
                EndEditing(new PointShape(motionArgs.WorldX, motionArgs.WorldY));
                editSucceed = true;
            }

            InteractiveResult result = new InteractiveResult();

            if (editSucceed)
            {
                result.DrawThisOverlay = DrawType.Draw;
                result.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            }
        
            return result;
        }

        /*protected override InteractiveResult MouseClickCore(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            bool editSucceed = false;
            editSucceed = this.AddVertex(new PointShape(interactionArguments.WorldX, interactionArguments.WorldY), interactionArguments.SearchingTolerance);
        
            InteractiveResult result = new InteractiveResult();
            if (editSucceed)
            {
                result.DrawThisOverlay = DrawType.Draw;
                result.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            }
            
            return result;
        }

        protected override InteractiveResult MouseDoubleClickCore(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            bool deleteSucceed = false;
            deleteSucceed = this.RemoveVertex(new PointShape(interactionArguments.WorldX, interactionArguments.WorldY), interactionArguments.SearchingTolerance);

            InteractiveResult result = new InteractiveResult();

            if (deleteSucceed)
            {
                result.DrawThisOverlay = DrawType.Draw;
                result.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            }

            return result;
        }*/

        protected Feature DragFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            return DragFeatureCore(sourceFeature, sourceControlPoint, targetControlPoint);
        }

        protected virtual Feature DragFeatureCore(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            FeatureEditEventArgs args = new FeatureEditEventArgs(sourceFeature, sourceControlPoint, targetControlPoint);
            OnFeatureDragging(args);

            if (args.Cancel)
            {
                return new Feature();
            }

            double offsetDistanceX = targetControlPoint.X - sourceControlPoint.X;
            double offsetDistanceY = targetControlPoint.Y - sourceControlPoint.Y;

            BaseShape baseShape = BaseShape.TranslateByOffset(sourceFeature.GetShape(), offsetDistanceX, offsetDistanceY, GeographyUnit.Meter, DistanceUnit.Meter);
            baseShape.Id = sourceFeature.Id;

            Feature returnFeature = new Feature(baseShape, sourceFeature.ColumnValues);
            OnFeatureDragged(new FeatureEventArgs(returnFeature));

            return returnFeature;
        }

        protected Feature ResizeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            return ResizeFeatureCore(sourceFeature, sourceControlPoint, targetControlPoint);
        }

        protected virtual Feature ResizeFeatureCore(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            FeatureEditEventArgs args = new FeatureEditEventArgs(sourceFeature, sourceControlPoint, targetControlPoint);
            OnFeatureResizing(args);

            if (args.Cancel)
            {
                return new Feature();
            }

            PointShape centerPointShape = sourceFeature.GetShape().GetCenterPoint();

            double currentDistance = MapUtil.GetDistance(targetControlPoint, centerPointShape);
            double referenceDistance = MapUtil.GetDistance(centerPointShape, sourceControlPoint);
            double scale = currentDistance / referenceDistance;

            BaseShape baseShape = BaseShape.ScaleTo(sourceFeature.GetShape(), scale);
            baseShape.Id = sourceFeature.Id;

            Feature returnFeature = new Feature(baseShape, sourceFeature.ColumnValues);
            OnFeatureResized(new FeatureEventArgs(returnFeature));
            return returnFeature;
        }

        protected Feature RotateFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            return RotateFeatureCore(sourceFeature, sourceControlPoint, targetControlPoint);
        }

        protected virtual Feature RotateFeatureCore(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            FeatureEditEventArgs args = new FeatureEditEventArgs(sourceFeature, sourceControlPoint, targetControlPoint);
            OnFeatureRotating(args);

            if (args.Cancel)
            {
                return new Feature();
            }

            PointShape centerPointShape = sourceFeature.GetShape().GetCenterPoint();

            float rotateAngle = GetRotatingAngle(targetControlPoint, sourceControlPoint, centerPointShape);

            BaseShape baseShape = BaseShape.Rotate(_originalEditingFeature, centerPointShape, rotateAngle);
            baseShape.Id = _originalEditingFeature.Id;

            Feature returnFeature = new Feature(baseShape, sourceFeature.ColumnValues);
            OnFeatureRotated(new FeatureEventArgs(returnFeature));

            return returnFeature;
        }

        protected bool AddVertex(PointShape targetPointShape, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            bool addSucceed = false;
            if (!_canAddVertex)
            {
                return false;
            }

            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);

            bool containExistingControlPoints = false;

            Collection<PointShape> allVertices = GetAllVerticesFromEditShapeLayer();

            foreach (PointShape vertex in allVertices)
            {
                if (vertex.IsWithin(searchingArea))
                {
                    containExistingControlPoints = true;
                    break;
                }
            }

            if (!containExistingControlPoints)
            {
                foreach (string key in _editShapesLayer.AllKeys())
                {
                    Feature editedFeature = AddVertexCore(_editShapesLayer.Find(key), targetPointShape, searchingTolerance);
                    if (editedFeature.GetWellKnownBinary() != null)
                    {
                        _editShapesLayer.Replace(key, editedFeature);
                        addSucceed = true;
                    }
                }

                if (addSucceed)
                {
                    EndEditing(targetPointShape);
                }
            }

            return addSucceed;
        }

        protected virtual Feature AddVertexCore(Feature targetFeature, PointShape targetPointShape, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            VertexFeatureEventArgs vertexAddingEditInteractiveOverlayEventArgs = new VertexFeatureEventArgs(targetFeature, new Vertex(targetPointShape));
            OnVertexAdding(vertexAddingEditInteractiveOverlayEventArgs);

            if (vertexAddingEditInteractiveOverlayEventArgs.Cancel)
            {
                return new Feature();
            }

            Feature returnFeature = new Feature();

            WellKnownType wellKnowType = targetFeature.GetWellKnownType();
            switch (wellKnowType)
            {
                case WellKnownType.Line:
                    returnFeature = AddVertexToLineFeature(targetFeature, targetPointShape, searchingTolerance);
                    break;
                case WellKnownType.Multiline:
                    returnFeature = AddVertexToMultilineFeature(targetFeature, targetPointShape, searchingTolerance);
                    break;
                case WellKnownType.Polygon:
                    returnFeature = AddVertexToPolygonFeature(targetFeature, targetPointShape, searchingTolerance);
                    break;
                case WellKnownType.Multipolygon:
                    returnFeature = AddVertexToMultipolygonFeature(targetFeature, targetPointShape, searchingTolerance);
                    break;
                case WellKnownType.Multipoint:
                case WellKnownType.Point:
                case WellKnownType.Invalid:
                default:
                    break;
            }

            OnVertexAdded(new VertexFeatureEventArgs(returnFeature, new Vertex(targetPointShape)));

            return returnFeature;
        }

        protected Feature MoveVertex(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            return MoveVertexCore(sourceFeature, sourceControlPoint, targetControlPoint);
        }

        protected virtual Feature MoveVertexCore(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            Validators.CheckParameterIsNotNull(sourceControlPoint, "sourceControlPoint");
            Validators.CheckParameterIsNotNull(targetControlPoint, "targetControlPoint");

            VertexEditEventArgs args = new VertexEditEventArgs(sourceFeature, new Vertex(sourceControlPoint), targetControlPoint);
            OnVertexMoving(args);

            if (args.Cancel)
            {
                return new Feature();
            }

            Feature returnFeature = new Feature();

            WellKnownType wellKnowType = sourceFeature.GetWellKnownType();
            switch (wellKnowType)
            {
                case WellKnownType.Multipoint:
                    returnFeature = MoveVertexForMultipointTypeFeature(sourceFeature, sourceControlPoint, targetControlPoint);
                    break;
                case WellKnownType.Line:
                    returnFeature = MoveVertexForLineTypeFeature(sourceFeature, sourceControlPoint, targetControlPoint);
                    break;
                case WellKnownType.Multiline:
                    returnFeature = MoveVertexForMultiLineTypeFeature(sourceFeature, sourceControlPoint, targetControlPoint);
                    break;
                case WellKnownType.Polygon:
                    returnFeature = MoveVertexForPolygonTypeFeature(sourceFeature, sourceControlPoint, targetControlPoint);
                    break;
                case WellKnownType.Multipolygon:
                    returnFeature = MoveVertexForMultipolygonTypeFeature(sourceFeature, sourceControlPoint, targetControlPoint);
                    break;
                case WellKnownType.Point:
                case WellKnownType.Invalid:
                default:
                    break;
            }

            foreach (string featureKey in _existingControlPointsLayer.AllKeys())
            {
                Feature feature = _existingControlPointsLayer.Find(featureKey);
                if (feature.ColumnValues != null)
                {
                    if (feature.ColumnValues[ExistingFeatureColumnName] == ExistingFeatureColumnValue)
                    {
                        _existingControlPointsLayer.Replace(featureKey, new Feature(targetControlPoint.GetWellKnownBinary(), feature.Id, feature.ColumnValues));
                    }
                }
            }

            OnVertexMoved(new VertexFeatureEventArgs(returnFeature, new Vertex(targetControlPoint)));

            return returnFeature;
        }

        protected bool RemoveVertex(PointShape targetPointShape, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(targetPointShape, "targetPointShape");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            bool deleteSucceed = false;
            if (!_canRemoveVertex)
            {
                return false;
            }

            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);

            foreach (Feature feature in ExistingControlPointsLayer.GetAll())
            {
                PointShape pointShape = (PointShape)feature.GetShape();
                if (pointShape.IsWithin(searchingArea))
                {
                    foreach (string featureKey in EditShapesLayer.AllKeys())
                    {
                        if (EditShapesLayer.Find(featureKey).Id == feature.Id)
                        {
                            
                            Feature newFeature = RemoveVertexCore(EditShapesLayer.Find(featureKey), new Vertex(pointShape), 0);

                            if (newFeature.Id != null)
                            {
                                EditShapesLayer.Replace(featureKey, newFeature);
                                deleteSucceed = true;
                            }
                        }
                    }
                }
            }

            if (deleteSucceed)
            {
                EndEditing(targetPointShape);
            }

            return deleteSucceed;
        }

        protected virtual Feature RemoveVertexCore(Feature editShapeFeature, Vertex selectedVertex, double searchingTolerance)
        {
            Validators.CheckParameterIsNotNull(selectedVertex, "selectedVertex");
            Validators.CheckValueIsBiggerThanOrEqualToZero(searchingTolerance, "searchingTolerance");

            VertexFeatureEventArgs args = new VertexFeatureEventArgs(editShapeFeature, selectedVertex);
            OnVertexRemoving(args);

            if (args.Cancel)
            {
                return new Feature();
            }

            Feature newFeature = new Feature();
            bool deleteSucceed = false;
            WellKnownType wkt = editShapeFeature.GetWellKnownType();
            switch (wkt)
            {
                case WellKnownType.Line:
                    {
                        LineShape lineShape = (LineShape)editShapeFeature.GetShape();
                        deleteSucceed = LineShape.RemoveVertex(selectedVertex, lineShape);
                        if (deleteSucceed)
                        {
                            newFeature = new Feature(lineShape.GetWellKnownBinary(), editShapeFeature.Id, editShapeFeature.ColumnValues);
                        }
                    }
                    break;
                case WellKnownType.Polygon:
                    {
                        PolygonShape polygonShape = (PolygonShape)editShapeFeature.GetShape();
                        deleteSucceed = PolygonShape.RemoveVertex(selectedVertex, polygonShape);
                        if (deleteSucceed)
                        {
                            newFeature = new Feature(polygonShape.GetWellKnownBinary(), editShapeFeature.Id, editShapeFeature.ColumnValues);
                        }
                    }
                    break;
                case WellKnownType.Multipoint:
                    MultipointShape multipointShape = (MultipointShape)editShapeFeature.GetShape();
                    deleteSucceed = MultipointShape.RemoveVertex(selectedVertex, multipointShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multipointShape.GetWellKnownBinary(), editShapeFeature.Id, editShapeFeature.ColumnValues);
                    }
                    break;
                case WellKnownType.Multiline:
                    MultilineShape multilineShape = (MultilineShape)editShapeFeature.GetShape();
                    deleteSucceed = MultilineShape.RemoveVertex(selectedVertex, multilineShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multilineShape.GetWellKnownBinary(), editShapeFeature.Id, editShapeFeature.ColumnValues);
                    }
                    break;
                case WellKnownType.Multipolygon:
                    MultipolygonShape multipolygonShape = (MultipolygonShape)editShapeFeature.GetShape();
                    deleteSucceed = MultipolygonShape.RemoveVertex(selectedVertex, multipolygonShape);
                    if (deleteSucceed)
                    {
                        newFeature = new Feature(multipolygonShape.GetWellKnownBinary(), editShapeFeature.Id, editShapeFeature.ColumnValues);
                    }
                    break;
                case WellKnownType.Point:
                case WellKnownType.Invalid:
                default:
                    break;
            }

            OnVertexRemoved(new VertexFeatureEventArgs(editShapeFeature, selectedVertex));

            return newFeature;
        }

        protected virtual void OnFeatureDragging(FeatureEditEventArgs e)
        {
            EventHandler<FeatureEditEventArgs> handler = FeatureDragging;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFeatureDragged(FeatureEventArgs e)
        {
            EventHandler<FeatureEventArgs> handler = FeatureDragged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFeatureResizing(FeatureEditEventArgs e)
        {
            EventHandler<FeatureEditEventArgs> handler = FeatureResizing;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFeatureResized(FeatureEventArgs e)
        {
            EventHandler<FeatureEventArgs> handler = FeatureResized;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFeatureRotating(FeatureEditEventArgs e)
        {
            EventHandler<FeatureEditEventArgs> handler = FeatureRotating;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnFeatureRotated(FeatureEventArgs e)
        {
            EventHandler<FeatureEventArgs> handler = FeatureRotated;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnControlPointSelecting(PointShapeEventArgs e)
        {
            EventHandler<PointShapeEventArgs> handler = ControlPointSelecting;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnControlPointSelected(FeatureEventArgs e)
        {
            EventHandler<FeatureEventArgs> handler = ControlPointSelected;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexAdding(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = VertexAdding;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexAdded(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = VertexAdded;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexMoving(VertexEditEventArgs e)
        {
            EventHandler<VertexEditEventArgs> handler = VertexMoving;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexMoved(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = VertexMoved;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexRemoving(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = VertexRemoving;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnVertexRemoved(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = VertexRemoved;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        private static IEnumerable<Feature> CaculateVertexControlPointsForMultipointTypeFeature(Feature multipointFeature)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            MultipointShape multiPointShape = multipointFeature.GetShape() as MultipointShape;
            for (int k = 0; k < multiPointShape.Points.Count; k++)
            {
                PointShape pointShape = new PointShape(multiPointShape.Points[k].X, multiPointShape.Points[k].Y);
                returnValues.Add(new Feature(new Vertex(pointShape)));
            }

            return returnValues;
        }

        private static IEnumerable<Feature> CaculateVertexControlPointsForLineTypeFeature(Feature lineFeature)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            LineShape lineShape = lineFeature.GetShape() as LineShape;
            for (int k = 0; k < lineShape.Vertices.Count; k++)
            {
                Feature reshapeFeature = new Feature(lineShape.Vertices[k]);
                returnValues.Add(reshapeFeature);
            }

            return returnValues;
        }

        private static IEnumerable<Feature> CaculateVertexControlPointsForMultilineTypeFeature(Feature multiLineFeature)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            MultilineShape multiLineShape = multiLineFeature.GetShape() as MultilineShape;
            for (int j = 0; j < multiLineShape.Lines.Count; j++)
            {
                LineShape lineShape = multiLineShape.Lines[j];
                for (int k = 0; k < lineShape.Vertices.Count; k++)
                {
                    Feature reshapeFeature = new Feature(lineShape.Vertices[k]);
                    returnValues.Add(reshapeFeature);
                }
            }

            return returnValues;
        }

        private static IEnumerable<Feature> CaculateVertexControlPointsForPolygonTypeFeature(Feature polygonFeature)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            PolygonShape polygonShape = polygonFeature.GetShape() as PolygonShape;
            RingShape outRing = polygonShape.OuterRing;
            for (int k = 0; k < outRing.Vertices.Count; k++)
            {
                Feature reshapeFeature = new Feature(outRing.Vertices[k]);
                returnValues.Add(reshapeFeature);
            }

            for (int j = 0; j < polygonShape.InnerRings.Count; j++)
            {
                RingShape innerRing = polygonShape.InnerRings[j];
                for (int k = 0; k < innerRing.Vertices.Count; k++)
                {
                    Feature reshapeFeature = new Feature(innerRing.Vertices[k]);
                    returnValues.Add(reshapeFeature);
                }
            }

            return returnValues;
        }

        private static IEnumerable<Feature> CaculateVertexControlPointsForMultipolygonTypeFeature(Feature multipolygonFeature)
        {
            Collection<Feature> returnValues = new Collection<Feature>();

            MultipolygonShape multiPolygonShape = multipolygonFeature.GetShape() as MultipolygonShape;
            for (int i = 0; i < multiPolygonShape.Polygons.Count; i++)
            {
                PolygonShape polygonShape = multiPolygonShape.Polygons[i];
                RingShape outRing = polygonShape.OuterRing;
                for (int k = 0; k < outRing.Vertices.Count; k++)
                {
                    Feature reshapeFeature = new Feature(outRing.Vertices[k]);
                    returnValues.Add(reshapeFeature);
                }

                for (int j = 0; j < polygonShape.InnerRings.Count; j++)
                {
                    RingShape innerRing = polygonShape.InnerRings[j];
                    for (int k = 0; k < innerRing.Vertices.Count; k++)
                    {
                        Feature reshapeFeature = new Feature(innerRing.Vertices[k]);
                        returnValues.Add(reshapeFeature);
                    }
                }
            }
            return returnValues;
        }

        private Feature AddVertexToLineFeature(Feature lineFeature, PointShape targetPointShape, double searchingTolerance)
        {
            Feature returnFeature = new Feature();
            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);
            LineShape targetLineShape = lineFeature.GetShape() as LineShape;

            for (int i = 0; i < targetLineShape.Vertices.Count - 1; i++)
            {
                LineShape currentLine = new LineShape(new Vertex[] { targetLineShape.Vertices[i], targetLineShape.Vertices[i + 1] });
                if (searchingArea.Intersects(currentLine))
                {
                    Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                    targetLineShape.Vertices.Insert(i + 1, vertexToBeAdded);
                    returnFeature = new Feature(targetLineShape.GetWellKnownBinary(), lineFeature.Id, lineFeature.ColumnValues);
                    break;
                }
            }

            return returnFeature;
        }

        private Feature AddVertexToMultilineFeature(Feature multilineFeature, PointShape targetPointShape, double searchingTolerance)
        {
            Feature returnFeature = new Feature();

            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);
            MultilineShape targetMultilineShape = multilineFeature.GetShape() as MultilineShape;

            foreach (LineShape targetLineShape in targetMultilineShape.Lines)
            {
                for (int i = 0; i < targetLineShape.Vertices.Count - 1; i++)
                {
                    LineShape currentLine = new LineShape(new Vertex[] { targetLineShape.Vertices[i], targetLineShape.Vertices[i + 1] });
                    if (searchingArea.Intersects(currentLine))
                    {
                        Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                        targetLineShape.Vertices.Insert(i + 1, vertexToBeAdded);
                        return new Feature(targetMultilineShape.GetWellKnownBinary(), multilineFeature.Id, multilineFeature.ColumnValues);
                    }
                }
            }

            return returnFeature;
        }

        private Feature AddVertexToPolygonFeature(Feature polygonFeature, PointShape targetPointShape, double searchingTolerance)
        {
            Feature returnFeature = new Feature();

            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);
            PolygonShape targetPolygonShape = polygonFeature.GetShape() as PolygonShape;

            RingShape outerRing = targetPolygonShape.OuterRing;
            for (int i = 0; i < outerRing.Vertices.Count - 1; i++)
            {
                LineShape currentLine = new LineShape(new Vertex[] { outerRing.Vertices[i], outerRing.Vertices[i + 1] });
                if (searchingArea.Intersects(currentLine))
                {
                    Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                    outerRing.Vertices.Insert(i + 1, vertexToBeAdded);
                    return new Feature(targetPolygonShape.GetWellKnownBinary(), polygonFeature.Id, polygonFeature.ColumnValues);
                }
            }

            for (int i = 0; i < targetPolygonShape.InnerRings.Count; i++)
            {
                RingShape innerRing = targetPolygonShape.InnerRings[i];
                for (int j = 0; j < innerRing.Vertices.Count - 1; j++)
                {
                    LineShape currentLine = new LineShape(new Vertex[] { innerRing.Vertices[j], innerRing.Vertices[j + 1] });
                    if (searchingArea.Intersects(currentLine))
                    {
                        Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                        innerRing.Vertices.Insert(j + 1, vertexToBeAdded);
                        return new Feature(targetPolygonShape.GetWellKnownBinary(), polygonFeature.Id, polygonFeature.ColumnValues);
                    }
                }
            }

            return returnFeature;
        }

        private Feature AddVertexToMultipolygonFeature(Feature multipolygonFeature, PointShape targetPointShape, double searchingTolerance)
        {
            Feature returnFeature = new Feature();

            RectangleShape searchingArea = new RectangleShape(targetPointShape.X - searchingTolerance, targetPointShape.Y + searchingTolerance, targetPointShape.X + searchingTolerance, targetPointShape.Y - searchingTolerance);
            MultipolygonShape targetMultipolygonShape = multipolygonFeature.GetShape() as MultipolygonShape;

            foreach (PolygonShape targetPolygonShape in targetMultipolygonShape.Polygons)
            {
                RingShape outerRing = targetPolygonShape.OuterRing;
                for (int i = 0; i < outerRing.Vertices.Count - 1; i++)
                {
                    LineShape currentLine = new LineShape(new Vertex[] { outerRing.Vertices[i], outerRing.Vertices[i + 1] });
                    if (searchingArea.Intersects(currentLine))
                    {
                        Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                        outerRing.Vertices.Insert(i + 1, vertexToBeAdded);
                        returnFeature = new Feature(targetMultipolygonShape.GetWellKnownBinary(), multipolygonFeature.Id, multipolygonFeature.ColumnValues);
                        return returnFeature;
                    }
                }

                for (int i = 0; i < targetPolygonShape.InnerRings.Count; i++)
                {
                    RingShape innerRing = targetPolygonShape.InnerRings[i];
                    for (int j = 0; j < innerRing.Vertices.Count - 1; j++)
                    {
                        LineShape currentLine = new LineShape(new Vertex[] { innerRing.Vertices[j], innerRing.Vertices[j + 1] });
                        if (searchingArea.Intersects(currentLine))
                        {
                            Vertex vertexToBeAdded = new Vertex(currentLine.GetClosestPointTo(targetPointShape, _mapUnit));
                            innerRing.Vertices.Insert(j + 1, vertexToBeAdded);
                            returnFeature = new Feature(targetMultipolygonShape.GetWellKnownBinary(), multipolygonFeature.Id, multipolygonFeature.ColumnValues);
                            return returnFeature;
                        }
                    }
                }
            }

            return returnFeature;
        }

        private Collection<PointShape> GetAllVerticesFromEditShapeLayer()
        {
            Collection<PointShape> vertices = new Collection<PointShape>();

            foreach (Feature feature in EditShapesLayer.GetAll())
            {
                Collection<PointShape> featureVertices = GetAllVerticesFromFeature(feature);
                foreach (PointShape pointShape in featureVertices)
                {
                    vertices.Add(pointShape);
                }
            }

            return vertices;
        }

        private static Collection<PointShape> GetAllVerticesFromFeature(Feature feature)
        {
            WellKnownType wellKnowType = feature.GetWellKnownType();
            Collection<PointShape> returnValues = new Collection<PointShape>();

            switch (wellKnowType)
            {
                case WellKnownType.Multipoint:
                    returnValues = GetAllVerticesFromMultipointTypeFeature(feature);
                    break;
                case WellKnownType.Line:
                    returnValues = GetAllVerticesFromLineTypeFeature(feature);
                    break;
                case WellKnownType.Multiline:
                    returnValues = GetAllVerticesFromMultilineTypeFeature(feature);
                    break;
                case WellKnownType.Polygon:
                    returnValues = GetAllVerticesFromPolygonTypeFeature(feature);
                    break;
                case WellKnownType.Multipolygon:
                    returnValues = GetAllVerticesFromMultipolygonTypeFeature(feature);
                    break;
                case WellKnownType.Point:
                case WellKnownType.Invalid:
                default:
                    break;
            }

            return returnValues;
        }

        private static Collection<PointShape> GetAllVerticesFromMultipointTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            MultipointShape multiPointShape = feature.GetShape() as MultipointShape;
            for (int k = 0; k < multiPointShape.Points.Count; k++)
            {
                returnValues.Add(multiPointShape.Points[k]);
            }

            return returnValues;
        }

        private static Collection<PointShape> GetAllVerticesFromLineTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            LineShape lineShape = feature.GetShape() as LineShape;
            for (int k = 0; k < lineShape.Vertices.Count; k++)
            {
                returnValues.Add(new PointShape(lineShape.Vertices[k]));
            }

            return returnValues;
        }

        private static Collection<PointShape> GetAllVerticesFromMultilineTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            MultilineShape multiLineShape = feature.GetShape() as MultilineShape;
            for (int j = 0; j < multiLineShape.Lines.Count; j++)
            {
                LineShape lineShape = multiLineShape.Lines[j];
                for (int k = 0; k < lineShape.Vertices.Count; k++)
                {
                    returnValues.Add(new PointShape(lineShape.Vertices[k]));
                }
            }

            return returnValues;
        }

        private static Collection<PointShape> GetAllVerticesFromPolygonTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            PolygonShape polygonShape = feature.GetShape() as PolygonShape;
            RingShape outerRing = polygonShape.OuterRing;
            for (int k = 0; k < outerRing.Vertices.Count; k++)
            {
                returnValues.Add(new PointShape(outerRing.Vertices[k]));
            }

            for (int j = 0; j < polygonShape.InnerRings.Count; j++)
            {
                RingShape innerRing = polygonShape.InnerRings[j];
                for (int k = 0; k < innerRing.Vertices.Count; k++)
                {
                    returnValues.Add(new PointShape(innerRing.Vertices[k]));
                }
            }

            return returnValues;
        }

        private static Collection<PointShape> GetAllVerticesFromMultipolygonTypeFeature(Feature feature)
        {
            Collection<PointShape> returnValues = new Collection<PointShape>();

            MultipolygonShape multiPolygonShape = feature.GetShape() as MultipolygonShape;
            for (int i = 0; i < multiPolygonShape.Polygons.Count; i++)
            {
                PolygonShape polygonShape = multiPolygonShape.Polygons[i];
                RingShape outerRing = polygonShape.OuterRing;
                for (int k = 0; k < outerRing.Vertices.Count; k++)
                {
                    returnValues.Add(new PointShape(outerRing.Vertices[k]));
                }

                for (int j = 0; j < polygonShape.InnerRings.Count; j++)
                {
                    RingShape innerRing = polygonShape.InnerRings[j];
                    for (int k = 0; k < innerRing.Vertices.Count; k++)
                    {
                        returnValues.Add(new PointShape(innerRing.Vertices[k]));
                    }
                }
            }
            return returnValues;
        }

        private static Feature MoveVertexForMultipointTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultipointShape originalShape = sourceFeature.GetShape() as MultipointShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.Points.Count; i++)
            {
                PointShape currentPoint = originalShape.Points[i];

                if (currentPoint.X == sourceControlPoint.X && currentPoint.Y == sourceControlPoint.Y)
                {
                    originalShape.Points[i].X = targetControlPoint.X;
                    originalShape.Points[i].Y = targetControlPoint.Y;
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        private static Feature MoveVertexForLineTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            LineShape originalShape = sourceFeature.GetShape() as LineShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.Vertices.Count; i++)
            {
                if (originalShape.Vertices[i].X == sourceControlPoint.X && originalShape.Vertices[i].Y == sourceControlPoint.Y)
                {
                    originalShape.Vertices[i] = new Vertex(targetControlPoint);
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        private static Feature MoveVertexForMultiLineTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultilineShape originalShape = sourceFeature.GetShape() as MultilineShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.Lines.Count; i++)
            {
                for (int j = 0; j < originalShape.Lines[i].Vertices.Count; j++)
                {
                    Vertex currentVertex = originalShape.Lines[i].Vertices[j];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        originalShape.Lines[i].Vertices[j] = new Vertex(targetControlPoint);
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        private static Feature MoveVertexForPolygonTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            PolygonShape originalShape = sourceFeature.GetShape() as PolygonShape;
            originalShape.Id = sourceFeature.Id;

            for (int i = 0; i < originalShape.OuterRing.Vertices.Count; i++)
            {
                Vertex currentVertex = originalShape.OuterRing.Vertices[i];

                if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                {
                    originalShape.OuterRing.Vertices[i] = new Vertex(targetControlPoint);
                }
            }

            for (int i = 0; i < originalShape.InnerRings.Count; i++)
            {
                for (int j = 0; j < originalShape.InnerRings[i].Vertices.Count; j++)
                {
                    Vertex currentVertex = originalShape.InnerRings[i].Vertices[j];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        originalShape.InnerRings[i].Vertices[j] = new Vertex(targetControlPoint);
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        private static Feature MoveVertexForMultipolygonTypeFeature(Feature sourceFeature, PointShape sourceControlPoint, PointShape targetControlPoint)
        {
            MultipolygonShape originalShape = sourceFeature.GetShape() as MultipolygonShape;
            originalShape.Id = sourceFeature.Id;

            for (int j = 0; j < originalShape.Polygons.Count; j++)
            {
                for (int i = 0; i < originalShape.Polygons[j].OuterRing.Vertices.Count; i++)
                {
                    Vertex currentVertex = originalShape.Polygons[j].OuterRing.Vertices[i];

                    if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                    {
                        originalShape.Polygons[j].OuterRing.Vertices[i] = new Vertex(targetControlPoint);
                    }
                }

                for (int k = 0; k < originalShape.Polygons[j].InnerRings.Count; k++)
                {
                    for (int i = 0; i < originalShape.Polygons[j].InnerRings[k].Vertices.Count; i++)
                    {
                        Vertex currentVertex = originalShape.Polygons[j].InnerRings[k].Vertices[i];

                        if (currentVertex.X == sourceControlPoint.X && currentVertex.Y == sourceControlPoint.Y)
                        {
                            originalShape.Polygons[j].InnerRings[k].Vertices[i] = new Vertex(targetControlPoint);
                        }
                    }
                }
            }

            return new Feature(originalShape, sourceFeature.ColumnValues);
        }

        private void SetDefaultStyle()
        {
            _editShapesLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Red), 10, GeoColor.FromArgb(100, 0, 0, 255), 2);
            _editShapesLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle = LineStyles.CreateSimpleLineStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Red), 3, true);
            _editShapesLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Red), GeoColor.StandardColors.Gray, 3);
            _editShapesLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            MemoryStream draggingNode = new MemoryStream();
            Bitmap dragginImage = BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.Shape_move);
            dragginImage.Compress(Bitmap.CompressFormat.Png, 0, draggingNode);
            _dragControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = new PointStyle(new GeoImage(draggingNode));
            //_dragControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimplePointStyle(PointSymbolType.Circle, GeoColor.StandardColors.SkyBlue, 20);
            _dragControlPointsLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            MemoryStream rotatingNode = new MemoryStream();
            Bitmap rotateImage = BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.Shape_rotate);
            rotateImage.Compress(Bitmap.CompressFormat.Png, 0, rotatingNode);
            _rotateControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = new PointStyle(new GeoImage(rotatingNode));
            //_rotateControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimplePointStyle(PointSymbolType.Circle, GeoColor.StandardColors.SkyBlue, 20);
            _rotateControlPointsLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            MemoryStream resizingNode = new MemoryStream();
            Bitmap resizeImage = BitmapFactory.DecodeResource(Resources, global::Mapgenix.GSuite.Android.Resource.Drawable.Shape_resize);
            resizeImage.Compress(Bitmap.CompressFormat.Png, 0, resizingNode);
            _resizeControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = new PointStyle(new GeoImage(resizingNode));
            //_resizeControlPointsLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimplePointStyle(PointSymbolType.Circle, GeoColor.StandardColors.SkyBlue, 20);
            _resizeControlPointsLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;

            PointStyle selectedPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.StandardColors.Red, 10, GeoColor.StandardColors.Black); 
            ValueStyle valueStyle = new ValueStyle();
            valueStyle.ColumnName = ExistingFeatureColumnName;
            valueStyle.ValueItems.Add(new ValueItem(string.Empty, PointStyles.CreateSimpleCircleStyle(GeoColor.StandardColors.White, 9, GeoColor.StandardColors.Black)));
            valueStyle.ValueItems.Add(new ValueItem(ExistingFeatureColumnValue, selectedPointStyle));
            _existingControlPointsLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(valueStyle);
            _existingControlPointsLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
        }

        private void ShowAllControlPointLayers(bool visible)
        {
            _dragControlPointsLayer.IsVisible = visible;
            _rotateControlPointsLayer.IsVisible = visible;
            _resizeControlPointsLayer.IsVisible = visible;
            _existingControlPointsLayer.IsVisible = visible;


        }

        private void ShowAllControlPointLayers(bool visiable, bool existingControlPointVisible)
        {
            _dragControlPointsLayer.IsVisible = visiable;
            _rotateControlPointsLayer.IsVisible = visiable;
            _resizeControlPointsLayer.IsVisible = visiable;
            _existingControlPointsLayer.IsVisible = existingControlPointVisible;


        }

        private static float GetRotatingAngle(PointShape currentPosition, PointShape referencePointShape, PointShape centerPointShape)
        {
            float resultAngle;

            double angle0 = Math.Atan2(referencePointShape.Y - centerPointShape.Y, referencePointShape.X - centerPointShape.X);
            double angle1 = Math.Atan2(currentPosition.Y - centerPointShape.Y, currentPosition.X - centerPointShape.X);
            double angle = angle1 - angle0;
            angle = angle * 180 / Math.PI;
            angle = angle - Math.Floor(angle / 360) * 360;
            if (angle < 0) { angle = angle + 360; }

            resultAngle = (float)angle;
            return resultAngle;
        }

  
        /*private static GeoImage GetGeoImageFromResource(Image image, ImageFormat format)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, format);
                return new GeoImage(stream);
            }
                
        }*/
    }
}
