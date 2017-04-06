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

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// TrackShape interactive process of the  Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class TrackInteractiveOverlay : BaseInteractiveOverlay
    {
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

            _vertices = new Collection<Vertex>();
            OverlayCanvas.Elevation = ZIndexes.TrackInteractiveOverlay;

            //OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.TrackInteractiveOverlay);
            //_translateTransform = new TranslateTransform();
            //OverlayCanvas.RenderTransform = _translateTransform;
            _gestureDetector = new GestureDetector(Context, new MapSimpleGestureManager());

            _gestureDetector.DoubleTap += (object sender, GestureDetector.DoubleTapEventArgs e) => {
                //EventManagerDoubleTap(sender, e.Event);
            };

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

       
        protected void EndTracking()
        {
            _isInTracking = false;
        }

        public bool OnTouchEvent(MotionEvent e, Map map)
        {

            _gestureDetector.OnTouchEvent(e);

            _map = map;
            PointF currentScreenPoint = new PointF(e.GetX(), e.GetY());
            PointShape worldCoordinate = ToWorldCoordinate(e.GetX(), e.GetY());

            MotionEventActions action = e.Action & MotionEventActions.Mask;
            switch(action)
            {
                case MotionEventActions.Down:
                    _vertices = new Collection<Vertex>();
                    AddVertexWithEvents(new Vertex(worldCoordinate.X, worldCoordinate.Y));
                    Feature newPoint = new Feature(_vertices[0]);

                    _trackShapeLayer.Add(newPoint.Id, newPoint);
                    break;
            }

            _map.Refresh(this);

            return true;
        }

        public PointShape ToWorldCoordinate(double screenX, double screenY)
        {
            PointF worldPoint = MapUtil.ToWorldCoordinate(_map.CurrentExtent, screenX, screenY, _map.MapWidth, _map.MapHeight);
            return new PointShape(worldPoint.X, worldPoint.Y);
        }


        private InteractiveResult TapDown(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();
            if (this.TrackMode != TrackMode.None)
            {
                interactiveResult.DrawThisOverlay = DrawType.Draw;
                double xInWorld = interactionArguments.WorldX;
                double yInWorld = interactionArguments.WorldY;
                if (!_isInTracking)
                {
                    _vertices = new Collection<Vertex>();
                }

                //if (interactionArguments.MouseButton == MapMouseButton.Right) return interactiveResult;
                switch (_trackMode)
                {
                    case TrackMode.Point:
                        AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        break;
                    case TrackMode.Line:
                    case TrackMode.Freehand:
                    case TrackMode.StraightLine:
                        if (_vertices.Count == 0)
                        {
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        }
                        AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        break;
                    case TrackMode.Polygon:
                        _mouseDownCount++;
                        if (_mouseDownCount == 1)
                        {
                            _vertices = new Collection<Vertex>();

                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        }
                        else if (_mouseDownCount == 2)
                        {
                            Feature currentFeature = GetCurrentFeature();
                            VertexFeatureEventArgs vertexAddingTrackInteractiveOverlayEventArgs = new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature);
                            OnVertexAdding(vertexAddingTrackInteractiveOverlayEventArgs);
                            if (vertexAddingTrackInteractiveOverlayEventArgs.Cancel)
                            {
                                return interactiveResult;
                            }
                            this.Vertices[1] = vertexAddingTrackInteractiveOverlayEventArgs.Vertex;
                            currentFeature = GetCurrentFeature();
                            OnVertexAdded(new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature));
                        }
                        else if (_mouseDownCount == 3)
                        {
                            this.Vertices[2] = new Vertex(xInWorld, yInWorld);

                            Feature currentFeature = GetCurrentFeature();
                            VertexFeatureEventArgs vertexAddingTrackInteractiveOverlayEventArgs = new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature);
                            OnVertexAdding(vertexAddingTrackInteractiveOverlayEventArgs);
                            if (vertexAddingTrackInteractiveOverlayEventArgs.Cancel)
                            {
                                return interactiveResult;
                            }
                            this.Vertices.Insert(_mouseDownCount - 1, vertexAddingTrackInteractiveOverlayEventArgs.Vertex);
                            currentFeature = GetCurrentFeature();
                            OnVertexAdded(new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature));
                        }
                        else
                        {
                            Feature currentFeature = GetCurrentFeature();
                            VertexFeatureEventArgs vertexAddingTrackInteractiveOverlayEventArgs = new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature);
                            OnVertexAdding(vertexAddingTrackInteractiveOverlayEventArgs);
                            if (vertexAddingTrackInteractiveOverlayEventArgs.Cancel)
                            {
                                return interactiveResult;
                            }
                            this.Vertices.Insert(_mouseDownCount - 1, vertexAddingTrackInteractiveOverlayEventArgs.Vertex);
                            currentFeature = GetCurrentFeature();
                            OnVertexAdded(new VertexFeatureEventArgs(new Vertex(xInWorld, yInWorld), currentFeature));
                        }
                        break;
                    case TrackMode.Rectangle:
                    case TrackMode.Square:
                    case TrackMode.Circle:
                    case TrackMode.Ellipse:
                        if (_vertices.Count == 0)
                        {
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                            AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        }
                        break;
                    case TrackMode.None:
                    case TrackMode.Custom:
                    default:
                        interactiveResult.DrawThisOverlay = DrawType.DoNotDraw;
                        break;
                }

                _isInTracking = true;
                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                WriteTrackShapeLayer();
            }

            return interactiveResult;
        }

        protected override InteractiveResult MotionDownCore(MapMotionEventArgs motionArgs)
        {
            Validators.CheckParameterIsNotNull(motionArgs, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            if (!_isInTracking)
            {
                _vertices = new Collection<Vertex>();
            }

            _isInTracking = true;

            if (_trackMode != TrackMode.None)
            {
                interactiveResult.DrawThisOverlay = DrawType.Draw;
                switch (_trackMode)
                {
                    case TrackMode.Point:
                        AddVertexWithEvents(new Vertex(motionArgs.WorldX, motionArgs.WorldY));
                        break;
                    case TrackMode.Line:
                    case TrackMode.Freehand:
                    case TrackMode.StraightLine:
                    case TrackMode.Polygon:
                    case TrackMode.Rectangle:
                    case TrackMode.Square:
                    case TrackMode.Circle:
                    case TrackMode.Ellipse:
                    case TrackMode.Custom:
                    default:
                        interactiveResult.DrawThisOverlay = DrawType.DoNotDraw;
                        break;
                }
            }

            
            interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

            WriteTrackShapeLayer();

            if(_trackMode == TrackMode.Point)
            {
                EndTracking();
            }

            return interactiveResult;

        }

        /*protected override InteractiveResult MotionMoveCore(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            if (this.TrackMode != TrackMode.None && _isInTracking)
            {
                interactiveResult.DrawThisOverlay = DrawType.Draw;

                double xInWorld = interactionArguments.WorldX;
                double yInWorld = interactionArguments.WorldY;

                switch (_trackMode)
                {
                    case TrackMode.Point:
                        if (_vertices.Count != 0)
                        {
                            UpdateVertexWithEvents(0, new Vertex(xInWorld, yInWorld));
                        }
                        break;
                    case TrackMode.Line:
                    case TrackMode.StraightLine:
                        if (_vertices.Count > 0)
                        {
                            UpdateVertexWithEvents(_vertices.Count - 1, new Vertex(xInWorld, yInWorld));
                        }
                        break;
                    case TrackMode.Polygon:
                        UpdateVertexWithEvents(_mouseDownCount, new Vertex(xInWorld, yInWorld));
                        break;
                    case TrackMode.Rectangle:
                    case TrackMode.Square:
                    case TrackMode.Circle:
                    case TrackMode.Ellipse:
                        if (_vertices.Count == 2)
                        {
                            UpdateVertexWithEvents(1, new Vertex(xInWorld, yInWorld));
                        }
                        break;

                    case TrackMode.Freehand:
                        AddVertexWithEvents(new Vertex(xInWorld, yInWorld));
                        break;
                    case TrackMode.None:
                    case TrackMode.Custom:
                    default:
                        interactiveResult.DrawThisOverlay = DrawType.DoNotDraw;
                        break;
                }
                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                WriteTrackShapeLayer();
            }

            return interactiveResult;
        }*/

        private InteractiveResult TapUp(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            if (this.TrackMode != TrackMode.None)
            {
                switch (_trackMode)
                {
                    case TrackMode.Rectangle:
                    case TrackMode.Square:
                    case TrackMode.Circle:
                    case TrackMode.Ellipse:
                    case TrackMode.StraightLine:
                    case TrackMode.Freehand:
                        interactiveResult.DrawThisOverlay = DrawType.Draw;

                        EndTracking();
                        break;
                    case TrackMode.Point:
                        EndTracking();
                        break;
                    case TrackMode.None:
                    case TrackMode.Custom:
                    case TrackMode.Line:
                    case TrackMode.Polygon:
                    default:
                        break;
                }
                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                if (!_isClicking)
                {
                    WriteTrackShapeLayer();
                }
                else
                {
                    _isClicking = false;
                }
            }

            return interactiveResult;
        }

        /*protected override InteractiveResult CLick(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            DrawType mouseDownResult = DrawType.DoNotDraw;
            DrawType mouseUpResult = DrawType.DoNotDraw;
            if (this.TrackMode != TrackMode.None)
            {
                _isClicking = true;
                mouseDownResult = TapDown(interactionArguments).DrawThisOverlay;
                mouseUpResult = TapUp(interactionArguments).DrawThisOverlay;
                if (mouseDownResult == DrawType.Draw || mouseUpResult == DrawType.Draw)
                {
                    interactiveResult.DrawThisOverlay = DrawType.Draw;
                }

                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                WriteTrackShapeLayer();
            }

            return interactiveResult;
        }*/

        protected override InteractiveResult DoubleTapCore(MapMotionEventArgs interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            if (this.TrackMode != TrackMode.None)
            {
                interactiveResult.DrawThisOverlay = DrawType.Draw;

                switch (_trackMode)
                {
                    case TrackMode.Line:
                        EndTracking();
                        break;
                    case TrackMode.Polygon:
                        if (_mouseDownCount > 1)
                        {
                            _mouseDownCount = 0;
                            EndTracking();
                        }
                        break;
                    case TrackMode.Point:
                    case TrackMode.None:
                    case TrackMode.Rectangle:
                    case TrackMode.Square:
                    case TrackMode.Circle:
                    case TrackMode.Ellipse:
                    case TrackMode.StraightLine:
                    case TrackMode.Freehand:
                    case TrackMode.Custom:
                    default:
                        interactiveResult.DrawThisOverlay = DrawType.DoNotDraw;
                        break;
                }

                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                WriteTrackShapeLayer();
            }

            return interactiveResult;
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

                    if (_vertices.Count > 3)
                    {
                        RingShape ringShape = new RingShape(_vertices);
                        polygonShape = new PolygonShape(ringShape);
                    }

                    shape = polygonShape;
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
                _trackMode = value;
            }
        }

        protected override void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            Validators.CheckParameterIsNotNull(targetExtent, "targetExtent");
           
            /*if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                if (PreviousExtent != null)
                {
                    double resolution = MapArguments.CurrentResolution;
                    double worldOffsetX = targetExtent.UpperLeftPoint.X - PreviousExtent.UpperLeftPoint.X;
                    double worldOffsetY = targetExtent.UpperLeftPoint.Y - PreviousExtent.UpperLeftPoint.Y;
                    double screenOffsetX = worldOffsetX / resolution;
                    double screenOffsetY = worldOffsetY / resolution;

                    ((RelativeLayout.LayoutParams)LayoutParameters).LeftMargin -= (int)screenOffsetX;
                    ((RelativeLayout.LayoutParams)LayoutParameters).TopMargin += (int)screenOffsetY;
                    //_translateTransform.X -= screenOffsetX;
                    //_translateTransform.Y += screenOffsetY;
                }
            }
            else
            {*/
                //_translateTransform.X = 0;
                //_translateTransform.Y = 0;

                //LayerTile tile = OverlayCanvas.Children.OfType<LayerTile>().FirstOrDefault(t => t.GetValue(System.Windows.Controls.Canvas.NameProperty).Equals(TrackLayerTileName));;
                LayerTile tile = OverlayCanvas.FindViewById<LayerTile>(TrackLayerId);
                /*if(tile != null)
                {
                    tile.Dispose();
                    OverlayCanvas.RemoveAllViews();
                    tile = null;
                }*/

                if (tile == null)
                {
                    tile = new LayerTile(Context);
                    tile.Id = TrackLayerId;
                    tile.IsAsync = false;
                    tile.HasWatermark = false;
                    tile.Elevation = 1;
                    //tile.SetValue(System.Windows.Controls.Canvas.NameProperty, TrackLayerTileName);
                    //tile.SetValue(System.Windows.Controls.Panel.ZIndexProperty, 1);
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
            //}
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

            /*if (RenderMode == RenderMode.DrawingVisual)
            {
                DrawingVisualGeoCanvas geoCanvas = new DrawingVisualGeoCanvas();
                RenderTargetBitmap nativeImage = new RenderTargetBitmap(tileSw, tileSh, geoCanvas.Dpi, geoCanvas.Dpi, PixelFormats.Pbgra32);
                geoCanvas.BeginDrawing(nativeImage, tile.TargetExtent, MapArguments.MapUnit);
                tile.Draw(geoCanvas);
                geoCanvas.EndDrawing();
                tile.CommitDrawing(geoCanvas, nativeImage);
            }
            else
            {*/
            //TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.GetRandomGeoColor(RandomColorType.Pastel), 20, GeoColor.FromArgb(200, GeoColor.StandardColors.Black), 1);
            using (Bitmap nativeImage = Bitmap.CreateBitmap(tileSw, tileSh, Bitmap.Config.Argb8888))
            {
                GdiPlusAndroidGeoCanvas geoCanvas = new GdiPlusAndroidGeoCanvas(Context);
                geoCanvas.BeginDrawing(nativeImage, tile.TargetExtent, MapArguments.MapUnit);
                DrawTileCore(geoCanvas);
                geoCanvas.EndDrawing();
                tile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
            }
            //}
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
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), 20, GeoColor.FromArgb(200, GeoColor.StandardColors.Black), 1);
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle = LineStyles.CreateSimpleLineStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), 3, true);
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle = AreaStyles.CreateSimpleAreaStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), GeoColor.FromArgb(255, GeoColor.StandardColors.Gray), 3);
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle.OuterPen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle.InnerPen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultLineStyle.CenterPen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultAreaStyle.OutlinePen.LineJoin = DrawingLineJoin.Round;
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
        }
    }
}
