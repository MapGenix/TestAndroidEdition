using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NativeAndroid = Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mapgenix.Shapes;
using Mapgenix.Utils;
using System.Collections.ObjectModel;
using Android.Graphics;
using Mapgenix.Canvas;

namespace Mapgenix.GSuite.Android
{
    public partial class Map : RelativeLayout
    {       
        private const int PreviousExtentsCapability = 30;
        private int _currentLevelIndex;
        private bool _isResizing;
        private double _previousSnappedScale;
        private double _targetSnappedScale;
        private double _targetMapScreenWidth;
        private double _targetMapScreenHeight;
        private double _minimumScale;
        private double _maximumScale;
        private RelativeLayout _overlayCanvas;
        private RelativeLayout _eventCanvas;
        private GridLayout _toolsGrid;
        //private DispatcherTimer _resizeTimer;
        private GeographyUnit _mapUnit;
        private RectangleShape _maxExtent;
        private RectangleShape _previousResizeExtent;
        private RectangleShape _currentResizeExtent;
        private RectangleShape _targetSnappedExtent;
        private SafeCollection<BaseOverlay> _overlays;
        private SafeCollection<BaseInteractiveOverlay> _interactiveOverlays;
        private Collection<double> _zoomLevelScales;
        private PointF _currentCenter;
        private PointF _currentMousePosition;
        private BackgroundOverlay _backgroundOverlay;
        //private AdornmentOverlay _adornmentOverlay;
        private MapTools _mapTools;
        private Collection<RectangleShape> _mapPreviousExtents;
        private bool _needsRefreshOverlayChildren;


        public Map(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public Map(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            MapResizeMode = MapResizeMode.PreserveScale;
            MapUnit = GeographyUnit.DecimalDegree;
            _mapTools = new MapTools(this);
           
            _mapPreviousExtents = new Collection<RectangleShape>();
            //InitializeResizeTimer();

            _interactiveOverlays = new SafeCollection<BaseInteractiveOverlay>();
            _overlays = new SafeCollection<BaseOverlay>();
            /*_overlays.Removing += OverlaysRemoving;
            _overlays.ClearingItems += OverlaysClearingItems;
            _interactiveOverlays.Removing += InteractiveOverlaysRemoving;
            _interactiveOverlays.ClearingItems += InteractiveOverlaysClearingItems;*/

            _toolsGrid = new GridLayout(Context);

            _currentCenter = new PointF(0f, 0f);
            _previousSnappedScale = Double.NaN;
            _zoomLevelScales = new Collection<double>();
            SyncZoomLevelScales(new ZoomLevelSet().GetZoomLevels());

            ExtentOverlay = new ExtentInteractiveOverlay(Context);
            /*TrackOverlay = new TrackInteractiveOverlay();
            EditOverlay = new EditInteractiveOverlay();
            _adornmentOverlay = new AdornmentOverlay();*/

            /*_overlayCanvas = new RelativeLayout(Context);
            RelativeLayout.LayoutParams p = new LayoutParams(this.LayoutParameters);
            p.TopMargin = 0;
            p.LeftMargin = 0;
            _overlayCanvas.LayoutParameters = p;
            AddView(_overlayCanvas, p);*/

            _minimumScale = 200;
            _maximumScale = double.MaxValue;
            SetMinimumWidth(1);
            SetMinimumHeight(1);

            //SizeChanged += WpfMapSizeChanged;
        }

        #region public properties

        public BackgroundOverlay BackgroundOverlay
        {
            get
            {
                if (_backgroundOverlay == null)
                {
                    _backgroundOverlay = new BackgroundOverlay(Context);
                }
                return _backgroundOverlay;
            }
            set
            {
                Validators.CheckParameterIsNotNull(value, "BackgroundOverlay");
                _backgroundOverlay = value;
            }
        }

        //public TrackInteractiveOverlay TrackOverlay { get; set; }

        public ExtentInteractiveOverlay ExtentOverlay { get; set; }


        //public EditInteractiveOverlay EditOverlay { get; set; }


        /*public AdornmentOverlay AdornmentOverlay
        {
            get { return _adornmentOverlay; }
            set { _adornmentOverlay = value; }
        }*/
        

        public GeographyUnit MapUnit
        {
            get { return _mapUnit; }
            set
            {
                Validators.CheckMapUnitIsValid(value);
                _maxExtent = MapUtil.GetDefaultMaxExtent(value);
                _mapUnit = value;
            }
        }


        public RectangleShape CurrentExtent
        {
            get { return GetRectangle(new PointShape(_currentCenter.X, _currentCenter.Y), ZoomLevelScales[_currentLevelIndex]); }
            set
            {
                if (CurrentExtent != null && value != null)
                {
                    RectangleShape previousExtent = CurrentExtent;
                    RectangleShape newSnappedExtent = GetSnappedExtent(value);

                    OnCurrentExtentChanging(new ExtentChangingEventArgs(previousExtent, newSnappedExtent, MapUnit, (float)LayoutParameters.Width, (float)LayoutParameters.Height, false));

                    int targetLevelIndex = GetSnappedZoomLevelIndex(value);
                    bool isLevelChanged = (_currentLevelIndex != targetLevelIndex);
                    _targetSnappedScale = MapUtil.GetScale(MapUnit, newSnappedExtent, LayoutParameters.Width, LayoutParameters.Height);
                    _previousSnappedScale = MapUtil.GetScale(MapUnit, previousExtent, LayoutParameters.Width, LayoutParameters.Height);

                    if (isLevelChanged)
                    {
                        OnCurrentScaleChanging(new ScaleChangingEventArgs(previousExtent, _previousSnappedScale, _targetSnappedScale, MapUnit, (float)LayoutParameters.Width, (float)LayoutParameters.Height, false));
                    }

                    PointShape centerPoint = value.GetCenterPoint();
                    _currentCenter = new PointF(Convert.ToSingle(centerPoint.X), Convert.ToSingle(centerPoint.Y));
                    _currentLevelIndex = targetLevelIndex;
                    if (Double.IsNaN(_previousSnappedScale))
                    {
                        _previousSnappedScale = ZoomLevelScales[_currentLevelIndex];
                    }

                    if (isLevelChanged)
                    {
                        OnCurrentScaleChanged(new ScaleChangedEventArgs(newSnappedExtent, _targetSnappedScale, MapUnit, (float)LayoutParameters.Width, (float)LayoutParameters.Height));
                    }

                    OnCurrentExtentChanged(new ExtentChangedEventArgs(newSnappedExtent, MapUnit, (float)LayoutParameters.Width, (float)LayoutParameters.Height));
                }
            }
        }

        public SafeCollection<BaseOverlay> Overlays { get { return _overlays; } }

        public SafeCollection<BaseInteractiveOverlay> InteractiveOverlays { get { return _interactiveOverlays; } }

        public RectangleShape MaxExtent { get { return _maxExtent; } }

        protected RelativeLayout OverlayCanvas { get { return _overlayCanvas; } }

        protected RelativeLayout EventCanvas { get { return _eventCanvas; } }

        public GridLayout ToolsGrid { get { return _toolsGrid; } }

        public MapTools MapTools { get { return _mapTools; } }

        public double CurrentScale
        {
            get { return ZoomLevelScales[_currentLevelIndex]; ; }
            set
            {
                Validators.CheckScaleIsValid(value, "CurrentScale");
                RectangleShape newExtent = GetRectangle(new PointShape(_currentCenter.X, _currentCenter.Y), value);
                CurrentExtent = newExtent;
            }
        }

        public double CurrentResolution
        {
            get { return MapUtil.GetResolutionFromScale(CurrentScale, MapUnit); }
            set
            {
                double newScale = MapUtil.GetScaleFromResolution(value, MapUnit);
                CurrentScale = newScale;
            }
        }

        public Collection<double> ZoomLevelScales { get { return _zoomLevelScales; } }

        public MapResizeMode MapResizeMode { get; set; }

        public double MaximumScale { get { return _maximumScale; } set { _maximumScale = value; } }

        public double MinimumScale { get { return _minimumScale; } set { _minimumScale = value; } }

        public RectangleShape RestrictExtent { get; set; }

        #endregion

        #region public methods

        public void Refresh()
        {
            if(_overlayCanvas == null)
            {
                _overlayCanvas = new RelativeLayout(Context);
                RelativeLayout.LayoutParams p = new LayoutParams(this.LayoutParameters);
                p.TopMargin = 0;
                p.LeftMargin = 0;
                _overlayCanvas.LayoutParameters = p;
                AddView(_overlayCanvas, p);
            }
            
            RelativeLayout.LayoutParams layoutExtent = new LayoutParams(this.LayoutParameters);
            layoutExtent.TopMargin = 0;
            layoutExtent.LeftMargin = 0;
            ExtentOverlay.LayoutParameters = layoutExtent;

            _needsRefreshOverlayChildren = true;
            Draw(CurrentExtent);
            _needsRefreshOverlayChildren = false;
        }

        public void Refresh(BaseOverlay redrawOverlay)
        {            
            if (IndexOfChild(redrawOverlay.OverlayCanvas) == 0 && Overlays.Contains(redrawOverlay))
            {
                ReOrderOverlayElements();
            }

            if (redrawOverlay != null && redrawOverlay.IsVisible && !redrawOverlay.IsEmpty)
            {
                _needsRefreshOverlayChildren = true;
                DrawOverlay(redrawOverlay, CurrentExtent, OverlayRefreshType.Redraw);
                _needsRefreshOverlayChildren = false;
            }
        }

        public void Refresh(IEnumerable<BaseOverlay> redrawOverlays)
        {
            foreach (BaseOverlay overlay in redrawOverlays)
            {
                Refresh(overlay);
            }
        }

        public RectangleShape GetSnappedExtent(RectangleShape extent)
        {
            int level = GetSnappedZoomLevelIndex(extent);
            double scale = ZoomLevelScales[level];
            PointShape center = extent.GetCenterPoint();
            return GetRectangle(center, scale);
        }

        public int GetSnappedZoomLevelIndex(RectangleShape targetExtent)
        {
            double screenWidth = LayoutParameters.Width;
            double screenHeight = LayoutParameters.Height;
            if (screenWidth == 0 || screenHeight == 0)
            {                
                /*Measure(new Size(float.PositiveInfinity, float.PositiveInfinity));
                screenWidth = DesiredSize.Width;
                screenHeight = DesiredSize.Height;*/
            }

            double resolution = MapUtil.GetResolution(targetExtent, screenWidth, screenHeight);
            double scale = MapUtil.GetScaleFromResolution(resolution, MapUnit);
            return GetSnappedZoomLevelIndex(scale);
        }


        public int GetSnappedZoomLevelIndex(double scale)
        {
            if (scale > _maximumScale)
            {
                scale = _maximumScale;
            }
            else if (scale < _minimumScale)
            {
                scale = _minimumScale;
            }

            return MapUtil.GetSnappedZoomLevelIndex(scale, _zoomLevelScales);
        }

        public PointShape ToWorldCoordinate(double screenX, double screenY)
        {
            PointF worldPoint = MapUtil.ToWorldCoordinate(CurrentExtent, screenX, screenY, LayoutParameters.Width, LayoutParameters.Height);
            return new PointShape(worldPoint.X, worldPoint.Y);
        }

        public PointShape ToWorldCoordinate(PointShape screenCoordinate)
        {
            return ToWorldCoordinate(screenCoordinate.X, screenCoordinate.Y);
        }

        public PointShape ToWorldCoordinate(Point screenCoordinate)
        {
            return ToWorldCoordinate(screenCoordinate.X, screenCoordinate.Y);
        }

        public PointShape ToWorldCoordinate(PointF screenCoordinate)
        {
            return ToWorldCoordinate(screenCoordinate.X, screenCoordinate.Y);
        }

        #endregion

        #region private methods

        private RectangleShape GetRectangle(PointShape center, double scale)
        {
            double resolution = MapUtil.GetResolutionFromScale(scale, MapUnit);
            double left = center.X - resolution * LayoutParameters.Width * .5;
            double top = center.Y + resolution * LayoutParameters.Height * .5;
            double right = center.X + resolution * LayoutParameters.Width * .5;
            double bottom = center.Y - resolution * LayoutParameters.Height * .5;
            return new RectangleShape(left, top, right, bottom);
        }

        private void Draw(RectangleShape targetExtent)
        {
            Draw(targetExtent, OverlayRefreshType.Redraw);
        }

        private RectangleShape GetAdjustedExtent(RectangleShape targetExtent)
        {
            if (RestrictExtent != null)
            {
                double left = targetExtent.LowerLeftPoint.X;
                double bottom = targetExtent.LowerLeftPoint.Y;
                double right = targetExtent.UpperRightPoint.X;
                double top = targetExtent.UpperRightPoint.Y;

                double restrictResolution = Math.Min(RestrictExtent.Width / LayoutParameters.Width, RestrictExtent.Height / LayoutParameters.Height) * .5;
                double restrictScale = MapUtil.GetScaleFromResolution(restrictResolution, MapUnit);
                double snappedRestrictScale = ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(restrictScale, ZoomLevelScales)];
                double snappedRestrictResolution = MapUtil.GetResolutionFromScale(snappedRestrictScale, MapUnit);
                if (snappedRestrictResolution >= CurrentResolution)
                {
                    if (left < RestrictExtent.LowerLeftPoint.X) left = RestrictExtent.LowerLeftPoint.X;
                    if (right > RestrictExtent.UpperRightPoint.X) right = RestrictExtent.UpperRightPoint.X;
                    if (top > RestrictExtent.UpperRightPoint.Y) top = RestrictExtent.UpperRightPoint.Y;
                    if (bottom < RestrictExtent.LowerLeftPoint.Y) bottom = RestrictExtent.LowerLeftPoint.Y;
                }
                else if (targetExtent.Width > RestrictExtent.Width && targetExtent.Height > RestrictExtent.Height)
                {
                    double targetResolution = MapUtil.GetResolution(targetExtent, Width, LayoutParameters.Height);
                    PointShape centerPoint = RestrictExtent.GetCenterPoint();
                    left = centerPoint.X - LayoutParameters.Width * targetResolution * .5;
                    right = centerPoint.X + LayoutParameters.Width * targetResolution * .5;
                    top = centerPoint.Y + LayoutParameters.Height * targetResolution * .5;
                    bottom = centerPoint.Y - LayoutParameters.Height * targetResolution * .5;
                }
                else
                {
                    double targetResolution = MapUtil.GetResolution(targetExtent, LayoutParameters.Width, LayoutParameters.Height);
                    PointShape centerPoint = RestrictExtent.GetCenterPoint();
                    if (targetExtent.Width > RestrictExtent.Width)
                    {
                        left = centerPoint.X - LayoutParameters.Width * targetResolution * .5;
                        right = centerPoint.X + LayoutParameters.Width * targetResolution * .5;
                    }
                    else if (right < RestrictExtent.LowerLeftPoint.X || left > RestrictExtent.UpperRightPoint.X)
                    {
                        left = RestrictExtent.LowerLeftPoint.X;
                        right = RestrictExtent.UpperRightPoint.X;
                    }
                    else
                    {
                        if (left < RestrictExtent.LowerLeftPoint.X) left = RestrictExtent.LowerLeftPoint.X;
                        else if (right > RestrictExtent.UpperRightPoint.X) right = RestrictExtent.UpperRightPoint.X;
                    }

                    if (targetExtent.Height > RestrictExtent.Height)
                    {
                        top = centerPoint.Y + LayoutParameters.Height * targetResolution * .5;
                        bottom = centerPoint.Y - LayoutParameters.Height * targetResolution * .5;
                    }
                    else if (bottom > RestrictExtent.UpperRightPoint.Y || top < RestrictExtent.LowerLeftPoint.Y)
                    {
                        bottom = RestrictExtent.LowerLeftPoint.Y;
                        top = RestrictExtent.UpperRightPoint.Y;
                    }
                    else
                    {
                        if (top > RestrictExtent.UpperRightPoint.Y) top = RestrictExtent.UpperRightPoint.Y;
                        else if (bottom < RestrictExtent.LowerLeftPoint.Y) bottom = RestrictExtent.LowerLeftPoint.Y;
                    }
                }

                return new RectangleShape(left, top, right, bottom);
            }
            else
            {
                return targetExtent;
            }
        }

        private void ReOrderOverlayElements()
        {
            int index = 1;
            RemoveAllViews();
            foreach (BaseOverlay overlay in Overlays)
            {
                //if (!(overlay is BaseMarkerOverlay) && !(overlay is PopupOverlay))
                //{
                    AddView(overlay.OverlayCanvas, index++);
                //}
            }
        }

        private Collection<BaseOverlay> CollectCurrentOverlays()
        {
            Collection<BaseOverlay> currentOverlays = new Collection<BaseOverlay>();
            if (_backgroundOverlay != null) { currentOverlays.Add(_backgroundOverlay); }

            for (int i = Overlays.Count - 1; i >= 0; i--)
            {
                currentOverlays.Add(Overlays[i]);
            }

            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = CollectCurrentInteractiveOverlays();
            foreach (BaseInteractiveOverlay interactiveOverlay in currentInteractiveOverlays)
            {
                currentOverlays.Add(interactiveOverlay);
            }
            //currentOverlays.Add(AdornmentOverlay);

            return currentOverlays;
        }

        private Collection<BaseInteractiveOverlay> CollectCurrentInteractiveOverlays()
        {
            Collection<BaseInteractiveOverlay> currentInteractiveOverlays = new Collection<BaseInteractiveOverlay>();
            foreach (BaseInteractiveOverlay interactiveOverlay in _interactiveOverlays.Where(o => o.IsVisible))
            {
                if (interactiveOverlay != null) { currentInteractiveOverlays.Add(interactiveOverlay); }
            }

            //if (EditOverlay.IsVisible) currentInteractiveOverlays.Add(EditOverlay);
            //if (TrackOverlay.IsVisible) currentInteractiveOverlays.Add(TrackOverlay);
            if (ExtentOverlay.IsVisible) currentInteractiveOverlays.Add(ExtentOverlay);

            return currentInteractiveOverlays;
        }

        private void DrawOverlays(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            DrawOverlays(CollectCurrentOverlays(), targetExtent, refreshType);
        }

        private void DrawOverlays(IEnumerable<BaseOverlay> drawingOverlays, RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            /*if (ExtentOverlay.ExtentChangedType != ExtentChangedType.Pan)
            {
                ReOrderOverlayElements();
                OverlaysEventArgs drawingArgs = new OverlaysEventArgs(drawingOverlays, targetExtent);
                OnOverlaysDrawing(drawingArgs);
                if (drawingArgs.Cancel) { return; }
            }*/

            foreach (BaseOverlay overlay in drawingOverlays)
            {
                if (overlay != null)
                {
                    DrawOverlay(overlay, targetExtent, refreshType);
                }
            }

            /*if (ExtentOverlay != null && ExtentOverlay.ExtentChangedType != ExtentChangedType.Pan)
            {
                _previousResizeExtent = targetExtent;
                _mapPreviousExtents.Add(targetExtent);
                LimitPreviousExtentCapability();
            }*/

            /*if (ExtentOverlay.ExtentChangedType != ExtentChangedType.Pan)
            {
                OverlaysEventArgs drawnArgs = new OverlaysEventArgs(drawingOverlays, targetExtent);
                OnOverlaysDrawn(drawnArgs);
            }*/
        }

        private void DrawOverlay(BaseOverlay overlay, RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            if (OverlayCanvas != null && OverlayCanvas.IndexOfChild(overlay.OverlayCanvas) == -1)
            {
                //OverlayCanvas.Children.Add(overlay.OverlayCanvas);
                OverlayCanvas.AddView(overlay.OverlayCanvas);
            }

            MapArguments arguments = GetMapArguments();
            overlay.Initialize(arguments);

            if (overlayRefreshType == OverlayRefreshType.Pan)
            {
                overlay.Draw(targetExtent, OverlayRefreshType.Pan);
            }
            else
            {
                OverlayEventArgs drawingArgs = new OverlayEventArgs(overlay, targetExtent);
                OnOverlayDrawing(drawingArgs);
                if (drawingArgs.Cancel) { return; }

                if (!overlay.IsEmpty)
                {
                    OverlayRefreshType currentOverlayRefreshType = overlayRefreshType;
                    BaseTileOverlay tileOverlay = overlay as BaseTileOverlay;
                    if (tileOverlay != null && tileOverlay.TileType != TileType.SingleTile && !_needsRefreshOverlayChildren)
                    {
                        currentOverlayRefreshType = OverlayRefreshType.Pan;
                    }

                    overlay.Draw(targetExtent, currentOverlayRefreshType);
                }

                OverlayEventArgs drawnArgs = new OverlayEventArgs(overlay, targetExtent);
                OnOverlayDrawn(drawnArgs);
            }
        }

        private MapArguments GetMapArguments()
        {
            MapArguments mapArgs = new MapArguments();
            mapArgs.ActualHeight = LayoutParameters.Height;
            mapArgs.ActualWidth = LayoutParameters.Width;
            mapArgs.CurrentExtent = (RectangleShape)CurrentExtent.CloneDeep();
            mapArgs.CurrentResolution = CurrentResolution;
            mapArgs.CurrentScale = CurrentScale;
            mapArgs.MapUnit = _mapUnit;
            mapArgs.MaximumScale = MaximumScale;
            mapArgs.MinimumScale = MinimumScale;
            mapArgs.MaxExtent = MaxExtent;

            foreach (double zoomLevelScale in _zoomLevelScales)
            {
                mapArgs.ZoomLevelScales.Add(zoomLevelScale);
            }

            return mapArgs;
        }

        private void LimitPreviousExtentCapability()
        {
            while (_mapPreviousExtents.Count > PreviousExtentsCapability)
            {
                _mapPreviousExtents.RemoveAt(0);
            }
        }

        private bool CheckIfZoomAnimationSkipped()
        {
            bool isSkipped = false;
            /*if (ExtentOverlay != null && (ExtentOverlay.ExtentChangedType == ExtentChangedType.TrackZoomIn || ExtentOverlay.ExtentChangedType == ExtentChangedType.TrackZoomOut))
            {
                isSkipped = true;
            }
            else if (_isResizing)
            {
                isSkipped = true;
            }*/

            return isSkipped;
        }

        private void SyncZoomLevelScales(IEnumerable<ZoomLevel> zoomLevels)
        {
            _zoomLevelScales.Clear();
            foreach (ZoomLevel level in zoomLevels)
            {
                if (level.Scale != 0)
                {
                    _zoomLevelScales.Add(level.Scale);
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsInPanning()
        {
            bool isPanning = false;
            if (ExtentOverlay != null && ExtentOverlay.ExtentChangedType == ExtentChangedType.Pan)
            {
                isPanning = true;
            }
            return isPanning;
        }

        #endregion

        #region protected methods

        protected void Draw(RectangleShape targetExtent, OverlayRefreshType refreshType)
        {
            DrawCore(targetExtent, refreshType);
        }

        protected virtual void DrawCore(RectangleShape targetExtent, OverlayRefreshType overlayRefreshType)
        {
            targetExtent = GetAdjustedExtent(targetExtent);

            RectangleShape previousExtent = CurrentExtent;
            _targetSnappedExtent = GetSnappedExtent(targetExtent);

            _targetSnappedScale = MapUtil.GetScale(MapUnit, _targetSnappedExtent, LayoutParameters.Width, LayoutParameters.Height);
            _previousSnappedScale = MapUtil.GetScale(MapUnit, previousExtent, LayoutParameters.Width, LayoutParameters.Height);

            if (MapUtil.IsFuzzyEqual(_targetSnappedScale, _previousSnappedScale))
            {
                CurrentExtent = _targetSnappedExtent;
                DrawOverlays(CurrentExtent, overlayRefreshType);
                MapTools.Refresh();
            }
            else
            {
                if (CheckIfZoomAnimationSkipped())
                {
                    CurrentExtent = _targetSnappedExtent;
                    Draw(_targetSnappedExtent, overlayRefreshType);
                }
                else
                {
                    ExecuteZoomAnimation(_targetSnappedScale, _previousSnappedScale, _currentMousePosition);
                }
            }
        }

        protected override void OnDraw(NativeAndroid.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
            /*if (_overlayCanvas == null)
            {
                _overlayCanvas = new RelativeLayout(Context);
                RelativeLayout.LayoutParams p = new LayoutParams(this.LayoutParameters);
                p.TopMargin = 0;
                p.LeftMargin = 0;
                _overlayCanvas.LayoutParameters = p;
                AddView(_overlayCanvas, p);
            }

            _needsRefreshOverlayChildren = true;
            Draw(CurrentExtent);
            _needsRefreshOverlayChildren = false;*/
        }

        #endregion
    }
}