using System;
using System.Windows;
using Mapgenix.Shapes;
using Mapgenix.Canvas;
using Android.Graphics;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace Mapgenix.GSuite.Android
{
    /// <summary>
    /// Class inheriting from InteractiveOverlay abstract class. This overlay deals with the
    /// Extent Changing interactive process with the Map Control using Mouse or Keyboard.
    /// </summary>
    [Serializable]
    public class ExtentInteractiveOverlay : BaseInteractiveOverlay
    {
        private PointF _tapPosition = new PointF();
        private PointF _originPosition = new PointF(0, 0);

        private PointF _trackStartScreenPoint = new PointF();
        private PointF _trackEndScreenPoint = new PointF();

        private int _zoomPercentage;

        [NonSerialized]
        private RelativeLayout _trackShape;

        public ExtentInteractiveOverlay(Context context)
            : base(context)
        {

            OverlayCanvas.Elevation = ZIndexes.ExtentInteractiveOverlay;
            //OverlayCanvas.SetValue(System.Windows.Controls.Panel.ZIndexProperty, ZIndexes.ExtentInteractiveOverlay);

            PanMode = MapPanMode.Default;
            MapDoubleTapMode = MapDoubleTapMode.Default;
            //LeftClickDragMode = MapLeftClickDragMode.Default;
            //DoubleLeftClickMode = MapDoubleLeftClickMode.Default;
            //RightClickDragMode = MapRightClickDragMode.Default;
            //DoubleRightClickMode = MapDoubleRightClickMode.Default;
            //LeftClickDragKey = Keys.ShiftKey;
            //RightClickDragKey = Keys.ShiftKey;
            _zoomPercentage = 50;
        }

        public MapPanMode PanMode { get; set; }

        public MapDoubleTapMode MapDoubleTapMode { get; set; }

        //public MapDoubleLeftClickMode DoubleLeftClickMode { get; set; }

        //public MapDoubleRightClickMode DoubleRightClickMode { get; set; }

        //public MapLeftClickDragMode LeftClickDragMode { get; set; }

        //public Keys LeftClickDragKey { get; set; }

        //public MapRightClickDragMode RightClickDragMode { get; set; }

        //public Keys RightClickDragKey { get; set; }

        public ExtentChangedType ExtentChangedType { get; protected set; }

        public int ZoomPercentage
        {
            get { return _zoomPercentage; }
            set { _zoomPercentage = value; }
        }

        protected override InteractiveResult MotionDownCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.MotionDownCore(motionArgs);
       
            if(motionArgs.MotionAction == MotionEventActions.Down)
            {
                ExtentChangedType = ExtentChangedType.Pan;
            }
            else return interactiveResult;

            _originPosition = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);
            interactiveResult.ProcessOtherOverlaysMode = ProcessOtherOverlaysMode.DoNotProcessOtherOverlays;
            return interactiveResult;
        }

        protected override InteractiveResult MotionMoveCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.MotionMoveCore(motionArgs);

            PointF currentPosition = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);
            _tapPosition = currentPosition;

            //ExtentChangedType = ExtentChangedType.Pan;

            if (ExtentChangedType == ExtentChangedType.Pan && motionArgs.ScreenPointers.Count == 1)
            {
                double currentResolution = MapArguments.CurrentResolution;
                double offsetScreenX = currentPosition.X - _originPosition.X;
                double offsetScreenY = currentPosition.Y - _originPosition.Y;
                double offsetWorldX = offsetScreenX * currentResolution;
                double offsetWorldY = offsetScreenY * currentResolution;

                double left = motionArgs.CurrentExtent.UpperLeftPoint.X - offsetWorldX;
                double top = motionArgs.CurrentExtent.UpperLeftPoint.Y + offsetWorldY;
                double right = motionArgs.CurrentExtent.LowerRightPoint.X - offsetWorldX;
                double bottom = motionArgs.CurrentExtent.LowerRightPoint.Y + offsetWorldY;

                interactiveResult.NewCurrentExtent = new RectangleShape(left, top, right, bottom);
                _originPosition = currentPosition;
            }

            return interactiveResult;
        }

        /*protected override InteractiveResult MotionPointerDownCore(MapMotionEventArgs motionArgs)
        {
            
        }

        private static float Funtion1(MapMotionEventArgs motionArgs)
        {
            double num1 = motionArgs.ScreenPointers[0].X - motionArgs.ScreenPointers[1].X;
            float num2 = motionArgs.ScreenPointers[0].Y - motionArgs.ScreenPointers[1].Y;
            float num3 = (float)Math.Sqrt((double)(num1 * num1 + (double)num2 * (double)num2));

            return num3;
        }

        private static void Function2(PointF point, MapMotionEventArgs motionArgs)
        {
            float num1 = motionArgs.ScreenPointers[0].X + motionArgs.ScreenPointers[1].X;
            float num2 = motionArgs.ScreenPointers[0].Y + motionArgs.ScreenPointers[1].Y;

            point.Set(num1, num2);
        }*/

        protected override InteractiveResult DoubleTapCore(MapMotionEventArgs interactionArguments)
        {
            InteractiveResult interactiveResult = base.DoubleTapCore(interactionArguments);
            if (MapDoubleTapMode == MapDoubleTapMode.Disabled)
            {
                return interactiveResult;
            }

            int level = MapArguments.GetSnappedZoomLevelIndex(interactionArguments.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];
            /*if (interactionArguments.PinchFactor <= 1)
            {
                ExtentChangedType = ExtentChangedType.MouseWheelZoomOut;
                targetScale *= ((100d + (double)ZoomPercentage) / 100d);
            }
            else*/
            {
                ExtentChangedType = ExtentChangedType.MouseWheelZoomIn;
                targetScale *= ((100d - (double)ZoomPercentage) / 100d);
            }

            _tapPosition = new PointF(interactionArguments.ScreenX, interactionArguments.ScreenY);
            targetScale = MapArguments.ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(targetScale, MapArguments.ZoomLevelScales, MapArguments.MinimumScale, MapArguments.MaximumScale)];
            double deltaX = interactionArguments.MapWidth * .5 - _tapPosition.X;
            double deltaY = _tapPosition.Y - interactionArguments.MapHeight * .5;
            double newResolution = MapUtil.GetResolutionFromScale(targetScale, interactionArguments.MapUnit);
            PointShape newWorldCenter = MapArguments.ToWorldCoordinate(new PointShape(_tapPosition.X, _tapPosition.Y));
            newWorldCenter.X += deltaX * newResolution;
            newWorldCenter.Y += deltaY * newResolution;
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new PointF((float)newWorldCenter.X, (float)newWorldCenter.Y), targetScale, interactionArguments.MapUnit, interactionArguments.MapWidth, interactionArguments.MapHeight);

            return interactiveResult;
        }

        /*protected override InteractiveResult PinchCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.PinchCore(motionArgs);
            if (MapDoubleTapMode == MapDoubleTapMode.Disabled)
            {
                return interactiveResult;
            }

            int level = MapArguments.GetSnappedZoomLevelIndex(motionArgs.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];

            if(motionArgs.PinchFactor != 1)
                targetScale *= motionArgs.PinchFactor;

            _tapPosition = new PointF(motionArgs.ScreenX, motionArgs.ScreenY);
            targetScale = MapArguments.ZoomLevelScales[MapUtil.GetSnappedZoomLevelIndex(targetScale, MapArguments.ZoomLevelScales, MapArguments.MinimumScale, MapArguments.MaximumScale)];
            double deltaX = motionArgs.MapWidth * .5 - _tapPosition.X;
            double deltaY = _tapPosition.Y - motionArgs.MapHeight * .5;
            double newResolution = MapUtil.GetResolutionFromScale(targetScale, motionArgs.MapUnit);
            PointShape newWorldCenter = MapArguments.ToWorldCoordinate(new PointShape(_tapPosition.X, _tapPosition.Y));
            newWorldCenter.X += deltaX * newResolution;
            newWorldCenter.Y += deltaY * newResolution;
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new PointF((float)newWorldCenter.X, (float)newWorldCenter.Y), targetScale, motionArgs.MapUnit, motionArgs.MapWidth, motionArgs.MapHeight);

            ExtentChangedType = ExtentChangedType.DoubleClickZoomOut;

            return interactiveResult;
        }*/

        protected override InteractiveResult PinchEndCore(MapMotionEventArgs motionArgs)
        {
            InteractiveResult interactiveResult = base.PinchEndCore(motionArgs);

            int level = MapArguments.GetSnappedZoomLevelIndex(motionArgs.CurrentExtent);
            double targetScale = MapArguments.ZoomLevelScales[level];

            targetScale /= motionArgs.PinchFactor;

            ExtentChangedType = ExtentChangedType.MouseWheelZoomIn;
            PointShape currentCenter = new PointShape(motionArgs.WorldX, motionArgs.WorldY);
            interactiveResult.NewCurrentExtent = MapUtil.CalculateExtent(new PointF((float)currentCenter.X, (float)currentCenter.Y), targetScale, motionArgs.MapUnit, motionArgs.MapWidth, motionArgs.MapHeight, motionArgs.Dpi);

            return interactiveResult;
        }

        /**protected override InteractiveResult MouseUpCore(InteractionArguments interactionArguments)
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
        }*/
    }
}
