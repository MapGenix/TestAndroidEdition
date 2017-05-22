using System;
using System.Collections.ObjectModel;
using System.Linq;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Styles;
using Mapgenix.Layers;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using System.Collections.Generic;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// TrackShape interactive process of the  Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class TrackInteractiveOverlay : BaseInteractiveOverlay
    {
        private const int TapTolerance = 14;
        private const string InTrackingFeatureKey = "InTrackingFeature";
        private const int TrackLayerId = 789456;

        private TrackMode _trackMode;
        private Collection<Vertex> _vertices;
        private InMemoryFeatureLayer _trackShapeLayer;
        private bool _isInTracking;
        private bool _isClicking;
        private int _mouseDownCount;
        private GestureDetector _gestureDetector;
        private Map _map;

        /*[NonSerialized]
        private TranslateTransform _translateTransform;*/

        public event EventHandler<ShapeEventArgs> TrackEnded;

        public event EventHandler<ShapeEventArgs> TrackEnding;

        public event EventHandler<VertexEventArgs> TrackStarted;

        public event EventHandler<VertexEventArgs> TrackStarting;

        public event EventHandler<VertexFeatureEventArgs> VertexAdded;

        public event EventHandler<VertexFeatureEventArgs> VertexAdding;

        public event EventHandler<VertexFeatureEventArgs> MouseMoved;

        public TrackInteractiveOverlay(Context context)
            : base(context)
        {
            _trackMode = TrackMode.None;
            RenderMode = RenderMode.GdiPlus;

            _trackShapeLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            _trackShapeLayer.Open();
            _trackShapeLayer.Columns.Add(new FeatureSource.FeatureSourceColumn("TYPE"));
            _trackShapeLayer.Close();

            _vertices = new Collection<Vertex>();
            OverlayCanvas.Elevation = ZIndexes.TrackInteractiveOverlay;
            MapSimpleGestureManager manager = new MapSimpleGestureManager();
            manager.LongPress += MotionLongPress;
            _gestureDetector = new GestureDetector(Context, manager);
            
            SetDefaultStyle();
        }

        public override bool IsEmpty
        {
            get
            {
                bool isEmpty = _trackShapeLayer.IsEmpty();
                if (isEmpty)
                {
                    Tile tile = OverlayCanvas.FindViewById<Tile>(TrackLayerId);
                    if (tile != null)
                    {
                        OverlayCanvas.RemoveView(tile);
                        tile.Dispose();
                        tile = null;
                    }
                }

                return isEmpty;
            }
        }

        public RenderMode RenderMode { get; set; }

        protected Collection<Vertex> Vertices
        {
            get { return _vertices; }
        }

        public InMemoryFeatureLayer TrackShapeLayer
        {
            get { return _trackShapeLayer; }
        }

       public int MouseDownCount
        {
            get { return _mouseDownCount; }
            set { _mouseDownCount = value; }
        }

        public bool OnTouchEvent(MotionEvent e, Map map)
        {

            _gestureDetector.OnTouchEvent(e);

            _map = map;
            PointF currentScreenPoint = new PointF(e.GetX(), e.GetY());

            MotionEventActions action = e.Action & MotionEventActions.Mask;
            switch(action)
            {
                case MotionEventActions.Down:
                    MotionDown(CollectMotionEventArguments(currentScreenPoint));
                    break;
                case MotionEventActions.Up:
                    MotionUp(CollectMotionEventArguments(currentScreenPoint));
                    break;
                case MotionEventActions.Move:
                    MotionMove(CollectMotionEventArguments(currentScreenPoint));
                    break;  
            }

            _map.Refresh(this);

            return true;
        }

        private MapMotionEventArgs CollectMotionEventArguments(PointF currentScreenPoint)
        {
            PointShape currentWorldPoint = ToWorldCoordinate(currentScreenPoint.X, currentScreenPoint.Y);
            MapMotionEventArgs arguments = new MapMotionEventArgs();
            arguments.CurrentExtent = _map.CurrentExtent;
            arguments.MapHeight = (int)_map.MapHeight;
            arguments.MapWidth = (int)_map.MapWidth;
            arguments.MapUnit = _map.MapUnit;
            arguments.Scale = _map.CurrentScale;
            arguments.ScreenX = (float)currentScreenPoint.X;
            arguments.ScreenY = (float)currentScreenPoint.Y;
            arguments.WorldX = currentWorldPoint.X;
            arguments.WorldY = currentWorldPoint.Y;
            arguments.MotionAction = MotionEventActions.Mask;

            if (!Double.IsNaN(_map.MapWidth) && _map.MapWidth != 0 && !Double.IsNaN(_map.MapHeight) && _map.MapHeight != 0)
            {
                arguments.SearchingTolerance = TapTolerance * Math.Max(_map.CurrentExtent.Width / _map.MapWidth, _map.CurrentExtent.Height / _map.MapHeight);
            }

            return arguments;
        }

        private void MotionDown(MapMotionEventArgs args)
        {
            Feature newPoint = new Feature();
            BaseShape shape = null;
            switch (_trackMode)
            {
                case TrackMode.Point:
                    _vertices = new Collection<Vertex>();
                    AddVertexWithEvents(new Vertex(args.WorldX, args.WorldY));

                    shape = GetTrackingShape();
                    newPoint = new Feature(shape);
                    newPoint.ColumnValues.Add("TYPE", "point");
                    _trackShapeLayer.Add(newPoint.Id, newPoint);
                    break;

                case TrackMode.Line:
                    AddVertexWithEvents(new Vertex(args.WorldX, args.WorldY));
                    newPoint = new Feature(args.WorldX, args.WorldY);
                    newPoint.ColumnValues.Add("TYPE", "tempVertex");
                    _trackShapeLayer.Add(newPoint.Id, newPoint);

                    if(_vertices.Count > 1)
                    {
                        shape = GetTrackingShape();
                        Feature tempLine = new Feature(shape.GetWellKnownBinary(), InTrackingFeatureKey);
                        _trackShapeLayer.Remove(InTrackingFeatureKey);
                        _trackShapeLayer.Add(InTrackingFeatureKey, tempLine);
                    }

                    break;

                case TrackMode.Polygon:
                    AddVertexWithEvents(new Vertex(args.WorldX, args.WorldY));
                    newPoint = new Feature(args.WorldX, args.WorldY);
                    newPoint.ColumnValues.Add("TYPE", "tempVertex");
                    _trackShapeLayer.Add(newPoint.Id, newPoint);
                    
                    if(_vertices.Count > 1)
                    {
                        shape = GetTrackingShape();
                        Feature tempLine = new Feature(shape.GetWellKnownBinary(), InTrackingFeatureKey);
                        _trackShapeLayer.Remove(InTrackingFeatureKey);
                        _trackShapeLayer.Add(InTrackingFeatureKey, tempLine);
                    }

                    break;

                case TrackMode.Square:
                case TrackMode.Rectangle:
                case TrackMode.Circle:
                case TrackMode.Ellipse:

                    if (_vertices.Count == 0)
                    {
                        AddVertexWithEvents(new Vertex(args.WorldX, args.WorldY));
                        AddVertexWithEvents(new Vertex(args.WorldX, args.WorldY));
                    }

                    break;
            }
        }

        private void MotionLongPress(object sender, MotionEvent e)
        {
            PointF currentScreenPoint = new PointF(e.GetX(), e.GetY());

            switch(_trackMode)
            {
                case TrackMode.Line:
                case TrackMode.Polygon:
                    EndTracking();
                    break;
            }
        }

        private void MotionMove(MapMotionEventArgs args)
        {
            BaseShape shape = null;

            switch (_trackMode)
            {
                case TrackMode.Square:
                case TrackMode.Rectangle:
                case TrackMode.Circle:
                case TrackMode.Ellipse:
                    if (_vertices.Count == 2)
                    {
                        UpdateVertexWithEvents(1, new Vertex(args.WorldX, args.WorldY));

                        shape = GetTrackingShape();
                        Feature tempLine = new Feature(shape.GetWellKnownBinary(), InTrackingFeatureKey);
                        _trackShapeLayer.Remove(InTrackingFeatureKey);
                        _trackShapeLayer.Add(InTrackingFeatureKey, tempLine);
                    }

                    break;
            }
        }

        private void MotionUp(MapMotionEventArgs args)
        {
            switch(_trackMode)
            {
                case TrackMode.Square:
                case TrackMode.Rectangle:
                case TrackMode.Ellipse:
                case TrackMode.Circle:
                    EndTracking();
                    break;
            }
        }

        public BaseShape GetTrackingShape()
        {
            return GetTrackingShapeCore();
        }

        protected virtual BaseShape GetTrackingShapeCore()
        {
            BaseShape shape = null;
            switch (_trackMode)
            {
                case TrackMode.Point:
                    PointShape pointShape = null;
                    if (_vertices.Count > 0)
                    {
                        pointShape = new PointShape(_vertices[0]);
                    }
                    shape = pointShape;
                    break;
                case TrackMode.Line:
                case TrackMode.Freehand:
                case TrackMode.StraightLine:
                    LineShape lineShape = null;

                    if (_vertices.Count > 1)
                    {
                        lineShape = new LineShape(_vertices);
                    }

                    shape = lineShape;
                    break;
                case TrackMode.Polygon:
                    PolygonShape polygonShape = null;
                    LineShape line = null;
                    if (_vertices.Count == 2)
                    {
                        line = new LineShape(_vertices);
                        shape = line;
                    }

                    if (_vertices.Count >= 3)
                    {
                        RingShape ringShape = new RingShape(_vertices);
                        polygonShape = new PolygonShape(ringShape);
                        shape = polygonShape;
                    }
                    
                    break;
                case TrackMode.Rectangle:
                    RectangleShape rectangleShape = null;
                    if (_vertices.Count == 2)
                    {
                        double upperLeftX = Math.Min(_vertices[0].X, _vertices[1].X);
                        double upperLeftY = Math.Max(_vertices[0].Y, _vertices[1].Y);
                        double lowerRightX = Math.Max(_vertices[0].X, _vertices[1].X);
                        double lowerRightY = Math.Min(_vertices[0].Y, _vertices[1].Y);
                        rectangleShape = new RectangleShape(upperLeftX, upperLeftY, lowerRightX, lowerRightY);
                    }
                    shape = rectangleShape;
                    break;
                case TrackMode.Square:
                    {
                        PolygonShape squareShape = null;
                        double xOffset = _vertices[1].X - _vertices[0].X;
                        double yOffset = _vertices[1].Y - _vertices[0].Y;

                        Vertex[] squareVertices = new Vertex[5];
                        squareVertices[0] = new Vertex(_vertices[0].X - xOffset, _vertices[0].Y - yOffset);
                        squareVertices[1] = new Vertex(_vertices[0].X - yOffset, _vertices[0].Y + xOffset);
                        squareVertices[2] = new Vertex(_vertices[0].X + xOffset, _vertices[0].Y + yOffset);
                        squareVertices[3] = new Vertex(_vertices[0].X + yOffset, _vertices[0].Y - xOffset);
                        squareVertices[4] = new Vertex(_vertices[0].X - xOffset, _vertices[0].Y - yOffset);

                        squareShape = new PolygonShape(new RingShape(squareVertices));

                        shape = squareShape;
                    }
                    break;
                case TrackMode.Circle:
                    PolygonShape circlePolygon = null;
                    if (_vertices.Count == 2)
                    {
                        double radius = Math.Sqrt(Math.Pow((_vertices[0].X - _vertices[1].X), 2) + Math.Pow((_vertices[0].Y - _vertices[1].Y), 2));
                        if (radius == 0) { radius += 0.000000001; }
                        EllipseShape ellipseShape = new EllipseShape(new PointShape(_vertices[0]), radius);
                        circlePolygon = ellipseShape.ToPolygon(9);
                    }

                    shape = circlePolygon;
                    break;
                case TrackMode.Ellipse:
                    PolygonShape ellipsePolygon = null;
                    if (_vertices.Count == 2)
                    {
                        double upperLeftX = Math.Min(_vertices[0].X, _vertices[1].X);
                        double upperLeftY = Math.Max(_vertices[0].Y, _vertices[1].Y);
                        double lowerRightX = Math.Max(_vertices[0].X, _vertices[1].X);
                        double lowerRightY = Math.Min(_vertices[0].Y, _vertices[1].Y);

                        PointShape centerPoint = new PointShape((lowerRightX + upperLeftX) / 2, (upperLeftY + lowerRightY) / 2);
                        double horizontalRadius = lowerRightX - centerPoint.X;
                        if (horizontalRadius == 0) { horizontalRadius += 0.0000000001; }

                        double verticalRadius = upperLeftY - centerPoint.Y;
                        if (verticalRadius == 0) { verticalRadius += 0.0000000001; }
                        EllipseShape ellipseShape = new EllipseShape(centerPoint, horizontalRadius, verticalRadius);
                        ellipsePolygon = ellipseShape.ToPolygon(9);
                    }

                    shape = ellipsePolygon;
                    break;

                case TrackMode.None:
                case TrackMode.Custom:
                default:
                    break;
            }
            return shape;
        }

        public PointShape ToWorldCoordinate(double screenX, double screenY)
        {
            PointF worldPoint = MapUtil.ToWorldCoordinate(_map.CurrentExtent, screenX, screenY, _map.MapWidth, _map.MapHeight);
            return new PointShape(worldPoint.X, worldPoint.Y);
        }

        private void EndTracking()
        {
            _vertices.Clear();
            _trackShapeLayer.Open();
            Feature tempFeature = _trackShapeLayer.FeatureSource.GetFeatureById(InTrackingFeatureKey, ReturningColumnsType.AllColumns);
            if (tempFeature != null)
            {
                string featureId = Guid.NewGuid().ToString();
                Feature newFeature = new Feature(tempFeature.GetWellKnownBinary(), featureId);
                _trackShapeLayer.Remove(InTrackingFeatureKey);
                _trackShapeLayer.Add(newFeature);

                var features = _trackShapeLayer.FeatureSource.GetFeaturesByColumnValue("TYPE", "tempVertex");

                if(features.Count > 0)
                {
                    try
                    {
                        foreach (Feature f in features)
                        {
                            _trackShapeLayer.Remove(f.Id);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            _trackShapeLayer.Close();
            _map.Refresh(this);
        }

        protected virtual void OnTrackEnded(ShapeEventArgs e)
        {
            EventHandler<ShapeEventArgs> handler = TrackEnded;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTrackEnding(ShapeEventArgs e)
        {
            EventHandler<ShapeEventArgs> handler = TrackEnding;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTrackStarting(VertexEventArgs e)
        {
            EventHandler<VertexEventArgs> handler = TrackStarting;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnTrackStarted(VertexEventArgs e)
        {
            EventHandler<VertexEventArgs> handler = TrackStarted;

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

        protected virtual void OnMouseMoved(VertexFeatureEventArgs e)
        {
            EventHandler<VertexFeatureEventArgs> handler = MouseMoved;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public TrackMode TrackMode
        {
            get
            {
                return _trackMode;
            }
            set
            {
                _vertices = new Collection<Vertex>();
                _trackMode = value;
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            Validators.CheckParameterIsNotNull(targetExtent, "targetExtent");
            LayerTile tile = OverlayCanvas.FindViewById<LayerTile>(TrackLayerId);

            if (tile == null)
            {
                tile = new LayerTile(Context);
                tile.Id = TrackLayerId;
                tile.IsAsync = false;
                tile.HasWatermark = false;
                tile.Elevation = 1;
                OverlayCanvas.AddView(tile);
            }
            else
            {
                tile.DrawingLayers.Clear();
            }

            tile.TargetExtent = targetExtent;
            RelativeLayout.LayoutParams p = new RelativeLayout.LayoutParams((int)MapArguments.ActualWidth, (int)MapArguments.ActualHeight);
            tile.LayoutParameters = p;
            tile.ZoomLevelIndex = MapArguments.GetSnappedZoomLevelIndex(targetExtent);
            tile.DrawingLayers.Add(_trackShapeLayer);

            DrawTile(tile);
        }

       
       protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (disposing && OverlayCanvas.ChildCount != 0)
                {
                    Tile tile = OverlayCanvas.GetChildAt(0) as Tile;
                    if (tile != null)
                    {
                        tile.Dispose();
                    }
                }
            }
        }

        private Feature GetCurrentFeature()
        {
            Feature currentFeature;

            switch (_trackMode)
            {
                case TrackMode.Point:
                    if (_vertices.Count > 0)
                    {
                        currentFeature = new Feature(GetTrackingShape());
                    }
                    else
                    {
                        currentFeature = new Feature();
                    }
                    break;
                case TrackMode.Line:
                case TrackMode.Freehand:
                case TrackMode.StraightLine:
                    if (_vertices.Count >= 2)
                    {
                        currentFeature = new Feature(GetTrackingShape());
                    }
                    else
                    {
                        currentFeature = new Feature();
                    }
                    break;
                case TrackMode.Polygon:
                    if (_vertices.Count >= 4)
                    {
                        currentFeature = new Feature(GetTrackingShape());
                    }
                    else
                    {
                        currentFeature = new Feature();
                    }
                    break;
                case TrackMode.Rectangle:
                case TrackMode.Square:
                case TrackMode.Circle:
                case TrackMode.Ellipse:
                    if (_vertices.Count >= 2)
                    {
                        currentFeature = new Feature(GetTrackingShape());
                    }
                    else
                    {
                        currentFeature = new Feature();
                    }
                    break;
                case TrackMode.None:
                case TrackMode.Custom:
                default:
                    currentFeature = new Feature();
                    break;
            }

            return currentFeature;
        }

        private void AddVertexWithEvents(Vertex trackVertex)
        {
            if (_vertices.Count == 0)
            {
                VertexEventArgs args = new VertexEventArgs(trackVertex);
                OnTrackStarting(args);

                if (args.Cancel)
                {
                    return;
                }
            }

            Feature currentFeature = GetCurrentFeature();
            VertexFeatureEventArgs vertexAddingTrackInteractiveOverlayEventArgs = new VertexFeatureEventArgs(trackVertex, currentFeature);
            OnVertexAdding(vertexAddingTrackInteractiveOverlayEventArgs);
            if (vertexAddingTrackInteractiveOverlayEventArgs.Cancel)
            {
                return;
            }
            _vertices.Add(vertexAddingTrackInteractiveOverlayEventArgs.Vertex);
            currentFeature = GetCurrentFeature();
            OnVertexAdded(new VertexFeatureEventArgs(trackVertex, currentFeature));

            if (_vertices.Count == 1)
            {
                var e = new VertexEventArgs(trackVertex);
                OnTrackStarted(e);
                _vertices[0] = e.Vertex;
            }
        }

        private void UpdateVertexWithEvents(int index, Vertex trackVertex)
        {
            _vertices[index] = trackVertex;
            Feature currentFeature = GetCurrentFeature();
            var e = new VertexFeatureEventArgs(trackVertex, currentFeature);
            OnMouseMoved(e);
            _vertices[index] = e.Vertex;
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

        private void WriteTrackShapeLayer()
        {
            try
            {
                BaseShape baseShape = GetTrackingShape();
                if (baseShape != null)
                {
                    ShapeEventArgs e = new ShapeEventArgs(baseShape);
                    OnTrackEnding(e);
                    if (e.Cancel) { return; }

                    Feature feature = new Feature(baseShape);

                    if(_trackMode == TrackMode.Point)
                    {
                        _trackShapeLayer.Add(feature.Id, feature);
                    }
                    else
                    {
                        if (this._isInTracking)
                        {
                            if (_trackShapeLayer.Contains(InTrackingFeatureKey))
                            {
                                _trackShapeLayer.Replace(InTrackingFeatureKey, feature);
                            }
                            else
                            {
                                _trackShapeLayer.Add(InTrackingFeatureKey, feature);
                            }
                        }
                        else
                        {
                            if (_trackShapeLayer.Contains(InTrackingFeatureKey))
                            {
                                _trackShapeLayer.Remove(InTrackingFeatureKey);
                                _trackShapeLayer.Add(feature.Id, feature);
                            }

                            ShapeEventArgs endedEventArgs = new ShapeEventArgs(GetTrackingShape());
                            OnTrackEnded(endedEventArgs);
                        }
                    }
                }
            }
            finally
            {
            }
        }

        private void SetDefaultStyle()
        {
            ValueStyle valueStyle = new ValueStyle();
            valueStyle.ColumnName = "TYPE";

            valueStyle.ValueItems.Add(new ValueItem("point", 
                PointStyles.CreateSimpleCircleStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), 20, GeoColor.FromArgb(200, GeoColor.StandardColors.Black), 1)));
            valueStyle.ValueItems.Add(new ValueItem("tempVertex",
                PointStyles.CreateSimpleSquareStyle(GeoColor.StandardColors.White, 15, GeoColor.StandardColors.Black, 1)));
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(valueStyle);

            LineStyle lineStyle = LineStyles.CreateSimpleLineStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), 3, true);
            lineStyle.OuterPen.LineJoin = DrawingLineJoin.Round;
            lineStyle.InnerPen.LineJoin = DrawingLineJoin.Round;
            lineStyle.CenterPen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(lineStyle);

            AreaStyle areaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), GeoColor.FromArgb(255, GeoColor.StandardColors.Gray), 3);
            areaStyle.OutlinePen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(areaStyle);

            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
        }
    }
}
