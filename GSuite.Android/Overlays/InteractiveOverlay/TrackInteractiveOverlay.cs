using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Mapgenix.Styles;
using Mapgenix.Layers;

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
        private const string TrackLayerTileName = "TrackLayerTile";

        private TrackMode _trackMode;
        private Collection<Vertex> _vertices;
        private InMemoryFeatureLayer _trackShapeLayer;
        private bool _isInTracking;
        private bool _isClicking;
        private int _mouseDownCount;   
        [NonSerialized]
        private TranslateTransform _translateTransform;

        public event EventHandler<ShapeEventArgs> TrackEnded;

        public event EventHandler<ShapeEventArgs> TrackEnding;

        public event EventHandler<VertexEventArgs> TrackStarted;

        public event EventHandler<VertexEventArgs> TrackStarting;

        public event EventHandler<VertexFeatureEventArgs> VertexAdded;

        public event EventHandler<VertexFeatureEventArgs> VertexAdding;

        public event EventHandler<VertexFeatureEventArgs> MouseMoved;

        public TrackInteractiveOverlay()
        {
            _trackMode = TrackMode.None;
            RenderMode = RenderMode.DrawingVisual;

            _trackShapeLayer = FeatureLayerFactory.CreateInMemoryFeatureLayer();
            _vertices = new Collection<Vertex>();

            OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.TrackInteractiveOverlay);
            _translateTransform = new TranslateTransform();
            OverlayCanvas.RenderTransform = _translateTransform;

            SetDefaultStyle();
        }

        public override bool IsEmpty
        {
            get
            {
                bool isEmpty = _trackShapeLayer.IsEmpty();
                if (isEmpty)
                {
                    Tile tile = OverlayCanvas.Children.OfType<Tile>().FirstOrDefault(t => t.GetValue(System.Windows.Controls.Canvas.NameProperty).Equals(TrackLayerTileName));
                    if (tile != null)
                    {
                        OverlayCanvas.Children.Remove(tile);
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

        protected override InteractiveResult MouseDownCore(InteractionArguments interactionArguments)
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

                if (interactionArguments.MouseButton == MapMouseButton.Right) return interactiveResult;
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

        protected override InteractiveResult MouseMoveCore(InteractionArguments interactionArguments)
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
        }

        protected override InteractiveResult MouseUpCore(InteractionArguments interactionArguments)
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

        protected override InteractiveResult MouseClickCore(InteractionArguments interactionArguments)
        {
            Validators.CheckParameterIsNotNull(interactionArguments, "interactionArguments");

            InteractiveResult interactiveResult = new InteractiveResult();

            DrawType mouseDownResult = DrawType.DoNotDraw;
            DrawType mouseUpResult = DrawType.DoNotDraw;
            if (this.TrackMode != TrackMode.None && interactionArguments.MouseButton != MapMouseButton.Right)
            {
                _isClicking = true;
                mouseDownResult = MouseDown(interactionArguments).DrawThisOverlay;
                mouseUpResult = MouseUp(interactionArguments).DrawThisOverlay;
                if (mouseDownResult == DrawType.Draw || mouseUpResult == DrawType.Draw)
                {
                    interactiveResult.DrawThisOverlay = DrawType.Draw;
                }

                interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;

                WriteTrackShapeLayer();
            }

            return interactiveResult;
        }

        protected override InteractiveResult MouseDoubleClickCore(InteractionArguments interactionArguments)
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
           
            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                if (PreviousExtent != null)
                {
                    double resolution = MapArguments.CurrentResolution;
                    double worldOffsetX = targetExtent.UpperLeftPoint.X - PreviousExtent.UpperLeftPoint.X;
                    double worldOffsetY = targetExtent.UpperLeftPoint.Y - PreviousExtent.UpperLeftPoint.Y;
                    double screenOffsetX = worldOffsetX / resolution;
                    double screenOffsetY = worldOffsetY / resolution;

                    _translateTransform.X -= screenOffsetX;
                    _translateTransform.Y += screenOffsetY;
                }
            }
            else
            {
                _translateTransform.X = 0;
                _translateTransform.Y = 0;

                LayerTile tile = OverlayCanvas.Children.OfType<LayerTile>().FirstOrDefault(t => t.GetValue(System.Windows.Controls.Canvas.NameProperty).Equals(TrackLayerTileName));;
                if (tile == null)
                {
                    tile = new LayerTile();
                    tile.IsAsync = false;
                    tile.HasWatermark = false;
                    tile.SetValue(System.Windows.Controls.Canvas.NameProperty, TrackLayerTileName);
                    tile.SetValue(System.Windows.Controls.Panel.ZIndexProperty, 1);
                    OverlayCanvas.Children.Add(tile);
                }
                else
                {
                    tile.DrawingLayers.Clear();
                }

                tile.TargetExtent = targetExtent;
                tile.Width = MapArguments.ActualWidth;
                tile.Height = MapArguments.ActualHeight;
                tile.ZoomLevelIndex = MapArguments.GetSnappedZoomLevelIndex(targetExtent);
                tile.DrawingLayers.Add(_trackShapeLayer);

                DrawTile(tile);
            }
        }

       
       protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (disposing && OverlayCanvas.Children.Count != 0)
                {
                    Tile tile = OverlayCanvas.Children[0] as Tile;
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

            if (RenderMode == RenderMode.DrawingVisual)
            {
                DrawingVisualGeoCanvas geoCanvas = new DrawingVisualGeoCanvas();
                RenderTargetBitmap nativeImage = new RenderTargetBitmap(tileSw, tileSh, geoCanvas.Dpi, geoCanvas.Dpi, PixelFormats.Pbgra32);
                geoCanvas.BeginDrawing(nativeImage, tile.TargetExtent, MapArguments.MapUnit);
                tile.Draw(geoCanvas);
                geoCanvas.EndDrawing();
                tile.CommitDrawing(geoCanvas, nativeImage);
            }
            else
            {
                using (Bitmap nativeImage = new Bitmap(tileSw, tileSh))
                {
                    GdiPlusGeoCanvas geoCanvas = new GdiPlusGeoCanvas();
                    geoCanvas.BeginDrawing(nativeImage, tile.TargetExtent, MapArguments.MapUnit);
                    DrawTileCore(geoCanvas);
                    geoCanvas.EndDrawing();
                    tile.CommitDrawing(geoCanvas, MapUtil.GetImageSourceFromNativeImage(nativeImage));
                }
            }
        }

        protected virtual void DrawTileCore(GdiPlusGeoCanvas geoCanvas)
        {
            LayerTile layerTile = OverlayCanvas.Children[0] as LayerTile;
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
                    ShapeEventArgs e = new ShapeEventArgs(GetTrackingShape());
                    OnTrackEnding(e);
                    if (e.Cancel) { return; }

                    Feature feature = new Feature(baseShape);
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
            finally
            {
            }
        }

        private void SetDefaultStyle()
        {
            TrackShapeLayer.ZoomLevelSet.ZoomLevel01.DefaultPointStyle = PointStyles.CreateSimpleCircleStyle(GeoColor.FromArgb(150, GeoColor.StandardColors.Green), 12, GeoColor.FromArgb(200, GeoColor.StandardColors.Black), 1);
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
