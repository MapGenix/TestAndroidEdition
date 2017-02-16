using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using Mapgenix.Shapes;
using Mapgenix.Canvas;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// Extent Changing interactive process with the Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class ExtentInteractiveOverlay : BaseInteractiveOverlay
    {
        private Point _mousePosition = new Point();
        private Point _originPosition = new Point();

        private Point _trackStartScreenPoint = new Point();
        private Point _trackEndScreenPoint = new Point();

        private int _zoomPercentage;

        [NonSerialized]
        private Rectangle _trackShape;

        public ExtentInteractiveOverlay()
        {
            OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.ExtentInteractiveOverlay);

            PanMode = MapPanMode.Default;
            MouseWheelMode = MapMouseWheelMode.Default;
            LeftClickDragMode = MapLeftClickDragMode.Default;
            DoubleLeftClickMode = MapDoubleLeftClickMode.Default;
            RightClickDragMode = MapRightClickDragMode.Default;
            DoubleRightClickMode = MapDoubleRightClickMode.Default;
            LeftClickDragKey = Keys.ShiftKey;
            RightClickDragKey = Keys.ShiftKey;
            _zoomPercentage = 50;
        }

        public MapPanMode PanMode { get; set; }

        public MapMouseWheelMode MouseWheelMode { get; set; }

        public MapDoubleLeftClickMode DoubleLeftClickMode { get; set; }

        public MapDoubleRightClickMode DoubleRightClickMode { get; set; }

        public MapLeftClickDragMode LeftClickDragMode { get; set; }

        public Keys LeftClickDragKey { get; set; }

        public MapRightClickDragMode RightClickDragMode { get; set; }

        public Keys RightClickDragKey { get; set; }

        public ExtentChangedType ExtentChangedType { get; protected set; }

        public int ZoomPercentage
        {
            get { return _zoomPercentage; }
            set { _zoomPercentage = value; }
        }

        protected override InteractiveResult MouseDownCore(InteractionArguments interactionArguments)
        {
            InteractiveResult interactiveResult = base.MouseDownCore(interactionArguments);
       
            if ((interactionArguments.MouseButton == MapMouseButton.Left && UnsafeHelper.IsKeyPressed(LeftClickDragKey) && LeftClickDragMode != MapLeftClickDragMode.Disabled)
                || (interactionArguments.MouseButton == MapMouseButton.Right && UnsafeHelper.IsKeyPressed(RightClickDragKey) && RightClickDragMode != MapRightClickDragMode.Disabled)
                || LeftClickDragKey == Keys.None || RightClickDragKey == Keys.None)
            {
                ExtentChangedType = ExtentChangedType.TrackZoomIn;
                _trackStartScreenPoint = new Point(interactionArguments.ScreenX, interactionArguments.ScreenY);

                _trackShape = new Rectangle();
                _trackShape.SetValue(System.Windows.Controls.Canvas.LeftProperty, _trackStartScreenPoint.X);
                _trackShape.SetValue(System.Windows.Controls.Canvas.TopProperty, _trackStartScreenPoint.Y);
                _trackShape.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.TrackZoomingRectangle);
                _trackShape.Width = 1;
                _trackShape.Height = 1;
                _trackShape.Fill = new SolidColorBrush(Colors.White);
                _trackShape.Stroke = new SolidColorBrush(Colors.Red);
                _trackShape.Opacity = .5;

                OverlayCanvas.Children.Add(_trackShape);
            }
            else if ((interactionArguments.MouseButton == MapMouseButton.Left || interactionArguments.MouseButton == MapMouseButton.Right) && PanMode != MapPanMode.Disabled)
            {
                ExtentChangedType = ExtentChangedType.Pan;
            }
            else return interactiveResult;

            _originPosition = new Point(interactionArguments.ScreenX, interactionArguments.ScreenY);
            interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            return interactiveResult;
        }

        protected override InteractiveResult MouseMoveCore(InteractionArguments interactionArguments)
        {
            InteractiveResult interactiveResult = base.MouseMoveCore(interactionArguments);

            Point currentPosition = new Point(interactionArguments.ScreenX, interactionArguments.ScreenY);
            _mousePosition = currentPosition;

            if (ExtentChangedType == ExtentChangedType.Pan)
            {
                double currentResolution = MapArguments.CurrentResolution;
                double offsetScreenX = currentPosition.X - _originPosition.X;
                double offsetScreenY = currentPosition.Y - _originPosition.Y;
                double offsetWorldX = offsetScreenX * currentResolution;
                double offsetWorldY = offsetScreenY * currentResolution;

                double left = interactionArguments.CurrentExtent.UpperLeftPoint.X - offsetWorldX;
                double top = interactionArguments.CurrentExtent.UpperLeftPoint.Y + offsetWorldY;
                double right = interactionArguments.CurrentExtent.LowerRightPoint.X - offsetWorldX;
                double bottom = interactionArguments.CurrentExtent.LowerRightPoint.Y + offsetWorldY;

                interactiveResult.NewCurrentExtent = new RectangleShape(left, top, right, bottom);
                _originPosition = currentPosition;
            }
            else if (ExtentChangedType == ExtentChangedType.TrackZoomIn)
            {
                if (_trackShape != null)
                {
                    if (currentPosition.X < _trackStartScreenPoint.X)
                    {
                        _trackShape.SetValue(System.Windows.Controls.Canvas.LeftProperty, _mousePosition.X);
                    }
                    if (currentPosition.Y < _trackStartScreenPoint.Y)
                    {
                        _trackShape.SetValue(System.Windows.Controls.Canvas.TopProperty, _mousePosition.Y);
                    }
                    _trackShape.Width = Math.Abs(_mousePosition.X - _trackStartScreenPoint.X);
                    _trackShape.Height = Math.Abs(_mousePosition.Y - _trackStartScreenPoint.Y);
                }
            }

            return interactiveResult;
        }

        protected override InteractiveResult MouseUpCore(InteractionArguments interactionArguments)
        {
            InteractiveResult interactiveResult = base.MouseUpCore(interactionArguments);
            Point currentScreenPoint = new Point(interactionArguments.ScreenX, interactionArguments.ScreenY);

            if (ExtentChangedType == ExtentChangedType.TrackZoomIn)
            {
                OverlayCanvas.Children.Clear();

                _trackEndScreenPoint = currentScreenPoint;
                PointShape startPointInDegree = MapArguments.ToWorldCoordinate(new PointShape(_trackStartScreenPoint.X, _trackStartScreenPoint.Y));
                PointShape endPointInDegree = MapArguments.ToWorldCoordinate(new PointShape(_trackEndScreenPoint.X, _trackEndScreenPoint.Y));
                double minX = startPointInDegree.X < endPointInDegree.X ? startPointInDegree.X : endPointInDegree.X;
                double maxX = startPointInDegree.X < endPointInDegree.X ? endPointInDegree.X : startPointInDegree.X;
                double minY = startPointInDegree.Y < endPointInDegree.Y ? startPointInDegree.Y : endPointInDegree.Y;
                double maxY = startPointInDegree.Y < endPointInDegree.Y ? endPointInDegree.Y : startPointInDegree.Y;

                RectangleShape newCurrentExtent = new RectangleShape(minX, maxY, maxX, minY);
                newCurrentExtent = MapArguments.GetSnappedExtent(newCurrentExtent);
                interactiveResult.NewCurrentExtent = newCurrentExtent;
                ExtentChangedType = ExtentChangedType.TrackZoomIn;
            }
            else if (ExtentChangedType == ExtentChangedType.Pan)
            {
                _mousePosition = currentScreenPoint;
                interactiveResult.NewCurrentExtent = interactionArguments.CurrentExtent;
                ExtentChangedType = ExtentChangedType.None;
            }

            return interactiveResult;
        }

        protected override InteractiveResult MouseWheelCore(InteractionArguments interactionArguments)
        {
            InteractiveResult interactiveResult = base.MouseWheelCore(interactionArguments);
            if (MouseWheelMode == MapMouseWheelMode.Disabled)
            {
                return interactiveResult;
            }

            int level = MapArguments.GetSnappedZoomLevelIndex(interactionArguments.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];
            if (interactionArguments.MouseWheelDelta <= 0)
            {
                ExtentChangedType = ExtentChangedType.MouseWheelZoomOut;
                targetScale *= ((100d + (double)ZoomPercentage) / 100d);
            }
            else
            {
                ExtentChangedType = ExtentChangedType.MouseWheelZoomIn;
                targetScale *= ((100d - (double)ZoomPercentage) / 100d);
            }

            _mousePosition = new Point(interactionArguments.ScreenX, interactionArguments.ScreenY);
            targetScale = MapArguments.ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(targetScale, MapArguments.ZoomLevelScales, MapArguments.MinimumScale, MapArguments.MaximumScale)];
            double deltaX = interactionArguments.MapWidth * .5 - _mousePosition.X;
            double deltaY = _mousePosition.Y - interactionArguments.MapHeight * .5;
            double newResolution = MapUtil.GetResolutionFromScale(targetScale, interactionArguments.MapUnit);
            PointShape newWorldCenter = MapArguments.ToWorldCoordinate(new PointShape(_mousePosition.X, _mousePosition.Y));
            newWorldCenter.X += deltaX * newResolution;
            newWorldCenter.Y += deltaY * newResolution;
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new Point(newWorldCenter.X, newWorldCenter.Y), targetScale, interactionArguments.MapUnit, interactionArguments.MapWidth, interactionArguments.MapHeight);

            return interactiveResult;
        }

        protected override InteractiveResult MouseDoubleClickCore(InteractionArguments interactionArguments)
        {
            InteractiveResult interactiveResult = base.MouseWheelCore(interactionArguments);
            if ((DoubleLeftClickMode == MapDoubleLeftClickMode.Disabled && interactionArguments.MouseButton == MapMouseButton.Left)
                || (DoubleRightClickMode == MapDoubleRightClickMode.Disabled && interactionArguments.MouseButton == MapMouseButton.Right))
            {
                return interactiveResult;
            }

            int zoomLevel = MapArguments.GetSnappedZoomLevelIndex(interactionArguments.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[zoomLevel];
            if (interactionArguments.MouseButton == MapMouseButton.Left)
            {
                ExtentChangedType = ExtentChangedType.DoubleClickZoomIn;
                targetScale *= ((100d - (double)ZoomPercentage) / 100d);
            }
            else
            {
                ExtentChangedType = ExtentChangedType.DoubleClickZoomOut;
                targetScale *= ((100d + (double)ZoomPercentage) / 100d);
            }

            double deltaX = interactionArguments.MapWidth * .5 - _mousePosition.X;
            double deltaY = _mousePosition.Y - interactionArguments.MapHeight * .5;
            targetScale = MapArguments.ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(targetScale, MapArguments.ZoomLevelScales, MapArguments.MinimumScale, MapArguments.MaximumScale)];
            double newResolution = MapUtil.GetResolutionFromScale(targetScale, interactionArguments.MapUnit);
            PointShape newWorldCenter = MapArguments.ToWorldCoordinate(new PointShape(_mousePosition.X, _mousePosition.Y));
            newWorldCenter.X += deltaX * newResolution;
            newWorldCenter.Y += deltaY * newResolution;
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new Point(newWorldCenter.X, newWorldCenter.Y), targetScale, interactionArguments.MapUnit, interactionArguments.MapWidth, interactionArguments.MapHeight);

            return interactiveResult;
        }


        protected override InteractiveResult MouseEnterCore(InteractionArguments interactionArguments)
        {
            if (interactionArguments.MouseButton == MapMouseButton.None)
            {
                ExtentChangedType = ExtentChangedType.None;
            }

            return base.MouseEnterCore(interactionArguments);
        }
    }
}
